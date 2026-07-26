// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MachineLearning;

// Q-Learning: tabular reinforcement learning.
// Q(s,a) ← Q(s,a) + learningRate * [reward + discountFactor * max_a'Q(s',a') - Q(s,a)]
// Epsilon-greedy exploration.
public sealed class QLearningAlgorithm
{
    private readonly Dictionary<(int state, int action), double> _qTable = [];
    private int _numActions;

    public OperationResult<bool> Train(
        Func<int, int, (int nextState, double reward, bool done)> environment,
        int   numStates,
        int   numActions,
        int   episodes          = 1000,
        double learningRate     = 0.1,
        double discountFactor   = 0.9,
        double explorationRate  = 0.3)
    {
        if (numStates  <= 0) return OperationResult<bool>.Fail("numStates must be > 0");
        if (numActions <= 0) return OperationResult<bool>.Fail("numActions must be > 0");

        _numActions = numActions;
        var rng     = new Random(42);

        // Initialise Q-table to zero
        for (int s = 0; s < numStates; s++)
            for (int a = 0; a < numActions; a++)
                _qTable[(s, a)] = 0.0;

        for (int episode = 0; episode < episodes; episode++)
        {
            int state = rng.Next(numStates); // random start

            for (int step = 0; step < 200; step++)
            {
                // Epsilon-greedy action selection
                int action = rng.NextDouble() < explorationRate
                    ? rng.Next(numActions)
                    : FindBestAction(state);

                var (nextState, reward, done) = environment(state, action);

                double maxFutureQ = Enumerable.Range(0, numActions)
                    .Max(a => _qTable[(nextState, a)]);

                double oldQ = _qTable[(state, action)];
                _qTable[(state, action)] = oldQ
                    + learningRate * (reward + discountFactor * maxFutureQ - oldQ);

                state = nextState;
                if (done) break;
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<int> BestAction(int state)
    {
        if (_numActions == 0) return OperationResult<int>.Fail("Model not trained");
        return OperationResult<int>.Ok(FindBestAction(state));
    }

    public OperationResult<double> QValue(int state, int action)
        => _qTable.TryGetValue((state, action), out double q)
            ? OperationResult<double>.Ok(q)
            : OperationResult<double>.Fail($"No Q-value for ({state},{action})");

    private int FindBestAction(int state)
    {
        int best = 0;
        double bestQ = double.NegativeInfinity;
        for (int a = 0; a < _numActions; a++)
        {
            double q = _qTable.GetValueOrDefault((state, a));
            if (q > bestQ) { bestQ = q; best = a; }
        }
        return best;
    }
}
