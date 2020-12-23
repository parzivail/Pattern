using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pattern
{
	public class PatternDescriptor
	{
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("author")] public string Author { get; set; }
		[JsonProperty("version")] public string Version { get; set; }
		[JsonProperty("fields")] public PatternField[] Fields { get; set; }
		[JsonProperty("files")] public List<PatternFile> Files { get; set; }
	}
}