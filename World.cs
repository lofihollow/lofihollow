using System; 
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
using LofiHollow.Minigames.Archaeology;
using LofiHollow.Minigames.Picross;

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
        public Dictionary<string, ArchArtifact> artifactLibrary = new();
        public Dictionary<string, Move> moveLibrary = new();
        public Dictionary<string, PicrossPuzzle> picrossLibrary = new();
        public Dictionary<string, TypeDef> typeLibrary = new();

        public List<string> wordGuessWords = new();


        public List<Chapter> Chapters = new();


        public Dictionary<SteamId, Player> otherPlayers = new();


        public bool DoneInitializing = false;
         
        public Player Player { get; set; }

         
        public World() {
            LoadSkillDefinitions();
            LoadTileDefinitions();
            LoadItemDefinitions();
            LoadMonsterDefinitions();
            LoadNPCDefinitions();
            LoadConstructibles();
            LoadMineTiles();
            LoadCraftingRecipes();
            LoadMissionDefinitions();
            LoadScripts();
            LoadArtifacts();
            LoadPicross();
            LoadMoves();
            LoadTypes();
            LoadWords();


          //  LoadAllMaps();
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

        public void RemakeLibraries() { 
            skillLibrary.Clear();
            tileLibrary.Clear();
            itemLibrary.Clear();
            monsterLibrary.Clear();
            npcLibrary.Clear();
            missionLibrary.Clear();
            scriptLibrary.Clear();
            artifactLibrary.Clear();
            picrossLibrary.Clear();

            LoadSkillDefinitions();
            LoadTileDefinitions();
            LoadItemDefinitions();
            LoadMonsterDefinitions();
            LoadNPCDefinitions();   
            LoadMissionDefinitions();
            LoadScripts();
            LoadArtifacts();
            LoadPicross();
        }


        public void LoadAllMaps() {
            if (Directory.Exists("./data/maps/Overworld/")) {
                string[] mapFiles = Directory.GetFiles("./data/maps/Overworld/");

                foreach (string fileName in mapFiles) {
                    Map map = Helper.DeserializeFromFileCompressed<Map>(fileName);
                    string name = map.MinimapTile.name;
                    if (name == "Forest" || name == "Forest Road")
                        map.AmbientMonsters = true;
                    else
                        map.AmbientMonsters = false;

                    Helper.SerializeToFileCompressed(map, fileName);
                }
            }
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

                            for (int i = 0; i < mod.ModConstructs.Count; i++) {
                                constructibles.Add(constructibles.Count, mod.ModConstructs[i]);
                            }

                            SortConstructibles();

                            for (int i = 0; i < mod.ModItems.Count; i++) {
                                if (!itemLibrary.ContainsKey(mod.ModItems[i].FullName()))
                                    itemLibrary.Add(mod.ModItems[i].FullName(), mod.ModItems[i]);
                            }

                            for (int i = 0; i < mod.ModMissions.Count; i++) {
                                if (!missionLibrary.ContainsKey(mod.ModMissions[i].FullName()))
                                    missionLibrary.Add(mod.ModMissions[i].FullName(), mod.ModMissions[i]);
                            }

                            for (int i = 0; i < mod.ModMonsters.Count; i++) {
                                if (!monsterLibrary.ContainsKey(mod.ModMonsters[i].FullName()))
                                    monsterLibrary.Add(mod.ModMonsters[i].FullName(), mod.ModMonsters[i]);
                            }

                            for (int i = 0; i < mod.ModNPCs.Count; i++) {
                                if (!npcLibrary.ContainsKey(mod.ModNPCs[i].FullName()))
                                    npcLibrary.Add(mod.ModNPCs[i].FullName(), mod.ModNPCs[i]);
                            }

                            for (int i = 0; i < mod.ModRecipes.Count; i++) {
                                recipeLibrary.Add(recipeLibrary.Count, mod.ModRecipes[i]);
                            }

                            SortRecipes();

                            for (int i = 0; i < mod.ModSkills.Count; i++) {
                                if (!skillLibrary.ContainsKey(mod.ModSkills[i].Name))
                                    skillLibrary.Add(mod.ModSkills[i].Name, mod.ModSkills[i]);
                            }

                            for (int i = 0; i < mod.ModTiles.Count; i++) {
                                if (!tileLibrary.ContainsKey(mod.ModTiles[i].FullName()))
                                    tileLibrary.Add(mod.ModTiles[i].FullName(), mod.ModTiles[i]);
                            }

                            for (int i = 0; i < mod.ModPicross.Count; i++) {
                                if (!picrossLibrary.ContainsKey(mod.ModPicross[i].FullName()))
                                    picrossLibrary.Add(mod.ModPicross[i].FullName(), mod.ModPicross[i]);
                            }

                            for (int i = 0; i < mod.ModScripts.Count; i++) {
                                if (!scriptLibrary.ContainsKey(mod.ModScripts[i].Name))
                                    scriptLibrary.Add(mod.ModScripts[i].FullName(), mod.ModScripts[i].Script);
                            }

                            for (int i = 0; i < mod.ModMaps.Count; i++) {
                                if (!maps.ContainsKey(mod.ModMaps[i].MapPos))
                                    maps.Add(mod.ModMaps[i].MapPos, mod.ModMaps[i].Map);
                            }
                        }
                    }
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

        public void GenerateAllFish() {
            while (fishLibrary.Count < 300) {
                FishDef newFish = GenerateFish();

                if (!fishLibrary.ContainsKey(newFish.FullName())) {
                    fishLibrary.Add(newFish.FullName(), newFish);

                    Item fishItem = Item.Copy("lh:Raw Fish");
                    fishItem.ForegroundR = newFish.colR;
                    fishItem.ForegroundG = newFish.colG;
                    fishItem.ForegroundB = newFish.colB;
                    fishItem.ItemGlyph = newFish.glyph;
                    

                    fishItem.Name = newFish.FishItemID.Split(":")[1];
                    fishItem.Package = "lh";
                    fishItem.AverageValue = 5;

                    if (!itemLibrary.ContainsKey(fishItem.FullName()))
                        itemLibrary.Add(fishItem.FullName(), fishItem);
                }
            }

            FishDef guaranteed = GenerateFish();

            while (fishLibrary.ContainsKey(guaranteed.FullName())) {
                guaranteed = GenerateFish();
            }

            guaranteed.CatchLocation = "Any";
            guaranteed.Season = "Any";
            guaranteed.EarliestTime = 360;
            guaranteed.LatestTime = 1560;
            guaranteed.RequiredLevel = 0;
            guaranteed.GrantedExp = 10;
            guaranteed.Strength = 2;
            guaranteed.FightChance = 50;
            guaranteed.FightLength = 4;

            fishLibrary.Add(guaranteed.FullName(), guaranteed);

            Item guaranteedItem = Item.Copy("lh:Raw Fish");
            guaranteedItem.ForegroundR = guaranteed.colR;
            guaranteedItem.ForegroundG = guaranteed.colG;
            guaranteedItem.ForegroundB = guaranteed.colB;
            guaranteedItem.ItemGlyph = guaranteed.glyph;


            guaranteedItem.Name = guaranteed.FishItemID.Split(":")[1];
            guaranteedItem.Package = "lh";
            guaranteedItem.AverageValue = 5;

            if (!itemLibrary.ContainsKey(guaranteedItem.FullName()))
                itemLibrary.Add(guaranteedItem.FullName(), guaranteedItem);
        }

        public FishDef GenerateFish() {
            string[] Locations = { "Freshwater", "Saltwater", "Brackish", "Any" };
            string[] Seasons = { "Spring", "Summer", "Fall", "Winter", "Holiday", "Any" };
            
            List<FishColor> Colors = new(); 
            Colors.Add(new("Black", new Color(30, 30, 30, 255)));
            Colors.Add(new("Blackfin", new Color(30, 30, 30, 200)));
            Colors.Add(new("Blacktip", new Color(40, 40, 40, 255)));
            Colors.Add(new("Blue", new Color(0, 0, 128)));
            Colors.Add(new("Bluefin", new Color(0, 0, 192)));
            Colors.Add(new("Bluntnose", Color.Gray));
            Colors.Add(new("Bobtail", Color.Gray));
            Colors.Add(new("Bonnethead", Color.Gray));
            Colors.Add(new("Bonytail", Color.LightGray));
            Colors.Add(new("Bonytongue", Color.LightGray));
            Colors.Add(new("Bristlenose", Color.Gray));
            Colors.Add(new("Broadband", Color.Gray));
            Colors.Add(new("Bronze", new Color(176, 141, 87)));
            Colors.Add(new("Brown", Color.Brown));
            Colors.Add(new("Bullhead", Color.Firebrick));
            Colors.Add(new("Cateye", Color.Yellow));
            Colors.Add(new("Central", Color.Gray));
            Colors.Add(new("Chain", Color.DarkGray));
            Colors.Add(new("Channel", Color.Gray));
            Colors.Add(new("Cherry", Color.AnsiRedBright));
            Colors.Add(new("Climbing", Color.Gray));
            Colors.Add(new("Collard", Color.Gray));
            Colors.Add(new("Cownose", Color.White));
            Colors.Add(new("Daggertooth", Color.Gray));
            Colors.Add(new("Dogteeth", Color.Gray));
            Colors.Add(new("Duckbill", Color.Gray));
            Colors.Add(new("Dusky", new Color(80, 80, 80)));
            Colors.Add(new("Elephantnose", Color.Gray));
            Colors.Add(new("Ember", Color.DarkRed));
            Colors.Add(new("Emerald", Color.Lime));
            Colors.Add(new("Fathead", Color.Gray));
            Colors.Add(new("Finback", Color.Gray));
            Colors.Add(new("Fire", Color.AnsiRed));
            Colors.Add(new("Flabby", Color.Gray));
            Colors.Add(new("Flagfin", Color.Gray));
            Colors.Add(new("Flagtail", Color.Gray));
            Colors.Add(new("Four-eyed", Color.Gray));
            Colors.Add(new("Frilled", Color.Gray));
            Colors.Add(new("Frogmouth", Color.Green));
            Colors.Add(new("Glass", Color.Cyan.SetAlpha(128)));
            Colors.Add(new("Glowlight", Color.Lime.SetAlpha(128)));
            Colors.Add(new("Golden", Color.Gold));
            Colors.Add(new("Goldeye", Color.Gold));
            Colors.Add(new("Gray", Color.Gray));
            Colors.Add(new("Green", Color.Green));
            Colors.Add(new("Greenspotted", Color.Green));
            Colors.Add(new("Hammerhead", Color.Gray));
            Colors.Add(new("Hardhead", Color.Gray));
            Colors.Add(new("Harelip", Color.Gray));
            Colors.Add(new("Horned", Color.Gray));
            Colors.Add(new("Jellynose", Color.Pink));
            Colors.Add(new("Lefteye", Color.Gray));
            Colors.Add(new("Lemon", Color.Yellow));
            Colors.Add(new("Lined", Color.Gray));
            Colors.Add(new("Long Neck", Color.Gray));
            Colors.Add(new("Long-Whisker", Color.Gray));
            Colors.Add(new("Longfin", Color.Gray));
            Colors.Add(new("Longjaw", Color.Gray));
            Colors.Add(new("Longnose", Color.Gray));
            Colors.Add(new("Loosejaw", Color.Gray));
            Colors.Add(new("Loweye", Color.Gray));
            Colors.Add(new("Mailcheek", Color.Gray));
            Colors.Add(new("Megamouth", Color.Gray));
            Colors.Add(new("Monkeyface", Color.Brown));
            Colors.Add(new("Mustache", Color.Brown));
            Colors.Add(new("Mustard", Color.Yellow));
            Colors.Add(new("Nakedback", Color.Gray));
            Colors.Add(new("Neon", Color.Cyan));
            Colors.Add(new("Olive", Color.Olive));
            Colors.Add(new("Orange", Color.Orange));
            Colors.Add(new("Pancake", Color.Beige));
            Colors.Add(new("Paradise", Color.SkyBlue));
            Colors.Add(new("Parasitic", Color.ForestGreen));
            Colors.Add(new("Pearl", new Color(234, 224, 200)));
            Colors.Add(new("Peppered", Color.DarkGray));
            Colors.Add(new("Prickleback", Color.Gray));
            Colors.Add(new("Prickly", Color.Gray));
            Colors.Add(new("Razorback", Color.Gray));
            Colors.Add(new("Red", Color.Red));
            Colors.Add(new("Redfin", Color.Red));
            Colors.Add(new("Redlip", Color.Red));
            Colors.Add(new("Redmouth", Color.Red));
            Colors.Add(new("Redtooth", Color.Red));
            Colors.Add(new("Ribbon", Color.Pink));
            Colors.Add(new("Ridgehead", Color.Gray));
            Colors.Add(new("River", Color.LightBlue));
            Colors.Add(new("Rock", Color.Gray));
            Colors.Add(new("Sand", Color.PaleGoldenrod));
            Colors.Add(new("Sandbar", Color.PaleGoldenrod));
            Colors.Add(new("Sawtail", Color.Gray));
            Colors.Add(new("Sharpnose", Color.Gray));
            Colors.Add(new("Sheepshead", Color.White));
            Colors.Add(new("Shortfin", Color.Gray));
            Colors.Add(new("Silver", Color.Silver));
            Colors.Add(new("Silverside", Color.Silver));
            Colors.Add(new("Sixgill", Color.Gray));
            Colors.Add(new("Slender", Color.Gray));
            Colors.Add(new("Slickhead", Color.Gray));
            Colors.Add(new("Slimehead", Color.Lime.SetAlpha(128)));
            Colors.Add(new("Slimy", Color.Lime.SetAlpha(128)));
            Colors.Add(new("Slipmouth", Color.Gray));
            Colors.Add(new("Smalleye", Color.Gray));
            Colors.Add(new("Smallmouth", Color.Gray));
            Colors.Add(new("Smalltooth", Color.Gray));
            Colors.Add(new("Smooth", Color.Gray));
            Colors.Add(new("Snakehead", Color.Green));
            Colors.Add(new("Spotted", Color.Gray));
            Colors.Add(new("Squarehead", Color.Gray));
            Colors.Add(new("Squaretail", Color.Gray));
            Colors.Add(new("Starry", Color.Purple));
            Colors.Add(new("Striped", Color.White));
            Colors.Add(new("Temperate", Color.Gray));
            Colors.Add(new("Thorny", Color.Green));
            Colors.Add(new("Thread-tail", Color.Gray));
            Colors.Add(new("Three spot", Color.Gray));
            Colors.Add(new("Threespine", Color.Gray));
            Colors.Add(new("Tidewater", Color.SeaGreen));
            Colors.Add(new("Top", Color.Gray));
            Colors.Add(new("Torrent", Color.LightBlue));
            Colors.Add(new("Tube", Color.Gray));
            Colors.Add(new("Upside-down", Color.Gray));
            Colors.Add(new("Velvet", Color.Purple));
            Colors.Add(new("Vermillion", Color.Magenta));
            Colors.Add(new("Warty", Color.Green));
            Colors.Add(new("Whiptail", Color.Gray));
            Colors.Add(new("White", Color.White));
            Colors.Add(new("White tip", Color.White));
            Colors.Add(new("X-ray", Color.Green.SetAlpha(60)));
            Colors.Add(new("Yellow", Color.Yellow));
            Colors.Add(new("Yellow-edge", Color.Yellow));
            Colors.Add(new("Yellow-eye", Color.Yellow));
            Colors.Add(new("Yellow-head", Color.Yellow));
            Colors.Add(new("Yellowback", Color.Yellow));
            Colors.Add(new("Yellowbelly", Color.Yellow));
            Colors.Add(new("Yellowfin", Color.Yellow));
            Colors.Add(new("Zebra", Color.White));





            List<FishTime> Times = new();
            Times.Add(new("Morning", 360, 540));
            Times.Add(new("Late Morning", 540, 720));
            Times.Add(new("Afternoon", 720, 1080));
            Times.Add(new("Evening", 1080, 1320));
            Times.Add(new("Night", 1320, 1560));
            Times.Add(new("Day", 540, 1080));
            Times.Add(new("Any", 360, 1560));

            List<FishSpecies> Species = new();
            Species.Add(new("Anchovy", 'a'));
            Species.Add(new("Bass", 'b'));
            Species.Add(new("Catfish", 'c'));
            Species.Add(new("Dogfish", 'd'));
            Species.Add(new("Eel", 'e'));
            Species.Add(new("Flounder", 'f'));
            Species.Add(new("Guppy", 'g'));
            Species.Add(new("Herring", 'h'));
            Species.Add(new("Icefish", 'i'));
            Species.Add(new("Jewelfish", 'j'));
            Species.Add(new("Koi", 'k'));
            Species.Add(new("Lobster", 'l'));
            Species.Add(new("Mackerel", 'm'));
            Species.Add(new("Needlefish", 'n'));
            Species.Add(new("Octopus", 'o'));
            Species.Add(new("Perch", 'p'));
            Species.Add(new("Quillback", 'q'));
            Species.Add(new("Rockfish", 'r'));
            Species.Add(new("Salmon", 's'));
            Species.Add(new("Trout", 't'));
            Species.Add(new("Unicorn Fish", 'u'));
            Species.Add(new("Viperfish", 'v'));
            Species.Add(new("Walleye", 'w'));
            Species.Add(new("Xenofish", 'x'));
            Species.Add(new("Yellowfish", 'y'));
            Species.Add(new("Zebrafish", 'z'));

            Species.Add(new("Angelfish", 'A'));
            Species.Add(new("Barracuda", 'B'));
            Species.Add(new("Carp", 'C'));
            Species.Add(new("Dolphin", 'D'));
            Species.Add(new("Emberling", 'E'));
            Species.Add(new("Frog", 'F'));
            Species.Add(new("Grouper", 'G'));
            Species.Add(new("Halibut", 'H'));
            Species.Add(new("Infrafish", 'I'));
            Species.Add(new("Jawfish", 'J'));
            Species.Add(new("Knifejaw", 'K'));
            Species.Add(new("Lionfish", 'L'));
            Species.Add(new("Marlin", 'M'));
            Species.Add(new("Noodlefish", 'N'));
            Species.Add(new("Oarfish", 'O'));
            Species.Add(new("Pollock", 'P'));
            Species.Add(new("Queenfish", 'Q'));
            Species.Add(new("Ray", 'R'));
            Species.Add(new("Shark", 'S'));
            Species.Add(new("Tuna", 'T'));
            Species.Add(new("Undulator", 'U'));
            Species.Add(new("Velvetfish", 'V'));
            Species.Add(new("Whale", 'W'));
            Species.Add(new("Xylocarp", 'X'));
            Species.Add(new("Yellowfin", 'Y'));
            Species.Add(new("Zebra Loach", 'Z'));
             

            FishDef fish = new();

            fish.CatchLocation = Locations[GameLoop.rand.Next(Locations.Length)];
            fish.Season = Seasons[GameLoop.rand.Next(Seasons.Length)];

            FishColor col = Colors[GameLoop.rand.Next(Colors.Count)];
            fish.colR = col.R;
            fish.colG = col.G;
            fish.colB = col.B;
            fish.colA = col.A;

            FishTime time = Times[GameLoop.rand.Next(Times.Count)];
            fish.EarliestTime = time.StartTime;
            fish.LatestTime = time.EndTime;

            FishSpecies spec = Species[GameLoop.rand.Next(Species.Count)];
            fish.glyph = spec.Glyph;


            fish.Strength = GameLoop.rand.Next(10);
            fish.MaxQuality = 11;
            fish.FightChance = (GameLoop.rand.Next(9) + 1) * 10;
            fish.FightLength = (GameLoop.rand.Next(10) + 1) * 10;
            fish.FilletID = "lh:Fish Fillet";
            fish.RequiredLevel = GameLoop.rand.Next(10) * 10;
            fish.GrantedExp = Math.Max(10, fish.RequiredLevel + 10);

            fish.Name = col.Name + " " + spec.Name;
            fish.PackageID = "lh";
            fish.FishItemID = "lh:Raw " + spec.Name;


            return fish;
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

        public void SortConstructibles() {
            List<Constructible> presort = new();

            foreach (KeyValuePair<int, Constructible> kv in constructibles) {
                presort.Add(kv.Value);
            }
             
            constructibles.Clear();

            List<Constructible> sorted = presort.OrderBy(o => o.RequiredLevel).ThenBy(o => o.Name).ToList();

            for (int i = 0; i < sorted.Count; i++) {
                constructibles.Add(i, sorted[i]);
            }
        }

        public void SortRecipes() {
            List<CraftingRecipe> presort = new();

            foreach (KeyValuePair<int, CraftingRecipe> kv in recipeLibrary) {
                presort.Add(kv.Value);
            }

            recipeLibrary.Clear();

            List<CraftingRecipe> sorted = presort.OrderBy(o => o.RequiredLevel).ThenBy(o => o.Name).ToList();

            for (int i = 0; i < sorted.Count; i++) {
                recipeLibrary.Add(i, sorted[i]);
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
                        npcLibrary.Add(npc.FullName(), npc);
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

                    monsterLibrary.Add(monster.FullName(), monster);
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

                    if (map.Tiles[i].AnimalBed != null) {
                        map.Tiles[i].AnimalBed.SpawnAnimal();
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

                 
                string fishPath = "./saves/" + Player.Name + "/fish.gz";

                List<FishDef> allFish = new();
                foreach (KeyValuePair<string, FishDef> kv in fishLibrary) {
                    allFish.Add(kv.Value);
                }

                Helper.SerializeToFileCompressed(allFish, fishPath);
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

                        if (Player.NoonbreezeApt != null) {
                            Map map = Player.NoonbreezeApt.map;
                            for (int i = 0; i < map.Tiles.Length; i++) {
                                if (map.Tiles[i].Lock != null) {
                                    if (!map.Tiles[i].Lock.Closed) {
                                        map.Tiles[i].Glyph = map.Tiles[i].Lock.OpenedGlyph;
                                        map.Tiles[i].IsBlockingMove = map.Tiles[i].Lock.OpenBlocksMove;
                                        map.Tiles[i].IsBlockingLOS = false;
                                    }
                                    else {
                                        map.Tiles[i].Glyph = map.Tiles[i].Lock.ClosedGlyph;
                                        map.Tiles[i].IsBlockingMove = true;
                                        map.Tiles[i].IsBlockingLOS = map.Tiles[i].Lock.ClosedBlocksLOS;
                                    }
                                }

                                if (Player.NoonbreezeApt.map.Tiles[i].AnimalBed != null) {
                                    Player.NoonbreezeApt.map.Tiles[i].AnimalBed.SpawnAnimal();
                                }
                            }
                        }
                    }

                    if (name[^1] == "time.dat") {
                        Player.Clock = JsonConvert.DeserializeObject<TimeManager>(json);
                    }

                    if (name[^1] == "fish.gz") {
                        List<FishDef> allFish = Helper.DeserializeFromFileCompressed<List<FishDef>>(fileName);
                        fishLibrary = new();

                        foreach (FishDef fish in allFish) {
                            fishLibrary.Add(fish.FullName(), fish);

                            Item fishItem = Item.Copy("lh:Raw Fish");

                            string[] split = fish.FishItemID.Split(":");
                            fishItem.ForegroundR = fish.colR;
                            fishItem.ForegroundG = fish.colG;
                            fishItem.ForegroundB = fish.colB;
                            fishItem.ItemGlyph = fish.glyph;
                            fishItem.Name = split[1];
                            fishItem.Package = split[0];
                            fishItem.AverageValue = fish.RequiredLevel / 2;

                            if (!itemLibrary.ContainsKey(fishItem.FullName())) {
                                itemLibrary.Add(fishItem.FullName(), fishItem);
                            }
                        }
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
            GameLoop.UIManager.AdventurerBoard.PopulateJobList();

            DoneInitializing = true;


            Player.DayStart = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

            Map currentMap = Helper.ResolveMap(Player.MapPos); 
            if (currentMap != null) {
                GameLoop.UIManager.Map.LoadMap(currentMap);
                GameLoop.UIManager.Map.UpdateVision();
                GameLoop.UIManager.Map.RenderOverlays();
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
            Player.Skills = new Dictionary<string, Skill>();

            foreach (KeyValuePair<string, Skill> kv in skillLibrary) {
                Skill skill = new(kv.Value);
                Player.Skills.Add(skill.Name, skill);
            }

            foreach (KeyValuePair<string, Mission> kv in missionLibrary) {
                Player.MissionLog.Add(kv.Key, kv.Value);
            }

            GameLoop.UIManager.Map.LoadMap(Player.MapPos);
          //  GameLoop.UIManager.Map.EntityRenderer.Add(Player);
            Player.ZIndex = 10;

            Player.UpdateHP();
            Player.CurrentHP = Player.MaxHP;
            Player.CurrentStamina = Player.MaxStamina;
        }

        public void FreshStart() {
            Player.UpdateHP();
            Player.CurrentHP = Player.MaxHP;
            Player.CurrentStamina = Player.MaxStamina;
            LoadMapAt(Player.MapPos);

            DoneInitializing = true;
            Player.Inventory[0] = Item.Copy("lh:Rusty Dagger");

            Player.Skills = new Dictionary<string, Skill>();
            
            foreach (KeyValuePair<string, Skill> kv in skillLibrary) {
                Skill skill = new(kv.Value);
                if (!Player.Skills.ContainsKey(skill.Name))
                    Player.Skills.Add(skill.Name, skill);
            } 

            foreach (KeyValuePair<string, Mission> kv in missionLibrary) { 
                if (!Player.MissionLog.ContainsKey(kv.Key))
                    Player.MissionLog.Add(kv.Key, kv.Value);
            } 

            GenerateAllFish();

            SavePlayer();
            LoadPlayer(Player.Name);

            GameLoop.UIManager.AddMsg("Press F1 for help at any time, or ? to view hotkeys.");

        }
    }
}
