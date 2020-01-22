using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject1.Utils
{
    public static class Json
    {
        public static bool Contains(this JObject jObject, string propertyName)
        {
            return jObject.TryGetValue(propertyName, out JToken value);
        }
    }
}
