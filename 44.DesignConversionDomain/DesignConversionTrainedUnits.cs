// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using System.Globalization;
using System.Text.RegularExpressions;

// Runtime scaffold only: these units preserve Slice 1 contracts and evidence flow.
// They do not contain learned weights, trained visual judgment, or deterministic shortcuts.
public sealed class DesignConversionRuntimeScaffoldCriticUnit : IDesignConversionCriticUnit
{
    public const string UnitId = "design_conversion_runtime_scaffold_critic";
    public const string Role = "critic_runtime_scaffold_input_evidence_only";
    public const string DomainTag = "DesignConversionDomain";
    public const string InputEvidenceOnlyWarning = "QA_DATA_IS_INPUT_EVIDENCE_NOT_HIDDEN_LEARNED_OUTPUT";

    // Отметка «геометрия сцены дошла до критика»: ставится только когда ряды геометрии
    // действительно переданы, поэтому по доказательствам видно, судил критик с геометрией или без.
    public const string SceneGeometrySuppliedEvidence = "DESIGN_CONVERSION_CRITIC_SCENE_GEOMETRY_SUPPLIED";

    // Отметка «список геометрии передан, но он пуст»: отдельная от предыдущей, чтобы пустой
    // список не выдавался за увиденную геометрию.
    public const string SceneGeometrySuppliedWithoutRowsEvidence = "DESIGN_CONVERSION_CRITIC_SCENE_GEOMETRY_SUPPLIED_WITHOUT_ROWS";

    // Начало ссылки со счётом рядов: сколько рядов пригодны из сколько переданных.
    public const string SceneGeometryRowCountEvidencePrefix = "scene-geometry-usable-rows:";

    // Главное честное предупреждение правки: геометрию критик теперь ВИДИТ, но обученным он от
    // этого не стал. По измерению он остаётся честным необученным каркасом, и правка добавляет
    // только путь, по которому сцена до него доходит.
    public const string SceneGeometrySeenButStillScaffoldWarning = "SCENE_GEOMETRY_SEEN_BUT_CRITIC_IS_STILL_UNTRAINED_SCAFFOLD";

    // Предупреждение о непригодных рядах: такой ряд НАЗВАН, а не осуждён молча.
    public const string SceneGeometryUnusableRowWarning = "SCENE_GEOMETRY_ROW_UNUSABLE_NOT_JUDGED";

    // Предупреждение «переданы ряды, но пригодных среди них нет».
    public const string SceneGeometryNoUsableRowWarning = "SCENE_GEOMETRY_SUPPLIED_BUT_NO_USABLE_ROW";

    // Предупреждение «список передан пустым, увидеть было нечего».
    public const string SceneGeometrySuppliedWithoutRowsWarning = "SCENE_GEOMETRY_SUPPLIED_WITHOUT_ROWS_NOTHING_SEEN";

    public Task<OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>> ExecuteAsync(
        LogicalUnitRequest<DesignConversionCriticUnitInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Fail(
                "DesignConversion critic scaffold cancelled.",
                "CANCELLED"));

        if (request is null || request.Input is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Fail(
                "DesignConversion critic input is required.",
                "CRITIC_INPUT_REQUIRED"));

        var input = request.Input;

        // Отдельный геометрический потребитель входа. Он стоит РЯДОМ со сводом итогов проверки
        // качества, а не вместо него: оценку и решение «прошло или не прошло» по-прежнему даёт
        // свод, а геометрия добавляет только доказательства и честные предупреждения.
        var sceneGeometry = ReadSceneGeometry(input);
        var verdict = BuildVerdictFromInputEvidence(input, sceneGeometry);
        var qaEvidenceRefs = BuildQaEvidenceRefs(input);

        var output = new DesignConversionCriticUnitOutput(
            OutputId: $"{UnitId}:{request.RunId}",
            Verdict: verdict,
            QaEvidenceRefs: qaEvidenceRefs,
            AiTeacherEvidenceRefs: input.AiTeacherEvidenceRefs.ToArray(),
            DeterministicLogicClaimedAsLearning: false);

