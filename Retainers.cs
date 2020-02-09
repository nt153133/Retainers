﻿using System;
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
using ff14bot.Pathing;
using ff14bot.Pathing.Service_Navigation;
using TreeSharp;
using static ff14bot.RemoteWindows.Talk;
using static Retainers.HelperFunctions;


namespace Retainers
{
    public class Retainers : BotBase
    {
        private static readonly string botName = "Retainers Test";

        private static bool done;

        private static readonly InventoryBagId[] inventoryBagId_0 = new InventoryBagId[6]
        {
            InventoryBagId.Bag1,
            InventoryBagId.Bag2,
            InventoryBagId.Bag3,
            InventoryBagId.Bag4,
            InventoryBagId.Bag5,
            InventoryBagId.Bag6
        };

        private Composite _root;

        private bool debug;

        private SettingsForm settings;

        public override string Name
        {
            get
            {
#if RB_CN
                return "雇员整理";
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
            if (settings == null || settings.IsDisposed)
                settings = new SettingsForm();
            try
            {
                settings.Show();
                settings.Activate();
            }
            catch (ArgumentOutOfRangeException ee)
            {
            }
        }

        private void Log(string text, params object[] args)
        {
            var msg = string.Format("[" + botName + "] " + text, args);
            Logging.Write(Colors.Green, msg);
        }

        private void LogVerbose(string text, params object[] args)
        {
            if (!debug)
                return;
            var msg = string.Format("[" + botName + "] " + text, args);
            Logging.WriteVerbose(msg);
        }

        private void LogCritical(string text, params object[] args)
        {
            var msg = string.Format("[" + botName + "] " + text, args);
            Logging.Write(Colors.OrangeRed, msg);
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

                //var retainerIndex = 0;

                //Settings variables
                debug = RetainerSettings.Instance.DebugLogging;

                await UseSummoningBell();
                await Coroutine.Wait(5000, () => RetainerList.IsOpen().Result);
                await Coroutine.Sleep(1000);

                int numRetainers = GetNumberOfRetainers();

                List<RetainerInventory> retList = new List<RetainerInventory>();
                List<KeyValuePair<uint, int>> moveToOrder = new List<KeyValuePair<uint, int>>();
                Dictionary<uint, List<KeyValuePair<int, uint>>> masterInventory = new Dictionary<uint, List<KeyValuePair<int, uint>>>();

                Dictionary<int,string> retainerNames = new Dictionary<int, string>();

                if (numRetainers <= 0)
                {
                    LogCritical("Can't find number of retainers either you have none or not near a bell");
                    RetainerList.Close();


                    TreeRoot.Stop("Failed: Find a bell or some retainers");
                    return true;
                }

                //Moves
                var moveFrom = new List<uint>[numRetainers];

                for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
                    moveFrom[retainerIndex] = new List<uint>();


                for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
                {
                    var inventory = new RetainerInventory();

                    if (! await RetainerList.IsOpen())
                    {
                        await UseSummoningBell();
                        await Coroutine.Wait(5000, () => RetainerList.IsOpen().Result);
                        await Coroutine.Sleep(1000);
                    }

                    if (! await RetainerList.IsOpen()) Log("Failed opening retainer list");

                    LogVerbose("Open:" + await RetainerList.IsOpen());

                    await Coroutine.Wait(5000, () => RetainerList.IsOpen().Result);

                    await Coroutine.Sleep(1000);
                    await RetainerList.SelectRetainer(retainerIndex);
                    await Coroutine.Sleep(200);
                    await Coroutine.Wait(5000, () => RetainerTasks.IsOpen);

                    if (!retainerNames.ContainsKey(retainerIndex))
                    {
                        retainerNames.Add(retainerIndex, GetRetainerName());
                    }

                    Log("Selected Retainer: " + retainerNames[retainerIndex]);

                    if (RetainerSettings.Instance.GetGil)
                        HelperFunctions.GetRetainerGil();

                    RetainerTasks.OpenInventory();
                    await Coroutine.Wait(5000, RetainerTasks.IsInventoryOpen);

                    if (RetainerTasks.IsInventoryOpen())
                    {
                        LogVerbose("Inventory open");
                        foreach (var retbag in InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds))
                        foreach (var item in retbag.FilledSlots.Where(FilterStackable))
                            try
                            {
                                inventory.AddItem(item);
                                if (masterInventory.ContainsKey(item.TrueItemId))
                                {
                                    masterInventory[item.TrueItemId]
                                        .Add(new KeyValuePair<int, uint>(retainerIndex, item.Count));
                                }
                                else
                                {
                                    masterInventory.Add(item.TrueItemId, new List<KeyValuePair<int, uint>>());
                                    masterInventory[item.TrueItemId]
                                        .Add(new KeyValuePair<int, uint>(retainerIndex, item.Count));
                                }

                            }
                            catch (Exception e)
                            {
                                LogCritical("SHIT:" + e);
                                throw;
                            }

                        LogVerbose("Inventory done");

                        Log("Checking retainer[{0}] against player inventory", retainerNames[retainerIndex]);

                        foreach (var item in InventoryManager.FilledSlots.Where(x => x.BagId == InventoryBagId.Bag1 || x.BagId == InventoryBagId.Bag2 || x.BagId == InventoryBagId.Bag3 || x.BagId == InventoryBagId.Bag4).Where(FilterStackable))
                            if (inventory.HasItem(item.TrueItemId))
                            {
                                Log("PLAYER AND RETAINER both have Name: " + item.Item.EnglishName +
                                    "\tItemCategory: " + item.Item.EquipmentCatagory + "\tId: " + item.Item.Id);

                                if (RetainerSettings.Instance.DepositFromPlayer)
                                {
                                    Log("Moved: " + MoveItem(item,inventory.GetItem(item.TrueItemId)));
                                    await Coroutine.Sleep(200);
                                }
                            }

                        Log("Done checking against player inventory");

                        RetainerTasks.CloseInventory();

                        await Coroutine.Sleep(500);

                        await Coroutine.Wait(3000, () => RetainerTasks.IsOpen);

                        RetainerTasks.CloseTasks();

                        await Coroutine.Sleep(500);

                        await Coroutine.Wait(1500, () => DialogOpen);

                        if (DialogOpen) Next();

                        await Coroutine.Sleep(200);

                        await Coroutine.Wait(3000, () => RetainerList.IsOpen().Result);

                        LogVerbose("Should be back at retainer list by now");

                        await Coroutine.Sleep(200);

                    }

                    retList.Add(inventory);
                }

                //await Coroutine.Sleep(1000);

                if (RetainerSettings.Instance.DontOrganizeRetainers || !RetainerSettings.Instance.DepositFromPlayer)
                {
                    RetainerList.Close();


                    TreeRoot.Stop("Done playing with retainers (Don't organize or don't deposit items.)");
                    return true;
                }


                if (debug)
                    foreach (var itemId in masterInventory)
                    {
                        var retainers = "";

                        foreach (var retainerId in itemId.Value)
                            retainers += $"Retainer[{retainerNames[retainerId.Key]}] has {retainerId.Value} ";

                        Log("Item {0}: {1}", itemId.Key, retainers);
                    }

                LogCritical("Duplicate items Found:");

                if (debug)
                    foreach (var itemId in masterInventory.Where(r => r.Value.Count > 1))
                    {
                        var retainers = "";
                        var retListInv = new List<KeyValuePair<int, uint>>(itemId.Value.OrderByDescending(r => r.Value));
                        
                        foreach (var retainerId in retListInv)
                            retainers += $"Retainer[{retainerNames[retainerId.Key]}] has {retainerId.Value} ";

                        Log("Item {0}: {1}", itemId.Key, retainers);
                    }

                /*
                 * Same as above but before the second foreach save retainer/count
                 * remove that one since it's where we're going to move stuff to
                 */
                int numOfMoves = 0;

                foreach (var itemId in masterInventory.Where(r => r.Value.Count > 1))
                {
                    List<KeyValuePair<int, uint>> retListInv = new List<KeyValuePair<int, uint>>(itemId.Value.OrderByDescending(r => r.Value));

                    int retainerTemp = retListInv[0].Key;
                    uint countTemp = retListInv[0].Value;

                    var retainers = "";

                    retListInv.RemoveAt(0);


                    foreach (var retainerId in retListInv)
                    {
                        retainers += $"Retainer[{retainerNames[retainerId.Key]}] has {retainerId.Value} ";
                        countTemp += retainerId.Value;
                    }

                    Log("Item: {4} ({0}) Total:{3} should be in {1} and {2}", itemId.Key, retainerNames[retainerTemp], retainers, countTemp, DataManager.GetItem(itemId.Key));

                    if (countTemp > 999)
                    {
                        LogCritical("This item will have a stack size over 999: {0}", itemId.Key);
                    }
                    else
                    {
                        numOfMoves++;
                        foreach (var retainerIdTemp in retListInv)
                            moveFrom[retainerIdTemp.Key].Add(itemId.Key);
                    }
                }

                LogCritical("Looks like we need to do {0} moves", numOfMoves);

                if (numOfMoves < InventoryManager.FreeSlots && numOfMoves > 0)
                {
                    LogCritical(
                        "Looks like we have {0} free spaces in inventory so we can just dump into player inventory",
                        InventoryManager.FreeSlots);

                    //First loop
                    for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
                    {
                        var inventory = new RetainerInventory();

                        if (! await RetainerList.IsOpen())
                        {
                            await UseSummoningBell();
                            await Coroutine.Wait(5000, () => RetainerList.IsOpen().Result);
                            await Coroutine.Sleep(1000);
                        }

                        await Coroutine.Wait(5000, () => RetainerList.IsOpen().Result);

                        await Coroutine.Sleep(1000);

                        if (! await RetainerList.IsOpen()) Log("Failed opening retainer list");

                        LogVerbose("Open:" + RetainerList.IsOpen().Result);
                        
                        await RetainerList.SelectRetainer(retainerIndex);

                        Log("Selected Retainer: " + retainerNames[retainerIndex]);

                        await Coroutine.Wait(5000, () => RetainerTasks.IsOpen);

                        RetainerTasks.OpenInventory();

                        await Coroutine.Wait(5000, RetainerTasks.IsInventoryOpen);

                        if (RetainerTasks.IsInventoryOpen())
                        {
                            LogVerbose("Inventory open");
                            foreach (Bag retbag in InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds))
                            foreach (var item in retbag.FilledSlots.Where(FilterStackable))
                                try
                                {
                                    inventory.AddItem(item);
                                    if (masterInventory.ContainsKey(item.TrueItemId))
                                    {
                                        masterInventory[item.TrueItemId].Add(new KeyValuePair<int, uint>(retainerIndex, item.Count));
                                    }
                                    else
                                    {
                                        masterInventory.Add(item.TrueItemId, new List<KeyValuePair<int, uint>>());
                                        masterInventory[item.TrueItemId].Add(new KeyValuePair<int, uint>(retainerIndex, item.Count));
                                    }

                                    //Logging.Write("Name: {0} Count: {1} BagId: {2} IsHQ: {3}", item.Item.EnglishName, item.Item.StackSize, item.BagId, item.Item.IsHighQuality);
                                }
                                catch (Exception e)
                                {
                                    Log("SHIT:" + e);
                                    throw;
                                }

                            LogVerbose("Inventory done");

                            Log("Checking retainer[{0}] against move list", retainerNames[retainerIndex]);


                            foreach (var item in moveFrom[retainerIndex])
                            {
                                var moved = false;
                                if (inventory.HasItem(item))
                                    foreach (var bagId in InventoryManager.GetBagsByInventoryBagId(inventoryBagId_0))
                                    {
                                        if (moved)
                                            break;

                                        foreach (var bagslot in bagId)
                                            if (!bagslot.IsFilled)
                                            {
                                                Log("Moved: " + inventory.GetItem(item).Move(bagslot));
                                                await Coroutine.Sleep(200);
                                                moved = true;
                                                break;
                                            }
                                    }
                            }

                            Log("Done checking against player inventory");

                            RetainerTasks.CloseInventory();

                            await Coroutine.Sleep(500);

                            await Coroutine.Wait(3000, () => RetainerTasks.IsOpen);

                            RetainerTasks.CloseTasks();

                            await Coroutine.Sleep(500);

                            await Coroutine.Wait(3000, () => DialogOpen);

                            if (DialogOpen) Next();

                            await Coroutine.Sleep(200);

                            await Coroutine.Wait(3000, () => RetainerList.IsOpen().Result);

                            LogVerbose("Should be back at retainer list by now");

                            //inventory.PrintList();
                        }
                    }
                }
                else
                {
                    if (numOfMoves <= 0)
                    {
                        LogCritical("No duplicate stacks found so no moved needed.");
                        RetainerList.Close();


                        TreeRoot.Stop("Done playing with retainers");
                        return true;
                    }

                    LogCritical("Crap, we don't have enough player inventory to dump it all here");
                    RetainerList.Close();


                    TreeRoot.Stop("Done playing with retainers");
                    return false;
                }

