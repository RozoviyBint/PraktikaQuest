using MongoDB.Bson;
using MongoDB.Driver;
using Messenger.Mongo;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class StoryService
{
    private readonly CollectionContext<GameState> _gameStateContext;
    private readonly CollectionContext<SceneBlock> _sceneBlockContext;
    private readonly CollectionContext<KeyEvent> _keyEventContext;
    private readonly CollectionContext<Inventory> _inventories;
    private readonly CollectionContext<Item> _items;

    public StoryService(
        CollectionContext<GameState> gameStateContext,
        CollectionContext<SceneBlock> sceneBlockContext,
        CollectionContext<KeyEvent> keyEventContext,
        CollectionContext<Inventory> inventories,
        CollectionContext<Item> items)
    {
        _gameStateContext = gameStateContext;
        _sceneBlockContext = sceneBlockContext;
        _keyEventContext = keyEventContext;
        _inventories = inventories;
        _items = items;
    }

    public async Task<SceneRenderDto> GetSubTreeUntilKeyEvent(int currentBlockId, List<int> visitedBlocks)
    {
        var result = new List<SceneBlockDto>();
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(currentBlockId);

        while (queue.Count > 0)
        {
            var blockId = queue.Dequeue();

            if (visited.Contains(blockId))
                continue;

            visited.Add(blockId);

            var block = await _sceneBlockContext.FindFirstAsync(blockId);
            if (block == null)
                continue;

            Console.WriteLine($"Обработан блок: {block.Id} | Описание: {block.Description}");

            var sceneDto = new SceneBlockDto
            {
                Id = block.Id,
                BackgroundImage = block.BackgroundImage,
                Description = block.Description,
                Routes = block.Routes.Select(r => new RouteDto
                {
                    TargetBlockId = r.TargetBlockId,
                    RouteDescription = r.RouteDescription
                }).ToList(),
                Event = block.Event != null
            };

            result.Add(sceneDto);

            if (block.Event?.EventType > 0)
            {
                Console.WriteLine($"Найдено ключевое событие в блоке: {block.Id}");
                continue;
            }

            foreach (var route in block.Routes)
            {
                Console.WriteLine($"Маршрут найден: {block.Id} -> {route.TargetBlockId} | {route.RouteDescription}");
                queue.Enqueue(route.TargetBlockId);
            }
        }

        return new SceneRenderDto { Scenes = result };
    }

    public async Task<List<ActionDto>> GetAvailableActions(ObjectId? playerId, int blockId)
    {
        var gameState = await _gameStateContext.FindByAnyFieldAsync("PlayerId", playerId);
        if (gameState == null)
            return new List<ActionDto>();

        if (gameState.CompletedEvents.Contains(blockId))
        {
            return null;
        }

        var block = await _sceneBlockContext.FindFirstAsync(blockId);
        if (block == null || block.Event == null)
            return new List<ActionDto>();

        var chosenActions = gameState.CompletedEvents_ChosenActions;
        var availableActions = new List<ActionDto>();

        foreach (var action in block.Event.Actions)
        {
            if (action.EventsActions == null || action.EventsActions.Count == 0)
            {
                availableActions.Add(new ActionDto
                {
                    Id = action.Id,
                    Description = action.Description,
                    RequiredItemId = action.RequiredItemId,
                    ResultBlockId = action.ResultBlockId
                });
                continue;
            }

            bool allConditionsMet = action.EventsActions.All(requiredEvent =>
                chosenActions.TryGetValue(requiredEvent.Key, out var chosenActionId) &&
                chosenActionId == requiredEvent.Value
            );

            if (allConditionsMet)
            {
                availableActions.Add(new ActionDto
                {
                    Id = action.Id,
                    Description = action.Description,
                    RequiredItemId = action.RequiredItemId,
                    ResultBlockId = action.ResultBlockId
                });
            }
        }

        return availableActions;
    }

    public async Task<SceneRenderDto> EventComplete(EventCompletionDto dto, ObjectId? playerId)
    {
        var gameState = await _gameStateContext.FindByAnyFieldAsync("PlayerId", playerId);
        var block = await _sceneBlockContext.FindFirstAsync(dto.BlockId);

        var uniqueBlocks = dto.NewBlocks.Where(block => !gameState.VisitedBlocks.Contains(block));
        var currentBlockId = block.Event.Actions[dto.ActionId].ResultBlockId;

        gameState.VisitedBlocks.AddRange(uniqueBlocks);
        gameState.CurrentBlockId = currentBlockId;
        gameState.CompletedEvents.Add(dto.BlockId);
        gameState.CompletedEvents_ChosenActions[block.Id.ToString()] = dto.ActionId;

        var filter = Builders<GameState>.Filter.Eq(g => g.Id, gameState.Id);
        var update = Builders<GameState>.Update
            .Set(g => g.VisitedBlocks, gameState.VisitedBlocks)
            .Set(g => g.CurrentBlockId, gameState.CurrentBlockId)
            .Set(g => g.CompletedEvents, gameState.CompletedEvents)
            .Set(g => g.CompletedEvents_ChosenActions, gameState.CompletedEvents_ChosenActions);

        await _gameStateContext.UpdateOneAsync(filter, update);
        var _gameState = await _gameStateContext.FindByAnyFieldAsync("PlayerId", playerId);

        return await GetSubTreeUntilKeyEvent(currentBlockId, gameState.VisitedBlocks);
    }

    public async Task AddItemToInventory(string playerId, int itemId)
    {
        var gameState = await _gameStateContext.FindByAnyFieldAsync("PlayerId", new ObjectId(playerId));
        var item = await _items.FindFirstAsync(itemId);

        gameState.Inventory.Items.Add(item);

        var filter = Builders<GameState>.Filter.Eq(g => g.Id, gameState.Id);
        var update = Builders<GameState>.Update.Set(g => g.Inventory.Items, gameState.Inventory.Items);

        await _gameStateContext.UpdateOneAsync(filter, update);
    }
}