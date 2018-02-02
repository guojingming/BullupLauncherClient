using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections;
using System.Web.Script.Serialization;
using System.Diagnostics;


namespace TCPLib {
    public class TCPClient {

        

        private string ip;
        public string IP {
            get { return ip; }
            set { ip = value; }
        }
        private int port;
        public int Port {
            get { return port; }
            set { port = value; }
        }
        private IPEndPoint ipEndPoint;
        private Socket mClientSocket;
        private bool isConnected = false;
        private int id;

        public TCPClient(string ip, int id ) {
            this.ip = ip;
            this.id = port;
            this.port = 6001;
            //初始化IP终端
            this.ipEndPoint = new IPEndPoint(IPAddress.Parse(this.ip), this.port);
            //初始化客户端Socket
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private String bullupPath = "";
        private String autoprogramPath = "";
        private Thread mConnectThread;
        private Thread mReceiveThread;
        private Dictionary<String, String> bullupFileMd5 = null;
        private Dictionary<String, String> autoscriptFileMd5 = null;

        public void ShutDown() {
            try {
                mConnectThread.Abort();
                mReceiveThread.Abort();
            } catch (Exception e) { 
                
            }
        }

        public void Start(String path) {

            bullupPath = path;
            autoprogramPath = "C:\\Users\\Public\\Bullup\\auto_program";
            getFilesMD5(bullupPath);

            mConnectThread = new Thread(this.ConnectToServer);
            mConnectThread.Start();
        }
        public void getFilesMD5(String path) {
            bullupFileMd5 = new Dictionary<String, String>();
            autoscriptFileMd5 = new Dictionary<String, String>();
            ArrayList files = new ArrayList();
            try {
                GetAllFiles(new DirectoryInfo(path), files);
            } catch (Exception e) {
                //return;
            }
            for (int i = 0; i < files.Count; i++) {
                String totalPath = files[i].ToString();
                String localPath = totalPath.Substring(totalPath.IndexOf(path) + path.Length);
                bullupFileMd5.Add(localPath, GetMD5HashFromFile(files[i].ToString()));
            }
            files.Clear();
            try {
                GetAllFiles(new DirectoryInfo("C:\\Users\\Public\\Bullup\\auto_program"), files);
            } catch (Exception e) { 
                //
            }
            for (int i = 0; i < files.Count; i++) {
                String totalPath = files[i].ToString();
                String localPath = totalPath.Substring(totalPath.IndexOf("C:\\Users\\Public\\Bullup\\auto_program") + "C:\\Users\\Public\\Bullup\\auto_program".Length);
                autoscriptFileMd5.Add(localPath, GetMD5HashFromFile(files[i].ToString()));
            }
        }

        public void GetAllFiles(DirectoryInfo rootDirectory, ArrayList files) {
            foreach (FileInfo file in rootDirectory.GetFiles("*")) {
                files.Add(file.FullName);
                //Console.WriteLine(file.FullName);
            }
            DirectoryInfo[] directories = rootDirectory.GetDirectories();
            foreach (DirectoryInfo directory in directories) {
                GetAllFiles(directory, files);
            }
        }

        public static string GetMD5HashFromFile(string fileName) {
            try {
                File.SetAttributes(fileName, FileAttributes.Normal);
                FileStream file = new FileStream(fileName, FileMode.Open);
                
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    stringBuilder.Append(retVal[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            } catch (Exception ex) {
                return "";
                //throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }


        public int maxCount = 0;
        public int currentCount = 0;
        public int oriCount = -1;

        public int getMaxCount() {
            return maxCount;
        }

        private void ConnectToServer() {
            while (!isConnected) {
                try {
                    mClientSocket.Connect(this.ipEndPoint);
                    this.isConnected = true;
                    Console.WriteLine("连接成功");
                } catch (Exception e) {
                   
                    Console.WriteLine(string.Format("因为一个错误的发生，暂时无法连接到服务器，错误信息为:{0}", e.Message));
                    this.isConnected = false;
                    mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);                   
                }
                Thread.Sleep(1000);
                //Console.WriteLine("正在尝试重新连接...");
            }
            //Console.WriteLine("连接服务器成功，现在可以和服务器进行会话了");
            //var mReceiveThread = new Thread(this.ReceiveMessage);
            mReceiveThread = new Thread(this.ReceiveFiles);
            mReceiveThread.Start();
        }

        //private void ReceiveMessage() {
        //    //设置循环标志位
        //    bool flag = true;
        //    while (flag) {
        //        try {
        //            //获取数据长度
        //            int receiveLength = this.mClientSocket.Receive(result);
        //            //获取服务器消息
        //            string serverMessage = Encoding.UTF8.GetString(result, 0, receiveLength);
        //            //输出服务器消息
        //            //Console.WriteLine("From server: " + serverMessage);
        //        } catch (Exception e) {
        //            //停止消息接收
        //            flag = false;
        //            //断开服务器
        //            this.mClientSocket.Shutdown(SocketShutdown.Both);
        //            //关闭套接字
        //            this.mClientSocket.Close();
        //            //重新尝试连接服务器
        //            this.isConnected = false;
        //            ConnectToServer();
        //        }
        //    }
        //}

        private void ReceiveFiles() {
            //杀进程
            //杀Bullup进程
            Process[] p1 = Process.GetProcessesByName("Bullup");
            for (int i = 0; i < p1.Length; i++) {
                try {
                    p1[i].Kill();
                } catch (Exception e) {

                }
            }
            Process[] p2 = Process.GetProcessesByName("BullupServiceNew");
            for (int i = 0; i < p2.Length; i++) {
                try {
                    p2[i].Kill();
                } catch (Exception e) {

                }
            }   


            try {
                Console.WriteLine("准备接收文件");
                byte[] result = new byte[300];
                //
                String path = bullupPath;
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                
                //传安装路径
RE_FILECOUNT:
                Console.WriteLine("准备Bullup文件列表");
                //Bullup文件列表序列化  发送
                String bullupFileJson = jsonSerialize.Serialize(bullupFileMd5);//将对象转换成json存储
                Console.WriteLine("发送Bullup文件列表");
SendMessage("BULLUPFILEJSON$" + bullupFileJson + "$");
                //确认接到
RecieveMessage(ref result);
                if (Encoding.UTF8.GetString(result).IndexOf("BULLUP_FILE_OK") == 0) {

                } else {
                    goto RE_FILECOUNT;
                }

RE_AUTOJSON:
                Console.WriteLine("准备auto_program文件列表");
                //AutoScript文件列表序列化  发送
                String autoprogramFileJson = jsonSerialize.Serialize(autoscriptFileMd5);//将对象转换成json存储
                Console.WriteLine("发送auto_program文件列表");
SendMessage("AUTOFILEJSON$" + autoprogramFileJson + "$");
                RecieveMessage(ref result);
                if (Encoding.UTF8.GetString(result).IndexOf("AUTOSCRIPT_FILE_OK") == 0) {

                } else {
                    goto RE_AUTOJSON;
                }

                Console.WriteLine("发送安装路径");
                //确认接到
                SendMessage("PATH$" + path + "$");
                //获取数据长度
                int receiveLength = RecieveMessage(ref result);
                string serverMessage = Encoding.UTF8.GetString(result, 0, receiveLength);
                Console.WriteLine("开始传输文件");
                if (serverMessage.IndexOf("INSTALLFILECOUNT$") == 0) {
                    //开始传输
                    serverMessage = serverMessage.Substring(17);
                    int fileCount = 0;
                    try {
                        fileCount = Int32.Parse(serverMessage.Substring(0, serverMessage.IndexOf("$")));
                    } catch (Exception e) {
                        Console.WriteLine("fileCount接收错误");
                        SendMessage("RE_FILECOUNT");
                        goto RE_FILECOUNT;
                    }
                    SendMessage("FILECOUNT_OK");
                    int transedCount = 0;
                    maxCount = fileCount;
                    oriCount = fileCount;
                    //
                    while (transedCount != fileCount) {
                        
                        //通知服务器开始传
                        //SendMessage(transedCount.ToString());
                        //接文件头  路径和大小
RE_SEND:
                        RecieveMessage(ref result);
                        String fileSizeStr = null;
                        String filePathStr = null;
                        String oriFilePathStr = null;
                        String oriFileSizeStr = null;
                        int fileSize = 0;
                        fileSizeStr = Encoding.UTF8.GetString(result);
                        oriFileSizeStr = fileSizeStr;
                        fileSizeStr = fileSizeStr.Substring(fileSizeStr.IndexOf("FILESIZE$") + 9);
                        try {
                            fileSize = Int32.Parse(fileSizeStr);
                        } catch (Exception e) {
                            Console.WriteLine("fileSize接收错误");
                            SendMessage("RE_SEND");
                            goto RE_SEND;
                        }
                        
                        SendMessage("SIZE_OK");
                        RecieveMessage(ref result);
                        filePathStr = Encoding.UTF8.GetString(result);
                        oriFilePathStr = filePathStr;
                        filePathStr = filePathStr.Substring(filePathStr.IndexOf("FILEPATH$") + 9);
                        filePathStr = filePathStr.Substring(0, filePathStr.LastIndexOf("$"));
Console.WriteLine(mClientSocket.LocalEndPoint.ToString() + " : " + transedCount + " / " + fileCount + " : " + filePathStr);
                        //通知服务器开始传文件数据
                        SendMessage("PATH_OK");

                        //准备文件存储区
                        byte[] file = new byte[fileSize];
                        int blockSize = 200 * 1024;
                        byte[] buffer = new byte[blockSize];
                        //开始传
                        int receievedSize = 0;
                        while (receievedSize != fileSize) {
                            int tempSize = mClientSocket.Receive(buffer);
                            for (int i = 0; i < tempSize; i++) {
                                file[receievedSize + i] = buffer[i];
                            }
                            receievedSize += tempSize;
                            Console.WriteLine("{0} / {1}", receievedSize, fileSize);
                        }
                        //写到本地磁盘
                        try {
                            WriteFile(filePathStr, file);
                            //更新文件md5列表
                            if (filePathStr.IndexOf("C:\\Users\\Public\\Bullup\\auto_program") == 0) {
                                String localPath = filePathStr.Substring(filePathStr.IndexOf("C:\\Users\\Public\\Bullup\\auto_program") + "C:\\Users\\Public\\Bullup\\auto_program".Length);
                                if (autoscriptFileMd5.ContainsKey(localPath)) {
                                    autoscriptFileMd5[localPath] = GetMD5HashFromFile(filePathStr);
                                } else {
                                    autoscriptFileMd5.Add(localPath, GetMD5HashFromFile(filePathStr));
                                } 
                            } else {
                                String localPath = filePathStr.Substring(filePathStr.IndexOf(bullupPath) + bullupPath.Length);
                                if (bullupFileMd5.ContainsKey(localPath)) {
                                    bullupFileMd5[localPath] = GetMD5HashFromFile(filePathStr);
                                } else {
                                    bullupFileMd5.Add(localPath, GetMD5HashFromFile(filePathStr));
                                }
                            }
                        } catch (Exception e) {
                            Console.WriteLine("写文件出错");
                            SendMessage("RE_SEND");
                            goto RE_SEND;
                        }
                        //通知服务器已接完该文件  服务器可以传下个文件
                        SendMessage("DATA_OK");
                        currentCount = transedCount;
                        transedCount++;
                    }
                    currentCount++;
                    Console.WriteLine(mClientSocket.LocalEndPoint.ToString() + " 传输完成");
                    CreateShortcuts(bullupPath + "\\Bullup.exe", "C:\\Users\\" + Environment.UserName + "\\Desktop", "斗牛电竞");
                } else if (serverMessage.IndexOf("UPDATEFILECOUNT$") == 0) {

                }
            } catch (Exception e) {
                //断开服务器
                this.mClientSocket.Shutdown(SocketShutdown.Both);
                //关闭套接字
                this.mClientSocket.Close();
                //重新尝试连接服务器
                this.isConnected = false;
                ConnectToServer();
            }
        }

        public int RecieveMessage(ref byte[] result) {
            Array.Clear(result, 0, result.Length);
            return this.mClientSocket.Receive(result);
        }

        public void WriteFile(String filePathStr, byte[] fileData) {

            //if (filePathStr.IndexOf("C:\\Users\\Public\\Bullup\\auto_program") == 0) {
            //    filePathStr = filePathStr.Substring(filePathStr.IndexOf("C:\\Users\\Public\\Bullup\\auto_program") + "C:\\Users\\Public\\Bullup\\auto_program".Length);
            //    filePathStr = "C:\\Users\\Public\\Bullup\\auto_program" + filePathStr;
            //}

            FileStream fs = null;
            try {
                fs = new FileStream(filePathStr, FileMode.CreateNew, FileAccess.Write);
            } catch (System.IO.DirectoryNotFoundException exception) {
                CreateDirectory(filePathStr.Substring(0, filePathStr.LastIndexOf("\\")));
                fs = new FileStream(filePathStr, FileMode.CreateNew, FileAccess.Write);
            } catch (System.IO.IOException exception) {
                //Console.WriteLine("file exsist");
                File.Delete(filePathStr);
                fs = new FileStream(filePathStr, FileMode.CreateNew, FileAccess.Write);
            } catch (Exception e) {
                int a = 0;
            }
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(fileData);
            bw.Close();
            fs.Close();
        }

        public void CreateDirectory(String directoryPath) {
            Directory.CreateDirectory(directoryPath);
        }

        public void SendMessage(string msg) {
//Console.WriteLine(this.mClientSocket.LocalEndPoint.ToString() + " send: " + msg);
            if (msg == string.Empty || this.mClientSocket == null) {
                return;
            }
            mClientSocket.Send(Encoding.UTF8.GetBytes(msg));
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
    }
}