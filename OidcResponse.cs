namespace endoimport
{
    public class OidcResponse
    {
        public string token_type { get; set; }
        public string id_token { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public int expires_at { get; set; }
        
        public Athlete athlete { get; set; }
    }
}