using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using c2mAPI;
namespace testSDKCSharp
{
    class rest_SimpleSendCustomAddress_Test
    {
        static void Main(string[] args)
        {
            Restc2mAPI r = new Restc2mAPI("username", "password", Restc2mAPI.liveMode.Stage);
            List<List<KeyValuePair<String, String>>> addressList =new List<List<KeyValuePair<String, String>>>();
        List<KeyValuePair<String, String>> customAddressItem = new List<KeyValuePair<String, String>>();
        customAddressItem.Add(new KeyValuePair< String, String>("name", "John"));;
        customAddressItem.Add(new KeyValuePair< String, String>("Address1", "1234 Test Street"));
        customAddressItem.Add(new KeyValuePair< String, String>("Address2", "Ste 335"));
        customAddressItem.Add(new KeyValuePair< String, String>("city", "Oak Brook"));
        customAddressItem.Add(new KeyValuePair< String, String>("StAtE", "IL"));
        customAddressItem.Add(new KeyValuePair< String, String>("Zip", "60523"));
        addressList.Add(customAddressItem);
        customAddressItem = new List<KeyValuePair< String, String>>();
        customAddressItem.Add(new KeyValuePair< String, String>("name", "John2"));
        customAddressItem.Add(new KeyValuePair< String, String>("Address1", "1234 Test Street"));
        customAddressItem.Add(new KeyValuePair< String, String>("Address2", "Ste 335"));
        customAddressItem.Add(new KeyValuePair< String, String>("city", "Oak Brook"));
        customAddressItem.Add(new KeyValuePair< String, String>("StAtE", "IL"));
        customAddressItem.Add(new KeyValuePair< String, String>("Zip", "60523"));
        addressList.Add(customAddressItem);
        r.runComplete(@"C:\c2m\test.pdf", r.createXMLFromCustomList(addressList, 12345), "Letter 8.5 x 11", "Address on Separate Page", "Next Day", "#10 Double Window", "Black and White", "White 24#", "Printing Both sides");
        Console.ReadLine();

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
