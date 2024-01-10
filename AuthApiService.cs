using System.Text;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthApiResponse> AuthenticateUser(string username, string email, string password)
    {
        var requestContent = new StringContent(
            JsonConvert.SerializeObject(new
            {
                UserName = username,
                Email = email,
                Password = password
            }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("https://web.socem.plymouth.ac.uk/COMP2001/auth/api/user", requestContent);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthApiResponse>(responseBody);
        }

        return null; // or handle the error as needed
    }
}

public class AuthApiResponse
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    // Include other properties as needed
}
