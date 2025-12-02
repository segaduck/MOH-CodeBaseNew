using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblEEC_ApplyDetailPrice_ApiData : IDBRow
    {
        /// <summary>自動編號唯一值</summary>
        [IdentityDBField]
        public long? keyid { get; set; }
        public long? master_keyid { get; set; }
        public string PatientIdNo { get; set; }
        public string PatientId { get; set; }
        public string AccessionNum { get; set; }
        public string HospitalId { get; set; }
        public string StudyUid { get; set; }
        public string Guid { get; set; }
        public string TemplateId { get; set; }
        public string CreatedDateTime { get; set; }
        public string Modality { get; set; }
        public string BodyPart { get; set; }
        public string PerfrmdStartDate { get; set; }
        public string HospitalName { get; set; }
        public string PerfrmdItem { get; set; }
        public string VerificationDate { get; set; }
        public string Report_XML { get; set; }
        public string Report_HTML { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EEC_ApplyDetailPrice_ApiData;
        }
    }
}