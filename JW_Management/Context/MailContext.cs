using System.Net;
using System.Net.Mail;
using MailKit.Security;
namespace JW_Management.Models
{
    public class MailContext(AppContext _appContext)
    {
        // Remetente e servidor vêm da configuração da congregação (ecrã Configuração); o nome do remetente e a cópia (Bcc) são globais. Nada vem do appsettings.
        private MailMessage Montar(string EmailDestino, string Assunto, string Mensagem, List<Attachment>? Anexos, bool comCopia)
        {
            var t = _appContext._currentTenant;
            var g = _appContext._tenantContext.Config?.Load() ?? new Dictionary<string, string?>();
            var nome = g.GetValueOrDefault(ConfigStore.MailNome);

            var myMail = new MailMessage { From = new MailAddress(t.Email, string.IsNullOrWhiteSpace(nome) ? t.NomeCongregacao : nome) };
            foreach (var item in EmailDestino.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) myMail.To.Add(item);
            if (comCopia && g.GetValueOrDefault(ConfigStore.MailBcc) is { Length: > 0 } bcc) myMail.Bcc.Add(new MailAddress(bcc));

            myMail.Subject = Assunto;
            myMail.SubjectEncoding = System.Text.Encoding.UTF8;
            myMail.Body = (DateTime.Now.Hour < 13 ? "Bom Dia, <br><br>" : "Boa Tarde, <br><br>") + Mensagem;
            myMail.BodyEncoding = System.Text.Encoding.UTF8;
            myMail.IsBodyHtml = true;

            //Assinatura
            myMail.Body += "<br><br><i>Atenção este é um email automático, por favor não responda a este email!</i><br><br>Powered by: JKSProds - Software";

            if (Anexos != null) foreach (var a in Anexos) myMail.Attachments.Add(a);
            return myMail;
        }

        // O envio exige email, servidor, porta e palavra-passe da congregação (ecrã Configuração).
        private bool Configurado(out Tenant t, out string senha, out int porta)
        {
            t = _appContext._currentTenant;
            senha = _appContext._tenantContext.Config?.Decifrar(t.EmailPassword) ?? t.EmailPassword ?? "";
            porta = 0;
            if (!string.IsNullOrWhiteSpace(t.Email) && !string.IsNullOrWhiteSpace(t.EmailHost) && int.TryParse(t.EmailSMTPPort, out porta) && senha != "") return true;
            Console.WriteLine($"[Email] {t.Nome}: envio não configurado (email, servidor, porta e palavra-passe).");
            return false;
        }

