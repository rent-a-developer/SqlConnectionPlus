namespace RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

public struct NotAValueTuple : IStructuralEquatable, IStructuralComparable, IComparable
{
    /// <inheritdoc />
    public Int32 CompareTo(Object? other, IComparer comparer) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Int32 CompareTo(Object? obj) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Boolean Equals(Object? other, IEqualityComparer comparer) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Int32 GetHashCode(IEqualityComparer comparer) =>
        throw new NotImplementedException();
}
