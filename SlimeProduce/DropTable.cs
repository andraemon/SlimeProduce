using System.Collections.Generic;

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
