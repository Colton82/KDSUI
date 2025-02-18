using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDSUI.Services
{
    public static class SessionManager
    {
        // This is a static class that will hold the user ID
        public static int _userId { get; set; }
        public static string _username { get; set; }
    }
}
