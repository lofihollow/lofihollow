using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.EntityData;
using LofiHollow.Managers; 
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;

namespace LofiHollow.UI {
    public class UI_Sidebar : InstantUI {   
        public int hotbarSelect = 0; 
        public bool MadeCoinThisFrame = false; 

        public Point tileSelected = new(0, 0);
          
        public UI_Sidebar(int width, int height) : base(width, height, "Sidebar") { 
            Win.CanDrag = false;
            Win.Position = new Point(0, 0);
        }
         

        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            Con.Clear(); 
            string[] months = { "Spring", "Summer", "Autumn", "Winter" };
              

            int needsY = 0;
            if (GameLoop.World.Player.HungerEnabled) {
                Con.Print(0, needsY++, "       Hunger:  " + GameLoop.World.Player.HungerBar.ToString().PadLeft(2, '0') + "%");
            } 

            if (GameLoop.World.Player.ThirstEnabled) {
                Con.Print(0, needsY++, "       Thirst:  " + GameLoop.World.Player.ThirstBar.ToString().PadLeft(2, '0') + "%");
            }

            if (GameLoop.World.Player.ThermalEnabled) {
                Con.Print(0, needsY++, "      Thermal:  " + GameLoop.World.Player.ThermalBar.ToString().PadLeft(2, '0') + "%");
            }

            if (GameLoop.World.Player.BathroomEnabled) {
                Con.Print(0, needsY++, "     Bathroom:  " + GameLoop.World.Player.BathroomBar.ToString().PadLeft(2, '0') + "%");
            }
             
            if (GameLoop.World.Player.HygieneEnabled) {
                Con.Print(0, needsY++, "      Hygiene:  " + GameLoop.World.Player.HygieneBar.ToString().PadLeft(2, '0') + "%");
            } 

            if (GameLoop.World.Player.EntertainmentEnabled) {
                Con.Print(0, needsY++, "Entertainment:  " + GameLoop.World.Player.EntertainmentBar.ToString().PadLeft(2, '0') + "%");
            }

            if (GameLoop.World.Player.SocialEnabled) {
                Con.Print(0, needsY++, "       Social:  " + GameLoop.World.Player.SocialBar.ToString().PadLeft(2, '0') + "%");
            }

            if (GameLoop.World.Player.EnvironmentEnabled) {
                Con.Print(0, needsY++, "  Environment:  " + GameLoop.World.Player.EnvironmentBar.ToString().PadLeft(2, '0') + "%");
            }

            if (GameLoop.World.Player.SleepEnabled) {
                Con.Print(0, needsY++, "        Sleep:  " + GameLoop.World.Player.SleepBar.ToString().PadLeft(2, '0') + "%");
            }

            Con.DrawLine(new Point(0, 9), new Point(40, 9), (char)196, Color.White, Color.Black);


            Con.DrawLine(new Point(20, 0), new Point(20, 8), (char)179, Color.White, Color.Black);

