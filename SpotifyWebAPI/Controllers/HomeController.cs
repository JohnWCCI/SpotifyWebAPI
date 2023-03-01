using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpotifyWebAPI.Models;
using SpotifyWebAPI.Services;
using System.Diagnostics;

namespace SpotifyWebAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISpotifyAccountService _spotifyAccountService;
        private readonly ISpotifyService _spotifyService;
        private readonly KeyValues _keyValues;
        public HomeController(ILogger<HomeController> logger, 
            IOptions<KeyValues> options,
            ISpotifyAccountService spotifyAccountService,
            ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;  
            _spotifyAccountService = spotifyAccountService;
            _keyValues = options.Value;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var token =await  GetToken();
            var newReleases = await _spotifyService.GetNewReleases("US", 20, token);
            return View(newReleases);
        }

        private async Task<string> GetToken()
        {
            return await _spotifyAccountService.GetToken(_keyValues.Clientid, _keyValues.ClientSecret);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}