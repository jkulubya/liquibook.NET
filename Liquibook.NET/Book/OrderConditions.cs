namespace Liquibook.NET.Book
{
    public enum OrderConditions
    {
        NoConditions = 0,
        AllOrNone = 1,
        ImmediateOrCancel = 2,
        FillOrKill = 3,
        Stop = 4
    }
}