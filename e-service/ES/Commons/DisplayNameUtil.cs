using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ES.Commons
{
    public class DisplayNameUtil
    {
        /// <summary>
        /// 取得指定 class type 欄位的 DisplayName 定義, 如果沒有定義 DisplayName 則回傳 propertyName 本身。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static string GetDisplayName(string propertyName, Type classType)
        {
            PropertyInfo pInfo = GetProperty(classType, propertyName);
            return GetDisplayName(pInfo, classType);
        }

        /// <summary>
        /// Gets the property Display Name.
        /// </summary>
        /// <param name="pInfo">The p info.</param>
        /// <returns></returns>
        private static string GetDisplayName(PropertyInfo pInfo, Type metaDataType)
        {
            if (null == pInfo)
            {
                return String.Empty;
            }

            string propertyName = pInfo.Name;

            DisplayAttribute attr = (DisplayAttribute)metaDataType.GetProperty(propertyName)
                .GetCustomAttributes(typeof(DisplayAttribute), true)
                .SingleOrDefault();

            if (attr == null)
            {
                MetadataTypeAttribute otherType =
                    (MetadataTypeAttribute)metaDataType.GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                    .FirstOrDefault();

                if (otherType != null)
                {
                    var property = otherType.MetadataClassType.GetProperty(propertyName);
                    if (property != null)
                    {
                        attr = (DisplayAttribute)property.GetCustomAttributes(typeof(DisplayNameAttribute), true).SingleOrDefault();
                    }
                }
            }
            return (attr != null) ? attr.Name : propertyName;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propName">Name of the prop.</param>
        /// <returns></returns>
        private static PropertyInfo GetProperty(Type type, string propName)
        {
            try
            {
                PropertyInfo[] infos = type.GetProperties();
                if (infos == null)
                {
                    return null;
                }
                foreach (PropertyInfo info in infos)
                {
                    if (propName.ToLower().Equals(info.Name.ToLower()))
                    {
                        return info;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
            return null;
        }


    }
}