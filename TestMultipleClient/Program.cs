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
                bullupPath[i] = "E:\\test" + i;
            }

            for (int i = 0; i < length; i++) {
                TCPClient client = new TCPClient("192.168.0.117", 6001);
                client.Start(bullupPath[i]);

            }
       
        }
    }
}
 