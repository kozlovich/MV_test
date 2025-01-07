using Microsoft.AspNetCore.Mvc;
using MV_test.Models;
using MV_test.Services;

namespace MV_test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly ISeedService _seedService;
    public DataController(ISeedService seedService)
    {
        _seedService = seedService;
    }

    [HttpGet("")]
    public List<Accomodation> Get()
    {
        return _seedService.GetSeededData();
    }
}
