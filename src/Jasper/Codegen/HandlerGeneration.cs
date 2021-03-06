﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Codegen
{
    public enum AsyncMode
    {
        ReturnCompletedTask,
        AsyncTask,
        ReturnFromLastNode
    }

    public class HandlerGeneration : Variable
    {
        private readonly GenerationConfig _config;
        private readonly Dictionary<Type, Variable> _variables = new Dictionary<Type, Variable>();
        private readonly IList<Variable> _created = new List<Variable>();

        public HandlerGeneration(HandlerChain chain, GenerationConfig config, string inputArg)
            : base(chain.InputType, inputArg, VariableCreation.Injected)
        {
            _config = config;

            if (chain.All(x => !x.IsAsync))
            {
                AsyncMode = AsyncMode.ReturnCompletedTask;
            }
            else if (chain.Count(x => x.IsAsync) == 1 && chain.Last().IsAsync && chain.Last().CanReturnTask())
            {
                AsyncMode = AsyncMode.ReturnFromLastNode;
            }
        }

        public Variable FindVariable(Type type)
        {
            if (_variables.ContainsKey(type))
            {
                return _variables[type];
            }

            var variable = findVariable(type);
            _variables.Add(type, variable);

            return variable;
        }

        private Variable findVariable(Type type)
        {
            if (type == VariableType) return this;
            foreach (var configSource in _config.Sources)
            {
                Console.WriteLine(configSource);
            }
            var source = _config.Sources.FirstOrDefault(x => x.Matches(type));
            if (source == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type),
                    $"Jasper doesn't know how to build a variable of type '{type.FullName}'");
            }

            return source.Create(type);
        }

        public AsyncMode AsyncMode { get; } = AsyncMode.AsyncTask;

        public InjectedField[] Fields { get; private set; } = new InjectedField[0];

        public void ResolveVariables(HandlerChain chain)
        {
            var frames = chain.ToArray();

            foreach (var frame in frames)
            {
                resolveVariables(frame);
            }

            Fields = GatherAllDependencies(_variables.Values).OfType<InjectedField>().ToArray();
        }

        private void resolveVariables(Frame frame)
        {
            frame.ResolveVariables(this);

            var ordered = GatherAllDependencies(_variables.Values.ToArray())
                .Where(x => x.Creation == VariableCreation.BuiltByFrame && !_created.Contains(x));

            foreach (var variable in ordered)
            {
                _created.Add(variable);
                variable.AttachFrame(frame);
            }
        }
    }
}