
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

namespace c2mAPI
{
    public class Batchc2mAPI
    {
        string _username = "";
        string _password = "";
        string _pdfFile = "";

        string _batchPDFFile = "";
        private const string _Smainurl = "https://stage-batch.click2mail.com";
        private const string _lmainurl = "https://batch.click2mail.com";

        private string _authinfo = string.Empty;

        private string _addressListName = "";
        public string pdf { get; set; }
        public int batchId { get; set; }
        public liveMode mode { get; set; }
        public List<batchJob> jobList { get; set; }

        public event statusChangedEventHandler statusChanged;
        public delegate void statusChangedEventHandler(string Reason);


        public Batchc2mAPI(string username, string pw, liveMode mode)
        {
            this.mode = mode;
            _username = username;
            _password = pw;
            _authinfo = _username + ":" + _password;
        }
        public int createBatchSimple()
        {
            string results = createbatch();
            batchId = Int32.Parse(parseReturnxml(results, "id"));

            return batchId;
        }
        //TOOLS

        private string createbatch()
        {

            HttpWebResponse response = null;
            StreamReader reader = default(StreamReader);
            Uri address = default(Uri);
            StringBuilder data = null;
            byte[] byteData = null;
            Stream postStream = null;

            address = new Uri(getBatchURL() + "/v1/batches");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(address);
            string authinfo = null;
            authinfo = Convert.ToBase64String(Encoding.Default.GetBytes(_authinfo));

            // Create the web request  
            request = (HttpWebRequest)WebRequest.Create(address);
            request.Headers["Authorization"] = "Basic " + authinfo;
            request.Method = "POST";
            request.ContentType = "text/plain";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                // This exception will be raised if the server didn't return 200 - OK  
                // Try to retrieve more information about the network error  
                if ((wex.Response != null))
                {
                    HttpWebResponse errorResponse = null;
                    try
                    {
                        errorResponse = (HttpWebResponse)wex.Response;
                        Console.WriteLine("The server returned '{0}' with the status code {1} ({2:d}).", errorResponse.StatusDescription, errorResponse.StatusCode, errorResponse.StatusCode);
                    }
                    finally
                    {
                        if ((errorResponse != null))
                            errorResponse.Close();
                    }
                }
            }
            finally
            {
                if ((postStream != null))
                    postStream.Close();
            }
            //

            try
            {
                reader = new StreamReader(response.GetResponseStream());

                // Console application output  
                string s = reader.ReadToEnd();
                reader.Close();
                // Console.Write(s)
                return s;
                //    c2m.StatusPick.jobStatus = parsexml(s, "status")
                //MsgBox(s)

            }
            finally
            {
                // If c2m.jobid = 0 Then
                //            c2m.StatusPick.jobStatus = 99
                //End If
                //If Not response Is Nothing Then response.Close()
            }
        }
        public string createXMLBatchPost()
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
            //create nodes
            System.Xml.XmlElement root = doc.CreateElement("batch");

            //"    <username>" & username & "</username>" &
            //"    <password>" & password & "</password>" &
            //"    <filename>" & fileName & "</filename>" &
            //"    <appSignature>MyTest App</appSignature>" &
            System.Xml.XmlElement attr = doc.CreateElement("username");
            attr.InnerXml = this._username;
            root.AppendChild(attr);

            attr = doc.CreateElement("password");
            attr.InnerXml = this._password;
            root.AppendChild(attr);

            attr = doc.CreateElement("filename");
            attr.InnerXml = this.pdf;
            root.AppendChild(attr);


