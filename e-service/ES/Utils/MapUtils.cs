using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Utils
{
    public class MapUtils
    {
        Dictionary<string, string> item = null;

        public MapUtils()
        {
            this.item = new Dictionary<string, string>();
        }

        public MapUtils(Dictionary<string, string> item)
        {
            this.item = item;

            if (item == null)
            {
                this.item = new Dictionary<string, string>();
            }
        }

        public void Put(string key, string value)
        {
            if (key == null || value == null)
            {
                return;
            }

            if (item.ContainsKey(key))
            {
                item[key] = value;
            }
            else
            {
                item.Add(key, value == null ? "" : value);
            }
        }

        public string Get(string key)
        {
            return (item.ContainsKey(key)) ? (item[key] == null ? "" : item[key]) : "";
        }

        public Dictionary<string, string> GetItem()
        {
            return this.item;
        }
    }
}