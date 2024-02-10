using System.Diagnostics.CodeAnalysis;
using TextAdventure;

var longswordItem = new Item()
{
    Name = "Longsword",
    Description = "Though worn by age, this sword has maintained enough of an edge to still be useful ... somewhat."
};

var keyItem = new Item()
{
    Name = "Key",
    Description = "A small, golden key."
};

var skeletonItem = new Item()
{
    Name = "Skeleton",
    Description = "Seated on the ground and slumped against the wall, it can't be said whether this is the skeleton of a servant or a king.",
    Items = [ keyItem ],
    BreakageReplacement = new Item()
    {
        Name = "Pile of Bones",
        Description = "A pile of bones from the skeleton you slayed."
    }
};
skeletonItem.UseEffects = new()
{
    [longswordItem] = (room, player) =>
    {
        player.Experience++;
        room.Items.Remove(skeletonItem);
        room.Items.AddRange(skeletonItem.ItemsOnBreakage);

        return "The skeleton breaks apart as you strike it with your longsword.";
    }
};

var beginningRoom = new Room()
{
    Name = "The Beginning",
    Description = "An empty room with stone walls."
};

var banquetRoom = new Room()
{
    Name = "A Large Hall",
    Description = "Once the location of countless lavish feasts and celebrations, this grand hall has now deteriorated. Its windows are boarded and a thick layer of dust coats each of its surfaces."
};

var pantryRoom = new Room()
{
    Name = "Pantry",
    Description = "The pantry is empty and cold.",
    Items = [ skeletonItem ]
};

var magicPassagewayRoom = new Room()
{
    Name = "Magic Passageway",
    Description = "You enter a long, narrow, dark passageway. You feel uneasy."
};

var treasureRoom = new Room()
{
    Name = "Treasure Vault",
    Description = "This small vault, hidden and guarded by magic, has rows upon rows of empty shelves.",
    Items = [ longswordItem ]
};

var endingRoom = new Room()
{
    Name = "The End",
    Description = "You survived!"
};

var banquetToEndingConnection = new RoomConnection()
{
    Name = "Door",
    DisambiguatingName = "Door to the North",
    Description = "A large, ornate door.",
    To = endingRoom,
    IsImpassable = true,
    ImpassableMessage = "You cannot open this door, it is locked."
};
banquetToEndingConnection.UseEffects = new()
{
    [keyItem] = (_, player) =>
    {
        banquetToEndingConnection.IsImpassable = false;
        player.Experience++;

        return "You unlock the door with the key.";
    }
};

beginningRoom.Connections = new()
{
    [Direction.North] = new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the North",
        To = banquetRoom
    }
};

banquetRoom.Connections = new()
{
    [Direction.South] =  new()
    {
        Name = "Long Dark Passageway",
        DisambiguatingName = "Passageway to the South",
        To = magicPassagewayRoom
    },
    [Direction.West] =  new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the West",
        To = pantryRoom
    },
    [Direction.North] = banquetToEndingConnection
};

magicPassagewayRoom.Connections = new()
{
    [Direction.South] =  new()
    {
        Name = "Long Dark Passageway",
        DisambiguatingName = "Passageway to the South",
        To = beginningRoom
    },
    [Direction.North] =  new()
    {
        Name = "Long Dark Passageway",
        DisambiguatingName = "Passageway to the North",
        To = treasureRoom
    }
};

treasureRoom.Connections = new()
{
    [Direction.South] =  new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the South",
        To = magicPassagewayRoom
    }
};

pantryRoom.Connections = new()
{
    [Direction.East] =  new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the East",
        To = banquetRoom
    }
};

endingRoom.Connections = new()
{
    [Direction.South] =  new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the South",
        To = banquetRoom
    }
};

var player = new Player();
var currentRoom = beginningRoom;

bool TryGetSingleTarget<T>(IEnumerable<T> candidates, string inputName, [NotNullWhen(true)] out T? target) where T : INamable
{
    target = default;

    if (!candidates.Any())
    {
        return false;
    }

    if (candidates.Count() == 1)
    {
        target = candidates.Single();
        return true;
    }

    Console.WriteLine($"There is more than one {inputName}. Which one do you mean?");

    for (int i = 0; i < candidates.Count(); i++)
    {
        Console.WriteLine($"    {i}: {candidates.ElementAt(i).DisambiguatingName}");
    }

    var choice = -1;
    Console.Write(">: ");

    while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > candidates.Count() - 1)
    {
        Console.WriteLine($"Sorry, that's not a valid input. Please enter a number from 0 to {candidates.Count() - 1}.");
        Console.Write(">: ");
    }

    target = candidates.ElementAt(choice);
    return true;
}

void ClearScreen()
{
    Console.Clear();
    Console.WriteLine($"Text Adventure | {player.Experience} XP");
}

void PrintCurrentRoomContents()
{
    if (currentRoom.Items.Count != 0)
    {
        Console.WriteLine();
        Console.WriteLine($"You see a {string.Join(", ", currentRoom.Items.Select(i => i.Name.ToLower()))}.");
    }

    if (currentRoom.Connections.Count != 0)
    {
        Console.WriteLine();
        Console.WriteLine($"There is {string.Join(", ", currentRoom.Connections.Select(c => $"a {c.Value.Name.ToLower()} to the {Enum.GetName(c.Key)?.ToLower()}"))}.");
    }
}

void PrintCurrentRoom()
{
    ClearScreen();

    Console.WriteLine(currentRoom.Name);
    Console.WriteLine();
    Console.WriteLine(currentRoom.Description);
    PrintCurrentRoomContents();
}