        var result = new LogicalUnitResult<DesignConversionCriticUnitOutput>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: DomainTag,
            Output: output,
            Confidence: verdict.Score,
            PredicateIds: ["design_conversion_critic_scaffold"],
            DomainTags: [DomainTag],
            EvidenceIds: qaEvidenceRefs
                .Concat(input.AiTeacherEvidenceRefs)
                // Ссылки геометрии встают здесь же: когда геометрии нет, набор пуст и состав
                // доказательств остаётся прежним ряд в ряд.
                .Concat(sceneGeometry.EvidenceRefs)
                .Append("DESIGN_CONVERSION_CRITIC_RUNTIME_SCAFFOLD")
                .Append("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            // Предупреждения каркаса остаются первыми и неизменными; предупреждения геометрии
            // дописываются после них и только когда геометрия передана.
            Warnings: new[]
                {
                    "RUNTIME_SCAFFOLD_NOT_LEARNED_CRITIC",
                    InputEvidenceOnlyWarning
                }
                .Concat(sceneGeometry.Warnings)
                .ToArray());

        return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Ok(result));
    }

    // Прежнее правило как ФУНКЦИЯ ВХОДА, а не копия нового кода.
    //
    // До правки состав доказательств критика собирался только из итогов проверки качества и
    // ссылок на доказательства, а геометрию он не видел ни в каком виде. Поэтому эта функция
    // зависит от первого довода (другой набор ссылок — другой ответ) и НЕ зависит от второго:
    // по геометрии прежнее поведение было постоянным, и это и есть измеренный дефект, а не
    // придуманный вход. Приёмочные случаи опираются на обе эти стороны.
    public static IReadOnlyList<string> PriorEvidenceRefsWithoutGeometry(
        IReadOnlyList<string> qaEvidenceRefs,
        IReadOnlyList<DesignConversionRelativeGeometry>? sceneGeometry)
        => (qaEvidenceRefs ?? []).ToArray();

    private static VisualVerdict BuildVerdictFromInputEvidence(
        DesignConversionCriticUnitInput input,
        SceneGeometryReading sceneGeometry)
    {
        var verdicts = input.QaResults
            .Select(result => result.Verdict)
            .Concat(input.ComparisonArtifacts.Select(artifact => artifact.Verdict))
            .ToArray();

        if (verdicts.Length == 0)
        {
            return new VisualVerdict(
                VerdictId: $"{input.Request.RequestId}:critic:no-render-qa-input",
                Score: 0.0,
                Defects: [],
                // Геометрия попадает в доказательства вердикта и на этом пути: если итогов
                // проверки не передали, по вердикту всё равно видно, видел критик сцену или нет.
                EvidenceRefs: BuildQaEvidenceRefs(input)
                    .Concat(sceneGeometry.EvidenceRefs)
                    .Append("NO_RENDER_QA_INPUT_SUPPLIED")
                    .ToArray(),
                Passed: false);
        }

        // Оценка и решение — по-прежнему только свод итогов проверки качества. Геометрия на эти
        // две величины не влияет, поэтому измеренные числа прошлых пакетов не сдвигаются.
        var score = verdicts.Min(verdict => verdict.Score);
        var passed = verdicts.All(verdict => verdict.Passed);
        var defects = verdicts.SelectMany(verdict => verdict.Defects).ToArray();
        var evidenceRefs = verdicts
            .SelectMany(verdict => verdict.EvidenceRefs)
            .Concat(BuildQaEvidenceRefs(input))
            .Concat(sceneGeometry.EvidenceRefs)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new VisualVerdict(
            VerdictId: $"{input.Request.RequestId}:critic:input-evidence-aggregate",
            Score: score,
            Defects: defects,
            EvidenceRefs: evidenceRefs,
            Passed: passed);
    }

    // Чтение геометрии сцены: превращает относительные прямоугольники в ссылки-доказательства
    // и предупреждения. Ничего не решает и ничего не оценивает.
    private static SceneGeometryReading ReadSceneGeometry(DesignConversionCriticUnitInput input)
    {
        var geometry = input.SceneGeometry;

        // Геометрии нет вовсе: не добавляем ни одной ссылки и ни одного предупреждения, поэтому
        // исторический пакет без геометрии даёт в точности прежний ответ.
        if (geometry is null)
            return new SceneGeometryReading(false, [], []);

        // Список передан, но пуст: это НЕ увиденная геометрия, и названо это отдельно.
        if (geometry.Count == 0)
            return new SceneGeometryReading(
                true,
                [SceneGeometrySuppliedWithoutRowsEvidence],
                [SceneGeometrySuppliedWithoutRowsWarning]);

        var evidenceRefs = new List<string>();
        var unusableCount = 0;
        for (var index = 0; index < geometry.Count; index++)
        {
            var row = geometry[index];

            // Непригодный ряд НАЗЫВАЕТСЯ с причиной и местом, а не судится молча.
            var unusableReason = UnusableGeometryReason(row);
            if (unusableReason is not null)
            {
                unusableCount++;
                evidenceRefs.Add(
                    $"scene-geometry-unusable:{index.ToString(CultureInfo.InvariantCulture)}:{unusableReason}");
                continue;
            }

            // Пригодный ряд виден в доказательствах по опознанию элемента и четырём долям.
            evidenceRefs.Add(
                $"scene-geometry:{row.ElementId}=" +
                $"{FormatRelative(row.RelativeLeft)},{FormatRelative(row.RelativeTop)}," +
                $"{FormatRelative(row.RelativeWidth)},{FormatRelative(row.RelativeHeight)}");
        }

        // Счёт пригодных и переданных рядов: по нему видно, сколько геометрии дошло до критика.
        var usableCount = geometry.Count - unusableCount;
        evidenceRefs.Add(
            $"{SceneGeometryRowCountEvidencePrefix}{usableCount.ToString(CultureInfo.InvariantCulture)}/{geometry.Count.ToString(CultureInfo.InvariantCulture)}");
        evidenceRefs.Add(SceneGeometrySuppliedEvidence);

        // Предупреждения: сначала главное — геометрию видно, но критик остался каркасом.
        var warnings = new List<string> { SceneGeometrySeenButStillScaffoldWarning };
        if (unusableCount > 0)
            warnings.Add(SceneGeometryUnusableRowWarning);
        if (usableCount == 0)
            warnings.Add(SceneGeometryNoUsableRowWarning);

        return new SceneGeometryReading(true, evidenceRefs, warnings);
    }

    // Причина непригодности ряда или пустая ссылка, когда ряд пригоден.
    private static string? UnusableGeometryReason(DesignConversionRelativeGeometry? row)
    {
        // Отсутствующий ряд: в списке оказалась пустая ссылка.
        if (row is null)
            return "missing-row";

        // Без опознания элемента прямоугольник не к чему отнести.
        if (string.IsNullOrWhiteSpace(row.ElementId))
            return "empty-element-id";

        // Не-число и бесконечность — это не доли полотна, а отсутствие измерения.
        if (!double.IsFinite(row.RelativeLeft) ||
            !double.IsFinite(row.RelativeTop) ||
            !double.IsFinite(row.RelativeWidth) ||
            !double.IsFinite(row.RelativeHeight))
            return "non-finite-number";

        // Нулевая или отрицательная сторона означает прямоугольник без площади.
        if (row.RelativeWidth <= 0.0 || row.RelativeHeight <= 0.0)
            return "non-positive-size";

        return null;
    }

    // Запись доли в ссылке-доказательстве: один и тот же вид на любой машине и в любой стране.
    private static string FormatRelative(double value)
        => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static string[] BuildQaEvidenceRefs(DesignConversionCriticUnitInput input)
        => input.QaResults
            .Select(result => $"render-qa-result:{result.ResultId}")
            .Concat(input.ComparisonArtifacts.Select(artifact => $"qa-comparison-artifact:{artifact.ArtifactId}"))
            .Concat(input.ComparisonArtifacts.SelectMany(artifact => artifact.EvidenceRefs))
            .Concat(input.DeterministicEvidenceRefs
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"deterministic-input-evidence:{pair.Key}={pair.Value}"))
            .Append("DETERMINISTIC_QA_INPUT_EVIDENCE_ONLY")
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    // Прочитанная геометрия: передана ли она, что из неё попало в доказательства и какие
    // предупреждения она породила. Оценки и решения здесь нет и быть не может.
    private sealed record SceneGeometryReading(
        bool Supplied,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> Warnings);
}

