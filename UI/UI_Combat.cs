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

namespace LofiHollow.UI {
    public class UI_Combat {
        public SadConsole.Console CombatConsole;
        public Window CombatWindow;

        public Combat Current;
        public int CurrentTarget = 0;
        public int CurrentAllyTurn = 0;

        public Stack<ColoredString> RecentCombatMsgs = new(5);

        public List<Item> Drops = new();

        public UI_Combat(int width, int height, string title) {
            CombatWindow = new(width, height);
            CombatWindow.CanDrag = false;
            CombatWindow.Position = new(11, 6);

            int invConWidth = width - 2;
            int invConHeight = height - 2;

            CombatConsole = new(invConWidth, invConHeight);
            CombatConsole.Position = new(1, 1);
            CombatWindow.Title = title.Align(HorizontalAlignment.Center, invConWidth, (char)196);


            CombatWindow.Children.Add(CombatConsole);
            GameLoop.UIManager.Children.Add(CombatWindow);

            CombatWindow.Show();
            CombatWindow.IsVisible = false;
        }

        public void StartCombat(string loc, int level) {
            Current = new();

            SpawnEnemies(loc, level);

            for (int i = 0; i < Current.Enemies.Count; i++) {
                if (Current.Enemies[i].GetActor() != null && Current.Enemies[i].GetActor() is MonsterWrapper mon) {
                    ApplyLevels(level, mon.monster);
                    mon.UpdateHP();
                }
            }

            if (GameLoop.NetworkManager != null)
                Current.Allies.Add(new CombatParticipant(GameLoop.NetworkManager.ownID)); 

            for (int i = 0; i < GameLoop.World.Player.PartyMembers.Count; i++) {
                if (GameLoop.World.otherPlayers.ContainsKey(GameLoop.World.Player.PartyMembers[i])) {
                    Player play = GameLoop.World.otherPlayers[GameLoop.World.Player.PartyMembers[i]];
                    if (play.MapPos == GameLoop.World.Player.MapPos) {
                        Current.Allies.Add(new CombatParticipant(GameLoop.World.Player.PartyMembers[i]));
                    }
                }
            }

            for (int i = 0; i < GameLoop.World.Player.PartyMembers.Count; i++) {
                if (GameLoop.World.otherPlayers.ContainsKey(GameLoop.World.Player.PartyMembers[i])) {
                    Player play = GameLoop.World.otherPlayers[GameLoop.World.Player.PartyMembers[i]];
                    if (play.MapPos == GameLoop.World.Player.MapPos) {
                        NetMsg initiateCombat = new("startCombat", Current.ToByteArray()); 
                        
                        GameLoop.SendMessageIfNeeded(initiateCombat, false, true, (ulong) GameLoop.World.Player.PartyMembers[i]);
                    }
                }
            }
        }

        public void SpawnEnemies(string loc, int level) {
            List<Monster> possible = new();

            foreach (KeyValuePair<string, Monster> kv in GameLoop.World.monsterLibrary) {
                if (kv.Value.SpawnLocation.Contains(loc)) {
                    if (kv.Value.MinLevel <= level && kv.Value.MaxLevel >= level) {
                        possible.Add(kv.Value);
                    }
                }
            }

            for (int i = 0; i < possible.Count; i++) {
                MonsterWrapper one = new MonsterWrapper(possible[i]);
                one.Level = level;
                Current.Enemies.Add(new CombatParticipant(one));
            }
        }

        public void ApplyLevels(int level, Monster mon) {
            int[] Stats;

            try {
                Stats = mon.StatGrowths.Split(",").Select(item => int.Parse(item)).ToArray(); 
                for (int i = 0; i < level; i++) {
                    if (Stats[0] > 1)
                        mon.Health += GameLoop.rand.Next(Stats[0]);
                    if (Stats[1] > 1)
                        mon.Speed += GameLoop.rand.Next(Stats[1]);
                    if (Stats[2] > 1)
                        mon.Attack += GameLoop.rand.Next(Stats[2]);
                    if (Stats[3] > 1)
                        mon.Defense += GameLoop.rand.Next(Stats[3]);
                    if (Stats[4] > 1)
                        mon.MAttack += GameLoop.rand.Next(Stats[4]);
                    if (Stats[5] > 1)
                        mon.MDefense += GameLoop.rand.Next(Stats[5]);
                } 
            } catch (Exception e) {
                GameLoop.UIManager.AddMsg("Error while applying levels to monster.");
            }
        }

