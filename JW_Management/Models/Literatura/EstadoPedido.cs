using System;
namespace JW_Management.Models
{
    public class EstadoPedido
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public bool Fornecido { get { return Descricao == "Fornecido"; } }

        public EstadoPedido()
        {
            this.Id = 1;
            this.Descricao = "Pendente";
        }

        public EstadoPedido(string descricao)
        {
            this.Id = 1;
            this.Descricao = descricao;
        }
    }
}

