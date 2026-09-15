using System.ComponentModel.DataAnnotations;

namespace E_Commerce.ViewModels
{
    public class EditCategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
