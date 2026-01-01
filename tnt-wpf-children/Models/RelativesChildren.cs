using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tnt_wpf_children.Models
{
    public class RelativesChildren
    {
        public int Id { get; set; }

        public string RelativeId { get; set; }
        public Relatives Relative { get; set; }

        public string ChildId { get; set; }
        public Children Child { get; set; }
    }
}
