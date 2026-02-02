using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Visonize.UsImaging.Domain.Service;
using Visonize.UsImaging.Infrastructure.Web.HubConfig;

namespace Visonize.UsImaging.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly IHubContext<ChartHub> _hub;

        public ChartController(IHubContext<ChartHub> hub, IRenderingService renderingService)
        {
            _hub = hub;
            ImageUpdater.hub = hub;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "Request Completed" });
        }

        public void UpdateImage(byte[] image)
        {
            _hub.Clients.All.SendAsync("TransferChartData", image);
        }
    }
}
