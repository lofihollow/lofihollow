using LofiHollow.Entities;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using System.Linq;
using System;
using LofiHollow.Managers;
using Steamworks;
using LofiHollow.Minigames.Photo;
using LofiHollow.EntityData;

namespace LofiHollow.UI { 
    public class UI_Combat : Lofi_UI {  
        public Combat Current;

        public int CurrentTarget = 0;
        public int CurrentAllyTurn = 0;

        public int CombatLogTop = 0;
        public List<TurnAction> SubmittedTurns = new();
        public List<ColoredString> RecentCombatMsgs = new();


        public bool UsedItemThisTurn = false;
        public bool TriedToFleeThisTurn = false;
        public bool CombatDone = false;
        public bool PickingDrops = false;

        public SadConsole.UI.Window InvWindow;
        public SadConsole.Console InvConsole;
        public int InvTopIndex = 0;

        public UI_Combat(int width, int height, string title) : base(width, height, title, "Combat") {
            Win.Position = new(11, 4);
            InvWindow = new(52, 32);
            InvWindow.Title = "[INVENTORY]";
            InvWindow.TitleAlignment = HorizontalAlignment.Center;
            InvWindow.Position = new Point(9, 4);
            InvWindow.CanDrag = false;

            Win.Children.Add(InvWindow);

            InvConsole = new(50, 30);
            InvConsole.Position = new Point(1, 1);
            InvWindow.Children.Add(InvConsole);

            InvWindow.IsVisible = false;

        }

        public void StartCombat(string loc, int levelMin, int levelMax) {
            if (SteamClient.IsValid) {
                Current = new();
                CombatDone = false; 
                CurrentAllyTurn = 0;
                CurrentTarget = 0;
                PickingDrops = false;
                TriedToFleeThisTurn = false;
                UsedItemThisTurn = false;

                SpawnEnemies(loc, levelMin, levelMax);

                if (Current.AllParticipants.Count == 0)
                    Toggle();

                for (int i = 0; i < Current.AllParticipants.Count; i++) {
                    if (Current.AllParticipants[i].Enemy && Current.AllParticipants[i].Owner == 0) {
                        ApplyLevels(Current.AllParticipants[i].Level, Current.AllParticipants[i]); 
                    }
                }

                if (GameLoop.World.Player.Equipment[10].Name != "(EMPTY)") {
                    SoulPhoto photo = GameLoop.World.Player.Equipment[10].SoulPhoto;
                    if (photo != null) {
                        Current.AllParticipants.Add(photo.GetCombined(SteamClient.SteamId));
                    } else {
                        Current.AllParticipants.Add(GameLoop.World.Player.GetCombatParticipant());
                    }
                }
                else {
                    Current.AllParticipants.Add(GameLoop.World.Player.GetCombatParticipant());
                }
                 
                SubmittedTurns.Clear();

                /*
                Current.BattleHost = SteamClient.SteamId;

                for (int i = 0; i < GameLoop.World.Player.PartyMembers.Count; i++) {
                    Current.OtherPlayers.Add(GameLoop.World.Player.PartyMembers[i]);
                }

                for (int i = 0; i < Current.OtherPlayers.Count; i++) {
                    if (GameLoop.World.otherPlayers.ContainsKey(Current.OtherPlayers[i])) {
                        Player play = GameLoop.World.otherPlayers[Current.OtherPlayers[i]];
                        if (play.MapPos == GameLoop.World.Player.MapPos) {
                            NetMsg initiateCombat = new("startCombat", Current.ToByteArray()); 
                            GameLoop.SendMessageIfNeeded(initiateCombat, false, true, Current.OtherPlayers[i]);
                        }
                    }
                }
                */
            }
        }

