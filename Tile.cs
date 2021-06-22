using System;

namespace ChessEngine {
	public class Tile {

		public Tile(Point position) {
			Position = position;
			Piece = null;

			IsLightSquare = (Position.Row + Position.Column) % 2 == 0;
		}

		public Piece Piece { get; set; }

		public Point Position { get; }

		public bool IsLightSquare { get; }

		public string GetPositionNotation() {
			return (char)('a' + Position.Column) + "" + (8 - Position.Row);
		}

		public bool HasPiece() {
			return Piece != null;
		}

		public bool CanMoveTo(Piece piece) {

			if (piece.Type == PieceType.Pawn) {
				return !HasPiece();
			}

			return !HasPiece() || CanTake(piece.IsWhite);
		}

		public bool CanTake(bool IsWhite) {
			return HasPiece() && Piece.IsWhite != IsWhite;
		}
	}
}
