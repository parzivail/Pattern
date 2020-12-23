using Newtonsoft.Json;

namespace Pattern
{
	public class PatternFile
	{
		[JsonProperty("source")] public string Source { get; set; }
		[JsonProperty("action")] public PatternFileAction Action { get; set; } = PatternFileAction.Modify;
		[JsonProperty("destination")] public string Destination { get; set; }
	}
}