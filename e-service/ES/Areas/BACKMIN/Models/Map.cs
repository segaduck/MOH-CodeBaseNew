using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES.Areas.Admin.Models
{
    [Serializable]
    public class Map
    {
        Dictionary<object,object> a;

        public Map()
        {
            a = new Dictionary<object, object>();
        }

        public void Add(object key, object value)
        {
            a.Add(key, value);
        }

        private object GetValue(object key)
        {
            foreach (KeyValuePair<object, object> pair in a)
            {
                if (pair.Key.Equals(key))
                {
                    return pair.Value;
                }
            }
            return null;
        }
 
        public object Get(object key)
        {
            return GetValue(key) == null ? null : GetValue(key);
        }

        public String GetString(object key)
        {
            return GetValue(key) == null ? null : GetValue(key).ToString();
        }

        public int GetInt(object key)
        {
            int i = 0;
            try
            {
                i = int.Parse(GetValue(key).ToString());
            }catch(Exception){
            }
            return i;
        }

        public bool GetBool(object key)
        {
            return bool.Parse(GetValue(key).ToString());
        }

        public DateTime? GetDateTime(object key)
        {
            DateTime? dt = GetValue(key) as DateTime?;
            return dt;
        }

        public void Clear()
        {
            a.Clear();
        }
    }
}
