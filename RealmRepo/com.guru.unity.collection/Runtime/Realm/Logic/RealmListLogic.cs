#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Guru.Collection.Orm;
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmListLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmListLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes List<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : IPageable, IProjectable, IExtraFilterable where TRes : IListResponse<TPo>, new() where TPo : RealmObject
        {
            TRes response = new TRes();

            try
            {
                var filterReq = RealmFilter.NewFilterRequest<TReq>(request);
                var pageableReq = request.GetPageRequest() ?? new PageRequest().FillDefaultPageRequest();
                var projection = request.GetFieldsIncluded();
                var result = realm.All<TPo>().ApplyFilterRequest(filterReq, request.GetExtraFilters()).ApplyProjection(projection).ApplyPageRequest(pageableReq);
                //var result = realm.All<TPo>().Filter(RealmFilter.GetFilter(filterReq)).ApplyPageRequest(pageableReq);
                TPo[] items = new TPo[result.Results.Count];
                for (int i = 0; i < result.Results.Count; i++)
                {
                    items[i] = result.Results[i].Freeze();
                }

                response.SetPageInfo(result.PageInfo);
                response.SetItems(items);

                LoggerUtils.Verbose("RealmListLogic", "List", $"列表查询成功 : collectionName={_collectionMeta.CollectionName}");
            }
            catch (Exception ex)
            {
                LoggerUtils.LogException(ex, $"列表查询失败 : collectionName={_collectionMeta.CollectionName}");
            }

            return response;
        }
    }
}