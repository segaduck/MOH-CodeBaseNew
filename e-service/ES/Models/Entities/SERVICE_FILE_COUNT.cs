using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblSERVICE_FILE_COUNT
{
 /// <summary>
/// 
/// </summary>
public DateTime? COUNT_DATE { get; set; }
 /// <summary>
/// 
/// </summary>
public string SRV_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public int? FILE_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public int? COUNTER { get; set; }
}
}