namespace MassTransit.Definition
{
    using Courier;


    public class DefaultExecuteActivityDefinition<TActivity, TArguments> :
        ExecuteActivityDefinition<TActivity, TArguments>
        where TActivity : class, ExecuteActivity<TArguments>
        where TArguments : class
    {
    }
}
