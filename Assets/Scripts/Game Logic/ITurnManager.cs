namespace DuckChess
{
    /// <summary>
    /// Manages the board and keeps track of whose turn it is, allowing only that player to move.
    /// </summary>
    public interface ITurnManager
    {
        
        /// <summary>
        /// Returns whether or not the game is over
        /// </summary>
        /// <returns>
        /// true if the game has ended
        /// <br></br>
        /// false if the game has ended
        /// </returns>
        public abstract bool IsGameOver();

        /// <summary>
        /// Make and record the given move
        /// </summary>
        /// <param name="move">The move to be made</param>
        public abstract void MakeMove(Move move);
    }
}