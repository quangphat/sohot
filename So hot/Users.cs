using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace So_hot
{
    public class Users
    {
        public string Name { get; set; }
        public bool Type { get; set; }
        public string DbPath { get; set; }
        public string ImgFolder { get; set; }
        public Users()
        {
            Name = "ClarkKent";
            Type = false;
            DbPath = string.Empty;
            ImgFolder = string.Empty;
        }
    }
}
