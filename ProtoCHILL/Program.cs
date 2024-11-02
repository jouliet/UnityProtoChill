using System;
using System.Collections.Generic;
using UMLStructure;

// C'est une classe qui permet de tester la structure de données UML que j'ai créé à partir des classes Class, Attribut et Method. 
// Pour la faire fonctionner, il faut changer le nom de la classe en Program et changer remplacer PossibleMain par Main. Inversement, il faut donner d'autres noms dans la fichier GPTGenerator. 
// Le build se fait toujours dans la classe Program et dans la fonction Main. 
public class PProgram
{
    public static void PossibleMain()
    {
        var gameManager = new Class("GameManager");
        var player = new Class("Player");
        var era = new Class("Era");
        var worldState = new Class("WorldState");
        var puzzleManager = new Class("PuzzleManager");
        var puzzle = new Class("Puzzle");
        var inventory = new Class("Inventory");
        var item = new Class("Item");
        var character = new Class("Character");

        gameManager.AddAttribute("currentEra", era, Visibility.Private);
        gameManager.AddAttribute("player", player, Visibility.Private);
        gameManager.AddAttribute("worldState", worldState, Visibility.Private);
        gameManager.AddAttribute("puzzleManager", puzzleManager, Visibility.Private);

        player.AddAttribute("position", PrimitiveType.Vector3, Visibility.Private);
        player.AddAttribute("inventory", inventory, Visibility.Private);

        era.AddAttribute("eraName", PrimitiveType.String, Visibility.Private);
        era.AddAttribute("environment", new Class("Environment"), Visibility.Private);
        era.AddAttribute("characters", new Class("List<Character>"), Visibility.Private, character); // Instanciation d'une liste de Character ici

        worldState.AddAttribute("globalChanges", PrimitiveType.Dictionary(PrimitiveType.String, PrimitiveType.Bool), Visibility.Private);

        puzzleManager.AddAttribute("puzzles", new Class("List<Puzzle>"), Visibility.Private, puzzle); // Instanciation d'une liste de Puzzle ici

        puzzle.AddAttribute("description", PrimitiveType.String, Visibility.Private);
        puzzle.AddAttribute("isCompleted", PrimitiveType.Bool, Visibility.Private);

        inventory.AddAttribute("items", new Class("List<Item>"), Visibility.Private, item); // Instanciation d'une liste d'Item ici

        item.AddAttribute("itemName", PrimitiveType.String, Visibility.Private);
        item.AddAttribute("effect", PrimitiveType.String, Visibility.Private);

        character.AddAttribute("name", PrimitiveType.String, Visibility.Private);
        character.AddAttribute("dialogue", new Class("List<string>"), Visibility.Private); // Instanciation d'une liste de string ici

        gameManager.AddMethod("startGame", PrimitiveType.Void, new List<Class>(), Visibility.Public);
        gameManager.AddMethod("switchEra", PrimitiveType.Void, new List<Class> { era }, Visibility.Public);
        gameManager.AddMethod("saveWorldState", PrimitiveType.Void, new List<Class>(), Visibility.Public);
        gameManager.AddMethod("loadWorldState", PrimitiveType.Void, new List<Class>(), Visibility.Public);

        player.AddMethod("move", PrimitiveType.Void, new List<Class>(), Visibility.Public);
        player.AddMethod("interact", PrimitiveType.Void, new List<Class>(), Visibility.Public);
        player.AddMethod("useItem", PrimitiveType.Void, new List<Class> { item }, Visibility.Public);

        era.AddMethod("loadEra", PrimitiveType.Void, new List<Class>(), Visibility.Public);
        era.AddMethod("unloadEra", PrimitiveType.Void, new List<Class>(), Visibility.Public);

        worldState.AddMethod("applyChange", PrimitiveType.Void, new List<Class> { PrimitiveType.String, PrimitiveType.Bool }, Visibility.Public);
        worldState.AddMethod("getCurrentState", PrimitiveType.Dictionary(PrimitiveType.String, PrimitiveType.Bool), new List<Class>(), Visibility.Public);

        puzzleManager.AddMethod("checkPuzzleCompletion", PrimitiveType.Void, new List<Class>(), Visibility.Public);
        puzzleManager.AddMethod("resetPuzzle", PrimitiveType.Void, new List<Class> { puzzle }, Visibility.Public);

        puzzle.AddMethod("solve", PrimitiveType.Void, new List<Class>(), Visibility.Public);
        puzzle.AddMethod("reset", PrimitiveType.Void, new List<Class>(), Visibility.Public);

        inventory.AddMethod("addItem", PrimitiveType.Void, new List<Class> { item }, Visibility.Public);
        inventory.AddMethod("removeItem", PrimitiveType.Void, new List<Class> { item }, Visibility.Public);

        item.AddMethod("use", PrimitiveType.Void, new List<Class>(), Visibility.Public);

        character.AddMethod("speak", PrimitiveType.Void, new List<Class>(), Visibility.Public);
        character.AddMethod("interact", PrimitiveType.Void, new List<Class>(), Visibility.Public);

        string typeName;

        foreach (var cls in ClassManager.Instance.Classes)
        {
            typeName = cls.Name;

            // Afficher la classe avec son ToString
            Console.WriteLine(cls);
        }
    }
}
