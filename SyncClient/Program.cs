using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPLib;

namespace SyncClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //保证端口号和服务端一致
            TCPLib.TCPClient c = new TCPClient("127.0.0.1", 6001);
            //执行Start方法
            c.Start();
            while (true)
            {
                //读取客户端输入的消息
                string msg = Console.ReadLine();
                //发送消息到服务端
                c.SendMessage(msg);
            }

        }
    }
}
