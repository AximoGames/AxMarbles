namespace AxEngine
{
    public enum MarbleColor
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 4,
        Yellow = 8,
        Orange = 16,
        White = 32,
        Black = 64,
        BombColor = 128,
        ColorJoker = Red | Green | Blue | Yellow | Orange | White | Black,
        BombJoker = ColorJoker | BombColor,
    }

}