using System;
using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class ProductReview : IEntity
    {
        public int Id { get; set; }

        public Product Product { get; set; }

        public User User { get; set; }

        [Required(ErrorMessage ="A rating is required.")]
        public int Rating { get; set; }

        [Display(Name = "Comment Title")]
        [Required(ErrorMessage = " The field Message Title is required")]
        public string ReviewTitle { get; set; }

        [Display(Name ="Comment")]
        [Required(ErrorMessage = "The field Message is required.")]
        public string ReviewText { get; set; }

        public DateTime ReviewDate { get; set; }

        public bool WouldRecommend { get; set; }

        public string Status { get; set; }

    }
}
