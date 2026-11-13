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

public sealed class CodeDocumentationOneLessonTerminalOutcomeTests(ITestOutputHelper output)
{
    private const string Recovery = CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode;
    private const string Capacity = CodeDocumentationOneLessonSession.CapacityExceededErrorCode;
    private const string Negative = "TERMINAL_NEGATIVE_PARENT_RETAINED";
    private const string Promoted = "TERMINAL_PROMOTED";
    private static readonly UTF8Encoding Utf8 = new(false, true);

    public static TheoryData<string> Shapes => new() { "early-negative", "post-negative", "promoted" };

    // The schema oracle is literal and independent. Expected membership, JSON
    // spelling, identity hashes and frame preimages never call a product writer.
    private static string[] References(string shape) => shape switch
    {
        "early-negative" => ["QualityDecisionReceipt", "DispositionReceipt", "FinalReopenReceipt"],
        "post-negative" => ["QualityDecisionReceipt", "DispositionReceipt", "FinalReopenReceipt", "BaselineReceipt", "TrialEvaluationReceipt"],
        "promoted" => ["QualityDecisionReceipt", "DispositionReceipt", "FinalReopenReceipt", "BaselineReceipt", "TrialEvaluationReceipt",
            "PublicationStartedReceipt", "PublicationReceipt"],
        _ => throw new ArgumentOutOfRangeException(nameof(shape)),
    };

    [Theory, MemberData(nameof(Shapes))]
    public void ClosedVariantsPreserveLiteralMembershipBytesAndDescriptiveIdentity(string shape)
    {
        var b = Binding();
        var json = Terminal(shape, b);
        var decoded = Require(Decode(json, b));
        var common = new[] { "OutcomeId", "SchemaVersion", "ObjectiveId", "SessionId", "TrainingRunId", "BindingSha256", "TerminalState" };
        Assert.Equal(common.Concat(References(shape)).Order(StringComparer.Ordinal), decoded.Value.EnumerateObject().Select(p => p.Name));
        Assert.Equal(shape == "early-negative" ? 10 : shape == "post-negative" ? 12 : 14, decoded.Value.EnumerateObject().Count());
        Assert.Equal(Utf8.GetBytes(json), decoded.CanonicalBytes);
        Assert.Equal(json, decoded.Value.GetRawText());
        Assert.Equal(new string('a', 32), decoded.OutcomeId);
        Assert.Equal(shape == "promoted" ? Promoted : Negative, decoded.TerminalState);
        Assert.Equal(Digest(Utf8.GetBytes(b.ObjectiveId)), decoded.Value.GetProperty("SessionId").GetString());
        Assert.Equal(BindingHash(b), decoded.Value.GetProperty("BindingSha256").GetString());
        foreach (var name in References(shape))
        {
            Assert.Equal(new[] { "ReceiptId", "ReceiptSha256" }, decoded.Value.GetProperty(name).EnumerateObject().Select(p => p.Name));
            Assert.Equal(new string('b', 32), decoded.Value.GetProperty(name).GetProperty("ReceiptId").GetString());
            Assert.Equal(H('c'), decoded.Value.GetProperty(name).GetProperty("ReceiptSha256").GetString());
        }
    }

    [Theory, MemberData(nameof(Shapes))]
    public void EachRequiredCommonAndReferenceMemberRejectsMissingNullWrongTypeAndRestores(string shape)
    {
        var b = Binding();
        var fields = TerminalFields(shape, b);
        foreach (var name in fields.Keys)
        {
            var missing = Copy(fields); missing.Remove(name); Reject(Obj(missing), b);
            var nil = Copy(fields); nil[name] = "null"; Reject(Obj(nil), b);
            foreach (var raw in new[] { "false", "[]", "0" })
            { var bad = Copy(fields); bad[name] = raw; Reject(Obj(bad), b); }
            Require(Decode(Obj(fields), b));
        }
    }

    [Theory, MemberData(nameof(Shapes))]
    public void EachObjectLevelRejectsDuplicateAliasUnknownAndNonObject(string shape)
    {
        var b = Binding(); var fields = TerminalFields(shape, b);
        foreach (var name in fields.Keys)
        {
            Reject(Duplicate(fields, name), b);
            var alias = Copy(fields); alias.Remove(name); alias[char.ToLowerInvariant(name[0]) + name[1..]] = fields[name];
            Reject(Obj(alias), b);
        }
        foreach (var name in new[] { "Unknown", "ExternalBaselineRef", "PreparedBaselineReceipt", "ReceiptType", "Payload", "ReceiptSha256" })
        { var bad = Copy(fields); bad[name] = Q("forbidden"); Reject(Obj(bad), b); }
        foreach (var raw in new[] { "[]", "\"outcome\"", "0", "true", "null" }) Reject(raw, b);
        Require(Decode(Obj(fields), b));
    }

