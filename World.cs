﻿using System; 
using SadRogue.Primitives;
using System.Collections.Generic;

using LofiHollow.Entities;
using System.IO;
using Newtonsoft.Json;
using LofiHollow.Entities.NPC;
using LofiHollow.Managers;
using System.Linq;
using LofiHollow.Minigames.Mining;
using LofiHollow.EntityData;
using LofiHollow.Missions;
using LofiHollow.DataTypes;
using Steamworks;

namespace LofiHollow {
    public class World { 
        public Dictionary<string, Tile> tileLibrary = new();
        public Dictionary<string, MineTile> mineTileLibrary = new();
        public Dictionary<string, Item> itemLibrary = new();
        public Dictionary<string, Monster> monsterLibrary = new();
        public Dictionary<Point3D, Map> maps = new();
        public Dictionary<string, Skill> skillLibrary = new();
        public Dictionary<string, NPC> npcLibrary = new();
        public Dictionary<string, FishDef> fishLibrary = new();
        public Dictionary<int, Constructible> constructibles = new();
        public Dictionary<int, CraftingRecipe> recipeLibrary = new();
        public Dictionary<string, Mission> missionLibrary = new();
        public Dictionary<string, string> scriptLibrary = new();

        public List<Chapter> Chapters = new();


        public Dictionary<CSteamID, Player> otherPlayers = new();


        public bool DoneInitializing = false;
         
        public Player Player { get; set; }

         
        public World() {
            LoadSkillDefinitions();
            LoadTileDefinitions();
            LoadItemDefinitions();
            LoadFishDefinitions();
            LoadMonsterDefinitions();
            LoadNPCDefinitions();
            LoadConstructibles();
            LoadMineTiles();
            LoadCraftingRecipes();
            LoadMissionDefinitions();
            LoadScripts();
        }

        public void InitPlayer() {
            CreatePlayer();
            CreateMap(Player.MapPos);

            if (Directory.Exists("./data/")) {
                string json = System.IO.File.ReadAllText("./data/minimap.dat");
                Dictionary<string, MinimapTile> minimap = JsonConvert.DeserializeObject<Dictionary<string, MinimapTile>>(json);

                foreach (KeyValuePair<string, MinimapTile> kv in minimap) {
                    string posString = kv.Key[1..^1];
                    string[] coords = posString.Split(",");
                    Point3D pos = new(Int32.Parse(coords[0]), Int32.Parse(coords[1]), Int32.Parse(coords[2]));
                    GameLoop.UIManager.Sidebar.minimap.Add(pos, kv.Value);
                }
            } 
        }

        public void LoadScripts() {
            if (Directory.Exists("./data/scripts/")) {
                string[] scriptFiles = Directory.GetFiles("./data/scripts/");

                foreach (string fileName in scriptFiles) {
                    string script = File.ReadAllText(fileName);
                    string name = fileName.Split("/")[^1][0..^4];
                    scriptLibrary.Add(name, script);
                }
            }
        }

        public void LoadItemDefinitions() {
            if (Directory.Exists("./data/items/")) {
                string[] itemFiles = Directory.GetFiles("./data/items/");

                foreach (string fileName in itemFiles) {
                    string json = File.ReadAllText(fileName);
                    Item item = JsonConvert.DeserializeObject<Item>(json);
                    itemLibrary.Add(item.FullName(), item); 
                }
            } 
        }

        public void LoadFishDefinitions() {
            if (Directory.Exists("./data/fish/")) {
                string[] tileFiles = Directory.GetFiles("./data/fish/");

                foreach (string fileName in tileFiles) {
                    string json = File.ReadAllText(fileName);

                    FishDef fish = JsonConvert.DeserializeObject<FishDef>(json);

                    fishLibrary.Add(fish.PackageID + ":" + fish.Name, fish);
                }
            } 
        }

        public void LoadConstructibles() {
            if (Directory.Exists("./data/constructibles/")) {
                string[] tileFiles = Directory.GetFiles("./data/constructibles/");

                List<Constructible> presort = new List<Constructible>();

                foreach (string fileName in tileFiles) {
                    string json = File.ReadAllText(fileName);

                    Constructible con = JsonConvert.DeserializeObject<Constructible>(json);

                    presort.Add(con);
                }

                List<Constructible> sorted = presort.OrderBy(o => o.RequiredLevel).ThenBy(o => o.Name).ToList();

                for (int i = 0; i < sorted.Count; i++) {
                    constructibles.Add(i, sorted[i]);
                }
            }
        }

