using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class SceneBlock
{
    [BsonId]
    public int Id { get; set; }
    public string BackgroundImage { get; set; }
    public List<SceneRoute> Routes { get; set; } = new List<SceneRoute>();
    public KeyEvent? Event { get; set; } = null;
    public List<string> Description { get; set; } = new List<string>();
}
