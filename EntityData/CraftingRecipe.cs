using LofiHollow.Entities;
using LofiHollow.Managers;
using System;
using System.Collections.Generic;
using ProtoBuf;

namespace LofiHollow.EntityData {
    [ProtoContract]
    public class CraftingRecipe {
        [ProtoMember(1)]
        public string Name = "";
        [ProtoMember(2)]
        public string FinishedID = "";
        [ProtoMember(3)]
        public int FinishedQty = 1;
        [ProtoMember(4)]
        public string Skill = "";
        [ProtoMember(5)]
        public int RequiredLevel = 1;
        [ProtoMember(6)]
        public int ExpGranted = 0;

        [ProtoMember(7)]
        public bool HasQuality = false;
        [ProtoMember(8)]
        public bool WeightBasedOutput = false;

        [ProtoMember(9)]
        public List<ConstructionMaterial> SpecificMaterials = new();
        [ProtoMember(10)]
        public List<CraftComponent> GenericMaterials = new();
        [ProtoMember(11)]
        public List<ToolData> RequiredTools = new();


        public CraftingRecipe() { }

        public void Craft(Player act, int quantity, int MinQuality) {
            if (ActorCanCraft(act, Skill, quantity, MinQuality) > -1) {
                double TotalQuality = 0;
                double TotalItems = 0;
                double TotalWeight = 0;

                int individualQuality = 0;

                for (int i = 0; i < SpecificMaterials.Count; i++) {
                    int quantityLeft = SpecificMaterials[i].ItemQuantity * quantity;
                    for (int j = 0; j < act.Inventory.Length; j++) {
                        if (act.Inventory[j].FullName() == SpecificMaterials[i].Name) {
                            if (act.Inventory[i].Quality == 0 || act.Inventory[i].Quality >= MinQuality) {
                                if (act.Inventory[j].ItemQuantity > quantityLeft) {
                                    act.Inventory[j].ItemQuantity -= quantityLeft;
                                    if (act.Inventory[j].Quality > individualQuality)
                                        individualQuality = act.Inventory[j].Quality;
                                    quantityLeft = 0;
                                    TotalWeight += act.Inventory[j].Weight;
                                } else if (act.Inventory[j].ItemQuantity == quantityLeft) {
                                    quantityLeft = 0;
                                    if (act.Inventory[j].Quality > individualQuality)
                                        individualQuality = act.Inventory[j].Quality;
                                    TotalWeight += act.Inventory[j].Weight;
                                    act.Inventory[j] = new("lh:(EMPTY)");
                                } else {
                                    quantityLeft -= act.Inventory[j].ItemQuantity;
                                    if (act.Inventory[j].Quality > individualQuality)
                                        individualQuality = act.Inventory[j].Quality;
                                    TotalWeight += act.Inventory[j].Weight;
                                    act.Inventory[j] = new("lh:(EMPTY)");
                                }
                            }
                        }
                        if (quantityLeft == 0) {
                            break;
                        }
                    }

                    TotalQuality += individualQuality * SpecificMaterials[i].ItemQuantity;
                    TotalItems += SpecificMaterials[i].ItemQuantity;
                    individualQuality = 0;
                }

                for (int i = 0; i < GenericMaterials.Count; i++) {
                    int amountLeft = GenericMaterials[i].Quantity * quantity;
                    if (GenericMaterials[i].CountsAsMultiple)
                        amountLeft *= GenericMaterials[i].Tier;

                    for (int j = 0; j < act.Inventory.Length; j++) {
                        if (act.Inventory[j].Craft != null && act.Inventory[j].ItemQuantity > 0 && (act.Inventory[i].Quality == 0 || act.Inventory[i].Quality >= MinQuality)) {
                            List<CraftComponent> craft = act.Inventory[j].Craft;
                            for (int k = 0; k < craft.Count; k++) {
                                if (craft[k].Property == GenericMaterials[i].Property) {
                                    if (craft[k].CountsAsMultiple) {
                                        if (craft[k].Tier > amountLeft) {
                                            if (act.Inventory[j].Quality > individualQuality)
                                                individualQuality = act.Inventory[j].Quality;
                                            amountLeft = 0;
                                            TotalWeight += act.Inventory[j].Weight;
                                            if (act.Inventory[j].ItemQuantity > 1)
                                                act.Inventory[j].ItemQuantity--;
                                        } else if (craft[k].Tier == amountLeft) {
                                            if (act.Inventory[j].Quality > individualQuality)
                                                individualQuality = act.Inventory[j].Quality;
                                            amountLeft = 0;
                                            TotalWeight += act.Inventory[j].Weight;
                                            if (act.Inventory[j].ItemQuantity > 1)
                                                act.Inventory[j].ItemQuantity--;
                                        } else {
                                            if (act.Inventory[j].ItemQuantity * craft[k].Tier > amountLeft) {
                                                if (act.Inventory[j].Quality > individualQuality)
                                                    individualQuality = act.Inventory[j].Quality;
                                                amountLeft = 0;
                                                TotalWeight += act.Inventory[j].Weight;
                                                int amountRequired = (int)Math.Ceiling((double)amountLeft / (double)craft[k].Tier);
                                                act.Inventory[j].ItemQuantity -= amountRequired;
                                            } else if (act.Inventory[j].ItemQuantity * craft[k].Tier == amountLeft) {
                                                if (act.Inventory[j].Quality > individualQuality)
                                                    individualQuality = act.Inventory[j].Quality;
                                                amountLeft = 0;
                                                TotalWeight += act.Inventory[j].Weight;
                                            } else {
                                                if (act.Inventory[j].Quality > individualQuality)
                                                    individualQuality = act.Inventory[j].Quality;
                                                amountLeft -= craft[k].Tier * act.Inventory[j].ItemQuantity;
                                                TotalWeight += act.Inventory[j].Weight;
                                                act.Inventory[j].ItemQuantity = 0;
                                            }
                                        }
                                    } else {
                                        if (act.Inventory[j].ItemQuantity > amountLeft) {
                                            if (act.Inventory[j].Quality > individualQuality)
                                                individualQuality = act.Inventory[j].Quality;
                                            TotalWeight += act.Inventory[j].Weight * amountLeft;
                                            act.Inventory[j].ItemQuantity -= amountLeft;
                                            amountLeft = 0;
                                        } else if (act.Inventory[j].ItemQuantity == amountLeft) {
                                            if (act.Inventory[j].Quality > individualQuality)
                                                individualQuality = act.Inventory[j].Quality;
                                            amountLeft = 0;
                                            TotalWeight += act.Inventory[j].Weight * act.Inventory[j].ItemQuantity;
                                            act.Inventory[j].ItemQuantity = 0;
                                        } else {
                                            if (act.Inventory[j].Quality > individualQuality)
                                                individualQuality = act.Inventory[j].Quality;
                                            amountLeft -= act.Inventory[j].ItemQuantity;
                                            TotalWeight += act.Inventory[j].Weight * act.Inventory[j].ItemQuantity;
                                            act.Inventory[j].ItemQuantity = 0;
                                        }
                                    }
                                }

                                if (amountLeft <= 0) {
                                    break;
                                }
                            }
                        }

                        if (amountLeft <= 0) {
                            break;
                        }
                    }

                    TotalQuality += individualQuality * GenericMaterials[i].Quantity;
                    TotalItems += GenericMaterials[i].Quantity;
                    individualQuality = 0;

                    for (int j = 0; j < GameLoop.World.Player.player.Inventory.Length; j++) {
                        if (GameLoop.World.Player.player.Inventory[j].ItemQuantity <= 0) {
                            act.Inventory[j] = new("lh:(EMPTY)");
                        }
                    }
                }

                int TotalIngredientQuality = 0;
                int QualityCap = (int)Math.Floor((GameLoop.World.Player.player.Skills[Skill].Level + 1f) / 10f) + 1;

                if (TotalQuality > 0)
                    TotalIngredientQuality = (int)Math.Floor(TotalQuality / TotalItems);

                Item finished = new(FinishedID);

                int Qty;

                if (WeightBasedOutput) {
                    Qty = (int)Math.Floor(TotalWeight / finished.Weight);
                    GameLoop.UIManager.AddMsg(TotalWeight + ", " + finished.Weight);
                } else {
                    Qty = FinishedQty * quantity;
                }

                if (finished.IsStackable) {
                    finished.ItemQuantity = Qty;
                    finished.Quality = Math.Min(TotalIngredientQuality, QualityCap);
                    CommandManager.AddItemToInv(act, finished);
                } else {
                    for (int i = 0; i < Qty; i++) {
                        Item one = new(finished);
                        one.Quality = Math.Min(TotalIngredientQuality, QualityCap);
                        CommandManager.AddItemToInv(act, one);
                    }
                }

                act.Skills[Skill].GrantExp(ExpGranted * quantity);
            }
        }


