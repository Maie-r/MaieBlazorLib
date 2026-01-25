using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace MaieBlazorLib.LocalTierLister
{
    class TierListSaveData
    {
        public List<TierListDTO> TierLists { get; set; }


        public TierListSaveData()
        {
            TierLists = new();
        }
        public TierListSaveData(List<TierList> TierLists) 
        {
            this.TierLists = new List<TierListDTO>();
            foreach (TierList tl in TierLists)
                this.TierLists.Add(TierListDTO.ToDTO(tl));
        }

        public static void Save(TierListSaveData saveData, string filename)
        {
            string folder = GetFolder();
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string jsonString = JsonSerializer.Serialize(saveData, options);
            File.WriteAllText(Path.Combine(folder, filename + ".json"), jsonString);
        }

        public void Save(string filename)
        {
            try
            {
                Save(this, filename);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving TierListSaveData to file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load from a local file with filename
        /// </summary>
        /// <returns>Converted List<TierList></returns>
        public static List<TierList> LoadFrom(string filename)
        {
            try
            {
                var path = Path.Combine(GetFolder(), filename + ".json");
                string jsonString = File.ReadAllText(path);
                return LoadFromRaw(jsonString);
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"File not found: {filename}.json");
                throw new FileNotFoundException($"File not found: {filename}.json");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading TierListSaveData from file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load from a Json that represents a list of Tier Lists (DTOS)
        /// </summary>
        /// <returns>Converted List<TierList></returns>
        public static List<TierList> LoadFromRaw(string DataJson)
        {
            try
            {
                DataJson = ParseJson(DataJson);
                TierListSaveData? data = JsonSerializer.Deserialize<TierListSaveData>(DataJson, options);
                List<TierList> tierLists = new List<TierList>();
                if (data != null)
                {
                    foreach (TierListDTO tlDTO in data.TierLists)
                        tierLists.Add(TierListDTO.ToObject(tlDTO));
                }
                return tierLists;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading TierListSaveData from file: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Parses the specified JSON string and ensures it conforms to the expected TierLists format.
        /// </summary>
        /// <remarks>If the input is a single TierList object or an array of TierList objects, the method
        /// wraps it in an object with a 'TierLists' property. If the input already contains a 'TierLists' array at the
        /// root, it is returned unchanged. If the input does not match any of these formats, the method returns the
        /// original string after logging an error.</remarks>
        /// <param name="importString">A JSON string representing either a single TierList object, an array of TierList objects, or an object
        /// containing a 'TierLists' array.</param>
        /// <returns>A JSON string formatted with a top-level 'TierLists' array containing the input data, or the original string
        /// if it already matches the expected format.</returns>
        static string ParseJson(string importString)
        {
            string result = importString;
            try
            {
                using JsonDocument doc = JsonDocument.Parse(result);

                var root = doc.RootElement;
                var rootkind = doc.RootElement.ValueKind;

                if (rootkind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("TierLists", out JsonElement tierlists))
                        if (tierlists.ValueKind == JsonValueKind.Array)
                            return result;
                        else
                            throw new InvalidOperationException("Couldn't parse Json: 'TierLists' Object array does not contain an Array");
                    else if (root.TryGetProperty("name", out JsonElement n) && root.TryGetProperty("tiers", out JsonElement t))
                    {
                        return $"{{\"TierLists\":[{result}]}}";
                    }
                    else
                        throw new InvalidOperationException("Couldn't parse Json: Root Object is not a TierList or TierLists array");
                }
                if (rootkind == JsonValueKind.Array)
                {
                    return $"{{\"TierLists\":{result}}}";
                }

                throw new InvalidOperationException("Couldn't parse Json: Root is not an Object or Array");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unhandled Exception: " + e.Message);
            }
            return result;
        }

        public static string ExportTierList(List<TierList> lists)
        {
            TierListSaveData saveData = new TierListSaveData(lists);
            string jsonString = JsonSerializer.Serialize(saveData, options);
            return jsonString;
        }

        public static string ExportTierList(TierList list)
        {
            TierListSaveData saveData = new TierListSaveData(new List<TierList>() { list });
            string jsonString = JsonSerializer.Serialize(saveData, options);
            return jsonString;
        }

        public static string ExportTierList(TierList[] lists)
        {
            TierListSaveData saveData = new TierListSaveData(new List<TierList>(lists));
            string jsonString = JsonSerializer.Serialize(saveData, options);
            return jsonString;
        }

        static string GetFolder()
        {
            return Path.Join(FileSystem.AppDataDirectory, "TierList");
        }

        static JsonSerializerOptions options = new()
        {
            //ReferenceHandler = ReferenceHandler.Preserve,
            //DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IncludeFields = true,
            WriteIndented = true
        };
    }

    class TierListDTO
    {
        public string name { get; set; }
        public List<TierDTO> tiers { get; set; }

        public static TierListDTO ToDTO(TierList tierlist)
        {
            TierListDTO tlDTO = new TierListDTO();
            tlDTO.name = tierlist.name;
            tlDTO.tiers = new List<TierDTO>();
            foreach (var tierPair in tierlist.tiers)
                tlDTO.tiers.Add(TierDTO.ToDTO(tierPair.Value));

            return tlDTO;
        }

        public static TierList ToObject(TierListDTO tierlistDTO)
        {
            TierList tl = new TierList(tierlistDTO.name);
            tl.tiers = new Dictionary<string, Tier>();
            foreach (TierDTO tDTO in tierlistDTO.tiers)
            {
                Tier t = TierDTO.ToObject(tDTO);
                tl.tiers.Add(t.name, t);
            }
            return tl;
        }

        public TierList ToObject()
        {
            try
            {
                return TierListDTO.ToObject(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting TierListDTO to TierList: {ex.Message}");
                throw;
            }
        }
    }

    class TierDTO
    {
        public string name { get; set; }
        public string color { get; set; }
        public List<TierItemDTO> items { get; set; }

        static public TierDTO ToDTO(Tier tier)
        {
            TierDTO tDTO = new TierDTO();
            tDTO.name = tier.name;
            tDTO.color = tier.color;
            tDTO.items = new List<TierItemDTO>();
            foreach (TierItem ti in tier.items)
                tDTO.items.Add(TierItemDTO.ToDTO(ti));

            return tDTO;
        }

        static public Tier ToObject(TierDTO tierDTO)
        {
            Tier t = new Tier(tierDTO.name, tierDTO.color);
            t.items = new List<TierItem>();
            foreach (TierItemDTO tiDTO in tierDTO.items)
            {
                var ti = TierItemDTO.ToObject(tiDTO);
                ti.parent = t;
                t.items.Add(ti);
            }
            return t;
        }

        public Tier ToObject()
        {
            try
            {
                return TierDTO.ToObject(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting TierDTO to Tier: {ex.Message}");
                throw;
            }
        }
    }

    class TierItemDTO
    {
        public string name { get; set; }
        public string img { get; set; }

        public static TierItemDTO ToDTO(TierItem tierItem)
        {
            TierItemDTO tiDTO = new TierItemDTO();
            tiDTO.name = tierItem.name;
            tiDTO.img = tierItem.img;
            return tiDTO;
        }

        public static TierItem ToObject(TierItemDTO tierItemDTO)
        {
            TierItem ti = new TierItem();
            ti.name = tierItemDTO.name;
            ti.img = tierItemDTO.img;
            return ti;
        }

        public TierItem ToObject()
        {
            try
            {
                return TierItemDTO.ToObject(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting TierItemDTO to TierItem: {ex.Message}");
                throw;
            }
        }
    }
}
