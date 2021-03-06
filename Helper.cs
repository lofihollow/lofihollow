using LofiHollow.Entities;
using LofiHollow.EntityData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression; 
using System.Text;
using LofiHollow.DataTypes;
using Steamworks;
using SadConsole.Input;
using Console = SadConsole.Console;

namespace LofiHollow {
    public static class Helper {
        public static bool KeyPressed(Keys key) {
            return GameHost.Instance.Keyboard.IsKeyPressed(key);
        }
        public static ColoredString HoverColoredString(string text, bool condition) {
            return new ColoredString(text, condition ? Color.Yellow : Color.White, Color.Black);
        }

        public static ColoredString RequirementString(string text, bool condition, bool secondCondition) {
            return new ColoredString(text, condition ? Color.Yellow : secondCondition ? Color.Cyan : Color.Red, Color.Black);
        }

        public static string Center(object obj, int width, int fillChar = ' ') {
            return obj.ToString().Align(HorizontalAlignment.Center, width, (char) fillChar);
        }

        public static ColoredString Checkmark(bool condition) {
            ColoredString check = new ColoredString(4.AsString(), Color.Lime, Color.Black);

            if (!condition)
                check = new ColoredString("x", Color.Red, Color.Black);

            return check;
        }

        public static T Clone<T>(this T source) { 
            if (Object.ReferenceEquals(source, null)) {
                return default(T);
            }

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            var serializeSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, serializeSettings), deserializeSettings);
        }

        public static string Truncate(string input, int len) {
            if (input.Length < len)
                return input;
            return input[0..(len - 1)];
        }

        public static int Average(int a, int b) {
            double c = a;
            double d = b;
            return (int)Math.Floor((c + d) / 2);
        }

        public static void DrawBox(Console con, int LeftX, int TopY, int w, int h, int r = 255, int g = 255, int b = 255) {
            int LeftXin = LeftX + 1;
            int TopYin = TopY + 1;
            int RightXin = LeftX + w;
            int BottomYin = TopY + h;
            int BottomY = TopY + h + 1;
            int RightX = LeftX + w + 1;

            Color fg = new Color(r, g, b);

            con.DrawLine(new Point(LeftXin, TopY), new Point(RightXin, TopY), 196, fg);
            con.DrawLine(new Point(LeftXin, BottomY), new Point(RightXin, BottomY), 196, fg);
            con.DrawLine(new Point(LeftX, TopYin), new Point(LeftX, BottomYin), 179, fg);
            con.DrawLine(new Point(RightX, TopYin), new Point(RightX, BottomYin), 179, fg);
            con.Print(LeftX, TopY, 218.AsString(), fg);
            con.Print(RightX, BottomY, 217.AsString(), fg); 
            con.Print(LeftX, BottomY, 192.AsString(), fg);
            con.Print(RightX, TopY, 191.AsString(), fg);
        }


        public static ColoredString LetterGrade(int Q, int align = -1) {
            string qual = "";
            Color col = Color.Black;
             
            switch (Q) {
                case 1:
                    qual = "F";
                    col = Color.Red;
                    break;
                case 2:
                    qual = "E";
                    col = Color.OrangeRed;
                    break;
                case 3:
                    qual = "D";
                    col = Color.Orange;
                    break;
                case 4:
                    qual = "C";
                    col = Color.Yellow;
                    break;
                case 5:
                    qual = "B";
                    col = Color.LimeGreen;
                    break;
                case 6:
                    qual = "A";
                    col = Color.DodgerBlue;
                    break;
                case 7:
                    qual = "A+";
                    col = Color.Cyan;
                    break;
                case 8:
                    qual = "S";
                    col = Color.HotPink;
                    break;
                case 9:
                    qual = "S+";
                    col = Color.MediumPurple;
                    break;
                case 10:
                    qual = "S++";
                    col = Color.BlueViolet;
                    break;
                case 11:
                    qual = 172.AsString();
                    col = Color.White;
                    break;
                case -1:
                    qual = "???";
                    col = Color.White;
                    break;
                default:
                    return new ColoredString("");
            }

            if (align != -1)
                return new ColoredString(qual.Align(HorizontalAlignment.Center, align), col, Color.Black);
            return new ColoredString(qual, col, Color.Black);
        }

        public static string TimeSinceDayStart() {
            double timeToAchieve = SadConsole.GameHost.Instance.GameRunningTotalTime.TotalMilliseconds - GameLoop.World.Player.DayStart;
            int minutes = (int)Math.Floor(timeToAchieve / 60000);
            timeToAchieve -= minutes * 60000;
            int seconds = (int)Math.Floor(timeToAchieve / 1000);
            timeToAchieve -= seconds * 1000;

            timeToAchieve = Math.Floor(timeToAchieve);

            string time = minutes.ToString().Align(HorizontalAlignment.Right, 2, '0') + "m";
            time += seconds.ToString().Align(HorizontalAlignment.Right, 2, '0') + "s";
            time += "." + timeToAchieve;

            return time;
        }


        public static double CropPrice(Plant plant, int quality) {
            double baseValueCoppers = 10;

            double daysToGrow = 0;
            double recur = 0;

            for (int i = 0; i < plant.Stages.Count - 1; i++) {
                daysToGrow += plant.Stages[i].DaysToNext + 1;
            }

            if (plant.HarvestRevert != -1) {
                for (int i = plant.HarvestRevert; i < plant.Stages.Count - 1; i++) {
                    recur += plant.Stages[i].DaysToNext + 1;
                }
            }

            double growthCycles;


            if (recur != 0)
                growthCycles = Math.Floor((28 - daysToGrow) / recur);
            else
                growthCycles = Math.Floor(28 / daysToGrow);

            double modifiedPrice = (baseValueCoppers - growthCycles);


            return Math.Round(modifiedPrice, 1);
        }

        public static Map ResolveMap(Point3D mapPos) {
            if (GameLoop.World.maps.ContainsKey(mapPos))
                return GameLoop.World.maps[mapPos];
            else if (!GameLoop.World.maps.ContainsKey(mapPos) && GameLoop.World.LoadMapAt(mapPos))
                return GameLoop.World.maps[mapPos];
            else if (mapPos.WorldArea.Contains("Apartment")) {
                string name = mapPos.WorldArea[0..^10];

                if (name == GameLoop.World.Player.Name)
                    return GameLoop.World.Player.NoonbreezeApt.map;

                foreach (KeyValuePair<SteamId, Player> kv in GameLoop.World.otherPlayers) {
                    if (name == kv.Value.Name)
                        return kv.Value.NoonbreezeApt.map;
                }
            }

            return null;
        }

        public static string ResolveName(SteamId id) {
            if (GameLoop.World.otherPlayers.ContainsKey(id))
                return GameLoop.World.otherPlayers[id].Name;
            return "[Not Connected: " + id + "]";
        }

        public static ColoredString ConvertCoppers(int copperValue) {
            int coinsLeft = copperValue;

            int jade = copperValue / 1000000; 
            coinsLeft -= jade * 1000000;

            int gold = coinsLeft / 10000;
            coinsLeft -= gold * 10000;

            int silver = coinsLeft / 100;
            coinsLeft -= silver * 100;

            int copper = coinsLeft;

            ColoredString build = new("", Color.White, Color.Black);

            ColoredString copperString = new(copper + "c", new Color(184, 115, 51), Color.Black);
            ColoredString silverString = new(silver + "s ", Color.Silver, Color.Black);
            ColoredString goldString = new(gold + "g ", Color.Yellow, Color.Black);
            ColoredString JadeString = new(jade + "j ", new Color(0, 168, 107), Color.Black);

            if (jade > 0)
                build += JadeString;
            if (gold > 0)
                build += goldString;
            if (silver > 0)
                build += silverString;
            if (copper > 0)
                build += copperString;

            return build;
        } 

        public static bool AnyEqual(string instance, params string[] checks) {
            for (int i = 0; i < checks.Length; i++) {
                if (instance == checks[i])
                    return true;
            }
            return false;
        }

        const int BufferSize = 8192;
        static readonly Encoding DefaultEncoding = new UTF8Encoding(false);
         

        public static void SerializeToFile(object value, string path, JsonSerializerSettings settings = null) {
            using StreamWriter output = new StreamWriter(path);
            string jsonString = JsonConvert.SerializeObject(value, Formatting.Indented);
            output.WriteLine(jsonString);
            output.Close();
        }

        public static void SerializeToFileCompressed(object value, string path, JsonSerializerSettings settings = null) {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                SerializeCompressed(value, fs, settings);
        }

        public static void SerializeCompressed(object value, Stream stream, JsonSerializerSettings settings = null) {
            using (var compressor = new GZipStream(stream, CompressionMode.Compress))
            using (var writer = new StreamWriter(compressor, DefaultEncoding, BufferSize)) {
                var serializer = JsonSerializer.CreateDefault(settings);
                serializer.Serialize(writer, value);
            }
        }

        public static byte[] ToByteArray(this object instance, JsonSerializerSettings settings = null) {
            MemoryStream stream = new();
            using (var compressor = new GZipStream(stream, CompressionMode.Compress))
            using (var writer = new StreamWriter(compressor, DefaultEncoding, BufferSize)) {
                var serializer = JsonSerializer.CreateDefault(settings);
                serializer.Serialize(writer, instance);
                writer.Close();
                compressor.Close();
            }
            

            return stream.ToArray();
        }

        public static T FromByteArray<T>(this byte[] input, JsonSerializerSettings settings = null) {
            MemoryStream stream = new(input);

            using (var compressor = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(compressor))
            using (var jsonReader = new JsonTextReader(reader)) {
                var serializer = JsonSerializer.CreateDefault(settings);
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public static T DeserializeFromFileCompressed<T>(string path, JsonSerializerSettings settings = null) {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return DeserializeCompressed<T>(fs, settings);
        }

        public static T DeserializeCompressed<T>(Stream stream, JsonSerializerSettings settings = null) {
            using (var compressor = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(compressor))
            using (var jsonReader = new JsonTextReader(reader)) {
                var serializer = JsonSerializer.CreateDefault(settings);
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }


    public static class DictionaryExtensions {
        public static T Get<T>(this Dictionary<string, object> instance, string name) {
            if (name != "Photo") {
                var result = ((JObject)instance[name]).Value<JObject>();
                var convert = result.ToObject<T>();
                return convert;
            } else {
                return ((T)instance[name]);
            }
        }

        public static List<T> GetList<T>(this Dictionary<string, object> instance, string name) {
            var result = ((JArray) instance[name]).Value<JArray>();
            List<T> list = result.ToObject<List<T>>();
            return list;
        }
    }

    public static class PointExtensions {
        public static GoRogue.Coord ToCoord(this Point instance) {
            return new GoRogue.Coord(instance.X, instance.Y);
        }

        public static Point ToPoint(this GoRogue.Coord instance) {
            return new Point(instance.X, instance.Y);
        }
    }

    public static class MiscExtensions {
        public static string AsString(this int instance) {
            return ((char)instance).ToString();
        } 

        public static void PrintDecorated(this SadConsole.Console instance, int x, int y, DecoratedString str) {
            instance.Print(x, y, str.Text);

            if (str.Decorators != null) {
                for (int i = 0; i < str.Decorators.Length; i++) {
                    instance.AddDecorator(x + i, y, 1, str.Decorators[i]);
                }
            }
        }

        public static void PrintClickable(this SadConsole.Console instance, int x, int y, string str, Action<string> OnClick, string ID) {
            Point mousePos = new MouseScreenObjectState(instance, GameHost.Instance.Mouse).CellPosition; 
            int length = str.Length;

            instance.Print(x, y, str, mousePos.X >= x && mousePos.X <= x + length && mousePos.Y == y ? Color.Yellow : Color.White);

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos.X >= x && mousePos.X <= x + length && mousePos.Y == y) {
                    OnClick(ID);
                }
            }
        }

        public static void PrintClickable(this SadConsole.Console instance, int x, int y, ColoredString str, Action<string> OnClick, string ID) {
            Point mousePos = new MouseScreenObjectState(instance, GameHost.Instance.Mouse).CellPosition;
            int length = str.Length - 1;

            instance.Print(x, y, str);

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos.X >= x && mousePos.X <= x + length && mousePos.Y == y) {
                    OnClick(ID);
                }
            }
        }
    }
}
