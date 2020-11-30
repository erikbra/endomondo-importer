using System.IO;

namespace endoimport
{
    public class Activity
    {
        /// <summary>
        /// The file to upload
        /// </summary>
        public Stream File { get; set; }
        
        /// <summary>
        /// The desired name of the resulting activity.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The desired description of the resulting activity.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether the resulting activity should be marked as having been performed on a trainer. 
        /// </summary>
        public bool Trainer { get; set; }
        
        /// <summary>
        /// Whether the resulting activity should be tagged as a commute.
        /// </summary>
        public bool Commute { get; set; }
        
        /// <summary>
        /// The format of the uploaded file. May take one of the following values: fit, fit.gz, tcx, tcx.gz, gpx, gpx.gz
        /// </summary>
        public string DataType { get; set; }
        
        /// <summary>
        /// The desired external identifier of the resulting activity.
        /// </summary>
        public string ExternalId { get; set; }
    }
}