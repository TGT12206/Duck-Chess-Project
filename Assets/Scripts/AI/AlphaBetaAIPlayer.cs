using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DuckChess
{
    public class AlphaBetaAIPlayer : RealTimePlayer
    {
        public override string Type { get { return "AlphaBetaAIPlayer"; } }
        public override int Color { get; set; }

        private const int NumActionsPerFrame = 10000;

        private Board board;
        private int maxDepth;
        private Move bestMove;
        private bool startSearch;
        private List<Move> legalMoves;
        private Stack<AlphaBetaNode> alphaBetaNodes;
        private Stack<int> significantMoveCounters;
        private Board searchBoard;
        private int currentDepth;

        public AlphaBetaAIPlayer(Board board, int color, int maxDepth)
        {
            this.board = board;
            this.Color = color;
            this.maxDepth = maxDepth;
            startSearch = true;
        }

        public override void Update()
        {
            LookForBestMove();
        }

        private void LookForBestMove()
        {
            if (startSearch)
            {
                InitializeSearch();
            }

            for (int i = 0; i < NumActionsPerFrame; i++)
            {
                if (alphaBetaNodes.Count == 0)
                {
                    Debug.Log("No more nodes to consider.");
                    // Search is complete
                    ChooseMove(bestMove);
                    startSearch = true;
                    return;
                }

                AlphaBetaNode currentNode = alphaBetaNodes.Peek();

                if (currentDepth < maxDepth && currentNode.indexLeftOffAt < legalMoves.Count)
                {
                    if (currentNode.ShouldPrune())
                    {
                        CloseNode();
                    }
                    else
                    {
                        ExpandNode();
                    }
                }
                else
                {
                    CloseNode();
                }
            }
        }

        private void InitializeSearch()
        {
            bestMove = new Move();
            legalMoves = board.legalMoves;
            searchBoard = board.Clone();
            alphaBetaNodes = new Stack<AlphaBetaNode>();
            significantMoveCounters = new Stack<int>();
            alphaBetaNodes.Push(new AlphaBetaNode(int.MinValue, int.MaxValue, true, new Move()));
            currentDepth = 1;
            startSearch = false;
        }

        private void ExpandNode()
        {
            currentDepth++;
            AlphaBetaNode parent = alphaBetaNodes.Peek();
            int indexOfNextMove = parent.indexLeftOffAt;
            Move nextMove = legalMoves[indexOfNextMove];

            bool isMaximizing = searchBoard.duckTurn ? !parent.isMaximizing : parent.isMaximizing;
            AlphaBetaNode newNode = new AlphaBetaNode(parent.alpha, parent.beta, isMaximizing, nextMove);

            searchBoard.MakeMove(ref nextMove);
            legalMoves = searchBoard.legalMoves;
            significantMoveCounters.Push(searchBoard.numPlySinceLastEvent);
            alphaBetaNodes.Push(newNode);
        }

        private void CloseNode()
        {
            AlphaBetaNode node = alphaBetaNodes.Pop();
            currentDepth--;

            if (alphaBetaNodes.Count == 0)
            {
                // If we've returned to the root, end the search
                return;
            }

            if (currentDepth == maxDepth || searchBoard.isGameOver)
            {
                node.value = EvaluateBoard(searchBoard);
                Debug.Log(node.value);
            }

            AlphaBetaNode parent = alphaBetaNodes.Peek();
            searchBoard.UnmakeMove(node.moveFromParent, significantMoveCounters.Pop());
            legalMoves = searchBoard.legalMoves;

            if (parent.JudgeNewValue(node.value, node.moveFromParent) && currentDepth == 1)
            {
                bestMove = node.moveFromParent;
            }

            parent.indexLeftOffAt++;
        }

        private int EvaluateBoard(Board board)
        {
           return BoardEvaluator.Evaluate(board, this.Color);
        }

        public override void UnmakeMove()
        {
            throw new NotImplementedException();
        }
    }
}