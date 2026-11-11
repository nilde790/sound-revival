using Microsoft.EntityFrameworkCore;
using SoundRevival.Dto.Common;
using SoundRevival.Dto.Listings;
using SoundRevival.Repository.Entities;
using SoundRevival.Repository.Interfaces;

namespace SoundRevival.Repository.Repository
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
                query = query.Where(l => EF.Functions.ILike(l.Title, $"%{filter.Search}%") || EF.Functions.ILike(l.Description, $"%{filter.Search}%"));

            }
          
            query = query.Where(l => l.Status == filter.Status);

            var page = new PagedResult<Listing>();

            page.TotalCount = await query.CountAsync();

            query = query.OrderBy(l => l.Title).ThenBy(l => l.Id);

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