                for (var retainerIndex = 0; retainerIndex < numRetainers; retainerIndex++)
                {
                    var inventory = new RetainerInventory();

                    if (! await RetainerList.IsOpen())
                    {
                        await UseSummoningBell();
                        await Coroutine.Wait(5000, () => RetainerList.IsOpen().Result);
                        await Coroutine.Sleep(1000);
                    }

                    await Coroutine.Wait(5000, () => RetainerList.IsOpen().Result);

                    await Coroutine.Sleep(1000);

                    if (! await RetainerList.IsOpen()) Log("Failed opening retainer list");

                    LogVerbose("Open:" + RetainerList.IsOpen().Result);

                    await RetainerList.SelectRetainer(retainerIndex);

                    Log("Selected Retainer: " + retainerNames[retainerIndex]);

                    await Coroutine.Wait(5000, () => RetainerTasks.IsOpen);

                    RetainerTasks.OpenInventory();

                    await Coroutine.Wait(5000, RetainerTasks.IsInventoryOpen);

                    if (RetainerTasks.IsInventoryOpen())
                    {
                        LogVerbose("Inventory open");
                        foreach (var retbag in InventoryManager.GetBagsByInventoryBagId(HelperFunctions.RetainerBagIds))
                        foreach (var item in retbag.FilledSlots.Where(FilterStackable))
                            try
                            {
                                inventory.AddItem(item);
                                if (masterInventory.ContainsKey(item.TrueItemId))
                                {
                                    masterInventory[item.TrueItemId]
                                        .Add(new KeyValuePair<int, uint>(retainerIndex, item.Count));
                                }
                                else
                                {
                                    masterInventory.Add(item.TrueItemId, new List<KeyValuePair<int, uint>>());
                                    masterInventory[item.TrueItemId]
                                        .Add(new KeyValuePair<int, uint>(retainerIndex, item.Count));
                                }
                            }
                            catch (Exception e)
                            {
                                Log("SHIT:" + e);
                                throw;
                            }

                        LogVerbose("Inventory done");

                        Log("Checking retainer[{0}] against player inventory", retainerNames[retainerIndex]);

                        foreach (var item in InventoryManager.FilledSlots.Where(x => x.BagId == InventoryBagId.Bag1 || x.BagId == InventoryBagId.Bag2 || x.BagId == InventoryBagId.Bag3 || x.BagId == InventoryBagId.Bag4).Where(FilterStackable))
                            if (inventory.HasItem(item.TrueItemId))
                            {
                                Log("BOTH PLAYER AND RETAINER HAVE Name: " + item.Item.EnglishName +
                                    "\tItemCategory: " + item.Item.EquipmentCatagory + "\tId: " + item.Item.Id);

                                if (RetainerSettings.Instance.DepositFromPlayer)
                                {
                                    Log("Moved: " + MoveItem(item,
                                            inventory.GetItem(item.TrueItemId)));
                                    await Coroutine.Sleep(200);
                                }
                            }

                        Log("Done checking against player inventory");

                        RetainerTasks.CloseInventory();

                        await Coroutine.Sleep(500);

                        await Coroutine.Wait(3000, () => RetainerTasks.IsOpen);

                        //await Coroutine.Sleep(1000);

                        //Call quit in tasks and get through dialog

                        RetainerTasks.CloseTasks();

                        await Coroutine.Sleep(500);

                        await Coroutine.Wait(3000, () => DialogOpen);

                        if (DialogOpen) Next();

                        await Coroutine.Sleep(200);

                        await Coroutine.Wait(3000, () => RetainerList.IsOpen().Result);

                        LogVerbose("Should be back at retainer list by now");

                        //inventory.PrintList();
                    }

                    retList.Add(inventory);
                }


