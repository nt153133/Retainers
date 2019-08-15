using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using TreeSharp;

namespace Retainers
{
    public class RetainerSettings : JsonSettings
    {
        private static RetainerSettings _settings;

        private bool _deposit;

        private int _numOfRetainers;


        public RetainerSettings() : base(Path.Combine(CharacterSettingsDirectory, "RetainerSettings.json"))
        {
        }

        public static RetainerSettings Instance => _settings ?? (_settings = new RetainerSettings());

        [Description("Entrust items to retainer if the have the same item?")]
        [DefaultValue(true)] //shift +x
        public bool DepositFromPlayer
        {
            get => _deposit;
            set
            {
                if (_deposit != value)
                {
                    _deposit = value;
                    Save();
                }
            }
        }

        [Description("How many retainers do you have? (warning setting higher then you have will crash game)")]
        [DefaultValue(2)]
        public int NumberOfRetainers
        {
            get => _numOfRetainers;
            set
            {
                if (_numOfRetainers != value)
                {
                    _numOfRetainers = value;
                    Save();
                }
            }
        }

    }

}