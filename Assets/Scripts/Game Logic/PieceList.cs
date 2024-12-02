using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DuckChess
{
    /// <summary>
    /// Represents a collection of squares occupied by a specific piece type.
    /// </summary>
    public class PieceList : IEnumerable<int>
    {
        /// <summary>
        /// Stores squares occupied by the specific piece type.
        /// </summary>
        private readonly int[] occupiedSquares;

        /// <summary>
        /// Maps each board square (0-63) to its index in <see cref="occupiedSquares"/>.
        /// </summary>
        private readonly int[] map;

        /// <summary>
        /// Number of pieces currently in this list.
        /// </summary>
        private int numPieces;

        /// <summary>
        /// Initializes a new instance of the <see cref="PieceList"/> class.
        /// </summary>
        /// <param name="maxPieceCount">Maximum number of pieces this list can track.</param>
        public PieceList(int maxPieceCount)
        {
            occupiedSquares = new int[maxPieceCount];
            map = new int[64];
            Clear();
        }

        /// <summary>
        /// Clears the list, resetting it to an empty state.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < occupiedSquares.Length; i++)
                occupiedSquares[i] = -1;

            for (int i = 0; i < map.Length; i++)
                map[i] = -1;

            numPieces = 0;
        }

        /// <summary>
        /// Gets the number of pieces in this list.
        /// </summary>
        public int Count => numPieces;

        /// <summary>
        /// Adds a piece at the specified square.
        /// </summary>
        /// <param name="square">Square index (0-63) where the piece is located.</param>
        public void AddPieceAtSquare(int square)
        {
            occupiedSquares[numPieces] = square;
            map[square] = numPieces;
            numPieces++;
        }

        /// <summary>
        /// Removes a piece from the specified square.
        /// </summary>
        /// <param name="square">Square index (0-63) to remove the piece from.</param>
        public void RemovePieceAtSquare(int square)
        {
            int pieceIndex = map[square];
            occupiedSquares[pieceIndex] = occupiedSquares[numPieces - 1];
            map[occupiedSquares[pieceIndex]] = pieceIndex;
            occupiedSquares[numPieces - 1] = -1;
            map[square] = -1;
            numPieces--;
        }

        /// <summary>
        /// Moves a piece from the start square to the target square.
        /// </summary>
        /// <param name="move">Move describing the start and target squares.</param>
        public void MovePiece(Move move)
        {
            int pieceIndex = map[move.StartSquare];
            occupiedSquares[pieceIndex] = move.TargetSquare;
            map[move.TargetSquare] = pieceIndex;
            map[move.StartSquare] = -1;
        }

        /// <summary>
        /// Undoes a move by reversing the start and target squares.
        /// </summary>
        /// <param name="move">Move to undo.</param>
        public void UnmovePiece(Move move)
        {
            int pieceIndex = map[move.TargetSquare];
            occupiedSquares[pieceIndex] = move.StartSquare;
            map[move.StartSquare] = pieceIndex;
            map[move.TargetSquare] = -1;
        }

        /// <summary>
        /// Merges two <see cref="PieceList"/> objects into a new list.
        /// </summary>
        public static PieceList MergePieceLists(PieceList firstList, PieceList secondList)
        {
            PieceList newList = new PieceList(firstList.Count + secondList.Count);
            foreach (var square in firstList)
                newList.AddPieceAtSquare(square);

            foreach (var square in secondList)
                newList.AddPieceAtSquare(square);

            return newList;
        }

        /// <summary>
        /// Merges another <see cref="PieceList"/> into this list.
        /// </summary>
        /// <param name="otherList">The other piece list to merge.</param>
        public void MergeWithPieceList(PieceList otherList)
        {
            foreach (var square in otherList)
                AddPieceAtSquare(square);
        }

        /// <summary>
        /// Returns the occupied square at the given index.
        /// </summary>
        public int this[int index] => occupiedSquares[index];

        /// <summary>
        /// Enumerates through all occupied squares.
        /// </summary>
        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < numPieces; i++)
                yield return occupiedSquares[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Clones this <see cref="PieceList"/> into a new instance.
        /// </summary>
        public PieceList Clone()
        {
            PieceList clone = new PieceList(occupiedSquares.Length);
            clone.numPieces = numPieces;
            occupiedSquares.CopyTo(clone.occupiedSquares, 0);
            map.CopyTo(clone.map, 0);
            return clone;
        }

        /// <summary>
        /// Returns a string representation of this piece list.
        /// </summary>
        public override string ToString()
        {
            return $"Occupied Squares: [{string.Join(", ", this)}]\nCount: {numPieces}";
        }
    }
}
