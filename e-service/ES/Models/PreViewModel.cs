using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace ES.Models
{
    public class PreViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string FILENAME { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string APP_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FILE_NO { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SRC_NO { get; set; }

        /// <summary>
        /// 完整路徑
        /// </summary>
        public string FILEPATHFULL { get; set; }
    }
}