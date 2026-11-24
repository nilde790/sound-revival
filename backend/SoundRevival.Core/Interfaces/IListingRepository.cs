using SoundRevival.Core.Entities;
using SoundRevival.Dto.Common;
using SoundRevival.Dto.Listings;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Core.Interfaces
{
    public interface IListingRepository
    {

        public Task<PagedResult<Listing>> GetListingsAsync(ListingFilterRequestDto filter,PaginationRequestDto pagination);

        public Task<Listing?> GetListingByIdAsync(Guid id);

        public Task<Listing?>GetListingDetailByIdAsync(Guid id);

        public void AddListing(Listing newListing);

        public void DeleteListing(Listing listing);

        public Task<bool> SaveChangesAsync();
    }
}

