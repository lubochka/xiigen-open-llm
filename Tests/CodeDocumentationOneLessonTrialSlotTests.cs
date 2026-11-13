// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using TrainableRouter.Training;
using Xunit.Abstractions;

public sealed class CodeDocumentationOneLessonTrialSlotTests(ITestOutputHelper output)
{
    private const string Recovery = CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode;
    private const string Capacity = CodeDocumentationOneLessonSession.CapacityExceededErrorCode;
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private static readonly string[] StartedNames = ["CaseOrdinal", "ModelSerializationBeforeSha256", "PlanSha256", "RepeatOrdinal",
        "ReservedEvaluatorAttemptOrdinal", "SlotId", "SlotOrdinal"];
    private static readonly string[] ObservationNames = ["CaseId", "EvaluationRequestIdSha256", "ModelSerializationAfterSha256",
        "Passed", "ReasonCode", "StudentAnswerCharCount", "StudentAnswerSha256"];

    public static TheoryData<string, bool> Reasons
    {
        get
        {
            var rows = new TheoryData<string, bool>();
            foreach (var reason in new[] { "correct", "missing_essential_behavior", "invented_behavior",
                "wrong_purpose_or_result", "unclear_or_unverifiable" })
                foreach (var passed in new[] { false, true }) rows.Add(reason, passed);
            return rows;
        }
    }

    [Theory]
    [InlineData(1, 2, false, 343)]
    [InlineData(1, 2, true, 347)]
    [InlineData(1, 5000, true, 353)]
    [InlineData(200, 25, true, 353)]
    public void StartedLegalEndpointsHaveExactSevenMembersAndIndependentBytes(int n, int r, bool maximum, int bytes)
    {
        var b = Binding(n, r);
        var fields = StartedFields(maximum ? r : 1, maximum ? n : 1, maximum ? n * r : 1, maximum ? 10000 : 1);
        var raw = Obj(fields); Assert.Equal(bytes, Utf8.GetByteCount(raw));
        var decoded = Require(Decode(raw, true, b));
        Assert.True(decoded.IsStarted); Assert.Equal(H('a'), decoded.SlotId);
        Assert.Equal(StartedNames, decoded.Value.EnumerateObject().Select(p => p.Name));
        Assert.Equal(7, decoded.Value.EnumerateObject().Count());
        Assert.Equal(raw, decoded.Value.GetRawText()); Assert.Equal(Utf8.GetBytes(raw), decoded.CanonicalBytes);
    }

    [Theory, MemberData(nameof(Reasons))]
    public void CompletedHasExactlyTwoMembersAndSevenObservationPrimitivesWithoutSemanticAuthority(string reason, bool passed)
    {
        foreach (var chars in new[] { 0, 65536 })
        {
            var raw = Obj(CompletedFields(reason, passed, chars));
            Assert.InRange(Utf8.GetByteCount(raw), 523, 547);
            var value = Require(Decode(raw, false, Binding()));
            Assert.False(value.IsStarted); Assert.Equal(H('a'), value.SlotId);
            Assert.Equal(new[] { "Observation", "SlotId" }, value.Value.EnumerateObject().Select(p => p.Name));
            var observation = value.Value.GetProperty("Observation");
            Assert.Equal(ObservationNames, observation.EnumerateObject().Select(p => p.Name));
            Assert.Equal(71, Utf8.GetByteCount(observation.GetProperty("CaseId").GetString()!));
            Assert.Equal(reason, observation.GetProperty("ReasonCode").GetString());
            Assert.Equal(passed, observation.GetProperty("Passed").GetBoolean());
            Assert.Equal(chars, observation.GetProperty("StudentAnswerCharCount").GetInt32());
            Assert.Equal(Utf8.GetBytes(raw), value.CanonicalBytes);
        }
    }

