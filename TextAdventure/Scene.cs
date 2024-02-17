namespace TextAdventure;

public class Scene : IDescribable
{
    public List<Prop> Props { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Dictionary<Direction, SceneConnection> Connections { get; set; } = [];
}
