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

        public static List<TierList> LoadFrom(string filename)
        {
            var path = Path.Combine(GetFolder(), filename + ".json");
            string jsonString = File.ReadAllText(path);
            TierListSaveData? data = JsonSerializer.Deserialize<TierListSaveData>(jsonString, options);
            List<TierList> tierLists = new List<TierList>();
            if (data != null)
            {
                foreach (TierListDTO tlDTO in data.TierLists)
                    tierLists.Add(TierListDTO.ToObject(tlDTO));
            }
            return tierLists;
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
