using GoRogue;
using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.EntityData; 
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq; 
using LofiHollow.DataTypes;
using Steamworks;
using LofiHollow.Minigames.Photo;

namespace LofiHollow.UI {
    public class UI_Map {
        public SadConsole.Console MapConsole;
        public SadConsole.Console OverlayConsole;
        public Window MapWindow;
        public MessageLogWindow MessageLog;
        public SadConsole.Entities.Renderer EntityRenderer;

        public GoRogue.FOV FOV;
        public GoRogue.SenseMapping.SenseMap LightMap;
        public bool LimitedVision = true;

        public UI_Map(int width, int height) {
            int mapConsoleWidth = width - 2;
            int mapConsoleHeight = height - 2;
            EntityRenderer = new SadConsole.Entities.Renderer();
            MapConsole = new SadConsole.Console(mapConsoleWidth, mapConsoleHeight);
            OverlayConsole = new SadConsole.Console(mapConsoleWidth, mapConsoleHeight);
            MapWindow = new(width, height);
            MapWindow.CanDrag = false;

            MapConsole.Position = new Point(1, 1);
            OverlayConsole.Position = new(1, 1);
            MapWindow.Title = "".Align(HorizontalAlignment.Center, mapConsoleWidth, (char)196);


            MapWindow.Children.Add(MapConsole);
            MapWindow.Children.Add(OverlayConsole);
            GameLoop.UIManager.Children.Add(MapWindow);

            MapConsole.SadComponents.Add(EntityRenderer);

            MapWindow.Show();
            MapWindow.IsVisible = false;

            MessageLog = new MessageLogWindow(72, 18, "Message Log");
            GameLoop.UIManager.Children.Add(MessageLog);
            MessageLog.Show();
            MessageLog.Position = new Point(0, 42); 
            MessageLog.IsVisible = false;

        }

        public void LoadMap(Map map, bool tryRelic = true) {
            bool SpawnRelicSpot = false;

            if (GameLoop.World.Player.MapPos.WorldArea == "Overworld" && tryRelic) {
                SpawnRelicSpot = true;
            }

            for (int i = 0; i < map.Tiles.Length; i++) {
                map.Tiles[i].Unshade();
                map.Tiles[i].IsVisible = false;



                if (GameLoop.World.Player.MapPos.WorldArea == "Overworld" && SpawnRelicSpot && GameLoop.rand.Next(10000) == 0 && map.Tiles[i].Name == "Grass") {
                    SpawnRelicSpot = false;
                    map.Tiles[i] = new("lh:Relic Spot");
                    LoadMap(map, false);
                }
            }

            MapConsole.Surface = new CellSurface(GameLoop.MapWidth, GameLoop.MapHeight, map.Tiles);
            MapWindow.Title = map.MinimapTile.name.Align(HorizontalAlignment.Center, GameLoop.MapWidth - 2, (char)196);


            LightMap = new GoRogue.SenseMapping.SenseMap(map.LightRes);
            LightMap.Calculate();
            SyncMapEntities(map);
        }

        public void LoadMap(Point3D pos, bool tryRelic = true) { 
            Map map = Helper.ResolveMap(pos);

            if (map != null) {
                LoadMap(map, tryRelic);
            }
        }

        public void SyncMapEntities(Map map) {
            if (GameLoop.World != null) { 
                MapConsole.ForceRendererRefresh = true;
                MapConsole.Children.Clear(); 
                EntityRenderer.RemoveAll(); 

                GameLoop.UIManager.Map.EntityRenderer.Add(GameLoop.World.Player);
                  
                foreach (Entity entity in map.Entities.Items) {
                    if (entity is ItemWrapper) {
                        EntityRenderer.Add(entity); 
                    } else {
                        if (entity is Actor act) {
                            GameLoop.UIManager.Map.EntityRenderer.Add(act);
                        }
                    }
                }

                foreach (KeyValuePair<string, NPC> kv in GameLoop.World.npcLibrary) { 
                    NPC temp = kv.Value; 
                    
                    if (temp.MapPos == GameLoop.World.Player.MapPos || (temp.MapPos.X == GameLoop.World.Player.MapPos.X && temp.MapPos.Y == GameLoop.World.Player.MapPos.Y && temp.MapPos.Z < GameLoop.World.Player.MapPos.Z)) {
                        if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(temp.Position.ToCoord())) {
                            GameLoop.UIManager.Map.EntityRenderer.Add(kv.Value);
                        } else {
                            GameLoop.UIManager.Map.EntityRenderer.Remove(kv.Value);
                        }
                    } else {
                        GameLoop.UIManager.Map.EntityRenderer.Remove(kv.Value);
                    } 
                }

                foreach (KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) { 
                    if (kv.Value.MapPos == GameLoop.World.Player.MapPos || (kv.Value.MapPos.X == GameLoop.World.Player.MapPos.X && kv.Value.MapPos.Y == GameLoop.World.Player.MapPos.Y && kv.Value.MapPos.Z < GameLoop.World.Player.MapPos.Z)) {
                        // EntityRenderer.Add(kv.Value); 
                        if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(kv.Value.Position.ToCoord())) {
                            GameLoop.UIManager.Map.EntityRenderer.Add(kv.Value); 
                        } else {
                            GameLoop.UIManager.Map.EntityRenderer.Remove(kv.Value);
                        }
                    } else {
                        GameLoop.UIManager.Map.EntityRenderer.Remove(kv.Value);
                    } 
                }




