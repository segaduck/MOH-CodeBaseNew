using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Models
{
    public class LogModel
    {
        public string CODE_DESC { get; set; }

        public string MODTIME { get; set; }

        public List<Hashtable> DIFFTABLE { get; set; }

    }

    public class TransLogModel {
        public string MODTIME { get; set; }

        public string DESC1 { get; set; }
    }

}