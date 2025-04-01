public class SceneRenderDto
{
    public List<SceneBlockDto> Scenes { get; set; } = new();
}

public class SceneBlockDto
{
    public int Id { get; set; }
    public string BackgroundImage { get; set; }
    public List<RouteDto> Routes { get; set; } = new();
    public bool Event { get; set; }
    public List<string> Description { get; set; }
}

public class RouteDto
{
    public int TargetBlockId { get; set; }
    public string RouteDescription { get; set; }
}

public class EventPreviewDto
{
    public string Image { get; set; }
    public string Description { get; set; }
}
