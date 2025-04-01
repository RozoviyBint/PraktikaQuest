using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class GameState
{
    [BsonId]
    public ObjectId Id { get; set; }

    public ObjectId PlayerId { get; set; }            
    public int CurrentBlockId { get; set; }       
    public List<int> VisitedBlocks { get; set; } = new(); // История посещённых блоков
    public List<int> CompletedEvents { get; set; } = new(); // Выполненные ключевые события
    public Dictionary<string, int> CompletedEvents_ChosenActions { get; set; } = new();

    public Inventory Inventory { get; set; } = new();

    //public Dictionary<ObjectId, List<ObjectId>> AvailableRoutes { get; set; } = new();
    // Ключ — SceneBlockId, значение — список доступных Route (TargetBlockId)

    //public Dictionary<ObjectId, string> UpdatedDescriptions { get; set; } = new();
    // Ключ — SceneBlockId, значение — изменённое описание (если есть)

    //public int GameProgress { get; set; } 
}
