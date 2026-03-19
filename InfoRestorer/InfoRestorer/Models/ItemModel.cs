using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adam.InfoRestorer.Models
{
    public class ItemModel
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Page { get; set; }
        public byte Rot { get; set; }

        public Item Item { get; set; }

        public static ItemModel FromItemJar(ItemJar jar, byte page)
        {
            return new ItemModel()
            {
                X = jar.x,
                Y = jar.y,
                Rot = jar.rot,
                Page = page,
                Item = jar.item
            };
        }

        public static ItemModel FromClothing(ushort itemId, byte quality, byte[] state)
        {
            return new ItemModel()
            {
                Page = byte.MaxValue,
                Item = new Item(itemId, 1, quality, state)
            };
        }
    }
}
