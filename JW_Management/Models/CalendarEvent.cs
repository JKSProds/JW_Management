using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JW_Management.Models
{
    public class CalendarEvent
    {
        public int id { get; set; }
        public string title { get; set; }
        public string tema { get; set; }
        public string obs { get; set; }
        public string congregacao { get; set; }
        public string contacto { get; set; }
        public string email { get; set; }
        public bool Dentro_Fora { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public bool allDay { get; set; }
        public string color { get; set; }
    }
}
