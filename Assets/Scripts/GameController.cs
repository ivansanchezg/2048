using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Number numberPrefab;

    Number[,] numbers;

    void Start()
    {
        numbers = new Number[4, 4];

        // Instantiate 2 numbers in 2 random positions
        var position1 = getRandomPosition();
        Position position2;
        do
        {
            position2 = getRandomPosition();
        } while (position1 == position2);

        print("Position 1: " + position1);
        print("Position 2: " + position2);

        var square1 = Instantiate(numberPrefab, new Vector3(position1.x, -position1.y, 1), Quaternion.identity);
        var square2 = Instantiate(numberPrefab, new Vector3(position2.x, -position2.y, 1), Quaternion.identity);

        numbers[position1.row, position1.col] = square1;
        numbers[position2.row, position2.col] = square2;
    }

    Position getRandomPosition()
    {
        int x = UnityEngine.Random.Range(0, 4);
        int y = UnityEngine.Random.Range(0, 4);
        return new Position(x, y);
    }
}

class Position
{
    public int row { get; private set; }
    public int col { get; private set; }
    public int x { get; private set; }
    public int y { get; private set; }

    internal Position(int x, int y)
    {
        this.x = x;
        this.y = y;
        col = x;
        row = y;
    }

    public static bool operator ==(Position a, Position b)
    {
        if (ReferenceEquals(a, b)) { return true; }
        if (a is null || b is null) { return false; }
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Position a, Position b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return obj is Position other && this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }

    public override string ToString()
    {
        return $"Position(x={x}, y={y}, row={row}, col={col})";
    }
}