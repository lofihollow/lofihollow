using SadRogue.Primitives;
using LofiHollow.DataTypes;

namespace LofiHollow.Managers {
    public class BuildManager {


        public static string GetApartmentNumber(string map, Point pos) {
            if (map == "Noonbreeze Apartments") {
                Rectangle A = new(12, 7, 7, 11);
                Rectangle B = new(20, 7, 7, 11);
                Rectangle C = new(28, 7, 7, 11);
                Rectangle D = new(36, 7, 7, 11);
                Rectangle E = new(44, 7, 7, 11);
                Rectangle F = new(12, 23, 7, 11);
                Rectangle G = new(20, 7, 7, 11);
                Rectangle H = new(28, 7, 7, 11);
                Rectangle I = new(36, 7, 7, 11);
                Rectangle J = new(44, 7, 7, 11);

                if (A.Contains(pos))
                    return "Noonbreeze Apt A";
                if (B.Contains(pos))
                    return "Noonbreeze Apt B";
                if (C.Contains(pos))
                    return "Noonbreeze Apt C";
                if (D.Contains(pos))
                    return "Noonbreeze Apt D";
                if (E.Contains(pos))
                    return "Noonbreeze Apt E";
                if (F.Contains(pos))
                    return "Noonbreeze Apt F";
                if (G.Contains(pos))
                    return "Noonbreeze Apt G";
                if (H.Contains(pos))
                    return "Noonbreeze Apt H";
                if (I.Contains(pos))
                    return "Noonbreeze Apt I";
                if (J.Contains(pos))
                    return "Noonbreeze Apt J";
            }

            return "ERROR";
        }


        public static bool CanBuildHere(string location, bool furniture) {
            if (GameLoop.World.Player.OwnedLocations.ContainsKey(location)) {
                if (GameLoop.World.Player.OwnedLocations[location].OnlyFurniture == false || GameLoop.World.Player.OwnedLocations[location].OnlyFurniture == furniture) {
                    return true;
                }
            }
            
            

            if (GameLoop.World.Player.MapPos == new Point3D(-1, 0, 0) && GameLoop.CheckFlag("farm")) {
                return true;
            }

            return false;
        }
    }
}
