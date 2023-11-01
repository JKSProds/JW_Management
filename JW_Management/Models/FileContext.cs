using iTextSharp.text.pdf;
using System.Collections;

namespace JW_Management.Models
{
    public class FileContext
    {
        private string Caminho = "C:\\Users\\Jorge Monteiro\\Desktop\\";
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
            CriarPasta(Caminho + "Territorios\\");
            return Caminho + "Territorios\\";
#else
            CriarPasta(CaminhoLinux + "territorio/");
            return CaminhoLinux + "territorio/";
#endif
        }

        public string ObterCaminhoLiteratura()
        {
#if DEBUG
            CriarPasta(Caminho + "Literatura\\");
            return Caminho + "Literatura\\";
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



    }
}
