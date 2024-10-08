﻿using Microsoft.AspNetCore.SignalR.Client;

namespace QuizzApp.Web
{
    public class RetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return retryContext.PreviousRetryCount switch
            {
                0 => TimeSpan.FromSeconds(2),
                1 => TimeSpan.FromSeconds(5),
                2 => TimeSpan.FromSeconds(10),
                3 => TimeSpan.FromSeconds(20),
                _ => TimeSpan.FromSeconds(40),
            };
        }
    }
}
