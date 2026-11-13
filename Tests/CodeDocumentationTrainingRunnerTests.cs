// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using TrainableRouter;
using TrainableRouter.Training;
using Xunit.Abstractions;

public sealed class CodeDocumentationTrainingRunnerTests(ITestOutputHelper output)
{
    [Fact]
    public async Task SelectedTeacherContent_ByteFenceNeverCertifiesTokensOrCreatesUpdateBatch()
    {
        var baselineLength = 0;
        using (var control = CodeDocumentationPreparedBaselineTests.TeacherContextHarness())
        {
            var (b, a) = await CodeDocumentationPreparedBaselineTests.BindTeacherCompleted(control);
            var actual = await CodeDocumentationPreparedBaselineTests.ConsumeTeacherContent(control, b, a,
                assess: CodeDocumentationPreparedBaselineTests.AssessTeacherHistory);
            Assert.True(actual.IsSuccess, actual.ErrorMessage); baselineLength = actual.Value!.CopyTeacherContentUtf8().Length;
            Assert.InRange(baselineLength, 1, 65534);
        }
        foreach (var size in new[] { 65535, 65536, 65537 })
        {
            using var h = CodeDocumentationPreparedBaselineTests.TeacherContextHarness(padding: size - baselineLength);
            var (b, a) = await CodeDocumentationPreparedBaselineTests.BindTeacherCompleted(h); var files = h.Files(); var prefix = h.HistoryBytes();
            var captures = 0; var assessments = 0; var generated = 0;
            var old = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting;
            var hook = h.Open.Session.SelectedStudentDurablePointForTesting;
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = _ => captures++;
                h.Open.Session.SelectedStudentDurablePointForTesting = stage =>
                { if (stage.Contains("generate", StringComparison.Ordinal)) { generated++; throw new InvalidOperationException("No new Generate in Teacher recipient."); } };
                var result = await CodeDocumentationPreparedBaselineTests.ConsumeTeacherContent(h, b, a, assess: (input, token) =>
                {
                    assessments++; Assert.EndsWith(new string('p', size - baselineLength), input.History.Pairs[0].Pair.Chosen);
                    return CodeDocumentationPreparedBaselineTests.AssessTeacherHistory(input, token);
                });
                if (size <= 65536)
                {
                    Assert.True(result.IsSuccess, result.ErrorCode + ":" + result.ErrorMessage);
                    Assert.Equal(size, result.Value!.CopyTeacherContentUtf8().Length); Assert.Equal(1, captures);
                    Assert.Equal("Unavailable", result.Value.TeacherTokenizerAdmission); Assert.False(result.Value.FullTeacherContextReady);
                    Assert.Equal(2, result.Value.History.Pairs.Count);
                    Assert.Empty(typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContent).GetConstructors());
                }
                else { Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherContentTooLargeErrorCode, result.ErrorCode); Assert.Equal(0, captures); }
                Assert.Equal(1, assessments); Assert.Equal(0, generated); Assert.Equal(prefix, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls);
                Assert.Equal(0, h.Open.Session.Snapshot.Effects.TeachingDispatched); Assert.Equal(0, h.Open.Session.Snapshot.Effects.OptimizerEntries);
                Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting); h.AssertFiles(files);
                output.WriteLine($"Teacher content bytes={size}; success={result.IsSuccess}; real tokenizer=Unavailable; new effects=0");
            }
            finally
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = old;
                h.Open.Session.SelectedStudentDurablePointForTesting = hook;
            }
        }
    }



    [Fact]
    public async Task SelectedStudentContext_PreservesMaterialAndRawResponseWithoutPairOrTeacherEffects()
    {
        using var h=CodeDocumentationPreparedBaselineTests.SelectedStudentHarness();var (b,a)=await CodeDocumentationPreparedBaselineTests.BindSelectedStudent(h);
        var captures=new List<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>();
        var old=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=captures.Add;
            var result=await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a);Assert.True(result.IsSuccess,result.ErrorMessage);
            var context=Assert.Single(captures);Assert.Same(context,result.Value);
            Assert.Equal("Teach branch Y",context.Material.OwnerInput);Assert.Equal("b",context.StudentResponse);
            Assert.Equal(Encoding.UTF8.GetBytes("Teach branch Y"),context.Material.CopyOwnerInputUtf8());
            Assert.Equal(Encoding.UTF8.GetBytes(context.Material.PaidModelReferenceAnswer),context.Material.CopyPaidReferenceUtf8());
            Assert.Equal(context.Material.PaidReferenceProvenance.ReferenceAnswerSha256,FrozenReferenceEntry.ComputeNormalizedTextSha256(context.Material.PaidModelReferenceAnswer));
            var raw=context.CopyStudentResponseUtf8();raw[0]^=1;var task=context.Material.CopyOwnerInputUtf8();task[0]^=1;
            Assert.Equal(new byte[]{(byte)'b'},context.CopyStudentResponseUtf8());Assert.Equal(Encoding.UTF8.GetBytes("Teach branch Y"),context.Material.CopyOwnerInputUtf8());
            Assert.Equal("SyntheticTest",context.Origin);Assert.False(context.FullTeacherContextReady);Assert.Equal(10,h.Client.Calls);
            Assert.Equal(0,h.Open.Session.Snapshot.Effects.OptimizerEntries);Assert.Equal(0,h.Open.Session.Snapshot.Effects.TeachingDispatched);
            Assert.Empty(typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext).GetConstructors());
            var issued=typeof(CodeDocumentationOneLessonSession.StudentResponseReadback).GetMethod("InspectAndIssue",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)!;
            var unauthorized=(OperationResult<CodeDocumentationOneLessonSession.StudentResponseReadback>)issued.Invoke(null,new object[]{h.Open.Session})!;
            Assert.False(unauthorized.IsSuccess);Assert.Null(unauthorized.Value); // A durable decoder alone cannot mint an issuer outside the private in-flight Session.
            Assert.DoesNotContain(h.Fixture.EnumerateOwnedOrdinaryFiles(),p=>p.Contains("student-response",StringComparison.Ordinal));

            // Completed replay still enters the real recipient and must discard a context invalidated there.
            foreach(var fault in new[]{"original","unused-source","receiving","throw","cancel"})
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=captures.Add;
                var control=await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a);
                Assert.True(control.IsSuccess,fault+" valid completed recipient control");Assert.Equal(context.CompletedFrameSha256,control.Value!.CompletedFrameSha256);
                var before=h.Files();var history=h.HistoryBytes();Assert.Equal(4,history.Count(x=>x==(byte)'\n'));
                var source=h.Grant.SourceSelection!.TeachingCurriculum!.Sources.Single(s=>s.SourceId=="teach-7");
                Assert.NotEqual(source.SourceId,context.Request.SourceId);
                var address=RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,source.DomainSlug,source.SubjectSlug,source.CorpusVersion).Value!;
                var path=fault=="original"?Path.Combine(h.Fixture.CorpusAddress.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName)
                    :Path.Combine(address.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
                var bytes=File.ReadAllBytes(path);var ticks=File.GetLastWriteTimeUtc(path);var evidence=b.Context.EvidenceRefs;
                var ownerHook=h.Open.Session.SelectionDurablePointForTesting;var studentHook=h.Open.Session.SelectedStudentDurablePointForTesting;
                var ownerChecks=0;var finalHook=0;var generated=0;var recipients=0;var writtenFaults=0;
                using var cancellation=new CancellationTokenSource();var primary=new InvalidOperationException("selected-student-recipient-primary");
                var canceled=new OperationCanceledException("selected-student-recipient-cancel",null,cancellation.Token);Exception? primaryFailure=null;
                try
                {
                    h.Open.Session.SelectionDurablePointForTesting=stage=>
                    {
                        if(stage!="student-final-owner-checked")return;ownerChecks++;
                        if(fault is "throw" or "cancel") Assert.True(h.Inspect(b,CancellationToken.None).IsSuccess,"Final None owner check must remain usable after the recipient primary failure.");
                    };
                    h.Open.Session.SelectedStudentDurablePointForTesting=stage=>
                    {
                        if(stage.Contains("generate",StringComparison.Ordinal)) {generated++;throw new InvalidOperationException("Completed recipient replay must not Generate.");}
                        if(stage=="student-final-owner-checked")finalHook++;
                    };
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=received=>
                    {
                        recipients++;Assert.Equal(context.CompletedFrameSha256,received.CompletedFrameSha256);Assert.Equal("b",received.StudentResponse);
                        if(fault=="throw")throw primary;
                        if(fault=="cancel") {cancellation.Cancel();throw canceled;}
                        if(fault=="receiving")
                        {
                            var field=b.Context.GetType().GetField("<EvidenceRefs>k__BackingField",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
                            Assert.NotNull(field);field.SetValue(b.Context,new[]{"late-selected-student-receiving-context"});
                        }
                        else {CodeDocumentationPreparedBaselineTests.RestoreReferenceOwnerFile(path,bytes.Concat(new byte[]{(byte)' '}).ToArray(),ticks);writtenFaults++;Assert.NotEqual(bytes,File.ReadAllBytes(path));}
                    };
                    if(fault=="throw") Assert.Same(primary,await Assert.ThrowsAsync<InvalidOperationException>(()=>CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a)));
                    else if(fault=="cancel")
                    {
                        var observed=await Assert.ThrowsAsync<OperationCanceledException>(()=>CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a,cancellation.Token));
                        Assert.Same(canceled,observed);Assert.Equal(cancellation.Token,observed.CancellationToken);Assert.True(cancellation.IsCancellationRequested);
                    }
                    else
                    {
                        var refused=await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a);
                        Assert.False(refused.IsSuccess,fault+" must invalidate the already-staged recipient context");Assert.Null(refused.Value);
                        Assert.Equal(fault=="receiving"?CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode:RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode,refused.ErrorCode);
                        output.WriteLine("STUDENT_RECIPIENT_LATE_FAULT fault="+fault+" code="+refused.ErrorCode+" writtenFaults="+writtenFaults);
                    }
                    Assert.Equal(1,recipients);Assert.Equal(1,ownerChecks);Assert.Equal(1,finalHook);Assert.Equal(0,generated);
                    Assert.Equal(fault is "original" or "unused-source"?1:0,writtenFaults);Assert.Equal(history,h.HistoryBytes());
                    Assert.Equal((1,1,false),h.Open.Session.SelectedStudentAccounting);Assert.Equal(10,h.Client.Calls);
                    Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),h.Open.Session.Snapshot.Effects);
                }
                catch(Exception failure){primaryFailure=failure;throw;}
                finally
                {
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=captures.Add;
                    h.Open.Session.SelectionDurablePointForTesting=ownerHook;h.Open.Session.SelectedStudentDurablePointForTesting=studentHook;
                    if(fault=="receiving") b.Context.GetType().GetField("<EvidenceRefs>k__BackingField",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.SetValue(b.Context,evidence);
                    if(fault is "original" or "unused-source")CodeDocumentationPreparedBaselineTests.RestoreFixtureAfterFault(()=>CodeDocumentationPreparedBaselineTests.RestoreReferenceOwnerFile(path,bytes,ticks),primaryFailure);
                }
                h.AssertFiles(before);
                var replayGenerated=0;
                try
                {
                    h.Open.Session.SelectedStudentDurablePointForTesting=stage=>{if(stage.Contains("generate",StringComparison.Ordinal)){replayGenerated++;throw new InvalidOperationException("Restored recipient replay must not Generate.");}};
                    var restored=await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a);
                    Assert.True(restored.IsSuccess,fault+" restored real recipient control");Assert.Equal(context.CompletedFrameSha256,restored.Value!.CompletedFrameSha256);
                    Assert.Equal(context.StudentResponse,restored.Value.StudentResponse);Assert.Equal(0,replayGenerated);Assert.Equal(history,h.HistoryBytes());h.AssertFiles(before);Assert.Equal(10,h.Client.Calls);
                }
                finally{h.Open.Session.SelectedStudentDurablePointForTesting=studentHook;}
            }
            h.Open.Dispose();Assert.False((await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a)).IsSuccess);
        }
        finally{CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=old;}
    }

    [Fact]
    public async Task SelectedMaterialAdmission_UsesExistingPurposeAndGovernanceDecisions()
    {
        using var h = CodeDocumentationPreparedBaselineTests.SelectedMaterialHarness(); var b = (await h.Consume()).Value!;
        var witness = h.Inspect(b); Assert.True(witness.IsSuccess, witness.ErrorMessage); var graph = witness.Value!.TeachingIdentities;
        var diagnosis = h.Classify(b).Value!;
        var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.OriginalReceiptId,
            diagnosis.OriginalReceiptSha256, diagnosis.Categories, graph).Value!;
        var evidence = new[] { "sha256:" + new string('b', 64), "sha256:" + new string('c', 64) };
        var valid = new TrainingDataGovernanceDecision(true, false, evidence); var before = h.Files();
        Assert.True(Capture(valid).IsSuccess, "genuine graph and valid existing decision control");
        foreach (var purpose in new[] { CodeDocumentationRepositoryOwnedDpoMaterialPurpose.CertificationExam,
            CodeDocumentationRepositoryOwnedDpoMaterialPurpose.DiagnosticFeedback, CodeDocumentationRepositoryOwnedDpoMaterialPurpose.Unknown,
            (CodeDocumentationRepositoryOwnedDpoMaterialPurpose)99 })
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthoringSourceNotTrainableErrorCode, Capture(valid, purpose).ErrorCode);
        var invalid = new (TrainingDataGovernanceDecision? Decision, string Code)[] {
            (null, TrainingDataGovernanceAdmission.RequiredErrorCode),
            (new(false, false, evidence), TrainingDataGovernanceAdmission.LicenseDeniedErrorCode),
            (new(false, true, Array.Empty<string>()), TrainingDataGovernanceAdmission.LicenseDeniedErrorCode),
            (new(true, true, Array.Empty<string>()), TrainingDataGovernanceAdmission.QuarantinedErrorCode),
            (new(true, false, Array.Empty<string>()), TrainingDataGovernanceAdmission.EvidenceInvalidErrorCode),
            (new(true, false, new[] { evidence[0], evidence[0] }), TrainingDataGovernanceAdmission.EvidenceInvalidErrorCode),
            (new(true, false, new[] { "sha256:" + new string('B', 64) }), TrainingDataGovernanceAdmission.EvidenceInvalidErrorCode),
            (new(true, false, Enumerable.Range(0, 33).Select(i => "sha256:" + i.ToString("x64")).ToArray()), TrainingDataGovernanceAdmission.EvidenceInvalidErrorCode)
        };
        foreach (var row in invalid)
        {
            var actual = Capture(row.Decision); var existing = TrainingDataGovernanceAdmission.ValidateDecision(row.Decision);
            Assert.Equal(row.Code, actual.ErrorCode); Assert.Equal(existing.ErrorMessage, actual.ErrorMessage);
        }
        var detached = Capture(valid).Value!; evidence[0] = "caller-mutated-reference";
        Assert.Equal("sha256:" + new string('b', 64), detached.Governance.EvidenceRefs[0]);
        var changedOrder = Capture(new(true, false, valid.EvidenceRefs.Reverse().ToArray())).Value!;
        Assert.NotEqual(detached.EvidenceSha256, changedOrder.EvidenceSha256);
        var bytes = input.CopyBytes(); bytes[0] ^= 1;
        var forged = new CodeDocumentationTeachingSelectionInputV1(input.OriginalReceiptId, input.OriginalReceiptSha256, input.SourceSha256, input.Categories.ToArray(), bytes);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(graph, forged,
                witness.Value.BindingSha256, "item-teach-7-1", CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference, valid).ErrorCode);
        var wrongSourceScalar = new CodeDocumentationTeachingSelectionInputV1(input.OriginalReceiptId, input.OriginalReceiptSha256,
            new string('d', 64), input.Categories.ToArray(), input.CopyBytes());
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(graph, wrongSourceScalar,
                witness.Value.BindingSha256, "item-teach-7-1", CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference, valid).ErrorCode);
        Assert.Empty(typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission).GetConstructors());
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(graph, input,
                witness.Value.BindingSha256, "missing-item", CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference, valid).ErrorCode);
        Assert.Equal(10, h.Client.Calls); h.AssertFiles(before); Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), h.Open.Session.Snapshot.Effects);
        OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission> Capture(TrainingDataGovernanceDecision? decision,
            CodeDocumentationRepositoryOwnedDpoMaterialPurpose purpose = CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference)
            => CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(graph, input,
                witness.Value.BindingSha256, "item-teach-7-1", purpose, decision);
    }

    [Fact]
    public async Task SelectedMaterialContext_ConsumesActualTaskAndReferenceOnly()
    {
        using var h = CodeDocumentationPreparedBaselineTests.SelectedMaterialHarness(); var b = (await h.Consume()).Value!;
        var admission = CodeDocumentationPreparedBaselineTests.SelectedMaterialAdmission(h, b);
        await CodeDocumentationPreparedBaselineTests.BindSelectedMaterial(h, b, admission); var before = h.Files();
        var prior = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting; var calls = 0;
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext? captured = null;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = context => { calls++; captured = context; };
            var actual = CodeDocumentationPreparedBaselineTests.ConsumeSelectedMaterial(h, b, admission); Assert.True(actual.IsSuccess, actual.ErrorMessage);
            Assert.Equal(1, calls); Assert.Same(captured, actual.Value); Assert.NotNull(captured);
            Assert.Contains("中文\n", captured!.OwnerInput, StringComparison.Ordinal); Assert.Contains("中文\n", captured.PaidModelReferenceAnswer, StringComparison.Ordinal);
            Assert.Equal(Encoding.UTF8.GetBytes(captured.OwnerInput), captured.CopyOwnerInputUtf8());
            Assert.Equal(Encoding.UTF8.GetBytes(captured.PaidModelReferenceAnswer), captured.CopyPaidReferenceUtf8());
            var task = captured.CopyOwnerInputUtf8(); var reference = captured.CopyPaidReferenceUtf8(); task[0] ^= 1; reference[0] ^= 1;
            Assert.Equal(Encoding.UTF8.GetBytes(captured.OwnerInput), captured.CopyOwnerInputUtf8());
            Assert.Equal(Encoding.UTF8.GetBytes(captured.PaidModelReferenceAnswer), captured.CopyPaidReferenceUtf8());
            Assert.True(captured.PaidReferenceProvenance.ClosedPaidReference); Assert.True(captured.PaidReferenceProvenance.PaidReferenceOutputNeverTrainable);
            Assert.True(captured.PaidReferenceProvenance.PaidReferenceOutputNeverDpoPairSide);
            Assert.Equal(FrozenReferenceEntry.ComputeNormalizedTextSha256(captured.PaidModelReferenceAnswer), captured.PaidReferenceProvenance.ReferenceAnswerSha256);
            Assert.Equal("item-teach-7-1", captured.Item.ItemId); Assert.Equal("SyntheticTest", captured.Origin);
            Assert.False(typeof(DpoPair).IsAssignableFrom(captured.GetType()));
            Assert.Null(captured.GetType().GetProperty("ReferenceRows")); Assert.Null(captured.GetType().GetProperty("StudentAnswer"));
        }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = prior; }
        h.AssertFiles(before); Assert.Equal(10, h.Client.Calls); Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), h.Open.Session.Snapshot.Effects);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RequiresExactlyFiftyTrainAndHeldOutRows()
    {
        var runner = new CodeDocumentationTrainingRunner();
        var examples = BuildExamples(49, "train");
        var heldOut = BuildExamples(50, "heldout");

        var result = await runner.TrainAndEvaluateAsync(examples, heldOut);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_BATCH_SIZE_NOT_50", result.ErrorCode);
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_TrainsRoutesAndMeasuresFiftyHeldOutRows()
    {
        var runner = new CodeDocumentationTrainingRunner();
        var training = BuildExamples(50, "train");
        var heldOut = BuildExamples(50, "heldout");

        var result = await runner.TrainAndEvaluateAsync(training, heldOut);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingExampleCount);
        Assert.Equal(50, result.Value.EvaluationExampleCount);
        Assert.Equal(50, result.Value.RoutedCorrect);
        Assert.Equal(50, result.Value.SourceCorrect);
        Assert.Equal(50, result.Value.VersionCorrect);
        Assert.Equal(50, result.Value.TransitCorrect);
        Assert.Equal(1.0, result.Value.RoutingAccuracy);
        Assert.Equal(1.0, result.Value.SourceGateAccuracy);
        Assert.Equal(1.0, result.Value.VersionGateAccuracy);
        Assert.Equal(1.0, result.Value.TransitGateAccuracy);
        Assert.Equal(1.0, result.Value.BottomUpOrderAccuracy);
        Assert.Equal(1.0, result.Value.CalleeSummaryTransitAccuracy);
        Assert.Equal(1.0, result.Value.LocalVsDelegatedAccuracy);
        Assert.True(result.Value.CpuMilliseconds >= 0);
        Assert.True(result.Value.ElapsedMilliseconds >= 0);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
        Assert.Equal(50, result.Value.DpoPairs.Length);
        Assert.All(result.Value.DpoPairs, pair => Assert.Contains("[WRONG:", pair.Rejected));
        Assert.All(result.Value.Cases, item => Assert.Equal(OrchestrationRoutingCurriculum.DocumentationDotNetUnitId, item.ActualUnitId));
    }

    [Fact]
    public async Task CodeDocumentationBatchFileRunner_ReadsJsonlAndRunsFiftyTrainFiftyHeldOutRows()
    {
        var root = Path.Combine(Path.GetTempPath(), $"code-doc-batch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var trainPath = Path.Combine(root, "TRAIN-PAIRS.jsonl");
        var heldOutPath = Path.Combine(root, "HELDOUT-ROWS.jsonl");
        await File.WriteAllLinesAsync(trainPath, BuildRows(50, "file-train").Select(row => JsonSerializer.Serialize(row, JsonOptions)));
        await File.WriteAllLinesAsync(heldOutPath, BuildRows(50, "file-heldout").Select(row => JsonSerializer.Serialize(row, JsonOptions)));

        var result = await new CodeDocumentationBatchFileRunner().RunAsync(trainPath, heldOutPath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.TrainingExampleCount);
        Assert.Equal(50, result.Value.EvaluationExampleCount);
        Assert.Equal(1.0, result.Value.RoutingAccuracy);
        Assert.Equal(1.0, result.Value.SourceGateAccuracy);
        Assert.Equal(1.0, result.Value.VersionGateAccuracy);
        Assert.Equal(1.0, result.Value.TransitGateAccuracy);
        Assert.True(result.Value.PeakWorkingSetMb > 0);
    }

    [Fact]
    public async Task CodeDocumentationBatchFileRunner_RejectsRowsWithoutWrongAnnotation()
    {
        var root = Path.Combine(Path.GetTempPath(), $"code-doc-batch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var trainPath = Path.Combine(root, "TRAIN-PAIRS.jsonl");
        var heldOutPath = Path.Combine(root, "HELDOUT-ROWS.jsonl");
        var trainRows = BuildRows(50, "bad-train");
        trainRows[0] = trainRows[0] with { Rejected = "Route to orchestration instead." };
        await File.WriteAllLinesAsync(trainPath, trainRows.Select(row => JsonSerializer.Serialize(row, JsonOptions)));
        await File.WriteAllLinesAsync(heldOutPath, BuildRows(50, "bad-heldout").Select(row => JsonSerializer.Serialize(row, JsonOptions)));

        var result = await new CodeDocumentationBatchFileRunner().RunAsync(trainPath, heldOutPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("MISSING_WRONG_ANNOTATION", result.ErrorCode);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoDataset_CleanMissSealsAndIdenticalRequestReusesDisk()
    {
        using var fixture = new RepositoryFixture();
        var rows =
            new[]
            {
                CreateReferenceDecision("csharp-clean-miss-001", "NET Core controller"),
                CreateReferenceDecision("csharp-clean-miss-002", ".NET Framework auth service")
            };
        var openTeacher = CreateOpenTeacherIdentity();
        var teacher = new CountingTeacherClient(openTeacher, prompt =>
        {
            Assert.Contains("paid_reference=", prompt, StringComparison.Ordinal);
            Assert.Contains("paid_reference_sha256=", prompt, StringComparison.Ordinal);
            return JsonSerializer.Serialize(new
            {
                chosen = $"Teacher-authored C# documentation route for {ExtractPromptField(prompt, "request_id")}. It keeps framework and callee evidence visible without copying the paid answer.",
                rejected = $"Teacher-authored weak C# documentation route for {ExtractPromptField(prompt, "request_id")}. It skips a required caller/callee check.",
                rejected_reason = "the weak side drops the version-aware bottom-up evidence"
            });
        });
        var factoryCalls = 0;
        var firstContext = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
            },
            maxPairsToAuthor: 2);
        var address = AssertSuccess(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateDefaultAddress(
                fixture.RepositoryRoot,
                "v1"));
        Assert.False(File.Exists(address.ManifestPath));

        var first =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(
                    address,
                    TeachingSource(rows),
                    firstContext);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            first.Value!.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(2, teacher.InferenceCalls);
        Assert.True(File.Exists(address.ManifestPath), address.ManifestPath);
        Assert.Equal(2, first.Value.Dataset.Manifest.PairCount);
        Assert.Equal(2, first.Value.Dataset.Pairs.Count);
        Assert.All(first.Value.Dataset.Pairs, pair =>
        {
            Assert.Equal(
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DomainSlug,
                pair.Domain);
            Assert.NotNull(pair.TeacherProvenance);
            Assert.Equal(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                pair.ChosenAuthorship?.AuthorKind);
            Assert.Equal(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                pair.RejectedAuthorship?.AuthorKind);
        });
        AssertPaidReferenceTextAbsentFromDataset(address, rows);
        AssertPaidReferenceTextAbsentFromPairObjects(first.Value.Dataset.Pairs, rows);
        var firstHashes = SnapshotRegularFiles(address.DatasetRoot);

        var secondContext = CreateAuthoringContext(
            () => throw new InvalidOperationException(
                "Saved C# documentation DPO must be reused from disk."),
            maxPairsToAuthor: 2);
        var second =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(
                    address,
                    TeachingSource(rows),
                    secondContext);

        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            second.Value!.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(2, teacher.InferenceCalls);
        Assert.Equal(
            first.Value.Dataset.Manifest.ManifestSha256,
            second.Value.Dataset.Manifest.ManifestSha256);
        Assert.Equal(firstHashes, SnapshotRegularFiles(address.DatasetRoot));
        AssertPaidReferenceTextAbsentFromDataset(address, rows);
        AssertPaidReferenceTextAbsentFromPairObjects(second.Value.Dataset.Pairs, rows);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoDataset_RejectsCertificationSourceBeforeCacheOrTeacher()
    {
        using var fixture = new RepositoryFixture();
        var rows =
            new[]
            {
                CreateReferenceDecision(
                    "csharp-certification-boundary-001",
                    "closed certification exam")
            };
        var openTeacher = CreateOpenTeacherIdentity();
        var teacher = new CountingTeacherClient(
            openTeacher,
            prompt => JsonSerializer.Serialize(new
            {
                chosen = $"Teaching answer for {ExtractPromptField(prompt, "request_id")}",
                rejected = "Teaching answer that omits required evidence.",
                rejected_reason = "the weak side omits required evidence"
            }));
        var factoryCalls = 0;
        var teachingContext = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
            },
            maxPairsToAuthor: 1);
        var address = AssertSuccess(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .CreateDefaultAddress(fixture.RepositoryRoot, "v1"));

        var teachingResult =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(
                    address,
                    TeachingSource(rows),
                    teachingContext);

        Assert.True(teachingResult.IsSuccess, teachingResult.ErrorMessage);
        Assert.Equal(1, factoryCalls);
        var beforeRejectedSource = SnapshotRegularFiles(address.DatasetRoot);
        var examSource = AssertSuccess(
            CodeDocumentationRepositoryOwnedDpoAuthoringSource.Create(
                rows,
                CodeDocumentationRepositoryOwnedDpoMaterialPurpose
                    .CertificationExam,
                "synthetic-certification-exam",
                closedExamExposed: true));
        var noTeacherContext = CreateAuthoringContext(
            () => throw new InvalidOperationException(
                "Certification material must be rejected before teacher creation."),
            maxPairsToAuthor: 1);

        var rejected =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(address, examSource, noTeacherContext);

        Assert.False(rejected.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AuthoringSourceNotTrainableErrorCode,
            rejected.ErrorCode);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(beforeRejectedSource, SnapshotRegularFiles(address.DatasetRoot));

        var rawRejected =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(address, rows, noTeacherContext);

        Assert.False(rawRejected.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AuthoringSourceNotTrainableErrorCode,
            rawRejected.ErrorCode);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(beforeRejectedSource, SnapshotRegularFiles(address.DatasetRoot));
    }

    [Theory]
    [InlineData(
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AngularDomainSlug,
        "Angular order editor component")]
    [InlineData(
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SqlDomainSlug,
        "SQL stored procedure inventory snapshot")]
    public async Task CodeDocumentationRepositoryOwnedDpoDataset_AngularAndSqlCleanMissSealAndIdenticalRequestReusesDisk(
        string domainSlug,
        string codeSurface)
    {
        using var fixture = new RepositoryFixture();
        var profile = AssertSuccess(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveSupportedDomainProfile(domainSlug));
        var rows =
            new[]
            {
                CreateReferenceDecision(
                    $"{domainSlug}-clean-miss-001",
                    codeSurface,
                    profile),
                CreateReferenceDecision(
                    $"{domainSlug}-clean-miss-002",
                    $"{codeSurface} dependency map",
                    profile)
            };
        var openTeacher = CreateOpenTeacherIdentity();
        var teacher = new CountingTeacherClient(openTeacher, prompt =>
        {
            Assert.Contains($"domain={domainSlug}", prompt, StringComparison.Ordinal);
            Assert.Contains("design_pattern=ilearning-unit", prompt, StringComparison.Ordinal);
            Assert.Contains("paid_reference=", prompt, StringComparison.Ordinal);
            Assert.Contains("paid_reference_sha256=", prompt, StringComparison.Ordinal);
            Assert.Contains(profile.RequiredTeacherFocus, prompt, StringComparison.Ordinal);
            return JsonSerializer.Serialize(new
            {
                chosen = $"Teacher-authored {profile.DisplayName} route for {ExtractPromptField(prompt, "request_id")}. It keeps {profile.RequiredTeacherFocus} visible without copying the paid answer.",
                rejected = $"Teacher-authored weak {profile.DisplayName} route for {ExtractPromptField(prompt, "request_id")}. It omits required domain evidence.",
                rejected_reason = "the weak side drops domain-specific documentation evidence"
            });
        });
        var factoryCalls = 0;
        var firstContext = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
            },
            maxPairsToAuthor: 2,
            domainProfile: profile);
        var address = AssertSuccess(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateAddress(
                fixture.RepositoryRoot,
                profile,
                "v1"));
        Assert.Equal(domainSlug, address.DomainSlug);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .DesignPatternSlug,
            address.DesignPatternSlug);
        Assert.Equal(
            $"training-data/dpo/{domainSlug}/ilearning-unit/v1/manifest.json",
            address.ManifestRelativePath);
        Assert.False(File.Exists(address.ManifestPath));

        var first =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(
                    address,
                    TeachingSource(rows),
                    firstContext);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            first.Value!.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(2, teacher.InferenceCalls);
        Assert.True(File.Exists(address.ManifestPath), address.ManifestPath);
        Assert.Equal(domainSlug, first.Value.Dataset.Manifest.Domain);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .DesignPatternSlug,
            first.Value.Dataset.Manifest.DesignPattern);
        Assert.All(first.Value.Dataset.Manifest.OrderedPairs, pair =>
            Assert.StartsWith(
                address.PairsRootRelativePath + "/",
                pair.RepositoryRelativePairFilePath,
                StringComparison.Ordinal));
        Assert.Equal(2, first.Value.Dataset.Manifest.PairCount);
        Assert.Equal(2, first.Value.Dataset.Pairs.Count);
        Assert.All(first.Value.Dataset.Pairs, pair =>
        {
            Assert.Equal(domainSlug, pair.Domain);
            Assert.NotNull(pair.TeacherProvenance);
            Assert.Equal(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                pair.ChosenAuthorship?.AuthorKind);
            Assert.Equal(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                pair.RejectedAuthorship?.AuthorKind);
        });
        AssertPaidReferenceTextAbsentFromDataset(address, rows);
        AssertPaidReferenceTextAbsentFromPairObjects(first.Value.Dataset.Pairs, rows);

        var firstIdentity = AssertSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.OpenManifestIdentityOnly(
                address));
        var firstReopened = AssertSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                address,
                firstIdentity.RequestIdentity));
        Assert.Equal(domainSlug, firstReopened.Manifest.Domain);
        Assert.Equal(
            first.Value.Dataset.Manifest.ManifestSha256,
            firstReopened.Manifest.ManifestSha256);
        AssertRepositoryOwnedPairsEqual(
            first.Value.Dataset.Pairs,
            firstReopened.Pairs);
        var firstHashes = SnapshotRegularFiles(address.DatasetRoot);

        var secondContext = CreateAuthoringContext(
            () => throw new InvalidOperationException(
                "Saved code documentation DPO must be reused from disk."),
            maxPairsToAuthor: 2,
            domainProfile: profile);
        var second =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(
                    address,
                    TeachingSource(rows),
                    secondContext);

        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            second.Value!.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(2, teacher.InferenceCalls);
        Assert.Equal(
            first.Value.Dataset.Manifest.ManifestSha256,
            second.Value.Dataset.Manifest.ManifestSha256);
        Assert.Equal(firstHashes, SnapshotRegularFiles(address.DatasetRoot));
        AssertRepositoryOwnedPairsEqual(
            firstReopened.Pairs,
            second.Value.Dataset.Pairs);
        AssertPaidReferenceTextAbsentFromDataset(address, rows);
        AssertPaidReferenceTextAbsentFromPairObjects(second.Value.Dataset.Pairs, rows);
    }

    [Fact]
    public async Task CodeDocumentationRepositoryOwnedDpoDataset_RejectsPaidReferenceOrWrongTeacherBeforeSeal()
    {
        using var fixture = new RepositoryFixture();
        var row = CreateReferenceDecision(
            "csharp-invalid-authoring-001",
            "legacy SQL repository");
        var address = AssertSuccess(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateDefaultAddress(
                fixture.RepositoryRoot,
                "v1"));
        var paidReference = BuildPaidReferenceAnswer(row);
        var paidReferenceContext = CreateAuthoringContext(
            () => OperationResult<ITrainingTeacherInferenceClient>.Ok(
                new CountingTeacherClient(CreateOpenTeacherIdentity(), _ =>
                    JsonSerializer.Serialize(new
                    {
                        chosen = paidReference,
                        rejected = "Teacher-authored weak answer that is different.",
                        rejected_reason = "chosen copied the paid reference"
                    }))),
            maxPairsToAuthor: 1);

        var paidReferenceResult =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(
                    address,
                    TeachingSource([row]),
                    paidReferenceContext);

        Assert.False(paidReferenceResult.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .TeacherOutputInvalidErrorCode,
            paidReferenceResult.ErrorCode);
        Assert.False(File.Exists(address.ManifestPath));
        Assert.False(Directory.Exists(address.PairsRoot)
                     && Directory.EnumerateFiles(
                         address.PairsRoot,
                         "*.dpo-pair.json",
                         SearchOption.AllDirectories).Any());

        var wrongTeacher = CreateOpenTeacherIdentity() with
        {
            ModelArtifactSha256 = new string('e', 64)
        };
        var wrongTeacherContext = CreateAuthoringContext(
            () => OperationResult<ITrainingTeacherInferenceClient>.Ok(
                new CountingTeacherClient(wrongTeacher, _ =>
                    JsonSerializer.Serialize(new
                    {
                        chosen = "Teacher-authored C# documentation answer.",
                        rejected = "Teacher-authored weak documentation answer.",
                        rejected_reason = "weak answer drops framework evidence"
                    }))),
            maxPairsToAuthor: 1);

        var wrongTeacherResult =
            await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(
                    address,
                    TeachingSource([row]),
                    wrongTeacherContext);

        Assert.False(wrongTeacherResult.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .TeacherProvenanceMismatchErrorCode,
            wrongTeacherResult.ErrorCode);
        Assert.False(File.Exists(address.ManifestPath));
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoCleanMissThenBatchRunnerReusesDisk()
    {
        using var fixture = new RepositoryFixture();
        var training = BuildExamples(50, "repo-train");
        var heldOut = BuildExamples(50, "repo-heldout");
        var openTeacher = CreateOpenTeacherIdentity();
        var teacher = new CountingTeacherClient(openTeacher, prompt =>
            JsonSerializer.Serialize(new
            {
                chosen = $"Teacher-authored repository C# documentation training source for {ExtractPromptField(prompt, "request_id")}. It preserves framework and caller/callee evidence.",
                rejected = $"Teacher-authored weak repository C# documentation route for {ExtractPromptField(prompt, "request_id")}. It omits the bottom-up callee evidence.",
                rejected_reason = "the weak side is not version-aware enough"
            }));
        var factoryCalls = 0;
        var context = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
            },
            maxPairsToAuthor: 2);
        var options = new CodeDocumentationRepositoryOwnedDpoRunOptions(
            fixture.RepositoryRoot,
            context,
            "v1",
            rows => OperationResult<
                CodeDocumentationRepositoryOwnedDpoAuthoringSource>.Ok(
                TeachingSource(rows)));
        var address = AssertSuccess(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .CreateDefaultAddress(fixture.RepositoryRoot, "v1"));

        var first = await new CodeDocumentationTrainingRunner()
            .TrainAndEvaluateAsync(
                training,
                heldOut,
                repositoryOwnedDpo: options);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(1.0, first.Value!.RoutingAccuracy);
        Assert.Equal(50, first.Value.DpoPairs.Length);
        Assert.Equal(2, first.Value.RepositoryOwnedDpoPairs.Length);
        Assert.NotNull(first.Value.RepositoryOwnedDpoReceipt);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            first.Value.RepositoryOwnedDpoReceipt!.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(2, teacher.InferenceCalls);
        Assert.Equal(address.ManifestRelativePath,
            first.Value.RepositoryOwnedDpoReceipt.RepositoryRelativeManifestPath);
        Assert.Equal(2, first.Value.RepositoryOwnedDpoReceipt.PairCount);

        var firstIdentity = AssertSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.OpenManifestIdentityOnly(
                address));
        var firstReopened = AssertSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                address,
                firstIdentity.RequestIdentity));
        Assert.Equal(
            firstReopened.Manifest.ManifestSha256,
            first.Value.RepositoryOwnedDpoReceipt.ManifestSha256);
        Assert.Equal(
            firstReopened.Manifest.RequestIdentity.RequestIdentitySha256,
            first.Value.RepositoryOwnedDpoReceipt.RequestIdentitySha256);
        Assert.Equal(
            firstReopened.Manifest.OrderedPairs,
            first.Value.RepositoryOwnedDpoReceipt.OrderedPairs);
        AssertRepositoryOwnedPairsEqual(
            firstReopened.Pairs,
            first.Value.RepositoryOwnedDpoPairs);
        AssertPaidReferenceTextAbsentFromDataset(address, heldOut.Select(
            example => new RoutingDecisionRecord(
                $"code-doc-eval-{example.ExampleId}",
                example.ExampleId,
                $"Document legacy C# code bottom-up for {example.Request.StackLabel} {example.Request.TargetFramework} {example.Request.CSharpLanguageVersion}. Symbol: {example.Request.SymbolId}. Level: {example.Request.DocumentationLevel}.",
                string.Join(
                    " ",
                    [
                        "documentation",
                        "legacy dotnet code documentation",
                        "bottom-up callee caller propagation",
                        example.Request.AppSide,
                        example.Request.RuntimeFamily,
                        example.Request.TargetFramework,
                        example.Request.CSharpLanguageVersion,
                        example.Request.WebFramework,
                        example.Request.FrontendFramework,
                        example.Request.StackLabel,
                        example.Request.ArtifactKind,
                        example.Request.DocumentationLevel
                    ]),
                example.ExpectedUnitId,
                1,
                [
                    new UnitScoringResult(
                        OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                        "wrong-specialist-unit",
                        "wrong-domain",
                        0,
                        0.01)
                ],
                example.CorrectReason,
                example.WrongRouteConsequence,
                DateTimeOffset.UnixEpoch)).ToArray());
        var firstHashes = SnapshotRegularFiles(address.DatasetRoot);

        var trainPath = Path.Combine(fixture.RepositoryRoot, "TRAIN.jsonl");
        var heldOutPath = Path.Combine(fixture.RepositoryRoot, "HELDOUT.jsonl");
        await File.WriteAllLinesAsync(
            trainPath,
            training.Select(example => JsonSerializer.Serialize(
                ToRow(example),
                JsonOptions)));
        await File.WriteAllLinesAsync(
            heldOutPath,
            heldOut.Select(example => JsonSerializer.Serialize(
                ToRow(example),
                JsonOptions)));

        var second = await new CodeDocumentationBatchFileRunner()
            .RunAsync(trainPath, heldOutPath, repositoryOwnedDpo: options);

        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.NotNull(second.Value!.RepositoryOwnedDpoReceipt);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            second.Value.RepositoryOwnedDpoReceipt!.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(2, teacher.InferenceCalls);
        Assert.Equal(
            first.Value.RepositoryOwnedDpoReceipt.ManifestSha256,
            second.Value.RepositoryOwnedDpoReceipt.ManifestSha256);
        Assert.Equal(
            first.Value.RepositoryOwnedDpoReceipt.RequestIdentitySha256,
            second.Value.RepositoryOwnedDpoReceipt.RequestIdentitySha256);
        Assert.Equal(firstHashes, SnapshotRegularFiles(address.DatasetRoot));
        AssertRepositoryOwnedPairsEqual(
            firstReopened.Pairs,
            second.Value.RepositoryOwnedDpoPairs);
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoInvalidOptionsFailBeforeTeacherOrWrites()
    {
        using var fixture = new RepositoryFixture();
        var training = BuildExamples(50, "invalid-repo-train");
        var heldOut = BuildExamples(50, "invalid-repo-heldout");
        var factoryCalls = 0;
        var validContext = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "factory must not be called");
            },
            maxPairsToAuthor: 2);

        var invalidContext = await new CodeDocumentationTrainingRunner()
            .TrainAndEvaluateAsync(
                training,
                heldOut,
                repositoryOwnedDpo:
                    new CodeDocumentationRepositoryOwnedDpoRunOptions(
                        fixture.RepositoryRoot,
                        null!,
                        "v1"));

        Assert.False(invalidContext.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            invalidContext.ErrorCode);
        Assert.Equal(0, factoryCalls);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-data")));

        var invalidAddress = await new CodeDocumentationTrainingRunner()
            .TrainAndEvaluateAsync(
                training,
                heldOut,
                repositoryOwnedDpo:
                    new CodeDocumentationRepositoryOwnedDpoRunOptions(
                        fixture.RepositoryRoot,
                        validContext,
                        "../escape"));

        Assert.False(invalidAddress.IsSuccess);
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode,
            invalidAddress.ErrorCode);
        Assert.Equal(0, factoryCalls);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-data")));

        var invalidSlugAddress =
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateAddress(
                fixture.RepositoryRoot,
                "angular.documentation",
                "v1");
        Assert.False(invalidSlugAddress.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            invalidSlugAddress.ErrorCode);
        Assert.Equal(0, factoryCalls);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-data")));

        var invalidProfileCalls = 0;
        var invalidProfileContext = CreateAuthoringContext(
            () =>
            {
                invalidProfileCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "factory must not be called");
            },
            maxPairsToAuthor: 1,
            domainProfile:
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AngularDomainProfile with
                {
                    RequiredTeacherFocus = "mutated profile"
                });

        var invalidProfile = await new CodeDocumentationTrainingRunner()
            .TrainAndEvaluateAsync(
                training,
                heldOut,
                repositoryOwnedDpo:
                    new CodeDocumentationRepositoryOwnedDpoRunOptions(
                        fixture.RepositoryRoot,
                        invalidProfileContext,
                        "v1"));

        Assert.False(invalidProfile.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            invalidProfile.ErrorCode);
        Assert.Equal(0, invalidProfileCalls);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-data")));
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoRequiresOwnerSourceBeforeWrites()
    {
        using var fixture = new RepositoryFixture();
        var training = BuildExamples(50, "owner-source-train");
        var heldOut = BuildExamples(50, "owner-source-heldout");
        var factoryCalls = 0;
        var context = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "teacher must not be created without an owner source");
            },
            maxPairsToAuthor: 2);

        var result = await new CodeDocumentationTrainingRunner()
            .TrainAndEvaluateAsync(
                training,
                heldOut,
                repositoryOwnedDpo:
                    new CodeDocumentationRepositoryOwnedDpoRunOptions(
                        fixture.RepositoryRoot,
                        context,
                        "v1"));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AuthoringSourceRequiredErrorCode,
            result.ErrorCode);
        Assert.Equal(0, factoryCalls);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-data")));
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoConvertsOrdinaryResolverFailure()
    {
        using var fixture = new RepositoryFixture();
        Func<int> getFactoryCalls;
        var options = CreateResolverThrowingOptions(
            fixture.RepositoryRoot,
            new InvalidOperationException("synthetic ordinary resolver failure"),
            out getFactoryCalls);

        var result = await new CodeDocumentationTrainingRunner()
            .TrainAndEvaluateAsync(
                BuildExamples(50, "resolver-ordinary-train"),
                BuildExamples(50, "resolver-ordinary-heldout"),
                repositoryOwnedDpo: options);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AuthoringSourceInvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, getFactoryCalls());
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-data")));
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoPropagatesDirectResolverCancellation()
    {
        using var fixture = new RepositoryFixture();
        Func<int> getFactoryCalls;
        var options = CreateResolverThrowingOptions(
            fixture.RepositoryRoot,
            new OperationCanceledException("synthetic direct resolver cancellation"),
            out getFactoryCalls);

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(
            () => new CodeDocumentationTrainingRunner()
                .TrainAndEvaluateAsync(
                    BuildExamples(50, "resolver-direct-cancel-train"),
                    BuildExamples(50, "resolver-direct-cancel-heldout"),
                    repositoryOwnedDpo: options));

        Assert.Equal(
            "synthetic direct resolver cancellation",
            exception.Message);
        Assert.Equal(0, getFactoryCalls());
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoPropagatesWrappedResolverCancellation()
    {
        using var fixture = new RepositoryFixture();
        Func<int> getFactoryCalls;
        var options = CreateResolverThrowingOptions(
            fixture.RepositoryRoot,
            new AggregateException(
                "synthetic wrapped resolver cancellation",
                new OperationCanceledException("synthetic nested cancellation")),
            out getFactoryCalls);

        var exception = await Assert.ThrowsAsync<AggregateException>(
            () => new CodeDocumentationTrainingRunner()
                .TrainAndEvaluateAsync(
                    BuildExamples(50, "resolver-wrapped-cancel-train"),
                    BuildExamples(50, "resolver-wrapped-cancel-heldout"),
                    repositoryOwnedDpo: options));

        var nested = Assert.Single(exception.InnerExceptions);
        Assert.IsType<OperationCanceledException>(nested);
        Assert.Equal(0, getFactoryCalls());
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoPropagatesSyntheticFatalResolverFailure()
    {
        using var fixture = new RepositoryFixture();
        Func<int> getFactoryCalls;
        var options = CreateResolverThrowingOptions(
            fixture.RepositoryRoot,
            new OutOfMemoryException("synthetic fatal resolver failure"),
            out getFactoryCalls);

        var exception = await Assert.ThrowsAsync<OutOfMemoryException>(
            () => new CodeDocumentationTrainingRunner()
                .TrainAndEvaluateAsync(
                    BuildExamples(50, "resolver-fatal-train"),
                    BuildExamples(50, "resolver-fatal-heldout"),
                    repositoryOwnedDpo: options));

        Assert.Equal("synthetic fatal resolver failure", exception.Message);
        Assert.Equal(0, getFactoryCalls());
    }

    [Fact]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoRejectsMismatchedOwnerSourceBeforeWrites()
    {
        using var fixture = new RepositoryFixture();
        var training = BuildExamples(50, "mismatch-source-train");
        var heldOut = BuildExamples(50, "mismatch-source-heldout");
        var factoryCalls = 0;
        var context = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "teacher must not be created for mismatched rows");
            },
            maxPairsToAuthor: 2);

        var result = await new CodeDocumentationTrainingRunner()
            .TrainAndEvaluateAsync(
                training,
                heldOut,
                repositoryOwnedDpo:
                    new CodeDocumentationRepositoryOwnedDpoRunOptions(
                        fixture.RepositoryRoot,
                        context,
                        "v1",
                        rows =>
                            OperationResult<
                                CodeDocumentationRepositoryOwnedDpoAuthoringSource>.Ok(
                                TeachingSource(rows.Take(1).ToArray()))));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AuthoringSourceBindingMismatchErrorCode,
            result.ErrorCode);
        Assert.Equal(0, factoryCalls);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-data")));
    }

    [Theory]
    [InlineData(CodeDocumentationRepositoryOwnedDpoMaterialPurpose.Unknown, false)]
    [InlineData(CodeDocumentationRepositoryOwnedDpoMaterialPurpose.CertificationExam, true)]
    [InlineData(CodeDocumentationRepositoryOwnedDpoMaterialPurpose.DiagnosticFeedback, false)]
    public async Task CodeDocumentationTrainingRunner_RepositoryOwnedDpoRejectsNonTeachingOwnerSource(
        CodeDocumentationRepositoryOwnedDpoMaterialPurpose purpose,
        bool closedExamExposed)
    {
        using var fixture = new RepositoryFixture();
        var training = BuildExamples(50, "nonteaching-source-train");
        var heldOut = BuildExamples(50, "nonteaching-source-heldout");
        var factoryCalls = 0;
        var context = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "teacher must not be created for non-teaching material");
            },
            maxPairsToAuthor: 2);

        var result = await new CodeDocumentationTrainingRunner()
            .TrainAndEvaluateAsync(
                training,
                heldOut,
                repositoryOwnedDpo:
                    new CodeDocumentationRepositoryOwnedDpoRunOptions(
                        fixture.RepositoryRoot,
                        context,
                        "v1",
                        rows => CodeDocumentationRepositoryOwnedDpoAuthoringSource
                            .Create(
                                rows,
                                purpose,
                                "synthetic-nonteaching-source",
                                closedExamExposed)));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AuthoringSourceNotTrainableErrorCode,
            result.ErrorCode);
        Assert.Equal(0, factoryCalls);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-data")));
    }

    [Fact]
    public async Task StrictSinglePair_CleanMissAndFreshCache_UseActualFinalBytesWithoutSecondTeacher()
    {
        using var fixture = new RepositoryFixture();
        var row = CreateReferenceDecision("single-clean", "controller");
        var source = TeachingSource([row]);
        var teacher = SinglePairTeacher();
        var factory = 0;
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        var address = SinglePairAddress(fixture);
        var expected = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .PrepareRequest(source, context));
        var first = AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, source, context));
        Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            first.Source);
        Assert.Equal(1, factory);
        Assert.Equal(1, teacher.InferenceCalls);
        var pair = Assert.Single(first.Dataset.Pairs);
        Assert.Equal(1, first.Dataset.Manifest.PairCount);
        var ordered = Assert.Single(first.Dataset.Manifest.OrderedPairs);
        Assert.Equal(expected.RequestIdentity.RequestIdentitySha256,
            first.Dataset.Manifest.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(ExpectedSingleTrainerPrompt(row), pair.Prompt);
        Assert.Contains("row_ordinal=0", Assert.Single(teacher.Prompts), StringComparison.Ordinal);
        Assert.Equal(FileDpoPairStore.ComputeIdentity(pair, schemaVersion: 5), ordered.PairId);
        Assert.NotNull(pair.TeacherProvenance);
        Assert.Equal(DPO.Models.DpoPairSideAuthorKind.OpenTeacher, pair.ChosenAuthorship!.AuthorKind);
        Assert.Equal(DPO.Models.DpoPairSideAuthorKind.OpenTeacher, pair.RejectedAuthorship!.AuthorKind);
        var reopened = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenSealedFileIdentity(address, expected.RequestIdentity));
        AssertRepositoryOwnedPairsEqual(first.Dataset.Pairs, reopened.Pairs);
        Assert.Equal(first.Dataset.Manifest.ManifestSha256, reopened.Manifest.ManifestSha256);
        Assert.Equal(new FileInfo(address.ManifestPath).Length, reopened.ManifestFileByteLength);
        Assert.Equal(FileSha256(address.ManifestPath), reopened.ManifestFileSha256);
        Assert.Equal(FileSha256(Path.Combine(address.RepositoryRoot,
                ordered.RepositoryRelativePairFilePath.Replace('/', Path.DirectorySeparatorChar))),
            ordered.PairFileSha256);
        AssertPaidReferenceTextAbsentFromDataset(address, [row]);
        var bytes = SnapshotSinglePairFiles(address);
        var freshFactory = 0;
        var freshTeacher = SinglePairTeacher();
        var freshContext = CreateAuthoringContext(() =>
        {
            freshFactory++;
            throw new InvalidOperationException("Matching cache must not construct a teacher.");
        }, 1);
        var second = AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, TeachingSource([row]), freshContext));
        Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset,
            second.Source);
        Assert.Equal(0, freshFactory);
        Assert.Equal(0, freshTeacher.InferenceCalls);
        Assert.Equal(bytes, SnapshotSinglePairFiles(address));
        Assert.Equal(first.Dataset.Manifest.ManifestSha256, second.Dataset.Manifest.ManifestSha256);
        Assert.Equal(first.Dataset.Manifest.RequestIdentity.RequestIdentitySha256,
            second.Dataset.Manifest.RequestIdentity.RequestIdentitySha256);
        AssertRepositoryOwnedPairsEqual(first.Dataset.Pairs, second.Dataset.Pairs);
        output.WriteLine("SINGLE_CLEAN_CACHE factory/inference=1/1 then0/0; fresh final-file/PairId/schema5/manifest/request identity; exact bytes,length,ticks,set unchanged");
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 2)]
    public async Task StrictSinglePair_AmbiguousCountOrMax_RejectsBeforeAnyEffects(int rowCount, int max)
    {
        using var fixture = new RepositoryFixture();
        var rows = Enumerable.Range(0, rowCount)
            .Select(i => CreateReferenceDecision("single-count-" + i, "controller")) .ToArray();
        var teacher = SinglePairTeacher();
        var factory = 0;
        var validContext = CreateAuthoringContext(() => OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher), 1);
        foreach (var row in rows)
            Assert.Single(AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .PrepareRequest(TeachingSource([row]), validContext)).PaidReferenceRows);
        var context = validContext with { MaxPairsToAuthor = max, CreateTeacherClient = () =>
        {
            factory++;
            throw new InvalidOperationException("Strict count guard must precede resolver.");
        }};
        var address = SinglePairAddress(fixture);
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorSinglePairAsync(address, TeachingSource(rows), context),
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        Assert.Equal(0, factory);
        Assert.Equal(0, teacher.InferenceCalls);
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "training-data")));
        output.WriteLine($"SINGLE_COUNT rows={rowCount} max={max}; every row individually train-ready; factory/inference0/0; training-data absent");
    }

    [Theory]
    [InlineData("null-rows")]
    [InlineData("empty-rows")]
    [InlineData("null-row")]
    [InlineData("null-candidates")]
    [InlineData("empty-candidates")]
    [InlineData("null-candidate")]
    public void StrictSinglePair_ActualSourceFactory_RejectsMalformedShapeBeforeHash(string mutation)
    {
        var row = CreateReferenceDecision("single-shape", "controller");
        IReadOnlyList<RoutingDecisionRecord>? rows = mutation switch
        {
            "null-rows" => null,
            "empty-rows" => [],
            "null-row" => [null!],
            "null-candidates" => [row with { RejectedCandidates = null! }],
            "empty-candidates" => [row with { RejectedCandidates = [] }],
            _ => [row with { RejectedCandidates = [null!] }]
        };
        var refused = CodeDocumentationRepositoryOwnedDpoAuthoringSource.Create(rows,
            CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference,
            "synthetic-approved-teaching", false);
        Assert.False(refused.IsSuccess);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthoringSourceInvalidErrorCode,
            refused.ErrorCode);
        output.WriteLine($"SINGLE_FACTORY_SHAPE {mutation}; genuine Source.Create SOURCE_INVALID; no strict-envelope claim");
    }

    [Theory]
    [InlineData("null-source")]
    [InlineData("null-context")]
    [InlineData("invalid-required-text")]
    public async Task StrictSinglePair_PublicGuard_RejectsMissingOrInvalidInputBeforeEffects(string mutation)
    {
        using var fixture = new RepositoryFixture();
        var row = CreateReferenceDecision("single-required", "controller");
        if (mutation == "invalid-required-text") row = row with { Prompt = "" };
        var source = TeachingSource([row]);
        var teacher = SinglePairTeacher();
        var factory = 0;
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(SinglePairAddress(fixture),
                mutation == "null-source" ? null! : source,
                mutation == "null-context" ? null! : context),
            mutation == "null-source"
                ? CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthoringSourceRequiredErrorCode
                : CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        Assert.Equal(0, factory);
        Assert.Equal(0, teacher.InferenceCalls);
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "training-data")));
        output.WriteLine($"SINGLE_PUBLIC_SHAPE {mutation}; intended typed guard; factory/inference0/0; no storage");
    }

    [Theory]
    [InlineData(CodeDocumentationRepositoryOwnedDpoMaterialPurpose.Unknown, false)]
    [InlineData(CodeDocumentationRepositoryOwnedDpoMaterialPurpose.DiagnosticFeedback, false)]
    [InlineData(CodeDocumentationRepositoryOwnedDpoMaterialPurpose.CertificationExam, false)]
    [InlineData(CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference, true)]
    public async Task StrictSinglePair_PurposeOrClosedExposure_CannotBypassGuardViaMatchingCache(
        CodeDocumentationRepositoryOwnedDpoMaterialPurpose purpose, bool exposed)
    {
        using var fixture = new RepositoryFixture();
        var row = CreateReferenceDecision("single-purpose-CANARY", "purpose-CANARY");
        var teacher = SinglePairTeacher();
        var validContext = CreateAuthoringContext(() => OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher), 1);
        var address = SinglePairAddress(fixture);
        AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, TeachingSource([row]), validContext));
        Assert.Equal(1, teacher.InferenceCalls);
        var files = SnapshotSinglePairFiles(address);
        var negativeTeacher = SinglePairTeacher();
        var factory = 0;
        var source = AssertSuccess(CodeDocumentationRepositoryOwnedDpoAuthoringSource
            .Create([row], purpose, "synthetic-purpose-CANARY", exposed));
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, source, validContext with { CreateTeacherClient = () =>
            {
                factory++;
                return OperationResult<ITrainingTeacherInferenceClient>.Ok(negativeTeacher);
            }}), CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthoringSourceNotTrainableErrorCode);
        Assert.Equal(0, factory);
        Assert.Equal(0, negativeTeacher.InferenceCalls);
        Assert.Empty(negativeTeacher.Prompts);
        Assert.Equal(files, SnapshotSinglePairFiles(address));
        output.WriteLine($"SINGLE_PURPOSE {purpose}/closed={exposed}; genuine matching cache cannot bypass purpose; no teacher/canary dispatch; exact cache preserved");
    }

    [Fact]
    public async Task StrictSinglePair_PreEntryWinningCandidateMutation_PreservesOriginalApproval()
    {
        using var fixture = new RepositoryFixture();
        var row = SinglePairRowWithTwoCandidates("single-binding");
        var source = TeachingSource([row]);
        var expectedPrompt = ExpectedSingleTrainerPrompt(row);
        var expectedWeak = row.RejectedCandidates[0].UnitId;
        var factory = 0;
        var teacher = SinglePairTeacher();
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        var expected = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .PrepareRequest(source, context));
        row.RejectedCandidates[0] = row.RejectedCandidates[0] with { UnitId = "another-legal-winner" };
        var changed = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .PrepareRequest(TeachingSource([row]), context));
        Assert.NotEqual(expected.RequestIdentity.RequestIdentitySha256,
            changed.RequestIdentity.RequestIdentitySha256);
        var address = SinglePairAddress(fixture);
        var result = AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, source, context));
        Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            result.Source);
        var pair = Assert.Single(result.Dataset.Pairs);
        Assert.Equal(expectedPrompt, pair.Prompt);
        Assert.DoesNotContain("another-legal-winner", pair.Prompt, StringComparison.Ordinal);
        Assert.Contains("weak_candidate=Route to " + expectedWeak + ".",
            Assert.Single(teacher.Prompts), StringComparison.Ordinal);
        Assert.Equal(expected.RequestIdentity.RequestIdentitySha256,
            result.Dataset.Manifest.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(ExpectedSingleGovernance(address, expected.RequestIdentity, row),
            pair.GovernanceDecision);
        Assert.Equal(expectedPrompt, ExpectedSingleTrainerPrompt(source.ReferenceRows[0]));
        Assert.Equal(1, factory);
        Assert.Equal(1, teacher.InferenceCalls);
        var opened = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenSealed(address, expected.RequestIdentity));
        AssertRepositoryOwnedPairsEqual(result.Dataset.Pairs, opened.Pairs);
        output.WriteLine("SINGLE_BINDING pre-entry winning mutation changes would-be V1; approved Create snapshot still supplies original prompt/weak candidate/governance/identity; factory1/inference1; actual sealed readback");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StrictSinglePair_PreEntryNullCandidateMutation_CannotCorruptApprovedSnapshot(
        bool mutatePublicRows)
    {
        using var fixture = new RepositoryFixture();
        var row = SinglePairRowWithTwoCandidates("single-null-after-approval");
        RoutingDecisionRecord[] originalRows = [row];
        var source = TeachingSource(originalRows);
        var expectedPrompt = ExpectedSingleTrainerPrompt(row);
        var teacher = SinglePairTeacher();
        var factory = 0;
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        var expected = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .PrepareRequest(source, context));
        var mutableRows = mutatePublicRows ? source.ReferenceRows : originalRows;
        mutableRows[0].RejectedCandidates[0] = null!;
        var address = SinglePairAddress(fixture);
        var result = AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, source, context));
        var pair = Assert.Single(result.Dataset.Pairs);
        Assert.Equal(expectedPrompt, pair.Prompt);
        Assert.Equal(expectedPrompt, ExpectedSingleTrainerPrompt(source.ReferenceRows[0]));
        Assert.All(source.ReferenceRows[0].RejectedCandidates, static candidate => Assert.NotNull(candidate));
        Assert.Equal(expected.RequestIdentity.RequestIdentitySha256,
            result.Dataset.Manifest.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(ExpectedSingleGovernance(address, expected.RequestIdentity, row),
            pair.GovernanceDecision);
        Assert.Equal(1, factory);
        Assert.Equal(1, teacher.InferenceCalls);
        var opened = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenSealed(address, expected.RequestIdentity));
        AssertRepositoryOwnedPairsEqual(result.Dataset.Pairs, opened.Pairs);
        output.WriteLine($"SINGLE_APPROVED_SHAPE public={mutatePublicRows}; post-Create external null cannot corrupt approved candidate arrays; factory1/inference1; original prompt/governance/identity and real readback; malformed factory-input tests remain refusals");
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public async Task ApprovedSource_PreEntryLowerScoreMutation_CannotChangeCleanMissOrCache(
        bool mutatePublicRows, bool strictSinglePair)
    {
        using var fixture = new RepositoryFixture();
        var row = SinglePairRowWithTwoCandidates("approved-lower-score") with
        {
            RejectedCandidates =
            [
                new UnitScoringResult("wrong-a", "rejected", "documentation", 0.9, 0.9),
                new UnitScoringResult("wrong-b", "rejected", "documentation", 0.1, 0.1)
            ]
        };
        RoutingDecisionRecord[] originalRows = [row];
        var source = TeachingSource(originalRows);
        var expectedPrompt = ExpectedSingleTrainerPrompt(row);
        var teacher = SinglePairTeacher();
        var factory = 0;
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        var expected = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .PrepareRequest(source, context));
        var approvedRowsIdentity = source.ReferenceRowsIdentitySha256;
        var mutableRows = mutatePublicRows ? source.ReferenceRows : originalRows;
        mutableRows[0].RejectedCandidates[1] = mutableRows[0].RejectedCandidates[1] with
        {
            UnitId = "changed-after-approval"
        };

        // This is the actual V1 hole: only the lower-score trainer candidate
        // changes. A newly approved changed source still has the old V1 hash.
        var changed = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .PrepareRequest(TeachingSource(mutableRows), context));
        Assert.Equal(expected.RequestIdentity.RequestIdentitySha256,
            changed.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(approvedRowsIdentity,
            TeachingSource(mutableRows).ReferenceRowsIdentitySha256);
        Assert.NotEqual(expectedPrompt, ExpectedSingleTrainerPrompt(mutableRows[0]));
        Assert.Contains("changed-after-approval", ExpectedSingleTrainerPrompt(mutableRows[0]),
            StringComparison.Ordinal);
        var address = SinglePairAddress(fixture);
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "training-data")));
        var first = AssertSuccess(strictSinglePair
            ? await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorSinglePairAsync(address, source, context)
            : await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(address, source, context));
        Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.NewlyPersistedRepositoryDataset,
            first.Source);
        Assert.Equal(1, factory);
        Assert.Equal(1, teacher.InferenceCalls);
        var pair = Assert.Single(first.Dataset.Pairs);
        Assert.Equal(expectedPrompt, pair.Prompt);
        Assert.DoesNotContain("changed-after-approval", pair.Prompt, StringComparison.Ordinal);
        Assert.Equal(expectedPrompt, ExpectedSingleTrainerPrompt(source.ReferenceRows[0]));
        Assert.Equal(approvedRowsIdentity, source.ReferenceRowsIdentitySha256);
        Assert.Equal(expected.RequestIdentity.RequestIdentitySha256,
            first.Dataset.Manifest.RequestIdentity.RequestIdentitySha256);
        var teacherPrompt = Assert.Single(teacher.Prompts);
        Assert.Contains("weak_candidate=Route to wrong-a.", teacherPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("changed-after-approval", teacherPrompt, StringComparison.Ordinal);
        Assert.Equal(ExpectedSingleGovernance(address, expected.RequestIdentity, row),
            pair.GovernanceDecision);
        var ordered = Assert.Single(first.Dataset.Manifest.OrderedPairs);
        Assert.Equal(FileDpoPairStore.ComputeIdentity(pair, schemaVersion: 5), ordered.PairId);
        var opened = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer
            .OpenSealedFileIdentity(address, expected.RequestIdentity));
        AssertRepositoryOwnedPairsEqual(first.Dataset.Pairs, opened.Pairs);
        Assert.Equal(FileSha256(address.ManifestPath), opened.ManifestFileSha256);
        Assert.Equal(FileSha256(Path.Combine(address.RepositoryRoot,
            ordered.RepositoryRelativePairFilePath.Replace('/', Path.DirectorySeparatorChar))),
            ordered.PairFileSha256);
        var files = SnapshotSinglePairFiles(address);
        var cacheFactory = 0;
        var cacheTeacher = SinglePairTeacher();
        var cacheContext = context with { CreateTeacherClient = () =>
        {
            cacheFactory++;
            throw new InvalidOperationException("The original approved snapshot must reuse its cache.");
        }};
        var cached = AssertSuccess(strictSinglePair
            ? await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorSinglePairAsync(address, source, cacheContext)
            : await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .ResolveOrAuthorAsync(address, source, cacheContext));
        Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset,
            cached.Source);
        Assert.Equal(0, cacheFactory);
        Assert.Equal(0, cacheTeacher.InferenceCalls);
        Assert.Equal(files, SnapshotSinglePairFiles(address));
        AssertRepositoryOwnedPairsEqual(first.Dataset.Pairs, cached.Dataset.Pairs);
        output.WriteLine($"APPROVED_CREATE_SNAPSHOT public={mutatePublicRows} strict={strictSinglePair}; pre-entry wrong-b mutation; V1 unchanged/would-be trainer prompt different; clean miss factory1/inference1; approved prompt/governance/final pair readback; fresh cache0/0/exact files unchanged");
    }

    [Fact]
    public async Task StrictSinglePair_FactoryAndAwaitedCallbackMutation_CannotChangeCapturedPromptsOrIdentity()
    {
        using var fixture = new RepositoryFixture();
        var row = SinglePairRowWithTwoCandidates("single-detached");
        var expectedPrompt = ExpectedSingleTrainerPrompt(row);
        var expectedWeak = row.RejectedCandidates[0].UnitId;
        var expectedOther = row.RejectedCandidates[1].UnitId;
        var source = TeachingSource([row]);
        var teacher = SinglePairTeacher();
        var factory = 0;
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            row.RejectedCandidates[0] = row.RejectedCandidates[0] with { UnitId = "factory-winner" };
            row.RejectedCandidates[1] = row.RejectedCandidates[1] with { UnitId = "factory-nonwinner" };
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(new SinglePairCallbackTeacher(teacher,
                async token =>
                {
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                    source.ReferenceRows[0].RejectedCandidates[0] = row.RejectedCandidates[0] with { UnitId = "awaited-winner" };
                    source.ReferenceRows[0].RejectedCandidates[1] = row.RejectedCandidates[1] with { UnitId = "awaited-nonwinner" };
                }));
        }, 1);
        var request = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(source, context));
        var address = SinglePairAddress(fixture);
        var result = AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, source, context));
        var pair = Assert.Single(result.Dataset.Pairs);
        Assert.Equal(expectedPrompt, pair.Prompt);
        Assert.Contains(expectedOther, pair.Prompt, StringComparison.Ordinal);
        Assert.Equal(expectedPrompt, ExpectedSingleTrainerPrompt(source.ReferenceRows[0]));
        var prompt = Assert.Single(teacher.Prompts);
        Assert.Contains("weak_candidate=Route to " + expectedWeak + ".", prompt, StringComparison.Ordinal);
        Assert.Contains("request_identity_sha256=" + request.RequestIdentity.RequestIdentitySha256, prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("factory-", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("awaited-", prompt, StringComparison.Ordinal);
        Assert.Equal(request.RequestIdentity.RequestIdentitySha256, result.Dataset.Manifest.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(1, factory);
        Assert.Equal(1, teacher.InferenceCalls);
        var opened = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(address, request.RequestIdentity));
        AssertRepositoryOwnedPairsEqual(result.Dataset.Pairs, opened.Pairs);
        output.WriteLine("SINGLE_CAPTURE real factory and awaited teacher mutate external/shared source winning+nonwinning candidates; original teacher/trainer prompt and identity remain captured; actual fresh pair readback");
    }

    [Fact]
    public async Task StrictSinglePair_ChangedRequestAtSameAddress_ConflictsWithoutAuthoringOrNewVersion()
    {
        using var fixture = new RepositoryFixture();
        var row = CreateReferenceDecision("single-conflict-A", "controller");
        var address = SinglePairAddress(fixture);
        var teacher = SinglePairTeacher();
        var context = CreateAuthoringContext(() => OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher), 1);
        AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, TeachingSource([row]), context));
        var files = SnapshotSinglePairFiles(address);
        var factory = 0;
        var changedTeacher = SinglePairTeacher();
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, TeachingSource([row with { RequestId = "single-conflict-B" }]),
                context with { CreateTeacherClient = () =>
                {
                    factory++;
                    return OperationResult<ITrainingTeacherInferenceClient>.Ok(changedTeacher);
                }}), RepositoryOwnedDpoDatasetManifestSealer.RequestIdentityMismatchErrorCode);
        Assert.Equal(0, factory);
        Assert.Equal(0, changedTeacher.InferenceCalls);
        Assert.Equal(files, SnapshotSinglePairFiles(address));
        Assert.Single(Directory.GetDirectories(Path.GetDirectoryName(address.DatasetRoot)!));
        output.WriteLine("SINGLE_CONFLICT same address/version changed genuine binding; lower identity refusal; factory/inference0/0; original bytes,ticks,set preserved; no alternate version");
    }

    [Fact]
    public async Task StrictSinglePair_GenericValidTwoPairCache_RejectsAtStrictCardinalityPostcondition()
    {
        using var fixture = new RepositoryFixture();
        using var bulkFixture = new RepositoryFixture();
        var row = CreateReferenceDecision("single-cardinality", "controller");
        var bulkRows = new[] { row, CreateReferenceDecision("single-cardinality-other", "service") };
        var bulkTeacher = SinglePairTeacher();
        var bulk = AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.ResolveOrAuthorAsync(
            SinglePairAddress(bulkFixture), TeachingSource(bulkRows),
            CreateAuthoringContext(() => OperationResult<ITrainingTeacherInferenceClient>.Ok(bulkTeacher), 2)));
        Assert.Equal(2, bulkTeacher.InferenceCalls);
        Assert.Equal(2, bulk.Dataset.Pairs.Count);
        var strictTeacher = SinglePairTeacher();
        var factory = 0;
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            throw new InvalidOperationException("Valid generic cache must bypass strict authoring factory.");
        }, 1);
        var source = TeachingSource([row]);
        var request = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(source, context));
        var address = SinglePairAddress(fixture);
        AssertSuccess(await RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(address,
            request.RequestIdentity, (_, _) => Task.FromResult(
                OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(bulk.Dataset.Pairs))));
        var generic = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(address, request.RequestIdentity));
        Assert.Equal(2, generic.Pairs.Count);
        Assert.Equal(request.RequestIdentity.RequestIdentitySha256, generic.Manifest.RequestIdentity.RequestIdentitySha256);
        var files = SnapshotSinglePairFiles(address);
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, source, context),
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        Assert.Equal(0, factory);
        Assert.Equal(0, strictTeacher.InferenceCalls);
        Assert.Equal(files, SnapshotSinglePairFiles(address));
        output.WriteLine("SINGLE_CARDINALITY lower generic OpenSealed accepts2 genuine permit/provenance/final-bound bulk pairs under valid single identity; strict INVALID postcondition; factory0; exact cache preserved");
    }

    [Theory]
    [InlineData("request-id-only")]
    [InlineData("nonwinning-candidate-only")]
    public async Task StrictSinglePair_GenericValidOtherLesson_RequiresBothPromptAndOrderedGovernanceBinding(string mutation)
    {
        using var fixture = new RepositoryFixture();
        var rowA = SinglePairRowWithTwoCandidates("single-semantic-A");
        var rowB = mutation == "request-id-only"
            ? rowA with { RequestId = "single-semantic-B", RejectedCandidates = rowA.RejectedCandidates.ToArray() }
            : rowA with { RejectedCandidates = [rowA.RejectedCandidates[0], rowA.RejectedCandidates[1] with { UnitId = "other-nonwinning-unit" }] };
        var teacher = SinglePairTeacher();
        var context = CreateAuthoringContext(() => OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher), 1);
        var requestA = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(TeachingSource([rowA]), context));
        var sourceB = TeachingSource([rowB]);
        var requestB = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(sourceB, context));
        var address = SinglePairAddress(fixture);
        // Genuine owner-authored A, sealed by the lower owner under valid B.
        AssertSuccess(await RepositoryOwnedDpoDatasetResolver.OpenExistingOrCreateAndSealAsync(address,
            requestB.RequestIdentity, (target, token) => CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AuthorPairsAsync(target, requestA, context, token)));
        var generic = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(address, requestB.RequestIdentity));
        var pairA = Assert.Single(generic.Pairs);
        var genuineB = Assert.Single(AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .AuthorPairsAsync(address, requestB, context)));
        if (mutation == "request-id-only")
        {
            Assert.Equal(pairA.Prompt, genuineB.Prompt);
            Assert.NotEqual(requestA.RequestIdentity.RequestIdentitySha256, requestB.RequestIdentity.RequestIdentitySha256);
            Assert.False(pairA.GovernanceDecision!.Equals(genuineB.GovernanceDecision));
        }
        else
        {
            Assert.NotEqual(pairA.Prompt, genuineB.Prompt);
            Assert.Equal(requestA.RequestIdentity.RequestIdentitySha256, requestB.RequestIdentity.RequestIdentitySha256);
            Assert.True(pairA.GovernanceDecision!.Equals(genuineB.GovernanceDecision));
        }
        Assert.Equal(2, teacher.InferenceCalls);
        var files = SnapshotSinglePairFiles(address);
        var factory = 0;
        var negativeTeacher = SinglePairTeacher();
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, sourceB, context with { CreateTeacherClient = () =>
            {
                factory++;
                throw new InvalidOperationException("Semantic cache mismatch must not reauthor.");
            }}), CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthoringSourceBindingMismatchErrorCode);
        Assert.Equal(0, factory);
        Assert.Equal(0, negativeTeacher.InferenceCalls);
        Assert.Equal(files, SnapshotSinglePairFiles(address));
        output.WriteLine($"SINGLE_SEMANTIC {mutation}; generic fresh B open accepts genuine A pair1; request-only prompt equal/refs unequal OR nonwinning v1+refs equal/prompt unequal; strict binding refusal, factory0, bytes/ticks/set preserved");
    }

    [Theory]
    [InlineData("malformed-json")]
    [InlineData("duplicate-property")]
    [InlineData("extra-property")]
    [InlineData("identical-sides")]
    [InlineData("paid-copy")]
    [InlineData("wrong-profile")]
    [InlineData("wrong-model")]
    [InlineData("wrong-digest")]
    [InlineData("wrong-provenance")]
    [InlineData("unconsumed-permit")]
    public async Task StrictSinglePair_ActualTeacherGuards_RejectBeforeSeal(string mutation)
    {
        using var fixture = new RepositoryFixture();
        var row = CreateReferenceDecision("single-output", "controller");
        var teacher = new CountingTeacherClient(CreateOpenTeacherIdentity(), prompt => mutation switch
        {
            "malformed-json" => "not-json",
            "duplicate-property" => "{\"chosen\":\"valid choice\",\"chosen\":\"duplicate\",\"rejected\":\"weaker\",\"rejected_reason\":\"missing evidence\"}",
            "extra-property" => "{\"chosen\":\"valid choice\",\"rejected\":\"weaker\",\"rejected_reason\":\"missing evidence\",\"extra\":\"no\"}",
            "identical-sides" => JsonSerializer.Serialize(new { chosen = "same", rejected = "same", rejected_reason = "missing evidence" }),
            "paid-copy" => JsonSerializer.Serialize(new { chosen = BuildPaidReferenceAnswer(row), rejected = "weaker", rejected_reason = "missing evidence" }),
            _ => ValidSinglePairOutput(prompt)
        }, consumePermit: mutation != "unconsumed-permit");
        var wrapper = new SinglePairCallbackTeacher(teacher, _ => Task.CompletedTask, value => mutation switch
        {
            "wrong-profile" => value with { InferenceProfile = LocalQwenOllamaInferenceProfileAuthority.Evaluation },
            "wrong-model" => value with { TrainingTeacherProvenance = value.TrainingTeacherProvenance with { ModelId = "wrong-qwen" } },
            "wrong-digest" => value with { TrainingTeacherProvenance = value.TrainingTeacherProvenance with { ModelArtifactDigest = new string('a', 64) } },
            "wrong-provenance" => value with { TrainingTeacherProvenance = value.TrainingTeacherProvenance with { ClosedProprietaryModel = true } },
            _ => value
        });
        var factory = 0;
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(wrapper);
        }, 1);
        var address = SinglePairAddress(fixture);
        var expectedCode = mutation == "unconsumed-permit"
            ? CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherFailedErrorCode
            : mutation.StartsWith("wrong-", StringComparison.Ordinal)
                ? CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherProvenanceMismatchErrorCode
                : CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherOutputInvalidErrorCode;
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, TeachingSource([row]), context), expectedCode);
        Assert.Equal(1, factory);
        Assert.Equal(1, teacher.InferenceCalls);
        Assert.False(File.Exists(address.ManifestPath));
        AssertNoSinglePairFiles(fixture);
        output.WriteLine($"SINGLE_TEACHER_GUARD {mutation}; actual permit/provenance/profile/parser path -> {expectedCode}; factory1 dispatch1; no seal/update/retry");
    }

    [Fact]
    public async Task StrictSinglePair_Cancellation_PreservesResolverPrecedenceAndCleanup()
    {
        using var fixture = new RepositoryFixture();
        var teacher = SinglePairTeacher();
        var factory = 0;
        var context = CreateAuthoringContext(() =>
        {
            factory++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var row = CreateReferenceDecision("single-cancel", "controller");
        var address = SinglePairAddress(fixture);
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.ResolveOrAuthorSinglePairAsync(
            address, TeachingSource([row]), context, cancelled.Token), RepositoryOwnedDpoDatasetResolver.CancelledErrorCode);
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.ResolveOrAuthorSinglePairAsync(
            address, null!, context, cancelled.Token), CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthoringSourceRequiredErrorCode);
        Assert.Equal(0, factory);
        Assert.Equal(0, teacher.InferenceCalls);
        Assert.False(Directory.Exists(Path.Combine(fixture.RepositoryRoot, "training-data")));
        using var during = new CancellationTokenSource();
        context = context with { CreateTeacherClient = () =>
        {
            factory++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(new SinglePairCallbackTeacher(teacher,
                async token =>
                {
                    await Task.Yield();
                    during.Cancel();
                    token.ThrowIfCancellationRequested();
                }));
        }};
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.ResolveOrAuthorSinglePairAsync(
            address, TeachingSource([row]), context, during.Token), RepositoryOwnedDpoDatasetResolver.CancelledErrorCode);
        Assert.Equal(1, factory);
        Assert.Equal(1, teacher.InferenceCalls);
        Assert.Single(teacher.Prompts);
        Assert.False(File.Exists(address.ManifestPath));
        AssertNoSinglePairFiles(fixture);
        output.WriteLine("SINGLE_CANCELLATION pre-cancelled valid -> resolver CANCELLED0factory; null source precedence preserved; cancellation after actual one-use permit/inference1 at awaited teacher callback -> lower cleanup/no final seal");
    }

    [Fact]
    public async Task StrictSinglePair_ActualLocalTeacherTransport_ReceivesApprovedFullTaskAndReusesCache()
    {
        using var fixture = new RepositoryFixture();
        var row = SinglePairRowWithTwoCandidates("full-teaching-task") with
        {
            Prompt = "Документируй вызовы C# и .NET: 中文 😀\r\nПроверь отмену\tи caller/callee."
        };
        var approvedTask = row.Prompt;
        var trainerPrompt = ExpectedSingleTrainerPrompt(row);
        var approvedWeak = row.RejectedCandidates[0].UnitId;
        var source = TeachingSource([row]);
        var handler = new SinglePairLocalTeacherHandler(async () =>
        {
            await Task.Yield();
            source.ReferenceRows[0].RejectedCandidates[0] = row.RejectedCandidates[0] with
                { UnitId = "awaited-external-mutation" };
        });
        var teacher = CreateRecordingLocalTeacher(handler);
        var factoryCalls = 0;
        var context = CreateAuthoringContext(() =>
        {
            factoryCalls++;
            row.RejectedCandidates[0] = row.RejectedCandidates[0] with { UnitId = "factory-external-mutation" };
            row = row with { Prompt = "caller changed the task after approval" };
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        var expected = AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(source, context));
        var address = SinglePairAddress(fixture);
        var first = AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, source, context));
        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, teacher.InferenceCalls);
        Assert.True(teacher.CallPermit!.CallIssued);
        Assert.Equal(1, teacher.DisposeCalls);
        Assert.Equal(new[] { "/api/tags", "/api/show", "/api/chat", "/api/ps", "/api/tags", "/api/show" },
            handler.Paths);
        Assert.Equal(LocalQwenOllamaInferenceProfileAuthority.AuthoringDescriptor.NumContext, handler.NumContext);
        var actualPrompt = Assert.Single(teacher.Prompts);
        Assert.Equal(actualPrompt, handler.ChatPrompt);
        Assert.DoesNotContain(actualPrompt, char.IsControl);
        using var envelope = JsonDocument.Parse(actualPrompt);
        Assert.Equal(approvedTask, envelope.RootElement.GetProperty("approved_task").GetString());
        var authoringContext = envelope.RootElement.GetProperty("authoring_context").GetString()!;
        Assert.NotEqual(approvedTask, ExtractPromptField(authoringContext, "task_slice"));
        Assert.Equal("Route to " + approvedWeak + ". " + row.WrongRouteConsequence,
            ExtractPromptField(authoringContext, "weak_candidate"));
        Assert.DoesNotContain("external-mutation", authoringContext, StringComparison.Ordinal);
        Assert.DoesNotContain("caller changed", actualPrompt, StringComparison.Ordinal);
        var pair = Assert.Single(first.Dataset.Pairs);
        Assert.Equal(trainerPrompt, pair.Prompt);
        Assert.Equal(expected.RequestIdentity.RequestIdentitySha256,
            first.Dataset.Manifest.RequestIdentity.RequestIdentitySha256);
        var reopened = AssertSuccess(RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(address, expected.RequestIdentity));
        AssertRepositoryOwnedPairsEqual(first.Dataset.Pairs, reopened.Pairs);
        var files = SnapshotSinglePairFiles(address);
        var cached = AssertSuccess(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(address, source, context with { CreateTeacherClient = () =>
            {
                factoryCalls++;
                throw new InvalidOperationException("Cache hit must not create another local teacher.");
            } }));
        Assert.Equal(RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset, cached.Source);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, teacher.InferenceCalls);
        Assert.Equal(6, handler.Paths.Count);
        Assert.Equal(files, SnapshotSinglePairFiles(address));
        output.WriteLine("STRICT_TASK actual local InferAsync and six scripted HTTP requests; decoded Unicode/multiline task preserved; caller/public nested mutations detached; fresh sealed cache has zero additional factory/teacher/HTTP calls");
    }

    [Fact]
    public async Task StrictSinglePair_ActualLocalTeacherTotalByteLimit_RejectsBeforePermitAndHttpWithoutTruncation()
    {
        using var fixture = new RepositoryFixture();
        var row = CreateReferenceDecision("full-context-overflow", "controller") with
        {
            Prompt = new string('x', LocalQwenOllamaTeacherInferenceClient.MaximumPromptUtf8Bytes)
        };
        var handler = new SinglePairLocalTeacherHandler();
        var teacher = CreateRecordingLocalTeacher(handler);
        var source = TeachingSource([row]);
        var factoryCalls = 0;
        var context = CreateAuthoringContext(() =>
        {
            factoryCalls++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        // The existing field guard admits the whole task; the actual client
        // must reject the larger serialized request rather than shortening it.
        AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(source, context));
        AssertSingleFailure(await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(SinglePairAddress(fixture), source, context),
            LocalQwenOllamaTeacherInferenceClient.RequestTooLargeErrorCode);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, teacher.InferenceCalls);
        Assert.False(teacher.CallPermit!.CallIssued);
        Assert.Equal(1, teacher.DisposeCalls);
        Assert.Empty(handler.Paths);
        var actual = Assert.Single(teacher.Prompts);
        Assert.True(Encoding.UTF8.GetByteCount(actual) > LocalQwenOllamaTeacherInferenceClient.MaximumPromptUtf8Bytes);
        using var envelope = JsonDocument.Parse(actual);
        Assert.Equal(row.Prompt, envelope.RootElement.GetProperty("approved_task").GetString());
        AssertNoSinglePairFiles(fixture);
        output.WriteLine("STRICT_TASK total transport bytes rejected by actual existing local client; exact request-too-large guard, method1/factory1 but permit0/HTTP0/sealed-pair0; 8192-token preflight remains unproved");
    }

    [Theory]
    [InlineData("task")]
    [InlineData("context")]
    public async Task StrictSinglePair_MalformedUtf16_CannotBecomeValidTeacherInputThroughJson(string field)
    {
        using var fixture = new RepositoryFixture();
        var row = CreateReferenceDecision("malformed-teaching-" + field, "controller");
        row = field == "task" ? row with { Prompt = "broken \uD800 task" }
            : row with { CorrectReason = "broken \uD800 reference" };
        var handler = new SinglePairLocalTeacherHandler();
        var teacher = CreateRecordingLocalTeacher(handler);
        var source = TeachingSource([row]);
        var factoryCalls = 0;
        var context = CreateAuthoringContext(() =>
        {
            factoryCalls++;
            return OperationResult<ITrainingTeacherInferenceClient>.Ok(teacher);
        }, 1);
        var result = await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveOrAuthorSinglePairAsync(SinglePairAddress(fixture), source, context);
        var normalizationObserved = teacher.Prompts.Count > 0 && teacher.Prompts[0].Contains("\\uFFFD", StringComparison.OrdinalIgnoreCase);
        output.WriteLine($"STRICT_UTF16 field={field} success={result.IsSuccess} code={result.ErrorCode} inference={teacher.InferenceCalls} HTTP={handler.Paths.Count} permit={teacher.CallPermit?.CallIssued} normalizationObserved={normalizationObserved}");
        AssertSingleFailure(result, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.InvalidErrorCode);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, teacher.DisposeCalls);
        Assert.Equal(0, teacher.InferenceCalls);
        Assert.Empty(handler.Paths);
        Assert.Null(teacher.CallPermit);
        AssertNoSinglePairFiles(fixture);
    }

    private static RecordingLocalTeacher CreateRecordingLocalTeacher(HttpMessageHandler handler)
    {
        var identity = CreateOpenTeacherIdentity();
        var configuration = AssertSuccess(ContinuousLearningTp1DpoConfiguration.Create(identity.ModelId,
            identity.ModelVersion, identity.ModelArtifactSha256, 1, "sha256:" + new string('a', 64),
            "sha256:" + new string('b', 64), Enumerable.Range(1, 32).Select(x => (byte)x).ToArray()));
        return new RecordingLocalTeacher(AssertSuccess(LocalQwenOllamaTeacherInferenceClient
            .CreateForTesting(configuration, handler, disposeHandler: false)));
    }

    private sealed class RecordingLocalTeacher(ITrainingTeacherInferenceClient inner) : ITrainingTeacherInferenceClient
    {
        public int InferenceCalls { get; private set; }
        public int DisposeCalls { get; private set; }
        public List<string> Prompts { get; } = [];
        public ITrainingTeacherCallPermit? CallPermit { get; private set; }
        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(string prompt,
            ITrainingTeacherCallPermit callPermit, CancellationToken cancellationToken = default)
        {
            InferenceCalls++;
            Prompts.Add(prompt);
            CallPermit = callPermit;
            return inner.InferAsync(prompt, callPermit, cancellationToken);
        }
        public void Dispose()
        {
            DisposeCalls++;
            inner.Dispose();
        }
    }

    private sealed class SinglePairLocalTeacherHandler(Func<Task>? beforeChat = null) : HttpMessageHandler
    {
        public List<string> Paths { get; } = [];
        public string? ChatPrompt { get; private set; }
        public int NumContext { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            Paths.Add(path);
            var teacher = CreateOpenTeacherIdentity();
            string response;
            if (path == "/api/chat")
            {
                if (beforeChat is not null) await beforeChat();
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
                ChatPrompt = Assert.Single(body.RootElement.GetProperty("messages").EnumerateArray())
                    .GetProperty("content").GetString()!;
                NumContext = body.RootElement.GetProperty("options").GetProperty("num_ctx").GetInt32();
                response = JsonSerializer.Serialize(new { model = teacher.ModelId, done = true,
                    message = new { role = "assistant", content = ValidSinglePairOutput(ChatPrompt) } });
            }
            else if (path == "/api/show")
                response = JsonSerializer.Serialize(new { modelfile = "FROM /test/.ollama/models/blobs/sha256-" + teacher.ModelArtifactSha256 + "\n" });
            else
                response = JsonSerializer.Serialize(new { models = new[] { new { name = teacher.ModelId,
                    model = teacher.ModelId, digest = teacher.ModelVersion } } });
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            };
        }
    }

    private static CountingTeacherClient SinglePairTeacher()
        => new(CreateOpenTeacherIdentity(), ValidSinglePairOutput);

    private static string ValidSinglePairOutput(string prompt)
        => JsonSerializer.Serialize(new
        {
            chosen = "Teacher-authored bottom-up documentation with caller/callee evidence for " + ExtractPromptField(prompt, "request_id"),
            rejected = "Teacher-authored weak route missing framework evidence for " + ExtractPromptField(prompt, "request_id"),
            rejected_reason = "the weak route omits the required version and call transit evidence"
        });

    private static RoutingDecisionRecord SinglePairRowWithTwoCandidates(string id)
    {
        var row = CreateReferenceDecision(id, "controller");
        return row with { RejectedCandidates = [row.RejectedCandidates[0],
            row.RejectedCandidates[0] with { UnitId = "captured-nonwinning-unit", Score = 0.01 }] };
    }

    private static RepositoryOwnedDpoDatasetAddress SinglePairAddress(RepositoryFixture fixture)
        => AssertSuccess(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateDefaultAddress(fixture.RepositoryRoot, "v1"));

    private static string ExpectedSingleTrainerPrompt(RoutingDecisionRecord row)
        => $"Prompt: {row.Prompt}\nTask slice: {row.TaskSlice}\nCandidate units: {row.ChosenUnitId}, {string.Join(", ", row.RejectedCandidates.Select(x => x.UnitId))}";

    private static TrainingDataGovernanceDecision ExpectedSingleGovernance(
        RepositoryOwnedDpoDatasetAddress address,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        RoutingDecisionRecord row)
        => new(licenseAccepted: true, isQuarantined: false, evidenceRefs:
        [
            "sha256:" + TextSha256("code-documentation-dpo-authoring\n" + address.ManifestRelativePath),
            "sha256:" + TextSha256("paid-reference\n" + BuildPaidReferenceAnswer(row)),
            "sha256:" + TextSha256("request-row\n" + row.RequestId),
            "sha256:" + TextSha256("task-slice\n" + row.TaskSlice),
            "sha256:" + TextSha256("row-ordinal\n0"),
            "sha256:" + identity.RequestIdentitySha256
        ]);

    private static string TextSha256(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static string FileSha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string[] SnapshotSinglePairFiles(RepositoryOwnedDpoDatasetAddress address)
        => Directory.EnumerateFiles(address.DatasetRoot, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .Select(path => $"{Path.GetRelativePath(address.DatasetRoot, path)}|{new FileInfo(path).Length}|{File.GetLastWriteTimeUtc(path).Ticks}|{FileSha256(path)}|{Convert.ToBase64String(File.ReadAllBytes(path))}")
            .ToArray();

    private static void AssertNoSinglePairFiles(RepositoryFixture fixture)
        => Assert.DoesNotContain(Directory.EnumerateFiles(fixture.RepositoryRoot, "*", SearchOption.AllDirectories),
            path => path.EndsWith(".dpo-pair.json", StringComparison.Ordinal) || path.Contains(".manifest.", StringComparison.Ordinal));

    private static void AssertSingleFailure(OperationResult<RepositoryOwnedDpoDatasetResolution> result, string code)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(code, result.ErrorCode);
    }

    private sealed class SinglePairCallbackTeacher(
        CountingTeacherClient inner,
        Func<CancellationToken, Task> callback,
        Func<TrainingTeacherInferenceResult, TrainingTeacherInferenceResult>? transform = null)
        : ITrainingTeacherInferenceClient
    {
        public async Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(string prompt,
            ITrainingTeacherCallPermit callPermit, CancellationToken cancellationToken = default)
        {
            var result = await inner.InferAsync(prompt, callPermit, cancellationToken);
            await callback(cancellationToken);
            return result.IsSuccess && result.Value is not null && transform is not null
                ? OperationResult<TrainingTeacherInferenceResult>.Ok(transform(result.Value))
                : result;
        }

        public void Dispose() => inner.Dispose();
    }

    private static CodeDocumentationTrainingExample[] BuildExamples(int count, string prefix)
        => Enumerable.Range(0, count)
            .Select(index =>
            {
                var request = index % 2 == 0
                    ? BuildNetCoreRequest($"{prefix}-netcore-{index:000}", index)
                    : BuildNetFrameworkRequest($"{prefix}-netfx-{index:000}", index);
                return new CodeDocumentationTrainingExample(
                    $"{prefix}-code-doc-{index:000}",
                    request,
                    OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                    true,
                    true,
                    "The request asks for bottom-up legacy .NET code documentation with version-aware routing.",
                    "the orchestration or translation unit cannot document legacy C# call transit, framework version, or caller/callee summaries.");
            })
            .ToArray();

    private static CodeDocumentationBatchRow[] BuildRows(int count, string prefix)
        => BuildExamples(count, prefix)
            .Select(example => ToRow(example))
            .ToArray();

    private static CodeDocumentationBatchRow ToRow(CodeDocumentationTrainingExample example)
        => new()
        {
            ExampleId = example.ExampleId,
            SourceId = example.Request.SourceId,
            Repository = example.Request.Repository,
            CommitSha = example.Request.CommitSha,
            FilePath = example.Request.FilePath,
            SymbolId = example.Request.SymbolId,
            CodeText = example.Request.CodeText,
            Calls = example.Request.Calls.ToArray(),
            KnownCallees = example.Request.KnownCallees.ToArray(),
            AppSide = example.Request.AppSide,
            RuntimeFamily = example.Request.RuntimeFamily,
            TargetFramework = example.Request.TargetFramework,
            CSharpLanguageVersion = example.Request.CSharpLanguageVersion,
            LanguageVersionConfidence = example.Request.LanguageVersionConfidence,
            ProjectStyle = example.Request.ProjectStyle,
            WebFramework = example.Request.WebFramework,
            WebFrameworkVersion = example.Request.WebFrameworkVersion,
            FrontendFramework = example.Request.FrontendFramework,
            FrontendFrameworkVersion = example.Request.FrontendFrameworkVersion,
            StackLabel = example.Request.StackLabel,
            ArtifactKind = example.Request.ArtifactKind,
            DocumentationLevel = example.Request.DocumentationLevel,
            Chosen = "Route to documentation and preserve bottom-up caller/callee evidence.",
            Rejected = "Route to orchestration and skip code documentation transit. [WRONG: wrong specialist - the rejected route cannot learn framework version or callee-summary propagation - callers would be documented without leaf-method evidence.]",
            ExpectedUnitId = example.ExpectedUnitId,
            ExpectVersionGatePass = example.ExpectVersionGatePass,
            ExpectTransitGatePass = example.ExpectTransitGatePass,
            CorrectReason = example.CorrectReason,
            WrongRouteConsequence = example.WrongRouteConsequence
        };

    private static CodeDocumentationRequest BuildNetCoreRequest(string requestId, int index)
        => new(
            requestId,
            "local-wizGameserver-static-eval",
            @"C:\Projects\wizGameserver",
            "local",
            @"WebApplication2\Controllers\DynamicController.cs",
            $"DynamicController.StoreDocument_{index}",
            "public ActionResult StoreDocument(string index_name, string id, object document) { var myDocument = ParseObjectAlternative(document); return Ok(myDocument); }",
            [
                new CodeDocumentationCallEdge(
                    $"DynamicController.StoreDocument_{index}",
                    "DynamicController.ParseObjectAlternative",
                    "direct_method_call")
            ],
            [
                new CodeDocumentationCalleeSummary(
                    "DynamicController.ParseObjectAlternative",
                    "Recursively normalizes JSON object and array values before the controller indexes the parsed document.",
                    ["DynamicController.cs:204"])
            ],
            "server",
            "NET_CORE",
            "netcoreapp3.1",
            "CSharp8",
            "inferred_high",
            "sdk_style",
            "ASP.NET Core WebApi",
            "Microsoft.AspNetCore.Authentication.JwtBearer 3.1.8",
            "none",
            "none",
            "NETCORE_WEBAPI",
            "controller_action",
            "D3");

    private static CodeDocumentationRequest BuildNetFrameworkRequest(string requestId, int index)
        => new(
            requestId,
            "local-genietasks-static-eval",
            @"C:\Projects\genietasks",
            "local",
            @"A1ProjectManagement\AuthController.cs",
            $"AuthController.Authenticate_{index}",
            "public UserBack Authenticate(LoginDetails user) { string passMD5 = user.password.ConvertToMD5(); return CreateBackUser(authUser); }",
            [
                new CodeDocumentationCallEdge($"AuthController.Authenticate_{index}", "HelpAuthClass.ConvertToMD5", "extension_method_call"),
                new CodeDocumentationCallEdge($"AuthController.Authenticate_{index}", "AuthController.CreateBackUser", "direct_method_call"),
                new CodeDocumentationCallEdge("UserBack.UserBack(Member)", "JwtManager.GenerateToken", "direct_method_call")
            ],
            [
                new CodeDocumentationCalleeSummary(
                    "HelpAuthClass.ConvertToMD5",
                    "Hashes the supplied password string with MD5 and returns a lowercase hexadecimal digest.",
                    ["AuthController.cs:114"]),
                new CodeDocumentationCalleeSummary(
                    "JwtManager.GenerateToken",
                    "Creates a JWT token with legacy claim fields from the UserBack wrapper.",
                    ["JwtManager.cs:12"]),
                new CodeDocumentationCalleeSummary(
                    "AuthController.CreateBackUser",
                    "Creates UserBack from Member and delegates token issuance to the UserBack constructor.",
                    ["AuthController.cs:211"])
            ],
            "mixed",
            "NET_FRAMEWORK",
            "net46",
            "CSharp5_or_CSharp6_review_required",
            "inferred_review_required",
            "legacy_non_sdk",
            "WebApi2 plus ASMX",
            "System.Web.Http 5.2.3 / Breeze.WebApi2 1.6.5",
            "AngularJS",
            "1.4.8",
            "NETFX_WEBAPI",
            "auth_workflow",
            "D3");

    private static CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext
        CreateAuthoringContext(
            Func<OperationResult<ITrainingTeacherInferenceClient>> createTeacher,
            int maxPairsToAuthor,
            CodeDocumentationRepositoryOwnedDpoDomainProfile? domainProfile =
                null)
    {
        var profile = domainProfile ??
                      CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                          .CSharpDomainProfile;
        return new CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext(
            createTeacher,
            CreateOpenTeacherIdentity(),
            new RepositoryOwnedDpoGenerationPolicyIdentity(
                $"{profile.DomainSlug}-local-qwen-dpo-pair-generation",
                "v1",
                new string('c', 64)),
            new RepositoryOwnedDpoCreatorIdentity(
                $"{profile.DomainSlug}-repository-owned-dpo-dataset-authoring",
                "v1",
                new string('d', 64)),
            maxPairsToAuthor,
            domainProfile);
    }

    private static RepositoryOwnedDpoOpenTeacherIdentity CreateOpenTeacherIdentity()
        => new(
            OpenWeightTeacherFamily.Qwen.ToString(),
            "qwen-test:9b",
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            new string('d', 64));

    private static CodeDocumentationRepositoryOwnedDpoRunOptions
        CreateResolverThrowingOptions(
            string repositoryRoot,
            Exception resolverException,
            out Func<int> getFactoryCalls)
    {
        var factoryCalls = 0;
        getFactoryCalls = () => factoryCalls;
        var context = CreateAuthoringContext(
            () =>
            {
                factoryCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "teacher must not be created when the resolver fails");
            },
            maxPairsToAuthor: 1);

        return new CodeDocumentationRepositoryOwnedDpoRunOptions(
            repositoryRoot,
            context,
            "v1",
            rows =>
            {
                _ = rows;
                throw resolverException;
            });
    }

    private static RoutingDecisionRecord CreateReferenceDecision(
        string requestId,
        string codeSurface,
        CodeDocumentationRepositoryOwnedDpoDomainProfile? domainProfile = null)
    {
        var profile = domainProfile ??
                      CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                          .CSharpDomainProfile;
        return new RoutingDecisionRecord(
            $"{profile.DomainSlug}-dpo-test-run",
            requestId,
            $"Document {codeSurface} bottom-up for the owning {profile.DisplayName} source.",
            string.Join(
                " ",
                profile.DomainSlug,
                "code documentation",
                "ILearningUnit",
                profile.RequiredTeacherFocus,
                codeSurface),
            OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
            0.99,
            [
                new UnitScoringResult(
                    OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
                    "orchestration-planning-unit",
                    "orchestration",
                    0.15,
                    0.21)
            ],
            $"Route to the code documentation unit because {codeSurface} needs {profile.RequiredTeacherFocus}.",
            $"The rejected route plans work but does not document {profile.DisplayName} behaviour or required evidence.",
            DateTimeOffset.UnixEpoch);
    }

    private static CodeDocumentationRepositoryOwnedDpoAuthoringSource TeachingSource(
        IReadOnlyList<RoutingDecisionRecord> rows)
        => AssertSuccess(
            CodeDocumentationRepositoryOwnedDpoAuthoringSource.Create(
                rows,
                CodeDocumentationRepositoryOwnedDpoMaterialPurpose
                    .ApprovedTeachingReference,
                "synthetic-approved-teaching",
                closedExamExposed: false));

    private static string BuildPaidReferenceAnswer(RoutingDecisionRecord row)
        => $"Route to {row.ChosenUnitId}. {row.CorrectReason}";

    private static void AssertPaidReferenceTextAbsentFromDataset(
        RepositoryOwnedDpoDatasetAddress address,
        IReadOnlyList<RoutingDecisionRecord> rows)
    {
        var datasetText = string.Join(
            "\n",
            Directory.EnumerateFiles(
                    address.DatasetRoot,
                    "*",
                    SearchOption.AllDirectories)
                .Where(static path =>
                    !string.Equals(
                        Path.GetFileName(path),
                        ".dpo-dataset-seal.lock",
                        StringComparison.Ordinal))
                .Select(File.ReadAllText));
        foreach (var row in rows)
        {
            Assert.DoesNotContain(
                BuildPaidReferenceAnswer(row),
                datasetText,
                StringComparison.Ordinal);
        }
    }

    private static void AssertPaidReferenceTextAbsentFromPairObjects(
        IReadOnlyList<ContinuousLearning.DpoPair> pairs,
        IReadOnlyList<RoutingDecisionRecord> rows)
    {
        foreach (var row in rows)
        {
            var paidReference = BuildPaidReferenceAnswer(row);
            foreach (var pair in pairs)
            {
                Assert.DoesNotContain(
                    paidReference,
                    pair.Prompt,
                    StringComparison.Ordinal);
                Assert.DoesNotContain(
                    paidReference,
                    pair.Chosen,
                    StringComparison.Ordinal);
                Assert.DoesNotContain(
                    paidReference,
                    pair.Rejected,
                    StringComparison.Ordinal);
            }
        }
    }

    private static IReadOnlyDictionary<string, string> SnapshotRegularFiles(
        string root)
        => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(static path =>
                !string.Equals(
                    Path.GetFileName(path),
                    ".dpo-dataset-seal.lock",
                    StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToDictionary(
                path => Path.GetRelativePath(root, path)
                    .Replace(Path.DirectorySeparatorChar, '/'),
                path => Convert.ToHexString(SHA256.HashData(
                        File.ReadAllBytes(path)))
                    .ToLowerInvariant(),
                StringComparer.Ordinal);

    private static void AssertRepositoryOwnedPairsEqual(
        IReadOnlyList<ContinuousLearning.DpoPair> expected,
        IReadOnlyList<ContinuousLearning.DpoPair> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
            Assert.Equal(expected[index], actual[index]);
    }

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private static string ExtractPromptField(string prompt, string key)
    {
        if (prompt.StartsWith('{'))
        {
            using var envelope = JsonDocument.Parse(prompt);
            prompt = envelope.RootElement.GetProperty("authoring_context").GetString()!;
        }
        var prefix = key + "=";
        var line = prompt
            .Split('\n')
            .FirstOrDefault(item =>
                item.StartsWith(prefix, StringComparison.Ordinal));
        return line is null ? "unknown" : line[prefix.Length..].Trim();
    }

    private sealed class CountingTeacherClient(
        RepositoryOwnedDpoOpenTeacherIdentity teacher,
        Func<string, string> outputFactory,
        bool consumePermit = true)
        : ITrainingTeacherInferenceClient
    {
        public int InferenceCalls { get; private set; }
        public List<string> Prompts { get; } = [];

        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = consumePermit ? callPermit.TryIssue() : OperationResult<int>.Ok(0);
            if (!issued.IsSuccess)
            {
                return Task.FromResult(
                    issued.MapError<TrainingTeacherInferenceResult>());
            }

            InferenceCalls++;
            Prompts.Add(prompt);
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                teacher.ModelId,
                teacher.ModelVersion,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                "csharp-code-documentation-dpo-test-" +
                ExtractPromptField(prompt, "request_id"),
                InvocationCount: 1)
            {
                ModelArtifactDigestKind = teacher.ModelArtifactDigestKind,
                ModelArtifactDigest = teacher.ModelArtifactSha256,
            };
            return Task.FromResult(
                OperationResult<TrainingTeacherInferenceResult>.Ok(
                    new TrainingTeacherInferenceResult(
                        outputFactory(prompt),
                        provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority
                                .Authoring
                    }));
        }

        public void Dispose()
        {
        }
    }

    private sealed class RepositoryFixture : IDisposable
    {
        public RepositoryFixture()
        {
            RepositoryRoot = Path.Combine(
                Path.GetTempPath(),
                "llm-mvp-csharp-code-doc-dpo",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RepositoryRoot);
            Directory.CreateDirectory(
                Path.Combine(RepositoryRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(RepositoryRoot, "11.DPO"));
            File.WriteAllText(
                Path.Combine(RepositoryRoot, "LlmFromScratch.sln"),
                string.Empty,
                Encoding.UTF8);
        }

        public string RepositoryRoot { get; }

        public void Dispose()
        {
            if (Directory.Exists(RepositoryRoot))
                Directory.Delete(RepositoryRoot, recursive: true);
        }
    }
}
