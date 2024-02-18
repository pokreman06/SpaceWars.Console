using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SWConsole;

public class Logic
{
    public Logic() { }
    public (int x, int y) findNearestPlayer((int x, int y)[] players, int Order)
    {
        if(players.Length == 0)
            return (0, 0);
        var x = new (int x, int y)[players.Length - 1];
        int y = 0;
        for (var i = 0; i<players.Length; i++)
        {

            if(i!=Order-1)
            {
                x[y]=players[i];
                y++;
            }
        }
        (int,int) bas = players[Order-1];
        double? smallest = null;
        (int, int) small = (0,0);
        foreach((int, int) player in x)
        {
            var distance = CalcDistance(player, bas);
            if(smallest == null || distance < smallest) { smallest = distance; small = player; }
        }

    return small;
    }

public Double CalcDistance((int x, int y) x,(int x, int y) y)
{
    int nx = x.x - y.x;
    int ny = x.y - y.y;
    int nnx = nx * nx;
    int nny = ny * ny;
    int n = nnx + nny;
    return Math.Sqrt(n);
}
public int FindHeading((int x, int y) first, (int x, int y) second)
    {
        second.x = second.x - first.x;
        second.y = second.y - first.y;
        double distance = CalcDistance((0,0), second);
        return (int)(Math.Acos(second.x/distance)*180/Math.PI);


    }

}
