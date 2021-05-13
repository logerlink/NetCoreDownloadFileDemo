﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Download.API.Models
{
    public class FileChunk
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 当前分片
        /// </summary>
        public int PartNumber { get; set; }
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 分片总数
        /// </summary>
        public int Chunks { get; set; }
        /// <summary>
        /// 文件读取起始位置
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// 文件读取结束位置
        /// </summary>
        public int End { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public int Total { get; set; }
    }
}
