using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyTTCBot.Commands
{
    public class InputCommand
    {
        public string Name { get; set; }

        public string[] Args { get; set; }

        public string RawInput { get; set; }

        public static explicit operator InputCommand(string text)
        {
            InputCommand inputCommand;
            try
            {
                inputCommand = new InputCommand { RawInput = text };
                var tokens = Regex.Split(text, @"\s");
                inputCommand.Name = tokens[0];
                inputCommand.Args = tokens.Skip(1).ToArray();
            }
            catch (Exception)
            {
                inputCommand = null;
            }
            return inputCommand;
        }
    }
}
