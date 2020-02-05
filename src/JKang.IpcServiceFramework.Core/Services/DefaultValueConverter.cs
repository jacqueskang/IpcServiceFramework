using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace JKang.IpcServiceFramework.Services
{
    public class DefaultValueConverter : IValueConverter
    {
        public bool TryConvert(object origValue, Type destType, out object destValue)
        {
            var destConcreteType = Nullable.GetUnderlyingType(destType);

            if (origValue == null)
            {
                destValue = null;
                return destType.IsClass || (destConcreteType != null);
            }

            if (destConcreteType != null)
                destType = destConcreteType;

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

            if (origValue is string origStringValue)
            {
                if ((destType == typeof(Guid)) && Guid.TryParse(origStringValue, out var guidResult))
                {
                    destValue = guidResult;
                    return true;
                }

                if ((destType == typeof(TimeSpan)) && TimeSpan.TryParse(origStringValue, CultureInfo.InvariantCulture, out var timeSpanResult))
                {
                    destValue = timeSpanResult;
                    return true;
                }

                if ((destType == typeof(DateTime)) && DateTime.TryParse(origStringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeResult))
                {
                    destValue = dateTimeResult;
                    return true;
                }
            }

            if ((origValue is TimeSpan timeSpan) && (destType == typeof(string)))
            {
                destValue = timeSpan.ToString("c");
                return true;
            }

            if ((origValue is DateTime dateTime) && (destType == typeof(string)))
            {
                destValue = dateTime.ToString("o");
                return true;
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
