using System;
using System.Collections.Generic;

using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Permissions;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Steamworks;
using fr34kyn01535.Uconomy;
using Rocket.API.Serialisation;

namespace Uconomy_Essentials
{
    public class PlayerUE : UnturnedPlayerComponent
    {
        private DateTime lastpaid;
        private UnturnedPlayerEvents rpe;

        protected void Start()
        {
            this.rpe = base.gameObject.transform.GetComponent<UnturnedPlayerEvents>();
            this.lastpaid = DateTime.Now;
            this.rpe.OnDeath += new UnturnedPlayerEvents.PlayerDeath(this.rpe_OnPlayerDeath);
            this.rpe.OnUpdateStat += this.rpe_OnUpdateStat;
        }
        private void rpe_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (player == null) return;
            if (murderer == null) return;
            UnturnedPlayer killer = UnturnedPlayer.FromCSteamID(murderer);
            if (killer == null) return;
            if (Uconomy_Essentials.Instance.Configuration.Instance.PayHit)
            {
                Uconomy u = Uconomy.Instance;
                decimal bal = u.Database.GetBalance(player.CSteamID.ToString());
                if (cause == EDeathCause.SUICIDE && Uconomy_Essentials.Instance.Configuration.Instance.LoseSuicide)
                {
                    // We are going to remove currency for the suicide
                    decimal loss = (decimal)Uconomy_Essentials.Instance.Configuration.Instance.LoseSuicideAmt * -1.0m;
                    if (Uconomy_Essentials.Instance.Configuration.Instance.LosePercentageToggle)
                    {
                        if (!(Uconomy_Essentials.Instance.Configuration.Instance.LoseSuicideAmt > 100.00))
                        {
                            loss = ((decimal)Uconomy_Essentials.Instance.Configuration.Instance.LoseSuicideAmt / 100) * bal * -1.0m;
                        }
                    }
                    if (bal + loss < 0.0m) loss = bal * -1.0m;
                    decimal bal1 = u.Database.IncreaseBalance(player.CSteamID.ToString(), loss);
                    Uconomy_Essentials.HandleEvent(player, (loss * -1.0m), "loss");
                    UnturnedChat.Say(player.CSteamID, Uconomy_Essentials.Instance.Translate("lose_suicide_msg", new object[] {Convert.ToInt32(loss * -1.00M), u.Configuration.Instance.MoneyName}));
                    if (bal1 != 0m) UnturnedChat.Say(player.CSteamID, Uconomy_Essentials.Instance.Translate("new_balance_msg", new object[] {bal1, u.Configuration.Instance.MoneyName}));
                    return;
                }
                else if (cause == EDeathCause.SUICIDE && !Uconomy_Essentials.Instance.Configuration.Instance.LoseSuicide)
                {
                    // We do nothing if they suicide.
                    return;
                }
                if (Uconomy_Essentials.Instance.Configuration.Instance.LoseMoneyOnDeath)
                {
                    decimal loss = (decimal)Uconomy_Essentials.Instance.Configuration.Instance.LoseMoneyOnDeathAmt * -1.0m;
                    if (Uconomy_Essentials.Instance.Configuration.Instance.LosePercentageToggle)
                    {
                        if (!(Uconomy_Essentials.Instance.Configuration.Instance.LoseSuicideAmt > 100.00))
                        {
                            loss = ((decimal)Uconomy_Essentials.Instance.Configuration.Instance.LoseMoneyOnDeathAmt / 100) * bal * -1.0m;
                        }
                    }
                    if (bal + loss < 0.0m) loss = bal * -1.0m;
                    decimal lostbal = u.Database.IncreaseBalance(player.CSteamID.ToString(), loss);
                    Uconomy_Essentials.HandleEvent(player, (loss * -1.0m), "loss");
                    UnturnedChat.Say(player.CSteamID, Uconomy_Essentials.Instance.Translate("lose_money_on_death_msg", new object[] {Convert.ToInt32(loss * -1.00M), u.Configuration.Instance.MoneyName}));
                }
                // Pay the other player for the kill
                decimal balk = u.Database.IncreaseBalance(murderer.ToString(), (decimal)Uconomy_Essentials.Instance.Configuration.Instance.PayHitAmt);
                Uconomy_Essentials.HandleEvent(player, (decimal)Uconomy_Essentials.Instance.Configuration.Instance.PayHitAmt, "paid");
                if (!Uconomy_Essentials.Instance.Configuration.Instance.SendPayHitMsg) return; // No message is to be sent, so job is done.
                UnturnedChat.Say(murderer, Uconomy_Essentials.Instance.Translate("to_killer_msg", new object[] {Uconomy_Essentials.Instance.Configuration.Instance.PayHitAmt.ToString(), u.Configuration.Instance.MoneyName, player.CharacterName}));
                if (bal != 0m) UnturnedChat.Say(murderer, Uconomy_Essentials.Instance.Translate("new_balance_msg", new object[] {balk.ToString(), u.Configuration.Instance.MoneyName}));
            }
        }
        private void rpe_OnUpdateStat(UnturnedPlayer player, EPlayerStat stat)
        {
            if (player == null) return;
            Uconomy u = Uconomy.Instance;
            if (Uconomy_Essentials.Instance.Configuration.Instance.PayZombie && stat == EPlayerStat.KILLS_ZOMBIES_NORMAL)
            {
                decimal balzk = u.Database.IncreaseBalance(player.CSteamID.ToString(), (decimal)Uconomy_Essentials.Instance.Configuration.Instance.PayZombieAmt);
                Uconomy_Essentials.HandleEvent(player, (decimal)Uconomy_Essentials.Instance.Configuration.Instance.PayZombieAmt, "paid");
                if (!Uconomy_Essentials.Instance.Configuration.Instance.SendPayZombieMsg) return; // No message is to be sent, so job is done.
                UnturnedChat.Say(player.CSteamID, Uconomy_Essentials.Instance.Translate("zombie_kill_paid_msg", new object[] { Uconomy_Essentials.Instance.Configuration.Instance.PayZombieAmt.ToString(), u.Configuration.Instance.MoneyName, balzk.ToString(), u.Configuration.Instance.MoneyName }));
            }
            else if (Uconomy_Essentials.Instance.Configuration.Instance.PayMegaZombie && stat == EPlayerStat.KILLS_ZOMBIES_MEGA)
            {
                decimal balzk = u.Database.IncreaseBalance(player.CSteamID.ToString(), (decimal)Uconomy_Essentials.Instance.Configuration.Instance.PayMegaZombieAmt);
                Uconomy_Essentials.HandleEvent(player, (decimal)Uconomy_Essentials.Instance.Configuration.Instance.PayMegaZombieAmt, "paid");
                if (!Uconomy_Essentials.Instance.Configuration.Instance.SendPayMegaZombieMsg) return; // No message is to be sent, so job is done.
                UnturnedChat.Say(player.CSteamID, Uconomy_Essentials.Instance.Translate("mega_zombie_kill_paid_msg", new object[] { Uconomy_Essentials.Instance.Configuration.Instance.PayMegaZombieAmt.ToString(), u.Configuration.Instance.MoneyName, balzk.ToString(), u.Configuration.Instance.MoneyName }));
            }
        }
        private void FixedUpdate()
        {
            if (Uconomy_Essentials.Instance.Configuration.Instance.PayTime && (DateTime.Now - this.lastpaid).TotalSeconds >= Uconomy_Essentials.Instance.Configuration.Instance.PayTimeSeconds)
            {
                this.lastpaid = DateTime.Now;
                decimal pay = 0.0m;
                Uconomy_Essentials.Instance.PayGroups.TryGetValue("all", out pay);
                string paygroup = "Player";
                if (pay == 0.0m)
                {
                    // We are checking for the different groups as All is not set.
                    if (this.Player.IsAdmin && Uconomy_Essentials.Instance.PayGroups.ContainsKey("admin"))
                    {
                        Uconomy_Essentials.Instance.PayGroups.TryGetValue("admin", out pay);
                        paygroup = "admin";
                        if (pay == 0.0m)
                        {
                            Rocket.Core.Logging.Logger.Log(Uconomy_Essentials.Instance.Translate("unable_to_pay_group_msg", new object[] {this.Player.CharacterName, "admin"}));
                            return;
                        }
                    }
                    else
                    {
                        // They aren't admin so we'll just go through like groups like normal.
                        
                        List<RocketPermissionsGroup> plgroups = R.Permissions.GetGroups(this.Player, true);
                        decimal pay2 = 0.0m;
                        foreach (RocketPermissionsGroup s in plgroups)
                        {
                            Rocket.Core.Logging.Logger.Log(s.Id);
                            Uconomy_Essentials.Instance.PayGroups.TryGetValue(s.Id, out pay2);
                            Rocket.Core.Logging.Logger.Log(pay2.ToString());
                            if (pay2 > pay)
                            {
                                pay = pay2;
                                paygroup = s.Id;
                            }
                        }
                        if (pay == 0.0m)
                        {
                            // We assume they are default group.
                            Uconomy_Essentials.Instance.PayGroups.TryGetValue("default", out pay);
                            if (pay == 0.0m)
                            {
                                // There was an error.  End it.
                                Rocket.Core.Logging.Logger.Log(Uconomy_Essentials.Instance.Translate("unable_to_pay_group_msg", new object[] {this.Player.CharacterName, ""}));
                                return;
                            }
                        }
                    }
                }
                decimal bal = Uconomy.Instance.Database.IncreaseBalance(this.Player.CSteamID.ToString(), pay);
                Uconomy_Essentials.HandleEvent(this.Player, pay, "paid");
                UnturnedChat.Say(this.Player.CSteamID, Uconomy_Essentials.Instance.Translate("pay_time_msg", new object[] {pay, Uconomy.Instance.Configuration.Instance.MoneyName, paygroup}));
                if (bal >= 0.0m) UnturnedChat.Say(this.Player.CSteamID, Uconomy_Essentials.Instance.Translate("new_balance_msg", new object[] { bal, Uconomy.Instance.Configuration.Instance.MoneyName }));
            }
        }
    }
}
