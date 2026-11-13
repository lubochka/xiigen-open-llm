// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using ContinuousLearning;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

namespace LlmFromScratch.Tests;

public sealed class ContinuousLearningDpoPairAuthorshipPersistenceTests
{
    private const string NoPreferredResponse = "[no preferred response]";
    private static readonly string ManifestDigest = new('a', 64);
    private static readonly string LayerDigest = new('1', 64);

    [Fact]
    public void SideAuthorshipConstructorRejectsUnsupportedKindsAndPassportBypass()
    {
        var passport = OpenTeacherPassport("value-object-open-teacher");

        Assert.Null(typeof(DpoPairSideAuthorship)
            .GetProperty(nameof(DpoPairSideAuthorship.AuthorKind))!
            .SetMethod);
        Assert.Null(typeof(DpoPairSideAuthorship)
            .GetProperty(nameof(DpoPairSideAuthorship.OpenTeacherPassport))!
            .SetMethod);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DpoPairSideAuthorship((DpoPairSideAuthorKind)999));
        Assert.Throws<ArgumentException>(() =>
            new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                openTeacherPassport: null));
        Assert.Throws<ArgumentException>(() =>
            new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel,
                passport));
        Assert.Throws<ArgumentException>(() =>
            new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                TrainingTeacherProvenance.Deterministic(
                    "value-object-deterministic-passport")));

        var valid = new DpoPairSideAuthorship(
            DpoPairSideAuthorKind.OpenTeacher,
            passport);

        Assert.Equal(DpoPairSideAuthorKind.OpenTeacher, valid.AuthorKind);
        Assert.Equal(passport, valid.OpenTeacherPassport);
    }

    [Fact]
    public void Router_EditedUserCorrectionWritesUserChosenAndLocalRejectedAuthors()
    {
        using var fixture = new PairStoreFixture();
        var feedback = WithDeterministicAdmission(new FeedbackEvent(
            "router user edit",
            "local model draft",
            "human correction",
            FeedbackSignal.Edited,
            "BUG"));

        var routed = fixture.Router.Route(feedback);
        var pair = Assert.Single(AssertSuccess(fixture.Router.GetDpoPairs()));

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(DpoPairSideAuthorKind.User, pair.ChosenAuthorship!.AuthorKind);
        Assert.Equal(DpoPairSideAuthorKind.LocalModel, pair.RejectedAuthorship!.AuthorKind);
        Assert.Null(pair.ChosenAuthorship.OpenTeacherPassport);
        Assert.Null(pair.RejectedAuthorship.OpenTeacherPassport);
    }

    [Fact]
    public void Router_RejectedNoPreferredWritesDeterministicChosenAndLocalRejectedAuthors()
    {
        using var fixture = new PairStoreFixture();
        var feedback = WithDeterministicAdmission(new FeedbackEvent(
            "router rejected",
            "local model rejected draft",
            null,
            FeedbackSignal.Rejected,
            "BUG"));

        var routed = fixture.Router.Route(feedback);
        var pair = Assert.Single(AssertSuccess(fixture.Router.GetDpoPairs()));

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(NoPreferredResponse, pair.Chosen);
        Assert.Equal(
            DpoPairSideAuthorKind.DeterministicGenerator,
            pair.ChosenAuthorship!.AuthorKind);
        Assert.Equal(
            DpoPairSideAuthorKind.LocalModel,
            pair.RejectedAuthorship!.AuthorKind);
    }

    [Fact]
    public void Router_OpenTeacherModelResponseCarriesBoundPassportOnThatSideOnly()
    {
        using var fixture = new PairStoreFixture();
        var feedback = new FeedbackEvent(
            "router open teacher",
            "teacher draft",
            "human correction",
            FeedbackSignal.Edited,
            "BUG")
        {
            TeacherProvenance = OpenTeacherPassport("router-open-teacher"),
            GovernanceDecision = Governance('b'),
        };

        var routed = fixture.Router.Route(feedback);
        var pair = Assert.Single(AssertSuccess(fixture.Router.GetDpoPairs()));

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(DpoPairSideAuthorKind.User, pair.ChosenAuthorship!.AuthorKind);
        Assert.Equal(
            DpoPairSideAuthorKind.OpenTeacher,
            pair.RejectedAuthorship!.AuthorKind);
        Assert.Equal(
            pair.TeacherProvenance,
            pair.RejectedAuthorship.OpenTeacherPassport);
        Assert.NotEqual(
            string.Empty,
            pair.RejectedAuthorship.OpenTeacherPassport!.TrainingContentSha256);
    }

    [Fact]
    public void Router_ContradictoryOrUnprovenExternalAuthorshipRejectsBeforeWrite()
    {
        using var fixture = new PairStoreFixture();
        var contradictory = WithDeterministicAdmission(new FeedbackEvent(
            "router contradiction",
            "local model draft",
            "also correction",
            FeedbackSignal.Rejected,
            "BUG"));
        var invalidExternal = new FeedbackEvent(
            "router invalid external",
            "external draft",
            "human correction",
            FeedbackSignal.Edited,
            "BUG")
        {
            TeacherProvenance = OpenTeacherPassport("router-invalid-external") with
            {
                ModelArtifactDigest = string.Empty,
            },
            GovernanceDecision = Governance('c'),
        };

        var first = fixture.Router.Route(contradictory);
        var second = fixture.Router.Route(invalidExternal);

        Assert.False(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(DpoPairSideAuthorshipPolicy.InvalidErrorCode, first.ErrorCode);
        Assert.Equal(DpoPairSideAuthorshipPolicy.InvalidErrorCode, second.ErrorCode);
        Assert.Empty(Directory.Exists(fixture.Root)
            ? Directory.GetFiles(fixture.Root, "*.dpo-pair.json")
            : []);
    }

    [Fact]
    public void Store_V5RoundTripRestoresBothAuthorsAndExactRecordDomain()
    {
        using var fixture = new PairStoreFixture();
        var pair = BindLocalAuthoredPair(
            "roundtrip",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.LocalModel);
        var id = AssertSuccess(new FileDpoPairStore(fixture.Root).Add(pair));

        var path = Assert.Single(Directory.GetFiles(fixture.Root, "*.dpo-pair.json"));
        var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var loaded = Assert.Single(AssertSuccess(new FileDpoPairStore(fixture.Root)
            .GetAll()));

        Assert.Equal(5, json["schemaVersion"]!.GetValue<int>());
        Assert.Equal("xiigen.dpo-pair.v5", json["recordDomain"]!.GetValue<string>());
        Assert.Equal(id, json["pairId"]!.GetValue<string>());
        Assert.Equal(pair, loaded);
        Assert.Equal(DpoPairSideAuthorKind.User, loaded.ChosenAuthorship!.AuthorKind);
        Assert.Equal(
            DpoPairSideAuthorKind.LocalModel,
            loaded.RejectedAuthorship!.AuthorKind);
    }

    [Fact]
    public void Store_V5AuthorMutationChangesSemanticIdentity()
    {
        using var firstFixture = new PairStoreFixture();
        using var secondFixture = new PairStoreFixture();
        var first = BindLocalAuthoredPair(
            "identity-author",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.LocalModel);
        var second = BindLocalAuthoredPair(
            "identity-author",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.DeterministicGenerator);

        var firstId = AssertSuccess(new FileDpoPairStore(firstFixture.Root).Add(first));
        var secondId = AssertSuccess(new FileDpoPairStore(secondFixture.Root).Add(second));

        Assert.NotEqual(firstId, secondId);
    }

    [Theory]
    [MemberData(nameof(ForgedDeterministicProvenanceMutations))]
    public void Store_V5DeterministicNoExternalForgeryRejectsBeforePersistence(
        string mutation)
    {
        foreach (var authors in NonOpenTeacherAuthorPairs())
        {
            using var fixture = new PairStoreFixture();
            var pair = BindLocalAuthoredPair(
                $"forged-persist-{mutation}-{authors.Chosen}-{authors.Rejected}",
                authors.Chosen,
                authors.Rejected);
            var forged = pair with
            {
                TeacherProvenance = ForgeDeterministicNoExternalProvenance(
                    pair.TeacherProvenance!,
                    mutation)
            };

            var result = new FileDpoPairStore(fixture.Root).Add(forged);

            Assert.False(result.IsSuccess);
            Assert.Equal("INVALID_DPO_PAIR", result.ErrorCode);
            Assert.Empty(Directory.Exists(fixture.Root)
                ? Directory.GetFiles(fixture.Root, "*.dpo-pair.json")
                : []);
        }
    }

    [Theory]
    [MemberData(nameof(ForgedDeterministicProvenanceMutations))]
    public void Store_V5DeterministicNoExternalForgeryRejectsBeforeTransaction(
        string mutation)
    {
        foreach (var authors in NonOpenTeacherAuthorPairs())
        {
            using var fixture = new PairStoreFixture();
            var pair = BindLocalAuthoredPair(
                $"forged-transaction-{mutation}-{authors.Chosen}-{authors.Rejected}",
                authors.Chosen,
                authors.Rejected);
            var forged = pair with
            {
                TeacherProvenance = ForgeDeterministicNoExternalProvenance(
                    pair.TeacherProvenance!,
                    mutation)
            };
            AssertSuccess(new FileDpoPairStore(fixture.Root).Add(pair));
            WriteForgedV5Record(fixture.Root, forged);

            var prepared = new FileDpoPairStore(fixture.Root)
                .PrepareTransaction(["BUG"]);

            Assert.False(prepared.IsSuccess);
            Assert.True(
                StringComparer.Ordinal.Equals(
                    prepared.ErrorCode,
                    "DPO_PAIR_STORE_CORRUPT") ||
                StringComparer.Ordinal.Equals(
                    prepared.ErrorCode,
                    LegalTrainingTeacherAdmissionAuthority
                        .ProvenanceInvalidErrorCode),
                prepared.ErrorMessage);
            Assert.Empty(Directory.GetFiles(
                fixture.Root,
                "*.dpo-transaction.json"));
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeDeterministicRequestIdMutations))]
    public void Store_V5UnsafeDeterministicRequestIdRejectsBeforePersistence(
        string scenario,
        string requestId)
    {
        foreach (var authors in NonOpenTeacherAuthorPairs())
        {
            using var fixture = new PairStoreFixture();
            var pair = BindLocalAuthoredPair(
                $"unsafe-request-persist-{scenario}-{authors.Chosen}-{authors.Rejected}",
                authors.Chosen,
                authors.Rejected);
            var forged = pair with
            {
                TeacherProvenance = pair.TeacherProvenance! with
                {
                    RequestId = requestId
                }
            };

            var result = new FileDpoPairStore(fixture.Root).Add(forged);

            Assert.False(result.IsSuccess);
            Assert.Equal("INVALID_DPO_PAIR", result.ErrorCode);
            Assert.Empty(Directory.Exists(fixture.Root)
                ? Directory.GetFiles(fixture.Root, "*.dpo-pair.json")
                : []);
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeDeterministicRequestIdMutations))]
    public void Store_V5UnsafeDeterministicRequestIdRejectsBeforeTransaction(
        string scenario,
        string requestId)
    {
        foreach (var authors in NonOpenTeacherAuthorPairs())
        {
            using var fixture = new PairStoreFixture();
            var pair = BindLocalAuthoredPair(
                $"unsafe-request-transaction-{scenario}-{authors.Chosen}-{authors.Rejected}",
                authors.Chosen,
                authors.Rejected);
            var forged = pair with
            {
                TeacherProvenance = pair.TeacherProvenance! with
                {
                    RequestId = requestId
                }
            };
            AssertSuccess(new FileDpoPairStore(fixture.Root).Add(pair));
            WriteForgedV5Record(fixture.Root, forged);

            var prepared = new FileDpoPairStore(fixture.Root)
                .PrepareTransaction(["BUG"]);

            Assert.False(prepared.IsSuccess);
            Assert.True(
                StringComparer.Ordinal.Equals(
                    prepared.ErrorCode,
                    "DPO_PAIR_STORE_CORRUPT") ||
                StringComparer.Ordinal.Equals(
                    prepared.ErrorCode,
                    LegalTrainingTeacherAdmissionAuthority
                        .ProvenanceInvalidErrorCode),
                prepared.ErrorMessage);
            Assert.Empty(Directory.GetFiles(
                fixture.Root,
                "*.dpo-transaction.json"));
        }
    }

    [Fact]
    public void Store_ExternalProvenanceRequiresExactOpenTeacherSide()
    {
        using var noSideFixture = new PairStoreFixture();
        using var wrongSideFixture = new PairStoreFixture();
        using var exactSideFixture = new PairStoreFixture();
        var noOpenTeacherSide = BindExternalAuthoredPair(
            "external-no-open-teacher",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.LocalModel);
        var exactOpenTeacherSide = BindExternalAuthoredPair(
            "external-exact-open-teacher",
            DpoPairSideAuthorKind.OpenTeacher,
            DpoPairSideAuthorKind.LocalModel);
        var wrongPassport = exactOpenTeacherSide.TeacherProvenance! with
        {
            RequestId = RequestId("wrong-open-teacher-passport")
        };
        var wrongOpenTeacherSide = exactOpenTeacherSide with
        {
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                wrongPassport)
        };

        var noSide = new FileDpoPairStore(noSideFixture.Root)
            .Add(noOpenTeacherSide);
        var wrongSide = new FileDpoPairStore(wrongSideFixture.Root)
            .Add(wrongOpenTeacherSide);
        var exactSide = new FileDpoPairStore(exactSideFixture.Root)
            .Add(exactOpenTeacherSide);

        Assert.False(noSide.IsSuccess);
        Assert.Equal("INVALID_DPO_PAIR", noSide.ErrorCode);
        Assert.False(wrongSide.IsSuccess);
        Assert.Equal("INVALID_DPO_PAIR", wrongSide.ErrorCode);
        Assert.True(exactSide.IsSuccess, exactSide.ErrorMessage);
        Assert.Single(Directory.GetFiles(
            exactSideFixture.Root,
            "*.dpo-pair.json"));
    }

    [Fact]
    public void Store_V4ContentBoundRecordStaysReadableWithoutAuthorsAndNotTrainingReady()
    {
        using var fixture = new PairStoreFixture();
        var legacy = BindLegacyV4Pair("legacy-v4");
        var added = AssertSuccess(new FileDpoPairStore(fixture.Root).Add(legacy));
        var path = Assert.Single(Directory.GetFiles(fixture.Root, "*.dpo-pair.json"));
        var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();

        var loaded = Assert.Single(AssertSuccess(new FileDpoPairStore(fixture.Root)
            .GetAll()));
        var prepared = new FileDpoPairStore(fixture.Root).PrepareTransaction(["BUG"]);

        Assert.Equal(4, json["schemaVersion"]!.GetValue<int>());
        Assert.False(json.ContainsKey("sideAuthorship"));
        Assert.Equal(added, json["pairId"]!.GetValue<string>());
        Assert.Null(loaded.ChosenAuthorship);
        Assert.Null(loaded.RejectedAuthorship);
        Assert.False(prepared.IsSuccess);
        Assert.Equal("DPO_TRANSACTION_LEGACY_CONTENT_BINDING_REJECTED", prepared.ErrorCode);
    }

    [Theory]
    [InlineData("one-sided")]
    [InlineData("undefined-enum")]
    [InlineData("forged-passport")]
    public void Store_TamperedV5SideAuthorshipIsCorrupt(string scenario)
    {
        using var fixture = new PairStoreFixture();
        var pair = BindOpenTeacherChosenPair("tamper-" + scenario);
        Assert.True(new FileDpoPairStore(fixture.Root).Add(pair).IsSuccess);
        var path = Assert.Single(Directory.GetFiles(fixture.Root, "*.dpo-pair.json"));
        var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var sideAuthorship = json["sideAuthorship"]!.AsObject();
        var chosen = sideAuthorship["chosen"]!.AsObject();

        switch (scenario)
        {
            case "one-sided":
                sideAuthorship.Remove("rejected");
                break;
            case "undefined-enum":
                chosen["authorKind"] = "UnknownAuthorKind";
                break;
            case "forged-passport":
                chosen["openTeacherPassport"]!["requestId"] = "forged-passport";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
        }
        File.WriteAllText(path, json.ToJsonString());

        var loaded = new FileDpoPairStore(fixture.Root).GetAll();

        Assert.False(loaded.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", loaded.ErrorCode);
    }

    [Fact]
    public void AtomicGroup_V5ChildAuthorshipIsPersistedAndTamperRejectsWholeGroup()
    {
        using var fixture = new PairStoreFixture();
        var critic = BindOpenTeacherChosenPair("atomic-authorship", "critic prompt");
        var improver = BindOpenTeacherChosenPair("atomic-authorship", "improver prompt");
        var group = AssertSuccess(DpoPairAtomicGroupEnvelope.Create(critic, improver));
        var store = new FileDpoPairStore(fixture.Root);
        Assert.True(store.AddAtomicGroup(group).IsSuccess);
        var path = Assert.Single(Directory.GetFiles(
            fixture.Root,
            "*.dpo-pair-group.json"));
        var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var firstPair = json["children"]![0]!["pair"]!.AsObject();
        Assert.Equal(5, firstPair["schemaVersion"]!.GetValue<int>());
        Assert.NotNull(firstPair["sideAuthorship"]);

        firstPair["sideAuthorship"]!.AsObject().Remove("rejected");
        File.WriteAllText(path, json.ToJsonString());

        var loaded = new FileDpoPairStore(fixture.Root).GetAll();

        Assert.False(loaded.IsSuccess);
        Assert.Equal("DPO_PAIR_STORE_CORRUPT", loaded.ErrorCode);
    }

    private static FeedbackEvent WithDeterministicAdmission(FeedbackEvent feedback)
        => feedback with
        {
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                RequestId(feedback.Prompt)),
            GovernanceDecision = Governance('a'),
        };

    private static DpoPair BindLocalAuthoredPair(
        string name,
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor)
        => AssertSuccess(DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "prompt " + name,
            "chosen",
            "rejected",
            "BUG",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                RequestId(name)),
            GovernanceDecision = Governance('a'),
            ChosenAuthorship = new DpoPairSideAuthorship(chosenAuthor),
            RejectedAuthorship = new DpoPairSideAuthorship(rejectedAuthor),
        }));

    private static DpoPair BindLegacyV4Pair(string name)
        => AssertSuccess(DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "prompt " + name,
            "chosen",
            "rejected",
            "BUG",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                RequestId(name)),
            GovernanceDecision = Governance('a'),
        }));

    private static DpoPair BindExternalAuthoredPair(
        string name,
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor)
    {
        var passport = OpenTeacherPassport(name);
        return AssertSuccess(DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "prompt " + name,
            "chosen",
            "rejected",
            "BUG",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = Governance('b'),
            ChosenAuthorship = Authorship(chosenAuthor, passport),
            RejectedAuthorship = Authorship(rejectedAuthor, passport),
        }));
    }

    private static DpoPair BindOpenTeacherChosenPair(
        string requestId,
        string prompt = "open teacher prompt")
    {
        var passport = OpenTeacherPassport(requestId);
        return AssertSuccess(DpoPairTrainingContentAuthority.Bind(new DpoPair(
            prompt,
            "teacher chosen",
            "local rejected",
            "BUG",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = Governance('b'),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel),
        }));
    }

    private static DpoPairSideAuthorship Authorship(
        DpoPairSideAuthorKind authorKind,
        TrainingTeacherProvenance passport)
        => authorKind is DpoPairSideAuthorKind.OpenTeacher
            ? new DpoPairSideAuthorship(authorKind, passport)
            : new DpoPairSideAuthorship(authorKind);

    public static IEnumerable<object[]> ForgedDeterministicProvenanceMutations()
    {
        yield return ["model"];
        yield return ["provider"];
        yield return ["provider-terms-checked"];
        yield return ["provider-terms-allow"];
        yield return ["license"];
        yield return ["closed"];
        yield return ["teacher-used"];
        yield return ["invocation"];
        yield return ["artifact"];
    }

    public static IEnumerable<object[]> UnsafeDeterministicRequestIdMutations()
    {
        yield return ["empty", string.Empty];
        yield return ["whitespace", "   "];
        yield return ["trimmed", " request-id "];
        yield return ["control", "request\u0001id"];
        yield return ["overlong", new string('r', 257)];
    }

    private static (DpoPairSideAuthorKind Chosen, DpoPairSideAuthorKind Rejected)[]
        NonOpenTeacherAuthorPairs()
        => [
            (DpoPairSideAuthorKind.User, DpoPairSideAuthorKind.LocalModel),
            (
                DpoPairSideAuthorKind.LocalModel,
                DpoPairSideAuthorKind.DeterministicGenerator),
            (
                DpoPairSideAuthorKind.DeterministicGenerator,
                DpoPairSideAuthorKind.User)
        ];

    private static TrainingTeacherProvenance ForgeDeterministicNoExternalProvenance(
        TrainingTeacherProvenance provenance,
        string mutation)
        => mutation switch
        {
            "model" => provenance with
            {
                ModelId = "qwen3.5:9b",
                ModelVersion = ManifestDigest
            },
            "provider" => provenance with
            {
                Family = OpenWeightTeacherFamily.Qwen,
                AccessMode = TrainingTeacherAccessMode.LocalHostedOpenWeight,
                HostingProvider = TrainingTeacherHostingProvider.Ollama
            },
            "provider-terms-checked" => provenance with
            {
                ProviderTermsChecked = true
            },
            "provider-terms-allow" => provenance with
            {
                ProviderTermsAllowTraining = true
            },
            "license" => provenance with
            {
                ModelLicense = TrainingTeacherModelLicense.Apache20
            },
            "closed" => provenance with
            {
                ClosedProprietaryModel = true
            },
            "teacher-used" => provenance with
            {
                TeacherOutputUsedForTraining = true
            },
            "invocation" => provenance with
            {
                InvocationCount = 1
            },
            "artifact" => provenance with
            {
                ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority
                    .OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = LayerDigest
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(mutation),
                mutation,
                null)
        };

    private static void WriteForgedV5Record(string root, DpoPair pair)
    {
        Directory.CreateDirectory(root);
        var identity = FileDpoPairStore.ComputeIdentity(pair, schemaVersion: 5);
        var destinationPath = Path.Combine(root, identity + ".dpo-pair.json");
        var json = new JsonObject
        {
            ["schemaVersion"] = 5,
            ["recordDomain"] = "xiigen.dpo-pair.v5",
            ["pairId"] = identity,
            ["prompt"] = pair.Prompt,
            ["chosen"] = pair.Chosen,
            ["rejected"] = pair.Rejected,
            ["domain"] = pair.Domain,
            ["capturedAt"] = pair.CapturedAt
                .ToUniversalTime()
                .ToString("O"),
            ["teacherProvenance"] = new JsonObject
            {
                ["provenance"] = TeacherProvenanceJson(
                    pair.TeacherProvenance!),
                ["governanceDecision"] = GovernanceDecisionJson(
                    pair.GovernanceDecision!)
            },
            ["sideAuthorship"] = new JsonObject
            {
                ["chosen"] = SideAuthorshipJson(pair.ChosenAuthorship!),
                ["rejected"] = SideAuthorshipJson(pair.RejectedAuthorship!)
            }
        };

        var existingPath = Directory.GetFiles(root, "*.dpo-pair.json")
            .SingleOrDefault();
        var writePath = existingPath ?? destinationPath;
        File.WriteAllText(writePath, json.ToJsonString());
        if (!StringComparer.Ordinal.Equals(
                Path.GetFullPath(writePath),
                Path.GetFullPath(destinationPath)))
        {
            File.Move(writePath, destinationPath);
        }
    }

    private static JsonObject TeacherProvenanceJson(
        TrainingTeacherProvenance provenance)
        => new()
        {
            ["sourceKind"] = provenance.SourceKind.ToString(),
            ["family"] = NullableEnumJson(provenance.Family),
            ["accessMode"] = NullableEnumJson(provenance.AccessMode),
            ["hostingProvider"] = NullableEnumJson(provenance.HostingProvider),
            ["modelId"] = provenance.ModelId,
            ["modelVersion"] = provenance.ModelVersion,
            ["modelLicense"] = NullableEnumJson(provenance.ModelLicense),
            ["providerTermsChecked"] = provenance.ProviderTermsChecked,
            ["providerTermsAllowTraining"] =
                provenance.ProviderTermsAllowTraining,
            ["closedProprietaryModel"] = provenance.ClosedProprietaryModel,
            ["teacherOutputUsedForTraining"] =
                provenance.TeacherOutputUsedForTraining,
            ["requestId"] = provenance.RequestId,
            ["invocationCount"] = provenance.InvocationCount,
            ["modelArtifactDigestKind"] = provenance.ModelArtifactDigestKind,
            ["modelArtifactDigest"] = provenance.ModelArtifactDigest,
            ["trainingContentSha256"] = provenance.TrainingContentSha256
        };

    private static JsonObject GovernanceDecisionJson(
        TrainingDataGovernanceDecision governance)
    {
        var evidenceRefs = new JsonArray();
        foreach (var evidenceRef in governance.EvidenceRefs)
            evidenceRefs.Add(evidenceRef);

        return new JsonObject
        {
            ["licenseAccepted"] = governance.LicenseAccepted,
            ["isQuarantined"] = governance.IsQuarantined,
            ["evidenceRefs"] = evidenceRefs
        };
    }

    private static JsonObject SideAuthorshipJson(
        DpoPairSideAuthorship authorship)
        => new()
        {
            ["authorKind"] = authorship.AuthorKind.ToString(),
            ["openTeacherPassport"] = authorship.OpenTeacherPassport is null
                ? null
                : TeacherProvenanceJson(authorship.OpenTeacherPassport)
        };

    private static JsonNode? NullableEnumJson<TEnum>(TEnum? value)
        where TEnum : struct, Enum
        => value is null ? null : JsonValue.Create(value.Value.ToString());

    private static TrainingTeacherProvenance OpenTeacherPassport(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            ManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority
                .OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = LayerDigest,
        };

    private static TrainingDataGovernanceDecision Governance(char evidence)
        => new(
            licenseAccepted: true,
            isQuarantined: false,
            [$"sha256:{new string(evidence, 64)}"]);

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static string RequestId(string value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private sealed class PairStoreFixture : IDisposable
    {
        internal PairStoreFixture()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "rich-pair-authorship-" + Guid.NewGuid().ToString("N"));
            Router = new FeedbackRouter(
                new NullAuthorshipSessionStorage(),
                new RecordingAuthorshipCache(),
                Root);
        }

        internal string Root { get; }

        internal FeedbackRouter Router { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class RecordingAuthorshipCache : IDomainDatasetCache
    {
        public OperationResult<bool> Add(TrainingExample example)
            => OperationResult<bool>.Ok(true);

        public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(
            string domain)
            => OperationResult<IReadOnlyList<TrainingExample>>.Ok([]);

        public OperationResult<int> TotalCount()
            => OperationResult<int>.Ok(0);

        public Task<OperationResult<bool>> FlushAsync()
            => Task.FromResult(OperationResult<bool>.Ok(true));
    }

    private sealed class NullAuthorshipSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(
            string sessionId,
            AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(OperationResult<AgentState>.Fail(
                "Not persisted.",
                "NULL_STORAGE"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
    }
}
