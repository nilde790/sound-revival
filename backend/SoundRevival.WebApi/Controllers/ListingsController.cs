using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoundRevival.Dto.Common;
using SoundRevival.Dto.Listings;
using SoundRevival.Repository.Interfaces;
using SoundRevival.WebApi.Extensions;

namespace SoundRevival.WebApi.Controllers
{
    [Route("api/listings")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService)
        {
            _listingService = listingService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetListings([FromQuery]PaginationRequestDto pagination, [FromQuery]ListingFilterRequestDto filter)
        {
            var result = await _listingService.GetListingsAsync(pagination, filter);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
           
            var result = await _listingService.GetByIdAsync(id, User.GetUserIdOrNull());
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreateListingRequestDto request)
        {
         
            var result = await _listingService.CreateAsync(User.GetUserId(),request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },result);

        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, UpdateListingRequestDto request)
        {
            var isAdmin = User.IsInRole("admin");
            var result = await _listingService.UpdateAsync(id, User.GetUserId(), request, isAdmin);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var isAdmin = User.IsInRole("admin");
            await _listingService.DeleteAsync(id, User.GetUserId(), isAdmin);
            return NoContent();

        }

        [HttpPatch("{id:guid}/status")]
        [Authorize]
        public async Task<IActionResult> ChangeStatus(Guid id, UpdateListingStatusRequestDto request)
        {
            var isAdmin = User.IsInRole("admin");
            await _listingService.ChangeStateAsync(id, User.GetUserId(), request, isAdmin);
            return NoContent();



        }
    }
}
