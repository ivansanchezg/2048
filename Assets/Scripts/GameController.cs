using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public Number numberPrefab;

    Number[,] numbers;

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

        // Instantiate 2 numbers in 2 random positions
        var position1 = getRandomPosition();
        Position position2;
        do
        {
            position2 = getRandomPosition();
        } while (position1 == position2);

        print("Position 1: " + position1);
        // print("Position 2: " + position2);

        var square1 = Instantiate(numberPrefab, new Vector3(position1.x, -position1.y, 1), Quaternion.identity);
        // var square2 = Instantiate(numberPrefab, new Vector3(position2.x, -position2.y, 1), Quaternion.identity);

        numbers[position1.row, position1.col] = square1;
        // numbers[position2.row, position2.col] = square2;
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isInputPaused) { return; }

        var input = context.ReadValue<Vector2>();
        print($"input: {input}");

        if (input.y == 1)
        {
            MoveUp();
        }
        else if (input.y == -1)
        {
            MoveDown();
        }
        else if (input.x == 1)
        {
            MoveRight();
        }
        else if (input.x == -1)
        {
            MoveLeft();
        }
    }

    void MoveUp()
    {
        print("Moving up");
        List<TileMove> movements = new();
        bool[,] merged = new bool[4, 4];

        for (int col = 0; col < 4; col++)
        {
            for (int row = 1; row < 4; row++)
            {
                if (numbers[row, col] == null) { continue; }

                Number currentNumber = numbers[row, col];

                int value = numbers[row, col].value;
                int targetRow = row;

                while (targetRow > 0 && numbers[targetRow - 1, col] == null)
                {
                    targetRow--;
                }

                if (targetRow > 0 && numbers[targetRow - 1, col].value == value && !merged[targetRow - 1, col])
                {
                    numbers[targetRow - 1, col].value *= 2;
                    numbers[row, col] = null;
                    merged[targetRow - 1, col] = true;
                    movements.Add(new TileMove(
                        numbers[targetRow - 1, col],
                        new Vector2Int(row, col),
                        new Vector2Int(targetRow - 1, col),
                        true
                    ));
                }
                else if (targetRow != row)
                {
                    numbers[targetRow, col] = currentNumber;
                    numbers[row, col] = null;
                    movements.Add(
                        new TileMove(currentNumber, new Vector2Int(col, row), new Vector2Int(col, targetRow), false)
                    );
                }
            }
        }

        // Execute tweening
        if (movements.Count == 0)
        {
            print("No valid moves upward");
            return;
        }

        PerformTweens(movements);
    }

    void MoveDown()
    {
        print("Move down");
        List<TileMove> movements = new();
        bool[,] merged = new bool[4, 4];

        for (int col = 0; col < 4; col++)
        {
            for (int row = 2; row >= 0; row--)
            {
                if (numbers[row, col] == null) { continue; }

                Number currentNumber = numbers[row, col];

                int value = numbers[row, col].value;
                int targetRow = row;

                while (targetRow < 3 && numbers[targetRow + 1, col] == null)
                {
                    targetRow++;
                }

                print("targetRow:" + targetRow);

                if (targetRow < 3 && numbers[targetRow + 1, col].value == value && !merged[targetRow + 1, col])
                {
                    numbers[targetRow + 1, col].value *= 2;
                    numbers[row, col] = null;
                    merged[targetRow + 1, col] = true;
                    movements.Add(new TileMove(
                        numbers[targetRow + 1, col],
                        new Vector2Int(row, col),
                        new Vector2Int(targetRow + 1, col),
                        true
                    ));
                }
                else if (targetRow != row)
                {
                    numbers[targetRow, col] = currentNumber;
                    numbers[row, col] = null;
                    movements.Add(
                        new TileMove(currentNumber, new Vector2Int(col, row), new Vector2Int(col, targetRow), false)
                    );
                }
            }
        }

        // Execute tweening
        if (movements.Count == 0)
        {
            print("No valid moves downwards");
            return;
        }

        PerformTweens(movements);
    }

    void MoveLeft()
    {
        print("Moving left");
        List<TileMove> movements = new();
        bool[,] merged = new bool[4, 4];

        for (int row = 0; row < 4; row++)
        {
            for (int col = 1; col < 4; col++)
            {
                if (numbers[row, col] == null) { continue; }

                Number currentNumber = numbers[row, col];
                int value = currentNumber.value;
                int targetCol = col;

                // Find the furthest empty spot to the left
                while (targetCol > 0 && numbers[row, targetCol - 1] == null)
                {
                    targetCol--;
                }

                // Merge if same value and not already merged
                if (targetCol > 0 && numbers[row, targetCol - 1].value == value && !merged[row, targetCol - 1])
                {
                    numbers[row, targetCol - 1].value *= 2;
                    numbers[row, col] = null;
                    merged[row, targetCol - 1] = true;

                    movements.Add(new TileMove(
                        numbers[row, targetCol - 1],
                        new Vector2Int(col, row),
                        new Vector2Int(targetCol - 1, row),
                        true
                    ));
                }
                else if (targetCol != col)
                {
                    numbers[row, targetCol] = currentNumber;
                    numbers[row, col] = null;

                    movements.Add(new TileMove(
                        currentNumber,
                        new Vector2Int(col, row),
                        new Vector2Int(targetCol, row),
                        false
                    ));
                }
            }
        }

        if (movements.Count == 0)
        {
            print("No valid moves leftward");
            return;
        }

        PerformTweens(movements);
    }

    void MoveRight()
    {
        print("Moving right");
        List<TileMove> movements = new();
        bool[,] merged = new bool[4, 4];

        for (int row = 0; row < 4; row++)
        {
            for (int col = 2; col >= 0; col--) // Start from second-to-last column
            {
                if (numbers[row, col] == null) { continue; }

                Number currentNumber = numbers[row, col];
                int value = currentNumber.value;
                int targetCol = col;

                // Find the furthest empty spot to the right
                while (targetCol < 3 && numbers[row, targetCol + 1] == null)
                {
                    targetCol++;
                }

                // Merge if same value and not already merged
                if (targetCol < 3 && numbers[row, targetCol + 1].value == value && !merged[row, targetCol + 1])
                {
                    numbers[row, targetCol + 1].value *= 2;
                    numbers[row, col] = null;
                    merged[row, targetCol + 1] = true;

                    movements.Add(new TileMove(
                        numbers[row, targetCol + 1],
                        new Vector2Int(col, row),
                        new Vector2Int(targetCol + 1, row),
                        true
                    ));
                }
                else if (targetCol != col)
                {
                    numbers[row, targetCol] = currentNumber;
                    numbers[row, col] = null;

                    movements.Add(new TileMove(
                        currentNumber,
                        new Vector2Int(col, row),
                        new Vector2Int(targetCol, row),
                        false
                    ));
                }
            }
        }

        if (movements.Count == 0)
        {
            print("No valid moves rightward");
            return;
        }

        PerformTweens(movements);
    }

    void PerformTweens(List<TileMove> movements)
    {
        isInputPaused = true;
        Tween lastTween;

        print("Performing tweening");
        for (int index = 0; index < movements.Count; index++)
        {
            var target = movements[index].target;
            var from = movements[index].from;
            var to = movements[index].to;

            print($"Moving from: {from} to {to} for tile in {target.transform.position}");

            lastTween = Tween.Position(
                target.transform,
                endValue: new Vector3(to.x, -to.y, 1),
                duration: 0.25f,
                ease: Ease.OutSine
            );

            if (index == movements.Count - 1)
            {
                lastTween.OnComplete(() => { isInputPaused = false; });
            }
        }
        print("Completed tweening");
    }

    Position getRandomPosition()
    {
        int x = UnityEngine.Random.Range(0, 4);
        int y = UnityEngine.Random.Range(0, 4);
        return new Position(x, y);
    }

    Position getRandomEmptyTile()
    {
        int x;
        int y;

        do
        {
            x = UnityEngine.Random.Range(0, 4);
            y = UnityEngine.Random.Range(0, 4);
        } while (numbers[x, y] != null);

        return new Position(x, y);
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