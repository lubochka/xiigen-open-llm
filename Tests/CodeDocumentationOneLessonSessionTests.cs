// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using ContinuousLearning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using ModelServer;
using TrainableRouter.Training;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

public sealed class CodeDocumentationOneLessonSessionTests
{

    [Fact]
    public async Task SyntheticStudentLease_ReservesResponseFramesWithoutChangingLegacyCapacity()
    {
        foreach(var row in new[] { (N:2,R:5), (N:200,R:25), (N:1,R:5000), (N:2,R:2500) })
        {
            var n=checked(row.N*row.R);
            using var f=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(10000,row.N);
            f.ProvisionOrdinaryV2(row.R,genuineTeachingCurriculum:CodeDocumentationPreparedBaselineTests.SelectionCurriculum(f));
            var catalog=ContinuousLearningModelServerStartup.Create(f.ReadEnvironment).OneLessonGrantCatalog;
            var grant=Require(catalog.Resolve(f.OrdinarySelector)); var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(f.RepositoryRoot));
            var b=new CodeDocumentationOneLessonSessionBinding(grant.ObjectiveId,grant.GrantId,grant.GrantVersion,grant.TrainingRunId,grant.CandidateSha256,grant.ParentManifestSha256,grant.ParentWeightsSha256,grant.ConfigurationSha256,grant.ExamConditionSha256)
                {FormatVersion=2,DescriptorsV2=grant.DescriptorsV2};
            Assert.Equal(row.N,f.Corpus.Corpus.Entries.Count(e=>e.Partition==FrozenReferencePartition.HeldOut));
            Assert.Equal(row.N,f.Profile.GenerationSettings.MaxHeldOutCases);Assert.Equal(row.R,grant.EvaluationPlanSelection!.RepeatCount);
            Assert.Equal(n,grant.EvaluationPlanSelection.ExpectedEvaluationCallCount);Assert.Equal(10000,catalog.MaxTeacherCalls);
            using(var ordinary=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,b,catalog.MaxTeacherCalls,grant)))
            {Assert.Equal(18+2*n,ordinary.Session.CapacityV2!.MaximumFrames);Assert.Equal(95249L+6548L*n,ordinary.Session.CapacityV2.MaximumBytes);}
            using(var lease=Require(CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,b,catalog,f.OrdinarySelector)))
            {
                var capacity=Require(lease.Session.ReadCapacityPrefixV2());Assert.Equal(n,capacity.SlotCount);
                Assert.Equal(20+2*n,capacity.MaximumFrames);Assert.Equal(627846L+6548L*n,capacity.MaximumBytes);
                Assert.Equal((0,0,false),lease.Session.SelectedStudentAccounting);Assert.False(File.Exists(f.OrdinaryHistory));
                Assert.True(lease.Session.ValidateContinuationCapacityV2(0,0,20+2*n,627846L+6548L*n).IsSuccess);
                Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,lease.Session.ValidateContinuationCapacityV2(0,0,21+2*n,627846L+6548L*n).ErrorCode);
                Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,lease.Session.ValidateContinuationCapacityV2(0,0,20+2*n,627847L+6548L*n).ErrorCode);
                Assert.Equal(capacity,Require(lease.Session.ReadCapacityPrefixV2()));Assert.Equal((0,0,false),lease.Session.SelectedStudentAccounting);
            }
            var changed=b with {ParentWeightsSha256=Hex('9')};
            Assert.False(CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,changed,catalog,f.OrdinarySelector).IsSuccess);
            Assert.False(File.Exists(f.OrdinaryHistory));
        }
        using var generic=new Fixture();
        using var legacy=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(generic.ModelRoot,V2Binding(generic),4));
        Assert.Equal(21,legacy.Session.CapacityV2!.MaximumFrames);Assert.Equal(75577,legacy.Session.CapacityV2.MaximumBytes);

        // Re-freeze genuine owner-derived dependencies; only bounded grant identity text changes.
        using(var f=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(10000,2))
        {
            f.ProvisionOrdinaryV2(5,objectiveId:new string('中',128),genuineTeachingCurriculum:CodeDocumentationPreparedBaselineTests.SelectionCurriculum(f));
            var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(f.RepositoryRoot));
            foreach(var size in new[]{1928,1929})
            {
                var grant=f.OrdinaryGrant with {GrantId=new string('\\',size-1923)+new string('g',128-(size-1923)),
                    GrantVersion=new string('v',64),TrainingRunId=new string('r',128)};
                var catalog=CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([grant],10000);
                grant=Require(catalog.Resolve(f.OrdinarySelector));var binding=StudentBinding(grant);
                Assert.Equal(size,CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(SelectionProjection(binding))).Length);
                using(Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,binding,10000,grant))) { }
                var before=StudentOwnedFiles(f.EnumerateOwnedOrdinaryFiles());
                var opened=CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,binding,catalog,f.OrdinarySelector);
                if(size==1928) {using var owner=Require(opened);Assert.Equal(40,owner.Session.CapacityV2!.MaximumFrames);Assert.Equal((0,0,false),owner.Session.SelectedStudentAccounting);}
                else {Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,opened.ErrorCode);Assert.Null(opened.Value);}
                AssertStudentOwnedFiles(before,f.EnumerateOwnedOrdinaryFiles());Assert.False(File.Exists(f.OrdinaryHistory));Assert.False(File.Exists(f.OrdinaryBaseline));
            }
        }
        // A genuine nineteen-call evaluator admits eight paid calls and the private one-response lease.
        {
            using var f=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(19,2);
            f.ProvisionOrdinaryV2(2,genuineTeachingCurriculum:CodeDocumentationPreparedBaselineTests.SelectionCurriculum(f));
            var catalog=ContinuousLearningModelServerStartup.Create(f.ReadEnvironment).OneLessonGrantCatalog;
            var grant=Require(catalog.Resolve(f.OrdinarySelector));Assert.Equal(19,grant.EvaluatorSelection!.Evaluator.CurrentConfiguration.MaxTeacherCalls);
            Assert.Equal(4,grant.EvaluationPlanSelection!.ExpectedEvaluationCallCount);
            var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(f.RepositoryRoot));
            using(var owner=Require(CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,StudentBinding(grant),catalog,f.OrdinarySelector)))
            {Assert.Equal(4,owner.Session.CapacityV2!.SlotCount);Assert.Equal(28,owner.Session.CapacityV2.MaximumFrames);Assert.Equal((0,0,false),owner.Session.SelectedStudentAccounting);}
            Assert.False(File.Exists(f.OrdinaryHistory));Assert.False(File.Exists(f.OrdinaryBaseline));Assert.False(File.Exists(f.OrdinaryClaim));
        }
        // Twenty paid calls cannot produce a genuine nineteen-call evaluator or grant; its owner refuses before Session capacity.
        {
            using var f=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(20,2);
            f.ProvisionOrdinaryV2(5,genuineTeachingCurriculum:CodeDocumentationPreparedBaselineTests.SelectionCurriculum(f));
            var catalog=ContinuousLearningModelServerStartup.Create(f.ReadEnvironment).OneLessonGrantCatalog;
            var grant=Require(catalog.Resolve(f.OrdinarySelector));var evaluator=grant.EvaluatorSelection!.Evaluator;
            Assert.Equal(20,evaluator.CurrentConfiguration.MaxTeacherCalls);Assert.Equal(10,grant.EvaluationPlanSelection!.ExpectedEvaluationCallCount);
            Assert.True(CodeDocumentationOneLessonEvaluatorSelection.Create(grant.CorpusSelection,grant.EvaluationPlanSelection,evaluator).IsSuccess);
            var before=StudentOwnedFiles(f.EnumerateOwnedOrdinaryFiles());
            var refused=CodeDocumentationOneLessonEvaluatorSelection.Create(grant.CorpusSelection,grant.EvaluationPlanSelection,
                evaluator with {CurrentConfiguration=evaluator.CurrentConfiguration with {MaxTeacherCalls=19}});
            Assert.Equal(CodeDocumentationOneLessonEvaluatorSelection.InvalidErrorCode,refused.ErrorCode);Assert.Null(refused.Value);
            AssertStudentOwnedFiles(before,f.EnumerateOwnedOrdinaryFiles());Assert.False(File.Exists(f.OrdinaryHistory));
            Assert.False(File.Exists(f.OrdinaryBaseline));Assert.False(File.Exists(f.OrdinaryClaim));
        }
        using(var h=CodeDocumentationPreparedBaselineTests.SelectedMaterialHarness())
        {
            var baseline=Require(await h.Consume());var admission=CodeDocumentationPreparedBaselineTests.SelectedMaterialAdmission(h,baseline);
            await CodeDocumentationPreparedBaselineTests.BindSelectedMaterial(h,baseline,admission);
            Assert.True(CodeDocumentationPreparedBaselineTests.ConsumeSelectedMaterial(h,baseline,admission).IsSuccess);
            var bytes=h.HistoryBytes();Assert.Equal(2,bytes.Count(x=>x==(byte)'\n'));var before=h.Files();var binding=h.Open.Session.Snapshot.Binding;h.Open.Dispose();
            var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(h.Fixture.RepositoryRoot));
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,
                CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,binding,h.Catalog,h.Fixture.OrdinarySelector).ErrorCode);
            h.AssertFiles(before);Assert.Equal(bytes,h.HistoryBytes());Assert.Equal(10,h.Client.Calls);
            Require(h.Reopen());Assert.True(CodeDocumentationPreparedBaselineTests.ConsumeSelectedMaterial(h,Require(await h.Consume()),admission).IsSuccess);
        }
        using(var f=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(10000,2))
        {
            f.ProvisionOrdinaryV2(5,genuineTeachingCurriculum:CodeDocumentationPreparedBaselineTests.SelectionCurriculum(f));
            var catalog=ContinuousLearningModelServerStartup.Create(f.ReadEnvironment).OneLessonGrantCatalog;var binding=StudentBinding(Require(catalog.Resolve(f.OrdinarySelector)));
            var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(f.RepositoryRoot));
            using(Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,binding,10000))) { }
            using(var owned=SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(f.OrdinaryHistory,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.Read,FileOptions.WriteThrough))
            {owned.Write(V2Frame(binding));owned.Flush(flushToDisk:true);}
            using(var control=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,binding,10000))) Assert.Equal(1,control.Session.CapacityV2!.PrefixFrames);
            var before=StudentOwnedFiles(f.EnumerateOwnedOrdinaryFiles());
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,
                CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,binding,catalog,f.OrdinarySelector).ErrorCode);
            AssertStudentOwnedFiles(before,f.EnumerateOwnedOrdinaryFiles());Assert.False(File.Exists(f.OrdinaryBaseline));Assert.False(File.Exists(f.OrdinaryClaim));
            using var restored=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,binding,10000));Assert.Equal(1,restored.Session.CapacityV2!.PrefixFrames);
        }
    }

    private static CodeDocumentationOneLessonSessionBinding StudentBinding(CodeDocumentationOneLessonGrantV1 grant)
        => new(grant.ObjectiveId,grant.GrantId,grant.GrantVersion,grant.TrainingRunId,grant.CandidateSha256,
            grant.ParentManifestSha256,grant.ParentWeightsSha256,grant.ConfigurationSha256,grant.ExamConditionSha256)
            {FormatVersion=2,DescriptorsV2=grant.DescriptorsV2};
    private static Dictionary<string,(byte[] Bytes,long Ticks)> StudentOwnedFiles(IEnumerable<string> paths)
        => paths.ToDictionary(p=>p,p=>(File.ReadAllBytes(p),File.GetLastWriteTimeUtc(p).Ticks));
    private static Dictionary<string,(byte[] Bytes,long Ticks)> StudentOwnedFiles(
        IEnumerable<string> paths, CodeDocumentationOneLessonSession owner)
    {
        // Read only this Session's retained exclusive lock through its genuine handle; every other file still uses its pathname.
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var lockPath = Assert.IsType<string>(typeof(CodeDocumentationOneLessonSession).GetField("_lockPath", fields)!.GetValue(owner));
        var heldLock = Assert.IsAssignableFrom<FileStream>(typeof(CodeDocumentationOneLessonSession).GetField("_ownerLock", fields)!.GetValue(owner));
        var allPaths = paths.ToArray();
        Assert.Contains(lockPath, allPaths);
        Assert.True(heldLock.CanRead && heldLock.CanSeek);
        Assert.False(heldLock.SafeFileHandle.IsClosed);
        Assert.False(heldLock.SafeFileHandle.IsInvalid);
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(lockPath));
        Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(heldLock.SafeFileHandle, lockPath));
        var position = heldLock.Position;
        var ticks = File.GetLastWriteTimeUtc(lockPath).Ticks;
        var lockBytes = new byte[checked((int)heldLock.Length)];
        try
        {
            heldLock.Position = 0;
            heldLock.ReadExactly(lockBytes);
            Assert.Equal(lockBytes.LongLength, heldLock.Length);
        }
        finally { heldLock.Position = position; }
        Assert.Equal(position, heldLock.Position);
        Assert.Equal(ticks, File.GetLastWriteTimeUtc(lockPath).Ticks);
        Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(heldLock.SafeFileHandle, lockPath));
        Assert.Equal(Encoding.UTF8.GetBytes("s04-one-lesson-session-v2\n" + owner.Snapshot.SessionId + "\n"), lockBytes);
        return allPaths.ToDictionary(p => p, p =>
            (string.Equals(p, lockPath, StringComparison.Ordinal) ? lockBytes : File.ReadAllBytes(p), File.GetLastWriteTimeUtc(p).Ticks));
    }
    private static void AssertStudentOwnedFiles(Dictionary<string,(byte[] Bytes,long Ticks)> before,IEnumerable<string> paths)
    {
        var after=StudentOwnedFiles(paths);Assert.Equal(before.Keys.Order(),after.Keys.Order());
        foreach(var path in before.Keys) {Assert.Equal(before[path].Bytes,after[path].Bytes);Assert.Equal(before[path].Ticks,after[path].Ticks);}
    }
    private static void AssertStudentOwnedFiles(Dictionary<string,(byte[] Bytes,long Ticks)> before,
        IEnumerable<string> paths, CodeDocumentationOneLessonSession owner)
    {
        var after=StudentOwnedFiles(paths,owner);Assert.Equal(before.Keys.Order(),after.Keys.Order());
        foreach(var path in before.Keys) {Assert.Equal(before[path].Bytes,after[path].Bytes);Assert.Equal(before[path].Ticks,after[path].Ticks);}
    }

    private static CodeDocumentationSelectedStudentRequestV1 StudentSyntaxRequest(CodeDocumentationOneLessonBindingProjectionV2 b,
        string task="Teach branch Y",CodeDocumentationSelectedStudentSettingsV1? settings=null)
    {
        settings ??=new("1","00000000","1","3f800000","0","2","65536");
        var f=Hex('f');
        return new("code-documentation-selected-student-input-v1","SyntheticTest",CodeDocumentationOneLessonV2Wire.SelectedStudentLeaseSha256(),
            CodeDocumentationOneLessonV2Wire.SelectedStudentBindingSha256(b),f,new string('f',32),f,f,f,f,
            "item-teach-2-0","teach-2","sha256:"+f,"sha256:"+f,task,Encoding.UTF8.GetByteCount(task).ToString(System.Globalization.CultureInfo.InvariantCulture),
            Sha256(task),f,f,f,f,f,f,settings,CodeDocumentationOneLessonV2Wire.SelectedStudentSettingsSha256(settings));
    }

    [Fact]
    public async Task SyntheticStudentWire_ClosedCanonicalBytesHashesAndCeilings()
    {
        var f=Hex('f');var b=new CodeDocumentationOneLessonBindingProjectionV2("syntax-objective","syntax-grant","v2","syntax-run",f,f,f,f,f,new(2,f,f,f,f,f,2,5,f,f));
        var request=StudentSyntaxRequest(b);
        Assert.Equal("04ba25692562c4607ce2a534cecc631eb820d23615fafdc01df256dcf7578b66",CodeDocumentationOneLessonV2Wire.SelectedStudentLeaseSha256());
        Assert.Equal("1c8fe6644beb8a612cdeb1b9e7db3a94af2697bd93f39970a46c450c2708c6cd",CodeDocumentationOneLessonV2Wire.SelectedStudentSettingsSha256(request.GenerationSettings));
        Assert.Equal("c9b2c05e117e251769e00bc9a46758f4fc83c4e3683e8a76963ea36c0d218d8d",CodeDocumentationOneLessonV2Wire.SelectedStudentRequestSha256(request));
        var started=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentStartedFrameBytes(b,f,request);
        Assert.Equal(3963,started.Length);var startedHash=CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(started.AsSpan(0,started.Length-1));
        Assert.Equal("b8b2ab3ad17664e2277c4856c6d67e28c8d8e3cee821b6247585d12a788e6161",startedHash);
        Assert.Equal(request,Require(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(started,b)).Request);
        var completed=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentCompletedFrameBytes(b,f,request,startedHash,"b",f);
        Assert.Equal(2373,completed.Length);Assert.Equal("803b8504b62c3fc693bb63f097c15f69d523c4a9254b9fba0e96e0d353af3580",CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(completed.AsSpan(0,completed.Length-1)));
        Assert.Equal("b",Require(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(completed,b,request,startedHash)).Response);
        var good=Encoding.UTF8.GetString(started);
        foreach(var bad in new[] { good[..^1], " "+good, good.Replace("\"Revision\":2","\"Revision\":\"2\"",StringComparison.Ordinal),
            good.Replace("\"Kind\":","\"Other\":null,\"Kind\":",StringComparison.Ordinal),good.Replace("\"Kind\":","\"Kind\":\"student-response\",\"Kind\":",StringComparison.Ordinal),
            good.Replace("\"Task\":\"Teach branch Y\"","\"Task\":null",StringComparison.Ordinal),good.Replace("\"Seed\":\"0\"","\"Seed\":\"+0\"",StringComparison.Ordinal),
            good.Replace("\"Temperature\":\"00000000\"","\"Temperature\":\"7fc00000\"",StringComparison.Ordinal),good.Replace("\"TaskUtf8ByteLength\":\"14\"","\"TaskUtf8ByteLength\":\"014\"",StringComparison.Ordinal),
            good.Replace("\"SyntheticTest\"","\"Native\"",StringComparison.Ordinal),good.Replace("\n","\r\n",StringComparison.Ordinal) })
        {Assert.NotEqual(good,bad);Assert.False(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(Encoding.UTF8.GetBytes(bad),b).IsSuccess,bad);}
        var raw="\0中文\r\n\t\u0001";var full=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentCompletedFrameBytes(b,f,request,startedHash,raw,f);
        Assert.Equal(raw,Require(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(full,b,request,startedHash)).Response);
        Assert.False(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(full,b,request,Hex('1')).IsSuccess);
        var max=StudentSyntaxRequest(b,new string('\t',65536),new("2048","40000000","10000","3f800000","-2147483648","200","65536")) with {ItemId=new string('i',128),SourceId=new string('s',128)};
        Assert.True(CodeDocumentationOneLessonV2Wire.ValidSelectedStudentRequest(max));var maxStarted=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentStartedFrameBytes(b,f,max);
        var maxStartedHash=CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(maxStarted.AsSpan(0,maxStarted.Length-1));
        var maxCompleted=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentCompletedFrameBytes(b,f,max,maxStartedHash,new string('\0',65536),f);
        Assert.InRange(maxStarted.Length,131072,136139);Assert.InRange(maxCompleted.Length,393216,396458);
        var commonBindingBytes=CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(b)).Length;
        Assert.Equal(136139,maxStarted.Length+2120-commonBindingBytes); // Actual canonical C# writer cross-checks the admitted common binding ceiling.
        Assert.Equal(396458,maxCompleted.Length+2120-commonBindingBytes+1); // NUL maximizes JSON escaping; add the sixth UTF8-length decimal digit for the independent196608-byte bound.
        Assert.True(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(maxStarted,b).IsSuccess);
        Assert.True(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(maxCompleted,b,max,maxStartedHash).IsSuccess);
        Assert.False(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(new byte[136140],b).IsSuccess);
        Assert.False(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(new byte[396459],b,max,maxStartedHash).IsSuccess);
        Assert.False(CodeDocumentationOneLessonV2Wire.ValidSelectedStudentRequest(max with {Task=max.Task+"\t"}));
        Assert.False(CodeDocumentationOneLessonV2Wire.ValidSelectedStudentResponse(max,new string('x',65537),f,out _));
        using var array=JsonDocument.Parse("[1]");Assert.Throws<JsonException>(()=>CodeDocumentationOneLessonV2Wire.CanonicalBytes(array.RootElement));

        // A valid ordinary owner first proves the two-frame prefix; large Student kinds confer no private reader mode.
        using(var h=CodeDocumentationPreparedBaselineTests.SelectedMaterialHarness())
        {
            var baseline=Require(await h.Consume());var admission=CodeDocumentationPreparedBaselineTests.SelectedMaterialAdmission(h,baseline);
            await CodeDocumentationPreparedBaselineTests.BindSelectedMaterial(h,baseline,admission);
            Assert.True(CodeDocumentationPreparedBaselineTests.ConsumeSelectedMaterial(h,baseline,admission).IsSuccess);
            var binding=h.Open.Session.Snapshot.Binding;var prefix=h.HistoryBytes();var before=h.Files();var maximumBytes=h.Open.Session.CapacityV2!.MaximumBytes;h.Open.Dispose();
            var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(h.Fixture.RepositoryRoot));
            var projection=SelectionProjection(binding);var largeRequest=StudentSyntaxRequest(projection,new string('\t',20000));
            var largeStarted=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentStartedFrameBytes(projection,Sha256(binding.ObjectiveId),largeRequest);
            var largeCompleted=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentCompletedFrameBytes(projection,Sha256(binding.ObjectiveId),largeRequest,
                CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(largeStarted.AsSpan(0,largeStarted.Length-1)),new string('\0',6000),Hex('f'));
            Assert.True(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(largeStarted,projection).IsSuccess);
            Assert.True(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(largeCompleted,projection,largeRequest,
                CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(largeStarted.AsSpan(0,largeStarted.Length-1))).IsSuccess);
            foreach(var large in new[]{largeStarted,largeCompleted})
            {
                Assert.InRange(large.Length,32769,396458);
                foreach(var invalidUtf8 in new[]{false,true})
                {
                    var frame=large.ToArray();if(invalidUtf8)frame[1]=0xff;var fault=prefix.Concat(frame).ToArray();Assert.InRange(fault.LongLength,1L,maximumBytes);Exception? primary=null;
                    try
                    {
                        CodeDocumentationPreparedBaselineTests.RestoreCustodyFile(h.Fixture.OrdinaryHistory,fault,new DateTime(before[h.Fixture.OrdinaryHistory].Ticks,DateTimeKind.Utc));
                        var faultFiles=h.Files();var refused=CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,binding,h.Catalog.MaxTeacherCalls,h.Grant);
                        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,refused.ErrorCode);Assert.Null(refused.Value);
                        h.AssertFiles(faultFiles);Assert.Equal(fault,h.HistoryBytes());Assert.Equal(10,h.Client.Calls);
                    }
                    catch(Exception failure){primary=failure;throw;}
                    finally{CodeDocumentationPreparedBaselineTests.RestoreFixtureAfterFault(()=>CodeDocumentationPreparedBaselineTests.RestoreCustodyFile(h.Fixture.OrdinaryHistory,prefix,new DateTime(before[h.Fixture.OrdinaryHistory].Ticks,DateTimeKind.Utc)),primary);}
                }
            }
            h.AssertFiles(before);Require(h.Reopen());Assert.True(CodeDocumentationPreparedBaselineTests.ConsumeSelectedMaterial(h,Require(await h.Consume()),admission).IsSuccess);Assert.Equal(10,h.Client.Calls);
        }
        using(var generic=new Fixture())
        {
            var binding=V2Binding(generic);var path=SeedV2Genesis(generic,binding);var prefix=File.ReadAllBytes(path);var ticks=File.GetLastWriteTimeUtc(path);
            using(var control=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(generic.ModelRoot,binding,4))) Assert.Equal(1,control.Session.CapacityV2!.PrefixFrames);
            var projection=SelectionProjection(binding);var largeRequest=StudentSyntaxRequest(projection,new string('\t',20000));
            var largeStarted=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentStartedFrameBytes(projection,Sha256(binding.ObjectiveId),largeRequest);
            var largeCompleted=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentCompletedFrameBytes(projection,Sha256(binding.ObjectiveId),largeRequest,
                CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(largeStarted.AsSpan(0,largeStarted.Length-1)),new string('\0',6000),Hex('f'));
            Assert.True(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(largeStarted,projection).IsSuccess);
            Assert.True(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentCompleted(largeCompleted,projection,largeRequest,
                CodeDocumentationOneLessonV2Wire.SelectedStudentBytesSha256(largeStarted.AsSpan(0,largeStarted.Length-1))).IsSuccess);
            foreach(var large in new[]{largeStarted,largeCompleted}) foreach(var invalidUtf8 in new[]{false,true})
            {
                Assert.InRange(large.Length,32769,396458);var frame=large.ToArray();if(invalidUtf8)frame[1]=0xff;var fault=prefix.Concat(frame).ToArray();Assert.InRange(fault.LongLength,1L,75577L);Exception? primary=null;
                try
                {
                    CodeDocumentationPreparedBaselineTests.RestoreCustodyFile(path,fault,ticks);
                    var before=StudentOwnedFiles(SnapshotFiles(generic.Repository).Select(p=>Path.Combine(generic.Repository,p)));
                    var refused=CodeDocumentationOneLessonSession.OpenCapacityValidationV2(generic.ModelRoot,binding,4);
                    Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,refused.ErrorCode);Assert.Null(refused.Value);
                    AssertStudentOwnedFiles(before,SnapshotFiles(generic.Repository).Select(p=>Path.Combine(generic.Repository,p)));Assert.Equal(fault,File.ReadAllBytes(path));
                }
                catch(Exception failure){primary=failure;throw;}
                finally{CodeDocumentationPreparedBaselineTests.RestoreFixtureAfterFault(()=>CodeDocumentationPreparedBaselineTests.RestoreCustodyFile(path,prefix,ticks),primary);}
            }
            using var restored=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(generic.ModelRoot,binding,4));Assert.Equal(1,restored.Session.CapacityV2!.PrefixFrames);Assert.Equal(prefix,File.ReadAllBytes(path));
        }
    }

    [Fact]
    public async Task SyntheticStudentResponse_ConcurrentOwnersHaveOneReservationAndNoReentry()
    {
        using var h=CodeDocumentationPreparedBaselineTests.SelectedStudentHarness();
        var (b,a)=await CodeDocumentationPreparedBaselineTests.BindSelectedStudent(h);var entered=0;var refused=0;
        h.Open.Session.SelectedStudentDurablePointForTesting=stage=>
        {
            if(stage!="student-before-generate")return;entered++;
            var reentry=CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a).GetAwaiter().GetResult();
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,reentry.ErrorCode);refused++;
            var competing=Task.Run(()=>CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a)).GetAwaiter().GetResult();
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,competing.ErrorCode);refused++;
            var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(h.Fixture.RepositoryRoot));
            Assert.False(CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,h.Open.Session.Snapshot.Binding,h.Catalog,h.Fixture.OrdinarySelector).IsSuccess);
        };
        var actual=await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a);Assert.True(actual.IsSuccess,actual.ErrorMessage);
        Assert.Equal("b",actual.Value!.StudentResponse);Assert.Equal(1,entered);Assert.Equal(2,refused);
        Assert.Equal((1,1,false),h.Open.Session.SelectedStudentAccounting);Assert.Equal(10,h.Client.Calls);
        h.Open.Session.SelectedStudentDurablePointForTesting=stage=>{if(stage.Contains("generate",StringComparison.Ordinal))throw new InvalidOperationException("replay Generate");};
        Assert.True((await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a)).IsSuccess);
        h.Open.Dispose();Assert.False((await CodeDocumentationPreparedBaselineTests.ConsumeSelectedStudent(h,b,a)).IsSuccess);
    }

    private static CodeDocumentationOneLessonBindingProjectionV2 SelectionProjection(CodeDocumentationOneLessonSessionBinding b)
        => new(b.ObjectiveId,b.GrantId,b.GrantVersion,b.TrainingRunId,b.CandidateSha256,b.ParentManifestSha256,
            b.ParentWeightsSha256,b.ConfigurationSha256,b.ExamConditionSha256,b.DescriptorsV2!);
    private static CodeDocumentationTeachingSelectionPayloadV1 SelectionPayload(string reason="选择 teaching identity.",IReadOnlyList<string>? categories=null,
        string? item=null,string? source=null,string? invocation=null)
        => Require(CodeDocumentationTeachingSelectionPayloadV1.Capture(new string('a',32),Hex('b'),Hex('c'),categories??
            new[]{"unclear_or_unverifiable","missing_essential_behavior","invented_behavior","wrong_purpose_or_result"},
            item??"item-1",source??"source-1","sha256:"+Hex('d'),"sha256:"+Hex('e'),reason,Hex('f'),Hex('0'),
            invocation??"invocation-1",Hex('1'),Hex('2')));

    [Fact]
    public void SelectionBindingWire_ExactOrderedCategoriesRoundTripAndDefaultArraysRemainRefused()
    {
        using var f=new Fixture(); var binding=V2Binding(f); var projection=SelectionProjection(binding);
        var categories=new[]{"unclear_or_unverifiable","missing_essential_behavior"}; var p=SelectionPayload(categories:categories);
        categories[0]="invented_behavior";
        var bytes=CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(projection,Hex('3'),Hex('4'),Hex('5'),p);
        var decoded=Require(CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(bytes,projection));
        Assert.Equal(new[]{"unclear_or_unverifiable","missing_essential_behavior"},decoded.Payload.Categories);
        Assert.Equal(bytes,CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(projection,decoded.SessionId,
            decoded.BindingSha256,decoded.GenesisFrameSha256,decoded.Payload));
        using var array=JsonDocument.Parse("[\"missing_essential_behavior\"]");
        Assert.Throws<JsonException>(()=>CodeDocumentationOneLessonV2Wire.CanonicalBytes(array.RootElement));
        var original=Encoding.UTF8.GetBytes("{\"Categories\":[\"missing_essential_behavior\"]}");
        var input=new CodeDocumentationTeachingSelectionInputV1(new string('a',32),Hex('b'),Hex('c'),["missing_essential_behavior"],original);
        var frozen=input.FrozenContextSha256; original[0]^=1; var detached=input.CopyBytes(); detached[0]^=1;
        Assert.Equal(frozen,input.FrozenContextSha256); Assert.Equal((byte)'{',input.CopyBytes()[0]);
    }

    [Fact]
    public void SelectionBindingWire_RejectsUnknownDuplicateNoncanonicalAndWrongArrayPaths()
    {
        using var f=new Fixture(); var projection=SelectionProjection(V2Binding(f));
        var bytes=CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(projection,Hex('3'),Hex('4'),Hex('5'),SelectionPayload());
        var good=Encoding.UTF8.GetString(bytes);
        var faults=new[]{good.TrimEnd('\n'),good.Replace("\"Revision\":1","\"Revision\":\"1\"",StringComparison.Ordinal),
            good.Replace("\"Revision\":1","\"Revision\":1.0",StringComparison.Ordinal),
            good.Replace("\"Kind\":","\"Unknown\":[],\"Kind\":",StringComparison.Ordinal),
            good.Replace("\"Kind\":","\"Kind\":\"TeachingSelectionBinding\",\"Kind\":",StringComparison.Ordinal),
            good.Replace("\"Reason\":\"选择 teaching identity.\"","\"Reason\":[]",StringComparison.Ordinal),
            good.Replace("\"OptimizerEntries\":\"0\"","\"OptimizerEntries\":\"1\"",StringComparison.Ordinal),
            good.Replace("\"SyntheticTest\"","\"Native\"",StringComparison.Ordinal),
            good.Replace("\"unclear_or_unverifiable\"","\"missing_essential_behavior\"",StringComparison.Ordinal),
            good.Replace("\"SourceSha256\":","\"OtherArray\":[],\"SourceSha256\":",StringComparison.Ordinal)," "+good,good.Replace("\n","\r\n",StringComparison.Ordinal)};
        foreach(var raw in faults)
        {Assert.NotEqual(good,raw); var result=CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(Encoding.UTF8.GetBytes(raw),projection); Assert.False(result.IsSuccess,raw); Assert.Null(result.Value);}
        var invalidUtf8=bytes.ToArray(); invalidUtf8[1]=0xff;
        Assert.False(CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(invalidUtf8,projection).IsSuccess);
        Assert.False(CodeDocumentationTeachingSelectionPayloadV1.Capture(new string('a',32),Hex('b'),Hex('c'),["correct"],"item","source",
            "sha256:"+Hex('d'),"sha256:"+Hex('e'),"reason",Hex('f'),Hex('0'),"invocation",Hex('1'),Hex('2')).IsSuccess);
        Assert.True(CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(bytes,projection).IsSuccess);
    }

    [Fact]
    public void SelectionBindingWire_MaxLfInclusiveFrameUsesExactCanonicalWriter()
    {
        using var f=new Fixture(); var b=V2Binding(f) with {ObjectiveId=new string('o',128),GrantId=new string('g',128),
            GrantVersion=new string('v',64),TrainingRunId=new string('r',128)};
        var projection=SelectionProjection(b); var p=SelectionPayload(new string('\0',4096),item:new string('i',128),source:new string('s',128),invocation:new string('n',128));
        var bytes=CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(projection,Hex('3'),Hex('4'),Hex('5'),p);
        Assert.Equal((byte)'\n',bytes[^1]); Assert.InRange(bytes.Length,24576,32768);
        var decoded=Require(CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(bytes,projection));
        Assert.Equal(p.Reason,decoded.Payload.Reason); Assert.Equal(bytes,CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(projection,
            decoded.SessionId,decoded.BindingSha256,decoded.GenesisFrameSha256,decoded.Payload));
        Assert.False(CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(new byte[32769],projection).IsSuccess);
        Assert.Throws<JsonException>(()=>CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(projection with {ObjectiveId=new string('x',129)},Hex('3'),Hex('4'),Hex('5'),p));
        Assert.False(CodeDocumentationTeachingSelectionPayloadV1.Capture(new string('a',32),Hex('b'),Hex('c'),p.Categories,p.ItemId,p.SourceId,
            p.ReferenceEntryId,p.SourceEntityFingerprint,new string('x',4097),p.FrozenContextSha256,p.MaterialAdmissionEvidenceSha256,p.InvocationId,p.LaunchSettingsSha256,p.LaunchAckSha256).IsSuccess);
    }

    [Fact]
    public void OrdinaryCapacityV2_NewSelectionReservationPreservesGenericProbeAndLegacyHeader()
    {
        foreach(var row in new[]{(Cases:1,Repeats:2,Calls:4,Slots:2,Frames:22,Bytes:108345L),
            (Cases:200,Repeats:25,Calls:10000,Slots:5000,Frames:10018,Bytes:32835249L),
            (Cases:1,Repeats:5000,Calls:10000,Slots:5000,Frames:10018,Bytes:32835249L),
            (Cases:2,Repeats:2500,Calls:10000,Slots:5000,Frames:10018,Bytes:32835249L)})
        {
            using var genericFixture=new Fixture();
            using(var generic=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(genericFixture.ModelRoot,V2Binding(genericFixture,row.Cases,row.Repeats),row.Calls)))
            {Assert.Equal(row.Frames-1,generic.Session.CapacityV2!.MaximumFrames); Assert.Equal(row.Bytes-32768,generic.Session.CapacityV2.MaximumBytes);}
            using var f=new CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture(row.Calls,row.Cases);
            f.ProvisionOrdinaryV2(row.Repeats);
            var heldOut=f.Corpus.Corpus.Entries.Where(e=>e.Partition==FrozenReferencePartition.HeldOut).ToArray();
            Assert.Equal(row.Cases,heldOut.Length);
            Assert.Equal(row.Cases,heldOut.Select(e=>e.EntryId).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(row.Cases,heldOut.Select(e=>e.PaidReferenceProvenance.RequestId).Distinct(StringComparer.Ordinal).Count());
            var catalog=ContinuousLearningModelServerStartup.Create(f.ReadEnvironment).OneLessonGrantCatalog;
            var grant=Require(catalog.Resolve(f.OrdinarySelector));
            Assert.NotNull(grant.ProfileSelection); Assert.NotNull(grant.CheckpointSelection);
            Assert.Equal(row.Cases,f.Profile.GenerationSettings.MaxHeldOutCases);
            Assert.Equal(row.Cases,grant.EvaluatorSelection!.Evaluator.GenerationSettings.MaxHeldOutCases);
            Assert.Equal(row.Cases,grant.DescriptorsV2!.CaseCount); Assert.Equal(row.Repeats,grant.DescriptorsV2.RepeatCount);
            Assert.Equal(row.Slots,grant.EvaluationPlanSelection!.ExpectedEvaluationCallCount);
            var b=new CodeDocumentationOneLessonSessionBinding(grant.ObjectiveId,grant.GrantId,grant.GrantVersion,grant.TrainingRunId,
                grant.CandidateSha256,grant.ParentManifestSha256,grant.ParentWeightsSha256,grant.ConfigurationSha256,grant.ExamConditionSha256)
                {FormatVersion=2,DescriptorsV2=grant.DescriptorsV2};
            var root=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(f.RepositoryRoot));
            using var ordinary=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,b,row.Calls,grant));
            var capacity=Require(ordinary.Session.ReadCapacityPrefixV2()); Assert.Equal(row.Slots,capacity.SlotCount);
            Assert.Equal(row.Frames,capacity.MaximumFrames); Assert.Equal(row.Bytes,capacity.MaximumBytes);
            Assert.True(ordinary.Session.ValidateContinuationCapacityV2(0,0,row.Frames,row.Bytes).IsSuccess);
            Assert.False(ordinary.Session.ValidateContinuationCapacityV2(0,0,row.Frames+1,row.Bytes).IsSuccess);
            Assert.False(ordinary.Session.ValidateContinuationCapacityV2(0,0,row.Frames,row.Bytes+1).IsSuccess);
            var lockPath=Path.Combine(f.RepositoryRoot,Path.GetDirectoryName(ordinary.Session.Snapshot.HistoryRelativePath)!,".owner.lock");
            var expectedHeader="s04-one-lesson-session-v2\n"+ordinary.Session.Snapshot.SessionId+"\n";
            Assert.False(File.Exists(f.OrdinaryHistory));
            ordinary.Dispose(); Assert.Equal(expectedHeader,File.ReadAllText(lockPath));
        }
    }

    [Theory]
    [InlineData(1, 2, 4, "BASELINE_PREREQUISITE_UNAVAILABLE")]
    [InlineData(200, 25, 10000, "BASELINE_PREREQUISITE_UNAVAILABLE")]
    [InlineData(200, 26, 10000, "CAPACITY_EXCEEDED")]
    [InlineData(1, 2, 3, "CAPACITY_EXCEEDED")]
    public async Task CapacityV2_MappedStartupCatalogReachesSessionGuardBeforeGenesisOrEffects(
        int cases, int repeats, int budget, string expectedSuffix)
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture, cases, repeats);
        var catalogReads = 0;
        var startup = ContinuousLearningModelServerStartup.Create(name =>
        {
            if (name == CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable)
            { catalogReads++; return V2CatalogJson(binding); }
            return name == "XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS" ? budget.ToString() : null;
        });
        var catalog = startup.OneLessonGrantCatalog;
        var selected = Require(catalog.Resolve(new(binding.ObjectiveId, "opaque-grant-01")));
        Assert.Equal(2, selected.FormatVersion);
        Assert.Equal(binding.DescriptorsV2, selected.DescriptorsV2);
        Assert.Equal(budget, catalog.MaxTeacherCalls);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(startup);
        await using var app = builder.Build();
        app.MapCodeDocumentationOneLessonPreflight(fixture.Repository);
        Assert.Same(catalog, app.Services.GetRequiredService<ContinuousLearningModelServerStartup>().OneLessonGrantCatalog);
        await app.StartAsync();
        try
        {
            var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            using var client = new HttpClient { BaseAddress = new Uri(Assert.Single(addresses!.Addresses)) };
            var request = new CodeDocumentationOneLessonPreflightRequestV1(new(binding.ObjectiveId, "opaque-grant-01"));
            for (var i = 0; i < 2; i++)
            {
                using var response = await client.PostAsJsonAsync(CodeDocumentationOneLessonPreflightWireContract.EndpointPath, request);
                var body = await response.Content.ReadFromJsonAsync<CodeDocumentationOneLessonPreflightResponseV1>();
                Assert.NotNull(body);
                Assert.False(body.IsSuccess);
                Assert.Equal(CodeDocumentationOneLessonSession.ErrorPrefix + expectedSuffix, body.ErrorCode);
                Assert.Null(body.TerminalOutcome);
                Assert.Equal(503, (int)response.StatusCode);
            }
            Assert.Equal(1, catalogReads);
            Assert.DoesNotContain(SnapshotFiles(fixture.Repository), x => x.EndsWith("session.jsonl", StringComparison.Ordinal));
            // The mapped preflight never constructs an effect service, writes a
            // model/pair/baseline, or changes durable effect accounting.
            Assert.Empty(Directory.GetFiles(Path.Combine(fixture.Repository, "training-artifacts", "models"), "*.jsonl", SearchOption.AllDirectories));
        }
        finally { await app.StopAsync(); }
    }

    [Theory]
    [InlineData(1, 2, 4, 2, 21, 75577)]
    [InlineData(200, 25, 10000, 5000, 10017, 32802481)]
    public void CapacityV2_SameOwnerLeaseAcceptsExactLimitsRejectsEachOverageAndCheckedOverflow(
        int cases, int repeats, int calls, int slots, int frames, long bytes)
    {
        using var fixture = new Fixture();
        using var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, V2Binding(fixture, cases, repeats), calls));
        var capacity = Require(owner.Session.ReadCapacityPrefixV2());
        Assert.Equal(slots, capacity.SlotCount);
        Assert.Equal(frames, capacity.MaximumFrames);
        Assert.Equal(bytes, capacity.MaximumBytes);
        Assert.Equal(0, capacity.PrefixFrames);
        Assert.Equal(0, capacity.PrefixBytes);
        Assert.True(owner.Session.ValidateContinuationCapacityV2(0, 0, frames, bytes).IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,
            owner.Session.ValidateContinuationCapacityV2(0, 0, frames + 1, bytes).ErrorCode);
        Assert.True(owner.Session.ValidateContinuationCapacityV2(0, 0, frames, bytes).IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,
            owner.Session.ValidateContinuationCapacityV2(0, 0, frames, bytes + 1).ErrorCode);
        Assert.True(owner.Session.ValidateContinuationCapacityV2(0, 0, frames, bytes).IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityInvalidErrorCode,
            owner.Session.ValidateContinuationCapacityV2(int.MaxValue, 0, 1, 0).ErrorCode);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityInvalidErrorCode,
            owner.Session.ValidateContinuationCapacityV2(0, long.MaxValue, 0, 1).ErrorCode);
        Assert.True(owner.Session.ValidateContinuationCapacityV2(0, 0, frames, bytes).IsSuccess);
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), owner.Session.Snapshot.Effects);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, owner.Session.Advance(0, new()).ErrorCode);
        Assert.DoesNotContain(SnapshotFiles(fixture.Repository), x => x.EndsWith("session.jsonl", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0, 2, 10000, "CAPACITY_INVALID")]
    [InlineData(201, 2, 10000, "CAPACITY_INVALID")]
    [InlineData(1, 1, 10000, "CAPACITY_INVALID")]
    [InlineData(200, int.MaxValue, 10000, "CAPACITY_INVALID")]
    [InlineData(1, 2, 10001, "CAPACITY_INVALID")]
    [InlineData(1, 2, 3, "CAPACITY_EXCEEDED")]
    public void CapacityV2_InvalidCountsFailBeforeCreatingOwnerAndRestoreLegalControl(int cases, int repeats, int calls, string error)
    {
        using var fixture = new Fixture();
        var legal = V2Binding(fixture);
        using (Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, legal, 4))) { }
        var before = SnapshotFiles(fixture.Repository);
        var result = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, V2Binding(fixture, cases, repeats), calls);
        Assert.Equal(CodeDocumentationOneLessonSession.ErrorPrefix + error, result.ErrorCode);
        Assert.Equal(before, SnapshotFiles(fixture.Repository));
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, legal, 4));
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), restored.Session.Snapshot.Effects);
    }

    [Fact]
    public async Task CapacityV2_ConcurrentClaimantsUseExistingLockAndIntactReopenReconstructsLease()
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture);
        var path = SeedV2Genesis(fixture, binding);
        var original = File.ReadAllBytes(path);
        using var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        var capacity = Require(owner.Session.ReadCapacityPrefixV2());
        Assert.Equal(1, capacity.PrefixFrames);
        Assert.Equal(original.LongLength, capacity.PrefixBytes);
        var competitors = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4))));
        Assert.All(competitors, r => Assert.Equal(CodeDocumentationOneLessonSession.BusyErrorCode, r.ErrorCode));
        Assert.Equal(original, File.ReadAllBytes(path));
        Assert.True(owner.Session.ValidateContinuationCapacityV2(capacity.PrefixFrames, capacity.PrefixBytes,
            capacity.RemainingFrames, capacity.RemainingBytes).IsSuccess);
        owner.Dispose();
        using var reopened = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        Assert.Equal(capacity, Require(reopened.Session.ReadCapacityPrefixV2()));
        Assert.Equal(original, File.ReadAllBytes(path));
        Assert.Equal(CodeDocumentationOneLessonSession.BaselinePrerequisiteUnavailableErrorCode,
            CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, binding with { ObjectiveId = "new-prerequisite-objective" }, 4).ErrorCode);
    }

    [Theory]
    [InlineData("torn")]
    [InlineData("crlf")]
    [InlineData("bom")]
    [InlineData("invalid-utf8")]
    [InlineData("whitespace")]
    [InlineData("revision-zero-prefix")]
    [InlineData("revision-overflow")]
    [InlineData("format-number")]
    [InlineData("duplicate")]
    [InlineData("alias")]
    [InlineData("unknown")]
    [InlineData("null")]
    [InlineData("escaped-ascii")]
    [InlineData("binding-changed")]
    public void CapacityV2_RawCanonicalPrefixFaultPreservesBytesAndRestoresPositiveControl(string fault)
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture);
        var path = SeedV2Genesis(fixture, binding);
        var original = File.ReadAllBytes(path);
        using (Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4))) { }
        var text = Encoding.UTF8.GetString(original);
        var corrupt = fault switch
        {
            "torn" => original[..^1],
            "crlf" => Encoding.UTF8.GetBytes(text.Replace("\n", "\r\n", StringComparison.Ordinal)),
            "bom" => new byte[] { 0xef, 0xbb, 0xbf }.Concat(original).ToArray(),
            "invalid-utf8" => original[..^1].Concat(new byte[] { 0xff, 10 }).ToArray(),
            "whitespace" => Encoding.UTF8.GetBytes(" " + text),
            "revision-zero-prefix" => Encoding.UTF8.GetBytes(text.Replace("\"Revision\":\"0\"", "\"Revision\":\"00\"", StringComparison.Ordinal)),
            "revision-overflow" => Encoding.UTF8.GetBytes(text.Replace("\"Revision\":\"0\"", "\"Revision\":\"9223372036854775808\"", StringComparison.Ordinal)),
            "format-number" => Encoding.UTF8.GetBytes(text.Replace("\"FormatVersion\":2", "\"FormatVersion\":2.0", StringComparison.Ordinal)),
            "duplicate" => Encoding.UTF8.GetBytes(text.Replace("\"Kind\":", "\"Kind\":\"session\",\"Kind\":", StringComparison.Ordinal)),
            "alias" => Encoding.UTF8.GetBytes(text.Replace("\"Kind\":", "\"kind\":", StringComparison.Ordinal)),
            "unknown" => Encoding.UTF8.GetBytes(text.Replace("\"Kind\":", "\"Unknown\":true,\"Kind\":", StringComparison.Ordinal)),
            "null" => Encoding.UTF8.GetBytes(text.Replace("\"Revision\":\"0\"", "\"Revision\":null", StringComparison.Ordinal)),
            "escaped-ascii" => Encoding.UTF8.GetBytes(text.Replace("\"session\"", "\"\\u0073ession\"", StringComparison.Ordinal)),
            "binding-changed" => Encoding.UTF8.GetBytes(text.Replace(Hex('1'), Hex('9'), StringComparison.Ordinal)),
            _ => throw new ArgumentOutOfRangeException(nameof(fault)),
        };
        Assert.NotEqual(original, corrupt);
        File.WriteAllBytes(path, corrupt);
        var result = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Equal(corrupt, File.ReadAllBytes(path));
        File.WriteAllBytes(path, original);
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), restored.Session.Snapshot.Effects);
        Assert.Equal(original, File.ReadAllBytes(path));
    }

    [Theory]
    [InlineData("transition", "RunnerStarted")]
    [InlineData("transition", "ProfileTrainingStarted")]
    [InlineData("TrialSlotStarted", "")]
    [InlineData("receipt", "FinalReopen")]
    [InlineData("terminal", "")]
    public void CapacityV2_AmbiguousOrUnsupportedHistoryCannotAuthorizeRetryOrReceiptReplay(string kind, string value)
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture);
        var path = SeedV2Genesis(fixture, binding);
        var genesis = File.ReadAllBytes(path);
        object payload = kind switch
        {
            "transition" => value,
            "TrialSlotStarted" => Ordered(("SlotId", Hex('a')), ("PlanSha256", binding.DescriptorsV2!.EvaluationPlanSha256),
                ("RepeatOrdinal", 1), ("CaseOrdinal", 1), ("SlotOrdinal", 1),
                ("ModelSerializationBeforeSha256", Hex('b')), ("ReservedEvaluatorAttemptOrdinal", 1)),
            "receipt" => Ordered(("ReceiptType", value)),
            _ => Ordered(("SchemaVersion", 2)),
        };
        var next = V2Frame(binding, 1, kind, Sha256(Encoding.UTF8.GetString(genesis[..^1])), payload);
        File.WriteAllBytes(path, genesis.Concat(next).ToArray());
        var before = File.ReadAllBytes(path);
        for (var i = 0; i < 2; i++)
        {
            var rejected = CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, binding, 4);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, rejected.ErrorCode);
            Assert.Equal(before, File.ReadAllBytes(path));
        }
        File.WriteAllBytes(path, genesis);
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), restored.Session.Snapshot.Effects);
    }

    [Theory]
    [InlineData("history")]
    [InlineData("lock")]
    public void CapacityV2_ReplacedHeldIdentityFailsClosedEvenWithIdenticalBytes(string target)
    {
        if (!OperatingSystem.IsLinux()) throw Xunit.Sdk.SkipException.ForSkip("Linux opened-file unlink identity case.");
        using var fixture = new Fixture();
        var binding = V2Binding(fixture);
        var path = SeedV2Genesis(fixture, binding);
        var replaced = target == "history" ? path : Path.Combine(Path.GetDirectoryName(path)!, ".owner.lock");
        var bytes = File.ReadAllBytes(replaced);
        using var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        Assert.True(owner.Session.ReadCapacityPrefixV2().IsSuccess);
        File.Delete(replaced); File.WriteAllBytes(replaced, bytes);
        File.SetUnixFileMode(replaced, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, owner.Session.ReadCapacityPrefixV2().ErrorCode);
        Assert.Equal(bytes, File.ReadAllBytes(replaced));
        owner.Dispose();
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        Assert.True(restored.Session.ReadCapacityPrefixV2().IsSuccess);
    }

    [Fact]
    public void CapacityV2_RawPrefixFrameCountAndByteTotalAreGuardedBeforeParsingAndPreserved()
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture);
        var path = SeedV2Genesis(fixture, binding);
        var genesis = File.ReadAllBytes(path);
        using (Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4))) { }
        // Sizing-only corrupt prefixes: neither is a receipt/lifecycle fixture.
        var overFrames = Enumerable.Range(0, 22).SelectMany(_ => genesis).ToArray();
        File.WriteAllBytes(path, overFrames);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,
            CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4).ErrorCode);
        Assert.Equal(overFrames, File.ReadAllBytes(path));
        File.WriteAllBytes(path, genesis);
        using (Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4))) { }
        var overBytes = new byte[75578]; overBytes[^1] = 10;
        File.WriteAllBytes(path, overBytes);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,
            CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4).ErrorCode);
        Assert.Equal(overBytes, File.ReadAllBytes(path));
        File.WriteAllBytes(path, genesis);
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        Assert.Equal(genesis.LongLength, Require(restored.Session.ReadCapacityPrefixV2()).PrefixBytes);
    }

    [Theory]
    [InlineData("short-first")]
    [InlineData("short-second")]
    [InlineData("unstable-second")]
    public void CapacityV2_NonOverflowReadRefusalRequiresRecoveryAndRestoresHeldReadAndReopen(string fault)
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture);
        var path = SeedV2Genesis(fixture, binding);
        using var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        var capacity = Require(owner.Session.ReadCapacityPrefixV2());
        var snapshot = owner.Session.Snapshot;
        var before = StudentOwnedFiles(Directory.EnumerateFiles(fixture.Repository, "*", SearchOption.AllDirectories), owner.Session);
        var historyField = typeof(CodeDocumentationOneLessonSession).GetField("_v2History",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var original = Assert.IsAssignableFrom<FileStream>(historyField.GetValue(owner.Session));
        using (var injected = new BoundedReadFaultStream(path, fault))
        {
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path));
            Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(injected.SafeFileHandle, path));
            Assert.InRange(injected.Length, 1, capacity.MaximumBytes);
            try
            {
                historyField.SetValue(owner.Session, injected); // Retain the real lock and original history handle; only this test-owned read stream changes.
                var refused = owner.Session.ReadCapacityPrefixV2();
                Assert.False(refused.IsSuccess);
                Assert.Null(refused.Value);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, refused.ErrorCode);
                Assert.True(injected.FaultReached);
                Assert.True(injected.ReadCalls >= 2);
                Assert.Equal(fault == "short-first" ? 1 : 2, injected.ReadPasses);
                Assert.Same(snapshot, owner.Session.Snapshot);
                Assert.Equal(capacity, owner.Session.CapacityV2);
                Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), owner.Session.Snapshot.Effects);
                Assert.Equal((0, 0, false), owner.Session.FullMaterialSelectionAccounting);
                Assert.Equal((0, 0, false), owner.Session.SelectedStudentAccounting);
                Assert.Equal(CodeDocumentationOneLessonSession.BusyErrorCode,
                    CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4).ErrorCode);
                AssertStudentOwnedFiles(before, Directory.EnumerateFiles(fixture.Repository, "*", SearchOption.AllDirectories), owner.Session);
            }
            finally { historyField.SetValue(owner.Session, original); }
        }
        Assert.Equal(capacity, Require(owner.Session.ReadCapacityPrefixV2()));
        AssertStudentOwnedFiles(before, Directory.EnumerateFiles(fixture.Repository, "*", SearchOption.AllDirectories), owner.Session);
        owner.Dispose();
        using var reopened = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        Assert.Equal(capacity, Require(reopened.Session.ReadCapacityPrefixV2()));
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), reopened.Session.Snapshot.Effects);
        AssertStudentOwnedFiles(before, Directory.EnumerateFiles(fixture.Repository, "*", SearchOption.AllDirectories), reopened.Session);
    }

    [Fact]
    public void CapacityV2_ObservedHeldOverboundLengthRefusesCapacityAndRestoresReadAndReopen()
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture);
        var path = SeedV2Genesis(fixture, binding);
        var genesis = File.ReadAllBytes(path);
        using var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        var capacity = Require(owner.Session.ReadCapacityPrefixV2());
        var snapshot = owner.Session.Snapshot;
        var lockPath = Path.Combine(Path.GetDirectoryName(path)!, ".owner.lock");
        var lockBefore = StudentOwnedFiles([lockPath], owner.Session);
        var historyField = typeof(CodeDocumentationOneLessonSession).GetField("_v2History",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var original = Assert.IsAssignableFrom<FileStream>(historyField.GetValue(owner.Session));
        original.Dispose(); // Release only the isolated fixture history reader so Windows permits this real physical-size witness.
        var overBytes = new byte[checked((int)capacity.MaximumBytes + 1)];
        overBytes[^1] = 10;
        try
        {
            File.WriteAllBytes(path, overBytes);
            var heldOverboundHistoryLastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
            using var overbound = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open,
                FileAccess.Read, FileShare.Read, FileOptions.SequentialScan);
            historyField.SetValue(owner.Session, overbound);
            Assert.Equal(capacity.MaximumBytes + 1, overbound.Length);
            Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(overbound.SafeFileHandle, path));
            var refused = owner.Session.ReadCapacityPrefixV2();
            Assert.False(refused.IsSuccess);
            Assert.Null(refused.Value);
            Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode, refused.ErrorCode);
            Assert.Equal(overBytes, File.ReadAllBytes(path));
            Assert.Equal(heldOverboundHistoryLastWriteTimeUtc, File.GetLastWriteTimeUtc(path));
            Assert.Same(snapshot, owner.Session.Snapshot);
            Assert.Equal(capacity, owner.Session.CapacityV2);
            Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), owner.Session.Snapshot.Effects);
            Assert.Equal((0, 0, false), owner.Session.FullMaterialSelectionAccounting);
            Assert.Equal((0, 0, false), owner.Session.SelectedStudentAccounting);
            AssertStudentOwnedFiles(lockBefore, [lockPath], owner.Session);
        }
        finally
        {
            File.WriteAllBytes(path, genesis);
            historyField.SetValue(owner.Session, SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open,
                FileAccess.Read, FileShare.Read, FileOptions.SequentialScan));
        }
        Assert.Equal(capacity, Require(owner.Session.ReadCapacityPrefixV2()));
        Assert.Equal(genesis, File.ReadAllBytes(path));
        AssertStudentOwnedFiles(lockBefore, [lockPath], owner.Session);
        owner.Dispose();
        File.WriteAllBytes(path, overBytes); // Independently reach the ordinary opener's actual physical-length refusal.
        var reopenedOverboundHistoryLastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
        try
        {
            Assert.Equal(capacity.MaximumBytes + 1, new FileInfo(path).Length);
            Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,
                CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4).ErrorCode);
            Assert.Equal(overBytes, File.ReadAllBytes(path));
            Assert.Equal(reopenedOverboundHistoryLastWriteTimeUtc, File.GetLastWriteTimeUtc(path));
            AssertStudentOwnedFiles(lockBefore, [lockPath]);
        }
        finally { File.WriteAllBytes(path, genesis); }
        using var reopened = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 4));
        Assert.Equal(capacity, Require(reopened.Session.ReadCapacityPrefixV2()));
        Assert.Equal(genesis, File.ReadAllBytes(path));
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), reopened.Session.Snapshot.Effects);
        AssertStudentOwnedFiles(lockBefore, [lockPath], reopened.Session);
    }

    private sealed class BoundedReadFaultStream(string path, string fault)
        : FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.SequentialScan)
    {
        internal int ReadCalls { get; private set; }
        internal int ReadPasses { get; private set; }
        internal bool FaultReached { get; private set; }
        private bool _shortByteRead;

        public override long Position
        {
            get => base.Position;
            set { base.Position = value; if (value == 0) { ReadPasses++; _shortByteRead = false; } }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCalls++;
            if (ReadPasses == (fault == "short-first" ? 1 : 2) && fault != "unstable-second")
            {
                if (_shortByteRead) { FaultReached = true; return 0; }
                _shortByteRead = true;
                return base.Read(buffer, offset, Math.Min(count, 1));
            }
            var read = base.Read(buffer, offset, count);
            if (fault == "unstable-second" && ReadPasses == 2 && read > 0 && !FaultReached)
            {
                buffer[offset] ^= 1; // Alter only the second returned buffer; the genuine file bytes remain intact.
                FaultReached = true;
            }
            return read;
        }
    }

    [Theory]
    [InlineData("duplicate-root")]
    [InlineData("duplicate-grant")]
    [InlineData("duplicate-descriptor")]
    [InlineData("alias")]
    [InlineData("null")]
    [InlineData("unknown")]
    [InlineData("wrong-type")]
    [InlineData("mixed-v1")]
    [InlineData("not-jcs")]
    [InlineData("bom")]
    [InlineData("case-zero")]
    [InlineData("case-over")]
    [InlineData("repeat-one")]
    [InlineData("repeat-over")]
    public void CapacityV2_CatalogStrictShapeRejectsOneFaultAndRestoresResolvedControl(string fault)
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture);
        var json = V2CatalogJson(binding);
        CodeDocumentationOneLessonGrantCatalog Load(string text) => CodeDocumentationOneLessonGrantCatalog.Load(
            n => n == CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable ? text : n == "XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS" ? "4" : null);
        var selector = new CodeDocumentationOneLessonSelectorV1(binding.ObjectiveId, "opaque-grant-01");
        Assert.True(Load(json).Resolve(selector).IsSuccess);
        var corrupt = fault switch
        {
            "duplicate-root" => json.Replace("\"SchemaVersion\":2,\"Grants\"", "\"SchemaVersion\":2,\"SchemaVersion\":2,\"Grants\"", StringComparison.Ordinal),
            "duplicate-grant" => json.Replace("\"GrantId\":", "\"GrantId\":\"synthetic-grant\",\"GrantId\":", StringComparison.Ordinal),
            "duplicate-descriptor" => json.Replace("\"CaseCount\":1", "\"CaseCount\":1,\"CaseCount\":1", StringComparison.Ordinal),
            "alias" => json.Replace("\"CaseCount\"", "\"caseCount\"", StringComparison.Ordinal),
            "null" => json.Replace("\"CaseCount\":1", "\"CaseCount\":null", StringComparison.Ordinal),
            "unknown" => json.Replace("\"CaseCount\":1", "\"CaseCount\":1,\"Unknown\":1", StringComparison.Ordinal),
            "wrong-type" => json.Replace("\"CaseCount\":1", "\"CaseCount\":\"1\"", StringComparison.Ordinal),
            "mixed-v1" => json.Replace("\"SchemaVersion\":2,\"Grants\"", "\"SchemaVersion\":1,\"Grants\"", StringComparison.Ordinal),
            "not-jcs" => json.Replace("\"CaseCount\":1", "\"CaseCount\":1.0", StringComparison.Ordinal),
            "case-zero" => json.Replace("\"CaseCount\":1", "\"CaseCount\":0", StringComparison.Ordinal),
            "case-over" => json.Replace("\"CaseCount\":1", "\"CaseCount\":201", StringComparison.Ordinal),
            "repeat-one" => json.Replace("\"RepeatCount\":2", "\"RepeatCount\":1", StringComparison.Ordinal),
            "repeat-over" => json.Replace("\"RepeatCount\":2", "\"RepeatCount\":5001", StringComparison.Ordinal),
            _ => "\uFEFF" + json,
        };
        Assert.NotEqual(json, corrupt);
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode, Load(corrupt).Resolve(selector).ErrorCode);
        Assert.True(Load(json).Resolve(selector).IsSuccess);
        Assert.Empty(SnapshotFiles(fixture.Repository).Where(x => x.EndsWith("session.jsonl", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(200, 2)]
    [InlineData(1, 5000)]
    public void CapacityV2_CatalogDescriptorAcceptsExactScalarCeilings(int cases, int repeats)
    {
        using var fixture = new Fixture();
        var binding = V2Binding(fixture, cases, repeats);
        var catalog = CodeDocumentationOneLessonGrantCatalog.Load(n => n == CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable
            ? V2CatalogJson(binding) : n == "XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS" ? "10000" : null);
        var selected = Require(catalog.Resolve(new(binding.ObjectiveId, "opaque-grant-01")));
        Assert.Equal(binding.DescriptorsV2, selected.DescriptorsV2);
        Assert.Equal(CodeDocumentationOneLessonSession.BaselinePrerequisiteUnavailableErrorCode,
            CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, binding, catalog.MaxTeacherCalls).ErrorCode);
        Assert.DoesNotContain(SnapshotFiles(fixture.Repository), x => x.EndsWith("session.jsonl", StringComparison.Ordinal));
    }

    [Fact]
    public void CapacityV2_JcsUsesUtf16PropertyOrderMinimalEscapingAndExactV1GoldenBytes()
    {
        const string input = "{\"z\":\"\\u2028\\u0800<\\\"\\\\\",\"a\":2}";
        using var document = JsonDocument.Parse(input);
        Assert.Equal("{\"a\":2,\"z\":\"\u2028\u0800<\\\"\\\\\"}", Encoding.UTF8.GetString(CodeDocumentationOneLessonV2Wire.CanonicalBytes(document.RootElement)));
        using var fixture = new Fixture();
        using var owner = Require(CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, fixture.Binding()));
        var b = fixture.Binding();
        var expected = "{\"Kind\":\"session\",\"FormatVersion\":1,\"SessionId\":\"" + Sha256(b.ObjectiveId) +
            "\",\"Binding\":{\"ObjectiveId\":\"" + b.ObjectiveId + "\",\"GrantId\":\"synthetic-grant\",\"GrantVersion\":\"grant-v1\",\"TrainingRunId\":\"synthetic-training-run-01\",\"CandidateSha256\":\"" + Hex('1') +
            "\",\"ParentManifestSha256\":\"" + Hex('2') + "\",\"ParentWeightsSha256\":\"" + Hex('3') + "\",\"ConfigurationSha256\":\"" + Hex('4') +
            "\",\"ExamConditionSha256\":\"" + Hex('5') + "\"},\"Revision\":0,\"PreviousFrameSha256\":\"\",\"Effects\":{\"BaselineDispatched\":0,\"BaselineCompleted\":0,\"TeachingDispatched\":0,\"TeachingCompleted\":0,\"EvaluatorDispatched\":0,\"EvaluatorCompleted\":0,\"OptimizerEntries\":0,\"EffectOutcomeUnknown\":false}}\n";
        Assert.Equal(Encoding.UTF8.GetBytes(expected), File.ReadAllBytes(Path.Combine(fixture.Repository, owner.Session.Snapshot.HistoryRelativePath)));
    }

    [Fact]
    public void CapacityV2_OverwideDescriptorIntegerFailsStartupBoundAndDirectOwnerOverflowFailsCapacity()
    {
        using var fixture = new Fixture();
        var wide = V2Binding(fixture, 200, int.MaxValue);
        var catalog = CodeDocumentationOneLessonGrantCatalog.Load(n => n == CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable
            ? V2CatalogJson(wide) : n == "XIIGEN_CONTINUOUS_LEARNING_MAX_TEACHER_CALLS" ? "10000" : null);
        Assert.Equal(CodeDocumentationOneLessonGrantCatalog.InvalidCatalogErrorCode,
            catalog.Resolve(new(wide.ObjectiveId, "opaque-grant-01")).ErrorCode);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityInvalidErrorCode,
            CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, wide, 10000).ErrorCode);
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, V2Binding(fixture), 4));
        Assert.Equal(0, restored.Session.CapacityV2!.PrefixFrames);
    }

    [Fact]
    public void CapacityV1_Original256FrameLimitRemainsUnchanged()
    {
        using var fixture = new Fixture();
        using var owner = Require(CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, fixture.Binding()));
        for (var revision = 0; revision < 255; revision++) Require(owner.Session.Advance(revision, new()));
        var path = Path.Combine(fixture.Repository, owner.Session.Snapshot.HistoryRelativePath);
        var before = File.ReadAllBytes(path);
        Assert.Equal(256, File.ReadAllLines(path).Length);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, owner.Session.Advance(255, new()).ErrorCode);
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    [Fact]
    public void CapacityV1_Original2097152ByteLimitAcceptsExactAndRejectsOneMoreWithoutTruncation()
    {
        using var fixture = new Fixture();
        string path;
        using (var owner = Require(CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, fixture.Binding())))
            path = Path.Combine(fixture.Repository, owner.Session.Snapshot.HistoryRelativePath);
        var genesis = File.ReadAllBytes(path);
        var exact = genesis[..^1].Concat(Enumerable.Repeat((byte)' ', 2097152 - genesis.Length)).Append((byte)10).ToArray();
        Assert.Equal(2097152, exact.Length);
        File.WriteAllBytes(path, exact);
        using (var owner = Require(CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, fixture.Binding())))
        {
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, owner.Session.Advance(0, new()).ErrorCode);
            Assert.Equal(exact, File.ReadAllBytes(path));
        }
        var over = exact[..^1].Append((byte)' ').Append((byte)10).ToArray();
        File.WriteAllBytes(path, over);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,
            CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, fixture.Binding()).ErrorCode);
        Assert.Equal(over, File.ReadAllBytes(path));
        File.WriteAllBytes(path, exact);
        using var restored = Require(CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, fixture.Binding()));
        Assert.Equal(exact, File.ReadAllBytes(path));
    }

    private static CodeDocumentationOneLessonSessionBinding V2Binding(Fixture fixture, int cases = 1, int repeats = 2)
        => fixture.Binding() with { FormatVersion = 2, DescriptorsV2 = new(2, Hex('a'), Hex('b'), Hex('c'), Hex('d'), Hex('e'), cases, repeats, Hex('f'), Hex('0')) };

    private static SortedDictionary<string, object> Ordered(params (string Name, object Value)[] properties)
        => new(properties.ToDictionary(p => p.Name, p => p.Value), StringComparer.Ordinal);

    private static object V2BindingObject(CodeDocumentationOneLessonSessionBinding b) => Ordered(
        ("ObjectiveId", b.ObjectiveId), ("GrantId", b.GrantId), ("GrantVersion", b.GrantVersion), ("TrainingRunId", b.TrainingRunId),
        ("CandidateSha256", b.CandidateSha256), ("ParentManifestSha256", b.ParentManifestSha256), ("ParentWeightsSha256", b.ParentWeightsSha256),
        ("ConfigurationSha256", b.ConfigurationSha256), ("ExamConditionSha256", b.ExamConditionSha256),
        ("DescriptorsV2", DescriptorObject(b.DescriptorsV2!)));

    private static object DescriptorObject(CodeDocumentationOneLessonDescriptorsV2 d) => Ordered(
        ("SchemaVersion", d.SchemaVersion), ("SourceSelectionSha256", d.SourceSelectionSha256), ("ParentCheckpointSha256", d.ParentCheckpointSha256),
        ("CandidateSelectionSha256", d.CandidateSelectionSha256), ("CorpusDescriptorSha256", d.CorpusDescriptorSha256), ("EvaluationPlanSha256", d.EvaluationPlanSha256),
        ("CaseCount", d.CaseCount), ("RepeatCount", d.RepeatCount), ("EvaluatorProfileSha256", d.EvaluatorProfileSha256), ("ProfileSelectionSha256", d.ProfileSelectionSha256));

    private static byte[] V2Frame(CodeDocumentationOneLessonSessionBinding binding, int revision = 0, string kind = "session", string previous = "", object? payload = null)
    {
        var frame = Ordered(("Kind", kind), ("FormatVersion", 2), ("SessionId", Sha256(binding.ObjectiveId)),
            ("BindingV2", V2BindingObject(binding)), ("BindingSha256", CodeDocumentationOneLessonSession.ComputeBindingSha256(binding)),
            ("Revision", revision.ToString()), ("PreviousFrameSha256", previous),
            ("Effects", Ordered(("BaselineDispatched", "0"), ("BaselineCompleted", "0"), ("TeachingDispatched", "0"),
                ("TeachingCompleted", "0"), ("EvaluatorDispatched", "0"), ("EvaluatorCompleted", "0"), ("OptimizerEntries", "0"), ("EffectOutcomeUnknown", false))));
        if (payload is not null) frame.Add(kind switch { "transition" => "TransitionEvent", "receipt" => "ProgressReceipt", "terminal" => "TerminalOutcome", _ => kind }, payload);
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(frame, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }) + "\n");
    }

    private static string SeedV2Genesis(Fixture fixture, CodeDocumentationOneLessonSessionBinding binding)
    {
        string path;
        using (var local = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, binding, 10000)))
            path = Path.Combine(fixture.Repository, local.Session.Snapshot.HistoryRelativePath);
        // Seed a real custody-owned file; an inherited Windows ACL is deliberately rejected by the reader.
        using (var history = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.CreateNew,
            FileAccess.ReadWrite, FileShare.Read, FileOptions.WriteThrough))
        {
            history.Write(V2Frame(binding));
            history.Flush(flushToDisk: true);
        }
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path));
        return path;
    }

    private static string V2CatalogJson(CodeDocumentationOneLessonSessionBinding binding)
    {
        var grant = (SortedDictionary<string, object>)V2BindingObject(binding);
        grant.Add("GrantSelector", "opaque-grant-01");
        return "{\"SchemaVersion\":2,\"Grants\":[" + JsonSerializer.Serialize(grant) + "]}";
    }

    [Fact]
    public void OpenOrCreate_IdenticalObjectiveReopensStableIdentityAndHistory()
    {
        using var fixture = new Fixture();
        CodeDocumentationOneLessonSessionSnapshot original;
        using (var created = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   fixture.ModelRoot,
                   fixture.Binding())))
        {
            Assert.True(created.Created);
            original = created.Session.Snapshot;
        }
        var originalBytes = File.ReadAllBytes(Path.Combine(
            fixture.Repository,
            original.HistoryRelativePath));

        using var reopened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));

        Assert.False(reopened.Created);
        Assert.Equal(original.SessionId, reopened.Session.Snapshot.SessionId);
        Assert.Equal(original.HistoryRelativePath, reopened.Session.Snapshot.HistoryRelativePath);
        Assert.Equal(original.HistorySha256, reopened.Session.Snapshot.HistorySha256);
        Assert.Equal(original.Binding, reopened.Session.Snapshot.Binding);
        Assert.Equal(0, reopened.Session.Snapshot.Revision);
        Assert.Equal(originalBytes, File.ReadAllBytes(Path.Combine(
            fixture.Repository,
            original.HistoryRelativePath)));
    }

    [Fact]
    public void LifecycleSeed_RetainedParentFrameIsCanonicalAndReopensWithoutRewrite()
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var historyPath = Path.Combine(
            fixture.Repository,
            opened.Session.Snapshot.HistoryRelativePath);

        var committed = Require(opened.Session.RecordTerminalOutcome(
            0,
            TerminalReceiptRefs(opened.Session.Snapshot)));

        Assert.Equal(1, committed.Revision);
        Assert.NotNull(committed.TerminalOutcome);
        Assert.Equal(
            CodeDocumentationOneLessonTerminalStateV1.TERMINAL_NEGATIVE_PARENT_RETAINED,
            committed.TerminalOutcome!.TerminalState);
        Assert.Null(committed.TerminalOutcome.PublicationReceipt);
        Assert.Equal(fixture.Binding().ObjectiveId, committed.TerminalOutcome.ObjectiveId);
        Assert.Equal(committed.SessionId, committed.TerminalOutcome.SessionId);
        Assert.Equal(fixture.Binding().TrainingRunId, committed.TerminalOutcome.TrainingRunId);
        Assert.Equal(
            CodeDocumentationOneLessonSession.ComputeBindingSha256(fixture.Binding()),
            committed.TerminalOutcome.BindingSha256);

        var committedBytes = File.ReadAllBytes(historyPath);
        var terminalLine = Encoding.UTF8.GetString(committedBytes)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)[^1];
        Assert.DoesNotContain("\"PublicationReceipt\"", terminalLine);
        Assert.Equal(Sha256(terminalLine), committed.TerminalOutcomeSha256);

        opened.Dispose();
        using var reopened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        Assert.False(reopened.Created);
        Assert.Equal(committed.TerminalOutcome.OutcomeId, reopened.Session.Snapshot.TerminalOutcome!.OutcomeId);
        Assert.Equal(committed.TerminalOutcomeSha256, reopened.Session.Snapshot.TerminalOutcomeSha256);
        Assert.Equal(committedBytes, File.ReadAllBytes(historyPath));
        reopened.Dispose();
    }

    [Fact]
    public void LifecycleSeed_PromotedFrameStoresOneTypedPublicationReference()
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));

        var committed = Require(opened.Session.RecordTerminalOutcome(
            0,
            TerminalReceiptRefs(opened.Session.Snapshot, includePublication: true)));

        Assert.NotNull(committed.TerminalOutcome);
        Assert.Equal(
            CodeDocumentationOneLessonTerminalStateV1.TERMINAL_PROMOTED,
            committed.TerminalOutcome!.TerminalState);
        Assert.Equal("publication-receipt", committed.TerminalOutcome.PublicationReceipt!.ReceiptId);
        var historyPath = Path.Combine(fixture.Repository, committed.HistoryRelativePath);
        using var frameDocument = JsonDocument.Parse(File.ReadAllLines(historyPath)[^1]);
        var frame = frameDocument.RootElement;
        var publication = frame.GetProperty("TerminalOutcome").GetProperty("PublicationReceipt");
        Assert.Equal(JsonValueKind.Object, publication.ValueKind);
        Assert.Equal("publication-receipt", publication.GetProperty("ReceiptId").GetString());
    }

    [Fact]
    public void ReadCanonicalOutcome_FailsClosedWithoutReceiptArtifactChainReader()
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var terminal = Require(opened.Session.RecordTerminalOutcome(
            0,
            TerminalReceiptRefs(opened.Session.Snapshot)));
        var historyPath = Path.Combine(
            fixture.Repository,
            terminal.HistoryRelativePath);
        var durableBytes = File.ReadAllBytes(historyPath);
        var selector = new CodeDocumentationOneLessonSelectorV1(
            fixture.Binding().ObjectiveId,
            "opaque-grant-01");
        var bindingSha256 = CodeDocumentationOneLessonSession
            .ComputeBindingSha256(fixture.Binding());

        var readback = opened.Session.ReadCanonicalOutcome(
            selector,
            bindingSha256,
            terminal.TerminalOutcome!.OutcomeId,
            terminal.TerminalOutcomeSha256);
        Assert.False(readback.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,
            readback.ErrorCode);

        var identityConflict = opened.Session.ReadCanonicalOutcome(
            selector with { ObjectiveId = "different-objective" },
            bindingSha256,
            terminal.TerminalOutcome.OutcomeId,
            terminal.TerminalOutcomeSha256);
        Assert.False(identityConflict.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.ObjectiveConflictErrorCode,
            identityConflict.ErrorCode);

        var changedOutcomeId = (terminal.TerminalOutcome.OutcomeId[0] == 'a' ? "b" : "a") +
                               terminal.TerminalOutcome.OutcomeId[1..];
        var outcomeConflict = opened.Session.ReadCanonicalOutcome(
            selector,
            bindingSha256,
            changedOutcomeId,
            terminal.TerminalOutcomeSha256);
        Assert.False(outcomeConflict.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.TerminalConflictErrorCode,
            outcomeConflict.ErrorCode);

        var frameHashConflict = opened.Session.ReadCanonicalOutcome(
            selector,
            bindingSha256,
            terminal.TerminalOutcome.OutcomeId,
            Hex('f'));
        Assert.False(frameHashConflict.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.TerminalConflictErrorCode,
            frameHashConflict.ErrorCode);
        Assert.Equal(durableBytes, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void LifecycleSeed_ReferenceEnvelopeIdempotencyAndConflictsDoNotMutate()
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var first = Require(opened.Session.RecordTerminalOutcome(
            0,
            TerminalReceiptRefs(opened.Session.Snapshot)));
        var historyPath = Path.Combine(fixture.Repository, first.HistoryRelativePath);
        var beforeRejectedWrites = File.ReadAllBytes(historyPath);

        var replay = Require(opened.Session.RecordTerminalOutcome(
            0,
            TerminalReceiptRefs(opened.Session.Snapshot)));
        Assert.Equal(first.TerminalOutcome!.OutcomeId, replay.TerminalOutcome!.OutcomeId);
        Assert.Equal(first.TerminalOutcomeSha256, replay.TerminalOutcomeSha256);
        Assert.Equal(beforeRejectedWrites, File.ReadAllBytes(historyPath));

        var different = opened.Session.RecordTerminalOutcome(
            1,
            TerminalReceiptRefs(
                opened.Session.Snapshot,
                dispositionReceiptId: "different-disposition"));
        Assert.False(different.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.TerminalConflictErrorCode, different.ErrorCode);

        var stale = opened.Session.RecordTerminalOutcome(
            -1,
            TerminalReceiptRefs(opened.Session.Snapshot));
        Assert.False(stale.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.StaleRevisionErrorCode, stale.ErrorCode);

        var advance = opened.Session.Advance(
            1,
            opened.Session.Snapshot.Effects);
        Assert.False(advance.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, advance.ErrorCode);
        Assert.Equal(beforeRejectedWrites, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void RecordTerminalOutcome_MismatchedVerifiedReferenceBindingIsRejectedWithoutAppend()
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var historyPath = Path.Combine(
            fixture.Repository,
            opened.Session.Snapshot.HistoryRelativePath);
        var before = File.ReadAllBytes(historyPath);
        var mismatchedReferences = TerminalReceiptRefs(opened.Session.Snapshot) with
        {
            BindingSha256 = Hex('f'),
        };

        var result = opened.Session.RecordTerminalOutcome(0, mismatchedReferences);

        Assert.False(result.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.InvalidErrorCode, result.ErrorCode);
        Assert.Equal(before, File.ReadAllBytes(historyPath));
    }

    [Theory]
    [InlineData("\"PublicationReceipt\":null,")]
    [InlineData("\"PublicationReceipt\":{\"ReceiptId\":\"publication-receipt\",\"ReceiptSha256\":\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\"},")]
    [InlineData("\"publicationReceipt\":null,")]
    [InlineData("\"PublicationReceipt\":null,\"PublicationReceipt\":null,")]
    public void OpenOrCreate_RetainedTerminalRejectsPublicationPropertyAndPreservesCorruptBytes(
        string injectedProperty)
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var historyPath = Path.Combine(
            fixture.Repository,
            opened.Session.Snapshot.HistoryRelativePath);
        Require(opened.Session.RecordTerminalOutcome(
            0,
            TerminalReceiptRefs(opened.Session.Snapshot)));
        opened.Dispose();

        var lines = File.ReadAllLines(historyPath);
        Assert.Equal(2, lines.Length);
        var corruptedTerminal = lines[1].Replace(
            "\"FinalReopenReceipt\":",
            injectedProperty + "\"FinalReopenReceipt\":",
            StringComparison.Ordinal);
        Assert.NotEqual(lines[1], corruptedTerminal);
        File.WriteAllText(
            historyPath,
            lines[0] + "\n" + corruptedTerminal + "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var corruptBytes = File.ReadAllBytes(historyPath);

        var rejected = CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding());

        Assert.False(rejected.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, rejected.ErrorCode);
        Assert.Equal(corruptBytes, File.ReadAllBytes(historyPath));
    }

    [Theory]
    [InlineData("revision-duplicate")]
    [InlineData("revision-alias")]
    [InlineData("binding-duplicate")]
    [InlineData("binding-alias")]
    [InlineData("effects-duplicate")]
    [InlineData("effects-alias")]
    [InlineData("nested-binding-duplicate")]
    [InlineData("nested-binding-alias")]
    [InlineData("nested-effects-duplicate")]
    [InlineData("nested-effects-alias")]
    public void OpenOrCreate_TerminalRejectsDuplicateOrCaseAliasedEnvelopeFieldsAndPreservesBytes(
        string corruption)
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var historyPath = Path.Combine(
            fixture.Repository,
            opened.Session.Snapshot.HistoryRelativePath);
        Require(opened.Session.RecordTerminalOutcome(
            0,
            TerminalReceiptRefs(opened.Session.Snapshot)));
        opened.Dispose();

        var lines = File.ReadAllLines(historyPath);
        Assert.Equal(2, lines.Length);
        using var frameDocument = JsonDocument.Parse(lines[1]);
        var bindingJson = frameDocument.RootElement.GetProperty("Binding").GetRawText();
        var effectsJson = frameDocument.RootElement.GetProperty("Effects").GetRawText();
        var objectiveId = fixture.Binding().ObjectiveId;
        var corruptedTerminal = corruption switch
        {
            "revision-duplicate" => lines[1].Replace(
                "\"PreviousFrameSha256\":",
                "\"Revision\":1,\"PreviousFrameSha256\":",
                StringComparison.Ordinal),
            "revision-alias" => lines[1].Replace(
                "\"PreviousFrameSha256\":",
                "\"revision\":0,\"PreviousFrameSha256\":",
                StringComparison.Ordinal),
            "binding-duplicate" => lines[1].Replace(
                "\"Revision\":",
                "\"Binding\":" + bindingJson + ",\"Revision\":",
                StringComparison.Ordinal),
            "binding-alias" => lines[1].Replace(
                "\"Revision\":",
                "\"binding\":" + bindingJson + ",\"Revision\":",
                StringComparison.Ordinal),
            "effects-duplicate" => lines[1].Replace(
                "\"TerminalOutcome\":",
                "\"Effects\":" + effectsJson + ",\"TerminalOutcome\":",
                StringComparison.Ordinal),
            "effects-alias" => lines[1].Replace(
                "\"TerminalOutcome\":",
                "\"effects\":" + effectsJson + ",\"TerminalOutcome\":",
                StringComparison.Ordinal),
            "nested-binding-duplicate" => lines[1].Replace(
                "\"GrantId\":",
                "\"ObjectiveId\":\"" + objectiveId + "\",\"GrantId\":",
                StringComparison.Ordinal),
            "nested-binding-alias" => lines[1].Replace(
                "\"GrantId\":",
                "\"objectiveId\":\"" + objectiveId + "\",\"GrantId\":",
                StringComparison.Ordinal),
            "nested-effects-duplicate" => lines[1].Replace(
                "\"BaselineCompleted\":",
                "\"BaselineDispatched\":0,\"BaselineCompleted\":",
                StringComparison.Ordinal),
            "nested-effects-alias" => lines[1].Replace(
                "\"BaselineCompleted\":",
                "\"baselineDispatched\":0,\"BaselineCompleted\":",
                StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(corruption)),
        };
        Assert.NotEqual(lines[1], corruptedTerminal);
        File.WriteAllText(
            historyPath,
            lines[0] + "\n" + corruptedTerminal + "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var corruptBytes = File.ReadAllBytes(historyPath);

        var rejected = CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding());

        Assert.False(rejected.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, rejected.ErrorCode);
        Assert.Equal(corruptBytes, File.ReadAllBytes(historyPath));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("null")]
    [InlineData("wrong-kind")]
    [InlineData("alias")]
    [InlineData("duplicate")]
    public void OpenOrCreate_PromotedTerminalRejectsInvalidPublicationReceiptWireShape(
        string corruption)
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var historyPath = Path.Combine(
            fixture.Repository,
            opened.Session.Snapshot.HistoryRelativePath);
        Require(opened.Session.RecordTerminalOutcome(
            0,
            TerminalReceiptRefs(opened.Session.Snapshot, includePublication: true)));
        opened.Dispose();

        var lines = File.ReadAllLines(historyPath);
        var publicationObject = "\"PublicationReceipt\":{\"ReceiptId\":\"publication-receipt\",\"ReceiptSha256\":\"" +
            Hex('d') + "\"}";
        var invalidTerminal = corruption switch
        {
            "missing" => lines[1].Replace("," + publicationObject, "", StringComparison.Ordinal),
            "null" => lines[1].Replace(publicationObject, "\"PublicationReceipt\":null", StringComparison.Ordinal),
            "wrong-kind" => lines[1].Replace(publicationObject, "\"PublicationReceipt\":\"not-an-object\"", StringComparison.Ordinal),
            "alias" => lines[1].Replace("PublicationReceipt", "publicationReceipt", StringComparison.Ordinal),
            "duplicate" => lines[1].Replace(
                "\"FinalReopenReceipt\":",
                "\"PublicationReceipt\":null,\"FinalReopenReceipt\":",
                StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(corruption)),
        };
        Assert.NotEqual(lines[1], invalidTerminal);
        File.WriteAllText(
            historyPath,
            lines[0] + "\n" + invalidTerminal + "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var corruptBytes = File.ReadAllBytes(historyPath);

        var rejected = CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding());

        Assert.False(rejected.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, rejected.ErrorCode);
        Assert.Equal(corruptBytes, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void OpenOrCreate_UnpairedSurrogateObjectiveReturnsInvalidWithoutThrowing()
    {
        using var fixture = new Fixture();
        var malformed = fixture.Binding() with { ObjectiveId = "invalid-\uD800" };

        var result = CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            malformed);

        Assert.False(result.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.InvalidErrorCode, result.ErrorCode);
    }

    [Fact]
    public void OpenOrCreate_ChangedParentUnderSameObjectiveConflictsWithoutChangingHistory()
    {
        using var fixture = new Fixture();
        string historyPath;
        byte[] before;
        using (var created = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   fixture.ModelRoot,
                   fixture.Binding())))
        {
            historyPath = Path.Combine(fixture.Repository, created.Session.Snapshot.HistoryRelativePath);
            before = File.ReadAllBytes(historyPath);
        }

        var changedParent = fixture.Binding() with { ParentManifestSha256 = Hex('9') };
        var conflict = CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            changedParent);

        Assert.False(conflict.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.ObjectiveConflictErrorCode, conflict.ErrorCode);
        Assert.Equal(before, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void OpenOrCreate_ChangedGrantVersionUnderSameObjectiveConflictsWithoutChangingHistory()
    {
        using var fixture = new Fixture();
        string historyPath;
        byte[] before;
        using (var created = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   fixture.ModelRoot,
                   fixture.Binding())))
        {
            historyPath = Path.Combine(
                fixture.Repository,
                created.Session.Snapshot.HistoryRelativePath);
            before = File.ReadAllBytes(historyPath);
        }

        var changedGrant = fixture.Binding() with { GrantVersion = "grant-v2" };
        var conflict = CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            changedGrant);

        Assert.False(conflict.IsSuccess);
        Assert.Equal(
            CodeDocumentationOneLessonSession.ObjectiveConflictErrorCode,
            conflict.ErrorCode);
        Assert.Equal(before, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void Preflight_SameOpaqueSelectorReopensAndChangedSourceBindingConflicts()
    {
        using var fixture = new Fixture();
        var binding = fixture.Binding();
        var request = new CodeDocumentationOneLessonPreflightRequestV1(
            new CodeDocumentationOneLessonSelectorV1(
                binding.ObjectiveId,
                "opaque-grant-01"));
        var firstCatalog = CodeDocumentationOneLessonGrantCatalog
            .FromTrustedGrants([Grant(binding, "grant-v1")]);

        var first = CodeDocumentationOneLessonEndpoint.Execute(
            request,
            firstCatalog,
            fixture.Repository);
        Assert.Equal(StatusCodes.Status200OK,
            ((IStatusCodeHttpResult)first).StatusCode);
        var firstResponse = Assert.IsAssignableFrom<IValueHttpResult>(first).Value
            as CodeDocumentationOneLessonPreflightResponseV1;
        Assert.NotNull(firstResponse);

        var replay = CodeDocumentationOneLessonEndpoint.Execute(
            request,
            firstCatalog,
            fixture.Repository);
        Assert.Equal(StatusCodes.Status200OK,
            ((IStatusCodeHttpResult)replay).StatusCode);
        var replayResponse = Assert.IsAssignableFrom<IValueHttpResult>(replay).Value
            as CodeDocumentationOneLessonPreflightResponseV1;
        Assert.NotNull(replayResponse);
        Assert.Equal(firstResponse.ObjectiveId, replayResponse.ObjectiveId);
        Assert.Equal(firstResponse.TrainingRunId, replayResponse.TrainingRunId);
        Assert.Equal(firstResponse.BindingSha256, replayResponse.BindingSha256);
        Assert.Equal("Reopened", replayResponse.Disposition);

        string historyPath;
        byte[] before;
        using (var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   fixture.ModelRoot,
                   binding)))
        {
            historyPath = Path.Combine(
                fixture.Repository,
                opened.Session.Snapshot.HistoryRelativePath);
            before = File.ReadAllBytes(historyPath);
        }

        var changedSourceGrant = Grant(binding, "grant-v1") with
        {
            CandidateSha256 = Hex('9')
        };
        var changedGrantCatalog = CodeDocumentationOneLessonGrantCatalog
            .FromTrustedGrants([changedSourceGrant]);
        var changedBinding = CodeDocumentationOneLessonEndpoint.Execute(
            request,
            changedGrantCatalog,
            fixture.Repository);

        Assert.Equal(StatusCodes.Status409Conflict,
            ((IStatusCodeHttpResult)changedBinding).StatusCode);
        Assert.Equal(before, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void Preflight_UnverifiedTerminalFrameFailsClosedAndChangedBindingDoesNotReplaceIt()
    {
        using var fixture = new Fixture();
        var binding = fixture.Binding();
        CodeDocumentationOneLessonSessionSnapshot terminal;
        string historyPath;
        using (var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   fixture.ModelRoot,
                   binding)))
        {
            terminal = Require(opened.Session.RecordTerminalOutcome(
                0,
                TerminalReceiptRefs(opened.Session.Snapshot)));
            historyPath = Path.Combine(
                fixture.Repository,
                terminal.HistoryRelativePath);
        }

        var durableBytes = File.ReadAllBytes(historyPath);
        var request = new CodeDocumentationOneLessonPreflightRequestV1(
            new CodeDocumentationOneLessonSelectorV1(
                binding.ObjectiveId,
                "opaque-grant-01"));
        var matchingCatalog = CodeDocumentationOneLessonGrantCatalog
            .FromTrustedGrants([Grant(binding, "grant-v1")]);
        var replay = CodeDocumentationOneLessonEndpoint.Execute(
            request,
            matchingCatalog,
            fixture.Repository);

        Assert.Equal(StatusCodes.Status409Conflict,
            ((IStatusCodeHttpResult)replay).StatusCode);
        var response = Assert.IsAssignableFrom<IValueHttpResult>(replay).Value
            as CodeDocumentationOneLessonPreflightResponseV1;
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal("OBJECTIVE_RECOVERY_REQUIRED", response.ErrorCode);
        Assert.NotEqual("TerminalReplay", response.Disposition);
        Assert.Null(response.TerminalOutcome);
        Assert.Equal(durableBytes, File.ReadAllBytes(historyPath));

        var changedBinding = binding with { CandidateSha256 = Hex('9') };
        var changedCatalog = CodeDocumentationOneLessonGrantCatalog
            .FromTrustedGrants([Grant(changedBinding, "grant-v1")]);
        var rejected = CodeDocumentationOneLessonEndpoint.Execute(
            request,
            changedCatalog,
            fixture.Repository);

        Assert.Equal(StatusCodes.Status409Conflict,
            ((IStatusCodeHttpResult)rejected).StatusCode);
        var changedResponse = Assert.IsAssignableFrom<IValueHttpResult>(rejected).Value
            as CodeDocumentationOneLessonPreflightResponseV1;
        Assert.NotNull(changedResponse);
        Assert.Equal("OBJECTIVE_BINDING_CONFLICT", changedResponse.ErrorCode);
        Assert.Equal(durableBytes, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void Preflight_IncompleteEffectHistoryRequiresRecoveryAndDoesNotReplay()
    {
        using var fixture = new Fixture();
        var binding = fixture.Binding();
        string historyPath;
        using (var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   fixture.ModelRoot,
                   binding)))
        {
            var incomplete = Require(opened.Session.Advance(
                0,
                new CodeDocumentationOneLessonEffectAccounting(
                    BaselineDispatched: 1,
                    EffectOutcomeUnknown: true)));
            historyPath = Path.Combine(
                fixture.Repository,
                incomplete.HistoryRelativePath);
        }

        var durableBytes = File.ReadAllBytes(historyPath);
        var request = new CodeDocumentationOneLessonPreflightRequestV1(
            new CodeDocumentationOneLessonSelectorV1(
                binding.ObjectiveId,
                "opaque-grant-01"));
        var catalog = CodeDocumentationOneLessonGrantCatalog
            .FromTrustedGrants([Grant(binding, "grant-v1")]);

        var rejected = CodeDocumentationOneLessonEndpoint.Execute(
            request,
            catalog,
            fixture.Repository);

        Assert.Equal(StatusCodes.Status409Conflict,
            ((IStatusCodeHttpResult)rejected).StatusCode);
        var response = Assert.IsAssignableFrom<IValueHttpResult>(rejected).Value
            as CodeDocumentationOneLessonPreflightResponseV1;
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal("OBJECTIVE_RECOVERY_REQUIRED", response.ErrorCode);
        Assert.Equal(durableBytes, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void TerminalAppendAndPreflight_RejectOutstandingDispatchEvenWhenUnknownFlagIsFalse()
    {
        using var fixture = new Fixture();
        var binding = fixture.Binding();
        string historyPath;
        using (var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   fixture.ModelRoot,
                   binding)))
        {
            var incomplete = Require(opened.Session.Advance(
                0,
                new CodeDocumentationOneLessonEffectAccounting(
                    BaselineDispatched: 1,
                    BaselineCompleted: 0,
                    EffectOutcomeUnknown: false)));
            historyPath = Path.Combine(fixture.Repository, incomplete.HistoryRelativePath);

            var append = opened.Session.RecordTerminalOutcome(
                incomplete.Revision,
                TerminalReceiptRefs(incomplete));
            Assert.False(append.IsSuccess);
            Assert.Equal(
                CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,
                append.ErrorCode);
        }

        var beforePreflight = File.ReadAllBytes(historyPath);
        var request = new CodeDocumentationOneLessonPreflightRequestV1(
            new CodeDocumentationOneLessonSelectorV1(binding.ObjectiveId, "opaque-grant-01"));
        var catalog = CodeDocumentationOneLessonGrantCatalog
            .FromTrustedGrants([Grant(binding, "grant-v1")]);
        var rejected = CodeDocumentationOneLessonEndpoint.Execute(
            request,
            catalog,
            fixture.Repository);

        Assert.Equal(StatusCodes.Status409Conflict,
            ((IStatusCodeHttpResult)rejected).StatusCode);
        var response = Assert.IsAssignableFrom<IValueHttpResult>(rejected).Value
            as CodeDocumentationOneLessonPreflightResponseV1;
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal("OBJECTIVE_RECOVERY_REQUIRED", response.ErrorCode);
        Assert.NotEqual("TerminalReplay", response.Disposition);
        Assert.Null(response.TerminalOutcome);
        Assert.Equal(beforePreflight, File.ReadAllBytes(historyPath));

        using var reopened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            binding));
        Assert.Null(reopened.Session.Snapshot.TerminalOutcome);
        Assert.Equal(1, reopened.Session.Snapshot.Effects.BaselineDispatched);
        Assert.Equal(0, reopened.Session.Snapshot.Effects.BaselineCompleted);
        Assert.False(reopened.Session.Snapshot.Effects.EffectOutcomeUnknown);
        Assert.Equal(beforePreflight, File.ReadAllBytes(historyPath));
    }

    [Theory]
    [InlineData("isSuccess", "IsSuccess")]
    [InlineData("errorCode", "ErrorCode")]
    [InlineData("errorMessage", "ErrorMessage")]
    [InlineData("disposition", "Disposition")]
    [InlineData("objectiveId", "ObjectiveId")]
    [InlineData("trainingRunId", "TrainingRunId")]
    [InlineData("bindingSha256", "BindingSha256")]
    [InlineData("sessionId", "SessionId")]
    [InlineData("sessionRevision", "SessionRevision")]
    [InlineData("terminalOutcome", "TerminalOutcome")]
    [InlineData("terminalOutcomeSha256", "TerminalOutcomeSha256")]
    public void PreflightClient_RejectsCaseAliasInExactResponseEnvelope(
        string canonicalName,
        string aliasName)
    {
        var json = CanonicalPreflightResponseEnvelope().Replace(
            "\"" + canonicalName + "\"",
            "\"" + aliasName + "\"",
            StringComparison.Ordinal);
        using var document = JsonDocument.Parse(json);

        Assert.False(CodeDocumentationOneLessonPreflightClient
            .HasExactResponseEnvelope(document.RootElement));
    }

    [Fact]
    public void PreflightClient_RejectsDuplicateAndUnknownResponseEnvelopeFields()
    {
        var canonical = CanonicalPreflightResponseEnvelope();
        using var duplicate = JsonDocument.Parse(
            canonical[..^1] + ",\"isSuccess\":true}");
        using var unknown = JsonDocument.Parse(
            canonical[..^1] + ",\"extra\":null}");
        using var valid = JsonDocument.Parse(canonical);

        Assert.True(CodeDocumentationOneLessonPreflightClient
            .HasExactResponseEnvelope(valid.RootElement));
        Assert.False(CodeDocumentationOneLessonPreflightClient
            .HasExactResponseEnvelope(duplicate.RootElement));
        Assert.False(CodeDocumentationOneLessonPreflightClient
            .HasExactResponseEnvelope(unknown.RootElement));
    }

    [Fact]
    public void Preflight_ClientObjectiveMismatchCannotCreateOrChangeSession()
    {
        using var fixture = new Fixture();
        var binding = fixture.Binding();
        var before = SnapshotFiles(fixture.Repository);
        var catalog = CodeDocumentationOneLessonGrantCatalog
            .FromTrustedGrants([Grant(binding, "grant-v1")]);
        var forgedRequest = new CodeDocumentationOneLessonPreflightRequestV1(
            new CodeDocumentationOneLessonSelectorV1(
                "caller-selected-objective",
                "opaque-grant-01"));

        var rejected = CodeDocumentationOneLessonEndpoint.Execute(
            forgedRequest,
            catalog,
            fixture.Repository);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            ((IStatusCodeHttpResult)rejected).StatusCode);
        Assert.Equal(before, SnapshotFiles(fixture.Repository));

        var validRequest = new CodeDocumentationOneLessonPreflightRequestV1(
            new CodeDocumentationOneLessonSelectorV1(
                binding.ObjectiveId,
                "opaque-grant-01"));
        var opened = CodeDocumentationOneLessonEndpoint.Execute(
            validRequest,
            catalog,
            fixture.Repository);

        Assert.Equal(
            StatusCodes.Status200OK,
            ((IStatusCodeHttpResult)opened).StatusCode);
        var response = Assert.IsAssignableFrom<IValueHttpResult>(opened).Value
            as CodeDocumentationOneLessonPreflightResponseV1;
        Assert.NotNull(response);
        Assert.Equal(binding.ObjectiveId, response.ObjectiveId);
        Assert.Equal(binding.TrainingRunId, response.TrainingRunId);
    }

    [Fact]
    public void PreflightRequest_ValidSelectorUsesCanonicalFieldsAndRoundTrips()
    {
        const string json =
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":\"opaque-grant-01\"}}";
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var request = JsonSerializer.Deserialize<
            CodeDocumentationOneLessonPreflightRequestV1>(json, options);

        Assert.NotNull(request?.Selector);
        Assert.Equal("objective-01", request.Selector.ObjectiveId);
        Assert.Equal("opaque-grant-01", request.Selector.GrantSelector);
        Assert.Equal(json, JsonSerializer.Serialize(request, options));
    }

    [Theory]
    [MemberData(nameof(PreflightRequest_MalformedSelectorJsonCases))]
    public void PreflightRequest_RejectsMalformedOrAmbiguousSelectorJson(string json)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<
            CodeDocumentationOneLessonPreflightRequestV1>(json, options));
    }

    public static TheoryData<string> PreflightRequest_MalformedSelectorJsonCases
        => new()
        {
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":\"opaque-grant-01\",\"grantSelector\":\"other-grant\"}}",
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":\"opaque-grant-01\",\"GrantSelector\":\"other-grant\"}}",
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"ObjectiveId\":\"other-objective\",\"grantSelector\":\"opaque-grant-01\"}}",
            "{\"selector\":{\"ObjectiveId\":\"objective-01\",\"grantSelector\":\"opaque-grant-01\"}}",
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"GrantSelector\":\"opaque-grant-01\"}}",
            "{\"Selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":\"opaque-grant-01\"}}",
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":\"opaque-grant-01\",\"trainingRunId\":\"caller-run\"}}",
            "{\"selector\":{\"objectiveId\":\"objective-01\"}}",
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":17}}",
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":\" opaque-grant-01\"}}",
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":\"opaque-grant-01\"},\"selector\":null}",
            "{\"selector\":{\"objectiveId\":\"objective-01\",\"grantSelector\":\"opaque-grant-01\"},\"Selector\":null}"
        };

    [Fact]
    public void Advance_PersistsMonotonicCountsAndRejectsStaleOrDecreasingWrites()
    {
        using var fixture = new Fixture();
        using var opened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var first = Require(opened.Session.Advance(
            expectedRevision: 0,
            new CodeDocumentationOneLessonEffectAccounting(
                BaselineDispatched: 1,
                BaselineCompleted: 1,
                EvaluatorDispatched: 2,
                EvaluatorCompleted: 1)));
        Assert.Equal(1, first.Revision);

        var beforeRejectedWrites = File.ReadAllBytes(
            Path.Combine(fixture.Repository, first.HistoryRelativePath));
        var stale = opened.Session.Advance(
            expectedRevision: 0,
            new CodeDocumentationOneLessonEffectAccounting(
                BaselineDispatched: 2,
                BaselineCompleted: 2,
                EvaluatorDispatched: 4,
                EvaluatorCompleted: 3));
        Assert.False(stale.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.StaleRevisionErrorCode, stale.ErrorCode);

        var decreasing = opened.Session.Advance(
            expectedRevision: 1,
            new CodeDocumentationOneLessonEffectAccounting());
        Assert.False(decreasing.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.NonMonotonicErrorCode, decreasing.ErrorCode);
        Assert.Equal(beforeRejectedWrites, File.ReadAllBytes(
            Path.Combine(fixture.Repository, first.HistoryRelativePath)));

        opened.Session.Dispose();
        using var reopened = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        Assert.Equal(1, reopened.Session.Snapshot.Revision);
        Assert.Equal(1, reopened.Session.Snapshot.Effects.BaselineCompleted);
        Assert.Equal(2, reopened.Session.Snapshot.Effects.EvaluatorDispatched);
        Assert.Equal(1, reopened.Session.Snapshot.Effects.EvaluatorCompleted);
    }

    [Fact]
    public void OpenOrCreate_CompetingObjectiveOwnerReturnsBusyWithoutCreatingAnotherHistory()
    {
        using var fixture = new Fixture();
        using var owner = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var historyPath = Path.Combine(
            fixture.Repository,
            owner.Session.Snapshot.HistoryRelativePath);
        var before = File.ReadAllBytes(historyPath);

        var contender = CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding());

        Assert.False(contender.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.BusyErrorCode, contender.ErrorCode);
        Assert.Equal(before, File.ReadAllBytes(historyPath));
    }

    [Fact]
    public void OpenOrCreate_ExistingSessionWithEmptyLockRequiresRecovery()
    {
        using var fixture = new Fixture();
        string historyPath;
        using (var created = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
                   fixture.ModelRoot,
                   fixture.Binding())))
        {
            historyPath = Path.Combine(fixture.Repository, created.Session.Snapshot.HistoryRelativePath);
        }

        var lockPath = Path.Combine(Path.GetDirectoryName(historyPath)!, ".owner.lock");
        File.WriteAllBytes(lockPath, Array.Empty<byte>());

        var result = CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding());

        Assert.False(result.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Empty(File.ReadAllBytes(lockPath));
    }

    [Fact]
    public void Advance_ReplacedOpenLockPathRequiresRecoveryWithoutMutatingHistory()
    {
        if (!OperatingSystem.IsLinux())
        {
            throw Xunit.Sdk.SkipException.ForSkip(
                "LINUX_ONLY_OPEN_LOCK_UNLINK: unlinking an open custody lock is exercised on Linux.");
        }

        using var fixture = new Fixture();
        using var owner = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));
        var historyPath = Path.Combine(
            fixture.Repository,
            owner.Session.Snapshot.HistoryRelativePath);
        var lockPath = Path.Combine(Path.GetDirectoryName(historyPath)!, ".owner.lock");
        var before = File.ReadAllBytes(historyPath);
        var lockBytes = System.Text.Encoding.UTF8.GetBytes(
            "s04-one-lesson-session-v1\n" + owner.Session.Snapshot.SessionId + "\n");

        File.Delete(lockPath);
        File.WriteAllBytes(lockPath, lockBytes);
        File.SetUnixFileMode(
            lockPath,
            UnixFileMode.UserRead | UnixFileMode.UserWrite);
        using var replacementOwner = Require(CodeDocumentationOneLessonSession.OpenOrCreate(
            fixture.ModelRoot,
            fixture.Binding()));

        var result = owner.Session.Advance(
            expectedRevision: 0,
            new CodeDocumentationOneLessonEffectAccounting(
                BaselineDispatched: 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Equal(before, File.ReadAllBytes(historyPath));
    }

    private static CodeDocumentationOneLessonGrantV1 Grant(
        CodeDocumentationOneLessonSessionBinding binding,
        string version)
        => new(
            GrantId: "synthetic-grant",
            GrantVersion: version,
            GrantSelector: "opaque-grant-01",
            ObjectiveId: binding.ObjectiveId,
            TrainingRunId: binding.TrainingRunId,
            CandidateSha256: binding.CandidateSha256,
            ParentManifestSha256: binding.ParentManifestSha256,
            ParentWeightsSha256: binding.ParentWeightsSha256,
            ConfigurationSha256: binding.ConfigurationSha256,
            ExamConditionSha256: binding.ExamConditionSha256);

    private static CodeDocumentationOneLessonVerifiedTerminalReceiptRefsV1 TerminalReceiptRefs(
        CodeDocumentationOneLessonSessionSnapshot snapshot,
        bool includePublication = false,
        string dispositionReceiptId = "disposition-receipt")
    // Deliberately synthetic reference envelopes for lifecycle-frame shape
    // and conflict tests only. These IDs/hashes are not receipt artifacts,
    // do not prove completion, and must never authorize replay.
        => new(
            snapshot.Binding.ObjectiveId,
            snapshot.SessionId,
            snapshot.Binding.TrainingRunId,
            CodeDocumentationOneLessonSession.ComputeBindingSha256(snapshot.Binding),
            new("trial-evaluation-receipt", Hex('a')),
            new("quality-decision-receipt", Hex('b')),
            new(dispositionReceiptId, Hex('c')),
            includePublication
                ? new CodeDocumentationOneLessonTypedReceiptRefV1<CodeDocumentationOneLessonPublicationReceiptV1>(
                    "publication-receipt",
                    Hex('d'))
            : null,
            new("final-reopen-receipt", Hex('e')));

    private static string CanonicalPreflightResponseEnvelope()
        => "{\"isSuccess\":true,\"errorCode\":\"\",\"errorMessage\":\"\"," +
           "\"disposition\":\"Fresh\",\"objectiveId\":\"objective-01\"," +
           "\"trainingRunId\":\"run-01\",\"bindingSha256\":\"" + Hex('a') +
           "\",\"sessionId\":\"session-01\",\"sessionRevision\":0," +
           "\"terminalOutcome\":null,\"terminalOutcomeSha256\":null}";

    private static T Require<T>(AgentInfrastructure.OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private static string Hex(char value) => new(value, 64);

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string[] SnapshotFiles(string root)
        => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

    private sealed class Fixture : IDisposable
    {
        private readonly string _root = Path.Combine(
            Path.GetTempPath(),
            "code-documentation-one-lesson-session-" + Guid.NewGuid().ToString("N"));

        internal Fixture()
        {
            Repository = Path.Combine(_root, "repo");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            Directory.CreateDirectory(Path.Combine(Repository, "training-artifacts", "models"));
            File.WriteAllText(Path.Combine(Repository, "LlmFromScratch.sln"), "synthetic fixture root");
            var root = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(Repository);
            Assert.True(root.IsSuccess, root.ErrorMessage);
            Assert.NotNull(root.Value);
            ModelRoot = root.Value!;
        }

        internal string Repository { get; }
        internal RepositoryOwnedModelArtifactRoot ModelRoot { get; }

        internal CodeDocumentationOneLessonSessionBinding Binding() => new(
            ObjectiveId: "code-documentation-one-lesson:synthetic-objective",
            GrantId: "synthetic-grant",
            GrantVersion: "grant-v1",
            TrainingRunId: "synthetic-training-run-01",
            CandidateSha256: Hex('1'),
            ParentManifestSha256: Hex('2'),
            ParentWeightsSha256: Hex('3'),
            ConfigurationSha256: Hex('4'),
            ExamConditionSha256: Hex('5'));

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
    }
}
