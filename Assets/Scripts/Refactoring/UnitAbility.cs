using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UnitAbility : MonoBehaviour
{
    // Общий класс-родитель для всех способностей
    public AbilityType abilityType;
    public string abilityName;
    public int range;

    public Sprite sprite;

    public virtual BattleInfo Use(BattleInfo info)
    {
        Debug.Log("Unit's ability is used!");
        return info;
    }

    protected static List<Point> Bts(IEnumerable<Point> barriers, Point start, Point end)
    {
        var a = new bool[8, 8];
        var b = new Point[8, 8];

        var q = new Queue<Point>();

        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                a[i, j] = true;

        if (!(barriers is null))
            foreach (var barrier in barriers)
                a[barrier.X, barrier.Y] = false;

        q.Enqueue(start);
        a[start.X, start.Y] = false;
        b[start.X, start.Y] = start;

        while (!q.Peek().Equals(end))
        {
            var e = q.Dequeue();

            var points = new[]
            {
                new Point(e.X - 1, e.Y),
                new Point(e.X + 1, e.Y),
                new Point(e.X, e.Y - 1),
                new Point(e.X, e.Y + 1),
                new Point(e.X + 1, e.Y + 1),
                new Point(e.X + 1, e.Y - 1),
                new Point(e.X - 1, e.Y + 1),
                new Point(e.X - 1, e.Y - 1),
            };

            foreach (var point in points)
            {
                if (point.X >= 0 && point.X < 8 && point.Y >= 0 && point.Y < 8 && a[point.X, point.Y])
                {
                    q.Enqueue(point);
                    a[point.X, point.Y] = false;
                    var p1 = point.X;
                    var p2 = point.Y;
                    var e1 = e.X;
                    var e2 = e.Y;
                    b[p1, p2] = new Point(e1, e2);
                }
            }
        }

        var result = new List<Point> { end, b[end.X, end.Y] };

        while (true)
        {
            var value = result[result.Count - 1];
            if (value.Equals(start)) break;
            var x = value.X;
            var y = value.Y;
            var v = b[x, y];
            result.Add(v);
        }

        result.ToArray();
        result.Reverse();
        result.RemoveAt(0);

        return result;
    }

    protected List<Point> GetUnitsAsPoints(BattleInfo info, params GameObject[] exclude)
    {
        var result = new List<Point>();
        foreach (GameObject cell in info.UnitsPositions.Keys.Where(x => !exclude.Contains(x)))
        {
            Point p = GameObjectToPoint(cell);
            result.Add(p);
        }

        return result;
    }

    protected Point GameObjectToPoint(GameObject cell)
    {
        // Парсим имя клетки формата 4_7 в точку Point(4,7)
        return new Point(int.Parse(cell.name.Split('_')[0]), int.Parse(cell.name.Split('_')[1]));
    }
}
