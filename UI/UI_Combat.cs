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

namespace LofiHollow.UI {
    public class UI_Combat {
        public SadConsole.Console CombatConsole;
        public Window CombatWindow;

        public Combat Current;
        public int CurrentTarget = 0;
        public int CurrentAllyTurn = 0;

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

            for (int i = 0; i < Current.EnemyMons.Count; i++) { 
                ApplyLevels(level, Current.EnemyMons[i]); 
            }

            if (GameLoop.NetworkManager != null)
                Current.AllyIDs.Add(GameLoop.NetworkManager.ownID); 

            for (int i = 0; i < GameLoop.World.Player.PartyMembers.Count; i++) {
                if (GameLoop.World.otherPlayers.ContainsKey(GameLoop.World.Player.PartyMembers[i])) {
                    Player play = GameLoop.World.otherPlayers[GameLoop.World.Player.PartyMembers[i]];
                    if (play.MapPos == GameLoop.World.Player.MapPos) {
                        Current.AllyIDs.Add(GameLoop.World.Player.PartyMembers[i]);
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
                Current.EnemyMons.Add(possible[i]);
                Current.EnemyMons.Add(possible[i]);
                Current.EnemyMons.Add(possible[i]);
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
                for (int i = 0; i < Current.AllyIDs.Count; i++) {
                    if (GameLoop.World.otherPlayers.ContainsKey(Current.AllyIDs[i])) {
                        Player play = GameLoop.World.otherPlayers[Current.AllyIDs[i]];
                        CombatConsole.Print(1, 1 + i, play.Name);

                        CombatConsole.Print(15 + (i * 5), 27, play.GetAppearance());
                    } else if (GameLoop.NetworkManager == null || (GameLoop.NetworkManager != null && GameLoop.NetworkManager.ownID == Current.AllyIDs[i])) {
                        Player play = GameLoop.World.Player;
                        CombatConsole.Print(1, 1 + i, play.Name);

                        CombatConsole.Print(5 + (i * 5), 27, play.GetAppearance());
                    } 
                }

                for (int i = 0; i < Current.EnemyMons.Count; i++) { 
                    CombatConsole.Print(39, 1 + i, Current.EnemyMons[i].Name.Align(HorizontalAlignment.Right, 30), CurrentTarget == i ? Color.Yellow : Color.White);
                    CombatConsole.Print(65 - (5 * i), 27, Current.EnemyMons[i].GetAppearance()); 

                    if (CurrentTarget == i) {
                        CombatConsole.Print(65 - (5 * i), 23, "|");
                        CombatConsole.Print(65 - (5 * i), 24, "|");
                        CombatConsole.Print(65 - (5 * i), 25, "V");
                    }
                }

                CombatConsole.DrawLine(new Point(0, 28), new Point(69, 28), 196, Color.Green); 
                CombatConsole.DrawLine(new Point(0, 29), new Point(69, 29), 196, Color.White);

                if (GameLoop.NetworkManager == null || (GameLoop.NetworkManager != null && Current.AllyIDs[CurrentAllyTurn] == GameLoop.NetworkManager.ownID)) {
                    CombatConsole.Print(1, 30, Helper.HoverColoredString("Attack".Align(HorizontalAlignment.Center, 15), mousePos.Y == 30 && mousePos.X < 20));

                    CombatConsole.Print(1, 31, Helper.HoverColoredString("Change Stance".Align(HorizontalAlignment.Center, 15), mousePos.Y == 31 && mousePos.X < 20));
                    CombatConsole.Print(1, 32, GameLoop.World.Player.CombatMode + " (" + GameLoop.World.Player.GetDamageType() + ")");
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
                    if (CurrentTarget + 1 < Current.EnemyMons.Count)
                        CurrentTarget++;
                    else
                        CurrentTarget = 0;
                } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                    if (CurrentTarget > 0)
                        CurrentTarget--;
                    else
                        CurrentTarget = Current.EnemyMons.Count - 1;
                }

                if (GameHost.Instance.Mouse.LeftClicked) {
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
