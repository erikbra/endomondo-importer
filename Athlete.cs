using System;
using System.Text.Json.Serialization;

namespace endoimport
{
    public class Athlete
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        
        [JsonPropertyName("resource_state")]
        public int ResourceState { get; set; }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Sex { get; set; }
        public bool Premium { get; set; }
        public bool Summit { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonPropertyName("badge_type_id")]
        public int BadgeTypeId { get; set; }
        
        [JsonPropertyName("profile_medium")]
        public Uri ProfileMedium { get; set; }
        
        public Uri Profile { get; set; }
        public string Friend { get; set; }
        public string Follower { get; set; }
     
    }
}