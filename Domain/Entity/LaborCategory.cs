using Domain.Entity.EntityHelper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entity
{
    public class LaborCategory : GuidModelBase
    {
        public string? CategoryName { get; set; }
        public decimal RatePerHour { get; set; }

        // Foreign key property to represent the relationship with Contract Basic Info
        [Required]
        public string ContractId { get; set; }
        [ForeignKey(nameof(ContractId))]
        public virtual ContractBasicInfo Contract { get; set; }
    }
}
