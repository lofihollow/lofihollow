using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LofiHollow.DataTypes;
using Steamworks;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptOut)]
    public class Apartment {
        public Map map;
        public int DaysLeft = 0;

        public bool GuestsAllowedWhileOut = false;
        public bool GuestsAllowedWhileIn = true;
        public bool Whitelist = true;
        public List<SteamId> AllowedGuests = new();

        [JsonConstructor]
        public Apartment() { }


        public void SetupNew(string AptName, int days) {
            map = new(GameLoop.World.GetMap(new Point3D(AptName, 0, 0, 0)));
            map.MinimapTile.name = GameLoop.World.Player.Name + " Apartment";
            DaysLeft = days;
        }

        public void AddDays(int days) {
            DaysLeft += days;
        }

        public void AddGuest(SteamId id) {
            if (!AllowedGuests.Contains(id))
                AllowedGuests.Add(id);
        }

        public void RemoveGuest(SteamId id) {
            if (AllowedGuests.Contains(id))
                AllowedGuests.Remove(id);
        }

        public bool CanEnter(SteamId guestID, bool PlayerHome) {
            if (DaysLeft >= 0) {
                if (PlayerHome) {
                    if (GuestsAllowedWhileIn)
                        if (Whitelist == AllowedGuests.Contains(guestID))
                            return true;
                } else {
                    if (GuestsAllowedWhileOut) {
                        if (Whitelist == AllowedGuests.Contains(guestID))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
