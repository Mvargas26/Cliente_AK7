using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliente_AK7.Models
{
    internal class Servidor
    {
        public string CodServidor { get; set; } = null!;

        public string NombServidor { get; set; } = null!;

        public string DescServidor { get; set; } = null!;

        public string UserAdmiServidor { get; set; } = null!;

        public string PassServidor { get; set; } = null!;

    }//fn class
}//fn space
