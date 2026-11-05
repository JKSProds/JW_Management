using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace JW_Management.Models;

// PDF dos programas: quadro mensal (A4 horizontal), serviço de campo e testemunho público (A4 vertical).
// Visual limpo: faixas de cor, linhas finas e zebra; só fontes base do PDF (sem ficheiros de fonte).
public static class PdfProgramas
{
    private static readonly CultureInfo Pt = new("pt-PT");
    private static readonly BaseColor Marca = new(42, 77, 208);
    private static readonly BaseColor Escuro = new(24, 39, 96);
    private static readonly BaseColor Tinta = new(33, 41, 66);
    private static readonly BaseColor Suave = new(98, 108, 132);
    private static readonly BaseColor Tint = new(232, 237, 251);
    private static readonly BaseColor Zebra = new(246, 248, 252);
    private static readonly BaseColor Linha = new(221, 227, 238);
    private static readonly BaseColor FimDeSemana = new(255, 248, 225);
    private static readonly BaseColor Ambar = new(255, 226, 138);
    private static readonly BaseColor AmbarTexto = new(102, 66, 0);

    private record Fontes(BaseFont Normal, BaseFont Negrito, BaseFont Italico);
    private static Fontes Carregar() => new(
        BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED),
        BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED),
        BaseFont.CreateFont(BaseFont.HELVETICA_OBLIQUE, BaseFont.CP1252, BaseFont.NOT_EMBEDDED));

    private static PdfPCell Cel(string txt, Font f, int al = Element.ALIGN_CENTER, int borda = Rectangle.NO_BORDER, BaseColor? fundo = null, int span = 1, float h = 11, float pad = 3, float vpad = 2)
    {
        var c = new PdfPCell(new Phrase(txt, f)) { HorizontalAlignment = al, VerticalAlignment = Element.ALIGN_MIDDLE, Border = borda, BorderWidth = .5f, BorderColor = Linha, Colspan = span, MinimumHeight = h, PaddingTop = vpad, PaddingBottom = vpad + 1, PaddingLeft = pad, PaddingRight = pad };
        if (fundo != null) c.BackgroundColor = fundo;
        return c;
    }

    // Cabeçalho comum aos três PDF: faixa azul escura com o título a branco e filete de cor.
    private static void Cabecalho(Document doc, Fontes f, string sobre, string titulo, string direita, float tam = 20)
    {
        var t = new PdfPTable(2) { WidthPercentage = 100 };
        t.SetWidths(new[] { 3f, 2f });
        var esq = new PdfPCell { Border = Rectangle.NO_BORDER, BackgroundColor = Escuro, PaddingLeft = 14, PaddingTop = 9, PaddingBottom = 9 };
        esq.AddElement(new Paragraph(sobre.ToUpperInvariant(), new Font(f.Negrito, 7.5f, Font.NORMAL, new BaseColor(160, 184, 255))) { SpacingAfter = 1 });
        esq.AddElement(new Paragraph(titulo, new Font(f.Negrito, tam, Font.NORMAL, BaseColor.White)));
        t.AddCell(esq);
        t.AddCell(new PdfPCell(new Phrase(direita, new Font(f.Negrito, 10, Font.NORMAL, BaseColor.White))) { Border = Rectangle.NO_BORDER, BackgroundColor = Escuro, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, PaddingRight = 14 });
        t.AddCell(new PdfPCell(new Phrase(" ", new Font(f.Normal, 2))) { Colspan = 2, Border = Rectangle.NO_BORDER, BackgroundColor = Marca, FixedHeight = 3 });
        doc.Add(t);
        doc.Add(new Paragraph(" ", new Font(f.Normal, 6)));
    }

    private static void Rodape(Document doc, Fontes f) =>
        doc.Add(new Paragraph($"Gerado a {DateTime.Now:dd/MM/yyyy}", new Font(f.Italico, 6.5f, Font.NORMAL, Suave)) { Alignment = Element.ALIGN_RIGHT, SpacingBefore = 4 });

    // ---------- Quadro mensal ----------
    public static byte[] Gerar(ProgramaMensal p)
    {
        var f = Carregar();
        var fValor = new Font(f.Normal, 7, Font.NORMAL, Tinta); var fRotulo = new Font(f.Negrito, 6.8f, Font.NORMAL, Suave);
        var fFaixa = new Font(f.Negrito, 7.2f, Font.NORMAL, BaseColor.White); var fSecao = new Font(f.Negrito, 6.8f, Font.NORMAL, Marca);
        var fSemana = new Font(f.Negrito, 6.8f, Font.NORMAL, BaseColor.White); var fEvento = new Font(f.Negrito, 7, Font.NORMAL, AmbarTexto);

        using var ms = new MemoryStream();
        var doc = new Document(new Rectangle(842, 595), 26, 26, 16, 12); // A4 horizontal, sem /Rotate
        PdfWriter.GetInstance(doc, ms);
        doc.Open();
        Cabecalho(doc, f, "Programa de atividades", p.Mes, p.Congregacao, 16);

        int n = p.Semanas.Count;
        var t = new PdfPTable(n + 1) { WidthPercentage = 100 };
        t.SetWidths(new[] { 2.3f }.Concat(Enumerable.Repeat(2f, n)).ToArray());

        t.AddCell(Cel("", fValor, h: 16, vpad: 1));
        foreach (var s in p.Semanas) t.AddCell(Cel(s.Titulo, fSemana, fundo: Marca, h: 16, vpad: 1));

        var linhasEvento = new[] { "min1", "min2", "min3", "std", "stl", "sto" };
        foreach (var sec in p.Secoes)
        {
            t.AddCell(Cel("", fValor, borda: Rectangle.NO_BORDER, span: n + 1, h: 2, pad: 0, vpad: 0));
            if (sec.Titulo != "")
            {
                if (sec.Faixa) t.AddCell(Cel(sec.Titulo, fFaixa, Element.ALIGN_LEFT, fundo: Escuro, span: n + 1, h: 10, pad: 6, vpad: 1));
                else t.AddCell(Cel(sec.Titulo, fSecao, Element.ALIGN_LEFT, fundo: Tint, span: n + 1, h: 10.5f, pad: 6, vpad: 1));
            }
            int z = 0;
            foreach (var l in sec.Linhas)
            {
                var fundo = z++ % 2 == 1 ? Zebra : null;
                t.AddCell(Cel(l.Rotulo, fRotulo, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, fundo, h: 10.5f, pad: 6, vpad: 1));
                foreach (var s in p.Semanas)
                {
                    var idx = Array.IndexOf(linhasEvento, l.Id);
                    if (s.Evento.Count > 0 && idx >= 0) t.AddCell(Cel(idx % 3 < s.Evento.Count ? s.Evento[idx % 3] : "", fEvento, fundo: Ambar, borda: Rectangle.BOTTOM_BORDER, h: 10.5f, vpad: 1));
                    else t.AddCell(Cel(s.Valores.GetValueOrDefault(l.Id, ""), fValor, borda: Rectangle.BOTTOM_BORDER, fundo: fundo, h: 10.5f, vpad: 1));
                }
            }
        }
        doc.Add(t);
        Rodape(doc, f);
        doc.Close();
        return ms.ToArray();
    }

    private static string Dia(string iso)
    {
        var d = DateTime.ParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var nome = Pt.DateTimeFormat.GetAbbreviatedDayName(d.DayOfWeek).TrimEnd('.');
        return char.ToUpper(nome[0]) + nome[1..] + " " + d.Day;
    }

    // ---------- Serviço de campo (vertical) ----------
    public static byte[] GerarCampo(List<CampoEntrada> entradas, string congregacao, int ano, int mes)
    {
        var f = Carregar();
        var fValor = new Font(f.Normal, 9, Font.NORMAL, Tinta); var fNeg = new Font(f.Negrito, 9, Font.NORMAL, Tinta);
        var fSemana = new Font(f.Negrito, 8, Font.NORMAL, Marca); var fCab = new Font(f.Negrito, 7.5f, Font.NORMAL, BaseColor.White);

        using var ms = new MemoryStream();
        var doc = new Document(new Rectangle(595, 842), 36, 36, 30, 28);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();
        Cabecalho(doc, f, "Serviço de campo", $"{ProgramaServico.NomeMes(mes)} {ano}", congregacao);

        var fCabCol = new Font(f.Negrito, 6.8f, Font.NORMAL, Marca);
        // uma tabela por semana (segunda a domingo), cada uma com a sua faixa e sem partir ao meio
        foreach (var sem in entradas.GroupBy(e => { var d = DateTime.ParseExact(e.Data, "yyyy-MM-dd", CultureInfo.InvariantCulture); return d.AddDays(-(((int)d.DayOfWeek + 6) % 7)); }).OrderBy(g => g.Key))
        {
            var t = new PdfPTable(4) { WidthPercentage = 100, KeepTogether = true, SpacingAfter = 10 };
            t.SetWidths(new[] { 1.2f, 0.9f, 2.2f, 2.4f });
            var fim = sem.Key.AddDays(6);
            t.AddCell(Cel(sem.Key.Month == fim.Month ? $"Semana de {sem.Key.Day} a {fim.Day} de {ProgramaServico.NomeMes(sem.Key.Month)}" : $"Semana de {sem.Key.Day} de {ProgramaServico.NomeMes(sem.Key.Month)} a {fim.Day} de {ProgramaServico.NomeMes(fim.Month)}", fCab, Element.ALIGN_LEFT, fundo: Marca, span: 4, h: 17, pad: 8));
            foreach (var h in new[] { "DIA", "HORA", "LOCAL", "DIRIGENTE" }) t.AddCell(Cel(h, fCabCol, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, Tint, h: 12, pad: 8));
            int z = 0;
            foreach (var e in sem.OrderBy(x => x.Data).ThenBy(x => x.Hora))
            {
                var fds = DateTime.ParseExact(e.Data, "yyyy-MM-dd", CultureInfo.InvariantCulture).DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                var fundo = fds ? FimDeSemana : z++ % 2 == 1 ? Zebra : null;
                t.AddCell(Cel(Dia(e.Data), fNeg, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, fundo, h: 17, pad: 8));
                t.AddCell(Cel(e.Hora, fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, fundo, h: 17, pad: 8));
                t.AddCell(Cel(e.Local, fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, fundo, h: 17, pad: 8));
                t.AddCell(Cel(e.Dirigente == "" ? "—" : e.Dirigente, fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, fundo, h: 17, pad: 8));
            }
            doc.Add(t);
        }
        Rodape(doc, f);
        doc.Close();
        return ms.ToArray();
    }

    // ---------- Testemunho público (vertical) ----------
    public static byte[] GerarTestemunho(List<TestemunhoEntrada> turnos, string congregacao, int ano, int mes)
    {
        var f = Carregar();
        var fValor = new Font(f.Normal, 9, Font.NORMAL, Tinta); var fNeg = new Font(f.Negrito, 9, Font.NORMAL, Tinta);
        var fVazio = new Font(f.Italico, 9, Font.NORMAL, Suave); var fLocal = new Font(f.Negrito, 9, Font.NORMAL, BaseColor.White);

        using var ms = new MemoryStream();
        var doc = new Document(new Rectangle(595, 842), 36, 36, 30, 28);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();
        Cabecalho(doc, f, "Testemunho público", $"{ProgramaServico.NomeMes(mes)} {ano}", congregacao);

        foreach (var local in turnos.GroupBy(t => t.Local).OrderBy(g => g.Key))
        {
            var t = new PdfPTable(3) { WidthPercentage = 100, KeepTogether = false, SpacingAfter = 10 };
            t.SetWidths(new[] { 1.2f, 1.6f, 4f });
            t.AddCell(Cel(local.Key == "" ? "Sem local" : local.Key, fLocal, Element.ALIGN_LEFT, fundo: Marca, span: 3, h: 17, pad: 8));
            int z = 0;
            foreach (var e in local.OrderBy(x => x.Data).ThenBy(x => x.Inicio))
            {
                var fundo = z++ % 2 == 1 ? Zebra : null;
                t.AddCell(Cel(Dia(e.Data), fNeg, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, fundo, h: 16, pad: 7));
                t.AddCell(Cel($"{e.Inicio} – {e.Fim}", fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, fundo, h: 16, pad: 7));
                t.AddCell(Cel(e.Pessoas.Count == 0 ? "por atribuir" : string.Join("  ·  ", e.Pessoas), e.Pessoas.Count == 0 ? fVazio : fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, fundo, h: 16, pad: 7));
            }
            doc.Add(t);
        }
        Rodape(doc, f);
        doc.Close();
        return ms.ToArray();
    }
}
