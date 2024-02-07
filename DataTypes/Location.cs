using System.Collections.Generic;

namespace LofiHollow.DataTypes {
    public class Location { 
        public string ID;
        public string DisplayName;
        public string Description;
        public List<DescriptionBlock> DescriptionBlocks;
        public List<ConnectionNode> ConnectedLocations;
        public List<NodeItem> ItemsAtNode;
        public List<Item> ItemsOnGround;
        public List<string> ObjectsHere;
        public bool IsLit = true;
        public bool InDevLottery = true;
         

        public Location(string i, string dn, string desc) {   
            ID = i;
            DisplayName = dn;
            Description = desc; 

            ConnectedLocations = new();
            ItemsAtNode = new();
            ItemsOnGround = new();
            DescriptionBlocks = new();
            ObjectsHere = new();
        } 
    }
}
