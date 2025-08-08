using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] SpriteRenderer gridBackground;
    [SerializeField] GameObject row0;
    [SerializeField] GameObject row1;
    [SerializeField] GameObject row2;
    [SerializeField] GameObject row3;
    [SerializeField] Number numberPrefab;

    [Header("UI")]
    [SerializeField] GameObject gameOverBanner;
    [SerializeField] GameObject wonBanner;

    const int COLS = 4;
    const int ROWS = 4;

    List<SpriteRenderer> tilesSpriteRenderers;

    Number[,] numbers;
    List<Number> numbersMergedIntoOtherToDestroy;
    List<Number> mergedNumbersToUpdate;

    PlayerInput playerInput;

    bool isInputEnabled;
    bool gameOver;
    bool won;

    // Colors
    Color cameraLightBackground = new Color(100f / 255f, 150f / 255f, 200f / 255f);
    Color cameraDarkBackground = new Color(40f / 255f, 40f / 255f, 40f / 255f);

    Color gridLightBackground = new Color(33f / 255f, 33f / 255f, 33f / 255f);
    Color gridDarkBackground = new Color(220f / 255f, 220f / 255f, 220f / 255f);

    Color tilesLightColor = new Color(220f / 255f, 220f / 255f, 220f / 255f);
    Color tilesDarkColor = new Color(110f / 255f, 110f / 255f, 110f / 255f);

    void Awake()
    {
        playerInput = new PlayerInput();
    }

    void OnEnable()
    {
        playerInput.PlayerController.Move.performed += OnMovePerformed;
        playerInput.PlayerController.Accept.performed += OnAccept;
        playerInput.PlayerController.Enable();

        GameSettings.instance.colorModeChanged += UpdateColors;
    }

    void OnDisable()
    {
        playerInput.PlayerController.Move.performed -= OnMovePerformed;
        playerInput.PlayerController.Accept.performed -= OnAccept;
        playerInput.PlayerController.Disable();

        GameSettings.instance.colorModeChanged -= UpdateColors;
    }

    void Start()
    {
        tilesSpriteRenderers = new();
        tilesSpriteRenderers.AddRange(row0.GetComponentsInChildren<SpriteRenderer>());
        tilesSpriteRenderers.AddRange(row1.GetComponentsInChildren<SpriteRenderer>());
        tilesSpriteRenderers.AddRange(row2.GetComponentsInChildren<SpriteRenderer>());
        tilesSpriteRenderers.AddRange(row3.GetComponentsInChildren<SpriteRenderer>());

        gameOverBanner.SetActive(false);
        wonBanner.SetActive(false);

        isInputEnabled = true;
        gameOver = false;
        won = false;
        numbers = new Number[ROWS, COLS];
        numbersMergedIntoOtherToDestroy = new();
        mergedNumbersToUpdate = new();
        InstantiateInitialNumbers();
    }

    public void Restart()
    {
        foreach (Number number in numbers)
        {
            if (number != null)
            {
                Destroy(number.gameObject);
            }            
        }
        Start();
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

        numberA.ToggleColors();
        numberB.ToggleColors();
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!isInputEnabled) { return; }

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

    void OnAccept(InputAction.CallbackContext context)
    {
        if (gameOver == true)
        {
            Restart();
        }

        if (won == true)
        {
            wonBanner.SetActive(false);
            isInputEnabled = true;
        }
    } 

    void ExecuteTurn(Direction direction)
    {
        isInputEnabled = false;
        var movements = Move(direction);

        // If there are no valid movements for that direction, we end the turn
        if (movements.Count == 0)
        {
            isInputEnabled = true;
            return;
        }

        var sequence = PerformTweens(movements, direction);
        sequence.OnComplete(() =>
        {
            UpdateMergedNumbers();
            DestroyMergedNumbers();
            SpawnNewNumber();
            CheckGameOver();
            CheckWin();
            isInputEnabled = true;
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
                        movements.Add(new TileMovement(mergedNumber, new Vector2Int(nextCol, nextRow)));
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

    Sequence PerformTweens(List<TileMovement> movements, Direction direction)
    {        
        Vector3 directionVector = direction switch
        {
            Direction.Up => Vector3.up,
            Direction.Down => Vector3.down,
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            _ => throw new ArgumentOutOfRangeException()
        };

        var sequence = Sequence.Create();

        var movementSequence = Sequence.Create();
        for (int index = 0; index < movements.Count; index++)
        {
            var target = movements[index].numberToMove;
            var to = movements[index].to;

            movementSequence.Group(Tween.Position(
                target.transform,
                endValue: new Vector3(to.x, -to.y, 1),
                duration: 0.15f,
                ease: Ease.OutSine
            ));
        }

        var bounceSequence = Sequence.Create();
        for (int index = 0; index < movements.Count; index++)
        {
            if (numbersMergedIntoOtherToDestroy.Contains(movements[index].numberToMove))
            {
                continue;
            }

            if (mergedNumbersToUpdate.Contains(movements[index].numberToMove))
            {
                continue;
            }

            var target = movements[index].numberToMove;
            bounceSequence.Group(
                Tween.PunchLocalPosition(
                    target.transform,
                    strength: directionVector * 0.20f,
                    duration: 0.08f,
                    frequency: 10f
                )
            );
        }

        sequence.Chain(movementSequence).Chain(bounceSequence);

        return sequence;
    }

    void SpawnNewNumber()
    {
        var emptyTile = getRandomEmptyTile();
        var number = Instantiate(numberPrefab, new Vector3(emptyTile.x, -emptyTile.y, 1), Quaternion.identity);
        numbers[emptyTile.row, emptyTile.col] = number;
        number.ToggleColors();
    }

    void CheckWin()
    {
        if (won) { return; }

        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                if (numbers[row, col] != null)
                {
                    if (numbers[row, col].value == 2048)
                    {
                        won = true;
                        wonBanner.SetActive(true);
                        isInputEnabled = false;
                    }
                }
            }
        }
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


        // Checking rows first
        for (int row = 0; row < ROWS - 1; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                if (numbers[row, col].value == numbers[row + 1, col].value)
                {
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
                    return;
                }
            }
        }

        isInputEnabled = false;
        gameOverBanner.SetActive(true);
        gameOver = true;
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
                .Chain(Tween.Scale(number.transform, new Vector3(0.95f, 0.95f, 1f), duration: 0.10f, ease: Ease.OutQuad))
                .Chain(Tween.Scale(number.transform, new Vector3(0.85f, 0.85f, 1f), duration: 0.10f, ease: Ease.OutQuad));
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

    void UpdateColors()
    {
        if (GameSettings.instance.colorMode == ColorMode.Light)
        {
            Camera.main.backgroundColor = cameraLightBackground;
            gridBackground.color = gridLightBackground;
            foreach (SpriteRenderer spriteRenderer in tilesSpriteRenderers)
            {
                spriteRenderer.color = tilesLightColor;
            }
        }
        else
        {
            Camera.main.backgroundColor = cameraDarkBackground;
            gridBackground.color = gridDarkBackground;
            foreach (SpriteRenderer spriteRenderer in tilesSpriteRenderers)
            {
                spriteRenderer.color = tilesDarkColor;
            }
        }

        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                if (numbers[row, col] != null)
                {
                    numbers[row, col].ToggleColors();
                }
            }
        }
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