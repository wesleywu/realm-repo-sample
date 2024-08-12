#nullable disable
using Guru.Internal;
using Realms;

namespace Guru.Collection.RealmDB
{
    public abstract class RealmRepository : IRepository
    {
        private const int MAX_CONCURRENT_TRANSACTIONS = 100;
        private const int MAX_NUMBER_OF_ACTIVE_VERSIONS = 10;

        internal ICollectionMeta meta { get; private set; }

        protected readonly RealmCreateLogic CreateLogic;
        protected readonly RealmUpdateLogic UpdateLogic;
        protected readonly RealmUpsertLogic UpsertLogic;
        protected readonly RealmGetLogic GetLogic;
        protected readonly RealmOneLogic OneLogic;
        protected readonly RealmListLogic ListLogic;
        protected readonly RealmCountLogic CountLogic;
        protected readonly RealmDeleteLogic DeleteLogic;
        protected readonly RealmDeleteMultiLogic DeleteMultiLogic;

        private readonly RealmConfiguration _realmConfig;
        private readonly SemaphoreSlim _transactionSemaphore;

        protected RealmRepository(RealmConfiguration realmConfig, ICollectionMeta meta)
        {
            this.meta = meta;

            this._realmConfig = realmConfig;
            this._realmConfig.MaxNumberOfActiveVersions = MAX_NUMBER_OF_ACTIVE_VERSIONS;
            this._transactionSemaphore = new SemaphoreSlim(MAX_CONCURRENT_TRANSACTIONS, MAX_CONCURRENT_TRANSACTIONS);

            this.CreateLogic = new RealmCreateLogic(new RealmLogicHelper(), meta);
            this.UpdateLogic = new RealmUpdateLogic(new RealmLogicHelper(), meta);
            this.UpsertLogic = new RealmUpsertLogic(new RealmLogicHelper(), meta);
            this.GetLogic = new RealmGetLogic(new RealmLogicHelper(), meta);
            this.OneLogic = new RealmOneLogic(new RealmLogicHelper(), meta);
            this.ListLogic = new RealmListLogic(new RealmLogicHelper(), meta);
            this.CountLogic = new RealmCountLogic(new RealmLogicHelper(), meta);
            this.DeleteLogic = new RealmDeleteLogic(new RealmLogicHelper(), meta);
            this.DeleteMultiLogic = new RealmDeleteMultiLogic(new RealmLogicHelper(), meta);
        }

        protected async Task<TResult> BeginTransaction<TResult>(Func<Realm, TResult> action)
        {
            await _transactionSemaphore.WaitAsync();
            try
            {
                return await Task.Run(() =>
                {
                    Realm realm = null;
                    try
                    {
                        realm = Realm.GetInstance(_realmConfig);
                        return realm.Write(() => action(realm));
                    }
                    catch (Exception e)
                    {
                        LoggerUtils.LogException(e);
                        //throw;
                        return default;
                    }
                    finally
                    {
                        realm?.Dispose();
                    }
                });
            }
            finally
            {
                _transactionSemaphore.Release();
            }
        }

        public void DeleteAll()
        {
            var realm = Realm.GetInstance(this._realmConfig);
            realm.Write(() => { realm.RemoveAll(); });
        }
    }
}