namespace JW_Management.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "NOT/DEFINED";
        public string Iniciais => Nome[0].ToString();
        public string Logotipo => $"/img/tenants/{Id}/logo_website.png";
        public string ConnectionString { get; set; }
        public double DbVersion { get; set; }

        public string NomeCongregacao { get; set; }
        public string TelefoneCongregacao { get; set; }
        public string MoradaCongregacao { get; set; }
        public string JWApiIdCongregacao { get; set; }

        public string Email { get; set; }
        public string EmailPassword { get; set; }
        public string EmailSMTPPort { get; set; }
        public string EmailHost { get; set; }
    }
}