using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JW_Management.Models
{
    public class Discurso
    {
     
        public int IdDiscurso { get; set; }
        [Display(Name = "Nome do Orador")]
        public string NomeOrador { get; set; }
        [Display(Name = "Congregação")]
        public string CongregacaoOrador { get; set; }
        [Display(Name = "Contacto")]
        public string ContactoOrador { get; set; }
        [Display(Name = "E-Mail")]
        public string EmailOrador { get; set; }
        [Display(Name = "Data do Discurso")]
        public DateTime DataDiscurso { get; set; }
        public int IdTemaDiscurso { get; set; }
        [Display(Name = "Tema do Discurso")]
        public string TemaDiscurso { get; set; }
        [Display(Name = "Este discurso vai ser realizado na congregação de origem?")]
        public bool Dentro_Fora { get; set; }
        [Display(Name = "Observações Adicionais")]
        [DataType(DataType.MultilineText)]
        public string Observacoes { get; set; }
    }
}
