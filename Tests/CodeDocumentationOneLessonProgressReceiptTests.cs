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

public sealed class CodeDocumentationOneLessonProgressReceiptTests(ITestOutputHelper output)
{
    private const string Recovery = CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode;
    private const string Capacity = CodeDocumentationOneLessonSession.CapacityExceededErrorCode;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    // Independent wire oracle: literal field sets and raw JSON values. Neither
    // expected schema, byte spelling nor digest uses the production writer.
    public static TheoryData<string> Shapes => new()
    {
        "SealedPair", "RunnerCompletion", "Baseline", "VerifiedAppliedUpdate", "TrialEvaluation",
        "QualityDecision:BaselineSufficient", "QualityDecision:Improved", "QualityDecision:NotImproved",
        "Disposition", "PublicationStarted", "PublicationReceipt", "FinalReopen",
    };

    public static TheoryData<string> ReferenceShapes => new()
    {
        "RunnerCompletion", "Baseline", "QualityDecision:BaselineSufficient",
        "QualityDecision:Improved", "QualityDecision:NotImproved", "Disposition",
        "PublicationStarted", "PublicationReceipt", "FinalReopen",
    };

    [Theory, MemberData(nameof(Shapes))]
    public void EveryClosedShapePreservesCompletePayloadAndIndependentObjectHash(string shape)
    {
        var b = Binding();
        var json = Receipt(shape, b);
        var decoded = Require(Decode(json, b));
        Assert.Equal(Utf8.GetBytes(json), decoded.CanonicalBytes);
        Assert.Equal(Digest(Utf8.GetBytes(json)), decoded.ReceiptSha256);
        Assert.Equal(json, decoded.Value.GetRawText());
        Assert.Equal(Payload(shape).Keys.Order(StringComparer.Ordinal),
            decoded.Payload.EnumerateObject().Select(p => p.Name));
        Assert.Equal(Type(shape), decoded.ReceiptType);
        Assert.Equal(new string('a', 32), decoded.ReceiptId);
        Assert.NotEqual(Digest(Utf8.GetBytes(json + "\n")), decoded.ReceiptSha256);
        Assert.NotEqual(Digest(Utf8.GetBytes(Obj(("ProgressReceipt", json)))), decoded.ReceiptSha256);
        Assert.NotEqual(Digest(Utf8.GetBytes(Reference())), decoded.ReceiptSha256);
    }

    [Theory, MemberData(nameof(Shapes))]
    public void EveryRequiredCommonAndPayloadMemberRejectsMissingNullWrongTypeAndRestores(string shape)
    {
        var b = Binding();
        var common = ReceiptFields(shape, b);
        foreach (var name in common.Keys)
        {
            var missing = Copy(common); missing.Remove(name);
            Reject(Obj(missing), b);
            var nil = Copy(common); nil[name] = "null"; Reject(Obj(nil), b);
            var wrong = Copy(common); wrong[name] = name == "SchemaVersion" ? "\"2\"" : "false";
            Reject(Obj(wrong), b);
            Require(Decode(Obj(common), b));
        }
        var payload = Payload(shape);
        foreach (var name in payload.Keys)
        {
            var missing = Copy(payload); missing.Remove(name);
            Reject(Receipt(shape, b, Obj(missing)), b);
            var nil = Copy(payload); nil[name] = "null";
            Reject(Receipt(shape, b, Obj(nil)), b);
            var wrong = Copy(payload); wrong[name] = "false";
            Reject(Receipt(shape, b, Obj(wrong)), b);
            Require(Decode(Receipt(shape, b), b));
        }
    }

    [Theory, MemberData(nameof(Shapes))]
    public void EveryObjectLevelRejectsDuplicatesCaseAliasesUnknownAndNonObjects(string shape)
    {
        var b = Binding(); var original = Receipt(shape, b); var fields = ReceiptFields(shape, b);
        foreach (var name in fields.Keys)
        {
            // Duplicating a full large payload can exceed the raw object cap
            // first. A compact first object keeps this duplicate-member test
            // below that cap; the second payload is the complete valid control.
            Reject(Duplicate(fields, name, name == "Payload" ? "{}" : null), b);
            var alias = Copy(fields); alias.Remove(name); alias[char.ToLowerInvariant(name[0]) + name[1..]] = fields[name];
            Reject(Obj(alias), b);
        }
        var unknown = Copy(fields); unknown["Unknown"] = "true"; Reject(Obj(unknown), b);
        var p = Payload(shape);
        foreach (var name in p.Keys)
        {
            Reject(Receipt(shape, b, Duplicate(p, name)), b);
            var alias = Copy(p); alias.Remove(name); alias[char.ToLowerInvariant(name[0]) + name[1..]] = p[name];
            Reject(Receipt(shape, b, Obj(alias)), b);
        }
        var extra = Copy(p); extra["Unknown"] = "true"; Reject(Receipt(shape, b, Obj(extra)), b);
        foreach (var scalar in new[] { "[]", "\"payload\"", "0", "true", "null" }) Reject(Receipt(shape, b, scalar), b);
        foreach (var scalar in new[] { "[]", "\"receipt\"", "0", "true", "null" }) Reject(scalar, b);
        Require(Decode(original, b));
    }

