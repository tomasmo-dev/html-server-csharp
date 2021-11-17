using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace TimeTableImporter
{
    class ttJSONLoader
    {
        public struct hourCardHolder
        {
            public hourCardHolder(string subName, string t, string rN, string gN, string teach, bool change, bool cancel, bool add)
            {
                subjectName = subName;
                time = t;
                roomName = rN;
                groupName = gN;
                teacher = teach;

                changed = change;
                cancelled = cancel;
                added = add;
            }

            public string subjectName;
            public string time;
            public string roomName;
            public string groupName;
            public string teacher;

            public bool changed;
            public bool cancelled;
            public bool added;
        }

        #region cookieHeader
        public static string CHeader = "";

        #endregion

        #region params
        static bool _static = false;
        static bool _partial = true;
        static string _date = DateTime.Now.ToString("o");
        static int _uid = 11592;
        #endregion

        static string _url = $"https://sis.ssakhk.cz/TimeTable/PersonalNew/?static={_static}&partial={_partial}&date={_date}&userid={_uid}";
        //static void Main(string[] args)
        //{
        //    string ttTest = getTimeTableForDay(4);
        //    string htmlToJson = htmlTimeTDayToJSON(ttTest);

        //    Console.WriteLine();
        //}

        public static string getTimeTableForDay(int day)
        {
            if (day <= 0 || day > 5)
            {
                return "";
            }
            WebRequest request = WebRequest.Create(_url);
            request.Headers.Add(CHeader);

            WebResponse _response = request.GetResponse();

            Stream resStream = _response.GetResponseStream();

            StreamReader sRead = new StreamReader(resStream);

            string response = sRead.ReadToEnd();

            string DaySelect;

            switch (day)
            {
                case 1:
                    DaySelect = "col1";
                    break;

                case 2:
                    DaySelect = "col2";
                    break;

                case 3:
                    DaySelect = "col3";
                    break;

                case 4:
                    DaySelect = "col4";
                    break;

                case 5:
                    DaySelect = "col5";
                    break;

                default:
                    DaySelect = "col1";
                    break;
            }

            #region releaseResources

            resStream.Close();
            resStream.Dispose();

            _response.Close();
            _response.Dispose();

            sRead.Close();
            sRead.Dispose();
            #endregion

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);

            string htmlDayRes = doc.DocumentNode.SelectSingleNode($"//div[contains(@class, '{DaySelect}')]").InnerHtml;
            return htmlDayRes;

        }
        public static string htmlTimeTDayToJSON(string htmlDay)
        {
            htmlDay = htmlDay.Replace("\r", "").Replace("\n", "");


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlDay);

            string TTDay;
            List<hourCardHolder> hourCards = new List<hourCardHolder>();

            #region dayNode
            HtmlNode dayNode = doc.DocumentNode.SelectSingleNode("/label");

            TTDay = dayNode.InnerText;
            #endregion

            #region hourNode

            HtmlNodeCollection hourNodes = doc.DocumentNode.SelectNodes("/div[contains(@class, 'hour-cards')]//*[contains(@class, 'hour-card')]");
            int index = 0;

            foreach (HtmlNode hourCarSINGLE in hourNodes)
            {
                string sub; // subject
                string tim; //time
                string roomN;
                string gName;
                string teac; //teacher

                bool change = false;
                bool added = false;
                bool cancelled = false;

                sub = hourCarSINGLE.SelectSingleNode("./div[contains(@class, 'subject-name')]").InnerText.Trim();
                tim = hourCarSINGLE.SelectSingleNode("./div[contains(@class, 'time')]").InnerText.Trim();
                roomN = hourCarSINGLE.SelectSingleNode("./div[contains(@class, 'room-name')]").InnerText.Trim();
                gName = hourCarSINGLE.SelectSingleNode("./div[contains(@class, 'group-name')]").InnerText.Trim();
                teac = hourCarSINGLE.SelectSingleNode("./div[contains(@class, 'teacher')]").InnerText.Trim();

                string[] parentClasses = hourNodes[index].Attributes["class"].Value.Split(" ");
                foreach (string _class in parentClasses)
                {
                    if (_class == null)
                    {
                        continue;
                    }

                    if (_class == "added-card")
                    {
                        added = true;
                    }
                    else if (_class == "canceled-card")
                    {
                        cancelled = true;
                    }
                    else if (_class == "changed-card")
                    {
                        change = true;
                    }
                }

                //
                hourCardHolder cardHolder = new hourCardHolder();
                cardHolder.subjectName = sub;
                cardHolder.time = tim;
                cardHolder.roomName = roomN;
                cardHolder.groupName = gName;
                cardHolder.teacher = teac;

                cardHolder.changed = change;
                cardHolder.added = added;
                cardHolder.cancelled = cancelled;
                //

                hourCards.Add(cardHolder);
                index++;

            }
            #endregion

            string output = "{\n" +
                            $"\"day\": \"{TTDay}\",\n" +
                            "\"hourcards\": [\n";

            index = 0;
            int maxV = hourCards.Count - 1;

            foreach (hourCardHolder subject in hourCards)
            {
                output += "{\n" +
                          $"\"subject\": \"{subject.subjectName}\",\n" +
                          $"\"time\": \"{subject.time}\",\n" +
                          $"\"room\": \"{subject.roomName}\",\n" +
                          $"\"group_name\": \"{subject.groupName}\",\n" +
                          $"\"teacher\": \"{subject.teacher}\",\n" +
                          $"\"added\": \"{subject.added}\",\n" +
                          $"\"changed\": \"{subject.changed}\",\n" +
                          $"\"cancelled\": \"{subject.cancelled}\"\n" +
                          "}";
                if (index != maxV)
                {
                    output += ",";
                }
                index++;
            }

            output += "]}";

            return output;
        }
    }
}
