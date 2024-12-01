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

        private const int NumActionsPerFrame = 100;

        private Board board;
        private int maxDepth;
        private Move bestMove;
        private bool startSearch;
        private List<Move> legalMoves;
        private Stack<AlphaBetaNode> alphaBetaNodes;
        private AlphaBetaNode topNode;
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
            try
            {
                LookForBestMove();
            }
            catch (Exception e)
            {
                Debug.Log("Invalid Move\n" + topNode.ToString() + "\n" + board.ToString() + "\n" + searchBoard.ToString() + "\n");
                return;
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
            if (bestMove.StartSquare == 0 && bestMove.TargetSquare == 0)
            {
                Debug.Log("Invalid Move\n" + topNode.ToString() + "\n" + board.ToString() + "\n" + searchBoard.ToString());
                bestMove = board.legalMoves[0];
            }
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
        }

        private void ExpandNode()
        {
            currentDepth++;
            AlphaBetaNode parent = alphaBetaNodes.Peek();
            int indexOfNextMove = parent.indexLeftOffAt;
            Move nextMove = legalMoves[indexOfNextMove];
            if (searchBoard.duckTurn)
            {
                Debug.Log("Legal Duck Move " + nextMove.ToString());
            }

            bool isMaximizing = searchBoard.duckTurn ? !parent.isMaximizing : parent.isMaximizing;
            AlphaBetaNode newNode = new AlphaBetaNode(parent.alpha, parent.beta, isMaximizing, nextMove, parent);

            searchBoard.MakeMove(ref nextMove);
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
