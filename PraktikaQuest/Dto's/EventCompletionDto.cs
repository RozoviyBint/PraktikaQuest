using MongoDB.Bson;

public class EventCompletionDto
{
    public int BlockId { get; set; }
    public int EventId { get; set; }
    public int ActionId { get; set; }
    public IEnumerable<int> NewBlocks { get; set; }
}
