using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input; 
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_CharGen : InstantUI {
        public string SelectedTextField = "";
        public int SkillTop = 0;

        public int[] AttributeCosts = { -4, -2, -1, 0, 1, 2, 3, 5, 7, 10, 13, 17 };

        public int strCost;
        public int dexCost;
        public int conCost;
        public int intCost;
        public int wisCost;
        public int chaCost;
        public int totalCost;

        public bool isHybrid = false;
        public bool lastChangedMainRace = false;

        public int SelectedPointBudget = 20;

        public UI_CharGen(int width, int height) : base(width, height, "CharGen") { 

        }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();


            Con.PrintStringField(1, 0, "Name: ", ref GameLoop.World.Player.Name, ref SelectedTextField, "Player_Name");
            Con.Print(1, 1, "Click above to type name, then hit enter to stop typing.", Color.DarkSlateGray);
            Con.Print(1, 2, "Select desired difficulty options and class skills to start game.", Color.DarkSlateGray);
            Con.Print(1, 3, "The " + 13.AsString() + " column lets you disable leveling for specific skills.", Color.DarkSlateGray);
            Con.Print(1, 4, "Use up/down arrows, numpad 8/2, or scroll wheel to scroll skill list.", Color.DarkSlateGray);
            Con.Print(1, 5, "Blue skills count as magic, red skills count as technology.", Color.DarkSlateGray);

            int y = 7;
            strCost = AttributeCosts[GameLoop.World.Player.Strength - 7]; 
            dexCost = AttributeCosts[GameLoop.World.Player.Dexterity - 7];
            conCost = AttributeCosts[GameLoop.World.Player.Constitution - 7];
            intCost = AttributeCosts[GameLoop.World.Player.Intelligence - 7];
            wisCost = AttributeCosts[GameLoop.World.Player.Wisdom - 7];
            chaCost = AttributeCosts[GameLoop.World.Player.Charisma - 7];
            totalCost = strCost + dexCost + conCost + intCost + wisCost + chaCost;


            Con.Print(1, y, "Attributes", Color.Cyan);
            Con.Print(12, y, "Limit ", Color.Gray);
            Con.PrintClickable(18, y, new ColoredString("10", SelectedPointBudget == 10 ? Color.Lime : Color.White, Color.Black), (id) => { SelectedPointBudget = 10; }, "10"); 
            Con.PrintClickable(21, y, new ColoredString("15", SelectedPointBudget == 15 ? Color.Lime : Color.White, Color.Black), (id) => { SelectedPointBudget = 15; }, "15");
            Con.PrintClickable(24, y, new ColoredString("20", SelectedPointBudget == 20 ? Color.Lime : Color.White, Color.Black), (id) => { SelectedPointBudget = 20; }, "20");
            Con.PrintClickable(27, y, new ColoredString("25", SelectedPointBudget == 25 ? Color.Lime : Color.White, Color.Black), (id) => { SelectedPointBudget = 25; }, "25");
            y++;
            Con.Print(1, y, "|     Strength (      )");
            Con.PrintAdjustableInt(17, y, 2, ref GameLoop.World.Player.Strength, 7, 18);
            Con.Print(25, y++, "[" + strCost.ToString().PadLeft(2) + "]");
            Con.Print(1, y, "|    Dexterity (      )"); 
            Con.PrintAdjustableInt(17, y, 2, ref GameLoop.World.Player.Dexterity, 7, 18);
            Con.Print(25, y++, "[" + dexCost.ToString().PadLeft(2) + "]");
            Con.Print(1, y, "| Constitution (      )"); 
            Con.PrintAdjustableInt(17, y, 2, ref GameLoop.World.Player.Constitution, 7, 18);
            Con.Print(25, y++, "[" + conCost.ToString().PadLeft(2) + "]");
            Con.Print(1, y, "| Intelligence (      )");
            Con.PrintAdjustableInt(17, y, 2, ref GameLoop.World.Player.Intelligence, 7, 18);
            Con.Print(25, y++, "[" + intCost.ToString().PadLeft(2) + "]");
            Con.Print(1, y, "|       Wisdom (      )");
            Con.PrintAdjustableInt(17, y, 2, ref GameLoop.World.Player.Wisdom, 7, 18);
            Con.Print(25, y++, "[" + wisCost.ToString().PadLeft(2) + "]");
            Con.Print(1, y, "|     Charisma (      )");
            Con.PrintAdjustableInt(17, y, 2, ref GameLoop.World.Player.Charisma, 7, 18);
            Con.Print(25, y++, "[" + chaCost.ToString().PadLeft(2) + "]");
            Con.Print(1, y, "|_____Points Spent______ ", Color.Gray);
            Con.Print(25, y++, totalCost.ToString().PadLeft(3), totalCost <= SelectedPointBudget ? Color.Lime : Color.Crimson);
            y++;

            Con.Print(1, y++, "Difficulty Options:", Color.Crimson);
            Con.PrintClickableBool(1, y++, "| Fast Travel Allowed ", ref GameLoop.World.Player.CanFastTravel);
            Con.PrintClickableBool(1, y++, "| Shops Allowed ", ref GameLoop.World.Player.CanUseShops);
            Con.PrintClickableBool(1, y++, "| Armor Allowed ", ref GameLoop.World.Player.CanUseArmor); 
            Con.PrintClickableBool(1, y++, "| Weapons Allowed ", ref GameLoop.World.Player.CanUseWeapons);
            Con.PrintClickableBool(1, y++, "| Magic Allowed ", ref GameLoop.World.Player.CanUseMagic);  
            Con.PrintClickableBool(1, y++, "| Blackouts are Deaths ", ref GameLoop.World.Player.BlackoutIsDeath);
            Con.PrintClickableBool(1, y++, "| Blackouts Enabled? ", ref GameLoop.World.Player.BlackoutsOn);
            Con.PrintScrollableInteger(1, y, "| Wake-up Time: ", ref GameLoop.World.Player.WakeupHour, false, 0, GameLoop.World.Player.BlackoutHour - 1, onlyPreface: true);
            Con.Print(18, y++, Helper.TimeString(GameLoop.World.Player.WakeupHour * 60));

            Con.PrintScrollableInteger(1, y, "| Blackout Time: ", ref GameLoop.World.Player.BlackoutHour, false, GameLoop.World.Player.WakeupHour + 1, 24, onlyPreface: true);
            Con.Print(18, y++, Helper.TimeString(GameLoop.World.Player.BlackoutHour * 60));
            y++;

            Con.Print(1, y++, "Enabled Needs:", Color.Green);
            Con.PrintClickableBool(1, y++, "| Hunger ", ref GameLoop.World.Player.HungerEnabled);
            Con.PrintClickableBool(1, y++, "| Thirst ", ref GameLoop.World.Player.ThirstEnabled);
            Con.PrintClickableBool(1, y++, "| Thermal ", ref GameLoop.World.Player.ThermalEnabled);
            Con.PrintClickableBool(1, y++, "| Bathroom ", ref GameLoop.World.Player.BathroomEnabled);
            Con.PrintClickableBool(1, y++, "| Hygiene ", ref GameLoop.World.Player.HygieneEnabled);
            Con.PrintClickableBool(1, y++, "| Entertainment ", ref GameLoop.World.Player.EntertainmentEnabled);
            Con.PrintClickableBool(1, y++, "| Social ", ref GameLoop.World.Player.SocialEnabled);
            Con.PrintClickableBool(1, y++, "| Environment ", ref GameLoop.World.Player.EnvironmentEnabled);
            Con.PrintClickableBool(1, y++, "| Sleep ", ref GameLoop.World.Player.SleepEnabled);

            int skillY = 7;
            Con.Print(33, skillY, "Skills: ");

            if (SkillTop < GameLoop.World.Player.Skills.Count - 15)
                Con.PrintClickable(42, skillY, 10.AsString(), ScrollSkills, "down"); 
            if (SkillTop > 0)
                Con.PrintClickable(44, skillY, 9.AsString(), ScrollSkills, "up");

            Con.Print(48, skillY, "C");
            Con.Print(50, skillY++, 13.AsString());

            int skillCount = 0;

            foreach (KeyValuePair<string, Skill> kv in GameLoop.World.skillLibrary) {
                Color dispColor = Color.White;

                if (kv.Value.IsMagic)
                    dispColor = Color.Turquoise;

                if (kv.Value.IsTech)
                    dispColor = Color.Crimson;

                if (skillCount % 2 == 0)
                    dispColor = dispColor.GetDark();


                if (skillCount >= SkillTop && skillCount < SkillTop + 15) {
                    Con.Print(33, skillY, "| " + kv.Key, dispColor);
                    Con.PrintClickable(48, skillY, Helper.Checkmark(GameLoop.World.Player.ClassSkills.Contains(kv.Key)), AddSkill, kv.Key);
                    Con.PrintClickable(50, skillY++, Helper.Checkmark(!GameLoop.World.Player.Skills[kv.Key].PermaOff), ToggleLeveling, kv.Key);
                    //Con.PrintClickable(35, skillY++, new ColoredString("| " + kv.Value.Name, GameLoop.World.Player.ClassSkills.Contains(kv.Key) ? Color.Lime : Color.White, Color.Black), AddSkill, kv.Key);
                }
                skillCount++;
            }

            skillY++;

            Con.Print(33, skillY++, "Selected Class Skills:");
            if (GameLoop.World.Player.ClassSkills.Count > 0) {
                GameLoop.World.Player.ClassSkills.Sort();

                for (int i = 0; i < GameLoop.World.Player.ClassSkills.Count; i++) {
                    string name = GameLoop.World.Player.ClassSkills[i];

                    if (GameLoop.World.skillLibrary.ContainsKey(name)) {
                        Skill kv = GameLoop.World.skillLibrary[name];

                        Color dispColor = Color.White;

                        if (kv.IsMagic)
                            dispColor = Color.Turquoise;

                        if (kv.IsTech)
                            dispColor = Color.Crimson;

                        if (skillCount % 2 == 0)
                            dispColor = dispColor.GetDark();

                        Con.Print(33, skillY++, "| " + GameLoop.World.Player.ClassSkills[i], dispColor);
                    }
                    else {
                        Con.Print(33, skillY++, "| " + GameLoop.World.Player.ClassSkills[i]);
                    }
                }
            }

            int raceY = 7;
            Con.Print(60, raceY, "Race: ");
            Con.PrintClickableBool(68, raceY++, "Hybrid ", ref isHybrid);

            if (!isHybrid)
                GameLoop.World.Player.SecondaryRace = "";

            Con.PrintClickable(60, raceY++, new ColoredString("| Human", (GameLoop.World.Player.Race == "Human" || GameLoop.World.Player.SecondaryRace == "Human") ? Color.Lime : Color.White, Color.Black), setRace, "Human");
            Con.PrintClickable(60, raceY++, new ColoredString("| Elf", (GameLoop.World.Player.Race == "Elf" || GameLoop.World.Player.SecondaryRace == "Elf") ? Color.Lime : Color.White, Color.Black), setRace, "Elf");
            Con.PrintClickable(60, raceY++, new ColoredString("| Dwarf", (GameLoop.World.Player.Race == "Dwarf" || GameLoop.World.Player.SecondaryRace == "Dwarf") ? Color.Lime : Color.White, Color.Black), setRace, "Dwarf");
            Con.PrintClickable(60, raceY++, new ColoredString("| Orc", (GameLoop.World.Player.Race == "Orc" || GameLoop.World.Player.SecondaryRace == "Orc") ? Color.Lime : Color.White, Color.Black), setRace, "Orc");
            Con.PrintClickable(60, raceY++, new ColoredString("| Leporid", (GameLoop.World.Player.Race == "Leporid" || GameLoop.World.Player.SecondaryRace == "Leporid") ? Color.Lime : Color.White, Color.Black), setRace, "Leporid");
            Con.PrintClickable(60, raceY++, new ColoredString("| Celestial", (GameLoop.World.Player.Race == "Celestial" || GameLoop.World.Player.SecondaryRace == "Celestial") ? Color.Lime : Color.White, Color.Black), setRace, "Celestial");
            Con.PrintClickable(60, raceY++, new ColoredString("| Infernal", (GameLoop.World.Player.Race == "Infernal" || GameLoop.World.Player.SecondaryRace == "Infernal") ? Color.Lime : Color.White, Color.Black), setRace, "Infernal");
            Con.PrintClickable(60, raceY++, new ColoredString("| Goblin", (GameLoop.World.Player.Race == "Goblin" || GameLoop.World.Player.SecondaryRace == "Goblin") ? Color.Lime : Color.White, Color.Black), setRace, "Goblin");

            string mainRace = GameLoop.World.Player.Race;
            string secondRace = GameLoop.World.Player.SecondaryRace;

            raceY++;
            raceY++;

            if (isHybrid) {
                if (mainRace == "Human" || secondRace == "Human") { Con.Print(60, raceY++, "All skills are class skills"); }
                if (mainRace == "Elf" || secondRace == "Elf") { Con.Print(60, raceY++, "+50% magic aptitude"); }
                if (mainRace == "Dwarf" || secondRace == "Dwarf") { Con.Print(60, raceY++, "+1 quality to crafted items"); }
                if (mainRace == "Orc" || secondRace == "Orc") { Con.Print(60, raceY++, "+50% tech aptitude"); }
                if (mainRace == "Leporid" || secondRace == "Leporid") { Con.Print(60, raceY++, "+50% Taming for taming insects"); }
                if (mainRace == "Celestial" || secondRace == "Celestial") { Con.Print(60, raceY++, "Always count as Good"); }
                if (mainRace == "Infernal" || secondRace == "Infernal") { Con.Print(60, raceY++, "Always count as Evil"); }
                if (mainRace == "Goblin" || secondRace == "Goblin") { Con.Print(60, raceY++, "+50% efficacy on afflicted statuses"); }
            } else {
                if (mainRace == "Human") {
                    Con.Print(60, raceY++, "All skills are class skills");
                    Con.Print(60, raceY++, "+50% experience gained for skills");
                } else if (mainRace == "Elf") {
                    Con.Print(60, raceY++, "Doubled magic aptitude"); 
                } else if (mainRace == "Dwarf") {
                    Con.Print(60, raceY++, "+2 quality to crafted items"); 
                } else if (mainRace == "Orc") {
                    Con.Print(60, raceY++, "Doubled tech aptitude"); 
                } else if (mainRace == "Leporid") {
                    Con.Print(60, raceY++, "Doubled Taming for taming insects"); 
                } else if (mainRace == "Celestial") {
                    Con.Print(60, raceY++, "Always count as Good");
                    Con.Print(60, raceY++, "(placeholder secondary)");
                } else if (mainRace == "Infernal") {
                    Con.Print(60, raceY++, "Always count as Evil");
                    Con.Print(60, raceY++, "(placeholder secondary)");
                } else if (mainRace == "Goblin") {
                    Con.Print(60, raceY++, "Doubled efficacy for afflicted statuses"); 
                }
            }

            Con.PrintClickable(1, 39, "[BACK]", UI_Clicks, "BackToMenu");

            if (GameLoop.World.Player.ClassSkills.Count < 9) {
                Con.Print(66, 39, "[MUST SELECT " + (9 - GameLoop.World.Player.ClassSkills.Count) + " MORE CLASS SKILLS]");
            } else {
                if (totalCost <= SelectedPointBudget) { 
                    Con.PrintClickable(93, 39, "[PLAY]", UI_Clicks, "StartGame");
                } else if (totalCost == 102) { 
                    Con.PrintClickable(93, 39, "[PLAY]", UI_Clicks, "FakeStart");
                } else {
                    Con.Print(66, 39, "[CANNOT GO OVER POINT BUY BUDGET]");
                }
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
            Rectangle skillArea = new Rectangle(33, 7, 18, 16);

            if (Helper.KeyPressed(Key.Up) || Helper.KeyPressed(Key.NumPad8) || (GameHost.Instance.Mouse.ScrollWheelValueChange < 0  && skillArea.Contains(mousePos))) {
                if (SkillTop > 0) {
                    SkillTop--;
                }
            }

            if (Helper.KeyPressed(Key.Down) || Helper.KeyPressed(Key.NumPad2) || (GameHost.Instance.Mouse.ScrollWheelValueChange > 0 && skillArea.Contains(mousePos))) {
                if (SkillTop < GameLoop.World.Player.Skills.Count - 15) {
                    SkillTop++;
                }
            }
        }

        public void setRace(string ID) {
            if (!isHybrid) {
                GameLoop.World.Player.Race = ID;
            } else {
                if (lastChangedMainRace) {
                    GameLoop.World.Player.SecondaryRace = ID;
                } else {
                    GameLoop.World.Player.Race = ID;
                }

                lastChangedMainRace.Flip();
            }
        }

        public override void UI_Clicks(string ID) {
            if (ID == "BackToMenu") {
                GameLoop.UIManager.ToggleUI("CharGen");
                GameLoop.UIManager.ToggleUI("MainMenu");
            } else if (ID == "StartGame") {
                GameLoop.World.FreshStart();
            } else if (ID == "FakeStart") {
                GameLoop.UIManager.ToggleUI("FakeStart");
            }
        }

        public void ScrollSkills(string ID) {
            if (ID == "up") {
                SkillTop = System.Math.Clamp(SkillTop - 5, 0, GameLoop.World.Player.Skills.Count - 15);
            } else if (ID == "down") {
                SkillTop = System.Math.Clamp(SkillTop + 5, 0, GameLoop.World.Player.Skills.Count - 15);
            }
        }

        public void AddSkill(string ID) {
            if (!GameLoop.World.Player.ClassSkills.Contains(ID)) {
                if (GameLoop.World.Player.ClassSkills.Count < 9) {
                    GameLoop.World.Player.ClassSkills.Add(ID);
                }
                else {
                    GameLoop.World.Player.ClassSkills.RemoveAt(0);
                    GameLoop.World.Player.ClassSkills.Add(ID);
                }
            } else {
                GameLoop.World.Player.ClassSkills.Remove(ID);
            }
        }

        public void ToggleLeveling(string ID) {
            if (GameLoop.World.Player.Skills.ContainsKey(ID)) {
                GameLoop.World.Player.Skills[ID].PermaOff.Flip();
            } 
        }
    }
}