    [Theory, MemberData(nameof(ReferenceShapes))]
    public void EveryReferenceRejectsAllClosedMemberAndWidthFaults(string shape)
    {
        var b = Binding(); var p = Payload(shape);
        foreach (var name in p.Keys.Where(n => n.EndsWith("Ref", StringComparison.Ordinal)))
        {
            var reference = ReferenceFields();
            foreach (var field in reference.Keys)
            {
                var missing = Copy(reference); missing.Remove(field); Fault(Obj(missing));
                var nil = Copy(reference); nil[field] = "null"; Fault(Obj(nil));
                var wrong = Copy(reference); wrong[field] = "true"; Fault(Obj(wrong));
                Fault(Duplicate(reference, field));
                var alias = Copy(reference); alias.Remove(field); alias[char.ToLowerInvariant(field[0]) + field[1..]] = reference[field]; Fault(Obj(alias));
                foreach (var value in BadHex(field == "ReceiptId" ? 32 : 64))
                {
                    var invalid = Copy(reference); invalid[field] = Q(value); Fault(Obj(invalid));
                }
            }
            var extra = Copy(reference); extra["Unknown"] = "0"; Fault(Obj(extra));
            foreach (var value in new[] { "[]", "\"ref\"", "1", "true", "null" }) Fault(value);
            Require(Decode(Receipt(shape, b), b));
            void Fault(string json) { var copy = Copy(p); copy[name] = json; Reject(Receipt(shape, b, Obj(copy)), b); }
        }
    }

    [Theory, MemberData(nameof(Shapes))]
    public void EveryHashAndIdRejectsWrongWidthCaseAlphabetTrimAndUnicode(string shape)
    {
        var b = Binding(); var p = Payload(shape);
        foreach (var name in p.Keys.Where(n => n.EndsWith("Sha256", StringComparison.Ordinal) || n == "TrainingPairId" || n == "PublicationId"))
            foreach (var value in BadHex(name == "PublicationId" ? 32 : 64))
            {
                var changed = Copy(p); changed[name] = Q(value); Reject(Receipt(shape, b, Obj(changed)), b);
            }
        foreach (var name in new[] { "ReceiptId", "SessionId", "BindingSha256" })
            foreach (var value in BadHex(name == "ReceiptId" ? 32 : 64))
            {
                var changed = ReceiptFields(shape, b); changed[name] = Q(value); Reject(Obj(changed), b);
            }
        Require(Decode(Receipt(shape, b), b));
    }

    [Theory, MemberData(nameof(Shapes))]
    public void RawCanonicalObjectRejectsOrderWhitespaceEscapesSchemaAliasesAndMalformedSurrogate(string shape)
    {
        var b = Binding(); var original = Receipt(shape, b); var fields = ReceiptFields(shape, b);
        foreach (var version in new[] { "2.0", "2e0", "2E+0", "-0", "0", "3", "true", "\"2\"", "9223372036854775808" })
        { var bad = Copy(fields); bad["SchemaVersion"] = version; Reject(Obj(bad), b); }
        Reject("{ " + original[1..], b);
        Reject("{" + string.Join(',', fields.Reverse().Select(p => Q(p.Key) + ":" + p.Value)) + "}", b);
        Reject(original.Replace("\"SchemaVersion\"", "\"\\u0053chemaVersion\"", StringComparison.Ordinal), b);
        Reject(original.Replace("\"a", "\"\\u0061", StringComparison.Ordinal), b);
        foreach (var name in Payload(shape).Keys.Concat(new[] { "ReceiptId", "ReceiptSha256" }).Distinct(StringComparer.Ordinal))
        {
            if (original.Contains(Q(name) + ":", StringComparison.Ordinal))
                Reject(original.Replace(Q(name) + ":", "\"\\u" + ((int)name[0]).ToString("x4",CultureInfo.InvariantCulture) + name[1..] + "\":", StringComparison.Ordinal), b);
        }
        Reject(original.Replace(Q(b.ObjectiveId), "\"\\uD800\"", StringComparison.Ordinal), b);
        Require(Decode(original, b));
    }

    [Theory]
    [InlineData("VerifiedAppliedUpdate", "OptimizerEntriesDecimal")]
    [InlineData("PublicationReceipt", "WeightsByteLengthDecimal")]
    [InlineData("FinalReopen", "WeightsByteLengthDecimal")]
    public void DecimalStringsAcceptFullUnsignedInt64AndRejectCoercion(string shape, string field)
    {
        var b = Binding(); var p = Payload(shape);
        foreach (var good in new[] { "0", "1", "9223372036854775807" })
        { var changed = Copy(p); changed[field] = Q(good); var d = Require(Decode(Receipt(shape, b, Obj(changed)), b)); Assert.Equal(good, d.Payload.GetProperty(field).GetString()); }
        foreach (var bad in new[] { "", "00", "01", "-0", "-1", "+1", " 1", "1 ", "1.0", "1e0", "١", "9223372036854775808", "10000000000000000000" })
        { var changed = Copy(p); changed[field] = Q(bad); Reject(Receipt(shape, b, Obj(changed)), b); }
        foreach (var bad in new[] { "0", "1.0", "1e0", "true", "null", "[]" })
        { var changed = Copy(p); changed[field] = bad; Reject(Receipt(shape, b, Obj(changed)), b); }
        Require(Decode(Receipt(shape, b), b));
    }