    [Theory, MemberData(nameof(Shapes))]
    public void EveryReferenceHasTwoExactNonNullHexStringsWithoutAliases(string shape)
    {
        var b = Binding(); var fields = TerminalFields(shape, b);
        Assert.NotEmpty(References(shape));
        foreach (var name in References(shape))
        {
            var reference = RefFields();
            foreach (var field in new[] { "ReceiptId", "ReceiptSha256" })
            {
                var missing = Copy(reference); missing.Remove(field); Fault(Obj(missing));
                var nil = Copy(reference); nil[field] = "null"; Fault(Obj(nil));
                foreach (var raw in new[] { "false", "32", "[]", "{}" })
                { var wrong = Copy(reference); wrong[field] = raw; Fault(Obj(wrong)); }
                Fault(Duplicate(reference, field));
                var alias = Copy(reference); alias.Remove(field); alias[char.ToLowerInvariant(field[0]) + field[1..]] = reference[field]; Fault(Obj(alias));
                foreach (var text in BadHex(field == "ReceiptId" ? 32 : 64))
                { var bad = Copy(reference); bad[field] = Q(text); Fault(Obj(bad)); }
            }
            var extra = Copy(reference); extra["Unknown"] = "true"; Fault(Obj(extra));
            foreach (var raw in new[] { "[]", "\"reference\"", "0", "true", "null" }) Fault(raw);
            Require(Decode(Obj(fields), b));
            void Fault(string raw) { var changed = Copy(fields); changed[name] = raw; Reject(Obj(changed), b); }
        }
    }

    [Theory, MemberData(nameof(Shapes))]
    public void CommonIdsAndHashesRejectWidthsAlphabetCaseTrimAndUnicode(string shape)
    {
        var b = Binding(); var fields = TerminalFields(shape, b);
        foreach (var name in new[] { "OutcomeId", "SessionId", "BindingSha256" })
            foreach (var value in BadHex(name == "OutcomeId" ? 32 : 64))
            { var bad = Copy(fields); bad[name] = Q(value); Reject(Obj(bad), b); }
        foreach (var name in new[] { "ObjectiveId", "TrainingRunId", "SessionId", "BindingSha256" })
        { var bad = Copy(fields); bad[name] = Q(name is "SessionId" or "BindingSha256" ? H('e') : "other"); Reject(Obj(bad), b); }
        Require(Decode(Obj(fields), b));
    }

    [Fact]
    public void DisjointFieldSetsRequireTheWholeTrialPairAndPromotedPublicationPair()
    {
        var b = Binding(); var early = TerminalFields("early-negative", b); var post = TerminalFields("post-negative", b);
        foreach (var name in new[] { "BaselineReceipt", "TrialEvaluationReceipt" })
        {
            var extra = Copy(early); extra[name] = Reference(); Reject(Obj(extra), b);
            var missing = Copy(post); missing.Remove(name); Reject(Obj(missing), b);
        }
        var both = Copy(early); both["BaselineReceipt"] = Reference(); both["TrialEvaluationReceipt"] = Reference();
        Assert.Equal(Terminal("post-negative", b), Require(Decode(Obj(both), b)).Value.GetRawText());
        both.Remove("BaselineReceipt"); both.Remove("TrialEvaluationReceipt"); Require(Decode(Obj(both), b));
        foreach (var shape in new[] { "early-negative", "post-negative" })
        {
            foreach (var name in new[] { "PublicationStartedReceipt", "PublicationReceipt" })
            { var extra = TerminalFields(shape, b); extra[name] = Reference(); Reject(Obj(extra), b); }
            var extraBoth = TerminalFields(shape, b); extraBoth["PublicationStartedReceipt"] = Reference(); extraBoth["PublicationReceipt"] = Reference();
            Reject(Obj(extraBoth), b);
            var wrongState = TerminalFields(shape, b); wrongState["TerminalState"] = Q(Promoted); Reject(Obj(wrongState), b);
            Require(Decode(Terminal(shape, b), b));
        }
        var promoted = TerminalFields("promoted", b);
        foreach (var name in References("promoted"))
        { var missing = Copy(promoted); missing.Remove(name); Reject(Obj(missing), b); }
        var negativeWithPublication = Copy(promoted); negativeWithPublication["TerminalState"] = Q(Negative); Reject(Obj(negativeWithPublication), b);
        negativeWithPublication.Remove("PublicationStartedReceipt"); negativeWithPublication.Remove("PublicationReceipt");
        Require(Decode(Obj(negativeWithPublication), b));
        Require(Decode(Obj(promoted), b));
    }

