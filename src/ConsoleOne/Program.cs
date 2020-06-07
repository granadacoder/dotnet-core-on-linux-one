namespace GranadaCoder.OrganizaionApp.ConsoleOne
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GranadaCoder.Organizaion.Domain;
    using GranadaCoder.OrganizaionApp.Bal.Managers;
    using GranadaCoder.OrganizaionApp.Bal.Managers.Interfaces;
    using GranadaCoder.OrganizationApp.Dal;
    using GranadaCoder.OrganizationApp.Dal.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog;
    using NLog.Extensions.Logging;

    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Logger lgr = LogManager.GetCurrentClassLogger();
            try
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                IServiceProvider servicesProvider = BuildDi(config);
                using (servicesProvider as IDisposable)
                {
                    IEmployeeManager empMan = servicesProvider.GetRequiredService<IEmployeeManager>();
                     empMan.DoSomething();

                    Employee emp = await empMan.GetByID(int.MaxValue);
                    ShowEmployee(lgr, "GetByID", emp);
                    ICollection<Employee> emps = await empMan.GetByDateOfBirth(DateTime.MaxValue);
                    ShowEmployeeCollection(lgr, "GetByDateOfBirth", emps);

                    Console.WriteLine("Press ANY key to exit");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                lgr.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }

            Console.WriteLine("Returning 0 and exiting.");

            return 0;
        }

        private static void ShowEmployee(Logger lgr, string label, Employee emp)
        {
            ICollection<Employee> emps = new List<Employee> { emp };
            ShowEmployeeCollection(lgr, label, emps);
        }

        private static void ShowEmployeeCollection(Logger lgr, string label, ICollection<Employee> emps)
        {
            if (null != emps)
            {
                foreach (Employee emp in emps)
                {
                    string msg = string.Format("{0} ::: {1},{2},{3},{4}", label, emp.ID, emp.FirstName, emp.LastName, emp.DateOfBirth);
                    lgr.Info(msg);
                }
            }
        }

        private static IServiceProvider BuildDi(IConfiguration config)
        {
            return new ServiceCollection()
                .AddSingleton<IEmployeeManager, EmployeeManager>()
                .AddTransient<IEmployeeDataLayer, EmployeeDataLayer>()

                .AddLogging(loggingBuilder =>
                {
                    // configure Logging with NLog
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    loggingBuilder.AddNLog(config);
                })

                .AddSingleton<IConfiguration>(config)
                .BuildServiceProvider();
        }
    }
}