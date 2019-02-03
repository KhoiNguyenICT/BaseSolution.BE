namespace BaseSolution.Core.Commons.Yesmarket.Support
{
    internal interface IValueComparer<in T>
    {
        bool Compare(T x, T y);
    }
}