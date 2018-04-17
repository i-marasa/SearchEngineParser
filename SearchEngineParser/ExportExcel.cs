using ElencySolutions.CsvHelper;
using SearchEngineParser.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngineParser
{
  public   class ExportExcel
    {
        CsvFile x;
        List<WebLink> _links;
        public ExportExcel(List<WebLink> links)
        {
            _links = links;
            
        }

        CsvFile process()
        {
            CsvFile csv = new CsvFile();
            csv.Headers.Add("Term");
            csv.Headers.Add("Search Engine");
            csv.Headers.Add("Title");
            csv.Headers.Add("Link");
            csv.Headers.Add("Active");
            csv.Headers.Add("Date Of Search");

            csv.Headers.Add("Orginal Link");
            csv.Headers.Add("From Cache");
            csv.Headers.Add("Character Set");
            csv.Headers.Add("Content Size");
            csv.Headers.Add("Link Modified");
            csv.Headers.Add("Server");
            csv.Headers.Add("Link Type Orginal");
            

            foreach (var l in _links)
            {
                CsvRecord r = new CsvRecord();
                r.Fields.Add(l.Term);
                r.Fields.Add(l.SearchEngineUsed);
                r.Fields.Add(l.Text);
                r.Fields.Add(l.ResponseUrl);
                r.Fields.Add(l.IsWorking ? "Yes" : "No (" + l.StatusDescription + ")");
                r.Fields.Add(l.DateOfSearch.HasValue ? l.DateOfSearch.Value.ToShortDateString() : "");

                r.Fields.Add(l.HrefOrginal);
                r.Fields.Add(l.IsFromCache ? "Yes" : "No");
                r.Fields.Add(l.CharacterSet);
                r.Fields.Add(l.ContentSize);
               

                r.Fields.Add(l.LastModified.ToShortDateString());
                r.Fields.Add(l.Server);
                r.Fields.Add(l.LinkTypeOrginal);

                csv.Records.Add(r);
            }
            return csv;
        }
        public void ExportToWepResponse(System.Web.HttpResponse Response)
        {

          

               // ExportToFile( "d:\\SearchEngineResults.csv");
            new CsvWriter().DownloadToClient(Response, process(), "SearchEngineResults.csv");
           

        }
        public void ExportToFile(string FilePath)
        {
            new CsvWriter().WriteCsv(process(), FilePath);

        }

    }
}
