using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DuplicateFileCleaner
{
    public partial class DoubleFolder : Form
    {
        private string p1;
        private string p2;
        Dictionary<string, List<FileInfo>> dic1;
        Dictionary<string, List<FileInfo>> dic2;
        private DoubleFolder()
        {
            InitializeComponent();
        }

        public DoubleFolder(string p1, string p2)
        {
            InitializeComponent();
            this.p1 = p1;
            this.p2 = p2;
        }

        private void DoubleFolder_Load(object sender, EventArgs e)
        {
            this.label1.Text = "目录：" + p1 + "\r\n\r\n " + this.label1.Text;
            this.label2.Text = "目录：" + p2 + "\r\n\r\n " + this.label2.Text;
            this.label1.Height += 10;
            this.label2.Height += 10;

            dic1 = Functions.GenMD5Dic(p1);
            dic2 = Functions.GenMD5Dic(p2);

            var iedic1 = dic1.OrderBy(o => o, new FielInfoDicComparer(dic2));
            foreach (var it in iedic1)
            {
                var grop = SingleFolder.GenGroup(listView1, it);
                if (dic2.ContainsKey(it.Key))
                {
                    foreach (ListViewItem vitm in grop.Items)
                    {
                        vitm.ForeColor = Color.DarkRed;
                    }
                }
            }

            var iedic2 = dic2.OrderBy(o => o, new FielInfoDicComparer(dic1));
            foreach (var it in iedic2)
            {
                var grop = SingleFolder.GenGroup(listView2, it);
                if (dic1.ContainsKey(it.Key))
                {
                    foreach (ListViewItem vitm in grop.Items)
                    {
                        vitm.Checked = true;
                        vitm.ForeColor = Color.DarkRed;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = checkBox1.Checked ? "确认彻底删除吗？" : "确定要删除这些资料到回收站？可以从回收站找回";
            if (MessageBox.Show(message, "〖更新准备〗确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                foreach (ListViewGroup group in listView2.Groups)
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
                            this.listView2.Items.Remove(vt);
                            if (checkBox1.Checked)
                            {
                                File.Delete(vt.SubItems[2].Text);
                            }
                            else
                            {
                                FileSystem.DeleteFile(vt.SubItems[2].Text, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                        }
                    }
                }
            }
        }

        private void DoubleFolder_FormClosed(object sender, FormClosedEventArgs e)
        {
            Functions.WriteMd5History(dic1);
            Functions.WriteMd5History(dic2);
        }
    }
}
