namespace MassTransit.Initializers.PropertyConverters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;


    public class ListPropertyConverter<TElement> :
        IPropertyConverter<List<TElement>, IEnumerable<TElement>>,
        IPropertyConverter<IList<TElement>, IEnumerable<TElement>>,
        IPropertyConverter<IEnumerable<TElement>, IEnumerable<TElement>>
    {
        public async Task<List<TElement>> Convert<TMessage>(InitializeContext<TMessage> context, IEnumerable<TElement> input)
            where TMessage : class
        {
            return input?.ToList();
        }

        async Task<IList<TElement>> IPropertyConverter<IList<TElement>, IEnumerable<TElement>>.Convert<TMessage>(InitializeContext<TMessage>
            context, IEnumerable<TElement> input)
        {
            return await Convert(context, input).ConfigureAwait(false);
        }

        async Task<IEnumerable<TElement>> IPropertyConverter<IEnumerable<TElement>, IEnumerable<TElement>>.Convert<TMessage>(
            InitializeContext<TMessage> context, IEnumerable<TElement> input)
        {
            return await Convert(context, input).ConfigureAwait(false);
        }
    }


    public class ListPropertyConverter<TElement, TInputElement> :
        IPropertyConverter<List<TElement>, IEnumerable<TInputElement>>,
        IPropertyConverter<IList<TElement>, IEnumerable<TInputElement>>,
        IPropertyConverter<IEnumerable<TElement>, IEnumerable<TInputElement>>
    {
        readonly IPropertyConverter<TElement, TInputElement> _converter;

        public ListPropertyConverter(IPropertyConverter<TElement, TInputElement> converter)
        {
            _converter = converter;
        }

        public async Task<List<TElement>> Convert<TMessage>(InitializeContext<TMessage> context, IEnumerable<TInputElement> input)
            where TMessage : class
        {
            if (input == null)
                return default;

            TElement[] elements = await Task.WhenAll(input.Select(x => _converter.Convert(context, x))).ConfigureAwait(false);

            return elements.ToList();
        }

        async Task<IList<TElement>> IPropertyConverter<IList<TElement>, IEnumerable<TInputElement>>.Convert<TMessage>(InitializeContext<TMessage> context,
            IEnumerable<TInputElement> input)
        {
            return await Convert(context, input).ConfigureAwait(false);
        }

        async Task<IEnumerable<TElement>> IPropertyConverter<IEnumerable<TElement>, IEnumerable<TInputElement>>.Convert<TMessage>(
            InitializeContext<TMessage> context, IEnumerable<TInputElement> input)
        {
            return await Convert(context, input).ConfigureAwait(false);
        }
    }
}
