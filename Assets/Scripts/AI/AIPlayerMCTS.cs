namespace DuckChess
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class AIPlayerMCTS : RealTimePlayer
    {
        public override string Type { get { return "AIPlayerMCTS"; } }
        public override int Color { get; set; }

        private Board board;
        private int simulationCount;
        private float timeLimit;

        public AIPlayerMCTS(Board board, int color, int simulationCount = 1000, float timeLimit = 5.0f)
        {
            this.board = board;
            this.Color = color;
            this.simulationCount = simulationCount;
            this.timeLimit = timeLimit;
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
            Node rootNode = new Node(null, Move.InvalidMove, board.Clone(), Color);
            float startTime = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - startTime < timeLimit)
            {
                Node node = rootNode;

                // Selection
                while (node.IsFullyExpanded && !node.IsTerminal)
                {
                    node = node.GetBestChild();
                }

                // Expansion
                if (!node.IsTerminal)
                {
                    node = node.Expand();
                }

                // Simulation
                int result = node.Simulate();

                // Backpropagation
                node.Backpropagate(result);
            }

            Node bestChild = rootNode.GetChildWithMostVisits();
            if (bestChild != null && bestChild.Move != Move.InvalidMove)
            {
                Debug.Log("AI Selected Move: " + bestChild.Move.ToString());
                return bestChild.Move;
            }
            else
            {
                // If no move found, pick a random legal move
                List<Move> legalMoves = GetAllLegalMoves(board);
                return legalMoves[UnityEngine.Random.Range(0, legalMoves.Count)];
            }
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

    public class Node
    {
        public Node Parent;
        public List<Node> Children;
        public Move Move;
        public Board BoardState;
        public int PlayerColor;
        public int Visits;
        public float Wins;
        public bool IsFullyExpanded;
        public bool IsTerminal;

        private List<Move> untriedMoves;
        private System.Random random = new System.Random();

        public Node(Node parent, Move move, Board boardState, int playerColor)
        {
            Parent = parent;
            Move = move;
            BoardState = boardState;
            PlayerColor = playerColor;
            Visits = 0;
            Wins = 0;
            Children = new List<Node>();
            IsTerminal = boardState.isGameOver;
            if (!IsTerminal)
            {
                untriedMoves = GetAllLegalMoves(boardState);
                IsFullyExpanded = untriedMoves.Count == 0;
            }
            else
            {
                untriedMoves = new List<Move>();
                IsFullyExpanded = true;
            }
        }

        public Node Expand()
        {
            Move move = untriedMoves[0];
            untriedMoves.RemoveAt(0);

            Board newBoard = BoardState.Clone();
            newBoard.MakeMove(move);

            Node childNode = new Node(this, move, newBoard, newBoard.turnColor);
            Children.Add(childNode);

            if (untriedMoves.Count == 0)
            {
                IsFullyExpanded = true;
            }

            return childNode;
        }

        public int Simulate()
        {
            Board simulationBoard = BoardState.Clone();
            int currentPlayer = simulationBoard.turnColor;

            while (!simulationBoard.isGameOver)
            {
                List<Move> possibleMoves = GetAllLegalMoves(simulationBoard);
                if (possibleMoves.Count == 0)
                    break;

                Move selectedMove = GetHeuristicMove(simulationBoard, possibleMoves, currentPlayer);
                simulationBoard.MakeMove(selectedMove);

                currentPlayer = simulationBoard.turnColor;
            }

            // Determine the result of the simulation
            if (simulationBoard.winnerColor == PlayerColor)
                return 1; // Win
            else if (simulationBoard.winnerColor == Piece.NoColor)
                return 0; // Draw
            else
                return -1; // Loss
        }

        public void Backpropagate(int result)
        {
            Visits++;
            Wins += result;

            if (Parent != null)
            {
                Parent.Backpropagate(-result);
            }
        }

        public Node GetBestChild()
        {
            Node bestChild = null;
            float bestValue = float.NegativeInfinity;

            foreach (Node child in Children)
            {
                float uctValue = (child.Wins / (child.Visits + 1e-6f)) +
                                 Mathf.Sqrt(2 * Mathf.Log(Visits + 1) / (child.Visits + 1e-6f));
                if (uctValue > bestValue)
                {
                    bestValue = uctValue;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        public Node GetChildWithMostVisits()
        {
            Node bestChild = null;
            int mostVisits = -1;

            foreach (Node child in Children)
            {
                if (child.Visits > mostVisits)
                {
                    mostVisits = child.Visits;
                    bestChild = child;
                }
            }

            return bestChild;
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

        // NEW METHOD: GetHeuristicMove
        private Move GetHeuristicMove(Board board, List<Move> moves, int currentPlayer)
        {
            // Assign a score to each move based on heuristic evaluation
            List<Tuple<Move, int>> scoredMoves = new List<Tuple<Move, int>>();
            foreach (var move in moves)
            {
                int score = EvaluateMove(board, move, currentPlayer);
                scoredMoves.Add(new Tuple<Move, int>(move, score));
            }

            // Sort moves based on the score (higher is better)
            scoredMoves.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            // Select the best move with some randomness
            int topN = Math.Min(5, scoredMoves.Count);
            int selectedIndex = random.Next(topN);
            return scoredMoves[selectedIndex].Item1;
        }

        // NEW METHOD: EvaluateMove
        private int EvaluateMove(Board board, Move move, int player)
        {
            int score = 0;

            int startPiece = board.Squares[move.StartSquare];
            int targetPiece = board.Squares[move.TargetSquare];

            int pieceType = Piece.PieceType(startPiece);
            int pieceColor = Piece.Color(startPiece);

            // Encourage capturing opponent's pieces
            if (Piece.Color(targetPiece) != Piece.NoColor && Piece.Color(targetPiece) != pieceColor)
            {
                int capturedPieceValue = GetPieceValue(Piece.PieceType(targetPiece));
                score += capturedPieceValue * 10; // Higher weight for captures
            }

            // Encourage advancing pawns
            if (pieceType == Piece.Pawn)
            {
                int direction = (pieceColor == Piece.White) ? 1 : -1;
                int startRow = Board.GetRowOf(move.StartSquare);
                int targetRow = Board.GetRowOf(move.TargetSquare);
                score += (targetRow - startRow) * direction * 2;
            }

            // Encourage central control
            int targetCol = Board.GetColumnOf(move.TargetSquare);
            int targetRowCentrality = (int)Math.Abs(3.5f - Board.GetRowOf(move.TargetSquare));
            int targetColCentrality = (int)Math.Abs(3.5f - targetCol);
            score += (int)(7 - (targetRowCentrality + targetColCentrality)); // Closer to center gets higher score

            // Penalize moving pieces to squares with duck
            if (board.Duck == move.TargetSquare)
            {
                score -= 50;
            }

            return score;
        }

        // NEW METHOD: GetPieceValue
        private int GetPieceValue(int pieceType)
        {
            switch (pieceType)
            {
                case Piece.Pawn: return 100;
                case Piece.Knight: return 320;
                case Piece.Bishop: return 330;
                case Piece.Rook: return 500;
                case Piece.Queen: return 900;
                case Piece.King: return 20000; // Arbitrary high value
                default: return 0;
            }
        }
    }
}
