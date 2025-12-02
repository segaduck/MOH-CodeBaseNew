using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// ¦@¥Î¥N½X
    /// </summary>
    public class TblCODE1 : IDBRow
    {
        public string ITEM { get; set; }
        public string CODE { get; set; }
        public string TEXT { get; set; }
        public string KEY1 { get; set; }
        public string KEY2 { get; set; }
        public string KEY3 { get; set; }
        public string MODUSER { get; set; }
        public string MODUSERNAME { get; set; }
        public string MODTIME { get; set; }
        public string MODIP { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.CODE1;
        }
    }
}