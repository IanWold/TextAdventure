namespace TextAdventure;

public class RoomConnection : IUsable, IDescribable
{
    public required Room To { get; set; }

    public string Name { get; set; } = "Passageway";

    public string DisambiguatingName { get; set; } = "Passageway";

    public string Description { get; set; } = string.Empty;

    public bool IsImpassable { get; set; }

    public string ImpassableMessage  { get; set; } = string.Empty;

    public Dictionary<Item, Func<Room, Player, string>> UseEffects { get; set; } = [];
}