void HandleDirectionInput(string directionInput)
{
    directionInput = directionInput.Trim();

    if (!Enum.TryParse<Direction>(directionInput, true, out var direction))
    {
        Console.WriteLine($"{directionInput} is not a valid direction.");
        return;
    }
    
    if (!currentRoom.Connections.TryGetValue(direction, out var connection))
    {
        Console.WriteLine($"There is no escape to the {directionInput.ToLower()}.");
        return;
    }

    if (connection.IsImpassable)
    {
        Console.WriteLine(connection.ImpassableMessage);
        return;
    }
    
    currentRoom = connection.To;
    PrintCurrentRoom();
}

void HandleInventoryInput()
{
    Console.WriteLine($"You have a {string.Join(", ", player.Inventory.Select(i => i.Name.ToLower()))}.");
}

void HandleTakeInput(string itemInput)
{
    itemInput = itemInput.Trim().ToLower();
    
    if (currentRoom.Items.FirstOrDefault(i => i.Name.ToLower() == itemInput) is not Item item)
    {
        Console.WriteLine($"There is no {itemInput} here.");
        return;
    }

    currentRoom.Items.Remove(item);
    player.Inventory.Add(item);
    Console.WriteLine($"The {item.Name} is in your inventory.");
}

void HandleDropInput(string itemInput)
{
    itemInput = itemInput.Trim().ToLower();

    if (player.Inventory.FirstOrDefault(i => i.Name.ToLower() == itemInput) is not Item item)
    {
        Console.WriteLine($"There is no {itemInput} in your inventory.");
        return;
    }

    player.Inventory.Remove(item);
    currentRoom.Items.Add(item);
    Console.WriteLine($"The {item.Name} falls to the floor.");
}

void HandleInspectInput(string itemInput)
{
    itemInput = itemInput.Trim().ToLower();

    IEnumerable<IDescribable> candidateTargets =
    [
        ..currentRoom.Items.Where(i => i.Name.ToLower() == itemInput),
        ..player.Inventory.Where(i => i.Name.ToLower() == itemInput),
        ..currentRoom.Connections.Where(c => c.Value.Name.ToLower() == itemInput).Select(c => c.Value)
    ];

    if (!TryGetSingleTarget(candidateTargets, itemInput, out var target))
    {
        Console.WriteLine($"There is no {itemInput} to inspect.");
        return;
    }

    Console.WriteLine(target.Name);
    Console.WriteLine(target.Description);

    if (target is Item item && item.Items.Count != 0)
    {
        Console.WriteLine($"The {item.Name} contains a {string.Join(", ", item.Items.Select(i => i.Name.ToLower()))}.");
    }

    return;
}

void HandleInspectCurrentRoomInput()
{
    if (currentRoom.Items.Count != 0)
    {
        Console.WriteLine();
        Console.WriteLine($"You see a {string.Join(", ", currentRoom.Items.Select(i => i.Name.ToLower()))}.");
    }

    if (currentRoom.Connections.Count != 0)
    {
        Console.WriteLine();
        Console.WriteLine($"There are passageways to the {string.Join(", ", currentRoom.Connections.Select(r => Enum.GetName(r.Key)?.ToLower()))}.");
    }
}

void HandleUseInput(string implementItemInput, string targetItemInput)
{
    implementItemInput = implementItemInput.Trim().ToLower();
    targetItemInput = targetItemInput.Trim().ToLower();

    var implementItem = player.Inventory.FirstOrDefault(i => i.Name.ToLower() == implementItemInput);

    if (implementItem is null)
    {
        Console.WriteLine($"You do not have a {implementItemInput} to use on the {targetItemInput}.");
        return;
    }

    IEnumerable<IUsable> candidateTargets =
    [
        ..currentRoom.Items.Where(i => i.Name.ToLower() == targetItemInput && i.UseEffects.ContainsKey(implementItem)),
        ..player.Inventory.Where(i => i.Name.ToLower() == targetItemInput && i.UseEffects.ContainsKey(implementItem)),
        ..currentRoom.Connections.Where(c => c.Value.Name.ToLower() == targetItemInput && c.Value.UseEffects.ContainsKey(implementItem)).Select(c => c.Value)
    ];

    if (!TryGetSingleTarget(candidateTargets, targetItemInput, out var target))
    {
        Console.WriteLine($"There is no {targetItemInput} to use with the {implementItemInput}.");
        return;
    }

    if (!target.UseEffects.TryGetValue(implementItem, out var effectAction))
    {
        Console.WriteLine($"Using the {implementItemInput} on the {targetItemInput} has no effect.");
        return;
    }

    var description = effectAction(currentRoom, player);
    Console.WriteLine(description);
}

PrintCurrentRoom();
var shouldContinue = true;

while (shouldContinue)
{
    Console.WriteLine();
    Console.Write(">: ");
    var input = Console.ReadLine()?.Split(' ');
    
    try
    {
        switch (input)
        {
            case ["go", string directionInput]:
                HandleDirectionInput(directionInput);
                break;

            case ["inventory"]:
                HandleInventoryInput();
                break;

            case ["take", string itemInput]:
                HandleTakeInput(itemInput);
                break;

            case ["drop", string itemInput]:
                HandleDropInput(itemInput);
                break;

            case ["inspect", string itemInput]:
                HandleInspectInput(itemInput);
                break;

            case ["inspect"]:
                HandleInspectCurrentRoomInput();
                break;

            case ["use", string implementItemInput, "on", string targetItemInput]:
                HandleUseInput(implementItemInput, targetItemInput);
                break;

            case ["exit"]:
                Console.Write("Are you sure (Y/N)? ");
                shouldContinue = Console.ReadLine()?.Trim().ToLower() != "y";
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error!");
        Console.WriteLine(ex.Message + ex.StackTrace);
    }
}
