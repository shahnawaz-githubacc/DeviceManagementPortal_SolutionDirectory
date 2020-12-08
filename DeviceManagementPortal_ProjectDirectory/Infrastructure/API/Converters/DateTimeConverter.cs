using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceManagementPortal.Utility;

namespace DeviceManagementPortal.Infrastructure.API.Converters
{
    public class DateTimeConverter : Newtonsoft.Json.JsonConverter
    {
        private readonly IConfiguration Configuration = null;
        private readonly string dateTimeFormat = string.Empty;
        public DateTimeConverter(IConfiguration configuration)
        {
            Configuration = configuration;
            dateTimeFormat = Configuration.ReadConfigurationKeyValue("ApplicationLevelConfigurations:DateTimeFormat", "dd/MM/yyyy HH:mm:ss");
        }
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(DateTime)) { return true; }
            else return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(dateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
