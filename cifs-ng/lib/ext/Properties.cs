/*
 *	jcifs-ng for dotnet
 *  ikas-mc@github 2021
 */

using System;
using System.Collections.Generic;
using System.IO;
namespace cifs_ng.lib.ext {
	public class Properties {
		private readonly Dictionary<string, string> items;

		public Properties() {
			items = new Dictionary<string, string>();
		}

		public void putAll(IDictionary<string, string> dic) {
			foreach (var kv in dic) {
				items.put(kv.Key, kv.Value);
			}
		}

		public Dictionary<string, string> toDictionary() {
			return new Dictionary<string, string>(items);
		}

		public void setProperty(string key, string value) {
			items.put(key, value);
		}

		public string getProperty(string key) {
			return items.get(key);
		}

		public string getProperty(string key, string def) {
			var value = items.get(key);
			return value ?? def;
		}

		public void load(Stream input) {
			var sr = new StreamReader(input);
			while (!sr.EndOfStream) {
				var line = sr.ReadLine();

				if (string.IsNullOrEmpty(line)) {
					continue;
				}

				var tokens = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
				items.put(tokens[0].Trim(), tokens[1]?.Trim());
			}
		}

	}
}