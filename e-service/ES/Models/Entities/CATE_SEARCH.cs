using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblCATE_SEARCH
{
 /// <summary>
/// 
/// </summary>
public int? SRL_NO { get; set; }
 /// <summary>
/// 
/// </summary>
public string TITLE { get; set; }
 /// <summary>
/// 
/// </summary>
public string CLS_SUB_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string CLS_ADM_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string CLS_SRV_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string KEYWORD { get; set; }
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