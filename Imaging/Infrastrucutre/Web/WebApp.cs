using System.IO.MemoryMappedFiles;
using System.Reflection.PortableExecutable;
using System.Diagnostics;

using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.SignalR;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

using DupploPulse.UsImaging.Domain.Service;
using DupploPulse.UsImaging.Infrastructure.Web.HubConfig;
using System.Collections.Concurrent;
using DupploPulse.UsImaging.Domain.Service.Infrastructure;

namespace DupploPulse.UsImaging.Infrastructure.Web
{
    internal class ImageUpdater
    {
        private static List<byte[]> cine = new List<byte[]>();
        public static IHubContext<ChartHub> hub;

        public ImageUpdater(IRenderingService renderingService)
        {
            if(renderingService!= null)
            {
/*                  Thread threadDequeue = new Thread(()=>{
                    Console.WriteLine("Thread started");
                    while(true)
                    {
                        
                        byte[] img;
                        bool success = this.imageQueue.TryDequeue(out img);
                        if(success)
                        {
                            Task.Run(()=>{
                            using (MemoryStream stream = new MemoryStream(img))
                             {                
                                string base64String = "";

                                // Create an Image object from the RGBA data
                                using (Image<Rgba32> image = Image.LoadPixelData<Rgba32>(img, 1024, 720))
                                {
                                    // Save PNG image to a memory stream
                                    using (MemoryStream pngStream = new MemoryStream())
                                    {
                                        image.Save(pngStream, new PngEncoder());

                                        // Convert PNG image to Base64 string
                                        base64String = Convert.ToBase64String(pngStream.ToArray());
                                        imagePngQueue.Enqueue(base64String);
                                    // base64String = Convert.ToBase64String(rgbaByteArray);
                                    }
                                }              

               
                             }  
                            });
                        }
                        Thread.Sleep(16);
                    }
                }); */
                
                //threadDequeue.Start();

               // renderingService.ImageRendered += OnImageRendered;

               // threadDequeue.Start();
            }
            else
            {
                Dictionary<int,string> images = new Dictionary<int, string>();
                for(int i = 0; i<10 ;i++)
                {
                    images.Add(i,File.ReadAllText($"Assets//serializedPngImage{i}.txt"));
                }
                Thread thread = new Thread(()=>{
                    int i=0;
                    while(true)
                    {
                        if(hub!=null)
                        {
                            
                            hub.Clients.All.SendAsync("TransferChartData", images[i]);
                            i++;
                            if(i==10) i = 0;
                            Thread.Sleep(16);
                        }
                    }
                });

                thread.Start();
            }
        }

/*         private ConcurrentQueue<byte[]> imageQueue = new ConcurrentQueue<byte[]>();
        private ConcurrentQueue<string> imagePngQueue = new ConcurrentQueue<string>(); */

        private  void OnImageRendered(object obj, IRgbImageReference rgbReference)
        {
            var rgbaByteArray = new byte[1] ;//= (Mana)rgbReference.
            var sw1 = Stopwatch.StartNew();

            int numOfPixels = 1024 * 720;
            
            byte[] red = new byte[numOfPixels];
            byte[] green = new byte[numOfPixels];
            byte[] blue = new byte[numOfPixels];
            
            for (int i = 0; i < numOfPixels; i++)
            {
                int index = i * 4;
                red[i] = rgbaByteArray[index];
                green[i] = rgbaByteArray[index + 1];
                blue[i] = rgbaByteArray[index + 2];
            }
            
            sw1.Stop();
            
            Console.WriteLine((int)sw1.ElapsedMilliseconds);
             //using (MemoryStream stream = new MemoryStream(rgbaByteArray))
              //  {                
                    string base64String = "";

                    // Create an Image object from the RGBA data
/*                     using (Image<Rgba32> image = Image.LoadPixelData<Rgba32>(rgbaByteArray, 1024, 720))
                    {
                        // Save PNG image to a memory stream
                        using (MemoryStream pngStream = new MemoryStream())
                        {
                            image.Save(pngStream, new PngEncoder());

                            // Convert PNG image to Base64 string
                            base64String = Convert.ToBase64String(pngStream.ToArray());
                          // base64String = Convert.ToBase64String(rgbaByteArray);
                        }
                    }      */            

                        if(hub!=null)
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            hub.Clients.All.SendAsync("TransferChartData", rgbaByteArray);
                            sw.Stop();
                            //Console.WriteLine("--- sw "+ sw.ElapsedMilliseconds);
                        }
                } 
          // });
       // }

    }

    public class WebApp
    {
        ImageUpdater iu;

        public WebApp(IRenderingService renderingService)
        {

            string[] args = new string[0];
            iu= new ImageUpdater(renderingService);

            var builder = WebApplication.CreateBuilder(args);


           // builder.Configuration  = configuration;
/*  builder.Configuration 
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder                
                    .WithOrigins("http://localhost:4200")
                   // .WithOrigins("*")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            }); */
 
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5000); // Replace 5000 with your desired port number
            }); 

            builder.Services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .SetIsOriginAllowed((host) => true)
                            .AllowAnyHeader());
                }); 

            builder.Services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(6);
                options.MaximumReceiveMessageSize = 1024 * 1024 * 10;
            }).AddMessagePackProtocol(); 

            builder.Services.AddSingleton<TimerManager>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton(renderingService);
            
            var app = builder.Build();
            
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseCors("CorsPolicy");

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ChartHub>("/chart");
            app.MapHub<MouseEventHub>("/mouseEventHub");

            //var connection = app.GetConnection();

            app.Run();
           

            Console.WriteLine("Finished UltrasoundAppBackEnd");
        }
    }
}