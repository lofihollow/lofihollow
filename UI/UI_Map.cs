using GoRogue;
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
                Tile tile = new(GameLoop.World.maps[GameLoop.World.Player.player.MapPos + new Point3D(0, 0, depth)].Tiles[i]);

                if (map.Tiles[i].Name == "Space") {
                    while (tile.Name == "Space" && GameLoop.World.maps.ContainsKey(GameLoop.World.Player.player.MapPos + new Point3D(0, 0, depth - 1))) {
                        depth--;
                        tile = new(GameLoop.World.maps[GameLoop.World.Player.player.MapPos + new Point3D(0, 0, depth)].Tiles[i]);
                    }

                    float mult = Math.Max(0.0f, 1.0f + (depth * 0.2f));

                    Color shaded = tile.Foreground * mult;

                    map.Tiles[i].ForegroundR = shaded.R;
                    map.Tiles[i].ForegroundG = shaded.G;
                    map.Tiles[i].ForegroundB = shaded.B;
                    map.Tiles[i].TileGlyph = tile.TileGlyph;
                }

                if (map.Tiles[i].Plant != null) {
                    if (!map.Tiles[i].Plant.WateredToday) {
                        MapConsole.SetEffect(i, new CustomBlink(168, Color.Blue));
                    }
                } else {
                    MapConsole.SetEffect(i, null);
                }

                map.Tiles[i].UpdateAppearance();
            }

            MapConsole.Surface = new CellSurface(GameLoop.MapWidth, GameLoop.MapHeight, map.Tiles);
            MapWindow.Title = GameLoop.World.maps[GameLoop.World.Player.player.MapPos].MinimapTile.name.Align(HorizontalAlignment.Center, GameLoop.MapWidth - 2, (char)196);


            LightMap = new GoRogue.SenseMapping.SenseMap(GameLoop.World.maps[GameLoop.World.Player.player.MapPos].LightRes);
            LightMap.Calculate();
            SyncMapEntities(map);
        }

        public void SyncMapEntities(Map map) {
            if (GameLoop.World != null) {
                MapConsole.ForceRendererRefresh = true;
                MapConsole.Children.Clear();
                EntityRenderer.RemoveAll();

                EntityRenderer.Add(GameLoop.World.Player);

                foreach (Entity entity in map.Entities.Items) {
                    EntityRenderer.Add(entity);
                }

                foreach (KeyValuePair<string, NPCWrapper> kv in GameLoop.World.npcLibrary) {
                    if (kv.Value.npc.MapPos == GameLoop.World.Player.player.MapPos || (kv.Value.npc.MapPos.X == GameLoop.World.Player.player.MapPos.X && kv.Value.npc.MapPos.Y == GameLoop.World.Player.player.MapPos.Y && kv.Value.npc.MapPos.Z < GameLoop.World.Player.player.MapPos.Z)) {
                        if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(kv.Value.Position.ToCoord())) {
                            EntityRenderer.Add(kv.Value);
                        } else {
                            EntityRenderer.Remove(kv.Value);
                        }
                    } else {
                        EntityRenderer.Remove(kv.Value);
                    }
                }

                foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                    if (kv.Value.player.MapPos == GameLoop.World.Player.player.MapPos || (kv.Value.player.MapPos.X == GameLoop.World.Player.player.MapPos.X && kv.Value.player.MapPos.Y == GameLoop.World.Player.player.MapPos.Y && kv.Value.player.MapPos.Z < GameLoop.World.Player.player.MapPos.Z)) {
                        if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(kv.Value.player.Position.ToCoord())) {
                            EntityRenderer.Add(kv.Value);
                        } else {
                            EntityRenderer.Remove(kv.Value);
                        }
                    } else {
                        EntityRenderer.Remove(kv.Value);
                    }
                }

                FOV = new GoRogue.FOV(GameLoop.World.maps[GameLoop.World.Player.player.MapPos].MapFOV);
                for (int i = 0; i < GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles.Length; i++) {
                    if (GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].EmitsLight != null) {
                        Light light = GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].EmitsLight;
                        LightMap.AddSenseSource(new GoRogue.SenseMapping.SenseSource(GoRogue.SenseMapping.SourceType.RIPPLE, new Coord(i % GameLoop.MapWidth, i / GameLoop.MapWidth), light.Radius, GoRogue.Distance.EUCLIDEAN, light.Intensity));
                    }
                }

                UpdateVision();
            }
        }


        public void UpdateNPCs() {
            if (GameLoop.SingleOrHosting()) {
                foreach (KeyValuePair<string, NPCWrapper> kv in GameLoop.World.npcLibrary) {
                    kv.Value.Update(false);
                }
            }

            foreach (KeyValuePair<string, NPCWrapper> kv in GameLoop.World.npcLibrary) {
                NPC ent = kv.Value.npc;

                if (ent.MapPos == GameLoop.World.Player.player.MapPos || (ent.MapPos.X == GameLoop.World.Player.player.MapPos.X && ent.MapPos.Y == GameLoop.World.Player.player.MapPos.Y && ent.MapPos.Z < GameLoop.World.Player.player.MapPos.Z)) {
                    if (LimitedVision) {
                        if (FOV.CurrentFOV.Contains(ent.Position.ToCoord())) {
                            EntityRenderer.Add(kv.Value);
                        } else {
                            EntityRenderer.Remove(kv.Value);
                        }
                    } else {
                        EntityRenderer.Add(kv.Value);
                    }
                } else {
                    EntityRenderer.Remove(kv.Value);
                }

                if (ent.MapPos.Z < GameLoop.World.Player.player.MapPos.Z) {
                    int depth = GameLoop.World.Player.player.MapPos.Z - ent.MapPos.Z;

                    ent.ForegroundA = 255 - (depth * 51);
                }
            }
        }

        public void RenderOverlays() {
            for (int i = 0; i < GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles.Length; i++) {
                MapConsole.ClearDecorators(i, 1);

                if (GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].Dec != null) {
                    Decorator dec = GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].Dec;
                    MapConsole.AddDecorator(i, 1, new CellDecorator(new Color(dec.R, dec.G, dec.B, GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].Foreground.A), dec.Glyph, Mirror.None));
                }

                if (GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].Plant != null) {
                    if (!GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].Plant.WateredToday) {
                        if (GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].Plant.CurrentStage != GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].Plant.Stages.Count - 1) {
                            if (GameLoop.World.maps[GameLoop.World.Player.player.MapPos].Tiles[i].Plant.CurrentStage != -1) {
                                if (MapConsole.GetEffect(i) == null) {
                                    MapConsole.SetEffect(i, new CustomBlink(168, Color.Blue));
                                }
                            }
                        }
                    }
                }
            }

            if (GameLoop.World.Player.player.MapPos == new Point3D(1, 0, -1)) {
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
                    GameLoop.World.maps[GameLoop.World.Player.player.MapPos].GetTile(new Point(pos.X, pos.Y)).SetLight(LightMap[pos]);
                }

                if (GameLoop.World.maps[GameLoop.World.Player.player.MapPos].GetTile(new Point(pos.X, pos.Y)) != null) {
                    Tile tile = GameLoop.World.maps[GameLoop.World.Player.player.MapPos].GetTile(new Point(pos.X, pos.Y));
                    if (tile.ExposedToSky) {
                        int baseAlpha = 0;

                        if (GameLoop.World.Player.player.Clock.GetCurrentTime() > 1110) {
                            baseAlpha = (GameLoop.World.Player.player.Clock.GetCurrentTime() - 1110);

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
                Point PlayerPosPixels = new(GameLoop.World.Player.player.Position.X, GameLoop.World.Player.player.Position.Y);

                Point offset = itemMouse - PlayerPosPixels;
                offset *= new Point(-1, -1);

                double rad = Math.Atan2(offset.Y, offset.X);

                double x = 10 * Math.Round(Math.Cos(rad), 2);
                double y = 10 * Math.Round(Math.Sin(rad), 2);

                Point circle = new((int)x, (int)y);

                Point temp = GameLoop.World.Player.player.Position + circle;
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

                var line = Lines.Get(GameLoop.World.Player.player.Position.ToCoord(), otherSide.ToCoord());
                int index = GameLoop.World.Player.player.Position.X < otherSide.X ? 0 : 10;

                foreach (var pos in line) {
                    if (index == GameLoop.UIManager.Sidebar.ChargeBar / 10) {
                        MapConsole.SetDecorator(pos.X, pos.Y, 1, new CellDecorator(Color.Lime, '*', Mirror.None));
                    }

                    if (GameLoop.World.Player.player.Position.X < otherSide.X) {
                        index++;
                    } else {
                        index--;
                    }
                }
            }
        }

        public void UpdateVision() {
            if (GameLoop.World.Player.player.Position.X <= GameLoop.MapWidth && GameLoop.World.Player.player.Position.Y <= GameLoop.MapHeight) {
                if (LimitedVision) {
                    FOV.Calculate(GameLoop.World.Player.player.Position.X, GameLoop.World.Player.player.Position.Y, GameLoop.World.Player.player.Vision);
                    foreach (var position in FOV.NewlyUnseen) {
                        GameLoop.World.maps[GameLoop.World.Player.player.MapPos].GetTile(new Point(position.X, position.Y)).Shade();
                        MapConsole.ClearDecorators(position.X, position.Y, 1);
                    }

                    foreach (var position in FOV.NewlySeen) {
                        GameLoop.World.maps[GameLoop.World.Player.player.MapPos].GetTile(new Point(position.X, position.Y)).IsVisible = true;
                        GameLoop.World.maps[GameLoop.World.Player.player.MapPos].GetTile(new Point(position.X, position.Y)).Unshade();
                    }
                }
            }

            foreach (KeyValuePair<long, PlayerWrapper> kv in GameLoop.World.otherPlayers) {
                if (kv.Value.player.MapPos == GameLoop.World.Player.player.MapPos || (kv.Value.player.MapPos.X == GameLoop.World.Player.player.MapPos.X && kv.Value.player.MapPos.Y == GameLoop.World.Player.player.MapPos.Y && kv.Value.player.MapPos.Z < GameLoop.World.Player.player.MapPos.Z)) {

                    if (GameLoop.UIManager.Map.FOV.CurrentFOV.Contains(kv.Value.player.Position.ToCoord())) {
                        EntityRenderer.Add(kv.Value);
                    } else {
                        EntityRenderer.Remove(kv.Value);
                    }
                } else {
                    EntityRenderer.Remove(kv.Value);
                }
            }
        }

    }
}
