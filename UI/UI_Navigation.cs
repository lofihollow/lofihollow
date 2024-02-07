using LofiHollow.DataTypes;
using LofiHollow.Entities.NPC;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;
using Key = SadConsole.Input.Keys;

namespace LofiHollow.UI {
    public class UI_Navigation : InstantUI {
        public string devOverride = "";
        public List<string> VisibleDescriptionBlocks = new();

        public List<NPC> NPCsHere = new();
        public Dictionary<string, List<NPC>> VisibleMapNPCs = new();

        public UI_Navigation(int width, int height) : base(width, height, "Navigation") {
            Win.CanDrag = false;
            Win.Position = new Point(40, 0);

        }


        public override void Update() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            Con.Clear();

            Location? loc = Helper.ResolveLoc(GameLoop.World.Player.NavLoc);
            if (loc != null) {
                Con.Print(0, 0, loc.DisplayName.Align(HorizontalAlignment.Center, 141));
                Con.DrawLine(new Point(0, 1), new Point(141, 1), (char)196, Color.White, Color.Black);

                string mainDesc = loc.Description;

                VisibleDescriptionBlocks.Clear();

                if (!GameLoop.UIManager.MapEditor.Win.IsVisible) {
                    mainDesc = loc.Description;


                    for (int i = 0; i < loc.DescriptionBlocks.Count; i++) {
                        if (loc.DescriptionBlocks[i].AllConditionsPassed()) {
                            if (loc.DescriptionBlocks[i].ReplacesMainBlock) {
                                mainDesc = loc.DescriptionBlocks[i].Description;
                            } else {
                                VisibleDescriptionBlocks.Add(loc.DescriptionBlocks[i].Description);
                            } 
                        }
                    }
                } else {
                    if (devOverride != "")
                        mainDesc = devOverride;

                    for (int i = 0; i < loc.DescriptionBlocks.Count; i++) {
                        if (!loc.DescriptionBlocks[i].ReplacesMainBlock) { 
                            VisibleDescriptionBlocks.Add(loc.DescriptionBlocks[i].Description);
                        }
                    }
                }
                 

                int descLow = Con.PrintMultiLine(0, 2, mainDesc, 141) + 1;

                for (int i = 0; i < VisibleDescriptionBlocks.Count; i++) {
                    descLow++;
                    descLow = Con.PrintMultiLine(0, descLow, VisibleDescriptionBlocks[i], 141) + 1;
                }

                Con.DrawLine(new Point(0, descLow), new Point(141, descLow++), (char)196, Color.White, Color.Black);

                Con.Print(0, descLow++, "Connected Locations:");

                for (int i = 0; i < loc.ConnectedLocations.Count; i++) {
                    ConnectionNode node = loc.ConnectedLocations[i];

                    Location? dest = Helper.ResolveLoc(node.LocationID);

                    if (node.AllConditionsPassed() || node.VisibleWhileInactive || GameLoop.DevMode) {
                        string nameText = dest == null ? node.LocationID : dest.DisplayName;
                        Color textColor = node.HasConditions() ? node.AllConditionsPassed() ? Color.Lime : Color.Red : Color.White;

                        Con.PrintClickable(0, descLow++, new ColoredString("| ") + new ColoredString(node.Direction + ": " + nameText, textColor, Color.Black), () => { GameLoop.World.Player.TryMoveTo(node); });

                        int textLen = ("| " + node.Direction + ": " + nameText).Length;

                        if (node.ReqSummary != null && node.ReqSummary != "") { 
                            Con.Print(textLen + 2, descLow - 1, "[" + node.ReqSummary + "]", Color.DarkSlateGray);
                            textLen += 2 + ("[" + node.ReqSummary + "]").Length + 1;
                        }

                        if (node.CostSummary != null && node.CostSummary != "") {
                            Con.Print(textLen, descLow - 1, "(" + node.CostSummary + ")", Color.DarkSlateBlue);
                        }


                        if (VisibleMapNPCs.ContainsKey(node.LocationID)) {
                            Con.Print(0, descLow, 192.AsString() + 196.AsString() + ">", Color.Gray);

                            int NPCsX = 4;

                            for (int j = 0; j < VisibleMapNPCs[node.LocationID].Count; j++) {
                                NPC thisOne = VisibleMapNPCs[node.LocationID][j];

                                if (NPCsX + thisOne.Name.Length + 2 < Con.Width) {
                                    string notFirst = j == 0 ? "" : ", ";
                                    Con.Print(NPCsX, descLow, new ColoredString(notFirst) + new ColoredString(thisOne.Name, thisOne.GetDispColor(), Color.Black));
                                    NPCsX += (notFirst.Length + thisOne.Name.Length);
                                }
                            }
                            descLow++;
                        }
                    }
                }
                 

                if (loc.ObjectsHere.Count > 0) {
                    Con.DrawLine(new Point(0, descLow), new Point(141, descLow++), (char)196, Color.White, Color.Black);

                    int objTitleY = descLow;
                    descLow++;

                    bool anyPrinted = false;

                    for (int i = 0; i < loc.ObjectsHere.Count; i++) {
                        NodeObject? obj = Helper.ResolveObj(loc.ObjectsHere[i]); 

                        if (obj == null) {
                            Con.Print(0, descLow++, "| " + loc.ObjectsHere[i]);
                        } else {
                            Color col = obj.AllConditionsPassed(GameLoop.World.Player, loc.ID) ? Color.Lime : Color.Red;

                            if (!obj.HideConditionsPassed()) {
                                anyPrinted = true;
                                Con.PrintClickable(0, descLow++, new ColoredString("| ") + new ColoredString(obj.DisplayName + ": " + obj.InteractVerb, col, Color.Black), () => { GameLoop.World.Player.TryUseObject(obj, loc.ID); });

                                int textLen = ("| " + obj.DisplayName + ": " + obj.InteractVerb).Length + 1;

                                if (obj.ReqSummary != null && obj.ReqSummary != "") {
                                    Con.Print(textLen, descLow - 1, "[" + obj.ReqSummary + "]", Color.DarkSlateGray);
                                    textLen += 2 + ("[" + obj.ReqSummary + "]").Length;
                                }

                                if (obj.CostSummary != null && obj.CostSummary != "") {
                                    Con.Print(textLen, descLow - 1, "(" + obj.CostSummary + ")", Color.DarkSlateBlue);
                                    textLen += 2 + ("(" + obj.ReqSummary + ")").Length;
                                }

                                if (obj.NeedSummary != null && obj.NeedSummary != "") {
                                    Con.Print(textLen, descLow - 1, "<" + obj.NeedSummary + ">", Color.Orange);
                                    textLen += 2 + ("<" + obj.NeedSummary + ">").Length;
                                }

                                if (obj.MinigameSummary != null && obj.MinigameSummary != "") {
                                    Con.Print(textLen, descLow - 1, "{" + obj.MinigameSummary + "}", Color.Orange);
                                    textLen += 2 + ("{" + obj.MinigameSummary + "}").Length;
                                }
                            }
                        }
                    }

                    if (anyPrinted) 
                        Con.Print(0, objTitleY, "Objects/Actions Here:");
                }



                if (loc.ItemsAtNode.Count > 0 || loc.ItemsOnGround.Count > 0) {
                    Con.DrawLine(new Point(0, descLow), new Point(141, descLow++), (char)196, Color.White, Color.Black);

                    Con.Print(0, descLow++, "Items Here:");

                    for (int i = 0; i < loc.ItemsAtNode.Count; i++) {
                        Item? item = Helper.ResolveItem(loc.ItemsAtNode[i].itemID);
                        string nameText = item == null ? loc.ItemsAtNode[i].itemID : item.Name;

                        Con.PrintClickable(0, descLow++, "| " + nameText, () => { GameLoop.World.Player.TryPickupNodeItem(loc.ItemsAtNode[i]); });
                    }

                    if (loc.ItemsAtNode.Count > 0 && loc.ItemsOnGround.Count > 0)
                        descLow++;

                    for (int i = 0; i < loc.ItemsOnGround.Count; i++) {
                        string nameText = loc.ItemsOnGround[i].Quantity > 0 ? (loc.ItemsOnGround[i].Quantity + "x " + loc.ItemsOnGround[i].Name) : loc.ItemsOnGround[i].Name;
                        Con.PrintClickable(0, descLow++, "| " + nameText, () => { GameLoop.World.Player.PickupItem(loc, i); });
                    }


                    Con.DrawLine(new Point(0, descLow), new Point(141, descLow++), (char)196, Color.White, Color.Black);
                }

                if (NPCsHere.Count > 0) {
                    Con.DrawLine(new Point(0, descLow), new Point(141, descLow++), (char)196, Color.White, Color.Black);

                    Con.Print(0, descLow++, "People Here:");

                    for (int i = 0; i < NPCsHere.Count; i++) {   
                        Con.PrintClickable(0, descLow++, new ColoredString("| ") + new ColoredString(NPCsHere[i].Name, NPCsHere[i].GetDispColor(), Color.Black), () => { GameLoop.World.Player.TrySpeakNPC(NPCsHere[i]); });
                    }
                }

            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;
        }


        public void MovedMaps() {
            NPCsHere.Clear();
            VisibleMapNPCs.Clear();

            Location? loc = Helper.ResolveLoc(GameLoop.World.Player.NavLoc);

            if (loc != null) {

                foreach (var kv in GameLoop.World.npcLibrary) {
                    if (kv.Value.NavLocation == GameLoop.World.Player.NavLoc) {
                        NPCsHere.Add(kv.Value);
                    } else {
                        for (int i = 0; i < loc.ConnectedLocations.Count; i++) {
                            if (kv.Value.NavLocation == loc.ConnectedLocations[i].LocationID) {
                                if (!VisibleMapNPCs.ContainsKey(loc.ConnectedLocations[i].LocationID)) {
                                    VisibleMapNPCs.Add(loc.ConnectedLocations[i].LocationID, new());
                                }

                                VisibleMapNPCs[loc.ConnectedLocations[i].LocationID].Add(kv.Value);
                            }
                        }
                    }
                }
            } 
        }
    }
}