        public void SpawnEnemies(string loc, int levelMin, int levelMax) {
            List<Monster> possible = new();

            foreach (KeyValuePair<string, Monster> kv in GameLoop.World.monsterLibrary) {
                if ((kv.Value.SpawnLocation.Contains(loc) || loc == "Any" || kv.Value.SpawnLocation == "Any") && (kv.Value.Level >= levelMin && kv.Value.Level <= levelMax)) {
                    possible.Add(kv.Value);
                }
            }

            if (possible.Count == 0)
                return;

            Monster rand = possible[GameLoop.rand.Next(possible.Count)];
            rand.HealthGrowth = GameLoop.rand.Next(16) + 1; 
            rand.SpeedGrowth = GameLoop.rand.Next(16) + 1; 
            rand.AttackGrowth = GameLoop.rand.Next(16) + 1; 
            rand.DefenseGrowth = GameLoop.rand.Next(16) + 1;
            rand.MAttackGrowth = GameLoop.rand.Next(16) + 1;
            rand.MDefenseGrowth = GameLoop.rand.Next(16) + 1;

            CombatParticipant one = rand.GetCombatParticipant(0, rand.Level, rand.Species);
            one.Enemy = true;
            Current.AllParticipants.Add(one);

            if (GameLoop.rand.Next(10) == 0) {
                Monster rand2 = possible[GameLoop.rand.Next(possible.Count)];
                rand2.HealthGrowth = GameLoop.rand.Next(16) + 1;
                rand2.SpeedGrowth = GameLoop.rand.Next(16) + 1;
                rand2.AttackGrowth = GameLoop.rand.Next(16) + 1;
                rand2.DefenseGrowth = GameLoop.rand.Next(16) + 1;
                rand2.MAttackGrowth = GameLoop.rand.Next(16) + 1;
                rand2.MDefenseGrowth = GameLoop.rand.Next(16) + 1;

                CombatParticipant two = rand2.GetCombatParticipant(0, rand2.Level, rand2.Species);
                two.Enemy = true;
                Current.AllParticipants.Add(two);
            }
        }

        public void ApplyLevels(int level, CombatParticipant mon) {
            mon.Health += (int) Math.Ceiling(((float) mon.HealthGrowth / 4f)) * level;
            mon.Speed += (int) Math.Ceiling(((float)mon.SpeedGrowth / 4f)) * level;
            mon.Attack += (int) Math.Ceiling(((float)mon.AttackGrowth / 4f)) * level;
            mon.Defense += (int) Math.Ceiling(((float)mon.DefenseGrowth / 4f)) * level;
            mon.MAttack += (int) Math.Ceiling(((float)mon.MAttackGrowth / 4f)) * level;
            mon.MDefense += (int) Math.Ceiling(((float)mon.MDefenseGrowth / 4f)) * level;
             
            mon.UpdateHP();

            for (int i = 0; i < mon.ContainedMon.MoveList.Count; i++) {
                string[] moveSplit = mon.ContainedMon.MoveList[i].Split(",");
                if (GameLoop.World.moveLibrary.ContainsKey(moveSplit[1])) {
                    int learnedLevel = Int32.Parse(moveSplit[0]);
                    Move move = GameLoop.World.moveLibrary[moveSplit[1]];
                    bool alreadyKnown = false;
                    
                    if (learnedLevel <= mon.Level) {
                        for (int j = 0; j < mon.KnownMoves.Count; j++) {
                            if (mon.KnownMoves[j].FullName() == move.FullName()) {
                                alreadyKnown = true;
                            }
                        }

                        if (!alreadyKnown) {
                            mon.KnownMoves.Add(move);
                        }
                    }
                }
            }
        }


        public void AddMsg(ColoredString msg) {
            RecentCombatMsgs.Add(msg);
            CombatLogTop = RecentCombatMsgs.Count - 1;
        }

        public override void Render() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            Con.Clear(); 

