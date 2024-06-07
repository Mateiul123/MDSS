using Google.Cloud.Speech.V1;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace drc.Services
{
    public class SpeechToTextService
    {
        private readonly SpeechClient _speechClient;
        private readonly GoogleCloudSettings _settings;

        public SpeechToTextService(IOptions<GoogleCloudSettings> settings)
        {
            _settings = settings.Value;
            var builder = new SpeechClientBuilder
            {
                CredentialsPath = _settings.JsonKeyFilePath
            };
            _speechClient = builder.Build();
        }

        public async Task<string> ConvertSpeechToText(string audioFilePath)
        {
            var audio = await File.ReadAllBytesAsync(audioFilePath);
            var response = await _speechClient.RecognizeAsync(new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Mp3,
                SampleRateHertz = 16000,
                LanguageCode = "en"
            }, RecognitionAudio.FromBytes(audio));

            return string.Join(" ", response.Results.SelectMany(result => result.Alternatives).Select(alternative => alternative.Transcript));
        }
    }
}
