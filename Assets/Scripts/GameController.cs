using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// TODOs
// Display game over
// Add restart button
// Add title and name
// Sound effects
// Dark mode (toggle)

public class GameController : MonoBehaviour
{
    private const int COLS = 4;
    private const int ROWS = 4;

    public Number numberPrefab;

    Number[,] numbers;
    List<Number> numbersMergedIntoOtherToDestroy;
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
        numbers = new Number[ROWS, COLS];
        numbersMergedIntoOtherToDestroy = new();
        mergedNumbersToUpdate = new();
        InstantiateInitialNumbers();
    }

    void InstantiateInitialNumbers()
    {
        var position1 = getRandomEmptyTile();
        Position position2;
        do
        {
            position2 = getRandomEmptyTile();
        } while (position1 == position2);

        var numberA = Instantiate(numberPrefab, new Vector3(position1.x, -position1.y, 1), Quaternion.identity);
        var numberB = Instantiate(numberPrefab, new Vector3(position2.x, -position2.y, 1), Quaternion.identity);

        numbers[position1.row, position1.col] = numberA;
        numbers[position2.row, position2.col] = numberB;
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isInputPaused) { return; }

        var input = context.ReadValue<Vector2>();
        if (input.y == 1)
        {
            ExecuteTurn(Direction.Up);
        }
        else if (input.y == -1)
        {
            ExecuteTurn(Direction.Down);
        }
        else if (input.x == 1)
        {
            ExecuteTurn(Direction.Right);
        }
        else if (input.x == -1)
        {
            ExecuteTurn(Direction.Left);
        }
    }

    void ExecuteTurn(Direction direction)
    {
        isInputPaused = true;
        var movements = Move(direction);

        // If there are no valid movements for that direction, we end the turn
        if (movements.Count == 0)
        {
            isInputPaused = false;
            return;
        }

        var sequence = PerformTweens(movements);
        sequence.OnComplete(() => {
            UpdateMergedNumbers();
            DestroyMergedNumbers();
            SpawnNewNumber();
            CheckGameOver();
            isInputPaused = false;
        });
    }

    List<TileMovement> Move(Direction direction)
    {
        List<TileMovement> movements = new();
        bool[,] merged = new bool[ROWS, COLS];

        // Directional vectors
        Vector2Int directionalVector = direction switch
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
                int row = currentRow;
                int col = currentCol;
                if (numbers[row, col] == null) { continue; }

                Number currentNumber = numbers[row, col];
                int value = currentNumber.value;
                int targetRow = row;
                int targetCol = col;
                bool skip = false;

                // Slide in the direction until blocked
                while (true)
                {
                    int nextRow = targetRow + directionalVector.y;
                    int nextCol = targetCol + directionalVector.x;

                    if (nextRow < 0 || nextRow >= ROWS || nextCol < 0 || nextCol >= COLS)
                    {
                        break;
                    }

                    // If the next tile is empty (null), the tile can move, 
                    // so we increase the update the targetRow and targetCol values
                    if (numbers[nextRow, nextCol] == null)
                    {
                        targetRow = nextRow;
                        targetCol = nextCol;
                    }
                    // If the next tile in range has the same value and we haven't marked it as merged, then we perform a merge
                    // and store the movement for the tweening.
                    else if (numbers[nextRow, nextCol].value == value && !merged[nextRow, nextCol])
                    {
                        var mergedNumber = numbers[row, col];
                        numbers[nextRow, nextCol].value *= 2;
                        numbersMergedIntoOtherToDestroy.Add(numbers[row, col]);
                        mergedNumbersToUpdate.Add(numbers[nextRow, nextCol]);
                        numbers[row, col] = null;
                        merged[nextRow, nextCol] = true;
                        movements.Add(new TileMovement(mergedNumber,new Vector2Int(nextCol, nextRow)));
                        skip = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }

                // If we didn't merged (skipped) and we moved to another tile, then store the movement for the tweening
                if (!skip && (targetRow != row || targetCol != col))
                {
                    numbers[targetRow, targetCol] = currentNumber;
                    numbers[row, col] = null;
                    movements.Add(new TileMovement(currentNumber, new Vector2Int(targetCol, targetRow)));
                }
            }
        }

        return movements;
    }

    Sequence PerformTweens(List<TileMovement> movements)
    {
        var sequence = Sequence.Create();
        for (int index = 0; index < movements.Count; index++)
        {
            var target = movements[index].numberToMove;
            var to = movements[index].to;

            sequence.Group(Tween.Position(
                target.transform,
                endValue: new Vector3(to.x, -to.y, 1),
                duration: 0.25f,
                ease: Ease.OutSine
            ));
        }
        return sequence;
    }

    void SpawnNewNumber()
    {
        var emptyTile = getRandomEmptyTile();
        var number = Instantiate(numberPrefab, new Vector3(emptyTile.x, -emptyTile.y, 1), Quaternion.identity);
        numbers[emptyTile.row, emptyTile.col] = number;
    }

    void CheckGameOver()
    {
        int count = 0;
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                if (numbers[row, col] != null)
                {
                    count++;
                }
            }
        }

        if (count < (ROWS * COLS))
        {
            return;
        }

        print("The board is full. Checking if there are any valid movements");

        // Checking rows first
        for (int row = 0; row < ROWS - 1; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                if (numbers[row, col].value == numbers[row + 1, col].value)
                {
                    print($"Found a possible movement between ({row}, {col}) and ({row + 1}, {col})");
                    return;
                }
            }
        }

        // Checking columns
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS - 1; col++)
            {
                if (numbers[row, col].value == numbers[row, col + 1].value)
                {
                    print($"Found a possible movement between ({row}, {col}) and ({row}, {col + 1})");
                    return;
                }
            }
        }

        print("No valid movements found. GAME OVER");
    }

    void DestroyMergedNumbers()
    {
        foreach (Number number in numbersMergedIntoOtherToDestroy)
        {
            Destroy(number.gameObject);
        }
        numbersMergedIntoOtherToDestroy.Clear();
    }

    void UpdateMergedNumbers()
    {
        foreach (Number number in mergedNumbersToUpdate)
        {
            number.UpdateTextAndColor();
            Sequence.Create()
                .Chain(Tween.Scale(number.transform, new Vector3(0.95f, 0.95f, 1f), duration: 0.15f, ease: Ease.OutQuad))
                .Chain(Tween.Scale(number.transform, new Vector3(0.85f, 0.85f, 1f), duration: 0.15f, ease: Ease.OutQuad));
        }
        mergedNumbersToUpdate.Clear();
    }

    Position getRandomEmptyTile()
    {
        int row;
        int col;

        do
        {
            row = UnityEngine.Random.Range(0, ROWS);
            col = UnityEngine.Random.Range(0, COLS);
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

struct TileMovement
{
    public Number numberToMove { get; private set; }
    public Vector2Int to { get; private set; }

    internal TileMovement(Number numberToMove, Vector2Int to)
    {
        this.numberToMove = numberToMove;
        this.to = to;
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}