using System;
using SQLite.Attributes;

public class SecureDbContentData
{
    [PrimaryKey]
    public string ContentId { get; set; }

    [Indexed]
    public string TagString { get; set; }

    [Indexed]
    public int? TagInt { get; set; }


    public string Content { get; set; }

    public byte[] HKey { get; set; }

    public byte[] HData { get; set; }

    [Indexed]
    public DateTimeOffset CreateTimeOffsetUtc { get; set; }
}
