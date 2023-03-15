using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliente_AK7.Models
{
    internal class MonitoreoServicio
    {
        public int IdMonitoreo { get; set; }

        public string IdServicio { get; set; } = null!;

        public int EstadoServicio { get; set; }

        public DateTime FechaMoniServicio { get; set; }

        public int TimeOutServicio { get; set; }
        public string estadoParam { get; set; } = null!;

    }//fin class
}
