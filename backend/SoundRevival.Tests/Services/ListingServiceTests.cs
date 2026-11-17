using Microsoft.Extensions.Time.Testing;
using Moq;
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
    }
}