public sealed record DesignConversionLearnedCorrectionPreference(
    string RuleId,
    double Weight,
    int ObservationCount,
    string FeatureKey,
    string Derivation);

// Runtime local improver: creates a traceable visible HTML/CSS candidate from visual defect signals.
// Stored learned weights live outside this unit; the learned wrapper may pass sanitized correction preferences.
public sealed class DesignConversionRuntimeScaffoldImproverUnit : IDesignConversionImproverUnit
{
    public const string UnitId = "design_conversion_runtime_scaffold_improver";
    public const string Role = "improver_runtime_scaffold_candidate_shell";
    public const string DomainTag = DesignConversionRuntimeScaffoldCriticUnit.DomainTag;
    public const string LearnedCorrectionChoiceAppliedEvidence = "DESIGN_CONVERSION_IMPROVER_LEARNED_CORRECTION_CHOICE_APPLIED";
    public const string LearnedCorrectionChoiceFallbackEvidence = "DESIGN_CONVERSION_IMPROVER_LEARNED_CORRECTION_NO_MATCH_FALLBACK";
    private const string LocalImproverClass = "dcm-local-improved";

    public Task<OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>> ExecuteAsync(
        LogicalUnitRequest<DesignConversionImproverUnitInput> request,
        CancellationToken cancellationToken = default)
        => ExecuteCoreAsync(request, [], cancellationToken);

