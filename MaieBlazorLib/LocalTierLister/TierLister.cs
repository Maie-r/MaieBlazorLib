using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;

namespace MaieBlazorLib.LocalTierLister
{
    public class TierLister
    {
        public List<TierList> TierLists;
        public TierLister()
        {
            LoadStuff();
        }

        public TierLister(string link)
        {
            LoadStuff(link);
        }

        public static TierList? GetSpecificList(string link, string name)
        {
            if (File.Exists(link))
            {
                string[] tierlists = File.ReadAllText(link).Split(';');
                foreach (string tierlist in tierlists) // every tier list
                {
                    if (tierlist.Split("/-/")[0].Contains(name))
                        return ReadList(tierlist);
                }
            }
            /*StreamReader sw = new StreamReader(link);
            string line;
            for (line = sw.ReadLine(); !line.Contains(name) && line != null; line = sw.ReadLine());
            if (line != null) // if the list does not exist
            {
                string list = "\r\n" + line;
                while (!line.StartsWith(';') && line != null)
                {
                    Debug.WriteLine(line);
                    list += line;
                    line = sw.ReadLine();
                }
                list += "\r\n";
                return ReadList(list);
            }*/
            Debug.WriteLine($"Couldn't find list {name} at {link} !!");
            return null;
        }

        // DEPRECATED
        static async Task<List<TierList>?> LoadFrom(string fileName)
        {
            List<TierList> TierLists = new List<TierList>();

            var path = Path.Combine(GetFolder(), fileName);

            if (!File.Exists(path))
                return null;

            string contents = await File.ReadAllTextAsync(path);
            string[] tierlists = contents.Split(';');
            foreach (string tierlist in tierlists) // every tier list
            {
                TierList temp = ReadList(tierlist);
                TierLists.Add(temp);
            }
            return TierLists;
        }

        /* WINDOWS ONLY IMPLEMENTATION
        static List<TierList> LoadFrom(string link)
        {
            List<TierList> TierLists = new List<TierList>();

            if (!File.Exists(link))
            {
                return null;
            }
            string[] tierlists = File.ReadAllText(link).Split(';');
            foreach (string tierlist in tierlists) // every tier list
            {
                TierList temp = ReadList(tierlist);
                TierLists.Add(temp);
            }
            return TierLists;
        }*/

        async void LoadStuff()
        {
            Directory.CreateDirectory(GetFolder());
            await LoadStuff(Path.Join(GetFolder(), "lists"));
        }

