using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VentePrivee.ApiServer;
using VentePrivee.ApiServer.Data;

namespace VentePrivee.Client
{
    public class AuthentificatedHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Getting the user with api key id and api key secret
            var user = AuthRepository.GetAllUsers().First();

            // Calculating signature
            var signature = await AwsAuthentificationAttribute.CalculateSignature(request, user);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(AwsAuthentificationAttribute.AuthenticationScheme,
                    string.Format("{0}:{1}", user.ApiKeyId, signature));
            return await base.SendAsync(request, cancellationToken);
        }
    }
}