    public Task<OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>> ExecuteWithLearnedCorrectionPreferencesAsync(
        LogicalUnitRequest<DesignConversionImproverUnitInput> request,
        IReadOnlyList<DesignConversionLearnedCorrectionPreference> learnedCorrectionPreferences,
        CancellationToken cancellationToken = default)
        => ExecuteCoreAsync(request, learnedCorrectionPreferences ?? [], cancellationToken);

    private Task<OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>> ExecuteCoreAsync(
        LogicalUnitRequest<DesignConversionImproverUnitInput> request,
        IReadOnlyList<DesignConversionLearnedCorrectionPreference> learnedCorrectionPreferences,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Fail(
                "DesignConversion improver scaffold cancelled.",
                "CANCELLED"));

        if (request is null || request.Input is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Fail(
                "DesignConversion improver input is required.",
                "IMPROVER_INPUT_REQUIRED"));

        var input = request.Input;
        var localPlan = BuildLocalImprovementPlan(input.CriticVerdict.Defects, learnedCorrectionPreferences);
        var selectedRules = localPlan.SelectedRules;
        var learnedSelectionApplied = localPlan.LearnedSelectionApplied;
        var improvedHtml = selectedRules.Count == 0
            ? input.Candidate.Html
            : EnsureLocalImproverClass(input.Candidate.Html);
        var improvedCss = selectedRules.Count == 0
            ? input.Candidate.Css
            : AppendLocalImproverCss(input.Candidate.Css, selectedRules);
        var htmlChanged = !string.Equals(improvedHtml, input.Candidate.Html, StringComparison.Ordinal);
        var cssChanged = !string.Equals(improvedCss, input.Candidate.Css, StringComparison.Ordinal);
        var candidate = input.Candidate with
        {
            Html = improvedHtml,
            Css = improvedCss,
            ChangeReasons = input.Candidate.ChangeReasons
                .Append("Runtime scaffold carries critic verdict forward; no learned visual fix is claimed.")
                .Concat(selectedRules.Select(rule => learnedSelectionApplied
                    ? $"Local learned correction ranking selected {rule.RuleId}: {rule.Reason}."
                    : $"Local deterministic improver applied {rule.RuleId}: {rule.Reason}."))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            EvidenceRefs = input.Candidate.EvidenceRefs
                .Concat(input.QaEvidenceRefs)
                .Concat(input.AiTeacherEvidenceRefs)
                .Append($"critic-verdict:{input.CriticVerdict.VerdictId}")
                .Concat(selectedRules.Select(rule => $"local-improver-rule:{rule.RuleId}"))
                .Concat(learnedSelectionApplied
                    ? selectedRules.Select(rule => $"learned-correction-choice:{rule.RuleId}")
                    : [])
                .Concat(input.CriticVerdict.Defects.Select(defect => $"visual-defect:{defect.DefectId}"))
                .Append(selectedRules.Count == 0
                    ? "DESIGN_CONVERSION_IMPROVER_NO_DEFECTS_NO_LOCAL_CHANGE"
                    : "DESIGN_CONVERSION_IMPROVER_LOCAL_NON_COPY")
                .Concat(learnedSelectionApplied
                    ? [LearnedCorrectionChoiceAppliedEvidence]
                    : localPlan.LearnedPreferencesSupplied ? [LearnedCorrectionChoiceFallbackEvidence] : [])
                .Append("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Metadata = input.Candidate.Metadata
                .Concat(new Dictionary<string, string>
                {
                    ["runtime_scaffold_unit"] = UnitId,
                    ["learned_improvement_claimed"] = bool.FalseString,
                    ["qa_data_role"] = "input_evidence_only",
                    ["local_improver_behavior"] = selectedRules.Count == 0
                        ? "no_defects_no_candidate_change"
                        : learnedSelectionApplied
                            ? "learned_ranked_visible_html_css_candidate"
                            : "defect_driven_visible_html_css_candidate",
                    ["local_improver_signal_count"] = selectedRules.Count.ToString(CultureInfo.InvariantCulture),
                    ["local_improver_available_rule_ids"] = string.Join("|", localPlan.AvailableRules.Select(rule => rule.RuleId)),
                    ["local_improver_selected_rule_ids"] = string.Join("|", selectedRules.Select(rule => rule.RuleId)),
                    ["candidate_html_changed"] = htmlChanged.ToString(),
                    ["candidate_css_changed"] = cssChanged.ToString(),
                    ["learned_correction_choice_applied"] = learnedSelectionApplied.ToString(),
                    ["learned_correction_choice_source"] = learnedSelectionApplied
                        ? "local_checkpoint_sanitized_features"
                        : "none",
                    ["learned_correction_feature_key"] = localPlan.SelectedLearnedPreference?.FeatureKey ?? "none",
                    ["learned_correction_weight"] = localPlan.SelectedLearnedPreference?.Weight.ToString("0.######", CultureInfo.InvariantCulture) ?? "0",
                    ["learned_integration_status"] = learnedSelectionApplied
                        ? "bounded:learned_weights_ranked_local_correction_choice"
                        : "remaining:learned_weights_not_used_for_local_rule_choice"
                })
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal)
        };

