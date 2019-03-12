namespace MassTransit.LamarIntegration.ScopeProviders
{
    using System;
    using System.Collections.Generic;
    using Context;
    using GreenPipes;
    using GreenPipes.Payloads;
    using Lamar;
    using Saga;
    using Scoping;
    using Scoping.SagaContexts;


    public class LamarSagaScopeProvider<TSaga> :
        ISagaScopeProvider<TSaga>
        where TSaga : class, ISaga
    {
        readonly IContainer _container;
        readonly IList<Action<ConsumeContext>> _scopeActions;

        public LamarSagaScopeProvider(IContainer container)
        {
            _container = container;
            _scopeActions = new List<Action<ConsumeContext>>();
        }

        public void Probe(ProbeContext context)
        {
            context.Add("provider", "lamar");
        }

        public ISagaScopeContext<T> GetScope<T>(ConsumeContext<T> context)
            where T : class
        {
            if (context.TryGetPayload<INestedContainer>(out var existingNestedContainer))
            {
                existingNestedContainer.Inject<ConsumeContext>(context);

                return new ExistingSagaScopeContext<T>(context);
            }

            var nestedContainer = _container.GetNestedContainer(context);
            try
            {
                var proxy = new ConsumeContextProxy<T>(context, new PayloadCacheScope(context));
                proxy.UpdatePayload(nestedContainer);

                foreach (Action<ConsumeContext> scopeAction in _scopeActions)
                    scopeAction(proxy);

                return new CreatedSagaScopeContext<INestedContainer, T>(nestedContainer, proxy);
            }
            catch
            {
                nestedContainer.Dispose();
                throw;
            }
        }

        public ISagaQueryScopeContext<TSaga, T> GetQueryScope<T>(SagaQueryConsumeContext<TSaga, T> context)
            where T : class
        {
            if (context.TryGetPayload<INestedContainer>(out var existingNestedContainer))
            {
                existingNestedContainer.Inject<ConsumeContext>(context);

                return new ExistingSagaQueryScopeContext<TSaga, T>(context);
            }

            var nestedContainer = _container.GetNestedContainer(context);
            try
            {
                var proxy = new SagaQueryConsumeContextProxy<TSaga, T>(context, new PayloadCacheScope(context), context.Query);
                proxy.UpdatePayload(nestedContainer);

                foreach (Action<ConsumeContext> scopeAction in _scopeActions)
                    scopeAction(proxy);

                return new CreatedSagaQueryScopeContext<INestedContainer, TSaga, T>(nestedContainer, proxy);
            }
            catch
            {
                nestedContainer.Dispose();
                throw;
            }
        }

        public void AddScopeAction(Action<ConsumeContext> action)
        {
            _scopeActions.Add(action);
        }
    }
}
