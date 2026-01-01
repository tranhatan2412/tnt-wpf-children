using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tnt_wpf_children.Models
{
    public class Children
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string FullName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RelativesChildren> RelativesChildren { get; set; }
    }
}
