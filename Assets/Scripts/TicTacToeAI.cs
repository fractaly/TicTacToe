using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState{none=0, cross=1, circle=-1}

[System.Serializable] public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{
    int _aiLevel;
    
    TicTacToeState[,] boardState = new TicTacToeState[3,3];
    
    List<(int, int)> availableSpots = new List<(int, int)>(){(0,0)};
    
    [SerializeField] private TicTacToeState playerState = TicTacToeState.cross;
    TicTacToeState aiState = TicTacToeState.circle;

    [SerializeField] private GameObject _xPrefab;
    [SerializeField] private GameObject _oPrefab;

    public UnityEvent onGameStarted;

    //Call this event with the player number to denote the winner
    public WinnerEvent onPlayerWin;

    ClickTrigger[,] _triggers;

    private void Awake()
    {
        if(onPlayerWin == null){
            onPlayerWin = new WinnerEvent();
        }
    }

    public void StartAI(int AILevel){
        _aiLevel = AILevel;
        StartGame();
    }

    public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
    {
        _triggers[myCoordX, myCoordY] = clickTrigger;
    }

    private void StartGame()
    {
        _triggers = new ClickTrigger[3,3];
        onGameStarted.Invoke();
    }
    
    // Update game with next player move and then trigger selection of next AI move
    public void PlayerSelects(int coordX, int coordY){
        boardState[coordX,coordY]=playerState;
        SetVisual(coordX, coordY, playerState);
        _triggers[coordX,coordY].SetInputEndabled(false);
        
        int? stateInfo=checkWinner();
        if(stateInfo==null){
            StartCoroutine(WaitCallAiSelects());
        }
    }
    
    // Delay call to AiSelects to make the game feel nicer
    public  IEnumerator WaitCallAiSelects()
    {
        yield return null;
        for(float t = 0 ; t <=0.8f; t += Time.deltaTime){
            yield return new WaitForFixedUpdate();
        }
        findNextAiMove();
        checkWinner();

    }

    // Update game with next AI move
    public void AiSelects(int coordX, int coordY){
        boardState[coordX,coordY]=aiState;
        SetVisual(coordX, coordY, aiState);
        _triggers[coordX,coordY].SetInputEndabled(false);
    }
    
    // Update the board UI
    private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
    {
        Instantiate(
            targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
            _triggers[coordX, coordY].transform.position,
            Quaternion.identity
        );
    }

    // Go through all available spots and select one for the AI player
    public void findNextAiMove()
    {
        availableSpots = new List<(int, int)>();
        for (int i=0; i<3; i++){
            for (int j = 0; j<3; j++)
            {
                if (boardState[i,j]==TicTacToeState.none)
                {
                    (int, int) coordinates = (i, j);
                    availableSpots.Add(coordinates);
                }
            }
        }
        // Atm randomly chose next AI spot    
        if (availableSpots.Count>0){
            var random = new System.Random();
            int index = random.Next(0, availableSpots.Count);
            (int,int) coordinatePair = availableSpots[index];
            AiSelects(coordinatePair.Item1, coordinatePair.Item2);
        }
    }

    // Method for checking if 3 board states are the same and non-empty
    private bool validEquals(TicTacToeState a, TicTacToeState b, TicTacToeState c)
    {
        if (a == b & b == c & a != TicTacToeState.none)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    // Check if anyone has won or a tie has occured
    public int? checkWinner()
    {
        // Check horizontal and vertical rows for winning condition
        for (int i=0; i<3; i++)
        {
            if (validEquals(boardState[i,0], boardState[i,1], boardState[i,2]))
            {
                announceWinner(boardState[i,0]);
                return((int)boardState[i,0]);
            }   
            if (validEquals(boardState[0,i], boardState[1,i], boardState[2,i]))
            {
                announceWinner(boardState[0,i]);
                return((int)boardState[0,i]);
            }
        }
        // Check diagonals for winning condition
        if (validEquals(boardState[0,0],boardState[1,1],boardState[2,2]))
        {
            announceWinner(boardState[0,0]);
            return((int)boardState[0,0]);
        }
        if (validEquals(boardState[0,2], boardState[1,1], boardState[2,0]))
        {
            announceWinner(boardState[0,2]);
            return((int)boardState[0,2]);
        }
        // Tie! - Invovke onPlayerWin
        if(availableSpots.Count==0){
            onPlayerWin.Invoke(0);
            return(0);
        }
        return (null);
    }
    
    // Invovke onPlayerWin with correct winner
    private void announceWinner(TicTacToeState state)
    {
        if (playerState == state)
        {
            onPlayerWin.Invoke(1);
        }
        else
        {
            onPlayerWin.Invoke(-1);
        }
    }
}