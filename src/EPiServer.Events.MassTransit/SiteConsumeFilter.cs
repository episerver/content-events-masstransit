using GreenPipes;
using MassTransit;
using System.Threading.Tasks;

namespace EPiServer.Events.MassTransit
{
    /// <summary>
    /// Adds filter to ignore consuming messages sent by itself.
    /// </summary>
    /// <typeparam name="T">Generic type will work on all consumers</typeparam>
    public class SiteConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        /// <summary>
        /// Part of the filter pipeline
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="next">The next step in the pipeline.</param>
        /// <returns>A <see cref="Task"/></returns>
        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            if (context.Headers.Get<string>("AppId") != MassTransitEventProvider.UniqueServerName)
            {
                await next.Send(context);
            }
        }

        /// <summary>
        /// Part of the filter pipeline
        /// </summary>
        /// <param name="context">The probe context.</param>
        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("SiteConsumeFilter");
        }
    }
}
