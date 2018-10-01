using System;
using System.Collections;
using System.Collections.Generic;
using Server.Items;
using System.IO;

namespace Server.Commands
{
    class Location : IComparable
    {
        public int X;
        public int Y;
        public int Z;
        public Map Map;
        public Location(int x, int y, int z, Map m)
        {
            X = x;
            Y = y;
            Z = z;
            Map = m;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Location))
                return GetHashCode().CompareTo(obj.GetHashCode());

            Location l = (Location)obj;
            if (l.Map.MapID != Map.MapID)
                return l.Map.MapID - Map.MapID;
            if (l.X != X)
                return l.X - X;
            if (l.Y != Y)
                return l.Y - Y;
            return l.Z - Z;
        }

        public override int GetHashCode()
        {
            string hashString = String.Format("{0}-{1}-{2}-{3}",
                X, Y, Z, Map.MapID);
            return hashString.GetHashCode();
        }
    }
    class TelDef
    {
        public Location Source;
        public Location Destination;
        public bool Back;
        public TelDef(Location s, Location d, bool b)
        {
            Source = s;
            Destination = d;
            Back = b;
        }
    }
    public class GenTeleporter
    {
        private static string m_Path = Path.Combine("Data", "teleporters.csv");
        private static char[] m_Sep = { ',' };

        public GenTeleporter()
        {
        }

        public static void Initialize()
        {
            CommandSystem.Register("TelGen", AccessLevel.Administrator, new CommandEventHandler(GenTeleporter_OnCommand));
            CommandSystem.Register("TelGenDelete", AccessLevel.Administrator, new CommandEventHandler(TelGenDelete_OnCommand));
            CommandSystem.Register("DawnwatchTelGen", AccessLevel.Administrator, new CommandEventHandler(CreateDawnwatchTeleporters_OnCommand));
        }

        private static void TelGenDelete_OnCommand(CommandEventArgs e)
        {
            WeakEntityCollection.Delete("tel");
        }

        [Usage("TelGen")]
        [Description("Generates world/dungeon teleporters for all facets.")]
        public static void GenTeleporter_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Generating teleporters, please wait.");

            TeleportersCreator c = new TeleportersCreator();

            StreamReader reader = new StreamReader(m_Path);

            string line;
            int lineNum = 0;
            while((line = reader.ReadLine()) != null)
            {
                ++lineNum;
                line = line.Trim();
                if (line.StartsWith("#"))
                    continue;
                string[] parts = line.Split(m_Sep);
                if(parts.Length != 9)
                {
                    e.Mobile.SendMessage(33, String.Format("Bad teleporter definition on line {0}", lineNum));
                    continue;
                }
                try
                {
                    c.CreateTeleporter(
                        int.Parse(parts[0]),
                        int.Parse(parts[1]),
                        int.Parse(parts[2]),
                        int.Parse(parts[4]),
                        int.Parse(parts[5]),
                        int.Parse(parts[6]),
                        Map.Parse(parts[3]),
                        Map.Parse(parts[7]),
                        bool.Parse(parts[8])
                    );
                }
                catch (FormatException)
                {
                    e.Mobile.SendMessage(33, String.Format("Bad number format on line {0}", lineNum));
                }
                catch(ArgumentException ex)
                {
                    e.Mobile.SendMessage(33, String.Format("Argument Execption {0} on line {1}", ex.Message, lineNum));
                }
            }
            reader.Close();

            e.Mobile.SendMessage("Teleporter generating complete.");
        }

        [Usage("DawnwatchTelGen")]
        [Description("Generate all of the teleporters in Dawnwatch.")]
        public static void CreateDawnwatchTeleporters_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Generating teleporters, please wait.");

            TeleportersCreator c = new TeleportersCreator();
            c.CreateDawnwatchTeleporters();

            e.Mobile.SendMessage("Teleporter generating complete.");
        }

        public class TeleportersCreator
        {
            private static readonly Queue m_Queue = new Queue();
            private int m_Count;
            public TeleportersCreator()
            {
            }

            public static bool FindTeleporter(Map map, Point3D p)
            {
                IPooledEnumerable eable = map.GetItemsInRange(p, 0);

                foreach (Item item in eable)
                {
                    if (item is Teleporter && !(item is KeywordTeleporter) && !(item is SkillTeleporter))
                    {
                        int delta = item.Z - p.Z;

                        if (delta >= -12 && delta <= 12)
                            m_Queue.Enqueue(item);
                    }
                }

                eable.Free();

                while (m_Queue.Count > 0)
                    ((Item)m_Queue.Dequeue()).Delete();

                return false;
            }

            public void CreateTeleporter(Point3D pointLocation, Point3D pointDestination, Map mapLocation, Map mapDestination, bool back)
            {
                if (!FindTeleporter(mapLocation, pointLocation))
                {
                    this.m_Count++;
				
                    Teleporter tel = new Teleporter(pointDestination, mapDestination);
					WeakEntityCollection.Add("tel", tel);

                    tel.MoveToWorld(pointLocation, mapLocation);
                }

                if (back && !FindTeleporter(mapDestination, pointDestination))
                {
                    this.m_Count++;

                    Teleporter telBack = new Teleporter(pointLocation, mapLocation);
					WeakEntityCollection.Add("tel", telBack);

                    telBack.MoveToWorld(pointDestination, mapDestination);
                }
            }

            public void CreateTeleporter(int xLoc, int yLoc, int zLoc, int xDest, int yDest, int zDest, Map map, bool back)
            {
                this.CreateTeleporter(new Point3D(xLoc, yLoc, zLoc), new Point3D(xDest, yDest, zDest), map, map, back);
            }

            public void CreateTeleporter(int xLoc, int yLoc, int zLoc, int xDest, int yDest, int zDest, Map mapLocation, Map mapDestination, bool back)
            {
                this.CreateTeleporter(new Point3D(xLoc, yLoc, zLoc), new Point3D(xDest, yDest, zDest), mapLocation, mapDestination, back);
            }

            public void CreateDawnwatchTeleporters()
			{
                Map map = Map.Felucca;
				// Pyramid
				CreateTeleporter( 5952, 87, 17, 3618, 453, 0, map, true );
				CreateTeleporter( 5953, 87, 17, 3619, 453, 0, map, true );
				CreateTeleporter( 514, 1559, 0, 5396, 127, 0, map, true );
				CreateTeleporter( 5878, 139, -13, 5983, 445, 7, map, true );
				CreateTeleporter( 5878, 140, -13, 5983, 446, 7, map, true );
				CreateTeleporter( 5984, 445, 12, 5879, 139, -8, map, true );
				CreateTeleporter( 5984, 446, 12, 5879, 140, -8, map, true );
				CreateTeleporter( 3618, 452, 5952, 86, 12, 0, map, true );
				CreateTeleporter( 3619, 452, 5953, 86, 12, 0, map, true );
				CreateTeleporter( 1156, 472, -13, 5328, 573, 7, map, true );
				CreateTeleporter( 1156, 473, -13, 5328, 574, 7, map, true );
				CreateTeleporter( 5243, 518, -13, 5338, 706, 7, map, true );
				CreateTeleporter( 5243, 519, -13, 5338, 707, 7, map, true );
				CreateTeleporter( 5329, 573, 12, 1157, 472, -8, map, true );
				CreateTeleporter( 5329, 574, 12, 1157, 473, -8, map, true );
				CreateTeleporter( 5339, 706, 12, 5244, 518, -8, map, true );
				CreateTeleporter( 5339, 707, 12, 5244, 519, -8, map, true );
				CreateTeleporter( 5376, 748, -13, 5359, 908, 7, map, true );
				CreateTeleporter( 5360, 908, 12, 5377, 748, -8, map, true );
				CreateTeleporter( 5360, 909, 12, 5377, 749, -8, map, true );
				CreateTeleporter( 5376, 749, -13, 5359, 909, 7, map, true );
				
				// Mage Keep Portal
				CreateTeleporter( 1826, 760, 0, 1824, 760, 20, map, true );
				
				// Dardin
				CreateTeleporter( 5697, 53, 17, 3005, 442, 0, map, true );
				CreateTeleporter( 5698, 53, 17, 3006, 442, 0, map, true );
				CreateTeleporter( 5646, 148, -13, 5779, 390, 7, map, true );
				CreateTeleporter( 5646, 149, -13, 5779, 391, 7, map, true );
				CreateTeleporter( 3005, 441, 5697, 52, 12, 0, map, true );
				CreateTeleporter( 3006, 441, 5698, 52, 12, 0, map, true );
				CreateTeleporter( 5780, 390, 12, 5647, 148, -8, map, true );
				CreateTeleporter( 5780, 391, 12, 5647, 149, -8, map, true );
				
				// Doom
				CreateTeleporter( 1622, 2562, 5324, 72, 7, 0, map, true );
				CreateTeleporter( 1622, 2561, 5324, 71, 7, 0, map, true );
				CreateTeleporter( 5398, 299, 12, 5212, 85, -8, map, true );
				CreateTeleporter( 5398, 298, 12, 5212, 84, -8, map, true );
				CreateTeleporter( 5325, 72, 12, 1623, 2562, 0, map, true );
				CreateTeleporter( 5325, 71, 12, 1623, 2561, 0, map, true );
				CreateTeleporter( 5211, 85, -13, 5397, 299, 7, map, true );
				CreateTeleporter( 5211, 84, -13, 5397, 298, 7, map, true );
				
				// Unknown cave
				CreateTeleporter( 1318, 3602, 45, 2994, 3696, 0, map, true );
				CreateTeleporter( 1318, 3603, 45, 2994, 3697, 0, map, true );
				CreateTeleporter( 2995, 3696, 0, 1319, 3602, 45, map, true );
				CreateTeleporter( 2995, 3697, 0, 1319, 3603, 45, map, true );

				// Clues
				CreateTeleporter( 5556, 2183, 12, 5406, 2340, -8, map, true );
				CreateTeleporter( 5557, 2183, 12, 5407, 2340, -8, map, true );
				CreateTeleporter( 5989, 2187, 12, 5704, 2173, -8, map, true );
				CreateTeleporter( 5990, 2187, 12, 5705, 2173, -8, map, true );
				CreateTeleporter( 3759, 2034, 0, 5313, 2277, 0, map, true );
				CreateTeleporter( 3760, 2034, 0, 5314, 2277, 0, map, true );
				CreateTeleporter( 5704, 2172, -13, 5989, 2186, 7, map, true );
				CreateTeleporter( 5705, 2172, -13, 5990, 2186, 7, map, true );
				CreateTeleporter( 5313, 2278, 0, 3759, 2035, 0, map, true );
				CreateTeleporter( 5314, 2278, 0, 3760, 2035, 0, map, true );
				CreateTeleporter( 5406, 2339, -13, 5556, 2182, 7, map, true );
				CreateTeleporter( 5407, 2339, -13, 5557, 2182, 7, map, true );

				// Time
				CreateTeleporter( 5630, 602, 17, 3831, 1488, 0, map, true ); 
				CreateTeleporter( 5631, 602, 17, 3832, 1488, 0, map, true );
				CreateTeleporter( 5573, 690, -13, 5560, 923, 7, map, true ); 
				CreateTeleporter( 5574, 690, -13, 5561, 923, 7, map, true );
				CreateTeleporter( 5560, 924, 12, 5573, 691, -8, map, true );
				CreateTeleporter( 5561, 924, 12, 5574, 691, -8, map, true );
				CreateTeleporter( 3831, 1487, 0, 5630, 601, 12, map, true );
				CreateTeleporter( 3832, 1487, 0, 5631, 601, 12, map, true );

				// King Crypt

				CreateTeleporter( 3063, 958, -22, 5213, 1842, 12, map, true );
				CreateTeleporter( 3064, 958, -22, 5214, 1842, 12, map, true );
				CreateTeleporter( 5213, 1843, 12, 3063, 959, -20, map, true );
				CreateTeleporter( 5214, 1843, 12, 3064, 959, -20, map, true );

				// TP Caves
				CreateTeleporter( 4180, 267, 0, 5180, 1760, 0, map, true );
				CreateTeleporter( 4181, 267, 0, 5181, 1760, 0, map, true );
				CreateTeleporter( 2508, 936, 0, 5702, 2370, 0, map, true );
				CreateTeleporter( 2509, 936, 0, 5703, 2370, 0, map, true );
				CreateTeleporter( 1000, 573, 0, 5672, 2490, 0, map, true );
				CreateTeleporter( 1001, 573, 0, 5673, 2490, 0, map, true );
				CreateTeleporter( 5702, 2371, 0, 2508, 937, 0, map, true );
				CreateTeleporter( 5703, 2371, 0, 2509, 937, 0, map, true );
				CreateTeleporter( 5672, 2491, 0, 1000, 574, 0, map, true );
				CreateTeleporter( 5673, 2491, 0, 1001, 574, 0, map, true ); 
				CreateTeleporter( 5329, 2512, 0, 2568, 2622, 0, map, true ); 
				CreateTeleporter( 5330, 2512, 0, 2569, 2622, 0, map, true ); 
				CreateTeleporter( 5424, 2515, 0, 2611, 2623, 0, map, true ); 
				CreateTeleporter( 5425, 2515, 0, 2612, 2623, 0, map, true ); 
				CreateTeleporter( 2568, 2621, 0, 5329, 2511, 0, map, true );
				CreateTeleporter( 2569, 2621, 0, 5330, 2511, 0, map, true ); 
				CreateTeleporter( 2611, 2622, 0, 5424, 2514, 0, map, true ); 
				CreateTeleporter( 2612, 2622, 0, 5425, 2514, 0, map, true );
				CreateTeleporter( 1889, 1453, 2, 5351, 1711, 0, map, true );
				CreateTeleporter( 1890, 1453, 2, 5352, 1711, 0, map, true ); 
				CreateTeleporter( 1555, 1405, 2, 5247, 1572, 0, map, true ); 
				CreateTeleporter( 1555, 1406, 2, 5247, 1573, 0 , map, true );
				CreateTeleporter( 3231, 1581, 0, 3204, 3692, 0, map, true ); 
				CreateTeleporter( 3232, 1581, 0, 3205, 3692, 0, map, true ); 
				CreateTeleporter( 5248, 1572, 0, 1556, 1405, 2, map, true ); 
				CreateTeleporter( 5248, 1573, 0, 1556, 1406, 2, map, true ); 
				CreateTeleporter( 3272, 1693, 0, 3307, 3818, 0, map, true ); 
				CreateTeleporter( 3273, 1693, 0, 3308, 3818, 0, map, true ); 
				CreateTeleporter( 5351, 1712, 0, 1889, 1454, 2, map, true ); 
				CreateTeleporter( 5352, 1712, 0, 1890, 1454, 2, map, true ); 
				CreateTeleporter( 5461, 1704, 0, 1841, 2209, 0, map, true ); 
				CreateTeleporter( 5462, 1704, 0, 1842, 2209, 0, map, true ); 
				CreateTeleporter( 5180, 1761, 0, 4180, 268, 0, map, true ); 
				CreateTeleporter( 5181, 1761, 0, 4181, 268, 0, map, true );
				CreateTeleporter( 1841, 2208, 0, 5461, 1703, 0, map, true ); 
				CreateTeleporter( 1842, 2208, 0, 5462, 1703, 0, map, true );
				CreateTeleporter( 3204, 3693, 0, 3231, 1582, 0, map, true ); 
				CreateTeleporter( 3205, 3693, 0, 3232, 1582, 0, map, true ); 
				CreateTeleporter( 3307, 3819, 0, 3272, 1694, 0, map, true ); 
				CreateTeleporter( 3308, 3819, 0, 3273, 1694, 0, map, true );

				// IronBlood
				CreateTeleporter( 4702, 1206, 5, 5430, 2889, 25, map, true );
				CreateTeleporter( 5430, 2890, 25, 4702, 1207, 5, map, true );
				CreateTeleporter( 4703, 1206, 5, 5431, 2889, 25, map, true );
				CreateTeleporter( 5431, 2890, 25, 4703, 1207, 5, map, true );

				// Underhill
				CreateTeleporter( 4459, 1263, 5, 4307, 3483, 25, map, true );
				CreateTeleporter( 4307, 3484, 25, 4459, 1264, 5, map, true );
				CreateTeleporter( 4460, 1263, 5, 4308, 3483, 25, map, true );
				CreateTeleporter( 4308, 3484, 25, 4460, 1264, 5, map, true );

				// Perin Depths
				CreateTeleporter( 4738, 1323, 5, 4309, 3827, 12, map, true );
				CreateTeleporter( 4310, 3827, 17, 4739, 1323, 5, map, true );
				CreateTeleporter( 4738, 1324, 5, 4309, 3828, 12, map, true );
				CreateTeleporter( 4310, 3828, 17, 4739, 1324, 5, map, true );
				CreateTeleporter( 4808, 1273, 5, 4401, 3684, 12, map, true );
				CreateTeleporter( 4401, 3685, 17, 4808, 1274, 5, map, true );
				CreateTeleporter( 4809, 1273, 5, 4402, 3684, 12, map, true );
				CreateTeleporter( 4402, 3685, 17, 4809, 1274, 5, map, true );

				// Isle Cave
				CreateTeleporter( 4789, 1290, 5, 3874, 3764, 0, map, true );
				CreateTeleporter( 3875, 3764, 0, 4790, 1290, 5, map, true );
				CreateTeleporter( 4789, 1291, 5, 3874, 3765, 0, map, true );
				CreateTeleporter( 3875, 3765, 0, 4790, 1291, 5, map, true );
				CreateTeleporter( 4876, 1234, 5, 3988, 3768, 0, map, true );
				CreateTeleporter( 3988, 3769, 0, 4876, 1235, 5, map, true );
				CreateTeleporter( 4877, 1234, 5, 3989, 3768, 0, map, true );
				CreateTeleporter( 3989, 3769, 0, 4877, 1235, 5, map, true );
				CreateTeleporter( 5004, 1262, 5, 3690, 3823, 0, map, true );
				CreateTeleporter( 3690, 3824, 0, 5004, 1263, 5, map, true );
				CreateTeleporter( 5005, 1262, 5, 3691, 3823, 0, map, true );
				CreateTeleporter( 3691, 3824, 0, 5005, 1263, 5, map, true );
				CreateTeleporter( 5049, 1217, 30, 3721, 3709, 0, map, true );
				CreateTeleporter( 3721, 3710, 0, 5049, 1218, 30, map, true );
				CreateTeleporter( 5050, 1217, 30, 3722, 3709, 0, map, true );
				CreateTeleporter( 3722, 3710, 0, 5050, 1218, 30, map, true );
				CreateTeleporter( 5064, 1204, 5, 3900, 3694, 0, map, true );
				CreateTeleporter( 3901, 3694, 0, 5065, 1204, 5, map, true );
				CreateTeleporter( 5064, 1205, 5, 3900, 3695, 0, map, true );
				CreateTeleporter( 3901, 3695, 0, 5065, 1205, 5, map, true );
				CreateTeleporter( 4634, 1219, 5, 4012, 3710, 0, map, true );
				CreateTeleporter( 4012, 3711, 0, 4634, 1220, 5, map, true );
				CreateTeleporter( 4635, 1219, 5, 4013, 3710, 0, map, true );
				CreateTeleporter( 4013, 3711, 0, 4635, 1220, 5, map, true );

				// Town Load British
				CreateTeleporter( 3068, 1030, -21, 3068, 1030, 0, map, true );

				// Ed Portal
				CreateTeleporter( 5931, 569, 0, 5880, 665, 0, map, true );
				CreateTeleporter( 5881, 665, 0, 5931, 568, 0, map, true );

				// Catacombs
				CreateTeleporter( 4036, 3343, 37, 1524, 3598, 0, map, true );
				CreateTeleporter( 4036, 3344, 37, 1524, 3599, 0, map, true );
				CreateTeleporter( 3913, 3482, 37, 1382, 3641, 42, map, true );
				CreateTeleporter( 1383, 3641, 31, 3914, 3482, 26, map, true );
				CreateTeleporter( 1523, 3598, 0, 4035, 3343, 32, map, true );
				CreateTeleporter( 1523, 3599, 0, 4035, 3344, 32, map, true );

				// Underhill
				CreateTeleporter( 4601, 1229, 5, 4449, 3449, 25, map, true );
				CreateTeleporter( 4450, 3449, 25, 4602, 1229, 5, map, true );
				CreateTeleporter( 4601, 1230, 5, 4449, 3450, 25, map, true );
				CreateTeleporter( 4450, 3450, 25, 4602, 1230, 5, map, true );

				// Ratman Lair 
				CreateTeleporter( 4456, 1218, 5, 4470, 3284, 25, map, true );
				CreateTeleporter( 4471, 3284, 25, 4457, 1218, 5, map, true );
				CreateTeleporter( 4456, 1219, 5, 4470, 3285, 25, map, true );
				CreateTeleporter( 4471, 3285, 25, 4457, 1219, 5, map, true );
				CreateTeleporter( 4445, 3298, -13, 4324, 3297, 12, map, true );
				CreateTeleporter( 4325, 3297, 17, 4446, 3298, -8, map, true );
				CreateTeleporter( 4445, 3299, -13, 4324, 3298, 12, map, true );
				CreateTeleporter( 4325, 3298, 17, 4446, 3299, -8, map, true );

				// Sephiroth
				CreateTeleporter( 4628, 1348, 5, 5186, 2958, 25, map, true );
				CreateTeleporter( 5186, 2959, 25, 4628, 1349, 5, map, true );
				CreateTeleporter( 4629, 1348, 5, 5187, 2958, 25, map, true );
				CreateTeleporter( 5187, 2959, 25, 4629, 1349, 5, map, true );
				
				// Exodus Dungeon
				CreateTeleporter( 876, 2650, -13, 5931, 590, 7, map, true );
				CreateTeleporter( 876, 2651, -13, 5931, 591, 7, map, true );
				CreateTeleporter( 5932, 590, 12, 877, 2650, -8, map, true );
				CreateTeleporter( 5932, 591, 12, 877, 2651, -8, map, true );
				
				// Ice Caves
				CreateTeleporter( 5539, 2798, 0, 4396, 1262, 5, map, true );
				CreateTeleporter( 4396, 1261, 5, 5539, 2797, 0, map, true );
				CreateTeleporter( 5540, 2798, 0, 4397, 1262, 5, map, true );
				CreateTeleporter( 4397, 1261, 5, 5540, 2797, 0, map, true );
				CreateTeleporter( 5576, 2779, 0, 4433, 1243, 5, map, true );
				CreateTeleporter( 4433, 1242, 5, 5576, 2778, 0, map, true );
				CreateTeleporter( 5577, 2779, 0, 4434, 1243, 5, map, true );
				CreateTeleporter( 4434, 1242, 5, 5577, 2778, 0, map, true );
				CreateTeleporter( 5568, 2738, 0, 4425, 1202, 5, map, true );
				CreateTeleporter( 4424, 1202, 5, 5567, 2738, 0, map, true );
				CreateTeleporter( 5568, 2739, 0, 4425, 1203, 5, map, true );
				CreateTeleporter( 4424, 1203, 5, 5567, 2739, 0, map, true );
				
				// Fires of Hell
				CreateTeleporter( 5242, 1102, -13, 5362, 1348, 7, map, true );
				CreateTeleporter( 5242, 1103, -13, 5362, 1349, 7, map, true );
				CreateTeleporter( 5332, 1293, 0, 3344, 1643, 0, map, true );
				CreateTeleporter( 5333, 1293, 0, 3345, 1643, 0, map, true );
				CreateTeleporter( 5606, 1317, 12, 5361, 1397, -8, map, true );
				CreateTeleporter( 5607, 1317, 12, 5362, 1397, -8, map, true );
				CreateTeleporter( 5363, 1348, 12, 5243, 1102, -8, map, true );
				CreateTeleporter( 5363, 1349, 12, 5243, 1103, -8, map, true );
				CreateTeleporter( 5361, 1396, -13, 5606, 1316, 7, map, true );
				CreateTeleporter( 5362, 1396, -13, 5607, 1316, 7, map, true );
				CreateTeleporter( 3344, 1642, 0, 5332, 1292, 0, map, true );
				CreateTeleporter( 3345, 1642, 0, 5333, 1292, 0, map, true );
				
				// Mines of Morinia
				CreateTeleporter( 5832, 944, -13, 5916, 1316, 7, map, true );
				CreateTeleporter( 5832, 945, -13, 5916, 1317, 7, map, true );
				CreateTeleporter( 5897, 1226, 0, 1021, 1366, 2, map, true );
				CreateTeleporter( 5898, 1226, 0, 1022, 1366, 2, map, true );
				CreateTeleporter( 5917, 1316, 12, 5833, 944, -8, map, true );
				CreateTeleporter( 5917, 1317, 12, 5833, 945, -8, map, true );
				CreateTeleporter( 1021, 1365, 2, 5897, 1225, 0, map, true );
				CreateTeleporter( 1022, 1365, 2, 5898, 1225, 0, map, true );
				CreateTeleporter( 5702, 1472, 12, 5882, 1608, -7, map, true );
				CreateTeleporter( 5702, 1473, 12, 5882, 1609, -7, map, true );
				CreateTeleporter( 5881, 1608, -12, 5701, 1472, 7, map, true );
				CreateTeleporter( 5881, 1609, -12, 5701, 1473, 7, map, true );
				
			}
        }
    }
}