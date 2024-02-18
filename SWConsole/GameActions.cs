using SpaceWarsServices;
using System;

namespace SWConsole;
public enum Direction { Right, Left }

public class GameActions
{
    private readonly JoinGameResponse joinGameResponse;
    private readonly ApiService apiService;
    private int heading;

    public GameActions(string playerName, JoinGameResponse joinGameResponse, ApiService apiService)
    {
        this.joinGameResponse = joinGameResponse;
        this.apiService = apiService;
        heading = joinGameResponse.Heading;
        PlayerName = playerName;
    }
    public async Task RotateLeftAsync(bool quickTurn) => await rotate(Direction.Left, quickTurn);

    public async Task RotateRightAsync(bool quickTurn) => await rotate(Direction.Right, quickTurn);
    public async Task RotateToDirection(int heading)
    {
        await apiService.QueueAction([new("changeHeading", heading.ToString())]);
    }

    private async Task rotate(Direction direction, bool quickTurn)
    {
        heading = (direction, quickTurn) switch
        {
            (Direction.Right, true) => heading + 10,
            (Direction.Right, false) => heading + 1,
            (Direction.Left, true) => heading - 10,
            (Direction.Left, false) => heading - 1,
            _ => 0,//turn north if someone calls this with a bogus Direction
        };
        heading = ClampRotation(heading);
        await apiService.QueueAction([new("changeHeading", heading.ToString())]);
    }

    public async Task MoveForwardAsync(bool lightSpeed)
    {
        heading = ClampRotation(heading);
        var actions = Enumerable.Range(0, lightSpeed ? 10 : 1)
                .Select(n => new QueueActionRequest("move", heading.ToString()));
        await apiService.QueueAction(actions);
    }

    public async Task FireWeaponAsync(bool shiftPressed, string? weapon = null) {
        int s = 1;
        if (shiftPressed)
            s = 10;
        for (int x = 0; x<s; x++)
        {
            await apiService.QueueAction([new("fire", weapon ?? CurrentWeapon)]); }
    }

    public async Task RepairShipAsync() => await apiService.QueueAction([new("repair", null)]);

    public async Task ClearQueueAsync() => await apiService.ClearAction();

    public async Task PurchaseItemAsync(string item) => await apiService.QueueAction([new("purchase", item)]);
    public async Task PurchasePowerFistAsync() => await apiService.QueueAction([new("purchase", "Power Fist")]);
   // public async Task PurchaseRailGunAsync() => await apiService.QueueAction([new("purchase", "Rail Gun")]);
    public async Task Copy() => await apiService.copy();

    private static int ClampRotation(int degrees)
    {
        degrees %= 360;
        if (degrees < 0)
            degrees += 360;
        return degrees;
    }
    public async Task faceNearestPlayer()
    {
        var temp = new Logic();
        var locations = await apiService.Locations();
        var x = temp.findNearestPlayer(locations, apiService.order);
        var _heading = ClampRotation(temp.FindHeading(x, locations[apiService.order-1]));
        heading = _heading;
        await apiService.QueueAction([new("changeHeading", _heading.ToString())]);
    }
    internal async Task ReadAndEmptyMessagesAsync()
    {
        var messages = await apiService.ReadAndEmptyMessages();
        GameMessages.AddRange(messages);
        //add weapons
        foreach (var weaponPurchaseMessage in messages.Where(m => m.Type == "SuccessfulWeaponPurchase"))
        {
            Weapons.Add(weaponPurchaseMessage.Message);
        }
    }
    internal void SelectWeapon(ConsoleKey key)
    {
        char c = (char)key;
        int index = c - '1';
        if (Weapons.Count > index)
        {
            CurrentWeapon = Weapons[index];
        }
    }

    public List<string> Weapons { get; set; } = new();
    public string CurrentWeapon { get; set; }
    public List<GameMessage> GameMessages { get; set; } = new();
    public string PlayerName { get; set; }
    public string Token => joinGameResponse.Token;
}

public static class IEnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }
}

