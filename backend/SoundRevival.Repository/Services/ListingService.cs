using SoundRevival.Dto.Common;
using SoundRevival.Dto.Constants;
using SoundRevival.Dto.Listings;
using SoundRevival.Repository.Entities;
using SoundRevival.Repository.Exceptions;
using SoundRevival.Repository.Interfaces;

namespace SoundRevival.Repository.Services
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;

        private readonly TimeProvider _timeProvider;

        public ListingService(IListingRepository listingRepository, TimeProvider timeProvider)
        {
            _listingRepository = listingRepository;
            _timeProvider = timeProvider;
        }

        private static ListingDetailResponseDto MapToDetails(Guid? userId, Listing listing)
        {
            var sellerDto = new SellerResponseDto()
            {
                Id = listing.User.Id,
                DisplayName = listing.User.DisplayName,
                Email = userId.HasValue ? listing.User.Email : null,
            };

            var imagesDto = listing.Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ListingImageResponseDto()
                {
                    Id = i.Id,
                    DisplayOrder = i.DisplayOrder,
                    Url = i.Url,

                }).ToList();

            var listingDto = new ListingDetailResponseDto()
            {
                Id = listing.Id,
                Category = listing.Category,
                Condition = listing.Condition,
                Description = listing.Description,
                CreatedAt = listing.CreatedAt,
                Images = imagesDto,
                Price = listing.Price,
                Seller = sellerDto,
                Status = listing.Status,
                Title = listing.Title,
                UpdatedAt = listing.UpdatedAt,

            };
            return listingDto;
        }

        private static void CheckCategoriesAndConditions(string category, string condition)
        {
            if (!ListingValues.Categories.Contains(category))
            {
                throw new BusinessRuleException($"Category '{category}' not valid");
            }
            if (!ListingValues.Conditions.Contains(condition))
            {
                throw new BusinessRuleException($"Condition '{condition}' not valid");
            }

        }

        private static void CheckOwnership(Guid userId, bool isAdmin, Listing listing)
        {
            if (userId != listing.UserId && !isAdmin)
            {
                throw new ForbiddenException("you are not allow to modify this listing");

            }
        }

        private static void CheckFilterValues(ListingFilterRequestDto filter)
        {
            if (filter.Category != null && !ListingValues.Categories.Contains(filter.Category))
            {
                throw new BusinessRuleException($"Category '{filter.Category}' not valid");
            }
            if (filter.Condition != null && !ListingValues.Conditions.Contains(filter.Condition))
            {
                throw new BusinessRuleException($"Condition '{filter.Condition}' not valid");
            }
            if (!ListingValues.Statuses.Contains(filter.Status))
            {
                throw new BusinessRuleException($"Status '{filter.Status}' not valid");
            }
            if (!ListingValues.Sorts.Contains(filter.Sort))
            {
                throw new BusinessRuleException($"Sort '{filter.Sort}' not valid");
            }
        }


        public async Task<ListingDetailResponseDto> GetByIdAsync(Guid id, Guid? userId)
        {

            var listing = await _listingRepository.GetListingDetailByIdAsync(id);

            if (listing == null)
            {
                throw new NotFoundException("listing not found");
            }

            ListingDetailResponseDto listingDto = MapToDetails(userId, listing);

            return listingDto;

        }
        public async Task<PagedResponseDto<ListingSummaryResponseDto>> GetListingsAsync(PaginationRequestDto page, ListingFilterRequestDto filter)
        {
            CheckFilterValues(filter);

            var paginatedItems = await _listingRepository.GetListingsAsync(filter, page);

            var listingsSummarized = paginatedItems.Items.Select(p => new ListingSummaryResponseDto()
            {
                Id = p.Id,
                Category = p.Category,
                Condition = p.Condition,
                CreatedAt = p.CreatedAt,
                Price = p.Price,
                Status = p.Status,
                Title = p.Title,
            }).ToList();


            var pagedResponseDto = new PagedResponseDto<ListingSummaryResponseDto>()
            {
                Data = listingsSummarized,
                TotalItems = paginatedItems.TotalCount,
                PageSize = page.PageSize,
                Page = page.Page,

            };

            return pagedResponseDto;

        }
        public async Task<ListingDetailResponseDto> CreateAsync(Guid userId, CreateListingRequestDto request)
        {
            var category = request.Category.Trim();
            var condition = request.Condition.Trim();
            CheckCategoriesAndConditions(category, condition);

            var title = request.Title.Trim();
            var description = request.Description.Trim();

            var newListing = new Listing
            {
                Id = Guid.NewGuid(),
                Category = category,
                Condition = condition,
                Description = description,
                Price = request.Price,
                Title = title,
                UserId = userId,

            };
        

            _listingRepository.AddListing(newListing);

            var saved = await _listingRepository.SaveChangesAsync();

            if(saved == false)
            {
                throw new InvalidOperationException("save failed");
            }

            var savedListing = await _listingRepository.GetListingDetailByIdAsync(newListing.Id) ?? throw new InvalidOperationException("listing not found right after saving");
            var response = MapToDetails(userId, savedListing);

            return response;

        }
        public async Task<ListingDetailResponseDto> UpdateAsync(Guid id, Guid userId, UpdateListingRequestDto request, bool isAdmin)
        {
            var listingToUpdate = await _listingRepository.GetListingByIdAsync(id) ?? throw new NotFoundException($"Could not find {id}");

            CheckOwnership(userId, isAdmin, listingToUpdate);

            var category = request.Category.Trim();
            var condition = request.Condition.Trim();
            CheckCategoriesAndConditions(category, condition);

            var title = request.Title.Trim();
            var description = request.Description.Trim();



            listingToUpdate.Category = category;
            listingToUpdate.Condition = condition;
            listingToUpdate.Description = description;
            listingToUpdate.Title = title;
            listingToUpdate.Price = request.Price;
            listingToUpdate.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;


            var updated = await _listingRepository.SaveChangesAsync();

            if (!updated)
            {
                throw new InvalidOperationException("save failed");
            }

            var updatedListing = await _listingRepository.GetListingDetailByIdAsync(listingToUpdate.Id) ?? throw new InvalidOperationException("listing not found right after updating");

            var response = MapToDetails(userId, updatedListing);
            return response;


        }
        public async Task DeleteAsync(Guid id, Guid userId, bool isAdmin)
        {
            var listingToDelete = await _listingRepository.GetListingByIdAsync(id) ?? throw new NotFoundException("listing not found");

            CheckOwnership(userId, isAdmin, listingToDelete);

            _listingRepository.DeleteListing(listingToDelete);
            var deleted = await _listingRepository.SaveChangesAsync();
            if (!deleted)
            {
                throw new InvalidOperationException("listing was not deleted");
            }
      
        }
        public async Task ChangeStateAsync(Guid id, Guid userId, UpdateListingStatusRequestDto request, bool isAdmin)
        {
            var listingToUpdate = await _listingRepository.GetListingByIdAsync(id) ?? throw new NotFoundException($"Could not find {id}");
            CheckOwnership(userId, isAdmin, listingToUpdate);

            var status = request.Status.Trim();
            if (!ListingValues.Statuses.Contains(status))
            {
                throw new BusinessRuleException($"Status '{status}' not valid");
            }
   


            listingToUpdate.Status = status;
            listingToUpdate.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;

            var updated = await _listingRepository.SaveChangesAsync();
            if (!updated)
            {
                throw new InvalidOperationException("save failed");
            }

        }
    }
}
