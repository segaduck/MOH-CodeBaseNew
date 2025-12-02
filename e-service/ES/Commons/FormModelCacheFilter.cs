using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace ES.Commons
{
    /// <summary>
    /// 對 FormModel(繼承自 PagingResultsViewModel) 參數自動進行 Session Chache 處理的 ActionFilter
    /// </summary>
    public class FormModelCacheFilter : ActionFilterAttribute
    {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 取回的 cached Form Model 是否有效,
        /// 若無效, 應重導至對應的 GET Index()
        /// </summary>
        public bool CacheFormModelInvalid { get; set; }

        /// <summary>
        /// 參見 System.Web.Mvc.ActionFilterAttribute.OnActionExecuting()
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            CacheFormModelInvalid = false;

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// 參見 System.Web.Mvc.ActionFilterAttribute.OnActionExecuted()
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //若 FormModel 已存在, 暫存至 Session 中
            object obj = filterContext.HttpContext.Items["__FormModelCacheFilter__"];

            base.OnActionExecuted(filterContext);
        }

    }

    /// <summary>
    /// 用在 FormModel Cache 進行 JSON 時 Serialize 時,
    /// 可以排除特定的 Property (如 UploadFile 欄位), 以避免失敗
    /// <para>text 文字上傳後若有用 stream 讀取, 就會因為 Stream.ReadTimeout 無法 get value 而失敗</para>
    /// </summary>
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public new static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType.IsSubclassOf( typeof(UploadFile) ) )
            {
                // FormModel 中所有繼承自 UploadFile 的欄位, 都標明不要 Serialize 
                property.ShouldSerialize = 
                    instance =>
                    {
                        return false;
                    };
            }

            return property;
        }
    }
}