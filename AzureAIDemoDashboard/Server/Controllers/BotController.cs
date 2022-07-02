namespace AzureAIDemoDashboard.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class BotController : ControllerBase
{
    private readonly BotClient _bot;

    public BotController(BotClient bot)
    {
        _bot = bot;
    }

    [HttpPost]
    [Route("{action}")]
    public async Task<IActionResult> SendMessage([FromBody] BotRequest request)
    {
        var responses = await _bot.SendMessage(request);
        return Ok(responses);
    }
}