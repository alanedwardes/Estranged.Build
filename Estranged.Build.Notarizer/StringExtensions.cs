using Claunia.PropertyList;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Estranged.Build.Notarizer
{
    internal static class StringExtensions
    {
        public static T DeserializePlist<T>(this string input)
        {
            var plist = PropertyListParser.Parse(Encoding.UTF8.GetBytes(input));

            var obj = plist.ToObject();

            var json = JObject.FromObject(obj);

            return json.ToObject<T>();
        }
    }
}
