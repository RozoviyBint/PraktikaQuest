using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class EventAction
{
    [BsonId]
    public int Id { get; set; }
    public string Description { get; set; }
    public int? RequiredItemId { get; set; }
    public Dictionary<string, int>? EventsActions {get;set;}
    public int ResultBlockId { get; set; }
}
