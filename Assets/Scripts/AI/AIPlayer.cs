namespace DuckChess
{
    using System;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;

    public class AIPlayer : RealTimePlayer
    {
        public override string Type { get { return "AIPlayer"; } }
        public override int Color { get; set; }

        private int maxDepth;
        private Board board;

        public AIPlayer(Board board, int color, int maxDepth)
        {
            this.board = board;
            this.Color = color;
            this.maxDepth = maxDepth;
        }

        public override void Update()
        {
            if (board.turnColor != Color || board.isGameOver)
                return;

            Move bestMove = GetBestMove();
            ChooseMove(bestMove);
        }

        private Move GetBestMove()
        {
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            Move bestMove = Move.InvalidMove;
            int maxEval = int.MinValue;

            List<Move> legalMoves = GetAllLegalMoves(board);

            foreach (Move move in legalMoves)
            {
                Debug.Log("Evaluating Move: " + move.ToString());

                Board newBoard = board.Clone();
                newBoard.MakeMove(move);
                int eval = AlphaBeta(newBoard, maxDepth - 1, alpha, beta, false);

                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, eval);
            }

            Debug.Log("AI Selected Move: " + bestMove.ToString());
            return bestMove;
        }


        private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0 || board.isGameOver)
            {
                return EvaluateBoard(board);
            }

            List<Move> legalMoves = GetAllLegalMoves(board);

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (Move move in legalMoves)
                {
                    Board newBoard = board.Clone();
                    newBoard.MakeMove(move);
                    int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, false);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (Move move in legalMoves)
                {
                    Board newBoard = board.Clone();
                    newBoard.MakeMove(move);
                    int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, true);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
        }

        private int EvaluateBoard(Board board)
        {
            int evaluation = 0;
            const int pawnValue = 100;
            const int knightValue = 320;
            const int bishopValue = 330;
            const int rookValue = 500;
            const int queenValue = 900;
            const int kingValue = 20000;

            for (int i = 0; i < 64; i++)
            {
                int piece = board[i];
                int pieceType = Piece.PieceType(piece);
                int pieceColor = Piece.Color(piece);
                int pieceValue = 0;

                switch (pieceType)
                {
                    case Piece.Pawn:
                        pieceValue = pawnValue;
                        break;
                    case Piece.Knight:
                        pieceValue = knightValue;
                        break;
                    case Piece.Bishop:
                        pieceValue = bishopValue;
                        break;
                    case Piece.Rook:
                        pieceValue = rookValue;
                        break;
                    case Piece.Queen:
                        pieceValue = queenValue;
                        break;
                    case Piece.King:
                        pieceValue = kingValue;
                        break;
                    default:
                        continue;
                }

                if (pieceColor == this.Color)
                {
                    evaluation += pieceValue;
                }
                else if (pieceColor != Piece.NoColor)
                {
                    evaluation -= pieceValue;
                }
            }

            return evaluation;
        }

        private List<Move> GetAllLegalMoves(Board board)
        {
            List<Move> legalMoves = new List<Move>();
            if (board.duckTurn)
            {
                LegalMoveGenerator.GenerateDuckMoves(ref legalMoves, board);
            }
            else
            {
                LegalMoveGenerator.GeneratePawnMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateKnightMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateBishopMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateRookMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateQueenMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateKingMoves(ref legalMoves, board);
            }
            return legalMoves;
        }
    }
}