        async Task LoadStuff(string filename)
        {
            try
            {
                TierLists = TierListSaveData.LoadFrom(filename);
                Debug.WriteLine(TierLists.Count);
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine("Attempting loading with legacy options...");
                // Legacy fallback
                try
                {
                    TierLists = new List<TierList>();
                    //Debug.WriteLine("loading from " + link);
                    var link = Path.Join(GetFolder(), "lists.txt");

                    if (!File.Exists(link))
                    {
                        await File.WriteAllTextAsync(link, string.Empty);
                    }
                    string content = await File.ReadAllTextAsync(link);
                    //Debug.WriteLine(content);
                    string[] tierlists = content.Split(';');
                    foreach (string tierlist in tierlists) // every tier list
                    {
                        //Debug.WriteLine("Ahoy!");
                        TierList temp = ReadList(tierlist);
                        TierLists.Add(temp);
                    }
                    Debug.WriteLine("Loaded legacy data and parsed it into current structure successfully!");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to load tierlists: " + ex.Message);
                    TierLists = new List<TierList>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load tierlists: " + ex.Message);
                TierLists = new List<TierList>();
            }

        }

        public List<TierList> LoadFromJson(string json)
        {
            return TierListSaveData.LoadFromRaw(json);
        }

        public static string ExportJson(TierLister tl)
        {
            return TierListSaveData.ExportTierList(tl.TierLists);
        }
        public static string ExportJson(TierList tl)
        {
            return TierListSaveData.ExportTierList(tl);
        }

        public string ExportJson()
        {
            return ExportJson(this);
        }

        /// <summary>
        /// It will only update by replacing a tierlist with the same name as the one in the parameter
        /// </summary>
        public static async void SaveSpecific(string link, TierList newtl)
        {
            List<TierList> TierLists = await LoadFrom(link);
            Debug.WriteLine(TierLists.Count);
            for (int j = 0; j < TierLists.Count; j++)
            {
                if (TierLists[j].name == newtl.name)
                {
                    Debug.WriteLine("Match found");
                    TierLists[j] = newtl;
                    break;
                }
            }
            string result = "";
            int i = 0;
            foreach (var list in TierLists) // each tier list
            {
                i++;
                result += list.name;
                foreach (Tier tier in list.tiers.Values)
                {
                    result += $"/-/{tier.name},{tier.color}\r\n";
                    foreach (var item in tier.items)
                    {
                        //Debug.WriteLine($"{item.name}, {item.img}");
                        result += $"{item.name},{item.img}" + "\r\n";
                    }
                }
                if (i < TierLists.Count)
                {
                    result += ";";
                }
            }
            await File.WriteAllTextAsync(link, result);
        }


        public async Task SaveAll()
        {
            try
            {
                SaveAllJson();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                try
                {
                    SaveAllLegacy();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to save tierlists after two attempts: " + ex.Message);
                }
            }
        }

        public async void SaveAllJson()
        {
            string folder = GetFolder();
            Directory.CreateDirectory(folder);
            string filePath = Path.Combine(folder, "lists");
            if (this.TierLists == null || this.TierLists.Count <= 0)
            {
                Debug.WriteLine("No Tierlists to save.");
                var emptysave = new TierListSaveData(new List<TierList>());
                emptysave.Save(filePath);
                return;
            }
            //var json = JsonSerializer.Serialize(championship, options);
            var saveData = new TierListSaveData(this.TierLists);
            saveData.Save(filePath);
        }

        static JsonSerializerOptions options = new()
        {
            //ReferenceHandler = ReferenceHandler.Preserve,
            //DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IncludeFields = true,
            WriteIndented = true
        };

        static string GetFolder()
        {
            return Path.Join(FileSystem.AppDataDirectory, "TierList");
        }

        // DEPRECATED ------------------------------------------
        static TierList ReadList(string tierlist)
        {
            string[] sep1 = tierlist.Split("/-/");
            TierList temp = new TierList(sep1[0]);
            for (int i = 1; i < sep1.Length; i++) // every tier
            {
                string[] sep2 = sep1[i].Split("\r\n");
                Tier temp2 = new Tier(sep2[0].Split(','));
                try
                {
                    temp.tiers.Add(temp2.name, temp2);
                }
                catch
                {
                    temp.tiers.Add(temp2.name + "(1)", temp2);
                }
                //Debug.WriteLine(sep2.Length);
                for (int j = 1; j < sep2.Length; j++) // every item
                {
                    if (sep2[j] == "") { break; }
                    temp.tiers[temp2.name].items.Add(new TierItem(sep2[j], temp2));
                }

            }
            return temp;
        }

        public async void SaveAllLegacy()
        {
            // Legacy fallback
            Debug.WriteLine("Saving");
            string folder = GetFolder();
            string result = "";
            int i = 0;
            foreach (var list in TierLists) // each tier list
            {
                i++;
                result += list.name;
                foreach (Tier tier in list.tiers.Values)
                {
                    result += $"/-/{tier.name},{tier.color}\r\n";
                    foreach (var item in tier.items)
                    {
                        //Debug.WriteLine($"{item.name}, {item.img}");
                        result += $"{item.name},{item.img}" + "\r\n";
                    }
                }
                if (i < TierLists.Count)
                {
                    result += ";";
                }

            }
            await File.WriteAllTextAsync(Path.Join(folder, "lists.txt"), result);
        }
    }

    public class TierList
    {
        public Dictionary<string, Tier> tiers;
        public string name;

        public TierList(string name)
        {
            this.name = name;
            tiers = new Dictionary<string, Tier>();
        }

        public TierList(string name, bool templatefill)
        {
            this.name = name;
            tiers = new Dictionary<string, Tier>();
            if (templatefill)
                TemplateTiers();
        }

        void TemplateTiers()
        {
            tiers.Add("S", new Tier("S", "#6603fcff"));
            tiers.Add("A", new Tier("A", "#d31cd9ff"));
            tiers.Add("B", new Tier("B", "#d92b59ff"));
            tiers.Add("C", new Tier("C", "#de6626ff"));
            tiers.Add("D", new Tier("D", "#e8b426ff"));
            tiers.Add("F", new Tier("F", "#6f6e78ff"));
        }

        public List<TierItem> GetAllItems()
        {
            List<TierItem> items = new List<TierItem>();
            foreach (Tier tier in tiers.Values)
            {
                items.AddRange(tier.items);
            }
            return items;
        }

        public string ExportJson()
        {
            return TierListSaveData.ExportTierList(this);
        }
    }

    public class Tier
    {
        public string name;
        public string ogname;
        public string color;
        public List<TierItem> items;
        public Tier(string name, string color)
        {
            this.name = name;
            this.ogname = name;
            this.color = color;
            items = new List<TierItem>();
        }

        public Tier(string[] data)
        {
            //Debug.WriteLine(data[0]);
            this.name = data[0];
            this.ogname = data[0];
            this.color = data[1];
            items = new List<TierItem>();
        }

        public void Add(TierItem item)
        {
            items.Add(item);
        }
    }

    public class TierItem
    {
        public string name { get; set; }
        public string img { get; set; }
        public Tier? parent { get; set; }

        public TierItem() { }

        public TierItem(string name) 
        {
            this.name = name;
            this.img = "";
        }

        public TierItem(string name, string img)
        {
            this.name = name;
            this.img = img;
        }

        //DEPRECATED
        public TierItem(string both, Tier parent)
        {
            string[] eh = both.Split(',');
            //Debug.WriteLine(both);
            name = eh[0];
            img = eh[1];
            this.parent = parent;
        }
    }
}
