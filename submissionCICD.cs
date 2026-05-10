using System;
using System.Xml;
using System.Xml.Schema;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace ConsoleApp1
{
    public class Submission
    {
        public static string xmlURL = "https://bharathbabu-01.github.io/cse445a4/NationalParks.xml";
        public static string xmlErrorURL = "https://bharathbabu-01.github.io/cse445a4/NationalParksErrors.xml";
        public static string xsdURL = "https://bharathbabu-01.github.io/cse445a4/NationalParks.xsd";

        public static void Main(string[] args)
        {
            string r1 = Verification(xmlURL, xsdURL);
            Console.WriteLine(r1);

            string r2 = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(r2);

            string r3 = Xml2Json(xmlURL);
            Console.WriteLine(r3);
        }

        public static string Verification(string xmlUrl, string xsdUrl)
        {
            string errors = "";
            bool hasError = false;

            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas.Add(null, xsdUrl);

                settings.ValidationEventHandler += (s, e) =>
                {
                    hasError = true;
                    errors += e.Message + "\n";
                };

                XmlReader reader = XmlReader.Create(xmlUrl, settings);
                while (reader.Read()) { }
                reader.Close();
            }
            catch (Exception err)
            {
                if (hasError)
                    return errors.Trim() + "\n" + err.Message;
                return err.Message;
            }

            if (hasError)
                return errors.Trim();

            return "No errors are found";
        }

        public static string Xml2Json(string xmlUrl)
        {
            WebClient downloader = new WebClient();
            string xmlData = downloader.DownloadString(xmlUrl);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlData);

            XmlNode root = doc.DocumentElement;

            string json = "{\"NationalParks\":{\"NationalPark\":[";
            int count = 0;

            foreach (XmlNode park in root.ChildNodes)
            {
                if (park.NodeType != XmlNodeType.Element)
                    continue;

                if (count > 0)
                    json += ",";
                count++;

                json += "{";

                string name = "";
                string phones = "";
                string addr = "";
                string rating = "";
                int phoneCount = 0;

                foreach (XmlNode item in park.ChildNodes)
                {
                    if (item.NodeType != XmlNodeType.Element)
                        continue;

                    if (item.Name == "Name")
                    {
                        name = item.InnerText;
                    }
                    else if (item.Name == "Phone")
                    {
                        if (phoneCount == 0)
                        {
                            phones += "\"Phone\":[";
                        }
                        else
                        {
                            phones += ",";
                        }
                        phones += "\"" + item.InnerText + "\"";
                        phoneCount++;
                    }
                    else if (item.Name == "Address")
                    {
                        string nearAirport = "";
                        if (item.Attributes["NearestAirport"] != null)
                            nearAirport = item.Attributes["NearestAirport"].Value;

                        string num = "";
                        string street = "";
                        string city = "";
                        string state = "";
                        string zipCode = "";

                        foreach (XmlNode addrPart in item.ChildNodes)
                        {
                            if (addrPart.NodeType != XmlNodeType.Element)
                                continue;

                            if (addrPart.Name == "Number") num = addrPart.InnerText;
                            if (addrPart.Name == "Street") street = addrPart.InnerText;
                            if (addrPart.Name == "City") city = addrPart.InnerText;
                            if (addrPart.Name == "State") state = addrPart.InnerText;
                            if (addrPart.Name == "Zip") zipCode = addrPart.InnerText;
                        }

                        addr = "\"Address\":{";
                        addr += "\"Number\":\"" + num + "\",";
                        addr += "\"Street\":\"" + street + "\",";
                        addr += "\"City\":\"" + city + "\",";
                        addr += "\"State\":\"" + state + "\",";
                        addr += "\"Zip\":\"" + zipCode + "\",";
                        addr += "\"@NearestAirport\":\"" + nearAirport + "\"";
                        addr += "}";
                    }
                }

                if (park.Attributes["Rating"] != null)
                    rating = park.Attributes["Rating"].Value;

                json += "\"Name\":\"" + name + "\",";

                if (phoneCount > 0)
                    phones += "]";
                if (phones != "")
                    json += phones + ",";

                json += addr;

                if (rating != "")
                    json += ",\"@Rating\":\"" + rating + "\"";

                json += "}";
            }

            json += "]}}";

            return json;
        }
    }
}