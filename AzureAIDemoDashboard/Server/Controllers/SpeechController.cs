using System.ComponentModel.DataAnnotations;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AzureAIDemoDashboard.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SpeechController : ControllerBase
{
    private readonly ILogger<SpeechController> _logger;
    private readonly Dictionary<string, SpeechConfiguration> _configuration = new();

    private class SpeechConfiguration
    {
        [Required]
        public string SubscriptionKey { get; set;} = "";
        
        [Required]
        public string Region { get; set;} = "";
        
        [Required]
        public string Language { get; set;} = "";
        
        [Required]
        public string Voice { get; set;} = "";
    }

    public SpeechController(
        ILogger<SpeechController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        configuration.GetSection("SpeechConfiguration").Bind(_configuration);
    }

    [HttpPost()]
    [Route("{action}")]
    public async Task<IActionResult> RecognizeAudio(IFormFile file, [FromQuery] string locale)
    {
        await using var stream = file.OpenReadStream();
        var speechConfig = GetSpeechConfigFromLocale(locale);
        var audioStreamFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1); // Audio format we expect from the Client
        using var audioConfig = AudioConfig.FromStreamInput(new GenericInputAudioStream(stream), audioStreamFormat);
        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
        var result = await recognizer.RecognizeOnceAsync();
        _logger.LogInformation("{result}: {text}", result.Reason, result.Text);

        var speechResult = new SpeechResult { Text = result.Text };
        return Ok(speechResult);
    }

    [HttpPost()]
    [Route("{action}")]
    public async Task<IActionResult> SynthesizeText(SynthesisRequest request)
    {
        var stream = new MemoryStream();
        var speechConfig = GetSpeechConfigFromLocale(request.Locale);
        using var audioConfig = AudioConfig.FromStreamOutput(new GenericOutputAudioStream(stream));
        using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
        await synthesizer.SpeakTextAsync(request.Text);
        stream.Position = 0;
        return File(stream, "audio/mpeg");
    }
    
    private SpeechConfig GetSpeechConfigFromLocale(string locale)
    {
        var config = _configuration[locale];
        var speechConfig = SpeechConfig.FromSubscription(config.SubscriptionKey,config.Region);
        speechConfig.SpeechRecognitionLanguage = config.Language;
        speechConfig.SpeechSynthesisLanguage = config.Language;
        speechConfig.SpeechSynthesisVoiceName = config.Voice;
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3);
        return speechConfig;
    }
}

public class GenericOutputAudioStream : PushAudioOutputStreamCallback
{
    private readonly Stream _stream;

    public GenericOutputAudioStream(Stream stream)
    {
        _stream = stream;
    }

    public override uint Write(byte[] dataBuffer)
    {
        _stream.Write(dataBuffer);
        return (uint)dataBuffer.Length;
    }
}

public class GenericInputAudioStream : PullAudioInputStreamCallback
{
    private readonly Stream _stream;

    public GenericInputAudioStream(Stream stream)
    {
        _stream = stream;
    }

    public override int Read(byte[] buffer, uint size)
    {
        return _stream.Read(buffer, 0, (int)size);
    }
}