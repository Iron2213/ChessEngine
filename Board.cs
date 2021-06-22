using System;
using System.Collections.Generic;

namespace ChessEngine {
	public class Board {
		public Tile[,] Tiles { get; set; }

		public bool WhiteToMove { get; set; } = true;

		public bool WhiteKSCastle { get; set; } = true;
		public bool WhiteQSCastle { get; set; } = true;

		public bool BlackKSCastle { get; set; } = true;
		public bool BlackQSCastle { get; set; } = true;

		public Point EnPassantTargetIndex { get; set; } = Point.Default;

		public int HalfmoveClock { get; set; }
		public int FullmoveNumber { get; set; }

		public List<Piece> BlackPieces { get; set; } = new List<Piece>();
		public List<Piece> WhitePieces { get; set; } = new List<Piece>();

		public List<Tile> OpponentControlledSquares { get; } = new List<Tile>();

		public Board() {
			Tiles = new Tile[8, 8];
			for (int x = 0; x < 8; x++) {
				for (int y = 0; y < 8; y++) {
					Tiles[x, y] = new Tile(new Point {
						Row = x,
						Column = y
					});
				}
			}
		}

		public Tile TileAt(Point position) {
			return Tiles[position.Row, position.Column];
		}

		public void PrintPosition() {

			Console.Clear();
			Console.WriteLine("§ Current position on the board §");
			Console.WriteLine("╔═══╦═══╦═══╦═══╦═══╦═══╦═══╦═══╗");
			Console.Write("║");
			for (int row = 0; row < 8; row++) {
				for (int column = 0; column < 8; column++) {
					Tile tile = Tiles[row, column];

					if (tile.Piece != null) {
						string pieceNotation = tile.Piece.Type switch {
							PieceType.Pawn => "p",
							PieceType.Bishop => "b",
							PieceType.Knight => "n",
							PieceType.Rook => "r",
							PieceType.Queen => "q",
							PieceType.King => "k",
							_ => " ",
						};
						if (tile.Piece.IsWhite)
							pieceNotation = pieceNotation.ToUpper();

						Console.Write($" {pieceNotation} ");
					}
					else {

						if (OpponentControlledSquares.Contains(tile)) {
							Console.Write(" ! ");
						}
						else
							Console.Write("   ");
					}

					Console.Write("║");
				}

				Console.WriteLine(" " + (8 - row));
				if (row == 7) {
					Console.WriteLine("╚═══╩═══╩═══╩═══╩═══╩═══╩═══╩═══╝");
					Console.WriteLine("  a   b   c   d   e   f   g   h  ");
				}
				else {
					Console.WriteLine("╠═══╬═══╬═══╬═══╬═══╬═══╬═══╬═══╣");
					Console.Write("║");
				}
			}
		}

		public void SetStartingPos(string FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {

			string[] FENFields = FEN.Split(' ');

			//
			// Field 1: Piece placement
			//
			int index = 0;
			for (int i = 0; i < FENFields[0].Length; i++) {

				switch (FEN[i]) {

					case '/':
						break;

					case char num when char.IsNumber(num): {
							index += int.Parse(num.ToString());
						}
						break;

					case char letter: {

							bool isWhite = char.IsUpper(letter);
							PieceType currentPiece = (char.ToLower(letter)) switch {
								'p' => PieceType.Pawn,
								'b' => PieceType.Bishop,
								'n' => PieceType.Knight,
								'r' => PieceType.Rook,
								'q' => PieceType.Queen,
								'k' => PieceType.King,
								_ => PieceType.None,
							};

							var newPiece = new Piece(currentPiece, isWhite);

							int column = index % 8;
							int row = index / 8;
							Tiles[row, column].Piece = newPiece;

							newPiece.Position = Tiles[row, column].Position;

							if (newPiece.IsWhite)
								WhitePieces.Add(newPiece);
							else
								BlackPieces.Add(newPiece);

							index++;
						}
						break;
				}
			}

			//
			// Field 2: Active color
			//
			WhiteToMove = FENFields[1].ToLower().Equals("w");

			//
			// Field 3: Castling rights
			//
			// Resetto tutto
			WhiteKSCastle = false;
			WhiteQSCastle = false;
			BlackKSCastle = false;
			BlackQSCastle = false;
			for (int i = 0; i < FENFields[2].Length; i++) {

				switch (FENFields[2][i]) {
					case 'K':
						WhiteKSCastle = true;
						break;
					case 'Q':
						WhiteQSCastle = true;
						break;
					case 'k':
						BlackKSCastle = true;
						break;
					case 'q':
						BlackQSCastle = true;
						break;
				}
			}

			//
			// Field 4: En passant target
			//
			if (!FENFields[3].Equals("-")) {
				EnPassantTargetIndex = Utility.IndexFromAN(FENFields[3]);
			}

			//
			// Field 5: Halfmove clock
			//
			HalfmoveClock = int.Parse(FENFields[4]);

			//
			// Field 6: Fullmove number
			//
			FullmoveNumber = int.Parse(FENFields[5]);
		}

		public void MovePiece(Tile currentTile, Tile targetTile) {
			var piece = currentTile.Piece;
			currentTile.Piece = null;
			targetTile.Piece = piece;
		}

		public void MakeMove(Move move) {
			WhiteToMove = !WhiteToMove;

			Tile currentTile = TileAt(move.CurrentPosition);
			Tile targetTile = TileAt(move.TargetPosition);

			var piece = currentTile.Piece;
			currentTile.Piece = null;

			piece.Position = currentTile.Position;

			if (move.Type == MoveType.Capture) {
				var opponentPieces = piece.IsWhite ? BlackPieces : WhitePieces;

				opponentPieces.Remove(targetTile.Piece);

				if (targetTile.Piece.Type == PieceType.Rook) {
					if (targetTile.Piece.IsWhite) {
						if (targetTile.Piece.Position.Equals(new Point(7, 0))) {
							WhiteQSCastle = false;
						}

						if (targetTile.Piece.Position.Equals(new Point(7, 7))) {
							WhiteKSCastle = false;
						}
					}
					else {
						if (targetTile.Piece.Position.Equals(new Point(0, 7))) {
							BlackQSCastle = false;
						}

						if (targetTile.Piece.Position.Equals(new Point(0, 7))) {
							BlackKSCastle = false;
						}
					}
				}
			}
			else if (move.Type == MoveType.EnPassant) {

			}

			if (move.PieceMoving == PieceType.King) {

				if (move.Type == MoveType.KSCastle) {
					var rookPosition = targetTile.Position + Engine.DirectionOffsets[Direction.Est];

					var rookTargetTile = TileAt(targetTile.Position + Engine.DirectionOffsets[Direction.West]);

					MovePiece(TileAt(rookPosition), rookTargetTile);
				}
				else if (move.Type == MoveType.QSCastle) {
					var rookPosition = targetTile.Position + (Engine.DirectionOffsets[Direction.West] * 2);

					var rookTargetTile = TileAt(targetTile.Position + Engine.DirectionOffsets[Direction.Est]);

					MovePiece(TileAt(rookPosition), rookTargetTile);
				}

				if (piece.IsWhite) {
					WhiteKSCastle = false;
					WhiteQSCastle = false;
				}
				else {
					BlackKSCastle = false;
					BlackQSCastle = false;
				}
			}

			targetTile.Piece = piece;
			EnPassantTargetIndex = Point.Default;
		}
	}
}
