using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblM_CASE_FLOW
{
 /// <summary>
/// 
/// </summary>
public string CASE_NO { get; set; }
 /// <summary>
/// 
/// </summary>
public string DOC_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public string FLOW_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? CASE_ENT_DATE { get; set; }
 /// <summary>
/// 
/// </summary>
public string CASE_ENT_PER { get; set; }
 /// <summary>
/// 
/// </summary>
public string CASE_ENT_UNIT { get; set; }
 /// <summary>
/// 
/// </summary>
public string SRC_CASE_ENT_DATE { get; set; }
}
}