// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2.Rp01Micro.NKu01; // N-KU-01 experiment-only namespace; product SemanticCompiler code is never edited by this experiment.

using ProductStackLabel = global::SemanticCompiler.StackLabel; // The facade guard compares against the product stack enum, so the port keeps the same type.

// Names the experimental copy of FileActionKnn.cs that an entry is bound to.
public enum NKu01Variant
{
    // Variant A: exact copy of the current product ActionLabelGenerator.Label (VariantA/FileActionKnn.cs).
    A,

    // Variant B: the same copy with DoStatement added to the hasLoop check, the single experimental factor (VariantB/FileActionKnn.cs).
    B
}

// Shared experimental entry for N-KU-01, used identically by both arms.
// It carries over, without logic changes, the row preparation of SemanticCompilerFacade.TrainFileActionKnn
// (12.SemanticCompiler/SemanticCompilerFacade.cs:194-203) and the guard/output of SemanticCompilerFacade.PredictFileAction
// (same file :327-335). Only the bound copy (A or B) differs; full Compile and its neighbour mechanisms are out of scope.
public sealed class NKu01FileActionEntry
{
    // Copy-bound ActionLabelGenerator.FeatureVector: the 30-kind syntax frequency vector the memory stores and queries with.
    private readonly Func<string, float[]> _featureVector;

    // Copy-bound ActionLabelGenerator.Label: the teacher label for each lesson; the A/B factor lives inside it.
    private readonly Func<string, int> _label;

    // Copy-bound FileActionKnnPredictor.Train: the real learner update that replaces the whole memory table.
    private readonly Action<IReadOnlyList<(float[] Features, int Label)>> _train;

    // Copy-bound FileActionKnnPredictor.Predict: the real learner read, a k=3 cosine vote over the memory table.
    private readonly Func<float[], int> _predict;

    // Copy-bound FileActionKnnPredictor.TrainingSize: the guard reads it exactly as the facade does.
    private readonly Func<int> _trainingSize;

    // Copy-bound ActionLabelGenerator.Labels: maps a predicted id to its label text, as the facade returns it.
    private readonly string[] _labels;

    // Facade option UseFileActionKnn; the card fixes it to true in both arms.
    private readonly bool _useFileActionKnn;

    // Private constructor: entries are created only through Create so both arms are wired the same way.
    private NKu01FileActionEntry(
        NKu01Variant variant,
        Func<string, float[]> featureVector,
        Func<string, int> label,
        Action<IReadOnlyList<(float[] Features, int Label)>> train,
        Func<float[], int> predict,
        Func<int> trainingSize,
        string[] labels,
        string labelGeneratorType,
        string predictorType,
        bool useFileActionKnn)
    {
        Variant = variant; // Which copy this entry is bound to.
        _featureVector = featureVector; // Copy's feature extractor.
        _label = label; // Copy's teacher label (the only A/B difference).
        _train = train; // Copy's memory update.
        _predict = predict; // Copy's memory read.
        _trainingSize = trainingSize; // Copy's memory size for the guard.
        _labels = labels; // Copy's label names.
        LabelGeneratorType = labelGeneratorType; // Evidence: full name of the bound label generator type.
        PredictorType = predictorType; // Evidence: full name of the bound predictor type.
        _useFileActionKnn = useFileActionKnn; // Facade option value used by the guard.
    }

    // Which experimental copy this entry is bound to.
    public NKu01Variant Variant { get; }

    // Full type name of the bound label generator, recorded to prove the runner used the copy, not the product type.
    public string LabelGeneratorType { get; }

    // Full type name of the bound predictor, recorded for the same binding proof.
    public string PredictorType { get; }

    // Number of real learner updates issued through this entry (evidence only, not facade logic).
    public int TrainCalls { get; private set; }

    // Number of real learner reads issued through this entry (evidence only, not facade logic).
    public int PredictCalls { get; private set; }

    // Number of queries answered by the guard with null before reaching the learner (evidence only).
    public int GuardNullReturns { get; private set; }

    // Current number of rows in the bound memory table.
    public int TrainingSize => _trainingSize();

