using System.IO;
using System.Web.Services.Description;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Net;
using System.Text;
using System;

namespace ES.Helpers
{
    /// <summary>
    /// 動態呼叫WebService  
    /// </summary>
    public static class WebServiceHelper
    {
        /// <summary>  
        /// 動態呼叫WebService  
        /// </summary>  
        /// <param name="url">WebService地址</param>  
        /// <param name="classname">類名</param>  
        /// <param name="methodname">方法名(模組名)</param>  
        /// <param name="args">引數列表</param>  
        /// <returns>object</returns>  

        public static object InvokeWebService(string url, string classname, string methodname, object[] args)
        {

            string @namespace = "ServiceBase.WebService.DynamicWebLoad";
            if (classname == null || classname == "")
            {
                classname = WebServiceHelper.GetClassName(url);
            }

            //獲取服務描述語言(WSDL)  
            WebClient wc = new WebClient();
            ServicePointManager.SecurityProtocol =
                            SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                            SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            Stream stream = wc.OpenRead(url + "?WSDL");
            ServiceDescription sd = ServiceDescription.Read(stream);
            ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
            sdi.AddServiceDescription(sd, "", "");
            CodeNamespace cn = new CodeNamespace(@namespace);

            //生成客戶端代理類程式碼  
            CodeCompileUnit ccu = new CodeCompileUnit();
            ccu.Namespaces.Add(cn);
            sdi.Import(cn, ccu);
            CSharpCodeProvider csc = new CSharpCodeProvider();
            ICodeCompiler icc = csc.CreateCompiler();

            //設定編譯器的引數  
            CompilerParameters cplist = new CompilerParameters();
            cplist.GenerateExecutable = false;
            cplist.GenerateInMemory = true;
            cplist.ReferencedAssemblies.Add("System.dll");
            cplist.ReferencedAssemblies.Add("System.XML.dll");
            cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
            cplist.ReferencedAssemblies.Add("System.Data.dll");

            //編譯代理類  
            CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
            if (true == cr.Errors.HasErrors)
            {
                System.Text.StringBuilder sb = new StringBuilder();
                foreach (CompilerError ce in cr.Errors)
                {
                    sb.Append(ce.ToString());
                    sb.Append(System.Environment.NewLine);
                }
                throw new Exception(sb.ToString());
            }

            //生成代理例項,並呼叫方法  
            System.Reflection.Assembly assembly = cr.CompiledAssembly;
            Type t = assembly.GetType(@namespace + "." + classname, true, true);
            object obj = Activator.CreateInstance(t);
            System.Reflection.MethodInfo mi = t.GetMethod(methodname);

            return mi.Invoke(obj, args);
        }

        private static string GetClassName(string url)
        {
            string[] parts = url.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }
    }
}