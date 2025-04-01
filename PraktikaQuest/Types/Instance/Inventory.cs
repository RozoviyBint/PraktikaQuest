using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class Inventory
{
    public List<Item> Items { get; set; } = new List<Item>();
}
