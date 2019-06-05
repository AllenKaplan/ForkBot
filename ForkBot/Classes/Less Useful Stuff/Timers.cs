﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace ForkBot
{
    public class Timers
    {
        public static Timer unpurge;
        public static async void UnPurge(object state)
        {
            await Var.purgeMessage.DeleteAsync();
            Var.purging = false;
            unpurge.Dispose();
        }

        public static Timer RemindTimer;
        public static async void Remind(object state)
        {
            if (!File.Exists("Files/userreminders.txt")) File.WriteAllText("Files/userreminders.txt", "");
            string[] reminders = File.ReadAllLines("Files/userreminders.txt");
            bool changed = false;
            for(int i = reminders.Count() - 1; i >= 0; i--)
            {
                //format: user_id//#//reminder//#//datetimeString
                var reminderData = reminders[i].Split(new string[] { "//#//" }, StringSplitOptions.None);
                if (Var.CurrentDate() > Functions.StringToDateTime(reminderData[2]))
                {
                    changed = true;
                    reminders[i] = "";

                    var user = Bot.client.GetUser(Convert.ToUInt64(reminderData[0]));
                    await user.SendMessageAsync(reminderData[1]);
                }
            }

            if (changed)
            {
                File.Delete("Files/userreminders.txt");
                File.WriteAllLines("Files/userreminders.txt", reminders.Where(x => x != ""));
            }
            
        }

        public static Timer BidTimer;
        public static async void Bid(object state)
        {
            string[] posts = File.ReadAllLines("Files/FreeMarket.txt");
            List<string> expired = new List<string>();
            List<string> expiringSoon = new List<string>();
            foreach(string post in posts)
            {
                var expiryDate = Functions.StringToDateTime(post.Split('|')[5]) + new TimeSpan(14,0,0,0);
                if (expiryDate - Var.CurrentDate() < new TimeSpan(0)) expired.Add(post);
                else if (expiryDate - Var.CurrentDate() < new TimeSpan(1, 0, 0, 0))
                {
                    string id = post.Split('|')[0];
                    if (Properties.Settings.Default.warnedFMs == null) Properties.Settings.Default.warnedFMs = new System.Collections.Specialized.StringCollection();
                    if (!Properties.Settings.Default.warnedFMs.Contains(id))
                    {
                        Properties.Settings.Default.warnedFMs.Add(id);
                        Properties.Settings.Default.Save();
                        expiringSoon.Add(post);
                    }

                }
            }
            
            // postID|userID|item|count|cost|dateposted
            //   [0] |  [1] |[2] | [3] | [4]|   [5]
            foreach (string warn in expiringSoon)
            {
                var data = warn.Split('|');
                ulong userID = Convert.ToUInt64(data[1]);
                var user = Bot.client.GetUser(userID);
                string postID = data[0];
                string count = data[3];
                string item = data[2];
                string price = data[4];
                await user.SendMessageAsync("", embed: new InfoEmbed("WARNING: FREE MARKET POSTING EXPIRATION", "This is your one warning that your Free Market posting of " +
                                                                    $"{count} {item} for {price} coins will be expiring in 24 hours. You may remove this posting for a 25% fee, or it will " +
                                                                    $"be auctioned off and the coins will go towards slots.").Build());
            }

            bool removed = false;
            List<string> bids = new List<string>();
            foreach(string expiry in expired)
            {
                var data = expiry.Split('|');
                ulong userID = Convert.ToUInt64(data[1]);
                var user = Bot.client.GetUser(userID);
                string postID = data[0];
                string count = data[3];
                string item = data[2];
                string price = data[4];
                await user.SendMessageAsync("", embed: new InfoEmbed("FREE MARKET POSTING EXPIRATION", "This message is to inform you that your Free Market posting of " +
                                                                    $"{count} {item} for {price} coins has expired. You have not removed it, and now it will " +
                                                                    $"be auctioned off and the coins will go towards slots.").Build());

                bids.Add(expiry);
                for (int i = 0; i < posts.Count(); i++)
                {
                    if (posts[i].Split('|')[0] == postID)
                    {
                        posts[i] = "";
                        removed = true;
                    }
                }
            }

            if (removed)
            {
                File.WriteAllLines("Files/FreeMarket.txt", posts.Where(x => x != ""));

                if (!File.Exists("Files/Bids.txt")) File.WriteAllText("Files/Bids.txt", "");
                foreach (string bid in bids)
                {
                    //finish bids
                }
            }
        }
    }
}
