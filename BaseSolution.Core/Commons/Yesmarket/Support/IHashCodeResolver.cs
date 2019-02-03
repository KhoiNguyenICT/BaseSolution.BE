namespace BaseSolution.Core.Commons.Yesmarket.Support
{
    internal interface IHashCodeResolver<in T>
    {
        int GetHashCodeFor(T obj);
    }
}