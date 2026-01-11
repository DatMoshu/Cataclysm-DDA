using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CataMapGen.Mapgen.Data;

namespace CataMapGen.Mapgen
{
    /// <summary>
    /// Loads CDDA-format JSON mapgen files into the registry
    /// Parses both mapgen definitions and palettes
    /// </summary>
    public class JsonMapgenLoader
    {
        private readonly MapgenRegistry _registry;

        public JsonMapgenLoader(MapgenRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>
        /// Load all JSON files from a directory
        /// </summary>
        public void LoadFromDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            foreach (var file in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    LoadFromFile(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Load a single JSON file
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            LoadFromJson(json);
        }

        /// <summary>
        /// Load from JSON string
        /// </summary>
        public void LoadFromJson(string json)
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            // CDDA files are usually arrays of objects
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    ParseElement(element);
                }
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                ParseElement(doc.RootElement);
            }
        }

        private void ParseElement(JsonElement element)
        {
            if (!element.TryGetProperty("type", out var typeProp))
                return;

            string type = typeProp.GetString();

            switch (type)
            {
                case "mapgen":
                    var def = ParseMapgenDefinition(element);
                    if (def != null)
                        _registry.RegisterDefinition(def);
                    break;

                case "palette":
                    var palette = ParsePalette(element);
                    if (palette != null)
                        _registry.RegisterPalette(palette);
                    break;
            }
        }

        private MapgenDefinition ParseMapgenDefinition(JsonElement element)
        {
            var def = new MapgenDefinition();

            if (element.TryGetProperty("method", out var methodProp))
                def.Method = methodProp.GetString();

            if (element.TryGetProperty("om_terrain", out var omProp))
                def.OmTerrain = omProp.GetString();

            if (element.TryGetProperty("weight", out var weightProp))
                def.Weight = weightProp.GetInt32();

            if (element.TryGetProperty("//", out var commentProp))
                def.Comment = commentProp.GetString();

            if (element.TryGetProperty("object", out var objProp))
                def.Object = ParseMapgenObject(objProp);

            return def;
        }

        private MapgenObject ParseMapgenObject(JsonElement element)
        {
            var obj = new MapgenObject();

            if (element.TryGetProperty("fill_ter", out var fillProp))
                obj.FillTer = fillProp.GetString();

            if (element.TryGetProperty("rows", out var rowsProp))
                obj.Rows = ParseStringArray(rowsProp);

            if (element.TryGetProperty("palettes", out var palettesProp))
                obj.Palettes = ParseStringList(palettesProp);

            if (element.TryGetProperty("terrain", out var terrainProp))
                obj.Terrain = ParseWeightedDict(terrainProp);

            if (element.TryGetProperty("furniture", out var furnProp))
                obj.Furniture = ParseWeightedDict(furnProp);

            if (element.TryGetProperty("items", out var itemsProp))
                obj.Items = ParseItemDict(itemsProp);

            if (element.TryGetProperty("place_loot", out var lootProp))
                obj.PlaceLoot = ParsePlaceLoot(lootProp);

            if (element.TryGetProperty("place_monsters", out var monsterProp))
                obj.PlaceMonsters = ParsePlaceMonsters(monsterProp);

            if (element.TryGetProperty("place_nested", out var nestedProp))
                obj.PlaceNested = ParsePlaceNested(nestedProp);

            return obj;
        }

        private MapgenPalette ParsePalette(JsonElement element)
        {
            var palette = new MapgenPalette();

            if (element.TryGetProperty("id", out var idProp))
                palette.Id = idProp.GetString();

            if (element.TryGetProperty("palettes", out var palettesProp))
                palette.Palettes = ParseStringList(palettesProp);

            if (element.TryGetProperty("terrain", out var terrainProp))
                palette.Terrain = ParseWeightedDict(terrainProp);

            if (element.TryGetProperty("furniture", out var furnProp))
                palette.Furniture = ParseWeightedDict(furnProp);

            if (element.TryGetProperty("items", out var itemsProp))
                palette.Items = ParseItemDict(itemsProp);

            return palette;
        }

        private string[] ParseStringArray(JsonElement element)
        {
            var list = new List<string>();
            foreach (var item in element.EnumerateArray())
                list.Add(item.GetString());
            return list.ToArray();
        }