            attr = doc.CreateElement("appSignature");
            attr.InnerXml = ".NET SDK API";
            root.AppendChild(attr);
            foreach (batchJob b in jobList)
            {
                dynamic job = doc.CreateElement("job");

                attr = doc.CreateElement("startingPage");
                attr.InnerXml = b.startingPage.ToString();
                job.AppendChild(attr);

                attr = doc.CreateElement("endingPage");
                attr.InnerXml = b.endingPage.ToString();
                job.AppendChild(attr);

                dynamic printProductIOptions = doc.CreateElement("printProductionOptions");
                job.AppendChild(printProductIOptions);
                //<documentClass>Letter 8.5 x 11</documentClass>" &
                //"            <layout>Address on First Page</layout>" &
                //"            <productionTime>Next Day</productionTime>" &
                //"            <envelope>#10 Double Window</envelope>" &
                //"            <color>Full Color</color>" &
                //"            <paperType>White 24#</paperType>" &
                //"            <printOption>Printing One side</printOption>" &
                //"            <mailClass>First Class</mailClass>" &
                attr = doc.CreateElement("documentClass");
                attr.InnerXml = b.documentClass;
                printProductIOptions.AppendChild(attr);

                attr = doc.CreateElement("layout");
                attr.InnerXml = b.layout;
                printProductIOptions.AppendChild(attr);

                attr = doc.CreateElement("productionTime");
                attr.InnerXml = b.productionTime;
                printProductIOptions.AppendChild(attr);
                attr = doc.CreateElement("envelope");
                attr.InnerXml = b.envelope;
                printProductIOptions.AppendChild(attr);
                attr = doc.CreateElement("color");
                attr.InnerXml = b.color;
                printProductIOptions.AppendChild(attr);

                attr = doc.CreateElement("paperType");
                attr.InnerXml = b.paperType;
                printProductIOptions.AppendChild(attr);
                attr = doc.CreateElement("printOption");
                attr.InnerXml = b.printOption;
                printProductIOptions.AppendChild(attr);
                attr = doc.CreateElement("mailClass");
                attr.InnerXml = b.mailClass;
                printProductIOptions.AppendChild(attr);

                XmlElement addressList = doc.CreateElement("recipients");
                job.AppendChild(addressList);
                foreach (addressItem ai in b.addressList)
                {

                    XmlElement address = doc.CreateElement("address");
                    addressList.AppendChild(address);
                    attr = doc.CreateElement("name");
                    if ((ai._First_name.Length > 0 & ai._Last_name.Length > 0))
                    {
                        attr.InnerText = ai._Last_name + ", " + ai._First_name;
                    }
                    else
                    {
                        attr.InnerText = (ai._First_name + " " + ai._Last_name).Trim();
                    }
                    address.AppendChild(attr);

                    attr = doc.CreateElement("organization");
                    attr.InnerText = ai._Organization.Trim();
                    address.AppendChild(attr);

                    attr = doc.CreateElement("address1");
                    attr.InnerText = ai._Address1.Trim();
                    address.AppendChild(attr);

                    attr = doc.CreateElement("address2");
                    attr.InnerText = ai._Address2.Trim();
                    address.AppendChild(attr);

                    attr = doc.CreateElement("address3");
                    attr.InnerText = "";
                    address.AppendChild(attr);

                    attr = doc.CreateElement("city");
                    attr.InnerText = ai._City.Trim();
                    address.AppendChild(attr);

                    attr = doc.CreateElement("state");
                    attr.InnerText = ai._State.Trim();
                    address.AppendChild(attr);

                    attr = doc.CreateElement("postalCode");
                    attr.InnerText = ai._Zip.Trim(); ;
                    address.AppendChild(attr);

                    attr = doc.CreateElement("country");
                    attr.InnerText = (ai._Country_nonUS).Trim(); ;
                    address.AppendChild(attr);


                }
                root.AppendChild(job);

            }

            doc.AppendChild(root);

            //doc.Declaration = New XDeclaration("1.0", "utf-8", Nothing)
            string xmlString = null;
            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();

