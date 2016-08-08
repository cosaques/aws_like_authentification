using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace VentePrivee.Client
{
    class Program
    {
        private const string ApiBaseAddress = "http://localhost:8080/api/test";
        private static readonly int[] _possibleInputs = { 1, 2, 3 };

        static void Main(string[] args)
        {
            MainMenu().Wait();
        }

        private static async Task MainMenu()
        {
            var exit = false;
            while (!exit)
            {
                Console.Write(@"Choose an API method : 
    1 - Authentificate(email, password) 
    2 - Confidentials(email)
    3 - Exit
Your choise : ");

                var input = Console.ReadLine();
                int result;
                if (!int.TryParse(input, out result) || !_possibleInputs.Contains(result))
                    continue;
                
                switch (result)
                {
                    case 1:
                        await CallAuthentificate();
                        break;
                    case 2:
                        await CallConfidentials();
                        break;
                    case 3:
                        exit = true;
                        break;
                }
            }
        }

        private static async Task CallConfidentials()
        {
            Console.Write("email : ");
            var email = Console.ReadLine();

            await
                CallApiMethod("confidentials", true, new Tuple<string, string>("email", email));
        }

        private static async Task CallAuthentificate()
        {
            Console.Write("email : ");
            var email = Console.ReadLine();
            Console.Write("password : ");
            var password = Console.ReadLine();

            await
                CallApiMethod("authentificate", false, new Tuple<string, string>("email", email),
                    new Tuple<string, string>("password", password));
        }

        static async Task CallApiMethod(string method, bool authentificated, params Tuple<string, string>[] parameters)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in parameters)
            {
                queryString[parameter.Item1] = parameter.Item2;
            }

            var uri = String.Format("{0}/{1}?{2}", ApiBaseAddress, method, queryString.ToString());
            Console.WriteLine("{0} call {1}", authentificated ? "Authentificated" : "Non authentificated", uri);

            var authentificatedHandler = new AuthentificatedHandler();
            var client = authentificated ? HttpClientFactory.Create(authentificatedHandler) : new HttpClient();

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var responseStr = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Successed: {0}. Response: {1}. Content-type: {2}", response.StatusCode, responseStr,
                    response.Content.Headers.ContentType.MediaType);
            }
            else
            {
                Console.WriteLine("Failed: {0}", response.StatusCode);
            }

            Console.WriteLine("Press ENTER to continue");
            Console.ReadLine();
        }
    
    }
}
