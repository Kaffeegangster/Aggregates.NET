﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Aggregates.Contracts;

namespace Aggregates.Extensions
{
    static class ReflectionExtensions
    {
        // some code from https://github.com/mfelicio/NDomain/blob/d30322bc64105ad2e4c961600ae24831f675b0e9/source/NDomain/Helpers/ReflectionUtils.cs

        public static Dictionary<string, Action<TState, object>> GetStateMutators<TState>() where TState : IState
        {
            var methods = typeof(TState)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(
                    m => (m.Name == "Handle" || m.Name == "Conflict") &&
                         m.GetParameters().Length == 1 &&
                         m.ReturnType == typeof(void))
                .ToArray();

            var stateEventMutators = from method in methods
                let eventType = method.GetParameters()[0].ParameterType
                select new
                {
                    Name = eventType.Name,
                    Type = method.Name,
                    Handler = BuildStateEventMutatorHandler<TState>(eventType, method)
                };

            return stateEventMutators.ToDictionary(m => $"{m.Type}.{m.Name}", m => m.Handler);
        }

        private static Action<TState, object> BuildStateEventMutatorHandler<TState>(Type eventType, MethodInfo method)
            where TState : IState
        {
            var stateParam = Expression.Parameter(typeof(TState), "state");
            var eventParam = Expression.Parameter(typeof(object), "ev");

            // state.On((TEvent)ev)
            var methodCallExpr = Expression.Call(stateParam,
                method,
                Expression.Convert(eventParam, eventType));

            var lambda = Expression.Lambda<Action<TState, object>>(methodCallExpr, stateParam, eventParam);
            return lambda.Compile();
        }

        public static Func<TEntity> BuildCreateEntityFunc<TEntity>()
            where TEntity : IEntity
        {

            var ctor = typeof(TEntity).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
            if (ctor == null)
                throw new AggregateException("Entity needs a PRIVATE parameterless constructor");

            var body = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<TEntity>>(body);

            return lambda.Compile();
        }
    }
}