using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
 /// <summary>
/// 
/// </summary>
public class TblAPIRESULT
{
 /// <summary>
/// 
/// </summary>
public string method { get; set; }
 /// <summary>
/// 
/// </summary>
public string success { get; set; }
 /// <summary>
/// 
/// </summary>
public string code { get; set; }
 /// <summary>
/// 
/// </summary>
public string message { get; set; }
 /// <summary>
/// 
/// </summary>
public string datetime { get; set; }
 /// <summary>
/// 
/// </summary>
public string resultdata { get; set; }
 /// <summary>
/// 
/// </summary>
public string identifier { get; set; }
 /// <summary>
/// 
/// </summary>
public string servicejson { get; set; }
}
}