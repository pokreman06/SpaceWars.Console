﻿using SWConsole;
using System.Diagnostics;

namespace SpaceWarsServices;

class Program
{

    int loc;
    static async Task Main(string[] args)
    {
        const ConsoleKey forwardKey = ConsoleKey.UpArrow;
        const ConsoleKey leftKey = ConsoleKey.LeftArrow;
        const ConsoleKey rightKey = ConsoleKey.RightArrow;
        const ConsoleKey fireKey = ConsoleKey.Spacebar;
        const ConsoleKey clearQueueKey = ConsoleKey.C;
        const ConsoleKey infoKey = ConsoleKey.I;
        const ConsoleKey shopKey = ConsoleKey.S;
        const ConsoleKey repairKey = ConsoleKey.R;
        const ConsoleKey readAndEmptyMessagesKey = ConsoleKey.M;
        const ConsoleKey face = ConsoleKey.Q;
        const ConsoleKey powerfist = ConsoleKey.Z;
        const ConsoleKey railgun = ConsoleKey.X;

        //**************************************************************************************
        //***  |    |    |    |                                            |    |    |    |    |
        //***  |    |    |    |       Change your key mappings here        |    |    |    |    |
        //***  V    V    V    V                                            V    V    V    V    V
        //**************************************************************************************
        Uri baseAddress = getApiBaseAddress(args);
        
        bool exitGame = false;
        var currentHeading = 0;
        var token = "";
        var service = new ApiService();
        List<PurchasableItem> Shop = new List<PurchasableItem>();
        JoinGameResponse joinGameResponse = null;

        Console.WriteLine("Please enter your name");
        var username = Console.ReadLine();
        try
        {
            joinGameResponse = await service.JoinGameAsync(username);
            token = joinGameResponse.Token;

            Shop = joinGameResponse.Shop.Select(item => new PurchasableItem(item.Cost, item.Name, item.Prerequisites)).ToList();

            Console.WriteLine($"Token:{joinGameResponse.Token}, Heading: {joinGameResponse.Heading}");
            Console.WriteLine($"Ship located at: {joinGameResponse.StartingLocation}, Game State is: {joinGameResponse.GameState}, Board Dimensions: {joinGameResponse.BoardWidth}, {joinGameResponse.BoardHeight}");

            OpenUrlInBrowser($"{baseAddress.AbsoluteUri}hud?token={token}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        var gameActions = new GameActions(username, joinGameResponse, service);
        gameActions.Weapons.Add("Basic Cannon");
        gameActions.CurrentWeapon = "Basic Cannon";

        while (!exitGame)
        {
            printStatus();
            ConsoleKeyInfo keyInfo = Console.ReadKey(true); // Read key without displaying it
            bool shiftPressed = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift);

            switch (keyInfo.Key)
            {
                case var key when key == powerfist:
                  await gameActions.PurchaseItemAsync("Power Fist");
                    break;
                    case var key when key == railgun:
                  await gameActions.PurchaseItemAsync("Rail Gun");
                    break;
                case var key when key == forwardKey:
                    await gameActions.MoveForwardAsync(shiftPressed);
                    break;
                case var key when key == face:
                    await gameActions.faceNearestPlayer();
                    break;
                case var key when key == leftKey:
                    await gameActions.RotateLeftAsync(shiftPressed);
                    break;
                case var key when key == rightKey:
                    await gameActions.RotateRightAsync(shiftPressed);
                    break;
                case var key when key == fireKey:
                    await gameActions.FireWeaponAsync(shiftPressed);
                    break;
                case var key when key == clearQueueKey:
                    await gameActions.ClearQueueAsync();
                    break;
                case var key when key == repairKey:
                    await gameActions.RepairShipAsync();
                    Console.WriteLine("Ship repair requested.");
                    break;
                case var key when key == infoKey:
                    foreach (var item in Shop)
                    {
                        Console.WriteLine($"upgrade: {item.Name}, cost: {item.Cost}");
                        Console.WriteLine("Press any key to continue.");
                        Console.ReadKey();
                    }
                    break;
                case var key when key == shopKey:

                    Console.WriteLine("please enter what you'd like to purchase from the shop, (if you've changed your mind enter x)");
                    var response = Console.ReadLine();
                    if (response == "x")
                    {
                        continue;
                    }

                    if (Shop.Any(item => item.Name.Equals(response, StringComparison.OrdinalIgnoreCase)))
                    {
                        await gameActions.PurchaseItemAsync(response);
                        Console.WriteLine($"Purchase of {response} requested.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid item. Please choose a valid item from the shop.");
                    }
                    break;
                case var key when key == readAndEmptyMessagesKey:
                    await gameActions.ReadAndEmptyMessagesAsync();
                    Console.WriteLine("Message queue read.");
                    break;
                case var key when key >= ConsoleKey.D0 && key <= ConsoleKey.D9:
                    gameActions.SelectWeapon(key);
                    Console.WriteLine($"Selected weapon {((char)key) - '1'} ({gameActions.CurrentWeapon}");
                    break;
                //**************************************************************************************
                //***  |    |    |    |                                            |    |    |    |    |
                //***  |    |    |    |       Add any other custom keys here       |    |    |    |    |
                //***  V    V    V    V                                            V    V    V    V    V
                //**************************************************************************************
                case ConsoleKey.N:
                    //example
                    break;
            }
        }

        void printStatus()
        {
            Console.Clear();
            Console.WriteLine($"Name: {username,-34} Token: {gameActions.Token}");
            Console.WriteLine($"Left: {leftKey,-12} Right: {rightKey,-12} Forward: {forwardKey,-12} Fire: {fireKey,-12} Clear Queue: {clearQueueKey,-12}");
            Console.WriteLine($"Info: {infoKey,-12}  Shop: {shopKey,-12}  Repair: {repairKey,-12} Read & Empty Messages: {readAndEmptyMessagesKey,-12}");
            Console.WriteLine("player order" + service.order);

            for (int i = 0; i < gameActions.Weapons.Count; i++)
            {
                string? weapon = gameActions.Weapons[i];
                if (weapon == gameActions.CurrentWeapon)
                {
                    weapon = $"**{weapon}**";
                }
                Console.Write($"{i + 1}: {weapon}   ");
            }
            Console.WriteLine();


            if (gameActions.GameMessages.Any())
            {
                Console.WriteLine();
                
                Console.WriteLine("Last 10 messages:");
                Console.WriteLine(new string('-', Console.WindowWidth));
                foreach (var msg in gameActions.GameMessages.TakeLast(10))
                {
                    Console.WriteLine($"{msg.Type,-30} {msg.Message}");
                }
            }
            Console.WriteLine(new string('=', Console.WindowWidth));
        }
    }


    private static Uri getApiBaseAddress(string[] args)
    {
        Uri baseAddress;
        if (args.Length == 0)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Please enter the URL to access Space Wars");
                    baseAddress = new Uri(Console.ReadLine());
                    break;
                }
                catch { }
            }
        }
        else
        {
            baseAddress = new Uri(args[0]);
        }
        return baseAddress;
    }

    static void OpenUrlInBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening URL in browser: {ex.Message}");
        }
    }
}