        // Uma só ligação ao servidor para todos os emails do lote. Devolve quantos foram enviados (0 se o envio não estiver configurado ou a ligação falhar).
        private int EnviarLote(List<MailMessage> mensagens)
        {
            if (!Configurado(out var t, out var senha, out var porta)) { foreach (var m in mensagens) m.Dispose(); return 0; }

            var enviados = 0;
            try
            {
                using var c = new MailKit.Net.Smtp.SmtpClient();
                // 465 = TLS desde o início da ligação; nas outras portas (587) a ligação passa a TLS depois de começar
                c.Connect(t.EmailHost, porta, porta == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                c.Authenticate(t.Email, senha);
                foreach (var m in mensagens)
                {
                    try { c.Send(new MailKit.SendRequest(MimeKit.MimeMessage.CreateFromMailMessage(m))); enviados++; }
                    catch (Exception e) when (e is not OperationCanceledException) { Console.WriteLine($"[Email] {t.Nome}: falha num envio: {e.Message}"); }
                }
                c.Disconnect(true);
            }
            catch (Exception e) { Console.WriteLine($"[Email] {t.Nome}: falha na ligação ao servidor de email: {e.Message}"); }
            finally { foreach (var m in mensagens) m.Dispose(); }
            return enviados;
        }

        private bool EnviarMail(string EmailDestino, string Assunto, string Mensagem, List<Attachment>? Anexos)
        {
            if (!Configurado(out _, out _, out _)) return false;
            try { return EnviarLote([Montar(EmailDestino, Assunto, Mensagem, Anexos, true)]) == 1; }
            catch (Exception e) { Console.WriteLine($"[Email] não foi possível preparar o email: {e.Message}"); return false; }
        }

        private static string H(string? s) => WebUtility.HtmlEncode(s ?? "");

        // Pedidos de literatura selecionados, enviados a um publicador. especiais = mostra também o estado do pedido.
        public bool MailPedidosPublicador(Publicador p, List<Literatura> lista, bool especiais)
        {
            if (lista.Count == 0 || string.IsNullOrWhiteSpace(p.Email)) return false;

            var m = $"<b>{H(p.Nome)}</b>, seguem os pedidos de literatura selecionados:<br><br>" +
                    "<table class='table' style='width:100%;border-width:1px;' border='1'><tr><th>Referência</th><th>Designação</th><th>Quantidade</th>" + (especiais ? "<th>Estado</th>" : "") + "</tr>";
            foreach (var l in lista)
                m += $"<tr><td style='padding: 5px;'>{H(l.Referencia)}</td><td style='padding: 5px;'>{H(l.Descricao)}</td><td style='padding: 5px;'>{l.Quantidade}</td>" +
                     (especiais ? $"<td style='padding: 5px;'>{H(l.EstadoPedido?.Descricao)}</td>" : "") + "</tr>";
            m += "</table>";

            return EnviarMail(p.Email, "Pedidos de literatura - " + DateTime.Now.ToShortDateString(), m, null);
        }

        // Programas do mês: um email por publicador (cada um com os PDF escolhidos), todos numa só ligação e sem cópia Bcc. Devolve quantos foram enviados.
        public int MailProgramas(List<Publicador> publicadores, List<(string Nome, byte[] Pdf)> pdfs, string mes)
        {
            if (!Configurado(out _, out _, out _)) return 0;
            var lote = new List<MailMessage>();
            foreach (var p in publicadores.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
            {
                try
                {
                    var anexos = pdfs.Select(f => new Attachment(new MemoryStream(f.Pdf), f.Nome, "application/pdf")).ToList();
                    lote.Add(Montar(p.Email!, "Programa de " + mes, $"<b>{H(p.Nome)}</b>, seguem em anexo os programas de {H(mes)}.", anexos, false));
                }
                catch (Exception e) { Console.WriteLine($"[Email] programa não preparado para um publicador: {e.Message}"); }
            }
            return lote.Count == 0 ? 0 : EnviarLote(lote);
        }

        public bool MailPedidosEspeciaisPendentes(List<Literatura> LstLiteratura, string EmailDestino)
        {
            if (LstLiteratura.Count() == 0 || string.IsNullOrEmpty(EmailDestino)) return false;

            string Mensagem = "Abaixo segue uma listagem com todos os pedidos especiais para a congregação: <br><br>";

            Mensagem += "<table class='table' style='width:100%;border-width:1px;' border='1'><tr><th>Referência</th><th>Designação</th><th>Quantidade</th><th>Publicador</th></tr>";

            foreach (var l in LstLiteratura)
            {
                Mensagem += "<tr><td style='padding: 5px;'>" + l.Referencia + "</td><td style='padding: 5px;'>" + l.Descricao + "</td><td style='padding: 5px;'>" + l.Quantidade + "</td><td style='padding: 5px;'>" + l.Publicador.Nome + "</td></tr>";
            }

            Mensagem += "</table>";

            return EnviarMail(EmailDestino, "Pedidos Novos - " + DateTime.Now.ToShortDateString(), Mensagem, null);
        }

        public bool MailPedidosPeriodicos(List<Literatura> LstLiteratura, string EmailDestino)
        {
            if (LstLiteratura.Count() == 0 || string.IsNullOrEmpty(EmailDestino)) return false;

            string Mensagem = "Abaixo segue uma listagem com todos os pedidos periodicos requisitados pelos irmãos da seguinte publicação: <br><br>";

            Mensagem += "Referência: <b>" + LstLiteratura.First().Referencia + "</b><br>Designação: <b>" + LstLiteratura.First().Descricao + "</b><br>Quantidade: <b>" + LstLiteratura.Sum(p => p.Quantidade) + " UN</b><br><br>";

            Mensagem += "<table class='table' style='width:100%;border-width:1px;' border='1'><tr><th>Publicador</th><th>Quantidade</th></tr>";

            foreach (var l in LstLiteratura)
            {
                Mensagem += "<tr><td style='padding: 5px;'>" + l.Publicador.Nome + "</td><td style='padding: 5px;'>" + l.Quantidade + "</td></tr>";
            }

            Mensagem += "</table>";

            return EnviarMail(EmailDestino, "Pedidos Periodicos - " + DateTime.Now.ToShortDateString(), Mensagem, null);
        }

        public bool MailPedidosPeriodicosGrupo(List<Literatura> LstLiteratura, string EmailDestino)
        {
            if (LstLiteratura.Count() == 0 || string.IsNullOrEmpty(EmailDestino)) return false;
            string Mensagem = "";

            foreach (var tLiteratura in LstLiteratura.GroupBy(o => o.Referencia))
            {
                 Mensagem += "Abaixo segue uma listagem com todos os pedidos periodicos requisitados pelos irmãos da seguinte publicação: <br><br>";
                Mensagem += "Referência: <b>" + tLiteratura.First().Referencia + "</b><br>Designação: <b>" + tLiteratura.First().Descricao + "</b><br>Quantidade: <b>" + tLiteratura.Sum(p => p.Quantidade) + " UN</b><br><br>";
                foreach (Grupo g in tLiteratura.Select(v => v.Publicador.Grupo).Distinct().OrderBy(v => v.Id))
                {
                    if (tLiteratura.Where(v => v.Publicador.Grupo.Id == g.Id).OrderBy(v => v.Publicador.Nome).Count() > 0)
                    {
                        Mensagem += "<b>Grupo " + g.Id + " [" + g.Responsavel.Nome + "]</b> <br><br>";

                        Mensagem += "<table class='table' style='width:100%;border-width:1px;' border='1'><tr><th>Publicador</th><th>Quantidade</th></tr>";

                        foreach (Literatura l in tLiteratura.Where(v => v.Publicador.Grupo.Id == g.Id).OrderBy(v => v.Publicador.Nome))
                        {
                            Mensagem += "<tr><td style='padding: 5px;'>" + l.Publicador.Nome + "</td><td style='padding: 5px;'>" + l.Quantidade + "</td></tr>";
                        }
                    }
                    Mensagem += "</table>";
                    Mensagem += "<br><br>";
                }
            }
            



            return EnviarMail(EmailDestino, "Pedidos Periodicos - " + DateTime.Now.ToShortDateString(), Mensagem, null);
        }
        
        public bool MailSenhaPublicador(Publicador p)
        {
            string Assunto = "🔐 Acesso Plataforma - Publicador - " + p.Nome;
            string Mensagem =
                $"<b>{p.Nome}</b>, requisitaste acesso á plataforma congregacional. Abaixo seguem os dados de acesso à tua conta: <br><br>Nome de Utilizador: <b>" +
                p.Username + "</b><br>Palavra-Passe: <b>" + p.Password + "</b><br><br><br><br>";

            Mensagem +=
                "<a style=\"background-color:#0a0a23;color: #fff;border:none; border-radius:10px; padding:15px; min-width: 120px;\"  href=\"" +
                p.GetUrl + "\">Abrir Plataforma</a><br><br>";
            
            return p.Email != null && EnviarMail(p.Email, Assunto, Mensagem, null);
        }
    }
}
