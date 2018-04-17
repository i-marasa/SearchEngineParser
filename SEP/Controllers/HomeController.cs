using Newtonsoft.Json;
using SearchEngineParser;
using SearchEngineParser.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEP.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            //var c = SearchEngine.GetCurrentEngines();
            //SearchEngine.AddNewSearchEngine();
            return View();
        }

        public ActionResult engines()
        {
            try
            {
                var engs = SearchEngine.GetSearchEngineNames;
                

                return Json(new { res = engs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        //public ActionResult search(string word, List<enginSearch> engins)
        //{
        //    try
        //    {
        //        List<enginSearch> results = new List<enginSearch>();
        //        enginSearch res = new enginSearch();
        //        foreach (var item in engins)
        //        {
        //            res = new enginSearch();
        //            res.enginName = item.enginName;
        //            res.searchResult = GetResults(item.enginName, word);
        //            results.Add(res);
        //        }
        //        var google = GetResults("Google", word);
        //        return Json(new { result = results }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public ActionResult search(string word, string engin)
        {
            try
            {
                string csscolor = getColor(engin);
                SearchEngine.SaveCurrentEngines(SearchEngine.GetCurrentEngines());
                var result = GetResults(engin, word);
                var res = result.Select(s => new
                {
                    s.CharacterSet,
                    s.ContentEncoding,
                    s.ContentLength,
                    s.ContentSize,
                    DateOfSearch = s.DateOfSearch.HasValue ? s.DateOfSearch.Value.ToString("MMM dd, yyyy") : "",
                    s.Href,
                    s.HrefOrginal,
                    s.IsFromCache,
                    s.IsWorking,
                    LastModified = s.LastModified.ToString("MMM dd, yyyy"),
                    s.linkIcon,
                    s.LinkType,
                    s.LinkTypeOrginal,
                    s.Method,
                    s.ProtocolVersion,
                    s.RequestMethod,
                    s.ResponseUrl,
                    s.SearchEngineUsed,
                    s.Server,
                    s.StatusDescription,
                    s.Term,
                    s.Text,
                    s.Timeout,
                });
                return Json(new { res = res, csscolor = csscolor }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void exportCSV(string jsonData)
        {
            try
            {
                //List<WebLink> searchresult;
                JsonSerializer serializer = new JsonSerializer();

                System.IO.StringReader sr = new StringReader(jsonData);
                Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(sr);
                List<WebLink> searchresult = (List<WebLink>)serializer.Deserialize(reader, typeof(List<WebLink>));

                ExportExcel exp = new ExportExcel(searchresult);
                exp.ExportToWepResponse(System.Web.HttpContext.Current.Response);
                //result.FirstOrDefault().
            }
            catch (Exception ex)
            {
            }
        }

        [HttpPost]
        public void export(List<WebLink> searchresult)
        {
            try
            {
                //List<WebLink> searchresult;
               
                ExportExcel exp = new ExportExcel(searchresult);
                exp.ExportToWepResponse(System.Web.HttpContext.Current.Response);
                //result.FirstOrDefault().
            }
            catch (Exception ex)
            {
            }
        }

        public void export()
        {
            try
            {
                var result = GetResults("Google", "IDS");
                ExportExcel exp = new ExportExcel(result);
                exp.ExportToWepResponse(System.Web.HttpContext.Current.Response);
                //result.FirstOrDefault().
            }
            catch (Exception ex)
            {
            }
        }
        private string getIcon(string linktype)
        {
            if (string.IsNullOrWhiteSpace(linktype))
                return "fa fa-exclamation";
            if ( linktype.Contains("YouTube Video"))
                return "fa fa-youtube-play";

            if (linktype.Contains("YouTube Link"))
                return "fa fa-youtube-play";

            if (linktype.Contains("Video"))
                return "fa fa-video-camera";

            if (linktype.Contains("Linkedin Profile"))
                return "fa fa-linkedin-square";

            if (linktype.Contains("Flickr Images"))
                return "fa fa-flickr";

            if (linktype.Contains("Facebook Page"))
                return "fa fa-facebook-square";

            if (linktype.Contains("PDF Document"))
                return "fa fa-file-pdf-o";

            if (linktype.Contains("Document"))
                return "fa fa-file-word-o";

            if (linktype.Contains("Backup File"))
                return "fa fa-database";

            if (linktype.Contains("Compressed File"))
                return "fa fa-file-archive-o";

            if (linktype.Contains("Image"))
                return "fa-file-image-o";

            if (linktype.Contains("Web Page"))
                return "fa fa-globe";

            else
            {
                return "fa fa-exclamation";
            }

        }
        private string getColor(string engin)
        {
            switch (engin.ToLower())
            {
                case "google":
                    return "primary";
                case "yahoo":
                    return "warning";
                case "bing":
                    return "danger";
                case "Baidu":
                    return "success";
                case "Yandex":
                    return "info";
                default:
                    return "info";
            }
        }

        private List<WebLink> GetResults(string EngineName, string term)
        {
            SearchEngine eng = new SearchEngine(EngineName, term, 10);
            eng.Timeout = 2000000;
            eng.RequestMethod = "Head"; // Set Head or Get
            var links = eng.GetResult();
            return links;

            // Text = google.timer.Elapsed.ToString();
        }
    }

    public class enginSearch
    {
        public string enginName {get; set;}
        public object searchResult { get; set; }
    }

}
