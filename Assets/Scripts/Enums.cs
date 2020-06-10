namespace ColoredSquares
{
    public enum SquareColor
    {
        Black,
        White,

        Red,
        Blue,
        Green,
        Yellow,
        Magenta,

        // Juxtacolored Squares only
        Orange,
        Cyan,
        Purple,
        Chestnut,
        Brown,
        Mauve,
        Azure,
        Jade,
        Forest,
        Gray
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
