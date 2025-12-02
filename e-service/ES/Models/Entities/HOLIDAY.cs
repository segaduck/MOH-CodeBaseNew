using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblHOLIDAY
    {
        /// <summary>
        /// 
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AllDayEvent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Location { get; set; }
    }
}