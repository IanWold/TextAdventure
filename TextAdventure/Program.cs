using System.Diagnostics.CodeAnalysis;
using TextAdventure;

var longswordProp = new Prop()
{
    Name = "Longsword",
    Description = "Though worn by age, this sword has maintained enough of an edge to still be useful ... somewhat."
};

var keyProp = new Prop()
{
    Name = "Key",
    Description = "A small, golden key."
};

var skeletonProp = new Prop()
{
    Name = "Skeleton",
    Description = "Seated on the ground and slumped against the wall, it can't be said whether this is the skeleton of a servant or a king.",
    Props = [ keyProp ],
    BreakageReplacement = new Prop()
    {
        Name = "Pile of Bones",
        Description = "A pile of bones from the skeleton you slayed."
    }
};
skeletonProp.UseEffects = new()
{
    [longswordProp] = (scene, player) =>
    {
        player.Experience++;
        scene.Props.Remove(skeletonProp);
        scene.Props.AddRange(skeletonProp.PropsOnBreakage);

        return "The skeleton breaks apart as you strike it with your longsword.";
    }
};

var beginningScene = new Scene()
{
    Name = "The Beginning",
    Description = "An empty room with stone walls."
};

var banquetScene = new Scene()
{
    Name = "A Large Hall",
    Description = "Once the location of countless lavish feasts and celebrations, this grand hall has now deteriorated. Its windows are boarded and a thick layer of dust coats each of its surfaces."
};

var pantryScene = new Scene()
{
    Name = "Pantry",
    Description = "The pantry is empty and cold.",
    Props = [ skeletonProp ]
};

var magicPassagewayScene = new Scene()
{
    Name = "Magic Passageway",
    Description = "You enter a long, narrow, dark passageway. You feel uneasy."
};

var treasureScene = new Scene()
{
    Name = "Treasure Vault",
    Description = "This small vault, hidden and guarded by magic, has rows upon rows of empty shelves.",
    Props = [ longswordProp ]
};

var endingScene = new Scene()
{
    Name = "The End",
    Description = "You survived!"
};

var banquetToEndingConnection = new SceneConnection()
{
    Name = "Door",
    DisambiguatingName = "Door to the North",
    Description = "A large, ornate door.",
    To = endingScene,
    IsImpassable = true,
    ImpassableMessage = "You cannot open this door, it is locked."
};
banquetToEndingConnection.UseEffects = new()
{
    [keyProp] = (_, player) =>
    {
        banquetToEndingConnection.IsImpassable = false;
        player.Experience++;

        return "You unlock the door with the key.";
    }
};

beginningScene.Connections = new()
{
    [Direction.North] = new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the North",
        To = banquetScene
    }
};

banquetScene.Connections = new()
{
    [Direction.South] =  new()
    {
        Name = "Long Dark Passageway",
        DisambiguatingName = "Passageway to the South",
        To = magicPassagewayScene
    },
    [Direction.West] =  new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the West",
        To = pantryScene
    },
    [Direction.North] = banquetToEndingConnection
};

magicPassagewayScene.Connections = new()
{
    [Direction.South] =  new()
    {
        Name = "Long Dark Passageway",
        DisambiguatingName = "Passageway to the South",
        To = beginningScene
    },
    [Direction.North] =  new()
    {
        Name = "Long Dark Passageway",
        DisambiguatingName = "Passageway to the North",
        To = treasureScene
    }
};

treasureScene.Connections = new()
{
    [Direction.South] =  new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the South",
        To = magicPassagewayScene
    }
};

pantryScene.Connections = new()
{
    [Direction.East] =  new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the East",
        To = banquetScene
    }
};

endingScene.Connections = new()
{
    [Direction.South] =  new()
    {
        Name = "Door",
        DisambiguatingName = "Door to the South",
        To = banquetScene
    }
};

var player = new Player();
var currentScene = beginningScene;

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

void PrintCurrentSceneContents()
{
    if (currentScene.Props.Count != 0)
    {
        Console.WriteLine();
        Console.WriteLine($"You see a {string.Join(", ", currentScene.Props.Select(i => i.Name.ToLower()))}.");
    }

    if (currentScene.Connections.Count != 0)
    {
        Console.WriteLine();
        Console.WriteLine($"There is {string.Join(", ", currentScene.Connections.Select(c => $"a {c.Value.Name.ToLower()} to the {Enum.GetName(c.Key)?.ToLower()}"))}.");
    }
}

void PrintCurrentScene()
{
    ClearScreen();

    Console.WriteLine(currentScene.Name);
    Console.WriteLine();
    Console.WriteLine(currentScene.Description);
    PrintCurrentSceneContents();
}

