using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.RemoteWindows;

namespace Retainers
{
    internal class RetainerTasks
    {
        public static InventoryBagId[] RetainerBagIds =
        {
            InventoryBagId.Retainer_Page1, InventoryBagId.Retainer_Page2, InventoryBagId.Retainer_Page3,
            InventoryBagId.Retainer_Page4, InventoryBagId.Retainer_Page5, InventoryBagId.Retainer_Page6,
            InventoryBagId.Retainer_Page7
        };

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
            internal static string Inventory = "Entrust or withdraw items.";
            internal static string Gil = "Entrust or withdraw gil.";
            internal static string SellYourInventory = "Sell items in your inventory on the market";
            internal static string SellRetainerInventory = "Sell items in your retainer's inventory on the market.";
            internal static string SaleHistory = "View sale history.";
            internal static string ViewVentureReport = "View venture report."; //Use Partial Search
            internal static string AssignVenture = "Assign venture."; //Use Partial Search
            internal static string ViewGear = "View retainer attributes and gear.";
            internal static string ResetClass = "Reset retainer class."; //Use Partial Search
            internal static string Quit = "Quit.";
        }
    }
}