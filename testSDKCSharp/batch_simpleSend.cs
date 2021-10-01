using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using c2mAPI;
namespace testSDKCSharp
{
    class batch_simpleSend
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Batch Test");
            Batchc2mAPI r = new Batchc2mAPI("mlavannis_rest", "3sap5t0E", Batchc2mAPI.liveMode.Stage);
            r.statusChanged += r_statusChanged;

            List<Batchc2mAPI.batchJob> batchJobs = new List<Batchc2mAPI.batchJob>();
            List<Batchc2mAPI.addressItem> addList = new List<Batchc2mAPI.addressItem>();
            Batchc2mAPI.batchJob job;

            Batchc2mAPI.addressItem address1 = new Batchc2mAPI.addressItem("John", "Smith", "MyOrg", "1234 Test st", "Ste 335", "Oak Brook", "IL", "60523", "United States");
            Batchc2mAPI.addressItem address2 = new Batchc2mAPI.addressItem("John", "Smith2", "MyOrg", "1234 Test st", "Ste 335", "Oak Brook", "IL", "60523", "");

            addList.Add(address1);
            addList.Add(address2);
            
            //will send pages 1-6 of pdf to above 2 addresses using below settings
            job = new Batchc2mAPI.batchJob(1, 6, "Letter 8.5 x 11", "Address on First Page", "Next Day", "#10 Double Window", "Full Color", "White 24#", "Printing One side", "First Class", addList);
            batchJobs.Add(job);            
            
            //Second Job in batch                       
            addList = new List<Batchc2mAPI.addressItem>();
            Batchc2mAPI.addressItem address3 = new Batchc2mAPI.addressItem("John3", "Smith", "MyOrg", "1234 Test Street", "Ste 335", "Oak Brook", "IL", "60523", "United States");
            addList.Add(address3);

            //will send pages 7-10 of pdf to above address using below settings
            job = new Batchc2mAPI.batchJob(7, 10, "Letter 8.5 x 11", "Address on First Page", "Next Day", "#10 Double Window", "Full Color", "White 24#", "Printing One side", "First Class", addList);
            batchJobs.Add(job);

            r.runComplete("/Users/mlavannis/Documents/PDFs/21pages.pdf", batchJobs);
            Console.ReadLine();
            Console.WriteLine("Completed Batch Test");

        }
        static void r_statusChanged(string reason)
        {
                    Console.WriteLine(reason);
        }
    }
}
