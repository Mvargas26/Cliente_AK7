namespace Cliente_AK7.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Servidor
    {
        [JsonProperty("codServidor")]
        public string CodServidor { get; set; }

        [JsonProperty("nombServidor")]
        public string NombServidor { get; set; }

        [JsonProperty("descServidor")]
        public string DescServidor { get; set; }

        [JsonProperty("userAdmiServidor")]
        public string UserAdmiServidor { get; set; }

        [JsonProperty("passServidor")]
        public string PassServidor { get; set; }
    }//fn class
}//fin space
