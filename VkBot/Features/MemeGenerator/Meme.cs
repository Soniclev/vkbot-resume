using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Features.MemeGenerator
{
    public class Meme
    {
        public Meme()
        {
        }

        public Meme(string name, string configPath)
        {
            Name = name;
            ConfigPath = configPath;
        }

        public string Name { get; set; }
        public string ConfigPath { get; set; }
    }
}
