using UnityEngine.Events;

namespace DuckChess
{
    public abstract class Player
    {

        /// <summary>
        /// The type of player. For example, a human player, an alpha beta ai, etc.
        /// </summary>
        public abstract string Type { get; }
        public abstract int Color { get; set; }
    }
}