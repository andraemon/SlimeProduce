using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeProduce
{
    class ItemDrop
    {
        public int parentSheetIndex { get; set; }
        public int minDrop { get; set; }
        public int maxDrop { get; set; }
        public double dropChance { get; set; }

        public ItemDrop(int index, int min, int max, double chance = 1.0)
        {
            parentSheetIndex = index;
            maxDrop = max;
            minDrop = min;
            dropChance = chance;
        }
    }
}
