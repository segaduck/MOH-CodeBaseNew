using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_001008_ATH 醫事人員請領英文證明書_附檔
    /// </summary>
    public class Apply_001008_AthViewModel : Apply_001008_AthModel
    {
        /// <summary>
        /// 附件電子檔
        /// </summary>
        //[Required]
        [Display(Name = "醫事人員或公共衛生師/專科中文證書電子檔")]
        public HttpPostedFileBase FILE_1 { get; set; }
        public string FILE_1_FILENAME { get; set; }

        public string FILE_1_TEXT { get; set; }

        public string FILE_1_FULLNAME { get; set; }

        /// <summary>
        /// 檔案類型
        /// </summary>
        public string FILE_1_MIME {
            get
            {
                string ret = null;
                if (this.FILE_1 != null)
                {
                    ret = this.FILE_1.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 是否已有舊檔
        /// </summary>
        public string HAS_OFILE { get; set; }

        /// <summary>
        /// 中英文證書電子檔(FOR 後台用)
        /// </summary>
        public string FILE_CERT { get; set; }
        public string FILE_CERT_TEXT { get { return this.FILE_1_TEXT; } }

        /// <summary>
        /// 檔案上傳時間
        /// </summary>
        public string ADD_TIME_TW
        {
            get
            {
                string ret = "";

                if (this.ADD_TIME != null)
                {
                    ret = HelperUtil.DateTimeToTwFormatLongString(this.ADD_TIME);
                }

                return ret;
            }
        }


        #region FileDLForTurbo用
        /// <summary>
        /// 變更後檔名
        /// </summary>
        public string FILE_NAME_TEXT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SEQ { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SRC { get; set; }
        #endregion
    }
}