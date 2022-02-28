using Quartz;

namespace Plan.Web
{
    internal class LogJob : IJob
    {
        private readonly ILogger<LogJob> logger;

        public LogJob(ILogger<LogJob> logger)
        {
            this.logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation(context.FireTimeUtc.ToString());
            return Task.CompletedTask;
        }
    }
}
