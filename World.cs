using System;
using SadConsole.Components;
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

namespace LofiHollow {
    public class World { 
        public Dictionary<int, TileBase> tileLibrary = new();
        public Dictionary<int, MineTile> mineTileLibrary = new();
        public Dictionary<string, Item> itemLibrary = new();
        public Dictionary<int, Monster> monsterLibrary = new();
        public Dictionary<Point3D, Map> maps = new();
        public Dictionary<int, Skill> skillLibrary = new();
        public Dictionary<int, NPC> npcLibrary = new();
        public Dictionary<string, FishDef> fishLibrary = new();
        public Dictionary<int, Constructible> constructibles = new();
        public Dictionary<int, CraftingRecipe> recipeLibrary = new();
        public Dictionary<string, Mission> missionLibrary = new();

        public List<Chapter> Chapters = new();


        public Dictionary<long, Player> otherPlayers = new();


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

                    if (fish.RawFish != null) {
                        itemLibrary.Add(fish.RawFish.FullName(), fish.RawFish);
                    }
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

                    mineTileLibrary.Add(tile.MineTileID, tile);
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

                    TileBase tile = JsonConvert.DeserializeObject<TileBase>(json);

                    tileLibrary.Add(tile.TileID, tile);
                }
            }
            /*
            string path = "./data/tiles/" + tile.TileID + "," + tile.Name + ".dat";

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

                    npcLibrary.Add(npc.npcID, npc);
                }
            }

        }

        public void LoadSkillDefinitions() { 
            if (Directory.Exists("./data/skills/")) {
                string[] skillFiles = Directory.GetFiles("./data/skills/");

                foreach (string fileName in skillFiles) {
                    string json = File.ReadAllText(fileName);

                    Skill skill = JsonConvert.DeserializeObject<Skill>(json);

                    skillLibrary.Add(skill.SkillID, skill);
                }
            } 
        }


        public void LoadMonsterDefinitions() {
            if (Directory.Exists("./data/monsters/")) {
                string[] monsterFiles = Directory.GetFiles("./data/monsters/");

                foreach (string fileName in monsterFiles) {
                    string json = File.ReadAllText(fileName);

                    Monster monster = JsonConvert.DeserializeObject<Monster>(json);

                    monsterLibrary.Add(monster.MonsterID, monster);
                }
            }
        }

        public bool LoadMapAt(Point3D mapPos, bool JustGiveBackMap = false) {
            if (Directory.Exists("./data/maps/")) {
                if (File.Exists("./data/maps/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat")) {
                    string json = System.IO.File.ReadAllText("./data/maps/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat");
                    Map map = JsonConvert.DeserializeObject<Map>(json);
                     
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

        public Map UnchangedMap(Point3D mapPos) {
            if (Directory.Exists("./data/maps/")) {
                if (File.Exists("./data/maps/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat")) {
                    string json = System.IO.File.ReadAllText("./data/maps/" + mapPos.X + "," + mapPos.Y + "," + mapPos.Z + ".dat");
                    Map map = JsonConvert.DeserializeObject<Map>(json);

                    return map;
                }
            }

            return null;
        }

        public void LoadPlayerFarm() {
            if (File.Exists("./saves/" + GameLoop.World.Player.Name + "/farm.dat")) {
                string json = System.IO.File.ReadAllText("./saves/" + GameLoop.World.Player.Name + "/farm.dat");
                Map map = JsonConvert.DeserializeObject<Map>(json);

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

        public static void SaveMapToFile(Map map, Point3D pos) {
            string path = "./data/maps/" + pos.X + "," + pos.Y + "," + pos.Z + ".dat";

            using StreamWriter output = new(path); 
            string jsonString = JsonConvert.SerializeObject(map, Formatting.Indented);
            output.WriteLine(jsonString);
            output.Close();
        }

        public void SavePlayer() {
            System.IO.Directory.CreateDirectory("./saves/" + Player.Name + "/");
            string path = "./saves/" + Player.Name + "/player.dat";

            using StreamWriter output = new(path);
            string jsonString = JsonConvert.SerializeObject(Player, Formatting.Indented);
            output.WriteLine(jsonString);
            output.Close();

            if (GameLoop.World.Player.OwnsFarm) {
                string farmPath = "./saves/" + Player.Name + "/farm.dat";

                if (!GameLoop.World.maps.ContainsKey(new(-1, 0, 0)))
                    GameLoop.World.LoadMapAt(new(-1, 0, 0));

                using StreamWriter farmOutput = new(farmPath);
                string farmJson = JsonConvert.SerializeObject(GameLoop.World.maps[new(-1, 0, 0)], Formatting.Indented);
                farmOutput.WriteLine(farmJson);
                output.Close();
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

                    if (name[^1] == "player.dat") {
                        Player = JsonConvert.DeserializeObject<Player>(json);
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

            foreach (KeyValuePair<string, Mission> kv in missionLibrary) {
                if (!Player.MissionLog.ContainsKey(kv.Key))
                    Player.MissionLog.Add(kv.Key, kv.Value);
            }

            DoneInitializing = true;
            GameLoop.UIManager.Map.LoadMap(Player.MapPos);
         //   GameLoop.UIManager.Map.EntityRenderer.Add(Player); 
        }



        public void CreateMap(Point3D pos) {
            if (!maps.ContainsKey(pos)) {
                if (!LoadMapAt(pos)) {
                    Map newMap = new(GameLoop.MapWidth, GameLoop.MapHeight);

                    if (pos.Z < 0) {
                        for (int i = 0; i < newMap.Tiles.Length; i++) {
                            newMap.Tiles[i] = new(31);
                        }

                        if (Player.MapPos.Z > pos.Z) {
                            newMap.Tiles[Player.Position.ToIndex(GameLoop.MapWidth)] = new(29);
                        }
                    }

                    if (pos.Z > 0) {
                        for (int i = 0; i < newMap.Tiles.Length; i++) {
                            newMap.Tiles[i] = new(32);
                        }

                        if (Player.MapPos.Z < pos.Z) {
                            newMap.Tiles[Player.Position.ToIndex(GameLoop.MapWidth)] = new(30);
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
            
            for (int i = 0; i < skillLibrary.Count; i++) {
                Skill skill = new(skillLibrary[i]);
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