    [Fact]
    public void IndependentLegalTupleArithmeticDistinguishes353FromConservative355Ceiling()
    {
        var count = 0; var maximum = 0;
        for (var n = 1; n <= 200; n++)
            for (var r = 2; r <= 5000; r++)
            {
                if ((long)n * r > 5000) continue;
                count++; maximum = Math.Max(maximum, Utf8.GetByteCount(Obj(StartedFields(r, n, n * r, 10000))));
            }
        Assert.Equal(29103, count); Assert.Equal(353, maximum);
        var independentMaxima = Obj(StartedFields(5000, 200, 5000, 10000));
        Assert.Equal(355, Utf8.GetByteCount(independentMaxima)); Assert.Equal(1000000, 200 * 5000);
        Reject(independentMaxima, true, Binding(200, 5000), Recovery);
        var completed = Obj(CompletedFields("missing_essential_behavior", false, 65536));
        Assert.Equal(547, Utf8.GetByteCount(completed)); Require(Decode(completed, false, Binding()));
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void EveryRootMemberRequiresExactNonNullClosedMembershipAndTypes(bool started)
    {
        var fields = Fields(started);
        foreach (var name in fields.Keys)
        {
            var missing = Copy(fields); missing.Remove(name); RejectMalformed(Obj(missing), started);
            var alias = Copy(fields); alias.Remove(name); alias[char.ToLowerInvariant(name[0]) + name[1..]] = fields[name];
            RejectMalformed(Obj(alias), started);
            foreach (var raw in new[] { "null", "[]", "\"wrong\"", "false" })
            {
                var bad = Copy(fields); bad[name] = raw; RejectMalformed(Obj(bad), started);
            }
        }
        foreach (var name in new[] { "X", "Unknown", "Kind", "SchemaVersion", "Payload", "ObservationArray", "RawAnswer", "Preview" })
        { var bad = Copy(fields); bad[name] = Q(""); RejectMalformed(Obj(bad), started); }
        Require(Decode(Obj(fields), started, Binding()));
    }

    [Fact]
    public void ObservationRequiresAllSevenExactMembersAndRejectsAmbiguityAndWrongTypes()
    {
        var observation = ObservationFields();
        foreach (var name in observation.Keys)
        {
            var missing = Copy(observation); missing.Remove(name); Fault(Obj(missing));
            var alias = Copy(observation); alias.Remove(name); alias[char.ToLowerInvariant(name[0]) + name[1..]] = observation[name];
            Fault(Obj(alias));
            foreach (var raw in new[] { "null", "[]", "{}", "\"wrong\"" })
            { var bad = Copy(observation); bad[name] = raw; Fault(Obj(bad)); }
        }
        foreach (var name in new[] { "X", "EvaluationRequestId", "StudentAnswer", "AnswerPreview", "Evidence", "Observations" })
        { var bad = Copy(observation); bad[name] = Q(""); Fault(Obj(bad)); }
        foreach (var raw in new[] { "[]", "\"observation\"", "0", "true", "null" }) Fault(raw);
        Require(Decode(Obj(CompletedFields()), false, Binding()));
        void Fault(string raw) { var changed = CompletedFields(); changed["Observation"] = raw; RejectMalformed(Obj(changed), false); }
    }

    [Fact]
    public void MeasuredDuplicateControlsProvePureCapacityPriorityAndInsideCapAmbiguity()
    {
        var started = StartedFields(); Assert.Equal(343, Utf8.GetByteCount(Obj(started)));
        var expectedStarted = new Dictionary<string, int> { ["SlotId"] = 419, ["PlanSha256"] = 423, ["RepeatOrdinal"] = 361,
            ["CaseOrdinal"] = 359, ["SlotOrdinal"] = 359, ["ModelSerializationBeforeSha256"] = 443,
            ["ReservedEvaluatorAttemptOrdinal"] = 379 };
        foreach (var name in started.Keys)
        { var raw = Duplicate(started, name); Assert.Equal(expectedStarted[name], Utf8.GetByteCount(raw)); Reject(raw, true, Binding(), Capacity); }
        var completed = CompletedFields(); Assert.Equal(523, Utf8.GetByteCount(Obj(completed)));
        foreach (var (name, bytes) in new[] { ("SlotId", 599), ("Observation", 969) })
        { var raw = Duplicate(completed, name); Assert.Equal(bytes, Utf8.GetByteCount(raw)); Reject(raw, false, Binding(), Capacity); }
        var expectedObservation = new Dictionary<string, int> { ["CaseId"] = 606, ["Passed"] = 537, ["ReasonCode"] = 546,
            ["StudentAnswerSha256"] = 612, ["StudentAnswerCharCount"] = 550, ["EvaluationRequestIdSha256"] = 618,
            ["ModelSerializationAfterSha256"] = 622 };
        foreach (var name in ObservationNames)
        {
            var changed = CompletedFields(); changed["Observation"] = Duplicate(ObservationFields(), name);
            var raw = Obj(changed); var bytes = Utf8.GetByteCount(raw); Assert.Equal(expectedObservation[name], bytes);
            Reject(raw, false, Binding(), name is "Passed" or "ReasonCode" ? Recovery : Capacity);
        }
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void HashSyntaxRejectsWrongWidthAlphabetCasePrefixAndUnicodeWithMeasuredRawPriority(bool started)
    {
        foreach (var name in started ? new[] { "SlotId", "PlanSha256", "ModelSerializationBeforeSha256" } : new[] { "SlotId" })
            foreach (var text in BadHex())
            { var bad = Fields(started); bad[name] = Q(text); RejectMalformed(Obj(bad), started); }
        if (!started)
            foreach (var name in new[] { "StudentAnswerSha256", "EvaluationRequestIdSha256", "ModelSerializationAfterSha256" })
                foreach (var text in BadHex())
                { var obs = ObservationFields(); obs[name] = Q(text); var bad = CompletedFields(); bad["Observation"] = Obj(obs); RejectMalformed(Obj(bad), false); }
        Require(Decode(Obj(Fields(started)), started, Binding()));
    }

    [Fact]
    public void CaseIdBooleanAndReasonRequireTheirExactPrimitives()
    {
        foreach (var text in new[] { "", H('b'), "SHA256:" + H('b'), "sha256:" + H('B'), "sha256:" + H('g'),
            "sha256:" + new string('b', 63), "sha256:" + new string('b', 65), " sha256:" + H('b'), "sha256:" + new string('é', 64) })
            Fault("CaseId", Q(text));
        foreach (var raw in new[] { "\"true\"", "1", "0", "null", "[]" }) Fault("Passed", raw);
        foreach (var text in new[] { "", "Correct", " correct", "correct ", "wrong", "missing-essential-behavior" }) Fault("ReasonCode", Q(text));
        foreach (var raw in new[] { "true", "5", "null", "[]" }) Fault("ReasonCode", raw);
        void Fault(string name, string raw) { var obs = ObservationFields(); obs[name] = raw; var bad = CompletedFields(); bad["Observation"] = Obj(obs); RejectMalformed(Obj(bad), false); }
    }

    [Theory]
    [InlineData("RepeatOrdinal", 1, 25)]
    [InlineData("CaseOrdinal", 1, 200)]
    [InlineData("SlotOrdinal", 1, 5000)]
    [InlineData("ReservedEvaluatorAttemptOrdinal", 1, 10000)]
    [InlineData("StudentAnswerCharCount", 0, 65536)]
    public void IntegerFieldsRequireCanonicalInt32AndExpectedBounds(string name, int minimum, int maximum)
    {
        var started = name != "StudentAnswerCharCount"; var binding = Binding(200, 25);
        foreach (var valid in new[] { minimum, maximum })
        {
            var fields = started ? StartedFields() : ObservationFields(); fields[name] = valid.ToString(CultureInfo.InvariantCulture);
            var raw = Wrap(fields); Assert.True(Utf8.GetByteCount(raw) <= (started ? 355 : 547)); Require(Decode(raw, started, binding));
        }
        foreach (var raw in new[] { (minimum - 1).ToString(CultureInfo.InvariantCulture), (maximum + 1).ToString(CultureInfo.InvariantCulture),
            "-1", "1.0", "1e0", "\"1\"", "2147483648", "9223372036854775808", "null", "true" })
        { var fields = started ? StartedFields() : ObservationFields(); fields[name] = raw; RejectMalformed(Wrap(fields), started, binding); }
        string Wrap(SortedDictionary<string, string> fields)
        { if (started) return Obj(fields); var completed = CompletedFields(); completed["Observation"] = Obj(fields); return Obj(completed); }
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void BindingGrammarPreservesPunctuationFirstRunAndEscapedUnicodeIdentities(bool started)
    {
        foreach (var b in new[] { Binding(), Binding() with { TrainingRunId = ".run" }, Binding() with { TrainingRunId = "_run" },
            Binding() with { ObjectiveId = new string('\u0800', 128), GrantId = new string('"', 128),
                GrantVersion = new string('\\', 64), TrainingRunId = new string('a', 128) } })
            Require(Decode(Obj(Fields(started)), started, b));
        var b0 = Binding();
        var invalid = new List<CodeDocumentationOneLessonSessionBinding?>
        {
            null, b0 with { FormatVersion = 1 }, b0 with { DescriptorsV2 = null },
            b0 with { ObjectiveId = "" }, b0 with { ObjectiveId = " objective" }, b0 with { ObjectiveId = "objective " },
            b0 with { ObjectiveId = new string('a', 129) }, b0 with { ObjectiveId = "\ud800" },
            b0 with { GrantId = new string('é', 65) }, b0 with { GrantId = " g" }, b0 with { GrantVersion = "v " },
            b0 with { GrantVersion = new string('a', 65) }, b0 with { GrantVersion = new string('é', 33) },
            b0 with { TrainingRunId = "" }, b0 with { TrainingRunId = "a/b" }, b0 with { TrainingRunId = "é" },
            b0 with { TrainingRunId = new string('a', 129) }, b0 with { CandidateSha256 = H('A') },
            b0 with { ParentManifestSha256 = H('g') }, b0 with { ParentWeightsSha256 = "sha256:" + H('a') },
            b0 with { ConfigurationSha256 = "" }, b0 with { ExamConditionSha256 = new string('a', 63) },
        };
        foreach (var d in new[] { b0.DescriptorsV2! with { SchemaVersion = 1 }, b0.DescriptorsV2! with { CaseCount = 0 },
            b0.DescriptorsV2! with { CaseCount = 201 }, b0.DescriptorsV2! with { RepeatCount = 1 },
            b0.DescriptorsV2! with { RepeatCount = 5001 }, b0.DescriptorsV2! with { CaseCount = 200, RepeatCount = 26 },
            b0.DescriptorsV2! with { SourceSelectionSha256 = H('A') }, b0.DescriptorsV2! with { ParentCheckpointSha256 = "" },
            b0.DescriptorsV2! with { CandidateSelectionSha256 = H('g') }, b0.DescriptorsV2! with { CorpusDescriptorSha256 = "sha256:" + H('a') },
            b0.DescriptorsV2! with { EvaluationPlanSha256 = new string('a', 65) }, b0.DescriptorsV2! with { EvaluatorProfileSha256 = new string('a', 63) },
            b0.DescriptorsV2! with { ProfileSelectionSha256 = H('A') } })
            invalid.Add(b0 with { DescriptorsV2 = d });
        foreach (var bad in invalid) Reject(Obj(Fields(started)), started, bad, Recovery);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void CanonicalRootNestedOrderWhitespaceEscapesAndNumericAliasesAreRejected(bool started)
    {
        var fields = Fields(started); var raw = Obj(fields);
        Reject("{" + string.Join(',', fields.Reverse().Select(p => Q(p.Key) + ":" + p.Value)) + "}", started, Binding(), Recovery);
        Reject(raw.Insert(1, " "), started, Binding(), Recovery);
        Reject(raw.Replace(Q("SlotId"), "\"\\u0053lotId\"", StringComparison.Ordinal), started, Binding(), Recovery);
        Reject(raw.Replace(Q(H('a')), "\"\\u0061" + new string('a', 63) + "\"", StringComparison.Ordinal), started, Binding(), Recovery);
        if (!started)
        {
            var obs = ObservationFields();
            fields["Observation"] = "{" + string.Join(',', obs.Reverse().Select(p => Q(p.Key) + ":" + p.Value)) + "}";
            Reject(Obj(fields), false, Binding(), Recovery);
            fields["Observation"] = Obj(obs).Replace(":0,", ":0.0,", StringComparison.Ordinal); Reject(Obj(fields), false, Binding(), Recovery);
        }
        Require(Decode(raw, started, Binding()));
    }

    [Theory]
    [InlineData(true, 354, Recovery)] [InlineData(true, 355, Recovery)] [InlineData(true, 356, Capacity)]
    [InlineData(false, 546, Recovery)] [InlineData(false, 547, Recovery)] [InlineData(false, 548, Capacity)]
    public void CompleteRawCapsPrecedeMalformedFieldsAndBinding(bool started, int bytes, string expected)
    {
        var raw = CapControl(Fields(started), bytes); Assert.Equal(bytes, Utf8.GetByteCount(raw));
        Reject(raw, started, Binding(), expected); Reject(raw, started, null, expected);
        foreach (var fault in new[] { "SlotId", started ? "PlanSha256" : "Observation" })
        { var fields = Fields(started); fields[fault] = "null"; var malformed = CapControl(fields, bytes); Assert.Equal(bytes, Utf8.GetByteCount(malformed)); Reject(malformed, started, Binding() with { FormatVersion = 1 }, expected); }
        var whitespace = Obj(Fields(started)).Insert(1, new string(' ', bytes - Utf8.GetByteCount(Obj(Fields(started)))));
        Assert.Equal(bytes, Utf8.GetByteCount(whitespace)); Reject(whitespace, started, Binding(), expected);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void NonobjectUndefinedAndDisposedGraphsHaveOrdinaryRecoveryWithoutPartialValue(bool started)
    {
        foreach (var raw in new[] { "[]", Q(new string('x', 1000)), "null", "true", "0" }) Reject(raw, started, Binding(), Recovery);
        AssertFailure(CodeDocumentationOneLessonTrialSlotV2.Decode(default, started, Binding()), Recovery);
        var document = JsonDocument.Parse(Obj(Fields(started))); var root = document.RootElement; document.Dispose();
        AssertFailure(CodeDocumentationOneLessonTrialSlotV2.Decode(root, started, Binding()), Recovery);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void SuccessOwnsDetachedGraphAndFreshDefensiveCanonicalBytes(bool started)
    {
        var raw = Obj(Fields(started)); var input = Utf8.GetBytes(raw);
        CodeDocumentationOneLessonTrialSlotV2 value;
        using (var document = JsonDocument.Parse(input)) value = Require(CodeDocumentationOneLessonTrialSlotV2.Decode(document.RootElement, started, Binding()));
        Array.Fill(input, (byte)'x');
        Assert.Equal(raw, value.Value.GetRawText()); Assert.Equal(H('a'), value.SlotId);
        var first = value.CanonicalBytes; first[0] = 0; var second = value.CanonicalBytes;
        Assert.NotSame(first, second); Assert.Equal(Utf8.GetBytes(raw), second);
        Assert.Equal(raw, value.Value.GetRawText());
    }

    [Fact]
    public void StructuralDescriptionDoesNotAuthenticatePlanOrdinalSlotOrAssessmentRelationships()
    {
        var b = Binding(2, 2); var fields = StartedFields(1, 1, 4, 10000);
        Assert.NotEqual(b.DescriptorsV2!.EvaluationPlanSha256, H('b'));
        Require(Decode(Obj(fields), true, b)); // Supplied SlotId and row-major mismatch remain descriptive syntax.
        Require(Decode(Obj(CompletedFields("correct", false, 0)), false, b));
        Require(Decode(Obj(CompletedFields("invented_behavior", true, 65536)), false, b));
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void ActualInternalAndPublicOwnerRefuseStructurallyValidSlotsWithoutEffects(bool started)
    {
        using var fixture = new Fixture(); var b = Binding(); var (path, genesis) = Seed(fixture, b);
        var raw = Obj(Fields(started)); Require(Decode(raw, started, b));
        var frame = Frame(b, 1, Kind(started), Digest(genesis[..^1]), raw);
        Assert.True(frame.Length <= (started ? 3176 : 3372));
        AssertRefusalAndRestore(fixture, b, path, genesis, genesis.Concat(frame).ToArray(), Recovery);
    }

    [Theory]
    [InlineData(true, 354, Recovery, 2146)] [InlineData(true, 355, Recovery, 2147)] [InlineData(true, 356, Capacity, 2148)]
    [InlineData(false, 546, Recovery, 2342)] [InlineData(false, 547, Recovery, 2343)] [InlineData(false, 548, Capacity, 2344)]
    public void ActualOwnerNestedCapRefusalIsReachableBelowFullFrameCap(bool started, int bytes, string expected, int frameBytes)
    {
        using var fixture = new Fixture(); var b = Binding(); var (path, genesis) = Seed(fixture, b);
        Assert.Equal(1699, genesis.Length); Assert.NotEqual(Digest(genesis), Digest(genesis[..^1]));
        var raw = CapControl(Fields(started), bytes); Assert.Equal(bytes, Utf8.GetByteCount(raw)); Reject(raw, started, b, expected);
        var frame = Frame(b, 1, Kind(started), Digest(genesis[..^1]), raw);
        Assert.Equal(frameBytes, frame.Length); Assert.True(frame.Length <= (started ? 3176 : 3372));
        AssertRefusalAndRestore(fixture, b, path, genesis, genesis.Concat(frame).ToArray(), expected);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void OwnerCommonCanonicalGuardPrecedesPureDuplicateCapacityRefusal(bool started)
    {
        using var fixture = new Fixture(); var b = Binding(); var (path, genesis) = Seed(fixture, b);
        var raw = Duplicate(Fields(started), "SlotId"); Assert.True(Utf8.GetByteCount(raw) > (started ? 355 : 547));
        Reject(raw, started, b, Capacity);
        var frame = Frame(b, 1, Kind(started), Digest(genesis[..^1]), raw);
        Assert.True(frame.Length <= (started ? 3176 : 3372));
        AssertRefusalAndRestore(fixture, b, path, genesis, genesis.Concat(frame).ToArray(), Recovery);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void HeldRawPrefixCustodyRemainsFirstBeforeNestedSlotCapacity(bool started)
    {
        using var fixture = new Fixture(); var b = Binding(); var (path, genesis) = Seed(fixture, b);
        var lockPath = Path.Combine(Path.GetDirectoryName(path)!, ".owner.lock"); var lockBytes = File.ReadAllBytes(lockPath);
        using var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, b, 10000));
        var raw = CapControl(Fields(started), started ? 356 : 548); Reject(raw, started, b, Capacity);
        var bytes = genesis.Concat(Frame(b, 1, Kind(started), Digest(genesis[..^1]), raw)).ToArray(); File.WriteAllBytes(path, bytes);
        AssertFailure(owner.Session.ReadCapacityPrefixV2(), Recovery); Assert.Equal(bytes, File.ReadAllBytes(path));
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), owner.Session.Snapshot.Effects);
        File.WriteAllBytes(path, genesis); Assert.Equal(1, Require(owner.Session.ReadCapacityPrefixV2()).PrefixFrames);
        owner.Dispose(); // Windows external lock read is legal only after the exclusive owner is released.
        Assert.Equal(genesis, File.ReadAllBytes(path)); Assert.Equal(lockBytes, File.ReadAllBytes(lockPath)); AssertOnlyExpectedFiles(path);
    }

    public static TheoryData<bool, string> ParserFaults
    {
        get
        {
            var rows = new TheoryData<bool, string>();
            foreach (var started in new[] { true, false })
                foreach (var fault in new[] { "leading-zero", "comment", "torn", "invalid-utf8" }) rows.Add(started, fault);
            return rows;
        }
    }

    [Theory, MemberData(nameof(ParserFaults))]
    public void ActualOwnerParserRejectsMalformedRawTokensWithoutClaimingPureDecoderExecution(bool started, string fault)
    {
        using var fixture = new Fixture(); var b = Binding(); var (path, genesis) = Seed(fixture, b);
        var raw = Obj(Fields(started));
        raw = fault switch
        {
            "leading-zero" => started ? raw.Replace("\"CaseOrdinal\":1", "\"CaseOrdinal\":01", StringComparison.Ordinal)
                : raw.Replace("\"StudentAnswerCharCount\":0", "\"StudentAnswerCharCount\":00", StringComparison.Ordinal),
            "comment" => raw.Insert(1, "/*comment*/"),
            "torn" => raw[..^1],
            "invalid-utf8" => raw,
            _ => throw new ArgumentOutOfRangeException(nameof(fault)),
        };
        Assert.True(Utf8.GetByteCount(raw) <= (started ? 355 : 547));
        var frame = Frame(b, 1, Kind(started), Digest(genesis[..^1]), raw);
        if (fault == "invalid-utf8")
        {
            var marker = Utf8.GetBytes("\"SlotId\":\""); var index = frame.AsSpan().IndexOf(marker); Assert.True(index >= 0);
            frame[index + marker.Length] = 0xff;
        }
        Assert.True(frame.Length <= (started ? 3176 : 3372));
        AssertRefusalAndRestore(fixture, b, path, genesis, genesis.Concat(frame).ToArray(), Recovery);
    }

    [Fact]
    public void RuntimeIdentityBindsNewTrialSlotDecoderAndTestToLoadedNormalArtifacts()
    {
        var product = typeof(CodeDocumentationOneLessonTrialSlotV2).Assembly;
        var tests = typeof(CodeDocumentationOneLessonTrialSlotTests).Assembly;
        Assert.Equal("TrainableRouter", product.GetName().Name); Assert.Equal("LlmFromScratch.Tests", tests.GetName().Name);
        var directory = Path.GetDirectoryName(tests.Location)!;
        Assert.EndsWith(Path.Combine("Tests", "bin", "Release", "net9.0"), directory, StringComparison.Ordinal);
        Assert.Equal(Path.Combine(directory, "TrainableRouter.dll"), product.Location);
        Assert.True(Require(Decode(Obj(StartedFields()), true, Binding())).IsStarted);
        foreach (var path in new[] { product.Location, tests.Location, Path.Combine(directory, "LlmFromScratch.Tests.runtimeconfig.json"),
            Path.Combine(directory, "LlmFromScratch.Tests.deps.json") })
        {
            var bytes = File.ReadAllBytes(path); Assert.NotEmpty(bytes);
            output.WriteLine("TRIAL_SLOT_RUNTIME_IDENTITY path={0} bytes={1} sha256={2}", path, bytes.Length, Digest(bytes));
        }
    }

    private static void AssertRefusalAndRestore(Fixture fixture, CodeDocumentationOneLessonSessionBinding b,
        string path, byte[] genesis, byte[] bytes, string expectedError)
    {
        var lockPath = Path.Combine(Path.GetDirectoryName(path)!, ".owner.lock"); var lockBytes = File.ReadAllBytes(lockPath);
        File.WriteAllBytes(path, bytes);
        for (var attempt = 0; attempt < 2; attempt++)
        {
            AssertFailure(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, b, 10000), expectedError);
            AssertFailure(CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, b, 10000), expectedError);
            Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.Equal(lockBytes, File.ReadAllBytes(lockPath)); AssertOnlyExpectedFiles(path);
        }
        File.WriteAllBytes(path, genesis);
        using (var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, b, 10000)))
        {
            var capacity = Require(restored.Session.ReadCapacityPrefixV2());
            Assert.Equal(1, capacity.PrefixFrames); Assert.Equal(genesis.Length, capacity.PrefixBytes);
            Assert.Equal(0, restored.Session.Snapshot.Revision); Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), restored.Session.Snapshot.Effects);
        }
        AssertFailure(CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, b, 10000),
            CodeDocumentationOneLessonSession.BaselinePrerequisiteUnavailableErrorCode); // Clean local Genesis remains unable to supply the external baseline authority.
        Assert.Equal(genesis, File.ReadAllBytes(path)); Assert.Equal(lockBytes, File.ReadAllBytes(lockPath)); AssertOnlyExpectedFiles(path);
    }

    private static void AssertOnlyExpectedFiles(string path)
        => Assert.Equal(new[] { ".owner.lock", "session.jsonl" }, Directory.GetFileSystemEntries(Path.GetDirectoryName(path)!)
            .Select(Path.GetFileName).Order(StringComparer.Ordinal));
    private static (string Path, byte[] Genesis) Seed(Fixture fixture, CodeDocumentationOneLessonSessionBinding b)
    {
        string path;
        using (var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, b, 10000)))
            path = Path.Combine(fixture.Repository, owner.Session.Snapshot.HistoryRelativePath);
        var genesis = Frame(b); File.WriteAllBytes(path, genesis);
        if (OperatingSystem.IsLinux()) File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        using (var positive = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, b, 10000)))
        {
            Assert.Equal(1, Require(positive.Session.ReadCapacityPrefixV2()).PrefixFrames);
            Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), positive.Session.Snapshot.Effects);
        }
        AssertOnlyExpectedFiles(path); return (path, genesis);
    }

    // Literal sorted object assembly, JSON spelling and direct SHA256 are the
    // independent expected-byte oracles. No production wire/hash helper builds fixtures.
    private static CodeDocumentationOneLessonSessionBinding Binding(int n = 1, int r = 2)
        => new("objective", "g", "v", "-run", H('1'), H('2'), H('3'), H('4'), H('5'))
            { FormatVersion = 2, DescriptorsV2 = new(2, H('a'), H('b'), H('c'), H('d'), H('e'), n, r, H('f'), H('0')) };
    private static SortedDictionary<string, string> StartedFields(int repeat = 1, int @case = 1, int slot = 1, int attempt = 1)
        => Dict(("SlotId", Q(H('a'))), ("PlanSha256", Q(H('b'))), ("ModelSerializationBeforeSha256", Q(H('c'))),
            ("RepeatOrdinal", repeat.ToString(CultureInfo.InvariantCulture)), ("CaseOrdinal", @case.ToString(CultureInfo.InvariantCulture)),
            ("SlotOrdinal", slot.ToString(CultureInfo.InvariantCulture)), ("ReservedEvaluatorAttemptOrdinal", attempt.ToString(CultureInfo.InvariantCulture)));
    private static SortedDictionary<string, string> ObservationFields(string reason = "correct", bool passed = true, int chars = 0)
        => Dict(("CaseId", Q("sha256:" + H('b'))), ("Passed", passed ? "true" : "false"), ("ReasonCode", Q(reason)),
            ("StudentAnswerSha256", Q(H('c'))), ("StudentAnswerCharCount", chars.ToString(CultureInfo.InvariantCulture)),
            ("EvaluationRequestIdSha256", Q(H('d'))), ("ModelSerializationAfterSha256", Q(H('e'))));
    private static SortedDictionary<string, string> CompletedFields(string reason = "correct", bool passed = true, int chars = 0)
        => Dict(("SlotId", Q(H('a'))), ("Observation", Obj(ObservationFields(reason, passed, chars))));
    private static SortedDictionary<string, string> Fields(bool started) => started ? StartedFields() : CompletedFields();
    private static string Kind(bool started) => started ? "TrialSlotStarted" : "TrialSlotCompleted";
    private static string CapControl(SortedDictionary<string, string> original, int bytes)
    {
        var fields = Copy(original); fields["X"] = Q(""); var baseBytes = Utf8.GetByteCount(Obj(fields)); Assert.True(baseBytes <= bytes);
        fields["X"] = Q(new string('x', bytes - baseBytes)); var raw = Obj(fields); Assert.Equal(bytes, Utf8.GetByteCount(raw)); return raw;
    }
    private static string DescriptorJson(CodeDocumentationOneLessonDescriptorsV2 d)
        => Obj(("SchemaVersion", d.SchemaVersion.ToString(CultureInfo.InvariantCulture)), ("SourceSelectionSha256", Q(d.SourceSelectionSha256)),
            ("ParentCheckpointSha256", Q(d.ParentCheckpointSha256)), ("CandidateSelectionSha256", Q(d.CandidateSelectionSha256)),
            ("CorpusDescriptorSha256", Q(d.CorpusDescriptorSha256)), ("EvaluationPlanSha256", Q(d.EvaluationPlanSha256)),
            ("CaseCount", d.CaseCount.ToString(CultureInfo.InvariantCulture)), ("RepeatCount", d.RepeatCount.ToString(CultureInfo.InvariantCulture)),
            ("EvaluatorProfileSha256", Q(d.EvaluatorProfileSha256)), ("ProfileSelectionSha256", Q(d.ProfileSelectionSha256)));
    private static string BindingJson(CodeDocumentationOneLessonSessionBinding b)
        => Obj(("ObjectiveId", Q(b.ObjectiveId)), ("GrantId", Q(b.GrantId)), ("GrantVersion", Q(b.GrantVersion)), ("TrainingRunId", Q(b.TrainingRunId)),
            ("CandidateSha256", Q(b.CandidateSha256)), ("ParentManifestSha256", Q(b.ParentManifestSha256)), ("ParentWeightsSha256", Q(b.ParentWeightsSha256)),
            ("ConfigurationSha256", Q(b.ConfigurationSha256)), ("ExamConditionSha256", Q(b.ExamConditionSha256)), ("DescriptorsV2", DescriptorJson(b.DescriptorsV2!)));
    private static string BindingHash(CodeDocumentationOneLessonSessionBinding b)
        => Digest(Utf8.GetBytes("code-documentation-one-lesson-binding-v2\n" + BindingJson(b)));
    private static byte[] Frame(CodeDocumentationOneLessonSessionBinding b, int revision = 0, string kind = "session", string previous = "", string? slot = null)
    {
        var fields = Dict(("Kind", Q(kind)), ("FormatVersion", "2"), ("SessionId", Q(Digest(Utf8.GetBytes(b.ObjectiveId)))),
            ("BindingV2", BindingJson(b)), ("BindingSha256", Q(BindingHash(b))), ("Revision", Q(revision.ToString(CultureInfo.InvariantCulture))),
            ("PreviousFrameSha256", Q(previous)), ("Effects", Obj(("BaselineDispatched", Q("0")), ("BaselineCompleted", Q("0")),
                ("TeachingDispatched", Q("0")), ("TeachingCompleted", Q("0")), ("EvaluatorDispatched", Q("0")), ("EvaluatorCompleted", Q("0")),
                ("OptimizerEntries", Q("0")), ("EffectOutcomeUnknown", "false"))));
        if (slot is not null) fields[kind] = slot;
        return Utf8.GetBytes(Obj(fields) + "\n");
    }
    private static OperationResult<CodeDocumentationOneLessonTrialSlotV2> Decode(string raw, bool started, CodeDocumentationOneLessonSessionBinding? binding)
    {
        using var document = JsonDocument.Parse(Utf8.GetBytes(raw), new JsonDocumentOptions { MaxDepth = 8 });
        return CodeDocumentationOneLessonTrialSlotV2.Decode(document.RootElement, started, binding);
    }
    private static void RejectMalformed(string raw, bool started, CodeDocumentationOneLessonSessionBinding? binding = null)
    {
        var bytes = Utf8.GetByteCount(raw); var cap = started ? 355 : 547;
        Reject(raw, started, binding ?? Binding(), bytes > cap ? Capacity : Recovery);
    }
    private static void Reject(string raw, bool started, CodeDocumentationOneLessonSessionBinding? binding, string expected)
    {
        var bytes = Utf8.GetByteCount(raw); using var parsed = JsonDocument.Parse(raw);
        if (parsed.RootElement.ValueKind == JsonValueKind.Object)
            Assert.Equal(expected == Capacity, bytes > (started ? 355 : 547));
        var result = CodeDocumentationOneLessonTrialSlotV2.Decode(parsed.RootElement, started, binding);
        AssertFailure(result, expected);
    }
    private static void AssertFailure<T>(OperationResult<T> result, string error)
    { Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(error, result.ErrorCode); }
    private static T Require<T>(OperationResult<T> result)
    { Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage); Assert.NotNull(result.Value); return result.Value!; }
    private static IEnumerable<string> BadHex() => new[] { "", new string('a', 63), new string('a', 65), H('A'), H('g'),
        "sha256:" + H('a'), " " + new string('a', 63), new string('a', 63) + " ", new string('é', 64), new string('ａ', 64) };
    private static string H(char c) => new(c, 64);
    private static string Digest(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static SortedDictionary<string, string> Dict(params (string Name, string Raw)[] pairs)
        => new(pairs.ToDictionary(p => p.Name, p => p.Raw), StringComparer.Ordinal);
    private static SortedDictionary<string, string> Copy(SortedDictionary<string, string> fields) => new(fields, StringComparer.Ordinal);
    private static string Obj(params (string Name, string Raw)[] pairs) => Obj(Dict(pairs));
    private static string Obj(SortedDictionary<string, string> fields) => "{" + string.Join(',', fields.Select(p => Q(p.Key) + ":" + p.Value)) + "}";
    private static string Duplicate(SortedDictionary<string, string> fields, string name) => "{" + Q(name) + ":" + fields[name] + "," + Obj(fields)[1..];
    private static string Q(string text)
    {
        var result = new StringBuilder("\"");
        foreach (var c in text)
            result.Append(c switch { '"' => "\\\"", '\\' => "\\\\", '\b' => "\\b", '\f' => "\\f", '\n' => "\\n", '\r' => "\\r", '\t' => "\\t",
                < ' ' => "\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture), _ => c.ToString() });
        return result.Append('"').ToString();
    }
    private sealed class Fixture : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "s04-trial-slot-" + Guid.NewGuid().ToString("N"));
        internal Fixture()
        {
            Repository = Path.Combine(_root, "repo");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning")); Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            Directory.CreateDirectory(Path.Combine(Repository, "training-artifacts", "models"));
            File.WriteAllText(Path.Combine(Repository, "LlmFromScratch.sln"), "synthetic test-owned root");
            ModelRoot = Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(Repository));
        }
        internal string Repository { get; }
        internal RepositoryOwnedModelArtifactRoot ModelRoot { get; }
        public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
    }
}
