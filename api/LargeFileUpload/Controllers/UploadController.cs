using LargeFileUpload.Common;
using LargeFileUpload.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace LargeFileUpload.Controllers
{
    public class UploadController : ApiController
    {
        [HttpPost]
        [Route("api/Upload/LargeFile")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public async Task<IHttpActionResult> LargeFile()
        {
            HttpContext _context = HttpContext.Current;
            string root = _context.Server.MapPath("~/App_Data");
            FileUploadHelper uploadFileService = new FileUploadHelper(root);
            try
            {
                UploadProcessingResult uploadResult = await uploadFileService.HandleRequest(Request);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stack = ex.StackTrace });
            }
            return Json(new { success = true, message = "file uploaded successfully" });
        }
    }
}
