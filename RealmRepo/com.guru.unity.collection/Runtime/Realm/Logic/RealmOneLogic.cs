#nullable disable
using System.Linq;
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmOneLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmOneLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes One<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : IPageable, IProjectable, IExtraFilterable where TRes : IOneResponse<TPo>, new() where TPo : RealmObject
        {
            var response = new TRes();
            try
            {
                var filterReq = RealmFilter.NewFilterRequest<TReq>(request);
                var projection = request.GetFieldsIncluded();
                TPo realmObject = realm.All<TPo>().ApplyFilterRequest(filterReq).ApplyProjection(projection).FirstOrDefault();
                //var realmObject = realm.All<TPo>().Filter(RealmFilter.GetFilter(filterReq)).FirstOrDefault();
                if (realmObject == null)
                {
                    response.SetFind(false);
                    LoggerUtils.LogWarning("RealmOneLogic", "One", $"单条记录查询失败 : 未找到{this._collectionMeta.CollectionName}符合条件的记录");
                }
                else
                {
                    // Todo: implement projector logic here.

                    // Freeze the object to avoid any further changes.
                    var frozenObject = realmObject.Freeze();
                    response.SetFind(true);
                    response.SetItem(frozenObject);

                    LoggerUtils.Verbose("RealmOneLogic", "One", $"单条记录查询成功 : collectionName={this._collectionMeta.CollectionName}");
                }
            }
            catch (QueryException ex)
            {
                LoggerUtils.LogException(ex, $"获取单条记录失败 : collectionName={this._collectionMeta.CollectionName}");
                response.SetFind(false);
            }

            return response;
        }
    }
}