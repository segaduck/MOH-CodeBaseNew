using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ES.Action.Form
{
    public class FormBaseWordAction : BaseAction
    {
        protected static byte[] MarkDocx(string tempFile, string templateFile, Dictionary<string, object> data)
        {
            File.Copy(templateFile, tempFile);

            using (WordprocessingDocument wd = WordprocessingDocument.Open(tempFile, true))
            {
                Parse(wd.MainDocumentPart, data);
            }

            byte[] buf = File.ReadAllBytes(tempFile);
            if (File.Exists(tempFile)) { File.Delete(tempFile); }
            return buf;
        }

        protected static void Parse(OpenXmlPart oxp, Dictionary<string, object> data)
        {
            string xmlString = null;
            using (StreamReader sr = new StreamReader(oxp.GetStream()))
            { xmlString = sr.ReadToEnd(); }

            foreach (string key in data.Keys)
            { xmlString = xmlString.Replace("[$" + key + "$]", GetString(data[key])); }  //這裡進行取代

            using (StreamWriter sw = new StreamWriter(oxp.GetStream(FileMode.Create)))
            { sw.Write(xmlString); }
        }

        private static string GetString(object o)
        {
            return o == null ? "" : o.ToString();
        }
    }
}