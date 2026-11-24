namespace JW_Management.Models
{
    public class Reuniao
    {
        public string? Stamp { get; set; }
        public string? SemanaReuniao { get; set; }
        // «SEMANA 48 DE 2026» -> segunda e domingo dessa semana (o importador numera por (dia do ano - 1) / 7 + 1: a segunda-feira cai nessa janela de 7 dias).
        public static (DateTime Inicio, DateTime Fim)? Intervalo(string? semana)
        {
            var m = System.Text.RegularExpressions.Regex.Match(semana ?? "", @"(\d+)\D+(\d{4})");
            if (!m.Success) return null;
            int n = int.Parse(m.Groups[1].Value), ano = int.Parse(m.Groups[2].Value);
            var janela = new DateTime(ano, 1, 1).AddDays(7 * (n - 1));
            var segunda = Enumerable.Range(0, 7).Select(i => janela.AddDays(i)).FirstOrDefault(d => d.DayOfWeek == DayOfWeek.Monday && d.Year == ano);
            return segunda == default ? null : (segunda, segunda.AddDays(6));
        }

        private static readonly string[] Meses = { "jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez" };
        // «2 a 8 nov 2026» ou «30 nov a 6 dez 2026»
        public string? Datas
        {
            get
            {
                if (Intervalo(SemanaReuniao) is not var (i, f)) return null;
                var ano = f.Year != i.Year ? $" {i.Year}" : "";
                var ini = i.Month == f.Month ? $"{i.Day}" : $"{i.Day} {Meses[i.Month - 1]}{ano}";
                return $"{ini} a {f.Day} {Meses[f.Month - 1]} {f.Year}";
            }
        }

        public string? EventoMeio { get; set; } // evento que substitui a reunião de semana (ex.: «Memorial»)
        public string? EventoFim { get; set; }  // evento que substitui a de fim de semana (ex.: «Assembleia de circuito»)
        public bool SemReuniao => EventoMeio != null && EventoFim != null;
        // As secções 0 a 299 são da reunião de semana e 300 a 499 da de fim de semana (ver HourglassImportador).
        public bool Cancelada(Designacao d) => d.TipoDesignacao!.Id switch { < 300 => EventoMeio != null, < 500 => EventoFim != null, _ => false };
        public string? ResumoEvento => (EventoMeio, EventoFim) switch
        {
            (null, null) => null,
            ({ } m, { } f) when m == f => m,
            ({ } m, null) => $"{m}: sem reunião de semana",
            (null, { } f) => $"{f}: sem reunião de fim de semana",
            ({ } m, { } f) => $"{m} (semana) e {f} (fim de semana)"
        }; // «Assembleia de circuito»…: semana sem reunião na congregação
        public List<Designacao>? Designacoes {get; set;}
        public List<Cantico> Canticos { get; set; } = new();
        public List<Grupo> Grupos { get; set; } = new();
        
        public List<Designacao> ObterDesignacoesTipo(int tipo) {
            return (this.Designacoes ?? new()).Where(t => t.TipoDesignacao!.Id > tipo).Where(t => t.TipoDesignacao!.Id < tipo+100).OrderBy(t => t.Stamp, StringComparer.Ordinal).DistinctBy(t => t.Distinct).ToList(); // ordem do programa: o Stamp cresce pela ordem em que o Hourglass lista as partes
        }
    }
}
