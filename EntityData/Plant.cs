using LofiHollow.Entities;
using LofiHollow.EntityData;
using LofiHollow.Managers;
using ProtoBuf;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.EntityData {
    [ProtoContract]
    public class Plant {
        [ProtoMember(1)]
        public int CurrentStage = 0;
        [ProtoMember(2)]
        public int DayCounter = 0;
        [ProtoMember(3)]
        public string GrowthSeason = "";
        [ProtoMember(4)]
        public int HarvestRevert = -1;
        [ProtoMember(5)]
        public string ProduceName = "";
        [ProtoMember(6)]
        public int RequiredLevel = 0;
        [ProtoMember(7)]
        public int ExpOnHarvest = 0;
        [ProtoMember(8)]
        public int ExpPerExtra = 0;
        [ProtoMember(9)]
        public int ProducePerHarvestMin = 0;
        [ProtoMember(10)]
        public int ProducePerHarvestMax = 0;
        [ProtoMember(11)]
        public bool ProduceIsSeed = false;
        [ProtoMember(12)]
        public bool WateredToday = false;
        [ProtoMember(13)]
        public List<PlantStage> Stages = new();


        public Plant() {

        }

        public Plant(Plant other) {
            CurrentStage = 0;
            DayCounter = 0;
            GrowthSeason = other.GrowthSeason;
            HarvestRevert = other.HarvestRevert;
            ProduceName = other.ProduceName;
            ProducePerHarvestMin = other.ProducePerHarvestMin;
            ProducePerHarvestMax = other.ProducePerHarvestMax;
            ProduceIsSeed = other.ProduceIsSeed;
            WateredToday = false;
            Stages = other.Stages;
        }

        public void DayUpdate() {
            if (GameLoop.World.Player.player.Clock.GetSeason() == GrowthSeason || GrowthSeason == "Any") {
                if (WateredToday) {
                    DayCounter++;
                    if (DayCounter > Stages[CurrentStage].DaysToNext) {
                        if (Stages.Count > CurrentStage + 1) {
                            CurrentStage++;
                            DayCounter = 1;
                        }
                    }
                    WateredToday = false;
                }
            }
        }

        public void Harvest(Player harvester) {
            if (CurrentStage != -1 && Stages[CurrentStage].HarvestItem != "") {
                Item produce = new(Stages[CurrentStage].HarvestItem);
                produce.ItemQuantity = ProducePerHarvestMin;
                if (ProducePerHarvestMax > ProducePerHarvestMin)
                    produce.ItemQuantity += GameLoop.rand.Next(ProducePerHarvestMax - ProducePerHarvestMin);

                if (ProduceIsSeed) {
                    Plant plant = new(this);
                    plant.CurrentStage = 0;
                    plant.DayCounter = 0;
                    produce.Plant = plant;
                }


                CommandManager.AddItemToInv(harvester, produce);

                CurrentStage = HarvestRevert;
                DayCounter = 0;
            }
        }
    }
}
