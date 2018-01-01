using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPLib;

namespace SyncClient {
    class Program {
        static void Main(string[] args) {
            TCPClient client = new TCPClient("127.0.0.1", 6001); 
            //执行Start方法
            client.Start("E:\\Temp");
        }
    }
}
