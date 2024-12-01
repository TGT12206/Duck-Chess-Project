using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckChess
{
    public class PieceList : IEnumerable<int>
    {
        /// <summary>
        /// A list that stores squares with the specific piece type.
        /// </summary>
        private readonly List<int> occupiedSquares;

        /// <summary>
        /// A mapping from square (0-63) to the index within the occupiedSquares list.
        /// </summary>
        private readonly int[] map;

        /// <summary>
        /// The maximum number of pieces this list can hold.
        /// </summary>
        private readonly int maxPieceCount;

        public PieceList(int maxPieceCount)
        {
            this.maxPieceCount = maxPieceCount;
            occupiedSquares = new List<int>(maxPieceCount);
            map = new int[64];
            Array.Fill(map, -1); // Initialize all squares as unmapped
        }

        /// <summary>
        /// The number of this piece type on the board.
        /// </summary>
        public int Count => occupiedSquares.Count;

        /// <summary>
        /// Adds a piece to the list and updates the map.
        /// </summary>
        public void AddPieceAtSquare(int square)
        {
            if (occupiedSquares.Count >= maxPieceCount)
            {
                throw new InvalidOperationException("Exceeded the maximum capacity of this PieceList.");
            }

            if (map[square] != -1)
            {
                throw new InvalidOperationException($"Square {square} is already occupied.");
            }

            occupiedSquares.Add(square);
            map[square] = occupiedSquares.Count - 1;
        }

        /// <summary>
        /// Removes the piece from the specified square and updates the map.
        /// </summary>
        public void RemovePieceAtSquare(int square)
        {
            int pieceIndex = map[square];
            if (pieceIndex == -1)
            {
                throw new InvalidOperationException($"Square {square} does not contain a piece.");
            }

            // Swap the last piece in the list with the one being removed
            int lastPieceSquare = occupiedSquares[^1];
            occupiedSquares[pieceIndex] = lastPieceSquare;
            map[lastPieceSquare] = pieceIndex;

            // Remove the last piece and update the map
            occupiedSquares.RemoveAt(occupiedSquares.Count - 1);
            map[square] = -1;
        }

        /// <summary>
        /// Updates the locations to reflect a move.
        /// </summary>
        public void MovePiece(Move move)
        {
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;

            RemovePieceAtSquare(startSquare);
            AddPieceAtSquare(targetSquare);
        }

        /// <summary>
        /// Reverts a move to its original state.
        /// </summary>
        public void UnmovePiece(Move move)
        {
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;

            RemovePieceAtSquare(targetSquare);
            AddPieceAtSquare(startSquare);
        }

        /// <summary>
        /// Combines two piece lists into a new readonly list.
        /// </summary>
        public static PieceList MergePieceLists(PieceList firstList, PieceList secondList)
        {
            int combinedCount = firstList.Count + secondList.Count;
            PieceList mergedList = new PieceList(combinedCount);

            foreach (int square in firstList)
            {
                mergedList.AddPieceAtSquare(square);
            }

            foreach (int square in secondList)
            {
                mergedList.AddPieceAtSquare(square);
            }

            return mergedList;
        }

        /// <summary>
        /// Adds the information of another list into this one.
        /// </summary>
        public void MergeWithPieceList(PieceList otherList)
        {
            foreach (int square in otherList)
            {
                AddPieceAtSquare(square);
            }
        }

        /// <summary>
        /// Allows indexing into the occupiedSquares list.
        /// </summary>
        public int this[int index] => occupiedSquares[index];

        /// <summary>
        /// Provides a formatted string of the piece list.
        /// </summary>
        public override string ToString()
        {
            return $"Occupied Squares: {string.Join(", ", occupiedSquares)} (Count: {Count})";
        }

        /// <summary>
        /// Creates a deep copy of this PieceList.
        /// </summary>
        public PieceList Clone()
        {
            PieceList clone = new PieceList(maxPieceCount);
            foreach (int square in occupiedSquares)
            {
                clone.AddPieceAtSquare(square);
            }
            return clone;
        }

        /// <summary>
        /// Allows iteration over occupied squares using foreach.
        /// </summary>
        public IEnumerator<int> GetEnumerator()
        {
            return occupiedSquares.GetEnumerator();
        }

        /// <summary>
        /// Non-generic enumerator for compatibility.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
