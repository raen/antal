using Antal.Core.Application.Abstractions;
using Antal.Core.Application.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Antal.Functions;

/// <summary>
/// Azure Functions timer-triggered function that periodically fetches jokes and persists them.
/// </summary>
/// <param name="jokeFetcher">The application service used to fetch and persist jokes.</param>
/// <param name="options">The options controlling fetch behavior, including <see cref="JokeFetcherOptions.Count"/>.</param>
public class JokeFetcherFunction(IJokeFetcher jokeFetcher, IOptions<JokeFetcherOptions> options)
{
    [Function("JokeFetcher")]
    public async Task Run(
        [TimerTrigger("%JokeFetcherTimerSchedule%")] TimerInfo myTimer,
        FunctionContext context)
    {
        var logger = context.GetLogger<JokeFetcherFunction>();
    
        try 
        {
            var result = await jokeFetcher.GetJokesAsync(options.Value.Count, context.CancellationToken);
            logger.LogInformation("Success: {Total} fetched, {Saved} saved", 
                result.TotalFetched, result.SavedToDatabase);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch jokes");
            throw; // Let Azure Functions handle retry
        }
    }
}