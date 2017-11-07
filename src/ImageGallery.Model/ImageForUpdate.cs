using System.ComponentModel.DataAnnotations;

namespace ImageGallery.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageForUpdate
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }


        [Required]
        [MaxLength(50)]
        public string Category { get; set; }

    }
}
