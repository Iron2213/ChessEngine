using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessEngine {

	class Program {
		static void Main(string[] args) {
			new Engine().Start();

			_ = Console.Read();
		}
	}

	public enum PieceType {
		None = 0,
		Pawn = 1,
		Bishop = 2,
		Knight = 3,
		Rook = 4,
		Queen = 5,
		King = 6
	}

	public enum MoveType {
		Movement,
		Capture,
		KSCastle,
		QSCastle,
		EnPassant
	}

	public struct Point {
		public static Point Default = new Point(-1, -1);

		public Point(int row, int column) {
			Row = row;
			Column = column;
		}
		public int Row { get; set; }
		public int Column { get; set; }

		public static Point operator +(Point a, Point b) {
			return new Point {
				Row = a.Row + b.Row,
				Column = a.Column + b.Column
			};
		}

		public static Point operator *(Point a, int b) {
			return new Point {
				Row = a.Row * b,
				Column = a.Column * b
			};
		}

		public override bool Equals(object obj) {
			if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
				return false;
			}
			else {
				Point p = (Point)obj;
				return (Row == p.Row) && (Column == p.Column);
			}
		}

		public override int GetHashCode() {
			return Row.GetHashCode() ^ Column.GetHashCode();
		}
	}

	public static class Utility {
		/// <summary>
		/// Converte un notazione algebrica standard in una posizione valida in un array con Length di 64.
		/// </summary>
		/// <returns></returns>
		public static Point IndexFromAN(string position) {

			if (position == null || position.Length != 2)
				throw new ArgumentException("Must provide correct Algebraic notation");

			int row = 8 - int.Parse(position[1].ToString());
			int column = 'a' - position[0];
			return new Point(row, column);
		}

		public static string GetPositionNotation(Point Position) {
			return (char)('a' + Position.Column) + "" + (8 - Position.Row);
		}
	}

	public struct Move {
		public PieceType PieceMoving { get; set; }
		public Point CurrentPosition { get; set; }
		public Point TargetPosition { get; set; }
		public MoveType Type { get; set; }

		public static bool IsInsideBounds(Point targetPosition) {
			return targetPosition.Row >= 0
				&& targetPosition.Row <= 7
				&& targetPosition.Column >= 0
				&& targetPosition.Column <= 7;
		}
	}

	public enum Direction {
		North,
		South,
		West,
		Est,
		NorthWest,
		NorthEst,
		SouthWest,
		SouthEst
	}

	public class Engine {

		public static readonly Dictionary<Direction, Point> DirectionOffsets = new Dictionary<Direction, Point>(8);

		private readonly Point[] KnightOffsets = {
			new Point(-2, -1),	// North-West
			new Point(-2, 1),	// North-Est
			new Point(-1, 2),	// Est-North
			new Point(1, 2),	// Est-South
			new Point(2, 1),	// South-Est
			new Point(2, -1),	// South-West
			new Point(1, -2),	// West-South
			new Point(-1, -2),	// West-North
		};

		private Board Board { get; set; } = new Board();

		public void Start() {

			DirectionOffsets.Add(Direction.North, new Point(-1, 0));
			DirectionOffsets.Add(Direction.South, new Point(1, 0));
			DirectionOffsets.Add(Direction.West, new Point(0, -1));
			DirectionOffsets.Add(Direction.Est, new Point(0, 1));
			DirectionOffsets.Add(Direction.NorthWest, new Point(-1, -1));
			DirectionOffsets.Add(Direction.NorthEst, new Point(-1, 1));
			DirectionOffsets.Add(Direction.SouthWest, new Point(1, -1));
			DirectionOffsets.Add(Direction.SouthEst, new Point(1, 1));

			// Board.SetStartingPos("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -");

			Board.SetStartingPos();

			var moves = GeneratePseudoLegalMoves();
			//List<Move> moves = null;
			//Random random = new Random();
			//Console.WriteLine("§ Played moves §");
			//for (int i = 0; i < 16; i++) {
			//	moves = GeneratePseudoLegalMoves();
			//	int index = random.Next(0, moves.Count);

			//	var move = moves[index];
			//	Board.MakeMove(move);

			//	Console.Write(move.PieceMoving + ": ");
			//	Console.Write(Utility.GetPositionNotation(move.CurrentPosition));
			//	Console.Write(" -> ");

			//	string moveText = move.Type switch {
			//		MoveType.Capture => "x" + Utility.GetPositionNotation(move.TargetPosition),
			//		MoveType.KSCastle => "O-O",
			//		MoveType.QSCastle => "O-O-O",
			//		_ => Utility.GetPositionNotation(move.TargetPosition),
			//	};

			//	Console.WriteLine(moveText);

			//	Console.WriteLine();
			//	Board.PrintPosition();
			//	Console.WriteLine();
			//}

			//Board.UnmakeMove();

			Board.PrintPosition();

			try {
				Console.WriteLine("Result: " + Perft(2));
			}
			catch (Exception) {
				Board.PrintPosition();
			}
			


			//Random random = new Random();

			//for (int i = 0; i < 8; i++) {
			//	var moves = GeneratePseudoLegalMoves();

			//	if (moves.Count == 0)
			//		break;

			//	int moveIndex = random.Next(0, moves.Count);

			//	Board.MakeMove(moves[moveIndex]);
			//	Board.PrintPosition();
			//}

			//Board.PrintPosition();



			//Board.MakeMove(moves.Find(i => i.Type == MoveType.QSCastle));

			//Board.PrintPosition();

			//Board.UnmakeMove();

			//Board.PrintPosition();

			//moves = GeneratePseudoLegalMoves();

			//Console.WriteLine();
			//Console.WriteLine("§ Pseudo-legal moves §");
			//foreach (var move in moves) {
			//	Console.Write(move.PieceMoving + ": ");
			//	Console.Write(Utility.GetPositionNotation(move.CurrentPosition));
			//	Console.Write(" -> ");

			//	string moveText = move.Type switch {
			//		MoveType.Capture => "x" + Utility.GetPositionNotation(move.TargetPosition),
			//		MoveType.KSCastle => "O-O",
			//		MoveType.QSCastle => "O-O-O",
			//		_ => Utility.GetPositionNotation(move.TargetPosition),
			//	};

			//	Console.WriteLine(moveText);
			//}
		}



		public ulong Perft(int depth) {

			if (depth == 0)
				return 1;

			var moves = GeneratePseudoLegalMoves();
			ulong nodes = 0;

			for (int i = 0; i < moves.Count; i++) {

				Board.MakeMove(moves[i]);

				if (!IsInCheck(Board.WhiteToMove)) {
					nodes += Perft(depth - 1);
				}

				Board.PrintPosition();
				Board.UnmakeMove();

			}

			return nodes;
		}








		public List<Move> GeneratePseudoLegalMoves() {
			List<Move> moves = new List<Move>();

			CalculateAttackedSquares();

			var currentMovingPieces = Board.WhiteToMove ? Board.WhitePieces : Board.BlackPieces;

			foreach (var piece in currentMovingPieces) {
				switch (piece.Type) {
					case PieceType.Pawn:
						moves.AddRange(GetPawnMoves(piece, piece.Position));
						break;

					case PieceType.Bishop:
						moves.AddRange(GetBishopMoves(piece, piece.Position));
						break;

					case PieceType.Knight:
						moves.AddRange(GetKnightMoves(piece, piece.Position));
						break;

					case PieceType.Rook:
						moves.AddRange(GetRookMoves(piece, piece.Position));
						break;

					case PieceType.Queen:
						moves.AddRange(GetQueenMoves(piece, piece.Position));
						break;

					case PieceType.King:
						moves.AddRange(GetKingMoves(piece, piece.Position));
						break;
				}
			}

			return moves;
		}

		public List<Move> GetPawnMoves(Piece piece, Point position) {
			List<Move> pawnMoves = new List<Move>();

			Point movementOffset = piece.IsWhite
				? DirectionOffsets[Direction.North]
				: DirectionOffsets[Direction.South];

			// If i'm in the starting square i check if i can move 2 square in the given direction
			bool canMoveTwoSquares = piece.IsWhite
				? position.Row == 6
				: position.Row == 1;

			Point newPosition = position + movementOffset;

			if (position.Row == 6 && position.Column == 0)
				_ = 1;

			if (Move.IsInsideBounds(newPosition)) {

				if (Board.TileAt(newPosition).CanMoveTo(piece))
					pawnMoves.Add(new Move() {
						Type = MoveType.Movement,
						PieceMoving = piece.Type,
						CurrentPosition = position,
						TargetPosition = newPosition
					});

				if (canMoveTwoSquares) {

					newPosition = position + (movementOffset * 2);

					if (Move.IsInsideBounds(newPosition)) {

						if (Board.TileAt(newPosition).CanMoveTo(piece))
							pawnMoves.Add(new Move() {
								Type = MoveType.Movement,
								PieceMoving = piece.Type,
								CurrentPosition = position,
								TargetPosition = newPosition
							});
					}
				}

				var targetPosition = position + (piece.IsWhite ? DirectionOffsets[Direction.NorthWest] : DirectionOffsets[Direction.SouthWest]);
				if (Move.IsInsideBounds(targetPosition)) {
					var targetTile = Board.TileAt(targetPosition);

					if (targetTile.CanTake(piece.IsWhite)) {

						pawnMoves.Add(new Move() {
							Type = MoveType.Capture,
							PieceMoving = piece.Type,
							CurrentPosition = position,
							TargetPosition = targetPosition
						});
					}
					else if (Board.EnPassantTargetIndex.Equals(targetPosition)) {
						pawnMoves.Add(new Move() {
							Type = MoveType.EnPassant,
							PieceMoving = piece.Type,
							CurrentPosition = position,
							TargetPosition = targetPosition
						});
					}
				}

				targetPosition = position + (piece.IsWhite ? DirectionOffsets[Direction.NorthEst] : DirectionOffsets[Direction.SouthEst]);
				if (Move.IsInsideBounds(targetPosition)) {
					var targetTile = Board.TileAt(targetPosition);

					if (targetTile.CanTake(piece.IsWhite)) {
						pawnMoves.Add(new Move() {
							Type = MoveType.Capture,
							PieceMoving = piece.Type,
							CurrentPosition = position,
							TargetPosition = targetPosition
						});
					}
					else if (Board.EnPassantTargetIndex.Equals(targetPosition)) {
						pawnMoves.Add(new Move() {
							Type = MoveType.EnPassant,
							PieceMoving = piece.Type,
							CurrentPosition = position,
							TargetPosition = targetPosition
						});
					}
				}
			}

			return pawnMoves;
		}

		public List<Move> GetKnightMoves(Piece piece, Point position) {
			List<Move> knightMoves = new List<Move>();

			foreach (var positionOffset in KnightOffsets) {
				Point targetPosition = position + positionOffset;

				if (Move.IsInsideBounds(targetPosition)) {

					Tile targetTile = Board.TileAt(targetPosition);

					if (targetTile.CanMoveTo(piece)) {
						knightMoves.Add(new Move() {
							Type = targetTile.CanTake(piece.IsWhite) ? MoveType.Capture : MoveType.Movement,
							PieceMoving = piece.Type,
							CurrentPosition = position,
							TargetPosition = targetPosition
						});
					}
				}
			}

			return knightMoves;
		}

		public List<Move> GetBishopMoves(Piece piece, Point position) {
			List<Move> bishopMoves = new List<Move>();

			foreach (var item in DirectionOffsets) {
				if (item.Key < Direction.NorthWest) {
					continue;
				}

				for (int i = 1; i <= 8; i++) {
					var targetPosition = position + (item.Value * i);
					if (Move.IsInsideBounds(targetPosition)) {

						Tile targetTile = Board.TileAt(targetPosition);

						if (!targetTile.HasPiece()) {
							bishopMoves.Add(new Move() {
								Type = MoveType.Movement,
								PieceMoving = piece.Type,
								CurrentPosition = position,
								TargetPosition = targetPosition
							});
						}
						else if (targetTile.CanTake(piece.IsWhite)) {
							bishopMoves.Add(new Move() {
								Type = MoveType.Capture,
								PieceMoving = piece.Type,
								CurrentPosition = position,
								TargetPosition = targetPosition
							});

							break;
						}
						else {
							break;
						}
					}
					else {
						break;
					}
				}
			}

			return bishopMoves;
		}

		public List<Move> GetRookMoves(Piece piece, Point position) {
			List<Move> rookMoves = new List<Move>();

			foreach (var item in DirectionOffsets) {
				if (item.Key >= Direction.NorthWest) {
					continue;
				}

				for (int i = 1; i <= 8; i++) {
					var targetPosition = position + (item.Value * i);
					if (Move.IsInsideBounds(targetPosition)) {

						Tile targetTile = Board.TileAt(targetPosition);

						if (!targetTile.HasPiece()) {
							rookMoves.Add(new Move() {
								Type = MoveType.Movement,
								PieceMoving = piece.Type,
								CurrentPosition = position,
								TargetPosition = targetPosition
							});
						}
						else if (targetTile.CanTake(piece.IsWhite)) {
							rookMoves.Add(new Move() {
								Type = MoveType.Capture,
								PieceMoving = piece.Type,
								CurrentPosition = position,
								TargetPosition = targetPosition
							});

							break;
						}
						else {
							break;
						}
					}
					else {
						break;
					}
				}
			}

			return rookMoves;
		}

		public List<Move> GetQueenMoves(Piece piece, Point position) {
			List<Move> queenMoves = new List<Move>();

			foreach (var item in DirectionOffsets) {
				for (int i = 1; i <= 8; i++) {
					var targetPosition = position + (item.Value * i);
					if (Move.IsInsideBounds(targetPosition)) {

						Tile targetTile = Board.TileAt(targetPosition);

						if (!targetTile.HasPiece()) {
							queenMoves.Add(new Move() {
								Type = MoveType.Movement,
								PieceMoving = piece.Type,
								CurrentPosition = position,
								TargetPosition = targetPosition
							});
						}
						else if (targetTile.CanTake(piece.IsWhite)) {
							queenMoves.Add(new Move() {
								Type = MoveType.Capture,
								PieceMoving = piece.Type,
								CurrentPosition = position,
								TargetPosition = targetPosition
							});

							break;
						}
						else {
							break;
						}
					}
					else {
						break;
					}
				}
			}

			return queenMoves;
		}

		public List<Move> GetKingMoves(Piece piece, Point position) {
			List<Move> kingMoves = new List<Move>();

			Point targetPosition;
			foreach (var offset in DirectionOffsets) {

				targetPosition = position + offset.Value;

				if (Move.IsInsideBounds(targetPosition)) {
					Tile targetTile = Board.TileAt(targetPosition);

					if (targetTile.CanMoveTo(piece) && !Board.OpponentControlledSquares.Contains(targetTile)) {
						kingMoves.Add(new Move() {
							Type = targetTile.CanTake(piece.IsWhite) ? MoveType.Capture : MoveType.Movement,
							PieceMoving = piece.Type,
							CurrentPosition = position,
							TargetPosition = targetPosition
						});
					}
				}
			}

			if (piece.IsWhite) {
				if (Board.WhiteKSCastle) {

					for (int i = 1; i <= 2; i++) {
						targetPosition = position + (DirectionOffsets[Direction.Est] * i);

						Tile targetTile = Board.TileAt(targetPosition);
						if (!targetTile.HasPiece() && !Board.OpponentControlledSquares.Contains(targetTile)) {

							if (i == 2)
								kingMoves.Add(new Move() {
									Type = MoveType.KSCastle,
									PieceMoving = piece.Type,
									CurrentPosition = position,
									TargetPosition = targetPosition
								});
						}
						else {
							break;
						}
					}
				}

				if (Board.WhiteQSCastle) {
					for (int i = 1; i <= 3; i++) {
						targetPosition = position + (DirectionOffsets[Direction.West] * i);

						Tile targetTile = Board.TileAt(targetPosition);
						if (!targetTile.HasPiece() && (i == 3 || !Board.OpponentControlledSquares.Contains(targetTile))) {

							if (i == 3)
								kingMoves.Add(new Move() {
									Type = MoveType.QSCastle,
									PieceMoving = piece.Type,
									CurrentPosition = position,
									TargetPosition = targetPosition + DirectionOffsets[Direction.Est]
								});
						}
						else {
							break;
						}
					}
				}
			}
			else {
				if (Board.BlackKSCastle) {

					for (int i = 1; i <= 2; i++) {
						targetPosition = position + (DirectionOffsets[Direction.Est] * i);

						Tile targetTile = Board.TileAt(targetPosition);
						if (!targetTile.HasPiece() && !Board.OpponentControlledSquares.Contains(targetTile)) {

							if (i == 2)
								kingMoves.Add(new Move() {
									Type = MoveType.KSCastle,
									PieceMoving = piece.Type,
									CurrentPosition = position,
									TargetPosition = targetPosition,
								});
						}
						else {
							break;
						}
					}
				}

				if (Board.BlackQSCastle) {
					for (int i = 1; i <= 3; i++) {
						targetPosition = position + (DirectionOffsets[Direction.West] * i);

						Tile targetTile = Board.TileAt(targetPosition);
						if (!targetTile.HasPiece() && (i == 3 || !Board.OpponentControlledSquares.Contains(targetTile))) {

							if (i == 3)
								kingMoves.Add(new Move() {
									Type = MoveType.QSCastle,
									PieceMoving = piece.Type,
									CurrentPosition = position,
									TargetPosition = targetPosition + DirectionOffsets[Direction.Est]
								});
						}
						else {
							break;
						}
					}
				}
			}

			return kingMoves;
		}

		public void CalculateAttackedSquares() {
			var attackingPieces = Board.WhiteToMove ? Board.BlackPieces : Board.WhitePieces;

			Board.OpponentControlledSquares.Clear();

			foreach (var piece in attackingPieces) {

				switch (piece.Type) {
					case PieceType.Pawn: {
							var targetPosition = piece.Position + (piece.IsWhite ? DirectionOffsets[Direction.NorthWest] : DirectionOffsets[Direction.SouthWest]);
							if (Move.IsInsideBounds(targetPosition)) {
								var targetTile = Board.TileAt(targetPosition);

								if (!targetTile.HasPiece() || targetTile.CanTake(piece.IsWhite)) {
									Board.OpponentControlledSquares.Add(targetTile);
								}
							}

							targetPosition = piece.Position + (piece.IsWhite ? DirectionOffsets[Direction.NorthEst] : DirectionOffsets[Direction.SouthEst]);
							if (Move.IsInsideBounds(targetPosition)) {
								var targetTile = Board.TileAt(targetPosition);

								if (!targetTile.HasPiece() || targetTile.CanTake(piece.IsWhite)) {
									Board.OpponentControlledSquares.Add(targetTile);
								}
							}
						}
						break;

					case PieceType.Bishop: {
							foreach (var item in DirectionOffsets) {
								if (item.Key < Direction.NorthWest) {
									continue;
								}

								for (int i = 1; i <= 8; i++) {
									var targetPosition = piece.Position + (item.Value * i);
									if (Move.IsInsideBounds(targetPosition)) {

										Tile targetTile = Board.TileAt(targetPosition);

										if (!targetTile.HasPiece()) {
											Board.OpponentControlledSquares.Add(targetTile);
										}
										else if (targetTile.CanTake(piece.IsWhite)) {
											Board.OpponentControlledSquares.Add(targetTile);

											break;
										}
										else {
											break;
										}
									}
									else {
										break;
									}
								}
							}
						}
						break;

					case PieceType.Knight: {
							foreach (var positionOffset in KnightOffsets) {
								Point targetPosition = piece.Position + positionOffset;

								if (Move.IsInsideBounds(targetPosition)) {

									Tile targetTile = Board.TileAt(targetPosition);

									if (targetTile.CanMoveTo(piece)) {
										Board.OpponentControlledSquares.Add(targetTile);
									}
								}
							}
						}
						break;

					case PieceType.Rook: {
							foreach (var item in DirectionOffsets) {
								if (item.Key >= Direction.NorthWest) {
									continue;
								}

								for (int i = 1; i <= 8; i++) {
									var targetPosition = piece.Position + (item.Value * i);
									if (Move.IsInsideBounds(targetPosition)) {

										Tile targetTile = Board.TileAt(targetPosition);

										if (!targetTile.HasPiece()) {
											Board.OpponentControlledSquares.Add(targetTile);
										}
										else if (targetTile.CanTake(piece.IsWhite)) {
											Board.OpponentControlledSquares.Add(targetTile);

											break;
										}
										else {
											break;
										}
									}
									else {
										break;
									}
								}
							}
						}
						break;

					case PieceType.Queen: {
							foreach (var item in DirectionOffsets) {
								for (int i = 1; i <= 8; i++) {
									var targetPosition = piece.Position + (item.Value * i);
									if (Move.IsInsideBounds(targetPosition)) {

										Tile targetTile = Board.TileAt(targetPosition);

										if (!targetTile.HasPiece()) {
											Board.OpponentControlledSquares.Add(targetTile);
										}
										else if (targetTile.CanTake(piece.IsWhite)) {
											Board.OpponentControlledSquares.Add(targetTile);

											break;
										}
										else {
											break;
										}
									}
									else {
										break;
									}
								}
							}
						}
						break;

					case PieceType.King: {
							foreach (var offset in DirectionOffsets) {

								Point targetPosition = piece.Position + offset.Value;

								if (Move.IsInsideBounds(targetPosition)) {
									Tile targetTile = Board.TileAt(targetPosition);

									if (targetTile.CanMoveTo(piece)) {
										Board.OpponentControlledSquares.Add(targetTile);
									}
								}
							}
						}
						break;
				}

			}
		}

		public bool IsInCheck(bool whiteToMove) {
			var opponentsPieces = whiteToMove ? Board.BlackPieces : Board.WhitePieces;

			var king = opponentsPieces.Find(i => i.Type == PieceType.King);

			foreach (var item in DirectionOffsets) {
				for (int i = 1; i <= 8; i++) {
					var targetPosition = king.Position + (item.Value * i);
					if (Move.IsInsideBounds(targetPosition)) {

						Tile targetTile = Board.TileAt(targetPosition);

						if (targetTile.HasPiece()) {
							if (targetTile.CanTake(king.IsWhite)) {
								return true;
							}
							else {
								break;
							}
						}
					}
					else {
						break;
					}
				}
			}

			return false;
		}
	}
}
