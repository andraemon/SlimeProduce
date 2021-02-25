using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeProduce
{
    class DropTable
    {
        public ColorRange colorRange { get; set; }
        public List<ItemDrop> itemDrops { get; set; }
        public DropTable(ColorRange c, List<ItemDrop> d)
        {
            colorRange = c;
            itemDrops = d;
        }
    }
}
