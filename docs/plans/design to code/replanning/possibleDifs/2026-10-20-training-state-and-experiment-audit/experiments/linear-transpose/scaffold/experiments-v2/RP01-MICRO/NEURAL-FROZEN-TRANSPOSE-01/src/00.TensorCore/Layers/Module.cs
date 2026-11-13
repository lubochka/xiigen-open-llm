// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;

namespace TensorCore.Layers;

// Базовый класс для всех слоёв нейронной сети.
// Аналог torch.nn.Module. Хранит параметры и дочерние слои.
public abstract class Module
{
    private readonly Dictionary<string, Parameter> _parameters = new();
    private readonly Dictionary<string, Module>    _submodules = new();

    // Зарегистрировать обучаемый параметр — вызывается в конструкторе дочернего класса
    protected void RegisterParameter(string name, Parameter parameter)
        => _parameters[name] = parameter;

    // Зарегистрировать дочерний слой (подмодуль)
    protected void RegisterModule(string name, Module module)
        => _submodules[name] = module;

    // Все параметры этого слоя и всех вложенных слоёв рекурсивно.
    // Используется оптимизатором для обновления весов.
    public IEnumerable<Parameter> Parameters()
    {
        foreach (var p in _parameters.Values)
            yield return p;

        foreach (var sub in _submodules.Values)
        foreach (var p in sub.Parameters())
            yield return p;
    }

    // Возвращает параметры вместе с устойчивым путём из имён регистрации,
    // не раскрывая вызывающему внутренние изменяемые словари модулей.
    protected IEnumerable<(string Name, Parameter Parameter)> NamedParameters()
        => NamedParameters(prefix: null);

    private IEnumerable<(string Name, Parameter Parameter)> NamedParameters(string? prefix)
    {
        foreach (var (name, parameter) in _parameters)
        {
            yield return (
                prefix is null ? name : $"{prefix}.{name}",
                parameter);
        }

        foreach (var (name, submodule) in _submodules)
        {
            var childPrefix = prefix is null ? name : $"{prefix}.{name}";
            foreach (var entry in submodule.NamedParameters(childPrefix))
                yield return entry;
        }
    }

    // Обнулить градиенты всех параметров перед новой итерацией
    public void ZeroGrad()
    {
        foreach (var p in Parameters())
            p.ZeroGrad();
    }
}
