using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Commons
{
    /// <summary>
    /// 上傳檔案類型不支援時丟出這個 Exception
    /// </summary>
    public class UnAcceptableFileTypeException : System.Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public UnAcceptableFileTypeException() : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public UnAcceptableFileTypeException(string msg): base(msg)
        {
        }
    }
}
