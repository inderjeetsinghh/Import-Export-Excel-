using Domain.Entity.EntityHelper;

namespace Domain.Entity
{
    public class ContractBasicInfo : GuidModelBase
    {
        public string? ContractName { get; set; }
        public string? ClientName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }      

        // Navigation property to represent the relationship with Labor Category
        public virtual ICollection<LaborCategory> LaborCategories { get; set; }
    }
}
