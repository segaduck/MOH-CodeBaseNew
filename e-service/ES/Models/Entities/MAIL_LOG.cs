using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblMAIL_LOG
{
 /// <summary>
/// 
/// </summary>
public int? SRL_NO { get; set; }
 /// <summary>
/// 
/// </summary>
public string SUBJECT { get; set; }
 /// <summary>
/// 
/// </summary>
public string BODY { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? SEND_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public string MAIL { get; set; }
 /// <summary>
/// 
/// </summary>
public string RESULT_MK { get; set; }
 /// <summary>
/// 
/// </summary>
public string SRV_ID { get; set; }

/// <summary>
/// 
/// </summary>
public string Ref_Project { get; set; }
    }
}