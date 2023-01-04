using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.CommonLibCore.Files
{
    /// <summary>
    /// 一个目录实体
    /// </summary>
    public class DirInfo
    {
        /// <summary>
        /// 目录名称
        /// </summary>
        public string DirName { get; set; }

        /// <summary>
        /// 目录的真实路径
        /// </summary>
        public string DirPath { get; set; }

        public DirInfo(string dirName, string dirPath)
        {
            this.DirName = dirName;
            this.DirPath = dirPath;
        }

    }
}
