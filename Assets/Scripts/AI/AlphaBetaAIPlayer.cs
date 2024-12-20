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
        private BoardUI boardUI;
        public int maxDepth;
        private Move bestMove;
        private bool startSearch;
        private List<Move> legalMoves;
        private Stack<AlphaBetaNode> alphaBetaNodes;
        private Board searchBoard;
        private int currentDepth;
        public bool skipDuckSearch;

        // For debugging, but might be useful
        private AlphaBetaNode topNode;

        // Debugging, can delete
        private int numMovesLookedAt;
        public bool showSearchBoard;

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
            LookForBestMove();
        }

        private void LookForBestMove()
        {
            if (startSearch)
            {
                if (skipDuckSearch)
                {
                    if (board.turnIsDuck)
                    {
                        topNode = topNode.child;
                        bestMove = topNode.moveToValue;
                        ChooseMove(bestMove);
                        return;
                    }
                }
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
            ChooseMove(bestMove);
            startSearch = true;
        }

        private void InitializeSearch()
        {
            bestMove = new Move();
            searchBoard = new Board(board);
            legalMoves = searchBoard.legalMoves;
            alphaBetaNodes = new Stack<AlphaBetaNode>();
            topNode = new AlphaBetaNode(int.MinValue, int.MaxValue, true, new Board(board));
            alphaBetaNodes.Push(topNode);
            currentDepth = 1;
            startSearch = false;
            numMovesLookedAt = 0;
        }

        private void ExpandNode()
        {
            currentDepth++;
            numMovesLookedAt++;
            AlphaBetaNode parent = alphaBetaNodes.Peek();
            int indexOfNextMove = parent.indexLeftOffAt;
            Move nextMove = legalMoves[indexOfNextMove];

            if (showSearchBoard)
            {
                boardUI.LoadPosition(ref searchBoard, true, "Depth: " + currentDepth);
            }

            // Make the new node
            searchBoard.MakeMove(ref nextMove);
            bool isMaximizing = searchBoard.turnColor == Color;
            AlphaBetaNode newNode = new AlphaBetaNode(parent.alpha, parent.beta, isMaximizing, new Board(searchBoard), nextMove);

            legalMoves = searchBoard.legalMoves;
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

            if (showSearchBoard)
            {
                boardUI.LoadPosition(ref searchBoard, true, "Depth: " + currentDepth);
            }

            if (currentDepth == 0)
            {
                return true;
            }

            AlphaBetaNode parent = alphaBetaNodes.Peek();
            searchBoard = new Board(parent.currentBoard);
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
