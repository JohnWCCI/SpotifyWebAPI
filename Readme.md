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

  ## Step 4. Building the Account Services
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


## Step 5. Building the Spotify Service
In the **Models** Folder add the class **Release**

     public class Release
        {
            public string Name { get; set; } = null!;
            public string Artists { get; set; } = null!;
            public string Date { get; set; } = null!;
            public string ImageUrl { get; set; } = null!;
            public string Link { get; set; } = null!;
        }
This class will contain the data from Spotify

In the **Models** Folder add the following class **GetNewReleaseResult**
This class will be used to translate the Json stream from spotify into a class.

      public class GetNewReleaseResult
        {
            public Albums? albums { get; set; }
        }
    
        public class Albums
        {
            public string? href { get; set; }
            public Item[]? items { get; set; }
            public int limit { get; set; }
            public string? next { get; set; }
            public int offset { get; set; }
            public object? previous { get; set; }
            public int total { get; set; }
        }
    
        public class Item
        {
            public string? album_type { get; set; }
            public Artist[]? artists { get; set; }
            public object[]? available_markets { get; set; }
            public External_Urls? external_urls { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public Image[]? images { get; set; }
            public string? name { get; set; }
            public string? release_date { get; set; }
            public string? release_date_precision { get; set; }
            public int total_tracks { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
        }
    
        public class External_Urls
        {
            public string? spotify { get; set; }
        }
    
        public class Artist
        {
            public External_Urls1? external_urls { get; set; }
            public string? href { get; set; }
            public string? id { get; set; }
            public string? name { get; set; }
            public string? type { get; set; }
            public string? uri { get; set; }
        }
    
        public class External_Urls1
        {
            public string? spotify { get; set; }
        }
    
        public class Image
        {
            public int height { get; set; }
            public string? url { get; set; }
            public int width { get; set; }
        }
    
    }

In the **Services** Folder add the interface **ISpotifyService**
 

    public interface ISpotifyService
        {
            Task<IEnumerable<Release>> GetNewReleases(string countryCode, int limit, string accessToken);
        }
In the **Services** Folder add  the class **SpotifyService**

Setup the constructor 

     private readonly HttpClient _httpClient;

        public SpotifyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
In the method  `GetNewReleases`

    public async Task<IEnumerable<Release>> GetNewReleases(string countryCode, int limit, string accessToken)

Setup the get request with the Authentication Headers

     _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
     
   Issue the Get Request to Shopify.

     var response = await _httpClient.GetAsync($"browse/new-releases?country={countryCode}&limit={limit}");
     
Check the Response for success

     response.EnsureSuccessStatusCode();
Get the Json content Stream from the response

     using var responseStream = await response.Content.ReadAsStreamAsync();
Deserialize the Json into the **GetNewReleaseResult** class

     var responseObject = await JsonSerializer.DeserializeAsync<GetNewReleaseResult>(responseStream);

Translate the GetNewReleaseResult into the Release class

 

      return responseObject?.albums?.items.Select(i => new Release
                {
                    Name = i.name,
                    Date = i.release_date,
                    ImageUrl = i.images.FirstOrDefault().url,
                    Link = i.external_urls.spotify,
                    Artists = string.Join(",", i.artists.Select(i => i.name))
                });

## Step 6. Adding the HttpClient to the DI

In the **Progam.cs** add the following code after  `builder.Services.Configure` line.

    builder.Services.AddHttpClient<ISpotifyAccountService, SpotifyAccountService>(c =>
    {
        c.BaseAddress = new Uri("https://accounts.spotify.com/api/");
    });
    
    builder.Services.AddHttpClient<ISpotifyService, SpotifyService>(c =>
    {
        c.BaseAddress = new Uri("https://api.spotify.com/v1/");
        c.DefaultRequestHeaders.Add("Accept", "application/.json");
    });

These 2 DI services will take care of the HTTP connection

## Step 7. Building the Display
In the **HomeController** add the following code

      private async Task<string> GetToken()
      {
          return await _spotifyAccountService.GetToken(_keyValues.Clientid, _keyValues.ClientSecret);
      }
Modify the the Index  method

    public async Task<IActionResult> Index()
    {
        var token =await  GetToken();
        var newReleases = await _spotifyService.GetNewReleases("US", 20, token);
         return View(newReleases);
     } 

In the **Index** View for the **HomeController** modify the code 

    @model IEnumerable<SpotifyWebAPI.Models.Release>
    
    @{
        ViewData["Title"] = "Home Page";
    }
    
    <h1>Welcome to Spotify</h1>
    
    <div class="row">
        @if (Model.Count() > 0)
        {
            @foreach (var item in Model)
            {
                <div class="card m-1" style="width:350px;">
                    <div class="card-body">
                        <h5 class="card-title">@Html.DisplayFor(modelItem => item.Artists)</h5>
                        <h6 class="card-subtitle mb-2 text-muted">@Html.DisplayFor(modelItem => item.Name)</h6>
                        <p class="card-text">@Html.DisplayFor(modelItem => item.Date)</p>
                        <img src="@Html.DisplayFor(modelItem => item.ImageUrl)" width="300" height="300" alt="Release picture" />
                        <a href="@Html.DisplayFor(modelItem => item.Link)" target="_blank">Listen</a>
                    </div>
                </div>
            }
        }
        else
        {
            <p>No data available</p>
        }
    </div>
