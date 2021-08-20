using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusTriggerAutoCompleteTrials.Functions
{
    public static class WithoutAutoCompleteAttribute
    {
        [FunctionName("WithoutAutoCompleteAttribute")]
        public static async Task RunAsync(
           [ServiceBusTrigger("sans-autocomplete")] Message message,
           MessageReceiver messageReceiver,
           ILogger logger,
           CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(1)); // Delay it a bit

            var text = Encoding.UTF8.GetString(message.Body);

            logger.LogWarning($"Now: {DateTime.UtcNow}");
            logger.LogWarning($"Locked until: {message.SystemProperties.LockedUntilUtc}");
            logger.LogWarning($"Lock token: {message.SystemProperties.LockToken}");

            switch (text)
            {
                case "abandon":
                    logger.LogWarning("Abandon and throw exception");
                    await messageReceiver.AbandonAsync(message.SystemProperties.LockToken, new Dictionary<string, object> { ["Hello"] = "Abandoned" });
                    throw new Exception("test");
                case "deadletter":
                    logger.LogWarning("Deadletter and throw exception");
                    await messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken, new Dictionary<string, object> { ["Hello"] = "Deadlettered" });
                    throw new Exception("test");
                case "timeout":
                    logger.LogWarning("Time out operation");
                    await Task.Delay(TimeSpan.FromSeconds(120), cancellationToken);
                    break;
                case "exception":
                    logger.LogWarning("Just throw exception");
                    throw new Exception("test");
                case "nothing":
                    logger.LogWarning("Do nothing");
                    break;
                default:
                    logger.LogWarning("Complete successfully");
                    await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
                    break;
            }
        }
    }
}
