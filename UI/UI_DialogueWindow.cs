using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.Managers;
using LofiHollow.Missions;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;


namespace LofiHollow.UI {
    public class UI_DialogueWindow {
        public bool BuyingFromShop = true;
        public List<Item> BuyingItems = new();
        public List<Item> SellingItems = new();
        public int BuyCopper = 0;
        public int BuySilver = 0;
        public int BuyGold = 0;
        public int BuyJade = 0;

        public string chitChat1 = "";
        public string chitChat2 = "";
        public string chitChat3 = "";
        public string chitChat4 = "";
        public string chitChat5 = "";

        public string dialogueOption = "None";
        public string dialogueLatest = "";
        public Mission CurrentMission;

        public NPC DialogueNPC = null;
        List<Item> TownHallPermits = new();

        public int shopTopIndex = 0;

        public SadConsole.Console DialogueConsole;
        public Window DialogueWindow;

        public UI_DialogueWindow(int width, int height, string title) {
            DialogueWindow = new(width, height);
            DialogueWindow.CanDrag = false;
            DialogueWindow.Position = new(11, 6);

            int diaConWidth = width - 2;
            int diaConHeight = height - 2;

            DialogueConsole = new(diaConWidth, diaConHeight);
            DialogueConsole.Position = new(1, 1);
            DialogueWindow.Title = title.Align(HorizontalAlignment.Center, diaConWidth, (char)196);


            DialogueWindow.Children.Add(DialogueConsole);
            GameLoop.UIManager.Children.Add(DialogueWindow);

            DialogueWindow.Show();
            DialogueWindow.IsVisible = false;
        }


        public void SetupDialogue(NPC npc) {
            DialogueNPC = npc;
            GameLoop.UIManager.selectedMenu = "Dialogue";
            DialogueConsole.Clear();

            dialogueOption = "None";
            DialogueWindow.IsVisible = true;

            foreach (KeyValuePair<string, Mission> kv in GameLoop.World.Player.MissionLog) {
                if (kv.Value.Stages[kv.Value.CurrentStage] != null) {
                    if (kv.Value.Stages[kv.Value.CurrentStage].NPC == DialogueNPC.Name || kv.Value.Stages[kv.Value.CurrentStage].NPC == "Any") {
                        if (kv.Value.Stages[kv.Value.CurrentStage].AllRequirementsMet(GameLoop.World.Player)) {
                            CurrentMission = kv.Value; 
                            break;
                        }
                    }
                }
            }  

            if (GameLoop.World.Player.MetNPCs.ContainsKey(DialogueNPC.Name)) {
                if (DialogueNPC.Greetings.ContainsKey(DialogueNPC.RelationshipDescriptor())) {
                    dialogueLatest = DialogueNPC.Greetings[DialogueNPC.RelationshipDescriptor()];
                } else {
                    dialogueLatest = "Error: Greeting not found for relationship " + DialogueNPC.RelationshipDescriptor();
                }
            } else {
               dialogueLatest = GameLoop.UIManager.DialogueWindow.DialogueNPC.Introduction;
                GameLoop.World.Player.MetNPCs.Add(DialogueNPC.Name, 0);
            }

            DialogueNPC.UpdateChitChats();
        }


