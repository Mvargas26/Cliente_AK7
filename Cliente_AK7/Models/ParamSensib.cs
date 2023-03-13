namespace Cliente_AK7.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ParamSensib
    {
        [JsonProperty("codServer")]
        public string CodServer { get; set; }

        [JsonProperty("codUmbral")]
        public string CodUmbral { get; set; }

        [JsonProperty("codComp")]
        public string CodComp { get; set; }

        [JsonProperty("porcentaje")]
        public long Porcentaje { get; set; }
    }//fin class
}//fin space
