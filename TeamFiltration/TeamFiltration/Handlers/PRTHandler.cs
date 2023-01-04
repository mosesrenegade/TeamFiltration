using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace TeamFiltration.Handlers
{
    /*
     * Credit to Dirkjan for this portion
     * https://github.com/dirkjanm/ROADtoken/blob/master/Program.cs
     * 
     * 
     * */

    public class PRTHandler
    {
        public GlobalArgumentsHandler _globalPropertiesHandler { get; set; }
        public PRTHandler(GlobalArgumentsHandler globalProperties)
        {
            _globalPropertiesHandler = globalProperties;
        }

  


        public void GetCookie(string cookie)
        {

            JwtSecurityTokenHandler jwsSecHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecToken = jwsSecHandler.ReadJwtToken(cookie);

            jwtSecToken.Payload.TryGetValue("request_nonce", out var request_nonce);


            var ses = new System.Net.Http.HttpClient();
            ses.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 10.0; Win64; x64; Trident/7.0; .NET4.0C; .NET4.0E)");
            ses.DefaultRequestHeaders.Add("UA-CPU", "AMD64");

            var request = new HttpRequestMessage(HttpMethod.Get, "https://login.microsoftonline.com/Common/oauth2/authorize");
            request.Headers.Add("x-ms-RefreshTokenCredential", cookie);

            var queryParams = new Dictionary<string, string>
            {
                { "resource", "https://graph.microsoft.com/" },
                { "client_id", "1fec8e78-bce4-4aaf-ab1b-5451cc387264" },
                { "response_type", "code" },
                { "haschrome", "1" },
                { "redirect_uri", "https://login.microsoftonline.com/common/oauth2/nativeclient" },
                { "client-request-id", Guid.NewGuid().ToString() },
                { "x-client-SKU", "PCL.Desktop" },
                { "x-client-Ver", "3.19.7.16602" },
                { "x-client-CPU", "x64" },
                { "x-client-OS", "Microsoft Windows NT 10.0.19569.0" },
                { "site_id", "501358" },
                { "sso_nonce", (string)request_nonce },
                { "mscrid", Guid.NewGuid().ToString() }
            };

            request.RequestUri = new Uri(request.RequestUri.ToString() + "?" + string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}")));

            var response = ses.SendAsync(request).Result;

            var refreshClient = new HttpClient(httpClientHandler);

            var loginPostBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", "CODE"),
                new KeyValuePair<string, string>("client_id",  "1fec8e78-bce4-4aaf-ab1b-5451cc387264"),
                new KeyValuePair<string, string>("resource", "https://outlook.office365.com"),

            });


            refreshClient.DefaultRequestHeaders.Add("Accept", "application/json");
            refreshClient.DefaultRequestHeaders.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 7.1.2; SM-G955F Build/NRD90M)");

            var httpResp = await refreshClient.PostAsync(_teamFiltrationConfig.GetBaseUrl(), loginPostBody);
            var contentResp = await httpResp.Content.ReadAsStringAsync();


        }

    }
}
