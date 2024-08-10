#if UNITY_EDITOR && CVR_CCK_EXISTS
using System;
using System.Collections.Generic;
using System.Text;

namespace NAK.LuaTools
{
	// entirely Exterrata, thank you <3
	public static class LuaScriptMerger {
		private static readonly HashSet<char> Delimiters = new() {
			' ', '\t', '\n', '\r', '(', ')', ';', '=', '"', //'.'':'
		};
		
		public static string MergeScripts(string[] scripts) {
			string script = Merge(scripts);
			List<int> markers = Scan(script);
			return Deduplicate(markers, script);
		}

		private static string Merge(string[] scripts) {
			StringBuilder builder = new();
			foreach (string script in scripts) {
				builder.AppendLine(script);
			}

			return builder.ToString();
		}

		private static List<int> Scan(string script) {
			ReadOnlySpan<char> text = script.AsSpan();
			List<int> markers = new();

			int start = 0, depth = 0;
			while (start < text.Length) {
				int end = start;
				while (end < text.Length && !Delimiters.Contains(text[end])) {
					end++;
				}

				if (end > start) {
					string word = text.Slice(start, end - start).ToString();
					switch (word) {
						case "require":
							markers.Add(start);
							break;
						case "end":
							if (--depth == 0) markers.Add(end + 1);
							break;
						case "function":
						case "if":
						case "for":
						case "while":
							if (depth++ == 0) markers.Add(start);
							break;
					}
				}

				start = end + 1;
			}

			return markers;
		}

		private static int LastWord(int marker, ReadOnlySpan<char> text) {
			while (marker >= 0 && !Delimiters.Contains(text[marker])) marker--;
			while (marker >= 0 && Delimiters.Contains(text[marker])) marker--;
			while (marker >= 0 && !Delimiters.Contains(text[marker])) marker--;
			return marker + 1;
		}
		
		private static int NextWord(int marker, ReadOnlySpan<char> text) {
			while (marker < text.Length && !Delimiters.Contains(text[marker])) marker++;
			while (marker < text.Length && Delimiters.Contains(text[marker])) marker++;
			return marker;
		}
		
		private static int LastWord(int marker, ReadOnlySpan<char> text, out int end) {
			while (marker >= 0 && !Delimiters.Contains(text[marker])) marker--;
			while (marker >= 0 && Delimiters.Contains(text[marker])) marker--;
			end = marker + 1;
			while (marker >= 0 && !Delimiters.Contains(text[marker])) marker--;
			return marker + 1;
		}
		
		private static int NextWord(int marker, ReadOnlySpan<char> text, out int end) {
			while (marker < text.Length && !Delimiters.Contains(text[marker])) marker++;
			while (marker < text.Length && Delimiters.Contains(text[marker])) marker++;
			int start = marker;
			while (marker < text.Length && !Delimiters.Contains(text[marker])) marker++;
			end = marker;
			return start;
		}

		private static string Deduplicate(List<int> markers, string script) {
			ReadOnlySpan<char> text = script.AsSpan();
			StringBuilder requireBuilder = new();
			StringBuilder functionBuilder = new();
			StringBuilder otherBuilder = new();

			Dictionary<string, string> modules = new();
			HashSet<string> functions = new();

			int start = 0;
			for (var i = 0; i < markers.Count; i++) {
				int end = markers[i];
				while (!Delimiters.Contains(text[end])) end++;
				string word = text.Slice(markers[i], end - markers[i]).ToString();
				
				switch (word) {
					case "require":
						// look backwards for variable name
						int varStart = LastWord(markers[i], text, out end);
						string variable = text.Slice(varStart, end - varStart).ToString();
						
						// check for local
						int localStart = LastWord(varStart, text, out end);
						if (text.Slice(localStart, end - localStart).ToString() != "local") {
							localStart = varStart;
						}
						
						// append last block
						otherBuilder.Append(text.Slice(start, localStart - start));
						
						// look forwards for module name
						start = NextWord(markers[i], text, out end);
						string module = text.Slice(start, end - start).ToString();
						
						start = NextWord(start, text);

						// if module name is duplicate replace (require("...")|require "...") with non duplicate require variable
						if (modules.TryAdd(module, variable)) {
							requireBuilder.Append($"{variable} = require(\"{module}\")\n");
						}
						else {
							requireBuilder.Append($"{variable} = {modules[module]}\n");
						}
						break;
					case "function":
						// extend functionStart to include local if it exists
						int functionStart = LastWord(markers[i], text, out int functionEnd);
						if (text.Slice(functionStart, functionEnd - functionStart).ToString() != "local") {
							functionStart = markers[i];
						}

						otherBuilder.Append(text.Slice(start, functionStart - start));
						
						// look forward for function name
						start = NextWord(markers[i], text, out end);
						string function = text.Slice(start, end - start).ToString();
						
						// next marker should be "end"
						i++;

						// append function if its unique
						if (functions.Add(function)) {
							functionBuilder.Append(text.Slice(functionStart, markers[i] - functionStart));
						}

						start = markers[i];
						break;
				}
			}

			// append last block
			otherBuilder.Append(text.Slice(start, text.Length - start));

			// merge order in 
			requireBuilder.Append('\n');
			requireBuilder.Append(otherBuilder);
			requireBuilder.Append('\n');
			requireBuilder.Append(functionBuilder);

			StringBuilder builder = new();

			bool lastNewline = false;
			int blockStart = 0;
			for (int i = 0; i < requireBuilder.Length; i++) {
				if (requireBuilder[i] == '\n' || requireBuilder[i] == '\r') {
					if (!lastNewline) {
						lastNewline = true;

						builder.Append(requireBuilder, blockStart, i - blockStart);
						builder.Append('\n');
					}
				}
				else {
					if (lastNewline)
						blockStart = i;
					lastNewline = false;
				}
			}
			
			return builder.ToString();
		}
	}
}
#endif