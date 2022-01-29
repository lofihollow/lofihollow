using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.EntityData;
using System.Text;
using LofiHollow.UI;
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

        public SadConsole.Console ScreenAppearance;


        [JsonProperty]
        public int SizeMod = 0;
        [JsonProperty]
        public int Vision = 36;


        [JsonProperty]
        public string CombatMode = "Attack";

        

        [JsonProperty]
        public int CombatLevel = 1;
        

        public double TimeLastActed = 0;


        [JsonProperty]
        public int CopperCoins = 0;
        [JsonProperty]
        public int SilverCoins = 0;
        [JsonProperty]
        public int GoldCoins = 0;
        [JsonProperty]
        public int JadeCoins = 0;

        [JsonProperty]
        public Item[] Inventory;
        [JsonProperty]
        public Item[] Equipment;


        [JsonProperty]
        public List<ItemDrop> DropTable = new();

        

        [JsonProperty]
        public Dictionary<string, Skill> Skills = new();


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

            if (SizeMod <= 0) {
                ScreenAppearance = new(1, 1);

                if (SizeMod <= -1)
                    ScreenAppearance.FontSize = new Point(6, 6);


                ScreenAppearance.Print(0, 0, new ColoredString(Appearance));
            }

            if (SizeMod >= 1) {
                int newSize = SizeMod + 1;
                ScreenAppearance = new(1, 1);
                ScreenAppearance.FontSize = new Point(12 * newSize, 12 * newSize); 
                
                ScreenAppearance.Print(0, 0, new ColoredString(Appearance));
            }

            if (SizeMod != 0)
                ScreenAppearance.UsePixelPositioning = true;

            UpdatePosition();
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
        
        public void UpdatePosition() {
            if (ScreenAppearance.UsePixelPositioning)
                ScreenAppearance.Position = new Point(Position.X * 12, Position.Y * 12);
            else
                ScreenAppearance.Position = Position;
        }

        protected Actor(Color foreground, int glyph, bool initInventory = false) : base(foreground, Color.Transparent, glyph) {
            Appearance.Foreground = foreground; 
            Appearance.Glyph = glyph;
            Appearance.Background = new Color(0, 0, 0, 50);

            ForegroundR = foreground.R;
            ForegroundG = foreground.G;
            ForegroundB = foreground.B;

            MaxStamina = 100;
            CurrentStamina = MaxStamina;
            
             

            Equipment = new Item[10];
            for (int i = 0; i < Equipment.Length; i++) {
                Equipment[i] = new Item("lh:(EMPTY)");
            }

            if (initInventory) {
                Inventory = new Item[9];

                for (int i = 0; i < Inventory.Length; i++) {
                    Inventory[i] = new Item("lh:(EMPTY)");
                }
            }
        }

        public ColoredString GetAppearance() {
            return new ColoredString(Appearance.GlyphCharacter.ToString(), Appearance.Foreground, Color.Transparent);
        }

        public void CalculateCombatLevel() {
            if (this is Monster mon) { 
                int combatStat = Math.Max(mon.MonAttack + mon.MonStrength, Math.Max(2 * mon.MonMagic, 2 * mon.MonRanged));

                CombatLevel = (int) Math.Floor((double) (((13 / 10) * combatStat) + mon.MonDefense + mon.MonConstitution) / 4);
            } else {
                int Attack = Skills["Attack"].Level;
                int Strength = Skills["Strength"].Level;
                int Magic = Skills["Magic"].Level;
                int Ranged = Skills["Ranged"].Level;
                int Defense = Skills["Defense"].Level;
                int Constitution = Skills["Constitution"].Level;

                int CombatStat = Math.Max(Attack + Strength, Math.Max(Magic, Ranged));

                CombatLevel = (int)Math.Floor((double)(((13 / 10) * CombatStat) + Defense + Constitution) / 4);
            }
        }

        public void UpdateHP() {
            if (this is Player) {
                MaxHP = Skills["Constitution"].Level;
                CurrentHP = MaxHP;
            } else {
                var mon = (Monster)this;
                MaxHP = mon.MonConstitution;
                CurrentHP = MaxHP;
            }
        }

        public int EffectiveAttackLevel(string damageType) {
            int effectiveAttackLevel = 0;
            if (damageType == "Crush" || damageType == "Slash" || damageType == "Stab") { 
                if (this is Player)
                    effectiveAttackLevel = Skills["Attack"].Level;
                else {
                    var mon = (Monster)this;
                    effectiveAttackLevel = mon.MonAttack;
                }

                if (CombatMode == "Attack")
                    effectiveAttackLevel += 3;
                if (CombatMode == "Balanced")
                    effectiveAttackLevel += 1;

                effectiveAttackLevel += 8;
            }

            return effectiveAttackLevel;
        }
         
        public int AttackRoll(string type) {
            int StyleBonus = 0;

            for (int i = 0; i < Equipment.Length; i++) {
                if (Equipment[i].Stats != null) {
                    if (type == "Slash")
                        StyleBonus += Equipment[i].Stats.SlashBonus;
                    if (type == "Stab")
                        StyleBonus += Equipment[i].Stats.StabBonus;
                    if (type == "Crush")
                        StyleBonus += Equipment[i].Stats.CrushBonus;
                    if (type == "Range")
                        StyleBonus += Equipment[i].Stats.RangeBonus;
                    if (type == "Magic")
                        StyleBonus += Equipment[i].Stats.MagicBonus;
                }
            }

            return EffectiveAttackLevel(type) * (StyleBonus + 64);
        }


        public int DefenceRoll(string damageType) {
            int effectiveDefenseLevel = 0;
            if (damageType == "Crush" || damageType == "Slash" || damageType == "Stab") { 
                if (this is Player)
                    effectiveDefenseLevel = Skills["Defense"].Level;
                else {
                    var mon = (Monster)this;
                    effectiveDefenseLevel = mon.MonDefense;
                }

                if (CombatMode == "Defense")
                    effectiveDefenseLevel += 3;
                if (CombatMode == "Balanced")
                    effectiveDefenseLevel += 1;

                effectiveDefenseLevel += 8;
            }


            int TotalDefenseBonus = 0;

            for (int i = 0; i < Equipment.Length; i++) {
                if (Equipment[i].Stats != null) {
                    if (damageType == "Slash")
                        TotalDefenseBonus += Equipment[i].Stats.ArmorVsSlash;
                    if (damageType == "Stab")
                        TotalDefenseBonus += Equipment[i].Stats.ArmorVsStab;
                    if (damageType == "Crush")
                        TotalDefenseBonus += Equipment[i].Stats.ArmorVsCrush;
                    if (damageType == "Range")
                        TotalDefenseBonus += Equipment[i].Stats.ArmorVsRange;
                    if (damageType == "Magic")
                        TotalDefenseBonus += Equipment[i].Stats.ArmorVsMagic;
                }
            }

            if (this is Player) {
                return effectiveDefenseLevel * (TotalDefenseBonus + 64);
            } else {
                var mon = (Monster)this;
                return (mon.MonDefense + 9) * (TotalDefenseBonus + 64);
            }
        }

        public int DamageRoll(string damageType) {
            int effectiveStrength = 0;
            if (damageType == "Crush" || damageType == "Slash" || damageType == "Stab") {
                

                if (this is Player)
                    effectiveStrength = Skills["Strength"].Level;
                else {
                    var mon = (Monster)this;
                    effectiveStrength = mon.MonStrength;
                }

                if (CombatMode == "Strength")
                    effectiveStrength += 3;
                if (CombatMode == "Balanced")
                    effectiveStrength += 1;

                effectiveStrength += 8;
            }

            int TotalStrengthBonus = 0;

            for (int i = 0; i < Equipment.Length; i++) {
                if (Equipment[i].Stats != null) {
                    if (damageType == "Slash")
                        TotalStrengthBonus += Equipment[i].Stats.StrengthBonus;
                    if (damageType == "Stab")
                        TotalStrengthBonus += Equipment[i].Stats.StrengthBonus;
                    if (damageType == "Crush")
                        TotalStrengthBonus += Equipment[i].Stats.StrengthBonus;
                    if (damageType == "Range")
                        TotalStrengthBonus += Equipment[i].Stats.RangeBonus;
                    if (damageType == "Magic")
                        TotalStrengthBonus += Equipment[i].Stats.MagicBonus;
                }
            }

            int maxDamage = (int) Math.Floor((double)(((effectiveStrength * (TotalStrengthBonus + 64)) + 320) / 640));

            if (maxDamage > 1) {
                return (GameLoop.rand.Next(maxDamage) + 1);
            } 

            return 1;
        }

        public string GetDamageType() {
            if (Equipment[0].Stats != null) {
                string[] types = Equipment[0].Stats.DamageType.Split(",");

                if (CombatMode == "Attack")
                    return types[0];
                if (CombatMode == "Strength")
                    return types[1];
                if (CombatMode == "Defense")
                    return types[2];
                if (CombatMode == "Balanced")
                    return types[3];
            }

            return "Crush";
        }



        public bool HasInventorySlotOpen(string stackID = "") {
            for (int i = 0; i < Inventory.Length; i++) {
                if (Inventory[i].Name == "(EMPTY)" || (Inventory[i].Name == stackID && stackID != "")) {
                    return true;
                }
            }
            return false;
        }

        public int TakeDamage(int damage) {
            int damageTaken = 0;
            if (damage > CurrentHP) {
                damageTaken = CurrentHP;
                CurrentHP = 0; 
            } else {
                CurrentHP -= damage;
                damageTaken = damage;
            }

            if (CurrentHP <= 0) {
                if (this is Player player)
                    player.PlayerDied();
                else
                    Death(true);
            }

            return damageTaken;
        }

        public bool MoveBy(Point positionChange) {
            int AgilityLevel = 0;
            if (TimeLastActed + (120 - (AgilityLevel)) > SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds) {
                return false;
            }

            TimeLastActed = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds;

            if (GameLoop.World.maps.TryGetValue(MapPos, out Map map)) {
                Point newPosition = Position + positionChange;
                if (newPosition.Y < 0 && GameLoop.World.maps.ContainsKey(MapPos - new Point3D(0, -1, 0)) && GameLoop.World.maps[MapPos - new Point3D(0, -1, 0)].MinimapTile.name == "Desert") {
                    GameLoop.UIManager.AddMsg("There's dangerous sandstorms that way, best not go there for now.");
                    return false;
                }


                if (GameLoop.World.maps[MapPos].GetEntityAt<Monster>(newPosition) != null) {
                    Monster monster = GameLoop.World.maps[MapPos].GetEntityAt<Monster>(newPosition);

                    CommandManager.Attack(this, monster, true);
                    return false;
                }


                // Interact with skilling tiles
                if (ID == GameLoop.World.Player.ID) {
                    if (newPosition.X < GameLoop.MapWidth && newPosition.X >= 0 && newPosition.Y < GameLoop.MapHeight && newPosition.Y >= 0) {
                        if (map.GetTile(newPosition).MiscString != "" && map.GetTile(newPosition).MiscString.Split(",").Length > 1) {
                            string[] split = map.GetTile(newPosition).MiscString.Split(",");
                            if (split[0] == "Skill") {
                                if (split.Length > 2)
                                    GameLoop.UIManager.Crafting.SetupCrafting(split[1], split[2], Int32.Parse(split[3]));
                                else
                                    GameLoop.UIManager.Crafting.SetupCrafting(split[1], "None", 1);
                            } else if (split[0] == "Monster Pen") {
                                GameLoop.UIManager.Minigames.CurrentGame = "Monster Pen";
                                GameLoop.UIManager.Minigames.MonsterPenManager.Setup(Int32.Parse(split[1]));
                                GameLoop.UIManager.Minigames.ToggleMinigame();
                            }
                        }

                        if (map.GetTile(newPosition).SkillableTile != null) {
                            SkillableTile tile = map.GetTile(newPosition).SkillableTile;
                            if (map.GetTile(newPosition).Name == tile.HarvestableName) {
                                if (Skills.ContainsKey(tile.RequiredSkill)) {
                                    if (Skills[tile.RequiredSkill].Level >= tile.RequiredLevel) {
                                        if (Equipment[0].ItemCategory == tile.HarvestTool || Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemCategory == tile.HarvestTool) {
                                            if (HasInventorySlotOpen()) {
                                                CommandManager.AddItemToInv(this, new Item(tile.ItemGiven));
                                                GameLoop.UIManager.AddMsg(tile.HarvestMessage);
                                                ExpendStamina(1);

                                                int choppedChance = GameLoop.rand.Next(100) + 1;

                                                if (choppedChance < 33) {
                                                    CommandManager.AddItemToInv(this, new Item(tile.DepletedItem));
                                                    map.GetTile(newPosition).Name = tile.DepletedName;
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

                // Slope movement (UP slope)
                if (map.GetTile(Position).Name == "Up Slope" && positionChange == new Point(0, -1)) {
                    if (!map.IsTileWalkable(Position + positionChange)) {
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
                if (map.GetTile(Position).Name == "Down Slope" && positionChange == new Point(0, 1)) {
                    if (!map.IsTileWalkable(Position + positionChange)) {
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
                if (map.GetTile(Position).Name == "Left Slope" && positionChange == new Point(-1, 0)) {
                    if (!map.IsTileWalkable(Position + positionChange)) {
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
                if (map.GetTile(Position).Name == "Right Slope" && positionChange == new Point(1, 0)) {
                    if (!map.IsTileWalkable(Position + positionChange)) {
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
                    GameLoop.SendMessageIfNeeded(new string[] { "requestEntities", GameLoop.World.Player.MapPos.ToString() }, false, false, 0);
                    
                    GameLoop.World.maps[GameLoop.World.Player.MapPos].Entities.Clear(); 

                    if (GameLoop.SingleOrHosting()) {
                        if (!GameLoop.World.Player.VisitedMaps.Contains(MapPos)) {
                            GameLoop.World.maps[MapPos].PopulateMonsters(MapPos);
                            GameLoop.World.Player.VisitedMaps.Add(MapPos);
                        }
                    }

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

                    if (map.IsTileWalkable(Position + positionChange)) {
                        // if there's an NPC here, initiate dialogue
                        if (ID == GameLoop.World.Player.ID) {
                            for (int i = 0; i < GameLoop.World.npcLibrary.Count; i++) {
                                NPC.NPC npc = GameLoop.World.npcLibrary[i];
                                if (npc.Position == newPosition && npc.MapPos == MapPos) {
                                    GameLoop.UIManager.DialogueWindow.SetupDialogue(npc); 

                                    

                                    return false;
                                }
                            }

                            TileBase tile = map.GetTile(Position + positionChange);
                            if (tile.Name == "Sign") {
                                GameLoop.UIManager.SignText(Position + positionChange, MapPos);
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

        public bool MoveTo(Point newPosition, Point3D mapLoc) { 
            bool movedMaps = false;
            if (!GameLoop.World.maps.ContainsKey(mapLoc)) { GameLoop.World.CreateMap(mapLoc); }

            if (GameLoop.World.maps[mapLoc].IsTileWalkable(newPosition)) {
                Position = newPosition;

                if (ScreenAppearance == null) {
                    UpdateAppearance();
                }

                ScreenAppearance.Position = newPosition;

                if (MapPos != mapLoc) { movedMaps = true; } 
                MapPos = mapLoc;

                if (movedMaps && ID == GameLoop.World.Player.ID) {
                    GameLoop.UIManager.Map.LoadMap(MapPos);
                }
            }

            return true;
        } 
        

        public void SpawnDrops() {
            for (int i = 0; i < DropTable.Count; i++) {
                ItemDrop drop = DropTable[i];

                int roll = GameLoop.rand.Next(drop.DropChance);

                if (roll == 0) {
                    ItemWrapper item = new(drop.Name);

                    if (item.item.IsStackable) {
                        item.item.ItemQuantity = GameLoop.rand.Next(drop.DropQuantity) + 1;
                        item.Position = Position;
                        item.MapPos = MapPos;
                        CommandManager.SpawnItem(item);
                    } else {
                        int qty = GameLoop.rand.Next(drop.DropQuantity) + 1;

                        for (int j = 0; j < qty; j++) {
                            ItemWrapper itemNonStack = new(drop.Name);
                            itemNonStack.Position = Position;
                            itemNonStack.MapPos = MapPos;
                            CommandManager.SpawnItem(itemNonStack);
                        }
                    }
                }
            } 
        } 

        public void Death(bool drops = true) {
            GameLoop.World.maps[MapPos].Remove(this);
            if (MapPos == GameLoop.World.Player.MapPos) {
                GameLoop.UIManager.Map.EntityRenderer.Remove(this);
                GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[MapPos]);
                GameLoop.UIManager.AddMsg(this.Name + " died.");
            } 

            if (drops)
                SpawnDrops();

            GameLoop.World.maps[MapPos].SpawnedMonsters--;

            if (GameLoop.World.maps[MapPos].SpawnedMonsters == 0) {
                GameLoop.World.Player.MapsClearedToday++; 
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
                        MoveTo(new Point(35, 6), new Point3D(0, 0, 0));
                        CurrentStamina = 0;
                    }
                }
            }
        }
    }
}
