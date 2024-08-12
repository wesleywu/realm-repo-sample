#nullable disable
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public class RealmCreateLogic
    {
        private readonly ICollectionMeta _collectionMeta;
        private readonly RealmLogicHelper _helper;

        public RealmCreateLogic(RealmLogicHelper helper, ICollectionMeta collectionMeta)
        {
            this._collectionMeta = collectionMeta;
            this._helper = helper;
        }

        public TRes Create<TReq, TRes, TPo>(Realm realm, TReq request) where TReq : ICreationRequest, IRealmMarshalable where TRes : ICreationResponse, new() where TPo : RealmObject
        {
            var response = new TRes();
            try
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

                response.SetMessage("创建记录成功");
                response.SetInsertedId(id);
                response.SetInsertCount(1);

                LoggerUtils.Verbose("RealmCreateLogic", "Create",$"创建记录成功 : collectionName={this._collectionMeta.CollectionName}, id={id}");
            }
            catch (Exception e)
            {
                response.SetMessage("创建记录失败: " + e.Message);
                response.SetInsertCount(0);
                LoggerUtils.LogException(e, $"创建记录失败 : collectionName={this._collectionMeta.CollectionName}");
            }

            return response;
        }
    }
}