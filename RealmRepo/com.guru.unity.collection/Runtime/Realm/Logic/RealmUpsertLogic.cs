#nullable disable
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmUpsertLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmUpsertLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes Upsert<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : IRealmMarshalable, IUpdateable<TPo>, IUpsertRequest where TRes : IUpsertResponse, new() where TPo : RealmObject
        {
            TRes response = new TRes();

            void insertNew()
            {
                var now = DateTimeOffset.Now;
                request.SetCreatedAt(now);
                request.SetUpdatedAt(now);
                string id = request.GenerateId();
                if (this._collectionMeta.UseIDObfuscating)
                {
                    // TODO: Implement ID obfuscation encode here.
                }

                var obj = request.ToRealmObject();
                realm.Add(obj);

                response.SetMessage("新增记录成功");
                response.SetUpsertedId(id);
                response.SetUpsertedCount(1);

                LoggerUtils.Verbose("RealmUpsertLogic", "Upsert", $"新增记录成功 : collectionName={this._collectionMeta.CollectionName}, id={id}");
            }

            void updateExisting(TPo realmObject)
            {
                request.UpdateObject(realmObject);
                request.SetUpdatedAt(DateTimeOffset.Now);

                response.SetMessage("更新记录成功");
                response.SetMatchedCount(1);
                response.SetModifiedCount(1);

                LoggerUtils.Verbose("RealmUpsertLogic", "Upsert", $"更新记录成功 : collectionName={this._collectionMeta.CollectionName}, id={request.GetId()}, modifiedCount={1}");
            }

            try
            {
                // 仅当传入参数没有设置 CreatedAt 值的时候，才设置 CreatedAt 为当前时间
                // 使用方可以通过是否设置 CreatedAt 值，来暗示当前 Upsert 操作是 Insert 还是 Update
                // 1. 如果 upsert req 未设置 CreatedAt，暗示要对新记录做 Insert
                // 2. 如果 upsert req 设置了 CreatedAt，往往这个值是从现有记录中获取到的，暗示要对现有记录做 Update
                // 所以，如果记录并非从数据库中获取，请不要设置其 CreatedAt 的值
                if (request.GetCreatedAt() == default)
                {
                    insertNew();
                }
                else
                {
                    var filterReq = RealmFilter.NewObjectIdFilter(request.GetId(), this._collectionMeta.UseIDObfuscating);
                    var realmObject = realm.All<TPo>().ApplyFilterRequest(filterReq).FirstOrDefault();
                    // var realmObject = realm.All<TPo>().Filter(RealmFilter.GetFilter(filterReq)).FirstOrDefault();
                    if (realmObject == null)
                    {
                        insertNew();
                    }
                    else
                    {
                        updateExisting(realmObject);
                    }
                }
            }
            catch (Exception ex)
            {
                response.SetUpsertedCount(0);
                response.SetMatchedCount(0);
                response.SetModifiedCount(0);

                LoggerUtils.LogException(ex, $"插入或更新记录失败 : collectionName={this._collectionMeta.CollectionName}");
            }

            return response;
        }
    }
}