        public void LoadMineTiles() {
            if (Directory.Exists("./data/mine/tiles/")) {
                string[] tileFiles = Directory.GetFiles("./data/mine/tiles/");

                foreach (string fileName in tileFiles) {
                    string json = File.ReadAllText(fileName);

                    MineTile tile = JsonConvert.DeserializeObject<MineTile>(json);

                    mineTileLibrary.Add(tile.Name, tile);
                }
            }

        }


        public void LoadCraftingRecipes() { 
            if (Directory.Exists("./data/recipes/")) {
                string[] tileFiles = Directory.GetFiles("./data/recipes/");

                List<CraftingRecipe> presort = new List<CraftingRecipe>();

                foreach (string fileName in tileFiles) {
                    string json = File.ReadAllText(fileName);

                    CraftingRecipe rec = JsonConvert.DeserializeObject<CraftingRecipe>(json);

                    presort.Add(rec);
                }

                List<CraftingRecipe> sorted = presort.OrderBy(o => o.RequiredLevel).ThenBy(o => o.Name).ToList();

                for (int i = 0; i < sorted.Count; i++) {
                    recipeLibrary.Add(i, sorted[i]);
                }
            }
        }

        public void LoadMissionDefinitions() {
            if (Directory.Exists("./data/missions/")) {
                string[] tileFiles = Directory.GetFiles("./data/missions/");

                foreach (string fileName in tileFiles) {
                    string json = File.ReadAllText(fileName);

                    Mission mission = JsonConvert.DeserializeObject<Mission>(json);

                    missionLibrary.Add(mission.Package + ":" + mission.Name, mission);
                }

                string chapterJson = System.IO.File.ReadAllText("./data/chapters.dat");
                Chapters = JsonConvert.DeserializeObject<List<Chapter>>(chapterJson);
            } 
        }




        public void LoadTileDefinitions() {
            if (Directory.Exists("./data/tiles/")) {
                string[] tileFiles = Directory.GetFiles("./data/tiles/");

                foreach (string fileName in tileFiles) {
                    string json = File.ReadAllText(fileName);

                    Tile tile = JsonConvert.DeserializeObject<Tile>(json);

                    tileLibrary.Add(tile.FullName(), tile);
                }
            }

            /*
            string path = "./data/tiles/" + tile.Name + ".dat";
            using StreamWriter output = new StreamWriter(path);
            string jsonString = JsonConvert.SerializeObject(tile, Formatting.Indented);
            output.WriteLine(jsonString);
            output.Close(); 
            */
        }


        public void LoadNPCDefinitions() {
            if (Directory.Exists("./data/npcs/")) {
                string[] tileFiles = Directory.GetFiles("./data/npcs/");

                foreach (string fileName in tileFiles) {
                    string json = File.ReadAllText(fileName);
                    NPC npc = JsonConvert.DeserializeObject<NPC>(json);

                    if (npc.Name != "")
                        npcLibrary.Add(npc.Name, npc);
                }
            }

        }

        public void LoadSkillDefinitions() { 
            if (Directory.Exists("./data/skills/")) {
                string[] skillFiles = Directory.GetFiles("./data/skills/");

                foreach (string fileName in skillFiles) {
                    string json = File.ReadAllText(fileName);

                    Skill skill = JsonConvert.DeserializeObject<Skill>(json);

                    skillLibrary.Add(skill.Name, skill);
                }
            } 
        }


        public void LoadMonsterDefinitions() {
            if (Directory.Exists("./data/monsters/")) {
                string[] monsterFiles = Directory.GetFiles("./data/monsters/");

                foreach (string fileName in monsterFiles) {
                    string json = File.ReadAllText(fileName);

                    Monster monster = JsonConvert.DeserializeObject<Monster>(json);

                    monsterLibrary.Add(monster.Package + ":" + monster.Name, monster);
                }
            }
        }

