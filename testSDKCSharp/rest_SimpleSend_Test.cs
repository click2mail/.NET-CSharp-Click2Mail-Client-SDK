using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using c2mAPI;


namespace testSDKCSharp
{
    class rest_SimpleSend
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting REST TEST");

            Restc2mAPI r = new Restc2mAPI("username", "password", Restc2mAPI.liveMode.Stage);
            r.statusChanged += r_statusChanged;
            r.jobStatusCheck += r_jobStatusCheck;
            r.addressList.Clear(); //Not needed just good habit to clear before you add any addresses
            Restc2mAPI.addressItem x ;
            x = new Restc2mAPI.addressItem("John", "Smith", "TestOrg", "1234 Test Street", "Ste 335", "", "Oak Brook", "IL", "60523", "");
            r.addressList.Add(x);
            x = new Restc2mAPI.addressItem("John", "Smith2", "TestOrg", "1234 Test Street", "Ste 335", "", "Oak Brook", "IL", "60523", "");
            r.addressList.Add(x);
            r.runComplete(@"/Users/mlavannis/Documents/PDFs/test_letter.pdf", r.createXMLFromAddressList(), "Letter 8.5 x 11", "Address on Separate Page", "Next Day", "#10 Double Window", "Black and White", "White 24#", "Printing Both sides");
            Console.ReadLine();
            Console.WriteLine("Completed REST TEST");
        }

        static void r_jobStatusCheck(string id, string status, string description)
        {
            Console.WriteLine("jobId is:" + id);
            Console.WriteLine("job Status is:" + status);
            Console.WriteLine("job Description is:" + description);
        }

        static void r_statusChanged(string reason)
        {
                    Console.WriteLine(reason);
        }
    }
}
