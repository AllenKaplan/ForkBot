﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ForkBot
{
    public class BMCommands : ModuleBase
    {
        Random rdm = new Random();

        public bool Check(ICommandContext Context, string item, bool remove = true)
        {
            var user = Functions.GetUser(Context.User);
            if (user.GetItemList().Contains(item))
            {
                if (remove) user.RemoveItem(item);
                return false;
            }
            return true;
        }

        [Command("tickets")]
        public async Task tickets(string command = "")
        {
            if (Check(Context, "tickets")) return;
            var user = Functions.GetUser(Context.User);

            if (user.GetData("bmlotto") == "0")
            {
                user.SetData("bmlotto", $"{rdm.Next(10)}{rdm.Next(10)}{rdm.Next(10)}{rdm.Next(10)}");
                await ReplyAsync("You now have two lotto numbers, they will both be checked when using `;lottery`. You can only replace your second lottery number using another :tickets:.");
            }
            else
            {
                user.SetData("bmlotto", $"{rdm.Next(10)}{rdm.Next(10)}{rdm.Next(10)}{rdm.Next(10)}");
                await ReplyAsync("You have successfully replaced your second lottery number.");
            }
        }

        [Command("credit_card")]
        public async Task creditCard()
        {
            if (Check(Context, "credit_card", false)) return;
            await ReplyAsync("This item functions passively! It will happen automatically when you gain allowance.");
        }

        [Command("warning")]
        public async Task warning()
        {
            User u = Functions.GetUser(Context.User);
            if (u.GetItemList().Contains("warning"))
            {
                await ReplyAsync("gimme a bit longer");
            }
        }

        [Command("gem")]
        public async Task gem()
        {
            if (Check(Context, "gem")) return;
            string gemTime = Functions.DateTimeToString(DateTime.Now + new TimeSpan(3, 0, 0));
            var user = Functions.GetUser(Context.User);
            user.SetData("gemtime", gemTime);
            await ReplyAsync("Your stat increases will be multiplied for 3 hours!");
        }

        [Command("fax")]
        public async Task Fax(ulong user, [Remainder] string message)
        {
            var reciever = Bot.client.GetUser(user);
            if (reciever == null || Check(Context, "fax", true)) return;


            string[] characters = { "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ", "ᴀAʙBᴄCᴅDᴇEғFɢGʜHɪIᴊJᴋKʟLᴍMɴNᴏOᴘPǫQʀRsSᴛTᴜUᴠVᴡWxXʏYᴢZ", "ₐₐbBcCdDₑₑfFgGₕₕᵢᵢⱼⱼₖₖₗₗₘₘₙₙₒₒₚₚqQᵣᵣₛₛₜₜᵤᵤᵥᵥwWₓₓyYzZ", "ᵃᴬᵇᴮᶜᶜᵈᴰᵉᴱᶠᶠᵍᴳʰᴴᶦᴵʲᴶᵏᴷˡᴸᵐᴹⁿᴺᵒᴼᵖᴾᑫQʳᴿˢˢᵗᵀᵘᵁᵛⱽʷᵂˣˣʸʸᶻᶻ" };
            string newMSG = "🕵💬 ";
            foreach (char c in message)
            {
                int script = rdm.Next(2) + 1;
                if (characters[0].Contains(c))
                    newMSG += characters[script][characters[0].IndexOf(c)];
                else newMSG += c;
            }

            await reciever.SendMessageAsync(newMSG);
        }
    }
}
