using System;

namespace ImageGallery.Model
{
    public class Image
    {      
        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }
 
        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }
 
        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string Category { get; set; }
    }
}
