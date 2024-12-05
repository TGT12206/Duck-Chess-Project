using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DuckChess
{
    public class MCTSNode
    {
        public MCTSNode Parent;
        public List<MCTSNode> Children;
        public Move MoveFromParent;
        public Board BoardState;
        public int Visits;
        public float TotalScore;
        public bool IsFullyExpanded;
        public bool IsTerminal;

        private List<Move> untriedMoves;
        private int PlayerColor;
        private System.Random random = new System.Random();

        public MCTSNode(MCTSNode parent, Move moveFromParent, Board boardState, int playerColor)
        {
            Parent = parent;
            MoveFromParent = moveFromParent;
            BoardState = boardState;
            Visits = 0;
            TotalScore = 0f;
            PlayerColor = playerColor;

            IsTerminal = boardState.isGameOver;
            Children = new List<MCTSNode>();

            if (!IsTerminal)
            {
                untriedMoves = new List<Move>(boardState.legalMoves);
                IsFullyExpanded = untriedMoves.Count == 0;
            }
            else
            {
                untriedMoves = new List<Move>();
                IsFullyExpanded = true;
            }
        }

        public MCTSNode Expand()
        {
            if (untriedMoves.Count == 0)
            {
                return null;
            }

            // Randomly select an untried move
            int index = random.Next(untriedMoves.Count);
            Move move = untriedMoves[index];
            untriedMoves.RemoveAt(index);

            // Apply the move to create a new board state
            Board newBoard = new Board(BoardState);
            newBoard.MakeMove(ref move);

            // Create the new child node
            MCTSNode childNode = new MCTSNode(this, move, newBoard, PlayerColor);
            Children.Add(childNode);

            if (untriedMoves.Count == 0)
            {
                IsFullyExpanded = true;
            }

            return childNode;
        }
        public MCTSNode Expand(BoardUI boardUI)
        {
            if (untriedMoves.Count == 0)
            {
                return null;
            }

            // Randomly select an untried move
            int index = random.Next(untriedMoves.Count);
            Move move = untriedMoves[index];
            untriedMoves.RemoveAt(index);

            // Apply the move to create a new board state
            Board newBoard = new Board(BoardState);
            newBoard.MakeMove(ref move);

            // Create the new child node
            MCTSNode childNode = new MCTSNode(this, move, newBoard, PlayerColor);
            boardUI.LoadPosition(ref newBoard, true, "");
            Children.Add(childNode);

            if (untriedMoves.Count == 0)
            {
                IsFullyExpanded = true;
            }

            return childNode;
        }

        public MCTSNode GetBestUCTChild()
        {
            MCTSNode bestChild = null;
            float bestUCTValue = float.MinValue;

            foreach (var child in Children)
            {
                if (child.Visits == 0)
                {
                    // Handle case where Visits is zero to avoid division by zero
                    return child;
                }

                float exploitation = child.TotalScore / child.Visits;
                float exploration = Mathf.Sqrt(2 * Mathf.Log(Visits) / child.Visits);
                float uctValue = exploitation + exploration;

                if (uctValue > bestUCTValue)
                {
                    bestUCTValue = uctValue;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        public Move GetBestMove()
        {
            MCTSNode bestChild = null;
            int mostVisits = -1;

            foreach (var child in Children)
            {
                if (child.Visits > mostVisits)
                {
                    mostVisits = child.Visits;
                    bestChild = child;
                }
            }

            if (bestChild != null)
            {
                return bestChild.MoveFromParent;
            }
            else
            {
                // Return an invalid move
                return Move.Invalid;
            }
        }
    }
}