                LogVerbose("Closing Retainer List");

                RetainerList.Close();


                TreeRoot.Stop("Done playing with retainers");

                done = true;
            }

            return true;
        }

        private async Task<bool> UseSummoningBell()
        {
            GameObject bell = HelperFunctions.NearestSummoningBell();

            if (bell == null)
            {
                LogCritical("No summoning bell near by");
                return false;
            }

            if (bell.Distance2D(Core.Me.Location) >= 3)
            {
                await MoveSummoningBell(bell.Location);
                if (bell.Distance2D(Core.Me.Location) >= 3) return false;
            }

            bell.Interact();
            // No need to wait on IsOpen when we already do it in the main task.
            await Coroutine.Wait(5000, () => RetainerList.IsOpen().Result);
            LogVerbose("Summoning Bell Used");

            return true;
        }


        private static async Task<bool> MoveSummoningBell(Vector3 loc)
        {
            var moving = MoveResult.GeneratingPath;
            while (!(moving == MoveResult.Done ||
                     moving == MoveResult.ReachedDestination ||
                     moving == MoveResult.Failed ||
                     moving == MoveResult.Failure ||
                     moving == MoveResult.PathGenerationFailed))
            {
                moving = Flightor.MoveTo(new FlyToParameters(loc));

                await Coroutine.Yield();
            }

            return true;
        }
    }
}