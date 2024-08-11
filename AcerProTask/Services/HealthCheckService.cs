using AcerProTask.Data;
using AcerProTask.Models;
using Microsoft.EntityFrameworkCore;


namespace AcerProTask.Services
{

    //Olay veya Mesajlaşma Sistemi Kurulumu alternatifi: Uygulamamızda bir olay veya mesajlaşma sistemi
    //(örneğin, .NET’in IObserver<T> yapısı, SignalR, Azure Service Bus, RabbitMQ, Kafka vb.)
    //kurarak güncellemeleri dinleyebiliriz.

    public class HealthCheckService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HealthCheckService(
            IServiceScopeFactory scopeFactory,
            ILogger<HealthCheckService> logger,
            IHttpClientFactory httpClientFactory
            )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var taskCancellationSources = new Dictionary<int, CancellationTokenSource>();

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var apps = await dbContext.TargetApps.Include(t => t.User).ToListAsync(stoppingToken);
                    var activeApps = apps
                        .Where(app => app.IsActive && !app.IsDeleted && app.MonitoringInterval > 0)
                        .ToList();

                    // İptal edilmesi gereken görevleri bul
                    var appsToRemove = taskCancellationSources.Keys.Except(activeApps.Select(a => a.Id)).ToList();
                    foreach (var appId in appsToRemove)
                    {
                        if (taskCancellationSources.TryGetValue(appId, out var cts))
                        {
                            cts.Cancel();
                            cts.Dispose();
                            taskCancellationSources.Remove(appId);
                        }
                    }

                    // Yeni görevler oluşturulacak
                    var newTasks = activeApps
                        .Where(app => !taskCancellationSources.ContainsKey(app.Id))
                        .ToList();

                    foreach (var app in newTasks)
                    {
                        var cts = new CancellationTokenSource();
                        taskCancellationSources[app.Id] = cts;

                        _ = Task.Run(async () =>
                        {
                            await MonitorAppAsync(app, httpClient, cts.Token);
                        }, cts.Token);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            async Task MonitorAppAsync(TargetApp app, HttpClient httpClient, CancellationToken cancellationToken)
            {
                using (var innerScope = _scopeFactory.CreateScope())
                {
                    var innerDbContext = innerScope.ServiceProvider.GetRequiredService<AppDbContext>();

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var response = await httpClient.GetAsync(app.Url, cancellationToken);
                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogWarning($"TargetApp {app.Name} is down! StatusCode: {(int)response.StatusCode}");

                                var subject = $"Target App {app.Name} is down {(int)response.StatusCode}.";
                                var htmlContent = $"<strong>The application {app.Name} is down with status code {(int)response.StatusCode}.</strong>";

                                await EmailService.SendEmailAsync(app.User.Email, subject, htmlContent);

                                var log = new HealthCheckLog
                                {
                                    TargetAppId = app.Id,
                                    StatusCode = (int)response.StatusCode,
                                    ErrorMessage = $"Status code: {(int)response.StatusCode}, indicates the application {app.Name} is down. {response.ReasonPhrase}",
                                    Timestamp = DateTime.UtcNow
                                };

                                innerDbContext.HealthCheckLogs.Add(log);
                                await innerDbContext.SaveChangesAsync(cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error checking health for TargetApp {app.Name}");

                            var log = new HealthCheckLog
                            {
                                TargetAppId = app.Id,
                                StatusCode = 0,
                                ErrorMessage = ex.Message,
                                Timestamp = DateTime.UtcNow
                            };

                            innerDbContext.HealthCheckLogs.Add(log);
                            await innerDbContext.SaveChangesAsync(cancellationToken);


                        }

                        await Task.Delay(TimeSpan.FromMinutes(app.MonitoringInterval), cancellationToken);
                    }
                }
            }

        }







    }




}
