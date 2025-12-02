using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblPAY_ACCOUNT
{
 /// <summary>
/// 
/// </summary>
public int? SRL_NO { get; set; }
 /// <summary>
/// 
/// </summary>
public string ACCOUNT { get; set; }
 /// <summary>
/// 
/// </summary>
public string PSWD { get; set; }
 /// <summary>
/// 
/// </summary>
public string OID { get; set; }
 /// <summary>
/// 
/// </summary>
public string SID { get; set; }
 /// <summary>
/// 
/// </summary>
public string PAY_DESC { get; set; }
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
}
}