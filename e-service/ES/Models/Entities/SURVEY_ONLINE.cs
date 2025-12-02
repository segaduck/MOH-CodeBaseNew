using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblSURVEY_ONLINE
{
 /// <summary>
/// 
/// </summary>
public string SRL_NO { get; set; }
 /// <summary>
/// 
/// </summary>
public int? UNIT_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string UNIT_NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public string SRV_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public string SRV_NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public string APPLY_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public int? Q1 { get; set; }
 /// <summary>
/// 
/// </summary>
public int? Q2 { get; set; }
 /// <summary>
/// 
/// </summary>
public int? Q3 { get; set; }
 /// <summary>
/// 
/// </summary>
public int? Q4 { get; set; }
 /// <summary>
/// 
/// </summary>
public int? Q5 { get; set; }
 /// <summary>
/// 
/// </summary>
public string RECOMMEND { get; set; }
 /// <summary>
/// 
/// </summary>
public string SATISFIED { get; set; }
 /// <summary>
/// 
/// </summary>
public int? SATISFIED_SCORE { get; set; }
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