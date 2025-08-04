using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public Number numberPrefab;

    Number[,] numbers;
    List<Number> mergedNumbersToDestroy;
    List<Number> mergedNumbersToUpdate;

    PlayerInput playerInput;

    bool isInputPaused;

    void Awake()
    {
        playerInput = new PlayerInput();
    }

    void OnEnable()
    {
        playerInput.PlayerController.Move.performed += OnMovePerformed;
        playerInput.PlayerController.Move.Enable();
    }

    void OnDisable()
    {
        playerInput.PlayerController.Move.performed -= OnMovePerformed;
        playerInput.PlayerController.Move.Disable();
    }

    void Start()
    {
        isInputPaused = false;
        numbers = new Number[4, 4];
        mergedNumbersToDestroy = new();
        mergedNumbersToUpdate = new();
        InstantiateInitialNumbers();
    }

    void InstantiateInitialNumbers()
    {
        // Instantiate 2 numbers in 2 random positions
        var position1 = getRandomPosition();
        Position position2;
        do
        {
            position2 = getRandomPosition();
        } while (position1 == position2);

        // TODO: Delete after testing
        print("Number A: " + position1);
        print("Number B: " + position2);
        print("--------------------------------");

        var square1 = Instantiate(numberPrefab, new Vector3(position1.x, -position1.y, 1), Quaternion.identity);
        square1.name = "Number A"; // TODO: Delete after testing
        var square2 = Instantiate(numberPrefab, new Vector3(position2.x, -position2.y, 1), Quaternion.identity);
        square2.name = "Number B"; // TODO: Delete after testing

        numbers[position1.row, position1.col] = square1;
        numbers[position2.row, position2.col] = square2;
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isInputPaused) { return; }

        var input = context.ReadValue<Vector2>();

        if (input.y == 1)
        {
            Move(Direction.Up);
        }
        else if (input.y == -1)
        {
            Move(Direction.Down);
        }
        else if (input.x == 1)
        {
            Move(Direction.Right);
        }
        else if (input.x == -1)
        {
            Move(Direction.Left);
        }
    }


    void Move(Direction direction)
    {
        print($"Moving {direction}");
        List<TileMove> movements = new();
        bool[,] merged = new bool[4, 4];

        // Directional vectors
        Vector2Int directionVector = direction switch
        {
            Direction.Up => new Vector2Int(0, -1),
            Direction.Down => new Vector2Int(0, 1),
            Direction.Left => new Vector2Int(-1, 0),
            Direction.Right => new Vector2Int(1, 0),
            _ => throw new ArgumentOutOfRangeException()
        };

        // Traversal order helpers
        int[] rowOrder = direction == Direction.Up ? new[] { 1, 2, 3 } :
                        direction == Direction.Down ? new[] { 2, 1, 0 } :
                        new[] { 0, 1, 2, 3 };

        int[] colOrder = direction == Direction.Left ? new[] { 1, 2, 3 } :
                        direction == Direction.Right ? new[] { 2, 1, 0 } :
                        new[] { 0, 1, 2, 3 };

        foreach (int currentRow in rowOrder)
        {
            foreach (int currentCol in colOrder)
            {
                int row = currentRow, col = currentCol;
                if (numbers[row, col] == null) continue;

                Number currentNumber = numbers[row, col];
                int value = currentNumber.value;
                int targetRow = row;
                int targetCol = col;
                bool skip = false;

                // Slide in the direction until blocked
                while (true)
                {
                    int nextRow = targetRow + directionVector.y;
                    int nextCol = targetCol + directionVector.x;

                    if (nextRow < 0 || nextRow >= 4 || nextCol < 0 || nextCol >= 4)
                    {
                        break;
                    }

                    if (numbers[nextRow, nextCol] == null)
                    {
                        targetRow = nextRow;
                        targetCol = nextCol;
                    }
                    else if (numbers[nextRow, nextCol].value == value && !merged[nextRow, nextCol])
                    {
                        var mergedNumber = numbers[row, col];
                        numbers[nextRow, nextCol].value *= 2;
                        mergedNumbersToDestroy.Add(numbers[row, col]);
                        mergedNumbersToUpdate.Add(numbers[nextRow, nextCol]);
                        numbers[row, col] = null;
                        merged[nextRow, nextCol] = true;

                        print($"{mergedNumber.name} is moving from ({col}, {row}) to ({nextCol}, {nextRow}). Merged = true");
                        movements.Add(new TileMove(
                            mergedNumber,
                            new Vector2Int(col, row),
                            new Vector2Int(nextCol, nextRow),
                            true
                        ));

                        skip = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }

                if (!skip && (targetRow != row || targetCol != col))
                {
                    numbers[targetRow, targetCol] = currentNumber;
                    numbers[row, col] = null;

                    print($"{currentNumber.name} is moving from ({col}, {row}) to ({targetCol}, {targetRow}). Merged = false");
                    movements.Add(new TileMove(
                        currentNumber,
                        new Vector2Int(col, row),
                        new Vector2Int(targetCol, targetRow),
                        false
                    ));
                }
            }
        }

        if (movements.Count == 0)
        {
            print($"No valid moves {direction.ToString().ToLower()}");
            return;
        }

        PerformTweens(movements);
    }

    void PerformTweens(List<TileMove> movements)
    {
        isInputPaused = true;

        print($"Performing tweening for {movements.Count} tiles");
        for (int index = 0; index < movements.Count; index++)
        {
            var target = movements[index].target;
            var from = movements[index].from;
            var to = movements[index].to;
            var merged = movements[index].merged;

            print($"Moving from: {from} to {to} for tile {target.name} in {target.transform.position}. Merged = {merged}");
            var tween = Tween.Position(
                target.transform,
                endValue: new Vector3(to.x, -to.y, 1),
                duration: 0.25f,
                ease: Ease.OutSine
            );

            if (merged && index == movements.Count - 1)
            {
                tween.OnComplete(() =>
                {
                    print("Last tween completed");
                    UpdateMergedNumbers();
                    DestroyMergedNumbers();
                    SpawnNewNumber();
                    isInputPaused = false;
                });
            }
            else if (index == movements.Count - 1)
            {
                tween.OnComplete(() =>
                {
                    print("Last tween completed");
                    UpdateMergedNumbers();
                    DestroyMergedNumbers();
                    SpawnNewNumber();
                    isInputPaused = false;
                });
            }
        }
    }

    void SpawnNewNumber()
    {
        var emptyTile = getRandomEmptyTile();
        var number = Instantiate(numberPrefab, new Vector3(emptyTile.x, -emptyTile.y, 1), Quaternion.identity);
        numbers[emptyTile.row, emptyTile.col] = number;
        print($"Spawning new number at ({emptyTile.col}, {emptyTile.row})");

        int count = 0;
        List<Tuple<int, int>> occupiedPositions = new();
        for (int i = 0; i < numbers.GetLength(0); i++)
        {
            for (int j = 0; j < numbers.GetLength(1); j++)
            {
                if (numbers[i, j] != null)
                {
                    count++;
                    occupiedPositions.Add(new (i, j));
                }
            }
        }

        print($"There are {count} numbers in the grid");
        print($"{string.Join(" | ", occupiedPositions)}");
        print("----------------------\n");
    }

    void DestroyMergedNumbers()
    {
        print($"Destroying {mergedNumbersToDestroy.Count} merged numbers");
        foreach (Number number in mergedNumbersToDestroy)
        {
            print($"Destroying {number.name} in position {number.transform.position}");
            Destroy(number.gameObject);
        }
        mergedNumbersToDestroy.Clear();
        print("Destroyed all merged numbers");
    }

    void UpdateMergedNumbers()
    {
        print($"Updating {mergedNumbersToUpdate.Count} merged numbers");
        foreach (Number number in mergedNumbersToUpdate)
        {
            print($"Updating {number.name} in position {number.transform.position}");
            number.UpdateTextAndColor();
            Sequence.Create()
                .Chain(Tween.Scale(number.transform, new Vector3(0.95f, 0.95f, 1f), duration: 0.15f, ease: Ease.OutQuad))
                .Chain(Tween.Scale(number.transform, new Vector3(0.85f, 0.85f, 1f), duration: 0.15f, ease: Ease.OutQuad));
        }
        mergedNumbersToUpdate.Clear();
        print($"Updated all {mergedNumbersToUpdate.Count} merged numbers");        
    }

    Position getRandomPosition()
    {
        int x = UnityEngine.Random.Range(0, 4);
        int y = UnityEngine.Random.Range(0, 4);
        return new Position(x, y);
    }

    Position getRandomEmptyTile()
    {
        int row;
        int col;

        do
        {
            row = UnityEngine.Random.Range(0, 4);
            col = UnityEngine.Random.Range(0, 4);
        } while (numbers[row, col] != null);

        return Position.fromRowAndCol(row, col);
    }
}

struct Position
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

    public static Position fromRowAndCol(int row, int col)
    {
        return new Position(col, row);
    }

    public static bool operator ==(Position a, Position b)
    {
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

struct TileMove
{
    public Number target { get; private set; }
    public Vector2Int from { get; private set; }
    public Vector2Int to { get; private set; }
    public bool merged { get; private set; }

    internal TileMove(Number target, Vector2Int from, Vector2Int to, bool merged)
    {
        this.target = target;
        this.from = from;
        this.to = to;
        this.merged = merged;
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}