        public void RenderCombat() {
            Point mousePos = new MouseScreenObjectState(CombatConsole, GameHost.Instance.Mouse).CellPosition; 
            CombatConsole.Clear(); 

            if (Current != null) {
                for (int i = 0; i < Current.Allies.Count; i++) {
                    if (Current.Allies[i].GetActor() != null && Current.Allies[i].GetActor() is Player play) {
                        CombatConsole.Print(1, 1 + i, play.Name);

                        CombatConsole.Print(15 + (i * 5), 27, play.GetAppearance());
                    }
                }

                for (int i = 0; i < Current.Enemies.Count; i++) {
                    if (Current.Enemies[i].GetActor() != null && Current.Enemies[i].GetActor() is MonsterWrapper mon) {
                        CombatConsole.Print(29, 1 + i, mon.monster.Name.Align(HorizontalAlignment.Right, 30), CurrentTarget == i ? Color.Yellow : Color.White);

                        int percent = (int) Math.Ceiling((double) ((double) mon.CurrentHP / (double) mon.MaxHP) * 10);
                        CombatConsole.DrawLine(new Point(60, 1 + i), new Point(60 + percent, 1+i), 254, Color.Lime);
                        CombatConsole.Print(65 - (5 * i), 27, mon.monster.GetAppearance());

                        if (CurrentTarget == i) {
                            CombatConsole.Print(65 - (5 * i), 23, "|");
                            CombatConsole.Print(65 - (5 * i), 24, "|");
                            CombatConsole.Print(65 - (5 * i), 25, "V");
                        }
                    }
                }

                CombatConsole.DrawLine(new Point(0, 28), new Point(69, 28), 196, Color.Green); 
                CombatConsole.DrawLine(new Point(0, 29), new Point(69, 29), 196, Color.White);

                if (GameLoop.NetworkManager == null || (GameLoop.NetworkManager != null && Current.Allies[CurrentAllyTurn].playerID == GameLoop.NetworkManager.ownID)) {
                    CombatConsole.PrintClickable(1, 30, "Attack".Align(HorizontalAlignment.Center, 20), CombatClick, "attack");
                    CombatConsole.PrintClickable(1, 32, "Change Stance".Align(HorizontalAlignment.Center, 20), CombatClick, "stance");
                    CombatConsole.Print(1, 33, (GameLoop.World.Player.CombatMode + " (" + GameLoop.World.Player.GetDamageType() + ")").Align(HorizontalAlignment.Center, 20));
                    CombatConsole.PrintClickable(1, 35, "Summon".Align(HorizontalAlignment.Center, 20), CombatClick, "summon");

                    if (GameLoop.World.Player.HighestToolTier("SoulCamera") > 0)
                        CombatConsole.PrintClickable(1, 37, "Soul Capture".Align(HorizontalAlignment.Center, 20), CombatClick, "soulphoto");
                }

                CombatConsole.DrawLine(new Point(22, 30), new Point(22, 39), 179, Color.White);

                for (int i = 0; i < RecentCombatMsgs.Count; i++) {
                    CombatConsole.Print(23, 30 + i, RecentCombatMsgs.ElementAt(i));
                }
            }
        }

