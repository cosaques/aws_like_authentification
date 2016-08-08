using System;
using System.Linq;
using System.Web.Http;
using VentePrivee.ApiServer.Data;

namespace VentePrivee.ApiServer.Controllers
{
    public class TestController: ApiController
    {
        [HttpGet]
        public bool Authentificate(string email, string password)
        {
            var user = AuthRepository.GetAllUsers().FirstOrDefault(u => u.Email == email && u.Password == password);

            return user != null;
        }

        [HttpGet]
        [AwsAuthentification]
        public bool Confidentials(string email)
        {
            var authApiId = RequestContext.Principal.Identity.Name;
            var user = AuthRepository.GetAllUsers().FirstOrDefault(u => u.ApiKeyId == authApiId);

            return user != null && email.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
