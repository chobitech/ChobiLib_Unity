using ChobiLib.Unity.Realm;
using System;
using UnityEngine;

public class RealmTest : AbsRealmMonoBehaviour
{
    public override string RealmFileName => "realm_test.realm";

    public override Type[] SchemeTypes => new Type[]
    {
        typeof(RealmColor),
    };


    protected override void Awake()
    {
        base.Awake();

        DeleteAllRealm();

        _ = ChobiRealm;

        //Debug.Log(GetRealmFileFullPath());
    }

    void Start()
    {
        WithTransactionAsync<Color>(
            async r =>
            {
                var color = RealmColor.CreateNew(Color.cyan);
                r.Add(color);
                return color;
            },
            c =>
            {
                Debug.Log($"inserted color = {c}");
            }
        );
    }
}
