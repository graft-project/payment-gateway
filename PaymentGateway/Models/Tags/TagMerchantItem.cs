using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Models.Tags
{
    public class TagMerchantSelector
    {
        public int MerchantId { get; set; }

        public List<TagMerchantItem> TagMerchantItems { get; set; }
    }

    public class TagMerchantItem
    {
        public int TagId { get; set; }
        public string TagName { get; set; }
        public bool IsSelected { get; set; }
    }
}