        private List<string> ParseStringList(JsonElement element)
        {
            var list = new List<string>();
            foreach (var item in element.EnumerateArray())
            {
                // Can be string or object with "param" property
                if (item.ValueKind == JsonValueKind.String)
                    list.Add(item.GetString());
            }
            return list;
        }

        private Dictionary<char, WeightedEntry<string>> ParseWeightedDict(JsonElement element)
        {
            var dict = new Dictionary<char, WeightedEntry<string>>();

            foreach (var prop in element.EnumerateObject())
            {
                if (string.IsNullOrEmpty(prop.Name)) continue;
                char symbol = prop.Name[0];

                var entry = ParseWeightedEntry(prop.Value);
                dict[symbol] = entry;
            }

            return dict;
        }

        private WeightedEntry<string> ParseWeightedEntry(JsonElement element)
        {
            var entry = new WeightedEntry<string>();

            if (element.ValueKind == JsonValueKind.String)
            {
                // Simple string value
                entry.Options.Add((element.GetString(), 100));
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                // Array = weighted list: [["value", weight], "value2", ...]
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        entry.Options.Add((item.GetString(), 1));
                    }
                    else if (item.ValueKind == JsonValueKind.Array)
                    {
                        var arr = item.EnumerateArray();
                        string val = null;
                        int weight = 1;
                        int idx = 0;
                        foreach (var subItem in item.EnumerateArray())
                        {
                            if (idx == 0)
                                val = subItem.GetString();
                            else if (idx == 1 && subItem.ValueKind == JsonValueKind.Number)
                                weight = subItem.GetInt32();
                            idx++;
                        }
                        if (val != null)
                            entry.Options.Add((val, weight));
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                // Object with param/fallback - simplified handling
                if (element.TryGetProperty("fallback", out var fallback))
                    entry.Options.Add((fallback.GetString(), 100));
            }

            return entry;
        }

        private Dictionary<char, List<ItemSpawnRule>> ParseItemDict(JsonElement element)
        {
            var dict = new Dictionary<char, List<ItemSpawnRule>>();

            foreach (var prop in element.EnumerateObject())
            {
                if (string.IsNullOrEmpty(prop.Name)) continue;
                char symbol = prop.Name[0];

                var rules = ParseItemRules(prop.Value);
                dict[symbol] = rules;
            }

            return dict;
        }

        private List<ItemSpawnRule> ParseItemRules(JsonElement element)
        {
            var rules = new List<ItemSpawnRule>();

            if (element.ValueKind == JsonValueKind.Object)
            {
                rules.Add(ParseSingleItemRule(element));
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                        rules.Add(ParseSingleItemRule(item));
                }
            }

