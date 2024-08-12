#nullable disable

namespace Guru.Internal
{
    public class CSingleton<T> where T : new()
    {
        private static T _instance;
        public static T GetSingleton()
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }
}