            if (Current != null) { 
                int allyX = 15;
                int allyY = 1;

                int enemyX = 65;
                int enemyY = 1;
                

                for (int i = 0; i < Current.AllParticipants.Count; i++) {
                    if (!Current.AllParticipants[i].Enemy) { // Render an ally
                        Con.Print(12, allyY, Current.AllParticipants[i].Name, CurrentTarget == i ? Color.Yellow : Color.White);

                        int percent = (int)Math.Ceiling((double)((double)Current.AllParticipants[i].CurrentHP / (double)Current.AllParticipants[i].MaxHP) * 10);
                        Con.DrawLine(new Point(1, allyY), new Point(percent, allyY), 254, Color.Lime);

                        allyY++;

                        if (CurrentTarget == i) {
                            Con.Print(allyX, 20, "|");
                            Con.Print(allyX, 21, "|");
                            Con.Print(allyX, 22, "V");
                        }

                        Con.Print(allyX, 24, Current.AllParticipants[i].GetAppearance()); 
                        Con.Print(allyX - 1, 26, "[" + i + "]");
                        allyX += 5;


                        if (Current.AllParticipants[i].Owner == SteamClient.SteamId) {
                            Con.Print(0, 28, "SPD __| ATK __| DEF __| MATK __| MDEF __|");

                            int spd = Current.AllParticipants[i].SpdBonus;
                            Color spdCol = spd < 0 ? Color.Red : spd > 0 ? Color.Lime : Color.White;
                            Con.Print(4, 28, spd.ToString().Align(HorizontalAlignment.Right, 2, ' '), spdCol);

                            int atk = Current.AllParticipants[i].AtkBonus;
                            Color atkCol = atk < 0 ? Color.Red : atk > 0 ? Color.Lime : Color.White;
                            Con.Print(12, 28, atk.ToString().Align(HorizontalAlignment.Right, 2, ' '), atkCol);

                            int def = Current.AllParticipants[i].DefBonus;
                            Color defCol = def < 0 ? Color.Red : def > 0 ? Color.Lime : Color.White;
                            Con.Print(20, 28, def.ToString().Align(HorizontalAlignment.Right, 2, ' '), defCol);

                            int matk = Current.AllParticipants[i].MAtkBonus;
                            Color matkCol = matk < 0 ? Color.Red : matk > 0 ? Color.Lime : Color.White;
                            Con.Print(29, 28, matk.ToString().Align(HorizontalAlignment.Right, 2, ' '), matkCol);

                            int mdef = Current.AllParticipants[i].MDefBonus;
                            Color mdefCol = mdef < 0 ? Color.Red : mdef > 0 ? Color.Lime : Color.White;
                            Con.Print(38, 28, mdef.ToString().Align(HorizontalAlignment.Right, 2, ' '), spdCol);
                        }
                    } else { // Render an enemy
                        Con.PrintClickable(28, enemyY, new ColoredString((417.AsString() + Current.AllParticipants[i].Level + " " + Current.AllParticipants[i].Name).Align(HorizontalAlignment.Right, 30), CurrentTarget == i ? Color.Yellow : Color.White, Color.Black), TargetClick, i.ToString());

                        int percent = (int)Math.Ceiling((double)((double)Current.AllParticipants[i].CurrentHP / (double)Current.AllParticipants[i].MaxHP) * 10);
                        Con.DrawLine(new Point(59, enemyY), new Point(58 + percent, enemyY), 254, Color.Lime);
                        enemyY++;

                        if (CurrentTarget == i) {
                            Con.Print(enemyX, 20, "|");
                            Con.Print(enemyX, 21, "|");
                            Con.Print(enemyX, 22, "V");
                        }

                        Con.PrintClickable(enemyX, 24, Current.AllParticipants[i].GetAppearance(), TargetClick, i.ToString());
                        Con.Print(enemyX - 1, 26, "[" + i + "]");
                        enemyX -= 5;
                    }
                }

                Con.DrawLine(new Point(0, 25), new Point(69, 25), 196, Color.Green); 
                Con.DrawLine(new Point(0, 27), new Point(69, 27), 196, Color.White);
                Con.DrawLine(new Point(0, 29), new Point(69, 29), 196, Color.White);

                Con.PrintClickable(1, 30, "Attack".Align(HorizontalAlignment.Center, 20), UI_Clicks, "attack");
                Con.PrintClickable(1, 33, "Change Stance".Align(HorizontalAlignment.Center, 20), UI_Clicks, "stance");
                Con.Print(1, 34, (GameLoop.World.Player.CombatMode + " (" + GameLoop.World.Player.GetDamageType() + ")").Align(HorizontalAlignment.Center, 20));
                 
                Con.PrintClickable(1, 37, "Use Item".Align(HorizontalAlignment.Center, 20), UI_Clicks, "item");

                if (GameLoop.World.Player.Equipment[10].SoulPhoto != null || GameLoop.World.Player.HasSoulPhoto())
                    Con.PrintClickable(1, 38, "Switch".Align(HorizontalAlignment.Center, 20), UI_Clicks, "switch");



                Con.PrintClickable(1, 49, "Flee".Align(HorizontalAlignment.Center, 20), UI_Clicks, "flee");

                Con.DrawLine(new Point(22, 30), new Point(22, 49), 179, Color.White);

                CombatLogTop = RecentCombatMsgs.Count - 1;

                for (int i = CombatLogTop; i > 0 && i > CombatLogTop - 20; i--) {
                    int line = CombatLogTop - i;
                    Con.Print(23, 30 + line, RecentCombatMsgs[i]);
                }
            }

