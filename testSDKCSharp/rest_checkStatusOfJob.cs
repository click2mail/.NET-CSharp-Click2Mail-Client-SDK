using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using c2mAPI;
namespace testSDKCSharp
{
    class rest_checkStatusOfJob
    {
        static void Main(string[] args)
        {
            Restc2mAPI r = new Restc2mAPI("username", "password", Restc2mAPI.liveMode.Stage);
            r.jobId = 1234;
            r.jobStatusCheck += r_jobStatusCheck;
            Console.Write(r.checkJobStatus());
            Console.ReadLine();
        }

        static void r_jobStatusCheck(string id, string status, string description)
        {
              
            Console.WriteLine("jobId is:" + id);
            Console.WriteLine("job Status is:" + status);
           Console.WriteLine("job Description is:" + description);
        
        }
    }

}
