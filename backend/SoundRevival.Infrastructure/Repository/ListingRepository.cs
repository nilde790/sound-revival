using Microsoft.EntityFrameworkCore;
using SoundRevival.Core.Entities;
using SoundRevival.Dto.Common;
using SoundRevival.Dto.Constants;
using SoundRevival.Dto.Listings;
using SoundRevival.Core.Interfaces;

namespace SoundRevival.Infrastructure.Repository
{
    public class ListingRepository : IListingRepository
    {

        private readonly AppDbContext _context;

        public ListingRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddListing(Listing newListing)
        {
            _context.Listings.Add(newListing);
           
        }

        public void DeleteListing(Listing listing)
        {
            _context.Listings.Remove(listing);
        }

        public async Task<Listing?> GetListingByIdAsync(Guid id)
        {
            var listing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == id);

            return listing;
            
        }

        public async Task<Listing?> GetListingDetailByIdAsync(Guid id)
        {
            var listing = await _context.Listings.AsNoTracking()
                .Include(l => l.User)
                .Include(l => l.Images)
                .FirstOrDefaultAsync(l => l.Id == id);

            return listing;

        }

        public async Task<PagedResult<Listing>> GetListingsAsync(ListingFilterRequestDto filter, PaginationRequestDto pagination)
        {
            IQueryable<Listing> query = _context.Listings.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(l => l.Category == filter.Category);

            }
            if (filter.MaxPrice != null)
            {
                query = query.Where(l => l.Price <= filter.MaxPrice);

            }
            if (filter.MinPrice != null)
            {
                query = query.Where(l => l.Price >= filter.MinPrice);

            }
            if (!string.IsNullOrWhiteSpace(filter.Condition))
            {
                query = query.Where(l => l.Condition == filter.Condition);

            }
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                // EF translates string.Contains to LIKE with the term escaped, so % and _ are matched literally.
                // Case-insensitive: the column is lowered in SQL (lower()), the term here with the invariant culture.
                var term = filter.Search.Trim().ToLowerInvariant();

                query = query.Where(l =>
                    l.Title.ToLower().Contains(term) ||
                    l.Description.ToLower().Contains(term));

            }
          
            query = query.Where(l => l.Status == filter.Status);

            var page = new PagedResult<Listing>();
            page.TotalCount = await query.CountAsync();
            query = filter.Sort switch
            {
                ListingValues.SortNewest => query.OrderByDescending(l => l.CreatedAt).ThenBy(l => l.Id),
                ListingValues.SortPriceAsc => query.OrderBy(l => l.Price).ThenBy(l => l.Id),
                ListingValues.SortPriceDesc => query.OrderByDescending(l => l.Price).ThenBy(l => l.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter.Sort, "Unknown sort value")
            };
                
            page.Items = await query.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();         
             
            return page;
        }  

        public async Task<bool> SaveChangesAsync()
        {
            var result = await _context.SaveChangesAsync();

            return result != 0;
         
           
        }
    }
}
