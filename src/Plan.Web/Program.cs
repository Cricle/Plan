using Azure.Identity;
using System.Net.Mail;

namespace Plan.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();
            builder.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>();

                    }
                    else
                    {
                        var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
                            config.AddAzureKeyVault(
                            keyVaultEndpoint,
                            new DefaultAzureCredential());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
