namespace Exyll.MotorolaUp
{
    using System;
    using System.IO;
    using System.Xml;
    using TidyNet;

    public class UptimeResolver
    {
        Tidy tidy;

        public UptimeResolver()
        {
            tidy = new Tidy()
            {
                Options =
                {
                    DocType = DocType.Strict,
                    DropFontTags = true,
                    LogicalEmphasis = true,
                    Xhtml = true,
                    XmlOut = true,
                    MakeClean = true,
                    TidyMark = false,
                }
            };
        }

        public TimeSpan GetUptime()
        {
            /* Declare the parameters that is needed */
            TidyMessageCollection tmc = new TidyMessageCollection();
            MemoryStream xhtmlStream = new MemoryStream();

            var r = System.Net.WebRequest.Create("http://192.168.100.1/indexData.htm");
            r.Timeout = 5000;
            using (var res = r.GetResponse())
            using (var htmlStream = res.GetResponseStream())
            {
                tidy.Parse(htmlStream, xhtmlStream, tmc);
                res.Close();
            }

            //string result = Encoding.UTF8.GetString(xhtmlStream.ToArray());

            var d = new System.Xml.XmlDocument();
            xhtmlStream.Position = 0;
            d.Load(xhtmlStream);

            var navigator = d.CreateNavigator();

            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            manager.AddNamespace("x", d.DocumentElement.NamespaceURI); // http://www.w3.org/1999/xhtml

            var eUptime = (XmlElement)d.SelectSingleNode("x:html/x:body/x:table[2]/x:tbody/x:tr[3]/x:td[2]", manager);

            var v = eUptime.InnerText;

            v = v.Replace(" days ", ":");
            v = v.Replace("h", "");
            v = v.Replace("m", "");
            v = v.Replace("s", "");

            return TimeSpan.Parse(v);
        }
    }
}
