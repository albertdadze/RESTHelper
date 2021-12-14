using System;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using REST;
using System.Security.Cryptography.X509Certificates;

namespace REST
{
    public enum HttpVerb {
        GET,
        POST,
        PUT,
        DELETE
    }

    public class RestClient {
        public string EndPoint { get; set; }

        public HttpVerb Method { get; set; }

        public string ContentType { get; set; }

        public string PostData { get; set; }

        public string AccessToken { get; set; }

        public bool UsesCertificate { get; set; }

        public string CertificateName { get; set; }

        public string CertificateKey { get; set; }

        public RestClient()
        {
            EndPoint = "";
            Method = HttpVerb.GET;
            ContentType = "application/json";
            PostData = "";
            AccessToken = "";
        }
        
        public RestClient(string endpoint)
        {
            EndPoint = endpoint;
            Method = HttpVerb.GET;
            ContentType = "application/json";
            PostData = "";
            AccessToken = "";
        }
       
        public RestClient(string endpoint, HttpVerb method)
        {
            EndPoint = endpoint;
            Method = method;
            AccessToken = "";
            ContentType = "application/json";
            PostData = "";
        }

        public RestClient(string endpoint, HttpVerb method, string accessToken)
        {
            EndPoint = endpoint;
            Method = method;
            AccessToken = accessToken;
            ContentType = "application/json";
            PostData = "";
        }


        public RestClient(string endpoint, HttpVerb method, string accessToken, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = "application/json";
            PostData = postData;
            AccessToken = accessToken;
        }

        public void ApplyCertificate(string name, string key)
        {
            CertificateName = name;
            CertificateKey = key;
            UsesCertificate = true;
        }

        public string MakeRequest(){
            return MakeRequest("");
        }

        public string MakeRequest(string parameters)
        {
            //string certName = @"C:\apps\GPSNP\gpsnp.pfx";
            //string password = @"Gpsnp_001";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

                if (UsesCertificate) { 
                    X509Certificate2Collection certificates = new X509Certificate2Collection();
                    certificates.Import(CertificateName, CertificateKey, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                    ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
                    request.ClientCertificates = certificates;
                    request.AllowAutoRedirect = true;
                }               
                
                request.Method = Method.ToString();
                request.ContentLength = 0;
                request.ContentType = ContentType;

                if (!AccessToken.Equals(""))
                    request.Headers.Add("Authorization", "Bearer " + AccessToken);

                if (!string.IsNullOrEmpty(PostData) && Method == HttpVerb.POST){
                    var encoding = new UTF8Encoding();
                    var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                    request.ContentLength = bytes.Length;

                    using (var writeStream = request.GetRequestStream()){
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse()){
                    var responseValue = string.Empty;

                    if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted){
                        var message = String.Format("Request failed. Received HTTP {0}:{1}", response.StatusCode, response.StatusDescription);
                        throw new ApplicationException(message);
                    }

                    // grab the response
                    using (var responseStream = response.GetResponseStream()){
                        if (responseStream != null)
                            using (var reader = new StreamReader(responseStream)){
                                responseValue = reader.ReadToEnd();
                            }
                    }

                    return responseValue;
                }
            } catch (Exception ex){
                RestLogger.WriteToLog("Error at endpoint: "+ EndPoint);
                RestLogger.WriteToLog(ex.ToString());
                RestLogger.WriteToLog("POSTDATA: " + PostData);
                return string.Empty;
            }
        }

    } // class

}
