using System;
using System.Collections.Generic;

using Rocket.API;

namespace Uconomy_Essentials
{
    public class UconomyEConfiguration : IRocketPluginConfiguration
    {
        public bool LosePercentageToggle;
        public bool PayTime;
        public List<Group> PayGroups;
        public int PayTimeSeconds;
        public bool PayHit;
        public float PayHitAmt;
        public bool SendPayHitMsg;
        public bool LoseSuicide;
        public float LoseSuicideAmt;
        public bool ExpExchange;
        public float ExpExchangerate;
        public bool MoneyExchange;
        public float MoneyExchangerate;
        public bool LoseMoneyOnDeath;
        public float LoseMoneyOnDeathAmt;
        public bool PayZombie;
        public float PayZombieAmt;
        public bool SendPayZombieMsg;
        public bool PayMegaZombie;
        public float PayMegaZombieAmt;
        public bool SendPayMegaZombieMsg;


        public void LoadDefaults()
        {
            LosePercentageToggle = true;
            PayTime = false;
            PayGroups = new List<Group>() {
                new Group{DisplayName = "all", Salary = 1.0m},
                new Group{DisplayName = "admin", Salary = 5.0m},
                new Group{DisplayName = "moderator", Salary = 4.0m},
                new Group{DisplayName = "guest", Salary = 1.0m}
            };
            PayTimeSeconds = 900;
            PayHit = false;
            PayHitAmt = 1.0f;
            SendPayHitMsg = true;
            LoseSuicide = false;
            LoseSuicideAmt = 1.0f;
            ExpExchange = false;
            ExpExchangerate = 0.5f;
            MoneyExchange = false;
            MoneyExchangerate = 0.5f;
            LoseMoneyOnDeath = false;
            LoseMoneyOnDeathAmt = 10.0f;
            PayZombie = false;
            PayZombieAmt = 0.5f;
            SendPayZombieMsg = true;
            PayMegaZombie = false;
            PayMegaZombieAmt = 5.0f;
            SendPayMegaZombieMsg = true;
        }
    }
}
