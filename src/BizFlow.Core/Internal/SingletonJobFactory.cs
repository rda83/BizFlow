﻿using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace BizFlow.Core.Internal
{
    class SingletonJobFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;

        public SingletonJobFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job) { }
    }
}
