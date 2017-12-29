using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPLib
{
    public class TCPClient
    {
       
        private byte[] result = new byte[1024];

        
        private string ip;
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        
        private int port;
        public int Port{
            get { return port; }
            set { port = value; }
        }

        private IPEndPoint ipEndPoint;

        private Socket mClientSocket;

        private bool isConnected = false;

        public TCPClient(string ip, int port){
            this.ip = ip;
            this.port = port;
            //初始化IP终端
            this.ipEndPoint = new IPEndPoint(IPAddress.Parse(this.ip), this.port);
            //初始化客户端Socket
            mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(){
            var mConnectThread = new Thread(this.ConnectToServer);
            mConnectThread.Start();
        }

        private void ConnectToServer(){
            while (!isConnected){
                try{
                    mClientSocket.Connect(this.ipEndPoint);
                    this.isConnected = true;
                }catch (Exception e){
                    Console.WriteLine(string.Format("因为一个错误的发生，暂时无法连接到服务器，错误信息为:{0}", e.Message));
                    this.isConnected = false;
                }
                Thread.Sleep(5000);
                Console.WriteLine("正在尝试重新连接...");
            }
            Console.WriteLine("连接服务器成功，现在可以和服务器进行会话了");
            var mReceiveThread = new Thread(this.ReceiveMessage);
            mReceiveThread.Start();
        }

        private void ReceiveMessage(){
            //设置循环标志位
            bool flag = true;
            while (flag){
                try{
                    //获取数据长度
                    int receiveLength = this.mClientSocket.Receive(result);
                    //获取服务器消息
                    string serverMessage = Encoding.UTF8.GetString(result, 0, receiveLength);
                    //输出服务器消息
                    Console.WriteLine("From server: " + serverMessage);
                }catch (Exception e){
                    //停止消息接收
                    flag = false;
                    //断开服务器
                    this.mClientSocket.Shutdown(SocketShutdown.Both);
                    //关闭套接字
                    this.mClientSocket.Close();
                    //重新尝试连接服务器
                    this.isConnected = false;
                    ConnectToServer();
                }
            }
        }

        public void SendMessage(string msg) {
            if (msg == string.Empty || this.mClientSocket == null) return;
            mClientSocket.Send(Encoding.UTF8.GetBytes(msg));
        }
    }
}