                FOV = new GoRogue.FOV(map.MapFOV);
                for (int i = 0; i < map.Tiles.Length; i++) {
                    if (map.Tiles[i].EmitsLight != null) {
                        Light light = map.Tiles[i].EmitsLight;
                        LightMap.AddSenseSource(new GoRogue.SenseMapping.SenseSource(GoRogue.SenseMapping.SourceType.RIPPLE, new Coord(i % GameLoop.MapWidth, i / GameLoop.MapWidth), light.Radius, GoRogue.Distance.EUCLIDEAN, light.Intensity));
                    }
                }

                UpdateVision(); 
            }
        }


        public void UpdateNPCs() {
            if (GameLoop.SingleOrHosting()) {
                foreach (KeyValuePair<string, NPC> kv in GameLoop.World.npcLibrary) {
                    kv.Value.Update(false);
                }
            }

            foreach (KeyValuePair<string, NPC> kv in GameLoop.World.npcLibrary) {
                NPC ent = kv.Value; 
                if (ent.MapPos == GameLoop.World.Player.MapPos || (ent.MapPos.X == GameLoop.World.Player.MapPos.X && ent.MapPos.Y == GameLoop.World.Player.MapPos.Y && ent.MapPos.Z < GameLoop.World.Player.MapPos.Z)) {
                    if (LimitedVision) {
                        if (FOV.CurrentFOV.Contains(ent.Position.ToCoord())) {
                            GameLoop.UIManager.Map.EntityRenderer.Add(ent);
                        } else {
                            GameLoop.UIManager.Map.EntityRenderer.Remove(ent);
                        }
                    } else {
                        GameLoop.UIManager.Map.EntityRenderer.Add(ent);
                    }
                } else {
                    GameLoop.UIManager.Map.EntityRenderer.Remove(ent);
                }

                if (ent.MapPos.Z < GameLoop.World.Player.MapPos.Z) {
                    int depth = GameLoop.World.Player.MapPos.Z - ent.MapPos.Z;

                    Color shaded = new(ent.Appearance.Foreground.R, ent.Appearance.Foreground.G, ent.Appearance.Foreground.B, 255 - (depth * 51));

                    Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);

                    if (map != null) {
                        if (map.GetTile(ent.Position).Name != "Space") {
                            GameLoop.UIManager.Map.EntityRenderer.Remove(ent);
                        }
                    }

                    ent.Appearance.Foreground = shaded;
                }
            } 
        }

        public void RenderOverlays() {
            Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);
            OverlayConsole.Clear();

            if (map != null) {
                for (int i = 0; i < map.Tiles.Length; i++) {
                    MapConsole.ClearDecorators(i, 1);
                    OverlayConsole.ClearDecorators(i, 1);
                    if (map.Tiles[i].Dec != null) {
                        Decorator dec = map.Tiles[i].Dec;
                        MapConsole.AddDecorator(i, 1, new CellDecorator(new Color(dec.R, dec.G, dec.B, map.Tiles[i].Foreground.A), dec.Glyph, Mirror.None));
                    }

                    if (map.Tiles[i].Plant != null) {
                        if (!map.Tiles[i].Plant.WateredToday) {
                            if (map.Tiles[i].Plant.CurrentStage != map.Tiles[i].Plant.Stages.Count - 1) {
                                if (map.Tiles[i].Plant.CurrentStage != -1) {
                                    if (MapConsole.GetEffect(i) == null) {
                                        MapConsole.SetEffect(i, new CustomBlink(168, Color.Blue));
                                    }
                                }
                            }
                        }
                    }
                }

                if (GameLoop.World.Player.Equipment[10].Name != "(EMPTY)") {
                    SoulPhoto photo = GameLoop.World.Player.Equipment[10].SoulPhoto;
                    OverlayConsole.PrintDecorated(photo.Position.X, photo.Position.Y, photo.GetAppearance());
                }

                foreach (var pos in FOV.CurrentFOV) {
                    if (LightMap.CurrentSenseMap.Contains(pos)) {
                        map.GetTile(new Point(pos.X, pos.Y)).SetLight(LightMap[pos]);
                    }

                    if (map.GetTile(new Point(pos.X, pos.Y)) != null) {
                        Tile tile = map.GetTile(new Point(pos.X, pos.Y));
                        if (tile.ExposedToSky) {
                            int baseAlpha = 0;

                            if (GameLoop.World.Player.Clock.GetCurrentTime() > 1110) {
                                baseAlpha = (GameLoop.World.Player.Clock.GetCurrentTime() - 1110);

                                if (baseAlpha < 0)
                                    baseAlpha = 0;
                                if (baseAlpha > 200)
                                    baseAlpha = 200;

                                MapConsole.AddDecorator(pos.X, pos.Y, 1, new CellDecorator(new Color(0, 0, 0, baseAlpha - tile.CurrentLight), 219, Mirror.None));
                            }
                        } else {
                            MapConsole.AddDecorator(pos.X, pos.Y, 1, new CellDecorator(new Color(0, 0, 0, 200 - tile.CurrentLight), 219, Mirror.None));
                        }
                    }
                }

                if (GameLoop.World.Player.Inventory[GameLoop.UIManager.Sidebar.hotbarSelect].ItemCat == "Camera") {
                    Point Pos = new MouseScreenObjectState(MapConsole, GameHost.Instance.Mouse).CellPosition;
                    if (Pos.X > 0 && Pos.Y > 0) {
                        Point topLeft = Pos - new Point(10, 10);
                        if (topLeft.X < 0)
                            topLeft = new Point(0, topLeft.Y);
                        if (topLeft.Y < 0)
                            topLeft = new Point(topLeft.X, 0);
                        if (topLeft.X + 21 > GameLoop.MapWidth)
                            topLeft = new Point(GameLoop.MapWidth - 21, topLeft.Y);
                        if (topLeft.Y + 21 > GameLoop.MapHeight)
                            topLeft = new Point(topLeft.X, GameLoop.MapHeight - 21);

                        OverlayConsole.AddDecorator(topLeft.X + 1, topLeft.Y, 2, new CellDecorator(Color.White, 196, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X, topLeft.Y + 1, 1, new CellDecorator(Color.White, 179, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X, topLeft.Y + 2, 1, new CellDecorator(Color.White, 179, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X + 18, topLeft.Y + 20, 2, new CellDecorator(Color.White, 196, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X + 20, topLeft.Y + 19, 1, new CellDecorator(Color.White, 179, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X + 20, topLeft.Y + 18, 1, new CellDecorator(Color.White, 179, Mirror.None));

                        OverlayConsole.AddDecorator(topLeft.X + 1, topLeft.Y + 20, 2, new CellDecorator(Color.White, 196, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X, topLeft.Y + 19, 1, new CellDecorator(Color.White, 179, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X, topLeft.Y + 18, 1, new CellDecorator(Color.White, 179, Mirror.None));

                        OverlayConsole.AddDecorator(topLeft.X + 18, topLeft.Y, 2, new CellDecorator(Color.White, 196, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X + 20, topLeft.Y + 1, 1, new CellDecorator(Color.White, 179, Mirror.None));
                        OverlayConsole.AddDecorator(topLeft.X + 20, topLeft.Y + 2, 1, new CellDecorator(Color.White, 179, Mirror.None));
                    }
                }


                if (GameLoop.UIManager.Sidebar.ChargeBar != 0) {
                    Point itemMouse = new MouseScreenObjectState(MapConsole, GameHost.Instance.Mouse).CellPosition;
                    Point PlayerPosPixels = new(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y);

                    Point offset = itemMouse - PlayerPosPixels;
                    offset *= new Point(-1, -1);

                    double rad = Math.Atan2(offset.Y, offset.X);

                    double x = 10 * Math.Round(Math.Cos(rad), 2);
                    double y = 10 * Math.Round(Math.Sin(rad), 2);

                    Point circle = new((int)x, (int)y);

                    Point temp = GameLoop.World.Player.Position + circle;
                    int cellX = temp.X;
                    int cellY = temp.Y;

                    if (cellX < 0)
                        cellX = 0;
                    if (cellY < 0)
                        cellY = 0;
                    if (cellX > GameLoop.MapWidth)
                        cellX = GameLoop.MapWidth;
                    if (cellY > GameLoop.MapHeight)
                        cellY = GameLoop.MapHeight;

                    Point otherSide = new(cellX, cellY);
                    GameLoop.UIManager.Sidebar.LureSpot = circle * new Point(-1, -1);

                    OverlayConsole.SetDecorator(otherSide.X, otherSide.Y, 1, new CellDecorator(Color.White, '*', Mirror.None));

                    var line = Lines.Get(GameLoop.World.Player.Position.ToCoord(), otherSide.ToCoord());
                    int index = GameLoop.World.Player.Position.X < otherSide.X ? 0 : 10;

                    foreach (var pos in line) {
                        if (index == GameLoop.UIManager.Sidebar.ChargeBar / 10) {
                            OverlayConsole.SetDecorator(pos.X, pos.Y, 1, new CellDecorator(Color.Lime, '*', Mirror.None));
                        }

                        if (GameLoop.World.Player.Position.X < otherSide.X) {
                            index++;
                        } else {
                            index--;
                        }
                    }
                }
            }
        }

        public void UpdateVision() {
            Point cellPos = GameLoop.World.Player.Position;

            if (cellPos.X <= GameLoop.MapWidth && cellPos.Y <= GameLoop.MapHeight) {
                if (LimitedVision) { 
                    Map viewMap = Helper.ResolveMap(GameLoop.World.Player.MapPos);

                    if (viewMap != null) { 

                        FOV.Calculate(cellPos.X, cellPos.Y, GameLoop.World.Player.Vision);
                        foreach (var position in FOV.NewlyUnseen) {
                            viewMap.GetTile(new Point(position.X, position.Y)).Shade();
                            MapConsole.ClearDecorators(position.X, position.Y, 1);
                        }

                        foreach (var position in FOV.NewlySeen) {
                            viewMap.GetTile(new Point(position.X, position.Y)).IsVisible = true;
                            viewMap.GetTile(new Point(position.X, position.Y)).Unshade();
                        }
                    }
                }
            }

            foreach (KeyValuePair<CSteamID, Player> kv in GameLoop.World.otherPlayers) {
                Point playerPos = kv.Value.Position;
                if (kv.Value.MapPos == GameLoop.World.Player.MapPos || (kv.Value.MapPos.X == GameLoop.World.Player.MapPos.X && kv.Value.MapPos.Y == GameLoop.World.Player.MapPos.Y && kv.Value.MapPos.Z < GameLoop.World.Player.MapPos.Z)) {
                    // EntityRenderer.Add(kv.Value); 
                    if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(playerPos.ToCoord())) {
                        GameLoop.UIManager.Map.EntityRenderer.Add(kv.Value);
                    } else {
                        GameLoop.UIManager.Map.EntityRenderer.Remove(kv.Value);
                    }
                } else {
                    GameLoop.UIManager.Map.EntityRenderer.Remove(kv.Value);
                } 
            } 
             
            Map map = Helper.ResolveMap(GameLoop.World.Player.MapPos);

            if (map != null) {
                foreach (var Position in FOV.CurrentFOV) {
                    int i = Position.ToPoint().ToIndex(GameLoop.MapWidth);

                    int depth = 0;
                    Map lower = Helper.ResolveMap(GameLoop.World.Player.MapPos + new Point3D(0, 0, depth));
                    if (lower != null) {
                        Tile tile = new(lower.Tiles[i]);

                        if (map.Tiles[i].Name == "Space") {
                            lower = Helper.ResolveMap(GameLoop.World.Player.MapPos + new Point3D(0, 0, depth - 1));
                            while (tile.Name == "Space" && lower != null) {
                                depth--;

                                lower = Helper.ResolveMap(GameLoop.World.Player.MapPos + new Point3D(0, 0, depth));
                                if (lower != null)
                                    tile = new Tile(lower.Tiles[i]);
                            }

                            float mult = Math.Max(0.0f, 1.0f + (depth * 0.2f));

                            Color shaded = tile.Foreground * mult;

                            map.Tiles[i].ForegroundR = shaded.R;
                            map.Tiles[i].ForegroundG = shaded.G;
                            map.Tiles[i].ForegroundB = shaded.B;
                            map.Tiles[i].TileGlyph = tile.TileGlyph;
                            map.Tiles[i].UpdateAppearance();
                        }
                    }
                }
            }

            LightMap.Calculate();
        }

    }
}
