namespace Guru.Collection
{
    public interface ICollectionMeta
    {
        string CollectionName { get; }
        bool UseIDObfuscating { get; }
        IDictionary<string, ICollectionFieldMeta> FieldMap { get; }
    }

    public interface ICollectionFieldMeta
    {
        string FieldName { get; }
        bool Required { get; }
    }
}