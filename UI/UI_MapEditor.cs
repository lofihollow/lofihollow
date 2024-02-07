using LofiHollow.DataTypes;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives; 
using System;
using System.Collections.Generic;
using System.IO;
using Console = SadConsole.Console;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_MapEditor : InstantUI {
        int selectedIndex = 0;
        int subIndex = 0;
        int selectedField = 0;
        string selectedType = "none";
        public string TypingField = "";


        public UI_MapEditor(int wid, int height) : base(wid, height, "MapEditor") { 
            Win.CanDrag = true;  
        }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();


            Location? loc = Helper.ResolveLoc(GameLoop.World.Player.NavLoc);

            if (loc != null) {  
                if (selectedType != "none") {
                    Con.PrintClickable(0, 17, "[BACK]", (id) => { selectedType = "none"; }, "");
                }
                else {
                    Con.Print(0, 0, "Map Editor Menu [" + loc.ID + "]");
                    Con.PrintStringField(0, 1, "| Edit Location Name", ref loc.DisplayName, ref TypingField, "locName", true);
                    Con.PrintStringField(0, 2, "| Edit Location Description", ref loc.Description, ref TypingField, "locDesc", true);
                    Con.PrintClickable(0, 3, "| Connected Locations", () => { selectedType = "connectedLoc"; });
                    Con.PrintClickable(0, 4, "| Items at Node", () => { selectedType = "item"; });
                    Con.PrintClickable(0, 5, "| Description Blocks", () => { selectedType = "descBlock"; });
                    Con.PrintClickable(0, 6, "| Objects at Node", () => { selectedType = "object"; });
                    Con.PrintClickableBool(0, 7, "| Map has Light ", ref loc.IsLit);
                    Con.PrintClickableBool(0, 8, "| Map is in dev lottery ", ref loc.InDevLottery);

                    Con.PrintClickable(72, 17, "[SAVE]", MapClick, "save");
                }

                if (selectedType == "connectedLoc") {
                    string countStr = loc.ConnectedLocations.Count > 1 ? ((selectedIndex + 1) + "/" + loc.ConnectedLocations.Count) : loc.ConnectedLocations.Count.ToString();

                    Con.Print(0, 0, "Connected Location (" + countStr + ")");
                    Con.PrintClickable(0, 1, new ColoredString("| Add New", Color.Lime, Color.Black), MapClick, "newLoc");

                    if (loc.ConnectedLocations.Count > 0) {
                        selectedIndex = Math.Clamp(selectedIndex, 0, loc.ConnectedLocations.Count - 1);

                        ConnectionNode node = loc.ConnectedLocations[selectedIndex];

                        Con.PrintStringField(0, 3, "| Edit Direction", ref node.Direction, ref TypingField, "nodeDir" + selectedIndex, true);
                        Con.PrintStringField(0, 4, "| Edit Destination ID", ref node.LocationID, ref TypingField, "nodeID" + selectedIndex, true);
                        Con.PrintClickable(0, 5, "| Select Skill Requirement", MapClick, "editSkillReq");

                        Con.PrintStringField(35, 3, "| Edit Requirements Hint", ref node.ReqSummary, ref TypingField, "nodeReq" + selectedIndex, true);
                        Con.PrintStringField(35, 4, "| Edit Costs Hint", ref node.CostSummary, ref TypingField, "nodeCost" + selectedIndex, true);

                        Con.Print(10, 2, "(" + loc.ConnectedLocations[selectedIndex].Direction + ":" + loc.ConnectedLocations[selectedIndex].LocationID + ")");

                        string condCount = node.ActiveConditions.Count > 1 ? ((subIndex + 1) + "/" + node.ActiveConditions.Count) : node.ActiveConditions.Count.ToString();
                        Con.PrintClickableBool(0, 6, "| Visible While Inactive ", ref node.VisibleWhileInactive);
                        Con.Print(0, 7, "| Active Conditions (" + condCount + ")", Color.Orange);
                        Con.PrintClickableBool(0, 8, "--| Requires All Conditions ", ref node.AllConditions);
                        Con.PrintClickable(0, 9, new ColoredString("--| Add Condition", Color.Lime, Color.Black), () => { node.ActiveConditions.Add(new()); subIndex = node.ActiveConditions.Count - 1; });

                        if (node.ActiveConditions.Count > 0) {
                            subIndex = Math.Clamp(subIndex, 0, node.ActiveConditions.Count - 1);
                            DataCondition cond = node.ActiveConditions[subIndex];

                            Con.PrintStringField(0, 11, "--| Data Checked: ", ref cond.Target, ref TypingField, "dataCondCheck" + subIndex);
                            Con.Print(0, 12, "--| Check: ");
                            Con.PrintClickable(10, 12, new ColoredString("has", cond.Check == "has" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "has"; });
                            Con.PrintClickable(14, 12, new ColoredString("!has", cond.Check == "!has" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "!has"; });
                            Con.PrintClickable(19, 12, new ColoredString("eq", cond.Check == "equals" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "equals"; });
                            Con.PrintClickable(22, 12, new ColoredString("not", cond.Check == "not" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "not"; });
                            Con.PrintClickable(26, 12, new ColoredString("gt", cond.Check == "above" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "above"; });
                            Con.PrintClickable(29, 12, new ColoredString("lt", cond.Check == "below" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "below"; });
                            Con.PrintClickable(32, 12, new ColoredString("bool", cond.Check == "bool" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "bool"; });
                            Con.PrintClickable(37, 12, new ColoredString("e<>", cond.Check == "e<>" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "e<>"; });
                            Con.PrintClickable(41, 12, new ColoredString("e<!>", cond.Check == "e<!>" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "e<!>"; });
                            Con.PrintClickable(46, 12, new ColoredString("i<>", cond.Check == "i<>" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "i<>"; });
                            Con.PrintClickable(50, 12, new ColoredString("i<!>", cond.Check == "i<!>" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "i<!>"; });

                            Con.Print(0, 13, "--| Target Number: ");
                            Con.PrintAdjustableInt(19, 13, 8, ref cond.CompareNum, int.MinValue, int.MaxValue);
                            Con.Print(0, 14, "--|  Range Number: ");
                            Con.PrintAdjustableInt(19, 14, 8, ref cond.Secondary, cond.CompareNum, int.MaxValue);




                            Con.Print(54, 16, "Condition: ");
                            Con.PrintClickable(72, 16, "[NEXT]", () => { subIndex++; });
                            Con.PrintClickable(65, 16, "[PREV]", () => { subIndex--; });


                            Con.PrintClickable(0, 10, new ColoredString("--| Delete Condition", Color.Red, Color.Black), () => { node.ActiveConditions.RemoveAt(subIndex); });
                        }

                        Con.PrintClickable(72, 17, "[NEXT]", () => { selectedIndex++; });
                        Con.PrintClickable(65, 17, "[PREV]", () => { selectedIndex--; });

                        Con.PrintClickable(0, 2, new ColoredString("| Delete", Color.Red, Color.Black), () => { loc.ConnectedLocations.RemoveAt(selectedIndex); });
                    }
                }
                else if (selectedType == "descBlock") {
                    string countStr = loc.DescriptionBlocks.Count > 1 ? ((selectedIndex + 1) + "/" + loc.DescriptionBlocks.Count) : loc.DescriptionBlocks.Count.ToString();

                    Con.Print(0, 0, "Description Blocks (" + countStr + ")");
                    Con.PrintClickable(0, 1, new ColoredString("| Add New", Color.Lime, Color.Black), () => { loc.DescriptionBlocks.Add(new()); });

                    if (loc.DescriptionBlocks.Count > 0) {
                        selectedIndex = Math.Clamp(selectedIndex, 0, loc.DescriptionBlocks.Count - 1);

                        DescriptionBlock block = loc.DescriptionBlocks[selectedIndex];

                        Con.PrintStringField(0, 3, "| Edit Description", ref block.Description, ref TypingField, "blockDesc" + selectedIndex, true);

                        if (block.ReplacesMainBlock) {
                            GameLoop.UIManager.Nav.devOverride = block.Description;
                        }
                        else {
                            GameLoop.UIManager.Nav.devOverride = "";
                        }


                        string condCount = block.ActiveConditions.Count > 1 ? ((subIndex + 1) + "/" + block.ActiveConditions.Count) : block.ActiveConditions.Count.ToString();
                        Con.PrintClickableBool(0, 4, "| Replaces Main Description ", ref block.ReplacesMainBlock);
                        Con.Print(0, 6, "| Active Conditions (" + condCount + ")", Color.Orange);
                        Con.PrintClickableBool(0, 7, "--| Requires All Conditions ", ref block.AllConditions);
                        Con.PrintClickable(0, 8, new ColoredString("--| Add Condition", Color.Lime, Color.Black), () => { block.ActiveConditions.Add(new()); subIndex = block.ActiveConditions.Count - 1; });

                        if (block.ActiveConditions.Count > 0) {
                            subIndex = Math.Clamp(subIndex, 0, block.ActiveConditions.Count - 1);
                            DataCondition cond = block.ActiveConditions[subIndex];

                            Con.PrintStringField(0, 10, "--| Data Checked: ", ref cond.Target, ref TypingField, "dataCondCheck" + subIndex);
                            Con.Print(0, 11, "--| Check: ");
                            Con.PrintClickable(10, 11, new ColoredString("has", cond.Check == "has" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "has"; });
                            Con.PrintClickable(14, 11, new ColoredString("!has", cond.Check == "!has" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "!has"; });
                            Con.PrintClickable(19, 11, new ColoredString("eq", cond.Check == "equals" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "equals"; });
                            Con.PrintClickable(22, 11, new ColoredString("not", cond.Check == "not" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "not"; });
                            Con.PrintClickable(26, 11, new ColoredString("gt", cond.Check == "above" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "above"; });
                            Con.PrintClickable(29, 11, new ColoredString("lt", cond.Check == "below" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "below"; });
                            Con.PrintClickable(32, 11, new ColoredString("bool", cond.Check == "bool" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "bool"; });
                            Con.PrintClickable(37, 11, new ColoredString("e<>", cond.Check == "e<>" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "e<>"; });
                            Con.PrintClickable(41, 11, new ColoredString("e<!>", cond.Check == "e<!>" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "e<!>"; });
                            Con.PrintClickable(46, 11, new ColoredString("i<>", cond.Check == "i<>" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "i<>"; });
                            Con.PrintClickable(50, 11, new ColoredString("i<!>", cond.Check == "i<!>" ? Color.Yellow : Color.White, Color.Black), () => { cond.Check = "i<!>"; });

                            Con.Print(0, 12, "--| Target Number: ");
                            Con.PrintAdjustableInt(19, 12, 8, ref cond.CompareNum, int.MinValue, int.MaxValue);
                            Con.Print(0, 13, "--|  Range Number: ");
                            Con.PrintAdjustableInt(19, 13, 8, ref cond.Secondary, cond.CompareNum, int.MaxValue);

                            if (cond.CompareNum > cond.Secondary) {
                                cond.Secondary = cond.CompareNum;
                            }




                            Con.Print(54, 16, "Condition: ");
                            Con.PrintClickable(72, 16, "[NEXT]", () => { subIndex++; });
                            Con.PrintClickable(65, 16, "[PREV]", () => { subIndex--; });


                            Con.PrintClickable(0, 9, new ColoredString("--| Delete Condition", Color.Red, Color.Black), () => { block.ActiveConditions.RemoveAt(subIndex); });
                        }



                        Con.Print(46, 17, "Description Block: ");
                        Con.PrintClickable(72, 17, "[NEXT]", () => { selectedIndex++; });
                        Con.PrintClickable(65, 17, "[PREV]", () => { selectedIndex--; });

                        Con.PrintClickable(0, 2, new ColoredString("| Delete", Color.Red, Color.Black), () => loc.DescriptionBlocks.RemoveAt(selectedIndex));
                    }
                }
                else if (selectedType == "item") {
                    string countStr = loc.ItemsAtNode.Count > 1 ? ((selectedIndex + 1) + "/" + loc.ItemsAtNode.Count) : loc.ItemsAtNode.Count.ToString();
                    Con.Print(0, 2, "Items at Node (" + countStr + ")");
                    Con.PrintClickable(0, 3, new ColoredString("| Add New", Color.Lime, Color.Black), MapClick, "newItem");

                    selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(0, loc.ItemsAtNode.Count - 1));
                    if (loc.ItemsAtNode.Count > 0 && selectedIndex >= 0) {

                        Con.PrintClickable(0, 4, new ColoredString("| Delete", Color.Red, Color.Black), MapClick, "delItem");

                        if (loc.ItemsAtNode.Count > 0) {
                            Con.Print(0, 5, "| zeri:");
                            Con.PrintClickable(9, 5, "-", MapClick, "zeriDownItem");
                            string zeriCount = loc.ItemsAtNode[selectedIndex].zeriCost.ToString();
                            Con.Print(11, 5, zeriCount);
                            Con.PrintClickable(11 + zeriCount.Length + 1, 5, "+", MapClick, "zeriUpItem");

                            Con.Print(0, 6, "| time:");
                            Con.PrintClickable(9, 6, "-", MapClick, "timeDownItem");
                            string timeCost = loc.ItemsAtNode[selectedIndex].timeCost.ToString();
                            Con.Print(11, 6, timeCost);
                            Con.PrintClickable(11 + timeCost.Length + 1, 6, "+", MapClick, "timeUpItem");

                            Con.Print(0, 7, "| Days:");
                            Con.PrintClickable(9, 7, "-", MapClick, "daysDown");
                            string resetDays = loc.ItemsAtNode[selectedIndex].resetDays.ToString();
                            Con.Print(11, 7, resetDays);
                            Con.PrintClickable(11 + resetDays.Length + 1, 7, "+", MapClick, "daysUp");

                            Con.PrintClickable(0, 8, "| Shop Item: " + loc.ItemsAtNode[selectedIndex].shopItem.ToString(), MapClick, "shopToggle");

                            Con.Print(0, 10, loc.ItemsAtNode[selectedIndex].itemID);


                            Con.PrintClickable(72, 17, "[NEXT]", () => { selectedIndex++; });
                            Con.PrintClickable(65, 17, "[PREV]", () => { selectedIndex--; });
                        }
                    }
                }
                else if (selectedType == "object") { 
                    string countStr = loc.ObjectsHere.Count > 1 ? ((selectedIndex + 1) + "/" + loc.ObjectsHere.Count) : loc.ObjectsHere.Count.ToString();
                    Con.Print(0, 0, "Objects at Node (" + countStr + ")");
                    Con.PrintClickable(0, 1, new ColoredString("| Add New", Color.Lime, Color.Black), () => { loc.ObjectsHere.Add(""); selectedIndex = loc.ObjectsHere.Count - 1; });

                    selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(0, loc.ObjectsHere.Count - 1));
                    if (loc.ObjectsHere.Count > 0 && selectedIndex >= 0) {

                        if (loc.ObjectsHere.Count > 0) {
                            string tempID = loc.ObjectsHere[selectedIndex];

                            Con.PrintStringField(0, 3, "| Edit Object ID", ref tempID, ref TypingField, "objectID" + selectedIndex, true);

                            loc.ObjectsHere[selectedIndex] = tempID;

                            Con.Print(0, 4, "| ID: " + loc.ObjectsHere[selectedIndex], Color.Turquoise);
                             

                            Con.PrintClickable(72, 17, "[NEXT]", () => { selectedIndex++; });
                            Con.PrintClickable(65, 17, "[PREV]", () => { selectedIndex--; });
                        }


                        Con.PrintClickable(0, 2, new ColoredString("| Delete", Color.Red, Color.Black), () => { loc.ObjectsHere.RemoveAt(selectedIndex); });
                    }
                }
            }
        }

        public void MapClick(string id) {
            Location? loc = Helper.ResolveLoc(GameLoop.World.Player.NavLoc);
            if (id == "save") { 
                if (loc != null) {
                    Helper.SerializeToFile(loc, "./data/locations/" + loc.ID + ".dat");
                }
            }
            else if (id == "indexUp") {
                selectedIndex++;
            }
            else if (id == "indexDown") {
                selectedIndex--;
            }   
            else if (id == "newLoc") {
                if (loc != null) {
                    loc.ConnectedLocations.Add(new("", ""));
                    selectedIndex = loc.ConnectedLocations.Count - 1;
                }
            }
            else if (id == "newItem") {
                if (loc != null) {
                    loc.ItemsAtNode.Add(new("", 0, 0, 0, false));
                    selectedIndex = loc.ItemsAtNode.Count - 1;
                }
            }
            else if (id == "delConnection") {
                if (loc != null) {
                    loc.ConnectedLocations.RemoveAt(selectedIndex);
                }
            }
            else if (id == "delItem") {
                if (loc != null) {
                    loc.ItemsAtNode.RemoveAt(selectedIndex);
                }
            }   

            else if (id == "backToMenu") {
                selectedType = "none";
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Location? loc = Helper.ResolveLoc(GameLoop.World.Player.NavLoc); 
        } 
    }
}
