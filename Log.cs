using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.Standard.MW.ListServicePlugIncs
{
    public class Log
    {
        public static void writestr(string ftype, string dataJSON, string data, string fname,string path)
        {
            
            //debug==================================================  
            //StreamWriter dout = new StreamWriter(@"c:\" + System.DateTime.Now.ToString("yyyMMddHHmmss") + ".txt");  
            StreamWriter dout = new StreamWriter(path + System.DateTime.Now.ToString("yyyMMdd") + fname + ".txt", true);
            //dout.Write(readme + "\r\n");
            dout.Write("\r\n事件：" + ftype + "\r\n请求JSON：" + "\r\n" + dataJSON + "\r\n返回结果：" + "\r\n" + data + "\r\n操作时间：" + System.DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
            //debug==================================================  
            dout.Close();
        }
    }
}
