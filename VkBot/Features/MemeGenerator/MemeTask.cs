using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VkBot.Features.MemeGenerator.Commands;

namespace VkBot.Features.MemeGenerator
{
    public class MemeTask
    {
        public List<IMemeCommand> MemeCommands { get; set; }
        private readonly Dictionary<string, Type> _commandsRoute = new Dictionary<string, Type>();
        private readonly IServiceProvider _serviceProvider;

        public MemeTask(IServiceProvider serviceProvider)
        {
            MemeCommands = new List<IMemeCommand>();
            _serviceProvider = serviceProvider;
            var avaibleCommands = serviceProvider.GetServices<IMemeCommand>();
            foreach (var command in avaibleCommands)
            {
                var commandName = command.GetCommandName();
                _commandsRoute.Add(commandName.ToLower(), command.GetType());
            }
        }

        public void LoadMemeConfig(string memeConfigPath)
        {
            var reader = new StreamReader(File.Open(memeConfigPath, FileMode.Open));
            var directoryPath = Path.GetDirectoryName(memeConfigPath);
            string line;
            var isDone = false;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.ToLower() == "done();")
                {
                    isDone = true;
                    break;
                }
                else
                {
                    var functionName = ParseFunctionName(line).ToLower();
                    if (_commandsRoute.ContainsKey(functionName))
                    {
                        var args = ParseArguments(line);
                        var commandType = _commandsRoute[functionName];
                        var command = (IMemeCommand)Activator.CreateInstance(commandType, _serviceProvider);
                        command.SetArgs(args);
                        command.SetMemeDirectory(directoryPath);
                        MemeCommands.Add(command);
                    }
                    else
                    {
                        Console.WriteLine($"Skipping unknown command: {functionName}");
                    }
                }
            }

            if (!isDone)
            {
                throw new ArgumentException("Invalid meme config", nameof(memeConfigPath));
            }
        }

        private Dictionary<string, string> ParseArguments(string line)
        {
            var remainedLine = line.Substring(ParseFunctionName(line).Length);
            if (remainedLine[0] != '(')
            {
                throw new ArgumentException("Invalid line", nameof(line));
            }
            else
            {
                remainedLine = remainedLine.Substring(1);
            }

            var result = new Dictionary<string, string>();
            string key = null;
            var tokenType = GetNextTokenType(remainedLine);
            while (remainedLine[0] != ')')
            {
                switch (tokenType)
                {
                    case ParseTokenType.Number:
                    case ParseTokenType.String:
                        var @string = ReadString(ref remainedLine);
                        if (GetNextTokenType(remainedLine) == ParseTokenType.Whitespace)
                            ReadWhitespace(ref remainedLine);
                        else if (GetNextTokenType(remainedLine) == ParseTokenType.Equal)
                        {
                            key = @string;
                        }
                        else if (GetNextTokenType(remainedLine) == ParseTokenType.CloseBracket
                                 || GetNextTokenType(remainedLine) == ParseTokenType.Comma)
                        {
                            if (key != null)
                            {
                                result[key] = @string;
                                key = null;
                            }
                            else
                            {
                                result.Add("@main_argument", @string);
                            }
                        }
                        break;
                    case ParseTokenType.Comma:
                    case ParseTokenType.Equal:
                        remainedLine = remainedLine.Substring(1);
                        break;
                    case ParseTokenType.Whitespace:
                        ReadWhitespace(ref remainedLine);
                        break;
                    default:
                        break;
                }
                tokenType = GetNextTokenType(remainedLine);
            }

            return result;
        }

        private void ReadWhitespace(ref string remainedLine)
        {
            var whitespaceLength = 0;

            for (var i = 0; i < remainedLine.Length; i++)
            {
                var c = remainedLine[i];

                if (char.IsWhiteSpace(c))
                {
                    whitespaceLength++;
                }
                else
                {
                    break;
                }

            }

            remainedLine = remainedLine.Substring(whitespaceLength);
        }

        private string ReadString(ref string remainedLine)
        {
            if (remainedLine[0] != '\'')
            {
                var stringBuilder = new StringBuilder();
                for (var i = 0; i < remainedLine.Length; i++)
                {
                    var c = remainedLine[i];

                    if (!char.IsLetterOrDigit(c) || c == '_')
                    {
                        break;
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }

                remainedLine = remainedLine.Substring(stringBuilder.Length);
                return stringBuilder.ToString();
            }
            else
            {
                var stringBuilder = new StringBuilder();
                var lastChar = remainedLine[0];
                for (var i = 1; i < remainedLine.Length; i++)
                {
                    var c = remainedLine[i];

                    if (c == '\'' && lastChar == '\\')
                        stringBuilder.Append('\'');
                    else
                    {
                        if (c == '\'')
                            break;
                        else
                        {
                            stringBuilder.Append(c);
                        }
                    }

                    lastChar = c;
                }

                remainedLine = remainedLine.Substring(1).Substring(stringBuilder.Length + 1);
                return stringBuilder.ToString();
            }
        }

        private ParseTokenType GetNextTokenType(string line)
        {
            var c = line[0];
            if (char.IsWhiteSpace(c))
                return ParseTokenType.Whitespace;
            if (Char.IsDigit(c))
                return ParseTokenType.Number;
            if (Char.IsLetter(c))
                return ParseTokenType.String;
            if (c == '=')
                return ParseTokenType.Equal;
            if (c == ',')
                return ParseTokenType.Comma;
            if (c == '\'')
                return ParseTokenType.String;
            if (c == '(')
                return ParseTokenType.OpenBracket;
            if (c == ')')
                return ParseTokenType.CloseBracket;
            throw new Exception("Invalid token!");
        }

        private string ParseFunctionName(string line)
        {
            var stringBuilder = new StringBuilder();
            var c = '\0';
            var i = 0;
            while ((c = line[i]) != '(')
            {
                stringBuilder.Append(line[i++]);
            }
            if (line[i] != '(')
                throw new ArgumentException("Invalid line to parse", nameof(line));
            return stringBuilder.ToString();
        }

        private enum ParseTokenType
        {
            Number,
            String,
            Comma,
            Equal,
            Whitespace,
            OpenBracket,
            CloseBracket
        }
    }
}
