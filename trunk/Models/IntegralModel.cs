using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Turbo.DataLayer;
using EECOnline.Commons;
using EECOnline.DataLayers;
using EECOnline.Services;
using EECOnline.Models.Entities;

namespace EECOnline.Models
{
    /// <summary>
    /// 積分列表
    /// </summary>
    public class IntegralModel
    {
        /// <summary>
        /// 課程名稱
        /// </summary>
        public string classname { get; set; }

        /// <summary>
        /// 積分
        /// </summary>
        public string classgrade { get; set; }

        /// <summary>
        /// 積分有效日期
        /// </summary>
        public string gradedate { get; set; }
    }

}