﻿using SadRogue.Primitives;
using SadConsole;
using SadConsole.UI;
using SadConsole.Input;
using System.IO;
using System;
using LofiHollow.Managers;
using Key = SadConsole.Input.Keys;
using LofiHollow.DataTypes;
using Newtonsoft.Json;
using LofiHollow.EntityData;
using LofiHollow.Entities; 
using LofiHollow.Entities.NPC;
using LofiHollow.Minigames.Archaeology;
using LofiHollow.Minigames.Picross;
using Steamworks;
using System.Collections.Generic;
using Steamworks.Data;
using Color = SadRogue.Primitives.Color;
using System.Linq;

namespace LofiHollow.UI {
    public class UI_ModMaker : InstantUI {
        public string ModMenuSelect = "List";
        public string ModSubSelect = "None";
        public bool CreatingMod = false;
        public int ModItemIndex = 0;
        public int CurrentGlyphIndex = 0;
        public int MapEditorTileIndex = 0;
        public int SelectedField = 0;
        public int DocumentationTop = 0;
        public Mod CurrentMod; 
         
        public string UploadText = "";
        public string CurrentChangelog = "";
         
        public int PaintR = 127;
        public int PaintG = 127;
        public int PaintB = 127;

        public bool HostMode = false;
        public bool LocalModList = false;

        public Dictionary<string, Mod> InstalledMods = new();

        public UI_ModMaker(int width, int height) : base(width, height, "ModMaker") { 
        }

        public async void UploadToWorkshop() {
            if (CurrentMod.Metadata.PublishedID == 0) {
                var result = await Steamworks.Ugc.Editor.NewCommunityFile
                                    .WithTitle(CurrentMod.Metadata.WorkshopTitle)
                                    .WithDescription(CurrentMod.Metadata.WorkshopDesc)
                                    .WithTag("Mod")
                                    .WithContent("./mods/" + CurrentMod.Metadata.WorkshopTitle + "/")
                                    .WithPublicVisibility()
                                    .SubmitAsync();

                if (result.Success) {
                    UploadText = "Upload Successful!"; 
                    CurrentMod.Metadata.PublishedID = result.FileId;
                    SaveMod();
                }
                else {
                    if (result.NeedsWorkshopAgreement) {
                        UploadText = "Needs Workshop EULA";
                    } 
                    else {
                        UploadText = "Unknown Failure";
                    }
                }
            } else {
                var result = await new Steamworks.Ugc.Editor(CurrentMod.Metadata.PublishedID)
                                    .WithTitle(CurrentMod.Metadata.WorkshopTitle)
                                    .WithDescription(CurrentMod.Metadata.WorkshopDesc)
                                    .WithChangeLog(CurrentChangelog)
                                    .WithContent("./mods/" + CurrentMod.Metadata.WorkshopTitle + "/")
                                    .WithPublicVisibility()
                                    .SubmitAsync();

                if (result.Success) {
                    SaveMod();
                    UploadText = "Upload Successful!";
                    CurrentChangelog = ""; 
                }
                else {
                    if (result.NeedsWorkshopAgreement) {
                        UploadText = "Needs Workshop EULA";
                    }
                    else {
                        UploadText = "Unknown Failure";
                    }
                }
            }
        }

        public void ToggleClick(string ID) {
            ulong id = ulong.Parse(ID);
            if (GameLoop.SteamManager.ModsEnabled.Contains(id))
                GameLoop.SteamManager.ModsEnabled.Remove(id);
            else
                GameLoop.SteamManager.ModsEnabled.Add(id); 

            GameLoop.SteamManager.SaveModList();
        }

        public void ResetThings(string going) { 
            ModSubSelect = going;
            ModItemIndex = 0;
            SelectedField = 0;
            DocumentationTop = 0;
        }

        public void ModMakerClicks(string ID) { 

            if (ID == "BackToMainMenu") {
                GameLoop.UIManager.ToggleUI("ModMaker");
            }

            else if (ID == "CreateNewMod") {
                ModMenuSelect = "Create";
                ModSubSelect = "Overview";
                CurrentMod = new();
                CreatingMod = true;
            }

            else if (ID == "BackToOverview") {
                ModMenuSelect = "Overview";
                ModSubSelect = "Overview";
                SaveMod();
                CreatingMod = false;
            }

            else if (ID == "SaveMod") { SaveMod(); }
             

            else if (ID == "Go Items") {
                ResetThings("Item");
                if (CurrentMod.ModItems.Count == 0)
                    CurrentMod.ModItems.Add(new());
            } 

            else if (ID == "Go Monsters") {
                ResetThings("Monster");
                if (CurrentMod.ModMonsters.Count == 0)
                    CurrentMod.ModMonsters.Add(new(Color.White));
            }

            else if (ID == "Go NPCs") {
                ResetThings("NPC");
                if (CurrentMod.ModNPCs.Count == 0)
                    CurrentMod.ModNPCs.Add(new());
            }

            else if (ID == "Go Skills") {
                ResetThings("Skill");
                if (CurrentMod.ModSkills.Count == 0)
                    CurrentMod.ModSkills.Add(new());
            }

            else if (ID == "Go Artifacts") {
                ResetThings("Artifact");
                if (CurrentMod.ModArtifacts.Count == 0)
                    CurrentMod.ModArtifacts.Add(new());
            }

            else if (ID == "Go Picross") {
                ResetThings("Picross");
                if (CurrentMod.ModPicross.Count == 0)
                    CurrentMod.ModPicross.Add(new());
            }

            else if (ID == "Go Scripts") {
                ResetThings("Script");
                if (CurrentMod.ModScripts.Count == 0)
                    CurrentMod.ModScripts.Add(new());
            } 
             
            else if (ID == "BackToList") {
                ModMenuSelect = "List";
                SaveMod();
                CreatingMod = false;
                CurrentMod = null;
                ModItemIndex = 0;
                ModSubSelect = "Overview";
                UploadText = "";
                FetchMods();
            }

            else if (ID == "ToCreateEdit") {
                ModMenuSelect = "Create";
                ModSubSelect = "Overview";
                CreatingMod = true;
            }

            
            else if (ID == "GoOverview") { ModSubSelect = "Overview"; }
            else if (ID == "OverviewConstructs") { ModSubSelect = "Constructible"; }
            else if (ID == "OverviewRecipes") { ModSubSelect = "Recipe"; }
            else if (ID == "OverviewItems") { ModSubSelect = "Item"; }
            else if (ID == "OverviewMissions") { ModSubSelect = "Mission"; }
            else if (ID == "OverviewMonsters") { ModSubSelect = "Monster"; }
            else if (ID == "OverviewNPCs") { ModSubSelect = "NPC"; }
            else if (ID == "OverviewSkills") { ModSubSelect = "Skill"; }
            else if (ID == "OverviewArtifacts") { ModSubSelect = "Artifact"; }
            else if (ID == "OverviewPicross") { ModSubSelect = "Picross"; }
            else if (ID == "OverviewScripts") { ModSubSelect = "Script"; }
            else if (ID == "OverviewTiles") { ModSubSelect = "Tile"; }
            else if (ID == "OverviewMaps") { ModSubSelect = "Map"; }

            else if (ID == "PreviousItem") { if (ModItemIndex > 0) ModItemIndex--; }
            else if (ID == "NextItem") {
                if (ModSubSelect == "Artifact") {
                    if (CurrentMod.ModArtifacts.Count < ModItemIndex + 2)
                        CurrentMod.ModArtifacts.Add(new());
                    ModItemIndex++;
                }

                else if (ModSubSelect == "Picross") {
                    if (CurrentMod.ModPicross.Count < ModItemIndex + 2)
                        CurrentMod.ModPicross.Add(new());
                    ModItemIndex++;
                }
                 
            }

            else if (ID == "PackMod") { PackMod(); }
            else if (ID == "UnpackMod") { UnpackMod(); }

            else if (ID == "PicrossEasy") {
                CurrentMod.ModPicross[ModItemIndex].Width = 5;
                CurrentMod.ModPicross[ModItemIndex].Height = 5;
                CurrentMod.ModPicross[ModItemIndex].Difficulty = "Easy";
                CurrentMod.ModPicross[ModItemIndex].Grid = new PicrossTile[CurrentMod.ModPicross[ModItemIndex].Width * CurrentMod.ModPicross[ModItemIndex].Height];
            
                for (int i = 0; i < CurrentMod.ModPicross[ModItemIndex].Grid.Length; i++) {
                    CurrentMod.ModPicross[ModItemIndex].Grid[i] = new PicrossTile();
                }
            }

            else if (ID == "PicrossMedium") {
                CurrentMod.ModPicross[ModItemIndex].Width = 10;
                CurrentMod.ModPicross[ModItemIndex].Height = 10;
                CurrentMod.ModPicross[ModItemIndex].Difficulty = "Medium";
                CurrentMod.ModPicross[ModItemIndex].Grid = new PicrossTile[CurrentMod.ModPicross[ModItemIndex].Width * CurrentMod.ModPicross[ModItemIndex].Height];

                for (int i = 0; i < CurrentMod.ModPicross[ModItemIndex].Grid.Length; i++) {
                    CurrentMod.ModPicross[ModItemIndex].Grid[i] = new PicrossTile();
                }
            }

            else if (ID == "PicrossHard") {
                CurrentMod.ModPicross[ModItemIndex].Width = 15;
                CurrentMod.ModPicross[ModItemIndex].Height = 15;
                CurrentMod.ModPicross[ModItemIndex].Difficulty = "Hard";
                CurrentMod.ModPicross[ModItemIndex].Grid = new PicrossTile[CurrentMod.ModPicross[ModItemIndex].Width * CurrentMod.ModPicross[ModItemIndex].Height];

                for (int i = 0; i < CurrentMod.ModPicross[ModItemIndex].Grid.Length; i++) {
                    CurrentMod.ModPicross[ModItemIndex].Grid[i] = new PicrossTile();
                }
            }

            else if (ID == "Upload") {
                SaveMod(); 

                if (CurrentMod != null && CurrentMod.Metadata.WorkshopTitle != "" && CurrentMod.Metadata.Package != "") {
                    UploadToWorkshop();
                }
                else if (CurrentMod.Metadata.WorkshopTitle == "") {
                    UploadText = "Needs Title";
                }
                else if (CurrentMod.Metadata.Package == "") {
                    UploadText = "Needs Package";
                }
            }

            else if (ID == "SelectWorkshopTitle") {
                SelectedField = 2;
            }

            else if (ID == "SelectWorkshopDesc") {
                SelectedField = 3;
            }

            else if (ID == "SelectChangelog") {
                SelectedField = 4;
            } 
            else if (ID == "SelectPackage") {
                SelectedField = 1;
            }
            else if (ID == "ToggleLocal") {
                LocalModList = !LocalModList;
                FetchMods();
            }

            else if (ID == "SelectMapName") {
                SelectedField = 1;
            }

            else if (ID == "SelectWorldArea") {
                SelectedField = 2;
            }

            else if (ID == "PlaytestPicross") {
                GameLoop.UIManager.Minigames.ToggleMinigame("Picross");
                PicrossPuzzle picTest = CurrentMod.ModPicross[ModItemIndex];
                GameLoop.UIManager.Minigames.Picross.Reset();
                GameLoop.UIManager.Minigames.Picross.Setup(picTest.Difficulty);
                GameLoop.UIManager.Minigames.Picross.Current = picTest;
                GameLoop.UIManager.Minigames.Picross.Timer = 999;
            }

            else {
                string[] splits = ID.Split(";");

                if (splits[0] == "Load") {
                    if (InstalledMods.ContainsKey(splits[1])) {
                        CurrentMod = InstalledMods[splits[1]];
                        ModMenuSelect = "Overview";
                        CreatingMod = false;
                    } 
                } 
            }
        }

