using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueCrystalCooking
{
    public class DrugeffectTimeObject
    {
        public long time = BlueCrystalCookingPlugin.getCurrentTime();
        public string playerId;
        public DrugeffectTimeObject(string playerId)
        {
            this.playerId = playerId;
        }
    }
}
