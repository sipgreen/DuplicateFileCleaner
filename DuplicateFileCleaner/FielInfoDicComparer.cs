using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DuplicateFileCleaner
{
    internal class FielInfoDicComparer : IComparer<KeyValuePair<string, List<FileInfo>>>
    {
        Dictionary<string, List<FileInfo>> dic;
        public FielInfoDicComparer(Dictionary<string, List<FileInfo>> ddic)
        {
            dic = ddic;
        }
        public int Compare(KeyValuePair<string, List<FileInfo>> x, KeyValuePair<string, List<FileInfo>> y)
        {
            int ret = 0;
            if(dic.ContainsKey(x.Key) && dic.ContainsKey(y.Key))
            {
                if(x.Value.Count > y.Value.Count)
                {
                    ret = 1;
                }
                else if(x.Value.Count == y.Value.Count)
                {
                    ret = StringComparer.CurrentCultureIgnoreCase.Compare(x.Key, y.Key) * -1;
                }
                else if(x.Value.Count < y.Value.Count)
                {
                    ret = -1;
                }
            }
            else if(dic.ContainsKey(x.Key) && !dic.ContainsKey(y.Key))
            {
                ret = 1;
            }
            else if (!dic.ContainsKey(x.Key) && dic.ContainsKey(y.Key))
            {
                ret = -1;
            }
            else
            {
                ret = 0;
            }
            return ret * -1;
        }
    }
}
