namespace ChobiLib.Unity.SQLite.SecureDb
{
    public interface IConvertibleToSecureDbContentData : IJsonable
    {
        public virtual SecureDbContentData ConvertToSecureDbContentData(string contentId = null, string tagString = null, int? tagInt = null)
        {
            return SecureDbContentData.CreateContentDataFromJson(
                ToJson(),
                contentId,
                tagString,
                tagInt
            );
        }

        
    }
}