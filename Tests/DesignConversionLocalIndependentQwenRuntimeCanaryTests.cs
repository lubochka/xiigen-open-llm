// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using DesignConversionDomain;

public sealed class DesignConversionLocalIndependentQwenRuntimeCanaryTests
{
    internal const string EnabledEnvironmentName =
        "XIIGEN_RUN_LOCAL_QWEN_JUDGE_CANARY";

    [LocalQwenJudgeCanaryFact]
    public async Task LocalQwenJudge_FourCallsChooseSameQualityAndReverseDirectionOnSwap()
    {
        RequireWindowsRuntime();
        RequireExplicitAcceptance();

        // Canary не угадывает модель по списку Ollama: он использует те же
        // три точных пина, которые обязательны для производственного запуска.
        var modelId = RequiredEnvironment(
            ContinuousLearningModelServerConfigurationLoader
                .ModelIdEnvironmentName);
        var manifestDigest = RequiredEnvironment(
            ContinuousLearningModelServerConfigurationLoader
                .OllamaManifestDigestEnvironmentName);
        var modelLayerDigest = RequiredEnvironment(
            ContinuousLearningModelServerConfigurationLoader
                .OllamaModelLayerDigestEnvironmentName);
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            modelId,
            manifestDigest,
            modelLayerDigest,
            maxTeacherCalls: 4,
            EvidenceRef('a'),
            EvidenceRef('b'),
            Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray());
        Assert.True(configuration.IsSuccess, configuration.ErrorMessage);
        var created = DesignConversionLocalIndependentPreferenceEvaluator.Create(
            configuration.Value!);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        using var evaluator = created.Value!;
        using var totalDeadline = new CancellationTokenSource(TimeSpan.FromMinutes(9));
        var directions = new List<string>();

        for (var index = 0; index < 4; index++)
        {
            var generatedIsGood = index < 2;
            var pair = DesignConversionLocalIndependentPreferenceEvaluatorTests
                .CreatePair(
                    $"real-qwen-canary-{index}",
                    generatedIsGood,
                    neutralizeQualityMetadata: true);
            var observedAtUtc = DateTimeOffset.UtcNow;
            var issued = await evaluator.IssueAsync(
                DesignConversionDpoGenerator.Sha256Ref(
                    $"real-qwen-canary-root-{index}"),
                pair,
                observedAtUtc,
                totalDeadline.Token);

            Assert.True(
                issued.IsSuccess,
                $"Вызов {index + 1} из 4 завершился ошибкой {issued.ErrorCode}: " +
                issued.ErrorMessage);
            var receipt = Assert.IsType<
                DesignConversionIndependentPreferenceEvidence>(issued.Value);
            var expectedDirection = generatedIsGood
                ? DesignConversionIndependentPreferenceEvidenceContract.GeneratedSide
                : DesignConversionIndependentPreferenceEvidenceContract.IncumbentSide;
            Assert.Equal(expectedDirection, receipt.PreferredDirection);
            Assert.Equal(
                LocalQwenOllamaInferenceProfileAuthority.Evaluation.ProfileId,
                receipt.EvaluationProfileId);
            Assert.Equal(
                LocalQwenOllamaInferenceProfileAuthority.Evaluation
                    .ConfigurationSha256,
                receipt.EvaluationProfileConfigurationSha256);
            Assert.True(
                evaluator.TryConsumeEvidence(
                    DesignConversionDpoGenerator.Sha256Ref(
                        $"real-qwen-canary-root-{index}"),
                    pair,
                    DateTimeOffset.UtcNow,
                    out var consumed));
            Assert.Equal(receipt.ReceiptSha256, consumed.ReceiptSha256);
            Assert.False(
                evaluator.TryConsumeEvidence(
                    DesignConversionDpoGenerator.Sha256Ref(
                        $"real-qwen-canary-root-{index}"),
                    pair,
                    DateTimeOffset.UtcNow,
                    out _));
            directions.Add(receipt.PreferredDirection);
        }

        Assert.Equal(
            new[] { "generated", "generated", "incumbent", "incumbent" },
            directions);
        // Тест сознательно не передаёт receipts генератору пар или тренеру:
        // реальный вызов измеряет только независимость решения и one-shot опеку.
    }

    private static string RequiredEnvironment(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Для включённого Qwen canary обязательна производственная переменная {name}.");
        }
        return value;
    }

    private static void RequireWindowsRuntime()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException(
                "Включённый Qwen canary обязан падать вне Windows, а не " +
                "маскироваться runtime skip.");
        }
    }

    private static void RequireExplicitAcceptance()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(EnabledEnvironmentName),
                "1",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Для включённого Qwen canary обязательна явная переменная {EnabledEnvironmentName}=1.");
        }
    }

    private static string EvidenceRef(char value)
        => $"sha256:{new string(value, 64)}";
}

file sealed class LocalQwenJudgeCanaryFactAttribute : FactAttribute
{
    public LocalQwenJudgeCanaryFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Пропуск на discovery-time — античитинговая граница: обычный
            // Linux/WSL прогон видит NotExecuted, а не runtime `$XunitDynamicSkip$`
            // failure. При настоящем Windows opt-in тело теста уже fail-closed
            // проверяет пины модели и качество ответа.
            Skip = "WINDOWS_ONLY_RUNTIME: локальный Qwen/Ollama canary запускается на Windows.";
            return;
        }

        if (!string.Equals(
                Environment.GetEnvironmentVariable(
                    DesignConversionLocalIndependentQwenRuntimeCanaryTests
                        .EnabledEnvironmentName),
                "1",
                StringComparison.Ordinal))
        {
            // Отсутствие opt-in — не зелёный результат и не причина дергать
            // модель: строка остается видимой как NotExecuted до явной приёмки.
            Skip = "OPTIONAL_LOCAL_RUNTIME: задайте " +
                DesignConversionLocalIndependentQwenRuntimeCanaryTests
                    .EnabledEnvironmentName +
                "=1 для четырёх реальных вызовов Qwen.";
        }
    }
}
