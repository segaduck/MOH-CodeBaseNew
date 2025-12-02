using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblCOSMETIC_ING
{
 /// <summary>
/// 
/// </summary>
public int? COS_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public string COS_CONTENT { get; set; }
 /// <summary>
/// 
/// </summary>
public string COS_USED { get; set; }
 /// <summary>
/// 
/// </summary>
public string COS_TYPE { get; set; }
 /// <summary>
/// 
/// </summary>
public decimal? COS_NUM_1 { get; set; }
 /// <summary>
/// 
/// </summary>
public decimal? COS_NUM_2 { get; set; }
 /// <summary>
/// 
/// </summary>
public string COS_NOTE { get; set; }
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