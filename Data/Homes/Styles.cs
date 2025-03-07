using System.ComponentModel.DataAnnotations;

namespace LehmanCustomConstruction.Data.Homes
{
    public class Styles
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "A title is required")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "A description is required")]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; } = 0; 

        public bool IsActive { get; set; } = true; 

        public string? Slug { get; set; }
    }
}