void HandleDirectionInput(string directionInput)
{
    directionInput = directionInput.Trim();

    if (!Enum.TryParse<Direction>(directionInput, true, out var direction))
    {
        Console.WriteLine($"{directionInput} is not a valid direction.");
        return;
    }
    
    if (!currentScene.Connections.TryGetValue(direction, out var connection))
    {
        Console.WriteLine($"There is no escape to the {directionInput.ToLower()}.");
        return;
    }

    if (connection.IsImpassable)
    {
        Console.WriteLine(connection.ImpassableMessage);
        return;
    }
    
    currentScene = connection.To;
    PrintCurrentScene();
}

void HandleInventoryInput()
{
    Console.WriteLine($"You have a {string.Join(", ", player.Inventory.Select(i => i.Name.ToLower()))}.");
}

void HandleTakeInput(string propInput)
{
    propInput = propInput.Trim().ToLower();
    
    if (currentScene.Props.FirstOrDefault(i => i.Name.ToLower() == propInput) is not Prop prop)
    {
        Console.WriteLine($"There is no {propInput} here.");
        return;
    }

    currentScene.Props.Remove(prop);
    player.Inventory.Add(prop);
    Console.WriteLine($"The {prop.Name} is in your inventory.");
}

void HandleDropInput(string propInput)
{
    propInput = propInput.Trim().ToLower();

    if (player.Inventory.FirstOrDefault(i => i.Name.ToLower() == propInput) is not Prop prop)
    {
        Console.WriteLine($"There is no {propInput} in your inventory.");
        return;
    }

    player.Inventory.Remove(prop);
    currentScene.Props.Add(prop);
    Console.WriteLine($"The {prop.Name} falls to the floor.");
}

void HandleInspectInput(string propInput)
{
    propInput = propInput.Trim().ToLower();

    IEnumerable<IDescribable> candidateTargets =
    [
        ..currentScene.Props.Where(i => i.Name.ToLower() == propInput),
        ..player.Inventory.Where(i => i.Name.ToLower() == propInput),
        ..currentScene.Connections.Where(c => c.Value.Name.ToLower() == propInput).Select(c => c.Value)
    ];

    if (!TryGetSingleTarget(candidateTargets, propInput, out var target))
    {
        Console.WriteLine($"There is no {propInput} to inspect.");
        return;
    }

    Console.WriteLine(target.Name);
    Console.WriteLine(target.Description);

    if (target is Prop prop && prop.Props.Count != 0)
    {
        Console.WriteLine($"The {prop.Name} contains a {string.Join(", ", prop.Props.Select(i => i.Name.ToLower()))}.");
    }

    return;
}

void HandleInspectCurrentSceneInput()
{
    if (currentScene.Props.Count != 0)
    {
        Console.WriteLine();
        Console.WriteLine($"You see a {string.Join(", ", currentScene.Props.Select(i => i.Name.ToLower()))}.");
    }

    if (currentScene.Connections.Count != 0)
    {
        Console.WriteLine();
        Console.WriteLine($"There are passageways to the {string.Join(", ", currentScene.Connections.Select(r => Enum.GetName(r.Key)?.ToLower()))}.");
    }
}

void HandleUseInput(string implementPropInput, string targetPropInput)
{
    implementPropInput = implementPropInput.Trim().ToLower();
    targetPropInput = targetPropInput.Trim().ToLower();

    var implementProp = player.Inventory.FirstOrDefault(i => i.Name.ToLower() == implementPropInput);

    if (implementProp is null)
    {
        Console.WriteLine($"You do not have a {implementPropInput} to use on the {targetPropInput}.");
        return;
    }

    IEnumerable<IUsable> candidateTargets =
    [
        ..currentScene.Props.Where(i => i.Name.ToLower() == targetPropInput && i.UseEffects.ContainsKey(implementProp)),
        ..player.Inventory.Where(i => i.Name.ToLower() == targetPropInput && i.UseEffects.ContainsKey(implementProp)),
        ..currentScene.Connections.Where(c => c.Value.Name.ToLower() == targetPropInput && c.Value.UseEffects.ContainsKey(implementProp)).Select(c => c.Value)
    ];

    if (!TryGetSingleTarget(candidateTargets, targetPropInput, out var target))
    {
        Console.WriteLine($"There is no {targetPropInput} to use with the {implementPropInput}.");
        return;
    }

    if (!target.UseEffects.TryGetValue(implementProp, out var effectAction))
    {
        Console.WriteLine($"Using the {implementPropInput} on the {targetPropInput} has no effect.");
        return;
    }

    var description = effectAction(currentScene, player);
    Console.WriteLine(description);
}

PrintCurrentScene();
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

            case ["take", string propInput]:
                HandleTakeInput(propInput);
                break;

            case ["drop", string propInput]:
                HandleDropInput(propInput);
                break;

            case ["inspect", string propInput]:
                HandleInspectInput(propInput);
                break;

            case ["inspect"]:
                HandleInspectCurrentSceneInput();
                break;

            case ["use", string implementPropInput, "on", string targetPropInput]:
                HandleUseInput(implementPropInput, targetPropInput);
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
