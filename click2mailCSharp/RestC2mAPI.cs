
//using Microsoft.VisualBasic;

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System;


namespace c2mAPI
{
    public class Restc2mAPI
    {

        string _username = "";
        string _password = "";
        string _pdfFile = "";

        string _batchPDFFile = "";
        private const string _lRestmainurl = "https://rest.click2mail.com";
        private const string _sRestmainurl = "https://stage-rest.click2mail.com";
        private string _authinfo = string.Empty;
        private string _addressListName = "";
        private List<addressItem> _al = new List<addressItem>();
        public int addressListId { get; set; }
        public int documentId { get; set; }
        public int jobId { get; set; }
        public liveMode mode { get; set; }

        public event statusChangedEventHandler statusChanged;
        public delegate void statusChangedEventHandler(string Reason);
        public event jobStatusCheckEventHandler jobStatusCheck;
        public delegate void jobStatusCheckEventHandler(string id, string status, string description);

        public Restc2mAPI(string username, string pw, liveMode mode)
        {
            this.mode = mode;
            _username = username;
            _password = pw;
            _authinfo = _username + ":" + _password;
        }
        //TOOLS
        private string parseReturnxml(string strxml, string lookfor)
        {
            Console.WriteLine(strxml + ", lookfor: " + lookfor);

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
        public string runComplete(string PDF, string addressList, string docClass, string layout, string productionTime, string envelope, string color, string papertype, string printOption)
        {
            Console.WriteLine("Creating document:" + PDF);
            createDocumentSimple(PDF);
            if (statusChanged != null)
            {
                statusChanged("DocumentID:" + documentId);
            }
            Console.WriteLine("Creating addresslist:" + addressList);
            createAddressListSimple(addressList);
            if (statusChanged != null)
            {
                statusChanged("AddressID:" + addressListId);
            }
            waitForCompletedAddressList();
            Console.WriteLine("Creating Job:" + docClass);
            createJobSimple(docClass, layout, productionTime, envelope, color, papertype, printOption);
            if (statusChanged != null)
            {
                statusChanged("JobID:" + jobId);
            }
            Console.WriteLine("Submitting Job:" + jobId);
            submitJobSimple();
            checkJobStatus();

            //RaiseEvent statusChanged(checkJobStatus())
            if (statusChanged != null)
            {
                statusChanged("Completed");
            }
            return "";
        }
        public string checkJobStatus()
        {
            string results = null;
            System.Collections.Specialized.NameValueCollection y = new System.Collections.Specialized.NameValueCollection();
            y.Clear();
            results = createJobPost(getRestURL() + "/molpro/jobs/" + jobId, y, "GET");
            if (jobStatusCheck != null)
            {
                jobStatusCheck(parseReturnxml(results, "id"), parseReturnxml(results, "status"), parseReturnxml(results, "description"));
            }
            return results;
        }
        public string submitJobSimple()
        {
            string results = null;
            System.Collections.Specialized.NameValueCollection y = new System.Collections.Specialized.NameValueCollection();
            y.Add("billingType", "User Credit");
            results = createJobPost(getRestURL() + "/molpro/jobs/" + jobId + "/submit", y, "POST");
            return results;
        }
        public int createJobSimple(string docClass, string layout, string productionTime, string envelope, string color, string papertype, string printOption)
        {
            System.Collections.Specialized.NameValueCollection y = new System.Collections.Specialized.NameValueCollection();
            y.Add("documentClass", docClass);
            y.Add("layout", layout);
            y.Add("productionTime", productionTime);
            y.Add("envelope", envelope);
            y.Add("color", color);
            y.Add("paperType", papertype);
            //'y.Add("printOption", "Printing One side")
            y.Add("printOption", printOption);

            y.Add("documentId", documentId.ToString());
            y.Add("addressId", addressListId.ToString());
            string results = null;
            results = createJobPost(getRestURL() + "/molpro/jobs", y, "POST");
            jobId = Int32.Parse(parseReturnxml(results, "id"));
            return jobId;
        }
        public void waitForCompletedAddressList()
        {
            System.Collections.Specialized.NameValueCollection y = new System.Collections.Specialized.NameValueCollection();

            string status = "0";
            string results = "0";
            results = createJobPost(getRestURL() + "/molpro/addressLists/" + addressListId, y, "GET");
            status = parseReturnxml(results, "status");

            if ((status != "3"))
            {
                while ((status != "3"))
                {
                    if (statusChanged != null)
                    {
                        statusChanged("Waiting Address List to processes.  Current Status is: " + status);
                    }
                    results = createJobPost(getRestURL() + "/molpro/addressLists/" + addressListId, y, "GET");
                    status = parseReturnxml(results, "status");
                    System.Threading.Thread.Sleep(5000);
                }
            }
            if (statusChanged != null)
            {
                statusChanged("The status received is 3, which means we can proceed");
            }
        }
        public int createDocumentSimple(string pdf)
        {
            System.Collections.Specialized.NameValueCollection x = new System.Collections.Specialized.NameValueCollection();
            x.Add("documentName", "sample Letter");
            x.Add("documentClass", "Letter 8.5 x 11");
            x.Add("documentFormat", "PDF");
            string results = createDocument(getRestURL() + "/molpro/documents/", pdf, "file", "application/pdf", x);
            documentId = Int32.Parse(parseReturnxml(results, "id"));
            return documentId;
        }

        public object createAddressListSimple(string Xml)
        {
            string results = createAddressList(getRestURL() + "/molpro/addressLists/", Xml);
            addressListId = Int32.Parse(parseReturnxml(results, "id"));
            return addressListId;
        }

        public List<addressItem> addressList
        {

            get { return _al; }
            set { _al = value; }
        }
        public class addressItem
        {
            public string _First_name = "";
            public string _Last_name = "";
            public string _Organization = "";
            public string _Address1 = "";
            public string _Address2 = "";
            public string _Address3 = "";
            public string _City = "";
            public string _State = "";
            public string _Zip = "";
            public string _Country_nonUS = "";
            public addressItem(string fname, string lname, string org, string address1, string address2, string address3, string city, string state, string zip, string country)
            {
                _First_name = fname;
                _Last_name = lname;
                _Organization = org;
                _Address1 = address1;
                _Address2 = address2;
                _Address3 = address3;
                _City = city;
                _State = state;
                _Zip = zip;
                _Country_nonUS = country;
            }
        }
        private string getRestURL()
        {
            if (this.mode == liveMode.Live)
            {
                return _lRestmainurl;
            }
            else
            {
                return _sRestmainurl;
            }
        }
        public System.IO.Stream GenerateStreamFromString(string s)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;

        }

