// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Text;
using System.Text.Json;
using ContinuousLearning;
using TrainableRouter.Training;

// The trusted root and the intake hash come from the operator, never from the selected source file.
if (!((args.Length == 4 && args[0] is ("inspect" or "author" or "reopen")) || // Preserve all three existing authoring/read commands.
    (args.Length == 6 && args[0] is ("train-existing" or "train-and-hold" or "resume-baseline-and-hold" or "publish-held")))) // Explicit scientific operations use the same real sealed source, endpoint and stable run ID.
{
    Console.Error.WriteLine("Usage: Rp01RawDpoLesson inspect|author|reopen <trusted-repository-root> <absolute-intake-path> <intake-sha256>; train-and-hold|resume-baseline-and-hold|publish-held <same-three-inputs> <modelserver-base-uri> <training-run-id>"); // First-RP01 raw input requires science hold; legacy train-existing is refused by the server.
    return 2;
}
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
var opened = CodeDocumentationRawDpoLessonSource.Open(args[1], args[2], args[3]);
if (!opened.IsSuccess || opened.Value is null) return Refuse(opened.ErrorCode);
var source = opened.Value;
var context = CodeDocumentationRawDpoLessonAuthoring.CreateProductionContext(source);
var request = CodeDocumentationRawDpoLessonAuthoring.PrepareRequest(source, context);
if (!request.IsSuccess || request.Value is null) return Refuse(request.ErrorCode);
if (args[0] == "inspect")
{
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        status = "SOURCE_ADMITTED_NO_MODEL_CALL", source.Identity, requestIdentitySha256 = request.Value.RequestIdentitySha256,
        manifestPath = source.Address.ManifestPath, authoringProfile = context.InferenceProfile,
        teacherCalls = 0, judgeStatus = "NOT_PERFORMED", optimizerSteps = 0
    }));
    return 0;
}
try
{
    if (args[0] is "train-existing" or "train-and-hold" or "resume-baseline-and-hold" or "publish-held") // Compose the judged existing lesson without another authoring or baseline-generation stage.
    { // The running ModelServer must already own the admitted training/scientific startup context.
        if (!Uri.TryCreate(args[4], UriKind.Absolute, out var baseUri)) return Refuse("RAW_LESSON_TRAINING_URI_INVALID"); // Let the existing client enforce loopback safety.
        var composed = CodeDocumentationRawDpoTrainingComposition.CreateRequest(source, context, baseUri, args[5], // Strict Reopen supplies the real manifest receipt and exact pair.
            ["raw-lesson-intake:sha256:" + source.Identity.IntakeSha256], // Attribute only the admitted intake; do not manufacture a judge or selection grant.
            CodeDocumentationRepositoryOwnedDpoTrainingWireContract.MaximumTimeout); // Keep the legacy request representation; explicit scientific methods use caller lifetime and ignore this duration.
        if (!composed.IsSuccess || composed.Value is null) return Refuse(composed.ErrorCode); // Refuse corrupt or changed input before HTTP.
        if (args[0] != "train-existing") // Explicit caller-lifetime methods do not use the legacy request timeout as a deadline.
        {
            var client = new CodeDocumentationScientificTrainingModelServerClient(); // Server owns frozen policy, private capture and independent verdict selectors.
            var result = args[0] == "publish-held" ? await client.PublishHeldForScienceAsync(composed.Value, cancellation.Token) : args[0] == "resume-baseline-and-hold" ? await client.ResumeBaselineAndHoldForScienceAsync(composed.Value, cancellation.Token) : await client.TrainAndHoldForScienceAsync(composed.Value, cancellation.Token); // Publish-only cannot run optimizer or assessments.
            if (!result.IsSuccess || result.Value is null) { Console.Error.WriteLine(JsonSerializer.Serialize(new { status = "SCIENTIFIC_REQUEST_REFUSED_RECONCILE_BEFORE_RETRY", errorCode = result.ErrorCode, optimizerSteps = "UNKNOWN_READ_SERVER_JOURNAL" })); return 1; } // Never retry an unknown effect automatically.
            Console.WriteLine(JsonSerializer.Serialize(result.Value)); // Status/private references only; no full closed inputs or answers.
            return 0; // HELD is a distinct result and makes no learning/publication claim.
        }
        var trained = await new CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient().TrainAndPublishAsync(composed.Value, cancellation.Token); // Invoke the existing endpoint/coordinator under caller cancellation.
        if (!trained.IsSuccess || trained.Value is null) // Downstream refusal may follow an optimizer effect and requires journal reconciliation.
        { // Never label a failed training request as zero optimizer steps or automatically retry it.
            Console.Error.WriteLine(JsonSerializer.Serialize(new { status = "TRAINING_REQUEST_REFUSED_RECONCILE_BEFORE_RETRY", errorCode = trained.ErrorCode, optimizerSteps = "UNKNOWN_READ_SERVER_JOURNAL" })); // Print no closed assessment input or answer.
            return 1; // Preserve an observable nonzero process exit.
        } // Only the existing quality-gated server response can report publication.
        Console.WriteLine(JsonSerializer.Serialize(trained.Value)); // Expose existing bounded response/evidence without inventing scientific metrics.
        return 0; // This branch returns the actual server result, not a routing accuracy claim.
    } // Existing inspect/author/reopen behavior continues below.
    RepositoryOwnedDpoDatasetOpenResult dataset;
    string resolution;
    if (args[0] == "reopen")
    {
        var result = CodeDocumentationRawDpoLessonAuthoring.Reopen(source, context);
        if (!result.IsSuccess || result.Value is null) return Refuse(result.ErrorCode);
        dataset = result.Value; resolution = "STRICT_REOPEN_NO_TEACHER_FACTORY";
    }
    else
    {
        var result = await CodeDocumentationRawDpoLessonAuthoring.ResolveOrAuthorAsync(source, context, cancellation.Token);
        if (!result.IsSuccess || result.Value is null) return Refuse(result.ErrorCode);
        dataset = result.Value.Dataset; resolution = result.Value.Source.ToString();
    }
    var pair = dataset.Pairs.Single();
    // Print hashes and canonical private paths only. The distinct judge reads the exact sealed pair file.
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        status = "CHINESE_PAIR_CANDIDATE_REQUIRES_DISTINCT_JUDGE", resolution,
        manifestPath = source.Address.ManifestPath, dataset.Manifest.ManifestSha256,
        requestIdentitySha256 = dataset.Manifest.RequestIdentity.RequestIdentitySha256,
        pair = dataset.Manifest.OrderedPairs.Single(), promptRawUtf8Sha256 = Hash(pair.Prompt),
        chosenRawUtf8Sha256 = Hash(pair.Chosen), rejectedRawUtf8Sha256 = Hash(pair.Rejected),
        judgeStatus = "NOT_PERFORMED", trainingEligibleByJudge = false, optimizerSteps = 0
    }));
    return 0;
}
catch (OperationCanceledException) // Cancellation after HTTP submission cannot prove zero optimizer effects.
{ // Keep existing author/read cancellation semantics and require reconciliation for training.
    if (args[0] is not ("train-existing" or "train-and-hold" or "resume-baseline-and-hold" or "publish-held")) return Refuse("RAW_LESSON_CALLER_CANCELLED"); // Author/read never enter the optimizer route.
    Console.Error.WriteLine(JsonSerializer.Serialize(new { status = "TRAINING_CANCELLED_RECONCILE_BEFORE_RETRY", errorCode = "RAW_LESSON_CALLER_CANCELLED", optimizerSteps = "UNKNOWN_READ_SERVER_JOURNAL" })); // Do not claim a cancelled training request had no effect.
    return 1; // The operator must read existing attempt/journal evidence before another invocation.
} // No automatic retry or new run ID is created.

static string Hash(string value) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(new UTF8Encoding(false, true).GetBytes(value))).ToLowerInvariant();
static int Refuse(string? code) { Console.Error.WriteLine(JsonSerializer.Serialize(new { status = "REFUSED", errorCode = code, optimizerSteps = 0 })); return 1; }
