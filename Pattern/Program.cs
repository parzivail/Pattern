using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Pattern
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length == 0)
				RunInteractive();
			else
			{
				var pattern = LoadPattern(args[0]);
				if (pattern == null)
				{
					Console.WriteLine($"No pattern.json found in: \"{args[0]}\"");
					return;
				}

				if (args.Length == 1)
					GetFieldsAndCompile(pattern);
				else
					ConsumeFieldsAndCompile(pattern, args.AsSpan(1));
			}
		}

		private static void RunInteractive()
		{
			var patternDirectory = "./Patterns";
			var patterns = LoadPatterns(patternDirectory);
			var pattern = SelectPattern(patterns);

			GetFieldsAndCompile(pattern);
		}

		private static void GetFieldsAndCompile(Pattern pattern)
		{
			var fieldKeys = pattern.Descriptor.Fields;
			var fieldValues = new Dictionary<string, string>();

			foreach (var fieldKey in fieldKeys)
			{
				if (fieldKey.Source != null)
				{
					fieldValues[fieldKey.Key] = GetTransformedValue(fieldValues, fieldKey);
					continue;
				}

				Console.Write($"{fieldKey.Name}: ");
				fieldValues[fieldKey.Key] = Console.ReadLine();
			}

			CompilePattern(pattern, fieldValues);
		}

		private static void ConsumeFieldsAndCompile(Pattern pattern, Span<string> values)
		{
			var fieldKeys = pattern.Descriptor.Fields;
			var fieldValues = new Dictionary<string, string>();

			var inputValueIdx = 0;
			foreach (var fieldKey in fieldKeys)
			{
				if (fieldKey.Source != null)
				{
					fieldValues[fieldKey.Key] = GetTransformedValue(fieldValues, fieldKey);
					continue;
				}
				
				if (inputValueIdx >= values.Length)
				{
					Console.WriteLine($"Skipped field \"{fieldKey.Name}\" with nonexistent value");
					continue;
				}

				fieldValues[fieldKey.Key] = values[inputValueIdx];
				inputValueIdx++;
			}

			CompilePattern(pattern, fieldValues);
		}

		private static string GetTransformedValue(Dictionary<string, string> fieldValues, PatternField field)
		{
			var originalValue = GetWords(fieldValues[field.Source]);

			switch (field.Case)
			{
				case PatternFieldCase.Pascal:
					return string.Join("", originalValue.Select(s => char.ToUpper(s[0]) + s.Substring(1)));
				case PatternFieldCase.Snake:
					return string.Join("_", originalValue);
				case PatternFieldCase.Kabob:
					return string.Join("-", originalValue);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static string[] GetWords(string str)
		{
			return str.Split(' ').Select(s => s.ToLower()).ToArray();
		}

		private static void CompilePattern(Pattern pattern, Dictionary<string, string> fieldValues)
		{
			var (patternDescriptor, path) = pattern;

			foreach (var file in patternDescriptor.Files)
			{
				var sourcePath = Path.Combine(path, file.Source);
				if (!File.Exists(sourcePath))
				{
					Console.WriteLine($"Skipped nonexistent source file \"{file.Source}\"");
					continue;
				}

				string destPath = null;
				if (file.Destination != null)
				{
					destPath = CompileString(fieldValues, file.Destination);

					var destDir = Path.GetDirectoryName(destPath);
					if (!Directory.Exists(destDir))
						Directory.CreateDirectory(destDir);
				}

				switch (file.Action)
				{
					case PatternFileAction.Modify:
					{
						var fileContents = File.ReadAllText(sourcePath);

						fileContents = CompileString(fieldValues, fileContents);

						if (destPath == null)
							Console.WriteLine(fileContents);
						else
							File.WriteAllText(destPath, fileContents);

						break;
					}
					case PatternFileAction.Copy:
						if (destPath == null)
						{
							using var fs = File.Open(sourcePath, FileMode.Open);
							using var cs = Console.OpenStandardOutput();
							fs.CopyTo(cs);
						}
						else
							File.Copy(sourcePath, destPath);

						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static string CompileString(Dictionary<string, string> fieldValues, string fileContents)
		{
			foreach (var (key, value) in fieldValues)
				fileContents = fileContents.Replace(key, value);

			return fileContents;
		}

		private static Pattern SelectPattern(List<Pattern> patterns)
		{
			for (var i = 0; i < patterns.Count; i++)
				Console.WriteLine($"[{i}] {patterns[i].Descriptor.Name} [{patterns[i].Descriptor.Version}] ({patterns[i].Descriptor.Author})");

			string input;
			int selected;
			do
			{
				Console.Write("> ");
				input = Console.ReadLine();
			} while (!int.TryParse(input, out selected) || selected < 0 || selected >= patterns.Count);

			return patterns[selected];
		}

		private static Pattern LoadPattern(string patternFolder)
		{
			var descriptorPath = Path.Combine(patternFolder, "pattern.json");
			if (!File.Exists(descriptorPath))
				return null;

			var descriptor = JsonConvert.DeserializeObject<PatternDescriptor>(File.ReadAllText(descriptorPath));
			return new Pattern(descriptor, patternFolder);
		}

		private static List<Pattern> LoadPatterns(string patternsDirectory)
		{
			var patterns = new List<Pattern>();

			if (!Directory.Exists(patternsDirectory))
			{
				Directory.CreateDirectory(patternsDirectory);
				return patterns;
			}

			foreach (var patternDirectory in Directory.GetDirectories(patternsDirectory))
			{
				var pattern = LoadPattern(patternDirectory);
				if (pattern == null)
				{
					Console.WriteLine($"Skipped pattern directory \"{Path.GetDirectoryName(patternDirectory)}\" with no pattern.json");
					continue;
				}

				patterns.Add(pattern);
			}

			return patterns;
		}
	}
}