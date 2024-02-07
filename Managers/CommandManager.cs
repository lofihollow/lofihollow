using System;
using SadRogue.Primitives;
using System.Text;
using LofiHollow.Entities;
using GoRogue.DiceNotation;
using SadConsole;
using Newtonsoft.Json;
using System.Collections.Generic;
using LofiHollow.EntityData;
using LofiHollow.DataTypes;
using Steamworks; 

namespace LofiHollow.Managers {
    public class CommandManager {
        public CommandManager() { }   
         
          
        public static void EquipItem(Player actor, int slot, Item item) {
            if (actor.Inventory.Count > slot && slot >= 0) { 
                if (item.EquipSlot >= 0 && item.EquipSlot <= 8) {
                    if (item.EquipSlot == 0) {
                        if (actor.EquippedLeftHand != null && actor.EquippedRightHand != null) {
                            Item oldEquip = actor.EquippedLeftHand;
                            actor.EquippedLeftHand = item;
                            actor.Inventory.RemoveAt(slot);
                            actor.Inventory.Add(oldEquip);
                        } else {
                            if (actor.EquippedLeftHand == null) {
                                actor.EquippedLeftHand = item;
                                actor.Inventory.RemoveAt(slot);
                            } else {
                                actor.EquippedRightHand = item;
                                actor.Inventory.RemoveAt(slot);
                            }
                        }
                    } 
                }
            }
        }  
    }
}
