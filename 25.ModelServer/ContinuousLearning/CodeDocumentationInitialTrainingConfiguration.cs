// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace ModelServer; // Keep first-training choices in the trusted server startup layer.

using AgentInfrastructure; // Business refusals remain typed results.
using ContinuousLearning; // Reuse the existing sealed parent loader and explicit optimizer authority.
using LoRA; // Verify the declared adapter topology rather than changing generic defaults.
using TrainableRouter.Training; // Reuse the admitted ordinary generation options and publication input.

internal sealed record CodeDocumentationInitialTrainingInputs( // Carry only first-root inputs verified before baseline or model construction.
    CodeDocumentationRepositoryOwnedModelCheckpointStartRequest StartRequest, // Retain the exact existing generation configuration with cold start denied.
    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization OptimizerAuthorization); // Keep the auditable initial optimizer permission distinct from model initialization.

internal sealed class CodeDocumentationInitialTrainingConfiguration // One captured process-local startup choice, never an HTTP request field.
{ // Disabled startup cannot infer permission from raw marker, root presence or a B0 creation reason.
    internal const string ModeEnvironmentName = "XIIGEN_CODE_DOCUMENTATION_INITIAL_TRAINING_MODE"; // Explicit effect-bearing selector for this initial path only.
    internal const string ReasonEnvironmentName = "XIIGEN_CODE_DOCUMENTATION_INITIAL_TRAINING_REASON"; // Operator supplies an auditable first-update reason.
    internal const string ParentEnvironmentName = "XIIGEN_CODE_DOCUMENTATION_INITIAL_TRAINING_PARENT_SHA256"; // Bind the grant to one exact canonical B0 manifest.
    internal const string FirstRootUpdateMode = "ROOT_B0_FIRST_UPDATE"; // No continuation or arbitrary optimizer reset mode is provided.
    internal const string Required = "CODE_DOCUMENTATION_INITIAL_TRAINING_CHOICE_REQUIRED"; // Absent settings fail before expensive C0.
    internal const string Invalid = "CODE_DOCUMENTATION_INITIAL_TRAINING_CHOICE_INVALID"; // Malformed or mismatched settings never authorize training.
    private readonly string? _mode, _reason, _parentHash; // Capture the complete public selector values at startup.
    private readonly CodeDocumentationGenerationConfigurationResult _generation; // Reuse the existing verified generation owner, not another parser.
    private CodeDocumentationInitialTrainingConfiguration(string? mode, string? reason, string? parentHash, CodeDocumentationGenerationConfigurationResult generation) // Construction stays inside the server configuration owner.
    { _mode = mode; _reason = reason; _parentHash = parentHash; _generation = generation; } // Keep caller wire data out of this snapshot.
    internal static CodeDocumentationInitialTrainingConfiguration Resolve(Func<string, string?> readEnvironment, CodeDocumentationGenerationConfigurationResult generation) // Same trusted startup reader as other server configuration.
        => new(readEnvironment(ModeEnvironmentName), readEnvironment(ReasonEnvironmentName), readEnvironment(ParentEnvironmentName), generation); // Read the explicit choice once per host, not from request JSON.
    internal OperationResult<bool> RequireChoice() // Permit a cheap refusal before services, models or assessment material are needed.
    { // The existing authorization record remains the authority for reason shape.
        if (string.IsNullOrEmpty(_mode) && _reason is null && _parentHash is null) return Refuse<bool>(Required); // Disabled is the default.
        var authorization = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization.Authorize(_reason!); // A reason is never substituted or inferred.
        var start = _generation.Options?.StartRequest; // The ordinary generation reader already checked schema, hash, custody and tokenizer.
        return _mode == FirstRootUpdateMode && authorization.IsWellFormed && // Unknown choices and malformed reasons do not fall back to fresh AdamW.
            _parentHash is { Length: 64 } && _parentHash.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f') && // Require one explicit canonical manifest digest.
            _generation.ErrorCode is null && start?.LoraConfig is { Dropout: 0f } && start.AdapterSeedAuthority is not null && // This bounded initial profile uses its actual zero-dropout configuration, not generic 0.05 defaults.
            start.BaseInitializationSeed == start.AdapterSeedAuthority.RootInitializationSeed && !start.ColdStartPolicy.IsAuthorized // Existing saved B0 is required; this grant cannot create another model.
            ? OperationResult<bool>.Ok(true) : Refuse<bool>(Invalid); // Fail before a baseline if the complete explicit choice is unavailable.
    } // No default permission is produced by this check.
    internal OperationResult<CodeDocumentationInitialTrainingInputs> Bind(ContinuousLearningRepositoryOwnedTrainingExecutionContext context) // Bind the choice to the admitted legal training context and real saved parent.
    { // Read only through existing metadata owners; no LlmModel is created here.
        var choice = RequireChoice(); // Keep missing/malformed refusal identical to the early endpoint check.
        if (!choice.IsSuccess) return choice.MapError<CodeDocumentationInitialTrainingInputs>(); // Preserve the exact reason family.
        var start = _generation.Options!.StartRequest; // This is the existing hash-verified generation/B0 configuration.
        var admitted = context.RepositoryStartRequest; // Server startup retains model identity, current lesson, legal context and training lease ownership.
        if (context.DpoStepCount != 1 || start.RepositoryRoot != admitted.RepositoryRoot || start.DomainSlug != admitted.DomainSlug || // This first-update choice cannot broaden root/domain or optimizer budget.
            start.DesignPatternSlug != admitted.DesignPatternSlug || start.ModelIdSlug != admitted.ModelIdSlug || start.ModelVersion != admitted.ModelVersion || // Preserve the admitted exact model tree.
            start.RuntimeCompatibility != admitted.RuntimeCompatibility || start.ModelConfig != context.ModelConfig || // Generation and training must agree on actual model shape and all runtime fingerprints.
            start.AdapterSeedAuthority!.RootInitializationSeed != context.RootInitializationSeed || // Do not invent a different initialization/dropout stream authority.
            _generation.Options.Tokenizer.ComputeStateSha256() != context.Tokenizer.ComputeStateSha256() || // The retained training tokenizer must be the same admitted artifact.
            LoraRandomStateIdentity.ComputeTopologySha256(start.ModelConfig, start.LoraConfig!, start.AdapterSeedAuthority) != start.RuntimeCompatibility.TopologySha256) // Bind exact rank, alpha, modules, dropout and declared seed streams.
            return Refuse<CodeDocumentationInitialTrainingInputs>(Invalid); // Refuse mismatched model configuration before model construction or C0.
        var loaded = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(start.RepositoryRoot, start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, start.ModelVersion, start.RuntimeCompatibility); // Reuse strict latest-compatible discovery, bytes and canonical manifest validation.
        if (!loaded.IsSuccess || loaded.Value is null) return loaded.MapError<CodeDocumentationInitialTrainingInputs>(); // Missing/corrupt parents remain refusals, never clean initialization.
        using var parent = loaded.Value; // Release the verified stream before entering existing training code.
        if (parent.Manifest.ManifestSha256 != _parentHash || parent.Manifest.Step != 1 || parent.Manifest.ParentCheckpoint is not null || // The explicit permission is valid only for this exact root B0.
            parent.Manifest.DpoApplication != RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate) // An already trained or legacy root cannot acquire this initial optimizer permission.
            return Refuse<CodeDocumentationInitialTrainingInputs>(Invalid); // Continuation stays refused before C0 instead of silently resetting optimizer state.
        return OperationResult<CodeDocumentationInitialTrainingInputs>.Ok(new(start, CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization.Authorize(_reason!))); // Pass the existing auditable authorization unchanged to its existing consumer.
    } // Dropout is zero here, so the existing executor requires absent RNG restore/cold-start grants; seeded adapter authority is preserved.
    internal OperationResult<RepositoryOwnedFullLoraCheckpointDescriptor?> BindCheckpointForStartup() // Only the captured first-root choice may bind the strict saved parent.
    { // Only the captured first-root choice may bind the strict saved parent.
        // Disabled legacy startup retains its adapter-free contract. Partial/unknown choices fail closed.
        if (string.IsNullOrEmpty(_mode) && _reason is null && _parentHash is null) // Only a fully absent selector retains the disabled legacy path.
            return OperationResult<RepositoryOwnedFullLoraCheckpointDescriptor?>.Ok(null); // Absence carries no full-LoRA declaration or optimizer permission.
        var choice = RequireChoice(); // Retain the existing explicit first-root selector and refusal family.
        if (!choice.IsSuccess) return choice.MapError<RepositoryOwnedFullLoraCheckpointDescriptor?>(); // Retain the existing explicit first-root selector and refusal family.
        var start = _generation.Options!.StartRequest; // Read the startup request from the already bound generation configuration.
        if (LoraRandomStateIdentity.ComputeTopologySha256(start.ModelConfig, start.LoraConfig!, start.AdapterSeedAuthority!) // Recompute the topology digest from the generation-owned model, LoRA and seed metadata.
            != start.RuntimeCompatibility.TopologySha256) // Compare the recomputed digest with the admitted runtime topology digest.
            return Refuse<RepositoryOwnedFullLoraCheckpointDescriptor?>(Invalid); // Refuse a topology mismatch before loading a parent or creating a checkpoint descriptor.
        var loaded = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(start.RepositoryRoot, // Ask the existing strict loader for the latest compatible checkpoint in this repository.
            start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, start.ModelVersion, start.RuntimeCompatibility); // Restrict that lookup to the exact domain, pattern, model version and runtime compatibility of the generation request.
        if (!loaded.IsSuccess || loaded.Value is null) return loaded.MapError<RepositoryOwnedFullLoraCheckpointDescriptor?>(); // Propagate the strict loader's refusal without creating a descriptor when no verified parent was returned.
        using var parent = loaded.Value; // Release the strict loader's verified stream after capturing only immutable metadata.
        if (parent.Manifest.ManifestSha256 != _parentHash || parent.Manifest.Step != 1 // Require the explicitly selected canonical parent manifest.
            || parent.Manifest.ParentCheckpoint is not null // Admit only the untrained step-one root, never a descendant or continuation.
            || parent.Manifest.DpoApplication != RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate) // Admit only the untrained step-one root, never a descendant or continuation.
            return Refuse<RepositoryOwnedFullLoraCheckpointDescriptor?>(Invalid); // Refuse a parent that is not the pinned step-one root with no parent and InitialBaselineNoUpdate provenance.
        return OperationResult<RepositoryOwnedFullLoraCheckpointDescriptor?>.Ok( // Return the admitted metadata-only descriptor as a successful startup binding result.
            new RepositoryOwnedFullLoraCheckpointDescriptor( // Capture the verified initial checkpoint metadata in the server-owned full-LoRA descriptor.
                new(start.RepositoryRoot, start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, // Capture the exact repository, domain, pattern and model lane from the generation request.
                    start.ModelVersion, start.RuntimeCompatibility, start.ColdStartPolicy), // Capture its model version, runtime compatibility and cold-start policy without granting continuation authority.
                start.ModelConfig, start.LoraConfig!, start.AdapterSeedAuthority!, parent)); // Pass the admitted model, LoRA and seed metadata together with the verified parent for immutable capture.
    } // Only the captured first-root choice may bind the strict saved parent.
    private static OperationResult<T> Refuse<T>(string code) => OperationResult<T>.Fail("Explicit first-root training choice, matching saved B0 generation configuration and auditable optimizer reason are required.", code); // Never expose private input or silently authorize fallback.
} // The unchanged coordinator and executor independently enforce optimizer and RNG contracts.
