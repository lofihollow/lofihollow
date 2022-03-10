using LofiHollow.DataTypes;
using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using LofiHollow.Minigames.Archaeology;
using LofiHollow.Minigames.Photo;
using LofiHollow.UI;
using SadConsole;
using SadRogue.Primitives;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LofiHollow {
    public class ItemHandler {
        public static void ConsumeItem() {
            CommandManager.RemoveOneItem(GameLoop.World.Player, GameLoop.UIManager.Sidebar.hotbarSelect);
        }

        // Single click
        public static void UseItem(Item item, Point Pos, Point3D MapPos, int distance) {
            Map map = Helper.ResolveMap(MapPos);

            if (map != null && Pos.X >= 0 && Pos.X <= GameLoop.MapWidth && Pos.Y >= 0 && Pos.Y <= GameLoop.MapHeight) {
                Tile tile = map.GetTile(Pos);

                if (tile.MiscString != "") {
                    string[] split = tile.MiscString.Split(",");
                    if (split[0] == "Skill") {
                        if (split.Length > 2)
                            GameLoop.UIManager.Crafting.SetupCrafting(split[1], split[2], Int32.Parse(split[3]));
                        else if (split.Length > 1)
                            GameLoop.UIManager.Crafting.SetupCrafting(split[1], split[2], 1);
                        else
                            GameLoop.UIManager.Crafting.SetupCrafting(split[1], "None", 1);
                    }
                }

                if (tile.Lock != null) {
                    map.ToggleLock(Pos, MapPos);
                }

                if (item.ItemCat == "Inn Voucher") {
                    GameLoop.World.Player.InnDays++;
                    if (GameLoop.World.Player.InnDays == 1)
                        GameLoop.UIManager.AddMsg(new ColoredString("You can sleep in the inn for a night!", Color.Lime, Color.Black));
                    else
                        GameLoop.UIManager.AddMsg(new ColoredString("You can sleep in the inn for " + GameLoop.World.Player.InnDays + " nights!", Color.Lime, Color.Black));
                    ConsumeItem();
                }

                if (tile.AnimalBed == null && item.ItemCat == "AnimalBed") {
                    if (Pos.X >= 1 && Pos.Y >= 1 && Pos.X < GameLoop.MapWidth && Pos.Y < GameLoop.MapHeight) {
                        string location = map.MinimapTile.name;

                        if (BuildManager.CanBuildHere(location, false)) {
                            if (tile.Name.ToLower().Contains("floor") || tile.Name.ToLower().Contains("grass")) {
                                map.GetTile(Pos).AnimalBed = new(Pos, GameLoop.World.Player.MapPos);
                                map.GetTile(Pos).Name = "Animal Bed";
                                map.GetTile(Pos).TileGlyph = 240;
                                map.GetTile(Pos).ForegroundR = 165;
                                map.GetTile(Pos).ForegroundG = 127;
                                map.GetTile(Pos).ForegroundB = 0;
                                map.GetTile(Pos).UpdateAppearance();
                                ConsumeItem();

                                NetMsg updateTile = new("updateTile", map.GetTile(Pos).ToByteArray());
                                updateTile.SetFullPos(Pos, MapPos);
                                GameLoop.SendMessageIfNeeded(updateTile, false, false);
                            }
                        }
                    }
                }

                if (tile.AnimalBed == null && item.ItemCat == "PetBed") {
                    if (Pos.X >= 1 && Pos.Y >= 1 && Pos.X < GameLoop.MapWidth && Pos.Y < GameLoop.MapHeight) {
                        string location = map.MinimapTile.name;

                        if (BuildManager.CanBuildHere(location, true)) {
                            if (tile.Name.ToLower().Contains("floor") || tile.Name.ToLower().Contains("grass")) {
                                map.GetTile(Pos).AnimalBed = new(Pos, GameLoop.World.Player.MapPos);
                                map.GetTile(Pos).Name = "Pet Bed";
                                map.GetTile(Pos).TileGlyph = 240;
                                map.GetTile(Pos).ForegroundR = 134;
                                map.GetTile(Pos).ForegroundG = 36;
                                map.GetTile(Pos).ForegroundB = 42;
                                map.GetTile(Pos).UpdateAppearance();
                                ConsumeItem();

                                NetMsg updateTile = new("updateTile", map.GetTile(Pos).ToByteArray());
                                updateTile.SetFullPos(Pos, MapPos);
                                GameLoop.SendMessageIfNeeded(updateTile, false, false);
                            }
                        }
                    }
                }

                if (item.ItemCat == "Shears") {
                    FarmAnimal animal = map.GetEntityAt<FarmAnimal>(Pos);
                    if (animal != null) {
                        if (animal.Shearable()) {
                            animal.ShearedToday = true;
                            CommandManager.AddItemToInv(GameLoop.World.Player, Item.Copy(animal.ShearItem));

                            Point bed = animal.RestSpot;
                            NetMsg updateTile = new("updateTile", map.GetTile(bed).ToByteArray());
                            updateTile.SetFullPos(bed, MapPos);
                            GameLoop.SendMessageIfNeeded(updateTile, false, false);
                        } else {
                            if (animal.ShearItem == "")
                                GameLoop.UIManager.AddMsg("The " + animal.Species + " can't be sheared.");
                            else if (animal.ShearedToday)
                                GameLoop.UIManager.AddMsg("You already sheared that " + animal.Species + " today!");
                            else if (animal.Age < animal.AdultAge)
                                GameLoop.UIManager.AddMsg("The " + animal.Species + " isn't old enough to be sheared.");
                            else if (animal.Happiness < 50)
                                GameLoop.UIManager.AddMsg("The " + animal.Species + " isn't happy enough to be sheared."); 
                        }
                    }
                }

                if (item.ItemCat == "MilkBucket") {
                    FarmAnimal animal = map.GetEntityAt<FarmAnimal>(Pos);
                    if (animal != null) {
                        if (animal.Milkable()) {
                            animal.MilkedToday = true;
                            CommandManager.AddItemToInv(GameLoop.World.Player, Item.Copy(animal.MilkItem));

                            Point bed = animal.RestSpot;
                            NetMsg updateTile = new("updateTile", map.GetTile(bed).ToByteArray());
                            updateTile.SetFullPos(bed, MapPos);
                            GameLoop.SendMessageIfNeeded(updateTile, false, false);
                        }
                        else {
                            if (animal.MilkItem == "")
                                GameLoop.UIManager.AddMsg("The " + animal.Species + " can't be milked.");
                            else if (animal.MilkedToday)
                                GameLoop.UIManager.AddMsg("You already milked that " + animal.Species + " today!");
                            else if (animal.Age < animal.AdultAge)
                                GameLoop.UIManager.AddMsg("The " + animal.Species + " isn't old enough to be milked.");
                            else if (animal.Happiness < 50)
                                GameLoop.UIManager.AddMsg("The " + animal.Species + " isn't happy enough to be milked.");
                        }
                    }
                }

                if (item.ItemCat == "Animal") {
                    if (tile.AnimalBed != null && tile.AnimalBed.Animal == null && tile.Name == "Animal Bed") {
                        map.GetTile(Pos).AnimalBed.Animal = item.SpawnAnimal.GetAnimal(Pos);
                        map.GetTile(Pos).AnimalBed.SpawnAnimal();

                        NetMsg updateTile = new("updateTile", map.GetTile(Pos).ToByteArray());
                        updateTile.MiscString1 = "SpawnAnimal";
                        updateTile.SetFullPos(Pos, MapPos);
                        GameLoop.SendMessageIfNeeded(updateTile, false, false);

                        ConsumeItem();
                    } 
                }

                if (item.ItemCat == "Pet") {
                    if (tile.AnimalBed != null && tile.AnimalBed.Animal == null && tile.Name == "Pet Bed") {
                        map.GetTile(Pos).AnimalBed.Animal = item.SpawnAnimal.GetAnimal(Pos);
                        map.GetTile(Pos).AnimalBed.SpawnAnimal();

                        NetMsg updateTile = new("updateTile", map.GetTile(Pos).ToByteArray());
                        updateTile.MiscString1 = "SpawnAnimal";
                        updateTile.SetFullPos(Pos, MapPos);
                        GameLoop.SendMessageIfNeeded(updateTile, false, false);

                        ConsumeItem();
                    } 
                }

                if (map.GetEntityAt<FarmAnimal>(Pos) != null) {
                    FarmAnimal spot = map.GetEntityAt<FarmAnimal>(Pos);
                    if (item.ItemCat == "NameTag" && item.Name != "Name Tag") {
                        spot.Nickname = item.Name;
                        spot.Name = spot.Nickname;
                        GameLoop.UIManager.AddMsg(new ColoredString("You named the " + spot.Species + " '" + spot.Nickname + "'!", Color.Lime, Color.Black));
                        ConsumeItem(); 
                    }
                    else if (item.ItemCat == "PetChow" && !spot.FedToday && spot.Pet) {
                        spot.FedToday = true;
                        if (spot.Nickname != spot.Species)
                            GameLoop.UIManager.AddMsg(new ColoredString("You feed " + spot.Nickname + ". They seem content.", Color.Lime, Color.Black));
                        else
                            GameLoop.UIManager.AddMsg(new ColoredString("You feed the " + spot.Nickname + ". They seem content.", Color.Lime, Color.Black));
                        ConsumeItem();
                    }
                    else if (item.ItemCat == "AnimalFeed" && !spot.FedToday && !spot.Pet) {
                        spot.FedToday = true;
                        if (spot.Nickname != spot.Species)
                            GameLoop.UIManager.AddMsg(new ColoredString("You feed " + spot.Nickname + ". They seem content.", Color.Lime, Color.Black));
                        else
                            GameLoop.UIManager.AddMsg(new ColoredString("You feed the " + spot.Nickname + ". They seem content.", Color.Lime, Color.Black));
                        ConsumeItem();
                    }
                    else if (item.ItemCat == "Brush" && !spot.BrushedToday) {
                        spot.Brush();
                    }
                    else if (!spot.PattedToday) {
                        spot.Pat();
                    }
                }

                if (item.Name == "Copper Coin" && !GameLoop.UIManager.Sidebar.MadeCoinThisFrame) {
                    GameLoop.World.Player.CopperCoins++;
                    ConsumeItem();
                }

                if (item.Name == "Silver Coin" && !GameLoop.UIManager.Sidebar.MadeCoinThisFrame) {
                    GameLoop.World.Player.SilverCoins++;
                    ConsumeItem();
                }

                if (item.Name == "Gold Coin" && !GameLoop.UIManager.Sidebar.MadeCoinThisFrame) {
                    GameLoop.World.Player.GoldCoins++;
                    ConsumeItem();
                }

                if (item.Name == "Jade Coin" && !GameLoop.UIManager.Sidebar.MadeCoinThisFrame) {
                    GameLoop.World.Player.JadeCoins++;
                    ConsumeItem();
                }

                if (item.ItemCat == "Hoe" || item.ItemCat == "Seed") {
                    if (GameLoop.World.Player.MapPos == new Point3D(-1, 0, 0) && !GameLoop.CheckFlag("farm")) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You need to buy this land from the town hall first.", Color.Red, Color.Black));
                    } else if (GameLoop.World.Player.MapPos != new Point3D(-1, 0, 0)) {
                        GameLoop.UIManager.AddMsg(new ColoredString("You probably shouldn't do that here.", Color.Red, Color.Black));
                    }
                }

                if (item.ItemCat == "Spade") {
                    if (tile.Name == "Relic Spot") {
                        List<ArchArtifact> possible = new();

                        foreach(KeyValuePair<string, ArchArtifact> kv in GameLoop.World.artifactLibrary) {
                            if (kv.Value.Location == "Any" || kv.Value.Location.Contains(map.MinimapTile.name)) {
                                possible.Add(kv.Value);
                            }
                        }

                        ArchArtifact rand = new(possible[GameLoop.rand.Next(possible.Count)]); 

                        Item artItem = Item.Copy("lh:Unknown Relic");
                        artItem.Artifact = rand;
                        artItem.Artifact.SetStats(artItem);
                        artItem.Artifact.Dirtify(false);
                        artItem.Artifact.Weather(7);
                        CommandManager.AddItemToInv(GameLoop.World.Player, artItem);
                        map.SetTile(Pos, new Tile("lh:Grass"));
                    }
                }

                if (item.Artifact != null && GameLoop.World.Player.Skills.ContainsKey("Archaeology") && GameLoop.World.Player.Skills["Archaeology"].Level >= item.Artifact.RequiredLevel) {
                    GameLoop.UIManager.Minigames.ArchCleaning.Current = item;
                    GameLoop.UIManager.Minigames.ToggleMinigame("ArchCleaning");
                }

                if (item.ItemCat == "Hammer") {
                    if (GameLoop.UIManager.Construction.SelectedConstructible != -1) {
                        if (GameLoop.World.Player.Skills["Construction"].Level >= GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].RequiredLevel) {
                            if (UI_Construction.CheckValidConstruction(GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible])) {
                                Constructible con = GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible];

                                string location = map.MinimapTile.name;

                                if (BuildManager.CanBuildHere(location, con.Furniture)) {
                                    if (tile.Name.ToLower().Contains("floor") || tile.Name.ToLower().Contains("grass")) {
                                        if (tile.Name != con.Name) {
                                            for (int i = 0; i < GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].MaterialsNeeded.Count; i++) {
                                                int needed = GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].MaterialsNeeded[i].ItemQuantity;

                                                for (int j = 0; j < GameLoop.World.Player.Inventory.Length; j++) {
                                                    if (needed > 0) {
                                                        if (con.MaterialsNeeded[i].ID.Contains("Nails")) {
                                                            if (GameLoop.World.Player.Inventory[j].Name == con.MaterialsNeeded[i].ID) {
                                                                if (GameLoop.World.Player.Inventory[j].ItemQuantity > con.MaterialsNeeded[i].ItemQuantity) {
                                                                    GameLoop.World.Player.Inventory[j].ItemQuantity -= con.MaterialsNeeded[i].ItemQuantity;
                                                                    needed -= con.MaterialsNeeded[i].ItemQuantity;
                                                                } else if (GameLoop.World.Player.Inventory[j].ItemQuantity == con.MaterialsNeeded[i].ItemQuantity) {
                                                                    GameLoop.World.Player.Inventory[j] = Item.Copy("lh:(EMPTY)");
                                                                    needed = 0;
                                                                } else {
                                                                    needed -= GameLoop.World.Player.Inventory[j].ItemQuantity;
                                                                    GameLoop.World.Player.Inventory[j] = Item.Copy("lh:(EMPTY)");
                                                                }
                                                            }
                                                        } else {
                                                            if (GameLoop.World.Player.Inventory[j].FullName() == con.MaterialsNeeded[i].ID) {
                                                                if (GameLoop.World.Player.Inventory[j].ItemQuantity > con.MaterialsNeeded[i].ItemQuantity) {
                                                                    GameLoop.World.Player.Inventory[j].ItemQuantity -= con.MaterialsNeeded[i].ItemQuantity;
                                                                    needed -= con.MaterialsNeeded[i].ItemQuantity;
                                                                } else if (GameLoop.World.Player.Inventory[j].ItemQuantity == con.MaterialsNeeded[i].ItemQuantity) {
                                                                    GameLoop.World.Player.Inventory[j] = Item.Copy("lh:(EMPTY)");
                                                                    needed = 0;
                                                                } else {
                                                                    needed -= GameLoop.World.Player.Inventory[j].ItemQuantity;
                                                                    GameLoop.World.Player.Inventory[j] = Item.Copy("lh:(EMPTY)");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                            map.GetTile(Pos).Name = con.Name;
                                            map.GetTile(Pos).TileGlyph = con.Glyph;
                                            map.GetTile(Pos).ForegroundR = con.ForegroundR;
                                            map.GetTile(Pos).ForegroundG = con.ForegroundG;
                                            map.GetTile(Pos).ForegroundB = con.ForegroundB;
                                            map.GetTile(Pos).Dec = con.Dec;
                                            map.GetTile(Pos).IsBlockingLOS = con.BlocksLOS;
                                            map.GetTile(Pos).IsBlockingMove = con.BlocksMove;
                                            map.GetTile(Pos).Container = con.Container;
                                            map.GetTile(Pos).Lock = con.Lock;
                                            map.GetTile(Pos).UpdateAppearance();

                                            MissionManager.Increment("Construction", con.Name, 1);
                                            GameLoop.SoundManager.PlaySound("hammer");
                                            GameLoop.World.Player.ExpendStamina(1);
                                            GameLoop.UIManager.Map.UpdateVision();
                                            GameLoop.World.Player.Skills["Construction"].GrantExp(con.ExpGranted);

                                            NetMsg updateTile = new("updateTile", map.GetTile(Pos).ToByteArray());
                                            updateTile.SetFullPos(Pos, MapPos);
                                            GameLoop.SendMessageIfNeeded(updateTile, false, false);
                                        } else {
                                            GameLoop.UIManager.AddMsg("That's already been built there!");
                                        }
                                    } else {
                                        GameLoop.UIManager.AddMsg("There's already something there.");
                                    }
                                } else {
                                    GameLoop.UIManager.AddMsg("You probably shouldn't build that here.");
                                }
                            } else {
                                GameLoop.UIManager.AddMsg("That needs: " + GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].Materials);
                            }
                        } else {
                            GameLoop.UIManager.AddMsg("You need " + GameLoop.World.constructibles[GameLoop.UIManager.Construction.SelectedConstructible].RequiredLevel + " Construction to build that.");
                        }
                    } else {
                        if (tile.Name.ToLower().Contains("floor") || tile.Name.ToLower().Contains("grass") || tile.Name == "Dirt") {
                            GameLoop.UIManager.AddMsg("Right click to select something to build first.");
                        } else {
                            if (!tile.DeconstructFlag) {
                                GameLoop.UIManager.AddMsg("Click again to deconstruct the " + tile.Name + ".");
                                map.GetTile(Pos).DeconstructFlag = true;
                            } else {
                                map.Tiles[Pos.X + (Pos.Y * GameLoop.MapWidth)] = new Tile("lh:Grass");
                                map.Tiles[Pos.X + (Pos.Y * GameLoop.MapWidth)].UpdateAppearance();
                                GameLoop.UIManager.Map.UpdateVision();

                                NetMsg updateTile = new("updateTile", map.Tiles[Pos.X + (Pos.Y * GameLoop.MapWidth)].ToByteArray());
                                updateTile.SetFullPos(Pos, MapPos);
                                GameLoop.SendMessageIfNeeded(updateTile, false, false);
                            }
                        }
                    }
                }


                if (item.Name == "Farm Permit") { // Permit is for the farm
                    if (GameLoop.SingleOrHosting()) {
                        if (!GameLoop.CheckFlag("farm")) {
                            GameLoop.World.Player.OwnsFarm = true;
                            ConsumeItem();
                            GameLoop.UIManager.AddMsg(new ColoredString("You unlocked the farm!", Color.Cyan, Color.Black));

                            if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
                                GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

                            GameLoop.SoundManager.PlaySound("successJingle");

                            NetMsg usedPermit = new("usedPermit");
                            usedPermit.MiscString1 = "farm";
                            GameLoop.SendMessageIfNeeded(usedPermit, false, true);
                        } else {
                            GameLoop.UIManager.AddMsg(new ColoredString("You already own the farm!", Color.Red, Color.Black));
                        }
                    } else if (!GameLoop.SingleOrHosting()) {
                        if (!GameLoop.CheckFlag("farm")) {
                            if (item.Weight != 0f) {
                                GameLoop.UIManager.AddMsg("Use again to unlock for the HOST player (not yourself!)");
                                item.Weight = 0f;
                            } else {
                                GameLoop.UIManager.AddMsg(new ColoredString("You unlocked the farm!", Color.Cyan, Color.Black));

                                if (!GameLoop.World.maps.ContainsKey(new Point3D(-1, 0, 0)))
                                    GameLoop.World.LoadMapAt(new Point3D(-1, 0, 0));

                                GameLoop.World.maps[new Point3D(-1, 0, 0)].MinimapTile.name = "Your Farm";

                                GameLoop.SoundManager.PlaySound("successJingle");

                                NetMsg usedPermit = new("usedPermit");
                                usedPermit.MiscString1 = "farm";
                                GameLoop.SendMessageIfNeeded(usedPermit, false, true);
                                ConsumeItem();
                            }
                        } else {
                            GameLoop.UIManager.AddMsg(new ColoredString("The host already owns the farm!", Color.Red, Color.Black));
                        }
                    }
                }

                if (item.Name == "Noonbreeze Apartment") {
                    if (GameLoop.World.Player.NoonbreezeApt == null) {
                        GameLoop.World.Player.OwnedLocations.Add(GameLoop.World.Player.Name + " Apartment", new OwnableLocation(GameLoop.World.Player.Name + " Apartment", true));
                        GameLoop.World.Player.NoonbreezeApt = new();
                        GameLoop.World.Player.NoonbreezeApt.SetupNew("Noonbreeze Apartments", 30);
                        ConsumeItem();
                        GameLoop.SoundManager.PlaySound("successJingle");
                        GameLoop.UIManager.AddMsg("You rented an apartment for 28 days!");

                        if (GameLoop.World.Player.Clock.IsItThisDay(1, 1) && GameLoop.World.Player.Clock.Year == 1) {
                            GameLoop.SteamManager.UnlockAchievement("APT_NOONBREEZE");
                        }
                    } else {
                        GameLoop.World.Player.NoonbreezeApt.AddDays(28);
                        ConsumeItem();
                        GameLoop.SoundManager.PlaySound("successJingle");
                        GameLoop.UIManager.AddMsg("You added 28 days to your lease!");
                    }
                }

                if (item.Name == "Medium Backpack") { // Used Medium Backpack
                    ConsumeItem();
                    List<Item> previous = GameLoop.World.Player.Inventory.ToList();
                    GameLoop.World.Player.Inventory = new Item[18];

                    for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                        GameLoop.World.Player.Inventory[i] = Item.Copy("lh:(EMPTY)");
                    }

                    for (int i = 0; i < previous.Count; i++) {
                        GameLoop.World.Player.Inventory[i] = previous[i];
                    }
                    GameLoop.SoundManager.PlaySound("successJingle");
                    GameLoop.UIManager.AddMsg("Your backpack can now hold 18 items!");
                }

                if (item.Name == "Large Backpack") { // Used Medium Backpack
                    ConsumeItem();
                    List<Item> previous = GameLoop.World.Player.Inventory.ToList();
                    GameLoop.World.Player.Inventory = new Item[27];

                    for (int i = 0; i < GameLoop.World.Player.Inventory.Length; i++) {
                        GameLoop.World.Player.Inventory[i] = Item.Copy("lh:(EMPTY)");
                    }

                    for (int i = 0; i < previous.Count; i++) {
                        GameLoop.World.Player.Inventory[i] = previous[i];
                    }
                    GameLoop.SoundManager.PlaySound("successJingle");
                    GameLoop.UIManager.AddMsg("Your backpack can now hold 27 items!");
                }

                if (item.ItemCat == "Camera") { // Used Camera
                    Item photo = Item.Copy("lh:Photo");
                    Photo data = new();

                    Point topLeft = Pos - new Point(10, 10);
                    if (topLeft.X < 0)
                        topLeft = new Point(0, topLeft.Y);
                    if (topLeft.Y < 0)
                        topLeft = new Point(topLeft.X, 0);
                    if (topLeft.X + 21 > GameLoop.MapWidth)
                        topLeft = new Point(GameLoop.MapWidth - 21, topLeft.Y);
                    if (topLeft.Y + 21 > GameLoop.MapHeight)
                        topLeft = new Point(topLeft.X, GameLoop.MapHeight - 21);

                    for (int x = 0; x < 21; x++) {
                        for (int y = 0; y < 21; y++) {
                            Point maploc = topLeft + new Point(x, y);
                            Tile thisTile = map.GetTile(maploc);

                            if (thisTile.IsVisible) {
                                if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(maploc.ToCoord())) {
                                    if (thisTile.Lock == null) {
                                        data.tiles[x + (y * 21)] = new(thisTile.Name, thisTile.ForegroundR, thisTile.ForegroundG, thisTile.ForegroundB, 255, thisTile.TileGlyph, thisTile.Dec);
                                    } else {
                                        if (thisTile.Lock.Closed)
                                            data.tiles[x + (y * 21)] = new(thisTile.Name, thisTile.ForegroundR, thisTile.ForegroundG, thisTile.ForegroundB, 255, thisTile.Lock.ClosedGlyph, thisTile.Dec);
                                        else
                                            data.tiles[x + (y * 21)] = new(thisTile.Name, thisTile.ForegroundR, thisTile.ForegroundG, thisTile.ForegroundB, 255, thisTile.Lock.OpenedGlyph, thisTile.Dec);
                                    }

                                } else {
                                    data.tiles[x + (y * 21)] = new("", 0, 0, 0, 255, 32, null);
                                }
                            } else {
                                data.tiles[x + (y * 21)] = new("", 0, 0, 0, 255, 32, null);
                            }

                            if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(maploc.ToCoord())) {
                                if (map.GetEntityAt<ItemWrapper>(maploc) != null) {
                                    PhotoEntity ent = new();
                                    ItemWrapper wrap = map.GetEntityAt<ItemWrapper>(maploc);

                                    ent.Glyph = wrap.item.ItemGlyph;
                                    ent.ColR = wrap.item.ForegroundR;
                                    ent.ColG = wrap.item.ForegroundG;
                                    ent.ColB = wrap.item.ForegroundB;
                                    ent.ColA = 255;

                                    ent.Name = wrap.item.Name;
                                    ent.Type = "Item";
                                    ent.X = x;
                                    ent.Y = y;

                                    if (wrap.item.Dec != null)
                                        ent.Dec = new(wrap.item.Dec);

                                    data.entities.Add(ent);
                                }

                                if (map.GetEntityAt<MonsterWrapper>(maploc) != null) {
                                    PhotoEntity ent = new();
                                    MonsterWrapper wrap = map.GetEntityAt<MonsterWrapper>(maploc);

                                    ent.Glyph = wrap.monster.ActorGlyph;
                                    ent.ColR = wrap.monster.ForegroundR;
                                    ent.ColG = wrap.monster.ForegroundG;
                                    ent.ColB = wrap.monster.ForegroundB;
                                    ent.ColA = 255;
                                    ent.Type = "Monster";

                                    ent.Name = wrap.monster.Species;
                                    ent.X = x;
                                    ent.Y = y;
                                    data.entities.Add(ent);
                                }

                                foreach (KeyValuePair<string, NPC> kv in GameLoop.World.npcLibrary) {
                                    if (kv.Value.Position == maploc && kv.Value.MapPos == GameLoop.World.Player.MapPos) {
                                        PhotoEntity ent = new();
                                        ent.Glyph = kv.Value.ActorGlyph;
                                        ent.ColR = kv.Value.ForegroundR;
                                        ent.ColG = kv.Value.ForegroundG;
                                        ent.ColB = kv.Value.ForegroundB;
                                        ent.ColA = 255;

                                        ent.Name = kv.Value.Name;
                                        ent.Type = "NPC";
                                        ent.X = x;
                                        ent.Y = y;
                                        data.entities.Add(ent);
                                    }
                                }

                                foreach (KeyValuePair<SteamId, Player> kv in GameLoop.World.otherPlayers) {
                                    if (kv.Value.Position == maploc && kv.Value.MapPos == GameLoop.World.Player.MapPos) {
                                        PhotoEntity ent = new();
                                        ent.Glyph = kv.Value.ActorGlyph;
                                        ent.ColR = kv.Value.ForegroundR;
                                        ent.ColG = kv.Value.ForegroundG;
                                        ent.ColB = kv.Value.ForegroundB;
                                        ent.ColA = 255;

                                        ent.Name = kv.Value.Name;
                                        ent.Type = "Player";
                                        ent.X = x;
                                        ent.Y = y;
                                        data.entities.Add(ent);
                                    }
                                }

                                if (GameLoop.World.Player.Position == maploc) {
                                    PhotoEntity ent = new();
                                    ent.Glyph = GameLoop.World.Player.ActorGlyph;
                                    ent.ColR = GameLoop.World.Player.ForegroundR;
                                    ent.ColG = GameLoop.World.Player.ForegroundG;
                                    ent.ColB = GameLoop.World.Player.ForegroundB;
                                    ent.ColA = 255;

                                    ent.Name = GameLoop.World.Player.Name;
                                    ent.X = x;
                                    ent.Y = y;
                                    ent.Type = "Player";
                                    data.entities.Add(ent);
                                }
                            }
                        }
                    }

                    data.SeasonTaken = GameLoop.World.Player.Clock.GetSeason();
                    data.DayTaken = GameLoop.World.Player.Clock.Day;
                    data.MinutesTaken = GameLoop.World.Player.Clock.GetCurrentTime();

                    photo.Photo = data;

                    CommandManager.AddItemToInv(GameLoop.World.Player, photo);
                    GameLoop.SoundManager.PlaySound("camera");

                }

                if (item.Photo != null) { // Used Photo
                    Photo photo = item.Photo;
                    GameLoop.UIManager.Photo.CurrentPhoto = photo;
                    GameLoop.UIManager.selectedMenu = "Photo";
                    GameLoop.UIManager.Photo.ShowingBoard = false;
                    GameLoop.UIManager.Photo.Toggle();
                }
            
                if (item.LeftClickScript != "") {
                    GameLoop.ScriptManager.SetupScript(item.LeftClickScript);
                }
            }
        }



        // Click and hold
        public static void ContinuousUseItem(Item item, Point Pos, Point3D MapPos, int distance) {
            Map map = Helper.ResolveMap(MapPos);
            bool Harvesting = false;

            if (map != null && Pos.X >= 0 && Pos.X <= GameLoop.MapWidth && Pos.Y >= 0 && Pos.Y <= GameLoop.MapHeight) {
                Tile tile = map.GetTile(Pos);
                if (item.Name == "(EMPTY)") {
                    if (distance < 5) {
                        if (tile.Plant != null) {
                            if (tile.Plant.CurrentStage != 0 && tile.Plant.CurrentStage != -1)
                                Harvesting = true;

                            tile.Plant.Harvest(GameLoop.World.Player);
                            if (tile.Plant.CurrentStage == -1) {
                                Tile tilled = new("lh:Dirt");
                                tilled.Name = "Tilled Dirt";
                                tilled.TileGlyph = 34;
                                tilled.UpdateAppearance();


                                map.SetTile(Pos, tilled);
                                GameLoop.UIManager.Map.MapConsole.SetEffect(Pos.X, Pos.X, null);
                                tile.UpdateAppearance();

                                NetMsg updateTile = new("updateTile", tilled.ToByteArray());
                                updateTile.SetFullPos(Pos, MapPos);
                                GameLoop.SendMessageIfNeeded(updateTile, false, false);
                            } else {
                                tile.UpdateAppearance();

                                NetMsg updateTile = new("updateTile", tile.ToByteArray());
                                updateTile.SetFullPos(Pos, MapPos);
                                GameLoop.SendMessageIfNeeded(updateTile, false, false);
                            }
                        }
                    }
                } 
                else {
                    if (item.ItemCat == "Fishing Rod") { // Fishing rod
                        if (GameLoop.UIManager.Sidebar.ChargeBar < 100) {
                            GameLoop.UIManager.Sidebar.ChargeBar++;
                        }
                    }

                    if (item.ItemCat == "Hoe") { // Clicked with a hoe 
                        if (distance < 5) {
                            if (tile.Name == "Dirt") {
                                if (GameLoop.World.Player.MapPos == new Point3D(-1, 0, 0) && GameLoop.CheckFlag("farm")) {
                                    map.GetTile(Pos).Name = "Tilled Dirt";
                                    map.GetTile(Pos).TileGlyph = 34;
                                    map.GetTile(Pos).UpdateAppearance();
                                    GameLoop.World.Player.ExpendStamina(1);
                                    GameLoop.SoundManager.PlaySound("till");

                                    NetMsg updateTile = new("updateTile", map.GetTile(Pos).ToByteArray());
                                    updateTile.SetFullPos(Pos, MapPos);
                                    GameLoop.SendMessageIfNeeded(updateTile, false, false);
                                }
                            }
                        }
                    }

                    if (item.ItemCat == "Watering Can") { // Clicked with a watering can 
                        if (distance < 5) {
                            if (tile.Name == "Well" || tile.Name.ToLower().Contains("water") && item.Durability != item.MaxDurability) {
                                item.Durability = item.MaxDurability;
                                GameLoop.SoundManager.PlaySound("water");
                            } else {
                                if (tile.Plant != null) {
                                    if (!tile.Plant.WateredToday && item.Durability > 0) {
                                        tile.Plant.WateredToday = true;
                                        item.Durability--;
                                        GameLoop.UIManager.Map.MapConsole.SetEffect(Pos.X, Pos.Y, null);
                                        tile.UpdateAppearance();
                                        GameLoop.UIManager.Map.MapConsole.SetCellAppearance(Pos.X, Pos.Y, tile);
                                        GameLoop.World.Player.ExpendStamina(1);
                                        GameLoop.SoundManager.PlaySound("water");
                                    }
                                    NetMsg updateTile = new("updateTile", tile.ToByteArray());
                                    updateTile.SetFullPos(Pos, MapPos);
                                    GameLoop.SendMessageIfNeeded(updateTile, false, false);
                                }
                            }
                        }
                    }

                    if (item.Plant != null && !Harvesting) { // Clicked with a seed
                        Plant plant = item.Plant;
                        if (distance < 5) {
                            if (tile.Name == "Tilled Dirt") {
                                if (MapPos == new Point3D(-1, 0, 0) && GameLoop.CheckFlag("farm")) {
                                    if (plant.RequiredLevel <= GameLoop.World.Player.Skills["Farming"].Level) {
                                        if (plant.GrowthSeason == "Any" || plant.GrowthSeason == GameLoop.World.Player.Clock.GetSeason()) {
                                            tile.Plant = new Plant(plant);
                                            tile.Name = tile.Plant.ProduceName + " Plant";
                                            tile.UpdateAppearance();
                                            GameLoop.UIManager.Map.MapConsole.SetEffect(Pos.X, Pos.Y, new CustomBlink(168, Color.Blue));
                                            ConsumeItem();
                                            GameLoop.SoundManager.PlaySound("plantSeed");

                                            NetMsg updateTile = new("updateTile", tile.ToByteArray());
                                            updateTile.SetFullPos(Pos, MapPos);
                                            GameLoop.SendMessageIfNeeded(updateTile, false, false);
                                        } else {
                                            Harvesting = true;
                                            GameLoop.UIManager.AddMsg(new ColoredString("You can't plant that right now. (" + plant.GrowthSeason + ")", Color.Red, Color.Black));
                                        }
                                    } else {
                                        Harvesting = true;
                                        GameLoop.UIManager.AddMsg(new ColoredString("You aren't high enough level to plant that. (" + plant.RequiredLevel + ")", Color.Red, Color.Black));
                                    }
                                }
                            }
                        }
                    }

                    if (item.LeftClickScript != "") {
                        GameLoop.ScriptManager.SetupScript(item.LeftReleaseScript);
                    }
                }
            }
        }

        public static void ReleaseClick(Item item, Point Pos, Point3D MapPos, int distance) {
            Map map = Helper.ResolveMap(MapPos);

            if (map != null) {
                if (GameLoop.UIManager.Sidebar.ChargeBar != 0) {
                    if (GameLoop.UIManager.Sidebar.LocalLure != null) {
                        if (map.GetEntityAt<ItemWrapper>(GameLoop.UIManager.Sidebar.LocalLure.Position) != null) {
                            ItemWrapper wrap = map.GetEntityAt<ItemWrapper>(GameLoop.UIManager.Sidebar.LocalLure.Position);
                            GameLoop.UIManager.AddMsg("Snagged the " + wrap.item.Name + "!");
                            CommandManager.AddItemToInv(GameLoop.World.Player, wrap.item);
                            GameLoop.World.Player.ExpendStamina(1);
                            wrap.Position = new Point(-1, -2);
                        } else if (GameLoop.UIManager.Sidebar.LocalLure.FishOnHook) {
                            Tile tile = map.GetTile(Pos);

                            GameLoop.UIManager.Minigames.FishingManager.InitiateFishing(GameLoop.World.Player.Clock.GetSeason(),
                            GameLoop.World.Player.Clock.GetCurrentTime(),
                            GameLoop.World.Player.Skills["Fishing"].Level,
                            tile.MiscString);
                            GameLoop.World.Player.ExpendStamina(1);
                        } else {
                            GameLoop.UIManager.Map.MapConsole.ClearDecorators(GameLoop.UIManager.Sidebar.LocalLure.Position.X, GameLoop.UIManager.Sidebar.LocalLure.Position.Y, 1);
                            GameLoop.UIManager.Sidebar.LocalLure.Position = new Point(-1, -1);
                        }
                    }
                    if (GameLoop.UIManager.Sidebar.ChargeBar >= 10) {
                        if (item.Name != "(EMPTY)") {
                            if (item.ItemCat == "Fishing Rod") { // Fishing rod
                                Point target = GameLoop.UIManager.Sidebar.LureSpot * new Point(2, 2);
                                int xDist = (int)((double)((target.X / 10) * (GameLoop.UIManager.Sidebar.ChargeBar / 10)));
                                int yDist = (int)((double)((target.Y / 10) * (GameLoop.UIManager.Sidebar.ChargeBar / 10)));

                                GameLoop.UIManager.Sidebar.LocalLure = new();
                                GameLoop.UIManager.Sidebar.LocalLure.Position = GameLoop.World.Player.Position;
                                GameLoop.UIManager.Sidebar.LocalLure.SetVelocity(xDist, yDist);
                                GameLoop.UIManager.Map.EntityRenderer.Add(GameLoop.UIManager.Sidebar.LocalLure);
                            }
                        }
                    }

                    GameLoop.UIManager.Sidebar.ChargeBar = 0;
                }
            }
        }

        public static void RightClickItem(Item item, Point Pos, Point3D MapPos, int distance) {
            Map map = Helper.ResolveMap(MapPos);

            if (map != null) {
                Tile tile = map.GetTile(Pos);

                if (tile != null) { 
                    if (item.ItemCat == "Hammer") {
                        GameLoop.UIManager.Construction.Toggle();
                    }

                    else if (item.Name == "(EMPTY)") { 
                        if (tile.Name == "Pet Bed" && tile.AnimalBed != null && tile.AnimalBed.Animal != null) {
                            map.Entities.Remove(tile.AnimalBed.Animal); 
                            GameLoop.UIManager.Map.EntityRenderer.Remove(tile.AnimalBed.Animal);

                            Item packed = tile.AnimalBed.Animal.ToItem("Pet");
                            CommandManager.AddItemToInv(GameLoop.World.Player, packed);

                            tile.AnimalBed.Animal = null;
                            GameLoop.UIManager.Map.SyncMapEntities(map);
                        }

                        else if (tile.Name == "Animal Bed" && tile.AnimalBed != null && tile.AnimalBed.Animal != null) {
                            map.Entities.Remove(tile.AnimalBed.Animal);
                            GameLoop.UIManager.Map.EntityRenderer.Remove(tile.AnimalBed.Animal);
                              
                            Item packed = tile.AnimalBed.Animal.ToItem("Animal");
                            
                            CommandManager.AddItemToInv(GameLoop.World.Player, packed);

                            tile.AnimalBed.Animal = null;
                            GameLoop.UIManager.Map.SyncMapEntities(map);
                        }
                    } 

                    else if (item.ItemCat == "NameTag") {
                        GameLoop.UIManager.Nametag.Toggle();
                        GameLoop.UIManager.Nametag.current = item;
                    }

                    else if (item.LeftClickScript != "") {
                        GameLoop.ScriptManager.SetupScript(item.RightClickScript);
                    }
                }
            }
        } 
    }
}