            if (GameLoop.World != null && GameLoop.World.DoneInitializing) {  

                Con.Print(29, 4, new ColoredString(3.AsString(), Color.Red, Color.Black));
                Con.Print(30, 4, new ColoredString((GameLoop.World.Player.CurrentHP + "/" + GameLoop.World.Player.MaxHP).Align(HorizontalAlignment.Right, 7), Color.Red, Color.Black));

                Con.Print(29, 5, new ColoredString(175.AsString(), Color.Lime, Color.Black));
                Con.Print(30, 5, new ColoredString((GameLoop.World.Player.CurrentStamina + "/" + GameLoop.World.Player.MaxStamina).Align(HorizontalAlignment.Right, 7), Color.Lime, Color.Black));

                       

                GameLoop.World.Player.CalculateAptitude();
                 

                if (GameLoop.World.Player.Clock != null) {
                    Con.Print(29, 0, Helper.TimeString(GameLoop.World.Player.Clock.GetCurrentTime())); 
                    Con.Print(27, 1, (months[GameLoop.World.Player.Clock.Month - 1] + " " + GameLoop.World.Player.Clock.Day).Align(HorizontalAlignment.Right, 10));
                    Con.Print(27, 2, ("Year " + GameLoop.World.Player.Clock.Year).Align(HorizontalAlignment.Right, 10));
                }


                Con.DrawLine(new Point(0, 13), new Point(40, 13), (char)196, Color.White, Color.Black);
                int y = 14;
                Con.Print(0, y, "Worn Equipment");
                y++;


                bool noLeft = GameLoop.World.Player.EquippedLeftHand == null;
                Con.Print(0, y, "| L. Hand: ");
                Con.Print(11, y++, noLeft ? "(EMPTY)" : GameLoop.World.Player.EquippedLeftHand.Name, noLeft ? Color.DarkSlateGray : Color.White);

                bool noRight = GameLoop.World.Player.EquippedRightHand == null;
                Con.Print(0, y, "| R. Hand: ");
                Con.Print(11, y++, noRight ? "(EMPTY)" : GameLoop.World.Player.EquippedRightHand.Name, noRight ? Color.DarkSlateGray : Color.White);

                bool noHead = GameLoop.World.Player.EquippedHelmet == null;
                Con.Print(0, y, "|    Head: ");
                Con.Print(11, y++, noHead ? "(EMPTY)" : GameLoop.World.Player.EquippedHelmet.Name, noHead ? Color.DarkSlateGray : Color.White);

                bool noTorso = GameLoop.World.Player.EquippedTorso == null;
                Con.Print(0, y, "|   Torso: ");
                Con.Print(11, y++, noTorso ? "(EMPTY)" : GameLoop.World.Player.EquippedTorso.Name, noTorso ? Color.DarkSlateGray : Color.White);

                bool noLegs = GameLoop.World.Player.EquippedLegs == null;
                Con.Print(0, y, "|    Legs: ");
                Con.Print(11, y++, noLegs ? "(EMPTY)" : GameLoop.World.Player.EquippedLegs.Name, noLegs ? Color.DarkSlateGray : Color.White);

                bool noHands = GameLoop.World.Player.EquippedGloves == null;
                Con.Print(0, y, "|   Hands: ");
                Con.Print(11, y++, noHands ? "(EMPTY)" : GameLoop.World.Player.EquippedGloves.Name, noHands ? Color.DarkSlateGray : Color.White);

                bool noFeet = GameLoop.World.Player.EquippedBoots == null;
                Con.Print(0, y, "|    Feet: ");
                Con.Print(11, y++, noFeet ? "(EMPTY)" : GameLoop.World.Player.EquippedBoots.Name, noFeet ? Color.DarkSlateGray : Color.White);

                bool noRing = GameLoop.World.Player.EquippedRing == null;
                Con.Print(0, y, "|    Ring: ");
                Con.Print(11, y++, noRing ? "(EMPTY)" : GameLoop.World.Player.EquippedRing.Name, noRing ? Color.DarkSlateGray : Color.White);

                bool noAmulet = GameLoop.World.Player.EquippedAmulet == null;
                Con.Print(0, y, "|  Amulet: ");
                Con.Print(11, y++, noAmulet ? "(EMPTY)" : GameLoop.World.Player.EquippedAmulet.Name, noAmulet ? Color.DarkSlateGray : Color.White);

                bool noBack = GameLoop.World.Player.EquippedBack == null;
                Con.Print(0, y, "|    Back: ");
                Con.Print(11, y++, noBack ? "(EMPTY)" : GameLoop.World.Player.EquippedBack.Name, noBack ? Color.DarkSlateGray : Color.White);

                y++;
                Con.Print(0, y++, "Backpack"); 
                for (int i = 0; i < 9 && i < GameLoop.World.Player.Inventory.Count; i++) {
                    Item item = GameLoop.World.Player.Inventory[i];

                    Con.Print(0, y, "|");

                    ColoredString LetterGrade = new("");
                    if (item.Quality > 0)
                        LetterGrade = new ColoredString(" [") + item.LetterGrade() + new ColoredString("]");


                    if (!item.IsStackable || (item.IsStackable && item.Quantity == 1)) {
                        Con.Print(3, y, new ColoredString(item.Name, i == hotbarSelect ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.TransparentBlack) + LetterGrade);
                    }
                    else
                        Con.Print(3, y, new ColoredString(("(" + item.Quantity + ") " + item.Name), i == hotbarSelect ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.TransparentBlack) + LetterGrade);


                    y++;
                }

            }
        }

        public void SidebarClick(string ID) {
            if (ID.Contains("Unequip")) {
                int slot = Int32.Parse(ID.Split(",")[1]);
                // TODO: Sidebar unequip
            } 
        }
    }
}
