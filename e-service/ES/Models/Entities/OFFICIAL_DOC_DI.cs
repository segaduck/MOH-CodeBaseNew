using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class OFFICIAL_DOC_DI
    {
        public string APP_ID { get; set; }

        /// <summary>
        /// {部門代碼}.{Account}.{文號}.DI
        /// </summary>
        public string di_filename { get; set; }

        /// <summary>
        /// BASE64字串
        /// </summary>
        public byte[] di_content_b64 { get; set; }

        public string xml_content { get; set; }

        /// <summary>
        /// 回傳狀態
        /// </summary>
        public string di_status { get; set; }

        /// <summary>
        /// 新增時間
        /// </summary>
        public DateTime? add_time { get; set; }

        /// <summary>
        /// 新增人員
        /// </summary>
        public string add_user { get; set; }
        /// <summary>
        /// 異質系統代碼 單位代碼_流水號
        /// </summary>
        public string SysID { get; set; }
        /// <summary>
        /// 驗證碼
        /// </summary>
        public string VerifyCode { get; set; }
        /// <summary>
        /// 單位代碼
        /// </summary>
        public string UnitCode { get; set; }
        /// <summary>
        /// 承辦人資訊 承辦人帳號資料
        /// </summary>
        public string EmployeeCode { get; set; }
        /// <summary>
        /// 公文DI,壓縮成ZIP檔後傳送BASE64編碼
        /// </summary>
        public string FileContent { get; set; }
        /// <summary>
        /// 成功與否
        /// </summary>
        public string Success { get; set; }
        /// <summary>
        /// 查詢TOKEN
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 創文之公文文號
        /// </summary>
        public string ObjectId { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMsg { get; set; }
    }
}