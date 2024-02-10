namespace TextAdventure;

public interface IUsable : INamable
{    Dictionary<Item, Func<Room, Player, string>> UseEffects { get; }
}
