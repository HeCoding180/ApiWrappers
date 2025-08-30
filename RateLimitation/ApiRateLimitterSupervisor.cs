using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWrappers.RateLimitation
{
    public static class ApiRateLimitterSupervisor
    {
        //   ---   Public Properties   ---

        /// <summary>
        /// Event that is raised when the rate limiters are to be started.
        /// </summary>
        public static event Action? Started;

        /// <summary>
        /// Event that is raised when the rate limiters are to be stopped.
        /// </summary>
        public static event Func<Task>? Stopped;

        //   ---   Public Methods   ---

        /// <summary>
        /// Method used to start the rate limiters.
        /// </summary>
        public static void Start()
        {
            Started?.Invoke();
        }

        /// <summary>
        /// Method used to asynchronously stop the rate limiters.
        /// </summary>
        public static async Task StopAsync()
        {
            if (Stopped is null)
                return;

            IEnumerable<Func<Task>> stopTasks = Stopped.GetInvocationList().Cast<Func<Task>>();

            foreach (Func<Task> stopTask in stopTasks)
            {
                await stopTask();
            }
        }
    }
}
