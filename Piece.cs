namespace ChessEngine {
	public class Piece {

		public Piece(PieceType type, bool isWhite) {
			Type = type;
			IsWhite = isWhite;
		}

		public PieceType Type { get; }
		public bool IsWhite { get; }
		public Point Position { get; set; }
	}
}
