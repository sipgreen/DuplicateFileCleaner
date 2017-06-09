using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DuplicateFileCleaner
{
    public partial class SingleFolder : Form
    {
        private string path;
        Dictionary<string, List<FileInfo>> dic;
        public SingleFolder(string pathStr)
        {
            InitializeComponent();
            this.path = pathStr;
            dic = new Dictionary<string, List<FileInfo>>();
        }


        private SingleFolder()
        {
            InitializeComponent();
        }

        private void SingleFolder_Load(object sender, EventArgs e)
        {
            dic = Functions.GenMD5Dic(path);
            var ie = dic.OrderBy(o => o.Value.Count * -1);
            foreach (var it in ie)
            {
                listView1.Groups.Add(GenGroup(listView1, it));
            }
        }

        public static ListViewGroup GenGroup(ListView listView1, KeyValuePair<string, List<FileInfo>> list)
        {
            ListViewGroup group = new ListViewGroup(list.Value[0].Name, HorizontalAlignment.Left);

            int indx = listView1.Items.Count;
            for (int i = 0; i < list.Value.Count; i++)
            {
                var it = list.Value[i];
                var strs = new string[] { it.Name, it.FullName, it.LastWriteTime.ToString(), list.Key };
                var vitm = Functions.GenListViewItem(indx, strs);
                group.Items.Add(vitm);
                listView1.Items.Add(vitm);
                listView1.Groups.Add(group);
                indx++;
            }
            for (int i = 0; i < group.Items.Count; i++)
            {
                if (i > 0)
                {
                    group.Items[i].BackColor = Color.YellowGreen;
                    group.Items[i].Checked = true;
                }
            }
            return group;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要删除这些资料到回收站？可以从回收站找回", "〖更新准备〗确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                foreach (ListViewGroup group in listView1.Groups)
                {
                    var d = new List<ListViewItem>();
                    foreach (ListViewItem tt in group.Items)
                    {
                        d.Add(tt);
                    }
                    foreach (ListViewItem vt in d)
                    {

                        if (vt.Checked)
                        {
                            group.Items.Remove(vt);
                            this.listView1.Items.Remove(vt);
                            FileSystem.DeleteFile(vt.SubItems[2].Text, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        }
                    }
                }
            }

        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Unchecked || e.NewValue == CheckState.Indeterminate)
            {
                return;
            }

            var itm = listView1.Items[e.Index];
            int count = 0;
            foreach (ListViewItem vt in itm.Group.Items)
            {
                if (!vt.Checked && !vt.Equals(itm))
                {
                    count++;
                }
            }
            if (count < 1)
            {
                if (MessageBox.Show("确定要删除:\r\n" + itm.Group.Header + "\r\n所有相同文件吗", "〖删除准备〗确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                }
                else
                {
                    e.NewValue = CheckState.Unchecked;
                    //itm.Checked = !itm.Checked; //改这样
                }
            }
        }

        private void sdfsdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count < 1)
            {
                return;
            }
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/select," + listView1.SelectedItems[0].SubItems[2].Text;
            System.Diagnostics.Process.Start(psi);
            //(sender as ListView)
            //ListViewHitTestInfo li = listView1.HitTest(e.Location);
            //if (li != null && li.Item != null)
            //{
            // li.Item.Checked = !li.Item.Checked; //改这样
            //}
        }

        private void SingleFolder_FormClosed(object sender, FormClosedEventArgs e)
        {
            Functions.WriteMd5History(dic);
        }

    }
}
