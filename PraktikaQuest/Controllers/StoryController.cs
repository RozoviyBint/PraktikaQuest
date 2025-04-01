using Messenger.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Security.Claims;
using System.Threading.Tasks;

 
public class StoryController : Controller
{
    // Получить поддерево до ближайшего ключевого события
    [Authorize]
    [HttpGet("api/story/tree")]
    public async Task<IActionResult> GetTree(
        [FromServices] StoryService storyService,
        [FromServices] CollectionContext<GameState> gameStateContext,
        [FromServices] RegistrAuthService registrAuthService)
    {
        var playerId = registrAuthService.GetCurrentUserId();

        var gameState = await gameStateContext.FindByAnyFieldAsync("PlayerId", playerId);
        if (gameState == null)
            return NotFound("Игровое состояние не найдено");

        var tree = await storyService.GetSubTreeUntilKeyEvent(gameState.CurrentBlockId,gameState.VisitedBlocks);
        return Ok(tree);
    }
    [Authorize]
    [HttpGet("api/story/available-actions")]
    public async Task<IActionResult> GetAvailableActions(
        [FromQuery] int blockId,
        [FromServices] StoryService storyService,
        [FromServices] RegistrAuthService registrAuthService)
    {
        var _playerId = registrAuthService.GetCurrentUserId();
        var availableActions = await storyService.GetAvailableActions(_playerId, blockId);
        if (availableActions == null)
        {
            return Ok(0);
        }
        return Ok(availableActions);
    }
    [Authorize]
    [HttpPost("api/story/event-complete")]
    public async Task<IActionResult> CompleteEvent(
        [FromServices] StoryService storyService,
        [FromBody] EventCompletionDto dto,
        [FromServices] RegistrAuthService registrAuthService)
    {
        if (dto == null)
        {
            return BadRequest("Invalid request data.");
        }
        var _playerId = registrAuthService.GetCurrentUserId();
        var result = await storyService.EventComplete(dto, _playerId);
        return Ok(result);
    }
    [Authorize]
    [HttpPost("api/story/add-item")]
    public async Task<IActionResult> AddItemToInventory([FromServices] StoryService storyService, [FromQuery] string playerId, [FromQuery] int itemId)
    {
        if (string.IsNullOrEmpty(playerId) || itemId <= 0)
        {
            return BadRequest("Invalid playerId or itemId.");
        }

        try
        {
            await storyService.AddItemToInventory(playerId, itemId);
            return Ok("Item added to inventory successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // Для игры без api-префикса
    [Authorize]
    [HttpGet("game")]
    public async Task<IActionResult> Story()
    {
        return View();
    }
}
