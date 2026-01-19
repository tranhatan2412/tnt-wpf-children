using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tnt_wpf_children.Models
{
    public class Relatives
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }
        public byte[]? FaceEmbedding { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
        public bool Status { get; set; } = true;

        public ICollection<Sessions> Sessions { get; set; }

        public override string ToString()
        {
            return $"{FullName} - {PhoneNumber}";
        }
    }
}
