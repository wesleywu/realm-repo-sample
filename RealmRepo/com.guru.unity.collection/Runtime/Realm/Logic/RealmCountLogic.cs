#nullable disable
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmCountLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmCountLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes Count<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : IExtraFilterable where TRes : ICountResponse, new() where TPo : RealmObject
        {
            TRes response = new TRes();

            try
            {
                var filterReq = RealmFilter.NewFilterRequest<TReq>(request);
                var realmObjects = realm.All<TPo>().ApplyFilterRequest(filterReq);
                //var realmObjects = realm.All<TPo>().Filter(RealmFilter.GetFilter(filterReq));
                int count = realmObjects.Count();
                response.SetTotalElements(count);

                LoggerUtils.Verbose("RealmCountLogic", "Count", $"获取记录总数成功 : collectionName={_collectionMeta.CollectionName}, count={count}");
            }
            catch (Exception ex)
            {
                LoggerUtils.LogException(ex, $"获取记录总数失败 : collectionName={_collectionMeta.CollectionName}");
                response.SetTotalElements(0);
            }

            return response;
        }
    }
}