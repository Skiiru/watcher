using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace History
{
    class URL
    {
        public string title;
        public string url;
        public DateTime time;
        public string browser;
        public URL(string u, string t,DateTime d,string b)
        {
            this.url = u;
            this.title = t;
            this.time = d;
            this.browser = b;
        }
    }
}
