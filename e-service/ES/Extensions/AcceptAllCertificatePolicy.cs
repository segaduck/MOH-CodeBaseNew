using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace ES.Extensions
{
    public class AcceptAllCertificatePolicy : System.Net.ICertificatePolicy
    {
        public bool CheckValidationResult(System.Net.ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            return true;
        }   
    }
}