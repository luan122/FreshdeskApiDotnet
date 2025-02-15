using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using FreshdeskApi.Client.Agents;
using FreshdeskApi.Client.Channel;
using FreshdeskApi.Client.Companies;
using FreshdeskApi.Client.Contacts;
using FreshdeskApi.Client.Conversations;
using FreshdeskApi.Client.Groups;
using FreshdeskApi.Client.Products;
using FreshdeskApi.Client.Solutions;
using FreshdeskApi.Client.TicketFields;
using FreshdeskApi.Client.Tickets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FreshdeskApi.Client.Extensions
{
    public static class IocExtensions
    {
        public static IServiceCollection AddFreshdeskApiClient(
            this IServiceCollection serviceCollection,
            Action<FreshdeskConfiguration> options,
            Action<IHttpClientBuilder>? configureHttpClientBuilder = null
        )
        {
            serviceCollection.Configure(options);

            return serviceCollection.AddFreshdeskApiClient(configureHttpClientBuilder);
        }

        public static IServiceCollection AddFreshdeskApiClient(
            this IServiceCollection serviceCollection,
            Action<IHttpClientBuilder>? configureHttpClientBuilder = null
        )
        {
            serviceCollection.AddOptions();

            var httpClientBuilder = serviceCollection.AddHttpClient<IFreshdeskHttpClient, FreshdeskHttpClient>(ConfigureFreshdeskHttpClient);
            configureHttpClientBuilder?.Invoke(httpClientBuilder);

            serviceCollection.AddScoped<IFreshdeskTicketClient, FreshdeskTicketClient>();
            serviceCollection.AddScoped<IFreshdeskContactClient, FreshdeskContactClient>();
            serviceCollection.AddScoped<IFreshdeskGroupClient, FreshdeskGroupClient>();
            serviceCollection.AddScoped<IFreshdeskProductClient, FreshdeskProductClient>();
            serviceCollection.AddScoped<IFreshdeskAgentClient, FreshdeskAgentClient>();
            serviceCollection.AddScoped<IFreshdeskCompaniesClient, FreshdeskCompaniesClient>();
            serviceCollection.AddScoped<IFreshdeskSolutionClient, FreshdeskSolutionClient>();
            serviceCollection.AddScoped<IFreshdeskTicketFieldsClient, FreshdeskTicketFieldsClient>();
            serviceCollection.AddScoped<IFreshdeskConversationsClient, FreshdeskConversationsClient>();
            serviceCollection.AddScoped<IFreshdeskChannelApiClient, FreshdeskChannelApiClient>();

            serviceCollection.AddScoped<IFreshdeskClient, FreshdeskClient>();

            return serviceCollection;
        }

        public static void ConfigureFreshdeskHttpClient(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            var options = serviceProvider.GetRequiredService<IOptions<FreshdeskConfiguration>>();

            var freshdeskConfiguration = options.Value;

            httpClient.ConfigureHttpClient(freshdeskConfiguration);
        }

        public static HttpClient ConfigureHttpClient(this HttpClient httpClient, FreshdeskConfiguration freshdeskConfiguration)
        {
            var apiKey = string.IsNullOrWhiteSpace(freshdeskConfiguration.ApiKey)
                ? throw new ArgumentOutOfRangeException(nameof(freshdeskConfiguration.ApiKey), freshdeskConfiguration.ApiKey, "API Key can't be blank")
                : freshdeskConfiguration.ApiKey;
            var freshdeskDomain = string.IsNullOrWhiteSpace(freshdeskConfiguration.FreshdeskDomain)
                ? throw new ArgumentOutOfRangeException(nameof(freshdeskConfiguration.FreshdeskDomain), freshdeskConfiguration.FreshdeskDomain, "Freshdesk domain can't be blank")
                : freshdeskConfiguration.FreshdeskDomain;

            httpClient.BaseAddress = new Uri(freshdeskDomain, UriKind.Absolute);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.Default.GetBytes($"{apiKey}:X"))
            );

            return httpClient;
        }

        public class FreshdeskConfiguration
        {
            public string FreshdeskDomain { get; set; } = null!;

            public string ApiKey { get; set; } = null!;
        }
    }
}
