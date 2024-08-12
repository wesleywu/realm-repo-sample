#nullable disable
using Guru.Collection.Orm;

namespace Guru.Collection.RealmDB
{
    /// <summary>
    /// Identifiable 标记一个结构体可以被 string 类型的 Id 属性唯一标识，即需要实现
    /// </summary>
    public interface IIdentifiable
    {
        string GetId();
    }
    /// <summary>
    /// 标记一个结构体需要实现 GenerateId 方法
    /// </summary>
    public interface IIdentifyGeneratable
    {
        string GenerateId();
    }
    /// <summary>
    /// ExtraFilterable 标记一个结构体需要实现 GetExtraFilters 方法
    /// </summary>
    public interface IExtraFilterable
    {
        PropertyFilter[] GetExtraFilters();
    }
    /// <summary>
    /// Pageable 标记一个结构体需实现 GetPageRequest 方法
    /// </summary>
    public interface IPageable
    {
        PageRequest GetPageRequest();
    }
    /// <summary>
    /// 标记一个结构体需实现 GetFieldsIncluded 方法
    /// </summary>
    public interface IProjectable
    {
        string[] GetFieldsIncluded();
    }

    public interface IUpdateable<in TPo>
    {
        void UpdateObject(TPo po);
    }

    /// <summary>
    /// CreateReq 标记一个结构体需要实现 IIdentifyGeneratable 接口, 以及 SetCreatedAt, SetUpdatedAt 方法
    /// </summary>
    public interface ICreationRequest : IIdentifyGeneratable
    {
        void SetCreatedAt(DateTimeOffset createdAt);
        void SetUpdatedAt(DateTimeOffset updatedAt);
    }
    /// <summary>
    /// UpdateReq 标记一个结构体需要实现 Identifiable 接口, 以及 SetCreatedAt, SetUpdatedAt 方法
    /// </summary>
    public interface IUpdateRequest : IIdentifiable
    {
        void SetCreatedAt(DateTimeOffset createdAt);
        void SetUpdatedAt(DateTimeOffset updatedAt);
    }
    /// <summary>
    /// UpsertReq 标记一个结构体需要实现 Identifiable 接口, 以及 GetCreatedAt, SetCreatedAt, SetUpdatedAt 方法
    /// </summary>
    public interface IUpsertRequest : IIdentifiable, IIdentifyGeneratable
    {
        DateTimeOffset GetCreatedAt();
        void SetCreatedAt(DateTimeOffset createdAt);
        void SetUpdatedAt(DateTimeOffset updatedAt);
    }

    /// <summary>
    /// CreateRes 标记一个结构体需要实现 SetMessage, SetInsertedID, SetInsertedCount 方法
    /// </summary>
    public interface ICreationResponse
    {
        void SetMessage(string message);                          // 操作结果的描述
        void SetInsertedId(string id);                            // 自动生成的主键_id的值，仅当执行了 insert 操作且自动生成了主键时才会返回，对于 ObjectId 类型的主键，返回的是 hex 格式的值，如果设置了ID混淆，会返回混淆后的结果
        void SetInsertCount(long count);                          // 被插入的记录条数
    }
    /// <summary>
    /// UpdateRes 标记一个结构体需要实现 SetMessage, SetMatchedCount, SetModifiedCount 方法
    /// </summary>
    public interface IUpdateResponse
    {
        void SetMessage(string message);                         // 操作结果的描述
        void SetMatchedCount(long count);                        // 主键匹配的记录条数
        void SetModifiedCount(long count);                        // 被修改的记录条数
    }
    /// <summary>
    /// UpsertRes 标记一个结构体需要实现 SetMessage, SetUpsertedId, SetMatchedCount, SetModifiedCount, SetUpsertedCount 方法
    /// </summary>
    public interface IUpsertResponse
    {
        void SetMessage(string message);                         // 操作结果的描述
        void SetUpsertedId(string id);                           // 自动生成的主键_id的值，仅当执行了 insert 操作且自动生成了主键时才会返回，对于 ObjectId 类型的主键，返回的是 hex 格式的值，如果设置了ID混淆，会返回混淆后的结果
        void SetMatchedCount(long count);                        // 主键匹配的记录条数（对于 insert 操作为0，对于 update 操作为1）
        void SetModifiedCount(long count);                        // 被修改的记录条数（对于 insert 操作为0，对于 update 操作为1）
        void SetUpsertedCount(long count);                       // 被插入的记录条数（对于 insert 操作为1，对于 update 操作为2）
    }
    /// <summary>
    /// CountRes 标记一个结构体需要实现 SetTotalElements 方法
    /// </summary>
    public interface ICountResponse
    {
        void SetTotalElements(long totalElements);               // 查询条件命中的记录总数
    }
    /// <summary>
    /// OneRes 标记一个结构体需要实现 SetFound, SetItem 方法
    /// </summary>
    /// <typeparam name="TPo"> 数据库对象类型 </typeparam>
    public interface IOneResponse<in TPo>
    {
        void SetFind(bool find);                                  // 查询条件是否命中记录
        void SetItem(TPo item);                                  // 查询条件命中的记录
    }
    /// <summary>
    /// ListRes 标记一个结构体需要实现 SetPageInfo, SetItems 方法
    /// </summary>
    /// <typeparam name="TPo"> 数据库对象类型 </typeparam>
    public interface IListResponse<in TPo>
    {
        void SetPageInfo(PageInfo pageInfo);                      // 页码、记录数信息
        void SetItems(TPo[] items);                               // 当前页记录数组
    }
    /// <summary>
    /// DeleteRes 标记一个结构体需要实现 SetMessage, SetDeletedCount 方法
    /// </summary>
    public interface IDeleteResponse
    {
        void SetMessage(string message);                        // 操作结果的描述
        void SetDeletedCount(long count);                       // 被删除的记录条数
    }
    /// <summary>
    /// DeleteMultiRes 标记一个结构体需要实现 SetMessage, SetDeletedCount 方法
    /// </summary>
    public interface IDeleteMultiResponse
    {
        void SetMessage(string message);                        // 操作结果的描述
        void SetDeletedCount(long count);                       // 被删除的记录条数
    }
}