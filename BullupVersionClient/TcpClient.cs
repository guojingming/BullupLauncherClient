using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

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

        public TCPClient(string ip, int port) {
            this.ip = ip;
            this.port = port;
            //初始化IP终端
            this.ipEndPoint = new IPEndPoint(IPAddress.Parse(this.ip), this.port);
            //初始化客户端Socket
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private String bullupPath = "";
        public void Start(String path) {
            bullupPath = path;
            var mConnectThread = new Thread(this.ConnectToServer);
            mConnectThread.Start();
        }

        public int maxCount = 0;
        public int currentCount = 0;

        public int getMaxCount() {
            return maxCount;
        }

        private void ConnectToServer() {
            while (!isConnected) {
                try {
                    mClientSocket.Connect(this.ipEndPoint);
                    this.isConnected = true;
                } catch (Exception e) {
                    //Console.WriteLine(string.Format("因为一个错误的发生，暂时无法连接到服务器，错误信息为:{0}", e.Message));
                    this.isConnected = false;
                }
                Thread.Sleep(1000);
                //Console.WriteLine("正在尝试重新连接...");
            }
            //Console.WriteLine("连接服务器成功，现在可以和服务器进行会话了");
            //var mReceiveThread = new Thread(this.ReceiveMessage);
            var mReceiveThread = new Thread(this.ReceiveFiles);
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
            try {
                byte[] result = new byte[1024];
                //
                String path = bullupPath;
                //传安装路径
                SendMessage("PATH#" + path + "#");
                //获取数据长度
                int receiveLength = this.mClientSocket.Receive(result);
                string serverMessage = Encoding.UTF8.GetString(result, 0, receiveLength);
                if (serverMessage.IndexOf("INSTALLFILECOUNT#") == 0) {
                    //开始传输
                    serverMessage = serverMessage.Substring(17);
                    int fileCount = Int32.Parse(serverMessage.Substring(0, serverMessage.IndexOf("#")));
                    int transedCount = 0;
                    maxCount = fileCount;
                    //
                    while (transedCount != fileCount) {
                        currentCount = transedCount;
                        //通知服务器开始传
                        //SendMessage(transedCount.ToString());
                        //接文件头  路径和大小
                        this.mClientSocket.Receive(result);
                        String fileSizeStr = null;
                        String filePathStr = null;
                        int fileSize = 0;
                        fileSizeStr = Encoding.UTF8.GetString(result);
                        fileSizeStr = fileSizeStr.Substring(fileSizeStr.IndexOf("FILESIZE#") + 9);
                        fileSizeStr = fileSizeStr.Substring(0, fileSizeStr.IndexOf("#"));
                        fileSize = Int32.Parse(fileSizeStr);

                        filePathStr = Encoding.UTF8.GetString(result);
                        filePathStr = filePathStr.Substring(filePathStr.IndexOf("FILEPATH#") + 9);
                        filePathStr = filePathStr.Substring(0, filePathStr.IndexOf("#"));
//Console.WriteLine(mClientSocket.LocalEndPoint.ToString() + " : " + transedCount + " / " + fileCount);
                        //通知服务器开始传文件数据
                        SendMessage("DATA_READY");

                        //准备文件存储区
                        byte[] file = new byte[fileSize];
                        int blockSize = 4 * 1024 * 1024;
                        byte[] buffer = new byte[blockSize];
                        //开始传
                        int receievedSize = 0;
                        while (receievedSize != fileSize) {
                            int tempSize = mClientSocket.Receive(buffer);
                            for (int i = 0; i < tempSize; i++) {
                                file[receievedSize + i] = buffer[i];
                            }
                            receievedSize += tempSize;
                        }
                        //写到本地磁盘
                        WriteFile(filePathStr, file);

                        //通知服务器已接完该文件  服务器可以传下个文件
                        SendMessage("DATA_OK");
                        transedCount++;
                    }
                    Console.WriteLine(mClientSocket.LocalEndPoint.ToString() + " 传输完成");
                } else if (serverMessage.IndexOf("UPDATEFILECOUNT#") == 0) {

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


        public void WriteFile(String filePathStr, byte[] fileData) {
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
    }
}