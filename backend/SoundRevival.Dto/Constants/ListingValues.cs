namespace SoundRevival.Dto.Constants
{
    public static class ListingValues
    {
        public static readonly IReadOnlySet<string> Categories = new HashSet<string> { "Guitars", "Keyboards", "Drums", "Wind Instruments", "Strings", "Other" };
        public static readonly IReadOnlySet<string> Conditions = new HashSet<string> { "New", "Like New", "Good", "Fair", "Poor" };
        public static readonly IReadOnlySet<string> Statuses = new HashSet<string> { Available, Sold };

        public const string Available = "available";
        public const string Sold = "sold";

        public static readonly IReadOnlySet<string> Sorts = new HashSet<string> { SortNewest, SortPriceAsc, SortPriceDesc };

        public const string SortNewest = "newest";
        public const string SortPriceAsc = "price_asc";
        public const string SortPriceDesc = "price_desc";
    }
}
