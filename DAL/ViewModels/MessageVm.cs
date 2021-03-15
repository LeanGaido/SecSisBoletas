using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.ViewModels
{
    public class MessageVm
    {
        public string Type { get; set; }

        public string Message { get; set; }

        public bool Dismissible { get; set; }
    }
}