        public void RenderDialogue() {
            DialogueConsole.Clear();
            Point mousePos = new MouseScreenObjectState(DialogueConsole, GameHost.Instance.Mouse).CellPosition;

            if (DialogueNPC != null) {
                DialogueWindow.IsFocused = true;
                int opinion = 0;
                if (GameLoop.World.Player.MetNPCs.ContainsKey(DialogueNPC.Name))
                    opinion = GameLoop.World.Player.MetNPCs[DialogueNPC.Name];

                DialogueWindow.Title = (DialogueNPC.Name + ", " + DialogueNPC.Occupation + " - " + DialogueNPC.RelationshipDescriptor() + " (" + opinion + ")").Align(HorizontalAlignment.Center, DialogueWindow.Width - 2, (char)196);

                if (dialogueOption == "None" || dialogueOption == "Goodbye") {
                    if (dialogueLatest.Contains('@') && !GameLoop.World.Player.Name.Contains('@')) {
                        int index = dialogueLatest.IndexOf('@');
                        dialogueLatest = dialogueLatest.Remove(index, 1);
                        dialogueLatest = dialogueLatest.Insert(index, GameLoop.World.Player.Name);
                    }
                    string[] allLines = dialogueLatest.Split("|");

                    for (int i = 0; i < allLines.Length; i++)
                        DialogueConsole.Print(1, 1 + i, new ColoredString(allLines[i], Color.White, Color.Black));
                }

                if (dialogueOption == "None") {
                    if (CurrentMission == null) {
                        foreach (KeyValuePair<string, Mission> kv in GameLoop.World.Player.MissionLog) {
                            if (kv.Value.Stages != null && kv.Value.Stages.Count > kv.Value.CurrentStage && kv.Value.Stages[kv.Value.CurrentStage] != null) {
                                if (kv.Value.Stages[kv.Value.CurrentStage].NPC == DialogueNPC.Name || kv.Value.Stages[kv.Value.CurrentStage].NPC == "Any") {
                                    if (kv.Value.Stages[kv.Value.CurrentStage].AllRequirementsMet(GameLoop.World.Player)) {
                                        CurrentMission = kv.Value;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //  DialogueConsole.Print(1, DialogueConsole.Height - 19, new ColoredString("Mission: [A book for Cobalt]", mousePos.Y == DialogueConsole.Height - 19 ? Color.Yellow : Color.White, Color.Black));

                    DialogueConsole.DrawLine(new Point(0, DialogueConsole.Height - 20), new Point(DialogueConsole.Width - 1, DialogueConsole.Height - 20), 196, Color.White, Color.Black);

                    if (DialogueNPC.Shop != null && DialogueNPC.Shop.ShopOpen(DialogueNPC))
                        DialogueConsole.Print(1, DialogueConsole.Height - 17, Helper.HoverColoredString("[Open Store]", mousePos.Y == DialogueConsole.Height - 17));

                    if (CurrentMission != null)
                        DialogueConsole.Print(1, DialogueConsole.Height - 19, Helper.HoverColoredString("[Mission] " + CurrentMission.Name, mousePos.Y == DialogueConsole.Height - 19));

                    DialogueConsole.Print(1, DialogueConsole.Height - 15, Helper.HoverColoredString("[Give Item]", mousePos.Y == DialogueConsole.Height - 15));

                    DialogueConsole.Print(1, DialogueConsole.Height - 12, Helper.HoverColoredString("Chit-chat: " + chitChat1, mousePos.Y == DialogueConsole.Height - 12));
                    DialogueConsole.Print(1, DialogueConsole.Height - 10, Helper.HoverColoredString("Chit-chat: " + chitChat2, mousePos.Y == DialogueConsole.Height - 10));
                    DialogueConsole.Print(1, DialogueConsole.Height - 8, Helper.HoverColoredString("Chit-chat: " + chitChat3, mousePos.Y == DialogueConsole.Height - 8));
                    DialogueConsole.Print(1, DialogueConsole.Height - 6, Helper.HoverColoredString("Chit-chat: " + chitChat4, mousePos.Y == DialogueConsole.Height - 6));
                    DialogueConsole.Print(1, DialogueConsole.Height - 4, Helper.HoverColoredString("Chit-chat: " + chitChat5, mousePos.Y == DialogueConsole.Height - 4));

                    DialogueConsole.Print(1, DialogueConsole.Height - 1, Helper.HoverColoredString("Nevermind.", mousePos.Y == DialogueConsole.Height - 1));
                } else if (dialogueOption == "Goodbye") {
                    DialogueConsole.Print(1, DialogueConsole.Height - 1, Helper.HoverColoredString("[Click anywhere to close]", mousePos.Y == DialogueConsole.Height - 1));
                } else if (dialogueOption == "Gift") {
                    int y = 1;
                    DialogueConsole.Print(1, y++, "Give what?");

                    for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                        Item item = GameLoop.World.Player.Inventory[i];

                        string nameWithDurability = item.Name;

                        if (item.Durability >= 0)
                            nameWithDurability = "[" + item.Durability + "] " + item.Name;

                        DialogueConsole.Print(1, y, item.AsColoredGlyph());
                        if (item.Dec != null) {
                            DialogueConsole.SetDecorator(1, y, 1, new CellDecorator(new Color(item.Dec.R, item.Dec.G, item.Dec.B), item.Dec.Glyph, Mirror.None));
                        }
                        if (!item.IsStackable || (item.IsStackable && item.ItemQuantity == 1))
                            DialogueConsole.Print(3, y, new ColoredString(nameWithDurability, mousePos.Y == y ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black));
                        else
                            DialogueConsole.Print(3, y, new ColoredString(("(" + item.ItemQuantity + ") " + item.Name), mousePos.Y == y ? Color.Yellow : item.Name == "(EMPTY)" ? Color.DarkSlateGray : Color.White, Color.Black));

                        y++;
                    }

                    DialogueConsole.Print(1, y + 2, Helper.HoverColoredString("Nevermind.", mousePos.Y == y + 2));
                } else if (dialogueOption == "Shop") {

                    ColoredString shopHeader = new(DialogueNPC.Shop.ShopName, Color.White, Color.Black);


                    ColoredString playerCopperString = new("CP:" + GameLoop.World.Player.CopperCoins, new Color(184, 115, 51), Color.Black);
                    ColoredString playerSilverString = new("SP:" + GameLoop.World.Player.SilverCoins, Color.Silver, Color.Black);
                    ColoredString playerGoldString = new("GP:" + GameLoop.World.Player.GoldCoins, Color.Yellow, Color.Black);
                    ColoredString playerJadeString = new("JP:" + GameLoop.World.Player.JadeCoins, new Color(0, 168, 107), Color.Black);

                    ColoredString playerMoney = new("", Color.White, Color.Black);
                    playerMoney += playerCopperString + new ColoredString(" / ", Color.White, Color.Black);
                    playerMoney += playerSilverString + new ColoredString(" / ", Color.White, Color.Black);
                    playerMoney += playerGoldString + new ColoredString(" / ", Color.White, Color.Black);
                    playerMoney += playerJadeString;



                    DialogueConsole.Print(1, 0, GameLoop.World.Player.Name);
                    DialogueConsole.Print(1, 1, playerMoney);
                    DialogueConsole.Print(DialogueConsole.Width - (shopHeader.Length + 1), 0, shopHeader);
                    DialogueConsole.DrawLine(new Point(0, 2), new Point(DialogueConsole.Width - 1, 2), 196);
                    if (BuyingFromShop) {
                        DialogueConsole.Print(0, 3, " Buy  |" + "Item Name".Align(HorizontalAlignment.Center, 23) + "|" + "Short Description".Align(HorizontalAlignment.Center, 27) + "|" + "Price".Align(HorizontalAlignment.Center, 11));
                        DialogueConsole.DrawLine(new Point(0, 4), new Point(DialogueConsole.Width - 1, 4), 196);

                        for (int j = shopTopIndex; j < DialogueNPC.Shop.SoldItems.Count && j < shopTopIndex + 14; j++) {
                            int i = j - shopTopIndex;
                            if (DialogueNPC.Shop.SoldItems[j].Contains("*")) {
                                string[] split = DialogueNPC.Shop.SoldItems[j].Split(":");
                                split[1] = split[1].Replace("*", "");
                                DialogueNPC.Shop.SoldItems.RemoveAt(j);

                                foreach (KeyValuePair<string, Item> kv in GameLoop.World.itemLibrary) {
                                    if (kv.Key.Contains(split[0] + ":") && kv.Key.Contains(split[1])) {
                                        DialogueNPC.Shop.SoldItems.Add(kv.Key);
                                    }
                                }

                                break;
                            } else {
                                Item item = new(DialogueNPC.Shop.SoldItems[j]);
                                int price = DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], item, false);

                                int buyQuantity = 0;

                                for (int k = 0; k < BuyingItems.Count; k++) {
                                    if (BuyingItems[k].StacksWith(item, true)) {
                                        buyQuantity += BuyingItems[k].ItemQuantity;
                                    }
                                }

                                DialogueConsole.Print(0, 5 + (2 * i), new ColoredString("-", mousePos.Y == 5 + (2 * i) && mousePos.X == 0 ? Color.Red : Color.White, Color.Black));
                                DialogueConsole.Print(1, 5 + (2 * i), new ColoredString(buyQuantity.ToString().Align(HorizontalAlignment.Center, 3), Color.White, Color.Black));
                                DialogueConsole.Print(5, 5 + (2 * i), new ColoredString("+", mousePos.Y == 5 + (2 * i) && mousePos.X == 5 ? Color.Lime : Color.White, Color.Black));
                                DialogueConsole.Print(6, 5 + (2 * i), Helper.HoverColoredString("|" + item.Name.Align(HorizontalAlignment.Center, 23) + "|" + item.ShortDesc.Align(HorizontalAlignment.Center, 27) + "|", mousePos.Y == 5 + (2 *i)));
                                DialogueConsole.Print(DialogueConsole.Width - 10, 5 + (2 * i), Helper.ConvertCoppers(price));
                                DialogueConsole.DrawLine(new Point(0, 6 + (2 * i)), new Point(DialogueConsole.Width, 6 + (2 * i)), '-', Color.White, Color.Black);
                            }
                        }

                        DialogueConsole.DrawLine(new Point(0, DialogueConsole.Height - 7), new Point(DialogueConsole.Width - 1, DialogueConsole.Height - 7), 196);
                        DialogueConsole.Print(1, DialogueConsole.Height - 6, Helper.HoverColoredString("[View Inventory]", (mousePos.Y == DialogueConsole.Height - 6 && mousePos.X >= 1 && mousePos.X <= 16)));
                    } else {
                        DialogueConsole.Print(0, 3, " Sell |" + "Item Name".Align(HorizontalAlignment.Center, 23) + "|" + "Short Description".Align(HorizontalAlignment.Center, 27) + "|" + "Price".Align(HorizontalAlignment.Center, 11));
                        DialogueConsole.DrawLine(new Point(0, 4), new Point(DialogueConsole.Width - 1, 4), 196);


                        for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                            Item item = new(GameLoop.World.Player.Inventory[i]);
                            item.ItemQuantity = GameLoop.World.Player.Inventory[i].ItemQuantity;
                            int price = DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], item, true);

                            int sellQuantity = 0;

                            for (int j = 0; j < SellingItems.Count; j++) {
                                if (SellingItems[j].StacksWith(item, true)) {
                                    sellQuantity = SellingItems[j].ItemQuantity;
                                }
                            }

                            string name = item.Name;

                            if (item.ItemQuantity > 1) {
                                name = "[" + item.ItemQuantity + "] " + name;
                            }

                            DialogueConsole.Print(0, 5 + i, new ColoredString("-", mousePos.Y == 5 + i && mousePos.X == 0 ? Color.Red : Color.White, Color.Black));
                            DialogueConsole.Print(1, 5 + i, new ColoredString(sellQuantity.ToString().Align(HorizontalAlignment.Center, 3), Color.White, Color.Black));
                            DialogueConsole.Print(5, 5 + i, new ColoredString("+", mousePos.Y == 5 + i && mousePos.X == 5 ? Color.Lime : Color.White, Color.Black));
                            DialogueConsole.Print(6, 5 + i, Helper.HoverColoredString("|" + name.Align(HorizontalAlignment.Center, 23) + "|" + item.ShortDesc.Align(HorizontalAlignment.Center, 27) + "|", mousePos.Y == 5 + i));
                            DialogueConsole.Print(DialogueConsole.Width - 10, 5 + i, Helper.ConvertCoppers(price));
                        }

                        DialogueConsole.DrawLine(new Point(0, DialogueConsole.Height - 7), new Point(DialogueConsole.Width - 1, DialogueConsole.Height - 7), 196);
                        DialogueConsole.Print(1, DialogueConsole.Height - 6, Helper.HoverColoredString("[View Shop]", (mousePos.Y == DialogueConsole.Height - 6 && mousePos.X >= 1 && mousePos.X <= 11)));
                    }

                    int buyValue = 0;

                    for (int j = 0; j < BuyingItems.Count; j++) {
                        buyValue += BuyingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], BuyingItems[j], false);
                    }

