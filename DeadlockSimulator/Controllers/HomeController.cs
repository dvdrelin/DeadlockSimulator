using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using DeadlockSimulator.Services;

namespace DeadlockSimulator.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class HomeController : Controller
    {
        private static readonly DurableService Service = new DurableService();

        public ActionResult Index(CancellationToken cancellationToken)
        {
            var interestingWhatWeHaveGot= Service
                .DoUsefulStuffWithResult<double?>(cancellationToken, 0)
                .Result;

            var weGot = interestingWhatWeHaveGot ?? double.Epsilon;

            ViewBag.Message = $"Timeout with 0ms delay was requested." +
                              $" Here we've got as result:{weGot}";

            return View();
        }

        public ActionResult Deadlock(CancellationToken cancellationToken, int ms = 500)
        {
            var interestingWhatWeHaveGot = Service
                .DoUsefulStuffWithResult<double?>(cancellationToken, ms)
                .Result;
            
            var weGot = interestingWhatWeHaveGot ?? double.Epsilon;

            ViewBag.Message = $"Timeout with {ms}ms delay was requested." +
                              $" Here we've got as result:{weGot}";

            return View();
        }

        public ActionResult ConcurrencyTaskRunResult()
        {
            ViewBag.Message = $"Concurrency Task.Run().Result requested";
            return View();
        }
        public ActionResult ConcurrencyAsyncAwait()
        {
            ViewBag.Message = $"Concurrency Async/Await requested";
            return View();
        }
        public ActionResult ConcurrencyTaskReturn()
        {
            ViewBag.Message = $"Concurrency returning Task requested";
            return View();
        }

        #region next actions follows the minimum overhead such as serialization etc

        [HttpPost] 
        public async Task<string> GetAnEmptyStringAsJson(
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            Interlocked.Increment(ref _actionsCount);

            var statistics = GetStatistics();
            var result = Task.Run(
                () => new DurableService().DoUsefulStuffWithResult<string>(cancellationToken, 100), cancellationToken)
                .Result;

            Interlocked.Decrement(ref _actionsCount);
            return result ?? statistics;
        }

        [HttpPost] 
        public async Task<string> GetAnEmptyStringAsJsonAsync(
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _actionsCount);

            var statistics = GetStatistics();
            var result = await new DurableService().DoUsefulStuffWithResult<string>(cancellationToken, 100);

            Interlocked.Decrement(ref _actionsCount);
            return result ?? statistics;
        }

        [HttpPost] 
        public Task<string> GetAnEmptyStringAsJsonTask(
            CancellationToken cancellationToken)
        {
            return new DurableService()
                .DoUsefulStuffWithResult<string>(cancellationToken, 100)
                .ContinueWith(continuationTask => GetStatistics(), cancellationToken);
        }

        #endregion

        private static string GetStatistics()
        {

            var threadsCountOfCurrentProcess = Process.GetCurrentProcess().Threads.Count;
            return "Stop me please!" +
                   $"\r\n Actions in action={_actionsCount}" +
                   $"\r\n Current process threads count={threadsCountOfCurrentProcess}" +
                   $"\r\n ThreadId={Thread.CurrentThread.ManagedThreadId}";
        }
        private static volatile int _actionsCount;
    }
}