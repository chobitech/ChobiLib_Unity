using Chobitech.Realm;
using System;

public class RealmTest : AbsRealmMonoBehaviour
{
    public override string RealmFileName => "realm_test.realm";

    public override Type[] SchemeTypes => new Type[]
    {
        typeof(RealmDateTime),
        typeof(RealmColor),
    };


    void Awake()
    {
        DeleteAllRealm();

        _ = ChobiRealm;

        //Debug.Log(GetRealmFileFullPath());
    }
}
