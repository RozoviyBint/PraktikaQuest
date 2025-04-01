using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
public class User
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Name { get; set; }
}
