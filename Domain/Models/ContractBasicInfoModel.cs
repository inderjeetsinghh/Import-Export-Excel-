using Domain.Entity.EntityHelper;

namespace Domain.Models
{
    public class ContractBasicInfoModel : GuidModelBase
    {
        public string? ContractName { get; set; }
        public string? ClientName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Navigation property to represent the relationship with Labor Category
        public List<LaborCategoryModel> LaborCategories { get; set; }
    }
}
