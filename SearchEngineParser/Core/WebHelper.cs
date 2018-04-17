using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using CsQuery;


namespace SearchEngineParser.Core
{
    [Serializable]
    public class WebHelper
    {

        [Serializable]
        public class RankResult
        {
            public string RankUrl { get; set; }
            public int RankValue { get; set; }

            public RankResult(string RankUrl, int RankValue)
            {
                this.RankUrl = RankUrl;
                this.RankValue = RankValue;
               


            }
        }
      
        /// <summary>
        /// Gets or Sets the max number of items that result from searching process
        /// </summary>
        public int ResultsCount { get; set; }
        public WebHelper()
        {
            ResultsCount = 10;
        }

      
        
        static bool IsContainsUrl(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (Match m in Regex.Matches(text, "\\."))
            {
                if (m.Index > 0 && char.IsLetterOrDigit(text[m.Index - 1]) &&
                m.Index < text.Length - 1 && char.IsLetterOrDigit(text[m.Index + 1])) return true;
            }
            return false;
        }
     
     


        /// <summary>
        /// Returns the content of a given web adress as string.
        /// </summary>
        /// <param name="Url">URL of the webpage</param>
        /// <returns>Website content</returns>
        public  string DownloadWebPage(string Url)
        {
            return DownloadWebPage(Url, null);
        }

        private  string DownloadWebPage(string Url,string stopLine)
        {
            // Open a connection
            HttpWebRequest WebRequestObject = (HttpWebRequest)HttpWebRequest.Create(Url);
            WebRequestObject.Proxy = InitialProxy();
            // You can also specify additional header values like 
            // the user agent or the referer:
            WebRequestObject.UserAgent = ".NET Framework/2.0";

            // Request response:
            WebResponse Response = WebRequestObject.GetResponse();

            // Open data stream:
            Stream WebStream = Response.GetResponseStream();

            // Create reader object:
            StreamReader Reader = new StreamReader(WebStream);
            string PageContent = "", line;
            if (stopLine == null)
                PageContent = Reader.ReadToEnd();
            else while (!Reader.EndOfStream)
                {
                    line = Reader.ReadLine();
                    PageContent += line + Environment.NewLine;
                    if (line.Contains(stopLine)) break;
                }
            // Cleanup
            Reader.Close();
            WebStream.Close();
            Response.Close();

            return PageContent;
        }
      
        /// <summary>
        /// Strip out the non-numeric data from a string, and return a number
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int StripNonNumeric(string str)
        {
            return Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(str, "\\D", ""));
        }
      

        public static string GetGoogleSearchUrl(string keyword, int searchDepth)
        {
            return string.Format("http://www.google.com/search?num=" +  searchDepth+ "&q={0}&btnG=Search", HttpUtility.UrlEncode(keyword));
        }

        public static string GetBingSearchUrl(string keyword, int searchDepth)
        {
            return string.Format("http://www.bing.com/search?q=" + keyword + "&go=&form=QBRE&count=" +searchDepth  + "&first=1", HttpUtility.UrlEncode(keyword));
        }
        public static string GetYahooSearchUrl(string keyword, int searchDepth)
        {
            return string.Format("http://search.yahoo.com/search?n={0}&p={1}", searchDepth, HttpUtility.UrlEncode(keyword));
        }
        public static string GetYouTubeSearchUrl(string keyword, int searchDepth)
        {
            return string.Format("http://www.youtube.com/results?search_query={0}", HttpUtility.UrlEncode(keyword));
        }
     

        /// <summary>
        /// Retrives the position of the url from a search
        /// on www.bing.com using the specified search term.
        /// </summary>
        public List<RankResult> GetBingRank(string keyword, string webUrl)
        {
            string search = GetBingSearchUrl(keyword, ResultsCount);
            string html = DownloadWebPage(search);
            List<RankResult> ranks = new List<RankResult>();
            string input = html.Replace("<b>", "").Replace("</b>", "").Replace("<strong>", "").Replace("</strong>", "");
            string pattern = "<li class=\"sa_wr\"(.*?)a href=\"([^\"]*)\"";
            MatchCollection matchs = Regex.Matches(input, pattern);
            // string host = webUrl.Replace("http://", "").Replace("www.", "");
            for (int k = 0; k < matchs.Count; k++)
            {
                string str5 = matchs[k].Groups[2].Value;
                if (str5.Contains(webUrl)) ranks.Add(new RankResult(webUrl, k + 1));
            }
            return ranks;
        }
        /// <summary>
        /// Retrives the position of the url from a search
        /// on www.yahoo.com using the specified search term.
        /// </summary>
        public List<RankResult> GetYahooRank(string keyword, string webUrl)
        {
            string search = GetYahooSearchUrl(keyword, ResultsCount);
            string html = DownloadWebPage(search);
            List<RankResult> ranks = new List<RankResult>();

            string input = html.Replace("<b>", "").Replace("</b>", "").Replace("\r", "").Replace("\n", "");
            string pattern = "<a class=\"?yschttl (.*?)href(.*?)http://([^\"]*)\"([^>]*)>(.*?)</a>(.*?)<(span)?(em)? class=\"?(.*?)url\"?>(.*?)</(span)?(em)?>";
            MatchCollection matchs = Regex.Matches(input, pattern);
            for (int k = 0; k < matchs.Count; k++)
            {
                string str5 = matchs[k].Groups[3].Value;
                if (str5.Contains(webUrl)) ranks.Add(new RankResult(webUrl, k + 1));
            }

            return ranks;
        }
        /// <summary>
        /// Get the ID of a youtube video from its URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetVideoIDFromUrl(string url)
        {
            url = url.Substring(url.IndexOf("?") + 1);
            string[] props = url.Split('&');

            string videoid = "";
            foreach (string prop in props)
            {
                if (prop.StartsWith("v="))
                    videoid = prop.Substring(prop.IndexOf("v=") + 2);
            }

            return videoid;
        }
        [NonSerialized]
        WebClient client;
        public byte[] DownloadData( string url)
        {
            if (client == null) client = new WebClient();
            client.Proxy = InitialProxy();
            return client.DownloadData(url);
        }
        public bool UseDefaultCredentials { get; set; }
        public string ProxyAddress { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyPassword { get; set; }
        public string ProxyDomain { get; set; }

        public  IWebProxy InitialProxy()
        {
            if (!UseDefaultCredentials && !string.IsNullOrEmpty(ProxyAddress))
            {
                IWebProxy proxy = new WebProxy(ProxyAddress);
                proxy.Credentials = new NetworkCredential(ProxyUserName, ProxyPassword, ProxyDomain);
                return proxy;
            }
            return null;
        }
    }
}