        public bool LoadMapAt(Point3D mapPos, bool JustGiveBackMap = false) {
            if (Directory.Exists("./data/maps/" + mapPos.WorldArea + "/")) {
                if (File.Exists("./data/maps/" + mapPos.WorldArea + "/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat.gz")) {
                    Map map = Helper.DeserializeFromFileCompressed<Map>("./data/maps/" + mapPos.WorldArea + "/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat.gz");
                    if (!maps.ContainsKey(mapPos)) {
                        maps.Add(mapPos, map);
                    } else {
                        maps[mapPos] = map;
                    }

                    if (mapPos != new Point3D(-1, 0, 0))
                        return true;
                }
            }

            return false;
        }

        public Map GetMap(Point3D mapPos) {
            if (Directory.Exists("./data/maps/" + mapPos.WorldArea + "/")) {
                if (File.Exists("./data/maps/" + mapPos.WorldArea + "/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat.gz")) {
                    return Helper.DeserializeFromFileCompressed<Map>("./data/maps/" + mapPos.WorldArea + "/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat.gz");
                }
            }

            if (maps.ContainsKey(mapPos))
                return maps[mapPos];

            return null;
        }

        public Map UnchangedMap(Point3D mapPos) {
            if (Directory.Exists("./data/maps/" + mapPos.WorldArea + "/")) {
                if (File.Exists("./data/maps/" + mapPos.WorldArea + "/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat")) {
                    string json = System.IO.File.ReadAllText("./data/maps/" + mapPos.WorldArea + "/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat");
                    Map map = JsonConvert.DeserializeObject<Map>(json);

                    return map;
                }
            }

            return null;
        }

        public void LoadPlayerFarm() {
            string farmPath = "./saves/" + GameLoop.World.Player.Name + "/farm.dat.gz";
            if (File.Exists(farmPath)) {  
                Map map = Helper.DeserializeFromFileCompressed<Map>(farmPath);

                Point3D farmPos = new(-1, 0, 0); 

                for (int i = 0; i < map.Tiles.Length; i++) {
                    if (map.Tiles[i].Lock != null) {  
                        if (!map.Tiles[i].Lock.Closed) {
                            map.Tiles[i].Glyph = map.Tiles[i].Lock.OpenedGlyph;
                            map.Tiles[i].IsBlockingMove = map.Tiles[i].Lock.OpenBlocksMove;
                            map.Tiles[i].IsBlockingLOS = false;
                        } else {
                            map.Tiles[i].Glyph = map.Tiles[i].Lock.ClosedGlyph;
                            map.Tiles[i].IsBlockingMove = true;
                            map.Tiles[i].IsBlockingLOS = map.Tiles[i].Lock.ClosedBlocksLOS;
                        }
                    }
                }


                if (!maps.ContainsKey(farmPos)) {
                    maps.Add(farmPos, map);
                } else {
                    maps[farmPos] = map;
                } 
            }
        }

        public void SaveMapToFile(Point3D pos) {
            if (maps.ContainsKey(pos)) {
                Map map = maps[pos];

                if (!Directory.Exists("./data/maps/" + pos.WorldArea + "/"))
                    Directory.CreateDirectory("./data/maps/" + pos.WorldArea + "/");

                string path = "./data/maps/" + pos.WorldArea + "/" + pos.X + "," + pos.Y + "," + pos.Z + ".dat.gz";
                Helper.SerializeToFileCompressed(map, path);
            }
        }

        public void SavePlayer() {
            System.IO.Directory.CreateDirectory("./saves/" + Player.Name + "/");
            string path = "./saves/" + Player.Name + "/player.dat.gz";
            Helper.SerializeToFileCompressed(Player, path);

            if (Player.OwnsFarm) {
                string farmPath = "./saves/" + Player.Name + "/farm.dat.gz";

                if (!maps.ContainsKey(new(-1, 0, 0)))
                    LoadMapAt(new(-1, 0, 0));

                Helper.SerializeToFileCompressed(maps[new(-1, 0, 0)], farmPath); 
            }

            if (GameLoop.SingleOrHosting()) {
                string timePath = "./saves/" + Player.Name + "/time.dat";

                using StreamWriter timeOutput = new(timePath);
                string timeJson = JsonConvert.SerializeObject(GameLoop.World.Player.Clock, Formatting.Indented);
                timeOutput.WriteLine(timeJson);
                timeOutput.Close();

                string monsterPens = "./saves/" + Player.Name + "/monsterPens.dat";

                using StreamWriter monsterOutput = new(monsterPens);
                string monsterJson = JsonConvert.SerializeObject(GameLoop.UIManager.Minigames.MonsterPenManager, Formatting.Indented);
                monsterOutput.WriteLine(monsterJson);
                monsterOutput.Close();
            }
        }

