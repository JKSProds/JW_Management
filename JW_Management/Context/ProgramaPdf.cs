using iTextSharp.text;
using iTextSharp.text.pdf;

namespace JW_Management.Models;

// PDF do programa mensal: A4 horizontal, uma coluna por semana, como o quadro de atividades da congregação.
public static class ProgramaPdf
{
    private static readonly BaseColor Faixa = new(189, 215, 238);
    private static readonly BaseColor Azul = new(31, 56, 140);
    private static readonly BaseColor Amarelo = new(255, 255, 0);

    public static byte[] Gerar(ProgramaMensal p)
    {
        var normal = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        var negrito = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        var italico = BaseFont.CreateFont(BaseFont.HELVETICA_BOLDOBLIQUE, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        var fValor = new Font(normal, 7); var fRotulo = new Font(negrito, 7); var fSecao = new Font(italico, 7, Font.NORMAL, Azul);
        var fSemana = new Font(negrito, 6.5f); var fAmarelo = new Font(negrito, 7);

        using var ms = new MemoryStream();
        var doc = new Document(new Rectangle(842, 595), 24, 24, 22, 18); // A4 horizontal, sem /Rotate
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        doc.Add(new Paragraph(p.Titulo, new Font(negrito, 15)) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 8 });

        int n = p.Semanas.Count;
        var t = new PdfPTable(n + 1) { WidthPercentage = 100 };
        t.SetWidths(new[] { 1.9f }.Concat(Enumerable.Repeat(2f, n)).ToArray());

        PdfPCell C(string txt, Font f, int al = Element.ALIGN_CENTER, int borda = Rectangle.BOX, BaseColor? fundo = null, int span = 1, float h = 11)
        {
            var c = new PdfPCell(new Phrase(txt, f)) { HorizontalAlignment = al, VerticalAlignment = Element.ALIGN_MIDDLE, Border = borda, BorderWidth = .5f, Colspan = span, MinimumHeight = h, PaddingTop = 1, PaddingBottom = 2, PaddingLeft = 2, PaddingRight = 2 };
            if (fundo != null) c.BackgroundColor = fundo;
            return c;
        }
        PdfPCell Vazio(int span = 1, float h = 5) => C("", fValor, borda: Rectangle.NO_BORDER, span: span, h: h);

        // Cabeçalho das semanas
        t.AddCell(Vazio(h: 18));
        foreach (var s in p.Semanas) t.AddCell(C(s.Titulo, fSemana, borda: Rectangle.NO_BORDER, h: 18));

        // Células de evento (amarelo): 3 linhas do ministério e 3 do estudo de A Sentinela
        var linhasEvento = new[] { "min1", "min2", "min3", "std", "stl", "sto" };

        foreach (var sec in p.Secoes)
        {
            t.AddCell(Vazio(n + 1, 4));
            if (sec.Titulo == "") { /* linha avulsa (limpeza), sem título */ }
            else if (sec.Faixa) t.AddCell(C(sec.Titulo, fSecao, borda: Rectangle.BOX, fundo: Faixa, span: n + 1));
            else
            {
                t.AddCell(C(sec.Titulo, fSecao, Element.ALIGN_LEFT, Rectangle.NO_BORDER, span: Math.Min(2, n + 1), h: 10));
                for (int i = 0; i < n - 1; i++) t.AddCell(Vazio(h: 10));
            }
            foreach (var l in sec.Linhas)
            {
                t.AddCell(C(l.Rotulo, fRotulo));
                foreach (var s in p.Semanas)
                {
                    var idx = Array.IndexOf(linhasEvento, l.Id);
                    if (s.Evento.Count > 0 && idx >= 0) t.AddCell(C(idx % 3 < s.Evento.Count ? s.Evento[idx % 3] : "", fAmarelo, fundo: Amarelo));
                    else t.AddCell(C(s.Valores.GetValueOrDefault(l.Id, ""), fValor));
                }
            }
        }
        doc.Add(t);
        doc.Close();
        return ms.ToArray();
    }
}
