using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    public class User
    {
        [JsonUtilities("id")]
        public string Id { get; set; }
        [JsonUtilities("first_name")]
        public string FirstName { get; set; }
        [JsonUtilities("last_name")]
        public string LastName { get; set; }
        [JsonUtilities("name")]
        public string Name { get; set; }
        [JsonUtilities("gender")]
        public string Gender { get; set; }
        [JsonUtilities("locale")]
        public string Locale { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}