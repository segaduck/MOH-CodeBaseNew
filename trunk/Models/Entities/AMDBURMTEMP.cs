using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 使用者申請暫存
    /// </summary>
    public class TblAMDBURMTEMP : IDBRow
    {
     /// <summary>
    /// 帳號
    /// </summary>
    public string userno { get; set; }

     /// <summary>
    /// 密碼
    /// </summary>
    public string pwd { get; set; }

     /// <summary>
    /// 使用者姓名
    /// </summary>
    public string username { get; set; }

     /// <summary>
    /// 身分證字號
    /// </summary>
    public string idno { get; set; }

     /// <summary>
    /// 出生年月日
    /// </summary>
    public string birthday { get; set; }

     /// <summary>
    /// 單位代號
    /// </summary>
    public string unitid { get; set; }

     /// <summary>
    /// 電話
    /// </summary>
    public string tel { get; set; }

     /// <summary>
    /// 手機
    /// </summary>
    public string phone { get; set; }

     /// <summary>
    /// 傳真
    /// </summary>
    public string fax { get; set; }

     /// <summary>
    /// 電子郵件
    /// </summary>
    public string emall { get; set; }

     /// <summary>
    /// 自然人憑證序號
    /// </summary>
    public string ssokey { get; set; }

     /// <summary>
    /// 編輯者帳號
    /// </summary>
    public string moduserid { get; set; }

     /// <summary>
    /// 編輯者姓名
    /// </summary>
    public string modusername { get; set; }

     /// <summary>
    /// 編輯時間
    /// </summary>
    public string modtime { get; set; }

     /// <summary>
    /// 編輯IP
    /// </summary>
    public string modip { get; set; }

     /// <summary>
    /// 刪除註記
    /// </summary>
    public string del_mk { get; set; }
    public DBRowTableName GetTableName()
    {
    return StaticCodeMap.TableName.AMDBURMTEMP;
    }
    }
}