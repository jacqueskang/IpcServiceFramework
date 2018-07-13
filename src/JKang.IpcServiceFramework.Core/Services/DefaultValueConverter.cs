using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace JKang.IpcServiceFramework.Services
{
    public class DefaultValueConverter : IValueConverter
    {
        public bool TryConvert(object origValue, Type destType, out object destValue)
        {
            if (destType.IsAssignableFrom(origValue.GetType()))
            {
                // copy value directly if it can be assigned to destType
                destValue = origValue;
                return true;
            }

            if (destType.IsEnum)
            {
                if (origValue is string str)
                {
                    try
                    {
                        destValue = Enum.Parse(destType, str, ignoreCase: true);
                        return true;
                    }
                    catch
                    { }
                }
                else
                {
                    try
                    {
                        destValue = Enum.ToObject(destType, origValue);
                        return true;
                    }
                    catch
                    { }
                }
            }

            if (origValue is string str2 && destType == typeof(Guid))
            {
                if (Guid.TryParse(str2, out Guid result))
                {
                    destValue = result;
                    return true;
                }
            }

            if (origValue is JObject jObj)
            {
                // rely on JSON.Net to convert complexe type
                destValue = jObj.ToObject(destType);
                // TODO: handle error
                return true;
            }

            if (origValue is JArray jArray)
            {
                destValue = jArray.ToObject(destType);
                return true;
            }

            try
            {
                destValue = Convert.ChangeType(origValue, destType);
                return true;
            }
            catch
            { }

            try
            {
                destValue = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(origValue), destType);
                return true;
            }
            catch
            { }

            destValue = null;
            return false;
        }
    }
}
