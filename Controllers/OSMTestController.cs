using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Operation;
using OsmSharp.Complete;
using OsmSharp.Streams;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

                    ///https://zh.wikipedia.org/wiki/%E6%BE%B3%E5%A4%A7%E5%88%A9%E4%BA%9E%E9%83%B5%E6%94%BF%E7%B7%A8%E7%A2%BC
                    ///postcode
                    foreach (var element in source)
                    {
                        try
                        {
                            if (element.Type == OsmSharp.OsmGeoType.Relation &&
                                element.Tags.Contains("boundary", "administrative") &&
                                element.Tags.Contains("admin_level", "4") &&
                                element.Tags.ContainsKey("name"))
                            {
                                string Name = element.Tags.ContainsKey("name") ? element.Tags["name"] : "";
                                string Int_Name = element.Tags.ContainsKey("int_name") ? element.Tags["int_name"] : "";
                                string Boundary = element.Tags.ContainsKey("boundary") ? element.Tags["boundary"] : "";
                                string Admin_Level = element.Tags.ContainsKey("admin_level") ? element.Tags["admin_level"] : "";

                                if (element.Tags["name"] == "Western")
                                {
                                    states.Add(new State
                                    {
                                        Name = "Western Australia",
                                        Int_Name = "Western Australia",
                                        Boundary = "administrative",
                                        Admin_Level = "4",
                                        Iso_3166_1 = "",
                                        Iso_3166_2 = "AU-WA"
                                    });
                                    continue;
                                }

                                states.Add(new State
                                {
                                    Name = Name,
                                    Int_Name = Int_Name,
                                    Boundary = Boundary,
                                    Admin_Level = Admin_Level,
                                    Iso_3166_1 = element.Tags.ContainsKey("ISO3166-1") ? element.Tags["ISO3166-1"] : "",
                                    Iso_3166_2 = element.Tags.ContainsKey("ISO3166-2") ? element.Tags["ISO3166-2"] : ""
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

        //public IActionResult SearchOSM(CountryTypes country, string suburb, string postcode, string state)
        //{
        //    if (MapDic.TryGetValue(country, out string path))
        //    {
        //        string combine = Path.Combine(dir, MapPath, path);
        //        using (var fileStream = new FileInfo(combine).OpenRead())
        //        {
        //            var source = new PBFOsmStreamSource(fileStream);
        //            var completeSource = source.ToComplete();
        //        }
        //    }
        //}

        [HttpGet("GetOSMSubhurb")]
        public IActionResult GetOSMSubhurb(CountryTypes country, string state)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            List<PostCodes> codes = GetAUPostCodes();
            List<object> subhurbs = new List<object>();
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
                            element.Tags.Contains("admin_level", "9") // && element.Tags.ContainsKey("place")
                            )
                        {
                            string Name = element.Tags.ContainsKey("name") ? element.Tags["name"] : "";
                            string Admin_Level = element.Tags.ContainsKey("admin_level") ? element.Tags["admin_level"] : "";
                            string Place = element.Tags.ContainsKey("place") ? element.Tags["place"] : "";
                            string Postal_Code = element.Tags.ContainsKey("postal_code") ? element.Tags["postal_code"] : "";
                            string Log_pid = element.Tags.ContainsKey("ref:psma:loc_pid") ? element.Tags["ref:psma:loc_pid"] : "";

                            subhurbs.Add(new Subhurb
                            {
                                Name = Name,
                                Admin_Level = Admin_Level,
                                Place = Place,
                                Postal_Code = Postal_Code,
                                Log_pid = Log_pid,
                                State = CheckState(codes, Postal_Code, Log_pid)
                                //Tags = element.Tags.Select(item => new TagKeyValue { Key = item.Key, Value = item.Value}).ToList()
                            });
                            //if (subhurbs.Count > 10) break;
                        }
                        //else if (element.Type == OsmSharp.OsmGeoType.Node && element.Tags.ContainsKey("name") &&
                        //    (element.Tags.Contains("place", "city") || element.Tags.Contains("place", "town"))
                        //    )
                        //{
                        //    subhurbs.Add(new CityOrTown
                        //    {
                        //        Name = element.Tags["name"],
                        //        Place = element.Tags["place"]
                        //    });
                        //}

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

        [HttpGet("GetOSMSubhurbWithoutPlaceTag")]
        public IActionResult GetOSMSubhurbWithoutPlaceTag(CountryTypes country, string state)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            List<PostCodes> codes = GetAUPostCodes();
            List<object> subhurbs = new List<object>();
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
                            element.Tags.Contains("admin_level", "9")
                            && !element.Tags.ContainsKey("place")
                            )
                        {
                            string Name = element.Tags.ContainsKey("name") ? element.Tags["name"] : "";
                            string Admin_Level = element.Tags.ContainsKey("admin_level") ? element.Tags["admin_level"] : "";
                            string Place = element.Tags.ContainsKey("place") ? element.Tags["place"] : "";
                            string Postal_Code = element.Tags.ContainsKey("postal_code") ? element.Tags["postal_code"] : "";
                            string Log_pid = element.Tags.ContainsKey("ref:psma:loc_pid") ? element.Tags["ref:psma:loc_pid"] : "";

                            subhurbs.Add(new Subhurb
                            {
                                Name = Name,
                                Admin_Level = Admin_Level,
                                Place = Place,
                                Postal_Code = Postal_Code,
                                Log_pid = Log_pid,
                                State = CheckState(codes, Postal_Code, Log_pid)
                                //Tags = element.Tags.Select(item => new TagKeyValue { Key = item.Key, Value = item.Value}).ToList()
                            });
                        }
                    }
                }
            }

            return Ok(subhurbs);
        }

        public string CheckState(List<PostCodes> postCodes, string code, string log_pid)
        {
            var match = Regex.Match(log_pid, @"^([A-Z]{2,3})\d");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else if (!string.IsNullOrWhiteSpace(code))
            {
                string state = "";
                int.TryParse(code, out int intCode);
                foreach (PostCodes postCode in postCodes)
                {
                    foreach (var range in postCode.Ranges)
                    {
                        if (intCode >= range.Start && intCode <= range.End)
                        {
                            state = postCode.State;
                            break;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(state)) break;
                }
                return state;
            }
            else
            {
                return "";
            }
        }
        private List<PostCodes> GetAUPostCodes()
        {
            List<PostCodes> postCodes = new List<PostCodes>();
            postCodes.Add(
                new PostCodes()
                {
                    State = "NSW",
                    Ranges = new List<CodeRange>() {
                        new CodeRange(1000, 1999),
                        new CodeRange(2000, 2599),
                        new CodeRange(2619, 2899),
                        new CodeRange(2921, 2999)
                    }
                });
            postCodes.Add(
                new PostCodes()
                {
                    State = "ACT",
                    Ranges = new List<CodeRange>() {
                        new CodeRange(0200, 0299),
                        new CodeRange(2600, 2618),
                        new CodeRange(2900, 2920)
                    }
                });
            postCodes.Add(
                new PostCodes()
                {
                    State = "VIC",
                    Ranges = new List<CodeRange>() {
                        new CodeRange(3000, 3996),
                        new CodeRange(8000, 8999),
                    }
                });
            postCodes.Add(
                new PostCodes()
                {
                    State = "QLD",
                    Ranges = new List<CodeRange>() {
                        new CodeRange(4000, 4999),
                        new CodeRange(9000, 9999)
                    }
                });
            postCodes.Add(
                new PostCodes()
                {
                    State = "SA",
                    Ranges = new List<CodeRange>() {
                        new CodeRange(5000, 5799),
                        new CodeRange(5800, 5999)
                    }
                });
            postCodes.Add(
                new PostCodes()
                {
                    State = "WA",
                    Ranges = new List<CodeRange>()
                    {
                        new CodeRange(6000, 6797),
                        new CodeRange(6800, 6999)
                    }
                });
            postCodes.Add(
                new PostCodes()
                {
                    State = "TAS",
                    Ranges = new List<CodeRange>()
                    {
                        new CodeRange(7000, 7799),
                        new CodeRange(7800, 0999)
                    }
                });
            postCodes.Add(
                new PostCodes()
                {
                    State = "NT",
                    Ranges = new List<CodeRange>()
                    {
                        new CodeRange(0800, 0899),
                        new CodeRange(0900, 0999)
                    }
                });
            return postCodes;
        }
        public class State
        {
            public string Int_Name { get; set; }
            public string Name { get; set; }
            public string Iso_3166_1 { get; set; }
            public string Iso_3166_2 { get; set; }
            public string ShortName
            {
                get
                {
                    string sname = "";
                    if (!string.IsNullOrWhiteSpace(Iso_3166_1)) sname = Iso_3166_1;
                    if (!string.IsNullOrWhiteSpace(Iso_3166_2)) sname = Iso_3166_2;
                    return sname.Replace("AU-", "");
                }
            }
            public string Admin_Level { get; set; }
            public string Boundary { get; set; }
        }
        public interface IPlace
        {
            string Name { get; set; }
            string Place { get; set; }
            //public List<TagKeyValue> Tags { get; set; }
        }
        public class Subhurb : IPlace
        {
            public string Name { get; set; }
            public string Admin_Level { set; get; }
            public string Place { get; set; }
            public string State
            {
                get;
                //{
                //    var match = Regex.Match(Log_pid, @"^([A-Z]{2,3})\d");
                //    if (match.Success)
                //    {
                //        return match.Groups[1].Value;
                //    }
                //    else
                //    {
                //        return "";
                //    }
                //}
                set;
            }
            public string Postal_Code { get; set; }
            public string Log_pid { get; set; }
        }
        public class CityOrTown : IPlace
        {
            public string Name { get; set; }
            public string Place { set; get; }
        }
        public class TagKeyValue
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        public class PostCodes
        {
            public string State { get; set; }
            public List<CodeRange> Ranges { get; set; }
        }
        public class CodeRange
        {
            public CodeRange(int start, int end)
            {
                this.Start = start;
                this.End = end;
            }
            public int Start { get; set; }
            public int End { get; set; }
        }
    }
}
