using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblSERVICE_RULE
{
 /// <summary>
/// 
/// </summary>
public string SRV_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public string RULE_A_MK { get; set; }
 /// <summary>
/// 
/// </summary>
public int? RULE_A_DAY { get; set; }
 /// <summary>
/// 
/// </summary>
public string RULE_B_MK { get; set; }
 /// <summary>
/// 
/// </summary>
public int? RULE_B_DAY { get; set; }
 /// <summary>
/// 
/// </summary>
public string RULE_C_MK { get; set; }
 /// <summary>
/// 
/// </summary>
public int? NOTIFY_COUNT { get; set; }
 /// <summary>
/// 
/// </summary>
public string RESET_MK { get; set; }
 /// <summary>
/// 
/// </summary>
public string HEAD_MK { get; set; }
 /// <summary>
/// 
/// </summary>
public string HEAD_ACC { get; set; }
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