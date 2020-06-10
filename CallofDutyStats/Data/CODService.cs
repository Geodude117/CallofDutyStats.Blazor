using CallofDutyStats.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CallofDutyStats.Data
{
    public class CODService
    {
        private string _codLoginUrl;
        private string _codBaseUrl;
        private string _codGetPlayerUrl;

        private string _email;
        private string _password;

        public CODService(IConfiguration configuration)
        {
            _codLoginUrl = configuration.GetSection("CODLoginURL").Value;
            _codBaseUrl = configuration.GetSection("CODBaseURL").Value;
            _codGetPlayerUrl = configuration.GetSection("CODGetPlayerUrl").Value;

            _email = configuration.GetSection("Email").Value;
            _password = configuration.GetSection("Password").Value;

        }

        public async Task<string> GetAsync(LoginModel requestModel)
        {

            string csrfValue;
            var cookieContainer = new CookieContainer();

            using (var httpClient = new HttpClient())
            {

                var response = httpClient.GetStringAsync(new Uri(_codLoginUrl)).Result;

                Regex regex = new Regex(@"<\s*meta[^>]*>(.*?)");
                var match = regex.Matches(response);
                csrfValue = match[9].Value;

                csrfValue = csrfValue.Replace("<meta name=", "");
                csrfValue = csrfValue.Replace("_csrf", "");
                csrfValue = csrfValue.Replace("\"", "");
                csrfValue = csrfValue.Replace(" content=", "");
                csrfValue = csrfValue.Replace("/>", "");

            }

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(_codBaseUrl) })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("username", _email),
                        new KeyValuePair<string, string>("password", _password),
                        new KeyValuePair<string, string>("_csrf", csrfValue),
                        new KeyValuePair<string, string>("remember_me", "true"),

                 });

                cookieContainer.Add(new Uri(_codBaseUrl), new Cookie("XSRF-TOKEN", csrfValue));
                cookieContainer.Add(new Uri(_codBaseUrl), new Cookie("new_SiteId", "cod"));
                cookieContainer.Add(new Uri(_codBaseUrl), new Cookie("comid", "cod"));

                HttpResponseMessage result = await client.PostAsync("/do_login?new_SiteId=cod", content);

                if (result.IsSuccessStatusCode)
                {
                    var t = result.Content.ReadAsStringAsync().Result;

                    IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(new Uri(_codBaseUrl)).Cast<Cookie>();
                }

            }

            string paramaterziedGetPlayeInfoUrl = String.Format(_codGetPlayerUrl, "mw", "psn", requestModel.UserID); /* "https://my.callofduty.com/api/papi-client/stats/cod/v1/title/mw/platform/psn/gamer/geo_dude117/profile/type/mp";*/

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(_codBaseUrl) })
            {

                var response = client.GetStringAsync(new Uri(paramaterziedGetPlayeInfoUrl)).Result;
                return response;
            }
        }
    }
}
