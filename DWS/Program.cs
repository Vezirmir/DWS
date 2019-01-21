using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml; 

namespace DailyExchange
{
    class Program
    {
        static void Main(string[] args)
        {
            int k = 0;
            string today = "http://www.tcmb.gov.tr/kurlar/today.xml";

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(today);
             
            DateTime exchangeDate = Convert.ToDateTime(xmlDoc.SelectSingleNode("//Tarih_Date").Attributes["Date"].Value);

            DataTable dtCurr = new DataTable();

            dtCurr.Columns.Add("date", typeof(DateTime));
            dtCurr.Columns.Add("ApproveDate", typeof(DateTime));
            dtCurr.Columns.Add("Rate1", typeof(decimal));
            dtCurr.Columns.Add("Rate2", typeof(decimal));
            dtCurr.Columns.Add("Rate3", typeof(decimal));
            dtCurr.Columns.Add("Rate4", typeof(decimal));
            dtCurr.Columns.Add("CurrencyCode", typeof(string));
            dtCurr.Columns.Add("CurrencyType", typeof(int));

            var path = @"D:\\DailyCurrency.txt";

            if (File.Exists(path))
            {
                var file = File.ReadAllBytes(path);

                var tumSatirlar = Encoding.GetEncoding(1254).GetString(file).Replace("\r", "").Split('\n');

                for (int index = 0; index < tumSatirlar.Length; index++)
                {
                    using (var outMs = new MemoryStream(file))
                    {
                        string[] items = tumSatirlar[index].Split(new char[] { '\t' });
                        if (items[0].Trim().ToString() == "" || items[1].Trim() == null || items[1].Trim().ToString() == "")
                            continue;

                        DataRow d = dtCurr.NewRow();
                        d["date"] = items[0].Trim();
                        d["ApproveDate"] = DateTime.Today;
                        d["Rate1"] = items[1].Trim();
                        d["Rate2"] = items[2].Trim();
                        d["Rate3"] = items[3].Trim();
                        d["Rate4"] = items[4].Trim();
                        d["CurrencyCode"] = items[5].Trim();

                        dtCurr.Rows.Add(d);
                    }
                }
            } 

            foreach (XmlNode currency in xmlDoc.SelectNodes("Tarih_Date/Currency"))
            {
                string code = currency.Attributes["Kod"].Value;
                if (code == "XDR")
                {
                    try
                    {
                        TextWriter sw = new StreamWriter(@"D:\\DailyCurrency.txt");
                         
                        if (dtCurr != null)
                        { 
                            int rowcount = dtCurr.Rows.Count;
                            for (int i = 0; i < rowcount; i++)
                            {
                                sw.WriteLine(dtCurr.Rows[i]["date"].ToString().Remove(10) + "\t" + dtCurr.Rows[i]["Rate1"].ToString() + "\t" + dtCurr.Rows[i]["Rate2"].ToString() + "\t" + dtCurr.Rows[i]["Rate3"].ToString() + "\t" + dtCurr.Rows[i]["Rate4"].ToString() + "\t" + dtCurr.Rows[i]["CurrencyCode"].ToString());
                            }
                            sw.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + " " + e.InnerException?.Message);
                    }

                    return;
                }
                string ForexBuying = xmlDoc.SelectSingleNode($"Tarih_Date/Currency[@Kod='{code}']/ForexBuying").InnerXml;
                string ForexSelling = xmlDoc.SelectSingleNode($"Tarih_Date/Currency[@Kod='{code}']/ForexSelling").InnerXml;
                string BanknoteBuying = xmlDoc.SelectSingleNode($"Tarih_Date/Currency[@Kod='{code}']/BanknoteBuying").InnerXml;
                string BanknoteSelling = xmlDoc.SelectSingleNode($"Tarih_Date/Currency[@Kod='{code}']/BanknoteSelling").InnerXml;



                if (dtCurr.AsEnumerable().Where(p => (DateTime)p["Date"] == DateTime.Today && (string)p["CurrencyCode"] == code).Count() == 0)
                {
                    dtCurr.Rows.Add();

                    dtCurr.Rows[k]["date"] = DateTime.Today;
                    dtCurr.Rows[k]["ApproveDate"] = DateTime.Today;
                    dtCurr.Rows[k]["Rate1"] = !string.IsNullOrWhiteSpace(ForexBuying) ? Convert.ToDecimal(ForexBuying, CultureInfo.InvariantCulture) : 0;
                    dtCurr.Rows[k]["Rate2"] = !string.IsNullOrWhiteSpace(ForexSelling) ? Convert.ToDecimal(ForexSelling, CultureInfo.InvariantCulture) : 0;
                    dtCurr.Rows[k]["Rate3"] = !string.IsNullOrWhiteSpace(BanknoteBuying) ? Convert.ToDecimal(BanknoteBuying, CultureInfo.InvariantCulture) : 0;
                    dtCurr.Rows[k]["Rate4"] = !string.IsNullOrWhiteSpace(BanknoteSelling) ? Convert.ToDecimal(BanknoteSelling, CultureInfo.InvariantCulture) : 0;
                    dtCurr.Rows[k]["CurrencyCode"] = code;
                    dtCurr.Rows[k]["CurrencyType"] = 1;
                    k++; 
                } 
            }
        }
    }
}
