using System;
using System.Collections.Generic;
using ES.Models.Entities;
using System.Web;

namespace ES.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class OdismohwModel
    {
        public DraftByCreatePOSTModel DBCPost { get; set; }
        public DraftByCreateResultModel DBCResult { get; set; }
    }
    public class DraftByCreatePOSTModel
    {
        public string SysID { get; set; }
        public string VerifyCode { get; set; }
        public string UnitCode { get; set; }
        public string EmployeeCode { get; set; }
        public byte[] FileContent { get; set; }
    }
    public class DraftByCreateResultModel
    {
        public bool Success { get; set; }
        // GUID
        public string Token { get; set; }
        // 創文之公文文號 文字(10)
        public string ObjectId { get; set; }
        public string ErrorMsg { get; set; }
        // A001 服務授權失敗
        // A002 非授權IP
        // B001 資料錯誤
        // B002 ZIP檔案超出上限
        // B003 檔案損毀
        // B004 DI檔個數非1個
        // B005 DI格式異常
        // FFFF 其他錯誤

    }

    public class DOC_DISaveModel : OFFICIAL_DOC_DI
    {

    }

    public class PostIncomingPOSTModel
    {
        public string SysID { get; set; }
        public string VerifyCode { get; set; }
        public byte[] FileContent { get; set; }
        public string Memo { get; set; }
    }

    public class PostIncomingResultModel
    {
        public bool Success { get; set; }
        // GUID
        public string Token { get; set; }
        public string ErrorMsg { get; set; }
        // A001 服務授權失敗
        // A002 非授權IP
        // B001 資料錯誤
        // B002 ZIP檔案超出上限
        // B003 檔案損毀
        // B004 DI檔個數非1個
        // B005 DI格式異常
        // FFFF 其他錯誤
    }

    public class DataQueryPOSTModel
    {
        public string SysID { get; set; }
        public string VerifyCode { get; set; }
        public String[] TokenArray { get; set; }
    }

    public class DataQueryResultModel
    {
        public bool Success { get; set; }
        public DataQueryBasicModel[] Data { get; set; }
        public string ErrorMsg { get; set; }
        // A001 服務授權失敗
        // A002 非授權IP
        // B001 資料錯誤
        // B002 ZIP檔案超出上限
        // B003 檔案損毀
        // B004 DI檔個數非1個
        // B005 DI格式異常
        // FFFF 其他錯誤
    }

    public class DataQueryBasicModel
    {
        // GUID
        public string Token { get; set; }
        /// <summary>
        /// 公文文號
        /// </summary>
        public string ObjectId { get; set; }
        public string Subject { get; set; }
        public string DocumentCharacter { get; set; }
        public string DetailDocumentCharacterId { get; set; }
        public string Priority { get; set; }
        public string Confidentiality { get; set; }
        public string UnitName { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? RegisterDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string CloseType { get; set; }
        public DateTime? ArchivedDate { get; set; }
        public float? FinshDays { get; set; }
        public int? FlowId { get; set; }
        public string FlowState { get; set; }
        public string CurrentUnitName { get; set; }
        public string CurrentEmployeeName { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? FinishTime { get; set; }
    }
}