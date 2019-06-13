using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace JKang.IpcServiceFramework.Services
{
    public class DefaultValueConverter : IValueConverter
    {
        public bool TryConvert(object origValue, Type destType, out object destValue)
        {
            if (origValue == null)
            {
                destValue = null;
                return destType.IsClass || (Nullable.GetUnderlyingType(destType) != null);
            }

            if (destType.IsInstanceOfType(origValue))
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
                if (destType.IsInterface || (destType.IsClass && destType.IsAbstract))
                {
                    KnowType kT = (KnowType) Attribute.GetCustomAttributes(destType).First(x => x.GetType() == typeof(KnowType));
                    if (kT != null)
                    {
                        destValue = jObj.ToObject(kT.Type);
                        return true;
                    }
                }

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
