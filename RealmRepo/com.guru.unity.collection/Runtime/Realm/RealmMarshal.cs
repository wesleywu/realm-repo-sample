#nullable disable
using Realms;

namespace Guru.Collection.RealmDB
{
    public interface IRealmMarshalable
    {
        RealmObject ToRealmObject();
    }

    public interface IRealmUnmarshalable
    {
        void FromRealmObject(RealmObject realmObject);
    }
}