using System;

namespace Server.Items
{
    public class RingOfIndifference : GoldRing
	{
        [Constructable]
        public RingOfIndifference()
        {
            this.LootType = LootType.Blessed;
            this.Name = "Ring of Indifference";
        }

        public RingOfIndifference(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}