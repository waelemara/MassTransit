namespace Automatonymous.Activities
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit;


    public class FaultedPublishActivity<TInstance, TData, TException, TMessage> :
        Activity<TInstance, TData>
        where TInstance : SagaStateMachineInstance
        where TData : class
        where TMessage : class
        where TException : Exception
    {
        readonly AsyncEventExceptionMessageFactory<TInstance, TData, TException, TMessage> _asyncMessageFactory;
        readonly EventExceptionMessageFactory<TInstance, TData, TException, TMessage> _messageFactory;
        readonly IPipe<PublishContext<TMessage>> _publishPipe;

        public FaultedPublishActivity(EventExceptionMessageFactory<TInstance, TData, TException, TMessage> messageFactory,
            Action<PublishContext<TMessage>> contextCallback)
            : this(contextCallback)
        {
            _messageFactory = messageFactory;
        }

        public FaultedPublishActivity(AsyncEventExceptionMessageFactory<TInstance, TData, TException, TMessage> messageFactory,
            Action<PublishContext<TMessage>> contextCallback)
            : this(contextCallback)
        {
            _asyncMessageFactory = messageFactory;
        }

        FaultedPublishActivity(Action<PublishContext<TMessage>> contextCallback)
        {
            _publishPipe = contextCallback != null ? Pipe.Execute(contextCallback) : Pipe.Empty<PublishContext<TMessage>>();
        }

        void Visitable.Accept(StateMachineVisitor inspector)
        {
            inspector.Visit(this);
        }

        Task Activity<TInstance, TData>.Execute(BehaviorContext<TInstance, TData> context, Behavior<TInstance, TData> next)
        {
            return next.Execute(context);
        }

        async Task Activity<TInstance, TData>.Faulted<T>(BehaviorExceptionContext<TInstance, TData, T> context,
            Behavior<TInstance, TData> next)
        {
            if (context.TryGetExceptionContext(out ConsumeExceptionEventContext<TInstance, TData, TException> exceptionContext))
            {
                var message = _messageFactory?.Invoke(exceptionContext) ?? await _asyncMessageFactory(exceptionContext).ConfigureAwait(false);

                await exceptionContext.Publish(message, _publishPipe).ConfigureAwait(false);
            }

            await next.Faulted(context).ConfigureAwait(false);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("publish-faulted");
            _publishPipe.Probe(scope);
        }
    }
}
