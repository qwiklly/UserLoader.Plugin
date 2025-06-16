using Newtonsoft.Json;
using System.Collections.Generic;
using UserLoader.Plugin.Models;

namespace UserLoader.Plugin.Responses
{
    public class UserResponse
    {
        [JsonProperty("users")]
        public List<DummyUser> Users { get; set; }
    }
}
