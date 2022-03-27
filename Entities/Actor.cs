using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.EntityData;
using System.Text;
using LofiHollow.DataTypes;
using LofiHollow.Managers; 


namespace LofiHollow.Entities {
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Actor : Entity {
        [JsonProperty]
        public int CurrentHP;
        [JsonProperty]
        public int MaxHP;

        [JsonProperty]
        public int CurrentStamina = 100;
        [JsonProperty]
        public int MaxStamina = 100;
         


        [JsonProperty]
        public int SizeMod = 0;
        [JsonProperty]
        public int Vision = 36;


        [JsonProperty]
        public string CombatMode = "Attack";

        [JsonProperty]
        public string ElementalAlignment = "Earth"; 

        [JsonProperty]
        public int CombatLevel = 1;

        [JsonProperty]
        public int HealthGrowth = 5;
        [JsonProperty]
        public int SpeedGrowth = 5;
        [JsonProperty]
        public int AttackGrowth = 5;
        [JsonProperty]
        public int DefenseGrowth = 5;
        [JsonProperty]
        public int MAttackGrowth = 5;
        [JsonProperty]
        public int MDefenseGrowth = 5;

        [JsonProperty]
        public int HealthEXP = 0;
        [JsonProperty]
        public int SpeedEXP = 0;
        [JsonProperty]
        public int AttackEXP = 0;
        [JsonProperty]
        public int DefenseEXP = 0;
        [JsonProperty]
        public int MAttackEXP = 0;
        [JsonProperty]
        public int MDefenseEXP = 0;


        public double TimeLastActed = 0;


        [JsonProperty]
        public Dictionary<string, Skill> Skills = new();

        [JsonProperty]
        public List<string> Types = new();

        [JsonProperty]
        public int ForegroundR = 0;
        [JsonProperty]
        public int ForegroundG = 0;
        [JsonProperty]
        public int ForegroundB = 0;
        [JsonProperty]
        public int ActorGlyph = 0;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            Appearance.Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Appearance.Background = new Color(0, 0, 0, 50);
            Appearance.Glyph = ActorGlyph; 
        }

        public void UpdateAppearance() {
            Appearance.Foreground = new Color(ForegroundR, ForegroundG, ForegroundB);
            Appearance.Background = new Color(0, 0, 0, 50);
            Appearance.Glyph = ActorGlyph;
        }

        public void SwitchMeleeMode() {
            if (CombatMode == "Attack") {
                CombatMode = "Strength";
            } else if (CombatMode == "Strength") {
                CombatMode = "Defense";
            } else if (CombatMode == "Defense") {
                CombatMode = "Balanced";
            } else if (CombatMode == "Balanced") {
                CombatMode = "Attack";
            }
        }

        public void CombatExp(int damage) {
            if (CombatMode == "Attack") {
                Skills["Attack"].GrantExp(damage * 4);
            } else if (CombatMode == "Strength") {
                Skills["Strength"].GrantExp(damage * 4);
            } else if (CombatMode == "Defense") {
                Skills["Defense"].GrantExp(damage * 4);
            } else if (CombatMode == "Balanced") {
                Skills["Attack"].GrantExp(damage);
                Skills["Strength"].GrantExp(damage);
                Skills["Defense"].GrantExp(damage);
                Skills["Constitution"].GrantExp(damage);
            }

            Skills["Constitution"].GrantExp(damage * 2);
        } 

        protected Actor(Color foreground, int glyph) : base(foreground, Color.Transparent, glyph) {
            Appearance.Foreground = foreground; 
            Appearance.Glyph = glyph;
            Appearance.Background = new Color(0, 0, 0, 50);

            ForegroundR = foreground.R;
            ForegroundG = foreground.G;
            ForegroundB = foreground.B;

            MaxStamina = 100;
            CurrentStamina = MaxStamina;
        }

        public ColoredString GetAppearance() {
            return new ColoredString(ActorGlyph.AsString(), Appearance.Foreground, Color.Transparent);
        }

        public float ModifiedDamage(string attackType, int damageIn) {
            for (int i = 0; i < Types.Count; i++) {
                if (GameLoop.World.typeLibrary.ContainsKey(Types[i])) {
                    TypeDef thisType = GameLoop.World.typeLibrary[Types[i]];

                    damageIn = thisType.ModDamage(damageIn, attackType);
                } 
            }

            if (GameLoop.World.typeLibrary.ContainsKey(ElementalAlignment)) {
                TypeDef coreType = GameLoop.World.typeLibrary[ElementalAlignment];
                damageIn = coreType.ModDamage(damageIn, attackType);
            }

            return damageIn;
        }


