using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

namespace Zaubar.Network {
    public class GetInfoFromTheasoftXML : MonoBehaviour {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text productionTypeText;
        [SerializeField] private TMP_Text dateText;

        private void OnEnable() {
            //get current XML

            const string fileName = "Export_XML9.xml";
            var localFilePath = $@"{Application.persistentDataPath}/{fileName}";

            //check age of local file if already existing, don't download if e.g. less than a day old 
            var threshold = DateTime.Now.AddDays(-1);
            if (!File.Exists(localFilePath) || File.GetCreationTime(localFilePath) < threshold) {
                var ftpClient = 
                    new FTPClient("ftp://domain.tld", "username", "password");
                ftpClient.DownloadFile(fileName, localFilePath, ParseXML);
            }
            else {
                ParseXML();
            }

            void ParseXML() { 
                //parse downloaded XML
                var xml = XDocument.Load(localFilePath);

                //Get from season -> Productions -> all Production items with IsInactive==false and IsPublished==true
                //get all Activities, sorted by start date with child Stage that matches App locations? and with child
                //ActivityType with attribute IsPerformance==true 
                if (xml.Root != null) {
                    //get all activities which match criteria
                    var nextActivities = (from activity in xml.Root.Descendants("Activity")
                        //production should be published and not inactive
                        where (string)activity.Parent?.Parent?.Attribute("IsPublished") == "true"
                        where (string)activity.Parent?.Parent?.Attribute("IsInactive") == "false"
                        //where (string)activity.Parent?.Parent?.Element("Genre")?.Attribute("GenreName") == "Oper"
                        //starting in the next year
                        where (DateTime)activity.Attribute("Start") > DateTime.Now &&
                              (DateTime)activity.Attribute("Start") < DateTime.Now.AddYears(1)
                        //is a performance
                        where (string)activity.Element("ActivityType")?.Attribute("IsPerformance") == "true"
                        //ordered by starting date
                        orderby (DateTime)activity.Attribute("Start")
                        select activity).Take(1);

                    if (nextActivities.Any()) {
                        var activity = nextActivities.First();
                        var start = DateTime.Parse(activity.Attribute("Start")?.Value);
                        var end = DateTime.Parse(activity.Attribute("End")?.Value);

                        var productionTitle = activity.Parent?.Parent?.Attribute("Title")?.Value;
                        //var title = activity.Element("Title").Value;
                        var stageName = activity.Element("Stage")?.Attribute("StageName")?.Value;
                        //var activityName = activity.Element("ActivityType")?.Attribute("Name")?.Value;
                        var activityName = "";
                        if (stageName != null) {
                            if (stageName.ToLower().Contains("düsseldorf")){
                                activityName = "Düsseldorf";
                            } else if (stageName.ToLower().Contains("duisburg")){
                                activityName = "Duisburg";
                            } else {
                                activityName = stageName;
                            }
                        }

                        titleText.text = $"{productionTitle}";
                        productionTypeText.text = $"{activityName}";

                        var weekday = Global.GetCultureForCurrentI2Language().DateTimeFormat.GetDayName(start.DayOfWeek);
                        dateText.text =
                            $"{weekday}, {start.Day}.{start.Month}.\n{start.Hour}:{start.Minute:D2} UHR";
                    }
                    else {
                        titleText.text = "";
                        productionTypeText.text = "";
                        dateText.text = "(Keine Veranstaltungen)";
                    }
                }
                else {
                    Debug.Log("Can't load/parse Theasoft XML!");
                }
            }
        }
    }
}
