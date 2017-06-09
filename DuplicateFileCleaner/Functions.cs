using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplicateFileCleaner
{
    public class Functions
    {
        private static string md5Cache = "md5Dictionary.txt";
        public static ListViewItem GenListViewItem(int index, string[] items)
        {
            ListViewItem lsvItem = new ListViewItem { Name = "index", Text = (index + 1).ToString() };
            lsvItem.SubItems.AddRange(new ListViewItem.ListViewSubItem[] { 
                new ListViewItem.ListViewSubItem(lsvItem,items[0]),//{Name="Message",Text=item},
                new ListViewItem.ListViewSubItem(lsvItem,items[1]),//{Name="TimeSpend"},
                new ListViewItem.ListViewSubItem(lsvItem,items[2]),//{Name="TimeSpend"},
                new ListViewItem.ListViewSubItem(lsvItem,items[3]),//{Name="TimeSpend"},
                });
            return lsvItem;
        }

        public static Dictionary<string, List<FileInfo>> GenMD5DicFullCpu(string path)
        {
            var hitoryDic = new Dictionary<string, string>();


            Dictionary<string, List<FileInfo>> dic = new Dictionary<string, List<FileInfo>>();
            ConcurrentQueue<KeyValuePair<string, FileInfo>> queue = new ConcurrentQueue<KeyValuePair<string, FileInfo>>();

            var files = Functions.GetFiles(path, ref hitoryDic);
            Parallel.ForEach(files, (fileInf) =>
            {
                int bufferSize = 1024 * 1024 * 4;//自定义缓冲区大小16K 
                byte[] buffer = new byte[bufferSize];
                string md5 = string.Empty;
                if (hitoryDic.ContainsKey(fileInf.FullName))
                {
                    md5 = hitoryDic[fileInf.FullName];
                }
                else
                {
                    md5 = Functions.getMD5ByHashAlgorithm(fileInf.FullName, buffer);
                }
                queue.Enqueue(new KeyValuePair<string, FileInfo>(md5, fileInf));
            });

            foreach (var it in queue)
            {
                if (!dic.ContainsKey(it.Key))
                {
                    dic.Add(it.Key, new List<FileInfo>());
                }
                dic[it.Key].Add(it.Value);
            }
            return dic;
        }


        public static Dictionary<string, List<FileInfo>> GenMD5Dic(string path)
        {
            var hitoryDic = new Dictionary<string, string>();

            Dictionary<string, List<FileInfo>> dic = new Dictionary<string, List<FileInfo>>();
            ConcurrentQueue<KeyValuePair<string, FileInfo>> queue = new ConcurrentQueue<KeyValuePair<string, FileInfo>>();
            var files = Functions.GetFiles(path, ref hitoryDic);
            int tplCnt = Environment.ProcessorCount > 4 ? Environment.ProcessorCount : Environment.ProcessorCount * 2;
            var parts = Partitioner.Create(0, files.Count, files.Count / tplCnt + (files.Count % tplCnt == 0 ? 0 : 1)).GetDynamicPartitions().ToList();
            ParallelHelper.ForEach(parts, Math.Min(4, parts.Count), (tuple, state) =>
            {
                int bufferSize = 1024 * 1024 * 4;//自定义缓冲区大小16K 
                byte[] buffer = new byte[bufferSize];
                for (int i = tuple.Item1; i < tuple.Item2; i++)
                {
                    FileInfo fileInf = files[i];
                    string md5 = string.Empty;

                    if (fileInf.Name.Equals(md5Cache))
                    {
                        continue;
                    }
                    if (hitoryDic.ContainsKey(fileInf.FullName))
                    {
                        md5 = hitoryDic[fileInf.FullName];
                    }
                    else
                    {
                        md5 = Functions.getMD5ByHashAlgorithm(fileInf.FullName, buffer);
                    }
                    queue.Enqueue(new KeyValuePair<string, FileInfo>(md5, fileInf));
                }
            });

            foreach (var it in queue)
            {
                if (!dic.ContainsKey(it.Key))
                {
                    dic.Add(it.Key, new List<FileInfo>());
                }
                dic[it.Key].Add(it.Value);
            }
            return dic;
        }


        public static List<FileInfo> GetFiles(string path, ref Dictionary<string, string> hitoryDic)
        {
            if (File.Exists(Path.Combine(path, md5Cache)))
            {
                using (FileStream fs = new FileStream(Path.Combine(path, md5Cache), FileMode.Open, FileAccess.Read))
                using (TextReader reader = new StreamReader(fs))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] arr = line.Split("*".ToCharArray());
                        string fullPath = Path.Combine(path, arr[0]);
                        if (arr.Length > 1 && !hitoryDic.ContainsKey(fullPath))
                        {
                            hitoryDic.Add(fullPath, arr[1]);
                        }
                    }
                }
            }

            var ret = new List<FileInfo>();
            DirectoryInfo info = new DirectoryInfo(path);

            if (info.GetDirectories().Length < 1)
            {
                ret.AddRange(info.GetFiles());
            }
            else
            {
                ret.AddRange(info.GetFiles());
                foreach (var it in info.GetDirectories())
                {
                    ret.AddRange(GetFiles(it.FullName, ref hitoryDic));
                }
            }
            return ret;
        }

        /// <summary>  
        /// 通过MD5CryptoServiceProvider类中的ComputeHash方法直接传入一个FileStream类实现计算MD5  
        /// 操作简单，代码少，调用即可  
        /// </summary>  
        /// <param name="path">文件地址</param>  
        /// <returns>MD5Hash</returns>  
        public static string getMD5ByMD5CryptoService(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("<{0}>, 不存在", path));
            string resule = string.Empty;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
                byte[] buffer = md5Provider.ComputeHash(fs);
                resule = BitConverter.ToString(buffer);
                resule = resule.Replace("-", "");
                md5Provider.Clear();
            }
            return resule;
        }

        public static string getMD5ByHashAlgorithm(string path)
        {
            int bufferSize = 1024 * 1024 * 16;//自定义缓冲区大小16K 
            return getMD5ByHashAlgorithm(path, new byte[bufferSize]);
        }

        /// <summary>  
        /// 通过HashAlgorithm的TransformBlock方法对流进行叠加运算获得MD5  
        /// 实现稍微复杂，但可使用与传输文件或接收文件时同步计算MD5值  
        /// 可自定义缓冲区大小，计算速度较快  
        /// </summary>  
        /// <param name="path">文件地址</param>  
        /// <returns>MD5Hash</returns>  
        public static string getMD5ByHashAlgorithm(string path, byte[] buffer)
        {
            if (!File.Exists(path))
                throw new ArgumentException(string.Format("<{0}>, 不存在", path));
            int bufferSize = buffer.Length;
            string md5 = string.Empty;
            using (Stream inputStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize * 2))
            {
                HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider();
                int readLength = 0;//每次读取长度  
                //var output = new byte[bufferSize];
                while ((readLength = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //计算MD5  
                    hashAlgorithm.TransformBlock(buffer, 0, readLength, buffer, 0);
                }
                //完成最后计算，必须调用(由于上一部循环已经完成所有运算，所以调用此方法时后面的两个参数都为0)  
                hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
                md5 = BitConverter.ToString(hashAlgorithm.Hash).Replace("-", "");
                hashAlgorithm.Clear();
            }
            return md5;
        }

        public static void WriteMd5History(Dictionary<string, List<FileInfo>> dic)
        {
            Dictionary<string, Dictionary<string, string>> dicPathFileMD5 = new Dictionary<string, Dictionary<string, string>>();
            foreach (var md5 in dic)
            {
                md5.Value.ForEach(o =>
                {
                    if (!dicPathFileMD5.ContainsKey(o.DirectoryName))
                    {
                        dicPathFileMD5.Add(o.DirectoryName, new Dictionary<string, string>());
                    }
                    dicPathFileMD5[o.DirectoryName].Add(o.Name, md5.Key);
                });
            }
            foreach (var it in dicPathFileMD5)
            {
                using (FileStream fs = new FileStream(Path.Combine(it.Key, md5Cache), FileMode.Create, FileAccess.Write))
                using (TextWriter writer = new StreamWriter(fs))
                {
                    foreach (var filemd5 in it.Value)
                    {
                        writer.WriteLine("{0}*{1}", filemd5.Key, filemd5.Value);
                    }
                }
            }
        }
    }
}
