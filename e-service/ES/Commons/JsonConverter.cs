using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ES.Commons
{
    public class StringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;

            string text = reader.Value.ToString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return text;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Not needed because this converter cannot write json");
        }

        public override bool CanWrite
        {
            get { return false; }
        }
    }

    public class IntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;

            string text = reader.Value.ToString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return System.Convert.ToInt32(text);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Not needed because this converter cannot write json");
        }

        public override bool CanWrite
        {
            get { return false; }
        }
    }

    public class NullableDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;

            string text = reader.Value.ToString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return HelperUtil.TransToDateTime(text);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Not needed because this converter cannot write json");
        }

        public override bool CanWrite
        {
            get { return false; }
        }
    }

    //public class Base64FileConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return objectType == typeof(HttpPostedFileBase);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.Value == null) return null;

    //        string text = reader.Value.ToString();

    //        if (string.IsNullOrWhiteSpace(text))
    //        {
    //            return null;
    //        }

    //        string[] arr = text.Split(new char[] { ':', ';', ',' });

    //        using (MemoryStream ms = new MemoryStream())
    //        {
    //        }

    //        byte[] ba = null; // TODO

    //        // HttpPostedFileBase file = new HttpPostedFileBaseExt();
    //        // file.InputStream.
    //        string data = arr[2];

    //        return file;
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException("Not needed because this converter cannot write json");
    //    }

    //    public override bool CanWrite
    //    {
    //        get { return false; }
    //    }
    //}

    //public class HttpPostedFileBaseExt : HttpPostedFileBase
    //{
    //    Stream stream;
    //    string contentType;
    //    string fileName;

    //    public HttpPostedFileBaseExt(Stream stream, string contentType, string fileName)
    //    {
    //        this.stream = stream;
    //        this.contentType = contentType;
    //        this.fileName = fileName;
    //    }

    //    public override int ContentLength
    //    {
    //        get { return (int)stream.Length; }
    //    }

    //    public override string ContentType
    //    {
    //        get { return contentType; }
    //    }

    //    public override string FileName
    //    {
    //        get { return fileName; }
    //    }

    //    public override Stream InputStream
    //    {
    //        get { return stream; }
    //    }

    //    public override void SaveAs(string filename)
    //    {
    //        base.SaveAs(filename);
    //    }

    //}


}