        var output = new DesignConversionImprovementOutput(
            OutputId: $"{UnitId}:{request.RunId}",
            CandidateId: candidate.CandidateId,
            Html: candidate.Html,
            Css: candidate.Css,
            Verdict: input.CriticVerdict,
            QaResults: [],
            EvidenceRefs: candidate.EvidenceRefs,
            Accepted: input.CriticVerdict.Passed);

        var unitOutput = new DesignConversionImproverUnitOutput(
            OutputId: output.OutputId,
            Candidate: candidate,
            Output: output,
            QaEvidenceRefs: input.QaEvidenceRefs.ToArray(),
            AiTeacherEvidenceRefs: input.AiTeacherEvidenceRefs.ToArray(),
            DeterministicLogicClaimedAsLearning: false);

        var result = new LogicalUnitResult<DesignConversionImproverUnitOutput>(
            UnitId: UnitId,
            UnitRole: Role,
            DomainTag: DomainTag,
            Output: unitOutput,
            Confidence: input.CriticVerdict.Score,
            PredicateIds: ["design_conversion_improver_scaffold"],
            DomainTags: [DomainTag],
            EvidenceIds: candidate.EvidenceRefs
                .Append("DESIGN_CONVERSION_IMPROVER_RUNTIME_SCAFFOLD")
                .Append(selectedRules.Count == 0
                    ? "DESIGN_CONVERSION_IMPROVER_NO_DEFECTS_NO_LOCAL_CHANGE"
                    : "DESIGN_CONVERSION_IMPROVER_LOCAL_NON_COPY")
                .Concat(learnedSelectionApplied
                    ? [LearnedCorrectionChoiceAppliedEvidence]
                    : localPlan.LearnedPreferencesSupplied ? [LearnedCorrectionChoiceFallbackEvidence] : [])
                .Append("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE")
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Warnings:
            [
                "RUNTIME_SCAFFOLD_NOT_LEARNED_IMPROVER",
                DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning
            ]);

