using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.EntityData;
using LofiHollow.Minigames.Archaeology;
using LofiHollow.Minigames.Picross;
using LofiHollow.Missions;
using Newtonsoft.Json; 
using System.Collections.Generic; 

namespace LofiHollow.DataTypes {
    [JsonObject(MemberSerialization.OptOut)]
    public class Mod {
        public bool Enabled = true;

        public ModMetadata Metadata = new();

        public List<Constructible> ModConstructs = new();
        public List<CraftingRecipe> ModRecipes = new();
        public List<Item> ModItems = new();  
        public List<Mission> ModMissions = new(); 
        public List<Monster> ModMonsters = new();
        public List<NPC> ModNPCs = new();
        public List<Skill> ModSkills = new();
        public List<ArchArtifact> ModArtifacts = new();
        public List<PicrossPuzzle> ModPicross = new();

        public List<Tile> ModTiles = new();
        public List<ModMap> ModMaps = new();
        public List<ModScript> ModScripts = new();
    }
}
