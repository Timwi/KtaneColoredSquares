namespace ColoredSquares
{
    public enum SquareColor
    {
        White,
        Red,
        Blue,
        Green,
        Yellow,
        Magenta,
        Black
    }

    public enum SquaresToRecolor
    {
        All,
        NonwhiteOnly
    }

    // Discolored Squares only
    enum Instruction
    {
        MoveUpLeft,
        MoveUp,
        MoveUpRight,
        MoveRight,
        MoveDownRight,
        MoveDown,
        MoveDownLeft,
        MoveLeft,
        MirrorHorizontally,
        MirrorVertically,
        MirrorDiagonallyA1D4,
        MirrorDiagonallyA4D1,
        Rotate90CW,
        Rotate90CCW,
        Rotate180,
        Stay
    }
}
