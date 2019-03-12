﻿namespace MassTransit.Scoping
{
    using System;
    using Courier;


    public interface ICompensateActivityScopeContext<out TActivity, out TLog> :
        IDisposable
        where TActivity : class, CompensateActivity<TLog>
        where TLog : class
    {
        CompensateActivityContext<TActivity, TLog> Context { get; }
    }
}