            if (Current == null && PickingDrops) {
                Con.Print(0, 5, "Monster Drops:".Align(HorizontalAlignment.Center, 70), Color.DarkSlateBlue);
                Con.Print(0, 6, "[SPACE] to close".Align(HorizontalAlignment.Center, 70), Color.DarkSlateGray);

                for (int i = 0; i < Current.Drops.Count; i++) {
                    Con.PrintClickable(0, 8 + (2 * i), (Current.Drops[i].ItemQuantity + "x " + Current.Drops[i].Name).Align(HorizontalAlignment.Center, 70), DropClicks, i.ToString());
                }
            }

            if (InvWindow.IsVisible)
                RenderInv();
        }


        public void PrintTurnResult(TurnResult turn) {
            if (turn.Damage != null)
                AddMsg(turn.Damage);
            if (turn.OwnStatus != null)
                AddMsg(turn.OwnStatus);
            if (turn.TargetStatus != null)
                AddMsg(turn.TargetStatus);
            AddMsg(new ColoredString("---------------------------------", Color.DarkSlateGray, Color.Black));
        }


        public void TargetClick(string ID) {
            int slot = Int32.Parse(ID);
            CurrentTarget = slot;
        }

        public void DropClicks(string ID) {
            int slot = Int32.Parse(ID);

            CommandManager.AddItemToInv(GameLoop.World.Player, Current.Drops[slot]);
            Current.Drops.RemoveAt(slot);

            if (Current.Drops.Count == 0) {
                Toggle();
                RecentCombatMsgs.Clear();
                Con.Clear();
            }
        }

