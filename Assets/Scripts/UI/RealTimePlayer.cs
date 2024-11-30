using DuckChess;
using UnityEngine.Events;

public abstract class RealTimePlayer : Player
{
    /// <summary>
    /// An asynchronous signal that can be invoked to let an
    /// instance of ITurnManager know that this player has chosen a move.
    /// </summary>
    public UnityEvent<Move> OnMoveChosen = new UnityEvent<Move>();

    /// <summary>
    /// Invokes a unity event to let the turn manager know that a move was chosen.
    /// The caller should guarantee that the move chosen is valid.
    /// </summary>
    public void ChooseMove(Move moveChosen)
    {
        OnMoveChosen.Invoke(moveChosen);
    }

    /// <summary>
    /// Undo a move and reset the variables you used.
    /// </summary>
    public abstract void UnmakeMove();

    /// <summary>
    /// Called every frame of this player's turn,
    /// handling what the player does during their turn.
    /// </summary>
    public abstract void Update();
}
