using System.ComponentModel.DataAnnotations;

namespace Domain.Entity.EntityHelper
{
    public class GuidModelBase
    {
        [Key]
        [StringLength(36)]
        public string Id { get; set; }

        protected GuidModelBase()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
