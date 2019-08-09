using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using Clio.Utilities;
using Clio.Utilities.Helpers;
using ff14bot;
using TreeSharp;
using ff14bot.Objects;
using ff14bot.Pathing;
using ff14bot.RemoteWindows;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using static ff14bot.RemoteWindows.Talk;


namespace Retainers
{
    public class Retainers : BotBase
    {
        private static readonly string botName = "Retainers Test";

        private Composite _root;


        public override string Name
        {
            get
            {
#if RB_CN
                return "栽培模式";
#else
                return "Retainers Test";
#endif
            }
        }

        public override bool WantButton => true;

        public override string EnglishName => "Retainers Test";

        public override PulseFlags PulseFlags => PulseFlags.All;

        public override bool RequiresProfile => false;

        public override Composite Root => _root;

        private static bool done = false;

        public override void OnButtonPress()
        {
            //forceWater = true;
        }

        private static void Log(string text, params object[] args)
        {
            var msg = string.Format("[" + botName + "] " + text, args);
            Logging.Write(Colors.Green, msg);
        }

        private static void LogVerbose(string text, params object[] args)
        {
            var msg = string.Format("[" + botName + "] " + text, args);
            Logging.WriteVerbose(msg);
        }

        public override void Start()
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            _root = new ActionRunCoroutine(r => RetainerTest());
            done = false;
        }


        //The await sleeps shouldn't be necessary but if they aren't there the game crashes some times since
        //it tries to send commands to a window that isn't open even though it reports it as open (guess it didn't load yet)
        private async Task<bool> RetainerTest()
        {
 
        if (!done)
        { 

                Log(" ");
                Log("==================================================");
                Log("====================Retainers=====================");
                Log("==================================================");
                Log(" ");

                int retainerIndex = 0;
                int numRetainers = 2;
                //       bool test = false;


                for (retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
                {
                    if (!RetainerList.IsOpen)
                    {
                        await UseSummoningBell();
                    }

                    await Coroutine.Wait(5000, () => RetainerList.IsOpen);

                    await Coroutine.Sleep(1000);

                    if (!RetainerList.IsOpen)
                    {
                        Log("Failed opening retainer list");
                    }

                    Log("Open:" + RetainerList.IsOpen);

                    await RetainerList.SelectRetainer(retainerIndex);

                    Log("Selected Retainer: " + retainerIndex);


                    await Coroutine.Wait(5000, () => RetainerTasks.IsOpen);

                    RetainerTasks.OpenInventory();

                    await Coroutine.Sleep(1000);

                    if (RetainerTasks.IsInventoryOpen())
                    {
                        Log("Inventory open");
                        foreach (Bag retbag in ff14bot.Managers.InventoryManager.GetBagsByInventoryBagId(RetainerTasks
                            .RetainerBagIds))
                        {
                            foreach (var item in retbag.FilledSlots)
                            {
                                try
                                {
                                    Log("Name: " + item.Item.EnglishName + "\tCount: " + item.Item.ItemCount() +
                                        "\tBagID: " + item.BagId);
                                }
                                catch (Exception e)
                                {
                                    Log("SHIT:" + e);
                                    throw;
                                }
                            }
                        }

                        Log("Inventory done");

                        RetainerTasks.CloseInventory();

                        await Coroutine.Sleep(2000);

                        //Call quit in tasks and get through dialog

                        RetainerTasks.CloseTasks();

                        await Coroutine.Sleep(1000);

                        await Coroutine.Wait(9000, () => DialogOpen);

                        if (DialogOpen)
                        {
                            Next();
                        }

                        await Coroutine.Sleep(5000);

                        Log("Should be back at retainer list by now");
                    }

                }

                RetainerList.Close();

                done = true;

        }

        return true;

        }

        private async Task<bool> UseSummoningBell()
        {

            List<GameObject> list;
            list = GameObjectManager.GameObjects
                            .Where(r => r.Name == "Summoning Bell")
                            .OrderBy(j => j.Distance())
                            .ToList();

            if (list.Count <= 0)
            {
                Log("No Summoning Bell Found");
                return false;
            }

            GameObject bell = list[0];

            Logging.Write("Found nearest bell: {0} Distance: {1}", bell, bell.Distance2D(Core.Me.Location));

            if (bell.Distance2D(Core.Me.Location) >= 3)
            {
                await CommonBehaviors.MoveAndStop(
                    r => bell.Location, r => 2.5f, true,
                    "Following selected target")
                    .ExecuteCoroutine();

                try
                {
                    Coroutine.Yield();
                }
                catch (Exception)
                {

                   
                }
                //await CommonTasks.MoveAndStop(new MoveToParameters(bell.Location, "Summoning Bell"), 2.5f, true);

            }

            if (bell.Distance2D(Core.Me.Location) <= 3)
            {
                bell.Target();
                bell.Interact();
                await Coroutine.Sleep(1000);

                await Coroutine.Wait(5000, () => RetainerList.IsOpen);
                Logging.Write("Summoning Bell Used");

            }

            return false;
        }

}
}