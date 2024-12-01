using System;
using System.Collections.Generic;
using UnityEngine;

namespace DuckChess
{
    public class AlphaBetaAIPlayer : RealTimePlayer
    {
        public override string Type => "ABPlayer";
        public override int Color { get; set; }

        private const int NodesPerFrame = 1000; // Number of nodes to explore per frame
        private readonly int maxDepth; // Max depth for the alpha-beta search

        private Board board; // Current game board
        private Board searchBoard; // Cloned board for search
        private Move bestMove; // The best move found
        private Stack<SearchNode> searchStack; // Stack to manage Alpha-Beta nodes
        private List<Move> legalMoves; // Legal moves for the current position
        private bool isSearching; // Indicates if search is ongoing

        public AlphaBetaAIPlayer(Board board, int color, int maxDepth)
        {
            this.board = board;
            this.Color = color;
            this.maxDepth = maxDepth;
            isSearching = true;
        }

        public override void Update()
        {
            if (isSearching)
            {
                SearchBestMove();
            }
        }

        private void SearchBestMove()
        {
            if (searchStack == null)
            {
                InitializeSearch();
            }

            for (int i = 0; i < NodesPerFrame; i++)
            {
                if (searchStack.Count == 0)
                {
                    Debug.Log($"Best move found: {bestMove}");
                    ChooseMove(bestMove);
                    isSearching = false;
                    return;
                }

                ProcessCurrentNode();
            }
        }

        private void InitializeSearch()
        {
            searchBoard = board.Clone();
            legalMoves = new List<Move>(board.legalMoves);
            bestMove = new Move();
            searchStack = new Stack<SearchNode>();

            // Push the root node
            searchStack.Push(new SearchNode(int.MinValue, int.MaxValue, true, 0, new Move()));
        }

        private void ProcessCurrentNode()
        {
            var currentNode = searchStack.Peek();

            if (currentNode.Depth >= maxDepth || currentNode.Index >= legalMoves.Count || searchBoard.isGameOver)
            {
                // Evaluate leaf nodes
                if (currentNode.Depth >= maxDepth || searchBoard.isGameOver)
                {
                    currentNode.Value = BoardEvaluator.Evaluate(searchBoard, Color);
                }

                // Backtrack and propagate values
                searchStack.Pop();
                if (searchStack.Count > 0)
                {
                    var parent = searchStack.Peek();
                    if (parent.IsMaximizing)
                    {
                        if (currentNode.Value > parent.Alpha)
                        {
                            parent.Alpha = currentNode.Value;
                            if (currentNode.Depth == 1)
                            {
                                bestMove = currentNode.Move;
                            }
                        }
                    }
                    else
                    {
                        if (currentNode.Value < parent.Beta)
                        {
                            parent.Beta = currentNode.Value;
                        }
                    }
                    parent.Index++;
                }
                searchBoard.UnmakeMove(currentNode.Move, searchBoard.numPlySinceLastEvent);
            }
            else
            {
                // Expand node
                var nextMove = legalMoves[currentNode.Index];
                searchBoard.MakeMove(ref nextMove);

                searchStack.Push(new SearchNode(
                    currentNode.Alpha,
                    currentNode.Beta,
                    !currentNode.IsMaximizing,
                    currentNode.Depth + 1,
                    nextMove));
            }
        }

        public override void UnmakeMove()
        {
            throw new NotImplementedException();
        }
    }

    public class SearchNode
    {
        public int Alpha { get; set; }
        public int Beta { get; set; }
        public bool IsMaximizing { get; }
        public int Depth { get; }
        public int Index { get; set; }
        public Move Move { get; }
        public int Value { get; set; }

        public SearchNode(int alpha, int beta, bool isMaximizing, int depth, Move move)
        {
            Alpha = alpha;
            Beta = beta;
            IsMaximizing = isMaximizing;
            Depth = depth;
            Index = 0;
            Move = move;
            Value = isMaximizing ? int.MinValue : int.MaxValue;
        }
    }

    
}
