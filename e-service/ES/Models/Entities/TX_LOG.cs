using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblTX_LOG
{
 /// <summary>
/// 
/// </summary>
public int? SRL_NO { get; set; }
 /// <summary>
/// 
/// </summary>
public DateTime? TX_TIME { get; set; }
 /// <summary>
/// 
/// </summary>
public string TX_CATE_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string TX_LOGIN_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public string TX_LOGIN_NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public int? TX_UNIT_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public int? TX_TYPE { get; set; }
 /// <summary>
/// 
/// </summary>
public string TX_DESC { get; set; }
}
}