                    int sellValue = 0;

                    for (int j = 0; j < SellingItems.Count; j++) {
                        sellValue += SellingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], SellingItems[j], true);
                    }

                    DialogueConsole.Print(28, DialogueConsole.Height - 6, "Coins");

                    ColoredString buyCopperString = new(BuyCopper.ToString(), new Color(184, 115, 51), Color.Black);
                    ColoredString buySilverString = new(BuySilver.ToString(), Color.Silver, Color.Black);
                    ColoredString buyGoldString = new(BuyGold.ToString(), Color.Yellow, Color.Black);
                    ColoredString buyJadeString = new(BuyJade.ToString(), new Color(0, 168, 107), Color.Black);

                    DialogueConsole.Print(30, DialogueConsole.Height - 5, buyCopperString);
                    DialogueConsole.Print(30, DialogueConsole.Height - 4, buySilverString);
                    DialogueConsole.Print(30, DialogueConsole.Height - 3, buyGoldString);
                    DialogueConsole.Print(30, DialogueConsole.Height - 2, buyJadeString);

                    DialogueConsole.Print(28, DialogueConsole.Height - 5, new ColoredString("-", mousePos == new Point(28, DialogueConsole.Height - 5) ? Color.Red : Color.White, Color.Black));
                    DialogueConsole.Print(28, DialogueConsole.Height - 4, new ColoredString("-", mousePos == new Point(28, DialogueConsole.Height - 4) ? Color.Red : Color.White, Color.Black));
                    DialogueConsole.Print(28, DialogueConsole.Height - 3, new ColoredString("-", mousePos == new Point(28, DialogueConsole.Height - 3) ? Color.Red : Color.White, Color.Black));
                    DialogueConsole.Print(28, DialogueConsole.Height - 2, new ColoredString("-", mousePos == new Point(28, DialogueConsole.Height - 2) ? Color.Red : Color.White, Color.Black));

                    DialogueConsole.Print(32, DialogueConsole.Height - 5, new ColoredString("+", mousePos == new Point(32, DialogueConsole.Height - 5) ? Color.Lime : Color.White, Color.Black));
                    DialogueConsole.Print(32, DialogueConsole.Height - 4, new ColoredString("+", mousePos == new Point(32, DialogueConsole.Height - 4) ? Color.Lime : Color.White, Color.Black));
                    DialogueConsole.Print(32, DialogueConsole.Height - 3, new ColoredString("+", mousePos == new Point(32, DialogueConsole.Height - 3) ? Color.Lime : Color.White, Color.Black));
                    DialogueConsole.Print(32, DialogueConsole.Height - 2, new ColoredString("+", mousePos == new Point(32, DialogueConsole.Height - 2) ? Color.Lime : Color.White, Color.Black));

                    sellValue += BuyCopper;
                    sellValue += BuySilver * 10;
                    sellValue += BuyGold * 100;
                    sellValue += BuyJade * 1000;

                    DialogueConsole.Print(27, DialogueConsole.Height - 1, "[EXACT]");

                    DialogueConsole.Print(1, DialogueConsole.Height - 4, new ColoredString("Buy Value: ", Color.White, Color.Black) + Helper.ConvertCoppers(buyValue));
                    DialogueConsole.Print(1, DialogueConsole.Height - 3, new ColoredString("Sell Value: ", Color.White, Color.Black) + Helper.ConvertCoppers(sellValue));

                    int diff = buyValue - sellValue;

                    string total = diff > 0 ? "You owe " : diff == 0 ? "Trade is equal" : "You are owed ";

                    if (diff < 0)
                        DialogueConsole.Print(1, DialogueConsole.Height - 2, new ColoredString(total, Color.White, Color.Black) + Helper.ConvertCoppers(-diff));
                    else if (diff > 0)
                        DialogueConsole.Print(1, DialogueConsole.Height - 2, new ColoredString(total, Color.White, Color.Black) + Helper.ConvertCoppers(diff));
                    else
                        DialogueConsole.Print(1, DialogueConsole.Height - 2, new ColoredString(total, Color.White, Color.Black));

                    if (diff <= 0)
                        DialogueConsole.Print(DialogueConsole.Width - 15, DialogueConsole.Height - 5, new ColoredString("[Accept - Gift]", Color.Green, Color.Black));
                    else
                        DialogueConsole.Print(DialogueConsole.Width - 15, DialogueConsole.Height - 5, new ColoredString("[Accept - Gift]", Color.Red, Color.Black));


                    if (diff <= 0)
                        DialogueConsole.Print(DialogueConsole.Width - 8, DialogueConsole.Height - 3, new ColoredString("[Accept]", Color.Green, Color.Black));
                    else
                        DialogueConsole.Print(DialogueConsole.Width - 8, DialogueConsole.Height - 3, new ColoredString("[Accept]", Color.Red, Color.Black));

                    DialogueConsole.Print(DialogueConsole.Width - 12, DialogueConsole.Height - 1, Helper.HoverColoredString("[Close Shop]", (mousePos.Y == DialogueConsole.Height - 1 && mousePos.X >= DialogueConsole.Width - 12 && mousePos.X <= DialogueConsole.Width - 1)));

                } else if (dialogueOption == "Mission") {
                    if (CurrentMission != null) {
                        if (CurrentMission.Stages.Count > CurrentMission.CurrentStage) {
                            if (CurrentMission.Stages[CurrentMission.CurrentStage].Dialogue.ContainsKey(CurrentMission.Stages[CurrentMission.CurrentStage].CurrentDialogue)) {
                                MissionDialogue dialogue = CurrentMission.Stages[CurrentMission.CurrentStage].Dialogue[CurrentMission.Stages[CurrentMission.CurrentStage].CurrentDialogue];
                                dialogueLatest = dialogue.Text;

                                if (dialogueLatest.Contains('@') && !GameLoop.World.Player.Name.Contains('@')) {
                                    int index = dialogueLatest.IndexOf('@');
                                    dialogueLatest = dialogueLatest.Remove(index, 1);
                                    dialogueLatest = dialogueLatest.Insert(index, GameLoop.World.Player.Name);
                                }
                                string[] allLines = dialogueLatest.Split("|");

                                for (int i = 0; i < allLines.Length; i++)
                                    DialogueConsole.Print(1, 1 + i, new ColoredString(allLines[i], Color.White, Color.Black));

                                DialogueConsole.DrawLine(new Point(0, DialogueConsole.Height - 20), new Point(DialogueConsole.Width - 1, DialogueConsole.Height - 20), 196, Color.White, Color.Black);
                                
                                for (int i = 0; i < dialogue.Responses.Count; i++) {
                                    DialogueConsole.Print(1, (DialogueConsole.Height - 18) + (i * 2), Helper.RequirementString(dialogue.Responses[i].Text, mousePos.Y == (DialogueConsole.Height - 18) + (i * 2), dialogue.Responses[i].MeetsAllRequirements(GameLoop.World.Player)));
                                } 
                            }
                        } 
                    } 
                } 
            }
        } 

        public void CaptureDialogueClicks() {
            Point mousePos = new MouseScreenObjectState(DialogueConsole, GameHost.Instance.Mouse).CellPosition;

            if (DialogueNPC.Shop != null) {
                if (BuyingFromShop) {
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                        if (shopTopIndex + 1 < DialogueNPC.Shop.SoldItems.Count - 14)
                            shopTopIndex++;
                    } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                        if (shopTopIndex > 0)
                            shopTopIndex--;
                    }
                } else {
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                        if (shopTopIndex + 1 < GameLoop.World.Player.Inventory.Length - 14)
                            shopTopIndex++;
                    } else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                        if (shopTopIndex > 0)
                            shopTopIndex--;
                    }
                }
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (dialogueOption == "Goodbye") {
                    dialogueOption = "None";
                    GameLoop.UIManager.selectedMenu = "None";
                    DialogueWindow.IsVisible = false;
                    DialogueNPC = null;
                    dialogueLatest = "";
                    DialogueConsole.Clear();
                } else if (dialogueOption == "Shop") {
                    if (mousePos.Y == DialogueConsole.Height - 1 && mousePos.X >= DialogueConsole.Width - 12 && mousePos.X <= DialogueConsole.Width - 1) {
                        dialogueOption = "None";
                        BuyingItems.Clear();
                        SellingItems.Clear();
                        BuyingFromShop = true;
                    }

                    if (mousePos.X == 28) {
                        if (mousePos.Y == DialogueConsole.Height - 5 && BuyCopper > 0) { BuyCopper--; }
                        if (mousePos.Y == DialogueConsole.Height - 4 && BuySilver > 0) { BuySilver--; }
                        if (mousePos.Y == DialogueConsole.Height - 3 && BuyGold > 0) { BuyGold--; }
                        if (mousePos.Y == DialogueConsole.Height - 2 && BuyJade > 0) { BuyJade--; }
                    }

                    if (mousePos.X == 32) {
                        if (mousePos.Y == DialogueConsole.Height - 5 && BuyCopper < GameLoop.World.Player.CopperCoins) { BuyCopper++; }
                        if (mousePos.Y == DialogueConsole.Height - 4 && BuySilver < GameLoop.World.Player.SilverCoins) { BuySilver++; }
                        if (mousePos.Y == DialogueConsole.Height - 3 && BuyGold < GameLoop.World.Player.GoldCoins) { BuyGold++; }
                        if (mousePos.Y == DialogueConsole.Height - 2 && BuyJade < GameLoop.World.Player.JadeCoins) { BuyJade++; }
                    }

                    int buyValue = 0;
                    for (int j = 0; j < BuyingItems.Count; j++) {
                        buyValue += BuyingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], BuyingItems[j], false);
                    }

                    int sellValue = 0;
                    for (int j = 0; j < SellingItems.Count; j++) {
                        sellValue += SellingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], SellingItems[j], true);
                    }

                    sellValue += BuyCopper;
                    sellValue += BuySilver * 10;
                    sellValue += BuyGold * 100;
                    sellValue += BuyJade * 1000;

                    int diff = buyValue - sellValue;

                    if (mousePos.X >= 27 && mousePos.X <= 33 && mousePos.Y == DialogueConsole.Height - 1 && diff > 0) {
                        if (diff >= 1000)
                            BuyJade = diff / 1000;
                        diff -= BuyJade * 1000;

                        if (diff >= 100)
                            BuyGold = diff / 100;
                        diff -= BuyGold * 100;

                        if (diff >= 10)
                            BuySilver = diff / 10;
                        diff -= BuySilver * 10;

                        BuyCopper = diff;

                        if (BuyJade > GameLoop.World.Player.JadeCoins) {
                            int jadeOff = BuyJade - GameLoop.World.Player.JadeCoins;
                            BuyJade = GameLoop.World.Player.JadeCoins;
                            BuyGold += jadeOff * 10;
                        }

                        if (BuyGold > GameLoop.World.Player.GoldCoins) {
                            int goldOff = BuyJade - GameLoop.World.Player.GoldCoins;
                            BuyGold = GameLoop.World.Player.GoldCoins;
                            BuySilver += goldOff * 10;
                        }

                        if (BuySilver > GameLoop.World.Player.SilverCoins) {
                            int silverOff = BuyJade - GameLoop.World.Player.SilverCoins;
                            BuySilver = GameLoop.World.Player.SilverCoins;
                            BuyCopper += silverOff * 10;
                        }

                        if (BuyCopper > GameLoop.World.Player.CopperCoins) {
                            BuyCopper = GameLoop.World.Player.CopperCoins;
                        }

                    }

                    if (diff <= 0) {
                        if (mousePos.X >= DialogueConsole.Width - 8 && mousePos.X <= DialogueConsole.Width - 3 && mousePos.Y == DialogueConsole.Height - 3) {
                            GameLoop.World.Player.CopperCoins -= BuyCopper;
                            GameLoop.World.Player.SilverCoins -= BuySilver;
                            GameLoop.World.Player.GoldCoins -= BuyGold;
                            GameLoop.World.Player.JadeCoins -= BuyJade;

                            diff *= -1;
                            int plat = 0;
                            int gold = 0;
                            int silver = 0;

                            if (diff > 1000)
                                plat = diff / 1000;
                            diff -= plat * 1000;

                            if (diff > 100)
                                gold = diff / 100;
                            diff -= gold * 100;

                            if (diff > 10)
                                silver = diff / 10;
                            diff -= silver * 10;


                            GameLoop.World.Player.CopperCoins += diff;
                            GameLoop.World.Player.SilverCoins += silver;
                            GameLoop.World.Player.GoldCoins += gold;
                            GameLoop.World.Player.JadeCoins += plat;

                            for (int i = 0; i < SellingItems.Count; i++) {
                                for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                                    if (GameLoop.World.Player.Inventory[j].StacksWith(SellingItems[i], true)) {
                                        GameLoop.World.Player.Inventory[j].ItemQuantity -= SellingItems[i].ItemQuantity;
                                        if (GameLoop.World.Player.Inventory[j].ItemQuantity <= 0) {
                                            GameLoop.World.Player.Inventory[j] = new Item("lh:(EMPTY)");
                                        }
                                        break;
                                    }
                                }
                            }

                            for (int i = 0; i < BuyingItems.Count; i++) {
                                CommandManager.AddItemToInv(GameLoop.World.Player, BuyingItems[i]);
                            }

                            BuyCopper = 0;
                            BuySilver = 0;
                            BuyGold = 0;
                            BuyJade = 0;

                            BuyingItems.Clear();
                            SellingItems.Clear();
                        } else if (mousePos.X >= DialogueConsole.Width - 15 && mousePos.X <= DialogueConsole.Width - 3 && mousePos.Y == DialogueConsole.Height - 5) {
                            GameLoop.World.Player.CopperCoins -= BuyCopper;
                            GameLoop.World.Player.SilverCoins -= BuySilver;
                            GameLoop.World.Player.GoldCoins -= BuyGold;
                            GameLoop.World.Player.JadeCoins -= BuyJade;

                            diff *= -1;

                            if (diff >= 100) {
                                string reaction = DialogueNPC.ReactGift("-2");
                                if (DialogueNPC.GiftResponses.ContainsKey(reaction))
                                    dialogueLatest = DialogueNPC.GiftResponses[reaction];
                                else
                                    dialogueLatest = "Error - No response for " + reaction + " gift.";
                            } else if (diff >= 10) {
                                string reaction = DialogueNPC.ReactGift("-3");
                                if (DialogueNPC.GiftResponses.ContainsKey(reaction))
                                    dialogueLatest = DialogueNPC.GiftResponses[reaction];
                                else
                                    dialogueLatest = "Error - No response for " + reaction + " gift.";
                            }



                            for (int i = 0; i < SellingItems.Count; i++) {
                                for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                                    if (GameLoop.World.Player.Inventory[j].StacksWith(SellingItems[i], true)) {
                                        GameLoop.World.Player.Inventory[j].ItemQuantity -= SellingItems[i].ItemQuantity;
                                        if (GameLoop.World.Player.Inventory[j].ItemQuantity <= 0) {
                                            GameLoop.World.Player.Inventory[j] = new Item("lh:(EMPTY)");
                                        }
                                        break;
                                    }
                                }
                            }

                            for (int i = 0; i < BuyingItems.Count; i++) {
                                CommandManager.AddItemToInv(GameLoop.World.Player, BuyingItems[i]);
                            }

                            BuyCopper = 0;
                            BuySilver = 0;
                            BuyGold = 0;
                            BuyJade = 0;

                            BuyingItems.Clear();
                            SellingItems.Clear();
                            dialogueOption = "None";
                            BuyingFromShop = false;
                            return;
                        }
                    }

                    if (BuyingFromShop) {
                        if (mousePos.Y == DialogueConsole.Height - 6 && mousePos.X >= 1 && mousePos.X <= 16) {
                            BuyingFromShop = false;
                        } else {
                            int itemSlot = -1;
                            if ((float)((mousePos.Y - 5) / 2f) == (mousePos.Y - 5) / 2) {
                                itemSlot = (mousePos.Y - 5) / 2;
                            }

                            if (itemSlot >= 0)
                                itemSlot += shopTopIndex;

                            if (itemSlot >= 0 && itemSlot <= DialogueNPC.Shop.SoldItems.Count) {
                                if (mousePos.X == 0) {
                                    for (int i = 0; i < BuyingItems.Count; i++) {
                                        if (BuyingItems[i].FullName() == DialogueNPC.Shop.SoldItems[itemSlot]) {
                                            if (BuyingItems[i].IsStackable && BuyingItems[i].ItemQuantity > 1) {
                                                if (GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.RightShift)) {
                                                    if (BuyingItems[i].ItemQuantity >= 10) {
                                                        BuyingItems[i].ItemQuantity -= 10;
                                                    } else {
                                                        BuyingItems.RemoveAt(i);
                                                    }
                                                } else {
                                                    BuyingItems[i].ItemQuantity--;
                                                }
                                                break;
                                            } else if (!BuyingItems[i].IsStackable || BuyingItems[i].ItemQuantity == 1) {
                                                BuyingItems.RemoveAt(i);
                                                break;
                                            }
                                        }
                                    }
                                } else if (mousePos.X == 5) {
                                    bool alreadyInList = false;
                                    for (int i = 0; i < BuyingItems.Count; i++) {
                                        if (BuyingItems[i].FullName() == DialogueNPC.Shop.SoldItems[itemSlot]) {
                                            if (BuyingItems[i].IsStackable) {
                                                alreadyInList = true;
                                                if (GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.RightShift)) {
                                                    BuyingItems[i].ItemQuantity += 10;
                                                } else {
                                                    BuyingItems[i].ItemQuantity++;
                                                }
                                                break;
                                            } else if (!BuyingItems[i].IsStackable) {
                                                alreadyInList = true;
                                                BuyingItems.Add(new Item(DialogueNPC.Shop.SoldItems[itemSlot]));
                                                break;
                                            }
                                        }
                                    }

                                    if (!alreadyInList) {
                                        BuyingItems.Add(new Item(DialogueNPC.Shop.SoldItems[itemSlot]));

                                        if (GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.RightShift)) {
                                            if (BuyingItems[^1].IsStackable)
                                                BuyingItems[^1].ItemQuantity = 10;
                                        }
                                    }
                                }
                            }
                        }
                    } else {
                        if (mousePos.Y == DialogueConsole.Height - 6 && mousePos.X >= 1 && mousePos.X <= 11) {
                            BuyingFromShop = true;
                        } else {
                            int itemSlot = mousePos.Y - 5;
                            if (itemSlot >= 0 && itemSlot <= GameLoop.World.Player.Inventory.Length) {
                                if (mousePos.X == 0) {
                                    for (int i = 0; i < SellingItems.Count; i++) {
                                        if (SellingItems[i].StacksWith(GameLoop.World.Player.Inventory[itemSlot], true)) {
                                            if (SellingItems[i].IsStackable && SellingItems[i].ItemQuantity > 1) {
                                                SellingItems[i].ItemQuantity--;
                                                break;
                                            } else if (!SellingItems[i].IsStackable || SellingItems[i].ItemQuantity == 1) {
                                                SellingItems.RemoveAt(i);
                                                break;
                                            }
                                        }
                                    }
                                } else if (mousePos.X == 5) {
                                    bool alreadyInList = false;

                                    for (int i = 0; i < SellingItems.Count; i++) {
                                        int thisItemCount = 0;
                                        int alreadyInListCount = 0;
                                        if (SellingItems[i].StacksWith(GameLoop.World.Player.Inventory[itemSlot], true)) {
                                            if (SellingItems[i].IsStackable) {
                                                alreadyInList = true;
                                                if (SellingItems[i].ItemQuantity < GameLoop.World.Player.Inventory[itemSlot].ItemQuantity) {
                                                    SellingItems[i].ItemQuantity++;
                                                    break;
                                                }
                                            } else if (!SellingItems[i].IsStackable) {
                                                for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                                                    if (SellingItems[i].StacksWith(GameLoop.World.Player.Inventory[itemSlot], true)) {
                                                        thisItemCount++;
                                                    }
                                                }

                                                for (int j = 0; j < SellingItems.Count; j++) {
                                                    if (SellingItems[i].StacksWith(GameLoop.World.Player.Inventory[itemSlot], true)) {
                                                        alreadyInListCount++;
                                                    }
                                                }

                                                if (alreadyInListCount < thisItemCount) {
                                                    alreadyInList = true;
                                                    SellingItems.Add(new Item(GameLoop.World.Player.Inventory[itemSlot]));
                                                    break;
                                                } else {
                                                    alreadyInList = true;
                                                }
                                            }
                                        }
                                    }

                                    if (!alreadyInList) {
                                        SellingItems.Add(new Item(GameLoop.World.Player.Inventory[itemSlot]));
                                        SellingItems[^1].ItemQuantity = 1;
                                    }
                                }
                            }
                        }
                    }
                } else if (dialogueOption == "Gift") {
                    if (mousePos.Y == 4 + GameLoop.World.Player.Inventory.Length) {
                        dialogueOption = "None";
                    } else if (mousePos.Y >= 2 && mousePos.Y <= 2 + GameLoop.World.Player.Inventory.Length) {
                        int slot = mousePos.Y - 2;
                        string itemID = CommandManager.RemoveOneItem(GameLoop.World.Player, slot);

                        if (itemID != "" && itemID != "(EMPTY)") {
                            string reaction = DialogueNPC.ReactGift(itemID);
                            dialogueOption = "None";
                            if (DialogueNPC.GiftResponses.ContainsKey(reaction))
                                dialogueLatest = DialogueNPC.GiftResponses[reaction];
                            else
                                dialogueLatest = "Error - No response for " + reaction + " gift.";
                        }
                    }
                } else if (dialogueOption == "None") {
                    if (mousePos.Y == DialogueConsole.Height - 1) {
                        dialogueOption = "Goodbye";
                        if (DialogueNPC.Farewells.ContainsKey(DialogueNPC.RelationshipDescriptor())) {
                            dialogueLatest = DialogueNPC.Farewells[DialogueNPC.RelationshipDescriptor()];
                        } else {
                            dialogueLatest = "Error: Greeting not found for relationship " + DialogueNPC.RelationshipDescriptor();
                        }
                    } else if (mousePos.Y <= DialogueConsole.Height - 4 && mousePos.Y >= DialogueConsole.Height - 12) {
                        string chat = "";

                        if (mousePos.Y == DialogueConsole.Height - 12)
                            chat = DialogueNPC.ChitChats[chitChat1];
                        else if (mousePos.Y == DialogueConsole.Height - 10)
                            chat = DialogueNPC.ChitChats[chitChat2];
                        else if (mousePos.Y == DialogueConsole.Height - 8)
                            chat = DialogueNPC.ChitChats[chitChat3];
                        else if (mousePos.Y == DialogueConsole.Height - 6)
                            chat = DialogueNPC.ChitChats[chitChat4];
                        else if (mousePos.Y == DialogueConsole.Height - 4)
                            chat = DialogueNPC.ChitChats[chitChat5];

                        if (chat != "") {
                            string[] chatParts = chat.Split("~");

                            if (chatParts.Length == 2) {
                                dialogueLatest = chatParts[1];
                                GameLoop.World.Player.MetNPCs[DialogueNPC.Name] += Int32.Parse(chatParts[0]);
                                DialogueNPC.UpdateChitChats();
                            }
                        }
                    } else if (mousePos.Y == DialogueConsole.Height - 15) { // Give item
                        dialogueOption = "Gift";
                    } else if (mousePos.Y == DialogueConsole.Height - 17) { // Open shop dialogue
                        if (DialogueNPC.Shop != null && DialogueNPC.Shop.ShopOpen(DialogueNPC))
                            dialogueOption = "Shop";
                    } else if (mousePos.Y == DialogueConsole.Height - 19) { // Open mission dialogue
                        if (CurrentMission != null)
                            dialogueOption = "Mission";
                    }
                } else if (dialogueOption == "Mission") {
                    int topOption = DialogueConsole.Height - 18;

                    if ((float)((mousePos.Y - topOption) / 2f) == (mousePos.Y - topOption) / 2) {
                        int clickedResponse = (mousePos.Y - topOption) / 2;

                        if (CurrentMission != null) {
                            if (CurrentMission.Stages.Count > CurrentMission.CurrentStage) {
                                if (CurrentMission.Stages[CurrentMission.CurrentStage].Dialogue.ContainsKey(CurrentMission.Stages[CurrentMission.CurrentStage].CurrentDialogue)) {
                                    MissionDialogue dialogue = CurrentMission.Stages[CurrentMission.CurrentStage].Dialogue[CurrentMission.Stages[CurrentMission.CurrentStage].CurrentDialogue];

                                    if (dialogue.Responses.Count > clickedResponse) {
                                        if (dialogue.Responses[clickedResponse].MeetsAllRequirements(GameLoop.World.Player)) {
                                            CurrentMission.SelectChoice(dialogue.Responses[clickedResponse]);

                                            if (dialogue.Responses[clickedResponse].ItemGiven != "") {
                                                CommandManager.AddItemToInv(GameLoop.World.Player, new(dialogue.Responses[clickedResponse].ItemGiven));
                                            }

                                            if (dialogue.Responses[clickedResponse].EndsDialogue) {
                                                dialogueOption = "None";
                                                CurrentMission = null;
                                                if (GameLoop.World.Player.MetNPCs.ContainsKey(DialogueNPC.Name)) {
                                                    if (DialogueNPC.Greetings.ContainsKey(DialogueNPC.RelationshipDescriptor())) {
                                                        dialogueLatest = DialogueNPC.Greetings[DialogueNPC.RelationshipDescriptor()];
                                                    } else {
                                                        dialogueLatest = "Error: Greeting not found for relationship " + DialogueNPC.RelationshipDescriptor();
                                                    }
                                                } else {
                                                    dialogueLatest = GameLoop.UIManager.DialogueWindow.DialogueNPC.Introduction;
                                                    GameLoop.World.Player.MetNPCs.Add(DialogueNPC.Name, 0);
                                                }

                                                DialogueNPC.UpdateChitChats();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } 
    }
}
