using TensorCore.Core;
using DPO.Models;
using TrainingLoop.Optimizers;
using TransformerModel;

namespace DPO;

// Direct Preference Optimisation тренер.
// Loss = -E[log σ(β · (log π(y_w|x) - log π(y_l|x) - log π_ref(y_w|x) + log π_ref(y_l|x)))]
// DNA rule 8: метрики сохраняются ДО инкремента счётчика шага.
public class DpoTrainer
{
    private const float Beta = 0.1f;  // коэффициент KL-штрафа

    private readonly LlmModel _policy;
    private readonly LlmModel _reference;
    private readonly AdamW    _optimizer;
    private readonly string   _checkpointDir;

    public DpoTrainer(LlmModel policy, LlmModel reference, string checkpointDir)
    {
        _policy        = policy;
        _reference     = reference;
        _optimizer     = new AdamW(weightDecay: 0.01f);
        _checkpointDir = checkpointDir;
        Directory.CreateDirectory(checkpointDir);
    }

    public DpoMetrics Step(DpoBatch batch, int step)
    {
        // Reference log-probs: модель заморожена, градиент не нужен
        float refChosenLogP   = LogProbs(_reference, batch.ChosenIds,   batch.ChosenMask);
        float refRejectedLogP = LogProbs(_reference, batch.RejectedIds, batch.RejectedMask);

        // Policy log-probs: обучаемая модель, нужен backward
        float polChosenLogP   = LogProbs(_policy, batch.ChosenIds,   batch.ChosenMask);
        float polRejectedLogP = LogProbs(_policy, batch.RejectedIds, batch.RejectedMask);

        // DPO margin: насколько политика предпочитает chosen над rejected относительно reference
        float chosenReward   = Beta * (polChosenLogP   - refChosenLogP);
        float rejectedReward = Beta * (polRejectedLogP - refRejectedLogP);
        float margin         = chosenReward - rejectedReward;

        // DPO loss: -log σ(margin)
        float loss = -MathF.Log(Sigmoid(margin) + 1e-8f);

        // Ручной backward: d(-log σ(m))/dm = -(1 - σ(m)) = σ(m) - 1
        float gradMargin = Sigmoid(margin) - 1f;
        BackwardLogProbs(_policy, batch.ChosenIds, batch.ChosenMask,  gradMargin * Beta);
        BackwardLogProbs(_policy, batch.RejectedIds, batch.RejectedMask, -gradMargin * Beta);

        _optimizer.Step(_policy.Parameters(), lr: 1e-5f);
        _policy.ZeroGrad();

        var metrics = new DpoMetrics
        {
            Step           = step,
            Loss           = loss,
            ChosenReward   = chosenReward,
            RejectedReward = rejectedReward,
            RewardMargin   = margin,
            Accuracy       = margin > 0 ? 1f : 0f
        };

        // DNA rule 8: сохраняем метрики перед возвратом (Step не увеличиваем здесь)
        SaveMetrics(metrics);
        return metrics;
    }

    // Средний log-prob модели по невыровненным позициям
    private static float LogProbs(LlmModel model, Tensor ids, Tensor mask)
    {
        var logits = model.Forward(ids);    // [1, T, vocab]
        int t      = logits.Shape.Dimensions[1];
        int v      = logits.Shape.Dimensions[2];

        float sum  = 0f;
        float cnt  = 0f;

        // Сдвиг на 1: предсказываем ids[t+1] по контексту до t
        for (int i = 0; i < t - 1; i++)
        {
            float maskVal = mask.Data[i + 1];
            if (maskVal == 0f) continue;

            int targetId = (int)ids.Data[i + 1];

            // LogSoftmax строки i
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
                if (logits.Data[i * v + j] > maxVal)
                    maxVal = logits.Data[i * v + j];

            float logSumExp = 0f;
            for (int j = 0; j < v; j++)
                logSumExp += MathF.Exp(logits.Data[i * v + j] - maxVal);
            logSumExp = MathF.Log(logSumExp);

            float logP = logits.Data[i * v + targetId] - maxVal - logSumExp;
            sum += logP * maskVal;
            cnt += maskVal;
        }

        return cnt > 0f ? sum / cnt : 0f;
    }

    // Упрощённый backward для log-probs (аккумулируем градиент в logits)
    private static void BackwardLogProbs(LlmModel model, Tensor ids, Tensor mask, float upstream)
    {
        var logits = model.Forward(ids);
        int t      = logits.Shape.Dimensions[1];
        int v      = logits.Shape.Dimensions[2];

        if (logits.Grad == null)
            logits.Grad = TensorOps.ZerosLike(logits);

        float cnt = 0f;
        for (int i = 0; i < t - 1; i++)
            if (mask.Data[i + 1] > 0f) cnt++;
        if (cnt == 0f) return;

        float scale = upstream / cnt;

        for (int i = 0; i < t - 1; i++)
        {
            if (mask.Data[i + 1] == 0f) continue;

            int targetId = (int)ids.Data[i + 1];

            // Softmax строки i
            float maxVal = float.NegativeInfinity;
            for (int j = 0; j < v; j++)
                if (logits.Data[i * v + j] > maxVal) maxVal = logits.Data[i * v + j];

            float expSum = 0f;
            var expVals  = new float[v];
            for (int j = 0; j < v; j++)
            {
                expVals[j] = MathF.Exp(logits.Data[i * v + j] - maxVal);
                expSum    += expVals[j];
            }

            // grad_logits[i,j] = (softmax[j] - one_hot[j]) * scale
            for (int j = 0; j < v; j++)
            {
                float sm = expVals[j] / expSum;
                float oh = j == targetId ? 1f : 0f;
                logits.Grad.Data[i * v + j] += (sm - oh) * scale;
            }
        }
    }

    private static float Sigmoid(float x) => 1f / (1f + MathF.Exp(-x));

    private void SaveMetrics(DpoMetrics m)
    {
        string path = Path.Combine(_checkpointDir, $"dpo_step_{m.Step:D6}.json");
        string tmp  = path + ".tmp";
        File.WriteAllText(tmp, System.Text.Json.JsonSerializer.Serialize(m));
        File.Move(tmp, path, overwrite: true);
    }
}
