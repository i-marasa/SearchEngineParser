using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchEngineParser.Core
{
  public   class WebLink
  {
      public int  Timeout { get; set; }

      public string Term { get; set; }

        public string Text { get; set; }
        public string Href { get;  set; }
        public string HrefOrginal { get;  set; }
        public string StatusDescription { get; private set; }
        public string SearchEngineUsed { get;  set; }

        public DateTime? DateOfSearch { get;private  set; }

        string _LinkType=null ;
        bool _IsWorking=false ;
        public string LinkTypeOrginal
        {
            get
            {
                if (_LinkType == null)
                    IsAccessibleAsync();
                return _LinkType;

            }
        }

        public string linkIcon
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LinkType))
                    return "fa fa-exclamation";
                if (LinkType.ContainsAny("YouTube Video",false))
                    return "fa fa-youtube-play";

                if (LinkType.ContainsAny("YouTube Link", false))
                    return "fa fa-youtube-play";

                if (LinkType.ContainsAny("Video", false))
                    return "fa fa-video-camera";

                if (LinkType.ContainsAny("Linkedin Profile", false))
                    return "fa fa-linkedin-square";

                if (LinkType.ContainsAny("Flickr Images", false))
                    return "fa fa-flickr";

                if (LinkType.ContainsAny("Facebook Page", false))
                    return "fa fa-facebook-square";

                if (LinkType.ContainsAny("PDF Document", false))
                    return "fa fa-file-pdf-o";

                if (LinkType.ContainsAny("Document", false))
                    return "fa fa-file-word-o";

                if (LinkType.ContainsAny("Backup File", false))
                    return "fa fa-database";

                if (LinkType.ContainsAny("Compressed File", false))
                    return "fa fa-file-archive-o";

                if (LinkType.ContainsAny("Image", false))
                    return "fa-file-image-o";

                if (LinkType.ContainsAny("Web Page", false))
                    return "fa fa-globe";

                else
                {
                    return "fa fa-globe";
                }
            }
        }
        public string LinkType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Href))
                    return "N/A";
                if (Href.ToLower().Contains("youtube.") && Href.ToLower().Contains("watch?"))
                    return "YouTube Video";

                if (Href.ToLower().Contains("youtube.") && !Href.ToLower().Contains("watch?"))
                    return "YouTube Link";

                if (Href.ContainsAny(".webm;.mp4;.avi;.mkv;.flv;.ogg;.ogv;.mov;.wmv;.rmvb;.mpeg;.mpg;.3gp;.rm"))
                    return "Video";

                if (Href.ContainsAny("linkedin.com"))
                    return "Linkedin Profile";

                if (Href.ContainsAny("flickr.com"))
                    return "Flickr Images";

                if (Href.ContainsAny("facebook.com"))
                    return "Facebook Page";

                if (Href.ContainsAny(".pdf;"))
                    return "PDF Document";

                if (Href.ContainsAny(".docx;.doc;.docm;.dotm;.dotx;.xls;.xlsx;.xlt;.pptx;.ppt;.opt"))
                    return "Document";

                if (Href.ContainsAny(".bak;"))
                    return "Backup File";

                if (Href.ContainsAny(".rar;.zip;.tar"))
                    return "Compressed File";

                if (Href.ContainsAny(".jpg;.jpeg;.jfif;.tiff;.png;.gif;.bmp"))
                    return "Image";

                if (Href.ContainsAny(".html;.htm;.mhtm;.mhtml;.chm;"))
                    return "Web Page";

                else
                {
                    if(string.IsNullOrEmpty( LinkTypeOrginal))
                        return "Link";
                    if ((LinkTypeOrginal + "").ToLower().ContainsAny("html;text;text/html"))
                        return "Link";
                    return LinkTypeOrginal;
                }
                
            }
        }
        public bool IsWorking
        {
            get
            {
                if (_LinkType == null)
                    IsAccessibleAsync();
                return _IsWorking;
            }
        }

         void  IsAccessibleAsync(){
            // IsAccessible(Href);
             Task.Factory.StartNew(() => IsAccessible(Href)).Wait() ;
        }
        private   void  IsAccessible(string url)
         {
            // return;
             _LinkType = "";
             DateOfSearch = DateTime.Now;
            if (url == null)
            {
                _IsWorking = false;
                return;
            }

            if (url.IndexOf(':') < 0)
                url = "http://" + url.TrimStart('/');

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                // _IsWorking = false;
              //  return;
            }

            var request = BuildRequestObject(url);
          
            try
            {
                using (var response =  request.GetResponse() as HttpWebResponse)
                {
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        StatusDescription = response.StatusDescription;
                        CharacterSet = response.CharacterSet;
                        ContentEncoding = response.ContentEncoding;
                        ContentLength = response.ContentLength;
                        IsFromCache = response.IsFromCache;
                        LastModified = response.LastModified;
                        Method = response.Method;
                        ProtocolVersion = response.ProtocolVersion.ToString();
                        ResponseUrl = response.ResponseUri.ToString();
                        Server = response.Server;


                        _LinkType = response.ContentType; 
                        _IsWorking = true;
                        return;
                    }

                    _IsWorking = false;
                    return;
                }
            }
            catch(Exception er)
            {
                StatusDescription = er.Message;
                _IsWorking = false;
            }
        }
       

        protected virtual HttpWebRequest BuildRequestObject(string  uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = true ;
            request.Method = "Head";
            if(!string.IsNullOrWhiteSpace(RequestMethod))
                request.Method = RequestMethod;

            request.UserAgent = "Mozilla/5.0";
            request.Accept = "*/*";
            if(Timeout>0)
                request.Timeout = Timeout;
          
            return request;
        }

        public string ContentEncoding { get; private set; }
        public string CharacterSet { get; private set; }
        public long ContentLength { get; private set; }
        public bool IsFromCache { get; private set; }
        public string Method { get; private set; }
        public string ProtocolVersion { get; private set; }
        public string ResponseUrl { get; private set; }
        public string Server { get; private set; }
        public DateTime LastModified { get; private set; }

        public string RequestMethod { get; set; }
        public string ContentSize
        {
            get
            {
                if (ContentLength > 0)
                    return ContentLength.ToFormattedFileSize();
                else
                    return string.Empty;
            }
        }
        
    }
}
