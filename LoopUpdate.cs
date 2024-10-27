using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.Resources;
using System.Text.Json;
using Azure.ResourceManager.AppService.Models;

namespace novocain.Function
{
    public class LoopUpdate
    {
        private readonly ILogger<LoopUpdate> _logger;

        public LoopUpdate(ILogger<LoopUpdate> logger)
        {
            _logger = logger;
        }

        [Function(nameof(LoopUpdate))]
        public async Task Run([ServiceBusTrigger("loopholequeue", Connection = "loopholeASB_SERVICEBUS")]ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
        {
            ArmClient client = new ArmClient(new DefaultAzureCredential());
            SubscriptionResource subscription = client.GetDefaultSubscription();
            ResourceGroupResource resourceGroup = client.GetDefaultSubscription().GetResourceGroup("staticWebAppRG");
            Azure.Response<StaticSiteResource> v = resourceGroup.GetStaticSiteAsync("staticWebApp").Result;

            var settingsDict = new AppServiceConfigurationDictionary();
            settingsDict.Properties["LoopHoleURL"] = message.Body.ToString();
            var vv = v.Value.CreateOrUpdateAppSettings(settingsDict);
            string jsonString = JsonSerializer.Serialize(vv);

            //return new OkObjectResult("Welcome to Azure Functions! \n" + jsonString);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
