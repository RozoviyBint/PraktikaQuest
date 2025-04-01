using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PraktikaQuest.Services;

namespace PraktikaQuest.Controllers
{
    
    
    [ApiController]
    [Route("api")]
    public class AdminController : Controller
    {
        // GET /api/story - Получить весь сюжет
        [HttpGet("story")]
        public async Task<IActionResult> GetAllStory([FromServices] AdminService adminService)
        {
            var blocks = await adminService.GetAllSceneBlocks();
            return Ok(blocks);
        }

        [HttpPost("createuser")]

        public async Task<IActionResult> CreateUser([FromServices] AdminService adminService, [FromBody] User user)
        {
            await adminService.CreateUser(user);
            return Ok();
        }

        // POST /api/block - Добавить новый блок
        [HttpPost("block")]
        public async Task<IActionResult> AddSceneBlock([FromServices] AdminService adminService, [FromBody] SceneBlock block)
        {
            await adminService.AddSceneBlock(block);
            return Ok();
        }

        // PUT /api/block/{id} - Редактировать блок
        [HttpPut("block/{id}")]
        public async Task<IActionResult> UpdateSceneBlock(
            [FromServices] AdminService adminService,
            int id,
            [FromBody] SceneBlock updatedBlock)
        {
            var block = await adminService.GetSceneBlockById(id);
            if (block == null) return NotFound();

            block.Description = updatedBlock.Description;
            block.BackgroundImage = updatedBlock.BackgroundImage;
            block.Routes = updatedBlock.Routes;
            block.Event = updatedBlock.Event;

            await adminService.DeleteSceneBlock(id);
            await adminService.AddSceneBlock(block);
            return Ok();
        }

        // DELETE /api/block/{id} - Удалить блок
        [HttpDelete("block/{id}")]
        public async Task<IActionResult> DeleteSceneBlock([FromServices] AdminService adminService, int id)
        {
            var result = await adminService.DeleteSceneBlock(id);
            return result ? Ok() : NotFound();
        }

        // POST /api/block/{id}/route - Добавить переход
        [HttpPost("block/{id}/route")]
        public async Task<IActionResult> AddRoute(
            [FromServices] AdminService adminService,
            int id,
            [FromBody] SceneRoute route)
        {
            var block = await adminService.GetSceneBlockById(id);
            if (block == null) return NotFound();

            block.Routes.Add(route);
            await adminService.DeleteSceneBlock(id);
            await adminService.AddSceneBlock(block);
            return Ok();
        }

        // POST /api/block/{id}/event - Добавить ключевое событие
        [HttpPost("block/{id}/event")]
        public async Task<IActionResult> AddKeyEvent(
            [FromServices] AdminService adminService,
            int id,
            [FromBody] KeyEvent keyEvent)
        {
            var block = await adminService.GetSceneBlockById(id);
            if (block == null) return NotFound();

            await adminService.AddKeyEvent(keyEvent);
            block.Event = keyEvent;

            await adminService.DeleteSceneBlock(id);
            await adminService.AddSceneBlock(block);
            return Ok();
        }

        // PUT /api/event/{id} - Редактировать событие
        [HttpPut("event/{id}")]
        public async Task<IActionResult> UpdateEvent(
            [FromServices] AdminService adminService,
            string id,
            [FromBody] KeyEvent updatedEvent)
        {
            var existingEvent = await adminService.GetKeyEventById(id);
            if (existingEvent == null) return NotFound();

            await adminService.AddKeyEvent(updatedEvent);
            return Ok();
        }

        // DELETE /api/event/{id} - Удалить событие
        [HttpDelete("event/{id}")]
        public async Task<IActionResult> DeleteEvent([FromServices] AdminService adminService, string id)
        {
            var deleted = await adminService._keyEvents.DeleteDocument(ObjectId.Parse(id));
            return deleted ? Ok() : NotFound();
        }

        // GET /api/block/{id} - Получить конкретный блок
        [HttpGet("block/{id}")]
        public async Task<IActionResult> GetSceneBlockById([FromServices] AdminService adminService, int id)
        {
            var block = await adminService.GetSceneBlockById(id);
            return block == null ? NotFound() : Ok(block);
        }
        [HttpPost("item")]
        public async Task<IActionResult> AddItem([FromServices] AdminService adminService, [FromBody] Item item)
        {
            if (item == null)
            {
                return BadRequest("Invalid item data.");
            }

            try
            {
                await adminService.AddItem(item); // Вызов метода из сервиса для добавления предмета
                return Ok("Item added successfully."); // Возвращаем успешный ответ
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
