using SearchEngineParser.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEP.Controllers
{
    public class SettingController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult engines()
        {
            try
            {
                var engs = SearchEngine.GetCurrentEngines();
                engs.Select(s => new
                {
                    s.EngineUrl,
                    s.EngineUrlHttp,
                    s.EngineUrlHttps,
                    s.HowManyResults,
                    s.MaxThreads,
                    s.Name,
                    s.RequestMethod,
                    s.ReturnFullUrl,
                    s.ReturnUrlAfter,
                    s.ReturnUrlBefor,
                    s.searchDepth,
                    s.SearchInHostOrSite,
                    s.SearchUrl,
                    s.term,
                    s.Timeout,
                    s.timer
                });

                return Json(new { res = engs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult saveEngin(string EngineUrl,
                    int HowManyResults,
                    string EngineName,
                    string EngineNameUpdt,
                    bool FullUrl,
                    string ReturnUrlAfter,
                    string ReturnUrlBefor,
                    string SearchUrl,
                    bool isUpdate)
        {
            try
            {
                //EngineName,EngineUrl,EngineUrlHttp,EngineUrlHttps,HowManyResults,SearchUrl,
                //ReturnFullUrl, ReturnUrlAfter, ReturnUrlBefor
               
                    SearchEngine se = new SearchEngine();
                    se.EngineUrl = EngineUrl;
                    se.HowManyResults = HowManyResults;
                    se.SearchUrl = EngineUrl + SearchUrl;
                    se.ReturnFullUrl = FullUrl;
                    se.ReturnUrlAfter = ReturnUrlAfter;
                    se.ReturnUrlBefor = ReturnUrlBefor;

                    if (!isUpdate)
                    {
                        se.Name = EngineName;
                        SearchEngine.AddNewSearchEngine(se);
                    }
                    else
                    {
                        se.Name = EngineNameUpdt;
                        SearchEngine.EditSearchEngine(se);
                    }
                    //EditSearchEngine
               
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception er)
            {
                Response.StatusCode = 500;
                return Json(er.Innerexception().Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteEngine(string del_id)
        {
            try
            {
                var item = SearchEngine.GetCurrentEngines().Find(s => s.Name.ToLower().Trim() == del_id.ToLower().Trim());
                if (item == null)
                    throw new Exception("This Search Engine is no longer exist.");
                SearchEngine.RemoveSearchEngine(item);
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception er)
            {
                Response.StatusCode = 500;
                return Json(er.Innerexception().Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
