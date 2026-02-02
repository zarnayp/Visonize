using System.Drawing.Imaging;
using Visonize.UsImaging.Infrastructure.Web.Models;

namespace Visonize.UsImaging.Infrastructure.Web
{
    public class TimerManager
    {
        private System.Threading.Timer? _timer;
        private AutoResetEvent? _autoResetEvent;
        private Action? _action;
        public DateTime TimerStarted { get; set; }
        public bool IsTimerStarted { get; set; }

        public void PrepareTimer(Action action)
        {
            _action = action;
            _autoResetEvent = new AutoResetEvent(false);
            _timer = new System.Threading.Timer(Execute, _autoResetEvent, 1000, 2000);
            TimerStarted = DateTime.Now;
            IsTimerStarted = true;
        }

        public void Execute(object? stateInfo)
        {
            _action();

            if ((DateTime.Now - TimerStarted).TotalSeconds > 60)
            {
                IsTimerStarted = false;
                _timer.Dispose();
            }
        }
    }


    public class DataManager
    {
        public static string GetImageData()
        {

            System.Drawing.Image image = System.Drawing.Image.FromFile("d:\\Projects\\Visonize\\UltrasoundApplication\\ultrasound-app\\src\\assets\\image.png");
            using (MemoryStream myStream = new MemoryStream())
            {
                image.Save(myStream, ImageFormat.Png);
                return Convert.ToBase64String(myStream.ToArray());
            }
        }

        public static List<ChartModel> GetData()
        {
            var r = new Random();
            return new List<ChartModel>()
        {
            new ChartModel { Data = new List<int> { r.Next(1, 40) }, Label = "Data1", BackgroundColor = "#5491DA" },
            new ChartModel { Data = new List<int> { r.Next(1, 40) }, Label = "Data2", BackgroundColor = "#E74C3C" },
            new ChartModel { Data = new List<int> { r.Next(1, 40) }, Label = "Data3", BackgroundColor = "#82E0AA" },
            new ChartModel { Data = new List<int> { r.Next(1, 40) }, Label = "Data4", BackgroundColor = "#E5E7E9" }
        };
        }
    }
}