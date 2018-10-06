using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Services.Virtues;

namespace Server.Gumps
{
    public class PermadeathGump : Gump
    {
        public PermadeathGump() : base(100, 0)
        {
            AddPage(0);

            AddBackground(0, 0, 400, 350, 2600);

            AddHtml(50, 20, 400, 35, "You Are Dying", false, false);

            AddHtml(50, 55, 300, 190, "Death has rolled his dice, and now he has set his sights on you. " +
					"A crushing blow from which you cannot recover, a thrust which unfortunately struck true. You are mortally " +
					"wounded, and your journey is coming to an end. Upon resurrection, you will have a short time to utter your last words. " +
					"Use that time wisely, for it is more than some are given...", true, true);

            AddButton(200, 277, 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddHtml(235, 277, 110, 35, "I accept my fate", false, false);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;
            from.CloseGump(typeof(PermadeathGump));
        }
    }
}