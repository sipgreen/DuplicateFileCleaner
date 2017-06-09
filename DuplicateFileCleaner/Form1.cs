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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (!Directory.Exists(this.textBox1.Text))
                {
                    MessageBox.Show("路径不合理");
                    return;
                }
                this.Hide();
                SingleFolder folder = new SingleFolder(this.textBox1.Text);
                folder.ShowDialog();
                this.Show();
            }
            else
            {
                if (!Directory.Exists(this.textBox2.Text) || !Directory.Exists(this.textBox3.Text))
                {
                    MessageBox.Show("路径不合理");
                    return;
                }
                if (this.textBox2.Text==this.textBox3.Text)
                {
                    MessageBox.Show("路径不能相同");
                    return;
                }
                this.Hide();
                DoubleFolder folder = new DoubleFolder(this.textBox2.Text, this.textBox3.Text);
                folder.ShowDialog();
                this.Show();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.ShowDialog();
            this.textBox1.Text = fbd.SelectedPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.ShowDialog();
            this.textBox2.Text = fbd.SelectedPath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            fbd.ShowDialog();
            this.textBox3.Text = fbd.SelectedPath;
        }
    }
}
