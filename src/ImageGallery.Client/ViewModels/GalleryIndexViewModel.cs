using ImageGallery.Model;
using System.Collections.Generic;

namespace ImageGallery.Client.ViewModels
{
    public class GalleryIndexViewModel
    {
        public IEnumerable<Image> Images { get; private set; }

        public string ImagesUri { get; private set; }

        public GalleryIndexViewModel(List<Image> images, string imagesUri)
        {
           Images = images;
           ImagesUri = imagesUri;
        }
    }
}
