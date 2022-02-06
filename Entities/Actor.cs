using System;
using System.Collections.Generic;
using SadConsole;
using SadRogue.Primitives;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using ProtoBuf;
using Newtonsoft.Json;

namespace LofiHollow.Entities {
    [ProtoContract]
    [ProtoInclude(17, typeof(Monster))]
    [ProtoInclude(18, typeof(Player))]
    [ProtoInclude(19, typeof(NPC.NPC))]
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class Actor {
        [ProtoMember(1)]
        public int CurrentHP;
        [ProtoMember(2)]
        public int MaxHP;

        [ProtoMember(3)]
        public int CurrentStamina = 100;
        [ProtoMember(4)]
        public int MaxStamina = 100;

        [ProtoMember(5)]
        public Point3D MapPos = new(0, 0, 0);

        [ProtoMember(6)]
        public int Vision = 36;


        [ProtoMember(7)]
        public string CombatMode = "Attack";
        [ProtoMember(8)]
        public int CombatLevel = 1;

        [ProtoMember(9)]
        public Dictionary<string, Skill> Skills = new();

        [ProtoMember(10)]
        public int ForegroundR = 0;
        [ProtoMember(11)]
        public int ForegroundG = 0;
        [ProtoMember(12)]
        public int ForegroundB = 0;
        [ProtoMember(13)]
        public int ForegroundA = 255;
        [ProtoMember(14)]
        public int ActorGlyph = 0;

        [ProtoMember(15)]
        public Point Position;

        [ProtoMember(16)]
        public string Name = "";

        [JsonIgnore]
        public double TimeLastActed = 0;
        [JsonIgnore]
        public double TimeLastTicked = 0;


        public void Death(bool drops = true) {
            if (MapPos == GameLoop.World.Player.player.MapPos) {
                GameLoop.UIManager.Map.SyncMapEntities(GameLoop.World.maps[MapPos]);
                GameLoop.UIManager.AddMsg(Name + " died.");
            }

            if (drops && this is Monster mon)
                mon.SpawnDrops();

            GameLoop.World.maps[MapPos].SpawnedMonsters--;

            if (GameLoop.World.maps[MapPos].SpawnedMonsters == 0) {
                GameLoop.World.Player.player.MapsClearedToday++;
            }
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
                Death(true);
            }

            return damageTaken;
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

        public Actor() {
            MaxStamina = 100;
            CurrentStamina = MaxStamina;
        }

        public ColoredString GetAppearance() {
            return new ColoredString(((char)ActorGlyph).ToString(), new Color(ForegroundR, ForegroundG, ForegroundB), Color.Transparent);
        }

        public int CalculateCombatLevel() {
            if (this is Monster mon) {
                int combatStat = Math.Max(mon.MonAttack + mon.MonStrength, Math.Max(2 * mon.MonMagic, 2 * mon.MonRanged));

                CombatLevel = (int)Math.Floor((double)(((13 / 10) * combatStat) + mon.MonDefense + mon.MonConstitution) / 4);
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

            return CombatLevel;
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

            if (this is Player play) {
                for (int i = 0; i < play.Equipment.Length; i++) {
                    if (play.Equipment[i].Stats != null) {
                        Equipment Stats = play.Equipment[i].Stats;

                        if (type == "Slash")
                            StyleBonus += Stats.SlashBonus;
                        if (type == "Stab")
                            StyleBonus += Stats.StabBonus;
                        if (type == "Crush")
                            StyleBonus += Stats.CrushBonus;
                        if (type == "Range")
                            StyleBonus += Stats.RangeBonus;
                        if (type == "Magic")
                            StyleBonus += Stats.MagicBonus;
                    }
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

            if (this is Player play) {
                for (int i = 0; i < play.Equipment.Length; i++) {
                    if (play.Equipment[i].Stats != null) {
                        Equipment Stats = play.Equipment[i].Stats;
                        if (damageType == "Slash")
                            TotalDefenseBonus += Stats.ArmorVsSlash;
                        if (damageType == "Stab")
                            TotalDefenseBonus += Stats.ArmorVsStab;
                        if (damageType == "Crush")
                            TotalDefenseBonus += Stats.ArmorVsCrush;
                        if (damageType == "Range")
                            TotalDefenseBonus += Stats.ArmorVsRange;
                        if (damageType == "Magic")
                            TotalDefenseBonus += Stats.ArmorVsMagic;
                    }
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

            if (this is Player play) {
                for (int i = 0; i < play.Equipment.Length; i++) {
                    if (play.Equipment[i].Stats != null) {
                        Equipment Stats = play.Equipment[i].Stats;
                        if (damageType == "Slash")
                            TotalStrengthBonus += Stats.StrengthBonus;
                        if (damageType == "Stab")
                            TotalStrengthBonus += Stats.StrengthBonus;
                        if (damageType == "Crush")
                            TotalStrengthBonus += Stats.StrengthBonus;
                        if (damageType == "Range")
                            TotalStrengthBonus += Stats.RangeBonus;
                        if (damageType == "Magic")
                            TotalStrengthBonus += Stats.MagicBonus;
                    }
                }
            }

            int maxDamage = (int)Math.Floor((double)(((effectiveStrength * (TotalStrengthBonus + 64)) + 320) / 640));

            if (maxDamage > 1) {
                return (GameLoop.rand.Next(maxDamage) + 1);
            }

            return 1;
        }

        public string GetDamageType() {
            if (this is Player play) {
                if (play.Equipment[0].Stats != null) {
                    Equipment Stats = play.Equipment[0].Stats;
                    string[] types = Stats.DamageType.Split(",");

                    if (CombatMode == "Attack")
                        return types[0];
                    if (CombatMode == "Strength")
                        return types[1];
                    if (CombatMode == "Defense")
                        return types[2];
                    if (CombatMode == "Balanced")
                        return types[3];
                }
            }

            return "Crush";
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


                if (GameLoop.World.maps[MapPos].GetEntityAt<MonsterWrapper>(newPosition) != null) {
                    Monster monster = GameLoop.World.maps[MapPos].GetEntityAt<MonsterWrapper>(newPosition).Wrapped;

                    CommandManager.Attack(this, monster, true);
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
                            } else if (split[0] == "Monster Pen") {
                                GameLoop.UIManager.Minigames.MonsterPenManager.Setup(Int32.Parse(split[1]));
                                GameLoop.UIManager.Minigames.ToggleMinigame("Monster Pen");
                            } else if (split[0] == "Minigame") {
                                if (split[1] == "Bartending") {
                                    GameLoop.UIManager.Minigames.ToggleMinigame("Bartending");
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
                                                CommandManager.AddItemToInv(play, new Item(tile.ItemGiven));
                                                GameLoop.UIManager.AddMsg(tile.HarvestMessage);
                                                ExpendStamina(1);

                                                int choppedChance = GameLoop.rand.Next(100) + 1;

                                                if (choppedChance < 33) {
                                                    CommandManager.AddItemToInv(play, new Item(tile.DepletedItem));
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

                if (movedMaps && this == GameLoop.World.Player.player) {
                    GameLoop.UIManager.Map.LoadMap(MapPos);

                    NetMsg request = new("requestEntities", null);
                    request.SetMapPos(GameLoop.World.Player.player.MapPos);
                    GameLoop.SendMessageIfNeeded(request, false, false);

                    GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Entities.Clear();

                    if (GameLoop.SingleOrHosting()) {
                        if (!GameLoop.World.Player.player.VisitedMaps.Contains(MapPos)) {
                            GameLoop.World.maps[MapPos].PopulateMonsters(MapPos);
                            GameLoop.World.Player.player.VisitedMaps.Add(MapPos);
                        }
                    }

                    return true;
                }

                if (newPosition.X >= 0 && newPosition.X <= GameLoop.MapWidth && newPosition.Y >= 0 && newPosition.Y <= GameLoop.MapHeight) {
                    if (map.GetTile(newPosition).Name.ToLower().Contains("door")) {
                        if (map.GetTile(newPosition).Lock != null) {
                            if (map.GetTile(newPosition).Lock.Closed) {
                                if (map.GetTile(newPosition).Lock.CanOpen()) {
                                    map.ToggleLock(newPosition, MapPos);
                                    return false;
                                } else {
                                    GameLoop.UIManager.AddMsg(new ColoredString("The door won't budge. Must be locked.", Color.Brown, Color.Black));
                                    return false;
                                }
                            }
                        } else {
                            LockOwner defaultLock = new();
                            defaultLock.OpenedGlyph = 23;
                            defaultLock.ClosedGlyph = 24;
                            map.GetTile(newPosition).Lock = defaultLock;
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
                        if (this == GameLoop.World.Player.player) {
                            foreach (KeyValuePair<string, NPCWrapper> kv in GameLoop.World.npcLibrary) {
                                if (kv.Value.Position == newPosition && kv.Value.npc.MapPos == MapPos) {
                                    GameLoop.UIManager.DialogueWindow.SetupDialogue(kv.Value.npc);
                                    return false;
                                }
                            }

                            Tile tile = map.GetTile(Position + positionChange);
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

                if (MapPos != mapLoc) { movedMaps = true; }
                MapPos = mapLoc;

                if (movedMaps && this == GameLoop.World.Player.player) {
                    GameLoop.UIManager.Map.LoadMap(MapPos);
                }
            }

            return true;
        }

        public void ExpendStamina(int amount) {
            CurrentStamina -= amount;
            if (CurrentStamina < 0) {
                if (this == GameLoop.World.Player.player) {
                    if (GameLoop.NetworkManager == null) { // in single-player, skip to the next day
                        GameLoop.World.Player.player.Clock.NextDay(true);
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
