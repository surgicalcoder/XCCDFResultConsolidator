using Newtonsoft.Json;

namespace XCCDFResultConsolidator.modSIC
{
    public class Definition
    {
        [JsonProperty("@definition_id")]
        public string ID { get; set; }
        [JsonProperty("@result")]
        public string Result { get; set; }
    }
}