using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliente_AK7.Models
{
    internal class MonitoreoServidor
    {
        public int IdMonitoreo { get; set; }

        public string IdServer { get; set; } = null!;

        public int UsoCpu { get; set; }

        public int UsoMemoria { get; set; }

        public int UsoEspacio { get; set; }

        public int EstadoServer { get; set; }

        public DateTime FechaMonitoreo { get; set; }

        public int TimeOut { get; set; }

    }//fin class
}
