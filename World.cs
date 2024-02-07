using System; 
using SadRogue.Primitives;
using System.Collections.Generic;

using LofiHollow.Entities;
using System.IO;
using Newtonsoft.Json;
using LofiHollow.Entities.NPC;
using LofiHollow.Managers;
using System.Linq; 
using LofiHollow.EntityData; 
using LofiHollow.DataTypes;
using Steamworks;
using LofiHollow.Minigames.Archaeology;
using LofiHollow.Minigames.Picross;

namespace LofiHollow {
    public class World {  
        public Dictionary<string, Item> itemLibrary = new();
        public Dictionary<string, Monster> monsterLibrary = new();
        public Dictionary<string, Skill> skillLibrary = new();
        public Dictionary<string, NPC> npcLibrary = new();
        public Dictionary<string, FishDef> fishLibrary = new();  
        public Dictionary<string, string> scriptLibrary = new();
        public Dictionary<string, ArchArtifact> artifactLibrary = new();
        public Dictionary<string, Move> moveLibrary = new();
        public Dictionary<string, PicrossPuzzle> picrossLibrary = new();
        public Dictionary<string, TypeDef> typeLibrary = new();
        public Dictionary<string, NodeObject> nodeObjectLibrary = new();

        public List<string> wordGuessWords = new();
         


        public Dictionary<string, Location> atlas = new();


        public bool DoneInitializing = false;
         
        public Player Player { get; set; }

         
        public World() {
            LoadSkillDefinitions(); 
            LoadItemDefinitions();
            LoadMonsterDefinitions();
            LoadNPCDefinitions();  
            LoadScripts();
            LoadArtifacts();
            LoadPicross();
            LoadMoves();
            LoadTypes();
            LoadWords();

            LoadNodeObjectsDefs();

            LoadAllMaps();
        }

        public void InitPlayer() {
            CreatePlayer(); 
        }

        public void RemakeLibraries() { 
            skillLibrary.Clear(); 
            itemLibrary.Clear();
            monsterLibrary.Clear();
            npcLibrary.Clear(); 
            scriptLibrary.Clear();
            artifactLibrary.Clear();
            picrossLibrary.Clear();
            nodeObjectLibrary.Clear();

            LoadSkillDefinitions(); 
            LoadItemDefinitions();
            LoadMonsterDefinitions();
            LoadNPCDefinitions();    
            LoadScripts();
            LoadArtifacts();
            LoadPicross();
            LoadNodeObjectsDefs();
        } 

        public void LoadAllMods() {   
            if (SteamClient.IsValid) {
                RemakeLibraries();

                string path = SteamApps.AppInstallDir() + "/../../workshop/content/1906540/";
                
                if (Directory.Exists(path)) {
                    string[] modFiles = Directory.GetDirectories(path);
                     
                    foreach (string fileName in modFiles) {
                        string modPath = Directory.GetFiles(fileName)[0];

                        Mod mod = Helper.DeserializeFromFileCompressed<Mod>(modPath);
                        

                        if (GameLoop.SteamManager.ModsEnabled.Contains(mod.Metadata.PublishedID.Value)) { 
                            for (int i = 0; i < mod.ModArtifacts.Count; i++) {
                                if (!artifactLibrary.ContainsKey(mod.ModArtifacts[i].FullName()))
                                    artifactLibrary.Add(mod.ModArtifacts[i].FullName(), mod.ModArtifacts[i]);
                            } 

                            for (int i = 0; i < mod.ModItems.Count; i++) {
                                if (!itemLibrary.ContainsKey(mod.ModItems[i].FullName()))
                                    itemLibrary.Add(mod.ModItems[i].FullName(), mod.ModItems[i]);
                            } 

                            for (int i = 0; i < mod.ModMonsters.Count; i++) {
                                if (!monsterLibrary.ContainsKey(mod.ModMonsters[i].Package + ":" + mod.ModMonsters[i].Name))
                                    monsterLibrary.Add(mod.ModMonsters[i].Package + ":" + mod.ModMonsters[i].Name, mod.ModMonsters[i]);
                            }

                            for (int i = 0; i < mod.ModNPCs.Count; i++) {
                                if (!npcLibrary.ContainsKey(mod.ModNPCs[i].Name))
                                    npcLibrary.Add(mod.ModNPCs[i].Name, mod.ModNPCs[i]);
                            }
                              

                            for (int i = 0; i < mod.ModSkills.Count; i++) {
                                if (!skillLibrary.ContainsKey(mod.ModSkills[i].Name))
                                    skillLibrary.Add(mod.ModSkills[i].Name, mod.ModSkills[i]);
                            } 

                            for (int i = 0; i < mod.ModPicross.Count; i++) {
                                if (!picrossLibrary.ContainsKey(mod.ModPicross[i].FullName()))
                                    picrossLibrary.Add(mod.ModPicross[i].FullName(), mod.ModPicross[i]);
                            }

                            for (int i = 0; i < mod.ModScripts.Count; i++) {
                                if (!scriptLibrary.ContainsKey(mod.ModScripts[i].Name))
                                    scriptLibrary.Add(mod.ModScripts[i].FullName(), mod.ModScripts[i].Script);
                            } 
                        }
                    }
                }
            }
        }

