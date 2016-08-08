using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using VentePrivee.ApiServer.Data;

namespace VentePrivee.ApiServer
{
    public class AwsAuthentificationAttribute : Attribute, IAuthenticationFilter
    {
        public const  string AuthenticationScheme = "AWS";

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;

            if (request.Headers.Authorization != null &&
                AuthenticationScheme.Equals(request.Headers.Authorization.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                var authParameter = request.Headers.Authorization.Parameter;

                string appId, signature;
                if (GetHeaderValues(authParameter, out appId, out signature))
                {
                    // get user
                    var user = AuthRepository.GetAllUsers().FirstOrDefault(u => u.ApiKeyId == appId);
                    if (user == null)
                    {
                        context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                    }
                    else
                    {
                        var correctSignature = CalculateSignature(request, user);

                        // compare the signatures
                        if (signature == correctSignature.Result)
                        {
                            var currentPrincipal = new GenericPrincipal(new GenericIdentity(appId), null);
                            context.Principal = currentPrincipal;
                        }
                        else
                        {
                            context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], request);
                        }
                    }
                }
                else
                {
                    context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], request);
                }
            }
            else
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], request);
            }

            return Task.FromResult(0);
        }

        public static async Task<string> CalculateSignature(HttpRequestMessage request, User user)
        {
            // HTTP - Verb
            var httpVerb = request.Method.Method;

            // Content-MD5
            var contentMd5 = string.Empty;
            if (request.Content != null)
            {
                var content = await request.Content.ReadAsByteArrayAsync();
                if (content.Length != 0)
                {
                    var md5 = MD5.Create();
                    var hash = md5.ComputeHash(content);
                    contentMd5 = Convert.ToBase64String(hash);
                }
            }

            // Resource
            var resource = WebUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());

            // Calculating signature
            string signatureHashBase64;
            var signature = String.Format("{0}\n{1}\n{2}", httpVerb, contentMd5, resource);

            var apiKeyBytes = Convert.FromBase64String(user.ApiSecretKey);
            var signatureBytes = Encoding.UTF8.GetBytes(signature);

            using (var hmac = new HMACSHA256(apiKeyBytes))
            {
                var signatureHash = hmac.ComputeHash(signatureBytes);
                signatureHashBase64 = Convert.ToBase64String(signatureHash);
            }

            return signatureHashBase64;
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ChallengeResult(context.Result);
            return Task.FromResult(0);
        }

        public bool AllowMultiple
        {
            get { return false; }
        }

        private bool GetHeaderValues(string parameter, out string apiId, out string signature)
        {
            var values = parameter.Split(':');

            if (values.Length == 2)
            {
                apiId = values[0];
                signature = values[1];
                return true;
            }

            apiId = String.Empty; signature = String.Empty;
            return false;
        }
    }

    public class ChallengeResult : IHttpActionResult
    {
        private readonly IHttpActionResult next;

        public ChallengeResult(IHttpActionResult next)
        {
            this.next = next;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await next.ExecuteAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(AwsAuthentificationAttribute.AuthenticationScheme));
            }

            return response;
        }
    }
}
