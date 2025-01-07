using Microsoft.AspNetCore.Mvc;
using MV_test.Models;
using MV_test.Services;

namespace MV_test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("")]
    public List<Offer> GetAll([FromQuery] SearchParameters searchParameters)
    {
        return _searchService.Search(searchParameters);
    }
}
