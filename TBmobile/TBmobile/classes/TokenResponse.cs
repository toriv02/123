using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TBmobile.classes
{
    class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}
