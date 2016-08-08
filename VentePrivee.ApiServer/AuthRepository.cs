using System.Collections.Generic;

namespace VentePrivee.ApiServer.Data
{
    public class AuthRepository
    {
        public static List<User> GetAllUsers()
        {
            return new List<User>
            {
                new User
                {
                    Email = "toto@gmail.com",
                    Password = "secret",
                    ApiKeyId = "AKIAIOSFODNN7EXAMPLE",
                    ApiSecretKey = "7M2AC/cP1fSvr2V55NRmntsQW1yA1RrdCs5pHCtIC/M="
                }
            };
        } 
    }
}
