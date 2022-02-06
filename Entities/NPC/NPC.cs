using Newtonsoft.Json;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;

namespace LofiHollow.Entities.NPC {
    [JsonObject(MemberSerialization.OptOut)]
    [ProtoContract]
    public class NPC : Actor {
        [ProtoMember(1)]
        public int npcID;
        [ProtoMember(2)]
        public NPCAi AI;
        [ProtoMember(3)]
        public string Occupation = "";

        [ProtoMember(4)]
        public int BirthMonth = 1;
        [ProtoMember(5)]
        public int BirthDay = 2;

        [ProtoMember(6)]
        public string Introduction = "";

        [ProtoMember(7)]
        public Dictionary<string, string> Greetings = new();

        [ProtoMember(8)]
        public Dictionary<string, string> Farewells = new();

        [ProtoMember(9)]
        public Dictionary<string, string> ChitChats = new();

        [ProtoMember(10)]
        public Dictionary<string, string> GiftResponses = new();

        [ProtoMember(11)]
        public List<string> HatedGiftIDs = new();
        [ProtoMember(12)]
        public List<string> DislikedGiftIDs = new();
        [ProtoMember(13)]
        public List<string> LikedGiftIDs = new();
        [ProtoMember(14)]
        public List<string> LovedGiftIDs = new();

        [JsonIgnore]
        public bool ReceivedGiftToday = false;

        [ProtoMember(15)]
        public ShopData Shop = new();

        [JsonConstructor]
        public NPC() { }


        public NPC(NPC other) {
            AI = other.AI;
            ForegroundR = other.ForegroundR;
            ForegroundG = other.ForegroundG;
            ForegroundB = other.ForegroundB;
            ActorGlyph = other.ActorGlyph;
        }

        public string RelationshipDescriptor() {
            int PlayerOpinion = 0;

            if (GameLoop.World.Player.player.MetNPCs.ContainsKey(Name)) {
                PlayerOpinion = GameLoop.World.Player.player.MetNPCs[Name];
            }

            if (PlayerOpinion == -100)
                return "Nemesis";

            if (PlayerOpinion <= -50)
                return "Hate";

            if (PlayerOpinion <= -25)
                return "Unfriendly";

            if (PlayerOpinion <= -10)
                return "Dislike";

            if (PlayerOpinion < 10)
                return "Neutral";

            if (PlayerOpinion <= 25)
                return "Like";

            if (PlayerOpinion <= 50)
                return "Friendly";

            if (PlayerOpinion <= 100)
                return "Close Friend";

            if (PlayerOpinion == 100)
                return "Best Friend";

            return "ERROR";
        }

        public bool IsBirthday() {
            return GameLoop.World.Player.player.Clock.IsItThisDay(BirthMonth, BirthDay);
        }


        public void UpdateChitChats() {
            int first = GameLoop.rand.Next(0, ChitChats.Count);
            int second = GameLoop.rand.Next(0, ChitChats.Count);
            int third = GameLoop.rand.Next(0, ChitChats.Count);
            int fourth = GameLoop.rand.Next(0, ChitChats.Count);
            int fifth = GameLoop.rand.Next(0, ChitChats.Count);

            while (second == first)
                second = GameLoop.rand.Next(0, ChitChats.Count);

            while (third == second || third == first)
                third = GameLoop.rand.Next(0, ChitChats.Count);

            while (fourth == third || fourth == second || fourth == first)
                fourth = GameLoop.rand.Next(0, ChitChats.Count);

            while (fifth == fourth || fifth == third || fifth == second || fifth == first)
                fifth = GameLoop.rand.Next(0, ChitChats.Count);


            GameLoop.UIManager.DialogueWindow.chitChat1 = ChitChats.ElementAt(first).Key;
            GameLoop.UIManager.DialogueWindow.chitChat2 = ChitChats.ElementAt(second).Key;
            GameLoop.UIManager.DialogueWindow.chitChat3 = ChitChats.ElementAt(third).Key;
            GameLoop.UIManager.DialogueWindow.chitChat4 = ChitChats.ElementAt(fourth).Key;
            GameLoop.UIManager.DialogueWindow.chitChat5 = ChitChats.ElementAt(fifth).Key;
        }

        public string ReactGift(string ID) {
            string react = "Neutral";
            int relModifier = 0;
            if (HatedGiftIDs.Contains(ID)) {
                relModifier = -10;
                react = "Hated";
            } else if (DislikedGiftIDs.Contains(ID)) {
                relModifier = -5;
                react = "Disliked";
            } else if (LikedGiftIDs.Contains(ID) || ID == "-3") { // 1 silver or more = liked gift
                relModifier = 5;
                react = "Liked";
            } else if (LovedGiftIDs.Contains(ID) || ID == "-2") { // More than 10 silver = loved gift
                relModifier = 10;
                react = "Loved";
            }

            if (IsBirthday())
                relModifier *= 2;

            if (!ReceivedGiftToday && GameLoop.World.Player.player.MetNPCs.ContainsKey(Name)) {
                GameLoop.World.Player.player.MetNPCs[Name] += relModifier;
                ReceivedGiftToday = true;
            }

            return react;
        }
    }
}
