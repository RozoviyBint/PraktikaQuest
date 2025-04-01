using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Item
{
    [BsonId]
    public int Id { get; set; }
    public string Image { get; set; }
    public string Description { get; set; }
}
