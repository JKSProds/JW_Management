using System.Security.Claims;

namespace JW_Management.Models
{
    public static class ClaimsPrincipalExtension
    {
        public static string ObterNomeCompleto(this ClaimsPrincipal principal)
        {
            var firstName = principal.Claims.Where(c => c.Type == ClaimTypes.GivenName).First();
            return $"{firstName.Value}";
        }

        public static string ObterCong(this ClaimsPrincipal principal)
        {
            var cong = principal.Claims.Where(c => c.Type == ClaimTypes.PrimaryGroupSid).First();
            return $"{cong.Value}";
        }
        public static string ObterImgUtilizador(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Where(c => c.Type == ClaimTypes.Thumbprint).Count() == 0) return "";
            var img = principal.Claims.Where(c => c.Type == ClaimTypes.Thumbprint).First();
            return img.Value;
        }

        public static int ObterTenant(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Where(c => c.Type == ClaimTypes.Sid).Count() == 0) return 0;
            var tenant = principal.Claims.Where(c => c.Type == ClaimTypes.Sid).First();
            return int.Parse(tenant.Value);
        }
        
        public static int ObterTenantOriginal(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Where(c => c.Type == ClaimTypes.PrimaryGroupSid).Count() == 0) return 0;
            var tenant = principal.Claims.Where(c => c.Type == ClaimTypes.PrimaryGroupSid).First();
            return int.Parse(tenant.Value);
        }   
        public static int ObterId(this ClaimsPrincipal principal)
        {
            if (principal.Claims.Where(c => c.Type == ClaimTypes.Name).Count() == 0) return 0;
            var id = principal.Claims.Where(c => c.Type == ClaimTypes.Name).First();
            return int.Parse(id.Value);
        }
    }
    
    public class Publicador
    {
        private readonly HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? NomeCurto { get {return this.Nome.Split(" ").First() + (this.Nome.Split(" ").Count() > 1 ? " " + this.Nome.Split(" ").Last()[0] + "." : "");} }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Telemovel { get; set; }
        public string? Morada { get; set; }
        public Grupo? Grupo { get; set; }

        public List<Movimentos>? Movimentos { get; set; }
        public List<Literatura>? Pedidos { get; set; }
        public List<Territorio>? Territorios { get; set; }
        public int TipoUtilizador { get; set; } //0 - Admin | 10 - Assistente | 20 - Coordenador | 30 - Secretario | 40 - Sup. Serviço | 50 - Servo Contas | 60 - Servo Literatura | 70 - Servo Territórios | 80 - Servo Ministerial | 90 - Publicador
        
        public string GetUrl => "https://" + httpContextAccessor.HttpContext?.Request.Host.Value;
        
        public string Role
        {
            get
            {
                switch (this.TipoUtilizador)
                {
                    case 0: return "Admin";
                    case 10: return "Assistente";
                    case 20: return "Coordenador";
                    case 30: return "Secretario";
                    case 40: return "Servico";
                    case 50: return "Contas";
                    case 60: return "Literatura";
                    case 70: return "Territorios";
                    case 80: return "Servo";
                    default: return "Publicador";

                }
            }
        }

        public Publicador()
        {
            if (string.IsNullOrEmpty(Username)) Username = string.Empty;
            if (string.IsNullOrEmpty(Nome)) Nome = string.Empty;
            if (string.IsNullOrEmpty(Password)) Password = string.Empty;
            if (string.IsNullOrEmpty(Email)) Email = string.Empty;
            if (string.IsNullOrEmpty(Telemovel)) Telemovel = string.Empty;
            if (Grupo == null) Grupo = new Grupo() { Id = 0, Nome = "N/D"};
            Territorios = new List<Territorio>();
            Movimentos = new List<Movimentos>();
            Pedidos = new List<Literatura>();
            
        }

    }
}
