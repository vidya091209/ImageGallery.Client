using System.Collections.Generic;

namespace ImageGallery.Client.ViewModels.Diagnostics
{
    /// <summary>
    /// 
    /// </summary>
    public class DiagnosticCheckCollection
    {
        /// <summary>
        /// 
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? Passed { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DiagnosticCheckResult> Results { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Notes { get; set; }

    }
}
