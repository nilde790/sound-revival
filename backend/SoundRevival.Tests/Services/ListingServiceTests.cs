using Microsoft.Extensions.Time.Testing;
using Moq;
using SoundRevival.Dto.Common;
using SoundRevival.Dto.Constants;
using SoundRevival.Dto.Listings;
using SoundRevival.Repository.Entities;
using SoundRevival.Repository.Exceptions;
using SoundRevival.Repository.Interfaces;
using SoundRevival.Repository.Services;

namespace SoundRevival.Tests.Services
{
    public class ListingServiceTests
    {
        private readonly Mock<IListingRepository> _mockRepo = new();
        private readonly FakeTimeProvider _timeProvider = new();
        private readonly ListingService _listingService;

        public ListingServiceTests()
        {
            _listingService = new ListingService(_mockRepo.Object, _timeProvider);
        }

        private static Listing CreateListing(Guid ownerId) => new()
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Fender Stratocaster",
            Description = "Used, well kept",
            Price = 500m,
            Category = "Guitars",
            Condition = "Good",
            User = new User
            {
                Id = ownerId,
                DisplayName = "Owner",
                Email = "owner@test.com"
            }
        };

        // A valid update request: each test changes only the field its scenario is about.
        private static UpdateListingRequestDto CreateUpdateRequest() => new()
        {
            Title = "Gibson Les Paul",
            Description = "Recently set up",
            Price = 900m,
            Category = "Guitars",
            Condition = "Like New"
        };

        // A valid create request: each test changes only the field its scenario is about.
        private static CreateListingRequestDto CreateCreateRequest() => new()
        {
            Title = "Yamaha P-125",
            Description = "Digital piano, like new",
            Price = 450m,
            Category = "Keyboards",
            Condition = "Like New"
        };



        // ---------- DeleteAsync ----------

        [Fact]
        public async Task DeleteAsync_UserIsNotOwner_ThrowsForbiddenException()
        {
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);

            await Assert.ThrowsAsync<ForbiddenException>(() => _listingService.DeleteAsync(listing.Id, strangerId, isAdmin: false));

            _mockRepo.Verify(l => l.DeleteListing(It.IsAny<Listing>()), Times.Never);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_UserIsOwner_DeletesListing()
        {
            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);
            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(true);

            await _listingService.DeleteAsync(listing.Id, ownerId, isAdmin: false);

            _mockRepo.Verify(l => l.DeleteListing(listing), Times.Once);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_UserIsAdmin_DeletesListing()
        {
            var adminId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);
            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(true);

            await _listingService.DeleteAsync(listing.Id, adminId, isAdmin: true);

            _mockRepo.Verify(l => l.DeleteListing(listing), Times.Once);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ListingNotFound_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid();
            var fakeListingId = Guid.NewGuid();