    [Theory]
    [InlineData("TrialEvaluation", "CompletedSlotCount", 5000)]
    [InlineData("PublicationReceipt", "ChildStep", int.MaxValue)]
    public void BoundedRawIntegersAcceptBothLimitsAndRejectAliases(string shape, string field, int maximum)
    {
        var b = Binding(); var p = Payload(shape);
        foreach (var good in new[] { 1, maximum })
        { var changed = Copy(p); changed[field] = good.ToString(CultureInfo.InvariantCulture); Assert.Equal(good, Require(Decode(Receipt(shape, b, Obj(changed)), b)).Payload.GetProperty(field).GetInt32()); }
        foreach (var bad in new[] { "0", "-0", "-1", "1.0", "1e0", "1E+0", "\"1\"", "true", "null", "[]", ((long)maximum + 1).ToString(CultureInfo.InvariantCulture), "9223372036854775808" })
        { var changed = Copy(p); changed[field] = bad; Reject(Receipt(shape, b, Obj(changed)), b); }
        Require(Decode(Receipt(shape, b), b));
    }

    [Fact]
    public void DiscriminantsAreClosedAndQualityDecisionHasThreeDisjointShapes()
    {
        var b = Binding();
        foreach (var (shape, field, legal) in new[]
        {
            ("Disposition", "Disposition", new[] { "RetainParent", "PromoteChild" }),
            ("FinalReopen", "SelectedRole", new[] { "Parent", "Child" }),
            ("QualityDecision:Improved", "Decision", new[] { "BaselineSufficient", "Improved", "NotImproved" }),
        })
        {
            foreach (var value in legal)
            {
                var actual = field == "Decision" ? "QualityDecision:" + value : shape;
                var p = Payload(actual); p[field] = Q(value); Require(Decode(Receipt(actual, b, Obj(p)), b));
                foreach (var bad in new[] { value.ToLowerInvariant(), " " + value, value + " ", value + "x", "", "Unknown" })
                { var changed = Copy(p); changed[field] = Q(bad); Reject(Receipt(actual, b, Obj(changed)), b); }
            }
        }
        foreach (var decision in new[] { "Improved", "NotImproved" })
        {
            var shape = "QualityDecision:" + decision; var p = Payload(shape);
            foreach (var name in new[] { "BaselineRef", "TrialEvaluationRef" })
            { var missing = Copy(p); missing.Remove(name); Reject(Receipt(shape, b, Obj(missing)), b); }
        }
        foreach (var name in new[] { "BaselineRef", "TrialEvaluationRef" })
        {
            foreach (var value in new[] { Reference(), "null" })
            { var extra = Payload("QualityDecision:BaselineSufficient"); extra[name] = value; Reject(Receipt("QualityDecision:BaselineSufficient", b, Obj(extra)), b); }
        }
        var root = ReceiptFields("SealedPair", b);
        foreach (var type in new[] { "sealedPair", "SealedPair ", "ProfileTraining", "RoutingRows", "PreparedBaseline", "", "Unknown" })
        { var changed = Copy(root); changed["ReceiptType"] = Q(type); Reject(Obj(changed), b); }
    }

