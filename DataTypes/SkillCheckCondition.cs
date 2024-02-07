using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LofiHollow.DataTypes {
    public class SkillCheckCondition {
        public string SkillCheckID = "";
        public string Skill = "";
        public int TargetDC = 0;
        public int HoursToRetry = 0; // Hours until it can be retried. If set to -1, can't be retried if failed
        public bool FailMovesPlayer = false;
        public string FailMovesToID = "";
    }
}
