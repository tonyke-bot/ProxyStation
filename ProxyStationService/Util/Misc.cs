using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyStation.Util
{
    public class Misc
    {
        public static string KebabCase2PascalCase(string kebabCase)
        {
            var words = kebabCase
                .Split('-')
                .Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1))
                .Aggregate((a, b) => a + b);
            return words;
        }

        public static bool SplitHostAndPort(string hostPort, out string host, out int port)
        {
            host = default;
            port = default;

            if (string.IsNullOrEmpty(hostPort)) return false;

            var parts = hostPort.Split(":");
            if (parts.Length != 2) return false;

            if (!int.TryParse(parts[1], out int _port)) return false;
            if (_port > 0xFFFF || _port < 1) return false;

            host = parts[0];
            port = _port;
            return true;
        }

        /// <summary>
        /// Parse properties file like:
        ///   [Section1]
        ///   key=value
        ///   key=value
        ///   
        ///   [Section2]
        ///   key=value
        ///   key=value
        /// into 
        /// </summary>
        /// <param name="fileContent">Content of a properties file</param>
        /// <returns>a dictionary whose key is section name and value is a list of text lines in this section</returns>
        public static Dictionary<string, List<string>> ParsePropertieFile(string fileContent, string commentLinePrefixes = "#;")
        {
            var result = new Dictionary<string, List<string>>();
            var commentPrefix = commentLinePrefixes
                .Select(c => c)
                .Where(c => c != ' ')
                .ToHashSet();
            var pos = 0;
            var length = fileContent.Length;

            // skip whitespaces in the beginning
            while (pos < length && char.IsWhiteSpace(fileContent[pos])) pos++;

            var sectionName = default(string);
            var sectionValues = default(List<string>);

            for (; pos < length;)
            {
                // move to next non-whitespace char or EOF
                if (char.IsWhiteSpace(fileContent[pos]))
                {
                    pos++;
                    continue;
                }

                var c = fileContent[pos];
                if (c == '[')
                {
                    // capture section name
                    var startPos = pos;
                    for (; pos < length && fileContent[pos] != ']'; pos++)
                    {
                        if (fileContent[pos] == '\n' || fileContent[pos] == '\r')
                        {
                            // mutli-line section name is illegal
                            throw new FormatException($"Expect ] but get EOL. Pos: {pos}");
                        }
                    }

                    if (pos >= length)
                    {
                        throw new FormatException($"Expect ] but get EOF");
                    }

                    sectionName = fileContent[(startPos + 1)..pos];
                    sectionValues = new List<string>();
                    result.TryAdd(sectionName, sectionValues);
                    pos++;
                }
                else if (commentPrefix.Contains(c))
                {
                    // comment line, move to next whitespace or EOF
                    for (; pos < length && fileContent[pos] != '\r' && fileContent[pos] != '\n'; pos++) ;
                }
                else if (sectionValues == default)  // values are not contained in a section
                {
                    throw new FormatException($"Expect [ or EOF but get character #{Convert.ToUInt32(c)}(ASCII)`. Pos: {pos}");
                }
                else
                {
                    // capture section values 
                    var startPos = pos;
                    for (; pos < length && fileContent[pos] != '\n' && fileContent[pos] != '\r'; pos++) ;
                    sectionValues.Add(fileContent[startPos..pos]);
                }
            }

            return result;
        }
    }
}