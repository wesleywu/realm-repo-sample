#nullable disable
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmUpdateLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmUpdateLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes Update<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : IRealmMarshalable, IUpdateRequest, IUpdateable<TPo> where TRes : IUpdateResponse, new() where TPo : RealmObject
        {
            TRes response = new TRes();

            try
            {
                var filterReq = RealmFilter.NewObjectIdFilter(request.GetId(), this._collectionMeta.UseIDObfuscating);
                TPo realmObject = realm.All<TPo>().ApplyFilterRequest(filterReq).FirstOrDefault();
                // var realmObject = realm.All<TPo>().Filter(RealmFilter.GetFilter(filterReq)).FirstOrDefault();
                if (realmObject == null)
                {
                    response.SetMatchedCount(0);
                    response.SetModifiedCount(0);

                    response.SetMessage($"更新记录失败 ：未找到{this._collectionMeta.CollectionName}主键为{request.GetId()}的记录");

                    LoggerUtils.LogWarning("RealmUpdateLogic", "Update", $"未找到{this._collectionMeta.CollectionName}主键为{request.GetId()}的记录");
                }
                else
                {
                    request.UpdateObject(realmObject);
                    request.SetUpdatedAt(DateTimeOffset.Now);

                    response.SetMessage("更新记录成功");
                    response.SetMatchedCount(1);
                    response.SetModifiedCount(1);

                    LoggerUtils.Verbose("RealmUpdateLogic", "Update", $"更新记录成功 : collectionName={this._collectionMeta.CollectionName}, id={request.GetId()}");
                }
            }
            catch (Exception ex)
            {
                response.SetMessage($"更新记录失败 ：{ex.Message}");
                response.SetMatchedCount(0);
                response.SetModifiedCount(0);

                LoggerUtils.LogException(ex, $"更新记录失败 : collectionName={this._collectionMeta.CollectionName}");
            }

            return response;
        }
    }
}