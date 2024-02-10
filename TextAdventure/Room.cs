namespace TextAdventure;

public class Room : IDescribable
{
    public List<Item> Items { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Dictionary<Direction, RoomConnection> Connections { get; set; } = [];
}
