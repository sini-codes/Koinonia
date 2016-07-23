using System.Collections;
using System.Net;
using System.Threading;

namespace Koinonia
{
    public class WebRequestManager : IWebRequestManager
    {
        public WebRequestManager(string authToken)
        {
            AuthToken = authToken;
        }

        public string AuthToken { get; set; }

        private WebClient GetWebClient(string accessToken)
        {
            var wc = new CommonWebClient();

            if (accessToken != null)
            {
                wc.Headers.Add("Authorization", "token " + accessToken);
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            }

            return wc;
        }

        public byte[] GetBytes(string url)
        {
            using (var wc = GetWebClient(AuthToken))
            {
                return wc.DownloadData(url);
            }
        }

        public string GetText(string url)
        {
            using (var wc = GetWebClient(AuthToken))
            {
                return wc.DownloadString(url);
            }
        }
    }

}

