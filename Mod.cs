using LofiHollow.Entities;
using LofiHollow.Entities.NPC;
using LofiHollow.EntityData;
using LofiHollow.Missions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow {
    [JsonObject(MemberSerialization.OptOut)]
    public class Mod {
        public bool Enabled = true;

        public string Name = "";
        public string Package = "";


        public List<Constructible> ModConstructs = new();
        public List<CraftingRecipe> ModRecipes = new();
        public List<FishDef> ModFish = new();
        public List<Item> ModItems = new();  
        public List<Mission> ModMissions = new(); 
        public List<Monster> ModMonsters = new();
        public List<NPC> ModNPCs = new();
        public List<Skill> ModSkills = new();
    }
}
