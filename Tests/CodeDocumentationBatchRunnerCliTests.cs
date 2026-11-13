// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using AgentInfrastructure;
using CodeDocBatchRunner;
using ContinuousLearning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ModelServer;
using TrainableRouter;
using TrainableRouter.Training;
using ClDpoPair = ContinuousLearning.DpoPair;

public sealed class CodeDocumentationBatchRunnerCliTests
{
    [Fact]
    public async Task OneLessonMappedRoute_FreshSyntheticSelectionRefusesWithoutEffects()
    {
        using var h = CodeDocumentationPreparedBaselineTests.SelectedMaterialHarness(); var fixture = h.Fixture;
        var original = await h.Consume(); Assert.True(original.IsSuccess, original.ErrorMessage); Assert.Equal(10, h.Client.Calls);
        var admission = CodeDocumentationPreparedBaselineTests.SelectedMaterialAdmission(h, original.Value!); var item = admission.Item;
        Assert.NotEqual(h.Inspect(original.Value!).Value!.TeachingIdentities.Items[0].DeclaredItem.ItemId, item.ItemId);
        var calls = 0; var lookups = 0; var captures = 0;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((_, _) =>
        {
            calls++;
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(new(
                item.ItemId, item.SourceId, item.ReferenceEntryId, item.SourceEntityFingerprint, "Synthetic explicit second eligible item.", admission.EvidenceSha256)));
        });
        h.ReleaseSession(); using var environment = fixture.ApplyEnvironment();
        await using (var warm = Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments, fixture.ReadEnvironment, fixture.RepositoryRoot))
        { await warm.StartAsync(); await warm.StopAsync(); }
        var files = fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        var factory = new SelectionEvaluationFactory(false);
        await using (var app = Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments, fixture.ReadEnvironment, fixture.RepositoryRoot))
        {
            app.Use(async (context, next) =>
            {
                var services = context.RequestServices;
                var material = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting;
                var student = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting;
                var teacher = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting;
                context.RequestServices = new SelectionHostServices(services, factory, () => { lookups++; return provider; }, () => admission);
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = _ => captures++;
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting = _ => captures++;
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = _ => captures++;
                try { await next(context); }
                finally
                {
                    context.RequestServices = services;
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = material;
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting = student;
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = teacher;
                }
            });
            await app.StartAsync();
            try
            {
                using var http = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(app) };
                using var response = await http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
                    new StringContent(JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(fixture.OrdinarySelector)), Encoding.UTF8, "application/json"));
                using var json = JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync());
                Assert.Equal(409, (int)response.StatusCode); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, json.RootElement.GetProperty("errorCode").GetString());
                Assert.Equal(1, lookups); Assert.Equal(0, calls); Assert.Equal(0, captures); Assert.Equal(1, factory.Creates);
                Assert.Equal(0, Assert.Single(factory.Clients).Calls); Assert.Equal(10, h.Client.Calls);
                Assert.False(File.Exists(fixture.OrdinaryHistory)); Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            }
            finally { await app.StopAsync(); }
        }
        var after = fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        Assert.Equal(files.Keys.Order(), after.Keys.Order()); foreach (var p in files.Keys) { Assert.Equal(files[p].Bytes, after[p].Bytes); Assert.Equal(files[p].Ticks, after[p].Ticks); }
        using var control = new CodeDocumentationPreparedBaselineTests.OrdinaryDiagnosticHarness((_, _) => (false, "missing_essential_behavior"), existingFixture: fixture);
        var restored = await control.Consume(); Assert.True(restored.IsSuccess, restored.ErrorMessage); Assert.Equal(0, control.Client.Calls);
        var witness = control.Inspect(restored.Value!); Assert.True(witness.IsSuccess, witness.ErrorMessage);
        var bound = await control.Open.Session.BindOrdinarySelectionDecisionSyntheticForTestingAsync(restored.Value!, control.Catalog, fixture.OrdinarySelector, witness.Value!, provider);
        Assert.True(bound.IsSuccess, bound.ErrorMessage); Assert.Equal(1, calls); Assert.Equal(item.ItemId, bound.Value!.Payload.ItemId);
        CodeDocumentationPreparedBaselineTests.AssertSyntheticSelectionHistory(control.HistoryBytes(), item.ItemId);
    }

    [Fact]
    public async Task OneLessonMappedRoute_SelectedStudentModeRemainsPrivateAndUnavailable()
    {
        // The shared normal Program control proves truthful partial 503 and fresh-host saved replay with a throwing provider-resolution canary.
        await OneLessonMappedRoute_SelectedMaterialReachesRecipientAndReplaySkipsOperator();
        using var fixture=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(10000,2);
        fixture.ProvisionOrdinaryV2(5,genuineTeachingCurriculum:CodeDocumentationPreparedBaselineTests.SelectionCurriculum(fixture));
        var captures=0;var old=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting;
        using var environment=fixture.ApplyEnvironment();
        await using var app=Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments,fixture.ReadEnvironment,fixture.RepositoryRoot);
        Assert.Null(app.Services.GetService<CodeDocumentationRepositoryOwnedPreparedBaselineJournal.SyntheticSelectedTaskStudentOperation>());
        app.Use(async(context,next)=>
        {
            var prior=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=_=>captures++;
            try{await next(context);}finally{CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=prior;}
        });
        await app.StartAsync();
        try
        {
            using var http=new HttpClient{BaseAddress=Program.ResolveStartedLoopbackAddress(app)};
            var selector=JsonSerializer.Serialize(fixture.OrdinarySelector);
            foreach(var field in new[]{"syntheticSelectedStudentMode","studentResponse","studentResponseLease"})
            {
                using var response=await http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
                    new StringContent("{\"selector\":"+selector+",\""+field+"\":\"not-an-effect-permit\"}",Encoding.UTF8,"application/json"));
                Assert.Equal(400,(int)response.StatusCode);
            }
            Assert.Equal(0,captures);Assert.False(File.Exists(fixture.OrdinaryHistory));Assert.False(File.Exists(fixture.OrdinaryBaseline));
            Assert.Equal(0,app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
        }
        finally{await app.StopAsync();CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting=old;}
    }

    [Fact]
    public async Task OneLessonMappedRoute_SelectedMaterialReachesRecipientAndReplaySkipsOperator()
    {
        using var synthetic = CodeDocumentationPreparedBaselineTests.SelectedMaterialHarness();
        var fixture = synthetic.Fixture;
        var grant = fixture.OrdinaryGrant;
        var graph = grant.SourceSelection!.OpenDeclaredTeachingCorpusIdentities(fixture.RepositoryRoot).Value!;
        var selected = graph.Items.Single(i => i.DeclaredItem.ItemId == "item-teach-7-1"); Assert.NotEqual(graph.Items[0], selected);
        var binding = new CodeDocumentationOneLessonSessionBinding(grant.ObjectiveId, grant.GrantId, grant.GrantVersion, grant.TrainingRunId,
            grant.CandidateSha256, grant.ParentManifestSha256, grant.ParentWeightsSha256, grant.ConfigurationSha256, grant.ExamConditionSha256)
            { FormatVersion = 2, DescriptorsV2 = grant.DescriptorsV2 };
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission? admission = null;
        CodeDocumentationTeachingSelectionInputV1? savedInput = null; var providerCalls = 0;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input, _) =>
        {
            providerCalls++; savedInput = input;
            var issued = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(graph, input,
                CodeDocumentationOneLessonSession.ComputeBindingSha256(binding), selected.DeclaredItem.ItemId,
                CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference,
                new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + new string('b', 64) }));
            Assert.True(issued.IsSuccess, issued.ErrorMessage); admission = issued.Value!;
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(new(
                selected.DeclaredItem.ItemId, selected.DeclaredItem.SourceId, selected.Entry.EntryId, selected.Entry.SourceEntityFingerprint,
                "Synthetic explicit non-first semantic selection.", admission.EvidenceSha256)));
        });
        using var environment = fixture.ApplyEnvironment();
        var captures = new List<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>();
        var consumed = await synthetic.Consume(); Assert.True(consumed.IsSuccess, consumed.ErrorMessage); var syntheticBaseline = consumed.Value!;
        var witness = synthetic.Inspect(syntheticBaseline); Assert.True(witness.IsSuccess, witness.ErrorMessage);
        var seeded = await synthetic.Open.Session.BindOrdinarySelectionDecisionSyntheticForTestingAsync(syntheticBaseline, synthetic.Catalog,
            fixture.OrdinarySelector, witness.Value!, provider);
        Assert.True(seeded.IsSuccess, seeded.ErrorMessage); Assert.Equal(1, providerCalls); Assert.Equal(10, synthetic.Client.Calls);
        CodeDocumentationPreparedBaselineTests.AssertSyntheticSelectionHistory(synthetic.HistoryBytes(), selected.DeclaredItem.ItemId); Assert.Equal(selected.DeclaredItem.ItemId, seeded.Value!.Payload.ItemId);
        Assert.NotNull(savedInput); Assert.NotNull(admission); Assert.True(admission.MatchesSelectionInput(CodeDocumentationOneLessonSession.ComputeBindingSha256(binding), savedInput, selected.DeclaredItem).IsSuccess);
        var syntheticAdmission = admission!;
        var syntheticControl = CodeDocumentationPreparedBaselineTests.ConsumeSelectedMaterial(synthetic, syntheticBaseline, syntheticAdmission);
        Assert.True(syntheticControl.IsSuccess, syntheticControl.ErrorCode);
        var first = syntheticControl.Value!; Assert.Equal("item-teach-7-1", first.Item.ItemId); Assert.Equal("SyntheticTest", first.Origin);
        Assert.Equal("Полная задача 中文\nsecond line\toriginal-exclusion-7-0", first.OwnerInput);
        Assert.Equal("Полный reference 中文\nfinal line\toriginal-exclusion-7-0", first.PaidModelReferenceAnswer);
        synthetic.ReleaseSession();
        // Create/release real adapter custody and readiness without a HTTP seed or provider resolution.
        await using (var warm = Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments, fixture.ReadEnvironment, fixture.RepositoryRoot))
        { await warm.StartAsync(); await warm.StopAsync(); }
        var files = fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        await RunHost(true, () => null, 503, "CODE_DOCUMENTATION_ONE_LESSON_SELECTION_BOUND_CONTINUATION_UNAVAILABLE", 0, 0);
        Assert.Empty(captures);
        await RunHost(true, () => admission, 409, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, 0, 0);
        Assert.Empty(captures);
        Assert.True(synthetic.Reopen().IsSuccess); var reopenedBaseline = await synthetic.Consume(); Assert.True(reopenedBaseline.IsSuccess, reopenedBaseline.ErrorMessage);
        var syntheticReplay = CodeDocumentationPreparedBaselineTests.ConsumeSelectedMaterial(synthetic, reopenedBaseline.Value!, syntheticAdmission);
        Assert.True(syntheticReplay.IsSuccess, syntheticReplay.ErrorCode);
        Assert.Equal(first.CopyOwnerInputUtf8(), syntheticReplay.Value!.CopyOwnerInputUtf8());
        Assert.Equal(first.CopyPaidReferenceUtf8(), syntheticReplay.Value.CopyPaidReferenceUtf8());
        synthetic.ReleaseSession();
        var foreign = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(graph, savedInput!,
            new string('c', 64), selected.DeclaredItem.ItemId, CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference, admission!.Governance).Value!;
        await RunHost(true, () => foreign, 409, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, 0, 0);
        Assert.Empty(captures); Assert.Equal(1, providerCalls);
        var after = fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        Assert.Equal(files.Keys.Order(), after.Keys.Order()); foreach (var path in files.Keys) { Assert.Equal(files[path].Bytes, after[path].Bytes); Assert.Equal(files[path].Ticks, after[path].Ticks); }
        async Task RunHost(bool replay, Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission?> material,
            int status, string code, int evaluationCalls, int operatorResolutions)
        {
            var factory = new SelectionEvaluationFactory(false); var resolutions = 0;
            await using var app = Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments, fixture.ReadEnvironment, fixture.RepositoryRoot);
            app.Use(async (context, next) =>
            {
                var services = context.RequestServices; var observer = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting;
                context.RequestServices = new SelectionHostServices(services, factory, () =>
                { resolutions++; if (replay) throw new InvalidOperationException("Saved material replay must not resolve the operator"); return provider; }, material);
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = captures.Add;
                try { await next(context); }
                finally { context.RequestServices = services; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = observer; }
            });
            await app.StartAsync();
            try
            {
                using var http = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(app) };
                using var response = await http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
                    new StringContent(JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(fixture.OrdinarySelector)), Encoding.UTF8, "application/json"));
                var bytes = await response.Content.ReadAsByteArrayAsync(); using var json = JsonDocument.Parse(bytes);
                Assert.Equal(status, (int)response.StatusCode); Assert.False(json.RootElement.GetProperty("isSuccess").GetBoolean());
                Assert.Equal(code, json.RootElement.GetProperty("errorCode").GetString());
                foreach (var field in new[] { "ownerInput", "paidModelReferenceAnswer", "materialAdmissionEvidenceSha256", "receipt", "decision" }) Assert.False(json.RootElement.TryGetProperty(field, out _));
                var text = Encoding.UTF8.GetString(bytes); Assert.DoesNotContain("original-exclusion", text, StringComparison.Ordinal);
                Assert.Equal(operatorResolutions, resolutions); Assert.Equal(1, factory.Creates); Assert.Equal(evaluationCalls, Assert.Single(factory.Clients).Calls);
                Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            }
            finally { await app.StopAsync(); }
        }
    }

    [Fact]
    public async Task OneLessonMappedRoute_SelectionBindingUsesHostIssuerAndReplaySkipsProvider()
    {
        using var fixture=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(10000,2);
        fixture.ProvisionOrdinaryV2(5,genuineTeachingCurriculum:CodeDocumentationPreparedBaselineTests.SelectionCurriculum(fixture));
        var graph=fixture.OrdinaryGrant.SourceSelection!.OpenDeclaredTeachingCorpusIdentities(fixture.RepositoryRoot).Value!;
        var selected=graph.Items.Single(i=>i.DeclaredItem.ItemId==graph.CategoryBindings.Single(c=>c.Category=="missing_essential_behavior").OrderedItemIds[0]);
        var providerCalls=0;
        var provider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
        {
            providerCalls++;
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(new(
                selected.DeclaredItem.ItemId,selected.DeclaredItem.SourceId,selected.Entry.EntryId,selected.Entry.SourceEntityFingerprint,
                "Synthetic fixture selected the declared teaching link.",new string('a',64))));
        });
        using var environment=fixture.ApplyEnvironment();
        await RunHost(null,false,false,async(app,factory,resolution)=>
        {
            await Failure(app,503,"CODE_DOCUMENTATION_ONE_LESSON_SELECTION_OPERATOR_UNAVAILABLE");
            Assert.Equal(1,resolution()); Assert.Equal(1,factory.Creates); Assert.Equal(10,Assert.Single(factory.Clients).Calls);
            Assert.False(File.Exists(fixture.OrdinaryHistory)); Assert.Equal(0,providerCalls);
        });
        await RunHost(provider,false,false,async(app,factory,resolution)=>
        {
            await Failure(app,409,CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode);
            Assert.Equal(1,resolution()); Assert.Equal(0,providerCalls); Assert.Equal(0,Assert.Single(factory.Clients).Calls);
            Assert.False(File.Exists(fixture.OrdinaryHistory));
        });
        using (var seedHarness = new CodeDocumentationPreparedBaselineTests.OrdinaryDiagnosticHarness(
            (_, _) => (false, "missing_essential_behavior"), existingFixture: fixture))
        {
            var original = await seedHarness.Consume(); Assert.True(original.IsSuccess, original.ErrorMessage); Assert.Equal(0, seedHarness.Client.Calls);
            var witness = seedHarness.Inspect(original.Value!); Assert.True(witness.IsSuccess, witness.ErrorMessage);
            var bound = await seedHarness.Open.Session.BindOrdinarySelectionDecisionSyntheticForTestingAsync(original.Value!, seedHarness.Catalog,
                fixture.OrdinarySelector, witness.Value!, provider);
            Assert.True(bound.IsSuccess, bound.ErrorMessage); Assert.Equal(1, providerCalls); CodeDocumentationPreparedBaselineTests.AssertSyntheticSelectionHistory(seedHarness.HistoryBytes(), selected.DeclaredItem.ItemId);
            Assert.Equal(selected.DeclaredItem.ItemId, bound.Value!.Payload.ItemId);
            seedHarness.ReleaseSession();
        }
        var history=File.ReadAllBytes(fixture.OrdinaryHistory); var baseline=File.ReadAllBytes(fixture.OrdinaryBaseline);
        var baselineTicks=File.GetLastWriteTimeUtc(fixture.OrdinaryBaseline);
        await RunHost(null,true,false,async(app,factory,resolution)=>
        {
            await Failure(app,503,"CODE_DOCUMENTATION_ONE_LESSON_SELECTION_BOUND_CONTINUATION_UNAVAILABLE");
            Assert.Equal(0,resolution()); Assert.Equal(1,factory.Creates); Assert.Equal(0,Assert.Single(factory.Clients).Calls);
            using var http=new HttpClient {BaseAddress=Program.ResolveStartedLoopbackAddress(app)};
            foreach(var field in new[]{"decision","model","launchAck"})
            {
                var json=JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(fixture.OrdinarySelector));
                using var response=await http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
                    new StringContent(json[..^1]+",\""+field+"\":\"untrusted\"}",Encoding.UTF8,"application/json"));
                Assert.Equal(400,(int)response.StatusCode);
            }
            File.Delete(fixture.OrdinaryBaseline);
            try
            {
                await Failure(app,409,CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode);
                Assert.False(File.Exists(fixture.OrdinaryBaseline)); Assert.Equal(1,factory.Creates); Assert.Equal(0,resolution());
                Assert.Equal(0,Assert.Single(factory.Clients).Calls);
                Assert.Equal(history,File.ReadAllBytes(fixture.OrdinaryHistory));
            }
            finally {CodeDocumentationPreparedBaselineTests.RestoreCustodyFile(fixture.OrdinaryBaseline,baseline,baselineTicks);}
            foreach(var path in new[]{fixture.OrdinaryBaseline,fixture.OrdinaryClaim})
            {
                var saved=File.ReadAllBytes(path); var ticks=File.GetLastWriteTimeUtc(path);
                try
                {
                    File.WriteAllBytes(path,saved.Concat(new byte[]{(byte)' '}).ToArray());
                    await Failure(app,409,CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode);
                    Assert.Equal(1,factory.Creates); Assert.Equal(0,resolution());
                    Assert.Equal(0,Assert.Single(factory.Clients).Calls); Assert.Equal(history,File.ReadAllBytes(fixture.OrdinaryHistory));
                }
                finally {CodeDocumentationPreparedBaselineTests.RestoreCustodyFile(path,saved,ticks);}
            }
        });
        Assert.Equal(1,providerCalls); Assert.Equal(history,File.ReadAllBytes(fixture.OrdinaryHistory));

        using var sufficient=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(10000,2);
        sufficient.ProvisionOrdinaryV2(5,genuineTeachingCurriculum:sufficient.SealGenuineTeaching());
        using(var sufficientEnvironment=sufficient.ApplyEnvironment())
        {
            await RunSufficientHost(sufficient,false);
            await RunSufficientHost(sufficient,true);
        }

        async Task Failure(WebApplication app,int expectedStatus,string expectedCode)
        {
            using var http=new HttpClient {BaseAddress=Program.ResolveStartedLoopbackAddress(app)};
            using var response=await http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
                new StringContent(JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(fixture.OrdinarySelector)),Encoding.UTF8,"application/json"));
            using var json=JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync()); var root=json.RootElement;
            Assert.Equal(expectedStatus,(int)response.StatusCode); Assert.False(root.GetProperty("isSuccess").GetBoolean());
            Assert.Equal(expectedCode,root.GetProperty("errorCode").GetString());
            Assert.False(root.TryGetProperty("decision",out _)); Assert.False(root.TryGetProperty("origin",out _));
            Assert.Equal(0,app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
        }
        async Task RunHost(CodeDocumentationOneLessonSelectionDecisionProvider? installed,bool throwOnLookup,bool pass,
            Func<WebApplication,SelectionEvaluationFactory,Func<int>,Task> assertion)
        {
            var factory=new SelectionEvaluationFactory(pass); var resolutions=0;
            await using var app=Program.CreateLocalLoopbackHostCore(fixture.ReadyFileArguments,fixture.ReadEnvironment,fixture.RepositoryRoot);
            app.Use(async(context,next)=>
            {
                var previous=context.RequestServices;
                context.RequestServices=new SelectionHostServices(previous,factory,()=>
                {resolutions++; if(throwOnLookup) throw new InvalidOperationException("Replay must not resolve operator"); return installed;});
                try {await next(context);} finally {context.RequestServices=previous;}
            });
            await app.StartAsync(); try {await assertion(app,factory,()=>resolutions);} finally {await app.StopAsync();}
        }
        async Task RunSufficientHost(CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture f,bool replay)
        {
            var factory=new SelectionEvaluationFactory(true); var resolutions=0;
            await using var app=Program.CreateLocalLoopbackHostCore(f.ReadyFileArguments,f.ReadEnvironment,f.RepositoryRoot);
            app.Use(async(context,next)=>
            {
                var previous=context.RequestServices; context.RequestServices=new SelectionHostServices(previous,factory,()=>
                {resolutions++;throw new InvalidOperationException("Sufficient route must not resolve operator");});
                try {await next(context);} finally {context.RequestServices=previous;}
            });
            await app.StartAsync();
            try
            {
                using var http=new HttpClient {BaseAddress=Program.ResolveStartedLoopbackAddress(app)};
                using var response=await http.PostAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath,
                    new StringContent(JsonSerializer.Serialize(new CodeDocumentationOneLessonPreflightRequestV1(f.OrdinarySelector)),Encoding.UTF8,"application/json"));
                using var json=JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync());
                Assert.Equal(200,(int)response.StatusCode); Assert.Equal(replay?"TerminalReplay":"Completed",json.RootElement.GetProperty("disposition").GetString());
                Assert.Equal(0,resolutions); Assert.Equal(replay?0:1,factory.Creates);
            }
            finally {await app.StopAsync();}
        }
    }

    [Fact]
    public async Task OneLessonMappedRoute_FreshCompleteAdmissionSetReachesFullRecipientGuard()
    {
        using var h = CodeDocumentationPreparedBaselineTests.FreshFullMaterialHarness();
        var b = (await h.Consume()).Value!; var a = CodeDocumentationPreparedBaselineTests.SelectionMaterialAdmissions(h, b);
        Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); h.ReleaseSession();
        var f = h.Fixture; using var environment = f.ApplyEnvironment(); var factory = new SelectionEvaluationFactory(false);
        var collections = 0; var singles = 0; var calls = 0; var lookups = 0; var raw = 0; var captures = 0;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((_, _) =>
        { calls++; throw new InvalidOperationException("Fresh ordinary mapped route cannot dispatch synthetic material."); });
        WebApplication CreateHost()
        {
            var app = Program.CreateLocalLoopbackHostCore(f.ReadyFileArguments, f.ReadEnvironment, f.RepositoryRoot);
            app.Use(async (context, next) =>
            {
                var previous = context.RequestServices;
                context.RequestServices = new SelectionHostServices(previous, factory, () => { lookups++; return provider; },
                    () => { singles++; return null; }, () => { collections++; return a; });
                try { await next(context); } finally { context.RequestServices = previous; }
            });
            return app;
        }
        await using (var warm = CreateHost()) { await warm.StartAsync(); await warm.StopAsync(); }
        var files = f.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        var effects = new OneLessonPreflightSideEffects(); var output = new List<string>(); var client = new CodeDocumentationOneLessonPreflightClient();
        var previousRead = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var previousCapture = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = point => { if (point == "raw-before-open") raw++; };
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
            await using var app = CreateHost(); await app.StartAsync();
            try
            {
                var dependencies = CreateOneLessonPreflightDependencies(effects, async (uri, selector, timeout, token) =>
                {
                    var result = await client.PreflightAsync(uri, selector, timeout, token);
                    Assert.False(result.IsSuccess); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, result.ErrorCode);
                    return result;
                }, output.Add);
                var exit = await CodeDocumentationBatchRunnerCli.RunAsync(new[] { "--one-lesson-objective", f.OrdinarySelector.ObjectiveId,
                    "--one-lesson-grant", f.OrdinarySelector.GrantSelector, "--model-server-base-uri", Program.ResolveStartedLoopbackAddress(app).ToString() }, dependencies);
                Assert.Equal(1, exit); Assert.Equal(1, effects.PreflightCalls); Assert.Empty(output);
                Assert.Contains(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, Assert.Single(effects.Errors), StringComparison.Ordinal);
                Assert.Equal(0, app.Services.GetRequiredService<CodeDocumentationTrainingRunner>().SelectedProfileTrainCalls);
            }
            finally { await app.StopAsync(); }
        }
        finally
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = previousRead;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previousCapture;
        }
        Assert.Equal(1, collections); Assert.Equal(0, singles); Assert.Equal(1, lookups); Assert.Equal(0, calls);
        Assert.Equal(1, factory.Creates); Assert.Equal(0, Assert.Single(factory.Clients).Calls); Assert.Equal(10, h.Client.Calls);
        Assert.Equal(0, raw); Assert.Equal(0, captures); Assert.False(File.Exists(f.OrdinaryHistory));
        Assert.Equal(0, effects.EnvironmentReads); Assert.Equal(0, effects.FileChecks); Assert.Equal(0, effects.RunRootCreations); Assert.Equal(0, effects.FileWrites);
        Assert.Equal(0, effects.ConfigurationResolutions); Assert.Equal(0, effects.TeacherClients); Assert.Equal(0, effects.RunBatchCalls); Assert.Equal(0, effects.PublicationCalls);
        var after = f.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        Assert.Equal(files.Keys.Order(), after.Keys.Order());
        foreach (var path in files.Keys) { Assert.Equal(files[path].Bytes, after[path].Bytes); Assert.Equal(files[path].Ticks, after[path].Ticks); }
    }

    private sealed class SelectionHostServices(IServiceProvider inner,
        ICodeDocumentationSavedModelAssessmentEvaluationClientFactory factory,
        Func<CodeDocumentationOneLessonSelectionDecisionProvider?> provider,
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission?>? material = null,
        Func<IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>>? materials = null) : IServiceProvider
    {
        public object? GetService(Type type)=>type==typeof(IServiceProvider)?this:
            type==typeof(ICodeDocumentationSavedModelAssessmentEvaluationClientFactory)?factory:
            type==typeof(CodeDocumentationOneLessonSelectionDecisionProvider)?provider():
            type==typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission)&&material is not null?material():
            type==typeof(IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>)&&materials is not null?materials():inner.GetService(type);
    }
    private sealed class SelectionEvaluationFactory(bool pass) : ICodeDocumentationSavedModelAssessmentEvaluationClientFactory
    {
        internal int Creates; internal List<CodeDocumentationPreparedBaselineTests.DiagnosticClient> Clients {get;}=[];
        public OperationResult<ILegalTeacherEvaluationInferenceClient> Create(ContinuousLearningTp1DpoConfiguration configuration)
        {
            Creates++; var client=new CodeDocumentationPreparedBaselineTests.DiagnosticClient(configuration,
                (_,_) => (pass,pass?"correct":"missing_essential_behavior")); Clients.Add(client);
            return OperationResult<ILegalTeacherEvaluationInferenceClient>.Ok(client);
        }
    }

    [Theory]
    [InlineData(false, "TERMINAL_NEGATIVE_PARENT_RETAINED")]
    [InlineData(true, "TERMINAL_PROMOTED")]
    public async Task OneLessonTerminalReplay_UnverifiedPreseededFrameFailsClosedBeforeRunnerEffects(
        bool includePublication,
        string seededTerminalState)
    {
        using var fixture = new UnverifiedTerminalFrameFixture(includePublication);
        Assert.Equal(seededTerminalState, fixture.TerminalOutcome.TerminalState.ToString());
        var grantJson = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1,
            Grants = new[] { fixture.Grant }
        });
        var startup = ContinuousLearningModelServerStartup.Create(name =>
            StringComparer.Ordinal.Equals(
                name,
                CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable)
                ? grantJson
                : null);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(startup);
        await using var app = builder.Build();
        app.MapCodeDocumentationOneLessonPreflight(fixture.Repository);
        await app.StartAsync();

        var baseUri = ModelServer.Program.ResolveStartedLoopbackAddress(app);
        var effects = new OneLessonPreflightSideEffects();
        var output = new List<string>();
        var responses = new List<CodeDocumentationOneLessonPreflightResponseV1>();
        var client = new CodeDocumentationOneLessonPreflightClient();
        var dependencies = CreateOneLessonPreflightDependencies(
            effects,
            (uri, selector, timeout, cancellationToken) =>
                ReadAndCaptureAsync(uri, selector, timeout, cancellationToken),
            output.Add);

        async Task<OperationResult<CodeDocumentationOneLessonPreflightResponseV1>>
            ReadAndCaptureAsync(
                Uri uri,
                CodeDocumentationOneLessonSelectorV1 selector,
                TimeSpan timeout,
                CancellationToken cancellationToken)
        {
            var result = await client.PreflightAsync(
                uri,
                selector,
                timeout,
                cancellationToken);
            if (result.IsSuccess && result.Value is not null)
                responses.Add(result.Value);
            return result;
        }

        var args = new[]
        {
            "--one-lesson-objective", fixture.Binding.ObjectiveId,
            "--one-lesson-grant", fixture.Grant.GrantSelector,
            "--model-server-base-uri", baseUri.ToString()
        };
        var historyBefore = File.ReadAllBytes(fixture.HistoryPath);
        var firstExit = await CodeDocumentationBatchRunnerCli.RunAsync(
            args,
            dependencies);
        var secondExit = await CodeDocumentationBatchRunnerCli.RunAsync(
            args,
            dependencies);

        Assert.Equal(1, firstExit);
        Assert.Equal(1, secondExit);
        Assert.Equal(2, effects.PreflightCalls);
        Assert.Empty(responses);
        Assert.Equal(2, effects.Errors.Count);
        Assert.All(effects.Errors, error =>
            Assert.Contains("OBJECTIVE_RECOVERY_REQUIRED", error, StringComparison.Ordinal));
        Assert.Empty(output);
        Assert.Equal(historyBefore, File.ReadAllBytes(fixture.HistoryPath));
        Assert.Equal(0, effects.EnvironmentReads);
        Assert.Equal(0, effects.FileChecks);
        Assert.Equal(0, effects.RunRootCreations);
        Assert.Equal(0, effects.FileWrites);
        Assert.Equal(0, effects.ConfigurationResolutions);
        Assert.Equal(0, effects.TeacherClients);
        Assert.Equal(0, effects.RunBatchCalls);
        Assert.Equal(0, effects.PublicationCalls);

        await app.StopAsync();
    }

    [Fact]
    public async Task OneLessonPreflight_ChangedAuthorizedBindingConflictsThroughCliBeforeEffects()
    {
        using var fixture = new UnverifiedTerminalFrameFixture(includePublication: false);
        var changedGrant = fixture.Grant with
        {
            CandidateSha256 = new string('9', 64)
        };
        var grantJson = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1,
            Grants = new[] { changedGrant }
        });
        var startup = ContinuousLearningModelServerStartup.Create(name =>
            StringComparer.Ordinal.Equals(
                name,
                CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable)
                ? grantJson
                : null);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(startup);
        await using var app = builder.Build();
        app.MapCodeDocumentationOneLessonPreflight(fixture.Repository);
        await app.StartAsync();

        var baseUri = ModelServer.Program.ResolveStartedLoopbackAddress(app);
        var effects = new OneLessonPreflightSideEffects();
        var client = new CodeDocumentationOneLessonPreflightClient();
        var dependencies = CreateOneLessonPreflightDependencies(
            effects,
            (uri, selector, timeout, cancellationToken) =>
                client.PreflightAsync(uri, selector, timeout, cancellationToken));
        var historyBefore = File.ReadAllBytes(fixture.HistoryPath);
        var args = new[]
        {
            "--one-lesson-objective", fixture.Binding.ObjectiveId,
            "--one-lesson-grant", fixture.Grant.GrantSelector,
            "--model-server-base-uri", baseUri.ToString()
        };

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(args, dependencies);

        Assert.Equal(1, exitCode);
        Assert.Single(effects.Errors);
        Assert.Contains("OBJECTIVE_BINDING_CONFLICT", effects.Errors[0], StringComparison.Ordinal);
        Assert.Equal(historyBefore, File.ReadAllBytes(fixture.HistoryPath));
        Assert.Equal(1, effects.PreflightCalls);
        Assert.Equal(0, effects.EnvironmentReads);
        Assert.Equal(0, effects.FileChecks);
        Assert.Equal(0, effects.RunRootCreations);
        Assert.Equal(0, effects.FileWrites);
        Assert.Equal(0, effects.ConfigurationResolutions);
        Assert.Equal(0, effects.TeacherClients);
        Assert.Equal(0, effects.RunBatchCalls);
        Assert.Equal(0, effects.PublicationCalls);

        await app.StopAsync();
    }

    [Fact]
    public async Task OneLessonPreflight_IdenticalSelectorReopensThroughMappedLoopbackRouteWithoutEffects()
    {
        using var fixture = new OneLessonPreflightFixture();
        var grantJson = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1,
            Grants = new[] { fixture.Grant }
        });
        var startup = ContinuousLearningModelServerStartup.Create(name =>
            StringComparer.Ordinal.Equals(
                name,
                CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable)
                ? grantJson
                : null);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(startup);
        await using var app = builder.Build();
        var routeDiagnostics = new List<string>();

        string SanitizeExceptionMessage(string message)
        {
            var sanitized = message
                .Replace(fixture.Repository, "<fixture-repository>", StringComparison.OrdinalIgnoreCase)
                .Replace(fixture.Grant.GrantSelector, "<grant-selector>", StringComparison.Ordinal);
            sanitized = Regex.Replace(
                sanitized,
                @"(?i)[""']?\b(authorization|token|secret|password|api[_-]?key|client[_-]?secret|access[_-]?token|refresh[_-]?token)\b[""']?\s*[:=]\s*[""']?(?:bearer\s+)?[^,\s;""']+",
                "$1=<redacted>");
            sanitized = Regex.Replace(
                sanitized,
                @"(?i)\b(bearer)\s+[A-Za-z0-9._~+/=-]+",
                "$1 <redacted>");
            sanitized = Regex.Replace(
                sanitized,
                @"(?i)\b(?:https?|file)://[^\s""'<>]+",
                "<uri>");
            sanitized = Regex.Replace(
                sanitized,
                @"(?i)(?:[a-z]:\\|\\\\)[^\s""'<>]+",
                "<path>");
            sanitized = Regex.Replace(
                sanitized,
                @"(?<![A-Za-z0-9])/(?:[A-Za-z0-9._-]+/)*[A-Za-z0-9._-]+",
                "<path>");
            sanitized = Regex.Replace(
                sanitized,
                @"\?[^\s""'<>]*",
                "?<redacted>");
            sanitized = Regex.Replace(
                sanitized,
                @"(?i)\b(body|payload|content)\s*[:=]\s*[^;]+",
                "$1=<redacted>");
            sanitized = Regex.Replace(
                sanitized,
                @"(?<![A-Za-z0-9])[A-Za-z0-9._~+/=-]{24,}(?![A-Za-z0-9])",
                "<redacted>");
            sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
            return sanitized.Length <= 240 ? sanitized : sanitized[..240] + "…";
        }

        string SummarizeExceptionChain(Exception exception)
        {
            var summaries = new List<string>();
            Exception? current = exception;
            for (var depth = 0; current is not null && depth < 3; depth++, current = current.InnerException)
            {
                var exceptionType = current.GetType().FullName ?? current.GetType().Name;
                summaries.Add($"{exceptionType}: {SanitizeExceptionMessage(current.Message)}");
            }

            return string.Join(" <- ", summaries);
        }

        app.Use(async (context, next) =>
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "/";
            var statusRecorded = false;
            context.Response.OnStarting(() =>
            {
                routeDiagnostics.Add(
                    $"HTTP {method} {path} final status={context.Response.StatusCode}");
                statusRecorded = true;
                return Task.CompletedTask;
            });

            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                var exceptionType = exception.GetType().FullName ?? exception.GetType().Name;
                routeDiagnostics.Add(
                    $"HTTP {method} {path} endpoint exception {exceptionType}: " +
                    SanitizeExceptionMessage(exception.Message));
                throw;
            }
            finally
            {
                if (!statusRecorded)
                {
                    routeDiagnostics.Add(
                        $"HTTP {method} {path} final status={context.Response.StatusCode} (response not started)");
                }
            }
        });

        // Exercise the real mapped handler/client with the startup-created
        // catalog in a loopback test host, not full normal-host composition.
        // This handler uses only grant resolution and session storage; the
        // runner-side counters below guard its trainer/update/publication seams.
        app.MapCodeDocumentationOneLessonPreflight(fixture.Repository);
        await app.StartAsync();

        var baseUri = ModelServer.Program.ResolveStartedLoopbackAddress(app);
        var effects = new OneLessonPreflightSideEffects();
        var output = new List<string>();
        var clientDiagnostics = new List<string>();
        var responses = new List<CodeDocumentationOneLessonPreflightResponseV1>();
        var client = new CodeDocumentationOneLessonPreflightClient(
            exception => clientDiagnostics.Add(SummarizeExceptionChain(exception)));
        var dependencies = CreateOneLessonPreflightDependencies(
            effects,
            ReadAndCaptureAsync,
            output.Add);

        async Task<OperationResult<CodeDocumentationOneLessonPreflightResponseV1>>
            ReadAndCaptureAsync(
                Uri uri,
                CodeDocumentationOneLessonSelectorV1 selector,
                TimeSpan timeout,
                CancellationToken cancellationToken)
        {
            var result = await client.PreflightAsync(
                uri,
                selector,
                timeout,
                cancellationToken);
            if (result.IsSuccess && result.Value is not null)
                responses.Add(result.Value);
            return result;
        }

        var args = new[]
        {
            "--one-lesson-objective", fixture.Binding.ObjectiveId,
            "--one-lesson-grant", fixture.Grant.GrantSelector,
            "--model-server-base-uri", baseUri.ToString()
        };

        string FailureDetails(string invocation, int exitCode)
            => $"{invocation} CLI exit={exitCode}; preflightCalls={effects.PreflightCalls}; " +
               $"responseCount={responses.Count}; output=[{string.Join(" | ", output)}]; " +
               $"errors=[{string.Join(" | ", effects.Errors)}]; " +
               $"clientExceptions=[{string.Join(" | ", clientDiagnostics)}]; " +
               $"http=[{string.Join(" | ", routeDiagnostics)}].";

        var firstExit = await CodeDocumentationBatchRunnerCli.RunAsync(
            args,
            dependencies);

        Assert.True(firstExit == 0, FailureDetails("First invocation", firstExit));
        Assert.Single(responses);
        var firstResponse = responses[0];
        Assert.Equal("Fresh", firstResponse.Disposition);

        var modelRoot = RepositoryOwnedTrainingArtifactLayout
            .ResolveModelArtifactRoot(fixture.Repository);
        Assert.True(modelRoot.IsSuccess, modelRoot.ErrorMessage);
        Assert.NotNull(modelRoot.Value);
        string historyPath;
        byte[] genesisHistory;
        using (var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   modelRoot.Value!,
                   fixture.Binding)))
        {
            historyPath = Path.Combine(
                fixture.Repository,
                opened.Session.Snapshot.HistoryRelativePath);
            genesisHistory = File.ReadAllBytes(historyPath);
        }

        var secondExit = await CodeDocumentationBatchRunnerCli.RunAsync(
            args,
            dependencies);

        Assert.True(secondExit == 0, FailureDetails("Second invocation", secondExit));
        Assert.Equal(2, effects.PreflightCalls);
        Assert.Empty(effects.Errors);
        Assert.Equal(2, output.Count);
        Assert.Contains(output, line => line.Contains("Fresh", StringComparison.Ordinal));
        Assert.Contains(output, line => line.Contains("Reopened", StringComparison.Ordinal));
        Assert.Empty(clientDiagnostics);
        Assert.Equal(
            2,
            routeDiagnostics.Count(diagnostic => StringComparer.Ordinal.Equals(
                diagnostic,
                $"HTTP POST {CodeDocumentationOneLessonPreflightWireContract.EndpointPath} final status=200")));
        Assert.Equal(2, responses.Count);
        var reopenedResponse = responses[1];
        Assert.Equal("Reopened", reopenedResponse.Disposition);
        Assert.Equal(fixture.Binding.ObjectiveId, firstResponse.ObjectiveId);
        Assert.Equal(fixture.Binding.TrainingRunId, firstResponse.TrainingRunId);
        Assert.Equal(
            CodeDocumentationOneLessonSession.ComputeBindingSha256(fixture.Binding),
            firstResponse.BindingSha256);
        Assert.False(string.IsNullOrWhiteSpace(firstResponse.SessionId));
        Assert.Equal(0, firstResponse.SessionRevision);
        Assert.Equal(firstResponse.ObjectiveId, reopenedResponse.ObjectiveId);
        Assert.Equal(firstResponse.TrainingRunId, reopenedResponse.TrainingRunId);
        Assert.Equal(firstResponse.BindingSha256, reopenedResponse.BindingSha256);
        Assert.Equal(firstResponse.SessionId, reopenedResponse.SessionId);
        Assert.Equal(firstResponse.SessionRevision, reopenedResponse.SessionRevision);
        Assert.Equal(genesisHistory, File.ReadAllBytes(historyPath));

        Assert.Equal(0, effects.EnvironmentReads);
        Assert.Equal(0, effects.FileChecks);
        Assert.Equal(0, effects.RunRootCreations);
        Assert.Equal(0, effects.FileWrites);
        Assert.Equal(0, effects.ConfigurationResolutions);
        Assert.Equal(0, effects.TeacherClients);
        Assert.Equal(0, effects.RunBatchCalls);
        Assert.Equal(0, effects.PublicationCalls);

        await app.StopAsync();
    }

    [Fact]
    public async Task OneLessonPreflight_ReplaysSelectorBeforeInputAndRunRootEffects()
    {
        var fileChecks = 0;
        var runRootCreations = 0;
        var runCalls = 0;
        var preflightCalls = 0;
        CodeDocumentationOneLessonSelectorV1? observedSelector = null;
        var output = new List<string>();
        var dependencies = new CodeDocumentationBatchRunnerDependencies
        {
            Now = () => DateTimeOffset.UtcNow,
            ReadEnvironment = _ => null,
            FileExists = _ =>
            {
                fileChecks++;
                return false;
            },
            CreateDirectory = _ => runRootCreations++,
            WriteTextAsync = (_, _, _, _) => Task.CompletedTask,
            WriteLinesAsync = (_, _, _, _) => Task.CompletedTask,
            WriteStdOut = output.Add,
            WriteStdErr = output.Add,
            ResolveAuthoringConfiguration = _ =>
                OperationResult<ContinuousLearningTp1DpoConfiguration>.Fail(
                    "must not resolve training configuration"),
            CreateTeacherClient = _ =>
                OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "must not create a teacher client"),
            RunBatchAsync = (_, _, _, _) =>
            {
                runCalls++;
                return Task.FromResult(
                    OperationResult<CodeDocumentationTrainingRunResult>.Fail(
                        "must not run a batch"));
            },
            PublishRepositoryOwnedDpoAsync = (_, _) => Task.FromResult(
                OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>.Fail(
                    "must not publish")),
            PreflightOneLessonAsync = (_, selector, _, _) =>
            {
                preflightCalls++;
                observedSelector = selector;
                return Task.FromResult(
                    OperationResult<CodeDocumentationOneLessonPreflightResponseV1>.Ok(
                        new(true, "", "", "Reopened", selector.ObjectiveId,
                            "stable-run-01", new string('a', 64), "session-id", 0)));
            }
        };

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            [
                "--one-lesson-objective", "objective-01",
                "--one-lesson-grant", "opaque-grant-token",
                "--model-server-base-uri", "http://127.0.0.1:5000"
            ],
            dependencies);

        Assert.Equal(0, exitCode);
        Assert.Equal(1, preflightCalls);
        Assert.Equal(
            new CodeDocumentationOneLessonSelectorV1(
                "objective-01",
                "opaque-grant-token"),
            observedSelector);
        Assert.Equal(0, fileChecks);
        Assert.Equal(0, runRootCreations);
        Assert.Equal(0, runCalls);
        Assert.Contains(output, line => line.Contains("Reopened", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(OneLessonPreflight_UnsupportedArgumentCases))]
    public async Task OneLessonPreflight_RejectsUnsupportedArgumentsByPresenceBeforeEffects(
        string[] unsupportedArguments)
    {
        var effects = new OneLessonPreflightSideEffects();
        var dependencies = CreateOneLessonPreflightDependencies(effects);
        var args = new List<string>
        {
            "--one-lesson-objective", "objective-01",
            "--one-lesson-grant", "opaque-grant-token",
            "--model-server-base-uri", "http://127.0.0.1:5000"
        };
        args.AddRange(unsupportedArguments);

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            args.ToArray(),
            dependencies);

        Assert.Equal(1, exitCode);
        Assert.NotEmpty(effects.Errors);
        Assert.Equal(0, effects.EnvironmentReads);
        Assert.Equal(0, effects.FileChecks);
        Assert.Equal(0, effects.RunRootCreations);
        Assert.Equal(0, effects.FileWrites);
        Assert.Equal(0, effects.ConfigurationResolutions);
        Assert.Equal(0, effects.TeacherClients);
        Assert.Equal(0, effects.RunBatchCalls);
        Assert.Equal(0, effects.PublicationCalls);
        Assert.Equal(0, effects.PreflightCalls);
    }

    public static TheoryData<string[]> OneLessonPreflight_UnsupportedArgumentCases
        => new()
        {
            { new[] { "--train", @"C:\input.jsonl" } },
            { new[] { "--train", "" } },
            { new[] { "--heldout", @"C:\heldout.jsonl" } },
            { new[] { "--heldout", "" } },
            { new[] { "--result-root", @"C:\results" } },
            { new[] { "--result-root", "" } },
            { new[] { "--run-id", "run-01" } },
            { new[] { "--run-id", "" } },
            { new[] { "--repository-root", @"C:\repo" } },
            { new[] { "--repository-root", "" } },
            { new[] { "--documentation-domain", "Angular" } },
            { new[] { "--documentation-domain", "" } },
            { new[] { "--dataset-version", "v1" } },
            { new[] { "--dataset-version", "" } },
            { new[] { "--model-server-timeout-seconds", "30" } },
            { new[] { "--model-server-timeout-seconds", "" } },
            { new[] { "unexpected-input.jsonl" } },
            { new[] { "" } }
        };

    [Theory]
    [MemberData(nameof(OneLessonPreflight_DuplicateOptionCases))]
    public async Task OneLessonPreflight_RejectsDuplicateOrCaseAliasedOptionsBeforeEffects(
        string[] duplicateArguments)
    {
        var effects = new OneLessonPreflightSideEffects();
        var dependencies = CreateOneLessonPreflightDependencies(effects);
        var args = new List<string>
        {
            "--one-lesson-objective", "objective-01",
            "--one-lesson-grant", "opaque-grant-token",
            "--model-server-base-uri", "http://127.0.0.1:5000"
        };
        args.AddRange(duplicateArguments);

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            args.ToArray(),
            dependencies);

        Assert.Equal(1, exitCode);
        Assert.NotEmpty(effects.Errors);
        Assert.Equal(0, effects.EnvironmentReads);
        Assert.Equal(0, effects.FileChecks);
        Assert.Equal(0, effects.RunRootCreations);
        Assert.Equal(0, effects.FileWrites);
        Assert.Equal(0, effects.ConfigurationResolutions);
        Assert.Equal(0, effects.TeacherClients);
        Assert.Equal(0, effects.RunBatchCalls);
        Assert.Equal(0, effects.PublicationCalls);
        Assert.Equal(0, effects.PreflightCalls);
    }

    public static TheoryData<string[]> OneLessonPreflight_DuplicateOptionCases
        => new()
        {
            { new[] { "--one-lesson-objective", "objective-02" } },
            { new[] { "--ONE-LESSON-GRANT", "another-grant" } },
            { new[] { "--model-server-base-uri", "http://localhost:5000" } }
        };

    [Theory]
    [MemberData(nameof(OneLessonPreflight_MalformedSelectorValueCases))]
    public async Task OneLessonPreflight_RejectsMalformedSelectorValuesBeforeEffects(
        string option,
        string malformedValue)
    {
        var effects = new OneLessonPreflightSideEffects();
        var dependencies = CreateOneLessonPreflightDependencies(effects);
        var args = new List<string>
        {
            "--one-lesson-objective", "objective-01",
            "--one-lesson-grant", "opaque-grant-token",
            "--model-server-base-uri", "http://127.0.0.1:5000"
        };
        args[args.IndexOf(option) + 1] = malformedValue;

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            args.ToArray(),
            dependencies);

        Assert.Equal(1, exitCode);
        Assert.NotEmpty(effects.Errors);
        Assert.Equal(0, effects.EnvironmentReads);
        Assert.Equal(0, effects.FileChecks);
        Assert.Equal(0, effects.RunRootCreations);
        Assert.Equal(0, effects.FileWrites);
        Assert.Equal(0, effects.ConfigurationResolutions);
        Assert.Equal(0, effects.TeacherClients);
        Assert.Equal(0, effects.RunBatchCalls);
        Assert.Equal(0, effects.PublicationCalls);
        Assert.Equal(0, effects.PreflightCalls);
    }

    public static TheoryData<string, string> OneLessonPreflight_MalformedSelectorValueCases
        => new()
        {
            { "--one-lesson-objective", " objective-01" },
            { "--one-lesson-objective", "objective-01 " },
            { "--one-lesson-objective", new string('o', 129) },
            { "--one-lesson-objective", "objective-\n01" },
            { "--one-lesson-grant", " opaque-grant-token" },
            { "--one-lesson-grant", "opaque-grant-token " },
            { "--one-lesson-grant", new string('g', 257) },
            { "--one-lesson-grant", "opaque-grant-\u0000token" }
        };

    [Theory]
    [MemberData(nameof(OneLessonPreflight_BlankRequiredOptionCases))]
    public async Task OneLessonPreflight_RejectsBlankRequiredOptionsBeforeEffects(
        string[] blankOptionArguments)
    {
        var effects = new OneLessonPreflightSideEffects();
        var dependencies = CreateOneLessonPreflightDependencies(effects);
        var args = new List<string>
        {
            "--one-lesson-objective", "objective-01",
            "--one-lesson-grant", "opaque-grant-token",
            "--model-server-base-uri", "http://127.0.0.1:5000"
        };
        args.AddRange(blankOptionArguments);

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            args.ToArray(),
            dependencies);

        Assert.Equal(1, exitCode);
        Assert.NotEmpty(effects.Errors);
        Assert.Equal(0, effects.EnvironmentReads);
        Assert.Equal(0, effects.FileChecks);
        Assert.Equal(0, effects.RunRootCreations);
        Assert.Equal(0, effects.FileWrites);
        Assert.Equal(0, effects.ConfigurationResolutions);
        Assert.Equal(0, effects.TeacherClients);
        Assert.Equal(0, effects.RunBatchCalls);
        Assert.Equal(0, effects.PublicationCalls);
        Assert.Equal(0, effects.PreflightCalls);
    }

    public static TheoryData<string[]> OneLessonPreflight_BlankRequiredOptionCases
        => new()
        {
            { new[] { "--one-lesson-objective", "" } },
            { new[] { "--one-lesson-grant", "" } },
            { new[] { "--model-server-base-uri", "" } }
        };

    private static CodeDocumentationBatchRunnerDependencies
        CreateOneLessonPreflightDependencies(
            OneLessonPreflightSideEffects effects,
            Func<Uri, CodeDocumentationOneLessonSelectorV1, TimeSpan,
                CancellationToken,
                Task<OperationResult<CodeDocumentationOneLessonPreflightResponseV1>>>?
                preflight = null,
            Action<string>? writeStdOut = null)
        => new()
        {
            Now = () => DateTimeOffset.UtcNow,
            ReadEnvironment = _ =>
            {
                effects.EnvironmentReads++;
                return null;
            },
            FileExists = _ =>
            {
                effects.FileChecks++;
                return false;
            },
            CreateDirectory = _ => effects.RunRootCreations++,
            WriteTextAsync = (_, _, _, _) =>
            {
                effects.FileWrites++;
                return Task.CompletedTask;
            },
            WriteLinesAsync = (_, _, _, _) =>
            {
                effects.FileWrites++;
                return Task.CompletedTask;
            },
            WriteStdOut = writeStdOut ?? (_ => { }),
            WriteStdErr = effects.Errors.Add,
            ResolveAuthoringConfiguration = _ =>
            {
                effects.ConfigurationResolutions++;
                return OperationResult<ContinuousLearningTp1DpoConfiguration>
                    .Fail("must not resolve training configuration");
            },
            CreateTeacherClient = _ =>
            {
                effects.TeacherClients++;
                return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "must not create a teacher client");
            },
            RunBatchAsync = (_, _, _, _) =>
            {
                effects.RunBatchCalls++;
                return Task.FromResult(
                    OperationResult<CodeDocumentationTrainingRunResult>.Fail(
                        "must not run a batch"));
            },
            PublishRepositoryOwnedDpoAsync = (_, _) =>
            {
                effects.PublicationCalls++;
                return Task.FromResult(OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                    .Fail("must not publish"));
            },
            PreflightOneLessonAsync = (uri, selector, timeout, cancellationToken) =>
            {
                effects.PreflightCalls++;
                if (preflight is not null)
                    return preflight(uri, selector, timeout, cancellationToken);

                return Task.FromResult(OperationResult<
                    CodeDocumentationOneLessonPreflightResponseV1>.Ok(
                    new(true, "", "", "Fresh", selector.ObjectiveId,
                        "run-01", new string('a', 64), "session-01", 0)));
            }
        };

    private sealed class OneLessonPreflightSideEffects
    {
        public int EnvironmentReads { get; set; }
        public int FileChecks { get; set; }
        public int RunRootCreations { get; set; }
        public int FileWrites { get; set; }
        public int ConfigurationResolutions { get; set; }
        public int TeacherClients { get; set; }
        public int RunBatchCalls { get; set; }
        public int PublicationCalls { get; set; }
        public int PreflightCalls { get; set; }
        public List<string> Errors { get; } = [];
    }

    private sealed class UnverifiedTerminalFrameFixture : IDisposable
    {
        private readonly string _root = Path.Combine(
            Path.GetTempPath(),
            "code-documentation-one-lesson-unverified-terminal-frame-" +
            Guid.NewGuid().ToString("N"));

        internal UnverifiedTerminalFrameFixture(bool includePublication)
        {
            Repository = Path.Combine(_root, "repo");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            Directory.CreateDirectory(Path.Combine(Repository, "training-artifacts", "models"));
            File.WriteAllText(
                Path.Combine(Repository, "LlmFromScratch.sln"),
                "synthetic terminal replay fixture");
            var modelRoot = RepositoryOwnedTrainingArtifactLayout
                .ResolveModelArtifactRoot(Repository);
            Assert.True(modelRoot.IsSuccess, modelRoot.ErrorMessage);
            Assert.NotNull(modelRoot.Value);
            Binding = new(
                "code-documentation-one-lesson:terminal-replay",
                "synthetic-grant",
                "grant-v1",
                "synthetic-training-run-terminal-replay",
                Hex('1'), Hex('2'), Hex('3'), Hex('4'), Hex('5'));
            Grant = new(
                Binding.GrantId,
                Binding.GrantVersion,
                "opaque-terminal-replay-selector",
                Binding.ObjectiveId,
                Binding.TrainingRunId,
                Binding.CandidateSha256,
                Binding.ParentManifestSha256,
                Binding.ParentWeightsSha256,
                Binding.ConfigurationSha256,
                Binding.ExamConditionSha256);

            using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                modelRoot.Value!,
                Binding));
            // This directly seeds a lifecycle frame with synthetic ID/hash
            // references. It is adversarial pre-existing state for fail-closed
            // tests, not a completed mapped-route request or receipt evidence.
            var snapshot = Require(opened.Session.RecordTerminalOutcome(
                0,
                new CodeDocumentationOneLessonVerifiedTerminalReceiptRefsV1(
                    Binding.ObjectiveId,
                    opened.Session.Snapshot.SessionId,
                    Binding.TrainingRunId,
                    CodeDocumentationOneLessonSession.ComputeBindingSha256(Binding),
                    new("trial-evaluation-receipt", Hex('a')),
                    new("quality-decision-receipt", Hex('b')),
                    new("disposition-receipt", Hex('c')),
                    includePublication
                        ? new CodeDocumentationOneLessonTypedReceiptRefV1<
                            CodeDocumentationOneLessonPublicationReceiptV1>(
                                "publication-receipt", Hex('d'))
                        : null,
                    new("final-reopen-receipt", Hex('e')))));
            TerminalOutcome = snapshot.TerminalOutcome!;
            HistoryPath = Path.Combine(Repository, snapshot.HistoryRelativePath);
        }

        internal string Repository { get; }
        internal CodeDocumentationOneLessonSessionBinding Binding { get; }
        internal CodeDocumentationOneLessonGrantV1 Grant { get; }
        internal CodeDocumentationOneLessonCanonicalOutcomeV1 TerminalOutcome { get; }
        internal string HistoryPath { get; }

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }

        private static string Hex(char value) => new(value, 64);
    }

    private sealed class OneLessonPreflightFixture : IDisposable
    {
        private readonly string _root = Path.Combine(
            Path.GetTempPath(),
            "code-documentation-one-lesson-identity-substrate-" +
            Guid.NewGuid().ToString("N"));

        internal OneLessonPreflightFixture()
        {
            Repository = Path.Combine(_root, "repo");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            Directory.CreateDirectory(Path.Combine(Repository, "training-artifacts", "models"));
            File.WriteAllText(
                Path.Combine(Repository, "LlmFromScratch.sln"),
                "synthetic identity-substrate fixture");

            Binding = new(
                "code-documentation-one-lesson:identity-substrate",
                "synthetic-identity-substrate-grant",
                "grant-v1",
                "synthetic-training-run-identity-substrate",
                Hex('1'), Hex('2'), Hex('3'), Hex('4'), Hex('5'));
            Grant = new(
                Binding.GrantId,
                Binding.GrantVersion,
                "opaque-identity-substrate-selector",
                Binding.ObjectiveId,
                Binding.TrainingRunId,
                Binding.CandidateSha256,
                Binding.ParentManifestSha256,
                Binding.ParentWeightsSha256,
                Binding.ConfigurationSha256,
                Binding.ExamConditionSha256);
        }

        internal string Repository { get; }
        internal CodeDocumentationOneLessonSessionBinding Binding { get; }
        internal CodeDocumentationOneLessonGrantV1 Grant { get; }

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }

        private static string Hex(char value) => new(value, 64);
    }

    private static T Require<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    [Fact]
    public async Task CodeDocumentationBatchRunnerCli_PassesRepositoryOwnedOptionsAndLabelsSummaryPairRoles()
    {
        using var fixture = new TestRepositoryFixture();
        var createdDirectories = new List<string>();
        var textWrites = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
        var lineWrites = new Dictionary<string, string[]>(
            StringComparer.OrdinalIgnoreCase);
        var configuration = CreateConfiguration();
        var receipt = CreateReceipt();
        var runCalls = 0;
        var configurationCalls = 0;
        var teacherClientCalls = 0;
        var publishCalls = 0;
        CodeDocumentationRepositoryOwnedDpoRunOptions? capturedOptions = null;
        var dependencies = new CodeDocumentationBatchRunnerDependencies
        {
            Now = () => DateTimeOffset.Parse("2026-09-06T12:00:00Z"),
            ReadEnvironment = _ => null,
            FileExists = _ => true,
            CreateDirectory = path => createdDirectories.Add(path),
            WriteTextAsync = (path, text, _, _) =>
            {
                textWrites[path] = text;
                return Task.CompletedTask;
            },
            WriteLinesAsync = (path, lines, _, _) =>
            {
                lineWrites[path] = lines.ToArray();
                return Task.CompletedTask;
            },
            WriteStdOut = _ => { },
            WriteStdErr = _ => { },
            ResolveAuthoringConfiguration = _ =>
            {
                configurationCalls++;
                return OperationResult<ContinuousLearningTp1DpoConfiguration>
                    .Ok(configuration);
            },
            CreateTeacherClient = _ =>
            {
                teacherClientCalls++;
                return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "The CLI must not eagerly create the teacher client.");
            },
            RunBatchAsync = (_, _, _, repositoryOwnedDpo) =>
            {
                runCalls++;
                capturedOptions = repositoryOwnedDpo;
                return Task.FromResult(
                    OperationResult<CodeDocumentationTrainingRunResult>.Ok(
                        CreateRunResult(receipt)));
            },
            PublishRepositoryOwnedDpoAsync = (_, _) =>
            {
                publishCalls++;
                return Task.FromResult(OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                    .Fail("publisher must not be called"));
            }
        };
        var resultRoot = Path.Combine(
            Path.GetTempPath(),
            "code-doc-cli-results-" + Guid.NewGuid().ToString("N"));

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            [
                "--train", @"C:\datasets\TRAIN-PAIRS.jsonl",
                "--heldout", @"C:\datasets\HELDOUT-ROWS.jsonl",
                "--result-root", resultRoot,
                "--run-id", "run-valid",
                "--repository-root", fixture.RepositoryRoot,
                "--documentation-domain", "Angular",
                "--dataset-version", "v7"
            ],
            dependencies);

        Assert.Equal(0, exitCode);
        Assert.Equal(1, runCalls);
        Assert.Equal(1, configurationCalls);
        Assert.Equal(0, teacherClientCalls);
        Assert.Equal(0, publishCalls);
        Assert.NotNull(capturedOptions);
        Assert.Equal(fixture.RepositoryRoot, capturedOptions!.RepositoryRoot);
        Assert.Equal("v7", capturedOptions.DatasetVersion);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AngularDomainSlug,
            capturedOptions.AuthoringContext.DomainProfile?.DomainSlug);

        var runRoot = Assert.Single(createdDirectories);
        Assert.EndsWith(
            Path.Combine("code-doc-cli-results-" + Path.GetFileName(resultRoot)[^32..], "run-valid"),
            runRoot,
            StringComparison.OrdinalIgnoreCase);
        var summaryPath = Path.Combine(
            runRoot,
            "code-doc-batch-run-summary.json");
        Assert.True(textWrites.TryGetValue(summaryPath, out var summaryText));
        using var summary = JsonDocument.Parse(summaryText);
        var root = summary.RootElement;
        Assert.Equal(
            2,
            root.GetProperty("repository_owned_training_pair_count")
                .GetInt32());
        Assert.Contains(
            "rich repository training pairs",
            root.GetProperty("repository_owned_training_pair_role")
                .GetString(),
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            root.GetProperty("legacy_evaluation_pair_count").GetInt32());
        Assert.Contains(
            "evaluation-only",
            root.GetProperty("legacy_evaluation_pair_role").GetString(),
            StringComparison.Ordinal);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AngularDomainSlug,
            root.GetProperty("repository_owned_domain").GetString());
        Assert.Equal(
            "v7",
            root.GetProperty("repository_owned_dataset_version").GetString());

        var receiptJson = root.GetProperty("repository_owned_dpo_receipt");
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset
                .ToString(),
            receiptJson.GetProperty("source").GetString());
        Assert.Equal(
            receipt.ManifestSha256,
            receiptJson.GetProperty("manifest_sha256").GetString());
        Assert.Equal(
            receipt.RepositoryRelativeManifestPath,
            receiptJson.GetProperty("repository_relative_manifest_path")
                .GetString());
        Assert.Equal(2, receiptJson.GetProperty("pair_count").GetInt32());
        Assert.Equal(
            receipt.RequestIdentitySha256,
            receiptJson.GetProperty("request_identity_sha256").GetString());
        var orderedHashes = receiptJson.GetProperty("ordered_pair_hashes")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();
        Assert.Equal(
            receipt.OrderedPairs.Select(pair => pair.PairFileSha256),
            orderedHashes);

        var legacyPairsPath = Path.Combine(
            runRoot,
            "code-doc-routing-dpo-pairs.jsonl");
        Assert.True(lineWrites.TryGetValue(legacyPairsPath, out var legacyLines));
        Assert.Single(legacyLines);
        Assert.Contains(
            "legacy evaluation-only routing pair",
            legacyLines[0],
            StringComparison.Ordinal);
        Assert.Contains(
            "Repository-owned DPO receipt",
            textWrites[Path.Combine(runRoot, "LOOK-LIKE-RUN-REPORT.md")],
            StringComparison.Ordinal);
        Assert.False(root.GetProperty("model_checkpoint_claimed").GetBoolean());
        Assert.Contains(
            "DPO-only",
            root.GetProperty("human_summary_ru").GetString(),
            StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(PublicationDomainCases))]
    public async Task CodeDocumentationBatchRunnerCli_ModelServerUriPostsOnceAndKeepsDomainOnReceiptRequestAndCheckpointClaim(
        string documentationDomain,
        string expectedDomainSlug,
        string expectedDpoManifestRelativePath,
        string expectedPublishedWeightsRelativePath,
        string expectedPublishedManifestRelativePath)
    {
        using var fixture = new TestRepositoryFixture();
        var createdDirectories = new List<string>();
        var textWrites = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
        var lineWrites = new Dictionary<string, string[]>(
            StringComparer.OrdinalIgnoreCase);
        var stdout = new List<string>();
        var receipt = CreateReceipt(expectedDomainSlug);
        var runCalls = 0;
        var publishCalls = 0;
        CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest?
            capturedPublication = null;
        var dependencies = new CodeDocumentationBatchRunnerDependencies
        {
            Now = () => DateTimeOffset.Parse("2026-09-06T12:00:00Z"),
            ReadEnvironment = _ => null,
            FileExists = _ => true,
            CreateDirectory = path => createdDirectories.Add(path),
            WriteTextAsync = (path, text, _, _) =>
            {
                textWrites[path] = text;
                return Task.CompletedTask;
            },
            WriteLinesAsync = (path, lines, _, _) =>
            {
                lineWrites[path] = lines.ToArray();
                return Task.CompletedTask;
            },
            WriteStdOut = value => stdout.Add(value),
            WriteStdErr = _ => { },
            ResolveAuthoringConfiguration = _ =>
                OperationResult<ContinuousLearningTp1DpoConfiguration>
                    .Ok(CreateConfiguration()),
            CreateTeacherClient = _ =>
                OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "The CLI must not eagerly create the teacher client."),
            RunBatchAsync = (_, _, _, _) =>
            {
                runCalls++;
                return Task.FromResult(
                    OperationResult<CodeDocumentationTrainingRunResult>.Ok(
                        CreateRunResult(receipt, expectedDomainSlug)));
            },
            PublishRepositoryOwnedDpoAsync = (request, _) =>
            {
                publishCalls++;
                capturedPublication = request;
                // Эта проверка не даёт CLI тихо превратить SQL-запрос в Angular/C#
                // до отправки publication request в ModelServer.
                Assert.Equal(expectedDomainSlug, request.DomainSlug);
                Assert.Equal(
                    expectedDpoManifestRelativePath,
                    request.RoutingRun.RepositoryOwnedDpoReceipt
                        ?.RepositoryRelativeManifestPath);
                return Task.FromResult(OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                    .Ok(CreatePublication(receipt, expectedDomainSlug)));
            }
        };
        var resultRoot = Path.Combine(
            Path.GetTempPath(),
            "code-doc-cli-publish-" + Guid.NewGuid().ToString("N"));

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            [
                "--train", @"C:\datasets\TRAIN-PAIRS.jsonl",
                "--heldout", @"C:\datasets\HELDOUT-ROWS.jsonl",
                "--result-root", resultRoot,
                "--run-id", "run-publish",
                "--repository-root", fixture.RepositoryRoot,
                "--documentation-domain", documentationDomain,
                "--dataset-version", "v7",
                "--model-server-base-uri", "http://127.0.0.1:5055",
                "--model-server-timeout-seconds", "7"
            ],
            dependencies);

        Assert.Equal(0, exitCode);
        Assert.Equal(1, runCalls);
        Assert.Equal(1, publishCalls);
        Assert.NotNull(capturedPublication);
        Assert.Equal(
            new Uri("http://127.0.0.1:5055"),
            capturedPublication!.BaseUri);
        Assert.Equal(TimeSpan.FromSeconds(7), capturedPublication.Timeout);
        Assert.Equal("run-publish", capturedPublication.TrainingRunId);
        Assert.Equal(expectedDomainSlug, capturedPublication.DomainSlug);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .DesignPatternSlug,
            capturedPublication.DesignPatternSlug);
        Assert.Equal(
            expectedDpoManifestRelativePath,
            capturedPublication.RoutingRun.RepositoryOwnedDpoReceipt
                ?.RepositoryRelativeManifestPath);
        Assert.All(
            capturedPublication.RoutingRun.RepositoryOwnedDpoPairs,
            pair => Assert.Equal(expectedDomainSlug, pair.Domain));

        var runRoot = Assert.Single(createdDirectories);
        using var summary = JsonDocument.Parse(
            textWrites[Path.Combine(runRoot, "code-doc-batch-run-summary.json")]);
        var root = summary.RootElement;
        Assert.True(root.GetProperty("model_checkpoint_claimed").GetBoolean());
        var publication =
            root.GetProperty("repository_owned_model_publish");
        Assert.Equal(1, publication.GetProperty("parent_checkpoint_step")
            .GetInt32());
        Assert.Equal(2, publication.GetProperty("published_checkpoint_step")
            .GetInt32());
        Assert.Equal(
            expectedDpoManifestRelativePath,
            publication.GetProperty("dpo_manifest_relative_path")
                .GetString());
        Assert.Equal(
            expectedPublishedWeightsRelativePath,
            publication.GetProperty("published_weights_relative_path")
                .GetString());
        Assert.Equal(
            expectedPublishedManifestRelativePath,
            publication.GetProperty("published_manifest_relative_path")
                .GetString());
        Assert.Contains(stdout, line =>
            line.Contains("Новый checkpoint step: 2", StringComparison.Ordinal));
        Assert.Contains(stdout, line =>
            line.Contains("DPO hash: " + receipt.ManifestSha256,
                StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://192.168.1.10:5055")]
    [InlineData("http://user:secret@localhost:5055")]
    [InlineData("http://localhost:5055?target=/internal")]
    [InlineData("http://localhost:5055/#fragment")]
    [InlineData("http://localhost:5055/internal")]
    public async Task CodeDocumentationBatchRunnerCli_ModelServerUriRejectsUnsafeShapesBeforeRun(
        string rawUri)
    {
        using var fixture = new TestRepositoryFixture();
        var stderr = new List<string>();
        var runCalls = 0;
        var publishCalls = 0;
        var dependencies = CreateSimplePublishingDependencies(
            CreateReceipt(),
            _ => runCalls++,
            _ =>
            {
                publishCalls++;
                return OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                    .Fail("publisher must not run");
            },
            stderr.Add);

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            CreatePublishArgs(fixture.RepositoryRoot, rawUri),
            dependencies);

        Assert.Equal(1, exitCode);
        Assert.Equal(0, runCalls);
        Assert.Equal(0, publishCalls);
        Assert.Contains(stderr, line =>
            line.Contains("loopback root URI", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("http://127.0.0.1:5055")]
    [InlineData("https://localhost:5055/")]
    [InlineData("http://[::1]:5055")]
    public async Task CodeDocumentationBatchRunnerCli_ModelServerUriAcceptsLoopbackRootShapes(
        string rawUri)
    {
        using var fixture = new TestRepositoryFixture();
        var receipt = CreateReceipt();
        CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest?
            capturedPublication = null;
        var dependencies = CreateSimplePublishingDependencies(
            receipt,
            _ => { },
            request =>
            {
                capturedPublication = request;
                return OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                    .Ok(CreatePublication(receipt));
            });

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            CreatePublishArgs(fixture.RepositoryRoot, rawUri),
            dependencies);

        Assert.Equal(0, exitCode);
        Assert.NotNull(capturedPublication);
        Assert.True(capturedPublication!.BaseUri.IsLoopback);
        Assert.Equal("/", capturedPublication.BaseUri.AbsolutePath);
        Assert.Equal("", capturedPublication.BaseUri.UserInfo);
        Assert.Equal("", capturedPublication.BaseUri.Query);
        Assert.Equal("", capturedPublication.BaseUri.Fragment);
    }

    [Fact]
    public async Task CodeDocumentationBatchRunnerCli_ModelServerDefaultTimeoutIsSixHours()
    {
        using var fixture = new TestRepositoryFixture();
        var receipt = CreateReceipt();
        CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest?
            capturedPublication = null;
        var dependencies = CreateSimplePublishingDependencies(
            receipt,
            _ => { },
            request =>
            {
                capturedPublication = request;
                return OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                    .Ok(CreatePublication(receipt));
            });

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            CreatePublishArgs(
                fixture.RepositoryRoot,
                "http://127.0.0.1:5055"),
            dependencies);

        Assert.Equal(0, exitCode);
        Assert.NotNull(capturedPublication);
        Assert.Equal(
            CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                .DefaultTimeout,
            capturedPublication!.Timeout);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("86401")]
    public async Task CodeDocumentationBatchRunnerCli_ModelServerTimeoutRejectsOutsideBoundsBeforeRun(
        string timeoutSeconds)
    {
        using var fixture = new TestRepositoryFixture();
        var stderr = new List<string>();
        var runCalls = 0;
        var publishCalls = 0;
        var args = CreatePublishArgs(
            fixture.RepositoryRoot,
            "http://127.0.0.1:5055")
            .Concat(["--model-server-timeout-seconds", timeoutSeconds])
            .ToArray();
        var dependencies = CreateSimplePublishingDependencies(
            CreateReceipt(),
            _ => runCalls++,
            _ =>
            {
                publishCalls++;
                return OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                    .Fail("publisher must not run");
            },
            stderr.Add);

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            args,
            dependencies);

        Assert.Equal(1, exitCode);
        Assert.Equal(0, runCalls);
        Assert.Equal(0, publishCalls);
        Assert.Contains(stderr, line =>
            line.Contains("86400", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CodeDocumentationBatchRunnerCli_ModelServerFailureExitsOneAndDoesNotClaimCheckpoint()
    {
        using var fixture = new TestRepositoryFixture();
        var createdDirectories = new List<string>();
        var textWrites = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
        var stderr = new List<string>();
        var receipt = CreateReceipt();
        var publishCalls = 0;
        var dependencies = new CodeDocumentationBatchRunnerDependencies
        {
            Now = () => DateTimeOffset.Parse("2026-09-06T12:00:00Z"),
            ReadEnvironment = _ => null,
            FileExists = _ => true,
            CreateDirectory = path => createdDirectories.Add(path),
            WriteTextAsync = (path, text, _, _) =>
            {
                textWrites[path] = text;
                return Task.CompletedTask;
            },
            WriteLinesAsync = (_, _, _, _) => Task.CompletedTask,
            WriteStdOut = _ => { },
            WriteStdErr = value => stderr.Add(value),
            ResolveAuthoringConfiguration = _ =>
                OperationResult<ContinuousLearningTp1DpoConfiguration>
                    .Ok(CreateConfiguration()),
            CreateTeacherClient = _ =>
                OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "The CLI must not eagerly create the teacher client."),
            RunBatchAsync = (_, _, _, _) => Task.FromResult(
                OperationResult<CodeDocumentationTrainingRunResult>.Ok(
                    CreateRunResult(receipt))),
            PublishRepositoryOwnedDpoAsync = (_, _) =>
            {
                publishCalls++;
                return Task.FromResult(OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                    .Fail(
                        "fake ModelServer unavailable",
                        "FAKE_MODEL_SERVER_UNAVAILABLE"));
            }
        };
        var resultRoot = Path.Combine(
            Path.GetTempPath(),
            "code-doc-cli-publish-fail-" + Guid.NewGuid().ToString("N"));

        var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
            [
                "--train", @"C:\datasets\TRAIN-PAIRS.jsonl",
                "--heldout", @"C:\datasets\HELDOUT-ROWS.jsonl",
                "--result-root", resultRoot,
                "--run-id", "run-publish-fail",
                "--repository-root", fixture.RepositoryRoot,
                "--documentation-domain", "Angular",
                "--dataset-version", "v7",
                "--model-server-base-uri", "http://127.0.0.1:5055"
            ],
            dependencies);

        Assert.Equal(1, exitCode);
        Assert.Equal(1, publishCalls);
        var runRoot = Assert.Single(createdDirectories);
        using var summary = JsonDocument.Parse(
            textWrites[Path.Combine(runRoot, "code-doc-batch-run-summary.json")]);
        var root = summary.RootElement;
        Assert.Equal("FAIL", root.GetProperty("status").GetString());
        Assert.False(root.GetProperty("model_checkpoint_claimed").GetBoolean());
        Assert.Contains(stderr, line =>
            line.Contains("Checkpoint модели не заявлен",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task CodeDocumentationBatchRunnerCli_InvalidRepositoryOwnedInputsFailBeforeFactoryRunOrWrites()
    {
        using var fixture = new TestRepositoryFixture();
        var cases = new[]
        {
            new InvalidRepositoryOwnedCase(
                "unknown-domain",
                fixture.RepositoryRoot,
                "python",
                "v1",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .InvalidErrorCode),
            new InvalidRepositoryOwnedCase(
                "unsafe-version",
                fixture.RepositoryRoot,
                "SQL",
                "../escape",
                RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode),
            new InvalidRepositoryOwnedCase(
                "missing-root",
                Path.Combine(fixture.RepositoryRoot, "missing"),
                "C#/.NET",
                "v1",
                RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode)
        };

        foreach (var testCase in cases)
        {
            var createdDirectories = new List<string>();
            var textWrites = new Dictionary<string, string>();
            var lineWrites = new Dictionary<string, string[]>();
            var stderr = new List<string>();
            var configurationCalls = 0;
            var teacherClientCalls = 0;
            var runCalls = 0;
            var dependencies = new CodeDocumentationBatchRunnerDependencies
            {
                Now = () => DateTimeOffset.Parse("2026-09-06T12:00:00Z"),
                ReadEnvironment = _ => null,
                FileExists = _ => true,
                CreateDirectory = path => createdDirectories.Add(path),
                WriteTextAsync = (path, text, _, _) =>
                {
                    textWrites[path] = text;
                    return Task.CompletedTask;
                },
                WriteLinesAsync = (path, lines, _, _) =>
                {
                    lineWrites[path] = lines.ToArray();
                    return Task.CompletedTask;
                },
                WriteStdOut = _ => { },
                WriteStdErr = value => stderr.Add(value),
                ResolveAuthoringConfiguration = _ =>
                {
                    configurationCalls++;
                    return OperationResult<ContinuousLearningTp1DpoConfiguration>
                        .Ok(CreateConfiguration());
                },
                CreateTeacherClient = _ =>
                {
                    teacherClientCalls++;
                    return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                        "factory must not be called");
                },
                RunBatchAsync = (_, _, _, _) =>
                {
                    runCalls++;
                    return Task.FromResult(
                        OperationResult<CodeDocumentationTrainingRunResult>
                            .Fail("runner must not run"));
                },
                PublishRepositoryOwnedDpoAsync = (_, _) =>
                    Task.FromResult(OperationResult<
                        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                        .Fail("publisher must not be called"))
            };

            var exitCode = await CodeDocumentationBatchRunnerCli.RunAsync(
                [
                    "--train", @"C:\datasets\TRAIN-PAIRS.jsonl",
                    "--heldout", @"C:\datasets\HELDOUT-ROWS.jsonl",
                    "--result-root", Path.Combine(
                        Path.GetTempPath(),
                        "code-doc-cli-invalid-" + testCase.Name),
                    "--run-id", "run-invalid",
                    "--repository-root", testCase.RepositoryRoot,
                    "--documentation-domain", testCase.Domain,
                    "--dataset-version", testCase.DatasetVersion
                ],
                dependencies);

            Assert.Equal(1, exitCode);
            Assert.Contains(
                stderr,
                line => line.Contains(testCase.ExpectedErrorCode,
                    StringComparison.Ordinal));
            Assert.Empty(createdDirectories);
            Assert.Empty(textWrites);
            Assert.Empty(lineWrites);
            Assert.Equal(0, configurationCalls);
            Assert.Equal(0, teacherClientCalls);
            Assert.Equal(0, runCalls);
            Assert.False(Directory.Exists(Path.Combine(
                fixture.RepositoryRoot,
                "training-data")));
        }
    }

    // Синтетические rich DPO пары несут тот же домен, что и receipt:
    // так fake-публикация доказывает сохранение домена на всём пути.
    private static CodeDocumentationTrainingRunResult CreateRunResult(
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt receipt,
        string domainSlug =
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .AngularDomainSlug)
        => new(
            50,
            50,
            50,
            50,
            50,
            50,
            1.0,
            1.0,
            1.0,
            1.0,
            1.0,
            1.0,
            1.0,
            12.5,
            64.0,
            25.0,
            [
                new RoutingDpoPair(
                    "legacy-pair-001",
                    "prompt",
                    "chosen",
                    "rejected [WRONG: evaluation-only]",
                    "eval-run",
                    OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                    OrchestrationRoutingCurriculum.OrchestrationV309UnitId)
            ],
            receipt,
            [
                new ClDpoPair(
                    "repository prompt 1",
                    "repository chosen 1",
                    "repository rejected 1",
                    domainSlug,
                    DateTimeOffset.UnixEpoch),
                new ClDpoPair(
                    "repository prompt 2",
                    "repository chosen 2",
                    "repository rejected 2",
                    domainSlug,
                    DateTimeOffset.UnixEpoch)
            ],
            [
                new CodeDocumentationTrainingCaseResult(
                    "case-001",
                    OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                    OrchestrationRoutingCurriculum.DocumentationDotNetUnitId,
                    true,
                    true,
                    true,
                    true,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    "PASS")
            ]);

    // Пути manifest/pair строятся из домена: SQL-positive test не должен
    // переиспользовать Angular receipt evidence.
    private static CodeDocumentationRepositoryOwnedDpoDatasetReceipt
        CreateReceipt(
            string domainSlug =
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AngularDomainSlug)
        => new(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            new string('a', 64),
            CreateDpoManifestRelativePath(domainSlug),
            2,
            new string('b', 64),
            [
                new RepositoryOwnedDpoDatasetManifestPair(
                    "pair-001",
                    CreateDpoPairRelativePath(domainSlug, "pair-001"),
                    new string('c', 64)),
                new RepositoryOwnedDpoDatasetManifestPair(
                    "pair-002",
                    CreateDpoPairRelativePath(domainSlug, "pair-002"),
                    new string('d', 64))
            ]);

    // Publication evidence строится из домена запроса, чтобы поймать
    // перекрёстные checkpoint claims между SQL и Angular.
    private static CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse
        CreatePublication(
            CodeDocumentationRepositoryOwnedDpoDatasetReceipt receipt,
            string domainSlug =
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AngularDomainSlug)
        => new(
            true,
            "",
            "",
            "DPO для документации кода открыт из репозитория и опубликован ModelServer.",
            receipt.RepositoryRelativeManifestPath,
            receipt.ManifestSha256,
            receipt.PairCount,
            1,
            2,
            CreatePublishedWeightsRelativePath(domainSlug),
            CreatePublishedManifestRelativePath(domainSlug),
            ["test:published"]);

    private static string[] CreatePublishArgs(
        string repositoryRoot,
        string modelServerBaseUri,
        string documentationDomain = "Angular")
        =>
        [
            "--train", @"C:\datasets\TRAIN-PAIRS.jsonl",
            "--heldout", @"C:\datasets\HELDOUT-ROWS.jsonl",
            "--result-root", Path.Combine(
                Path.GetTempPath(),
                "code-doc-cli-bridge-" + Guid.NewGuid().ToString("N")),
            "--run-id", "run-publish",
            "--repository-root", repositoryRoot,
            "--documentation-domain", documentationDomain,
            "--dataset-version", "v7",
            "--model-server-base-uri", modelServerBaseUri
        ];

    public static TheoryData<string, string, string, string, string>
        PublicationDomainCases =>
        new()
        {
            {
                "Angular",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .AngularDomainSlug,
                CreateDpoManifestRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .AngularDomainSlug),
                CreatePublishedWeightsRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .AngularDomainSlug),
                CreatePublishedManifestRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .AngularDomainSlug)
            },
            {
                "SQL",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .SqlDomainSlug,
                CreateDpoManifestRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .SqlDomainSlug),
                CreatePublishedWeightsRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .SqlDomainSlug),
                CreatePublishedManifestRelativePath(
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .SqlDomainSlug)
            }
        };

    private static string CreateDpoManifestRelativePath(string domainSlug)
        => $"training-data/dpo/{domainSlug}/ilearning-unit/v7/manifest.json";

    private static string CreateDpoPairRelativePath(
        string domainSlug,
        string pairId)
        => $"training-data/dpo/{domainSlug}/ilearning-unit/v7/pairs/{pairId}.dpo-pair.json";

    private static string CreatePublishedWeightsRelativePath(string domainSlug)
        => "training-artifacts/models/" +
           $"{domainSlug}/ilearning-unit/code-documentation-llm/v1/checkpoints/000002/weights.bin";

    private static string CreatePublishedManifestRelativePath(string domainSlug)
        => "training-artifacts/models/" +
           $"{domainSlug}/ilearning-unit/code-documentation-llm/v1/checkpoints/000002/manifest.json";

    private static CodeDocumentationBatchRunnerDependencies
        CreateSimplePublishingDependencies(
            CodeDocumentationRepositoryOwnedDpoDatasetReceipt receipt,
            Action<CodeDocumentationRepositoryOwnedDpoRunOptions?>? onRun = null,
            Func<CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest,
                OperationResult<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>?
                onPublish = null,
            Action<string>? writeStdErr = null)
        => new()
        {
            Now = () => DateTimeOffset.Parse("2026-09-06T12:00:00Z"),
            ReadEnvironment = _ => null,
            FileExists = _ => true,
            CreateDirectory = _ => { },
            WriteTextAsync = (_, _, _, _) => Task.CompletedTask,
            WriteLinesAsync = (_, _, _, _) => Task.CompletedTask,
            WriteStdOut = _ => { },
            WriteStdErr = writeStdErr ?? (_ => { }),
            ResolveAuthoringConfiguration = _ =>
                OperationResult<ContinuousLearningTp1DpoConfiguration>
                    .Ok(CreateConfiguration()),
            CreateTeacherClient = _ =>
                OperationResult<ITrainingTeacherInferenceClient>.Fail(
                    "The CLI must not eagerly create the teacher client."),
            RunBatchAsync = (_, _, _, repositoryOwnedDpo) =>
            {
                onRun?.Invoke(repositoryOwnedDpo);
                return Task.FromResult(
                    OperationResult<CodeDocumentationTrainingRunResult>.Ok(
                        CreateRunResult(receipt)));
            },
            PublishRepositoryOwnedDpoAsync = (request, _) =>
                Task.FromResult(
                    onPublish?.Invoke(request) ??
                    OperationResult<
                        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>
                        .Ok(CreatePublication(receipt)))
        };

    private static ContinuousLearningTp1DpoConfiguration CreateConfiguration()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('e', 64),
            new string('f', 64),
            50,
            EvidenceRef('1'),
            EvidenceRef('2'),
            Enumerable.Range(0, 32).Select(index => (byte)index).ToArray());
        Assert.True(configuration.IsSuccess, configuration.ErrorMessage);
        return configuration.Value!;
    }

    private static string EvidenceRef(char character)
        => "sha256:" + new string(character, 64);

    private sealed record InvalidRepositoryOwnedCase(
        string Name,
        string RepositoryRoot,
        string Domain,
        string DatasetVersion,
        string ExpectedErrorCode);

    private sealed class TestRepositoryFixture : IDisposable
    {
        public TestRepositoryFixture()
        {
            RepositoryRoot = Path.Combine(
                Path.GetTempPath(),
                "llm-mvp-code-doc-cli-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RepositoryRoot);
            Directory.CreateDirectory(
                Path.Combine(RepositoryRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(RepositoryRoot, "11.DPO"));
            File.WriteAllText(
                Path.Combine(RepositoryRoot, "LlmFromScratch.sln"),
                "",
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
