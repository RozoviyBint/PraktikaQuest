using Messenger.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PraktikaQuest.Services
{
    public class AdminService
    {
        private readonly CollectionContext<SceneBlock> _sceneBlocks;
        public readonly CollectionContext<KeyEvent> _keyEvents;
        //private readonly CollectionContext<EventAction> _eventActions;
        //private readonly CollectionContext<Inventory> _inventories;
        private readonly CollectionContext<Item> _items;
        private readonly CollectionContext<User> _users;
        private readonly CollectionContext<GameState> _gamestates;

        public AdminService(
            CollectionContext<SceneBlock> sceneBlocks,
            CollectionContext<KeyEvent> keyEvents,
            //CollectionContext<EventAction> eventActions,
            //CollectionContext<Inventory> inventories,
            CollectionContext<Item> items,
            CollectionContext<User> users,
            CollectionContext<GameState> gamestates)
        {
            _sceneBlocks = sceneBlocks;
            _keyEvents = keyEvents;
            //_eventActions = eventActions;
            //_inventories = inventories;
            _items = items;
            _users = users;
            _gamestates = gamestates;
        }

        // SceneBlock Methods
        public async Task AddSceneBlock(SceneBlock block) => await _sceneBlocks.InsertDocument(block);

        public async Task<List<SceneBlock>> GetAllSceneBlocks() =>
            await _sceneBlocks.FindDocs(Builders<SceneBlock>.Filter.Empty);

        public async Task<SceneBlock?> GetSceneBlockById(int id) =>
            await _sceneBlocks.FindFirstAsync(id);

        public async Task<bool> DeleteSceneBlock(int id) =>
            await _sceneBlocks.DeleteDocument(id);

       
        // KeyEvent Methods
        public async Task AddKeyEvent(KeyEvent keyEvent) => await _keyEvents.InsertDocument(keyEvent);

        public async Task<KeyEvent?> GetKeyEventById(string id) =>
            await _keyEvents.FindFirstAsync(ObjectId.Parse(id));

        public async Task<List<KeyEvent>> GetAllKeyEvents() =>
            await _keyEvents.FindDocs(Builders<KeyEvent>.Filter.Empty);

        // EventAction Methods
        /*public async Task AddEventAction(EventAction action) => await _eventActions.InsertDocument(action);

        public async Task<List<EventAction>> GetAllEventActions() =>
            await _eventActions.FindDocs(Builders<EventAction>.Filter.Empty);

        public async Task<EventAction?> GetEventActionById(string id) =>
            await _eventActions.FindFirstAsync(ObjectId.Parse(id));

        // Inventory Methods
        public async Task AddInventory(Inventory inventory) => await _inventories.InsertDocument(inventory);

        public async Task<Inventory?> GetInventoryById(string id) =>
            await _inventories.FindFirstAsync(ObjectId.Parse(id));

        public async Task UpdateInventoryItems(string inventoryId, List<Item> items)
        {
            var update = Builders<Inventory>.Update.Set(i => i.Items, items);
            await _inventories.UpdateOneAsync(Builders<Inventory>.Filter.Eq(i => i.Id, ObjectId.Parse(inventoryId)), update);
        }
        */
        // Item Methods
        public async Task AddItem(Item item) => await _items.InsertDocument(item);


        /*public async Task<Item?> GetItemById(string id) =>
            await _items.FindFirstAsync(ObjectId.Parse(id));

        public async Task<List<Item>> GetAllItems() =>
            await _items.FindDocs(Builders<Item>.Filter.Empty);*/

        public async Task CreateUser(User user) 
        {
            await _users.InsertDocument(user);
            var game = new GameState
            {
                PlayerId = user.Id,
                CurrentBlockId = 1
            };
            await _gamestates.InsertDocument(game);
        } 
    }
}