        public void LoadPlayer(string playerName) {  

            if (Directory.Exists("./saves/" + playerName + "/")) {
                string[] playerSaves = Directory.GetFiles("./saves/" + playerName + "/");

                foreach (string fileName in playerSaves) {
                    string json = File.ReadAllText(fileName);

                    string[] name = fileName.Split("/");

                    if (name[^1] == "player.dat.gz") {
                        Player = Helper.DeserializeFromFileCompressed<Player>(fileName);
                    }

                    if (name[^1] == "time.dat") {
                        Player.Clock = JsonConvert.DeserializeObject<TimeManager>(json);
                    }

                    if (name[^1] == "monsterPens.dat") {
                        GameLoop.UIManager.Minigames.MonsterPenManager = JsonConvert.DeserializeObject<MonsterPenManager>(json);
                    }
                }
            }

            if (Player.OwnsFarm) {
                LoadPlayerFarm();
            }

            foreach (KeyValuePair<string, Skill> kv in skillLibrary) {
                Skill skill = new(kv.Value);
                if (!Player.Skills.ContainsKey(skill.Name))
                    Player.Skills.Add(skill.Name, skill);
            }

            foreach (KeyValuePair<string, Mission> kv in missionLibrary) {
                if (!Player.MissionLog.ContainsKey(kv.Key))
                    Player.MissionLog.Add(kv.Key, kv.Value);
            }


            GameLoop.UIManager.Photo.PopulateJobList();

            Player.UsePixelPositioning = true; 
            DoneInitializing = true;

            Player.DayStart = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

            Map map = Helper.ResolveMap(Player.MapPos); 
            if (map != null) {
                GameLoop.UIManager.Map.LoadMap(map);
            } 
        }



        public void CreateMap(Point3D pos) {
            if (!maps.ContainsKey(pos)) {
                if (!LoadMapAt(pos)) {
                    Map newMap = new(GameLoop.MapWidth, GameLoop.MapHeight);

                    if (pos.Z < 0) {
                        for (int i = 0; i < newMap.Tiles.Length; i++) {
                            newMap.Tiles[i] = new("lh:Empty");
                        }

                        if (Player.MapPos.Z > pos.Z) {
                            newMap.Tiles[Player.Position.ToIndex(GameLoop.MapWidth)] = new("lh:Up Stairs");
                        }
                    }

                    if (pos.Z > 0) {
                        for (int i = 0; i < newMap.Tiles.Length; i++) {
                            newMap.Tiles[i] = new("lh:Space");
                        }

                        if (Player.MapPos.Z < pos.Z) {
                            newMap.Tiles[Player.Position.ToIndex(GameLoop.MapWidth)] = new("lh:Down Stairs");
                        }
                    }

                    if (!maps.ContainsKey(pos))
                        maps.Add(pos, newMap);
                }
            }
        } 

        public void CreatePlayer() {
            Player = new(Color.Yellow);
            Player.Position = new(25, 25);
            Player.MapPos = new(3, 1, 0);
            Player.Name = "Player";

            Player.UsePixelPositioning = true;

            GameLoop.UIManager.Map.LoadMap(Player.MapPos);
          //  GameLoop.UIManager.Map.EntityRenderer.Add(Player);
            Player.ZIndex = 10;

            Player.MaxHP = 0;
            Player.CurrentHP = Player.MaxHP;
            
        }

        public void FreshStart() {
            Player.MaxHP = 10;
            Player.CurrentHP = Player.MaxHP;
            LoadMapAt(Player.MapPos);

            DoneInitializing = true;
            Player.Inventory[0] = new Item("lh:Rusty Dagger");

            Player.Skills = new Dictionary<string, Skill>();
            
            foreach (KeyValuePair<string, Skill> kv in skillLibrary) {
                Skill skill = new(kv.Value);
                Player.Skills.Add(skill.Name, skill);
            } 

            foreach (KeyValuePair<string, Mission> kv in missionLibrary) {
                Player.MissionLog.Add(kv.Key, kv.Value);
            }

            SavePlayer();
            LoadPlayer(Player.Name);

            GameLoop.UIManager.AddMsg("Press F1 for help at any time, or ? to view hotkeys.");
        }
    }
}