        public override void Input() { 
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape) && CombatDone && !PickingDrops) {
                if (Current.Drops.Count == 0) {
                    Toggle();
                    RecentCombatMsgs.Clear();
                    Con.Clear();
                } else {
                    Current = null;
                    PickingDrops = true;
                }
            }

            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Space) && CombatDone && PickingDrops) { 
                Toggle();
                RecentCombatMsgs.Clear();
                Con.Clear(); 
            }

            if (Current != null) {
                if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                    if (!InvWindow.IsVisible) {
                        if (CurrentTarget + 1 < Current.AllParticipants.Count)
                            CurrentTarget++;
                        else
                            CurrentTarget = 0;
                    } else {
                        if (InvTopIndex < GameLoop.World.Player.Inventory.Length - 15)
                            InvTopIndex++;
                    }
                } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                    if (!InvWindow.IsVisible) {
                        if (CurrentTarget > 0)
                            CurrentTarget--;
                        else
                            CurrentTarget = Current.AllParticipants.Count - 1;
                    } else {
                        if (InvTopIndex > 0)
                            InvTopIndex--;
                    }
                }
            }
        } 

        public override void UI_Clicks(string id) {
            if (id == "stance") {
                GameLoop.World.Player.SwitchMeleeMode();
            }

            else if (id == "flee" && !CombatDone) {
                if (!TriedToFleeThisTurn) {
                    CombatParticipant target = Current.AllParticipants[CurrentTarget];

                    CombatParticipant player = null;

                    for (int i = 0; i < Current.AllParticipants.Count; i++) {
                        if (Current.AllParticipants[i].Owner == SteamClient.SteamId) {
                            player = Current.AllParticipants[i];
                        }
                    }

                    if (target != player) {
                        if (player.StatWithStage("Speed") > target.StatWithStage("Speed")) {
                            CombatDone = true;
                            AddMsg(new ColoredString(player.Name + " fled successfully - ESC to close.", Color.Lime, Color.Black));
                        }
                        else {
                            int playSpeed = player.Speed;
                            int enemySpeed = target.Speed;

                            int escape = (int)((Math.Floor((double)(playSpeed * 128) / enemySpeed) + 30) % 256);

                            if (GameLoop.rand.Next(256) < escape) {
                                CombatDone = true;
                                AddMsg(new ColoredString(player.Name + " fled successfully - ESC to close.", Color.Lime, Color.Black));
                            }
                            else {
                                AddMsg(new ColoredString(player.Name + " couldn't escape!", Color.Red, Color.Black));
                            }
                        }

                        TriedToFleeThisTurn = true;
                    }
                } else {
                    AddMsg(new ColoredString("Already tried to flee this turn.", Color.Yellow, Color.Black));
                }
            }

            else if (id == "attack" && !CombatDone) {
                if (Current.AllParticipants[CurrentTarget].Enemy) {
                    Move playerAttack = new();

                    if (GameLoop.World.Player.Equipment[0] != null) {
                        if (GameLoop.World.Player.Equipment[0].Stats != null) {
                            Equipment weapon = GameLoop.World.Player.Equipment[0].Stats;
                            playerAttack.Name = "Slash";
                            playerAttack.Type = weapon.DamageType;
                            playerAttack.Power = weapon.Power;
                            playerAttack.Accuracy = weapon.Accuracy;
                        } else {
                            playerAttack.Name = "Punch";
                            playerAttack.Type = "Fire";
                            playerAttack.Power = 10;
                            playerAttack.Accuracy = 30; 
                        }
                    }

                    CombatParticipant target = Current.AllParticipants[CurrentTarget];

                    CombatParticipant player = null;

                    for (int i = 0; i < Current.AllParticipants.Count; i++) {
                        if (Current.AllParticipants[i].Owner == SteamClient.SteamId && Current.AllParticipants[i].ID == "Player") {
                            player = Current.AllParticipants[i];
                        }
                    }

                    TurnAction turn = new();
                    turn.Owner = player;
                    turn.Target = target;
                    turn.Action = "Attack";
                    turn.UsingMove = playerAttack;
                    turn.Speed = player.StatWithStage("Speed");

                    // MAKE IT CHECK TO SEE IF THE PLAYER HAS ALREADY SENT A TURN FOR MULTIPLAYER COMBAT
                    SubmittedTurns.Add(turn);

                    ExecuteAllTurns();
                }

                /*
                if (Current.Allies[CurrentAllyTurn].playerID == GameLoop.NetworkManager.ownID && Current.Enemies[CurrentTarget].GetActor() != null) {
                    if (Current.Enemies[CurrentTarget].GetActor() is MonsterWrapper mon) {
                        ColoredString result = CommandManager.Attack(GameLoop.World.Player, mon);
                        RecentCombatMsgs.Push(result);
                        PerformCleanup();
                    }
                }
                */
            } 

            else if (id == "item" && !CombatDone) {
                if (Current != null) {
                    InvWindow.IsVisible = true;
                }
            }
        }

        public void InvClicks(string ID) {
            if (ID == "closeInv") {
                InvWindow.IsVisible = false;
            }

            else {
                string[] split = ID.Split(",");
                int slot = Int32.Parse(split[1]);
                Item item = GameLoop.World.Player.Inventory[slot];

                if (!UsedItemThisTurn) {
                    if (split[0] == "equip") {
                        CommandManager.EquipItem(GameLoop.World.Player, slot, item);
                        UsedItemThisTurn = true;
                    }
                    else if (split[0] == "use") {
                        if (item.ItemCat == "Camera") {
                            if (item.IsStackable) {
                                CommandManager.RemoveOneItem(GameLoop.World.Player, slot);
                            }
                            Item photo = Item.Copy("lh:MonsterPhoto");
                            CombatParticipant target = Current.AllParticipants[CurrentTarget];
                            photo.Name = "Photo (" + target.Name + ")";
                            CommandManager.AddItemToInv(GameLoop.World.Player, photo);
                            UsedItemThisTurn = true;
                        }
                        else if (item.ItemCat == "Consumable") {
                            if (item.Heal != null) {
                                for (int i = 0; i < Current.AllParticipants.Count; i++) {
                                    if (Current.AllParticipants[i].Owner == SteamClient.SteamId) {
                                        if (Current.AllParticipants[i].CurrentHP != Current.AllParticipants[i].MaxHP) {
                                            int heal = GoRogue.DiceNotation.Dice.Roll(item.Heal.HealAmount);
                                            Current.AllParticipants[i].TakeDamage(heal * -1);
                                            UsedItemThisTurn = true;
                                            AddMsg(new("Used " + item.Name + " and healed " + heal + "!"));
                                            break;
                                        } else {
                                            AddMsg(new("Already at max HP!"));
                                        }
                                    }
                                }
                            }

                            if (item.Tonic != null) {
                                for (int i = 0; i < Current.AllParticipants.Count; i++) {
                                    CombatParticipant thisOne = Current.AllParticipants[i];
                                    if (thisOne.Owner == SteamClient.SteamId) {
                                        if (item.Tonic.StatChanged == "Clear") { 
                                            thisOne.SpdBonus = 0;
                                            thisOne.AtkBonus = 0;
                                            thisOne.DefBonus = 0;
                                            thisOne.MAtkBonus = 0;
                                            thisOne.MDefBonus = 0;

                                            AddMsg(new("Used " + item.Name + " and cleared all stat changes!"));

                                            break;
                                        }
                                        else if (item.Tonic.StatChanged == "LeaveOnlyNegative") { 
                                            if (thisOne.SpdBonus > 0)
                                                thisOne.SpdBonus = 0;
                                            if (thisOne.AtkBonus > 0)
                                                thisOne.AtkBonus = 0;
                                            if (thisOne.DefBonus > 0)
                                                thisOne.DefBonus = 0;
                                            if (thisOne.MAtkBonus > 0)
                                                thisOne.MAtkBonus = 0;
                                            if (thisOne.MDefBonus > 0)
                                                thisOne.MDefBonus = 0;

                                            AddMsg(new("Used " + item.Name + " and cleared positive stat changes!"));

                                            break;
                                        }

                                        else if (item.Tonic.StatChanged == "LeaveOnlyPositive") { 
                                            if (thisOne.SpdBonus < 0)
                                                thisOne.SpdBonus = 0;
                                            if (thisOne.AtkBonus < 0)
                                                thisOne.AtkBonus = 0;
                                            if (thisOne.DefBonus < 0)
                                                thisOne.DefBonus = 0;
                                            if (thisOne.MAtkBonus < 0)
                                                thisOne.MAtkBonus = 0;
                                            if (thisOne.MDefBonus < 0)
                                                thisOne.MDefBonus = 0;

                                            AddMsg(new("Used " + item.Name + " and cleared negative stat changes!"));

                                            break;
                                        }

                                        else {
                                            thisOne.FlatChange(item.Tonic.StatChanged, item.Tonic.AmountChanged);
                                            int amountChanged = item.Tonic.AmountChanged;
                                            ColoredString statusChange;

                                            if (amountChanged == -1) { statusChange = new ColoredString(thisOne.Name + " used " + item.Name + ", " + item.Tonic.StatChanged + "-1!", Color.Crimson, Color.Black); }
                                            else if (amountChanged < -1) { statusChange = new ColoredString(thisOne.Name + " used " + item.Name + ", " + item.Tonic.StatChanged + "-" + amountChanged + "!", Color.Red, Color.Black); }
                                            else if (amountChanged == 1) { statusChange = new ColoredString(thisOne.Name + " used " + item.Name + ", " + item.Tonic.StatChanged + " +1!", Color.Green, Color.Black); }
                                            else { statusChange = new ColoredString(thisOne.Name + " used " + item.Name + ", " + item.Tonic.StatChanged + " +" + amountChanged + "!", Color.LimeGreen, Color.Black); }

                                            UsedItemThisTurn = true;

                                            AddMsg(statusChange);
                                            break;
                                        }
                                    }
                                }
                            }

                            if (UsedItemThisTurn)
                                CommandManager.RemoveOneItem(GameLoop.World.Player, slot);
                        } 
                    }

                    if (UsedItemThisTurn) {
                        InvWindow.IsVisible = false;
                        // ADD IT TO THE SUBMITTED TURNS LATER FOR MULTIPLAYER COMBAT
                        ExecuteAllTurns();
                    }
                }
                else {
                    AddMsg(new ColoredString("You already used an item this turn!", Color.Yellow, Color.Black));
                }
            } 
        }

        public void RenderInv() {
            InvWindow.IsFocused = true;

            if (GameHost.Instance.Keyboard.IsKeyDown(Key.Escape)) {
                InvWindow.IsVisible = false;
            }


            InvConsole.Clear();

            InvConsole.PrintClickable(49, 0, "X", InvClicks, "closeInv");

            InvConsole.Print(0, 1, "Name".Align(HorizontalAlignment.Center, 20) + "|" + "Description".Align(HorizontalAlignment.Center, 21) + "|", Color.DarkSlateBlue);
            InvConsole.DrawLine(new Point(0, 2), new Point(50, 2), 196, Color.DarkSlateBlue);


            for (int i = InvTopIndex; i < GameLoop.World.Player.Inventory.Length && i < InvTopIndex + 14; i++) {
                int lineNum = i - InvTopIndex;
                Item item = GameLoop.World.Player.Inventory[i];
                string line = item.Name.Align(HorizontalAlignment.Center, 20) + "|" + item.ShortDesc.Align(HorizontalAlignment.Center, 21) + "|";
                string useWord = item.EquipSlot >= 0 ? "EQUIP" : item.BattleUse ? "USE" : "";
                if (useWord != "") {
                    InvConsole.Print(0, 3 + (lineNum * 2), line, Color.White);
                    InvConsole.PrintClickable(42, 3 + (lineNum * 2), useWord.Align(HorizontalAlignment.Center, 8), InvClicks, useWord.ToLower() + "," + i);
                } 
                else {
                    InvConsole.Print(0, 3 + (lineNum * 2), line, Color.DarkSlateGray);
                }
                InvConsole.DrawLine(new Point(0, 4 + (lineNum * 2)), new Point(50, 4 + (lineNum * 2)), '-', Color.DarkSlateBlue);
            }



            InvConsole.DrawLine(new Point(20, 3), new Point(20, 30), '|', Color.DarkSlateBlue);
            InvConsole.DrawLine(new Point(42, 3), new Point(42, 30), '|', Color.DarkSlateBlue);
        }


        public void ExecuteAllTurns() {
            PickEnemyMoves();

            List<TurnAction> presort = new();

            for (int i = 0; i < SubmittedTurns.Count; i++) {
                presort.Add(SubmittedTurns[i]);
            }

            List<TurnAction> sorted = presort.OrderBy(o => o.Speed).ThenBy(o => o.Owner.Name).ToList();


            for (int i = 0; i < sorted.Count; i++) {
                if (sorted[i].Action == "Attack") {
                    TurnResult turn = CommandManager.Attack(sorted[i].Owner, sorted[i].Target, sorted[i].UsingMove);

                    if (turn.Damage != null)
                        AddMsg(turn.Damage); 
                    if (turn.OwnStatus != null)
                        AddMsg(turn.OwnStatus);
                    if (turn.TargetStatus != null)
                        AddMsg(turn.TargetStatus);
                    AddMsg(new ColoredString("---------------------------------", Color.DarkSlateGray, Color.Black));
                }
            }

            PerformCleanup();

            SubmittedTurns.Clear();
            UsedItemThisTurn = false;
            TriedToFleeThisTurn = false;
        }

        public void PickEnemyMoves() {
            List<CombatParticipant> nonenemies = new();

            for (int j = 0; j < Current.AllParticipants.Count; j++) {
                if (!Current.AllParticipants[j].Enemy) {
                    nonenemies.Add(Current.AllParticipants[j]);
                }
            }

            for (int i = 0; i < Current.AllParticipants.Count; i++) {
                if (Current.AllParticipants[i].Owner == 0) {
                    TurnAction turn = new();
                    turn.Owner = Current.AllParticipants[i];
                    turn.Speed = Current.AllParticipants[i].StatWithStage("Speed");
                    turn.Action = "Attack";
                    turn.UsingMove = Current.AllParticipants[i].KnownMoves[GameLoop.rand.Next(Current.AllParticipants[i].KnownMoves.Count)];

                    turn.Target = nonenemies[GameLoop.rand.Next(nonenemies.Count)];

                    SubmittedTurns.Add(turn);
                }
            }
        }
       

        public void PerformCleanup() {
            if (Current != null) {
                int EnemiesLeft = 0;
                for (int i = Current.AllParticipants.Count - 1; i >= 0; i--) {
                    if (Current.AllParticipants[i].CurrentHP <= 0) {
                        AddMsg(new ColoredString(Current.AllParticipants[i].Name + " died.", Color.Red, Color.Black));

                        if (Current.AllParticipants[i].Owner == SteamClient.SteamId) {
                            CombatDone = true;
                            AddMsg(new ColoredString("You died! Press ESC to exit combat.", Color.Lime, Color.Black));
                        }

                        if (Current.AllParticipants[i].Owner == 0 && Current.AllParticipants[i].ContainedMon != null) { 
                            for (int j = 0; j < Current.AllParticipants[i].ContainedMon.DropTable.Count; j++) {
                                string[] split = Current.AllParticipants[i].ContainedMon.DropTable[j].Split(";");
                                int chance1 = Int32.Parse(split[0]) - 1;
                                int chanceMax = Int32.Parse(split[1]);
                                int quantity = Int32.Parse(split[2]);
                                if (GameLoop.World.itemLibrary.ContainsKey(split[3])) {
                                    Item item = Item.Copy(split[3], quantity);
                                    
                                    if (GameLoop.rand.Next(chanceMax) <= chance1) {
                                        Current.Drops.Add(item); 
                                    } 
                                }
                            } 
                        }


                        Current.AllParticipants.RemoveAt(i);
                        CurrentTarget = 0;
                    }
                }

                for (int i = 0; i < Current.AllParticipants.Count; i++) {
                    if (Current.AllParticipants[i].Enemy) {
                        EnemiesLeft++;
                    }
                }

                if (EnemiesLeft == 0) {
                    CombatDone = true;
                    AddMsg(new ColoredString("No enemies left - press ESC to exit combat!", Color.Lime, Color.Black)); 
                }
            }
        } 
    }
}
