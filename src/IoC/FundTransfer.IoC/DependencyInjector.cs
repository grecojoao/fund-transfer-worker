using FundTransfer.Domain.AcessoAccount;
using FundTransfer.Domain.Bus.Publishers;
using FundTransfer.Domain.Handlers;
using FundTransfer.Domain.Repositories;
using FundTransfer.Domain.Services.Contracts;
using FundTransfer.Infra.RabbitMq.Consumers;
using FundTransfer.Infra.RabbitMq.Publishers;
using FundTransfer.Infra.Storage.RavenDb.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;

namespace FundTransfer.IoC
{
    public static class DependencyInjector
    {
        public static Task InjectDependencies(IServiceCollection services, IConfiguration configuration)
        {
            InjectDomain(services);
            InjectStorage(services, configuration);
            InjectBus(services);
            return Task.CompletedTask;
        }

        private static void InjectDomain(IServiceCollection services)
        {
            services.AddTransient<FundTransferHandler>();
            services.AddTransient<IAccountService, AccountService>();
        }

        private static void InjectStorage(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITransferRepository, TransferRepository>();
            services.AddSingleton(a => ConfigureDataBase(configuration));
        }

        private static void InjectBus(IServiceCollection services)
        {
            services.AddTransient<TransferConsumer>();
            services.AddTransient<PendingTransferConsumer>();
            services.AddTransient<IBusPublisher, PendingTransferPublisher>();
        }

        private static DocumentStore ConfigureDataBase(IConfiguration configuration) =>
            new()
            {
                Urls = new string[] { configuration["ConnectionStrings:RavenDb:Url"] },
                Database = configuration["ConnectionStrings:RavenDb:DataBase"]
            };
    }
}