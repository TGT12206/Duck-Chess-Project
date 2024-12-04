using System;
using System.Collections.Generic;
using UnityEngine;

namespace DuckChess
{
    public class MCTSAIPlayer : RealTimePlayer
    {
        public override string Type { get { return "MCTSAIPlayer"; } }
        public override int Color { get; set; }

        private const int NumSimulationsPerFrame = 100;

        private const int NumSimulationsPerTurn = 1500;

        private const int maxSimulationDepth = 20; // Adjust as needed

        private Board board;
        private BoardUI boardUI;
        private Move bestMove;
        private bool startSearch;
        private MCTSNode rootNode;
        private Board searchBoard;

        private int numSims = 0;

        // Debugging
        public bool showSearchBoard;

        public MCTSAIPlayer(Board board, int color, BoardUI boardUI)
        {
            this.board = board;
            this.Color = color;
            this.boardUI = boardUI;
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

        private void InitializeSearch()
        {
            searchBoard = new Board(board);
            rootNode = new MCTSNode(null, Move.Invalid, searchBoard, Color);
            startSearch = false;
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
            if (board.isGameOver)
            {
                if (board.winnerColor == Color)
                {
                    return 1; // Win
                }
                else if (board.winnerColor == Piece.NoColor)
                {
                    return 0; // Draw
                }
                else
                {
                    return -1; // Loss
                }
            }
            else
            {
                // Use heuristic evaluation if the game hasn't ended
                int evaluation = BoardEvaluator.Evaluate(board, Color);
                // Normalize evaluation to -1, 0, or 1
                return Math.Sign(evaluation);
            }
        }

        public override void UnmakeMove()
        {
            throw new NotImplementedException();
        }
    }
}
