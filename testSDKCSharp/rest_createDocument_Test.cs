using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using c2mAPI;
namespace testSDKCSharp
{
    class rest_createDocument_Test
    {
        static void Main(string[] args)
        {
                  Restc2mAPI r = new Restc2mAPI("username", "password", Restc2mAPI.liveMode.Stage);
                  Console.Write("DocumentID IS:" + r.createDocumentSimple(@"C:\c2m\test.pdf"));
                  Console.ReadLine();
        }
    }

}
