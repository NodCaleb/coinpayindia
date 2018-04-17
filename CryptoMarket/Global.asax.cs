#region

using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CryptoMarket.Source;
using CryptoMarket.Source.Core.CustomCoinsProtocols;
using CryptoMarket.Source.Core.Platforms;
using CryptoMarket.Source.Core.Platforms.BTC_e;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Quartz;
using Quartz.Impl;

#endregion

namespace CryptoMarket {
    /// <summary>
    /// </summary>
    public class MvcApplication : HttpApplication {
        /// <summary>
        /// </summary>
        protected void Application_Start() {
            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();
            // WebApiConfig.Register(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BundleTable.EnableOptimizations = true;

      

            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new PrincipalUserIdProvider());
            GlobalHost.Configuration.ConnectionTimeout = new TimeSpan(0, 0, 5, 0);
            GlobalHost.Configuration.KeepAlive = new TimeSpan(0, 0, 0, 2);

            ISchedulerFactory schedFact = new StdSchedulerFactory();

            var sched = schedFact.GetScheduler();
            sched.Start();

            // YoGoldProtocol.RegisterTransferFilter();
            // YoCoinProtocol.RegisterTransferFilter();

            //sched.ScheduleJob(JobBuilder.Create<DepositsManager.DepositAutomation.EthereumDepositProcessor>().Build(), TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(3).RepeatForever()).Build());

            //sched.ScheduleJob(JobBuilder.Create<YoCoinProtocol.DepositYoCJob>().Build(), TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()).Build());

            //sched.ScheduleJob(JobBuilder.Create<YoGoldProtocol.DepositYoGJob>().Build(), TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()).Build());

            sched.ScheduleJob(JobBuilder.Create<MatchingEngine.MatchJob>().Build(), TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever()).Build());

            //sched.ScheduleJob(JobBuilder.Create<Workers.OrdersJob>().Build(), TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(Workers.OrdersJob.ExecutionIntervalSeconds).RepeatForever()).Build());

            sched.ScheduleJob(JobBuilder.Create<WithdrawManager.WithdrawProcessor>().Build(), TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()).Build());

            sched.ScheduleJob(JobBuilder.Create<DepositsManager.DepositAutomation.DepositProcessor>().Build(), TriggerBuilder.Create().StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()).Build());

        }
    }
}