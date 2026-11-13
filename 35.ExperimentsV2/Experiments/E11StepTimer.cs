// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Tokenizer;

namespace ExperimentsV2;

// E11: измеряет время шага обучения зонд-нейросети для трёх представлений.
// Одни и те же файлы токенизируются тремя способами.
// Для каждого: замер времени на шаг + шагов до сходимости.
public sealed class E11StepTimer
{
    private const int WarmupSteps          = 20;
    private const int MeasureSteps         = 200;
    private const int MaxTrainSteps        = 1000;
    private const float ConvergenceThreshold = 0.005f;

    public OperationResult<ExperimentV2Result> Run(
        string combinedSource,
        int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(combinedSource))
            return OperationResult<ExperimentV2Result>.Fail("Нет текста для E11");

        var bpeTokens  = TokenizeBpe(combinedSource, vocabSize: 512);
        var astTokens  = TokenizeAst(combinedSource);
        var charTokens = combinedSource.Select(c => c < 128 ? (int)c : 127).ToList();

        if (bpeTokens.Count < 20 || astTokens.Count < 20 || charTokens.Count < 20)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало токенов");

        var bpeMetrics  = MeasureTraining(bpeTokens,  vocabSize: 512, label: "BPE");
        var astMetrics  = MeasureTraining(astTokens,  vocabSize: 200, label: "AST");
        var charMetrics = MeasureTraining(charTokens, vocabSize: 128, label: "Char");

        double speedupAst  = bpeMetrics.MsPerStep / astMetrics.MsPerStep;
        double speedupChar = bpeMetrics.MsPerStep / charMetrics.MsPerStep;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E11-STEP-TIMER",
            Tier              = tier,
            BaselinePrimary   = bpeMetrics.MsPerStep,
            EnhancedPrimary   = astMetrics.MsPerStep,
            PrimaryMetricName = "ms_per_step",
            LowerIsBetter     = true,
            ScalingSignal     = speedupAst,
            ScalingSignalNote = "speedup_ast_vs_bpe (во сколько раз AST быстрее BPE на шаг)",
            BaselineTimeMs    = bpeMetrics.TotalTrainMs,
            EnhancedTimeMs    = astMetrics.TotalTrainMs,
            Notes             = $"BPE: {bpeMetrics.MsPerStep:F2}ms/step, " +
                                $"stepsToConv={bpeMetrics.StepsToConverge}, " +
                                $"tokens={bpeTokens.Count} | " +
                                $"AST: {astMetrics.MsPerStep:F2}ms/step, " +
                                $"stepsToConv={astMetrics.StepsToConverge}, " +
                                $"tokens={astTokens.Count} | " +
                                $"Char: {charMetrics.MsPerStep:F2}ms/step, " +
                                $"tokens={charTokens.Count} | " +
                                $"speedupAST={speedupAst:F1}x, " +
                                $"speedupChar={speedupChar:F1}x"
        });
    }

    private TrainingMetrics MeasureTraining(
        List<int> tokens, int vocabSize, string label)
    {
        int splitAt = (int)(tokens.Count * 0.8);
        var train   = tokens.Take(splitAt).ToList();
        var val     = tokens.Skip(splitAt).ToList();

        var net = new ProbeNeuralNetwork(vocabSize, hiddenSize: 64, lr: 0.01f);

        for (int i = 0; i < WarmupSteps && i < train.Count - 1; i++)
            net.TrainStep(train[i], train[i + 1]);

        long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int step = 0; step < MeasureSteps; step++)
        {
            int idx = step % (train.Count - 1);
            net.TrainStep(train[idx], train[idx + 1]);
        }
        double measureMs = (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
            / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
        double msPerStep = measureMs / MeasureSteps;

        // Поиск сходимости
        net = new ProbeNeuralNetwork(vocabSize, hiddenSize: 64, lr: 0.01f);
        float prevLoss       = float.MaxValue;
        int stepsToConverge  = MaxTrainSteps;

        long tTrain = System.Diagnostics.Stopwatch.GetTimestamp();
        for (int step = 0; step < MaxTrainSteps; step++)
        {
            int idx = step % (train.Count - 1);
            net.TrainStep(train[idx], train[idx + 1]);

            if (step > 0 && step % 50 == 0)
            {
                double acc          = net.Evaluate(val.Take(200).ToList());
                float currentLoss   = (float)(1.0 - acc);

                if (MathF.Abs(prevLoss - currentLoss) < ConvergenceThreshold)
                {
                    stepsToConverge = step;
                    break;
                }
                prevLoss = currentLoss;
            }
        }
        double totalTrainMs = (System.Diagnostics.Stopwatch.GetTimestamp() - tTrain)
            / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;

        return new TrainingMetrics(msPerStep, stepsToConverge, totalTrainMs);
    }

    private static List<int> TokenizeBpe(string source, int vocabSize)
    {
        var tok = new BpeTokenizer();
        tok.Train(source, vocabSize);
        return tok.Encode(source);
    }

    private static List<int> TokenizeAst(string source)
    {
        var result = E01AstTokenizer.ParseAstTokens(source);
        return result.IsSuccess ? result.Value!.tokens : new List<int>();
    }

    private readonly record struct TrainingMetrics(
        double MsPerStep, int StepsToConverge, double TotalTrainMs);
}
