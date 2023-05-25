using Domain.Entity;
using Domain.Entity.EntityHelper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class LaborCategoryModel : GuidModelBase
    {
        public string? CategoryName { get; set; }
        public decimal RatePerHour { get; set; }
        public string ContractId { get; set; }
    }
}