        private string createAddressList(string uri, string xml)
        {
            string responseText = "";
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);

            request.ContentType = "application/xml";
            request.Method = "POST";
            request.KeepAlive = true;
            //request.Credentials = Net.CredentialCache.DefaultCredentials

            string authinfo = null;
            authinfo = Convert.ToBase64String(Encoding.Default.GetBytes(_authinfo));
            request.Headers["Authorization"] = "Basic " + authinfo;

            using (System.IO.Stream requestStream = request.GetRequestStream())
            {

                using (Stream fileStream = GenerateStreamFromString(xml))
                {

                    byte[] buffer = new byte[4097];
                    Int32 bytesRead = fileStream.Read(buffer, 0, buffer.Length);


                    while ((bytesRead > 0))
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                    }

                }



            }

            System.Net.WebResponse response = null;


            try
            {
                response = request.GetResponse();

                using (System.IO.Stream responseStream = response.GetResponseStream())
                {

                    using (System.IO.StreamReader responseReader = new System.IO.StreamReader(responseStream))
                    {

                        responseText = responseReader.ReadToEnd();


                    }

                }


            }
            catch (System.Net.WebException exception)
            {
                response = exception.Response;


                if ((response != null))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {

                        responseText = reader.ReadToEnd();


                    }

                    response.Close();

                }


            }
            finally
            {
                request = null;

            }
            return responseText;

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
        public string createJobPost(string url, System.Collections.Specialized.NameValueCollection nameValueCollection, string Method)
        {
            // Here we convert the nameValueCollection to POST data.
            // This will only work if nameValueCollection contains some items.
            dynamic parameters = new StringBuilder();
            string responseText = "";
            foreach (string key in nameValueCollection.Keys)
            {
                parameters.AppendFormat("{0}={1}&", WebUtility.UrlEncode(key), WebUtility.UrlEncode(nameValueCollection[key]));
            }
            if ((parameters.Length > 0))
            {
                parameters.Length -= 1;
            }

            // Here we create the request and write the POST data to it.
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = Method;
            request.KeepAlive = true;
            //request.Credentials = Net.CredentialCache.DefaultCredentials

            string authinfo = null;
            authinfo = Convert.ToBase64String(Encoding.Default.GetBytes(_authinfo));
            request.Headers["Authorization"] = "Basic " + authinfo;

            if ((parameters.Length > 0))
            {
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(parameters.ToString());
                }
            }

            System.Net.WebResponse response = null;


            try
            {
                response = request.GetResponse();

                using (System.IO.Stream responseStream = response.GetResponseStream())
                {

                    using (System.IO.StreamReader responseReader = new System.IO.StreamReader(responseStream))
                    {

                        responseText = responseReader.ReadToEnd();


                    }

                }


            }
            catch (System.Net.WebException exception)
            {
                response = exception.Response;


                if ((response != null))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {

                        responseText = reader.ReadToEnd();


                    }

                    response.Close();

                }


            }
            finally
            {
                request = null;

            }
            return responseText;
        }

        public string createXMLFromAddressList()
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            //create nodes
            System.Xml.XmlElement root = doc.CreateElement("addressList");


            System.Xml.XmlElement addressListName = doc.CreateElement("addressListName");
            _addressListName = Guid.NewGuid().ToString("N");
            addressListName.InnerXml = _addressListName;

            root.AppendChild(addressListName);

            System.Xml.XmlElement addressMappingId = doc.CreateElement("addressMappingId");
            addressMappingId.InnerXml = "2";
            root.AppendChild(addressMappingId);

            System.Xml.XmlElement addresses = doc.CreateElement("addresses");
            root.AppendChild(addresses);

            foreach (addressItem a in addressList)
            {
                System.Xml.XmlElement address = doc.CreateElement("address");
                System.Xml.XmlElement fname = doc.CreateElement("First_name");
                fname.InnerXml = a._First_name;
                address.AppendChild(fname);
                System.Xml.XmlElement lname = doc.CreateElement("Last_name");
                lname.InnerXml = a._Last_name;
                address.AppendChild(lname);
                System.Xml.XmlElement Organization = doc.CreateElement("Organization");
                Organization.InnerXml = a._Organization;
                address.AppendChild(Organization);
                System.Xml.XmlElement Address1 = doc.CreateElement("Address1");
                Address1.InnerXml = a._Address1;
                address.AppendChild(Address1);
                System.Xml.XmlElement Address2 = doc.CreateElement("Address2");
                Address2.InnerXml = a._Address2;
                address.AppendChild(Address2);
                System.Xml.XmlElement Address3 = doc.CreateElement("Address3");
                Address3.InnerXml = a._Address3;
                address.AppendChild(Address3);
                System.Xml.XmlElement City = doc.CreateElement("City");
                City.InnerXml = a._City;
                address.AppendChild(City);
                System.Xml.XmlElement State = doc.CreateElement("State");
                State.InnerXml = a._State;
                address.AppendChild(State);
                System.Xml.XmlElement zip = doc.CreateElement("zip");
                zip.InnerXml = a._Zip;
                address.AppendChild(zip);
                System.Xml.XmlElement country = doc.CreateElement("Country_non-US");
                country.InnerXml = a._Country_nonUS;
                address.AppendChild(country);
                addresses.AppendChild(address);
            }

            doc.AppendChild(root);
            string xmlString = null;
            using (StringWriter stringWriter = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();

                    xmlString = stringWriter.GetStringBuilder().ToString();
                }
            }
            return xmlString;

        }

        public string createXMLFromCustomList(List<List<KeyValuePair<string, string>>> myList, int AddressListId)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            //create nodes
            System.Xml.XmlElement root = doc.CreateElement("addressList");


            System.Xml.XmlElement addressListName = doc.CreateElement("addressListName");
            _addressListName = Guid.NewGuid().ToString("N");
            addressListName.InnerXml = _addressListName;

            root.AppendChild(addressListName);

            System.Xml.XmlElement addressMappingId = doc.CreateElement("addressMappingId");
            addressMappingId.InnerXml = AddressListId.ToString();

            root.AppendChild(addressMappingId);

            System.Xml.XmlElement addresses = doc.CreateElement("addresses");
            root.AppendChild(addresses);
            System.Xml.XmlElement address = null;
            foreach (List<KeyValuePair<string, string>> a in myList)
            {

                foreach (KeyValuePair<string, string> aa in a)
                {
                    address = doc.CreateElement("address");
                    System.Xml.XmlElement i = doc.CreateElement(aa.Key);
                    i.InnerXml = aa.Value;
                    address.AppendChild(i);
                }
                addresses.AppendChild(address);
            }

            doc.AppendChild(root);
            string xmlString = null;
            using (StringWriter stringWriter = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                using (XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();

                    xmlString = stringWriter.GetStringBuilder().ToString();
                }
            }
            return xmlString;

        }
        private string createDocument(string uri, string filePath, string fileParameterName, string contentType, System.Collections.Specialized.NameValueCollection otherParameters)
        {
            string responseText = "";
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            string newLine = System.Environment.NewLine;
            byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes(newLine + "--" + boundary + newLine);
            System.Net.HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(uri);

            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            //request.Credentials = Net.CredentialCache.DefaultCredentials

            string authinfo = null;
            authinfo = Convert.ToBase64String(Encoding.Default.GetBytes(_authinfo));
            request.Headers["Authorization"] = "Basic " + authinfo;

            using (System.IO.Stream requestStream = request.GetRequestStream())
            {

                string formDataTemplate = "Content-Disposition: form-data; name=\"{0}\"{1}Content-Type: text/plain; charset=UTF-8{1}{1}{2}";


                foreach (string key in otherParameters.Keys)
                {
                    requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    string formItem = string.Format(formDataTemplate, key, newLine, otherParameters[key]);
                    byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(formItem);
                    requestStream.Write(formItemBytes, 0, formItemBytes.Length);

                }

                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}Content-Type: {3}{2}Content-Transfer-Encoding: binary{2}{2}";
                string header = string.Format(headerTemplate, fileParameterName, filePath, newLine, contentType);
                byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
                requestStream.Write(headerBytes, 0, headerBytes.Length);

                using (System.IO.FileStream fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {

                    byte[] buffer = new byte[4097];
                    Int32 bytesRead = fileStream.Read(buffer, 0, buffer.Length);


                    while ((bytesRead > 0))
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                        bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                    }

                }

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes(newLine + "--" + boundary + "--" + newLine);
                requestStream.Write(trailer, 0, trailer.Length);

            }

            System.Net.WebResponse response = null;


            try
            {
                response = request.GetResponse();

                using (System.IO.Stream responseStream = response.GetResponseStream())
                {

                    using (System.IO.StreamReader responseReader = new System.IO.StreamReader(responseStream))
                    {

                        responseText = responseReader.ReadToEnd();


                    }

                }


            }
            catch (System.Net.WebException exception)
            {
                response = exception.Response;


                if ((response != null))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {

                        responseText = reader.ReadToEnd();


                    }

                    response.Close();

                }


            }
            finally
            {
                request = null;

            }
            return responseText;

        }
    }

}