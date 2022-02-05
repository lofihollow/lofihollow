﻿using GoRogue;
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
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.UI {
    public class UI_Map {
        public SadConsole.Console MapConsole;
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

            MapWindow = new(width, height);
            MapWindow.CanDrag = false;

            MapConsole.Position = new Point(1, 1);
            MapWindow.Title = "".Align(HorizontalAlignment.Center, mapConsoleWidth, (char)196);


            MapWindow.Children.Add(MapConsole);
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

        public void LoadMap(Point3D pos) {
            if (!GameLoop.World.maps.ContainsKey(pos)) { GameLoop.World.CreateMap(pos); }
            Map map = GameLoop.World.maps[pos];
              
            for (int i = 0; i < map.Tiles.Length; i++) {
                map.Tiles[i].Unshade();
                map.Tiles[i].IsVisible = false;

                int depth = 0;
                TileBase tile = new(GameLoop.World.maps[GameLoop.World.Player.MapPos + new Point3D(0, 0, depth)].Tiles[i]);

                if (map.Tiles[i].Name == "Space") {
                    while (tile.Name == "Space" && GameLoop.World.maps.ContainsKey(GameLoop.World.Player.MapPos + new Point3D(0, 0, depth - 1))) {
                        depth--;
                        tile = new TileBase(GameLoop.World.maps[GameLoop.World.Player.MapPos + new Point3D(0, 0, depth)].Tiles[i]);
                    }

                    float mult = Math.Max(0.0f, 1.0f + (depth * 0.2f));

                    Color shaded = tile.Foreground * mult;

                    map.Tiles[i].ForegroundR = shaded.R;
                    map.Tiles[i].ForegroundG = shaded.G;
                    map.Tiles[i].ForegroundB = shaded.B;
                    map.Tiles[i].TileGlyph = tile.TileGlyph;
                    map.Tiles[i].UpdateAppearance();
                }

                if (map.Tiles[i].Plant != null) {
                    if (!map.Tiles[i].Plant.WateredToday) {
                        MapConsole.SetEffect(i, new CustomBlink(168, Color.Blue));
                    }
                } else {
                    MapConsole.SetEffect(i, null);
                }
            }

            MapConsole.Surface = new CellSurface(GameLoop.MapWidth, GameLoop.MapHeight, map.Tiles);
            MapWindow.Title = GameLoop.World.maps[GameLoop.World.Player.MapPos].MinimapTile.name.Align(HorizontalAlignment.Center, GameLoop.MapWidth - 2, (char)196);


            LightMap = new GoRogue.SenseMapping.SenseMap(GameLoop.World.maps[GameLoop.World.Player.MapPos].LightRes);
            LightMap.Calculate();
            SyncMapEntities(map);
        }

        public void SyncMapEntities(Map map) {
            if (GameLoop.World != null) { 
                MapConsole.ForceRendererRefresh = true;
                MapConsole.Children.Clear(); 
                EntityRenderer.RemoveAll();

                if (GameLoop.World.Player.ScreenAppearance == null)
                    GameLoop.World.Player.UpdateAppearance();
                MapConsole.Children.Add(GameLoop.World.Player.ScreenAppearance);

                foreach (Entity entity in map.Entities.Items) {
                    if (entity is ItemWrapper) {
                        EntityRenderer.Add(entity); 
                    } else {
                        if (entity is Actor act) {
                            MapConsole.Children.Add(act.ScreenAppearance);
                        }
                    }
                }

                for (int i = 0; i < GameLoop.World.npcLibrary.Count; i++) {
                    NPC temp = GameLoop.World.npcLibrary[i];
                    if (temp.ScreenAppearance == null)
                        temp.UpdateAppearance();
                    MapConsole.Children.Add(temp.ScreenAppearance);
                    
                    if (temp.MapPos == GameLoop.World.Player.MapPos || (temp.MapPos.X == GameLoop.World.Player.MapPos.X && temp.MapPos.Y == GameLoop.World.Player.MapPos.Y && temp.MapPos.Z < GameLoop.World.Player.MapPos.Z)) {
                        if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(temp.Position.ToCoord())) {
                            temp.ScreenAppearance.IsVisible = true;
                        } else {
                            temp.ScreenAppearance.IsVisible = false;
                        }
                    } else {
                        temp.ScreenAppearance.IsVisible = false;
                    }

                    if (temp.ScreenAppearance.Position != temp.Position) {
                        temp.ScreenAppearance.Position = temp.Position;
                    }
                }

                foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
                    if (kv.Value.ScreenAppearance == null)
                        kv.Value.UpdateAppearance();
                    MapConsole.Children.Add(kv.Value.ScreenAppearance);

                    if (kv.Value.MapPos == GameLoop.World.Player.MapPos || (kv.Value.MapPos.X == GameLoop.World.Player.MapPos.X && kv.Value.MapPos.Y == GameLoop.World.Player.MapPos.Y && kv.Value.MapPos.Z < GameLoop.World.Player.MapPos.Z)) {
                        // EntityRenderer.Add(kv.Value); 
                        if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(kv.Value.Position.ToCoord())) {
                            kv.Value.ScreenAppearance.IsVisible = true;
                        } else {
                            kv.Value.ScreenAppearance.IsVisible = false;
                        }
                    } else {
                        kv.Value.ScreenAppearance.IsVisible = false;
                    }

                    if (kv.Value.ScreenAppearance.Position != kv.Value.Position) {
                        kv.Value.ScreenAppearance.Position = kv.Value.Position;
                    }
                }




                FOV = new GoRogue.FOV(GameLoop.World.maps[GameLoop.World.Player.MapPos].MapFOV);
                for (int i = 0; i < GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles.Length; i++) {
                    if (GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].EmitsLight != null) {
                        Light light = GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].EmitsLight;
                        LightMap.AddSenseSource(new GoRogue.SenseMapping.SenseSource(GoRogue.SenseMapping.SourceType.RIPPLE, new Coord(i % GameLoop.MapWidth, i / GameLoop.MapWidth), light.Radius, GoRogue.Distance.EUCLIDEAN, light.Intensity));
                    }
                }

                UpdateVision(); 
            }
        }


        public void UpdateNPCs() {
            if (GameLoop.SingleOrHosting()) {
                for (int i = 0; i < GameLoop.World.npcLibrary.Count; i++) {
                    GameLoop.World.npcLibrary[i].Update(false);
                }
            }
             
            for (int i = 0; i < GameLoop.World.npcLibrary.Count; i++) {
                NPC ent = GameLoop.World.npcLibrary[i];
                if (ent.ScreenAppearance == null)
                    ent.UpdateAppearance();

                if (ent.MapPos == GameLoop.World.Player.MapPos || (ent.MapPos.X == GameLoop.World.Player.MapPos.X && ent.MapPos.Y == GameLoop.World.Player.MapPos.Y && ent.MapPos.Z < GameLoop.World.Player.MapPos.Z)) {
                    if (LimitedVision) {
                        if (FOV.CurrentFOV.Contains(ent.Position.ToCoord())) {
                            ent.ScreenAppearance.IsVisible = true;
                        } else {
                            ent.ScreenAppearance.IsVisible = false;
                        }
                    } else {
                        ent.ScreenAppearance.IsVisible = true;
                    }
                } else {
                    ent.ScreenAppearance.IsVisible = false;
                }

                if (ent.MapPos.Z < GameLoop.World.Player.MapPos.Z) {
                    int depth = GameLoop.World.Player.MapPos.Z - ent.MapPos.Z;

                    Color shaded = new(ent.Appearance.Foreground.R, ent.Appearance.Foreground.G, ent.Appearance.Foreground.B, 255 - (depth * 51));

                    ent.Appearance.Foreground = shaded;
                }
            } 
        }

        public void RenderOverlays() {
            for (int i = 0; i < GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles.Length; i++) {
                MapConsole.ClearDecorators(i, 1);

                if (GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].Dec != null) {
                    Decorator dec = GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].Dec;
                    MapConsole.AddDecorator(i, 1, new CellDecorator(new Color(dec.R, dec.G, dec.B, GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].Foreground.A), dec.Glyph, Mirror.None));
                }

                if (GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].Plant != null) {
                    if (!GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].Plant.WateredToday) {
                        if (GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].Plant.CurrentStage != GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].Plant.Stages.Count - 1) {
                            if (GameLoop.World.maps[GameLoop.World.Player.MapPos].Tiles[i].Plant.CurrentStage != -1) {
                                if (MapConsole.GetEffect(i) == null) { 
                                    MapConsole.SetEffect(i, new CustomBlink(168, Color.Blue));
                                }
                            }
                        }
                    }
                }
            }

            if (GameLoop.World.Player.MapPos == new Point3D(1, 0, -1)) {
                if (GameLoop.UIManager.Minigames.MonsterPenManager != null) {
                    if (GameLoop.UIManager.Minigames.MonsterPenManager.FirstPen != null) {
                        if (GameLoop.UIManager.Minigames.MonsterPenManager.FirstPen.Monster.Name != "(EMPTY)") {
                            if (FOV.CurrentFOV.Contains(new Coord(21, 30)))
                                MapConsole.Print(21, 30, GameLoop.UIManager.Minigames.MonsterPenManager.FirstPen.Monster.Appearance());
                        }
                    }

                    if (GameLoop.UIManager.Minigames.MonsterPenManager.SecondPen != null) {
                        if (GameLoop.UIManager.Minigames.MonsterPenManager.SecondPen.Monster.Name != "(EMPTY)") {
                            if (FOV.CurrentFOV.Contains(new Coord(21, 32)))
                                MapConsole.Print(21, 32, GameLoop.UIManager.Minigames.MonsterPenManager.SecondPen.Monster.Appearance());
                        }
                    }

                    if (GameLoop.UIManager.Minigames.MonsterPenManager.ThirdPen != null) {
                        if (GameLoop.UIManager.Minigames.MonsterPenManager.ThirdPen.Monster.Name != "(EMPTY)") {
                            if (FOV.CurrentFOV.Contains(new Coord(21, 34)))
                                MapConsole.Print(21, 34, GameLoop.UIManager.Minigames.MonsterPenManager.ThirdPen.Monster.Appearance());
                        }
                    }
                } 
            }

            foreach (var pos in FOV.CurrentFOV) {
                if (LightMap.CurrentSenseMap.Contains(pos)) {
                    GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(new Point(pos.X, pos.Y)).SetLight(LightMap[pos]); 
                }

                if (GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(new Point(pos.X, pos.Y)) != null) {
                    TileBase tile = GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(new Point(pos.X, pos.Y));
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

                MapConsole.SetDecorator(otherSide.X, otherSide.Y, 1, new CellDecorator(Color.White, '*', Mirror.None));
                 
                var line = Lines.Get(GameLoop.World.Player.Position.ToCoord(), otherSide.ToCoord());
                int index = GameLoop.World.Player.Position.X < otherSide.X ? 0 : 10;

                foreach (var pos in line) {
                    if (index == GameLoop.UIManager.Sidebar.ChargeBar / 10) {
                        MapConsole.SetDecorator(pos.X, pos.Y, 1, new CellDecorator(Color.Lime, '*', Mirror.None));
                    }

                    if (GameLoop.World.Player.Position.X < otherSide.X) {
                        index++;
                    } else {
                        index--;
                    }
                } 
            }
        }

        public void UpdateVision() {
            if (GameLoop.World.Player.Position.X <= GameLoop.MapWidth && GameLoop.World.Player.Position.Y <= GameLoop.MapHeight) {
                if (LimitedVision) {
                    FOV.Calculate(GameLoop.World.Player.Position.X, GameLoop.World.Player.Position.Y, GameLoop.World.Player.Vision);
                    foreach (var position in FOV.NewlyUnseen) {
                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(new Point(position.X, position.Y)).Shade();
                        MapConsole.ClearDecorators(position.X, position.Y, 1);
                    }

                    foreach (var position in FOV.NewlySeen) {
                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(new Point(position.X, position.Y)).IsVisible = true;
                        GameLoop.World.maps[GameLoop.World.Player.MapPos].GetTile(new Point(position.X, position.Y)).Unshade();
                    }
                }
            }

            foreach (KeyValuePair<long, Player> kv in GameLoop.World.otherPlayers) {
                if (kv.Value.ScreenAppearance == null)
                    kv.Value.UpdateAppearance();
                MapConsole.Children.Add(kv.Value.ScreenAppearance);

                if (kv.Value.MapPos == GameLoop.World.Player.MapPos || (kv.Value.MapPos.X == GameLoop.World.Player.MapPos.X && kv.Value.MapPos.Y == GameLoop.World.Player.MapPos.Y && kv.Value.MapPos.Z < GameLoop.World.Player.MapPos.Z)) {
                    // EntityRenderer.Add(kv.Value); 
                    if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(kv.Value.Position.ToCoord())) {
                        kv.Value.ScreenAppearance.IsVisible = true;
                    } else {
                        kv.Value.ScreenAppearance.IsVisible = false;
                    }
                } else {
                    kv.Value.ScreenAppearance.IsVisible = false;
                }

                if (kv.Value.ScreenAppearance.Position != kv.Value.Position) {
                    kv.Value.ScreenAppearance.Position = kv.Value.Position;
                }
            } 
        }

    }
}
