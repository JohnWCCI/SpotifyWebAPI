# Consuming a Web API
## Step 1. Setup Spotify as a Developer

 1. Go to [Developer Spotify ](https://developer.spotify.com/)
 2. Login (or create a new user)
 3. Go to **Dashboard**
 4. **Create an app**

Save the **ClientID** and the **Client Secret** (The App Settings is a good place)

Use this later Authication....

     "Spotify": {
        "Clientid": "ID here",
        "ClientSecret": "Secret here"
      } 
## Step 2. Start the Application
 Build a New "ASP.NET Core Web App (Model-View-Controller)" (use .NET 7.0)
  Name it "SpotifyWebAPI"
## Step 3. Add Spotify Authentication Configuration
 Add authentication to the Appsettings.json

      {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "Spotify": {
        "Clientid": "ID here",
        "ClientSecret": "Secret here"
      }
    }

In the Models folder add a class "KeyValues"
Add 3 string properties 

 

    public class KeyValues
        {
            public static string Section = "Spotify";
            public string Clientid { get; set; } = null!;
            public string ClientSecret { get; set; } = null!;
        }

 In the Program.cs
    Add  the following code,this will add the Spotify Values to the DI that can be used.

    builder.Services.Configure<KeyValues>(builder.Configuration.GetSection(KeyValues.Section));
    right after the var builder is created

 In the HomeController add private fields

      private readonly KeyValues _keyValues;

      add to the Consturctor add the following parameter
      "IOptions<KeyValues> options"
      Then add with in the constructor block

       _keyValues = options.Value;

  ## Step 4. Building the services
In the Models folder
    Add AuthResult class

     public class AuthResult
    {
        public string access_token { get; set; } = null!;
        public string token_type { get; set; } = null!;
        public int expires_in { get; set; }
    }
    This will allow us to better translate the Json string returned by Spotify after requesting authentication token

  Add a folder "Services"

 add **ISpotifyAccountService** in the services folder 
 add the method to get the token. This will be done async

      Task<string> GetToken(string clientId, string clientSecret);
Add a class to Services Folder 
  
  SpotifyAccountService
  implement the interface ISpotifyAccountService

In the Constructor of the SpotifyAccountService
  

      private readonly HttpClient _httpClient;

    public SpotifyAccountService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

The HttpClient will be set by the HttpClientFactory in the DI


In the method:   **public async Task<string> GetToken(string clientId, string clientSecret)**

 
   Build a request message

      var request = new HttpRequestMessage(HttpMethod.Post, "token");`
        
   Build the request headers
   

     request.Headers.Authorization = new AuthenticationHeaderValue(
              "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));


   Builds the request content
  

       request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                 {
                     {"grant_type", "client_credentials"}
                 });

 
   Make the call to Spotify to request access token
  

     var response = await _httpClient.SendAsync(request);

 
   Ckeck the response
   

    response.EnsureSuccessStatusCode();
       
   Read the reponse stream, the using statement will ensure dispose will be called
  

     using var responseStream = await response.Content.ReadAsStreamAsync();

 
   Deserialize the reponse into AuthResult
  

     var authResult = await JsonSerializer.DeserializeAsync<AuthResult>(responseStream);

 
   Return the auth token
    

    return authResult.access_token;




