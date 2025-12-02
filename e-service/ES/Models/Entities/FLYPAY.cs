using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblFLYPAY
{
 /// <summary>
/// 
/// </summary>
public string FIRST_NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public string MIDDLE_NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public string LAST_NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? FLYDATE { get; set; }
 /// <summary>
/// 
/// </summary>
public string FLYNUM { get; set; }
 /// <summary>
/// 
/// </summary>
public string PASPORTKIND { get; set; }
 /// <summary>
/// 
/// </summary>
public string PASPORTNUM { get; set; }
 /// <summary>
/// 
/// </summary>
public string CTRY { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? BIRTH { get; set; }
 /// <summary>
/// 
/// </summary>
public string PASPORTSEX { get; set; }
 /// <summary>
/// 
/// </summary>
public string TRATYPE { get; set; }
 /// <summary>
/// 
/// </summary>
public string DEL_MK { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? DEL_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public string DEL_FUN_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string DEL_ACC { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? UPD_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public string UPD_FUN_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string UPD_ACC { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? ADD_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public string ADD_FUN_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string ADD_ACC { get; set; }
 /// <summary>
/// 
/// </summary>
public int? FLY_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? CHECKIN { get; set; }
 /// <summary>
/// 
/// </summary>
public string PAY_STATUS { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? PAY_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public string PAYAMOUNT { get; set; }
 /// <summary>
/// 
/// </summary>
public string SESSION_KEY { get; set; }
}
}