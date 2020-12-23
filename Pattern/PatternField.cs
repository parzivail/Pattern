using Newtonsoft.Json;

namespace Pattern
{
	public class PatternField
	{
		[JsonProperty("key")] public string Key { get; set; }
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("source")] public string Source { get; set; }
		[JsonProperty("case")] public PatternFieldCase Case { get; set; }
	}
}