using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VkBot.Information.User.Models
{
    public class GeoPosition
    {
        [Key]
        public long UserId { get; set; }
        public string Place { get; set; }
        public string Commentary { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
