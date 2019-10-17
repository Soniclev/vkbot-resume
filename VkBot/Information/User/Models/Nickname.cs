using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace VkBot.Information.User.Models
{
    public class Nickname
    {
        [Key]
        public long UserId { get; set; }
        public string Nick { get; set; }

        private const int MaxNickLength = 30;
        private readonly string[] _stopSymbols = { "?", "!", ".", ",", "`", "~", "%", "^", ":", ";", "=", "+", "/", "\\", "'", "\"" };

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Nick))
                return false;

            if (Nick.All(x => !(Char.IsLetter(x) || Char.IsDigit(x))))
                return false;

            if (Nick.Length > MaxNickLength)
                return false;

            foreach (var stopSymbol in _stopSymbols)
            {
                if (Nick.Contains(stopSymbol))
                    return false;
            }

            return true;
        }
    }
}
