using System;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using static ff14bot.RemoteWindows.Talk;

namespace Retainers
{
    internal class RetainerList
    {
        internal static string windowName = "RetainerList";

        public static bool IsOpen => RaptureAtkUnitManager.GetWindowByName(windowName) != null;

        public static async Task<bool> SelectRetainer(int index)
        {
            if (!IsOpen)
            {
                Logging.Write("Retainer selection window not open");
                return false;
            }

            Logging.Write("Selecting retainer: {0}", index);

            try
            {
                RaptureAtkUnitManager.GetWindowByName(windowName).SendAction(2, 3UL, 2UL, 3UL, (ulong) index);

                await Coroutine.Sleep(1000);

                await Coroutine.Wait(9000, () => DialogOpen);

                if (DialogOpen) Next();

                await Coroutine.Sleep(1000);

                if (SelectString.IsOpen)
                    return true;
            }
            catch (Exception ex)
            {
                Logging.Write("Error selecting retainer: {0}", ex);
            }

            return false;
        }

        public static void Close()
        {
            var windowByName = RaptureAtkUnitManager.GetWindowByName(windowName);
            if (windowByName == null)
                return;
            RaptureAtkUnitManager.GetWindowByName(windowName).SendAction(1, 3UL, (ulong) uint.MaxValue);
        }
    }
}