    [Theory, MemberData(nameof(Shapes))]
    public void StateSchemaAndRawCanonicalSpellingAreClosed(string shape)
    {
        var b = Binding(); var fields = TerminalFields(shape, b); var original = Obj(fields);
        var state = shape == "promoted" ? Promoted : Negative;
        foreach (var value in new[] { state.ToLowerInvariant(), " " + state, state + " ", state + "x", "", "Unknown", "TerminalPromoted" })
        { var bad = Copy(fields); bad["TerminalState"] = Q(value); Reject(Obj(bad), b); }
        foreach (var raw in new[] { "2.0", "2e0", "2E+0", "-0", "0", "1", "3", "true", "\"2\"", "9223372036854775808" })
        { var bad = Copy(fields); bad["SchemaVersion"] = raw; Reject(Obj(bad), b); }
        Reject("{ " + original[1..], b);
        Reject(original[..^1] + " }", b);
        Reject("{" + string.Join(',', fields.Reverse().Select(p => Q(p.Key) + ":" + p.Value)) + "}", b);
        foreach (var name in fields.Keys.Concat(new[] { "ReceiptId", "ReceiptSha256" }))
            Reject(original.Replace(Q(name) + ":", "\"\\u" + ((int)name[0]).ToString("x4", CultureInfo.InvariantCulture) + name[1..] + "\":", StringComparison.Ordinal), b);
        Reject(original.Replace(Q(state), "\"\\u0054" + state[1..] + "\"", StringComparison.Ordinal), b);
        Reject(original.Replace("\"aaaaaaaa", "\"\\u0061aaaaaaa", StringComparison.Ordinal), b);
        Reject(original.Replace(Q(b.ObjectiveId), "\"\\uD800\"", StringComparison.Ordinal), b);
        Require(Decode(original, b));
    }

