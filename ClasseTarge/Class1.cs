using System.Text.Json;

namespace ClasseTarge
{
    public class Platerecognizer
    {
        public record Root(List<results> results);

        public record results(string plate);


        private static readonly string apiKey = "a85e55203502e69ba97f60268c5d40342a04cae6";
        private static readonly string apiUrl = "https://api.platerecognizer.com/v1/plate-reader/";

        public static async Task<string> RecognizeLicensePlate(string imagePath)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Token {apiKey}");

                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(imagePath)), "upload", "image.jpg");

                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Root Plate = JsonSerializer.Deserialize<Root>(responseContent)!;
                    return Plate.results[0].plate;
                }
                else
                {
                    return "Errore: " + response.ReasonPhrase;
                }
            }
        }



    }
}
