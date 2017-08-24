using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using XCCDFParser;


namespace XCCDFResultConsolidator
{
    public class ConsolidatedResult
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public ConsolidatedResultStatus Status { get; set; }
    }

    public enum ConsolidatedResultStatus
    {
        Passed,
        Failed,
        Unknown,
        NotFound,
        Other
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("XCCDFResultsConsolidator <path to XCCDF> <Path to results.xml>");
                return;
            }

            string pathToXccdf = args[0];
            string pathToResults = args[1];

            var results = GetResults(pathToResults);

            var xccdf = GetXccdf(pathToXccdf);

        

            List<ConsolidatedResult> consolidatedResult = new List<ConsolidatedResult>();

            foreach (var benchmarkCheck in xccdf.Benchmark.Group)
            {
                var res = new ConsolidatedResult
                {
                    Id = benchmarkCheck.Id,
                    Name = benchmarkCheck.Title
                };

                var item = results.Oval_Results.Results.System.Definitions.Definition.FirstOrDefault(e => e.ID == benchmarkCheck.Rule.Check.Name);

                if (item == null)
                {
                    res.Status =ConsolidatedResultStatus.NotFound;
                }
                else
                {
                    switch (item.Result)
                    {
                        case "true":
                            res.Status = ConsolidatedResultStatus.Passed;
                            break;
                        case "false":
                            res.Status = ConsolidatedResultStatus.Failed;
                            break;
                        case "unknown":
                            res.Status = ConsolidatedResultStatus.Unknown;
                            break;
                        default:
                            res.Status = ConsolidatedResultStatus.Other;
                            break;
                    }
                }

                consolidatedResult.Add(res);
            }

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Passed Items");
            builder.AppendLine("============");

            OutputForType(consolidatedResult, ConsolidatedResultStatus.Passed, builder);
            builder.AppendLine("");

            builder.AppendLine("Unknown Items");
            builder.AppendLine("=============");
            OutputForType(consolidatedResult, ConsolidatedResultStatus.Unknown, builder);
            builder.AppendLine("");


            builder.AppendLine("Other Items");
            builder.AppendLine("=============");
            OutputForType(consolidatedResult, ConsolidatedResultStatus.Other, builder);
            builder.AppendLine("");

            builder.AppendLine("Failed Items");
            builder.AppendLine("=============");
            OutputForType(consolidatedResult, ConsolidatedResultStatus.Failed, builder);
            builder.AppendLine("");


            File.WriteAllText( pathToResults.Replace(Path.GetExtension(pathToResults), "-consolidated.txt" ) , builder.ToString());

        }

        private static void OutputForType(List<ConsolidatedResult> consolidatedResult, ConsolidatedResultStatus consolidatedResultStatus, StringBuilder builder)
        {
            foreach (var result in consolidatedResult.Where(e => e.Status == consolidatedResultStatus))
            {
                builder.AppendLine($"[{result.Id}] {result.Name}");
            }
        }

        private static Container GetXccdf(string pathToXccdf)
        {
            var readAllText = File.ReadAllText(pathToXccdf);

            var document = new XmlDocument();

            document.LoadXml(readAllText);

            var json = JsonConvert.SerializeXmlNode(document);

            var ee = JsonConvert.DeserializeObject<Container>(json);

            return ee;
        }

        private static modSIC.Container GetResults(string Path)
        {
            var readAllText = File.ReadAllText(Path);
            var document = new XmlDocument();
            document.LoadXml(readAllText);
            var json = JsonConvert.SerializeXmlNode(document);
            
            var ee = JsonConvert.DeserializeObject<modSIC.Container>(json);

            return ee;
        }
    }
}
