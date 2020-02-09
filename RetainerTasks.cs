﻿using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.RemoteWindows;

namespace Retainers
{
    internal class RetainerTasks
    {

        public static bool IsOpen => SelectString.IsOpen;


        public static bool OpenInventory()
        {
            if (!IsOpen)
            {
                Logging.Write("Retainer task window not open");
                return false;
            }
            
            return SelectString.ClickLineEquals(RetainerTaskStrings.Inventory);
        }

        public static bool CloseInventory()
        {
            if (!IsInventoryOpen()) return true;

            if (RaptureAtkUnitManager.GetWindowByName("InventoryRetainer") != null)
            {
                RaptureAtkUnitManager.GetWindowByName("InventoryRetainer").SendAction(1, 3, (ulong) uint.MaxValue);
                return true;
            }

            if (RaptureAtkUnitManager.GetWindowByName("InventoryRetainerLarge") != null)
            {
                RaptureAtkUnitManager.GetWindowByName("InventoryRetainerLarge").SendAction(1, 3, (ulong) uint.MaxValue);
                return true;
            }

            return false;
        }

        public static bool CloseTasks()
        {
            if (!IsOpen) return true;

            return SelectString.ClickLineEquals(RetainerTaskStrings.Quit);
        }

        public static bool IsInventoryOpen()
        {
            return RaptureAtkUnitManager.GetWindowByName("InventoryRetainer") != null ||
                   RaptureAtkUnitManager.GetWindowByName("InventoryRetainerLarge") != null;
        }

        internal static class RetainerTaskStrings
        {
            //For partial string searches use SelectIconString.ClickLineContains(string) and not Equals
#if RB_CN
            internal static string Inventory = "道具管理";
            internal static string Gil = "金币管理";
            internal static string SellYourInventory = "出售（玩家所持物品）";
            internal static string SellRetainerInventory = "出售（雇员所持物品）";
            internal static string SaleHistory = "查看出售记录";
            internal static string ViewVentureReport = "查看雇员探险情况"; //Use Partial Search
            internal static string AssignVenture = "委托雇员进行探险"; //Use Partial Search since it adds (Complete) or (In Progress)
            internal static string ViewGear = "设置雇员装备";
            internal static string ResetClass = "设置雇员职业"; //Use Partial Search
            internal static string Quit = "让雇员返回";
#else
			internal static string Inventory = "Entrust or withdraw items.";
            internal static string Gil = "Entrust or withdraw gil.";
            internal static string SellYourInventory = "Sell items in your inventory on the market";
            internal static string SellRetainerInventory = "Sell items in your retainer's inventory on the market.";
            internal static string SaleHistory = "View sale history.";
            internal static string ViewVentureReport = "View venture report."; //Use Partial Search
            internal static string AssignVenture = "Assign venture."; //Use Partial Search since it adds (Complete) or (In Progress)
            internal static string ViewGear = "View retainer attributes and gear.";
            internal static string ResetClass = "Reset retainer class."; //Use Partial Search
            internal static string Quit = "Quit.";
#endif
        }
    }
}