        public void CombatInput() {
            Point mousePos = new MouseScreenObjectState(CombatConsole, GameHost.Instance.Mouse).CellPosition;
            if (GameHost.Instance.Keyboard.IsKeyReleased(Key.Escape)) {
                Toggle();
            }

            if (Current != null) {
                if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                    if (CurrentTarget + 1 < Current.Enemies.Count)
                        CurrentTarget++;
                    else
                        CurrentTarget = 0;
                } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                    if (CurrentTarget > 0)
                        CurrentTarget--;
                    else
                        CurrentTarget = Current.Enemies.Count - 1;
                }
            }
        } 

        public void CombatClick(string id) {
            if (id == "stance") {
                GameLoop.World.Player.SwitchMeleeMode();
            }

            if (id == "attack") {
                if (Current.Allies[CurrentAllyTurn].playerID == GameLoop.NetworkManager.ownID && Current.Enemies[CurrentTarget].GetActor() != null) {
                    if (Current.Enemies[CurrentTarget].GetActor() is MonsterWrapper mon) {
                        ColoredString result = CommandManager.Attack(GameLoop.World.Player, mon);
                        RecentCombatMsgs.Push(result);
                        PerformCleanup();
                    }
                }
            } 

            if (id == "soulphoto") {
                if (Current != null) {
                    if (Current.Enemies[CurrentTarget].GetActor() != null && Current.Enemies[CurrentTarget].GetActor() is MonsterWrapper mon) {
                        int camTier = GameLoop.World.Player.HighestToolTier("SoulCamera"); 
                        AttemptCapture(Current.Enemies, camTier);
                        PerformCleanup();
                    }
                }
            }
        }

        public void AttemptCapture(List<CombatParticipant> enemies, int cameraTier) { 
            ColoredString captureString;
            if (enemies.Count == 1) {
                MonsterWrapper wrap = enemies[0].GetActor() as MonsterWrapper;
                int captureRate = wrap.monster.CaptureRate;
                int maxHp = wrap.MaxHP;
                int currHP = wrap.CurrentHP;

                int modifiedCap = Math.Max(captureRate - cameraTier, 1);

                double chance = Math.Max((((3f * maxHp) - (2f * currHP)) * modifiedCap) / (3 * maxHp), 1);

                int roll = GameLoop.rand.Next(255) + 1; 

                if (roll <= chance) {
                    captureString = new ColoredString("You captured the " + wrap.monster.Name + "!");
                    // Turn them into an NFT and add them to the players inventory
                    Monster clone = new();
                    clone.SetAll(wrap.monster);
                    Item nft = new("lh:Soul");
                    nft.SoulPhoto = new(clone, wrap.CombatLevel);
                    CommandManager.AddItemToInv(GameLoop.World.Player, nft);
                    wrap.CurrentHP = 0;
                }
                else {
                    captureString = new ColoredString("You failed to capture the " + wrap.monster.Name + "!");
                }
            } else {
                MonsterWrapper wrap1 = enemies[0].GetActor() as MonsterWrapper;
                MonsterWrapper wrap2 = enemies[1].GetActor() as MonsterWrapper;
                int captureRate = (wrap1.monster.CaptureRate + wrap2.monster.CaptureRate) / 2;
                int maxHp = (wrap1.MaxHP + wrap2.MaxHP) / 2;
                int currHP = (wrap1.CurrentHP + wrap2.CurrentHP) / 2;

                int modifiedCap = Math.Max(captureRate - cameraTier, 1);

                double chance = Math.Max((((3f * maxHp) - (2f * currHP)) * modifiedCap) / (3 * maxHp), 1);

                int roll = GameLoop.rand.Next(255) + 1; 

                if (roll <= chance) {
                    captureString = new ColoredString("Something has gone horribly wrong...");
                    // Turn them into an NFT and add them to the players inventory
                    Monster clone = new();
                    clone.SetAll(wrap1.monster);
                    Monster secondClone = new();
                    secondClone.SetAll(wrap2.monster);
                    Item nft = new("lh:Soul");
                    nft.SoulPhoto = new(clone, wrap1.CombatLevel);
                    nft.SoulPhoto.Contained2 = secondClone;
                    CommandManager.AddItemToInv(GameLoop.World.Player, nft);
                    wrap1.CurrentHP = 0;
                    wrap2.CurrentHP = 0;
                }
                else {
                    captureString = new ColoredString("You failed to capture the monsters!");
                }
            }

            RecentCombatMsgs.Push(captureString);
        }

        public void PerformCleanup() {
            if (Current != null) {
                for (int i = Current.Enemies.Count - 1; i >= 0; i--) {
                    if (Current.Enemies[i].GetActor() != null) {
                        if (Current.Enemies[i].GetActor().CurrentHP <= 0) {
                            if (Current.Enemies[i].GetActor() is MonsterWrapper mon) {
                                mon.SpawnDrops(Drops);
                            }
                            Current.Enemies.RemoveAt(i);
                            CurrentTarget = 0;
                        }
                    }
                }

                if (Current.Enemies.Count == 0) {
                    Current = null;
                    Toggle();
                }
            }
        }

        public void Toggle() {
            if (CombatWindow.IsVisible) {
                GameLoop.UIManager.selectedMenu = "None";
                CombatWindow.IsVisible = false;
                GameLoop.UIManager.Map.MapConsole.IsFocused = true;
            } else {
                GameLoop.UIManager.selectedMenu = "Combat"; 
                CombatWindow.IsVisible = true;
                CombatWindow.IsFocused = true;
            }
        }
    }
}
