using MHA.ECLAIM.Entities.ViewModel.Claim;
using System.Text.Json.Serialization;

namespace MHA.ECLAIM.Entities.DTO
{
    public class APIRequestDTO<T>
    {
        [JsonPropertyName("data")]
        public T ?Data { get; set; }

        [JsonPropertyName("spHostUrl")]
        public string SpHostUrl { get; set; } = string.Empty;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("appAcessToken")]
        public string AppAccessToken { get; set; } = string.Empty;
    }
}
