using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class KeyEvent
{
    [BsonId]
    public int Id { get; set; }
    public EventType EventType { get; set; } 
    public string Image { get; set; } 
    public List<EventAction> Actions { get; set; } = new List<EventAction>();

    public string EventDescription { get; set; }
}

public enum EventType
{
    AddItem,
    UseItem,
    Story
}
