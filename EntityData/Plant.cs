﻿using LofiHollow.Entities; 
using LofiHollow.Managers;
using Newtonsoft.Json;
using SadRogue.Primitives; 
using System.Collections.Generic;
using LofiHollow.DataTypes;

namespace LofiHollow.EntityData {
    [JsonObject(MemberSerialization.OptIn)]
    public class Plant {
        [JsonProperty]
        public int CurrentStage = 0;

        [JsonProperty]
        public int DayCounter = 0;

        [JsonProperty]
        public string GrowthSeason = "";

        [JsonProperty]
        public int HarvestRevert = -1;

        [JsonProperty]
        public string ProduceName = ""; 

        [JsonProperty]
        public int RequiredLevel = 0;

        [JsonProperty]
        public int ExpOnHarvest = 0;

        [JsonProperty]
        public int ExpPerExtra = 0;

        [JsonProperty]
        public int ProducePerHarvestMin = 0;
        [JsonProperty]
        public int ProducePerHarvestMax = 0;

        [JsonProperty]
        public bool ProduceIsSeed = false;

        [JsonProperty]
        public bool WateredToday = false;



        [JsonProperty]
        public List<PlantStage> Stages = new();


        [JsonConstructor]
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
            if (GameLoop.World.Player.Clock.GetSeason() == GrowthSeason || GrowthSeason == "Any") {
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
                Item produce = Item.Copy(Stages[CurrentStage].HarvestItem);
                produce.Quantity = ProducePerHarvestMin;
                if (ProducePerHarvestMax > ProducePerHarvestMin)
                    produce.Quantity += GameLoop.rand.Next(ProducePerHarvestMax - ProducePerHarvestMin);

                if (ProduceIsSeed) {
                    Plant plant = new(this);
                    plant.CurrentStage = 0;
                    plant.DayCounter = 0;
                    produce.Plant = plant;
                }


                harvester.AddItemToInventory(produce);

                CurrentStage = HarvestRevert;
                DayCounter = 0; 
            }
        }
    }
}
