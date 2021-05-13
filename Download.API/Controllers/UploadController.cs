using Download.API.Filters;
using Download.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Download.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : Controller
    {
        #region view
        [HttpGet("Multi")]
        public IActionResult Multi() => View();

        [HttpGet("MultiFile2")]
        public IActionResult MultiFile2() => View();

        [HttpGet("MultiForm")]
        public IActionResult MultiForm() => View();

        [HttpGet("One")]
        public IActionResult One() => View();

        [HttpGet("OneForm")]
        public IActionResult OneForm() => View();
        [HttpGet("Model")]
        public IActionResult Model() => View();
        [HttpGet("ModelForm")]
        public IActionResult ModelForm() => View();
        [HttpGet("chunk")]
        public IActionResult Chunk() => View();
        #endregion

        /// <summary>
        /// 单文件上传
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("one")]
        public IActionResult UploadOneFile(IFormFile formFile)
        {
            //formFile 即前端传过来的单个文件
            var path = AppDomain.CurrentDomain.BaseDirectory + "\\Files\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            using (var fs = new FileStream(path,FileMode.OpenOrCreate))
            {
                formFile.CopyTo(fs);
            }

            return Ok("上传成功");
        }
        /// <summary>
        /// 表单内单文件上传
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("oneForm")]
        public IActionResult UploadOneForm(IFormFile formFile)
        {
            //formFile 即前端传过来的单个文件
            var path = AppDomain.CurrentDomain.BaseDirectory + "\\Files\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                formFile.CopyTo(fs);
            }

            return Ok("上传成功");
        }
        /// <summary>
        /// 多文件上传
        /// </summary>
        /// <returns></returns>
        [HttpPost("multi")]
        public IActionResult UploadMultiFile(IFormFileCollection formFiles)
        {
            //formFiles无法获取到数据  需通过Request.Form.Files来获取
            var files = Request.Form.Files;
            // todo 保存到本地...
            return Ok("上传成功");
        }



        /// <summary>
        /// 表单内多文件上传
        /// </summary>
        /// <param name="formFiles"></param>
        /// <returns></returns>
        [HttpPost("multiForm")]
        public IActionResult UploadMultiFileForm(IFormFileCollection formFiles)
        {
            //formFiles 即前端input标签的name属性，必须一致否则无法获取到文件
            //直接对整个表单初始化formData，可以直接拿到IFormFileCollection
            var files = Request.Form.Files;
            // todo 保存到本地...
            return Ok("上传成功");
        }








        /// <summary>
        /// Model内携带文件
        /// </summary>
        /// <param name="formFiles"></param>
        /// <returns></returns>
        [HttpPost("model")]
        public IActionResult UploadModel()
        {
            // todo 保存到本地...
            //可获取到文件
            var formFile = Request.Form.Files;              //获取前端所有文件对象(单文件、多文件) 和前端的formData的key值无关
            //通过Name来区分多文件和单文件
            var names = formFile.Select(x => x.Name).ToList();
            var nameStr = string.Join(",", names);
            //非文件  对象的字符串   
            var formFiles = Request.Form["formFiles"];      //[object FileList]

            //可获取到json字符串
            var jsonModelStr = Request.Form["apiModel"];    //{"username":"多对对"}
            var jsonModel = JsonConvert.DeserializeObject<ApiModel>(jsonModelStr);

            //无法直接转换为json对象  
            var jsonModelStr2 = Request.Form["apiModel2"];  //[object Object]
            //此处报错
            var jsonModel2 = JsonConvert.DeserializeObject(jsonModelStr2);

            return Ok("上传成功");
        }













        /// <summary>
        /// Model内携带文件 通过表单上传
        /// </summary>
        /// <param name="formFiles"></param>
        /// <returns></returns>
        [HttpPost("modelForm")]
        public IActionResult UploadModelForm()
        {
            //获取全部文件
            var files = Request.Form.Files;
            //单文件
            var formFile = files.Where(x => x.Name == "formFile").ToList();
            //多文件
            var formFiles = files.Where(x => x.Name == "formFiles").ToList();
            var username = Request.Form["username"];
            // todo 保存到本地...
            return Ok("上传成功");
        }










        /// <summary>
        /// 大文件分片上传
        /// </summary>
        /// <returns></returns>
        [HttpPost("chunk")]
        [DisableFormValueModelBinding]
        //Unexpected end of Stream, the content may have already been read by another component. 报错 DisableFormValueModelBindingAttribute
        public async Task<IActionResult> UploadChunkAsync([FromQuery] FileChunk chunk)
        {
            try
            {
                var boundary = GetBoundary(Request.ContentType);
                if (string.IsNullOrEmpty(boundary)) throw new Exception("错误请求");
                var reader = new MultipartReader(boundary, Request.Body);
                //读取下一片分片数据
                var section = await reader.ReadNextSectionAsync();
                while (section != null)
                {
                    var buffer = new byte[chunk.Size];
                    var fileName = GetFileName(section.ContentDisposition);
                    fileName = fileName.Trim('"');
                    chunk.FileName = fileName;
                    var path = AppDomain.CurrentDomain.BaseDirectory + "Files\\" + fileName;
                    using (var stream = new FileStream(path, FileMode.Append))
                    {
                        int bytesRead;
                        do
                        {
                            bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                    section = await reader.ReadNextSectionAsync();
                }
                if (chunk.PartNumber == chunk.Chunks)
                {
                    await MergeChunkFile(chunk);
                }

                return Ok("上传成功");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 将分片文件合并成一个文件
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        private async Task MergeChunkFile(FileChunk chunk)
        {
            //上传目录
            var path = AppDomain.CurrentDomain.BaseDirectory + "Files\\" + chunk.FileName;
            //分片文件命名约定
            var partToken = FileSort.PART_NUMBER;
            //上传文件的实际名称
            var baseFileName = chunk.FileName.Substring(0, chunk.FileName.IndexOf(partToken));
            //根据命名约定查询指定目录下符合条件的所有分片文件
            var searchPattern = $"{Path.GetFileName(baseFileName)}{partToken}*";
            //获取分片文件
            var fileList = Directory.GetFiles(Path.GetDirectoryName(path),searchPattern);
            if (!fileList.Any()) return ;

            var mergeFiles = new List<FileSort>();
            foreach (var file in fileList)
            {
                var sort = new FileSort
                {
                    FileName = file
                };
                baseFileName = file.Substring(0,file.IndexOf(partToken));
                var fileIndex = file.Substring(file.IndexOf(partToken)+partToken.Length);
                int.TryParse(fileIndex,out var number);
                sort.PartNumber = number;
                mergeFiles.Add(sort);
            }
            //排序所有分片
            mergeFiles = mergeFiles.OrderBy(x => x.PartNumber).ToList();
            //合并文件
            using (var fileStream = new FileStream(baseFileName,FileMode.Create))
            {
                foreach (var fileSort in mergeFiles)
                {
                    using (FileStream fileChunk = new FileStream(fileSort.FileName,FileMode.Open))
                    {
                        await fileChunk.CopyToAsync(fileStream);
                    }
                }
            }
            //删除分片文件
            DeleteFile(mergeFiles);
        }
        /// <summary>
        /// 合并后删除分片文件
        /// </summary>
        /// <param name="mergeFiles"></param>
        private void DeleteFile(List<FileSort> mergeFiles)
        {
            foreach (var file in mergeFiles)
            {
                System.IO.File.Delete(file.FileName);
            }
        }
        /// <summary>
        /// 根据请求信息获取文件名称
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetFileName(string content)
        {
            return content.Split(';')
                .SingleOrDefault(p => p.Contains("filename"))
                .Split('=')
                .Last()
                .Trim();
        }
        private string GetBoundary(string contentType)
        {
            var elements = contentType.Split(' ');
            var element = elements.Where(e => e.StartsWith("boundary=")).First();
            var boundary = element.Substring("boundary=".Length);
            if (boundary.Length >= 2 && boundary[0] == '"' && boundary[boundary[boundary.Length -1]] == '"')
            {
                boundary = boundary.Substring(1, boundary.Length - 2);
            }
            return boundary;
        }
        
    }

    public class ApiUploadModel
    {
        public string Username { get; set; }
        public IFormFile FormFile { get; set; }
        public IFormFileCollection FormFiles { get; set; }
    }
    public class ApiModel
    {
        public string Username { get; set; }
    }

}
