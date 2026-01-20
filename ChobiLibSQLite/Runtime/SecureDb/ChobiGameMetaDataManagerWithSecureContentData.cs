
/*
using System;
using ChobiLib.Unity.Serializables;
using UnityEngine;

namespace ChobiLib.Unity.SQLite.SecureDb
{
    [RequireComponent(typeof(AbsChobiSQLiteMonoBehaviour))]
    public class ChobiGameMetaDataManagerWithSecureContentData : MonoBehaviour
    {
        private AbsChobiSQLiteMonoBehaviour _db;

        [SerializeField]
        private string contentId = "chobiGameMetaData";

        public ChobiGameMetaData GameMetaData { get; private set; }

        public DateTime StartDateTime { get; private set; }

        void Awake()
        {
            _db = GetComponent<AbsChobiSQLiteMonoBehaviour>();
        }

        void Start()
        {
            StartDateTime = DateTime.Now;
        }
    }
}
*/
