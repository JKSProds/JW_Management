using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace JW_Management.Models;

// PDF dos programas: quadro mensal (A4 horizontal), serviço de campo e testemunho público (A4 vertical).
// Visual a preto e branco: tons de cinza, linhas finas e sem cores; só fontes base do PDF (sem ficheiros de fonte).
public static class PdfProgramas
{
    private static readonly CultureInfo Pt = new("pt-PT");
    private static readonly BaseColor Tinta = new(25, 25, 25);
    private static readonly BaseColor Suave = new(105, 105, 105);
    private static readonly BaseColor Cinza = new(236, 236, 236);
    private static readonly BaseColor Linha = new(200, 200, 200);

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

    // Cabeçalho comum aos três PDF: título a preto com um filete por baixo.
    private static void Cabecalho(Document doc, Fontes f, string sobre, string titulo, string direita, float tam = 20)
    {
        var t = new PdfPTable(2) { WidthPercentage = 100 };
        t.SetWidths(new[] { 3f, 2f });
        var esq = new PdfPCell { Border = Rectangle.BOTTOM_BORDER, BorderWidth = 1, BorderColor = Tinta, PaddingBottom = 6, PaddingLeft = 0 };
        esq.AddElement(new Paragraph(sobre.ToUpperInvariant(), new Font(f.Negrito, 7.5f, Font.NORMAL, Suave)) { SpacingAfter = 1 });
        esq.AddElement(new Paragraph(titulo, new Font(f.Negrito, tam, Font.NORMAL, Tinta)));
        t.AddCell(esq);
        t.AddCell(new PdfPCell(new Phrase(direita, new Font(f.Negrito, 10, Font.NORMAL, Tinta))) { Border = Rectangle.BOTTOM_BORDER, BorderWidth = 1, BorderColor = Tinta, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_BOTTOM, PaddingBottom = 6, PaddingRight = 0 });
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
        var fFaixa = new Font(f.Negrito, 7.5f, Font.NORMAL, BaseColor.White); var fSecao = new Font(f.Negrito, 6.6f, Font.NORMAL, Tinta);
        var fSemana = new Font(f.Negrito, 6.8f, Font.NORMAL, Tinta); var fEvento = new Font(f.Negrito, 7, Font.NORMAL, Tinta);

        using var ms = new MemoryStream();
        var doc = new Document(new Rectangle(842, 595), 26, 26, 16, 12); // A4 horizontal, sem /Rotate
        PdfWriter.GetInstance(doc, ms);
        doc.Open();
        Cabecalho(doc, f, "Programa de atividades", p.Mes, p.Congregacao, 16);

        int n = p.Semanas.Count;
        var t = new PdfPTable(n + 1) { WidthPercentage = 100 };
        t.SetWidths(new[] { 2.3f }.Concat(Enumerable.Repeat(2f, n)).ToArray());

        t.AddCell(Cel("", fValor, h: 16, vpad: .4f));
        foreach (var s in p.Semanas) t.AddCell(Cel(s.Titulo, fSemana, borda: Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER, h: 16, vpad: .4f));

        var linhasEvento = new[] { "min1", "min2", "min3", "std", "stl", "sto" };
        foreach (var sec in p.Secoes)
        {
            // faixa preta = reunião; cinza = parte da reunião; espaço maior antes de cada reunião
            t.AddCell(Cel("", fValor, borda: Rectangle.NO_BORDER, span: n + 1, h: sec.Faixa ? 7 : 2, pad: 0, vpad: 0));
            if (sec.Faixa) t.AddCell(Cel(sec.Titulo, fFaixa, Element.ALIGN_LEFT, fundo: Tinta, span: n + 1, h: 11, pad: 6, vpad: .4f));
            else t.AddCell(Cel(sec.Titulo, fSecao, Element.ALIGN_LEFT, fundo: Cinza, span: n + 1, h: 10, pad: 6, vpad: .4f));
            foreach (var l in sec.Linhas)
            {
                t.AddCell(Cel(l.Rotulo, fRotulo, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 9.8f, pad: 6, vpad: .4f));
                foreach (var s in p.Semanas)
                {
                    var idx = Array.IndexOf(linhasEvento, l.Id);
                    if (s.Evento.Count > 0 && idx >= 0) t.AddCell(Cel(idx % 3 < s.Evento.Count ? s.Evento[idx % 3] : "", fEvento, fundo: Cinza, borda: Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER, h: 9.8f, vpad: .4f));
                    else t.AddCell(Cel(Valor(s, l.Id), fValor, borda: Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER, h: 9.8f, vpad: .4f));
                }
            }
        }
        doc.Add(t);
        Rodape(doc, f);
        doc.Close();
        return ms.ToArray();
    }