            return rules;
        }

        private ItemSpawnRule ParseSingleItemRule(JsonElement element)
        {
            var rule = new ItemSpawnRule();

            if (element.TryGetProperty("item", out var itemProp))
                rule.Item = itemProp.GetString();

            if (element.TryGetProperty("chance", out var chanceProp))
                rule.Chance = chanceProp.GetInt32();

            if (element.TryGetProperty("repeat", out var repeatProp))
            {
                if (repeatProp.ValueKind == JsonValueKind.Array)
                {
                    var arr = repeatProp.EnumerateArray();
                    int idx = 0;
                    foreach (var item in repeatProp.EnumerateArray())
                    {
                        if (idx == 0) rule.RepeatMin = item.GetInt32();
                        else if (idx == 1) rule.RepeatMax = item.GetInt32();
                        idx++;
                    }
                }
                else if (repeatProp.ValueKind == JsonValueKind.Number)
                {
                    rule.RepeatMin = rule.RepeatMax = repeatProp.GetInt32();
                }
            }

            return rule;
        }

        private List<PlaceLootRule> ParsePlaceLoot(JsonElement element)
        {
            var rules = new List<PlaceLootRule>();

            foreach (var item in element.EnumerateArray())
            {
                var rule = new PlaceLootRule();

                if (item.TryGetProperty("item", out var itemProp))
                    rule.Item = itemProp.GetString();

                if (item.TryGetProperty("x", out var xProp))
                    rule.X = xProp.GetInt32();

                if (item.TryGetProperty("y", out var yProp))
                    rule.Y = yProp.GetInt32();

                rules.Add(rule);
            }

            return rules;
        }

        private List<MonsterSpawnRule> ParsePlaceMonsters(JsonElement element)
        {
            var rules = new List<MonsterSpawnRule>();

            foreach (var item in element.EnumerateArray())
            {
                var rule = new MonsterSpawnRule();

                if (item.TryGetProperty("monster", out var mProp))
                    rule.Monster = mProp.GetString();

                // x can be int or [min, max]
                if (item.TryGetProperty("x", out var xProp))
                {
                    if (xProp.ValueKind == JsonValueKind.Number)
                    {
                        rule.XMin = rule.XMax = xProp.GetInt32();
                    }
                    else if (xProp.ValueKind == JsonValueKind.Array)
                    {
                        int idx = 0;
                        foreach (var v in xProp.EnumerateArray())
                        {
                            if (idx == 0) rule.XMin = v.GetInt32();
                            else if (idx == 1) rule.XMax = v.GetInt32();
                            idx++;
                        }
                    }
                }

                if (item.TryGetProperty("y", out var yProp))
                {
                    if (yProp.ValueKind == JsonValueKind.Number)
                    {
                        rule.YMin = rule.YMax = yProp.GetInt32();
                    }
                    else if (yProp.ValueKind == JsonValueKind.Array)
                    {
                        int idx = 0;
                        foreach (var v in yProp.EnumerateArray())
                        {
                            if (idx == 0) rule.YMin = v.GetInt32();
                            else if (idx == 1) rule.YMax = v.GetInt32();
                            idx++;
                        }
                    }
                }

                if (item.TryGetProperty("repeat", out var repeatProp) && repeatProp.ValueKind == JsonValueKind.Array)
                {
                    int idx = 0;
                    foreach (var v in repeatProp.EnumerateArray())
                    {
                        if (idx == 0) rule.RepeatMin = v.GetInt32();
                        else if (idx == 1) rule.RepeatMax = v.GetInt32();
                        idx++;
                    }
                }

                rules.Add(rule);
            }

            return rules;
        }

        private List<PlaceNestedRule> ParsePlaceNested(JsonElement element)
        {
            var rules = new List<PlaceNestedRule>();

            foreach (var item in element.EnumerateArray())
            {
                var rule = new PlaceNestedRule();

                if (item.TryGetProperty("chunks", out var chunksProp))
                {
                    foreach (var chunk in chunksProp.EnumerateArray())
                    {
                        if (chunk.ValueKind == JsonValueKind.String)
                        {
                            rule.Chunks.Add((chunk.GetString(), 1));
                        }
                        else if (chunk.ValueKind == JsonValueKind.Array)
                        {
                            string id = null;
                            int weight = 1;
                            int idx = 0;
                            foreach (var v in chunk.EnumerateArray())
                            {
                                if (idx == 0) id = v.GetString();
                                else if (idx == 1 && v.ValueKind == JsonValueKind.Number) weight = v.GetInt32();
                                idx++;
                            }
                            if (id != null)
                                rule.Chunks.Add((id, weight));
                        }
                    }
                }

                // x can be int or [min, max]
                if (item.TryGetProperty("x", out var xProp))
                {
                    if (xProp.ValueKind == JsonValueKind.Number)
                        rule.XMin = rule.XMax = xProp.GetInt32();
                    else if (xProp.ValueKind == JsonValueKind.Array)
                    {
                        int idx = 0;
                        foreach (var v in xProp.EnumerateArray())
                        {
                            if (idx == 0) rule.XMin = v.GetInt32();
                            else if (idx == 1) rule.XMax = v.GetInt32();
                            idx++;
                        }
                    }
                }

                if (item.TryGetProperty("y", out var yProp))
                {
                    if (yProp.ValueKind == JsonValueKind.Number)
                        rule.YMin = rule.YMax = yProp.GetInt32();
                    else if (yProp.ValueKind == JsonValueKind.Array)
                    {
                        int idx = 0;
                        foreach (var v in yProp.EnumerateArray())
                        {
                            if (idx == 0) rule.YMin = v.GetInt32();
                            else if (idx == 1) rule.YMax = v.GetInt32();
                            idx++;
                        }
                    }
                }

                rules.Add(rule);
            }

            return rules;
        }
    }
}
