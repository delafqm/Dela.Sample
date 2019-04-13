namespace Dela.Sample.WebMvc
{
    internal class TokenResult
    {
        public string access_token { set; get; }
        public int expires_in { set; get; }
        public string token_type { set; get; }
    }
}