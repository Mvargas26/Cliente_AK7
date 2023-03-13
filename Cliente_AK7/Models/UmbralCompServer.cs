using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliente_AK7.Models
{
    internal class UmbralCompServer
    {
        public string CodServer { get; set; } = null!;

        public string CodUmbral { get; set; } = null!;

        public string CodComp { get; set; } = null!;

        public int Porcentaje { get; set; }

    }//fin class
}//fin space
