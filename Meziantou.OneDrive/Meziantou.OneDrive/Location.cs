using CodeFluent.Runtime.Utilities;

namespace Meziantou.OneDrive
{
    public class Location
    {
        [JsonUtilities("latitude")]
        public double Latitude { get; set; }
        [JsonUtilities("longitude")]
        public double Longitude { get; set; }
        [JsonUtilities("altitude")]
        public double Altitude { get; set; }
    }
}