    [Fact]
    public void ExpectedBindingUsesInheritedPureSyntaxAndSameFrameFactsWithoutAuthority()
    {
        var b = Binding(); var json = Receipt("SealedPair", b);
        foreach (var name in new[] { "ObjectiveId", "TrainingRunId", "SessionId", "BindingSha256" })
        { var p = ReceiptFields("SealedPair", b); p[name] = Q(name.EndsWith("Id", StringComparison.Ordinal) ? "other" : H('b')); Reject(Obj(p), b); }
        foreach (var invalid in new[]
        {
            b with { FormatVersion = 1 }, b with { FormatVersion = 3 }, b with { DescriptorsV2 = null },
            b with { ObjectiveId = "" }, b with { ObjectiveId = " objective" }, b with { ObjectiveId = "objective " },
            b with { ObjectiveId = "a\nb" }, b with { ObjectiveId = new string('x',129) }, b with { ObjectiveId = "\ud800" },
            b with { GrantId = "" }, b with { GrantId = " g" }, b with { GrantId = new string('\u0800',128) },
            b with { GrantVersion = "v " }, b with { GrantVersion = new string('x',65) }, b with { GrantVersion = new string('\u0800',64) },
            b with { TrainingRunId = "" }, b with { TrainingRunId = "a/b" }, b with { TrainingRunId = "é" }, b with { TrainingRunId = new string('x',129) },
            b with { CandidateSha256 = H('A') }, b with { ParentManifestSha256 = H('g') }, b with { ParentWeightsSha256 = "" },
            b with { ConfigurationSha256 = H('a') + "0" }, b with { ExamConditionSha256 = " " + H('a') },
            b with { DescriptorsV2 = b.DescriptorsV2! with { SchemaVersion = 1 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { SourceSelectionSha256 = "bad" } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { CaseCount = 0 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { CaseCount = 201 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { RepeatCount = 1 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { RepeatCount = 5001 } },
            b with { DescriptorsV2 = b.DescriptorsV2! with { CaseCount = 200, RepeatCount = 26 } },
        }) Assert.Equal(Recovery, Decode(json, invalid).ErrorCode);
        Assert.Equal(Recovery, Decode(json, null).ErrorCode);
        foreach (var run in new[] { ".", "_", ":", "-", "-._:AZaz09", new string('a',128) })
        { var valid = b with { TrainingRunId = run }; Require(Decode(Receipt("SealedPair", valid), valid)); }
        foreach (var objective in new[] { "a", "\u0800", "😀", "\"\\", new string('\u0800',128) })
        { var valid = b with { ObjectiveId = objective }; Require(Decode(Receipt("SealedPair", valid), valid)); }
        foreach (var descriptor in new[]
        {
            b.DescriptorsV2! with { ParentCheckpointSha256 = "bad" },
            b.DescriptorsV2! with { CandidateSelectionSha256 = "bad" },
            b.DescriptorsV2! with { CorpusDescriptorSha256 = "bad" },
            b.DescriptorsV2! with { EvaluationPlanSha256 = "bad" },
            b.DescriptorsV2! with { EvaluatorProfileSha256 = "bad" },
            b.DescriptorsV2! with { ProfileSelectionSha256 = "bad" },
        }) Assert.Equal(Recovery,Decode(json,b with { DescriptorsV2=descriptor }).ErrorCode);
        foreach(var descriptor in new[] { b.DescriptorsV2! with { CaseCount=200,RepeatCount=25 }, b.DescriptorsV2! with { CaseCount=1,RepeatCount=5000 } })
        { var valid=b with { DescriptorsV2=descriptor };Require(Decode(Receipt("SealedPair",valid),valid)); }
        var escaped = b with { GrantId = new string('"',128), GrantVersion = new string('"',64) };
        Require(Decode(Receipt("SealedPair", escaped), escaped));
        Require(Decode(json, b));
    }

    [Fact]
    public void DescriptiveSnapshotSurvivesDocumentDisposalCallerMutationAndPayloadChange()
    {
        var b = Binding(); var original = Receipt("FinalReopen", b); var source = Utf8.GetBytes(original);
        CodeDocumentationOneLessonProgressReceiptV2 decoded;
        using (var document = JsonDocument.Parse(source)) decoded = Require(CodeDocumentationOneLessonProgressReceiptV2.Decode(document.RootElement, b));
        Array.Fill(source, (byte)'x');
        var exposed = decoded.CanonicalBytes; Array.Fill(exposed, (byte)'x');
        Assert.Equal(Utf8.GetBytes(original), decoded.CanonicalBytes);
        Assert.Equal(original, decoded.Value.GetRawText());
        Assert.Equal(Payload("FinalReopen")["CheckpointSha256"], Q(decoded.Payload.GetProperty("CheckpointSha256").GetString()!));
        var changed = Payload("FinalReopen"); changed["CheckpointSha256"] = Q(H('b'));
        var json = Receipt("FinalReopen", b, Obj(changed));
        Assert.Equal(Utf8.GetByteCount(original), Utf8.GetByteCount(json));
        Assert.NotEqual(decoded.ReceiptSha256, Require(Decode(json,b)).ReceiptSha256);
        Assert.Equal(Recovery, CodeDocumentationOneLessonProgressReceiptV2.Decode(default, b).ErrorCode);
        var disposed = JsonDocument.Parse(original); var value = disposed.RootElement; disposed.Dispose();
        Assert.Equal(Recovery, CodeDocumentationOneLessonProgressReceiptV2.Decode(value,b).ErrorCode);
    }

    [Fact]
    public void F01_Parent1298Child1297AndRawObjectCapPrecedesMalformedDecimal()
    {
        var b = WideBinding();
        var parent = WideReceipt(b, "Parent"); var child = WideReceipt(b, "Child");
        Assert.Equal(1298, Utf8.GetByteCount(parent)); Assert.Equal(1297, Utf8.GetByteCount(child));
        Assert.Equal(1298, Require(Decode(parent,b)).CanonicalBytes.Length);
        Assert.Equal(1297, Require(Decode(child,b)).CanonicalBytes.Length);
        var over = parent.Replace("9223372036854775807", "10000000000000000000", StringComparison.Ordinal);
        Assert.Equal(1299, Utf8.GetByteCount(over)); Reject(over,b,Capacity);
        // Same-size overflow is malformed, not capacity; restores exact-cap.
        Reject(parent.Replace("9223372036854775807", "9223372036854775808", StringComparison.Ordinal),b);
        Require(Decode(parent,b));
        var tooWideObjective = b with { ObjectiveId = new string('\u0800',129) };
        Reject(WideReceipt(tooWideObjective,"Parent"),tooWideObjective,Capacity);
    }

    [Theory]
    [InlineData("SealedPair",1169)] [InlineData("RunnerCompletion",1360)] [InlineData("Baseline",1239)]
    [InlineData("VerifiedAppliedUpdate",1235)] [InlineData("TrialEvaluation",1372)] [InlineData("QualityDecision:Improved",1388)]
    [InlineData("Disposition",1092)] [InlineData("PublicationStarted",1214)] [InlineData("PublicationReceipt",1360)] [InlineData("FinalReopen",1298)]
    public void RecognizedRawTypeUsesItsOwnCeilingBeforeOrdinaryShapeErrors(string shape, int cap)
    {
        var b = Binding(); var json = Receipt(shape,b);
        // Deliberately malformed whitespace padded complete objects isolate the
        // raw byte guard. Some conservative ceilings are unreachable by valid
        // closed shapes; padding is not advertised as a positive typed receipt.
        var exact = "{" + new string(' ',cap-Utf8.GetByteCount(json)) + json[1..];
        Assert.Equal(cap,Utf8.GetByteCount(exact)); Reject(exact,b);
        Reject("{ " + exact[1..],b,Capacity);
        Require(Decode(json,b));
    }

    [Fact]
    public void ActualOpenPrefixReachesObjectCapThenKeepsValidReceiptsUnsupportedAndBytesIntact()
    {
        using var fixture = new Fixture(); var b = WideBinding();
        var (path,genesis) = Seed(fixture,b);
        var lockPath = Path.Combine(Path.GetDirectoryName(path)!,".owner.lock"); var lockBefore = File.ReadAllBytes(lockPath);
        var valid = WideReceipt(b,"Parent"); var invalid = valid.Replace("9223372036854775807","10000000000000000000",StringComparison.Ordinal);
        var validLine = Frame(b,1,"receipt",Digest(genesis[..^1]),valid);
        var invalidLine = Frame(b,1,"receipt",Digest(genesis[..^1]),invalid);
        Assert.Equal(3582,validLine.Length); Assert.Equal(3583,invalidLine.Length); Assert.True(invalidLine.Length < 4108);
        var bytes = genesis.Concat(invalidLine).ToArray(); File.WriteAllBytes(path,bytes);
        var over = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000);
        Assert.False(over.IsSuccess); Assert.Null(over.Value); Assert.Equal(Capacity,over.ErrorCode);
        Assert.Equal(bytes,File.ReadAllBytes(path)); Assert.Equal(lockBefore,File.ReadAllBytes(lockPath));
        bytes = genesis.Concat(validLine).ToArray(); File.WriteAllBytes(path,bytes);
        Assert.Equal(1298,Require(Decode(valid,b)).CanonicalBytes.Length);
        var unsupported = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000);
        Assert.False(unsupported.IsSuccess); Assert.Null(unsupported.Value); Assert.Equal(Recovery,unsupported.ErrorCode);
        var publicResult = CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot,b,10000);
        Assert.False(publicResult.IsSuccess); Assert.Null(publicResult.Value); Assert.Equal(Recovery,publicResult.ErrorCode);
        Assert.Equal(bytes,File.ReadAllBytes(path)); Assert.Equal(lockBefore,File.ReadAllBytes(lockPath));
        Assert.Equal(new[]{".owner.lock","session.jsonl"},Directory.GetFileSystemEntries(Path.GetDirectoryName(path)!).Select(Path.GetFileName).Order(StringComparer.Ordinal));
        File.WriteAllBytes(path,genesis);
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000));
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),restored.Session.Snapshot.Effects);
        Assert.Equal(1,Require(restored.Session.ReadCapacityPrefixV2()).PrefixFrames);
        restored.Dispose(); // Release the exclusive lock before external byte readback.
        Assert.Equal(genesis,File.ReadAllBytes(path)); Assert.Equal(lockBefore,File.ReadAllBytes(lockPath));
    }

    [Theory, MemberData(nameof(Shapes))]
    public void ActualOpenPrefixDeniesEachValidReceiptWithoutEffects(string shape)
    {
        using var fixture = new Fixture(); var b = Binding(); var (path,genesis) = Seed(fixture,b);
        var lockPath = Path.Combine(Path.GetDirectoryName(path)!,".owner.lock"); var lockBytes = File.ReadAllBytes(lockPath);
        var receipt = Receipt(shape,b); Require(Decode(receipt,b));
        var bytes = genesis.Concat(Frame(b,1,"receipt",Digest(genesis[..^1]),receipt)).ToArray(); File.WriteAllBytes(path,bytes);
        var result = CodeDocumentationOneLessonSession.OpenOrCreate(fixture.ModelRoot,b,10000);
        Assert.Equal(Recovery,result.ErrorCode); Assert.Null(result.Value); Assert.False(result.IsSuccess);
        Assert.Equal(bytes,File.ReadAllBytes(path)); Assert.Equal(lockBytes,File.ReadAllBytes(lockPath));
        File.WriteAllBytes(path,genesis);
        using var restored = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000));
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),restored.Session.Snapshot.Effects);
    }

    [Fact]
    public void ChangedHeldPrefixFailsCustodyBeforeNestedDecoderAndRestoresOwnedControl()
    {
        using var fixture = new Fixture(); var b = WideBinding(); var (path,genesis) = Seed(fixture,b);
        using var owner = Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000));
        var bad = WideReceipt(b,"Parent").Replace("9223372036854775807","10000000000000000000",StringComparison.Ordinal);
        var bytes = genesis.Concat(Frame(b,1,"receipt",Digest(genesis[..^1]),bad)).ToArray(); File.WriteAllBytes(path,bytes);
        Assert.Equal(Recovery,owner.Session.ReadCapacityPrefixV2().ErrorCode);
        Assert.Equal(bytes,File.ReadAllBytes(path));
        File.WriteAllBytes(path,genesis);
        Assert.Equal(1,Require(owner.Session.ReadCapacityPrefixV2()).PrefixFrames);
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),owner.Session.Snapshot.Effects);
    }

    [Theory]
    [InlineData("comment")] [InlineData("trailing-comma")] [InlineData("torn-object")]
    [InlineData("bom")] [InlineData("garbage")] [InlineData("invalid-utf8")]
    public void MalformedRawReceiptWireFailsInActualSessionReaderAndRestoresGenesis(string fault)
    {
        using var fixture=new Fixture();var b=Binding();var (path,genesis)=Seed(fixture,b);
        var lockPath=Path.Combine(Path.GetDirectoryName(path)!,".owner.lock");var lockBytes=File.ReadAllBytes(lockPath);
        var receipt=Receipt("SealedPair",b);Require(Decode(receipt,b));
        var bad=fault switch
        {
            "comment"=>receipt.Replace("\"Payload\":{","\"Payload\":{/*comment*/",StringComparison.Ordinal),
            "trailing-comma"=>receipt[..^1]+",}",
            "torn-object"=>receipt[..^1],
            "bom"=>"\ufeff"+receipt,
            "garbage"=>receipt+"garbage",
            "invalid-utf8"=>receipt,
            _=>throw new ArgumentOutOfRangeException(nameof(fault)),
        };
        var frame=Frame(b,1,"receipt",Digest(genesis[..^1]),bad);
        if(fault=="invalid-utf8")
        {
            var marker=Utf8.GetBytes("\"ReceiptType\":\"SealedPair\"");
            var start=frame.AsSpan().IndexOf(marker);Assert.True(start>=0);
            frame[start+"\"ReceiptType\":\"".Length]=0xff;
        }
        Assert.True(frame.Length<3980);var bytes=genesis.Concat(frame).ToArray();File.WriteAllBytes(path,bytes);
        var result=CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000);
        Assert.False(result.IsSuccess);Assert.Null(result.Value);Assert.Equal(Recovery,result.ErrorCode);
        Assert.Equal(bytes,File.ReadAllBytes(path));Assert.Equal(lockBytes,File.ReadAllBytes(lockPath));
        File.WriteAllBytes(path,genesis);
        using var restored=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000));
        Assert.Equal(1,Require(restored.Session.ReadCapacityPrefixV2()).PrefixFrames);
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),restored.Session.Snapshot.Effects);
        restored.Dispose(); // Preserve exclusive custody while held; read its bytes after release.
        Assert.Equal(genesis,File.ReadAllBytes(path));Assert.Equal(lockBytes,File.ReadAllBytes(lockPath));
    }

    [Fact]
    public void RuntimeIdentityBindsActualDecoderAndTestsAssemblyToNormalDependencyFiles()
    {
        // This executes inside the new test assembly; TRX stdout attributes the
        // observed loaded assembly bytes and normal config/deps to this fact.
        var assembly = typeof(CodeDocumentationOneLessonProgressReceiptV2).Assembly;
        Assert.Equal("TrainableRouter",assembly.GetName().Name);
        var testAssembly = typeof(CodeDocumentationOneLessonProgressReceiptTests).Assembly;
        var dir = Path.GetDirectoryName(testAssembly.Location)!;
        foreach (var path in new[] { assembly.Location,testAssembly.Location,
            Path.Combine(dir,"LlmFromScratch.Tests.runtimeconfig.json"),Path.Combine(dir,"LlmFromScratch.Tests.deps.json") })
        {
            var bytes = File.ReadAllBytes(path);
            output.WriteLine("RUNTIME_IDENTITY path={0} bytes={1} sha256={2}",path,bytes.Length,Digest(bytes));
            Assert.NotEmpty(bytes);
        }
        Require(Decode(Receipt("SealedPair",Binding()),Binding()));
    }

    private static CodeDocumentationOneLessonSessionBinding Binding() => new("objective","g","v","-run",H('1'),H('2'),H('3'),H('4'),H('5'))
        { FormatVersion=2,DescriptorsV2=new(2,H('a'),H('b'),H('c'),H('d'),H('e'),1,2,H('f'),H('0')) };
    private static CodeDocumentationOneLessonSessionBinding WideBinding() => Binding() with
        { ObjectiveId=new string('\u0800',128),TrainingRunId=new string('a',128),DescriptorsV2=Binding().DescriptorsV2! with { RepeatCount=5000 } };
    private static string WideReceipt(CodeDocumentationOneLessonSessionBinding b,string role)
    { var p=Payload("FinalReopen");p["SelectedRole"]=Q(role);p["WeightsByteLengthDecimal"]=Q("9223372036854775807");return Receipt("FinalReopen",b,Obj(p)); }

    private static SortedDictionary<string,string> Payload(string shape) => shape switch
    {
        "SealedPair" => Dict(("SourceSelectionSha256",Q(H('1'))),("TrainingPairId",Q(H('2'))),("SealedRowSha256",Q(H('3'))),("AuthoringAttemptIdSha256",Q(H('4')))),
        "RunnerCompletion" => Dict(("SealedPairRef",Reference()),("RunnerOutcomeSha256",Q(H('1')))),
        "Baseline" => Dict(("ExternalBaselineRef",Reference()),("BaselineObjectIdSha256",Q(H('1'))),("TrainingPairId",Q(H('2'))),("AttemptStateSha256",Q(H('3')))),
        "VerifiedAppliedUpdate" => Dict(("AttemptObjectIdSha256",Q(H('1'))),("AttemptStateSha256",Q(H('2'))),("ParentCheckpointSha256",Q(H('3'))),("AppliedWeightsSha256",Q(H('4'))),("OptimizerEntriesDecimal",Q("0"))),
        "TrialEvaluation" => Dict(("EvaluationPlanSha256",Q(H('1'))),("SourceSelectionSha256",Q(H('2'))),("TrainingPairId",Q(H('3'))),("CompletedSlotCount","1"),("SlotCompletionSetSha256",Q(H('4'))),("CaseSetSha256",Q(H('5'))),("ConditionSha256",Q(H('6')))),
        "QualityDecision:BaselineSufficient" => Dict(("Decision",Q("BaselineSufficient")),("ExternalBaselineRef",Reference()),("DecisionInputsSha256",Q(H('1')))),
        "QualityDecision:Improved" or "QualityDecision:NotImproved" => Dict(("Decision",Q(shape.Split(':')[1])),("ExternalBaselineRef",Reference()),("DecisionInputsSha256",Q(H('1'))),("BaselineRef",Reference()),("TrialEvaluationRef",Reference())),
        "Disposition" => Dict(("Disposition",Q("RetainParent")),("QualityDecisionRef",Reference()),("SelectedCheckpointSha256",Q(H('1')))),
        "PublicationStarted" => Dict(("DispositionRef",Reference()),("PublicationId",Q(new string('a',32))),("ParentCheckpointSha256",Q(H('1'))),("ProposedChildCheckpointSha256",Q(H('2')))),
        "PublicationReceipt" => Dict(("PublicationStartedRef",Reference()),("PublicationId",Q(new string('a',32))),("ChildCheckpointSha256",Q(H('1'))),("ManifestSha256",Q(H('2'))),("WeightsSha256",Q(H('3'))),("WeightsByteLengthDecimal",Q("0")),("ChildStep","1")),
        "FinalReopen" => Dict(("SelectedRole",Q("Parent")),("CheckpointSha256",Q(H('1'))),("ManifestSha256",Q(H('2'))),("WeightsSha256",Q(H('3'))),("WeightsByteLengthDecimal",Q("0")),("PredecessorReceiptRef",Reference())),
        _ => throw new ArgumentOutOfRangeException(nameof(shape)),
    };
    private static string Type(string shape)=>shape.Split(':')[0];
    private static SortedDictionary<string,string> ReceiptFields(string shape,CodeDocumentationOneLessonSessionBinding b,string? payload=null)
        =>Dict(("SchemaVersion","2"),("ReceiptType",Q(Type(shape))),("ReceiptId",Q(new string('a',32))),
            ("ObjectiveId",Q(b.ObjectiveId)),("SessionId",Q(Digest(Utf8.GetBytes(b.ObjectiveId)))),("TrainingRunId",Q(b.TrainingRunId)),
            ("BindingSha256",Q(BindingHash(b))),("Payload",payload??Obj(Payload(shape))));
    private static string Receipt(string shape,CodeDocumentationOneLessonSessionBinding b,string? payload=null)=>Obj(ReceiptFields(shape,b,payload));
    private static SortedDictionary<string,string> ReferenceFields()=>Dict(("ReceiptId",Q(new string('b',32))),("ReceiptSha256",Q(H('c'))));
    private static string Reference()=>Obj(ReferenceFields());
    private static string BindingJson(CodeDocumentationOneLessonSessionBinding b)
    {
        var d=b.DescriptorsV2!;
        return Obj(("ObjectiveId",Q(b.ObjectiveId)),("GrantId",Q(b.GrantId)),("GrantVersion",Q(b.GrantVersion)),("TrainingRunId",Q(b.TrainingRunId)),
            ("CandidateSha256",Q(b.CandidateSha256)),("ParentManifestSha256",Q(b.ParentManifestSha256)),("ParentWeightsSha256",Q(b.ParentWeightsSha256)),
            ("ConfigurationSha256",Q(b.ConfigurationSha256)),("ExamConditionSha256",Q(b.ExamConditionSha256)),
            ("DescriptorsV2",Obj(("SchemaVersion","2"),("SourceSelectionSha256",Q(d.SourceSelectionSha256)),("ParentCheckpointSha256",Q(d.ParentCheckpointSha256)),
                ("CandidateSelectionSha256",Q(d.CandidateSelectionSha256)),("CorpusDescriptorSha256",Q(d.CorpusDescriptorSha256)),("EvaluationPlanSha256",Q(d.EvaluationPlanSha256)),
                ("CaseCount",d.CaseCount.ToString(CultureInfo.InvariantCulture)),("RepeatCount",d.RepeatCount.ToString(CultureInfo.InvariantCulture)),
                ("EvaluatorProfileSha256",Q(d.EvaluatorProfileSha256)),("ProfileSelectionSha256",Q(d.ProfileSelectionSha256)))));
    }
    private static string BindingHash(CodeDocumentationOneLessonSessionBinding b)=>Digest(Utf8.GetBytes("code-documentation-one-lesson-binding-v2\n"+BindingJson(b)));
    private static byte[] Frame(CodeDocumentationOneLessonSessionBinding b,int revision=0,string kind="session",string previous="",string? receipt=null)
    {
        var fields=Dict(("Kind",Q(kind)),("FormatVersion","2"),("SessionId",Q(Digest(Utf8.GetBytes(b.ObjectiveId)))),("BindingV2",BindingJson(b)),
            ("BindingSha256",Q(BindingHash(b))),("Revision",Q(revision.ToString(CultureInfo.InvariantCulture))),("PreviousFrameSha256",Q(previous)),
            ("Effects",Obj(("BaselineDispatched",Q("0")),("BaselineCompleted",Q("0")),("TeachingDispatched",Q("0")),("TeachingCompleted",Q("0")),
                ("EvaluatorDispatched",Q("0")),("EvaluatorCompleted",Q("0")),("OptimizerEntries",Q("0")),("EffectOutcomeUnknown","false"))));
        if(receipt is not null)fields["ProgressReceipt"]=receipt;
        return Utf8.GetBytes(Obj(fields)+"\n");
    }
    private static (string Path,byte[] Genesis) Seed(Fixture fixture,CodeDocumentationOneLessonSessionBinding b)
    {
        string path;
        using(var owner=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000)))
            path=Path.Combine(fixture.Repository,owner.Session.Snapshot.HistoryRelativePath);
        var genesis=Frame(b);File.WriteAllBytes(path,genesis);
        // New test-owned fixture file receives the existing restrictive policy;
        // no production file, existing fixture or policy is weakened.
        if(OperatingSystem.IsLinux())File.SetUnixFileMode(path,UnixFileMode.UserRead|UnixFileMode.UserWrite);
        using(var positive=Require(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(fixture.ModelRoot,b,10000)))
            Assert.Equal(1,Require(positive.Session.ReadCapacityPrefixV2()).PrefixFrames);
        return(path,genesis);
    }
    private static OperationResult<CodeDocumentationOneLessonProgressReceiptV2> Decode(string json,CodeDocumentationOneLessonSessionBinding? b)
    {
        using var d=JsonDocument.Parse(Utf8.GetBytes(json),new JsonDocumentOptions{MaxDepth=8});
        return CodeDocumentationOneLessonProgressReceiptV2.Decode(d.RootElement,b);
    }
    private static void Reject(string json,CodeDocumentationOneLessonSessionBinding? b,string error=Recovery)
    {
        // Structural mutations MUST parse. An unrelated parse failure cannot
        // satisfy a schema, primitive, binding or object-cap assertion.
        using var document=JsonDocument.Parse(Utf8.GetBytes(json),new JsonDocumentOptions{MaxDepth=8});
        var result=CodeDocumentationOneLessonProgressReceiptV2.Decode(document.RootElement,b);
        Assert.False(result.IsSuccess);Assert.Null(result.Value);Assert.Equal(error,result.ErrorCode);
    }
    private static T Require<T>(OperationResult<T> r){Assert.True(r.IsSuccess,r.ErrorCode+": "+r.ErrorMessage);Assert.NotNull(r.Value);return r.Value!;}
    private static string H(char c)=>new(c,64);
    private static string Digest(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static IEnumerable<string> BadHex(int n)=>new[]{"",new string('a',n-1),new string('a',n+1),new string('A',n),new string('g',n)," "+new string('a',n-1),new string('a',n-1)+" ",new string('é',n)};
    private static SortedDictionary<string,string> Dict(params(string Name,string Raw)[] pairs)=>new(pairs.ToDictionary(p=>p.Name,p=>p.Raw),StringComparer.Ordinal);
    private static SortedDictionary<string,string> Copy(SortedDictionary<string,string> source)=>new(source,StringComparer.Ordinal);
    private static string Obj(params(string Name,string Raw)[] pairs)=>Obj(Dict(pairs));
    private static string Obj(SortedDictionary<string,string> fields)=>"{"+string.Join(',',fields.Select(p=>Q(p.Key)+":"+p.Value))+"}";
    private static string Duplicate(SortedDictionary<string,string> fields,string name,string? firstRaw=null)=>"{"+Q(name)+":"+(firstRaw??fields[name])+","+Obj(fields)[1..];
    private static string Q(string text)
    {
        var s=new StringBuilder("\"");
        foreach(var c in text)s.Append(c switch{'"'=>"\\\"",'\\'=>"\\\\",'\b'=>"\\b",'\f'=>"\\f",'\n'=>"\\n",'\r'=>"\\r",'\t'=>"\\t",< ' '=>"\\u"+((int)c).ToString("x4",CultureInfo.InvariantCulture),_=>c.ToString()});
        return s.Append('"').ToString();
    }
    private sealed class Fixture:IDisposable
    {
        private readonly string _root=Path.Combine(Path.GetTempPath(),"s04-progress-receipt-"+Guid.NewGuid().ToString("N"));
        internal Fixture()
        {
            Repository=Path.Combine(_root,"repo");
            Directory.CreateDirectory(Path.Combine(Repository,"14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository,"11.DPO"));
            Directory.CreateDirectory(Path.Combine(Repository,"training-artifacts","models"));
            File.WriteAllText(Path.Combine(Repository,"LlmFromScratch.sln"),"synthetic test-owned root");
            ModelRoot=Require(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(Repository));
        }
        internal string Repository{get;}
        internal RepositoryOwnedModelArtifactRoot ModelRoot{get;}
        public void Dispose(){if(Directory.Exists(_root))Directory.Delete(_root,true);}
    }
}
