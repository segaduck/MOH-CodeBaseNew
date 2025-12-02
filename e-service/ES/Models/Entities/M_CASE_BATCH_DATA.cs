using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblM_CASE_BATCH_DATA
{
 /// <summary>
/// 
/// </summary>
public string CASE_NO { get; set; }
 /// <summary>
/// 
/// </summary>
public string CASE_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string FLOW_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string CASE_OWNER_UNIT { get; set; }
 /// <summary>
/// 
/// </summary>
public string CASE_OWNER_PER { get; set; }
 /// <summary>
/// 
/// </summary>
public string CASE_SUBJECT { get; set; }
 /// <summary>
/// 
/// </summary>
public string CASE_FRM_NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? CASE_DUE_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? CASE_IN_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? CASE_CLOSE_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public int? CASE_CLOSE_DAYS { get; set; }
 /// <summary>
/// 
/// </summary>
public int? CASE_PAIR_DAYS { get; set; }
 /// <summary>
/// 
/// </summary>
public string CASE_FLAG { get; set; }
 /// <summary>
/// 
/// </summary>
public string STATUS_MK { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? SC_DATE { get; set; }
 /// <summary>
/// 
/// </summary>
public string LICENSE_CD { get; set; }
}
}