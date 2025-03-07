using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LehmanCustomConstruction.Data.Homes
{
    public class Portfolio
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "A project name is required")]
        public string? ProjectName { get; set; }

        [Required(ErrorMessage = "A description is required")]
        public string? Description { get; set; }

        public string? MainImageUrl { get; set; } // Main image of the project

        public DateTime CompletionDate { get; set; }

        // Foreign key to Styles
        [ForeignKey("Style")]
        public int StyleID { get; set; }

        public Styles? Style { get; set; } // Navigation property

        public string? FloorPlanImageUrl { get; set; }

        public string? Address { get; set; }

        public string? ClientName { get; set; }

        public decimal? ProjectCost { get; set; }
    }
}