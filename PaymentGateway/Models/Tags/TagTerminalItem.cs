using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Models.Tags
{
    public class TagTerminalSelector
    {
        public int TerminalId { get; set; }

        public List<TagTerminalItem> TagTerminalItems { get; set; }
    }

    public class TagTerminalItem
    {
        public int TagId { get; set; }
        public string TagName { get; set; }
        public bool IsSelected { get; set; }
    }
}