        public int TakeDamage(int damage) {
            int currHp = CurrentHP;

            CurrentHP = Math.Clamp(CurrentHP - damage, 0, MaxHP);

            int damageTaken = currHp - CurrentHP;

            if (CurrentHP <= 0) {
                if (this is Player player)
                    player.PlayerDied();
                else
                    Death(true);
            }

            return damageTaken;
        }

        public bool MoveBy(Point positionChange) {
            Map map = Helper.ResolveMap(MapPos);

            if (map != null) {
                Point newPosition = Position + positionChange; 
                if (newPosition.Y < 0 && GameLoop.World.maps.ContainsKey(MapPos - new Point3D(0, -1, 0)) && GameLoop.World.maps[MapPos - new Point3D(0, -1, 0)].MinimapTile.name == "Desert") {
                    GameLoop.UIManager.AddMsg("There's dangerous sandstorms that way, best not go there for now.");
                    return false;
                }

                // Interact with skilling tiles
                if (this is Player play) {
                    if (newPosition.X < GameLoop.MapWidth && newPosition.X >= 0 && newPosition.Y < GameLoop.MapHeight && newPosition.Y >= 0) {
                        if (map.GetTile(newPosition).MiscString != "" && map.GetTile(newPosition).MiscString.Split(",").Length > 1) {
                            string[] split = map.GetTile(newPosition).MiscString.Split(",");
                            if (split[0] == "Skill") {
                                if (split.Length > 2)
                                    GameLoop.UIManager.Crafting.SetupCrafting(split[1], split[2], Int32.Parse(split[3]));
                                else
                                    GameLoop.UIManager.Crafting.SetupCrafting(split[1], "None", 1);
                            } else if (split[0] == "Minigame") { 
                                 GameLoop.UIManager.Minigames.ToggleMinigame(split[1]); 
                            } else if (split[0] == "Apartments") {
                                if (split[1] == "Noonbreeze") {
                                    GameLoop.UIManager.Teleport.Toggle("Noonbreeze Apartments");
                                }
                            } else if (split[0] == "Board") {
                                if (split[1] == "Photography") {
                                    GameLoop.UIManager.Photo.ShowingBoard = true;
                                    GameLoop.UIManager.Photo.Toggle();
                                }
                                else if (split[1] == "Adventure") {
                                    GameLoop.UIManager.AdventurerBoard.Toggle();
                                }
                                else if (split[1] == "Courier") {

                                }
                            }
                        }

                        if (map.GetTile(newPosition).SkillableTile != null) {
                            SkillableTile tile = map.GetTile(newPosition).SkillableTile;
                            if (map.GetTile(newPosition).Name == tile.HarvestableName) {
                                if (Skills.ContainsKey(tile.RequiredSkill)) {
                                    if (Skills[tile.RequiredSkill].Level >= tile.RequiredLevel) {
                                        if (play.Equipment[0].ItemCat == tile.HarvestTool || play.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemCat == tile.HarvestTool) {
                                            if (play.HasInventorySlotOpen()) {
                                                CommandManager.AddItemToInv(play, Item.Copy(tile.ItemGiven));
                                                GameLoop.UIManager.AddMsg(tile.HarvestMessage);
                                                ExpendStamina(1);

                                                int choppedChance = GameLoop.rand.Next(100) + 1;

                                                if (choppedChance < 33) {
                                                    CommandManager.AddItemToInv(play, Item.Copy(tile.DepletedItem));
                                                    map.GetTile(newPosition).Name = tile.DepletedName;
                                                    map.GetTile(newPosition).UpdateAppearance();
                                                    GameLoop.UIManager.AddMsg(tile.DepleteMessage);
                                                    Skills[tile.RequiredSkill].Experience += tile.ExpOnDeplete;
                                                } else {
                                                    Skills[tile.RequiredSkill].Experience += tile.ExpOnHarvest;
                                                }

                                                if (Skills[tile.RequiredSkill].Experience >= Skills[tile.RequiredSkill].ExpToLevel()) {
                                                    Skills[tile.RequiredSkill].Experience -= Skills[tile.RequiredSkill].ExpToLevel();
                                                    Skills[tile.RequiredSkill].Level++;
                                                    GameLoop.UIManager.AddMsg(new ColoredString("You leveled " + tile.RequiredSkill + " to " + Skills[tile.RequiredSkill].Level + "!", Color.Cyan, Color.Black));
                                                }
                                            } else {
                                                GameLoop.UIManager.AddMsg(new ColoredString("Your inventory is full.", Color.Red, Color.Black));
                                            }
                                        } else {
                                            GameLoop.UIManager.AddMsg(new ColoredString("You don't have the right tool equipped.", Color.Red, Color.Black));
                                        }
                                    } else {
                                        GameLoop.UIManager.AddMsg(new ColoredString("That requires level " + tile.RequiredLevel + " " + tile.RequiredSkill + ".", Color.Red, Color.Black));
                                    }
                                }
                            } else {
                                GameLoop.UIManager.AddMsg(new ColoredString("That's not harvestable yet, come back in a " + tile.RestoreTime + ".", Color.Red, Color.Black));
                            }
                        }
                    }
                }

                Point CurrentPos = Position;

                // Slope movement (UP slope)
                if (map.GetTile(CurrentPos).Name == "Up Slope" && positionChange == new Point(0, -1)) {
                    if (!map.IsTileWalkable(newPosition)) {
                        // This is an up slope, move up a map instead of up on the current map
                        if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 0, 1))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 0, 1)); }
                        MapPos += new Point3D(0, 0, 1);
                    } else {
                        if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 0, -1))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 0, -1)); }
                        MapPos += new Point3D(0, 0, -1);
                    }
                    GameLoop.UIManager.Map.LoadMap(MapPos);
                    return true;
                }

                // Slope movement (DOWN slope)
                if (map.GetTile(CurrentPos).Name == "Down Slope" && positionChange == new Point(0, 1)) {
                    if (!map.IsTileWalkable(newPosition)) {
                        // This is an up slope, move up a map instead of up on the current map
                        if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 0, 1))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 0, 1)); }
                        MapPos += new Point3D(0, 0, 1);
                    } else {
                        if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 0, -1))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 0, -1)); }
                        MapPos += new Point3D(0, 0, -1);
                    }
                    GameLoop.UIManager.Map.LoadMap(MapPos);
                    return true;
                }

                // Slope movement (LEFT slope)
                if (map.GetTile(CurrentPos).Name == "Left Slope" && positionChange == new Point(-1, 0)) {
                    if (!map.IsTileWalkable(newPosition)) {
                        // This is an up slope, move up a map instead of up on the current map
                        if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 0, 1))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 0, 1)); }
                        MapPos += new Point3D(0, 0, 1);
                    } else {
                        if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 0, -1))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 0, -1)); }
                        MapPos += new Point3D(0, 0, -1);
                    }
                    GameLoop.UIManager.Map.LoadMap(MapPos);
                    return true;
                }

                // Slope movement (RIGHT slope)
                if (map.GetTile(CurrentPos).Name == "Right Slope" && positionChange == new Point(1, 0)) {
                    if (!map.IsTileWalkable(newPosition)) {
                        // This is an up slope, move up a map instead of up on the current map
                        if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 0, 1))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 0, 1)); }
                        MapPos += new Point3D(0, 0, 1);
                    } else {
                        if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 0, -1))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 0, -1)); }
                        MapPos += new Point3D(0, 0, -1);
                    }
                    GameLoop.UIManager.Map.LoadMap(MapPos);
                    return true;
                }


                
                bool movedMaps = false;

                // Moved off the map (left)
                if (newPosition.X < 0) {
                    if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(-1, 0, 0))) { GameLoop.World.CreateMap(MapPos + new Point3D(-1, 0, 0)); }

                    MapPos += new Point3D(-1, 0, 0);
                    Position = new Point(GameLoop.MapWidth - 1, newPosition.Y);
                    movedMaps = true;
                }

                // Moved off the map (right)
                if (newPosition.X >= GameLoop.MapWidth) {
                    if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(1, 0, 0))) { GameLoop.World.CreateMap(MapPos + new Point3D(1, 0, 0)); }

                    MapPos += new Point3D(1, 0, 0);
                    Position = new Point(0, newPosition.Y);
                    movedMaps = true;
                }

                // Moved off the map (up)
                if (newPosition.Y < 0) {
                    if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, -1, 0))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, -1, 0)); }

                    MapPos += new Point3D(0, -1, 0);
                    Position = new Point(newPosition.X, GameLoop.MapHeight - 1);
                    movedMaps = true;
                }

                // Moved off the map (down)
                if (newPosition.Y >= GameLoop.MapHeight) {
                    if (!GameLoop.World.maps.ContainsKey(MapPos + new Point3D(0, 1, 0))) { GameLoop.World.CreateMap(MapPos + new Point3D(0, 1, 0)); }

                    MapPos += new Point3D(0, 1, 0);
                    Position = new Point(newPosition.X, 0);
                    movedMaps = true;
                }

                if (movedMaps && ID == GameLoop.World.Player.ID) {
                    GameLoop.UIManager.Map.LoadMap(MapPos);  
                    return true;
                } 

                if (newPosition.X >= 0 && newPosition.X <= GameLoop.MapWidth && newPosition.Y >= 0 && newPosition.Y <= GameLoop.MapHeight) {
                    if (map.GetTile(newPosition).Name.ToLower().Contains("door")) {
                        if (map.GetTile(newPosition).Lock.Closed) {
                            if (map.GetTile(newPosition).Lock.CanOpen()) {
                                map.ToggleLock(newPosition, MapPos);
                                return false;
                            } else {
                                GameLoop.UIManager.AddMsg(new ColoredString("The door won't budge. Must be locked.", Color.Brown, Color.Black));
                                return false;
                            }
                        }
                    }

                    if (map.GetTile(newPosition).Container != null) {
                        if (map.GetTile(newPosition).Lock != null && map.GetTile(newPosition).Lock.Closed) {
                            if (map.GetTile(newPosition).Lock.CanOpen()) {
                                map.ToggleLock(newPosition, MapPos);
                                return false;
                            } else {
                                GameLoop.UIManager.AddMsg(new ColoredString("The " + map.GetTile(newPosition).Name + " won't open. Must be locked.", Color.Brown, Color.Black));
                                return false;
                            }
                        } else {
                            GameLoop.UIManager.Container.SetupContainer(map.GetTile(newPosition).Container, newPosition);
                        }
                    }

                    if (map.IsTileWalkable(newPosition)) {
                        // if there's an NPC here, initiate dialogue
                        if (ID == GameLoop.World.Player.ID) {
                            foreach (KeyValuePair<string, NPC.NPC> kv in GameLoop.World.npcLibrary) {
                                NPC.NPC npc = kv.Value;
                                if (npc.Position == newPosition && npc.MapPos == MapPos) {
                                    GameLoop.UIManager.DialogueWindow.SetupDialogue(npc); 

                                    

                                    return false;
                                }
                            }

                            Tile tile = map.GetTile(newPosition);
                            if (tile.Name == "Sign") {
                                GameLoop.UIManager.SignText(newPosition, MapPos);
                                return true;
                            }
                        }

                        Position += positionChange; 
                        return true;
                    }
                }
            }
            return false;
        }

        public bool FlexibleMapMove(Point newPosition, Point3D mapLoc, Map map) {
            Position = newPosition; 
            MapPos = mapLoc;
            GameLoop.UIManager.Map.LoadMap(map);

            NetMsg movePlayer = new("movePlayer");
            movePlayer.SetFullPos(Position, MapPos);
            GameLoop.SendMessageIfNeeded(movePlayer, false, true);
             
            NetMsg reqMap = new("fullMap");
            reqMap.SetMap(MapPos);
            GameLoop.SendMessageIfNeeded(reqMap, false, false, 0); 

            return true;
        }

        public bool MoveTo(Point newPosition, Point3D mapLoc) { 
            bool movedMaps = false;

            Map map = Helper.ResolveMap(mapLoc);

            if (map != null) {
                Position = newPosition; 

                if (MapPos != mapLoc) { movedMaps = true; }
                MapPos = mapLoc;

                if (movedMaps && ID == GameLoop.World.Player.ID) {
                    GameLoop.UIManager.Map.LoadMap(map);
                }
            }

            return true;
        } 

        public void Death(bool drops = true) {
            Map map = Helper.ResolveMap(MapPos);

            if (map != null) {
                map.Remove(this);
                if (MapPos == GameLoop.World.Player.MapPos) {
                    GameLoop.UIManager.Map.EntityRenderer.Remove(this);
                    GameLoop.UIManager.Map.SyncMapEntities(map);
                    GameLoop.UIManager.AddMsg(this.Name + " died.");
                }
            }
        }

        public void ExpendStamina(int amount) {
            CurrentStamina -= amount;
            if (CurrentStamina < 0) {
                if (this == GameLoop.World.Player) {
                    if (GameLoop.NetworkManager == null) { // in single-player, skip to the next day
                        GameLoop.World.Player.Clock.NextDay(true);
                    } else {
                        GameLoop.UIManager.AddMsg(new ColoredString("You blacked out!", Color.Red, Color.Black));
                        MoveTo(new Point(18, 26), new Point3D(1, 1, 0));
                        CurrentStamina = 0;
                        CurrentHP -= 10;
                        if (CurrentHP <= 0) {
                            if (this is Player play) {
                                play.PlayerDied();
                            }
                            else {
                                Death();
                            }
                        }
                    }
                }
            }
        }
    }
}
