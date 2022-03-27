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
using LofiHollow.DataTypes;


namespace LofiHollow.UI {
    public class UI_DialogueWindow : Lofi_UI{
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

        public int mostRecentChange = 0;

        public string dialogueOption = "None";
        public string dialogueLatest = "";
        public Mission CurrentMission;

        public NPC DialogueNPC = null;
        List<Item> TownHallPermits = new();

        public int shopTopIndex = 0; 

        public UI_DialogueWindow(int width, int height, string title) : base(width, height, title, "Dialogue") { }


        public void SetupDialogue(NPC npc) {
            DialogueNPC = npc;
            GameLoop.UIManager.selectedMenu = "Dialogue";
            Con.Clear();

            dialogueOption = "None";
            Win.IsVisible = true;

            if (!DialogueNPC.Animal) {
                foreach (KeyValuePair<string, Mission> kv in GameLoop.World.Player.MissionLog) {
                    if (kv.Value.CurrentStage < kv.Value.Stages.Count && kv.Value.Stages[kv.Value.CurrentStage] != null) {
                        if (kv.Value.Stages[kv.Value.CurrentStage].NPC == DialogueNPC.Name || kv.Value.Stages[kv.Value.CurrentStage].NPC == "Any") {
                            if (kv.Value.Stages[kv.Value.CurrentStage].AllRequirementsMet(GameLoop.World.Player)) {
                                CurrentMission = kv.Value;
                                break;
                            }
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

                if (DialogueNPC.RelationshipDescriptor() == "Nemesis" || DialogueNPC.RelationshipDescriptor() == "Hate")
                    mostRecentChange = -2;
                else if (DialogueNPC.RelationshipDescriptor() == "Unfriendly" || DialogueNPC.RelationshipDescriptor() == "Dislike")
                    mostRecentChange = -1;
                else if (DialogueNPC.RelationshipDescriptor() == "Neutral")
                    mostRecentChange = 0;
                else if (DialogueNPC.RelationshipDescriptor() == "Like" || DialogueNPC.RelationshipDescriptor() == "Friendly")
                    mostRecentChange = 1;
                else if (DialogueNPC.RelationshipDescriptor() == "Close Friend" || DialogueNPC.RelationshipDescriptor() == "Best Friend")
                    mostRecentChange = 2;
            } else {
               dialogueLatest = GameLoop.UIManager.DialogueWindow.DialogueNPC.Introduction;
                GameLoop.World.Player.MetNPCs.Add(DialogueNPC.Name, 0);
                mostRecentChange = 0;
            }

            DialogueNPC.UpdateChitChats();
        }


        public override void Render() {
            Con.Clear();
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            if (DialogueNPC != null) {
                Win.IsFocused = true;
                int opinion = 0;
                if (GameLoop.World.Player.MetNPCs.ContainsKey(DialogueNPC.Name))
                    opinion = GameLoop.World.Player.MetNPCs[DialogueNPC.Name];

                Win.Title = (DialogueNPC.Name + ", " + DialogueNPC.Occupation + " - " + DialogueNPC.RelationshipDescriptor() + " (" + opinion + ")").Align(HorizontalAlignment.Center, Win.Width - 2, (char)196);

                if (dialogueOption == "None" || dialogueOption == "Goodbye") {
                    if (dialogueLatest.Contains('@') && !GameLoop.World.Player.Name.Contains('@')) {
                        int index = dialogueLatest.IndexOf('@');
                        dialogueLatest = dialogueLatest.Remove(index, 1);
                        dialogueLatest = dialogueLatest.Insert(index, GameLoop.World.Player.Name);
                    }
                    string[] allLines = dialogueLatest.Split("|");

                    for (int i = 0; i < allLines.Length; i++)
                        Con.Print(1, 1 + i, new ColoredString(allLines[i], Color.White, Color.Black));
                }

                if (dialogueOption == "None") {

                    if (!DialogueNPC.Animal) {
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

                        if (DialogueNPC.Shop != null && DialogueNPC.Shop.ShopOpen(DialogueNPC))
                            Con.PrintClickable(1, Con.Height - 17, "[Open Store]", UI_Clicks, "OpenStore");

                        if (CurrentMission != null)
                            Con.PrintClickable(1, Con.Height - 19, "[Mission] " + CurrentMission.Name, UI_Clicks, "Mission");
                    }

                    Con.DrawLine(new Point(0, Con.Height - 20), new Point(Con.Width - 1, Con.Height - 20), 196, Color.White, Color.Black);

                    Con.PrintClickable(1, Con.Height - 15, "[Give Item]", UI_Clicks, "GiveItem");

                    Con.PrintClickable(1, Con.Height - 12, "Chit-chat: " + chitChat1, UI_Clicks, "ChitChat1");
                    Con.PrintClickable(1, Con.Height - 10, "Chit-chat: " + chitChat2, UI_Clicks, "ChitChat2");
                    Con.PrintClickable(1, Con.Height - 8, "Chit-chat: " + chitChat3, UI_Clicks, "ChitChat3");
                    Con.PrintClickable(1, Con.Height - 6, "Chit-chat: " + chitChat4, UI_Clicks, "ChitChat4");
                    Con.PrintClickable(1, Con.Height - 4, "Chit-chat: " + chitChat5, UI_Clicks, "ChitChat5");

                    Con.PrintClickable(1, Con.Height - 1, "Nevermind.", UI_Clicks, "Nevermind");


                    int faceX = 58;
                    int faceY = 8;
                    Helper.DrawBox(Con, faceX, faceY, 10, 10);


                    Con.Print(faceX + 3, faceY + 4, 254.AsString(), DialogueNPC.Appearance.Foreground);
                    Con.Print(faceX + 4, faceY + 4, 254.AsString(), DialogueNPC.Appearance.Foreground);
                    Con.Print(faceX + 3, faceY + 5, 254.AsString(), DialogueNPC.Appearance.Foreground);
                    Con.Print(faceX + 4, faceY + 5, 254.AsString(), DialogueNPC.Appearance.Foreground);

                    Con.Print(faceX + 8, faceY + 4, 254.AsString(), DialogueNPC.Appearance.Foreground);
                    Con.Print(faceX + 7, faceY + 4, 254.AsString(), DialogueNPC.Appearance.Foreground);
                    Con.Print(faceX + 8, faceY + 5, 254.AsString(), DialogueNPC.Appearance.Foreground);
                    Con.Print(faceX + 7, faceY + 5, 254.AsString(), DialogueNPC.Appearance.Foreground);


                    if (mostRecentChange == 0) { 
                        Con.DrawLine(new Point(faceX + 3, faceY + 8), new Point(faceX + 8, faceY + 8), 254, DialogueNPC.Appearance.Foreground);
                    }

                    else if (mostRecentChange == -1) {
                        Con.Print(faceX + 3, faceY + 9, 254.AsString(), DialogueNPC.Appearance.Foreground);
                        Con.Print(faceX + 8, faceY + 9, 254.AsString(), DialogueNPC.Appearance.Foreground);
                        Con.DrawLine(new Point(faceX + 4, faceY + 8), new Point(faceX + 7, faceY + 8), 254, DialogueNPC.Appearance.Foreground);
                    }

                    else if (mostRecentChange <= -2) {
                        Con.DrawLine(new Point(faceX + 3, faceY + 10), new Point(faceX + 8, faceY + 10), 254, DialogueNPC.Appearance.Foreground);
                        Con.Print(faceX + 3, faceY + 9, 254.AsString(), DialogueNPC.Appearance.Foreground);
                        Con.Print(faceX + 8, faceY + 9, 254.AsString(), DialogueNPC.Appearance.Foreground);
                        Con.DrawLine(new Point(faceX + 4, faceY + 8), new Point(faceX + 7, faceY + 8), 254, DialogueNPC.Appearance.Foreground);
                    }

                    else if (mostRecentChange == 1) {
                        Con.Print(faceX + 3, faceY + 8, 254.AsString(), DialogueNPC.Appearance.Foreground);
                        Con.Print(faceX + 8, faceY + 8, 254.AsString(), DialogueNPC.Appearance.Foreground);
                        Con.DrawLine(new Point(faceX + 4, faceY + 9), new Point(faceX + 7, faceY + 9), 254, DialogueNPC.Appearance.Foreground);
                    }

                    else if (mostRecentChange >= 2) { 
                        Con.DrawLine(new Point(faceX + 3, faceY + 8), new Point(faceX + 8, faceY + 8), 254, DialogueNPC.Appearance.Foreground);
                        Con.Print(faceX + 3, faceY + 9, 254.AsString(), DialogueNPC.Appearance.Foreground);
                        Con.Print(faceX + 8, faceY + 9, 254.AsString(), DialogueNPC.Appearance.Foreground);
                        Con.DrawLine(new Point(faceX + 4, faceY + 10), new Point(faceX + 7, faceY + 10), 254, DialogueNPC.Appearance.Foreground);
                    }
                } else if (dialogueOption == "Goodbye") {
                    Con.Print(1, Con.Height - 1, Helper.HoverColoredString("[Click anywhere to close]", mousePos.Y == Con.Height - 1));
                } else if (dialogueOption == "Gift") {
                    int y = 1;
                    Con.Print(1, y++, "Give what?");

                    for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                        Item item = GameLoop.World.Player.Inventory[i];

                        string nameWithDurability = item.Name;

                        if (item.Durability >= 0)
                            nameWithDurability = "[" + item.Durability + "] " + item.Name;

                        Con.Print(1, y, item.AsColoredGlyph());
                        if (item.Dec != null) {
                            Con.SetDecorator(1, y, 1, new CellDecorator(new Color(item.Dec.R, item.Dec.G, item.Dec.B), item.Dec.Glyph, Mirror.None));
                        }

                        if (item.Name == "(EMPTY)") {
                            Con.Print(3, y, new ColoredString(nameWithDurability, Color.DarkSlateGray, Color.Black));
                        } else {
                            if (!item.IsStackable || (item.IsStackable && item.ItemQuantity == 1))
                                Con.PrintClickable(3, y, nameWithDurability, UI_Clicks, "GiftSelect," + i);
                            else
                                Con.PrintClickable(3, y, "(" + item.ItemQuantity + ") " + item.Name, UI_Clicks, "GiftSelect," + i); 
                        } 

                        y++;
                    }

                    Con.PrintClickable(1, y + 2, "Nevermind.", UI_Clicks, "GiftCancel"); 
                } 
                else if (dialogueOption == "Shop") {

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



                    Con.Print(1, 0, GameLoop.World.Player.Name);
                    Con.Print(1, 1, playerMoney);
                    Con.Print(Con.Width - (shopHeader.Length + 1), 0, shopHeader);
                    Con.DrawLine(new Point(0, 2), new Point(Con.Width - 1, 2), 196);
                    if (BuyingFromShop) {
                        Con.Print(0, 3, " Buy  |" + "Item Name".Align(HorizontalAlignment.Center, 23) + "|" + "Short Description".Align(HorizontalAlignment.Center, 27) + "|" + "Price".Align(HorizontalAlignment.Center, 11));
                        Con.DrawLine(new Point(0, 4), new Point(Con.Width - 1, 4), 196);

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
                                Item item = Item.Copy(DialogueNPC.Shop.SoldItems[j]);
                                int price = DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], item, false);

                                int buyQuantity = 0;

                                for (int k = 0; k < BuyingItems.Count; k++) {
                                    if (BuyingItems[k].StacksWith(item, true)) {
                                        buyQuantity += BuyingItems[k].ItemQuantity;
                                    }
                                }

                                Con.PrintClickable(0, 5 + (2 * i), "-", UI_Clicks, "ShopAddItem," + j);
                                Con.Print(1, 5 + (2 * i), new ColoredString(buyQuantity.ToString().Align(HorizontalAlignment.Center, 3), Color.White, Color.Black));
                                Con.PrintClickable (5, 5 + (2 * i), "+", UI_Clicks, "ShopRemoveItem," + j);
                                Con.Print(6, 5 + (2 * i), Helper.HoverColoredString("|" + item.Name.Align(HorizontalAlignment.Center, 23) + "|" + item.ShortDesc.Align(HorizontalAlignment.Center, 27) + "|", mousePos.Y == 5 + (2 *i)));
                                Con.Print(Con.Width - 10, 5 + (2 * i), Helper.ConvertCoppers(price));
                                Con.DrawLine(new Point(0, 6 + (2 * i)), new Point(Con.Width, 6 + (2 * i)), '-', Color.White, Color.Black);
                            }
                        }

                        Con.DrawLine(new Point(0, Con.Height - 7), new Point(Con.Width - 1, Con.Height - 7), 196);
                        Con.PrintClickable(1, Con.Height - 6, "[View Inventory]", UI_Clicks, "ShopShowInv");
                    } else {
                        Con.Print(0, 3, " Sell |" + "Item Name".Align(HorizontalAlignment.Center, 23) + "|" + "Short Description".Align(HorizontalAlignment.Center, 27) + "|" + "Price".Align(HorizontalAlignment.Center, 11));
                        Con.DrawLine(new Point(0, 4), new Point(Con.Width - 1, 4), 196);


                        for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                            Item item = Item.Copy(GameLoop.World.Player.Inventory[i]);
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

                            if (item.Name != "(EMPTY)") {
                                Con.PrintClickable(0, 5 + i, "-", UI_Clicks, "InvAddItem," + i);
                                Con.Print(1, 5 + i, new ColoredString(sellQuantity.ToString().Align(HorizontalAlignment.Center, 3), Color.White, Color.Black));
                                Con.PrintClickable(5, 5 + i, "+", UI_Clicks, "InvRemoveItem," + i);
                                Con.Print(6, 5 + i, Helper.HoverColoredString("|" + name.Align(HorizontalAlignment.Center, 23) + "|" + item.ShortDesc.Align(HorizontalAlignment.Center, 27) + "|", mousePos.Y == 5 + i));
                                Con.Print(Con.Width - 10, 5 + i, Helper.ConvertCoppers(price));
                            } else {
                                Con.Print(6, 5 + i, item.Name.Align(HorizontalAlignment.Center, 23), Color.DarkSlateGray);
                            }
                        }

                        Con.DrawLine(new Point(0, Con.Height - 7), new Point(Con.Width - 1, Con.Height - 7), 196);
                        Con.PrintClickable(1, Con.Height - 6, "[View Shop]", UI_Clicks, "ShopShowShop");
                    }

