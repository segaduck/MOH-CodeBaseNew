using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace ES.Extensions
{
    public class ImageResult : ActionResult
    {
        private MemoryStream _imgStream;
        private string _contentType;
        private string _filename;

        public ImageResult(MemoryStream imgStream, string contentType)
        {
            _imgStream = imgStream;
            _contentType = contentType;
        }

        public ImageResult(MemoryStream imgStream, string contentType, string filename)
        {
            _imgStream = imgStream;
            _contentType = contentType;
            _filename = filename;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentException("context");
            if (_imgStream == null)
                throw new ArgumentException("imgStream is null");
            if (_contentType == null)
                throw new ArgumentException("contentType is null");

            HttpResponseBase response = context.HttpContext.Response;

            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.ContentType = _contentType;
            if (_filename != null)
            {
                response.AddHeader("Content-Disposition", "attachment;filename=" + _filename);
            }

            _imgStream.WriteTo(response.OutputStream);
            
        }
    }
}