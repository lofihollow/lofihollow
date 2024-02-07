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
using System.Linq;

namespace LofiHollow {
    public static class Helper {
        public static double CursorTicked = 0;
        public static bool CursorVisible = true;
          
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
         
        public static bool PlayerHasData(string query) {
            return GameLoop.World.Player.MiscData.ContainsKey(query);
        }

        public static bool PlayerDataAbove(string query, int target) {
            if (query == "GLOBAL_TIME") { return GameLoop.World.Player.Clock.GetCurrentTime() > target; } 
            if (query == "GLOBAL_DAY") { return GameLoop.World.Player.Clock.Day > target; }
            if (query == "GLOBAL_WEEKDAY") { return GameLoop.World.Player.Clock.Day % 7 > target; }
            if (query == "GLOBAL_SEASON") { return GameLoop.World.Player.Clock.Month > target; } 
            if (query == "GLOBAL_APTITUDE") { return GameLoop.World.Player.MagitechAptitude > target; } 
            if (query == "GLOBAL_LAW") { return GameLoop.World.Player.AlignmentLaw > target; }
            if (query == "GLOBAL_GOOD") { return GameLoop.World.Player.AlignmentGood > target; }
            if (query == "GLOBAL_STR") { return GameLoop.World.Player.Strength > target; }
            if (query == "GLOBAL_DEX") { return GameLoop.World.Player.Dexterity > target; }
            if (query == "GLOBAL_CON") { return GameLoop.World.Player.Constitution > target; }
            if (query == "GLOBAL_INT") { return GameLoop.World.Player.Intelligence > target; }
            if (query == "GLOBAL_WIS") { return GameLoop.World.Player.Wisdom > target; }
            if (query == "GLOBAL_CHA") { return GameLoop.World.Player.Charisma > target; }

            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                return GameLoop.World.Player.MiscData[query] > target;
            }
            return false;
        }

