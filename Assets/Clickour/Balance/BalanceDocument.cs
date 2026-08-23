using System.Collections.Generic;
using System.Text;

namespace Clickour.Balance
{
    public sealed class BalanceDocument
    {
        readonly List<string> keys = new();
        readonly Dictionary<string, string> values = new();

        public IReadOnlyList<string> Keys => keys;

        public static BalanceDocument Parse(string yaml)
        {
            var document = new BalanceDocument();

            foreach (var raw_line in yaml.Replace("\r", string.Empty).Split('\n'))
            {
                var line = raw_line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                var separator = line.IndexOf(':');
                var key = line[..separator].Trim();
                var value = line[(separator + 1)..].Trim();
                document.keys.Add(key);
                document.values.Add(key, value);
            }

            return document;
        }

        public string Get(string key) => values[key];

        public void Set(string key, string value) => values[key] = value;

        public string ToYaml()
        {
            var yaml = new StringBuilder();
            foreach (var key in keys)
                yaml.Append(key).Append(": ").AppendLine(values[key]);
            return yaml.ToString();
        }
    }
}
