using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblSERVICE_CATE
{
 /// <summary>
/// 
/// </summary>
public int? SC_ID { get; set; }
 /// <summary>
/// 
/// </summary>
public int? SC_PID { get; set; }
 /// <summary>
/// 
/// </summary>
public int? UNIT_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string NAME { get; set; }
 /// <summary>
/// 
/// </summary>
public string ICON1 { get; set; }
 /// <summary>
/// 
/// </summary>
public string ICON2 { get; set; }
 /// <summary>
/// 
/// </summary>
public string XML_TAG { get; set; }
 /// <summary>
/// 
/// </summary>
public int? LEVEL { get; set; }
 /// <summary>
/// 
/// </summary>
public int? SEQ_NO { get; set; }
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