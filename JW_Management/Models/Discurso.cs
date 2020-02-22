using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JW_Management.Models
{
    public class Discurso
    {
        public int IdDiscurso { get; set; }
        public string NomeOrador { get; set; }
        public string CongregacaoOrador { get; set; }
        public string ContactoOrador { get; set; }
        public DateTime DataDiscurso { get; set; }
        public int IdTemaDiscurso { get; set; }
        public string TemaDiscurso { get; set; }

        public bool Dentro_Fora { get; set; }
        public string Observacoes { get; set; }
    }
}
