
using Microsoft.AspNetCore.SignalR;
using DupploPulse.UsImaging.Domain.Service;

namespace DupploPulse.UsImaging.Infrastructure.Web.HubConfig
{

    public class MouseEventHub : Hub
    {
        IRenderingService renderingService;
        public MouseEventHub(IRenderingService renderingService)
        {
            this.renderingService = renderingService;
        }

        public async Task SendMouseEvent(string eventType, float x, float y)
        {
            if(eventType == "mousedown")
            {
                this.renderingService.MouseDown(x,y);                
            }
            else if(eventType == "mouseup")
            {
                this.renderingService.MouseUp(x,y);        
            }
            else if(eventType == "mousemove")
            {
                this.renderingService.MouseMove(x,y);  
            }
            //this.renderingService
            // You can handle or broadcast the event to connected clients if needed
            //await Clients.All.SendAsync("ReceiveMouseEvent", eventType, x, y);
        }
    }
}