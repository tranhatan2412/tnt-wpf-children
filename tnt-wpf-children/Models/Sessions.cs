using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tnt_wpf_children.Models
{
    public class Sessions
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string RelativeId { get; set; }
        public Relatives Relative { get; set; }

        public DateTime CheckinTime { get; set; } = DateTime.UtcNow;

        public DateTime? CheckoutTime { get; set; }

        public bool Status { get; set; } = true;
    }
}
