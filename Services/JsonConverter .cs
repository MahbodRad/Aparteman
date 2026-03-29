using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System;
using System.Runtime.InteropServices;

namespace Aparteman.Services
{

    public class DataRowJsonConverter : JsonConverter<DataRow>
    {
        public override void WriteJson(JsonWriter writer, DataRow row, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (DataColumn column in row.Table.Columns)
            {
                writer.WritePropertyName(column.ColumnName.ToLower());
                serializer.Serialize(writer, row[column]);
            }

            writer.WriteEndObject();
        }

        public override DataRow ReadJson(
            JsonReader reader,
            Type objectType,
            DataRow existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


    //public static class JsonHelper
    //{
    //    public static string Serialize(object obj)
    //    {
    //        var settings = new JsonSerializerSettings
    //        {
    //            StringEscapeHandling = StringEscapeHandling.Default
    //        };

    //        return JsonConvert.SerializeObject(obj, settings);
    //    }
    //}

    //public class DataRowJsonConverter : JsonConverter
    //{
    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        DataRow row = value as DataRow;
    //        JObject jObject = new JObject();

    //        foreach (DataColumn column in row.Table.Columns)
    //        {
    //            // از serializer موجود استفاده کن تا تنظیمات global حفظ بشه
    //            JToken token = JToken.FromObject(row[column], JsonSerializer.Create(new JsonSerializerSettings
    //            {
    //                StringEscapeHandling = StringEscapeHandling.Default
    //            }));

    //            jObject[column.ColumnName.ToLower()] = token;
    //        }

    //        jObject.WriteTo(writer);
    //    }


    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException("Reading JSON is not implemented.");
    //    }

    //    public override bool CanConvert(Type objectType)
    //    {

    //        //String changedString = SysUtil.StringEncodingConvert(input, fromCharset, toCharset);

    //        return typeof(DataRow).IsAssignableFrom(objectType);
    //    }
    //}

}