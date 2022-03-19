using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plan.Core.Models;
using Plan.Identity;
using Plan.Services.Jobs;
using Quartz;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan.Services
{
    public class NotifyEngine
    {
        public NotifyEngine(IServiceScopeFactory scopeFactory, NotifyJobManager notifyJobManager)
        {
            ScopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            NotifyJobManager = notifyJobManager ?? throw new ArgumentNullException(nameof(notifyJobManager));
        }

        private INotifyProvider notifyProvider;

        public IServiceScopeFactory ScopeFactory { get; }

        public NotifyJobManager NotifyJobManager { get; }

        public INotifyProvider NotifyProvider => notifyProvider;

        public void SetNotifyProvider(INotifyProvider provider)
        {
            notifyProvider = provider;
        }

        protected void ThrowIfNotifyProviderNull()
        {
            if (notifyProvider == null)
            {
                throw new InvalidOperationException($"NotifyProvider is null, must call SetNotifyProvider");
            }
        }

        public Task SendAsync(PlanItem item)
        {
            ThrowIfNotifyProviderNull();
            return NotifyProvider.SendAsync(new NotifyContext { Engine = this, Item = item });
        }
    }
    public interface INotifyProvider
    {
        Task SendAsync(INotifyContext context);
    }
    public interface INotifyContext
    {
        NotifyEngine Engine { get; }

        PlanItem Item { get; }
    }
    public abstract class NotifyProviderBase : INotifyProvider, IDisposable
    {
        private ILogger logger;
        private IServiceScope scope;
        private PlanDbContext dbContext;
        private IDatabase redis;

        public ILogger Logger => logger;
        public IServiceProvider ServiceProvider => scope.ServiceProvider;
        public PlanDbContext DbContext => dbContext;
        public IDatabase Redis => redis;

        public Task SendAsync(INotifyContext context)
        {
            scope = context.Engine.ScopeFactory.CreateScope();
            var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
            logger = loggerFactory.CreateLogger(GetType());
            dbContext = scope.ServiceProvider.GetService<PlanDbContext>();
            redis = scope.ServiceProvider.GetService<IDatabase>();
            return OnSendAsync(context);
        }

        protected abstract Task OnSendAsync(INotifyContext context);

        public void Dispose()
        {
            scope?.Dispose();
        }
    }
    public enum NotifyTypes : int
    {
        Pushy = 0,
        Gandi = 1,
    }
}
