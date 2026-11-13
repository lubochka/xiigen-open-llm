using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

internal static class Program
{
    private const string CoreCommit = "62bcc8f2368840e22d2ea98ef16d9bd526ad9a39";
    private static readonly (string Prompt, string Answer)[] Train =
        [("C# result sign: 9-4 =", "+"), ("C# result sign: 4-9 =", "-")];
    private static readonly (string Prompt, string Answer)[] Exam =
        [("C# result sign: 8-3 =", "+"), ("C# result sign: 3-8 =", "-")];

    public static int Main(string[] args)
    {
        if (args.Length != 3 || (args[0] != "A" && args[0] != "B") || args[1] != "code-sign")
            throw new ArgumentException("Usage: <A|B> code-sign <new-result.json>");
        var arm = args[0];
        var cfg = new ModelConfig { VocabSize = 256, ContextLen = 64, HiddenDim = 64,
            NumLayers = 1, NumHeads = 4, NumKvHeads = 2, FfnHiddenDim = 176,
            RopeTheta = 10000f, RmsNormEps = 1e-5f };
        LlmModel model;
        using (Initializers.UseDeterministicSeed(62001UL)) model = new LlmModel(cfg);
        LoraApplicator.ApplySeeded(model,
            new LoraConfig { Rank = 4, Alpha = 8f, Dropout = 0f, TargetModules = ["q_proj", "v_proj"] },
            62002UL, "frozen-transpose-probe-v1");
        var trainable = model.Parameters().Where(p => p.Value.RequiresGrad).ToArray();
        var frozen = model.Parameters().Where(p => !p.Value.RequiresGrad).ToArray();
        if (trainable.Length == 0 || frozen.Length == 0) throw new InvalidOperationException("Missing actual trainable/frozen state.");
        var trainer = new Trainer(new TrainingConfig { BatchSize = 1, SeqLen = 64,
            WarmupSteps = 0, TotalSteps = 4, MaxLr = 1e-3f, MinLr = 1e-3f,
            GradClipNorm = 1f, GradAccumSteps = 1, WeightDecay = 0f });
        var optimizer = new AdamW(beta1: 0.9f, beta2: 0.95f, weightDecay: 0f, eps: 1e-8f);
        var sequences = Train.Select(row => Bytes(row.Prompt + row.Answer)).ToArray();
        var frozenBefore = ParametersHash(frozen);
        var trainedBefore = ParametersHash(trainable);
        // A0/B0 use the same path and do not prepopulate B's cache for free.
        Linear.ExperimentReuseFrozenTranspose = false;
        var before = Observe(model);
        Linear.ExperimentReuseFrozenTranspose = arm == "B";
        using var process = Process.GetCurrentProcess();
        var cpu0 = process.TotalProcessorTime;
        var watch = new Stopwatch();
        var losses = new float[4];
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);
        var alloc0 = GC.GetAllocatedBytesForCurrentThread();
        watch.Start();
        for (int step = 0; step < 4; step++)
            losses[step] = trainer.TrainSupervisedNextTokenStep(model, optimizer, sequences[step % 2], weight: 1f);
        watch.Stop();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - alloc0;
        process.Refresh();
        var cpuMs = (process.TotalProcessorTime - cpu0).TotalMilliseconds;
        var gcCounts = new[] { GC.CollectionCount(0)-gen0, GC.CollectionCount(1)-gen1, GC.CollectionCount(2)-gen2 };
        var cacheHits = Linear.ExperimentCacheHits;
        var cacheMisses = Linear.ExperimentCacheMisses;
        var cachedFloats = Linear.ExperimentCachedFloats;
        var peakWorkingSet = process.PeakWorkingSet64;
        var frozenAfter = ParametersHash(frozen);
        var trainedAfter = ParametersHash(trainable);
        var optimizerState = optimizer.CaptureState();
        var optimizerHash = OptimizerHash(optimizerState);
        var after = Observe(model);
        var readout = new
        {
            id = "NEURAL-FROZEN-TRANSPOSE-01", core_commit = CoreCommit, arm, domain = args[1],
            actual_learner = "04.TrainingLoop/Trainer.cs::TrainSupervisedNextTokenStep",
            actual_update_rule = "native SFT next-token CE -> native backward -> native AdamW",
            dpo_steps = 0, optimizer_steps = optimizerState.Step, external_teacher_calls = 0,
            transformer_layers = cfg.NumLayers, trainable_adapter_layers = 2,
            trainable_parameter_arrays = trainable.Length, frozen_parameter_arrays = frozen.Length,
            unit_count = 1, flow_level = "not used by this native learner probe",
            model_config = cfg, base_seed = 62001UL, lora_seed = 62002UL,
            byte_tokenizer = "UTF-8 IDs 0..255; exactly native BPE with zero merges; no EOS added",
            train = Train.Select(x => new { prompt = x.Prompt, answer = x.Answer }),
            exam = Exam.Select(x => new { prompt = x.Prompt, answer = x.Answer }),
            train_order = new[] { 0, 1, 0, 1 }, full_sequence_sft = true,
            frozen_before_sha256 = frozenBefore, frozen_after_sha256 = frozenAfter,
            trainable_before_sha256 = trainedBefore, trainable_after_sha256 = trainedAfter,
            optimizer_after_sha256 = optimizerHash,
            actual_parameter_write = trainedBefore != trainedAfter,
            frozen_state_unchanged = frozenBefore == frozenAfter,
            before, after, training_losses = losses,
            train_cost = new { allocated_bytes = allocated, elapsed_ms = watch.Elapsed.TotalMilliseconds,
                process_cpu_ms = cpuMs, gc_collections = gcCounts },
            process_peak_working_set_bytes = peakWorkingSet,
            mechanism = new { train_cache_hits = cacheHits, train_cache_misses = cacheMisses,
                retained_cache_float_payload_bytes = cachedFloats * sizeof(float) },
            limitations = new[] { "SFT/LoRA probe, not a DPO success claim", "One run per arm; timing may be noisy", "No restore, base mutation, concurrent training, or cross-model cache", "All training prompt targets are intentionally unchanged" }
        };
        using var stream = new FileStream(args[2], FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, readout, new JsonSerializerOptions { WriteIndented = true });
        return 0;
    }

    private static int[] Bytes(string text) => Encoding.UTF8.GetBytes(text).Select(x => (int)x).ToArray();

    private static object[] Observe(LlmModel model)
    {
        return Exam.Select(row =>
        {
            var ids = Bytes(row.Prompt);
            var tensor = new Tensor(ids.Select(i => (float)i).ToArray(), new TensorShape(1, ids.Length));
            var logits = model.Forward(tensor, training: false);
            var offset = (ids.Length - 1) * model.Config.VocabSize;
            var max = double.NegativeInfinity;
            var best = 0;
            for (int i = 0; i < model.Config.VocabSize; i++)
                if (logits.Data[offset+i] > max) { max = logits.Data[offset+i]; best = i; }
            double sum = 0;
            for (int i = 0; i < model.Config.VocabSize; i++) sum += Math.Exp(logits.Data[offset+i] - max);
            int target = Bytes(row.Answer)[0];
            double nll = max + Math.Log(sum) - logits.Data[offset+target];
            var values = logits.Data.AsSpan(offset, model.Config.VocabSize).ToArray();
            return (object)new { prompt = row.Prompt, gold_token_id = target, predicted_token_id = best,
                exact = best == target, target_nll = nll, last_logits = values };
        }).ToArray();
    }

    private static string ParametersHash(IEnumerable<Parameter> parameters)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        foreach (var parameter in parameters)
        {
            writer.Write(parameter.Value.Shape.Rank);
            foreach (var d in parameter.Value.Shape.Dimensions) writer.Write(d);
            foreach (float value in parameter.Value.Data) writer.Write(value);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    private static string OptimizerHash(AdamWStateSnapshot snapshot)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(snapshot.Step);
        foreach (var entry in snapshot.Entries)
        {
            writer.Write(entry.FirstMoment.Length);
            foreach (float value in entry.FirstMoment) writer.Write(value);
            foreach (float value in entry.SecondMoment) writer.Write(value);
        }
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }
}
