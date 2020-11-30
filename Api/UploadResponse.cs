using System.Text.Json.Serialization;

namespace endoimport
{
    public class UploadResponse
    {
        [JsonPropertyName("id_str")]
        public string IdStr { get; set; }
        
        [JsonPropertyName("activity_id")]
        public string ActivityId { get; set; }
        
        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }
        
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("error")]
        public string Error { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; }
        
    }
}