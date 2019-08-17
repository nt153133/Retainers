using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteWindows;

namespace Retainers
{
    public static class HelperFunctions
    {
        public static readonly InventoryBagId[] PlayerInventoryBagIds = new InventoryBagId[6]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4,
            InventoryBagId.Bag5,
            InventoryBagId.Bag6
        };


        public static readonly InventoryBagId[] RetainerBagIds =
        {
            InventoryBagId.Retainer_Page1, InventoryBagId.Retainer_Page2, InventoryBagId.Retainer_Page3,
            InventoryBagId.Retainer_Page4, InventoryBagId.Retainer_Page5, InventoryBagId.Retainer_Page6,
            InventoryBagId.Retainer_Page7
        };

        public static readonly InventoryBagId RetainerGilId = InventoryBagId.Retainer_Gil;

        public static readonly InventoryBagId PlayerGilId = InventoryBagId.Currency;

        public static readonly int GilItemId = 1;


        public static bool FilterStackable(BagSlot item)
        {
            if (item.IsCollectable)
                return false;

            if (item.Item.StackSize < 2)
                return false;

            if (item.Count == item.Item.StackSize)
                return false;

            return true;
        }

        public static uint NormalRawId(uint trueItemId)
        {
            if (trueItemId > 1000000U)
                return trueItemId - 1000000U;

            return trueItemId;
        }

        public static bool MoveItem(BagSlot fromBagSlot, BagSlot toBagSlot)
        {
            if (fromBagSlot.Count + toBagSlot.Count > toBagSlot.Item.StackSize)
                return false;

            return fromBagSlot.Move(toBagSlot);
        }

        public static int GetNumberOfRetainers()
        {
            string bell = GetBellLuaString();
            var numOfRetainers = 0;

            if (bell.Length > 0)
                numOfRetainers =
                    Lua.GetReturnVal<int>(string.Format("return _G['{0}']:GetRetainerEmployedCount();", bell));

            return numOfRetainers;
        }

        public static string GetRetainerName()
        {
            if (GetBellLuaString().Length > 0 && CanGetName())
                return Lua.GetReturnVal<string>($"return _G['{GetBellLuaString()}']:GetRetainerName();");

            return "";
        }

        public static string GetBellLuaString()
        {
            string func = "local values = '' for key,value in pairs(_G) do if string.match(key, '{0}:') then return key;   end end return values;";
            string searchString = "CmnDefRetainerBell";
            string bell = Lua.GetReturnVal<string>(string.Format(func, searchString)).Trim();

            return bell;
        }

        private static bool CanGetName()
        {
            return (RaptureAtkUnitManager.GetWindowByName("InventoryRetainer") != null ||
                    RaptureAtkUnitManager.GetWindowByName("InventoryRetainerLarge") != null || SelectString.IsOpen);
        }

        public static GameObject NearestSummoningBell()
        {
            List<GameObject> list = GameObjectManager.GameObjects
                .Where(r => r.Name == "Summoning Bell")
                .OrderBy(j => j.Distance())
                .ToList();

            if (list.Count <= 0)
            {
                LogCritical("No Summoning Bell Found");
                return null;
            }

            GameObject bell = list[0];

            LogCritical("Found nearest bell: {0} Distance: {1}", bell, bell.Distance2D(Core.Me.Location));

            return bell;
        }

        private static void LogCritical(string text, params object[] args)
        {
            var msg = string.Format("[Helpers] " + text, args);
            Logging.Write(Colors.OrangeRed, msg);
        }

        public static IEnumerable<BagSlot> GetPlayerStackableBagSlots()
        {
            return InventoryManager.FilledSlots.Where(x => x.BagId == InventoryBagId.Bag1 || x.BagId == InventoryBagId.Bag2 || x.BagId == InventoryBagId.Bag3 || x.BagId == InventoryBagId.Bag4).Where(FilterStackable);
        }
    }
}