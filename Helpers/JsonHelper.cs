using Newtonsoft.Json.Linq;

namespace CatalogoCPProxy.Helpers
{
    public static class JsonHelper
    {
        public static JToken ConvertJsonKeysToLowercase(JToken token)
        {
            if (token is JObject jObject)
            {
                var newObject = new JObject();
                foreach (var property in jObject.Properties())
                {
                    newObject.Add(property.Name.ToLower(), ConvertJsonKeysToLowercase(property.Value));
                }
                return newObject;
            }
            else if (token is JArray jArray)
            {
                var newArray = new JArray();
                foreach (var item in jArray)
                {
                    newArray.Add(ConvertJsonKeysToLowercase(item));
                }
                return newArray;
            }
            else
            {
                return token;
            }
        }
    }
}
