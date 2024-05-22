using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AccountAddressProgram
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // address, login и password нужно указать свои;
            var address = new Uri("http://localhost:1003/");
            var login = "Supervisor";
            var password = "Supervisor1";
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = address;
                var authResult = Authorize(client, login, password).Result;
                if (authResult)
                {
                    var bpmcsrf = cookies.GetCookies(address)["BPMCSRF"].Value;
                    client.DefaultRequestHeaders.Add("BPMCSRF", bpmcsrf);
                    var odataCount = GetAddressesCountWithOdata(client).Result;
                    Console.WriteLine("count with OData: " + odataCount);
                    var serviceCount = GetAddressesCountWithService(client).Result;
                    Console.WriteLine("count with Service: " + serviceCount);
                }
            }
        }

        static async Task<bool> Authorize(HttpClient client, string login, string password)
        {
            try
            {
                var loginData = new Dictionary<string, string>() { { "UserName", login }, { "UserPassword", password } };
                using var response = await client.PostAsJsonAsync("ServiceModel/AuthService.svc/Login", loginData);
                var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                var code = responseData["Code"].GetInt32();
                return code == 0;
            }
            catch (Exception) {
                return false;
            }
        }

        static async Task<int> GetAddressesCountWithOdata(HttpClient client)
        {
            try
            {
                using var response = await client.GetAsync("0/odata/AccountAddress?$filter=contains(Address,'А')&$count=true&$top=0");
                var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                var count = responseData["@odata.count"].GetInt32();
                return count;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        static async Task<int> GetAddressesCountWithService(HttpClient client)
        {
            try
            {
                using var response = await client.GetAsync("0/rest/UsrAccountAddressService/GetAddressesWithStringCount?str=А");
                var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                var count = responseData["GetAddressesWithStringCountResult"].GetInt32();
                return count;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}
