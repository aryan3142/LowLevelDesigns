using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

using Microsoft.AspNetCore.WebSockets;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace LowLevelDesigns.FileSharingAndEditingSystem
{
    public class Startup
    {
        // This is used to add services to the dependency injection container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSockets(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(30);
            });

            services.AddControllers();
            services.AddSingleton<WebSocketHandler>();
        }

        // This is used to configure the request processing pipeline, here you add middleware components
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            // Sets up the routing middleware.
            app.UseRouting();

            // Responsible for executing the endpoint and generating the response
            app.UseEndpoints(endpoints =>
            {
                // Maps the controller routes to the endpoints based on url params and Http methods
                endpoints.MapControllers();
            });
        }
    }

    [ApiController]
    [Route("api/collab")]
    public class CollaborationController : ControllerBase
    {
        private readonly WebSocketHandler _webSocketHandler;

        public CollaborationController(WebSocketHandler webSocketHandler)
        {
            _webSocketHandler = webSocketHandler;
        }

        [HttpGet("ws/{documentId")]
        public async Task GetWebSocket(string documentId)
        {
            /*
             * Here's a step by explanation:
             * 1. Check if the request is a websocket request and if the documentId is not null or empty
             * 2. The await keyword is used to wait for the AcceptWebSocketAsync() method to complete
             * 3. The using var statement declares a variable websocket and ensures that it will be disposed of when it goes out of scope
             * 4. The webSocket object is used withIn the scope of using statment and once the scope is exited, the Dispose method is called automatically.
             * 5. This process of "using var" is useful for managing the lifescycle of disposable objects in a clean and concise manner
             */
            if (HttpContext.WebSockets.IsWebSocketRequest && !string.IsNullOrEmpty(documentId))
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _webSocketHandler.ListenForMessages(webSocket, documentId);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile()
        {
            var form = await Request.ReadFormAsync();
            var file = form.Files[0];
            var filePath = Path.Combine("Uploads", file.FileName);
            Directory.CreateDirectory("Uploads");
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return Ok(new { Path = filePath });
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var filePath = Path.Combine("Uploads", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }

    public class RealTimeCollabApp
    {

    }
}
