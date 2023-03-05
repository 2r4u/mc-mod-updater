// See https://aka.ms/new-console-template for more information
using Modrinth;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
using System.Collections;
using System;
using System.Net;
using System.Xml.Linq;
using System.Net.Http;

HttpClient client = new HttpClient();
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
await eraseJson(@"..\\..\\..\\data.json");

Console.WriteLine(">Welcome to Anthony's Minecraft Mod Updater!");
Thread.Sleep(500);
Console.WriteLine(">What version of Minecraft are you updating to?");
String version = Console.ReadLine();
String mcPath = "..\\..\\..\\..\\..\\..\\AppData\\Roaming\\.minecraft\\mods";

string[] files = Directory.GetFiles(@mcPath);
List<Mod> mods = new List<Mod>();
ArrayList fnames = new ArrayList();
Console.WriteLine(">These are the files in your mods folder:");
foreach (string file in files) {
    Console.WriteLine(Path.GetFileName(file).Replace("\\", "").Replace("..", ""));
    fnames.Add(Path.GetFileName(file));
}
Console.WriteLine(">Are you sure you want to replace them with their updated versions?");
if (yn(Console.ReadLine()))
{
    foreach (String file in fnames)
    {
        await updateMods(client, file, version,mods);
    }
    clean(mods);
    Console.WriteLine("==================================================");
    Console.WriteLine(">These are the mods that will be downloaded:");
    for (int i=0;i<mods.Count; i++)
    {
        Thread.Sleep(200);
        Mod m = mods[i];
        Console.WriteLine(m.Name+": "+m.Description);
        
    }
    Console.WriteLine(">Some of these mods may not be correct.");
    Thread.Sleep(200);
    prune(mods);
    await dlprocessAsync(client,mods,version,mcPath);
    
}
else
{
    Console.WriteLine(">That's too bad. Run this app again when you want to.");
}
static async Task dlprocessAsync(HttpClient c,List<Mod> mods, String v,string p)
{
    Console.WriteLine(">Here is your upated list of mods:");
    for (int i = 0; i < mods.Count; i++)
    {
        Thread.Sleep(200);
        Mod m = mods[i];
        Console.WriteLine(m.Name + ": " + m.Description);
    }
    Console.WriteLine(">Are you ready to download?");
    if (yn(Console.ReadLine()))
    {
        foreach (Mod m in mods)
        {
            Console.WriteLine("Downloading " + m.Name);
            await save(c,m,v,p);
        }
    }
    else
    {
        Console.WriteLine(">Do you want to remove more mods?");
        if (yn(Console.ReadLine()))
        {
            prune(mods);
        }
        else
        {
            Console.WriteLine(">Do you want to exit?");
            if (yn(Console.ReadLine()))
            {
                Console.WriteLine("adios");
            }
            else
            {
                await dlprocessAsync(c,mods, v,p);
            }
        }
    }
}
static async Task save(HttpClient client, Mod mod, String version,string path)
{
    var vjson = await client.GetStringAsync("https://api.modrinth.com/v2/project/" + mod.Id + "/version?loaders=[\"fabric\"]&game_versions=[\"" + version + "\"]");
    List<Base> b = JsonSerializer.Deserialize<List<Base>>(vjson);
    string furl = b[0].files[0].url;
    await DownloadFileAsync(client, furl, mod.Name,path);
}
 static async Task DownloadFileAsync(HttpClient client,string uri, string name,string outputPath)
{
    Uri uriResult;
    string path = outputPath + "\\" + name + ".jar";
    if (!Uri.TryCreate(uri, UriKind.Absolute, out uriResult))
        throw new InvalidOperationException("URI is invalid.");

    if (!File.Exists(path))
        File.Create(path);
        File.SetAttributes(path,(new FileInfo(path)).Attributes | FileAttributes.Normal);

    byte[] fileBytes = await client.GetByteArrayAsync(uri);
    File.WriteAllBytes(path, fileBytes);
}
static async Task eraseJson(string path)
{
    File.WriteAllText(@path, String.Empty);
}

static async Task updateMods(HttpClient client, String file, String version, List<Mod> list)
{
    
    /*File.AppendAllText(@"..\\..\\..\\data.json", json);*/
    Console.WriteLine(GetUntilOrEmpty(1, file, "-"));
    try
    {
        var json = await client.GetStringAsync("https://api.modrinth.com/v2/search?limit=1&query=" + GetUntilOrEmpty(1, file, "-") + "&index=downloads&facets=[[\"categories:fabric\"],[\"versions:" + version + "\"]]");
        var f = JsonSerializer.Deserialize<Root>(json);
        Console.WriteLine(f.hits[0].title);
        list.Add(new Mod(f.hits[0].project_id,f.hits[0].title, f.hits[0].description, f.hits[0].versions));
    }
    catch(Exception e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine("This mod might not be on Modrinth or updated to this version.");
    }
    
}
static void prune(List<Mod> mods)
{
    bool loop = true;
    while (loop)
    {
        for (int i = 0; i < mods.Count; i++)
        {
            Mod m = mods[i];
            Console.WriteLine(i + 1 + ". " + m.Name + ": " + m.Description);
        }
        Console.WriteLine(">Please enter the number of any mod you don't want to download. If there are none, press Enter.");
        Thread.Sleep(500);
        try
        {
            int mn = Int32.Parse(Console.ReadLine()) - 1;
            Console.WriteLine(">Removed " + mods[mn].Name);
            mods.Remove(mods[mn]);
        }
        catch
        {
            Console.WriteLine(">Are you done?");
            if (yn(Console.ReadLine()))
            {
                loop = false;
            }
            else
            {
                continue;
            }
        }
    }
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

static bool yn(String str)
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
static void clean(List<Mod> list)
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
public class Mod
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Versions { get; set; }
    public Mod(string id,string name, string description, List<string> versions)
    {
        Name = name;
        Description = description;
        Versions = versions;
        Id = id;
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
public class file
{
    public Hashes hashes { get; set; }
    public string url { get; set; }
    public string filename { get; set; }
    public bool primary { get; set; }
    public int size { get; set; }
    public object file_type { get; set; }
}

public class Hashes
{
    public string sha1 { get; set; }
    public string sha512 { get; set; }
}

public class Base
{
    public string id { get; set; }
    public string project_id { get; set; }
    public string author_id { get; set; }
    public bool featured { get; set; }
    public string name { get; set; }
    public string version_number { get; set; }
    public string changelog { get; set; }
    public object changelog_url { get; set; }
    public DateTime date_published { get; set; }
    public int downloads { get; set; }
    public string version_type { get; set; }
    public string status { get; set; }
    public object requested_status { get; set; }
    public List<file> files { get; set; }
    public List<object> dependencies { get; set; }
    public List<string> game_versions { get; set; }
    public List<string> loaders { get; set; }
}