        public static bool PlayerDataBelow(string query, int target) {
            if (query == "GLOBAL_TIME") { return GameLoop.World.Player.Clock.GetCurrentTime() < target; }
            if (query == "GLOBAL_DAY") { return GameLoop.World.Player.Clock.Day < target; }
            if (query == "GLOBAL_WEEKDAY") { return GameLoop.World.Player.Clock.Day % 7 < target; }
            if (query == "GLOBAL_SEASON") { return GameLoop.World.Player.Clock.Month < target; }
            if (query == "GLOBAL_APTITUDE") { return GameLoop.World.Player.MagitechAptitude < target; }
            if (query == "GLOBAL_LAW") { return GameLoop.World.Player.AlignmentLaw < target; }
            if (query == "GLOBAL_GOOD") { return GameLoop.World.Player.AlignmentGood < target; }
            if (query == "GLOBAL_STR") { return GameLoop.World.Player.Strength < target; }
            if (query == "GLOBAL_DEX") { return GameLoop.World.Player.Dexterity < target; }
            if (query == "GLOBAL_CON") { return GameLoop.World.Player.Constitution < target; }
            if (query == "GLOBAL_INT") { return GameLoop.World.Player.Intelligence < target; }
            if (query == "GLOBAL_WIS") { return GameLoop.World.Player.Wisdom < target; }
            if (query == "GLOBAL_CHA") { return GameLoop.World.Player.Charisma < target; }

            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                return GameLoop.World.Player.MiscData[query] < target;
            }
            return false;
        }

        public static bool PlayerDataEquals(string query, int target) {
            if (query == "GLOBAL_TIME") { return GameLoop.World.Player.Clock.GetCurrentTime() == target; }
            if (query == "GLOBAL_DAY") { return GameLoop.World.Player.Clock.Day == target; }
            if (query == "GLOBAL_WEEKDAY") { return GameLoop.World.Player.Clock.Day % 7 == target; }
            if (query == "GLOBAL_SEASON") { return GameLoop.World.Player.Clock.Month == target; }
            if (query == "GLOBAL_APTITUDE") { return GameLoop.World.Player.MagitechAptitude == target; }
            if (query == "GLOBAL_LAW") { return GameLoop.World.Player.AlignmentLaw == target; }
            if (query == "GLOBAL_GOOD") { return GameLoop.World.Player.AlignmentGood == target; }
            if (query == "GLOBAL_STR") { return GameLoop.World.Player.Strength == target; }
            if (query == "GLOBAL_DEX") { return GameLoop.World.Player.Dexterity == target; }
            if (query == "GLOBAL_CON") { return GameLoop.World.Player.Constitution == target; }
            if (query == "GLOBAL_INT") { return GameLoop.World.Player.Intelligence == target; }
            if (query == "GLOBAL_WIS") { return GameLoop.World.Player.Wisdom == target; }
            if (query == "GLOBAL_CHA") { return GameLoop.World.Player.Charisma == target; }

            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                return GameLoop.World.Player.MiscData[query] == target;
            }
            return false;
        }

        public static bool PlayerDataNot(string query, int target) {
            if (query == "GLOBAL_TIME") { return GameLoop.World.Player.Clock.GetCurrentTime() != target; }
            if (query == "GLOBAL_DAY") { return GameLoop.World.Player.Clock.Day != target; }
            if (query == "GLOBAL_WEEKDAY") { return GameLoop.World.Player.Clock.Day % 7 != target; }
            if (query == "GLOBAL_SEASON") { return GameLoop.World.Player.Clock.Month != target; }
            if (query == "GLOBAL_APTITUDE") { return GameLoop.World.Player.MagitechAptitude != target; }
            if (query == "GLOBAL_LAW") { return GameLoop.World.Player.AlignmentLaw != target; }
            if (query == "GLOBAL_GOOD") { return GameLoop.World.Player.AlignmentGood != target; }
            if (query == "GLOBAL_STR") { return GameLoop.World.Player.Strength != target; }
            if (query == "GLOBAL_DEX") { return GameLoop.World.Player.Dexterity != target; }
            if (query == "GLOBAL_CON") { return GameLoop.World.Player.Constitution != target; }
            if (query == "GLOBAL_INT") { return GameLoop.World.Player.Intelligence != target; }
            if (query == "GLOBAL_WIS") { return GameLoop.World.Player.Wisdom != target; }
            if (query == "GLOBAL_CHA") { return GameLoop.World.Player.Charisma != target; }

            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                return GameLoop.World.Player.MiscData[query] != target;
            }
            return false;
        }

        public static int GetPlayerData(string query) {
            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                return GameLoop.World.Player.MiscData[query];
            }
            return int.MinValue;
        }

        public static void ErasePlayerData(string query) {
            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                GameLoop.World.Player.MiscData.Remove(query);
            }
        }

        public static void SetPlayerData(string query, int newData) {
            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                GameLoop.World.Player.MiscData[query] = newData;
            } else {
                GameLoop.World.Player.MiscData.Add(query, newData);
            }
        }

        public static void ModifyPlayerData(string query, int modBy) {
            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                GameLoop.World.Player.MiscData[query] += modBy;
            } else {
                GameLoop.World.Player.MiscData.Add(query, modBy);
            }
        }

        public static bool PlayerDataAsBool(string query) {
            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                if (GameLoop.World.Player.MiscData[query] == 0)
                    return false;
                else
                    return true;
            } else {
                return false;
            }
        }

        public static void PlayerDataToggleBool(string query) {
            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                if (GameLoop.World.Player.MiscData[query] == 0) {
                    GameLoop.World.Player.MiscData[query] = 1;
                } else {
                    GameLoop.World.Player.MiscData[query] = 0;
                }
            } else {
                GameLoop.World.Player.MiscData.Add(query, 1);
            }
        }

        public static void PlayerDataFlipBit(string query, int position) {
            if (GameLoop.World.Player.MiscData.ContainsKey(query)) {
                GameLoop.World.Player.MiscData[query] ^= (1 << position);
            } else {
                GameLoop.World.Player.MiscData.Add(query, 0);
                GameLoop.World.Player.MiscData[query] ^= (1 << position);
            }
        }

        public static bool KeyPressed(Keys key) {
            return GameHost.Instance.Keyboard.IsKeyPressed(key);
        }

        static HashSet<Keys> TriggeredHotkeys = new();
        static HashSet<Keys> SecondaryList = new();
        public static bool HotkeyDown(Keys key) {
            if (!TriggeredHotkeys.Contains(key) && GameHost.Instance.Keyboard.IsKeyPressed(key)) {
                TriggeredHotkeys.Add(key);
                return true;
            }

            return false;
        }

        public static void ClearKeys() {
            SecondaryList.Clear();
            foreach (Keys key in TriggeredHotkeys) {
                if (GameHost.Instance.Keyboard.IsKeyDown(key)) {
                    SecondaryList.Add(key);
                }
            }
            TriggeredHotkeys.Clear();

            foreach (Keys key in SecondaryList) {
                TriggeredHotkeys.Add(key);
            }
        }

        public static bool EitherShift() {
            if (GameHost.Instance.Keyboard.IsKeyDown(Keys.LeftShift) || GameHost.Instance.Keyboard.IsKeyDown(Keys.RightShift))
                return true;
            return false;
        }
        public static bool EitherControl() {
            if (GameHost.Instance.Keyboard.IsKeyDown(Keys.LeftControl) || GameHost.Instance.Keyboard.IsKeyDown(Keys.RightControl))
                return true;
            return false;
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

          

        public static Location? ResolveLoc(string nav) {
            if (GameLoop.World.atlas.ContainsKey(nav))
                return GameLoop.World.atlas[nav]; 

            if (GameLoop.DevMode) {
                GameLoop.World.atlas.Add(nav, new(nav, "", ""));
                return GameLoop.World.atlas[nav];
            }

            return null;
        } 

        public static NodeObject? ResolveObj(string obj) {
            if (GameLoop.World.nodeObjectLibrary.ContainsKey(obj))
                return Clone(GameLoop.World.nodeObjectLibrary[obj]);
            return null;
        }

        public static Item? ResolveItem(string item) {
            if (GameLoop.World.itemLibrary.ContainsKey(item))
                return Clone(GameLoop.World.itemLibrary[item]);
            return null;
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

        public static double Timer() {
            return Math.Floor(GameHost.Instance.GameRunningTotalTime.TotalMilliseconds);
        }

        public static void Flip(this ref bool instance) {
            instance = !instance;
        }

        public static ColoredString GetDarker(this ColoredString instance) {
            for (int i = 0; i < instance.Length; i++) {
                instance[i].Foreground = instance[i].Foreground.GetDarker();
            }
            return instance;
        }

        public static string TimeString(int numMinutes) {
            int hours = numMinutes / 60;
            int min = numMinutes % 60;

            string ampm = "AM";

            if (numMinutes >= 720) {
                ampm = "PM"; 
            }

            if (numMinutes >= 1440) {
                ampm = "AM";
            }


            if (hours > 12)
                hours -= 12;


            return hours.ToString().PadLeft(2, ' ') + ":" + (min.ToString().PadLeft(2, '0')) + " " + ampm;
        }


        public static void PrintClickableBool(this SadConsole.Console instance, int x, int y, string input, ref bool toggler) {
            instance.PrintClickableBool(x, y, new ColoredString(input), ref toggler);
        }
        
        public static void PrintClickableBool(this SadConsole.Console instance, int x, int y, ColoredString input, ref bool toggler) {
            Point mousePos = new MouseScreenObjectState(instance, GameHost.Instance.Mouse).CellPosition;

            ColoredString str = input + Helper.Checkmark(toggler);

            if (mousePos.X >= x && mousePos.X < x + str.Length && mousePos.Y == y) {
                instance.Print(x, y, str.GetDarker());
            }
            else {
                instance.Print(x, y, str);
            }

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos.X >= x && mousePos.X < x + str.Length && mousePos.Y == y) {
                    toggler.Flip();
                }
            }
        }

        public static void PrintScrollableInteger(this Console instance, int x, int y, string prefaceText, ref int number, bool asString = false, int min = int.MinValue, int max = int.MaxValue, int baseStep = 1, int shiftStep = 5, int controlStep = 10, bool onlyPreface = false) {
            Point mousePos = new MouseScreenObjectState(instance, GameHost.Instance.Mouse).CellPosition;
            string numString = number.ToString();

            if (asString)
                numString = number.AsString();


            Rectangle clickableArea = new Rectangle(new Point(x, y), new Point(x + numString.Length + prefaceText.Length - 1, y));

            if (onlyPreface)
                clickableArea = new Rectangle(new Point(x, y), new Point(x + prefaceText.Length - 1, y));

            int mod = baseStep;
            if (Helper.EitherShift())
                mod *= shiftStep;
            if (Helper.EitherControl())
                mod *= controlStep;


            if (clickableArea.Contains(mousePos)) {
                if (GameHost.Instance.Mouse.ScrollWheelValueChange > 0 || GameHost.Instance.Mouse.RightClicked) {
                    number = Math.Clamp(number - mod, min, max);
                }
                else if (GameHost.Instance.Mouse.ScrollWheelValueChange < 0 || GameHost.Instance.Mouse.LeftClicked) {
                    number = Math.Clamp(number + mod, min, max);
                }
            }


            instance.Print(x, y, prefaceText);

            if (!onlyPreface)
                instance.Print(x + prefaceText.Length, y, numString);
        }

        public static void PrintStringField(this Console instance, int x, int y, string prefaceText, ref string field, ref string selected, string plainName, bool onlyPreface = false) {
            Point mousePos = new MouseScreenObjectState(instance, GameHost.Instance.Mouse).CellPosition;
            if (field == null)
                field = "";

            if (field != null) {
                Rectangle clickableArea = new Rectangle(new Point(x, y), new Point(x + field.Length + prefaceText.Length - 1, y));

                if (onlyPreface)
                    clickableArea = new Rectangle(new Point(x, y), new Point(x + prefaceText.Length - 1, y));

                int end = x + field.Length;
                if (field.Length == 0) {
                    clickableArea = new Rectangle(new Point(x, y), new Point(x + prefaceText.Length + "(blank)".Length - 1, y));
                    end = x + "(blank)".Length;
                    instance.Print(x, y, prefaceText, selected == plainName ? Color.Green : clickableArea.Contains(mousePos) ? Color.Yellow : Color.White);

                    if (!onlyPreface)
                        instance.Print(x + prefaceText.Length, y, "(blank)", clickableArea.Contains(mousePos) ? Color.Yellow : Color.White);
                }
                else {
                    instance.Print(x, y, prefaceText, selected == plainName ? Color.Green : clickableArea.Contains(mousePos) ? Color.Yellow : Color.White);

                    if (!onlyPreface)
                        instance.Print(x + prefaceText.Length, y, field, clickableArea.Contains(mousePos) ? Color.Yellow : Color.White);
                }

                if (GameHost.Instance.Mouse.LeftClicked) {
                    if (clickableArea.Contains(mousePos))
                        selected = plainName;
                }

                if (selected == plainName) {
                    if (Helper.CursorTicked + 200 < Timer()) {
                        Helper.CursorTicked = Timer();
                        Helper.CursorVisible = !Helper.CursorVisible;
                    }

                    if (!onlyPreface)
                        instance.Print(end + prefaceText.Length, y, Helper.CursorVisible ? "_" : " ");

                    foreach (var key in GameHost.Instance.Keyboard.KeysPressed) {
                        if ((key.Character >= 'A' && key.Character <= 'z') || (key.Character >= '0' && key.Character <= '9'
                            || key.Character == ';' || key.Character == ':' || key.Character == '|' || key.Character == '-' || key.Character == '+'
                            || key.Character == '.' || key.Character == ',' || key.Character == '(' || key.Character == ')' || key.Character == '\''
                            || key.Character == '!' || key.Character == '?')) {
                            field += key.Character;
                        }

                        if (key.Character == '$') {
                            field += (char) 15;
                        }
                    }

                    if (GameHost.Instance.Keyboard.IsKeyPressed(Keys.Space)) {
                        field += " ";
                    }

                    if (GameHost.Instance.Keyboard.IsKeyPressed(Keys.Back)) {
                        if (field.Length > 0)
                            field = field[0..^1];
                    }

                    if (GameHost.Instance.Keyboard.IsKeyPressed(Keys.Enter)) {
                        selected = "none";
                    }
                }
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

        public static void PrintAdjustableInt(this Console instance, int x, int y, int width, ref int target, int min, int max) {
            Point mousePos = new MouseScreenObjectState(instance, GameHost.Instance.Mouse).CellPosition;
            Point minusPos = new Point(x, y);
            Point plusPos = new Point(x + 3 + width, y);
            instance.Print(x, y, "-", mousePos == minusPos ? Color.Yellow : Color.White);
            instance.Print(x + 2, y, target.ToString().PadLeft(width));
            instance.Print(x + 3 + width, y, "+", mousePos == plusPos ? Color.Yellow : Color.White);

            int mod = 1;

            if (Helper.EitherShift())
                mod *= 5;

            if (Helper.EitherControl())
                mod *= 10;

            if (GameHost.Instance.Mouse.LeftClicked && mousePos == minusPos) {
                target = Math.Clamp(target - mod, min, max);
            }

            if (GameHost.Instance.Mouse.LeftClicked && mousePos == plusPos) {
                target = Math.Clamp(target + mod, min, max);
            }
        }

        public static void PrintVertical(this Console instance, int x, int y, string str, bool down = true) {
            instance.PrintVertical(x, y, new ColoredString(str), down);
        }

        public static void PrintVertical(this Console instance, int x, int y, ColoredString str, bool down = true) {
            for (int i = 0; i < str.Length; i++) {
                int printY = y + (down ? i : -i);
                if (printY >= 0 && printY < instance.Height) {
                    instance.Print(x, printY, str[i].Glyph.AsString(), str[i].Foreground, str[i].Background);
                }
            }
        }

        public static void PrintDecorated(this SadConsole.Console instance, int x, int y, DecoratedString str) {
            instance.Print(x, y, str.Text);

            if (str.Decorators != null) {
                for (int i = 0; i < str.Decorators.Length; i++) {
                    instance.AddDecorator(x + i, y, 1, str.Decorators[i]);
                }
            }
        }

        public static int PrintMultiLine(this Console instance, int x, int y, string str, int width, int colR = 255, int colG = 255, int colB = 255) {
            List<string> words = str.Split(" ").ToList();

            int cX = x;
            int cY = y;

            foreach (string word in words) {
                if (cX + word.Length + 1 < width) {
                    instance.Print(cX, cY, word + " ");
                    cX += word.Length + 1;
                }
                else {
                    cX = x;
                    cY++;
                    instance.Print(cX, cY, word + " ");
                    cX += word.Length + 1;
                }
            }

            return cY;
        }

        public static void PrintClickable(this SadConsole.Console instance, int x, int y, string str, Action OnClick) { 
            instance.PrintClickable(x, y, new ColoredString(str), OnClick);
        }

        public static void PrintClickable(this SadConsole.Console instance, int x, int y, ColoredString str, Action OnClick) {
            Point mousePos = new MouseScreenObjectState(instance, GameHost.Instance.Mouse).CellPosition;
            int length = str.Length - 1;

            instance.Print(x, y, mousePos.X >= x && mousePos.X <= x + length && mousePos.Y == y ? str.GetDarker() : str);

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos.X >= x && mousePos.X <= x + length && mousePos.Y == y) {
                    OnClick();
                }
            }
        }

        public static void PrintClickable(this SadConsole.Console instance, int x, int y, string str, Action<string> OnClick, string ID) {
            instance.PrintClickable(x, y, new ColoredString(str), OnClick, ID); 
        }

        public static void PrintClickable(this SadConsole.Console instance, int x, int y, ColoredString str, Action<string> OnClick, string ID) {
            Point mousePos = new MouseScreenObjectState(instance, GameHost.Instance.Mouse).CellPosition;
            int length = str.Length - 1;

            instance.Print(x, y, mousePos.X >= x && mousePos.X <= x + length && mousePos.Y == y ? str.GetDarker() : str);

            if (GameHost.Instance.Mouse.LeftClicked) {
                if (mousePos.X >= x && mousePos.X <= x + length && mousePos.Y == y) {
                    OnClick(ID);
                }
            }
        } 
    }
}
