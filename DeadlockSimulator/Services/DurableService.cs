using System.Threading;
using System.Threading.Tasks;

namespace DeadlockSimulator.Services
{
    public class DurableService
    {
        public async Task DoUsefulStuff(CancellationToken cancellationToken, int timeout = 500)
        {
            await Task.Delay(timeout, cancellationToken);
        }

        public async Task<T> DoUsefulStuffWithResult<T>(CancellationToken cancellationToken, int timeout = 500)
        {
            await DoUsefulStuff(cancellationToken, timeout);
            return default;
        }
    }
}