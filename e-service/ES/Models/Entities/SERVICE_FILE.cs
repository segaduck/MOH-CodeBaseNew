using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblSERVICE_FILE
{
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
public string TITLE { get; set; }
 /// <summary>
/// 
/// </summary>
public string FILENAME { get; set; }
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
 /// <summary>
/// 
/// </summary>
public string FILE_TYPE_CD { get; set; }
 /// <summary>
/// 
/// </summary>
public string FILE_URL { get; set; }
}
}