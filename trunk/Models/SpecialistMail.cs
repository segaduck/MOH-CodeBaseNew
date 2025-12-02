using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace EECOnline.Models
{
    /// <summary>
    /// 承辦人信箱
    /// </summary>
    public class SpecialistMail
    {
        public string name { get; set; }

        public string userno { get; set; }

        public string username { get; set; }

        public string mail { get; set; }
    }
}