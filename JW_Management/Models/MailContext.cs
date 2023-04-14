using System.Net.Mail;
namespace JW_Management.Models
{
    public static class MailContext
    {
        private static bool EnviarMail(string EmailDestino, string Assunto, string Mensagem, Attachment anexo)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            try
            {
                SmtpClient mySmtpClient = new SmtpClient(configuration.GetValue<string>("Mail:SMTP"), 587)
                {
                    UseDefaultCredentials = false,
                    EnableSsl = true
                };

                System.Net.NetworkCredential basicAuthenticationInfo = new
                   System.Net.NetworkCredential(configuration.GetValue<string>("Mail:Email"), configuration.GetValue<string>("Mail:Password"));
                mySmtpClient.Credentials = basicAuthenticationInfo;

                MailAddress from = new MailAddress(configuration.GetValue<string>("Mail:Email")!, configuration.GetValue<string>("Mail:Nome"));
                MailMessage myMail = new MailMessage();

                foreach (var item in EmailDestino.Split(";"))
                {
                    myMail.To.Add(item);
                }

                myMail.From = from;
                myMail.Bcc.Add(new MailAddress(configuration.GetValue<string>("Mail:EmailBcc")!));

                myMail.Subject = Assunto;
                myMail.SubjectEncoding = System.Text.Encoding.UTF8;

                if (int.Parse(DateTime.Now.ToString("HH")) < 13)
                {
                    myMail.Body = "Bom Dia, <br><br>";
                }
                else
                {
                    myMail.Body = "Boa Tarde, <br><br>";
                }

                myMail.Body += Mensagem;
                myMail.BodyEncoding = System.Text.Encoding.UTF8;
                myMail.IsBodyHtml = true;

                //Assinatura
                myMail.Body += "<br><br><i>Atenção este é um email automático, por favor não responda a este email!</i><br><br>Powered by: JKSProds - Software";

                if (anexo != null) myMail.Attachments.Add(anexo);

                mySmtpClient.SendMailAsync(myMail);
            }
            catch (SmtpException ex)
            {
                return false;
                throw new ApplicationException
                  ("SmtpException has occured: " + ex.Message);

            }
            return true;
        }
        public static bool MailPedidosEspeciaisPendentes(List<Literatura> LstLiteratura, string EmailDestino)
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

        public static bool MailTerritorioAtribuido(Territorio t, string EmailDestino)
        {
            if (string.IsNullOrEmpty(t.Stamp) || string.IsNullOrEmpty(EmailDestino)) return false;

            string Mensagem = "Abaixo segue o território atribuido a si para trabalhar num prazo máximo de 6 meses:<br><br>";

            Mensagem += "Núm. do Território: <b>" + t.Id + "</b><br>";
            Mensagem += "Nome: <b>" + t.Nome + "</b><br>";
            Mensagem += "Descrição: " + t.Descricao + "<br><br>";

            Mensagem += "<a href='" + t.Url + "' class='button'></a>";

            return EnviarMail(EmailDestino, "Território Atribuido - " + t.Nome + " (" + t.Id + ")", Mensagem, null);
        }
        public static bool MailTerritorioDevolver(Territorio t, string EmailDestino)
        {
            if (string.IsNullOrEmpty(t.Stamp) || string.IsNullOrEmpty(EmailDestino)) return false;

            string Mensagem = "Serve o presente para informar que o territorio abaixo foi devolvido:<br><br>";

            Mensagem += "Núm. do Território: <b>" + t.Id + "</b><br>";
            Mensagem += "Nome: <b>" + t.Nome + "</b><br><br>";
            Mensagem += "<b>SE AINDA O TEM FISICAMENTE POR FAVOR ENTREGUE-O AO SERVO DOS TERRITÓRIOS O MAIS DEPRESA POSSIVEL E NÃO O TRABALHE ENTRETANTO!</b>";

            return EnviarMail(EmailDestino, "Território Devolvido - " + t.Nome + " (" + t.Id + ")", Mensagem, null);
        }
    }
}
