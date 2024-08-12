#nullable disable
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmDeleteLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmDeleteLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes Delete<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : IIdentifiable where TRes : IDeleteResponse, new() where TPo : RealmObject
        {
            TRes response = new TRes();

            try
            {
                var filterReq = RealmFilter.NewFilterRequest<TReq>(request);
                var realmObject = realm.All<TPo>().ApplyFilterRequest(filterReq).FirstOrDefault();
                // var realmObject = realm.All<TPo>().Filter(RealmFilter.GetFilter(filterReq)).FirstOrDefault();
                if (realmObject == null)
                {
                    response.SetMessage("删除记录失败 : 未找到记录。");
                    response.SetDeletedCount(0);

                    LoggerUtils.LogWarning("RealmDeleteLogic", "Delete", $"删除记录失败 : 未找到{this._collectionMeta.CollectionName}符合条件的记录。");
                }
                else
                {
                    realm.Remove(realmObject);
                    response.SetDeletedCount(1);

                    LoggerUtils.Verbose("RealmDeleteLogic", "Delete", $"删除记录成功 : collectionName={this._collectionMeta.CollectionName}");
                }
            }
            catch (Exception ex)
            {
                response.SetMessage($"删除记录失败 : {ex.Message}");
                response.SetDeletedCount(0);

                LoggerUtils.LogException(ex, $"删除记录失败 : collectionName={this._collectionMeta.CollectionName}");
            }

            return response;
        }
    }
}