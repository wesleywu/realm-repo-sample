#nullable disable
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmGetLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmGetLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes Get<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : IIdentifiable, IProjectable where TRes : IRealmUnmarshalable, new() where TPo : RealmObject
        {
            var response = new TRes();
            try
            {
                var filterReq = RealmFilter.NewObjectIdFilter(request.GetId(), this._collectionMeta.UseIDObfuscating);
                var projection = request.GetFieldsIncluded();
                var realmObject = realm.All<TPo>().ApplyFilterRequest(filterReq).ApplyProjection(projection).FirstOrDefault();
                //var realmObject = realm.All<TPo>().Filter(RealmFilter.GetFilter(filterReq)).FirstOrDefault();
                if (realmObject == null)
                {
                    LoggerUtils.LogWarning("RealmGetLogic", "Get",  $"主键查询失败: 未找到{this._collectionMeta.CollectionName}主键为{request.GetId()}的记录");
                }
                else
                {
                    // Todo: implement projector logic here.

                    // Freeze the object to avoid any further changes.
                    var frozenObject = realmObject.Freeze();
                    response.FromRealmObject(frozenObject);

                    LoggerUtils.Verbose("RealmGetLogic", "Get",  $"主键查询成功: collectionName={this._collectionMeta.CollectionName}，key={request.GetId()}");
                }
            }
            catch (QueryException ex)
            {
                LoggerUtils.LogException(ex, $"主键查询失败 : collectionName={this._collectionMeta.CollectionName}，key={request.GetId()}");
            }

            return response;
        }
    }
}