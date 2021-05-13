using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Download.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 正常get请求下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("download")]
        public IActionResult GetDownload()
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "Files\\ManYou.pdf";
            return File(new FileStream(filePath, FileMode.Open), "application/pdf", "ManYou" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf");
        }

        /// <summary>
        /// post请求下载
        /// </summary>
        /// <returns></returns>
        [HttpPost("download1")]
        public IActionResult PostDownload()
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "Files\\ManYou.pdf";
            return File(new FileStream(filePath, FileMode.Open), "application/pdf", "ManYou" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf");
        }
    }
}
