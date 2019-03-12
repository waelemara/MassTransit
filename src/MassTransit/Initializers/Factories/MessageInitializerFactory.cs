namespace MassTransit.Initializers.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Conventions;
    using GreenPipes.Internals.Extensions;


    public class MessageInitializerFactory<TMessage, TInput> :
        IMessageInitializerFactory<TMessage>
        where TMessage : class
        where TInput : class
    {
        readonly IInitializerConvention[] _conventions;

        public MessageInitializerFactory(IInitializerConvention[] conventions)
        {
            _conventions = conventions;
        }

        public IMessageInitializer<TMessage> CreateMessageInitializer()
        {
            var builder = new MessageInitializerBuilder<TMessage, TInput>();

            foreach (IPropertyInitializerInspector<TMessage, TInput> inspector in CreatePropertyInspectors())
            {
                foreach (var convention in _conventions)
                {
                    if (inspector.Apply(builder, convention))
                        break;
                }
            }

            foreach (IHeaderInitializerInspector<TMessage, TInput> inspector in CreateHeaderInspectors())
            {
                foreach (var convention in _conventions)
                {
                    if (inspector.Apply(builder, convention))
                        break;
                }
            }

            foreach (IHeaderInitializerInspector<TMessage, TInput> inspector in CreateInputHeaderInspectors())
            {
                foreach (var convention in _conventions)
                {
                    if (inspector.Apply(builder, convention))
                        break;
                }
            }

            return builder.Build();
        }

        static IEnumerable<IPropertyInitializerInspector<TMessage, TInput>> CreatePropertyInspectors()
        {
            return typeof(TMessage).GetAllProperties().Where(x => x.CanRead)
                .Select(x => (IPropertyInitializerInspector<TMessage, TInput>)Activator.CreateInstance(
                    typeof(PropertyInitializerInspector<,,>).MakeGenericType(typeof(TMessage), typeof(TInput), x.PropertyType), x.Name));
        }

        static IEnumerable<IHeaderInitializerInspector<TMessage, TInput>> CreateInputHeaderInspectors()
        {
            return typeof(TInput).GetAllProperties().Where(x => x.CanRead)
                .Select(x => (IHeaderInitializerInspector<TMessage, TInput>)Activator.CreateInstance(
                    typeof(InputHeaderInitializerInspector<,>).MakeGenericType(typeof(TMessage), typeof(TInput)), x.Name));
        }

        static IEnumerable<IHeaderInitializerInspector<TMessage, TInput>> CreateHeaderInspectors()
        {
            yield return CreateHeaderInspector(x => x.SourceAddress);
            yield return CreateHeaderInspector(x => x.DestinationAddress);
            yield return CreateHeaderInspector(x => x.ResponseAddress);
            yield return CreateHeaderInspector(x => x.FaultAddress);

            yield return CreateHeaderInspector(x => x.RequestId);
            yield return CreateHeaderInspector(x => x.MessageId);
            yield return CreateHeaderInspector(x => x.CorrelationId);

            yield return CreateHeaderInspector(x => x.ConversationId);
            yield return CreateHeaderInspector(x => x.InitiatorId);

            yield return CreateHeaderInspector(x => x.ScheduledMessageId);

            yield return CreateHeaderInspector(x => x.TimeToLive);

            yield return CreateHeaderInspector(x => x.Durable);
        }

        static IHeaderInitializerInspector<TMessage, TInput> CreateHeaderInspector(Expression<Func<SendContext, Guid?>> expression)
        {
            return new HeaderInitializerInspector<TMessage, TInput, Guid?>(expression.GetMemberName());
        }

        static IHeaderInitializerInspector<TMessage, TInput> CreateHeaderInspector(Expression<Func<SendContext, TimeSpan?>> expression)
        {
            return new HeaderInitializerInspector<TMessage, TInput, TimeSpan?>(expression.GetMemberName());
        }

        static IHeaderInitializerInspector<TMessage, TInput> CreateHeaderInspector(Expression<Func<SendContext, Uri>> expression)
        {
            return new HeaderInitializerInspector<TMessage, TInput, Uri>(expression.GetMemberName());
        }

        static IHeaderInitializerInspector<TMessage, TInput> CreateHeaderInspector(Expression<Func<SendContext, bool>> expression)
        {
            return new HeaderInitializerInspector<TMessage, TInput, bool>(expression.GetMemberName());
        }
    }
}