                    int buyValue = 0;

                    for (int j = 0; j < BuyingItems.Count; j++) {
                        buyValue += BuyingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], BuyingItems[j], false);
                    }

                    int sellValue = 0;

                    for (int j = 0; j < SellingItems.Count; j++) {
                        sellValue += SellingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], SellingItems[j], true);
                    }

                    Con.Print(28, Con.Height - 6, "Coins");

                    ColoredString buyCopperString = new(BuyCopper.ToString(), new Color(184, 115, 51), Color.Black);
                    ColoredString buySilverString = new(BuySilver.ToString(), Color.Silver, Color.Black);
                    ColoredString buyGoldString = new(BuyGold.ToString(), Color.Yellow, Color.Black);
                    ColoredString buyJadeString = new(BuyJade.ToString(), new Color(0, 168, 107), Color.Black);

                    Con.Print(30, Con.Height - 5, buyCopperString);
                    Con.Print(30, Con.Height - 4, buySilverString);
                    Con.Print(30, Con.Height - 3, buyGoldString);
                    Con.Print(30, Con.Height - 2, buyJadeString);

                    Con.PrintClickable(28, Con.Height - 5, "-", UI_Clicks, "CopperSub");
                    Con.PrintClickable(28, Con.Height - 4, "-", UI_Clicks, "SilverSub");
                    Con.PrintClickable(28, Con.Height - 3, "-", UI_Clicks, "GoldSub");
                    Con.PrintClickable(28, Con.Height - 2, "-", UI_Clicks, "JadeSub");

                    Con.PrintClickable(32, Con.Height - 5, "+", UI_Clicks, "CopperAdd");
                    Con.PrintClickable(32, Con.Height - 4, "+", UI_Clicks, "SilverAdd");
                    Con.PrintClickable(32, Con.Height - 3, "+", UI_Clicks, "GoldAdd");
                    Con.PrintClickable(32, Con.Height - 2, "+", UI_Clicks, "JadeAdd");
                    sellValue += BuyCopper;
                    sellValue += BuySilver * 100;
                    sellValue += BuyGold * 10000;
                    sellValue += BuyJade * 1000000;

                    Con.PrintClickable(27, Con.Height - 1, "[EXACT]", UI_Clicks, "ShopExact");

                    Con.Print(1, Con.Height - 4, new ColoredString("Buy Value: ", Color.White, Color.Black) + Helper.ConvertCoppers(buyValue));
                    Con.Print(1, Con.Height - 3, new ColoredString("Sell Value: ", Color.White, Color.Black) + Helper.ConvertCoppers(sellValue));

                    int diff = buyValue - sellValue;

                    string total = diff > 0 ? "You owe " : diff == 0 ? "Trade is equal" : "You are owed ";

                    if (diff < 0)
                        Con.Print(1, Con.Height - 2, new ColoredString(total, Color.White, Color.Black) + Helper.ConvertCoppers(-diff));
                    else if (diff > 0)
                        Con.Print(1, Con.Height - 2, new ColoredString(total, Color.White, Color.Black) + Helper.ConvertCoppers(diff));
                    else
                        Con.Print(1, Con.Height - 2, new ColoredString(total, Color.White, Color.Black));

                    if (diff <= 0)
                        Con.PrintClickable(Con.Width - 15, Con.Height - 5, new ColoredString("[Accept - Gift]", Color.Green, Color.Black), UI_Clicks, "ShopAcceptGift"); 
                    else
                        Con.Print(Con.Width - 15, Con.Height - 5, new ColoredString("[Accept - Gift]", Color.Red, Color.Black));


                    if (diff <= 0)
                        Con.PrintClickable(Con.Width - 8, Con.Height - 3, new ColoredString("[Accept]", Color.Green, Color.Black), UI_Clicks, "ShopAccept");
                    else
                        Con.Print(Con.Width - 8, Con.Height - 3, new ColoredString("[Accept]", Color.Red, Color.Black));

                    Con.PrintClickable(Con.Width - 12, Con.Height - 1, "[Close Shop]", UI_Clicks, "ShopCancel");

                } 
                else if (dialogueOption == "Mission") {
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
                                    Con.Print(1, 1 + i, new ColoredString(allLines[i], Color.White, Color.Black));

                                Con.DrawLine(new Point(0, Con.Height - 20), new Point(Con.Width - 1, Con.Height - 20), 196, Color.White, Color.Black);
                                
                                for (int i = 0; i < dialogue.Responses.Count; i++) {
                                    Con.PrintClickable(1, (Con.Height - 18) + (i * 2), Helper.RequirementString(dialogue.Responses[i].Text, mousePos.Y == (Con.Height - 18) + (i * 2), dialogue.Responses[i].MeetsAllRequirements(GameLoop.World.Player)), UI_Clicks, "MissionDialogue," + i);
                                  //  DialogueConsole.Print(1, (DialogueConsole.Height - 18) + (i * 2), Helper.RequirementString(dialogue.Responses[i].Text, mousePos.Y == (DialogueConsole.Height - 18) + (i * 2), dialogue.Responses[i].MeetsAllRequirements(GameLoop.World.Player)));
                                } 
                            }
                        } 
                    } 
                } 
            }
        }

        public override void UI_Clicks(string ID) {
            if (ID == "ChitChat1") {
                string chat = DialogueNPC.ChitChats[chitChat1];

                if (chat != "") {
                    string[] chatParts = chat.Split("~");

                    if (chatParts.Length == 2) {
                        dialogueLatest = chatParts[1];
                        mostRecentChange = Int32.Parse(chatParts[0]);
                        int newRel = Math.Clamp(GameLoop.World.Player.MetNPCs[DialogueNPC.Name] + Int32.Parse(chatParts[0]), 0, 100);
                        GameLoop.World.Player.MetNPCs[DialogueNPC.Name] = newRel;
                        DialogueNPC.UpdateChitChats();
                        GameLoop.SteamManager.CountSocials();
                    }
                }
            }
            else if (ID == "ChitChat2") {
                string chat = DialogueNPC.ChitChats[chitChat2];

                if (chat != "") {
                    string[] chatParts = chat.Split("~");

                    if (chatParts.Length == 2) {
                        dialogueLatest = chatParts[1];
                        mostRecentChange = Int32.Parse(chatParts[0]);
                        int newRel = Math.Clamp(GameLoop.World.Player.MetNPCs[DialogueNPC.Name] + Int32.Parse(chatParts[0]), 0, 100);
                        GameLoop.World.Player.MetNPCs[DialogueNPC.Name] = newRel;
                        DialogueNPC.UpdateChitChats();
                        GameLoop.SteamManager.CountSocials();
                    }
                }
            }
            else if (ID == "ChitChat3") {
                string chat = DialogueNPC.ChitChats[chitChat3];

                if (chat != "") {
                    string[] chatParts = chat.Split("~");

                    if (chatParts.Length == 2) {
                        dialogueLatest = chatParts[1];
                        mostRecentChange = Int32.Parse(chatParts[0]);
                        int newRel = Math.Clamp(GameLoop.World.Player.MetNPCs[DialogueNPC.Name] + Int32.Parse(chatParts[0]), 0, 100);
                        GameLoop.World.Player.MetNPCs[DialogueNPC.Name] = newRel;
                        DialogueNPC.UpdateChitChats();
                        GameLoop.SteamManager.CountSocials();
                    }
                }
            }
            else if (ID == "ChitChat4") {
                string chat = DialogueNPC.ChitChats[chitChat4];

                if (chat != "") {
                    string[] chatParts = chat.Split("~");

                    if (chatParts.Length == 2) {
                        dialogueLatest = chatParts[1];
                        mostRecentChange = Int32.Parse(chatParts[0]);
                        int newRel = Math.Clamp(GameLoop.World.Player.MetNPCs[DialogueNPC.Name] + Int32.Parse(chatParts[0]), 0, 100);
                        GameLoop.World.Player.MetNPCs[DialogueNPC.Name] = newRel;
                        DialogueNPC.UpdateChitChats();
                        GameLoop.SteamManager.CountSocials();
                    }
                }
            }
            else if (ID == "ChitChat5") {
                string chat = DialogueNPC.ChitChats[chitChat5];

                if (chat != "") {
                    string[] chatParts = chat.Split("~");

                    if (chatParts.Length == 2) {
                        dialogueLatest = chatParts[1];
                        mostRecentChange = Int32.Parse(chatParts[0]);
                        int newRel = Math.Clamp(GameLoop.World.Player.MetNPCs[DialogueNPC.Name] + Int32.Parse(chatParts[0]), 0, 100);
                        GameLoop.World.Player.MetNPCs[DialogueNPC.Name] = newRel;
                        DialogueNPC.UpdateChitChats();
                        GameLoop.SteamManager.CountSocials();
                    }
                }
            }
            else if (ID == "Nevermind") {
                dialogueOption = "Goodbye";
                if (DialogueNPC.Farewells.ContainsKey(DialogueNPC.RelationshipDescriptor())) {
                    dialogueLatest = DialogueNPC.Farewells[DialogueNPC.RelationshipDescriptor()];
                }
                else {
                    dialogueLatest = "Error: Greeting not found for relationship " + DialogueNPC.RelationshipDescriptor();
                }
            }
            else if (ID == "GiveItem") {
                dialogueOption = "Gift";
            }
            else if (ID == "Mission") {
                if (CurrentMission != null)
                    dialogueOption = "Mission";
            }
            else if (ID == "OpenStore") {
                if (DialogueNPC.Shop != null && DialogueNPC.Shop.ShopOpen(DialogueNPC))
                    dialogueOption = "Shop";
            }
            else if (ID == "GiftCancel") {
                dialogueOption = "None";
            }
            else if (ID.Contains("GiftSelect")) {
                int slot = Int32.Parse(ID.Split(",")[1]);
                string itemID = CommandManager.RemoveOneItem(GameLoop.World.Player, slot);

                if (itemID != "" && itemID != "(EMPTY)") {
                    string reaction = DialogueNPC.ReactGift(itemID);
                    dialogueOption = "None";
                    if (DialogueNPC.GiftResponses.ContainsKey(reaction))
                        dialogueLatest = DialogueNPC.GiftResponses[reaction];
                    else
                        dialogueLatest = "Error - No response for " + reaction + " gift.";
                    GameLoop.SteamManager.CountSocials();
                }
            }
            else if (ID.Contains("MissionDialogue")) {
                int clickedResponse = Int32.Parse(ID.Split(",")[1]);

                if (CurrentMission != null) {
                    if (CurrentMission.Stages.Count > CurrentMission.CurrentStage) {
                        if (CurrentMission.Stages[CurrentMission.CurrentStage].Dialogue.ContainsKey(CurrentMission.Stages[CurrentMission.CurrentStage].CurrentDialogue)) {
                            MissionDialogue dialogue = CurrentMission.Stages[CurrentMission.CurrentStage].Dialogue[CurrentMission.Stages[CurrentMission.CurrentStage].CurrentDialogue];

                            if (dialogue.Responses.Count > clickedResponse) {
                                if (dialogue.Responses[clickedResponse].MeetsAllRequirements(GameLoop.World.Player)) {
                                    CurrentMission.SelectChoice(dialogue.Responses[clickedResponse]);

                                    if (dialogue.Responses[clickedResponse].ItemGiven != "") {
                                        CommandManager.AddItemToInv(GameLoop.World.Player, Item.Copy(dialogue.Responses[clickedResponse].ItemGiven));
                                    }

                                    if (dialogue.Responses[clickedResponse].EndsDialogue) {
                                        dialogueOption = "None";
                                        CurrentMission = null;
                                        if (GameLoop.World.Player.MetNPCs.ContainsKey(DialogueNPC.Name)) {
                                            if (DialogueNPC.Greetings.ContainsKey(DialogueNPC.RelationshipDescriptor())) {
                                                dialogueLatest = DialogueNPC.Greetings[DialogueNPC.RelationshipDescriptor()];
                                            }
                                            else {
                                                dialogueLatest = "Error: Greeting not found for relationship " + DialogueNPC.RelationshipDescriptor();
                                            }
                                        }
                                        else {
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
            else if (ID == "CopperSub") { if (BuyCopper > 0) BuyCopper--; }
            else if (ID == "CopperAdd") { if (BuyCopper < GameLoop.World.Player.CopperCoins) BuyCopper++; }
            else if (ID == "SilverSub") { if (BuySilver > 0) BuySilver--; }
            else if (ID == "SilverAdd") { if (BuySilver < GameLoop.World.Player.SilverCoins) BuySilver++; }
            else if (ID == "GoldSub") { if (BuyGold > 0) BuyGold--; }
            else if (ID == "GoldAdd") { if (BuyGold < GameLoop.World.Player.GoldCoins) BuyGold++; }
            else if (ID == "JadeSub") { if (BuyJade > 0) BuyJade--; }
            else if (ID == "JadeAdd") { if (BuyJade < GameLoop.World.Player.JadeCoins) BuyJade++; }
            else if (ID == "ShopCancel") {
                dialogueOption = "None";
                BuyingItems.Clear();
                SellingItems.Clear();
                BuyingFromShop = true;
            }
            else if (ID == "ShopAcceptGift") {
                int buyValue = 0;
                for (int j = 0; j < BuyingItems.Count; j++) {
                    buyValue += BuyingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], BuyingItems[j], false);
                }

                int sellValue = 0;
                for (int j = 0; j < SellingItems.Count; j++) {
                    sellValue += SellingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], SellingItems[j], true);
                }

                sellValue += BuyCopper;
                sellValue += BuySilver * 100;
                sellValue += BuyGold * 10000;
                sellValue += BuyJade * 1000000;

                int diff = buyValue - sellValue;

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
                    GameLoop.SteamManager.CountSocials();
                }
                else if (diff >= 10) {
                    string reaction = DialogueNPC.ReactGift("-3");
                    if (DialogueNPC.GiftResponses.ContainsKey(reaction))
                        dialogueLatest = DialogueNPC.GiftResponses[reaction];
                    else
                        dialogueLatest = "Error - No response for " + reaction + " gift.";
                    GameLoop.SteamManager.CountSocials();
                }



                for (int i = 0; i < SellingItems.Count; i++) {
                    for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                        if (GameLoop.World.Player.Inventory[j].StacksWith(SellingItems[i], true)) {
                            GameLoop.World.Player.Inventory[j].ItemQuantity -= SellingItems[i].ItemQuantity;
                            if (GameLoop.World.Player.Inventory[j].ItemQuantity <= 0) {
                                GameLoop.World.Player.Inventory[j] = Item.Copy("lh:(EMPTY)");
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
            }
            else if (ID == "ShopExact") {
                int buyValue = 0;
                for (int j = 0; j < BuyingItems.Count; j++) {
                    buyValue += BuyingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], BuyingItems[j], false);
                }

                int sellValue = 0;
                for (int j = 0; j < SellingItems.Count; j++) {
                    sellValue += SellingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], SellingItems[j], true);
                } 

                int diff = buyValue - sellValue;

                if (diff > 1000000) {
                    int jadeNeeded = diff / 1000000;
                    if (GameLoop.World.Player.JadeCoins >= jadeNeeded) {
                        BuyJade = jadeNeeded;
                    } else {
                        BuyJade = GameLoop.World.Player.JadeCoins;
                    }

                    diff -= BuyJade * 1000000;
                }

                if (diff > 10000) {
                    int goldNeeded = diff / 10000;

                    if (GameLoop.World.Player.GoldCoins >= goldNeeded)
                        BuyGold = goldNeeded;
                    else
                        BuyGold = GameLoop.World.Player.GoldCoins;

                    diff -= BuyGold * 10000;
                }

                if (diff > 100) {
                    int silverNeeded = diff / 100;

                    if (GameLoop.World.Player.SilverCoins >= silverNeeded)
                        BuySilver = silverNeeded;
                    else
                        BuySilver = GameLoop.World.Player.SilverCoins;

                    diff -= BuySilver * 100;
                }

                if (GameLoop.World.Player.CopperCoins >= diff)
                    BuyCopper = diff;
                else
                    BuyCopper = GameLoop.World.Player.CopperCoins;
            }
            else if (ID == "ShopAccept") {
                int buyValue = 0;
                for (int j = 0; j < BuyingItems.Count; j++) {
                    buyValue += BuyingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], BuyingItems[j], false);
                }

                int sellValue = 0;
                for (int j = 0; j < SellingItems.Count; j++) {
                    sellValue += SellingItems[j].ItemQuantity * DialogueNPC.Shop.GetPrice(GameLoop.World.Player.MetNPCs[DialogueNPC.Name], SellingItems[j], true);
                }

                sellValue += BuyCopper;
                sellValue += BuySilver * 100;
                sellValue += BuyGold * 10000;
                sellValue += BuyJade * 1000000;

                int diff = buyValue - sellValue;

                GameLoop.World.Player.CopperCoins -= BuyCopper;
                GameLoop.World.Player.SilverCoins -= BuySilver;
                GameLoop.World.Player.GoldCoins -= BuyGold;
                GameLoop.World.Player.JadeCoins -= BuyJade;

                diff *= -1;
                int plat = 0;
                int gold = 0;
                int silver = 0;

                if (diff > 1000000)
                    plat = diff / 1000000;
                diff -= plat * 1000000;

                if (diff > 10000)
                    gold = diff / 10000;
                diff -= gold * 10000;

                if (diff > 10)
                    silver = diff / 100;
                diff -= silver * 100;


                GameLoop.World.Player.CopperCoins += diff;
                GameLoop.World.Player.SilverCoins += silver;
                GameLoop.World.Player.GoldCoins += gold;
                GameLoop.World.Player.JadeCoins += plat;

                for (int i = 0; i < SellingItems.Count; i++) {
                    for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                        if (GameLoop.World.Player.Inventory[j].StacksWith(SellingItems[i], true)) {
                            GameLoop.World.Player.Inventory[j].ItemQuantity -= SellingItems[i].ItemQuantity;
                            if (GameLoop.World.Player.Inventory[j].ItemQuantity <= 0) {
                                GameLoop.World.Player.Inventory[j] = Item.Copy("lh:(EMPTY)");
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
            }
            else if (ID == "ShopShowInv") { BuyingFromShop = false; }
            else if (ID == "ShopShowShop") { BuyingFromShop = true; }
            else if (ID.Contains("ShopAddItem")) {
                int itemSlot = Int32.Parse(ID.Split(",")[1]);

                for (int i = 0; i < BuyingItems.Count; i++) {
                    if (BuyingItems[i].FullName() == DialogueNPC.Shop.SoldItems[itemSlot]) {
                        if (BuyingItems[i].IsStackable && BuyingItems[i].ItemQuantity > 1) {
                            if (GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.RightShift)) {
                                if (BuyingItems[i].ItemQuantity >= 10) {
                                    BuyingItems[i].ItemQuantity -= 10;
                                }
                                else {
                                    BuyingItems.RemoveAt(i);
                                }
                            }
                            else {
                                BuyingItems[i].ItemQuantity--;
                            }
                            break;
                        }
                        else if (!BuyingItems[i].IsStackable || BuyingItems[i].ItemQuantity == 1) {
                            BuyingItems.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            else if (ID.Contains("ShopRemoveItem")) {
                int itemSlot = Int32.Parse(ID.Split(",")[1]);

                bool alreadyInList = false;
                for (int i = 0; i < BuyingItems.Count; i++) {
                    if (BuyingItems[i].FullName() == DialogueNPC.Shop.SoldItems[itemSlot]) {
                        if (BuyingItems[i].IsStackable) {
                            alreadyInList = true;
                            if (GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.RightShift)) {
                                BuyingItems[i].ItemQuantity += 10;
                            }
                            else {
                                BuyingItems[i].ItemQuantity++;
                            }
                            break;
                        }
                        else if (!BuyingItems[i].IsStackable) {
                            alreadyInList = true;
                            BuyingItems.Add(Item.Copy(DialogueNPC.Shop.SoldItems[itemSlot]));
                            break;
                        }
                    }
                }

                if (!alreadyInList) {
                    BuyingItems.Add(Item.Copy(DialogueNPC.Shop.SoldItems[itemSlot]));

                    if (GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(SadConsole.Input.Keys.RightShift)) {
                        if (BuyingItems[^1].IsStackable)
                            BuyingItems[^1].ItemQuantity = 10;
                    }
                }
            }
            else if (ID.Contains("InvAddItem")) {
                int itemSlot = Int32.Parse(ID.Split(",")[1]);

                for (int i = 0; i < SellingItems.Count; i++) {
                    if (SellingItems[i].StacksWith(GameLoop.World.Player.Inventory[itemSlot], true)) {
                        if (SellingItems[i].IsStackable && SellingItems[i].ItemQuantity > 1) {
                            SellingItems[i].ItemQuantity--;
                            break;
                        }
                        else if (!SellingItems[i].IsStackable || SellingItems[i].ItemQuantity == 1) {
                            SellingItems.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            else if (ID.Contains("InvRemoveItem")) {
                int itemSlot = Int32.Parse(ID.Split(",")[1]);

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
                        }
                        else if (!SellingItems[i].IsStackable) {
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
                                SellingItems.Add(Item.Copy(GameLoop.World.Player.Inventory[itemSlot]));
                                break;
                            }
                            else {
                                alreadyInList = true;
                            }
                        }
                    }
                }

                if (!alreadyInList) {
                    SellingItems.Add(Item.Copy(GameLoop.World.Player.Inventory[itemSlot]));
                    SellingItems[^1].ItemQuantity = 1;
                }
            }
        }

        public override void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

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
            
            if (GameHost.Instance.Keyboard.IsKeyPressed(Keys.Escape)) {
                if (dialogueOption == "Goodbye") {
                    dialogueOption = "None";
                    GameLoop.UIManager.selectedMenu = "None";
                    Win.IsVisible = false;
                    DialogueNPC = null;
                    dialogueLatest = "";
                    Con.Clear();
                } else if (dialogueOption == "Shop") {
                    dialogueOption = "None";
                    BuyingItems.Clear();
                    SellingItems.Clear();
                    BuyingFromShop = true;
                } else if (dialogueOption == "None") {
                    dialogueOption = "Goodbye";
                }
            }


            if (GameHost.Instance.Mouse.LeftClicked) {
                if (dialogueOption == "Goodbye") {
                    dialogueOption = "None";
                    GameLoop.UIManager.selectedMenu = "None";
                    Win.IsVisible = false;
                    DialogueNPC = null;
                    dialogueLatest = "";
                    Con.Clear();
                }
            }
        } 
    }
}
