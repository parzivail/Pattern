using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pattern
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum PatternFieldCase
	{
		Pascal,
		Snake,
		Kabob
	}
}