            _mockRepo.Setup(l => l.GetListingByIdAsync(fakeListingId)).ReturnsAsync((Listing?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _listingService.DeleteAsync(fakeListingId, userId, isAdmin: false));

            _mockRepo.Verify(l => l.DeleteListing(It.IsAny<Listing>()), Times.Never);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_SaveFails_ThrowsInvalidOperationException()
        {
            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);
            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _listingService.DeleteAsync(listing.Id, ownerId, isAdmin: false));
        }

        // ---------- ChangeStateAsync ----------

        [Fact]
        public async Task ChangeStateAsync_UserIsNotOwner_ThrowsForbiddenException()
        {
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);
            var request = new UpdateListingStatusRequestDto { Status = ListingValues.Sold };

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);

            await Assert.ThrowsAsync<ForbiddenException>(() => _listingService.ChangeStateAsync(listing.Id, strangerId, request, isAdmin: false));

            Assert.Equal(ListingValues.Available, listing.Status);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ChangeStateAsync_ListingNotFound_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid();
            var fakeListingId = Guid.NewGuid();
            var request = new UpdateListingStatusRequestDto { Status = ListingValues.Sold };

            _mockRepo.Setup(l => l.GetListingByIdAsync(fakeListingId)).ReturnsAsync((Listing?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _listingService.ChangeStateAsync(fakeListingId, userId, request, isAdmin: false));

            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ChangeStateAsync_SaveFails_ThrowsInvalidOperationException()
        {
            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);
            var request = new UpdateListingStatusRequestDto { Status = ListingValues.Sold };

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);
            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _listingService.ChangeStateAsync(listing.Id, ownerId, request, isAdmin: false));
        }

        [Theory]
        [InlineData("NotValid")]
        [InlineData("AvaIlaBle")]
        [InlineData("SOLd")]
        [InlineData("")]
        [InlineData("          ")]
        public async Task ChangeStateAsync_InvalidStatus_ThrowsBusinessRuleException(string status)
        {
            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);
            var request = new UpdateListingStatusRequestDto { Status = status };

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);

            await Assert.ThrowsAsync<BusinessRuleException>(() => _listingService.ChangeStateAsync(listing.Id, ownerId, request, isAdmin: false));

            Assert.Equal(ListingValues.Available, listing.Status);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ChangeStateAsync_ValidStatus_UpdatesStatusAndTimestamp()
        {
            var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
            _timeProvider.SetUtcNow(now);

            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);
            var request = new UpdateListingStatusRequestDto { Status = ListingValues.Sold };

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);
            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(true);

            await _listingService.ChangeStateAsync(listing.Id, ownerId, request, isAdmin: false);

            Assert.Equal(ListingValues.Sold, listing.Status);
            Assert.Equal(now.UtcDateTime, listing.UpdatedAt);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Once);
        }

        // ---------- UpdateAsync ----------

        [Fact]
        public async Task UpdateAsync_UserIsNotOwner_ThrowsForbiddenException()
        {
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);
            var originalTitle = listing.Title;

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);

            await Assert.ThrowsAsync<ForbiddenException>(() => _listingService.UpdateAsync(listing.Id, strangerId, CreateUpdateRequest(), isAdmin: false));

            Assert.Equal(originalTitle, listing.Title);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ListingNotFound_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid();
            var fakeListingId = Guid.NewGuid();

            _mockRepo.Setup(l => l.GetListingByIdAsync(fakeListingId)).ReturnsAsync((Listing?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _listingService.UpdateAsync(fakeListingId, userId, CreateUpdateRequest(), isAdmin: false));

            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Theory]
        [InlineData("Banjo")]
        [InlineData("guitars")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_InvalidCategory_ThrowsBusinessRuleException(string category)
        {
            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);
            var request = CreateUpdateRequest();
            request.Category = category;

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);

            await Assert.ThrowsAsync<BusinessRuleException>(() => _listingService.UpdateAsync(listing.Id, ownerId, request, isAdmin: false));

            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Theory]
        [InlineData("Broken")]
        [InlineData("like new")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_InvalidCondition_ThrowsBusinessRuleException(string condition)
        {
            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);
            var request = CreateUpdateRequest();
            request.Condition = condition;

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);

            await Assert.ThrowsAsync<BusinessRuleException>(() => _listingService.UpdateAsync(listing.Id, ownerId, request, isAdmin: false));

            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_SaveFails_ThrowsInvalidOperationException()
        {
            var ownerId = Guid.NewGuid();
            var listing = CreateListing(ownerId);

            _mockRepo.Setup(l => l.GetListingByIdAsync(listing.Id)).ReturnsAsync(listing);
            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _listingService.UpdateAsync(listing.Id, ownerId, CreateUpdateRequest(), isAdmin: false));
        }

        [Fact]
        public async Task UpdateAsync_ValidRequest_UpdatesListingAndReturnsDetail()
        {
            var ownerId = Guid.NewGuid();
            var listingToUpdate = CreateListing(ownerId);
            var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
            _timeProvider.SetUtcNow(now);
            var request = CreateUpdateRequest();
            request.Title = "   Title   ";
            request.Description = "  Description  ";
            request.Category = "  Guitars  ";
            request.Condition = " New  ";

            _mockRepo.Setup(l => l.GetListingByIdAsync(listingToUpdate.Id)).ReturnsAsync(listingToUpdate);
            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(true);

            var reloadedListing = CreateListing(ownerId);
            reloadedListing.Id = listingToUpdate.Id;
            reloadedListing.Title = "Reloaded title";

            _mockRepo.Setup(l => l.GetListingDetailByIdAsync(listingToUpdate.Id)).ReturnsAsync(reloadedListing);

      
            var result = await _listingService.UpdateAsync(listingToUpdate.Id, ownerId, request, isAdmin: false);

            Assert.Equal("Title",listingToUpdate.Title);
            Assert.Equal("Description", listingToUpdate.Description);
            Assert.Equal("Guitars", listingToUpdate.Category);
            Assert.Equal("New", listingToUpdate.Condition);
            Assert.Equal(request.Price, listingToUpdate.Price);
            Assert.Equal(now.UtcDateTime, listingToUpdate.UpdatedAt);

            Assert.Equal(listingToUpdate.Id, result.Id);
            Assert.Equal("owner@test.com", result.Seller.Email);
            Assert.Equal("Reloaded title", result.Title);

            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Once());

        }

        // ---------- CreateAsync ----------

        [Theory]
        [InlineData("Banjo")]
        [InlineData("guitars")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_InvalidCategory_ThrowsBusinessRuleException(string category)
        {
            var userId = Guid.NewGuid();
            var request = CreateCreateRequest();
            request.Category = category;

            await Assert.ThrowsAsync<BusinessRuleException>(() => _listingService.CreateAsync(userId, request));

            _mockRepo.Verify(l => l.AddListing(It.IsAny<Listing>()), Times.Never);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Theory]
        [InlineData("Broken")]
        [InlineData("like new")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_InvalidCondition_ThrowsBusinessRuleException(string condition)
        {
            var userId = Guid.NewGuid();
            var request = CreateCreateRequest();
            request.Condition = condition;

            await Assert.ThrowsAsync<BusinessRuleException>(() => _listingService.CreateAsync(userId, request));

            _mockRepo.Verify(l => l.AddListing(It.IsAny<Listing>()), Times.Never);
            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_SaveFails_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();

            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _listingService.CreateAsync(userId, CreateCreateRequest()));

            _mockRepo.Verify(l => l.GetListingDetailByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ReloadReturnsNull_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();

            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(true);
            _mockRepo.Setup(l => l.GetListingDetailByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Listing?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _listingService.CreateAsync(userId, CreateCreateRequest()));
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_AddsListingAndReturnsDetail()
        {
            var userId = Guid.NewGuid();
            var request = CreateCreateRequest();
            request.Title = " Title ";
            request.Description = " Description ";
            request.Category = " Guitars ";
            request.Condition = " New ";

            Listing? captured = null;

            _mockRepo.Setup(l => l.AddListing(It.IsAny<Listing>())).Callback<Listing>(l => captured = l);
            _mockRepo.Setup(l => l.SaveChangesAsync()).ReturnsAsync(true);

            var reloadedListing = CreateListing(userId);
            reloadedListing.Title = "Reloaded Title";
            _mockRepo.Setup(l => l.GetListingDetailByIdAsync(It.IsAny<Guid>())).ReturnsAsync(reloadedListing);

            var result = await _listingService.CreateAsync(userId, request);

            Assert.NotNull(captured);
            Assert.NotEqual(Guid.Empty, captured.Id);
            Assert.Equal("Title", captured.Title);
            Assert.Equal("Description", captured.Description);
            Assert.Equal("Guitars", captured.Category);
            Assert.Equal("New", captured.Condition);
            Assert.Equal(request.Price, captured.Price);
            Assert.Equal(userId, captured.UserId);

            Assert.Equal("Reloaded Title", result.Title);
            Assert.Equal("owner@test.com", result.Seller.Email);

            _mockRepo.Verify(l => l.SaveChangesAsync(), Times.Once);
            _mockRepo.Verify(l => l.GetListingDetailByIdAsync(captured.Id), Times.Once);
        }

        // ---------- GetByIdAsync ----------

        [Fact]
        public async Task GetByIdAsync_ListingNotFound_ThrowsNotFoundException()
        {
            var fakeListingId = Guid.NewGuid();

            _mockRepo.Setup(l => l.GetListingDetailByIdAsync(fakeListingId)).ReturnsAsync((Listing?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _listingService.GetByIdAsync(fakeListingId, userId: null));
        }

        [Fact]
        public async Task GetByIdAsync_AnonymousUser_HidesSellerEmail()
        {
            var listing = CreateListing(Guid.NewGuid());

            _mockRepo.Setup(l => l.GetListingDetailByIdAsync(listing.Id)).ReturnsAsync(listing);

            var result = await _listingService.GetByIdAsync(listing.Id, userId: null);

            Assert.Null(result.Seller.Email);
            Assert.Equal(listing.User.DisplayName, result.Seller.DisplayName);
        }

        [Fact]
        public async Task GetByIdAsync_AuthenticatedUser_ShowsSellerEmail()
        {
            var listing = CreateListing(Guid.NewGuid());
            var viewerId = Guid.NewGuid();

            _mockRepo.Setup(l => l.GetListingDetailByIdAsync(listing.Id)).ReturnsAsync(listing);

            var result = await _listingService.GetByIdAsync(listing.Id, viewerId);

            Assert.Equal("owner@test.com", result.Seller.Email);
        }

        [Fact]
        public async Task GetByIdAsync_ListingWithImages_ReturnsImagesOrderedByDisplayOrder()
        {
            var listing = CreateListing(Guid.NewGuid());
            var firstImage = new Image { Id = Guid.NewGuid(), Url = "img-1.jpg", DisplayOrder = 1 };

            // Added out of order on purpose: the test only proves the sort if the input is unsorted.
            listing.Images.Add(new Image { Id = Guid.NewGuid(), Url = "img-3.jpg", DisplayOrder = 3 });
            listing.Images.Add(firstImage);
            listing.Images.Add(new Image { Id = Guid.NewGuid(), Url = "img-2.jpg", DisplayOrder = 2 });

            _mockRepo.Setup(l => l.GetListingDetailByIdAsync(listing.Id)).ReturnsAsync(listing);

            var result = await _listingService.GetByIdAsync(listing.Id, userId: null);

            Assert.Equal(new short[] { 1, 2, 3 }, result.Images.Select(i => i.DisplayOrder));

            var first = result.Images.First();
            Assert.Equal(firstImage.Id, first.Id);
            Assert.Equal(firstImage.Url, first.Url);
        }

        // ---------- GetListingsAsync ----------

        [Fact]
        public async Task GetListingsAsync_ListingsFound_MapsToSummaries()
        {
            var listing = CreateListing(Guid.NewGuid());
            listing.Status = ListingValues.Sold;
            listing.CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var pagination = new PaginationRequestDto { Page = 1, PageSize = 10 };
            var filter = new ListingFilterRequestDto();

            _mockRepo.Setup(l => l.GetListingsAsync(filter, pagination))
                .ReturnsAsync(new PagedResult<Listing> { Items = [listing], TotalCount = 1 });

            var result = await _listingService.GetListingsAsync(pagination, filter);

            var summary = Assert.Single(result.Data);
            Assert.Equal(listing.Id, summary.Id);
            Assert.Equal(listing.Title, summary.Title);
            Assert.Equal(listing.Price, summary.Price);
            Assert.Equal(listing.Category, summary.Category);
            Assert.Equal(listing.Condition, summary.Condition);
            Assert.Equal(ListingValues.Sold, summary.Status);
            Assert.Equal(listing.CreatedAt, summary.CreatedAt);
        }

        [Fact]
        public async Task GetListingsAsync_NoListings_ReturnsEmptyPage()
        {
            var pagination = new PaginationRequestDto { Page = 1, PageSize = 10 };
            var filter = new ListingFilterRequestDto();

            _mockRepo.Setup(l => l.GetListingsAsync(filter, pagination))
                .ReturnsAsync(new PagedResult<Listing> { Items = [], TotalCount = 0 });

            var result = await _listingService.GetListingsAsync(pagination, filter);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalItems);
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public async Task GetListingsAsync_ListingsFound_ReturnsPagingFields()
        {
            var pagination = new PaginationRequestDto { Page = 3, PageSize = 10 };
            var filter = new ListingFilterRequestDto();

            _mockRepo.Setup(l => l.GetListingsAsync(filter, pagination))
                .ReturnsAsync(new PagedResult<Listing> 
                { 
                    Items = [CreateListing(Guid.NewGuid()), CreateListing(Guid.NewGuid()), CreateListing(Guid.NewGuid())],
                    TotalCount = 23 
                });

            var result = await _listingService.GetListingsAsync(pagination, filter);

            Assert.Equal(3, result.TotalPages);
            Assert.Equal(23, result.TotalItems);    
            Assert.Equal(3, result.Page);    
            Assert.Equal(10, result.PageSize);
        }

        // Filter values: exact match against ListingValues, checked before the repository is called.

        [Theory]
        [InlineData("Guitars", "Good", ListingValues.Available, ListingValues.SortNewest)]
        [InlineData(null, null, ListingValues.Sold, ListingValues.SortPriceAsc)]
        [InlineData(null, "Like New", ListingValues.Available, ListingValues.SortPriceDesc)]
        public async Task GetListingsAsync_ValidFilterValues_QueriesRepository(string? category, string? condition, string status, string sort)
        {
            var pagination = new PaginationRequestDto { Page = 1, PageSize = 10 };
            var filter = new ListingFilterRequestDto
            {
                Category = category,
                Condition = condition,
                Status = status,
                Sort = sort
            };

            _mockRepo.Setup(l => l.GetListingsAsync(filter, pagination))
                .ReturnsAsync(new PagedResult<Listing> { Items = [], TotalCount = 0 });

            await _listingService.GetListingsAsync(pagination, filter);

            _mockRepo.Verify(l => l.GetListingsAsync(filter, pagination), Times.Once);
        }

        [Theory]
        [InlineData("Banjo")]
        [InlineData("guitars")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetListingsAsync_InvalidCategory_ThrowsBusinessRuleException(string category)
        {
            var filter = new ListingFilterRequestDto { Category = category };

            await AssertFilterRejected(filter);
        }

        [Theory]
        [InlineData("Broken")]
        [InlineData("like new")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetListingsAsync_InvalidCondition_ThrowsBusinessRuleException(string condition)
        {
            var filter = new ListingFilterRequestDto { Condition = condition };

            await AssertFilterRejected(filter);
        }

        [Theory]
        [InlineData("reserved")]
        [InlineData("Available")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetListingsAsync_InvalidStatus_ThrowsBusinessRuleException(string? status)
        {
            var filter = new ListingFilterRequestDto { Status = status! };

            await AssertFilterRejected(filter);
        }

        [Theory]
        [InlineData("oldest")]
        [InlineData("PRICE_ASC")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetListingsAsync_InvalidSort_ThrowsBusinessRuleException(string? sort)
        {
            var filter = new ListingFilterRequestDto { Sort = sort! };

            await AssertFilterRejected(filter);
        }

        private async Task AssertFilterRejected(ListingFilterRequestDto filter)
        {
            var pagination = new PaginationRequestDto { Page = 1, PageSize = 10 };

            await Assert.ThrowsAsync<BusinessRuleException>(() => _listingService.GetListingsAsync(pagination, filter));

            _mockRepo.Verify(
                l => l.GetListingsAsync(It.IsAny<ListingFilterRequestDto>(), It.IsAny<PaginationRequestDto>()),
                Times.Never);
        }
    }
}
