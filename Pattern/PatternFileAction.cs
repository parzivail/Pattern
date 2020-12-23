using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pattern
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum PatternFileAction
	{
		Modify,
		Copy
	}
}