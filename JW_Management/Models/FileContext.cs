using Google.Protobuf.WellKnownTypes;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.rtf;
using System.Collections;
using System.Reflection.Metadata;
using System.Text;
using OfficeOpenXml;

namespace JW_Management.Models
{
    public class FileContext
    {
        private string Caminho = "D:\\";
        private string CaminhoLinux = "/app/img/";

        public string ObterCaminho()
        {
#if DEBUG
            CriarPasta(Caminho);
            return Caminho;
#else
            CriarPasta(CaminhoLinux);
            return CaminhoLinux;
#endif
        }

        public string ObterCaminhoTerritorios()
        {
#if DEBUG
            CriarPasta(Caminho + "territorio\\");
            return Caminho + "territorio/";
#else
            CriarPasta(CaminhoLinux + "territorio/");
            return CaminhoLinux + "territorio/";
#endif
        }

        public string ObterCaminhoLiteratura()
        {
#if DEBUG
            CriarPasta(Caminho + "literatura\\");
            return Caminho + "literatura/";
#else
            CriarPasta(CaminhoLinux + "literatura/");
            return CaminhoLinux + "literatura/";
#endif
        }

        private bool CriarPasta(string Caminho)
        {
            try
            {
                if (!Directory.Exists(Caminho)) Directory.CreateDirectory(Caminho);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool CriarFicheiro(string Caminho, IFormFile ficheiro)
        {
            try
            {
                using (Stream fileStream = new FileStream(Caminho, FileMode.Create))
                {
                    ficheiro.CopyTo(fileStream);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public byte[] ObterFicheiro(string Caminho)
        {
            try
            {
                if (File.Exists(Caminho))
                {
                    return File.ReadAllBytes(Caminho);
                }
                else
                {
                    return File.ReadAllBytes("wwwroot/img/l_no_photo.png");
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        public bool ApagarFicheiro(string Caminho)
        {
            try
            {
                if (File.Exists(Caminho))
                {
                    File.Delete(Caminho);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool GuardarAnexoTerritorio(IFormFile file, string Name)
        {
            return CriarFicheiro(ObterCaminhoTerritorios() + Name, file);
        }

        public bool GuardarImagemLiteratura(IFormFile file, string Name)
        {
            return CriarFicheiro(ObterCaminhoLiteratura() + Name, file);
        }


        public MemoryStream PreencherFormularioS28(DbContext context)
        {
            string pdfTemplate = AppDomain.CurrentDomain.BaseDirectory + "S-28_TPO.pdf";
            var outputPdfStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, outputPdfStream);
            AcroFields pdfFormFields = pdfStamper.AcroFields;

            int x = 901;
            int mes = DateTime.Now.Month >= 3 && DateTime.Now.Month <= 8 ? 3 : 9;
            int ano = DateTime.Now.Year;

            pdfFormFields.SetField("900_0_Text", "Português");
            pdfFormFields.SetField("900_1_Text", (DateTime.Now.Month >= 3 && DateTime.Now.Month <= 8 ? "Março -> Agosto" : "Setembro -> Fevereiro") + " " + ano);

            foreach (var l in context.ObterLiteraturas(mes - 1, ano))
            {
                if (string.IsNullOrEmpty(pdfFormFields.GetField(x + "_86_S28Value"))) pdfFormFields.SetField(x + "_86_S28Value", "0");
                if (string.IsNullOrEmpty(pdfFormFields.GetField(x + "_5_S28Value"))) pdfFormFields.SetField(x + "_5_S28Value", "0");
                if (string.IsNullOrEmpty(pdfFormFields.GetField(x + "_54_S28Value"))) pdfFormFields.SetField(x + "_54_S28Value", "0");
                if (string.IsNullOrEmpty(pdfFormFields.GetField(x + "_82_S28Value"))) pdfFormFields.SetField(x + "_82_S28Value", "0");

                if (l.Linha > 0)
                {
                    pdfFormFields.SetField(x + "_" + l.Linha + "_S28Value", l.Quantidade.ToString());
                }
                else
                {
                    if (l.Tipo.Id == 1)
                    {
                        pdfFormFields.SetField(x + "_86_S28Value", (int.Parse(pdfFormFields.GetField(x + "_86_S28Value")) + l.Quantidade).ToString());
                    }
                    else if (l.Tipo.Id == 2)
                    {
                        pdfFormFields.SetField(x + "_5_S28Value", (int.Parse(pdfFormFields.GetField(x + "_5_S28Value")) + l.Quantidade).ToString());
                    }
                    else if (l.Tipo.Id == 3)
                    {
                        pdfFormFields.SetField(x + "_54_S28Value", (int.Parse(pdfFormFields.GetField(x + "_54_S28Value")) + l.Quantidade).ToString());
                    }
                    else if (l.Tipo.Id == 4)
                    {
                        pdfFormFields.SetField(x + "_82_S28Value", (int.Parse(pdfFormFields.GetField(x + "_82_S28Value")) + l.Quantidade).ToString());
                    }
                }
            }

            x++;

            for (int i = 0; i < 6; i++)
            {
                for (int j = x; j < x + 3; j++)
                {
                    pdfFormFields.SetField(j + "_86_S28Calc", "0");
                    pdfFormFields.SetField(j + "_86_S28Value", "0");
                }

                for (int j = x; j < x + 3; j++)
                {
                    pdfFormFields.SetField(j + "_5_S28Value", "0");
                    pdfFormFields.SetField(j + "_5_S28Calc", "0");
                }

                for (int j = x; j < x + 3; j++)
                {
                    pdfFormFields.SetField(j + "_54_S28Value", "0");
                    pdfFormFields.SetField(j + "_54_S28Calc", "0");
                }

                for (int j = x; j < x + 3; j++)
                {
                    pdfFormFields.SetField(j + "_82_S28Value", "0");
                    pdfFormFields.SetField(j + "_82_S28Calc", "0");
                }

                foreach (var l in context.ObterLiteraturas(mes, ano))
                {
                    if (l.Linha > 0)
                    {
                        pdfFormFields.SetField(x + "_" + l.Linha + "_S28Value", l.Entradas.ToString());
                        pdfFormFields.SetField(x + 1 + "_" + l.Linha + "_S28Value", l.Quantidade.ToString());
                        pdfFormFields.SetField(x + 2 + "_" + l.Linha + "_S28Calc", l.Saidas.ToString());
                    }
                    else
                    {
                        if (l.Tipo.Id == 1)
                        {
                            pdfFormFields.SetField(x + "_86_S28Value", (int.Parse(pdfFormFields.GetField(x + "_86_S28Value")) + l.Entradas).ToString());
                            pdfFormFields.SetField(x + 1 + "_86_S28Value", (int.Parse(pdfFormFields.GetField(x + 1 + "_86_S28Value")) + l.Quantidade).ToString());
                            pdfFormFields.SetField(x + 2 + "_86_S28Value", (int.Parse(pdfFormFields.GetField(x + 2 + "_86_S28Calc")) + l.Saidas).ToString());
                        }
                        else if (l.Tipo.Id == 2)
                        {
                            pdfFormFields.SetField(x + "_5_S28Value", (int.Parse(pdfFormFields.GetField(x + "_5_S28Value")) + l.Entradas).ToString());
                            pdfFormFields.SetField(x + 1 + "_5_S28Value", (int.Parse(pdfFormFields.GetField(x + 1 + "_5_S28Value")) + l.Quantidade).ToString());
                            pdfFormFields.SetField(x + 2 + "_5_S28Value", (int.Parse(pdfFormFields.GetField(x + 2 + "_5_S28Calc")) + l.Saidas).ToString());
                        }
                        else if (l.Tipo.Id == 3)
                        {
                            pdfFormFields.SetField(x + "_54_S28Value", (int.Parse(pdfFormFields.GetField(x + "_54_S28Value")) + l.Entradas).ToString());
                            pdfFormFields.SetField(x + 1 + "_54_S28Value", (int.Parse(pdfFormFields.GetField(x + 1 + "_54_S28Value")) + l.Quantidade).ToString());
                            pdfFormFields.SetField(x + 2 + "_54_S28Value", (int.Parse(pdfFormFields.GetField(x + 2 + "_54_S28Calc")) + l.Saidas).ToString());
                        }
                        else if (l.Tipo.Id == 4)
                        {
                            pdfFormFields.SetField(x + "_82_S28Value", (int.Parse(pdfFormFields.GetField(x + "_82_S28Value")) + l.Entradas).ToString());
                            pdfFormFields.SetField(x + 1 + "_82_S28Value", (int.Parse(pdfFormFields.GetField(x + 1 + "_82_S28Value")) + l.Quantidade).ToString());
                            pdfFormFields.SetField(x + 2 + "_82_S28Value", (int.Parse(pdfFormFields.GetField(x + 2 + "_82_S28Calc")) + l.Saidas).ToString());
                        }
                    }
                }

                x += 3;
                mes++;
                if (mes > 12)
                {
                    ano++;
                    mes = 1;
                }
            }

            pdfStamper.FormFlattening = true;
            pdfStamper.Close();

            return outputPdfStream;

        }

        public MemoryStream PreencherFormularioS13(DbContext context, List<Territorio> t, DateTime dInicio, DateTime dFim)
        {
            string pdfTemplate = AppDomain.CurrentDomain.BaseDirectory + "S-13_TPO.pdf";
            string a = DateTime.Now.Year.ToString();
            List<TipoTerritorio> z = context.ObterTiposTerritorio();
            List<MemoryStream> m = new List<MemoryStream>();


            foreach (var d in z)
            {
                int i = 0;
                List<Territorio> l = new List<Territorio>();
                foreach (var t2 in t.Where(t => t.Tipo.Id == d.Id))
                {
                    if (t2.Registros.Count / 4 + i >= 20)
                    {
                        m.Add(PreencherPaginaIndividualS13(pdfTemplate, a, d.Descricao ?? "", l));
                        l.Clear();
                        i = 0;
                    }

                    l.Add(t2);
                    i += t2.Registros.Count / 4 + 1;
                }
                if (l.Count() > 0) m.Add(PreencherPaginaIndividualS13(pdfTemplate, a, d.Descricao ?? "", l));
            }

            return CombinePdfStreams(m.ToArray());
        }

        private MemoryStream PreencherPaginaIndividualS13(string pdfTemplate, string AnoServico, string TipoTerritorio, List<Territorio> LstTerritorios)
        {
            // Create a MemoryStream to store the modified PDF
            MemoryStream memoryStream = new MemoryStream();
            Font fontB = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            // Create a PdfReader to read the existing PDF
            using (PdfReader reader = new PdfReader(pdfTemplate))
            {
                // Create a PdfStamper to modify the existing PDF
                using (PdfStamper stamper = new PdfStamper(reader, memoryStream))
                {
                    int y = 85;
                    // Get the PdfContentByte for the page
                    PdfContentByte contentByte = stamper.GetOverContent(1);

                    // Add text overlay
                    ColumnText.ShowTextAligned(contentByte, Element.ALIGN_LEFT, new Phrase(AnoServico), 140, reader.GetPageSize(1).Height - y, 0);
                    ColumnText.ShowTextAligned(contentByte, Element.ALIGN_LEFT, new Phrase(TipoTerritorio), 310, reader.GetPageSize(1).Height - y, 0);

                    y += 75;

                    foreach (var t in LstTerritorios)
                    {
                        int x = 39;
                        ColumnText.ShowTextAligned(contentByte, Element.ALIGN_LEFT, new Phrase(t.Id), x, reader.GetPageSize(1).Height - y, 0);

                        x += 35;
                        if (t.Registros.Count > 0) ColumnText.ShowTextAligned(contentByte, Element.ALIGN_LEFT, new Phrase(t.Registros!.Last().UltimoMovimento.DataMovimento.ToString("dd/MM/yy")), x, reader.GetPageSize(1).Height - y, 0);

                        x += 60;
                        for (int i = 0; i < t.Registros.Count(); i++)
                        {
                            RegistroTerritorio l = t.Registros[i];
                            if (i == 4) { y += 31; x = 134; }

                            //Nome Publicador
                            ColumnText.ShowTextAligned(contentByte, Element.ALIGN_LEFT, new Phrase(l.Entrada.Publicador.Nome, fontB), x, reader.GetPageSize(1).Height - y + 10, 0);

                            //Entrada
                            ColumnText.ShowTextAligned(contentByte, Element.ALIGN_LEFT, new Phrase(l.Entrada.DataMovimento.ToShortDateString(), fontB), x, reader.GetPageSize(1).Height - y - 7, 0);

                            //Saida
                            ColumnText.ShowTextAligned(contentByte, Element.ALIGN_LEFT, new Phrase((l.Saida.DataMovimento == DateTime.MinValue ? "" : l.Saida.DataMovimento.ToShortDateString()), fontB), x + 52, reader.GetPageSize(1).Height - y - 7, 0);
                            x += 108;
                        }

                        y += 31;
                    }

                }
            }

            // Reset the MemoryStream position to the beginning before returning
            memoryStream.Position = 0;

            return memoryStream;
        }

        private MemoryStream CombinePdfStreams(params MemoryStream[] pdfStreams)
        {
            MemoryStream combinedStream = new MemoryStream();
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            PdfCopy copy = new PdfCopy(document, combinedStream);
            document.Open();

            foreach (MemoryStream pdfStream in pdfStreams)
            {
                PdfReader reader = new PdfReader(pdfStream.ToArray());
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfImportedPage page = copy.GetImportedPage(reader, i);
                    copy.AddPage(page);
                }
                copy.FreeReader(reader);
            }

            document.Close();
            return combinedStream;
        }

        public List<Designacao> ImportarDesignacoes(IFormFile file, DbContext context)
        {
            var filePath = Path.GetTempFileName();
            List<Designacao> LstDesignacao = new List<Designacao>();
            List<TipoDesignacao> LstTipos = context.ObterTiposDesignacao();
            List<Publicador> LstPublicadores = context.ObterPublicadores(false);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new System.IO.FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                // Check if the specified column is within the bounds of the worksheet
                for (int colIndex = 2; colIndex <= colCount; colIndex++)
                {
                    bool a = colIndex % 2 == 1;
                    string s = a ? worksheet.Cells[2, colIndex - 1].Text : worksheet.Cells[2, colIndex].Text;

                    // Iterate through each row in the specified column
                    for (int rowIndex = 3; rowIndex <= rowCount; rowIndex++)
                    {
                        string p = worksheet.Cells[rowIndex, colIndex].Text.Contains("Sala") || worksheet.Cells[rowIndex, colIndex].Text.Contains("Salão") ? "" : worksheet.Cells[rowIndex, colIndex].Text;
                        string d = string.IsNullOrEmpty(worksheet.Cells[rowIndex, 1].Text) ? worksheet.Cells[rowIndex - 1, 1].Text : worksheet.Cells[rowIndex, 1].Text;
                        if (!string.IsNullOrEmpty(p) && !string.IsNullOrEmpty(d))
                        {
                            foreach (var pub in p.Split("|"))
                            {
                                LstDesignacao.Add(new Designacao()
                                {
                                    Stamp = DateTime.Now.Ticks.ToString(),
                                    StampReuniao = s.Trim().Replace(" ", ""),
                                    SemanaReuniao = s.Trim(),
                                    NomeDesignacao = d.Trim(),
                                    NomePublicador = pub.Trim(),
                                    Publicador = LstPublicadores.Where(u => u.Nome.StartsWith(pub.Trim())).DefaultIfEmpty(new Publicador()).First(),
                                    TipoDesignacao = LstTipos.Where(t => t.DescricaoAdicional == d.Trim()).DefaultIfEmpty(new TipoDesignacao()).First(),
                                    Local = a ? "Sala" : "Auditorio"
                                });
                                //if (LstTipos.Where(u => u.DescricaoAdicional == LstDesignacao.Last().TipoDesignacao.DescricaoAdicional).Count() > 1) LstTipos.Remove(LstTipos.Where(u => u.Id == LstDesignacao.Last().TipoDesignacao.Id).First());
                            }
                        }
                    }
                }
            }
            return LstDesignacao;
        }

        public MemoryStream PreencherFormularioS140(Reuniao r)
        {
            string pdfTemplate = AppDomain.CurrentDomain.BaseDirectory + "S-140_TPO.pdf";
            var outputPdfStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(pdfTemplate);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, outputPdfStream);
            AcroFields pdfFormFields = pdfStamper.AcroFields;
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            BaseFont baseFontBold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            pdfFormFields.SetFieldProperty("SemanaReuniao", "textfont", baseFontBold, null);
            pdfFormFields.SetField("SemanaReuniao", r.SemanaReuniao);

            foreach (var d in r.Designacoes.Where(d => d.Atribuida).DistinctBy(d => d.Distinct).ToList())
            {
                pdfFormFields.SetFieldProperty(d.TipoDesignacao.Id + "_Designacao", "textfont", baseFontBold, null);
                pdfFormFields.SetField(d.TipoDesignacao.Id + "_Designacao", d.NomeDesignacao + (d.NMin > 0 ? " (" + d.NMin + " min.)" : ""));
                pdfFormFields.SetFieldProperty(d.TipoDesignacao.Id + "_Pub" + (d.SalaAdicional ? "_1" : ""), "textfont", baseFont, null);
                pdfFormFields.SetField(d.TipoDesignacao.Id + "_Pub" + (d.SalaAdicional ? "_1" : ""), string.Join(" | ", r.Designacoes.Where(i => i.Distinct == d.Distinct).Select(i => r.Designacoes.Where(i => i.Distinct == d.Distinct).Count() > 1 ? i.Publicador.NomeCurto : i.Publicador.Nome).ToArray()));
            }

            if (r.Canticos.Count() >= 3)
            {
                pdfFormFields.SetField("1_Cant", r.Canticos[0].Id.ToString());
                pdfFormFields.SetField("2_Cant", r.Canticos[1].Id.ToString());
                pdfFormFields.SetField("3_Cant", r.Canticos[2].Id.ToString());
            }

            pdfStamper.FormFlattening = true;
            pdfStamper.Close();

            return outputPdfStream;

        }
    }
}
