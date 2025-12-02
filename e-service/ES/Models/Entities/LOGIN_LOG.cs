using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblLOGIN_LOG
{
 /// <summary>
/// 
/// </summary>
public string LOGIN_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? LOGIN_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public string NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public int? UNIT_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string IP_ADDR { get; set; }
 /// <summary>
/// 
/// </summary>
public string STATUS { get; set; }
 /// <summary>
/// 
/// </summary>
public int? FAIL_TOTAL { get; set; }
 /// <summary>
/// 
/// </summary>
public int? FAIL_COUNT { get; set; }
}
}