    // Evidence only: the bound copy's feature vector for a source, so a reviewer can recompute distances independently.
    // The runner calls it after all measured questions, so it never changes the measured answers or their timing.
    public float[] FeatureVectorForEvidence(string source) => _featureVector(source);

    // Build an entry bound to one experimental copy, with the facade's k=3 predictor (SemanticCompilerFacade.cs:43).
    public static OperationResult<NKu01FileActionEntry> Create(NKu01Variant variant, bool useFileActionKnn = true)
    {
        if (variant == NKu01Variant.A)
        {
            var predictorA = new VariantA.FileActionKnnPredictor(k: 3); // Fresh empty memory of copy A, same k as the facade.
            return OperationResult<NKu01FileActionEntry>.Ok(new NKu01FileActionEntry(
                variant,
                VariantA.ActionLabelGenerator.FeatureVector, // Copy A features (identical code to B).
                VariantA.ActionLabelGenerator.Label, // Copy A label: current product logic.
                predictorA.Train, // Copy A memory update.
                predictorA.Predict, // Copy A memory read.
                () => predictorA.TrainingSize, // Copy A memory size.
                VariantA.ActionLabelGenerator.Labels, // Copy A label names.
                typeof(VariantA.ActionLabelGenerator).FullName!, // Binding proof for the label generator.
                typeof(VariantA.FileActionKnnPredictor).FullName!, // Binding proof for the predictor.
                useFileActionKnn));
        }

        if (variant == NKu01Variant.B)
        {
            var predictorB = new VariantB.FileActionKnnPredictor(k: 3); // Fresh empty memory of copy B, same k as the facade.
            return OperationResult<NKu01FileActionEntry>.Ok(new NKu01FileActionEntry(
                variant,
                VariantB.ActionLabelGenerator.FeatureVector, // Copy B features (identical code to A).
                VariantB.ActionLabelGenerator.Label, // Copy B label: product logic plus DoStatement in hasLoop.
                predictorB.Train, // Copy B memory update.
                predictorB.Predict, // Copy B memory read.
                () => predictorB.TrainingSize, // Copy B memory size.
                VariantB.ActionLabelGenerator.Labels, // Copy B label names.
                typeof(VariantB.ActionLabelGenerator).FullName!, // Binding proof for the label generator.
                typeof(VariantB.FileActionKnnPredictor).FullName!, // Binding proof for the predictor.
                useFileActionKnn));
        }

        return OperationResult<NKu01FileActionEntry>.Fail($"Unknown N-KU-01 variant: {variant}"); // No throw: unknown arm is reported as a failure result.
    }

    // Port of SemanticCompilerFacade.TrainFileActionKnn (:194-203): same filter, same label and features, same >=4 rule.
    // Returns the prepared rows so the runner can record exactly which labels the bound copy taught.
    public IReadOnlyList<(float[] Features, int Label)> TrainFileActionKnn(IReadOnlyList<string> csharpSources)
    {
        // Same row preparation as the facade: skip blank sources, then vectorize and label each source.
        var examples = csharpSources
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(source => (Features: _featureVector(source), Label: _label(source)))
            .ToArray();

        // Same admission rule as the facade: the memory is trained only when at least four rows exist.
        if (examples.Length >= 4)
        {
            _train(examples); // Real learner update: the copy's Train replaces its memory table with these rows.
            TrainCalls++; // Evidence counter for the actual update call.
        }

        return examples; // Evidence: the exact rows (features and teacher labels) handed to the learner.
    }

    // Port of SemanticCompilerFacade.PredictFileAction (:327-335): same guard and same label lookup.
    public string? PredictFileAction(string source, ProductStackLabel stack)
    {
        // Same guard as the facade: option off, a non-C# stack or fewer than four memory rows gives no advice.
        if (!_useFileActionKnn || stack != ProductStackLabel.CSharp || _trainingSize() < 4)
        {
            GuardNullReturns++; // Evidence counter: the learner was not consulted for this query.
            return null; // Same output as the facade: no advice.
        }

        PredictCalls++; // Evidence counter for the actual learner read.
        var label = _predict(_featureVector(source)); // Same read path as the facade: vectorize the query and let the trained copy vote.
        return _labels[label]; // Same output as the facade: the label text.
    }
}
