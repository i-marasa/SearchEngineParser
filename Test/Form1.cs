using CsQuery;
using SearchEngineParser;
using SearchEngineParser.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        TaskThreadManager t = null;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //var c = SearchEngine.GetCurrentEngines();
                //SearchEngine.AddNewSearchEngine(new SearchEngine { Name = "google" });
                t = new TaskThreadManager(10);
          button1.Enabled=false ;
          listView1.Items.Clear();
                
        //  GetResults("Youtube", textBox1.Text, 3);
          t.DoWork(() => GetResults("Google", textBox1.Text));
          //t.DoWork(() => GetResults("Yahoo", textBox1.Text, 1));
          //t.DoWork(() => GetResults("Bing", textBox1.Text, 2));
          //t.DoWork(() => GetResults("Baidu", textBox1.Text, 3));
          //t.DoWork(() => GetResults("Yandex", textBox1.Text));



         // GetResults("yahoo", textBox1.Text, 0);
            }
            catch (Exception er)
            {

                Text = er.Message;

            }
            finally { Text = "Working.."; }
            
        }

        private void GetResults(string EngineName, string term, int GroupIndex = 0)
        {
            SearchEngine google = new SearchEngine(EngineName,term , 10);
            google.Timeout = 2000000;
            google.RequestMethod = "Head"; // Set Head or Get
           // google.SearchInHostOrSite = "google.com";
            var links = google.GetResult();
            foreach (var weblink in links)
            {
                logToListview(weblink, GroupIndex,EngineName+" ("+ google.timer.Elapsed.ToHumanReadable() +")");
            }
           
           // Text = google.timer.Elapsed.ToString();
        }
        private void GetResults(string EngineName, string term)
        {
           
            SearchEngine google = new SearchEngine(EngineName, term, 10);            
            google.Timeout = 2000000;
            google.RequestMethod = "Head"; // Set Head or Get
            var links = google.GetResult();
            //new ExportExcel(links).ExportToFile("d:\\xxxxuuuuuuy.csv");
            foreach (var weblink in links)
            {
                 logToListview(weblink, 0, EngineName + " (" + google.timer.Elapsed.ToHumanReadable() + ")");
            }

             Text = google.timer.Elapsed.ToString();
        }
        void logToListview(WebLink weblink,int GroupIndex,string time)
        {
            ControlInviker.ControlInvike(listView1, () =>
            {
                ListViewItem l = new ListViewItem((listView1.Groups[GroupIndex].Items.Count + 1) +"- "+ weblink.Text);
                l.Tag = weblink;
                l.SubItems.Add(weblink.Href);
                l.SubItems.Add(weblink.LinkType);
                l.SubItems.Add(weblink.IsWorking ? "Yes" : "No |" + weblink.StatusDescription);
                l.Group = listView1.Groups[GroupIndex];
                l.Group.Header =  time;
                
                listView1.BeginUpdate();
               // lock (listView1)
               // {
                    listView1.Items.Add(l);
               // }
                
               // listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView1.EndUpdate();
            });



        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count < 1) return;
            Clipboard.Clear();
            Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text );
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string term = "IDS ken";
            if ("Ken Mulkearn - United Kingdom | LinkedIn".ContainsAny(term))
            {
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            t.AbortAll(); button2.Enabled = false;
            while (t.HasRunningThreads ())
            {
                button2.Enabled = false;
                Application.DoEvents();
            }
            Text = "Done";
            button1.Enabled =button2.Enabled= true;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                propertyGrid1.SelectedObject = listView1.SelectedItems[0].Tag;
            }
        }

     
    }
}
