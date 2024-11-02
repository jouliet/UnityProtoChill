using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

// Programme pour tester le fonctionnement de l'api
// On peut lui mettre les fichiers en prompt avec la fonction TakeFilesAsString
// Le problème: ca demande surement un trop gros token et coute chere. 
class Program
{
    public static void Main(string[] args)
    {
        // Run in the Powershell : setx OPENAI_API_KEY  "Your API Key"
        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("La clé API n'a pas été trouvée dans les variables d'environnement.");
                return;
            }
        ChatClient client = new("gpt-4o", apiKey);
        ChatCompletion completion = client.CompleteChat("Say 'ya'" );
        Console.WriteLine($"[ASSISTANT]: {completion.Content[0].Text}");
    }

    private static string ReadFile(string filePath){
        string fileContent;
        try
        {
            fileContent = File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la lecture du fichier : {ex.Message}");
            fileContent = "Erreur de lecture du fichier";
        }
        return fileContent;
    }



    // Method pour prendre les fichiers à mettre dans le prompt. 
    private static string TakeFilesAsString(){
        return ReadFile("C:/Users/Jules/Documents/Simon/UnityProtoChill/ProtoCHILL/caca.cs");
    }
}