        return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Ok(result));
    }

    public static IReadOnlyList<string> FindLocalCorrectionRuleIds(IReadOnlyList<VisualDefect> defects)
        => BuildLocalImprovementRules(defects)
            .Select(rule => rule.RuleId)
            .ToArray();

    public static string NormalizeCorrectionRuleId(string signal)
    {
        if (ContainsAny(signal, "contrast", "color", "foreground", "background"))
            return "contrast";
        if (ContainsAny(signal, "spacing", "gap", "padding", "margin", "crowded"))
            return "spacing";
        if (ContainsAny(signal, "overflow", "clip", "clipping", "wrap", "truncat"))
            return "overflow";
        if (ContainsAny(signal, "font", "text", "typography", "readability"))
            return "typography";

        var sanitized = SanitizeRuleToken(signal);
        return string.IsNullOrWhiteSpace(sanitized) ? "generic-visual-defect" : sanitized;
    }

    private static LocalImprovementPlan BuildLocalImprovementPlan(
        IReadOnlyList<VisualDefect> defects,
        IReadOnlyList<DesignConversionLearnedCorrectionPreference> learnedCorrectionPreferences)
    {
        var availableRules = BuildLocalImprovementRules(defects);
        if (availableRules.Length == 0)
        {
            return new LocalImprovementPlan(
                AvailableRules: [],
                SelectedRules: [],
                LearnedPreferencesSupplied: learnedCorrectionPreferences.Count > 0,
                LearnedSelectionApplied: false,
                SelectedLearnedPreference: null);
        }

        var bestPreferences = learnedCorrectionPreferences
            .Where(IsValidLearnedPreference)
            .GroupBy(preference => preference.RuleId, StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(preference => preference.Weight)
                .ThenByDescending(preference => preference.ObservationCount)
                .ThenBy(preference => preference.FeatureKey, StringComparer.Ordinal)
                .First())
            .ToDictionary(preference => preference.RuleId, StringComparer.Ordinal);

        var ranked = availableRules
            .Select(rule => new
            {
                Rule = rule,
                Preference = bestPreferences.TryGetValue(rule.RuleId, out var preference)
                    ? preference
                    : null
            })
            .Where(item => item.Preference is not null && item.Preference.Weight > 0)
            .OrderByDescending(item => item.Preference!.Weight)
            .ThenByDescending(item => item.Preference!.ObservationCount)
            .ThenBy(item => item.Rule.RuleId, StringComparer.Ordinal)
            .ToArray();

        if (ranked.Length == 0)
        {
            return new LocalImprovementPlan(
                AvailableRules: availableRules,
                SelectedRules: availableRules,
                LearnedPreferencesSupplied: bestPreferences.Count > 0,
                LearnedSelectionApplied: false,
                SelectedLearnedPreference: null);
        }

        return new LocalImprovementPlan(
            AvailableRules: availableRules,
            SelectedRules: [ranked[0].Rule],
            LearnedPreferencesSupplied: true,
            LearnedSelectionApplied: true,
            SelectedLearnedPreference: ranked[0].Preference);
    }

    private static LocalImprovementRule[] BuildLocalImprovementRules(IReadOnlyList<VisualDefect> defects)
    {
        if (defects is null || defects.Count == 0)
            return [];

        var rules = new Dictionary<string, LocalImprovementRule>(StringComparer.Ordinal);
        foreach (var defect in defects)
        {
            var signal = $"{defect.Category} {defect.Severity} {defect.Description}";
            if (ContainsAny(signal, "contrast", "color", "foreground", "background"))
            {
                rules.TryAdd("contrast", new LocalImprovementRule(
                    "contrast",
                    "color:#111827;background-color:#ffffff;",
                    "raise text/background contrast from visual QA defect signal"));
            }

            if (ContainsAny(signal, "spacing", "gap", "padding", "margin", "crowded"))
            {
                rules.TryAdd("spacing", new LocalImprovementRule(
                    "spacing",
                    "gap:12px;padding:16px;",
                    "increase spacing and padding from visual QA defect signal"));
            }

            if (ContainsAny(signal, "overflow", "clip", "clipping", "wrap", "truncat"))
            {
                rules.TryAdd("overflow", new LocalImprovementRule(
                    "overflow",
                    "max-width:100%;overflow-wrap:anywhere;box-sizing:border-box;",
                    "reduce overflow and wrapping defects from visual QA signal"));
            }

            if (ContainsAny(signal, "font", "text", "typography", "readability"))
            {
                rules.TryAdd("typography", new LocalImprovementRule(
                    "typography",
                    "font-size:16px;line-height:1.5;",
                    "improve text readability from visual QA defect signal"));
            }
        }

        if (rules.Count == 0)
        {
            rules.TryAdd("generic-visual-defect", new LocalImprovementRule(
                "generic-visual-defect",
                "min-height:44px;",
                "make a bounded visible candidate change for an uncategorized visual defect"));
        }

        return rules.Values.OrderBy(rule => rule.RuleId, StringComparer.Ordinal).ToArray();
    }

    private static bool ContainsAny(string signal, params string[] tokens)
        => !string.IsNullOrWhiteSpace(signal) &&
           tokens.Any(token => signal.Contains(token, StringComparison.OrdinalIgnoreCase));

    private static bool IsValidLearnedPreference(DesignConversionLearnedCorrectionPreference preference)
        => preference is not null &&
           !string.IsNullOrWhiteSpace(preference.RuleId) &&
           double.IsFinite(preference.Weight) &&
           preference.ObservationCount > 0 &&
           !string.IsNullOrWhiteSpace(preference.FeatureKey);

    private static string SanitizeRuleToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsAsciiLetterOrDigit(ch) ? ch : '_')
            .ToArray();
        return new string(chars).Trim('_');
    }

    private static string EnsureLocalImproverClass(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return $"<div class=\"{LocalImproverClass}\"></div>";

        if (html.Contains(LocalImproverClass, StringComparison.Ordinal))
            return html;

        const string classPattern = "(<[^>\\s]+[^>]*\\bclass\\s*=\\s*\")([^\"]*)(\")";
        if (Regex.IsMatch(html, classPattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)))
        {
            return Regex.Replace(
                html,
                classPattern,
                match => $"{match.Groups[1].Value}{match.Groups[2].Value} {LocalImproverClass}{match.Groups[3].Value}",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(100));
        }

        const string tagPattern = "(<[a-zA-Z][a-zA-Z0-9:-]*)(\\s|>)";
        return Regex.Replace(
            html,
            tagPattern,
            match => string.Equals(match.Groups[2].Value, ">", StringComparison.Ordinal)
                ? $"{match.Groups[1].Value} class=\"{LocalImproverClass}\">"
                : $"{match.Groups[1].Value} class=\"{LocalImproverClass}\"{match.Groups[2].Value}",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100));
    }

    private static string AppendLocalImproverCss(string css, IReadOnlyList<LocalImprovementRule> rules)
    {
        var currentCss = css ?? string.Empty;
        var declarations = string.Concat(rules.Select(rule => rule.CssDeclarations));
        return $"{currentCss}{Environment.NewLine}.{LocalImproverClass}{{{declarations}}}{Environment.NewLine}";
    }

    private sealed record LocalImprovementRule(string RuleId, string CssDeclarations, string Reason);

    private sealed record LocalImprovementPlan(
        IReadOnlyList<LocalImprovementRule> AvailableRules,
        IReadOnlyList<LocalImprovementRule> SelectedRules,
        bool LearnedPreferencesSupplied,
        bool LearnedSelectionApplied,
        DesignConversionLearnedCorrectionPreference? SelectedLearnedPreference);
}