        public void FetchMods() {
            InstalledMods.Clear();

            string path = "./mods/";

            if (!LocalModList)
                path = SteamApps.AppInstallDir() + "/../../workshop/content/1906540/";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
              
            foreach (var modPath in Directory.GetDirectories(path)) {
                string fileName = Directory.GetFiles(modPath)[0];
                Mod mod = Helper.DeserializeFromFileCompressed<Mod>(fileName);

                if (!InstalledMods.ContainsKey(mod.Metadata.WorkshopTitle))
                    InstalledMods.Add(mod.Metadata.WorkshopTitle, mod);
            }
        }

        public void DrawMod() {
            Con.Clear(); 

            Con.DrawBox(new Rectangle(0, 0, 96, 50), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.White, Color.Black), new ColoredGlyph(Color.White, Color.Black)));

            Con.DrawLine(new Point(20, 1), new Point(20, 48), 179, Color.White, Color.Black);

            if (ModMenuSelect == "Create") {
                Con.PrintClickable(1, 1, "[BACK]", ModMakerClicks, "BackToOverview");
                Con.PrintClickable(10, 1, "[SAVE]", ModMakerClicks, "SaveMod");
                 
                Con.Print(1, 3, "Name: ", Color.DarkSlateGray);
                Con.Print(1, 4, Helper.Truncate(CurrentMod.Metadata.WorkshopTitle, 14));

                Con.Print(1, 5, "Package:", Color.DarkSlateGray);
                Con.Print(1, 6, Helper.Truncate(CurrentMod.Metadata.Package, 14));


                Con.PrintClickable(1, 8, "Back to Overview", ModMakerClicks, "GoOverview");
                Con.PrintClickable(1, 10, "Constructibles", ModMakerClicks, "Go Constructibles");
                Con.PrintClickable(1, 12, "Crafting Recipes", ModMakerClicks, "Go Recipes"); 
                Con.PrintClickable(1, 14, "Items", ModMakerClicks, "Go Items");
                Con.PrintClickable(1, 16, "Missions", ModMakerClicks, "Go Missions");
                Con.PrintClickable(1, 18, "Monsters", ModMakerClicks, "Go Monsters");
                Con.PrintClickable(1, 20, "NPCs", ModMakerClicks, "Go NPCs");
                Con.PrintClickable(1, 22, "Skills", ModMakerClicks, "Go Skills");
                Con.PrintClickable(1, 24, "Artifacts", ModMakerClicks, "Go Artifacts");
                Con.PrintClickable(1, 26, "Picross", ModMakerClicks, "Go Picross");
                Con.PrintClickable(1, 28, "Tiles", ModMakerClicks, "Go Tiles");
                Con.PrintClickable(1, 30, "Maps", ModMakerClicks, "Go Maps");

                Con.Print(1, 44, UploadText);

                if (ModSubSelect == "Overview") {
                    Con.Print(21, 1, "Listen.", Color.Coral);
                    Con.Print(21, 2, "I tried to make a whole built-in mod thing, I really did.", Color.SkyBlue);
                    Con.Print(21, 3, "But there's so much data attached to everything that ", Color.Coral);
                    Con.Print(21, 4, "it's just not feasible to have all of it packed in here.", Color.SkyBlue);
                    Con.Print(21, 6, "So instead, this menu is mostly documentation for how to", Color.Coral);
                    Con.Print(21, 7, "define each type of data, and then tools to pack it up.", Color.SkyBlue);

                    Con.PrintClickable(21, 10, new ColoredString("Click this and type to enter the Workshop Title:", SelectedField == 2 ? Color.Yellow : Color.DarkSlateGray, Color.Black), ModMakerClicks, "SelectWorkshopTitle");
                    Con.Print(21, 11, CurrentMod.Metadata.WorkshopTitle);

                    Con.PrintClickable(21, 13, new ColoredString("Click this and type to enter the Package ID:", SelectedField == 1 ? Color.Yellow : Color.DarkSlateGray, Color.Black), ModMakerClicks, "SelectPackage");
                    Con.Print(21, 14, CurrentMod.Metadata.Package);

                    Con.PrintClickable(21, 17, new ColoredString("Click this and type to enter the Workshop Description:", SelectedField == 3 ? Color.Yellow : Color.DarkSlateGray, Color.Black), ModMakerClicks, "SelectWorkshopDesc");

                    string[] allDescLines = CurrentMod.Metadata.WorkshopDesc.Split((char)10);

                    for (int i = 0; i < allDescLines.Length; i++) { 
                        Con.Print(21, 18 + i, allDescLines[i]);
                    } 

                    Con.PrintClickable(21, 25, new ColoredString("Click this and type to enter the Update Changelog:", SelectedField == 4 ? Color.Yellow : Color.DarkSlateGray, Color.Black), ModMakerClicks, "SelectChangelog");
                    string[] allChangelogLines = CurrentChangelog.Split((char)10);

                    for (int i = 0; i < allChangelogLines.Length; i++) {
                        Con.Print(21, 26 + i, allChangelogLines[i]);
                    }
                }


                else if (ModSubSelect == "Constructible") {
                    Con.Print(21, 1, "Constructibles: Furniture or tiles you can place using Construction", Color.Teal);
                    Con.Print(21, 4, "Easy Fields: (Fields that don't require much extra work to set)", Color.DarkSlateGray);
                    Con.Print(21, 5, "  - Name: The name of the construction in the construction menu.", Color.Coral);
                    Con.Print(21, 7, "  - Package: An identifier for things from your mod/mods.", Color.SkyBlue);
                    Con.Print(21, 9, "  - Materials: A brief string with the material requirements if the", Color.Coral);
                    Con.Print(21, 10, "               player doesn't have all the materials when building.", Color.Coral);
                    Con.Print(21, 12, "  - Glyph: The glyph index, in the core font sheet, of the tile.", Color.SkyBlue);
                    Con.Print(21, 14, "  - ForegroundR: The red value of the tile's RGB color.", Color.DarkRed);
                    Con.Print(21, 16, "  - ForegroundG: The green value of the tile's RGB color.", Color.DarkGreen);
                    Con.Print(21, 18, "  - ForegroundB: The blue value of the tile's RGB color.", Color.CadetBlue);
                    Con.Print(21, 20, "  - RequiredLevel: The Construction level required to build this.", Color.Coral);
                    Con.Print(21, 22, "  - ExpGranted: The amount of experience granted for building.", Color.SkyBlue);
                    Con.Print(21, 24, "  - BlocksMove: Whether or not the tile blocks player movement.", Color.Coral);
                    Con.Print(21, 26, "  - BlocksLOS: Whether or not the tile blocks player vision.", Color.SkyBlue);
                    Con.Print(21, 28, "  - Furniture: Only furniture can be built in apartments", Color.Coral);
                    Con.Print(21, 32, "Hard Fields: (Fields that require a bit of extra work)", Color.DarkSlateGray);
                    Con.Print(21, 33, "  - " + new ColoredString("Dec: Decorators are rendered OVER the main tile to add detail.", Color.DarkSlateBlue, Color.Black));
                    Con.Print(21, 34, "  -  -     " + new ColoredString("R: The red value of the decorator.", Color.DarkRed, Color.Black));
                    Con.Print(21, 35, "  -  -     " + new ColoredString("G: The green value of the decorator.", Color.DarkGreen, Color.Black));
                    Con.Print(21, 36, "  -  -     " + new ColoredString("B: The blue value of the decorator.", Color.CadetBlue, Color.Black));
                    Con.Print(21, 37, "  -  - Glyph: The symbol the decorator draws over the main glyph.", Color.SkyBlue);
                    Con.Print(21, 39, "  - " + new ColoredString("MaterialsNeeded: A detailed list of all ingredients needed.", Color.DarkSlateBlue, Color.Black));
                    Con.Print(21, 40, "  -  - ID: The specific ID (package:name) of the material.", Color.Coral);
                    Con.Print(21, 41, "  -  - ItemQuantity: The amount of this item needed per tile.", Color.SkyBlue);
                }


                else if (ModSubSelect == "Recipe") {
                    List<ColoredString> Lines = new();
                    Lines.Add(new("Easy Fields: (Fields that don't require much extra work to set)", Color.DarkSlateGray, Color.Black));
                    Lines.Add(new("  - Name: The recipe name (ex, '2x Ladder')", Color.Coral, Color.Black)); 
                    Lines.Add(new("  - FinishedID: The ID of the finished item (ex, 'lh:Ladder')", Color.SkyBlue, Color.Black)); 
                    Lines.Add(new("  - FinishedQty: The amount of finished item this recipe gives.", Color.Coral, Color.Black)); 
                    Lines.Add(new("  - Skill: The skill used to craft this recipe.", Color.SkyBlue, Color.Black)); 
                    Lines.Add(new("  - RequiredLevel: The level needed in the required skill.", Color.Coral, Color.Black)); 
                    Lines.Add(new("  - ExpGranted: The amount of exp granted per craft.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("")); 
                    Lines.Add(new("Hard Fields: (Fields that require a bit of extra work)", Color.DarkSlateGray, Color.Black));
                    Lines.Add(new("  - SpecificMaterials: Materials needed by ID", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("  - - ID: The ID of the material (ex, 'lh:Copper Ingot')", Color.Coral, Color.Black));
                    Lines.Add(new("  - - ItemQuantity: Number of material needed", Color.SkyBlue, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new("  - GenericMaterials: General materials needed by tag", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("  - - Property: The name of the tag on the material", Color.Coral, Color.Black));
                    Lines.Add(new("  - - Tier: The tier of material need (ex, Copper is Tier 10)", Color.SkyBlue, Color.Black));
                    Lines.Add(new("  - - Quantity: The amount of the material needed", Color.Coral, Color.Black));
                    Lines.Add(new("  - - CountsAsMultiple: Whether or not the material counts for", Color.SkyBlue, Color.Black));
                    Lines.Add(new("  - - - multiple. For example Fuel: Wood is worth 1, Coal is 5.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("  - - - The Tier is used for this calculation.", Color.SkyBlue, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new("  - RequiredTools: Tool tags needed on inventory items to craft.", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("  - - Property: The name of the tag on the material", Color.Coral, Color.Black));
                    Lines.Add(new("  - - Tier: The required tool tier for an item to qualify.", Color.SkyBlue, Color.Black));

                    Con.Print(21, 1, "Recipes: Definitions for crafting items via the crafting menu.", Color.Teal);


                    for (int i = DocumentationTop; i < DocumentationTop + 46 && i < Lines.Count; i++) {
                        int line = i - DocumentationTop;
                        Con.Print(21, 3 + line, Lines[i]);
                    }
                }
                 
                else if (ModSubSelect == "Item") {
                    List<ColoredString> Lines = new();

                    Lines.Add(new("Easy Fields: (Fields that don't require much extra work to set)", Color.DarkSlateGray, Color.Black));
                    Lines.Add(new("- Name: The item name", Color.Coral, Color.Black));
                    Lines.Add(new("- Package: The mod package, used to form the full ID.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- SubID: This is checked to see if items are identical.", Color.Coral, Color.Black));
                    Lines.Add(new("- ItemQuantity: Don't set this, it gets overwritten anyways.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- IsStackable: Whether or not the item stacks in your inventory.", Color.Coral, Color.Black));
                    Lines.Add(new("- ShortDesc: The description shown in shops or the inventory screen.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- Description: The full description for specific item views.", Color.Coral, Color.Black));
                    Lines.Add(new("- AverageValue: The value of the item before relationship modifiers.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- Weight: The weight of the item in kilograms, currently unused.", Color.Coral, Color.Black));
                    Lines.Add(new("- Durability: How much durability the item starts with.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- MaxDurability: The maximum durability the item can be repaired to.", Color.Coral, Color.Black));
                    Lines.Add(new("- Quality: Don't set this, it gets overwritten.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- ItemTier: The tier of the item. Usually multiples of 10 (10/20/90)", Color.Coral, Color.Black));
                    Lines.Add(new("- ItemCat: The category of the item, to detect which effect to use", Color.SkyBlue, Color.Black)); 
                    Lines.Add(new("- - Some categories: Hoe, Hammer, Pickaxe, Fishing Rod, Hatchet,", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - NameTag, Animal, Pet, AnimalFeed, PetChow, AnimalBed, PetBed,", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - Watering Can, Seed, Camera, Shears, MilkBucket, Brush", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- EquipSlot: -1 for none, otherwise 0-10 for which equipment slot", Color.Coral, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new ColoredString("- ") + new ColoredString("ForegroundR: The red value of the tile's RGB color.", Color.DarkRed, Color.Black));
                    Lines.Add(new ColoredString("- ") + new ColoredString("ForegroundG: The green value of the tile's RGB color.", Color.DarkGreen, Color.Black));
                    Lines.Add(new ColoredString("- ") + new ColoredString("ForegroundB: The blue value of the tile's RGB color.", Color.CadetBlue, Color.Black));
                    Lines.Add(new("- ItemGlyph: The primary symbol the item uses for it's appearance.", Color.SkyBlue, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new("The following items are in the format package:name, ex 'lh:test'", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- LeftClickScript: Activated when left clicking with the item.", Color.Coral, Color.Black)); 
                    Lines.Add(new("- RightClickScript: Activated when right clicking with the item.", Color.SkyBlue, Color.Black)); 
                    Lines.Add(new("- LeftClickHoldScript: Activated when left click is released.", Color.Coral, Color.Black));
                    Lines.Add(new("")); 
                    Lines.Add(new("Hard Fields: (Fields that require a bit of extra work)", Color.DarkSlateGray, Color.Black));
                    Lines.Add(new("- Dec: Decorators are rendered OVER the main tile to add detail.", Color.DarkSlateBlue, Color.Black));
                    Lines.Add("- - " + new ColoredString("R: The red value of the decorator.", Color.DarkRed, Color.Black));
                    Lines.Add("- - " + new ColoredString("G: The green value of the decorator.", Color.DarkGreen, Color.Black));
                    Lines.Add("- - " + new ColoredString("B: The blue value of the decorator.", Color.CadetBlue, Color.Black));
                    Lines.Add(new("- Plant: Usually used for seeds, detailing growth stages.", Color.DarkSlateBlue, Color.Black)); 
                    Lines.Add(new("- (Strap in, because this one is crazy complicated)", Color.DarkSlateGray, Color.Black)); 
                    Lines.Add(new("- - GrowthSeason: Spring/Summer/Fall/Winter/Holiday/Any ", Color.Coral, Color.Black)); 
                    Lines.Add(new("- - HarvestRevert: The stage the plant returns to when harvesting", Color.SkyBlue, Color.Black));
                    Lines.Add(new("              (-1 if the plant doesn't revert to an earlier stage) ", Color.DarkSlateGray, Color.Black));
                    Lines.Add(new("- - ProduceName: The name of the crop, mostly used for mouse-over", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - RequiredLevel: The level needed to plant this plant.", Color.Coral, Color.Black));
                    Lines.Add(new("- - ExpOnHarvest: The exp granted by harvesting this plant.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - ExpPerExtra: If this plant has multiple fruits per harvest,", Color.Coral, Color.Black));
                    Lines.Add(new("                 this is the bonus exp per extra item.", Color.Coral, Color.Black));
                    Lines.Add(new("- - ProducePerHarvestMin: Minimum output per harvest", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - ProducePerHarvestMax: Maximum output per harvest", Color.Coral, Color.Black));
                    Lines.Add(new("- - ProduceIsSeed: Whether or not the output can be planted ", Color.SkyBlue, Color.Black));
                    Lines.Add(new("                   as a seed. (ex, potatoes) ", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - Stages: This is a list of the stages the crop has. ", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - - DaysToNext: Days spent in this stage. At 0, grows the ", Color.Coral, Color.Black));
                    Lines.Add(new("                  next day. Must be watered every day to grow.", Color.Coral, Color.Black));
                    Lines.Add(new("- - - HarvestItem: The ID of the item given if harvested at this", Color.SkyBlue, Color.Black));
                    Lines.Add(new("                   stage. Leave empty if it can't be harvested yet.", Color.SkyBlue, Color.Black));
                    Lines.Add("- - - " + new ColoredString("ColorR: Appearance red value of base symbol", Color.DarkRed, Color.Black));
                    Lines.Add("- - - " + new ColoredString("ColorG: Appearance green value of base symbol", Color.DarkGreen, Color.Black));
                    Lines.Add("- - - " + new ColoredString("ColorB: Appearance blue value of base symbol", Color.CadetBlue, Color.Black));
                    Lines.Add(new("- - - Glyph: The base symbol this stage uses.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - - Dec: (as above)", Color.Coral, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new("- Tool: A list of all the crafting properties a tool has", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - Property: The property recipes search for ", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - Tier: The tier this tool has (ex, Copper = Tier 10) ", Color.Coral, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new("- Craft: A list of all the generic material properties an item has", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - Property: The name of the tag on the material", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - Tier: The tier of material need (ex, Copper is Tier 10)", Color.Coral, Color.Black));
                    Lines.Add(new("- - Quantity: The amount of the material needed", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - CountsAsMultiple: Whether or not the material counts for", Color.Coral, Color.Black));
                    Lines.Add(new("- - - multiple. For example Fuel: Wood is worth 1, Coal is 5.", Color.Coral, Color.Black));
                    Lines.Add(new("- - - The Tier is used for this calculation.", Color.Coral, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new("- Stats: Equipment stats when wielded by a player.", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - DamageType: The damage alignment it deals (if a weapon)", Color.SkyBlue, Color.Black));
                    Lines.Add(new("               (Wood, Fire, Earth, Metal, or Water aligned)", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - Physical: true if physical, false if magical", Color.Coral, Color.Black));
                    Lines.Add(new("- - Power: The amount of damage it deals (if a weapon)", Color.Coral, Color.Black));
                    Lines.Add(new("- - Accuracy: Accuracy of the item (if a weapon)", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - Armor: Defense against physical attacks", Color.Coral, Color.Black));
                    Lines.Add(new("- - MagicArmor: Defense against magic attacks", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- Heal: Used for items that healing potions", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - HealAmount: Die format for amount healed (ex, 1d6)", Color.Coral, Color.Black));
                    Lines.Add(new("- SpawnAnimal: If the item can be used to spawn an animal", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("- - Species: The name of the animal (ex, Pig)", Color.Coral, Color.Black));
                    Lines.Add(new("- - Glyph, R, G, B: These work the same as always", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - MilkItem: ID of item given when milked (ex, lh:Milk Bottle)", Color.Coral, Color.Black));
                    Lines.Add(new("- - ShearItem: ID of item given when sheared (ex, lh:Ball of Wool)", Color.SkyBlue, Color.Black)); 
                    Lines.Add(new("- - AdultAge: The amount of days to reach adulthood (ex, 10)", Color.Coral, Color.Black)); 
                    Lines.Add(new("- - MilkItem: ID of item given when milked (ex, lh:Milk Bottle)", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- - Pet: Whether this is a pet (true) or a farm animal (false)", Color.Coral, Color.Black));
                    Lines.Add(new("( Don't set Photo, SoulPhoto, or Artifact. These are for )", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new("( internal assigning and saving/loading )", Color.DarkSlateBlue, Color.Black));

                    Con.Print(21, 1, "Items: Definitions for items you can hold and use.", Color.Teal); 

                    for (int i = DocumentationTop; i < DocumentationTop + 46 && i < Lines.Count; i++) {
                        int line = i - DocumentationTop;
                        Con.Print(21, 3 + line, Lines[i]);
                    }
                }

                else if (ModSubSelect == "Mission") {
                    List<ColoredString> Lines = new();

                    Lines.Add(new("Easy Fields: (Fields that don't require much extra work to set)", Color.DarkSlateGray, Color.Black));
                    Lines.Add(new("- Name: The name of the mission", Color.Coral, Color.Black));
                    Lines.Add(new("- Package: Used to form the full ID of the mission", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- Chapter: Which chapter the mission show up under in the log", Color.Coral, Color.Black));


                    Con.Print(21, 1, "Missions: Story quests for the player to complete", Color.Teal);

                    for (int i = DocumentationTop; i < DocumentationTop + 46 && i < Lines.Count; i++) {
                        int line = i - DocumentationTop;
                        Con.Print(21, 3 + line, Lines[i]);
                    }
                }

                else if (ModSubSelect == "Monster") {
                    List<ColoredString> Lines = new();

                    Lines.Add(new("Easy Fields: (Fields that don't require much extra work to set)", Color.DarkSlateGray, Color.Black));
                    Lines.Add(new("- Species: The name of the monster", Color.Coral, Color.Black));
                    Lines.Add(new("- Package: Used to form the full ID of the monster", Color.SkyBlue, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new("- Health: The base Health stat for the monster.", Color.Coral, Color.Black));
                    Lines.Add(new("- Speed: The base Speed stat for the monster.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- Attack: The base Attack stat for the monster.", Color.Coral, Color.Black));
                    Lines.Add(new("- Defense: The base Defense stat for the monster.", Color.SkyBlue, Color.Black));
                    Lines.Add(new("- MAttack: The base Magic Attack stat for the monster.", Color.Coral, Color.Black));
                    Lines.Add(new("- MDefense: The base Magic Defense stat for the monster.", Color.SkyBlue, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new("- StatGranted: The stat EXP granted when killed (format: 'Attack,1')", Color.Coral, Color.Black));
                    Lines.Add(new("- SpawnLocation: The tag or tags used to spawn the monster, or 'Any'.", Color.SkyBlue, Color.Black)); 
                    Lines.Add(new("- ElementalAlignment: The elemental type. Wood-Fire-Earth-Metal-Water.", Color.Coral, Color.Black)); 
                    Lines.Add(new("- Types: A list of all non-element types the monster has.", Color.SkyBlue, Color.Black)); 
                    Lines.Add(new("- MoveList: A list of all moves the monster can use (format: '1,lh:Bite')", Color.Coral, Color.Black));
                    Lines.Add(new("- DropTable: A list of drops the monster has. (format: '1,4,1,lh:Core Fragment')", Color.SkyBlue, Color.Black));
                    Lines.Add(new("    First number is number to generate, second is max number, third is quantity.", Color.DarkSlateBlue, Color.Black));
                    Lines.Add(new(""));
                    Lines.Add(new ColoredString("- ") + new ColoredString("ForegroundR: The red value of the monster's RGB color.", Color.DarkRed, Color.Black));
                    Lines.Add(new ColoredString("- ") + new ColoredString("ForegroundG: The green value of the monster's RGB color.", Color.DarkGreen, Color.Black));
                    Lines.Add(new ColoredString("- ") + new ColoredString("ForegroundB: The blue value of the monster's RGB color.", Color.CadetBlue, Color.Black));
                    Lines.Add(new("- ActorGlyph: A letter that the monster uses for appearance (format: 'h')", Color.SkyBlue, Color.Black));
                    Con.Print(21, 1, "Missions: Story quests for the player to complete", Color.Teal);

                    for (int i = DocumentationTop; i < DocumentationTop + 46 && i < Lines.Count; i++) {
                        int line = i - DocumentationTop;
                        Con.Print(21, 3 + line, Lines[i]);
                    }
                }

                else if (ModSubSelect == "NPC") {
                    Con.Print(21, 1, "NPC");
                }

                else if (ModSubSelect == "Skill") {
                    Con.Print(21, 1, "Skills: Self explanatory, literally one field.", Color.Teal);
                    Con.Print(21, 4, " - Name: The name of the skill"); 
                }

                else if (ModSubSelect == "Artifact") {
                    Con.Print(22, 1, "You'll have to edit a lot of this data externally too.");
                    Con.Print(22, 3, "Name: " + CurrentMod.ModArtifacts[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);
                    Con.Print(22, 5, "Glyph: " + CurrentGlyphIndex.AsString(), new Color(PaintR, PaintG, PaintB));
                    Con.Print(22, 7, "ForeR: " + PaintR);
                    Con.Print(22, 9, "ForeG: " + PaintG);
                    Con.Print(22, 11, "ForeB: " + PaintB);


                    Helper.DrawBox(Con, 49, 4, 30, 30);

                    Dictionary<Color, int> ColorsUsed = new();

                    for (int x = 0; x < 30; x++) {
                        for (int y = 0; y < 30; y++) {
                            Con.Print(50 + x, 5 + y, CurrentMod.ModArtifacts[ModItemIndex].Tiles[x + (y * 30)].GetAppearance());

                            ArchTile archTile = CurrentMod.ModArtifacts[ModItemIndex].Tiles[x + (y * 30)];
                            Color archColor = new Color(archTile.ForeR, archTile.ForeG, archTile.ForeB);
                            if (ColorsUsed.ContainsKey(archColor))
                                ColorsUsed[archColor]++;
                            else
                                ColorsUsed.Add(archColor, 1);
                        }
                    }
                     
                    for (int i = 0; i < 400; i++) {
                        int x = i % 32;
                        int y = i / 32;
                        Color col = i == CurrentGlyphIndex ? Color.Lime : Color.White;
                        Con.PrintClickable(49 + x, 36 + y, new ColoredString(i.AsString(), col, Color.Black), SetGlyph, i.ToString());
                        
                    }


                    int line = 15;
                    foreach (KeyValuePair<Color, int> kv in ColorsUsed) {
                        string TileCount = kv.Value.ToString();
                        string col = kv.Key.R + "," + kv.Key.G + "," + kv.Key.B;
                        Con.PrintClickable(21, line, new ColoredString(col, kv.Key, Color.Black) + ": " + TileCount, PicrossColorMemory, col);
                        line++;
                    }
                }

                else if (ModSubSelect == "Picross") {
                    Con.Print(22, 1, "Clues are generated automatically during play.");
                    Con.Print(22, 3, "Name: " + CurrentMod.ModPicross[ModItemIndex].Name, SelectedField == 2 ? Color.Yellow : Color.White);
                    Con.Print(22, 5, "Difficulty: ");

                    string diff = CurrentMod.ModPicross[ModItemIndex].Difficulty;
                    Con.PrintClickable(34, 5, new ColoredString("E", diff == "Easy" ? Color.Lime : Color.White, Color.Black), ModMakerClicks, "PicrossEasy");
                    Con.PrintClickable(36, 5, new ColoredString("M", diff == "Medium" ? Color.Yellow : Color.White, Color.Black), ModMakerClicks, "PicrossMedium");
                    Con.PrintClickable(38, 5, new ColoredString("H", diff == "Hard" ? Color.Red : Color.White, Color.Black), ModMakerClicks, "PicrossHard");

                    Con.Print(22, 7, "ForeR: " + PaintR);
                    Con.Print(22, 9, "ForeG: " + PaintG);
                    Con.Print(22, 11, "ForeB: " + PaintB);

                    Con.PrintClickable(22, 13, "Playtest Puzzle", ModMakerClicks, "PlaytestPicross");
                     
                    Helper.DrawBox(Con, 49, 4, CurrentMod.ModPicross[ModItemIndex].Width, CurrentMod.ModPicross[ModItemIndex].Height);

                    Dictionary<Color, int> ColorsUsed = new();


                    for (int x = 0; x < CurrentMod.ModPicross[ModItemIndex].Width; x++) {
                        for (int y = 0; y < CurrentMod.ModPicross[ModItemIndex].Height; y++) {
                            Con.Print(50 + x, 5 + y, CurrentMod.ModPicross[ModItemIndex].Grid[x + (y * CurrentMod.ModPicross[ModItemIndex].Width)].GetAppearance(true));
                            PicrossTile thisTile = CurrentMod.ModPicross[ModItemIndex].Grid[x + (y * CurrentMod.ModPicross[ModItemIndex].Width)];
                            if (thisTile.PartOfSolution) {
                                Color thisColor = new(thisTile.R, thisTile.G, thisTile.B);
                                if (ColorsUsed.ContainsKey(thisColor))
                                    ColorsUsed[thisColor]++;
                                else
                                    ColorsUsed.Add(thisColor, 1);
                            }
                        }
                    }

                    int line = 15;
                    foreach(KeyValuePair<Color, int> kv in ColorsUsed) {
                        string TileCount = kv.Value.ToString();
                        string col = kv.Key.R + "," + kv.Key.G + "," + kv.Key.B;
                        Con.PrintClickable(21, line, new ColoredString(col, kv.Key, Color.Black) + ": " + TileCount, PicrossColorMemory, col);
                        line++;        
                    }
                }

                else if (ModSubSelect == "Tile") {

                } 

                Con.PrintClickable(1, 48, "[PACK]", ModMakerClicks, "PackMod");
                Con.PrintClickable(11, 48, "[UNPACK]", ModMakerClicks, "UnpackMod");

                Con.PrintClickable(1, 46, "[WORKSHOP UPLOAD]", ModMakerClicks, "Upload");


                if (ModSubSelect == "Artifact" || ModSubSelect == "Picross" || ModSubSelect == "Map") {
                    Con.PrintClickable(21, 48, "Previous", ModMakerClicks, "PreviousItem");
                    Con.PrintClickable(85, 48, "Next", ModMakerClicks, "NextItem");
                }
            }

            if (ModMenuSelect == "List") {
                Con.PrintClickable(1, 1, "[BACK]", ModMakerClicks, "BackToMainMenu");
                Con.PrintClickable(10, 1, "[NEW]", ModMakerClicks, "CreateNewMod");


                Con.Print(1, 3, LocalModList ? "Viewing Local" : "Viewing Workshop");
                if (LocalModList)
                    Con.PrintClickable(1, 4, "[WORKSHOP MODS]", ModMakerClicks, "ToggleLocal");
                else 
                    Con.PrintClickable(1, 4, "[LOCAL MODS]", ModMakerClicks, "ToggleLocal");

                if (!LocalModList) {
                    Con.Print(1, 6, "Must be in local");
                    Con.Print(1, 7, "view to edit mods");
                }

                string path = "./mods/";

                if (!LocalModList)
                    path = SteamApps.AppInstallDir() + "/../../workshop/content/1906540/";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                Con.Print(21, 1, "Mod Name".Align(HorizontalAlignment.Center, 50) + "|" + "Enabled".Align(HorizontalAlignment.Center, 9));
                Con.DrawLine(new Point(21, 2), new Point(88, 2), 196, Color.White, Color.Black);

                int y = 3;

                foreach (var kv in InstalledMods) {
                    Mod mod = kv.Value;
                    Con.PrintClickable(75, y, Helper.Checkmark(GameLoop.SteamManager.ModsEnabled.Contains(mod.Metadata.PublishedID.Value)), ToggleClick, mod.Metadata.PublishedID.Value.ToString());

                    Con.PrintClickable(21, y, mod.Metadata.WorkshopTitle.Align(HorizontalAlignment.Center, 50), ModMakerClicks, "Load;" + mod.Metadata.WorkshopTitle);

                    y++;
                }
            }

            if (ModMenuSelect == "Overview") {
                Con.PrintClickable(1, 1, "[BACK]", ModMakerClicks, "BackToList");

                if (LocalModList)
                    Con.PrintClickable(10, 1, "[EDIT]", ModMakerClicks, "ToCreateEdit");

                if (CurrentMod != null) { 
                    Con.Print(1, 3, Helper.Truncate(CurrentMod.Metadata.WorkshopTitle, 20));

                    Con.PrintClickable(1, 4, "Back to Overview", ModMakerClicks, "GoOverview");
                     

                    if (CurrentMod.ModItems.Count > 0)
                        Con.PrintClickable(1, 12, "Items", ModMakerClicks, "OverviewItems");
                    else
                        Con.Print(1, 12, new ColoredString("Items", Color.DarkSlateGray, Color.Black));
                     

                    if (CurrentMod.ModMonsters.Count > 0)
                        Con.PrintClickable(1, 16, "Monsters", ModMakerClicks, "OverviewMonsters");
                    else
                        Con.Print(1, 16, new ColoredString("Monsters", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModNPCs.Count > 0)
                        Con.PrintClickable(1, 18, "NPCs", ModMakerClicks, "OverviewNPCs");
                    else
                        Con.Print(1, 18, new ColoredString("NPCs", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModSkills.Count > 0)
                        Con.PrintClickable(1, 20, "Skills", ModMakerClicks, "OverviewSkills");
                    else
                        Con.Print(1, 20, new ColoredString("Skills", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModArtifacts.Count > 0)
                        Con.PrintClickable(1, 22, "Artifacts", ModMakerClicks, "OverviewArtifacts");
                    else
                        Con.Print(1, 22, new ColoredString("Artifacts", Color.DarkSlateGray, Color.Black));

                    if (CurrentMod.ModPicross.Count > 0)
                        Con.PrintClickable(1, 24, "Picross", ModMakerClicks, "OverviewPicross");
                    else
                        Con.Print(1, 24, new ColoredString("Picross", Color.DarkSlateGray, Color.Black));


                    if (ModSubSelect == "None" || ModSubSelect == "Overview") {
                        Con.Print(21, 1, "  Mod Name: " + CurrentMod.Metadata.WorkshopTitle);
                        Con.Print(21, 3, "Mod Prefix: " + CurrentMod.Metadata.Package);

                        ColoredString check = new ColoredString(4.AsString(), Color.Lime, Color.Black);
                        if (!CurrentMod.Enabled)
                            check = new ColoredString("x", Color.Red, Color.Black);
                        Con.Print(21, 5, "Enabled: ");
                        Con.PrintClickable(30, 5, check, ToggleClick, CurrentMod.Metadata.PublishedID.Value.ToString());
                         
                        Con.Print(21, 12, "Scripts Added: " + CurrentMod.ModScripts.Count);
                        Con.Print(21, 14, "Items Added: " + CurrentMod.ModItems.Count); 
                        Con.Print(21, 18, "Monsters Added: " + CurrentMod.ModMonsters.Count);
                        Con.Print(21, 20, "NPCs Added: " + CurrentMod.ModNPCs.Count);
                        Con.Print(21, 22, "Skills Added: " + CurrentMod.ModSkills.Count);
                        Con.Print(21, 24, "Artifacts Added: " + CurrentMod.ModArtifacts.Count);
                        Con.Print(21, 26, "Picross Puzzles Added: " + CurrentMod.ModPicross.Count); 
                    }
                    else if (ModSubSelect == "Artifact") {
                        Helper.DrawBox(Con, 22, 4, 30, 30);

                        Con.Print(21, 1, new ColoredString("Name: ", Color.DarkSlateGray, Color.Black) + new ColoredString(CurrentMod.ModArtifacts[ModItemIndex].Name, Color.White, Color.Black));

                        for (int x = 0; x < 30; x++) {
                            for (int y = 0; y < 30; y++) {
                                Con.Print(23 + x, 5 + y, CurrentMod.ModArtifacts[ModItemIndex].Tiles[x + (y * 30)].GetAppearance()); 
                            }
                        }
                    }
                    else if (ModSubSelect == "Picross") {
                        Con.Print(21, 1, new ColoredString("Name: ", Color.DarkSlateGray, Color.Black) + new ColoredString(CurrentMod.ModPicross[ModItemIndex].Name, Color.White, Color.Black));
                        Con.Print(21, 2, new ColoredString("Difficulty: ", Color.DarkSlateGray, Color.Black) + new ColoredString(CurrentMod.ModPicross[ModItemIndex].Difficulty, Color.White, Color.Black));


                        Helper.DrawBox(Con, 22, 4, CurrentMod.ModPicross[ModItemIndex].Width, CurrentMod.ModPicross[ModItemIndex].Height);
                         
                        for (int x = 0; x < CurrentMod.ModPicross[ModItemIndex].Width; x++) {
                            for (int y = 0; y < CurrentMod.ModPicross[ModItemIndex].Height; y++) {
                                Con.Print(23 + x, 5 + y, CurrentMod.ModPicross[ModItemIndex].Grid[x + (y * CurrentMod.ModPicross[ModItemIndex].Width)].GetAppearance(true));
                                PicrossTile thisTile = CurrentMod.ModPicross[ModItemIndex].Grid[x + (y * CurrentMod.ModPicross[ModItemIndex].Width)]; 
                            }
                        }
                    }
                }
            }

        } 

        public void PicrossColorMemory(string ID) {
            string[] split = ID.Split(",");

            PaintR = Int32.Parse(split[0]);
            PaintG = Int32.Parse(split[1]);
            PaintB = Int32.Parse(split[2]);
        }

        public void SetGlyph(string ID) {
            int index = Int32.Parse(ID);
            CurrentGlyphIndex = index;
        }

        public void Input() {
            Point mousePos = new MouseScreenObjectState(Con, GameHost.Instance.Mouse).CellPosition;

            if (CurrentMod != null && ModMenuSelect == "Create") {
                foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                    if ((key.Character >= 'A' && key.Character <= 'z') || (key.Character >= '0' && key.Character <= '9'
                        || key.Character == ';' || key.Character == ':' || key.Character == '|' || key.Character == '-' || key.Character == '+'
                        || key.Character == '.' || key.Character == ',')) {
                        RunInput(key.Character.ToString(), false);
                    }
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Space)) {
                    RunInput(" ", false);
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Back)) {
                    RunInput("", true);
                }

                if (GameHost.Instance.Keyboard.IsKeyPressed(Key.Enter)) {
                    RunInput(10.AsString(), false);
                }

                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (mousePos.Y == 3 && mousePos.X < 20)
                        SelectedField = 0;
                    if (mousePos.Y == 5 && mousePos.X < 20)
                        SelectedField = 1;
                }
                 
                if (ModMenuSelect == "Create" && ModSubSelect == "Artifact") {
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                        if (mousePos.Y == 5) {
                            if (!Helper.EitherShift())
                                CurrentGlyphIndex = Math.Clamp(CurrentGlyphIndex + 10, 0, 400);
                            else
                                CurrentGlyphIndex = Math.Clamp(CurrentGlyphIndex + 1, 0, 400);
                        }
                        else if (mousePos.Y == 7) {
                            if (!Helper.EitherShift())
                                PaintR = Math.Clamp(PaintR + 10, 0, 255);
                            else
                                PaintR = Math.Clamp(PaintR + 1, 0, 255);
                        }
                        else if (mousePos.Y == 9) {
                            if (!Helper.EitherShift())
                                PaintG = Math.Clamp(PaintG + 10, 0, 255);
                            else
                                PaintG = Math.Clamp(PaintG + 1, 0, 255);
                        }
                        else if (mousePos.Y == 11) {
                            if (!Helper.EitherShift())
                                PaintB = Math.Clamp(PaintB + 10, 0, 255);
                            else
                                PaintB = Math.Clamp(PaintB + 1, 0, 255);
                        }
                    }
                    else if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                        if (mousePos.Y == 5) {
                            if (!Helper.EitherShift())
                                CurrentGlyphIndex = Math.Clamp(CurrentGlyphIndex - 10, 0, 400);
                            else
                                CurrentGlyphIndex = Math.Clamp(CurrentGlyphIndex - 1, 0, 400);
                        }
                        else if (mousePos.Y == 7) {
                            if (!Helper.EitherShift())
                                PaintR = Math.Clamp(PaintR - 10, 0, 255);
                            else
                                PaintR = Math.Clamp(PaintR - 1, 0, 255);
                        }
                        else if (mousePos.Y == 9) {
                            if (!Helper.EitherShift())
                                PaintG = Math.Clamp(PaintG - 10, 0, 255);
                            else
                                PaintG = Math.Clamp(PaintG - 1, 0, 255);
                        }
                        else if (mousePos.Y == 11) {
                            if (!Helper.EitherShift())
                                PaintB = Math.Clamp(PaintB - 10, 0, 255);
                            else
                                PaintB = Math.Clamp(PaintB - 1, 0, 255);
                        }
                    }

                    if (GameHost.Instance.Mouse.LeftButtonDown) {
                        if (mousePos.Y == 3 && mousePos.X > 20 && mousePos.X < 40)
                            SelectedField = 2;


                        Point offset = mousePos - new Point(50, 5);

                        if (offset.X >= 0 && offset.Y >= 0 && offset.X <= 29 && offset.Y <= 29) {
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeR = PaintR;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeG = PaintG;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeB = PaintB;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].Glyph = CurrentGlyphIndex;
                        }
                    }

                    if (GameHost.Instance.Mouse.RightButtonDown) {
                        Point offset = mousePos - new Point(50, 5);

                        if (offset.X >= 0 && offset.Y >= 0 && offset.X <= 29 && offset.Y <= 29) {
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeR = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeG = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeB = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].Glyph = 32;
                        }
                    }

                    if (GameHost.Instance.Mouse.RightClicked) {
                        Point offset = mousePos - new Point(50, 5);

                        if (offset.X >= 0 && offset.Y >= 0 && offset.X <= 29 && offset.Y <= 29) {
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeR = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeG = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].ForeB = 0;
                            CurrentMod.ModArtifacts[ModItemIndex].Tiles[offset.ToIndex(30)].Glyph = 32;
                        }
                    }
                }

                if (ModMenuSelect == "Create" && ModSubSelect != "Picross" && ModSubSelect != "Artifact") {
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                        if (DocumentationTop > 5)
                            DocumentationTop -= 5;
                        else
                            DocumentationTop = 0;
                    }
                    else if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                        DocumentationTop += 5;
                    }

                    if (GameHost.Instance.Mouse.RightClicked) {
                        DocumentationTop = 0;
                    }
                }

                if (ModMenuSelect == "Create" && ModSubSelect == "Picross") {
                    if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) {
                        if (mousePos.Y == 7) {
                            if (!Helper.EitherShift())
                                PaintR = Math.Clamp(PaintR + 10, 0, 255);
                            else
                                PaintR = Math.Clamp(PaintR + 1, 0, 255);
                        }
                        else if (mousePos.Y == 9) {
                            if (!Helper.EitherShift())
                                PaintG = Math.Clamp(PaintG + 10, 0, 255);
                            else
                                PaintG = Math.Clamp(PaintG + 1, 0, 255);
                        }
                        else if (mousePos.Y == 11) {
                            if (!Helper.EitherShift())
                                PaintB = Math.Clamp(PaintB + 10, 0, 255);
                            else
                                PaintB = Math.Clamp(PaintB + 1, 0, 255);
                        }
                    }
                    else if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                        if (mousePos.Y == 7) {
                            if (!Helper.EitherShift())
                                PaintR = Math.Clamp(PaintR - 10, 0, 255);
                            else
                                PaintR = Math.Clamp(PaintR - 1, 0, 255);
                        }
                        else if (mousePos.Y == 9) {
                            if (!Helper.EitherShift())
                                PaintG = Math.Clamp(PaintG - 10, 0, 255);
                            else
                                PaintG = Math.Clamp(PaintG - 1, 0, 255);
                        }
                        else if (mousePos.Y == 11) {
                            if (!Helper.EitherShift())
                                PaintB = Math.Clamp(PaintB - 10, 0, 255);
                            else
                                PaintB = Math.Clamp(PaintB - 1, 0, 255);
                        }
                    }

                    if (GameHost.Instance.Mouse.LeftButtonDown) {
                        if (mousePos.Y == 3 && mousePos.X > 20 && mousePos.X < 40)
                            SelectedField = 2;


                        Point offset = mousePos - new Point(50, 5);

                        PicrossPuzzle current = CurrentMod.ModPicross[ModItemIndex];

                        if (offset.X >= 0 && offset.Y >= 0 && offset.X < current.Width && offset.Y < current.Height) {
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].R = PaintR;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].G = PaintG;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].B = PaintB;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].PartOfSolution = true;
                        }
                    }

                    if (GameHost.Instance.Mouse.RightButtonDown) {
                        Point offset = mousePos - new Point(50, 5);
                        PicrossPuzzle current = CurrentMod.ModPicross[ModItemIndex];
                        if (offset.X >= 0 && offset.Y >= 0 && offset.X < current.Width && offset.Y < current.Height) {
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].R = 0;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].G = 0;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].B = 0;
                            CurrentMod.ModPicross[ModItemIndex].Grid[offset.ToIndex(current.Width)].PartOfSolution = false;
                        }
                    }
                }
            }
            else if (ModMenuSelect == "Overview") {
                if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0) { 
                    if (ModSubSelect == "Picross")
                        ModItemIndex = Math.Clamp(ModItemIndex + 1, 0, CurrentMod.ModPicross.Count - 1); 
                    else if (ModSubSelect == "Artifact")
                        ModItemIndex = Math.Clamp(ModItemIndex + 1, 0, CurrentMod.ModArtifacts.Count - 1);
                }
                else if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0) {
                    if (ModSubSelect == "Picross")
                        ModItemIndex = Math.Clamp(ModItemIndex - 1, 0, CurrentMod.ModPicross.Count - 1);
                    else if (ModSubSelect == "Artifact")
                        ModItemIndex = Math.Clamp(ModItemIndex - 1, 0, CurrentMod.ModArtifacts.Count - 1);
                }
            }
        }

        public void RemoveOneCharacter(ref string input) {
            if (input.Length > 0)
                input = input[0..^1];
        }

        public void AddWithCap(ref int input, int add, int cap) {
            input += add;
            if (input > cap)
                input = cap;
        }

        public void SubMinZero(ref int input, int sub, int min = 0) {
            input -= sub;
            if (input < min)
                input = min;
        }

        public void SaveMod() {
            if (CurrentMod != null && CurrentMod.Metadata.WorkshopTitle != "" && CurrentMod.Metadata.Package != "") {
                string filepath = "./mods/";

                if (!Directory.Exists(filepath))
                    Directory.CreateDirectory(filepath);

                if (Directory.Exists(filepath + CurrentMod.Metadata.WorkshopTitle + "/"))
                    Directory.Delete(filepath + CurrentMod.Metadata.WorkshopTitle + "/", true);

                Directory.CreateDirectory(filepath + CurrentMod.Metadata.WorkshopTitle + "/");


                string path = filepath + CurrentMod.Metadata.WorkshopTitle + "/" + CurrentMod.Metadata.WorkshopTitle + ".dat.gz";

                for (int i = 0; i < CurrentMod.ModArtifacts.Count; i++) {
                    CurrentMod.ModArtifacts[i].Package = CurrentMod.Metadata.Package;
                } 

                for (int i = 0; i < CurrentMod.ModItems.Count; i++) {
                    CurrentMod.ModItems[i].Package = CurrentMod.Metadata.Package;
                } 

                for (int i = 0; i < CurrentMod.ModMonsters.Count; i++) {
                    CurrentMod.ModMonsters[i].Package = CurrentMod.Metadata.Package;
                }
                 
                for (int i = 0; i < CurrentMod.ModPicross.Count; i++) {
                    CurrentMod.ModPicross[i].Package = CurrentMod.Metadata.Package;
                } 

                for (int i = 0; i < CurrentMod.ModScripts.Count; i++) {
                    CurrentMod.ModScripts[i].Package = CurrentMod.Metadata.Package;
                }
                 




                Helper.SerializeToFileCompressed(CurrentMod, path);

                UploadText = "Saved Successfully";
            } 
            else if (CurrentMod.Metadata.WorkshopTitle == "") {
                UploadText = "Needs Title";
            }
            else if (CurrentMod.Metadata.Package == "") {
                UploadText = "Needs Package";
            }
        }

        public void UnpackMod() {
            if (CurrentMod != null) { 

                Directory.CreateDirectory("./sandbox/items/");
                for (int i = 0; i < CurrentMod.ModItems.Count; i++) {
                    string path = "./sandbox/items/" + CurrentMod.ModItems[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModItems[i], path);
                } 

                Directory.CreateDirectory("./sandbox/monsters/");
                for (int i = 0; i < CurrentMod.ModMonsters.Count; i++) {
                    string path = "./sandbox/monsters/" + CurrentMod.ModMonsters[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModMonsters[i], path);
                }

                Directory.CreateDirectory("./sandbox/npcs/");
                for (int i = 0; i < CurrentMod.ModNPCs.Count; i++) {
                    string path = "./sandbox/npcs/" + CurrentMod.ModNPCs[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModNPCs[i], path);
                }
                 

                Directory.CreateDirectory("./sandbox/artifacts/");
                for (int i = 0; i < CurrentMod.ModArtifacts.Count; i++) {
                    string path = "./sandbox/artifacts/" + CurrentMod.ModArtifacts[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModArtifacts[i], path);
                }

                Directory.CreateDirectory("./sandbox/picross/");
                for (int i = 0; i < CurrentMod.ModPicross.Count; i++) {
                    string path = "./sandbox/picross/" + CurrentMod.ModPicross[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModPicross[i], path);
                }

                Directory.CreateDirectory("./sandbox/scripts/");
                for (int i = 0; i < CurrentMod.ModScripts.Count; i++) {
                    string path = "./sandbox/scripts/" + CurrentMod.ModScripts[i].Name + ".lua"; 
                    using StreamWriter output = new StreamWriter(path);
                    string scriptString = CurrentMod.ModScripts[i].Script;
                    output.WriteLine(scriptString);
                    output.Close();
                }

                Directory.CreateDirectory("./sandbox/skills/");
                for (int i = 0; i < CurrentMod.ModSkills.Count; i++) {
                    string path = "./sandbox/skills/" + CurrentMod.ModSkills[i].Name + ".dat";
                    Helper.SerializeToFile(CurrentMod.ModSkills[i], path);
                } 

                string metaPath = "./sandbox/metadata.dat";
                Helper.SerializeToFile(CurrentMod.Metadata, metaPath);
            }
        }

        public void PackMod() {
            Mod load = new();
              
            if (Directory.Exists("./sandbox/items/")) {
                string[] files = Directory.GetFiles("./sandbox/items/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Item>(json);
                    load.ModItems.Add(item);
                }
            } 

            if (Directory.Exists("./sandbox/monsters/")) {
                string[] files = Directory.GetFiles("./sandbox/monsters/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Monster>(json);
                    load.ModMonsters.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/npcs/")) {
                string[] files = Directory.GetFiles("./sandbox/npcs/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<NPC>(json);
                    load.ModNPCs.Add(item);
                }
            }
             

            if (Directory.Exists("./sandbox/artifacts/")) {
                string[] files = Directory.GetFiles("./sandbox/artifacts/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<ArchArtifact>(json);
                    load.ModArtifacts.Add(item);
                }
            }

            if (Directory.Exists("./sandbox/picross/")) {
                string[] files = Directory.GetFiles("./sandbox/picross/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<PicrossPuzzle>(json);
                    load.ModPicross.Add(item);
                }
            }


            if (Directory.Exists("./sandbox/scripts/")) {
                string[] files = Directory.GetFiles("./sandbox/scripts/");
                foreach (string fileName in files) {
                    string script = File.ReadAllText(fileName);
                    string name = fileName.Split("/")[^1][0..^4];
                    ModScript ob = new();
                    ob.Script = script;
                    ob.Name = name;
                    load.ModScripts.Add(ob);
                }
            }

            if (Directory.Exists("./sandbox/skills/")) {
                string[] files = Directory.GetFiles("./sandbox/skills/");
                foreach (string fileName in files) {
                    string json = File.ReadAllText(fileName);
                    var item = JsonConvert.DeserializeObject<Skill>(json);
                    load.ModSkills.Add(item);
                }
            } 

            string metaPath = "./sandbox/metadata.dat";
            string metaJson = File.ReadAllText(metaPath);
            var meta = JsonConvert.DeserializeObject<ModMetadata>(metaJson);
            load.Metadata = meta;

            CurrentMod = load;


        }

        public void RunInput(string add, bool backspace) {
            if (CurrentMod != null) {
                if (backspace) { 
                    if (SelectedField == 1 && ModSubSelect == "Overview")
                        RemoveOneCharacter(ref CurrentMod.Metadata.Package);
                    else if (SelectedField == 2 && ModSubSelect == "Overview")
                        RemoveOneCharacter(ref CurrentMod.Metadata.WorkshopTitle);
                    else if (SelectedField == 3 && ModSubSelect == "Overview")
                        RemoveOneCharacter(ref CurrentMod.Metadata.WorkshopDesc);
                    else if (SelectedField == 4 && ModSubSelect == "Overview")
                        RemoveOneCharacter(ref CurrentChangelog);
                    else if (SelectedField == 2 && ModSubSelect == "Artifact")
                        RemoveOneCharacter(ref CurrentMod.ModArtifacts[ModItemIndex].Name);
                    else if (SelectedField == 2 && ModSubSelect == "Picross")
                        RemoveOneCharacter(ref CurrentMod.ModPicross[ModItemIndex].Name); 
                } else { 
                    if (SelectedField == 1 && ModSubSelect == "Overview")
                        CurrentMod.Metadata.Package += add;
                    else if (SelectedField == 2 && ModSubSelect == "Overview")
                        CurrentMod.Metadata.WorkshopTitle += add;
                    else if (SelectedField == 3 && ModSubSelect == "Overview")
                        CurrentMod.Metadata.WorkshopDesc += add;
                    else  if (SelectedField == 4 && ModSubSelect == "Overview")
                        CurrentChangelog += add;
                    else if (SelectedField == 2 && ModSubSelect == "Artifact")
                        CurrentMod.ModArtifacts[ModItemIndex].Name += add;
                    else if (SelectedField == 2 && ModSubSelect == "Picross")
                        CurrentMod.ModPicross[ModItemIndex].Name += add; 
                }
            }
        }
    }
}
