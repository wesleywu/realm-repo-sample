#nullable disable
using Guru.Internal;
using Realms;
using Realms.Exceptions;

namespace Guru.Collection.RealmDB
{
    public class RealmFactory : CSingleton<RealmFactory>
    {
        public RealmConfiguration CreateRealm(string realmPath)
        {
            RealmConfiguration realmConfig = new RealmConfiguration(realmPath);
            string localVersionFileUrl = Path.Combine(realmPath, "../", "local_version.txt");

            ulong localSchemaVersion;
            if (!File.Exists(localVersionFileUrl))
            {
                localSchemaVersion = 1;
                IOUtility.MakeSureDirectoryExist(localVersionFileUrl);
                using (StreamWriter streamWriter = new StreamWriter(localVersionFileUrl, false))
                {
                    streamWriter.Write(localSchemaVersion);
                }
            }
            else
            {
                string versionText = File.ReadAllText(localVersionFileUrl);
                if (string.IsNullOrEmpty(versionText))
                {
                    throw new InvalidDataException("Local schema version is empty");
                }

                if (!ulong.TryParse(versionText, out localSchemaVersion))
                {
                    throw new InvalidDataException("Local schema version is not a valid number");
                }
            }

            realmConfig.SchemaVersion = localSchemaVersion;
            try
            {
                Realm.GetInstance(realmConfig);
            }
            catch (RealmMigrationNeededException _)
            {
                realmConfig.SchemaVersion++;
                realmConfig.MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    realmConfig.SchemaVersion = migration.NewRealm.Config.SchemaVersion;
                    using (StreamWriter streamWriter = new StreamWriter(localVersionFileUrl, false))
                    {
                        streamWriter.Write(migration.NewRealm.Config.SchemaVersion);
                    }
                };
                Realm.GetInstance(realmConfig);
            }

            return realmConfig;
        }
    }
}