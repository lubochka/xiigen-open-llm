// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace DesignConversionDomain;

public sealed class DesignConversionDomainMarker;

// Относительный прямоугольник одного элемента сцены: то, о чём критик судит.
//
// Зачем он назван отдельной записью. Критик разметки выдавал вердикт, НЕ имея на входе
// геометрии вовсе: в его договоре входа были только итоги проверки качества и ссылки на
// доказательства. Опыт измерил следствие прямо: с подменённым входом критик отвечал одно и то же
// на всех четырёх сценах — ровно порог вырождения, — а с фактическими относительными
// прямоугольниками различал их. То есть решал не критик, а наличие или отсутствие моста между
// сценой и ним.
//
// Почему координаты ОТНОСИТЕЛЬНЫЕ. Относительные доли не зависят от размера полотна, поэтому один
// и тот же разбор годится для разных размеров вывода, и сравнение не ломается от масштаба.
public sealed record DesignConversionRelativeGeometry(
    string ElementId,        // Опознание элемента сцены, к которому относится прямоугольник.
    double RelativeLeft,     // Левый край как доля ширины полотна.
    double RelativeTop,      // Верхний край как доля высоты полотна.
    double RelativeWidth,    // Ширина как доля ширины полотна.
    double RelativeHeight);  // Высота как доля высоты полотна.

public sealed record DesignConversionCriticUnitInput(
    SubmittedDesignConversionRequest Request,
    IReadOnlyList<RenderQaResultRecord> QaResults,
    IReadOnlyList<DesignConversionQaComparisonArtifact> ComparisonArtifacts,
    IReadOnlyList<string> AiTeacherEvidenceRefs,
    IReadOnlyDictionary<string, string> DeterministicEvidenceRefs,
    // SceneGeometry — геометрия сцены, о которой судит критик.
    //
    // Довод необязательный и стоит последним со значением «пусто», поэтому единственное
    // нынешнее построение входа работает без правок, а исторические пакеты без геометрии
    // остаются побайтово прежними: доступ к геометрии обязательным для них не становится.
    IReadOnlyList<DesignConversionRelativeGeometry>? SceneGeometry = null)
{
    [System.Text.Json.Serialization.JsonIgnore]
    internal DesignConversionDpoPolicyRuntimePair? DpoPolicyRuntimePair
    {
        get;
        init;
    }
}

public sealed record DesignConversionCriticUnitOutput(
    string OutputId,
    VisualVerdict Verdict,
    IReadOnlyList<string> QaEvidenceRefs,
    IReadOnlyList<string> AiTeacherEvidenceRefs,
    bool DeterministicLogicClaimedAsLearning);

public sealed record DesignConversionImproverUnitInput(
    SubmittedDesignConversionRequest Request,
    VisualVerdict CriticVerdict,
    DesignConversionImprovementCandidate Candidate,
    IReadOnlyList<string> QaEvidenceRefs,
    IReadOnlyList<string> AiTeacherEvidenceRefs);

public sealed record DesignConversionImproverUnitOutput(
    string OutputId,
    DesignConversionImprovementCandidate Candidate,
    DesignConversionImprovementOutput Output,
    IReadOnlyList<string> QaEvidenceRefs,
    IReadOnlyList<string> AiTeacherEvidenceRefs,
    bool DeterministicLogicClaimedAsLearning);

public interface IDesignConversionCriticUnit
    : ILogicalUnit<DesignConversionCriticUnitInput, DesignConversionCriticUnitOutput>
{
}

public interface IDesignConversionImproverUnit
    : ILogicalUnit<DesignConversionImproverUnitInput, DesignConversionImproverUnitOutput>
{
}
