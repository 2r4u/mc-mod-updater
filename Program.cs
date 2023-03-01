// See https://aka.ms/new-console-template for more information
using Modrinth;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
using System.Collections;

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");



await eraseJson(@"..\\..\\..\\data.json");

Console.WriteLine("What version of Minecraft are you updating to?");
String version = Console.ReadLine();
String mcPath = "..\\..\\..\\..\\..\\..\\AppData\\Roaming\\.minecraft\\mods";
    
string[] files = Directory.GetFiles(@mcPath);
ArrayList fnames = new ArrayList();
Console.WriteLine("These are the files in your mods folder:");
foreach (string file in files) {
    Console.WriteLine(Path.GetFileName(file).Replace("\\", "").Replace("..", ""));
    fnames.Add(Path.GetFileName(file));
}
Console.WriteLine("Are you sure you want to replace them with their updated versions?");
if (yn(Console.ReadLine()))
{
    foreach (String file in fnames)
    {
        await updateMods(client, file, version);
    }
    Console.WriteLine("These are the file names of the mods that will be downloaded:");
    var jsonfile = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(@"..\\..\\..\\data.json"));
    FromJsonElement(jsonfile);
}
else
{
    Console.WriteLine("That's too bad. Run this app again when you want to.");
}
static async Task eraseJson(string path)
{
    File.WriteAllText(@path, String.Empty);
}
static (string? name, String author) FromJsonElement(JsonElement jsonElement)
{
    var name = jsonElement.GetProperty("hits").GetProperty("title").GetString();
    var author = jsonElement.GetProperty("hits").GetProperty("author").GetString();
    return (name, author);
}

static async Task updateMods(HttpClient client, String file, String version) 
{   
    var json = await client.GetStringAsync("https://api.modrinth.com/v2/search?limit=1&query="+GetUntilOrEmpty(1,file,"-")+"&index=downloads&facets=[[\"categories:fabric\"],[\"versions:" + version + "\"]]");
    Console.WriteLine(GetUntilOrEmpty(1,file,"-"));
    File.AppendAllText(@"..\\..\\..\\data.json", json);
    

    
}
static string GetUntilOrEmpty( int mode,string text, String stopAt)
{   if (mode == 1)
    {
        if (!String.IsNullOrWhiteSpace(text))
        {
            int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

            if (charLocation > 0)
            {
                return text.Substring(0, charLocation);
            }
        }
    }
    else
    {
        if (!String.IsNullOrWhiteSpace(text))
        {
            int charLocation = text.LastIndexOf(stopAt, StringComparison.Ordinal);

            if (charLocation > 0)
            {
                return text.Substring(charLocation,text.Length);
            }
        }
    }

    return String.Empty;
}

bool yn(String str)
{
    String[] ys = { "yes", "y", "yup", "yeah", "sure" };
    String[] ns = { "no", "n", "nope", "nah" };
    for (int i = 0; i < 4; i++)
    {
        if (str.ToLower().Equals(ys[i]))
        {
            return true;
        }
        else if (str.ToLower().Equals(ns[i]))
        {
            return false;
        }
    }
    return false;
}
