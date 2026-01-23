using ChobiLib.Unity.SQLite.SecureDb;

public readonly struct AbsSecureDbContentDataHolder<T> where T : AbsSecureDbContentData
{
    public readonly T data;
    public readonly bool isHashCheckOk;

    public AbsSecureDbContentDataHolder(T data, bool isHashCheckOk)
    {
        this.data = data;
        this.isHashCheckOk = isHashCheckOk;
    }
}