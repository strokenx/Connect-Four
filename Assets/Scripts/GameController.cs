using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    // Enumerated constants used to define the state of game pieces
    enum Piece
    {
        Empty = 0,
        FirstPlayer = 1,
        SecondPlayer = 2
    }

    // Definition of variables
    private int numRows = 10;
    private int numColumns = 10;
    private int numPiecesToWin = 4;
    private float dropTime = 2f;
    private float timeLeft;
    private bool btnPlayAgainTouching = false;
    private Color btnPlayAgainOrigColor;
    private Color btnPlayAgainHoverColor = new Color(255, 143, 4);
    private bool isPlayer1Turn = true;
    private bool isLoading = true;
    private bool isDropping = false;
    private bool mouseButtonPressed = false;
    private bool gameOver = false;
    private bool isCheckingForWinner = false;

    [Range(10, 60)]
    public int turnDuration = 30;

    // Array of the game field
    private int[,] field;

    public GameObject player1Piece;
    public GameObject player2Piece;
    public GameObject pieceField;
    public GameObject playersWinText;
    public GameObject playersTurnText;
    public GameObject timerText;
    public GameObject btnPlayAgain;
    private GameObject gameObjectField;

    // Temporary gameObject, holds the piece at mouse position until the mouse has clicked
    private GameObject gameObjectTurn;

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        CreateField();
        btnPlayAgainOrigColor = btnPlayAgain.GetComponent<Renderer>().material.color;
        timeLeft = turnDuration;
    }

    /// <summary>
    /// Function that looks if there is a playing field and then creates one.
    /// </summary>
    private void CreateField()
    {
        playersWinText.SetActive(false);
        timerText.SetActive(false);
        playersTurnText.SetActive(true);
        btnPlayAgain.SetActive(false);

        isLoading = true;

        gameObjectField = GameObject.Find("Field");
        if (gameObjectField != null)
        {
            DestroyImmediate(gameObjectField);
        }
        gameObjectField = new GameObject("Field");

        // Create an empty field and instantiate the cells
        field = new int[numColumns, numRows];
        for (int x = 0; x < numColumns; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                field[x, y] = (int)Piece.Empty;
                GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
                g.transform.parent = gameObjectField.transform;
            }
        }

        isLoading = false;
        gameOver = false;

        // The camera and the respective dialog boxes are centered
        Camera.main.orthographic = true;

        Camera.main.transform.position = new Vector3(
            (numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f), Camera.main.transform.position.z);

        Camera.main.orthographicSize = (numRows + 5) / 2f;

        playersWinText.transform.position = new Vector3(
            (numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) + 1, playersWinText.transform.position.z);

        timerText.transform.position = new Vector3(
            (numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) + 1, timerText.transform.position.z);

        playersTurnText.transform.position = new Vector3(
            (numColumns - 1) / 2.0f, -((numRows - 12) / 2.0f) + 1, playersTurnText.transform.position.z);

        btnPlayAgain.transform.position = new Vector3(
            (numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) - 1, btnPlayAgain.transform.position.z);

    }

    /// <summary>
    /// Spawns a piece at mouse position above the first row
    /// </summary>
    /// <returns>The piece.</returns>
    private GameObject SpawnPiece()
    {
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        GameObject piece = Instantiate(
                isPlayer1Turn ? player1Piece : player2Piece, // Is players turn? then spawn FirstPlayer piece, else spawn SecondPlayer piece
                new Vector3(
                Mathf.Clamp(spawnPos.x, 0, numColumns - 1),
                gameObjectField.transform.position.y + 1, 0), // Spawn it above the first row
                Quaternion.identity) as GameObject;

        return piece;
    }

    /// <summary>
    /// Function that checks if the play again button is pressed.
    /// </summary>
    private void UpdatePlayAgainButton()
    {
        RaycastHit hit;
        //Ray shooting out of the camera from where the mouse is
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit) && hit.collider.name == btnPlayAgain.name)
        {
            btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
            //Check if the left mouse has been pressed down this frame
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
            {
                btnPlayAgainTouching = true;

                //CreateField();
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
        else
        {
            btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
        }

        if (Input.touchCount == 0)
        {
            btnPlayAgainTouching = false;
        }
    }

    /// <summary>
    /// Function that fills the game cell when the player clicks, also changes the player's turn according to the time limit
    /// </summary>
    private void FillGameCell()
    {
        timeLeft -= Time.deltaTime;

        if (gameObjectTurn == null)
        {
            gameObjectTurn = SpawnPiece();
        }
        else
        {
            // Update the objects position
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            gameObjectTurn.transform.position = new Vector3(
                Mathf.Clamp(pos.x, 0, numColumns - 1),
                gameObjectField.transform.position.y + 1, 0);

            // Click the left mouse button to drop the piece into the selected column
            if (Input.GetMouseButtonDown(0) && !mouseButtonPressed && !isDropping)
            {
                mouseButtonPressed = true;

                StartCoroutine(dropPiece(gameObjectTurn));
                timerText.SetActive(false);
                timeLeft = turnDuration;
            }
            else
            {
                mouseButtonPressed = false;
            }
        }
        if (timeLeft <= 3)
        {
            timerText.SetActive(true);
            timerText.GetComponent<TextMesh>().text = "" + timeLeft.ToString("f0");
            if (timeLeft < 0)
            {
                isPlayer1Turn = !isPlayer1Turn;
                DestroyImmediate(gameObjectTurn);
                gameObjectTurn = SpawnPiece();
                timerText.SetActive(false);
                timeLeft = turnDuration;
            }
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (isLoading)
        {
            return;
        }

        if (isCheckingForWinner)
        {
            return;
        }

        if (gameOver)
        {
            playersWinText.SetActive(true);
            playersTurnText.SetActive(false);
            btnPlayAgain.SetActive(true);

            UpdatePlayAgainButton();

            return;
        }

        playersTurnText.GetComponent<TextMesh>().text = isPlayer1Turn ? "Player 1 Turn" : "Player 2 Turn";
        FillGameCell();
    }

    /// <summary>
    /// This function searches for a empty cell and lets 
    /// the object fall down into this cell
    /// </summary>
    /// <param name="gObject">Game Object piece.</param>
    private IEnumerator dropPiece(GameObject gObject)
    {
        isDropping = true;

        Vector3 startPosition = gObject.transform.position;
        Vector3 endPosition = new Vector3();

        // Round to a grid cell
        int x = Mathf.RoundToInt(startPosition.x);
        startPosition = new Vector3(x, startPosition.y, startPosition.z);

        // Check if there is a free cell in the selected column
        bool foundFreeCell = false;
        for (int i = numRows - 1; i >= 0; i--)
        {
            if (field[x, i] == 0)
            {
                foundFreeCell = true;
                field[x, i] = isPlayer1Turn ? (int)Piece.FirstPlayer : (int)Piece.SecondPlayer;
                endPosition = new Vector3(x, i * -1, startPosition.z);

                break;
            }
        }

        if (foundFreeCell)
        {
            // Instantiate a new Piece, disable the temporary one
            GameObject g = Instantiate(gObject) as GameObject;
            gameObjectTurn.GetComponent<Renderer>().enabled = false;

            float distance = Vector3.Distance(startPosition, endPosition);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * dropTime * ((numRows - distance) + 1);

                g.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                yield return null;
            }

            g.transform.parent = gameObjectField.transform;

            // Remove the temporary gameObject
            DestroyImmediate(gameObjectTurn);

            // Run coroutine to check if someone has won
            StartCoroutine(Won());

            // Wait until winning check is done
            while (isCheckingForWinner)
            {
                yield return null;
            }

            isPlayer1Turn = !isPlayer1Turn;
        }

        isDropping = false;

        yield return 0;
    }

    /// <summary>
    /// Function that checks the winner
    /// </summary>
    private IEnumerator Won()
    {
        isCheckingForWinner = true;

        for (int x = 0; x < numColumns; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                // Get the Laymask to Raycast against, if it is Players turn only include
                // Layermask FirstPlayer otherwise Layermask SecondPlayer
                int layermask = isPlayer1Turn ? (1 << 8) : (1 << 9);

                // If its Players turn ignore SecondPlayer as Starting piece and vise versa
                if (field[x, y] != (isPlayer1Turn ? (int)Piece.FirstPlayer : (int)Piece.SecondPlayer))
                {
                    continue;
                }

                // shoot a ray of length 'numPiecesToWin - 1' to the right to test horizontally
                RaycastHit[] hitsHorz = Physics.RaycastAll(
                    new Vector3(x, y * -1, 0),
                    Vector3.right,
                    numPiecesToWin - 1,
                    layermask);

                // return true (won) if enough hits
                if (hitsHorz.Length == numPiecesToWin - 1)
                {
                    gameOver = true;
                    break;
                }

                // shoot a ray up to test vertically
                RaycastHit[] hitsVert = Physics.RaycastAll(
                    new Vector3(x, y * -1, 0),
                    Vector3.up,
                    numPiecesToWin - 1,
                    layermask);

                if (hitsVert.Length == numPiecesToWin - 1)
                {
                    gameOver = true;
                    break;
                }

                // calculate the length of the ray to shoot diagonally
                float length = Vector2.Distance(new Vector2(0, 0), new Vector2(numPiecesToWin - 1, numPiecesToWin - 1));

                RaycastHit[] hitsDiaLeft = Physics.RaycastAll(
                    new Vector3(x, y * -1, 0),
                    new Vector3(-1, 1),
                    length,
                    layermask);

                if (hitsDiaLeft.Length == numPiecesToWin - 1)
                {
                    gameOver = true;
                    break;
                }

                RaycastHit[] hitsDiaRight = Physics.RaycastAll(
                    new Vector3(x, y * -1, 0),
                    new Vector3(1, 1),
                    length,
                    layermask);

                if (hitsDiaRight.Length == numPiecesToWin - 1)
                {
                    gameOver = true;
                    break;
                }

                yield return null;
            }

            yield return null;
        }

        // if Game Over update the winning text to show who has won
        if (gameOver == true)
        {
            if (isPlayer1Turn)
            {
                playersWinText.GetComponent<TextMesh>().text = "Player 1 Wins!";
            }
            else
            {
                playersWinText.GetComponent<TextMesh>().text = "Player 2 Wins!";
            }
        }
        else
        {
            // check if there are any empty cells left, if not set game over and update text to show a draw
            if (!FieldContainsEmptyCell())
            {
                gameOver = true;
                playersWinText.GetComponent<TextMesh>().text = "Draw!";
            }
        }

        isCheckingForWinner = false;

        yield return 0;
    }

    /// <summary>
    /// check if the field contains an empty cell
    /// </summary>
    /// <returns><c>true</c>, if it contains empty cell, <c>false</c> otherwise.</returns>
    private bool FieldContainsEmptyCell()
    {
        for (int x = 0; x < numColumns; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                if (field[x, y] == (int)Piece.Empty)
                    return true;
            }
        }
        return false;
    }
}