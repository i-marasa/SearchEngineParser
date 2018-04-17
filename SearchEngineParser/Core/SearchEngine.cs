using CsQuery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SearchEngineParser.Core
{
    /// <summary>
    /// Represent SearchEngine Properties
    /// </summary>
    [Serializable]
    public class SearchEngine
    {
        public SearchEngine()
        {

        }
        /// <summary>
        /// Search Engine Name such as: google , yahoo , bing
        /// </summary>
        /// <param name="Name"></param>
        public SearchEngine(string EngineName, string Term, int howmanyresults, int Maxthreads = 10)
        {
            if (string.IsNullOrWhiteSpace(EngineName))
                throw new ArgumentNullException("EngineName");
            HowManyResults = howmanyresults;
            searchDepth = HowManyResults * 2;
            MaxThreads = Maxthreads;
            term = Term;

            Name = EngineName;
            GetCurrentEngines();
            SearchUrl = GetSearchUrl(EngineName, Term);

        }
        public static string DecodeURL(string url)
        {
            url = url + "";
            return HttpUtility.UrlDecode(url);
        }

        /// <summary>
        /// If this filled , then the search result will restricted for that host or site only
        /// </summary>
        public string SearchInHostOrSite { get; set; }
        public int HowManyResults { get; set; }
        public int searchDepth { get; set; }

        public string Name { get; set; }
        public string SearchUrl { get; set; }
        public string EngineUrl { get; set; }
        public string RequestMethod { get; set; }

        public string EngineUrlHttp
        {
            get
            {
                if (!EngineUrl.ToLower().Trim().StartsWith("https") && !EngineUrl.ToLower().Trim().StartsWith("http"))
                    return "http://" + EngineUrl.Trim();
                return EngineUrl.ReplaceFirst("https", "http", true);
            }
        }
        public string EngineUrlHttps
        {
            get
            {
                if (!EngineUrl.ToLower().Trim().StartsWith("https"))
                    return EngineUrl.ReplaceFirst("http", "https", true);
                return EngineUrl;
            }
        }

        public bool ReturnFullUrl { get; set; }
        public string ReturnUrlAfter { get; set; }
        public string ReturnUrlBefor { get; set; }
        public int MaxThreads { get; set; }
        public string term { get; set; }

        public int Timeout { get; set; }


        public Task<List<WebLink>> GetResultAsync()
        {
            // TaskThreadManager t = new TaskThreadManager(1);
            // t.DoWork(() => GetResult(term));
            return Task.Factory.StartNew(() => GetResult());
        }
        public Stopwatch timer { get; set; }
        TaskThreadManager t = null;
        public List<WebLink> GetResult()
        {
            t = new TaskThreadManager(MaxThreads);
            bool _crawlComplete = false;
            var url = this.SearchUrl;
            if (!string.IsNullOrWhiteSpace(SearchInHostOrSite)) // Restricted Search mode in spaciefed host or site
            {
                if (SearchInHostOrSite.ToLower().Contains("site:"))
                    url = url.Replace("{site:}", SearchInHostOrSite);
                else
                    url = url.Replace("{site:}", "Site:" + SearchInHostOrSite);
            }
            else
                url = url.Replace("{site:}", "");

            // t._logger.DebugFormat("Start Download search Page...");

            var Doc = CQ.CreateFromUrl(url, new CsQuery.Web.ServerConfig { TimeoutSeconds = Timeout / 1000, UserAgent = "Mozilla/5.0" });

            // t._logger.DebugFormat("search Page downloaded.");

            List<WebLink> links = new List<WebLink>();

            timer = Stopwatch.StartNew();

            foreach (var link in Doc["a[href!='#']"])
            {
                //if(!link.OuterHTML.Contains("USCIS") ) continue;

                var linkText = "";// link["href"] + "";
                var innerText = link.Cq().Text();
                if (term.ToLower().Contains("site:") && Name.ToLower() == "google" && innerText.ToLower() != "here")
                {
                    if (link.ParentNode != null && link.ParentNode.ParentNode != null)
                        linkText = link.ParentNode.ParentNode.InnerText;
                    if (!linkText.ContainsAny(term)) continue;
                }
                else
                if (!innerText.ContainsAny(term)) continue;
                var href = link["href"] + "";
                if (!href.ToLower().StartsWith("http"))
                    href = ReturnFullUrl ? href : EngineUrl + href;
                t.DoWork(() =>
                {
                    var weblink = new WebLink { Term = this.term, SearchEngineUsed = Name, RequestMethod = RequestMethod, Timeout = Timeout, Href = href, Text = innerText, HrefOrginal = link["href"] };
                    weblink.Href = GetValidFinalUrl(weblink.Href);

                    var s = weblink.IsWorking;
                    if (!string.IsNullOrWhiteSpace(weblink.ResponseUrl))
                        weblink.Href = weblink.ResponseUrl;
                    links.Add(weblink);


                });

            }
            while (!_crawlComplete)
            {
                if (!t.HasRunningThreads())
                    _crawlComplete = true;
                else
                {
                    // t._logger.DebugFormat("Waiting for links to be scheduled...");
                    System.Threading.Thread.Sleep(500);
                }
            }
            links = links.OrderByDescending(o => o.IsWorking).Take(HowManyResults).ToList();

            timer.Stop();
            return links;
        }
        public string GetValidFinalUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(ReturnUrlAfter))
                return url;
            if (!url.ToLower().Contains(ReturnUrlAfter.ToLower()))
                return url;

            int startcut = url.ToLower().IndexOf(ReturnUrlAfter);
            if (startcut >= 0)
                startcut = url.ToLower().IndexOf(ReturnUrlAfter) + ReturnUrlAfter.Length;

            int endcut = -1;
            if (!string.IsNullOrEmpty(ReturnUrlBefor))
                endcut = url.ToLower().IndexOf(ReturnUrlBefor);
            if (startcut == -1 && endcut == -1)
                return url;

            var r = url.Substring(startcut);
            if (!string.IsNullOrWhiteSpace(ReturnUrlBefor) && endcut >= 0)
                r = url.Substring(startcut, endcut - startcut);

            return DecodeURL(r.Trim().Trim('"'));
        }

        string GetSearchUrl(string EngineName, string keyword)
        {
            if (string.IsNullOrWhiteSpace(EngineName))
                throw new ArgumentNullException("EngineName");
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentNullException("keyword");

            if (_CurrentEngines == null) return "";
            var ss = _CurrentEngines.Find(s => s.Name.ToLower().Trim() == EngineName.ToLower().Trim());
            ReturnUrlAfter = ss.ReturnUrlAfter;
            ReturnUrlBefor = ss.ReturnUrlBefor;
            ReturnFullUrl = ss.ReturnFullUrl;
            EngineUrl = ss.EngineUrl;
            ReturnUrlAfter = ss.ReturnUrlAfter;

            var url = ss.SearchUrl.Replace("{howmanyresults}", searchDepth + "");
            //url = url.Replace("{term}", HttpUtility.UrlEncode(keyword) + "");
            url = url.Replace("{term}", HttpUtility.UrlEncode(keyword) + " {site:}");

            return url;

        }

        static List<SearchEngine> GetListOfDefaultEngins()
        {
            List<SearchEngine> l = new List<SearchEngine>();
            int howmany = 10;
            var term = "Term";

            SearchEngine Engine = new SearchEngine();
            Engine.Name = "Google"; Engine.HowManyResults = howmany; Engine.searchDepth = howmany * 2; Engine.MaxThreads = howmany; Engine.term = term;

            Engine.EngineUrl = "https://www.google.com";
            Engine.SearchUrl = Engine.EngineUrl + "/search?num={howmanyresults}&q={term}&btnG=Search";
            Engine.ReturnUrlAfter = "url?q=".ToLower().Trim();
            Engine.ReturnUrlBefor = "&sa=".ToLower().Trim();
            Engine.ReturnFullUrl = false;
            l.Add(Engine);

            Engine = new SearchEngine();
            Engine.Name = "Yahoo"; Engine.HowManyResults = howmany; Engine.searchDepth = howmany * 2; Engine.MaxThreads = howmany; Engine.term = term;

            Engine.EngineUrl = "https://search.yahoo.com";
            Engine.SearchUrl = Engine.EngineUrl + "/search?n={howmanyresults}&p={term}";
            Engine.ReturnUrlAfter = "RU=".ToLower().Trim();
            Engine.ReturnUrlBefor = "/RK=".ToLower().Trim();
            Engine.ReturnFullUrl = true;
            l.Add(Engine);

            Engine = new SearchEngine();
            Engine.Name = "Bing"; Engine.HowManyResults = howmany; Engine.searchDepth = howmany * 2; Engine.MaxThreads = howmany; Engine.term = term;

            Engine.EngineUrl = "http://www.bing.com";
            Engine.SearchUrl = Engine.EngineUrl + "/search?q={term}&go=&form=QBRE&count={howmanyresults}&first=1";
            Engine.ReturnFullUrl = false;
            l.Add(Engine);

            Engine = new SearchEngine();
            Engine.Name = "Baidu"; Engine.HowManyResults = howmany; Engine.searchDepth = howmany * 2; Engine.MaxThreads = howmany; Engine.term = term;

            Engine.EngineUrl = "http://www.baidu.com";
            Engine.SearchUrl = Engine.EngineUrl + "/s?ie=utf-8&f=8&rsv_bp=1&tn=baidu&wd={term}&num={howmanyresults}";
            Engine.ReturnFullUrl = false;
            l.Add(Engine);


            Engine = new SearchEngine();
            Engine.Name = "Yandex"; Engine.HowManyResults = howmany; Engine.searchDepth = howmany * 2; Engine.MaxThreads = howmany; Engine.term = term;

            Engine.EngineUrl = "https://www.yandex.com";
            Engine.SearchUrl = Engine.EngineUrl + "/yandsearch?lr=87&text={term}&num={howmanyresults}";
            Engine.ReturnFullUrl = false;
            l.Add(Engine);

            return l;

        }



        static  List<SearchEngine> _CurrentEngines { get; set; }
        /// <summary>
        /// Get list of current search engine as list of names.
        /// </summary>
        /// <returns></returns>
        static public List<string > GetSearchEngineNames
        {
            get
            {
                List<string> l = new List<string>();
                _CurrentEngines = GetCurrentEngines();
                if (_CurrentEngines != null && _CurrentEngines.Count > 0)
                    foreach (var item in _CurrentEngines)
                        l.Add(item.Name);

                return l;
            }
        }
        static public List<SearchEngine> GetCurrentEngines()
        {
            if (_CurrentEngines != null && _CurrentEngines.Count > 0)
                return _CurrentEngines;

            ConfigSettings c = new ConfigSettings();
            _CurrentEngines = c.DeSerializeObject<List<SearchEngine>>();
            if (_CurrentEngines == null || _CurrentEngines.Count == 0)
                _CurrentEngines = GetListOfDefaultEngins();
            return _CurrentEngines;
        }

        /// <summary>
        /// Adds Search Engine and store it in xml file.
        /// </summary>
        /// <exception cref="If search engine already exist then an exception will raise."></exception>
        /// <param name="SearchEngine"></param>
        static public void AddNewSearchEngine(SearchEngine SearchEngine)
        {
            if (SearchEngine == null)
                throw new ArgumentNullException("SearchEngine");
            if (_CurrentEngines == null)
                throw new ArgumentNullException("_CurrentEngines");

            if (_CurrentEngines.Find(s => s.Name.ToLower().Trim() == SearchEngine.Name.ToLower().Trim()) != null)
                throw new Exception("This Search Engine is already exist.");

            _CurrentEngines.Add(SearchEngine);
            SaveCurrentEngines(_CurrentEngines);
        }
        /// <summary>
        /// Remove specified SearchEngine and store it in xml file.
        /// </summary>
        /// <param name="SearchEngine"></param>
        static public void RemoveSearchEngine(SearchEngine SearchEngine)
        {
            if (SearchEngine == null)
                throw new ArgumentNullException("SearchEngine");
            if (_CurrentEngines == null)
                throw new ArgumentNullException("_CurrentEngines");
            if (_CurrentEngines.Count < 1) return;

            var found = _CurrentEngines.Find(s => s.Name.ToLower().Trim() == SearchEngine.Name.ToLower().Trim());
            if (found == null)
                throw new Exception("This Search Engine is no longer exist.");
            _CurrentEngines.Remove(found);            
            SaveCurrentEngines(_CurrentEngines);
        }

        /// <summary>
        /// Use Search Engine Name as Key
        /// </summary>
        /// <param name="SearchEngine"></param>
        static public void EditSearchEngine(SearchEngine SearchEngine)
        {
            if (SearchEngine == null)
                throw new ArgumentNullException("SearchEngine");
            if (_CurrentEngines == null)
                throw new ArgumentNullException("_CurrentEngines");

            var found = _CurrentEngines.Find(s => s.Name.ToLower().Trim() == SearchEngine.Name.ToLower().Trim());
            if(found==null )
                throw new Exception("This Search Engine is no longer exist.");
            _CurrentEngines.Remove(found);
            _CurrentEngines.Add(SearchEngine);
            SaveCurrentEngines(_CurrentEngines);
        }
        static public void SaveCurrentEngines(List<SearchEngine> SearchEngines)
        {
            ConfigSettings c = new ConfigSettings();
            c.SerializeObject<List<SearchEngine>>(SearchEngines);
        }
    }

}
