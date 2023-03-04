// See https://aka.ms/new-console-template for more information
using Modrinth;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
using System.Collections;
using System;

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");



await eraseJson(@"..\\..\\..\\data.json");

Console.WriteLine("What version of Minecraft are you updating to?");
String version = Console.ReadLine();
String mcPath = "..\\..\\..\\..\\..\\..\\AppData\\Roaming\\.minecraft\\mods";
    
string[] files = Directory.GetFiles(@mcPath);
List<Mod> mods = new List<Mod>();
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
        await updateMods(client, file, version,mods);
    }
    clean(mods);
    Console.WriteLine("==================================================");
    Console.WriteLine("These are the mods that will be downloaded:");
    Thread.Sleep(3000);
    for (int i=0;i<mods.Count; i++)
    {
        Mod m = mods[i];
        Console.WriteLine(m.Name+": "+m.Description);
    }
    Thread.Sleep(3000);
    Console.WriteLine("Some of these mods may not be correct. Please enter the number of the one you wish to remove.");
    while (true)
        //get user to narrow down mods until all are correct
    {
        Console.WriteLine((i + 1) + ". " +);
        int mn = Int32.Parse(Console.ReadLine());
    }
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
static async Task updateMods(HttpClient client, String file, String version, List<Mod> list)
{
    var json = await client.GetStringAsync("https://api.modrinth.com/v2/search?limit=1&query=" + GetUntilOrEmpty(1, file, "-") + "&index=downloads&facets=[[\"categories:fabric\"],[\"versions:" + version + "\"]]");
    /*File.AppendAllText(@"..\\..\\..\\data.json", json);*/
    Console.WriteLine(GetUntilOrEmpty(1, file, "-"));
    try
    {
        var f = JsonSerializer.Deserialize<Root>(json);
        Console.WriteLine(f.hits[0].title);
        list.Add(new Mod(f.hits[0].title, f.hits[0].description, f.hits[0].versions));
    }
    catch(Exception e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine("This mod might not be on Modrinth or updated to this version.");
    }
    /*using JsonDocument doc = JsonDocument.Parse(json);
    JsonElement root = doc.RootElement;
    String js = ""1""root[0].Ge;*/
    
    /*List<string> lines = File.ReadAllLines(@"..\\..\\..\\data.json").ToList<string>();
    lines.Insert(lines.Capacity - 2, js);*/
    /* File.WriteAllLines(@"..\\..\\..\\data.json", lines);*/
    



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
void clean(List<Mod> list)
{
    List<string> names = new List<string>();
    
    if(list != null) 
    {

        foreach (Mod mod in list.ToList())
        {
            if (names.Contains(mod.Name))
            {
               list.Remove(mod);
            }
            else
            {
                names.Add(mod.Name);
            }
        }

    }
    else
    {
        Console.WriteLine("bruh");
    }
}
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Mod
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Versions { get; set; }
    public Mod(string name, string description, List<string> versions)
    {
        Name = name;
        Description = description;
        Versions = versions;
    }
    
}
public class Hit
{
    public string project_id { get; set; }
    public string project_type { get; set; }
    public string slug { get; set; }
    public string author { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public List<string> categories { get; set; }
    public List<string> display_categories { get; set; }
    public List<string> versions { get; set; }
    public int downloads { get; set; }
    public int follows { get; set; }
    public string icon_url { get; set; }
    public DateTime date_created { get; set; }
    public DateTime date_modified { get; set; }
    public string latest_version { get; set; }
    public string license { get; set; }
    public string client_side { get; set; }
    public string server_side { get; set; }
    public List<object> gallery { get; set; }
    public object featured_gallery { get; set; }
    public int color { get; set; }
}

public class Root
{
    public List<Hit> hits { get; set; }
    public int offset { get; set; }
    public int limit { get; set; }
    public int total_hits { get; set; }
}



