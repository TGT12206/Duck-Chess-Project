namespace DuckChess
{
    using System;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;

    public class AlphaBetaAIPlayer : RealTimePlayer
    {
        public override string Type { get { return "AlphaBetaAIPlayer"; } }
        public override int Color { get; set; }

        private const int numActionsPerFrame = 10000;

        private Board board;

        private int maxDepth;
        private Board searchBoard;
        private Move bestMove;
        private bool startSearch;
        private List<Move> legalMoves;
        private Stack<AlphaBetaNode> alphaBetaNodes;
        public Stack<int> significantMoveCounters;
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
                bestMove = new Move();
                legalMoves = board.legalMoves;
                searchBoard = board.Clone();
                alphaBetaNodes = new Stack<AlphaBetaNode>();
                alphaBetaNodes.Push(new AlphaBetaNode(int.MinValue, int.MaxValue, true, new Move()));
                significantMoveCounters = new Stack<int>();
                currentDepth = 1;
                startSearch = false;
            }
            for (int i = 0; i < numActionsPerFrame; i++)
            {
                AlphaBetaNode currentNode = alphaBetaNodes.Peek();
                if (currentDepth < maxDepth && currentNode.indexLeftOffAt < legalMoves.Count)
                {
                    if (currentNode.ShouldPrune())
                    {
                        // If we should prune this subtree, then close and skip it
                        CloseNode();
                    } else
                    {
                        // Expand the next node
                        ExpandNode();
                    }
                } else
                {
                    if (currentDepth == 1)
                    {
                        // If we're back at the top and there are no more legal moves
                        ChooseMove(bestMove);
                        startSearch = true;
                        return;
                    } else
                    {
                        // No more moves in this subtree. Go up a layer
                        CloseNode();
                    }
                }
            }
        }

        private void ExpandNode()
        {
            currentDepth++;
            AlphaBetaNode parent = alphaBetaNodes.Peek();
            int indexOfNextMove = parent.indexLeftOffAt;
            Move nextMove = legalMoves[indexOfNextMove];
            bool isMaximizing = searchBoard.duckTurn ? !parent.isMaximizing : parent.isMaximizing;
            AlphaBetaNode node = new AlphaBetaNode(parent.alpha, parent.beta, isMaximizing, nextMove);
            searchBoard.MakeMove(ref nextMove);
            legalMoves = searchBoard.legalMoves;
            significantMoveCounters.Push(board.numPlySinceLastEvent);
            alphaBetaNodes.Push(node);
        }

        private void CloseNode()
        {
            currentDepth--;
            AlphaBetaNode node = alphaBetaNodes.Pop();
            AlphaBetaNode parent = alphaBetaNodes.Peek();
            Move moveFromParentToNode = node.moveFromParent;
            searchBoard.UnmakeMove(moveFromParentToNode, significantMoveCounters.Pop());
            legalMoves = searchBoard.legalMoves;
            if (currentDepth == maxDepth || searchBoard.isGameOver)
            {
                node.value = EvaluateBoard(searchBoard);
            }
            if (parent.JudgeNewValue(node.value, moveFromParentToNode) &&
                currentDepth == 1
            )
            {
                // If we have come back to the base node and
                // this new move is better than the previous best one
                bestMove = moveFromParentToNode;
            }
            parent.indexLeftOffAt++;
        }

        //private Move GetBestMove()
        //{
        //    int alpha = int.MinValue;
        //    int beta = int.MaxValue;
        //    Move bestMove = Move.InvalidMove;
        //    int maxEval = int.MinValue;

        //    List<Move> legalMoves = GetAllLegalMoves(board);

        //    foreach (Move move in legalMoves)
        //    {
        //        Debug.Log("Evaluating Move: " + move.ToString());

        //        Board newBoard = board.Clone();
        //        newBoard.MakeMove(move);
        //        int eval = AlphaBeta(newBoard, maxDepth - 1, alpha, beta, false);

        //        if (eval > maxEval)
        //        {
        //            maxEval = eval;
        //            bestMove = move;
        //        }
        //        alpha = Math.Max(alpha, eval);
        //    }

        //    Debug.Log("AI Selected Move: " + bestMove.ToString());
        //    return bestMove;
        //}


        //private int AlphaBeta(Board board, int alpha, int beta, bool maximizingPlayer)
        //{
        //    if (maximizingPlayer)
        //    {
        //        int maxEval = int.MinValue;
        //        foreach (Move move in legalMoves)
        //        {
        //            Board newBoard = board.Clone();
        //            newBoard.MakeMove(move);
        //            int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, false);
        //            maxEval = Math.Max(maxEval, eval);
        //            alpha = Math.Max(alpha, eval);
        //            if (beta <= alpha)
        //                break;
        //        }
        //        return maxEval;
        //    }
        //    else
        //    {
        //        int minEval = int.MaxValue;
        //        foreach (Move move in legalMoves)
        //        {
        //            Board newBoard = board.Clone();
        //            newBoard.MakeMove(move);
        //            int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, true);
        //            minEval = Math.Min(minEval, eval);
        //            beta = Math.Min(beta, eval);
        //            if (beta <= alpha)
        //                break;
        //        }
        //        return minEval;
        //    }
        //}

        private int EvaluateBoard(Board board)
        {
            if (board.isGameOver)
            {
                bool isWhite = Color == Piece.White;
                switch (board.winnerColor)
                {
                    case Piece.NoColor:
                        return 0;
                    case Piece.Black:
                        return isWhite ? int.MinValue : int.MaxValue;
                    case Piece.White:
                        return isWhite ? int.MaxValue : int.MinValue;
                }
            }
            int evaluation = 0;
            const int pawnValue = 100;
            const int knightValue = 320;
            const int bishopValue = 330;
            const int rookValue = 500;
            const int queenValue = 900;
            const int kingValue = 100;

            PieceList allPieces = board.AllPieces;

            for (int i = 0; i < allPieces.Count; i++)
            {
                int pieceLocation = allPieces[i];
                int piece = board[pieceLocation];
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
                else
                {
                    evaluation -= pieceValue;
                }
            }

            return evaluation;
        }

        public override void UnmakeMove()
        {
            throw new NotImplementedException();
        }
    }
}
