﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net;
using System.IO;

namespace ForkBot
{
    public class Bot
    {
        static void Main(string[] args) => new Bot().Run().GetAwaiter().GetResult();

        Random rdm = new Random();

        public static DiscordSocketClient client;
        public static CommandService commands;
        public static List<User> users = new List<User>();

        public async Task Run()
        {
            Start:
            try
            {
                Console.WriteLine("Welcome. Initializing ForkBot...");
                client = new DiscordSocketClient();
                Console.WriteLine("Client Initialized.");
                commands = new CommandService();
                Console.WriteLine("Command Service Initialized.");
                await InstallCommands();
                Console.WriteLine("Commands Installed, logging in.");
                await client.LoginAsync(TokenType.Bot, File.ReadAllText("Constants/bottoken"));
                Console.WriteLine("Successfully logged in!");
                await client.StartAsync();
                Console.WriteLine("ForkBot successfully intialized.");
                Functions.LoadUsers();
                Timer banCheck = new Timer(new TimerCallback(TimerCall),null,0,1000);
                Timer hourlyTimer = new Timer(new TimerCallback(Hourly), null, 0, 1000*60*60);
                Timer life = new Timer(new TimerCallback(Timers.Life), null, 0, 1);
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("                                  ERROR                        ");
                Console.WriteLine("==========================================================================\n");
                Console.WriteLine($"Error occured in {e.Source}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);

                Again:

                Console.WriteLine("Would you like to try reconnecting? [Y/N]");
                var input = Console.Read();

                if (input == 121) { Console.Clear(); goto Start; }
                else if (input == 110) Environment.Exit(0);

                Console.WriteLine("Invalid input.");
                goto Again;
            }
        }

        async void TimerCall(object state) //code that is run every second
        {
            for(int i = 0; i < Var.leaveBanned.Count(); i++)
            {
                if (DateTime.Now > Var.unbanTime[i])
                {
                    var user = Var.leaveBanned[i];
                    var g = user.Guild;
                    await g.RemoveBanAsync(user);
                    Var.leaveBanned.Remove(user);
                    Var.unbanTime.Remove(Var.unbanTime[i]);

                    InfoEmbed iEmb = new InfoEmbed("USER UNBAN", $"User {user} has been unbanned.");
                    await (await g.GetDefaultChannelAsync()).SendMessageAsync("", embed: iEmb.Build());
                }
            }
        }

        void Hourly(object state) //code that is run every hour
        {
            Functions.SaveUsers();
        }

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            client.UserJoined += HandleJoin;
            client.UserLeft += HandleLeave;
            client.MessageDeleted += HandleDelete;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
        
        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;

            var user = Functions.GetUser(message.Author);

            if (Var.presentWaiting && message.Content == Convert.ToString(Var.presentNum))
            {
                Var.presentWaiting = false;
                await message.Channel.SendMessageAsync($"{message.Author.Username}! You got...");
                var presents = Functions.GetItemList();
                var presentData = presents[rdm.Next(presents.Count())].Split('|');
                Var.present = presentData[0];
                Var.rPresent = Var.present;
                var presentName = Var.present.Replace('_', ' ');
                var pMessage = presentData[1];
                await message.Channel.SendMessageAsync($"A {Func.ToTitleCase(presentName)}! :{Var.present}: {pMessage}");
                if (Var.present == "santa")
                {
                    await message.Channel.SendMessageAsync("You got...");
                    string sMessage = "";
                    for(int i = 0; i < 5; i++)
                    {
                        string sPresent = presents[rdm.Next(presents.Count())].Split('|')[0];
                        sMessage += ":" + sPresent + ": ";
                        user.Items.Add(sPresent);
                    }
                    await message.Channel.SendMessageAsync(sMessage);

                    Var.replaceable = false;
                }
                else user.Items.Add(Var.present);
                
                if (Var.replaceable)
                {
                    await message.Channel.SendMessageAsync($"Don't like this gift? Press {Var.presentNum} again to replace it once!");
                    Var.replacing = true;
                    Var.presentReplacer = message.Author;
                }
            }
            else if (Var.replaceable && Var.replacing && message.Content == Convert.ToString(Var.presentNum) && message.Author == Var.presentReplacer)
            {
                user.Items.Remove(Var.present);
                await message.Channel.SendMessageAsync("Okay! I'll be right back.");
                await Functions.SendAnimation(message.Channel, Constants.EmoteAnimations.presentReturn, $":{Var.rPresent}:");
                await message.Channel.SendMessageAsync($"A **new** present appears! :gift: Press {Var.presentNum} to open it!");
                Var.presentWaiting = true;
                Var.replacing = false;
                Var.replaceable = false;
            }
            
            
            if (message.HasCharPrefix(';', ref argPos))
            {
                var context = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                    var emb = new InfoEmbed("ERROR:", result.ErrorReason).Build();
                    await message.Channel.SendMessageAsync("",embed:emb);
                }
            }
            else return;
        }
        public async Task HandleJoin(SocketGuildUser user)
        {
            await (user.Guild.GetChannel(Constants.Channels.GENERAL) as IMessageChannel).SendMessageAsync($"{user.Username}! Welcome to {user.Guild.Name}! Go to {user.Guild.GetChannel(271843457121779712)} to get a role.");
        }
        public async Task HandleLeave(SocketGuildUser user)
        {
            await (user.Guild.GetChannel(Constants.Channels.GENERAL) as IMessageChannel).SendMessageAsync($"{user.Username} has left the server.");
            Console.WriteLine($"{user.Username} has been banned for 15 mins due to leaving the server.");
            Var.leaveBanned.Add(user);
            Var.unbanTime.Add(DateTime.Now + new TimeSpan(0, 15, 0));
            await user.Guild.AddBanAsync(user, reason:"Tempban for leaving server. Done automatically by ForkBot to prevent spam leave-joining. To be unbanned at: " + (DateTime.Now + new TimeSpan(0, 15, 0)).TimeOfDay);
        }
        public async Task HandleDelete(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            IMessage msg = await cache.DownloadAsync();
            EmbedBuilder emb = new EmbedBuilder();
            emb.Title = "MESSAGE DELETED";
            emb.Author.Name = msg.Author.Username;
            emb.Author.IconUrl = msg.Author.GetAvatarUrl();
            emb.Description = msg.Content;
            var chan = client.GetChannel(Constants.Channels.DELETED_MESSAGES) as IMessageChannel;
            await chan.SendMessageAsync("", embed: emb.Build());
        }
    }
}

