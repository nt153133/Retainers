using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Service_Navigation;
using TreeSharp;
using static ff14bot.RemoteWindows.Talk;


namespace Retainers
{
    public class Retainers : BotBase
    {
        private static readonly string botName = "Retainers Test";

        private static bool done;

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


        /*The await sleeps shouldn't be necessary but if they aren't there the game crashes some times since
        it tries to send commands to a window that isn't open even though it reports it as open (guess it didn't load yet)*/
        private async Task<bool> RetainerTest()
        {
            if (!done)
            {
                Log(" ");
                Log("==================================================");
                Log("====================Retainers=====================");
                Log("==================================================");
                Log(" ");

                var retainerIndex = 0;
                var numRetainers = 2;
                //       bool test = false;


                for (retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
                {
                    var inventory = new RetainerInventory();

                    if (!RetainerList.IsOpen) await UseSummoningBell();

                    await Coroutine.Wait(5000, () => RetainerList.IsOpen);

                    await Coroutine.Sleep(1000);

                    if (!RetainerList.IsOpen) Log("Failed opening retainer list");

                    Log("Open:" + RetainerList.IsOpen);

                    await RetainerList.SelectRetainer(retainerIndex);

                    Log("Selected Retainer: " + retainerIndex);


                    await Coroutine.Wait(5000, () => RetainerTasks.IsOpen);

                    RetainerTasks.OpenInventory();

                    await Coroutine.Sleep(500);

                    if (RetainerTasks.IsInventoryOpen())
                    {
                        Log("Inventory open");
                        foreach (var retbag in InventoryManager.GetBagsByInventoryBagId(RetainerTasks
                            .RetainerBagIds))
                        foreach (var item in retbag.FilledSlots)
                            try
                            {
                                inventory.AddItem(item);
                                //Logging.Write("Name: {0} Count: {1} BagId: {2} IsHQ: {3}", item.Item.EnglishName, item.Item.StackSize, item.BagId, item.Item.IsHighQuality);
                            }
                            catch (Exception e)
                            {
                                Log("SHIT:" + e);
                                throw;
                            }

                        Log("Inventory done");

                        Log("Checking against player inventory");

                        foreach (var item in ff14bot.Managers.InventoryManager.FilledSlots.Where(x => x.BagId == InventoryBagId.Bag1 || x.BagId == InventoryBagId.Bag2 || x.BagId == InventoryBagId.Bag3 || x.BagId == InventoryBagId.Bag4))
                        {
                            if (inventory.HasItem(item.TrueItemId))
                                Log("BOTH PLAYER AND RETAINER HAVE Name: " + item.Item.EnglishName + "\tItemCategory: " + item.Item.EquipmentCatagory + "\tId: " + item.Item.Id);
                        }

                        Log("Done checking against player inventory");

                        RetainerTasks.CloseInventory();

                        await Coroutine.Wait(5000, () => RetainerTasks.IsOpen);

                        //await Coroutine.Sleep(1000);

                        //Call quit in tasks and get through dialog

                        RetainerTasks.CloseTasks();

                        await Coroutine.Sleep(500);

                        await Coroutine.Wait(9000, () => DialogOpen);

                        if (DialogOpen) Next();

                        await Coroutine.Sleep(200);

                        await Coroutine.Wait(5000, () => RetainerList.IsOpen);

                        Log("Should be back at retainer list by now");

                        inventory.PrintList();
                    }
                }

                //await Coroutine.Sleep(1000);

                Log("Closing Retainer List");

                RetainerList.Close();

                TreeRoot.Stop("Stop Requested");

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

            var bell = list[0];

            Logging.Write("Found nearest bell: {0} Distance: {1}", bell, bell.Distance2D(Core.Me.Location));

/* This only works with the Yield but the yield also throws exceptions (but still works) */
            if (bell.Distance2D(Core.Me.Location) >= 3)
            {
//                await CommonBehaviors.MoveAndStop(
//                    r => bell.Location, r => 2.5f, true,
//                    "Moving to the Bell")
//                    .ExecuteCoroutine();


                MoveSummoningBell(bell.Location);
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


        private async void MoveSummoningBell(Vector3 loc)
        {
            await CommonBehaviors.MoveAndStop(
                    r => loc, r => 2.5f, true,
                    "Moving to the Bell")
                .ExecuteCoroutine();

            await Coroutine.Yield();
        }
    }
}