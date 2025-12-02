using ES.Utils;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ES.Extensions
{
    public class WordConverter
    {

        private static Microsoft.Office.Interop.Word.Application _wordApplication = null;      

        public enum WordFileFormat  
        {  
            WordDoc,  
            WordDocx  
        }

        public static byte[] Convert(byte[] DocFileData, WordFileFormat Format, string SRV_ID)  
        {

            string TempDirPath = DataUtils.GetConfig("FOLDER_SERVICE_FILE");

            string docTempFileName = Guid.NewGuid().ToString();  
            string pdfTempFileName = Guid.NewGuid().ToString() + ".pdf"; 

    

            if (Format == WordFileFormat.WordDoc)  
                docTempFileName += ".doc";  
            else if (Format == WordFileFormat.WordDocx) 
            docTempFileName += ".docx"; 
            else 
                throw new NotSupportedException("ERROR_DOC_FORMAT_NOT_SUPPORTED");  

    

            object optionalNullParam = Type.Missing;  
            MemoryStream stream = new MemoryStream();  
            Application wordapp =  GetInstance();  
            object tempDocFilePath = TempDirPath + @"\" + docTempFileName; 
            object tempPdfFilePath = TempDirPath + @"\" + pdfTempFileName; 
            object wordDocSaveAs = WdSaveFormat.wdFormatPDF;  
            object wordCloseOption = WdSaveOptions.wdDoNotSaveChanges;      

            FileStream tempStream = new FileStream(tempDocFilePath.ToString(), FileMode.Create, FileAccess.Write);
            tempStream.Write(DocFileData, 0, DocFileData.Length);  
            tempStream.Flush();  
            tempStream.Close(); 

            Document docInstance = wordapp.Documents.Open( 
                ref tempDocFilePath, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam,   
                ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam,   
                ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam);  

            docInstance.Activate();  
            docInstance.SaveAs(  
                ref tempPdfFilePath, ref wordDocSaveAs, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam,   
                ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam,
                ref optionalNullParam, ref optionalNullParam, ref optionalNullParam, ref optionalNullParam);  

    

            ((_Document)docInstance).Close(ref wordCloseOption, ref optionalNullParam, ref optionalNullParam); 
            docInstance = null;  

            FileStream pdfStream = new FileStream(tempPdfFilePath.ToString(), FileMode.Open, FileAccess.Read);
            
            byte[] pdfData = new byte[pdfStream.Length];
            pdfStream.Read(pdfData, 0, pdfData.Length);
            pdfStream.Close();

    

            // delete temp file.  
            File.Delete(TempDirPath + docTempFileName);
            File.Delete(TempDirPath + pdfTempFileName);
            //return pdfData;
            return pdfData;

        } 
 

       
        public static Microsoft.Office.Interop.Word.Application GetInstance()  
        {  
            if (_wordApplication == null)
            {
                _wordApplication = new Microsoft.Office.Interop.Word.ApplicationClass();  
                _wordApplication.Visible = false;  
                _wordApplication.ScreenUpdating = false;      

                return _wordApplication; 
            }
            else
            {
                return _wordApplication;  
            }  

        }  

    

         public static void Free()  
         {  
             if (_wordApplication != null)  
             {  
                 object optionalNullParam = Type.Missing;  
                 object wordSaveOption = WdSaveOptions.wdDoNotSaveChanges;  
                 ((_Application)_wordApplication).Quit(ref wordSaveOption, ref optionalNullParam, ref optionalNullParam);  
                 _wordApplication = null;    

                 GC.Collect();  
             }  
         }  



          


    }
}