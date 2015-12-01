using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace AiLib.MvcHttp
{
    public static class MvcBackgroundTask
    {
        // http://haacked.com/archive/2011/10/16/the-dangers-of-implementing-recurring-background-tasks-in-asp-net.aspx/

        public static void IISRun(Task task)
        {
            IISTaskManager.Run(() =>
              { });
        }


        // http://brian-federici.com/blog/2013/7/8/ensuring-tasks-complete-in-aspnet-mvc
        public class IISTaskManager
        {

            public static void Run(Action action)
            {
                new IISBackgroundTask().DoWork(action);
            }
        }

        // Generic object for completing tasks in a background thread
        public class IISBackgroundTask : IRegisteredObject
        {
            // Constructs the object and registers itself with the hosting environment.
            public IISBackgroundTask()
            {
                HostingEnvironment.RegisterObject(this);
            }

            //   Requests a registered object to unregister.
            void IRegisteredObject.Stop(bool immediate) { }

            // Invokes the <paramref name="action"/> as a Task.
            public void DoWork(Action action)
            {
                try
                {
                    _task = Task.Run(action);
                }
                catch (AggregateException ex)
                {
                    // Log exceptions
                    foreach (var innerEx in ex.InnerExceptions)
                    {
                        //_logger.ErrorException(innerEx.ToString(), innerEx);
                    }
                }
                catch (Exception) // ex)
                {
                    //_logger.ErrorException(ex.ToString(), ex);
                }
            }


            private Task _task;
            // private static Logger _logger = LogManager.GetCurrentClassLogger();

        }

    }
}
