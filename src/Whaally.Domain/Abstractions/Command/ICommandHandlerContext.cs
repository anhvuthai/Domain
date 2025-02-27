﻿using FluentResults;
using Whaally.Domain.Abstractions.Aggregate;
using Whaally.Domain.Abstractions.Event;

namespace Whaally.Domain.Abstractions.Command;

public interface ICommandHandlerContext : IContext, IProvideAggregateInstance
{
    /// <summary>
    ///     The optimistic result of this commands evaluation wrapped in message envelopes.
    /// 
    ///     Used for further evaluation in case of successfull command evaluation.
    /// </summary>
    public IReadOnlyCollection<IEventEnvelope> Events { get; }

    /// <summary>
    ///     Stages an event as the optimistic result of this command.
    /// </summary>
    /// <param name="event">The event staged as a result of command evaluation</param>
    public void StageEvent(IEvent @event);

    /// <summary>
    ///     Immediately invokes the provided command in the context of the current commands' execution.
    /// </summary>
    /// <param name="command"></param>
    public void EvaluateCommand(ICommand command);
}

public interface ICommandHandlerContext<TAggregate>
    : ICommandHandlerContext, IProvideAggregateInstance<TAggregate>
    where TAggregate : class, IAggregate
{
    // This method is merely a workaround to deal with IEvent objects.
    // Later on they cannot be dealt with.
    public void StageEvent(Type eventType, IEvent @event) =>
        GetType()
            .GetMethod(nameof(StageEvent))!
            .MakeGenericMethod(eventType)
            .Invoke(this, new object[] { @event });

    void ICommandHandlerContext.StageEvent(IEvent @event) =>
        GetType()
            .GetMethod(nameof(StageEvent))!
            .MakeGenericMethod(@event.GetType())
            .Invoke(this, new[] { @event });

    void ICommandHandlerContext.EvaluateCommand(ICommand command) =>
        GetType()
            .GetMethod(nameof(EvaluateCommand))!
            .MakeGenericMethod(command.GetType())
            .Invoke(this, new[] { command });
    
    public void StageEvent<TEvent>(TEvent @event)
        where TEvent : class, IEvent;

    public IResultBase EvaluateCommand<TCommand>(TCommand command)
        where TCommand : class, ICommand;
}
