using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Operation;
using OsmSharp.Complete;
using OsmSharp.Streams;
using System.Linq;
using System.Xml.Linq;
using static OSMTest.Controllers.OSMTestController;

namespace OSMTest.Controllers
{
    [Route("[controller]")]
    public class OSMTestController : Controller
    {
        public const string MapPath = "C:/";
        public Dictionary<CountryTypes, string> MapDic = new Dictionary<CountryTypes, string>()
        {
           { CountryTypes.AU, "australia-latest.osm.pbf" },
            { CountryTypes.TW, "taiwan-latest.osm.pbf"}
        };
        [HttpGet("GetOSMStates")]
        public IActionResult GetOSMStates(CountryTypes country)
        {
            List<string> names = new List<string>();
            Dictionary<string, List<TagKeyValue>> dic = new Dictionary<string, List<TagKeyValue>>();
            List<State> states = new List<State>();

            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (MapDic.TryGetValue(country, out string path))
            {
                string combine = Path.Combine(MapPath, path);
                using (var fileStream = new FileInfo(combine).OpenRead())
                {
                    var source = new PBFOsmStreamSource(fileStream);
                    var completeSource = source.ToComplete();

                    foreach (var element in source)
                    {
                        try
                        {
                            if (element.Type == OsmSharp.OsmGeoType.Relation &&
                                element.Tags.Contains("boundary", "administrative") &&
                                element.Tags.Contains("admin_level", "4") &&
                                element.Tags.ContainsKey("name"))
                            {
                                //var relateion = element as CompleteRelation;
                                //dic.Add(element.Tags["int_name"], element.Tags.Select(item => new TagKeyValue { Key = item.Key, Value = item.Value }).ToList());
                                string Name = element.Tags.ContainsKey("name") ? element.Tags["name"] : "";
                                string Int_Name = element.Tags.ContainsKey("int_name") ? element.Tags["int_name"] : "";
                                string Boundary = element.Tags.ContainsKey("boundary") ? element.Tags["boundary"] : "";
                                string Admin_Level = element.Tags.ContainsKey("admin_level") ? element.Tags["admin_level"] : "";

                                states.Add(new State
                                {
                                    Name = Name,
                                    Int_Name = Int_Name,
                                    Boundary = Boundary,
                                    Admin_Level = Admin_Level
                                });
                                if (states.Count > 20) break;
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            return Ok(states);
        }

        [HttpGet("GetOSMSubhurb")]
        public IActionResult GetOSMSubhurb(CountryTypes country, string state)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            List<string> names = new List<string>();
            Dictionary<string, List<TagKeyValue>> dic = new Dictionary<string, List<TagKeyValue>>();
            List<Subhurb> subhurbs = new List<Subhurb>();
            if (MapDic.TryGetValue(country, out string path))
            {
                string combine = Path.Combine(dir, MapPath, path);
                using (var fileStream = new FileInfo(combine).OpenRead())
                {
                    var source = new PBFOsmStreamSource(fileStream);
                    var completeSource = source.ToComplete();

                    foreach (var element in source)
                    {
                        if (element.Type == OsmSharp.OsmGeoType.Relation &&
                            element.Tags.Contains("boundary", "administrative") &&
                            element.Tags.Contains("admin_level", "9") &&
                            element.Tags.Contains("place", "suburb"))
                        {
                            string Name = element.Tags.ContainsKey("name") ? element.Tags["name"] : "";
                            string Place = element.Tags.ContainsKey("place") ? element.Tags["place"] : "";
                            string Postal_Code = element.Tags.ContainsKey("postal_code") ? element.Tags["postal_code"] : "";
                            string Log_pid = element.Tags.ContainsKey("ref:psma:loc_pid") ? element.Tags["ref:psma:loc_pid"] : "";

                            subhurbs.Add(new Subhurb
                            {
                                Name = Name,
                                Place = Place,
                                Postal_Code = Postal_Code,
                                Log_pid = Log_pid
                                //Tags = element.Tags.Select(item => new TagKeyValue { Key = item.Key, Value = item.Value}).ToList()
                            });
                            //if (subhurbs.Count > 10) break;
                        }

                        //if ((element.Type == OsmSharp.OsmGeoType.Relation || element.Type == OsmSharp.OsmGeoType.Way) &&
                        //    //element.Tags.Contains("boundary", "administrative") &&
                        //    //element.Tags.Contains("admin_level", "4") &&
                        //    element.Tags.ContainsKey("place") &&
                        //    element.Tags["place"] == "suburb" &&
                        //    //new[] { "suburb" }.Contains(element.Tags["place"]) &&
                        //    element.Tags.ContainsKey("name"))
                        //{
                        //    //names.Add(element.Tags["name"]);
                        //    string Name = element.Tags.ContainsKey["name"] ? element.Tags["name"] : "";
                        //    subhurbs.Add(
                        //        new Subhurb
                        //        {
                        //            Name = Name,
                        //            Tags = element.Tags.Select(item => new TagKeyValue { Key = item.Key, Value = item.Value }).ToList()
                        //        });
                        //    if (subhurbs.Count > 10) break;
                        //}
                    }
                }
            }

            return Ok(subhurbs);
        }

        public class State
        {
            public string Int_Name { get; set; }
            public string Name { get; set; }
            public string Admin_Level { get; set; }
            public string Boundary { get; set; }
        }
        public class Subhurb
        {
            public string Name { get; set; }
            public string Place { get; set; }
            public string Postal_Code { get; set; }
            public string Log_pid { get; set; }
            //public List<TagKeyValue> Tags { get; set; }
        }
        public class TagKeyValue
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
