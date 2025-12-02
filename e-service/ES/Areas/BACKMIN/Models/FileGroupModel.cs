using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class FileGroupModel
    {
        public string FILE_NAME { get; set; }
        public string FILE_NAME_TEXT { get; set; }
        public string SEQ { get; set; }
        public string NOTICE_NO { get; set; }
        public string SRC { get; set; }
        public string SRC_FILENAME { get; set; }
        public string BATCH_INDEX { get; set; }
    }
}