                    xmlString = stringWriter.GetStringBuilder().ToString();
                }
            }




            return xmlString;

        }
        public class batchJob
        {
            public int startingPage { get; set; }
            public int endingPage { get; set; }
            public string documentClass { get; set; }
            public string layout { get; set; }
            public string productionTime { get; set; }
            public string envelope { get; set; }
            public string color { get; set; }
            public string paperType { get; set; }
            public string printOption { get; set; }
            public string mailClass { get; set; }
            public List<addressItem> addressList { get; set; }

            public batchJob(int startingPage, int endingPage, string documentClass, string layout, string productionTime, string envelope, string color, string paperType, string printOption, string mailClass,
            List<addressItem> addressList)
            {
                this.startingPage = startingPage;
                this.endingPage = endingPage;
                this.documentClass = documentClass;
                this.layout = layout;
                this.productionTime = productionTime;
                this.envelope = envelope;
                this.color = color;
                this.paperType = paperType;
                this.printOption = printOption;
                this.mailClass = mailClass;
                this.addressList = addressList;
            }
        }
        public class addressItem
        {
            public string _First_name = "";
            public string _Last_name = "";
            public string _Organization = "";
            public string _Address1 = "";

            public string _Address2 = "";
            public string _City = "";
            public string _State = "";
            public string _Zip = "";
            public string _Country_nonUS = "";
            public addressItem(string fname, string lname, string org, string address1, string address2, string city, string state, string zip, string country)
            {
                _First_name = fname;
                _Last_name = lname;
                _Organization = org;
                _Address1 = address1;
                _Address2 = address2;
                _City = city;
                _State = state;
                _Zip = zip;
                _Country_nonUS = country;
            }
        }
        private void uploadBatchxml()
        {
            XmlDocument _XMLDOC = new XmlDocument();
            //Console.Write(createXMLBatchPost())
            //Return
            _XMLDOC.LoadXml(createXMLBatchPost());
            string strURI = string.Empty;
            strURI = getBatchURL() + "/v1/batches/" + batchId;
            PutObject(strURI, _XMLDOC);
            return;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURI);
            string authinfo = null;
            authinfo = Convert.ToBase64String(Encoding.Default.GetBytes(_authinfo));
            request.Headers["Authorization"] = "Basic " + authinfo;
            request.Accept = "text/xml";
            request.Method = "PUT";
            using (MemoryStream ms = new MemoryStream())
            {
                _XMLDOC.Save(ms);
                request.ContentLength = ms.Length;
                ms.WriteTo(request.GetRequestStream());
            }
            string result = null;

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }

            return;


            //Console.WriteLine(result)
        }

        public String PutObject(string postUrl, XmlDocument xmlDoc)
        {
            NetworkCredential myCreds = new NetworkCredential(_username, _password);

            MemoryStream xmlStream = new MemoryStream();
            xmlDoc.Save(xmlStream);

            string result = "";
            xmlStream.Flush();
            //Adjust this if you want read your data 
            xmlStream.Position = 0;

            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                client.Credentials = myCreds;
                client.Headers.Add("Content-Type", "application/xml");
                byte[] b = client.UploadData(postUrl, "PUT", xmlStream.ToArray());
                //Dim b As Byte() = client.UploadFile(postUrl, "PUT", "C:\test\test.xml")

                result = client.Encoding.GetString(b);
            }

            return result;
        }
        private string parseReturnxml(string strxml, string lookfor)
        {

            string s = "0";

            // Create an XmlReader
            using (XmlReader reader = XmlReader.Create(new StringReader(strxml)))
            {

                //            reader.ReadToFollowing(lookfor)
                //reader.MoveToFirstAttribute()
                //Dim genre As String = reader.Value
                //output.AppendLine("The genre value: " + genre)

                reader.ReadToFollowing(lookfor);
                s = reader.ReadElementContentAsString();
                reader.Close();
            }
            return s;
        }

        public void uploadBatchPDF()
        {
            WebClient client = new WebClient();

            string strURI = string.Empty;
            strURI = getBatchURL() + "/v1/batches/" + batchId;
            string authinfo = null;
            authinfo = Convert.ToBase64String(Encoding.Default.GetBytes(_authinfo));
            client.Headers["Authorization"] = "Basic " + authinfo;
            client.Headers.Add("Content-Type", "application/pdf");
            //Dim sentXml As Byte() = System.Text.Encoding.ASCII.GetBytes(_XMLDOC.OuterXml)

            FileInfo fInfo = new FileInfo(pdf);

            long numBytes = fInfo.Length;

            FileStream fStream = new FileStream(pdf, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fStream);

            byte[] data = br.ReadBytes(Convert.ToInt32(numBytes));

            // Show the number of bytes in the array.


            br.Close();

            fStream.Close();




            byte[] response = client.UploadData(strURI, "PUT", data);

            Console.WriteLine(System.Text.Encoding.Default.GetString(response));


            //Console.WriteLine(response.ToString())
        }
        public string getbatchstatus()
        {
            string strURI = string.Empty;
            strURI = getBatchURL() + "/v1/batches/" + batchId;
            Console.WriteLine(strURI);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURI);
            string authinfo = null;
            authinfo = Convert.ToBase64String(Encoding.Default.GetBytes(_authinfo));
            request.Headers["Authorization"] = "Basic " + authinfo;
            request.Method = System.Net.WebRequestMethods.Http.Get;
            string result = null;
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();

                    }
                }
                return result;
                //Console.Write(result)

                return parseReturnxml(result, "hasErrors");
            }
            catch (Exception ex)
            {
                return ex.Message;
                //Console.Write(ex.Message)
            }
            return "";
        }
        private void submitbatch()
        {

            HttpWebResponse response = null;
            StreamReader reader = default(StreamReader);
            Uri address = default(Uri);

            Stream postStream = null;

            address = new Uri(getBatchURL() + "/v1/batches/" + batchId);
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(address);
            string authinfo = null;
            authinfo = Convert.ToBase64String(Encoding.Default.GetBytes(_authinfo));

            // Create the web request  
            request = (HttpWebRequest)WebRequest.Create(address);
            request.Headers["Authorization"] = "Basic " + authinfo;
            request.Method = "POST";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                // This exception will be raised if the server didn't return 200 - OK  
                // Try to retrieve more information about the network error  
                if ((wex.Response != null))
                {
                    HttpWebResponse errorResponse = null;
                    try
                    {
                        errorResponse = (HttpWebResponse)wex.Response;
                        Console.WriteLine("The server returned '{0}' with the status code {1} ({2:d}).", errorResponse.StatusDescription, errorResponse.StatusCode, errorResponse.StatusCode);
                    }
                    finally
                    {
                        if ((errorResponse != null))
                            errorResponse.Close();
                    }
                }
            }
            finally
            {
                if ((postStream != null))
                    postStream.Close();
            }
            //

            try
            {
                reader = new StreamReader(response.GetResponseStream());
            }
            finally
            {
            }

        }
        public string runComplete(string PDF, List<batchJob> jobList)
        {
            batchId = createBatchSimple();
            this.pdf = PDF;
            this.jobList = jobList;
            if (statusChanged != null)
            {
                statusChanged("BatchID Created:" + batchId);
            }
            //RaiseEvent statusChanged(createXMLBatchPost())
            uploadBatchxml();
            if (statusChanged != null)
            {
                statusChanged("XML UPLOAD Completed");
            }
            uploadBatchPDF();
            if (statusChanged != null)
            {
                statusChanged("PDF UPLOAD Completed");
            }
            submitbatch();
            if (statusChanged != null)
            {
                statusChanged("Batch UPLOAD Completed");
            }

            if (statusChanged != null)
            {
                statusChanged(getbatchstatus());
            }
            return "";
        }

        private string getBatchURL()
        {
            if (mode == liveMode.Live)
            {
                return _lmainurl;
            }
            else
            {
                return _Smainurl;
            }
        }

        public enum liveMode
        {
            Live = 1,
            Stage = 0
        }
        public string RemoveTroublesomeCharacters(string inString)
        {
            if (inString == null)
            {
                return null;
            }

            StringBuilder newString = new StringBuilder();
            char ch = '\0';


            for (int i = 0; i <= inString.Length - 1; i++)
            {
                ch = inString[i];
                // remove any characters outside the valid UTF-8 range as well as all control characters
                // except tabs and new lines
                //if ((ch < 0x00FD && ch > 0x001F) || ch == '\t' || ch == '\n' || ch == '\r')
                //if using .NET version prior to 4, use above logic
                if (XmlConvert.IsXmlChar(ch))
                {
                    //this method is new in .NET 4
                    newString.Append(ch);
                }
            }
            return newString.ToString();

        }

    }
}