    // Programas guardados antes da separação dos cânticos têm «12 - 45 - 100» numa só chave (cmm/cwm).
    private static string Valor(ProgramaSemana s, string id)
    {
        if (s.Valores.TryGetValue(id, out var v)) return v;
        if (id.Length == 4 && id[..3] is "cmm" or "cwm" && int.TryParse(id[3..], out var i) && s.Valores.TryGetValue(id[..3], out var junto))
        { var p = junto.Split(" - "); return i <= p.Length ? p[i - 1] : ""; }
        return "";
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
        var fCab = new Font(f.Negrito, 7.5f, Font.NORMAL, Tinta);

        using var ms = new MemoryStream();
        var doc = new Document(new Rectangle(595, 842), 36, 36, 30, 28);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();
        Cabecalho(doc, f, "Serviço de campo", $"{ProgramaServico.NomeMes(mes)} {ano}", congregacao);

        var fCabCol = new Font(f.Negrito, 6.8f, Font.NORMAL, Suave);
        // uma tabela por semana (segunda a domingo), cada uma com a sua faixa e sem partir ao meio
        foreach (var sem in entradas.GroupBy(e => { var d = DateTime.ParseExact(e.Data, "yyyy-MM-dd", CultureInfo.InvariantCulture); return d.AddDays(-(((int)d.DayOfWeek + 6) % 7)); }).OrderBy(g => g.Key))
        {
            var t = new PdfPTable(4) { WidthPercentage = 100, KeepTogether = true, SpacingAfter = 10 };
            t.SetWidths(new[] { 1.2f, 0.9f, 2.2f, 2.4f });
            var fim = sem.Key.AddDays(6);
            t.AddCell(Cel(sem.Key.Month == fim.Month ? $"Semana de {sem.Key.Day} a {fim.Day} de {ProgramaServico.NomeMes(sem.Key.Month)}" : $"Semana de {sem.Key.Day} de {ProgramaServico.NomeMes(sem.Key.Month)} a {fim.Day} de {ProgramaServico.NomeMes(fim.Month)}", fCab, Element.ALIGN_LEFT, fundo: Cinza, span: 4, h: 17, pad: 8));
            foreach (var h in new[] { "DIA", "HORA", "LOCAL", "DIRIGENTE" }) t.AddCell(Cel(h, fCabCol, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 12, pad: 8));
            foreach (var e in sem.OrderBy(x => x.Data).ThenBy(x => x.Hora))
            {
                t.AddCell(Cel(Dia(e.Data), fNeg, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 17, pad: 8));
                t.AddCell(Cel(e.Hora, fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 17, pad: 8));
                t.AddCell(Cel(e.Local, fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 17, pad: 8));
                t.AddCell(Cel(e.Dirigente == "" ? "—" : e.Dirigente, fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 17, pad: 8));
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
        var fVazio = new Font(f.Italico, 9, Font.NORMAL, Suave); var fLocal = new Font(f.Negrito, 9, Font.NORMAL, Tinta);

        using var ms = new MemoryStream();
        var doc = new Document(new Rectangle(595, 842), 36, 36, 30, 28);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();
        Cabecalho(doc, f, "Testemunho público", $"{ProgramaServico.NomeMes(mes)} {ano}", congregacao);

        foreach (var local in turnos.GroupBy(t => t.Local).OrderBy(g => g.Key))
        {
            var t = new PdfPTable(3) { WidthPercentage = 100, KeepTogether = false, SpacingAfter = 10 };
            t.SetWidths(new[] { 1.2f, 1.6f, 4f });
            t.AddCell(Cel(local.Key == "" ? "Sem local" : local.Key, fLocal, Element.ALIGN_LEFT, fundo: Cinza, span: 3, h: 17, pad: 8));
            foreach (var e in local.OrderBy(x => x.Data).ThenBy(x => x.Inicio))
            {
                t.AddCell(Cel(Dia(e.Data), fNeg, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 16, pad: 7));
                t.AddCell(Cel($"{e.Inicio} – {e.Fim}", fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 16, pad: 7));
                t.AddCell(Cel(e.Pessoas.Count == 0 ? "por atribuir" : string.Join("  ·  ", e.Pessoas), e.Pessoas.Count == 0 ? fVazio : fValor, Element.ALIGN_LEFT, Rectangle.BOTTOM_BORDER, h: 16, pad: 7));
            }
            doc.Add(t);
        }
        Rodape(doc, f);
        doc.Close();
        return ms.ToArray();
    }
}
