using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCPLib;

namespace TestMultipleClient {
    class Program {
        static void Main(string[] args) {
            int length = 1;
            String[] bullupPath = new String[length];
            for (int i = 0; i < length; i++) {
                bullupPath[i] = "C:\\test" + i;
            }
            for (int i = 0; i < length; i++) {
                //TCPClient client = new TCPClient("127.0.0.1", i);
                TCPClient client = new TCPClient("13.58.18.43", i);
                //TCPClient client = new TCPClient("192.168.0.117", i);
                client.Start(bullupPath[i]);
            }
            Console.ReadKey();
        }
    }
}
 