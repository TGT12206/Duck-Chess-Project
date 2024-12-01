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

        private const int NumActionsPerFrame = 1;

        private Board board;
        private BoardUI boardUI;
        private int maxDepth;
        private Move bestMove;
        private bool startSearch;
        private List<Move> legalMoves;
        private Stack<AlphaBetaNode> alphaBetaNodes;
        private Stack<int> significantMoveCounters;
        private Board searchBoard;
        private int currentDepth;

        // For debugging, but might be useful
        private AlphaBetaNode topNode;

        // Debugging, can delete
        private int movesLookedAt;

        public AlphaBetaAIPlayer(Board board, int color, int maxDepth, BoardUI boardUI)
        {
            this.board = board;
            this.Color = color;
            this.maxDepth = maxDepth;
            startSearch = true;
            this.boardUI = boardUI;
        }

        public override void Update()
        {
            try
            {
                LookForBestMove();
            }
            catch (Exception e)
            {
                Debug.Log("Invalid Move" + movesLookedAt + "\n" + topNode.ToString() + "\n" + board.ToString() + "\n" + searchBoard.ToString() + "\n");
                Debug.LogException(e);
                throw e;
            }
        }

        private void LookForBestMove()
        {
            if (startSearch)
            {
                InitializeSearch();
            }

            for (int i = 0; i < NumActionsPerFrame; i++)
            {
                AlphaBetaNode currentNode = alphaBetaNodes.Peek();

                if (currentDepth < maxDepth && !searchBoard.isGameOver && currentNode.indexLeftOffAt < legalMoves.Count)
                {
                    if (currentNode.ShouldPrune())
                    {
                        if (CloseNode())
                        {
                            FinishSearch();
                            return;
                        }
                    }
                    else
                    {
                        ExpandNode();
                    }
                }
                else
                {
                    if (CloseNode())
                    {
                        FinishSearch();
                        return;
                    }
                }
            }
        }

        private void FinishSearch()
        {
            // Search is complete
            Debug.Log("Chose Move" + movesLookedAt + "\n" + topNode.ToString() + "\n" + board.ToString() + "\n" + searchBoard.ToString() + "\n");
            ChooseMove(bestMove);
            startSearch = true;
        }

        private void InitializeSearch()
        {
            bestMove = new Move();
            searchBoard = board.Clone();
            legalMoves = searchBoard.legalMoves;
            alphaBetaNodes = new Stack<AlphaBetaNode>();
            significantMoveCounters = new Stack<int>();
            topNode = new AlphaBetaNode(int.MinValue, int.MaxValue, true, new Move());
            alphaBetaNodes.Push(topNode);
            currentDepth = 1;
            startSearch = false;
            movesLookedAt = 0;
        }

        private void ExpandNode()
        {
            currentDepth++;
            movesLookedAt++;
            AlphaBetaNode parent = alphaBetaNodes.Peek();
            int indexOfNextMove = parent.indexLeftOffAt;
            Move nextMove = legalMoves[indexOfNextMove];

            if (nextMove.StartSquare == 33 && nextMove.TargetSquare == 24)
            {
                Debug.Log("Search Board: " + searchBoard.ToString());
            }

            bool isMaximizing = searchBoard.duckTurn ? !parent.isMaximizing : parent.isMaximizing;
            
            // Making a move on a board saves any captured piece onto the move,
            // so we need to make the move before saving.
            searchBoard.MakeMove(ref nextMove);
            AlphaBetaNode newNode = new AlphaBetaNode(parent.alpha, parent.beta, isMaximizing, nextMove, parent);
            
            legalMoves = searchBoard.legalMoves;
            significantMoveCounters.Push(searchBoard.numPlySinceLastEvent);
            alphaBetaNodes.Push(newNode);
        }

        private bool CloseNode()
        {
            AlphaBetaNode node = alphaBetaNodes.Pop();
            if (currentDepth >= maxDepth || searchBoard.isGameOver)
            {
                node.value = EvaluateBoard(searchBoard);
            }

            currentDepth--;

            if (currentDepth == 0)
            {
                return true;
            }

            AlphaBetaNode parent = alphaBetaNodes.Peek();
            searchBoard.UnmakeMove(node.moveFromParent, significantMoveCounters.Pop());
            legalMoves = searchBoard.legalMoves;

            if (parent.JudgeNewValue(node.value, node.moveFromParent, node) && currentDepth == 1)
            {
                bestMove = node.moveFromParent;
                boardUI.AISelectPiece(bestMove);
            }

            parent.indexLeftOffAt++;
            return false;
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