    [Fact]
    public void ExpectedBindingSyntaxAndCountsAreCheckedEvenWhenTerminalRepeatsThem()
    {
        var b = Binding(); var original = Terminal("early-negative", b);
        Assert.Equal(Recovery, Decode(original, null).ErrorCode);
        var malformed = new[]
        {
            b with { FormatVersion = 1 }, b with { FormatVersion = 3 }, b with { DescriptorsV2 = null },
            b with { ObjectiveId = "" }, b with { ObjectiveId = " objective" }, b with { ObjectiveId = "objective " },
            b with { ObjectiveId = "a\nb" }, b with { ObjectiveId = new string('x', 129) },
            b with { ObjectiveId = new string('\u0800', 127) + "😀" },
            b with { GrantId = "" }, b with { GrantId = " g" }, b with { GrantId = "g " }, b with { GrantId = "a\tb" },
            b with { GrantId = new string('x', 129) }, b with { GrantId = new string('é', 65) },
            b with { GrantVersion = "" }, b with { GrantVersion = " v" }, b with { GrantVersion = "v " },
            b with { GrantVersion = "a\rb" }, b with { GrantVersion = new string('x', 65) }, b with { GrantVersion = new string('é', 33) },
            b with { TrainingRunId = "" }, b with { TrainingRunId = "a/b" }, b with { TrainingRunId = "é" },
            b with { TrainingRunId = " run" }, b with { TrainingRunId = new string('x', 129) },
            b with { CandidateSha256 = H('A') }, b with { ParentManifestSha256 = H('g') }, b with { ParentWeightsSha256 = "" },
            b with { ConfigurationSha256 = H('a') + "0" }, b with { ExamConditionSha256 = " " + H('a') },
            b with { DescriptorsV2 = b.DescriptorsV2! with { SchemaVersion = 1 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { SourceSelectionSha256 = "bad" } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { ParentCheckpointSha256 = "bad" } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { CandidateSelectionSha256 = "bad" } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { CorpusDescriptorSha256 = "bad" } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { EvaluationPlanSha256 = "bad" } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { EvaluatorProfileSha256 = "bad" } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { ProfileSelectionSha256 = "bad" } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { CaseCount = 0 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { CaseCount = 201 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { RepeatCount = 1 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { RepeatCount = 5001 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { CaseCount = 200, RepeatCount = 26 } },
        };
        Assert.NotEmpty(malformed);
        foreach (var invalid in malformed)
        {
            // Repeated fields and the independent digest are rebuilt from this
            // input; rejection cannot be satisfied by an old binding hash alone.
            Reject(Terminal("early-negative", invalid), invalid);
            Require(Decode(original, b));
        }
        foreach (var invalid in new[] { b with { ObjectiveId = "\ud800" }, b with { GrantId = "\ud800" }, b with { GrantVersion = "\ud800" } })
            Reject(original, invalid);
        Reject(original, b with { ObjectiveId = null! });
        Reject(original, b with { TrainingRunId = null! });
        foreach (var changed in new[] { b with { ObjectiveId = "other" }, b with { TrainingRunId = "other" },
            b with { CandidateSha256 = H('b') }, b with { GrantId = "other" } }) Reject(original, changed);
        Require(Decode(original, b));
    }

    [Fact]
    public void InheritedIdentityIntersectionAndPunctuationAcceptTheirActualBoundaries()
    {
        var b = Binding();
        foreach (var run in new[] { ".", "_", ":", "-", "-._:AZaz09", new string('a', 128) }) Accept(b with { TrainingRunId = run });
        foreach (var objective in new[] { "a", "\u0800", "😀", "\"\\", new string('\u0800', 128), new string('a', 128), string.Concat(Enumerable.Repeat("😀", 64)) })
            Accept(b with { ObjectiveId = objective });
        foreach (var grant in new[] { "g", new string('"', 128), new string('é', 64) }) Accept(b with { GrantId = grant });
        foreach (var version in new[] { "v", new string('"', 64), new string('é', 32) }) Accept(b with { GrantVersion = version });
        foreach (var d in new[] { b.DescriptorsV2! with { CaseCount = 200, RepeatCount = 25 }, b.DescriptorsV2! with { CaseCount = 1, RepeatCount = 5000 } })
            Accept(b with { DescriptorsV2 = d });
        void Accept(CodeDocumentationOneLessonSessionBinding binding)
        {
            var decoded = Require(Decode(Terminal("early-negative", binding), binding));
            Assert.Equal(binding.ObjectiveId, decoded.Value.GetProperty("ObjectiveId").GetString());
            Assert.Equal(binding.TrainingRunId, decoded.Value.GetProperty("TrainingRunId").GetString());
            Assert.Equal(BindingHash(binding), decoded.Value.GetProperty("BindingSha256").GetString());
        }
    }

    [Theory, MemberData(nameof(Shapes))]
    public void SnapshotOwnsDisposedDocumentAndCallerAndGetterBytes(string shape)
    {
        var b = Binding(); var original = Terminal(shape, b); var source = Utf8.GetBytes(original);
        CodeDocumentationOneLessonTerminalOutcomeV2 decoded;
        using (var document = JsonDocument.Parse(source))
            decoded = Require(CodeDocumentationOneLessonTerminalOutcomeV2.Decode(document.RootElement, b));
        Array.Fill(source, (byte)'x');
        var exposed = decoded.CanonicalBytes; Array.Fill(exposed, (byte)'x');
        Assert.Equal(Utf8.GetBytes(original), decoded.CanonicalBytes);
        Assert.Equal(original, decoded.Value.GetRawText());
        var changed = TerminalFields(shape, b); var reference = RefFields(); reference["ReceiptSha256"] = Q(H('d'));
        changed["FinalReopenReceipt"] = Obj(reference);
        var other = Require(Decode(Obj(changed), b));
        Assert.Equal(decoded.CanonicalBytes.Length, other.CanonicalBytes.Length);
        Assert.False(decoded.CanonicalBytes.AsSpan().SequenceEqual(other.CanonicalBytes));
        Assert.Equal(H('c'), decoded.Value.GetProperty("FinalReopenReceipt").GetProperty("ReceiptSha256").GetString());
        Assert.Equal(H('d'), other.Value.GetProperty("FinalReopenReceipt").GetProperty("ReceiptSha256").GetString());
        var outcomeChange = TerminalFields(shape, b); outcomeChange["OutcomeId"] = Q(new string('f', 32));
        Assert.Equal(new string('f', 32), Require(Decode(Obj(outcomeChange), b)).OutcomeId);
        Assert.Equal(Recovery, CodeDocumentationOneLessonTerminalOutcomeV2.Decode(default, b).ErrorCode);
        using var disposed = JsonDocument.Parse(original); var value = disposed.RootElement; disposed.Dispose();
        var rejected = CodeDocumentationOneLessonTerminalOutcomeV2.Decode(value, b);
        Assert.False(rejected.IsSuccess); Assert.Null(rejected.Value); Assert.Equal(Recovery, rejected.ErrorCode);
    }

    [Theory, MemberData(nameof(Shapes))]
    public void LiteralMaximumDimensionsAreClosedBelowConservativeObjectAndFrameBounds(string shape)
    {
        var b = WideBinding(); var json = Terminal(shape, b); var genesis = Frame(b);
        Assert.Equal(shape == "early-negative" ? 1291 : shape == "post-negative" ? 1598 : 1895, Utf8.GetByteCount(json));
        Assert.Equal(Utf8.GetBytes(json), Require(Decode(json, b)).CanonicalBytes);
        Assert.Equal(695, Utf8.GetByteCount(DescriptorJson(b.DescriptorsV2!)));
        Assert.Equal(2118, Utf8.GetByteCount(BindingJson(b)));
        Assert.Equal(2583, genesis.Length);
        var frame = Frame(b, 1, "terminal", Digest(genesis[..^1]), json);
        Assert.Equal(shape == "early-negative" ? 3958 : shape == "post-negative" ? 4265 : 4562, frame.Length);
        Assert.True(frame.Length <= 4865);
        output.WriteLine("STATIC_DIMENSION shape={0} object={1} descriptor=695 binding=2118 genesis={2} frame={3}; independent byte arithmetic, no owner/chain proof", shape, Utf8.GetByteCount(json), genesis.Length, frame.Length);
    }

    [Fact]
    public void FixedRawObjectCapPrecedesStateSchemaMembersAndExpectedBinding()
    {
        var b = Binding(); var fields = TerminalFields("early-negative", b);
        var exact = CapControl(fields, 2053); var over = CapControl(fields, 2054);
        Assert.Equal(2053, Utf8.GetByteCount(exact)); Assert.Equal(2054, Utf8.GetByteCount(over));
        Reject(exact, b); Reject(over, b, Capacity);
        foreach (var fault in new[] { "state", "schema", "missing", "binding" })
        {
            var changed = Copy(fields);
            if (fault == "state") changed["TerminalState"] = Q("Unknown");
            if (fault == "schema") changed["SchemaVersion"] = "2.0";
            if (fault == "missing") changed.Remove("OutcomeId");
            Reject(CapControl(changed, 2053), fault == "binding" ? null : b);
            Reject(CapControl(changed, 2054), fault == "binding" ? null : b, Capacity);
            Require(Decode(Terminal("early-negative", b), b));
        }
        // These extra-string objects are malformed controls, not valid maxima.
        Require(Decode(Terminal("early-negative", b), b));
    }

    [Theory, MemberData(nameof(Shapes))]
    public void ActualInternalAndPublicOpenRefuseAllClosedTerminalsWithoutEffectsOrByteChanges(string shape)
    {
        using var fixture = new Fixture(); var b = WideBinding(); var (path, genesis) = Seed(fixture, b);
        var terminal = Terminal(shape, b); Require(Decode(terminal, b));
        var bytes = genesis.Concat(Frame(b, 1, "terminal", Digest(genesis[..^1]), terminal)).ToArray();
        AssertRefusalAndRestore(fixture, b, path, genesis, bytes, Recovery);
    }

    [Theory]
    [InlineData(2053, Recovery, 4720)] [InlineData(2054, Capacity, 4721)]
    public void ActualReaderEnforcesNestedObjectBoundInsideItsFullLineBound(int objectBytes, string expectedError, int lineBytes)
    {
        using var fixture = new Fixture(); var b = WideBinding(); var (path, genesis) = Seed(fixture, b);
        var json = CapControl(TerminalFields("promoted", b), objectBytes);
        Assert.Equal(objectBytes, Utf8.GetByteCount(json)); Reject(json, b, expectedError);
        var frame = Frame(b, 1, "terminal", Digest(genesis[..^1]), json);
        Assert.Equal(lineBytes, frame.Length); Assert.True(frame.Length <= 4865);
        AssertRefusalAndRestore(fixture, b, path, genesis, genesis.Concat(frame).ToArray(), expectedError);
    }

    [Theory]
    [InlineData("comment")] [InlineData("trailing-comma")] [InlineData("torn-object")]
    [InlineData("bom")] [InlineData("garbage")] [InlineData("invalid-utf8")]
    public void MalformedRawJsonAndUtf8AreRejectedByActualSessionParser(string fault)
    {
        using var fixture = new Fixture(); var b = Binding(); var (path, genesis) = Seed(fixture, b);
        var terminal = Terminal("early-negative", b); Require(Decode(terminal, b));
        var bad = fault switch
        {
            "comment" => terminal.Insert(1, "/*comment*/"),
            "trailing-comma" => terminal[..^1] + ",}",
            "torn-object" => terminal[..^1],
            "bom" => "\ufeff" + terminal,
            "garbage" => terminal + "garbage",
            "invalid-utf8" => terminal,
            _ => throw new ArgumentOutOfRangeException(nameof(fault)),
        };
        var frame = Frame(b, 1, "terminal", Digest(genesis[..^1]), bad);
        if (fault == "invalid-utf8")
        {
            var marker = Utf8.GetBytes("\"OutcomeId\":\""); var start = frame.AsSpan().IndexOf(marker);
            Assert.True(start >= 0); frame[start + marker.Length] = 0xff;
        }
        Assert.True(frame.Length <= 4865);
        AssertRefusalAndRestore(fixture, b, path, genesis, genesis.Concat(frame).ToArray(), Recovery);
    }

    [Fact]
    public void ChangedHeldPrefixRefusalIsCustodyEvidenceOnlyThenRestoresOwnedGenesis()
    {
        using var fixture = new Fixture(); var b = WideBinding(); var (path, genesis) = Seed(fixture, b);
        var lockPath = Path.Combine(Path.GetDirectoryName(path)!, ".owner.lock"); var lockBytes = File.ReadAllBytes(lockPath);
        using var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, b, 10000));
        var malformed = CapControl(TerminalFields("promoted", b), 2054);
        var bytes = genesis.Concat(Frame(b, 1, "terminal", Digest(genesis[..^1]), malformed)).ToArray(); File.WriteAllBytes(path, bytes);
        var refused = owner.Session.ReadCapacityPrefixV2();
        Assert.False(refused.IsSuccess); Assert.Null(refused.Value); Assert.Equal(Recovery, refused.ErrorCode);
        Assert.Equal(bytes, File.ReadAllBytes(path));
        File.WriteAllBytes(path, genesis);
        Assert.Equal(1, Require(owner.Session.ReadCapacityPrefixV2()).PrefixFrames);
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), owner.Session.Snapshot.Effects);
        owner.Dispose(); // Release exclusive custody before external lock byte readback.
        Assert.Equal(genesis, File.ReadAllBytes(path)); Assert.Equal(lockBytes, File.ReadAllBytes(lockPath));
        AssertOnlyExpectedFiles(path);
    }

    [Fact]
    public void RuntimeIdentityBindsNewTerminalDecoderAndTestToLoadedNormalArtifacts()
    {
        var product = typeof(CodeDocumentationOneLessonTerminalOutcomeV2).Assembly;
        var tests = typeof(CodeDocumentationOneLessonTerminalOutcomeTests).Assembly;
        Assert.Equal("TrainableRouter", product.GetName().Name);
        Assert.Equal("LlmFromScratch.Tests", tests.GetName().Name);
        var directory = Path.GetDirectoryName(tests.Location)!;
        Assert.Equal(Path.Combine(directory, "TrainableRouter.dll"), product.Location);
        foreach (var path in new[] { product.Location, tests.Location, Path.Combine(directory, "LlmFromScratch.Tests.runtimeconfig.json"),
            Path.Combine(directory, "LlmFromScratch.Tests.deps.json") })
        {
            var bytes = File.ReadAllBytes(path); Assert.NotEmpty(bytes);
            output.WriteLine("TERMINAL_RUNTIME_IDENTITY path={0} bytes={1} sha256={2}", path, bytes.Length, Digest(bytes));
        }
        Assert.Equal(Negative, Require(Decode(Terminal("early-negative", Binding()), Binding())).TerminalState);
    }

    private static void AssertRefusalAndRestore(Fixture fixture, CodeDocumentationOneLessonSessionBinding b,
        string path, byte[] genesis, byte[] bytes, string expectedError)
    {
        var lockPath = Path.Combine(Path.GetDirectoryName(path)!, ".owner.lock"); var lockBytes = File.ReadAllBytes(lockPath);
        File.WriteAllBytes(path, bytes);
        var internalResult = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, b, 10000);
        Assert.False(internalResult.IsSuccess); Assert.Null(internalResult.Value); Assert.Equal(expectedError, internalResult.ErrorCode);
        var publicResult = CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot, b, 10000);
        Assert.False(publicResult.IsSuccess); Assert.Null(publicResult.Value); Assert.Equal(expectedError, publicResult.ErrorCode);
        Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.Equal(lockBytes, File.ReadAllBytes(lockPath)); AssertOnlyExpectedFiles(path);
        File.WriteAllBytes(path, genesis);
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot, b, 10000));
        var capacity = Require(restored.Session.ReadCapacityPrefixV2());
        Assert.Equal(1, capacity.PrefixFrames); Assert.Equal(genesis.Length, capacity.PrefixBytes);
        Assert.Equal(0, restored.Session.Snapshot.Revision);
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), restored.Session.Snapshot.Effects);
        restored.Dispose(); // Owner lock bytes are externally read only after release.
        Assert.Equal(genesis, File.ReadAllBytes(path)); Assert.Equal(lockBytes, File.ReadAllBytes(lockPath)); AssertOnlyExpectedFiles(path);
    }

    private static void AssertOnlyExpectedFiles(string path)
        => Assert.Equal(new[] { ".owner.lock", "session.jsonl" }, Directory.GetFileSystemEntries(Path.GetDirectoryName(path)!)
            .Select(Path.GetFileName).Order(StringComparer.Ordinal));

    private static CodeDocumentationOneLessonSessionBinding Binding() => new("objective", "g", "v", "-run", H('1'), H('2'), H('3'), H('4'), H('5'))
        { FormatVersion = 2, DescriptorsV2 = new(2, H('a'), H('b'), H('c'), H('d'), H('e'), 1, 2, H('f'), H('0')) };
    private static CodeDocumentationOneLessonSessionBinding WideBinding() => Binding() with
    {
        ObjectiveId = new string('\u0800', 128), TrainingRunId = new string('a', 128),
        GrantId = new string('"', 128), GrantVersion = new string('"', 64),
        DescriptorsV2 = Binding().DescriptorsV2! with { RepeatCount = 5000 },
    };

    private static SortedDictionary<string, string> TerminalFields(string shape, CodeDocumentationOneLessonSessionBinding b)
    {
        var fields = Dict(("OutcomeId", Q(new string('a', 32))), ("SchemaVersion", "2"), ("ObjectiveId", Q(b.ObjectiveId)),
            ("SessionId", Q(Digest(Utf8.GetBytes(b.ObjectiveId)))), ("TrainingRunId", Q(b.TrainingRunId)), ("BindingSha256", Q(BindingHash(b))),
            ("TerminalState", Q(shape == "promoted" ? Promoted : Negative)));
        foreach (var name in References(shape)) fields[name] = Reference();
        return fields;
    }
    private static string Terminal(string shape, CodeDocumentationOneLessonSessionBinding b) => Obj(TerminalFields(shape, b));
    private static SortedDictionary<string, string> RefFields() => Dict(("ReceiptId", Q(new string('b', 32))), ("ReceiptSha256", Q(H('c'))));
    private static string Reference() => Obj(RefFields());

    private static string DescriptorJson(CodeDocumentationOneLessonDescriptorsV2 d)
        => Obj(("SchemaVersion", d.SchemaVersion.ToString(CultureInfo.InvariantCulture)), ("SourceSelectionSha256", Q(d.SourceSelectionSha256)),
            ("ParentCheckpointSha256", Q(d.ParentCheckpointSha256)), ("CandidateSelectionSha256", Q(d.CandidateSelectionSha256)),
            ("CorpusDescriptorSha256", Q(d.CorpusDescriptorSha256)), ("EvaluationPlanSha256", Q(d.EvaluationPlanSha256)),
            ("CaseCount", d.CaseCount.ToString(CultureInfo.InvariantCulture)), ("RepeatCount", d.RepeatCount.ToString(CultureInfo.InvariantCulture)),
            ("EvaluatorProfileSha256", Q(d.EvaluatorProfileSha256)), ("ProfileSelectionSha256", Q(d.ProfileSelectionSha256)));
    private static string BindingJson(CodeDocumentationOneLessonSessionBinding b)
        => Obj(("ObjectiveId", Q(b.ObjectiveId)), ("GrantId", Q(b.GrantId)), ("GrantVersion", Q(b.GrantVersion)), ("TrainingRunId", Q(b.TrainingRunId)),
            ("CandidateSha256", Q(b.CandidateSha256)), ("ParentManifestSha256", Q(b.ParentManifestSha256)), ("ParentWeightsSha256", Q(b.ParentWeightsSha256)),
            ("ConfigurationSha256", Q(b.ConfigurationSha256)), ("ExamConditionSha256", Q(b.ExamConditionSha256)),
            ("DescriptorsV2", b.DescriptorsV2 is null ? "null" : DescriptorJson(b.DescriptorsV2)));
    private static string BindingHash(CodeDocumentationOneLessonSessionBinding b)
        => Digest(Utf8.GetBytes("code-documentation-one-lesson-binding-v2\n" + BindingJson(b)));

    private static byte[] Frame(CodeDocumentationOneLessonSessionBinding b, int revision = 0, string kind = "session", string previous = "", string? terminal = null)
    {
        var fields = Dict(("Kind", Q(kind)), ("FormatVersion", "2"), ("SessionId", Q(Digest(Utf8.GetBytes(b.ObjectiveId)))), ("BindingV2", BindingJson(b)),
            ("BindingSha256", Q(BindingHash(b))), ("Revision", Q(revision.ToString(CultureInfo.InvariantCulture))), ("PreviousFrameSha256", Q(previous)),
            ("Effects", Obj(("BaselineDispatched", Q("0")), ("BaselineCompleted", Q("0")), ("TeachingDispatched", Q("0")), ("TeachingCompleted", Q("0")),
                ("EvaluatorDispatched", Q("0")), ("EvaluatorCompleted", Q("0")), ("OptimizerEntries", Q("0")), ("EffectOutcomeUnknown", "false"))));
        if (terminal is not null) fields["TerminalOutcome"] = terminal;
        return Utf8.GetBytes(Obj(fields) + "\n");
    }

    private static string CapControl(SortedDictionary<string, string> original, int bytes)
    {
        var fields = Copy(original); fields["Unknown"] = Q("");
        var baseBytes = Utf8.GetByteCount(Obj(fields)); Assert.True(baseBytes <= bytes);
        fields["Unknown"] = Q(new string('x', bytes - baseBytes));
        return Obj(fields);
    }

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
        return (path, genesis);
    }

    private static OperationResult<CodeDocumentationOneLessonTerminalOutcomeV2> Decode(string json, CodeDocumentationOneLessonSessionBinding? binding)
    {
        using var document = JsonDocument.Parse(Utf8.GetBytes(json), new JsonDocumentOptions { MaxDepth = 8 });
        return CodeDocumentationOneLessonTerminalOutcomeV2.Decode(document.RootElement, binding);
    }
    private static void Reject(string json, CodeDocumentationOneLessonSessionBinding? binding, string error = Recovery)
    {
        // Every structural mutation must parse and call the actual decoder;
        // parser failures cannot masquerade as evidence for a schema assertion.
        var result = Decode(json, binding);
        Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(error, result.ErrorCode);
    }
    private static T Require<T>(OperationResult<T> result)
    { Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage); Assert.NotNull(result.Value); return result.Value!; }
    private static string H(char c) => new(c, 64);
    private static string Digest(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static IEnumerable<string> BadHex(int width) => new[] { "", new string('a', width - 1), new string('a', width + 1), new string('A', width),
        new string('g', width), " " + new string('a', width - 1), new string('a', width - 1) + " ", new string('é', width), new string('ａ', width), new string('a', width - 1) + "\n" };
    private static SortedDictionary<string, string> Dict(params (string Name, string Raw)[] pairs)
        => new(pairs.ToDictionary(p => p.Name, p => p.Raw), StringComparer.Ordinal);
    private static SortedDictionary<string, string> Copy(SortedDictionary<string, string> source) => new(source, StringComparer.Ordinal);
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
        private readonly string _root = Path.Combine(Path.GetTempPath(), "s04-terminal-outcome-" + Guid.NewGuid().ToString("N"));
        internal Fixture()
        {
            Repository = Path.Combine(_root, "repo");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            Directory.CreateDirectory(Path.Combine(Repository, "training-artifacts", "models"));
            File.WriteAllText(Path.Combine(Repository, "LlmFromScratch.sln"), "synthetic test-owned root");
            ModelRoot = Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(Repository));
        }
        internal string Repository { get; }
        internal RepositoryOwnedModelArtifactRoot ModelRoot { get; }
        public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
    }
}
