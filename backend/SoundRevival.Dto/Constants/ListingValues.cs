namespace SoundRevival.Dto.Constants
{
    public static class ListingValues
    {
        public static readonly IReadOnlySet<string> Categories = new HashSet<string> { "Guitars", "Keyboards", "Drums", "Wind Instruments", "Strings", "Other" };
        public static readonly IReadOnlySet<string> Conditions = new HashSet<string> { "New", "Like New", "Good", "Fair", "Poor" };
        public static readonly IReadOnlySet<string> Statuses = new HashSet<string> { Available, Sold };

        public const string Available = "available";
        public const string Sold = "sold";

    }
}
