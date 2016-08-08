namespace VentePrivee.ApiServer.Data
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ApiKeyId { get; set; }
        public string ApiSecretKey { get; set; }
    }
}