        public void LoadAllMaps() {
            if (Directory.Exists("./data/locations/")) {
                string[] itemFiles = Directory.GetFiles("./data/locations/");

                foreach (string fileName in itemFiles) {
                    string json = File.ReadAllText(fileName);
                    Location item = JsonConvert.DeserializeObject<Location>(json);
                    atlas.Add(item.ID, item);
                }
            }
        }

        public void LoadWords() {
            if (File.Exists("./data/wordgame.txt")) {
                string[] tileFiles = File.ReadAllLines("./data/wordgame.txt");

                foreach (string word in tileFiles) {
                    wordGuessWords.Add(word);
                }
            }
        }

        public void LoadMoves() {
            if (Directory.Exists("./data/moves/")) {
                string[] itemFiles = Directory.GetFiles("./data/moves/");

                foreach (string fileName in itemFiles) {
                    string json = File.ReadAllText(fileName);
                    Move item = JsonConvert.DeserializeObject<Move>(json);
                    moveLibrary.Add(item.FullName(), item);
                } 
            }
        }

        public void LoadTypes() {
            if (Directory.Exists("./data/types/")) {
                string[] itemFiles = Directory.GetFiles("./data/types/");

                foreach (string fileName in itemFiles) {
                    string json = File.ReadAllText(fileName);
                    TypeDef item = JsonConvert.DeserializeObject<TypeDef>(json);
                    typeLibrary.Add(item.Name, item);
                } 
            }
        }

        public void LoadScripts() {
            if (Directory.Exists("./data/scripts/")) {
                string[] scriptFiles = Directory.GetFiles("./data/scripts/");

                foreach (string fileName in scriptFiles) {
                    string script = File.ReadAllText(fileName);
                    string name = fileName.Split("/")[^1][0..^4];
                    scriptLibrary.Add("lh:" + name, script);
                }
            }
        }

        public void LoadArtifacts() {
            if (Directory.Exists("./data/artifacts/")) {
                string[] itemFiles = Directory.GetFiles("./data/artifacts/");

                foreach (string fileName in itemFiles) {
                    string json = File.ReadAllText(fileName);
                    ArchArtifact item = JsonConvert.DeserializeObject<ArchArtifact>(json);
                    artifactLibrary.Add(item.FullName(), item);
                }
            }
        }

        public void LoadPicross() {
            if (Directory.Exists("./data/picross/")) {
                string[] itemFiles = Directory.GetFiles("./data/picross/");

                foreach (string fileName in itemFiles) {
                    string json = File.ReadAllText(fileName);
                    PicrossPuzzle item = JsonConvert.DeserializeObject<PicrossPuzzle>(json);
                    picrossLibrary.Add(item.Package + ":" + item.Name, item);
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

        public void LoadNodeObjectsDefs() {
            if (Directory.Exists("./data/nodeObjects/")) {
                string[] files = Directory.GetFiles("./data/nodeObjects/");

                foreach (string name in files) {
                    string json = File.ReadAllText(name);

                    NodeObject obj = JsonConvert.DeserializeObject<NodeObject>(json);

                    if (obj.DisplayName != "") {
                        nodeObjectLibrary.Add(obj.FullName(), obj);
                    }
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
         

        public void SaveMapToFile(string loc) {
            if (atlas.ContainsKey(loc)) {
                Location map = atlas[loc];
                 
                string path = "./data/maps/" + loc + ".dat.gz";
                Helper.SerializeToFileCompressed(map, path);
            }
        }

        public void SavePlayer() { 
            string path = "./saves/" + Player.Name + ".json";
            Helper.SerializeToFileCompressed(Player, path); 
        }

        public void LoadPlayer(string playerName) {
            string fileName = "./saves/" + playerName + ".json";
            if (File.Exists(fileName)) {
                Player = Helper.DeserializeFromFileCompressed<Player>(fileName);
            } 

            foreach (KeyValuePair<string, Skill> kv in skillLibrary) {
                Skill skill = new(kv.Value);
                if (!Player.Skills.ContainsKey(skill.Name))
                    Player.Skills.Add(skill.Name, skill);
            }

             
            DoneInitializing = true; 
            Player.DayStart = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;
              
            GameLoop.UIManager.HandleMenuChange(false);
             
        }
         
        public void CreatePlayer() {
            Player = new(Color.Yellow);
            Player.NavLoc = "NoonbreezeCoast";
            Player.Name = "Player";
            Player.Skills = new Dictionary<string, Skill>();

            foreach (KeyValuePair<string, Skill> kv in skillLibrary) {
                Skill skill = new(kv.Value);
                Player.Skills.Add(skill.Name, skill);
            }
               
            Player.CurrentStamina = Player.MaxStamina;
        }

        public void FreshStart() { 
            Player.CurrentStamina = Player.MaxStamina;  

            DoneInitializing = true;

            Player.Skills = new Dictionary<string, Skill>();
            
            foreach (KeyValuePair<string, Skill> kv in skillLibrary) {
                Skill skill = new(kv.Value);
                if (!Player.Skills.ContainsKey(skill.Name))
                    Player.Skills.Add(skill.Name, skill);
            }


            if (Player.StartScenario == "Shipwrecked") {
                //Player.Inventory.Add(Item.Copy("lh:Rusty Dagger"));
                Player.NavLoc = "NoonbreezeCoast";
                Player.MiscData.Add("Start_Shipwrecked", 0);
            }


            SavePlayer();
            LoadPlayer(Player.Name);

            GameLoop.UIManager.AddMsg("Press F1 for help at any time, or ? to view hotkeys.");

        }
    }
}
