using System;
using System.Net;

namespace Koinonia
{
    public class CommonWebClient : WebClient
    {
        public CommonWebClient(int timeout)
        {
            Timeout = timeout;
        }

        public CommonWebClient()
        {
            Timeout = 1000 * 30;
        }

        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            webRequest.Timeout = Timeout;
            return webRequest;
        }
    }
}