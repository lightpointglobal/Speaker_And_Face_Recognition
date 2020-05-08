using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechAndFaceRecognizerWebCore.Models
{
    public class HeaderViewModel
    {
        public bool UserIsAuthenticated { get; set; }

        public string UserName { get; set; }
    }
}
