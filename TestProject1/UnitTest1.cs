using SpaceWarsServices;
using System.Security.Cryptography.X509Certificates;
using static System.Net.WebRequestMethods;

namespace TestProject1;


public class UnitTest1
{
    [Fact]
    public void findNearestPlayerReturnsBlank()
    {
        var test = new Logic();
       Assert.Equal(test.findNearestPlayer(new (int,int)[] {}, 0), (0,0));
    }
    [Fact]
    public void CalcDistanceWorks()
    {
        var test = new Logic();
        Assert.Equal(1, (int)test.CalcDistance((1, 1), (1, 2)));
        Assert.Equal(2, (int)test.CalcDistance( (1, 1), (1, 3)));


    }
    [Fact]
    public void FindNearestPlayerReturnsNearest() 
    {
        var test = new Logic();
        Assert.Equal(test.findNearestPlayer(new (int, int)[] {(0,0),(1,1),(1,0) }, 0), (1, 0));
    }
    [Fact]
    public void findHeadingworks()
    {
        var test = new Logic();
        Assert.Equal(90, test.FindHeading((0, 0), (0, 1)));
    }
    [Fact]
    public async void GetListOfPlayers()
    {
        using HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("https://snowspacewars.azurewebsites.net/") };
        var service = new ApiService(httpClient);
        var y = await service.Locations();
        int x = 0;

    }


}