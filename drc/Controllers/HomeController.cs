using drc.Models;
using drc.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace drc.Controllers
{
    public class HomeController : Controller
    {
        private readonly HateSpeechService _hateSpeechService;
        private readonly SpeechToTextService _speechToTextService;

        public HomeController(HateSpeechService hateSpeechService, SpeechToTextService speechToTextService)
        {
            _hateSpeechService = hateSpeechService;
            _speechToTextService = speechToTextService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeText(string inputText)
        {
            var result = await _hateSpeechService.DetectHateSpeechAsync(inputText);

            ViewData["Result"] = result;
            ViewData["TextLength"] = inputText.Length;
            ViewData["WordCount"] = inputText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            ViewData["CharacterCount"] = inputText.Replace(" ", "").Length;
            ViewData["MostFrequentWord"] = GetMostFrequentWord(inputText);

            return View("Index");
        }

        [HttpPost]
        public IActionResult WordCount(string inputText)
        {
            ViewData["TotalWordCount"] = inputText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            ViewData["UniqueWordCount"] = inputText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().Count();
            return View("Index");
        }

        private string GetMostFrequentWord(string text)
        {
            var words = text.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var wordCounts = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (wordCounts.ContainsKey(word))
                {
                    wordCounts[word]++;
                }
                else
                {
                    wordCounts[word] = 1;
                }
            }

            return wordCounts.OrderByDescending(w => w.Value).FirstOrDefault().Key;
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeAudio(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                return Content("Please select an audio file.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", audioFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            var text = await _speechToTextService.ConvertSpeechToText(filePath);
            var result = await _hateSpeechService.DetectHateSpeechAsync(text);

            ViewData["AudioResult"] = result;
            return View("Index");
        }

        public async Task<IActionResult> TestProvidedAudio()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ttsMP3.com_VoiceText_2024-6-6_22-54-45.mp3");
            var text = await _speechToTextService.ConvertSpeechToText(filePath);
            var result = await _hateSpeechService.DetectHateSpeechAsync(text);

            ViewData["AudioResult"] = result;
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
