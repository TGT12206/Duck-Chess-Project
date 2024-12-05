using System;
using System.Collections.Generic;
using UnityEngine;

namespace DuckChess
{
    public class MixedAIPlayer : RealTimePlayer
    {
        public override string Type { get { return "MixedAIPlayer"; } }
        public override int Color { get; set; }

        private const int NumActionsPerFrame = 100;

        private Board board;
        private BoardUI boardUI;
        private int maxDepth;
        private Move bestMove;
        private bool startSearch;
        private List<Move> legalMoves;
        private Stack<AlphaBetaNode> alphaBetaNodes;
        private Board searchBoard;
        private int currentDepth;
        public bool skipDuckSearch;
        private bool isEndGame;

        private const int NumSimulationsPerFrame = 100;

        private const int NumSimulationsPerTurn = 50000;

        private const int maxSimulationDepth = 10; // Adjust as needed
        private MCTSNode rootNode;

        private int numSims = 0;

        // Debugging
        public bool showSearchBoard;

        public MixedAIPlayer(Board board, int color, int maxDepth, BoardUI boardUI)
        {
            this.board = board;
            this.Color = color;
            this.boardUI = boardUI;
            startSearch = true;
            this.maxDepth = maxDepth;
            startSearch = true;
            this.boardUI = boardUI;
        }

        public override void Update()
        {
            if (isEndGame)
            {
                LookForBestABMove();
            }
            LookForBestMCTSMove();
        }

        private void LookForBestMCTSMove()
        {
            if (startSearch)
            {
                InitializeMCTSSearch();
            }

            for (int i = 0; i < NumSimulationsPerFrame; i++)
            {
                MCTSNode node = Selection();
                if (node == null)
                {
                    continue; // No node to process
                }

                int result = Simulation(node);
                Backpropagation(node, result);
                numSims++;
            }

            if (numSims >= NumSimulationsPerTurn) {
                bestMove = rootNode.GetBestMove();

                if (bestMove.IsValid())
                {
                    FinishSearch();
                }
                numSims = 0;
            }

        }

        private void FinishSearch()
        {
            ChooseMove(bestMove);
            startSearch = true;
        }

        private void InitializeMCTSSearch()
        {
            searchBoard = new Board(board);
            rootNode = new MCTSNode(null, Move.Invalid, searchBoard, Color);
            startSearch = false;
            isEndGame = BoardEvaluator.IsEndGame(board);
        }

        private MCTSNode Selection()
        {
            MCTSNode node = rootNode;

            while (node.IsFullyExpanded && !node.IsTerminal)
            {
                node = node.GetBestUCTChild();
                if (node == null)
                {
                    break;
                }
            }

            if (node != null && !node.IsTerminal)
            {
                if (showSearchBoard)
                {
                    node = node.Expand(boardUI);
                }
                node = node.Expand();
            }

            return node;
        }

        private int Simulation(MCTSNode node)
        {
            Board simulationBoard = new Board(node.BoardState);
            int currentPlayer = simulationBoard.turnColor;

            int simulationDepth = 0;

            while (!simulationBoard.isGameOver && simulationDepth < maxSimulationDepth)
            {
                List<Move> legalMoves = simulationBoard.legalMoves;
                if (legalMoves.Count == 0)
                {
                    break;
                }

                if (showSearchBoard)
                {
                    boardUI.LoadPosition(ref simulationBoard, true, "");
                }

                // Randomly select a move
                Move randomMove = legalMoves[UnityEngine.Random.Range(0, legalMoves.Count)];
                simulationBoard.MakeMove(ref randomMove);

                currentPlayer = simulationBoard.turnColor;
                simulationDepth++;
            }

            // Evaluate the final board state
            return EvaluateResult(simulationBoard);
        }

        private void Backpropagation(MCTSNode node, int result)
        {
            while (node != null)
            {
                node.Visits++;
                node.TotalScore += result;
                node = node.Parent;
            }
        }

        private int EvaluateResult(Board board)
        {
            // if (board.isGameOver)
            // {
            //     if (board.winnerColor == Color)
            //     {
            //         return 1; // Win
            //     }
            //     else if (board.winnerColor == Piece.NoColor)
            //     {
            //         return 0; // Draw
            //     }
            //     else
            //     {
            //         return -1; // Loss
            //     }
            // }
            // else
            // {
            // Use heuristic evaluation if the game hasn't ended
            int evaluation = BoardEvaluator.Evaluate(board, Color);
            // Normalize evaluation to -1, 0, or 1
            return Math.Sign(evaluation);
            // }
        }

        // For debugging, but might be useful
        private AlphaBetaNode topNode;

        // Debugging, can delete
        private int numMovesLookedAt;
        private void LookForBestABMove()
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
                InitializeABSearch();
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

        private void InitializeABSearch()
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
