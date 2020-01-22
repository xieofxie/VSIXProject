using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Models
{
    internal class ConfigBase<T>
    {
        public ConfigBase(JObject source)
        {
            Config = source.ToObject<T>();
        }

        public string FilePath { get; set; }

        public T Config { get; set; }
    }
}
