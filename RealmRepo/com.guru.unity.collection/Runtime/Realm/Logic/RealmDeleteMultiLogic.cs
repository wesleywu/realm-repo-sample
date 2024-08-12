#nullable disable
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmDeleteMultiLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmDeleteMultiLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes DeleteMulti<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : IExtraFilterable where TRes : IDeleteMultiResponse, new() where TPo : RealmObject
        {
            TRes response = new TRes();

            try
            {
                var filterReq = RealmFilter.NewFilterRequest<TReq>(request);
                var realmObjects = realm.All<TPo>().ApplyFilterRequest(filterReq).ToList();
                // var realmObjects = realm.All<TPo>().Filter(RealmFilter.GetFilter(filterReq));
                if (!realmObjects.Any())
                {
                    response.SetMessage("删除多条记录失败 : 未找到记录。");
                    response.SetDeletedCount(0);

                    LoggerUtils.LogWarning("RealmDeleteMultiLogic", "DeleteMulti", $"删除多条记录失败 : 未找到{this._collectionMeta.CollectionName}符合条件的记录。");
                }
                else
                {
                    int count = realmObjects.Count();
                    foreach (var obj in realmObjects)
                    {
                        realm.Remove(obj);
                    }

                    response.SetMessage("删除多条记录成功。");
                    response.SetDeletedCount(count);

                    LoggerUtils.Verbose("RealmDeleteMultiLogic", "DeleteMulti", $"删除多条记录成功 : collectionName={this._collectionMeta.CollectionName}, deletedCount={count}");
                }
            }
            catch (Exception ex)
            {
                response.SetMessage($"删除多条记录失败 : {ex.Message}。");
                response.SetDeletedCount(0);

                LoggerUtils.LogException(ex, $"删除多条记录失败 : collectionName={this._collectionMeta.CollectionName}");
            }

            return response;
        }
    }
}