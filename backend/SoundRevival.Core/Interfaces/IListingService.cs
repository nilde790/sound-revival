using SoundRevival.Dto.Common;
using SoundRevival.Dto.Listings;

namespace SoundRevival.Core.Interfaces;

public interface IListingService
{
    public Task<PagedResponseDto<ListingSummaryResponseDto>> GetListingsAsync(PaginationRequestDto page, ListingFilterRequestDto filter);

    public Task<ListingDetailResponseDto> GetByIdAsync(Guid id, Guid? userId);

    public Task<ListingDetailResponseDto> CreateAsync(Guid userId, CreateListingRequestDto request);

    public Task<ListingDetailResponseDto> UpdateAsync(Guid id, Guid userId, UpdateListingRequestDto request, bool isAdmin);

    public Task DeleteAsync(Guid id, Guid userId, bool isAdmin);

    public Task ChangeStateAsync(Guid id, Guid userId, UpdateListingStatusRequestDto request, bool isAdmin);

}