        public int ActorCanCraft(Player act, string menuSkill, int quantity, int MinQuality) {
            if (menuSkill != Skill)
                return -1;
            if (!act.Skills.ContainsKey(Skill))
                return -1;
            if (act.Skills[Skill].Level < RequiredLevel)
                return -1;

            double TotalQuality = 0;
            double TotalItems = 0;

            for (int i = 0; i < RequiredTools.Count; i++) {
                if (!RequiredTools[i].ActorHasTool(act))
                    return -1;
            }

            for (int i = 0; i < GenericMaterials.Count; i++) {
                int quality = GenericMaterials[i].ActorHasComponent(act, quantity, MinQuality);
                if (quality < 0) {
                    return -1;
                } else {
                    TotalQuality += quality;
                    TotalItems += GenericMaterials[i].Quantity;
                }
            }

            for (int i = 0; i < SpecificMaterials.Count; i++) {
                int quality = SpecificMaterials[i].ActorHasComponent(act, quantity, MinQuality);
                if (quality < 0) {
                    return -1;
                } else {
                    TotalQuality += quality;
                    TotalItems += SpecificMaterials[i].ItemQuantity;
                }
            }

            int TotalIngredientQuality = 0;

            if (TotalQuality > 0)
                TotalIngredientQuality = (int)Math.Floor(TotalQuality / TotalItems);

            return TotalIngredientQuality;
        }
    }
}
