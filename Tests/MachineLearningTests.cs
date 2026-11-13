// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using MachineLearning;

namespace LlmFromScratch.Tests;

// Unit + integration tests for sub-project 14 (MachineLearning) — algorithms 14-20.
// All implementations are internal pure-C#. No external parity.
public class MachineLearningTests
{
    private static (InMemoryModelStateStorage storage, InMemoryPredictionLogger logger)
        MakeDeps() => (new InMemoryModelStateStorage(), new InMemoryPredictionLogger());

    // ──────────────────────────────────────────────────────────────────────────
    // OperationResult
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void OperationResult_Ok_HasValue()
    {
        var r = OperationResult<int>.Ok(7);
        Assert.True(r.IsSuccess);
        Assert.Equal(7, r.Value);
    }

    [Fact]
    public void OperationResult_Fail_HasError()
    {
        var r = OperationResult<int>.Fail("bad", "BAD");
        Assert.False(r.IsSuccess);
        Assert.Equal("bad", r.ErrorMessage);
        Assert.Equal("BAD", r.ErrorCode);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LinearRegression (sub-project 14)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void LinearRegression_Fit_RecoversSlopeAndIntercept()
    {
        // y = 2x + 1
        var algo = new LinearRegressionAlgorithm();
        var X    = new List<double> { 1, 2, 3, 4, 5 };
        var Y    = X.Select(x => 2 * x + 1).ToList();
        var r    = algo.Fit(X, Y);
        Assert.True(r.IsSuccess);
        Assert.Equal(2.0, r.Value!["slope"],     3);
        Assert.Equal(1.0, r.Value!["intercept"], 3);
    }

    [Fact]
    public void LinearRegression_Predict_AfterFit()
    {
        var algo = new LinearRegressionAlgorithm();
        algo.Fit([0, 1, 2], [0, 2, 4]);
        var pred = algo.Predict(5);
        Assert.True(pred.IsSuccess);
        Assert.Equal(10.0, pred.Value!, 2);
    }

    [Fact]
    public void LinearRegression_TooFewPoints_ReturnsFail()
    {
        var r = new LinearRegressionAlgorithm().Fit([1.0], [2.0]);
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void LinearRegression_PredictBeforeFit_ReturnsFail()
    {
        var r = new LinearRegressionAlgorithm().Predict(3.0);
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void LinearRegressionService_Idempotency()
    {
        var (s, l) = MakeDeps();
        var svc = new LinearRegressionService(s, l);
        var r1  = svc.Train("op1", [1, 2, 3], [2, 4, 6]);
        var r2  = svc.Train("op1", [1, 2, 3], [2, 4, 6]);
        Assert.True(r1.IsSuccess);
        Assert.True(r2.IsSuccess);
        Assert.Equal(r1.Value!["slope"], r2.Value!["slope"], 6);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // NaiveBayes (sub-project 15)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void NaiveBayes_Classify_PositiveNegative()
    {
        var algo = new NaiveBayesAlgorithm();
        algo.Fit(
            [["good","great","love"], ["bad","terrible","hate"],
             ["amazing","wonderful"], ["awful","horrible"]],
            ["positive","negative","positive","negative"]);

        var pos = algo.Classify(["good","great"]);
        var neg = algo.Classify(["bad","horrible"]);
        Assert.True(pos.IsSuccess);
        Assert.Equal("positive", pos.Value);
        Assert.True(neg.IsSuccess);
        Assert.Equal("negative", neg.Value);
    }

    [Fact]
    public void NaiveBayes_EmptyTrainingSet_ReturnsFail()
    {
        var r = new NaiveBayesAlgorithm().Fit([], []);
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void NaiveBayes_ClassifyBeforeFit_ReturnsFail()
    {
        var r = new NaiveBayesAlgorithm().Classify(["word"]);
        Assert.False(r.IsSuccess);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MultilayerPerceptron (sub-project 16)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void MLP_LearnXOR()
    {
        // XOR problem: MLP must learn non-linear boundary
        var X = new List<double[]> { new double[]{0,0}, new double[]{0,1}, new double[]{1,0}, new double[]{1,1} };
        var Y = new List<double[]> { new double[]{0},  new double[]{1},   new double[]{1},   new double[]{0}   };

        var algo = new MultilayerPerceptronAlgorithm();
        var fit  = algo.Fit(X, Y, hiddenSize: 4, learningRate: 0.5, epochs: 5000);
        Assert.True(fit.IsSuccess);

        // After training the output for [0,0] should be near 0, for [0,1] near 1
        var p00 = algo.Predict([0, 0]);
        var p01 = algo.Predict([0, 1]);
        Assert.True(p00.IsSuccess);
        Assert.True(p01.IsSuccess);
        Assert.True(p00.Value![0] < 0.3, $"XOR[0,0] expected ~0, got {p00.Value![0]:F3}");
        Assert.True(p01.Value![0] > 0.6, $"XOR[0,1] expected ~1, got {p01.Value![0]:F3}");
    }

    [Fact]
    public void MLP_PredictBeforeFit_ReturnsFail()
    {
        var r = new MultilayerPerceptronAlgorithm().Predict([1.0, 0.0]);
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void MLP_EmptyTrainingSet_ReturnsFail()
    {
        var r = new MultilayerPerceptronAlgorithm().Fit([], [], hiddenSize: 4);
        Assert.False(r.IsSuccess);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // KMeans (sub-project 17)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void KMeans_TwoClearClusters()
    {
        // Two well-separated groups: {0,0},{1,0},{0,1} and {10,10},{11,10},{10,11}
        var points = new List<double[]>
        {
            new double[]{0,0},  new double[]{1,0},  new double[]{0,1},
            new double[]{10,10},new double[]{11,10}, new double[]{10,11}
        };
        var algo = new KMeansAlgorithm();
        var r    = algo.Fit(points, k: 2);
        Assert.True(r.IsSuccess);
        int[] labels = r.Value!;

        // All first-3 must have the same label; all last-3 must have the same different label
        Assert.Equal(labels[0], labels[1]);
        Assert.Equal(labels[0], labels[2]);
        Assert.Equal(labels[3], labels[4]);
        Assert.Equal(labels[3], labels[5]);
        Assert.NotEqual(labels[0], labels[3]);
    }

    [Fact]
    public void KMeans_CentroidsCount_MatchesK()
    {
        var points = Enumerable.Range(0, 9)
            .Select(i => new double[] { i, i }).ToList();
        var algo = new KMeansAlgorithm();
        algo.Fit(points, k: 3);
        Assert.Equal(3, algo.Centroids.Count);
    }

    [Fact]
    public void KMeans_InvalidK_ReturnsFail()
    {
        var r = new KMeansAlgorithm().Fit([[1.0, 2.0]], k: 5);
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void KMeans_Predict_ReturnsValidCluster()
    {
        var points = new List<double[]> { new double[]{0,0},new double[]{1,0},new double[]{10,10},new double[]{11,10} };
        var algo   = new KMeansAlgorithm();
        algo.Fit(points, k: 2);
        var r = algo.Predict([0.5, 0.0]);
        Assert.True(r.IsSuccess);
        Assert.InRange(r.Value!, 0, 1);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Apriori (sub-project 18)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Apriori_FindsFrequentItemsets()
    {
        var transactions = new List<List<string>>
        {
            new(){"milk","bread","butter"},
            new(){"milk","bread"},
            new(){"milk","butter"},
            new(){"bread","butter"},
            new(){"milk","bread","butter"}
        };
        var algo = new AprioriAlgorithm();
        var r    = algo.Mine(transactions, minimumSupport: 0.6, minimumConfidence: 0.7);
        Assert.True(r.IsSuccess);
        var (frequent, rules) = r.Value!;
        Assert.True(frequent.Count > 0);
        Assert.Contains(frequent, f => f.Items.Contains("milk"));
    }

    [Fact]
    public void Apriori_GeneratesRules()
    {
        var transactions = new List<List<string>>
        {
            new(){"a","b","c"}, new(){"a","b"}, new(){"a","c"},
            new(){"b","c"},     new(){"a","b","c"}
        };
        var algo = new AprioriAlgorithm();
        var r    = algo.Mine(transactions, minimumSupport: 0.4, minimumConfidence: 0.7);
        Assert.True(r.IsSuccess);
        var (_, rules) = r.Value!;
        Assert.True(rules.Count > 0);
        Assert.All(rules, rule =>
        {
            Assert.True(rule.Support > 0);
            Assert.True(rule.Confidence >= 0.7);
        });
    }

    [Fact]
    public void Apriori_EmptyTransactions_ReturnsFail()
    {
        var r = new AprioriAlgorithm().Mine([], minimumSupport: 0.5);
        Assert.False(r.IsSuccess);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PCA (sub-project 19)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PCA_ReducesDimensionality()
    {
        // 4D data → 2D projection
        var rng  = new Random(42);
        var data = Enumerable.Range(0, 20)
            .Select(_ => new double[]
                { rng.NextDouble(), rng.NextDouble(),
                  rng.NextDouble(), rng.NextDouble() })
            .ToList();

        var algo = new PcaAlgorithm();
        var fit  = algo.Fit(data, numComponents: 2);
        Assert.True(fit.IsSuccess);

        var t = algo.Transform(data[0]);
        Assert.True(t.IsSuccess);
        Assert.Equal(2, t.Value!.Length);
    }

    [Fact]
    public void PCA_ExplainedVarianceRatio_SumsToOne()
    {
        var data = Enumerable.Range(0, 10)
            .Select(i => new double[] { i * 1.0, i * 2.0, i * 0.5 })
            .ToList();
        var algo = new PcaAlgorithm();
        algo.Fit(data, numComponents: 3);
        double total = algo.ExplainedVarianceRatio.Sum();
        Assert.Equal(1.0, total, 3);
    }

    [Fact]
    public void PCA_TooManyComponents_ReturnsFail()
    {
        var data = new List<double[]> { new double[] { 1.0, 2.0 } };
        var r    = new PcaAlgorithm().Fit(data, numComponents: 5);
        Assert.False(r.IsSuccess);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Q-Learning (sub-project 20)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void QLearning_LearnsBestActionInSimpleEnv()
    {
        // Simple 2-state, 2-action env: state 0, action 0 → reward 1 (goal)
        //                                              action 1 → reward -1 (penalty)
        //                               state 1, any action → reward 0, back to state 0
        (int next, double reward, bool done) Env(int state, int action) => state switch
        {
            0 when action == 0 => (1, 1.0, true),
            0                  => (0, -0.5, false),
            _                  => (0, 0.0, false)
        };

        var algo = new QLearningAlgorithm();
        var r    = algo.Train(Env, numStates: 2, numActions: 2,
            episodes: 500, learningRate: 0.5, discountFactor: 0.9, explorationRate: 0.1);
        Assert.True(r.IsSuccess);

        // After training, best action in state 0 should be action 0
        var best = algo.BestAction(0);
        Assert.True(best.IsSuccess);
        Assert.Equal(0, best.Value);
    }

    [Fact]
    public void QLearning_QValue_UpdatedAfterTraining()
    {
        (int next, double reward, bool done) Env(int s, int a) =>
            (0, a == 0 ? 1.0 : -1.0, true);

        var algo = new QLearningAlgorithm();
        algo.Train(Env, numStates: 1, numActions: 2,
            episodes: 200, learningRate: 0.5, explorationRate: 0.0);

        var q0 = algo.QValue(0, 0);
        var q1 = algo.QValue(0, 1);
        Assert.True(q0.IsSuccess);
        Assert.True(q1.IsSuccess);
        Assert.True(q0.Value > q1.Value, $"Q(0,0)={q0.Value:F3} should > Q(0,1)={q1.Value:F3}");
    }

    [Fact]
    public void QLearning_InvalidNumStates_ReturnsFail()
    {
        var r = new QLearningAlgorithm().Train((_, _) => (0, 0, true), numStates: 0, numActions: 2);
        Assert.False(r.IsSuccess);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MachineLearningDispatcher
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Dispatcher_ReturnsService_ForKnownAlgorithm()
    {
        var (s, l) = MakeDeps();
        var disp = new MachineLearningDispatcher(
            new LinearRegressionService(s, l),
            new NaiveBayesService(s, l),
            new MultilayerPerceptronService(s, l),
            new KMeansService(s, l));

        var r = disp.Dispatch("linear_regression", []);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Dispatcher_ReturnsFail_ForUnknownAlgorithm()
    {
        var (s, l) = MakeDeps();
        var disp = new MachineLearningDispatcher(
            new LinearRegressionService(s, l),
            new NaiveBayesService(s, l),
            new MultilayerPerceptronService(s, l),
            new KMeansService(s, l));

        var r = disp.Dispatch("unknown_algo", []);
        Assert.False(r.IsSuccess);
    }

    [Fact]
    public void Dispatcher_ListsAvailableAlgorithms()
    {
        var (s, l) = MakeDeps();
        var disp = new MachineLearningDispatcher(
            new LinearRegressionService(s, l),
            new NaiveBayesService(s, l),
            new MultilayerPerceptronService(s, l),
            new KMeansService(s, l));

        Assert.Contains("linear_regression", disp.AvailableAlgorithms);
        Assert.Contains("naive_bayes",       disp.AvailableAlgorithms);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Fabric: InMemoryModelStateStorage
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void InMemoryStorage_SaveAndLoad_RoundTrips()
    {
        var storage = new InMemoryModelStateStorage();
        var params_ = new Dictionary<string, object> { ["slope"] = 2.5, ["intercept"] = 0.1 };
        var saveR   = storage.Save("test_model", params_);
        Assert.True(saveR.IsSuccess);

        var loadR = storage.Load("test_model");
        Assert.True(loadR.IsSuccess);
        Assert.Equal(2.5, (double)loadR.Value!["slope"], 6);
    }

    [Fact]
    public void InMemoryStorage_LoadMissing_ReturnsFail()
    {
        var r = new InMemoryModelStateStorage().Load("no_such");
        Assert.False(r.IsSuccess);
    }
}
