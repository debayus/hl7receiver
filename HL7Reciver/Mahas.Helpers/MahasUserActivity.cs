//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;

//namespace Mahas.Helpers
//{
//    public class MahasUserActivity
//    {
//        private IConfiguration Configuration { get; }
//        private HttpRequest Request { get; set; }
//        private ClaimsPrincipal User { get; set; }

//        public MahasUserActivity(IConfiguration configuration, HttpRequest request = null, ClaimsPrincipal user = null)
//        {
//            Configuration = configuration;
//            Request = request;
//            User = user;
//        }

//        public static async void Save<T>(string action, T model, IConfiguration configuration, HttpRequest request = null, ClaimsPrincipal user = null) where T : class, new()
//        {
//            var valus = new List<string>();
//            var table = typeof(T).GetCustomAttribute<DbTableAttribute>()?.Name ?? typeof(T).Name;
//            foreach (var prop in typeof(T).GetProperties())
//            {
//                var keyAtt = prop.GetCustomAttribute<DbKeyAttribute>();
//                if (keyAtt?.Key ?? false) valus.Add(prop.GetValue(model).ToString());
//            }

//            var s = new MahasUserActivity(configuration, request, user);
//            await s.Save($"{action} {table} {string.Join(", ", valus)}");
//        }

//        public static async void Save(string action, IConfiguration configuration, HttpRequest request = null, ClaimsPrincipal user = null)
//        {
//            var s = new MahasUserActivity(configuration, request, user);
//            await s.Save(action);
//        }

//        private async Task Save(string action = null)
//        {
//            try
//            {
//                var config = Configuration.GetSection("UserActivity");
//                if (!bool.Parse(config["Enable"])) return;
//                using var s = new MahasConnection(config["ConnectionString"]);
//                s.OpenTransaction();
//                await s.Insert(new MahasUserActivityModel()
//                {
//                    ApplicationCode = config["ApplicationCode"],
//                    ApplicationName = config["ApplicationName"],
//                    Date = DateTime.Now,
//                    Host = Request?.Headers["Host"].ToString(),
//                    UserAgent = Request?.Headers["User-Agent"].ToString(),
//                    Referer = Request?.Headers["Referer"].ToString(),
//                    IpAddress = Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
//                    Id_User = User?.FindFirstValue(ClaimTypes.NameIdentifier),
//                    Action = action,
//                });
//                await s.Transaction.CommitAsync();
//            }
//            catch (Exception ex)
//            {
//                var time = $"{DateTime.Now:HH}:{DateTime.Now:mm}:{DateTime.Now:ss}";
//                if (!Directory.Exists("logs"))
//                    Directory.CreateDirectory("logs");
//                using StreamWriter writer = File.AppendText($"logs/{DateTime.Today:yyyyMMdd}.txt");
//                writer.WriteLine($"{time} : {ex.Message}");
//            }
//        }
//    }

//    [DbTable("trUserActivity")]
//    public class MahasUserActivityModel
//    {
//        [DbKey(true)]
//        [DbColumn]
//        public int Id { get; set; }

//        [DbColumn]
//        public DateTime Date { get; set; }

//        [DbColumn]
//        public string ApplicationCode { get; set; }

//        [DbColumn]
//        public string ApplicationName { get; set; }

//        [DbColumn]
//        public string Id_User { get; set; }

//        [DbColumn]
//        public string IpAddress { get; set; }

//        [DbColumn]
//        public string Referer { get; set; }

//        [DbColumn]
//        public string UserAgent { get; set; }

//        [DbColumn]
//        public string Host { get; set; }

//        [DbColumn]
//        public string Action { get; set; }
//    }
//}