using DeviceManagementPortal.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.API.Converters
{
    public class BadCharacterConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(string)) { return true; }
            else return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string stringValue = (value as string);
            if (!string.IsNullOrEmpty(stringValue))
            {
                writer.WriteValue(AppUtility.SanitizeOutgoingData(stringValue));
            }
        }
    }
}
