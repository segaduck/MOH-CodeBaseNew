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
    /// 檔案預覽
    /// </summary>
    public class PreviewFileModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string apy_id { get; set; }

        // <summary>
        /// 
        /// </summary>
        public string filetitle { get; set; }
        

        /// <summary>
        /// 
        /// </summary>
        public string apy_main_key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string apy_src_key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string apy_src_extion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string apy_filename { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string apy_file_path { get; set; }
    }

}