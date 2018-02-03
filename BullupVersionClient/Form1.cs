using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCPLib;

namespace BullupVersionClient {

    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void Form1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left & this.WindowState == FormWindowState.Normal) {
                // 移动窗体
                this.Capture = false;
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private String bullupPath = "";

        private TCPClient client;

        private void button1_Click(object sender, EventArgs e) {
            //if (bullupPath == "") {
            //    FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            //    folderDlg.ShowDialog();
            //    bullupPath = folderDlg.SelectedPath;
            //    textBox1.Text = bullupPath;
            //    if (bullupPath != "") {
            //        button1.Text = "开始安装/更新";
            //    }
            //} else {
            //    if (button1.Text == "开始安装/更新") {
            //        client = new TCPClient("18.220.98.48", 6001);
            //        //执行Start方法
            //        client.Start(bullupPath);
            //        button1.Enabled = false;

            //        Thread th = new Thread(ThreadChild);
            //        th.Start();
            //    }
            //}
        }


        protected void CreateShortcuts(String targetPath, String savePath, String saveName) {
            IWshRuntimeLibrary.IWshShell shell_class = new IWshRuntimeLibrary.IWshShell_Class();
            IWshRuntimeLibrary.IWshShortcut shortcut = null;
            //if (!Directory.Exists(targetPath))
            //    return;
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            try {
                shortcut = shell_class.CreateShortcut(savePath + @"/" + saveName + ".lnk") as IWshRuntimeLibrary.IWshShortcut;
                shortcut.TargetPath = targetPath;
                shortcut.Save();
                //MessageBox.Show("创建快捷方式成功！");
            } catch (Exception ex) {
                //MessageBox.Show("创建快捷方式失败！");
            }
        } 

        private void ThreadChild() {
            while(true){
                try {
                    if(client.oriCount == 0){
                        MessageBox.Show("安装/更新完成");
                        this.Close();
                        break;
                    }
                    progressBar1.Maximum = client.maxCount;
                    progressBar1.Value = client.currentCount;
                    label1.Text = (progressBar1.Value).ToString();
                    label3.Text = client.maxCount.ToString();

                    progressBar2.Maximum = client.fileMaxSize;
                    if(client.fileCurrentSize < client.fileMaxSize){
                        progressBar2.Value = client.fileCurrentSize;
                    } else {
                        progressBar2.Value = client.fileMaxSize;
                    }

                    if (progressBar2.Maximum != 0) {
                        int value = progressBar2.Value * 100 / progressBar2.Maximum;
                        if (value > 100) {
                            value = 100;
                        }
                        label7.Text = value.ToString();
                    } else {
                        label7.Text = "0";
                    }

                    label12.Text = client.fileCurrentName;

                    if (client.maxCount == progressBar1.Value && client.currentCount != 0) {
                        MessageBox.Show("安装/更新完成");
                        this.Close();
                        //创建桌面快捷方式
                        //Environment.UserName
                        //CreateShortcuts(bullupPath + "\\Bullup.exe", "C:\\Users\\" + Environment.UserName + "\\Desktop", "斗牛电竞");
                        break;
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
                Thread.Sleep(50);
            }
            
        }

        private void label2_Click(object sender, EventArgs e) {

        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void progressBar1_Click(object sender, EventArgs e) {

        }

        private void label3_Click(object sender, EventArgs e) {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void button2_Click(object sender, EventArgs e) {
            try {
                uiThread.Abort();
            } catch (Exception ex) { 
            
            }
            try {
                client.ShutDown();
            } catch (Exception ex) { 
            
            }
            
            
            this.Close();
            System.Environment.Exit(0);
        }

        private void button3_Click(object sender, EventArgs e) {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_Load(object sender, EventArgs e) {
            Control.CheckForIllegalCrossThreadCalls = false;
            label4.SetBounds(1000,1000,70,20);
            pictureBox1.BackgroundImage = Properties.Resources._2;
        }

        private Thread uiThread;

        private void pictureBox1_Click(object sender, EventArgs e) {
            if (bullupPath == "") {
                FolderBrowserDialog folderDlg = new FolderBrowserDialog();
                folderDlg.ShowDialog();
                bullupPath = folderDlg.SelectedPath;
                textBox1.Text = bullupPath;
                if (bullupPath != "") {
                    label4.Text = "下载";
                    pictureBox1.BackgroundImage = Properties.Resources._1;
                }
            } else {
                if (label4.Text == "下载") {
                      

                    client = new TCPClient("13.58.18.43", 0);
                    
                    //执行Start方法
                    client.Start(bullupPath);
                    pictureBox1.Enabled = false;

                    uiThread = new Thread(ThreadChild);
                    uiThread.Start();
                    button1.Enabled = true;
                }
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e) {
            if ( label4.Text == "选择" ) {
                pictureBox1.BackgroundImage = Properties.Resources._4;
            } else if (label4.Text == "下载") {
                pictureBox1.BackgroundImage = Properties.Resources._3;
            }

           
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (label4.Text == "选择") {
                pictureBox1.BackgroundImage = Properties.Resources._6;
            } else if (label4.Text == "下载") {
                pictureBox1.BackgroundImage = Properties.Resources._5;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e) {
            if (label4.Text == "选择") {
                pictureBox1.BackgroundImage = Properties.Resources._2;
            } else if (label4.Text == "下载") {
                pictureBox1.BackgroundImage = Properties.Resources._1;
            }
        }

        private void button1_Click_1(object sender, EventArgs e) {
            client.ShutDown();
            client = new TCPClient("13.58.18.43", 0);

            //执行Start方法
            client.Start(bullupPath);
            try {
                uiThread.Abort();
            } catch (Exception ex) { 
            
            }
            
            uiThread = new Thread(ThreadChild);
            uiThread.Start();
        }
    }
}
