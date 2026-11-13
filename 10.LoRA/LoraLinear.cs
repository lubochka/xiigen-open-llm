// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using TensorCore.Randomness;
using TransformerModel.Layers;
using TransformerModel;

namespace LoRA;

// Output = base(x) + scaling * (dropout(x) @ A^T) @ B^T.
// Construction deliberately does not mutate or own the supplied base projection.
public class LoraLinear : Module, IAttentionProjectionAdapter
{
    private readonly Linear _base;
    private readonly Parameter _loraA;
    private readonly Parameter _loraB;
    private readonly Dropout _dropout;

    public Linear BaseProjection => _base;
    public Module RegistrationModule => this;
    public int Rank { get; }
    public float Alpha { get; }
    public float Scaling { get; }
    public float DropoutProbability { get; }
    public LoraAdapterRandomSeeds? RandomSeeds { get; }
    public string DropoutStreamDomain { get; }

    public IReadOnlyList<(string Suffix, Parameter Parameter)> AdapterParameters =>
    [
        ("lora_a", _loraA),
        ("lora_b", _loraB)
    ];

    public LoraLinear(Linear baseLinear, LoraConfig config)
    {
        ArgumentNullException.ThrowIfNull(baseLinear);
        ArgumentNullException.ThrowIfNull(config);

        _base = baseLinear;
        Rank = config.Rank;
        Alpha = config.Alpha;
        Scaling = config.Scaling;
        DropoutProbability = config.Dropout;
        RandomSeeds = null;
        DropoutStreamDomain = "";
        _dropout = new Dropout(config.Dropout);

        var aData = Initializers.KaimingUniform(config.Rank, baseLinear.InFeatures);
        _loraA = new Parameter(
            new Tensor(aData, new TensorShape(config.Rank, baseLinear.InFeatures)));

        var bData = Initializers.Zeros(baseLinear.OutFeatures, config.Rank);
        _loraB = new Parameter(
            new Tensor(bData, new TensorShape(baseLinear.OutFeatures, config.Rank)));

        RegisterParameter("lora_a", _loraA);
        RegisterParameter("lora_b", _loraB);
    }

    public LoraLinear(
        Linear baseLinear,
        LoraConfig config,
        LoraAdapterRandomSeeds randomSeeds)
        : this(baseLinear, config, randomSeeds, "")
    {
    }

    public LoraLinear(
        Linear baseLinear,
        LoraConfig config,
        LoraAdapterRandomSeeds randomSeeds,
        string dropoutStreamDomain)
    {
        ArgumentNullException.ThrowIfNull(baseLinear);
        ArgumentNullException.ThrowIfNull(config);

        _base = baseLinear;
        Rank = config.Rank;
        Alpha = config.Alpha;
        Scaling = config.Scaling;
        DropoutProbability = config.Dropout;
        RandomSeeds = randomSeeds;
        DropoutStreamDomain = dropoutStreamDomain ?? throw new ArgumentNullException(
            nameof(dropoutStreamDomain));
        _dropout = new Dropout(config.Dropout, randomSeeds.DropoutSeed);

        var aData = Initializers.KaimingUniform(
            config.Rank,
            baseLinear.InFeatures,
            randomSeeds.InitializationSeed);
        _loraA = new Parameter(
            new Tensor(aData, new TensorShape(config.Rank, baseLinear.InFeatures)));

        var bData = Initializers.Zeros(baseLinear.OutFeatures, config.Rank);
        _loraB = new Parameter(
            new Tensor(bData, new TensorShape(baseLinear.OutFeatures, config.Rank)));

        RegisterParameter("lora_a", _loraA);
        RegisterParameter("lora_b", _loraB);
    }

    public Tensor Forward(Tensor x) => Forward(x, training: false);

    public Tensor Forward(Tensor x, bool training)
        => Forward(x, training, CancellationToken.None);

    public Tensor Forward(Tensor x, bool training, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var baseOut = _base.Forward(x, cancellationToken);
        var xDrop = _dropout.Forward(x, training);
        cancellationToken.ThrowIfCancellationRequested();
        var aT = TensorOps.Transpose(_loraA.Value, 0, 1, cancellationToken);
        var bT = TensorOps.Transpose(_loraB.Value, 0, 1, cancellationToken);
        var lora = TensorOps.MatMul(
            TensorOps.MatMul(xDrop, aT, cancellationToken),
            bT,
            cancellationToken);
        lora = TensorOps.Scale(lora, Scaling, cancellationToken);
        return TensorOps.Add(baseOut, lora, cancellationToken);
    }

    public TransformerAdapterRandomState CaptureRandomState()
    {
        if (RandomSeeds is not { } seeds && DropoutProbability > 0f)
        {
            throw new InvalidOperationException(
                "DPO_RANDOM_STATE_NONDETERMINISTIC_ADAPTER");
        }

        var state = _dropout.CaptureDeterministicState();
        var effectiveSeeds = RandomSeeds ?? new LoraAdapterRandomSeeds(0, 0);
        return new TransformerAdapterRandomState(
            DropoutStreamDomain,
            effectiveSeeds.InitializationSeed,
            effectiveSeeds.DropoutSeed,
            state.ConsumedCursor);
    }

    public void RestoreRandomState(TransformerAdapterRandomState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (RandomSeeds is not { } seeds && DropoutProbability > 0f)
        {
            throw new InvalidOperationException(
                "DPO_RANDOM_STATE_NONDETERMINISTIC_ADAPTER");
        }
        if (RandomSeeds is { } actual &&
            (actual.InitializationSeed != state.InitializationSeed ||
             actual.DropoutSeed != state.DropoutSeed))
        {
            throw new InvalidOperationException("DPO_RANDOM_STATE_SEED_MISMATCH");
        }
        if (!string.Equals(
                DropoutStreamDomain,
                state.StreamDomain,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("DPO_RANDOM_STATE_DOMAIN_MISMATCH");
        }

        _dropout.RestoreDeterministicState(
            new SplitMix64RandomState(
                state.DropoutSeed,
                state.ConsumedCursor));
    }

    internal bool HasCompatibleConfiguration(LoraConfig config)
        => Rank == config.Rank
           && Alpha.Equals(config.Alpha)
           && Scaling.Equals(config.Scaling)
           && DropoutProbability.Equals(config.Dropout);

    internal void EnableAdapterGradients()
    {
        _loraA.Value.RequiresGrad = true;
        _loraB.Value.RequiresGrad = true;
    }
}
