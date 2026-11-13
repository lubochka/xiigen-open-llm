// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using ModelServer.DesignConversion;

public sealed class DesignConversionArtifactAccessPolicyTests
{
    private static readonly DateTimeOffset NowUtc =
        new(2026, 7, 29, 12, 0, 0, TimeSpan.Zero);
    private static readonly string ValidSha256 = "sha256:" + new string('a', 64);
    private const string TenantId = "tenant-alpha";
    private const string JobId = "job-001";
    private const string ArtifactRefId = "artifact-result-001";
    private const string StorageRef = "artifact-store/tenant-alpha/job-001/artifact-result-001/blob";

    [Fact]
    public void ArtifactAccessPolicyAuthorizedReadTests()
    {
        var policy = new ArtifactAccessPolicy();
        var result = policy.TryAuthorizeRead(
            Request(role: ArtifactAccessPolicy.TenantUserRole),
            StoredArtifact(),
            out var audit,
            NowUtc);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var grant = result.Value!;
        Assert.StartsWith("brokered-artifact-read:", grant.BrokeredAccessRef, StringComparison.Ordinal);
        Assert.Equal(NowUtc.AddMinutes(15), grant.ExpiresAtUtc);
        Assert.Equal($"{TenantId}/{JobId}/{ArtifactRefId}:read", grant.Scope);
        Assert.Equal(TenantId, grant.TenantId);
        Assert.Equal(JobId, grant.JobId);
        Assert.Equal(ArtifactRefId, grant.ArtifactRefId);
        Assert.Equal(ArtifactAccessPolicy.TenantUserRole, grant.Role);
        Assert.Equal(ArtifactAccessPolicy.ResultSafePurpose, grant.Purpose);
        Assert.Equal(ArtifactAccessPolicy.ResultSafeClassification, grant.Classification);
        AssertSafePublicText(grant.ToString()!);

        Assert.Equal("allow", audit.Decision);
        Assert.Equal(ArtifactAccessPolicy.AllowReason, audit.Reason);
        Assert.Equal(grant.ExpiresAtUtc, audit.ExpiresAtUtc);
        Assert.Equal(grant.RedactedStorageRefHash, audit.RedactedStorageRefHash);
        AssertSafePublicText(audit.ToString()!);
    }

    [Fact]
    public void ArtifactAccessPolicyCrossTenantDeniedTests()
    {
        var policy = new ArtifactAccessPolicy();
        var result = policy.TryAuthorizeRead(
            Request(tenantId: "tenant-beta"),
            StoredArtifact(),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.TenantMismatchCode);
        Assert.Equal("tenant-beta", audit.TenantId);
        AssertSafePublicText(result.ErrorMessage);
        AssertSafePublicText(audit.ToString()!);
    }

    [Fact]
    public void ArtifactSignedUrlExpiryTests()
    {
        var policy = new ArtifactAccessPolicy();

        var shortRetention = policy.TryAuthorizeRead(
            Request(),
            StoredArtifact(retentionExpiresAtUtc: NowUtc.AddMinutes(5)),
            out var allowAudit,
            NowUtc);

        Assert.True(shortRetention.IsSuccess, shortRetention.ErrorMessage);
        Assert.Equal(NowUtc.AddMinutes(5), shortRetention.Value!.ExpiresAtUtc);
        Assert.Equal(NowUtc.AddMinutes(5), allowAudit.ExpiresAtUtc);
        Assert.DoesNotContain("http://", shortRetention.Value.BrokeredAccessRef, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", shortRetention.Value.BrokeredAccessRef, StringComparison.OrdinalIgnoreCase);

        var expiredRetention = policy.TryAuthorizeRead(
            Request(),
            StoredArtifact(retentionExpiresAtUtc: NowUtc.AddTicks(-1)),
            out var retentionAudit,
            NowUtc);

        AssertDenied(expiredRetention, retentionAudit, ArtifactAccessPolicy.RetentionExpiredCode);

        var expiredPresentedGrant = policy.TryAuthorizeRead(
            Request(presentedBrokeredGrantExpiresAtUtc: NowUtc.AddTicks(-1)),
            StoredArtifact(),
            out var tokenAudit,
            NowUtc);

        AssertDenied(expiredPresentedGrant, tokenAudit, ArtifactAccessPolicy.BrokeredGrantExpiredCode);
    }

    [Theory]
    [InlineData("artifact-store/tenant-alpha/job-001/../secret/blob")]
    [InlineData("artifact-store/tenant-alpha/job-001/%2e%2e/secret/blob")]
    [InlineData("artifact-store/tenant-alpha/job-001/%252e%252e/secret/blob")]
    [InlineData("/var/data/artifact.png")]
    [InlineData(@"C:\data\artifact.png")]
    [InlineData(@"\\server\share\artifact.png")]
    [InlineData(@"\\?\C:\data\artifact.png")]
    [InlineData("file:///C:/data/artifact.png")]
    [InlineData("https://storage.example.test/signed/path?token=secret")]
    [InlineData("artifact-store//tenant-alpha/job-001/artifact")]
    [InlineData("bucket/key/object")]
    public void ArtifactPathTraversalDeniedTests(string storageRef)
    {
        var policy = new ArtifactAccessPolicy();
        var result = policy.TryAuthorizeRead(
            Request(),
            StoredArtifact(storageRef: storageRef),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.StorageRefUnsafeCode);
        AssertSafePublicText(result.ErrorMessage);
        AssertSafePublicText(audit.ToString()!);
    }

    [Fact]
    public void ArtifactNoRawPathExposureTests()
    {
        const string rawSignedUrl =
            "https://storage.example.test/bucket/private-object.png?sig=secret-token";
        var policy = new ArtifactAccessPolicy();

        var result = policy.TryAuthorizeRead(
            Request(),
            StoredArtifact(storageRef: rawSignedUrl),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.StorageRefUnsafeCode);
        Assert.StartsWith("sha256:", audit.RedactedStorageRefHash, StringComparison.Ordinal);
        Assert.DoesNotContain(rawSignedUrl, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(rawSignedUrl, result.ErrorCode, StringComparison.Ordinal);
        Assert.DoesNotContain(rawSignedUrl, audit.ToString(), StringComparison.Ordinal);
        AssertSafePublicText(result.ErrorMessage);
        AssertSafePublicText(audit.ToString()!);
    }

    [Fact]
    public void ArtifactAccessPolicyMaliciousRequestScalarAuditRedactsRawValuesTests()
    {
        const string maliciousCorrelationId = "https://evil.example.test/corr?token=secret-token";
        const string maliciousTenantId = "tenant-alpha-secret-token";
        const string maliciousJobId = "file:///C:/jobs/secret-token";
        const string maliciousArtifactRefId = @"..\private\secret-token";
        const string maliciousRole = "tenant-user\r\nbearer-secret-token";
        var policy = new ArtifactAccessPolicy();

        var result = policy.TryAuthorizeRead(
            new ArtifactAccessPolicyReadRequest(
                CorrelationId: maliciousCorrelationId,
                TenantId: maliciousTenantId,
                JobId: maliciousJobId,
                ArtifactRefId: maliciousArtifactRefId,
                CallerRole: maliciousRole),
            StoredArtifact(),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.CorrelationIdInvalidCode);
        Assert.StartsWith("redacted-", audit.CorrelationId, StringComparison.Ordinal);
        Assert.StartsWith("redacted-", audit.TenantId, StringComparison.Ordinal);
        Assert.StartsWith("redacted-", audit.JobId, StringComparison.Ordinal);
        Assert.StartsWith("redacted-", audit.ArtifactRefId, StringComparison.Ordinal);
        Assert.StartsWith("redacted-", audit.Role, StringComparison.Ordinal);
        AssertNoRawValues(
            [maliciousCorrelationId, maliciousTenantId, maliciousJobId, maliciousArtifactRefId, maliciousRole],
            result,
            audit);
        AssertSafePublicText(result.ErrorMessage);
        AssertSafePublicText(audit.ToString()!);
    }

    [Fact]
    public void ArtifactAccessPolicyMaliciousStoredScalarAuditRedactsRawValuesTests()
    {
        const string maliciousTenantId = "tenant-alpha-secret-token";
        const string maliciousJobId = "https://evil.example.test/jobs/secret-token";
        const string maliciousArtifactRefId = @"C:\private\artifact-secret-token";
        var policy = new ArtifactAccessPolicy();

        var result = policy.TryAuthorizeRead(
            null,
            StoredArtifact(
                tenantId: maliciousTenantId,
                jobId: maliciousJobId,
                artifactRefId: maliciousArtifactRefId),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.RequestRequiredCode);
        Assert.StartsWith("redacted-", audit.TenantId, StringComparison.Ordinal);
        Assert.StartsWith("redacted-", audit.JobId, StringComparison.Ordinal);
        Assert.StartsWith("redacted-", audit.ArtifactRefId, StringComparison.Ordinal);
        AssertNoRawValues([maliciousTenantId, maliciousJobId, maliciousArtifactRefId], result, audit);
        AssertSafePublicText(result.ErrorMessage);
        AssertSafePublicText(audit.ToString()!);
    }

    [Fact]
    public void ArtifactAccessPolicyBrokeredRefsAreNonDeterministicInDefaultProductionModeTests()
    {
        var policy = new ArtifactAccessPolicy();

        var first = policy.TryAuthorizeRead(Request(), StoredArtifact(), out _, NowUtc);
        var second = policy.TryAuthorizeRead(Request(), StoredArtifact(), out _, NowUtc);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.NotEqual(first.Value!.BrokeredAccessRef, second.Value!.BrokeredAccessRef);
        AssertBrokeredAccessRefIsOpaque(first.Value.BrokeredAccessRef);
        AssertBrokeredAccessRefIsOpaque(second.Value.BrokeredAccessRef);
    }

    [Fact]
    public void ArtifactAccessPolicyBrokeredRefsCanUseDeterministicNonceProviderForTests()
    {
        var signingSecret = Enumerable.Repeat((byte)0x42, 32).ToArray();
        var nonce = Enumerable.Range(0, 16).Select(static value => (byte)value).ToArray();
        var firstPolicy = new ArtifactAccessPolicy(null, null, signingSecret, () => nonce.ToArray());
        var secondPolicy = new ArtifactAccessPolicy(null, null, signingSecret, () => nonce.ToArray());

        var first = firstPolicy.TryAuthorizeRead(Request(), StoredArtifact(), out _, NowUtc);
        var second = secondPolicy.TryAuthorizeRead(Request(), StoredArtifact(), out _, NowUtc);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value!.BrokeredAccessRef, second.Value!.BrokeredAccessRef);
        Assert.StartsWith(
            "brokered-artifact-read:" + Convert.ToHexString(nonce).ToLowerInvariant() + ".",
            first.Value.BrokeredAccessRef,
            StringComparison.Ordinal);
        AssertBrokeredAccessRefIsOpaque(first.Value.BrokeredAccessRef);
    }

    [Fact]
    public void ArtifactAccessPolicyWrongJobDeniedTests()
    {
        var policy = new ArtifactAccessPolicy();
        var result = policy.TryAuthorizeRead(
            Request(jobId: "job-002"),
            StoredArtifact(),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.JobMismatchCode);
    }

    [Fact]
    public void ArtifactAccessPolicyWrongArtifactRefDeniedTests()
    {
        var policy = new ArtifactAccessPolicy();
        var result = policy.TryAuthorizeRead(
            Request(artifactRefId: "artifact-result-002"),
            StoredArtifact(),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.ArtifactRefMismatchCode);
    }

    [Fact]
    public void ArtifactAccessPolicyInvalidRoleDeniedTests()
    {
        var policy = new ArtifactAccessPolicy();
        var result = policy.TryAuthorizeRead(
            Request(role: "viewer"),
            StoredArtifact(),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.CallerRoleInvalidCode);
    }

    [Fact]
    public void ArtifactAccessPolicyMissingRoleDeniedTests()
    {
        var policy = new ArtifactAccessPolicy();
        var result = policy.TryAuthorizeRead(
            Request(role: string.Empty),
            StoredArtifact(),
            out var audit,
            NowUtc);

        AssertDenied(result, audit, ArtifactAccessPolicy.CallerRoleInvalidCode);
    }

    [Fact]
    public void ArtifactAccessPolicyQuarantinedDeletedAndMalformedShaDeniedTests()
    {
        var policy = new ArtifactAccessPolicy();

        var quarantined = policy.TryAuthorizeRead(
            Request(),
            StoredArtifact(status: "quarantined"),
            out var quarantinedAudit,
            NowUtc);
        AssertDenied(quarantined, quarantinedAudit, ArtifactAccessPolicy.StatusNotReadableCode);

        var deleted = policy.TryAuthorizeRead(
            Request(),
            StoredArtifact(status: "deleted"),
            out var deletedAudit,
            NowUtc);
        AssertDenied(deleted, deletedAudit, ArtifactAccessPolicy.StatusNotReadableCode);

        var malformedSha = policy.TryAuthorizeRead(
            Request(),
            StoredArtifact(contentSha256: "sha256:" + new string('A', 64)),
            out var malformedShaAudit,
            NowUtc);
        AssertDenied(malformedSha, malformedShaAudit, ArtifactAccessPolicy.ContentHashInvalidCode);
    }

    [Fact]
    public void ArtifactAccessPolicyElevatedRolesReadEvidenceAndTrainingTests()
    {
        var policy = new ArtifactAccessPolicy();

        var architectEvidence = policy.TryAuthorizeRead(
            Request(role: ArtifactAccessPolicy.ArchitectRole),
            StoredArtifact(
                purpose: ArtifactAccessPolicy.EvidencePurpose,
                classification: ArtifactAccessPolicy.EvidenceClassification),
            out var architectAudit,
            NowUtc);
        Assert.True(architectEvidence.IsSuccess, architectEvidence.ErrorMessage);
        Assert.Equal("allow", architectAudit.Decision);

        var localTraining = policy.TryAuthorizeRead(
            Request(role: ArtifactAccessPolicy.LocalTrainingRole),
            StoredArtifact(
                purpose: ArtifactAccessPolicy.TrainingPurpose,
                classification: ArtifactAccessPolicy.TrainingClassification),
            out var localTrainingAudit,
            NowUtc);
        Assert.True(localTraining.IsSuccess, localTraining.ErrorMessage);
        Assert.Equal("allow", localTrainingAudit.Decision);
    }

    [Fact]
    public void ArtifactAccessPolicyProviderPrivateRawDeniedUnlessDiagnosticPurposeAllowedTests()
    {
        var defaultPolicy = new ArtifactAccessPolicy();

        var adminPrivate = defaultPolicy.TryAuthorizeRead(
            Request(role: ArtifactAccessPolicy.AdminRole),
            StoredArtifact(
                purpose: ArtifactAccessPolicy.ProviderPrivatePurpose,
                classification: ArtifactAccessPolicy.ProviderPrivateClassification),
            out var adminAudit,
            NowUtc);
        AssertDenied(adminPrivate, adminAudit, ArtifactAccessPolicy.RoleDeniedCode);

        var diagnosticWithoutPolicyInput = defaultPolicy.TryAuthorizeRead(
            Request(role: ArtifactAccessPolicy.DiagnosticRole),
            StoredArtifact(
                purpose: ArtifactAccessPolicy.ProviderPrivatePurpose,
                classification: ArtifactAccessPolicy.ProviderPrivateClassification),
            out var diagnosticDeniedAudit,
            NowUtc);
        AssertDenied(diagnosticWithoutPolicyInput, diagnosticDeniedAudit, ArtifactAccessPolicy.RoleDeniedCode);

        var diagnosticPolicy = new ArtifactAccessPolicy(
            diagnosticReadablePurposes: new[] { ArtifactAccessPolicy.ProviderPrivatePurpose });
        var diagnosticAllowed = diagnosticPolicy.TryAuthorizeRead(
            Request(role: ArtifactAccessPolicy.DiagnosticRole),
            StoredArtifact(
                purpose: ArtifactAccessPolicy.ProviderPrivatePurpose,
                classification: ArtifactAccessPolicy.ProviderPrivateClassification),
            out var diagnosticAllowedAudit,
            NowUtc);

        Assert.True(diagnosticAllowed.IsSuccess, diagnosticAllowed.ErrorMessage);
        Assert.Equal("allow", diagnosticAllowedAudit.Decision);
    }

    private static ArtifactAccessPolicyReadRequest Request(
        string tenantId = TenantId,
        string jobId = JobId,
        string artifactRefId = ArtifactRefId,
        string role = ArtifactAccessPolicy.TenantUserRole,
        DateTimeOffset? presentedBrokeredGrantExpiresAtUtc = null)
        => new(
            CorrelationId: "corr-001",
            TenantId: tenantId,
            JobId: jobId,
            ArtifactRefId: artifactRefId,
            CallerRole: role,
            PresentedBrokeredGrantExpiresAtUtc: presentedBrokeredGrantExpiresAtUtc);

    private static ArtifactAccessPolicyStoredArtifact StoredArtifact(
        string tenantId = TenantId,
        string jobId = JobId,
        string artifactRefId = ArtifactRefId,
        string purpose = ArtifactAccessPolicy.ResultSafePurpose,
        string classification = ArtifactAccessPolicy.ResultSafeClassification,
        string status = ArtifactAccessPolicy.ActiveStatus,
        DateTimeOffset? retentionExpiresAtUtc = null,
        string storageRef = StorageRef,
        string? contentSha256 = null)
        => new(
            TenantId: tenantId,
            JobId: jobId,
            ArtifactRefId: artifactRefId,
            Purpose: purpose,
            Classification: classification,
            Status: status,
            RetentionExpiresAtUtc: retentionExpiresAtUtc ?? NowUtc.AddHours(1),
            StorageRef: storageRef,
            ContentSha256: contentSha256 ?? ValidSha256);

    private static void AssertDenied(
        OperationResult<ArtifactAccessPolicyReadGrant> result,
        ArtifactAccessPolicyAuditEvent audit,
        string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal("deny", audit.Decision);
        Assert.Equal(expectedCode, audit.Reason);
        Assert.Null(audit.ExpiresAtUtc);
        Assert.StartsWith("sha256:", audit.RedactedStorageRefHash, StringComparison.Ordinal);
    }

    private static void AssertSafePublicText(string value)
    {
        Assert.DoesNotContain(StorageRef, value, StringComparison.Ordinal);
        Assert.DoesNotContain("https://", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("file:", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"C:\", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"\\server", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-token", value, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoRawValues(
        IEnumerable<string> rawValues,
        OperationResult<ArtifactAccessPolicyReadGrant> result,
        ArtifactAccessPolicyAuditEvent audit)
    {
        var publicText = string.Join(
            "\n",
            result.ErrorMessage,
            result.ErrorCode,
            result.Value?.ToString() ?? string.Empty,
            audit.ToString());

        foreach (var rawValue in rawValues)
        {
            Assert.DoesNotContain(rawValue, publicText, StringComparison.Ordinal);
        }
    }

    private static void AssertBrokeredAccessRefIsOpaque(string brokeredAccessRef)
    {
        Assert.StartsWith("brokered-artifact-read:", brokeredAccessRef, StringComparison.Ordinal);
        Assert.DoesNotContain(TenantId, brokeredAccessRef, StringComparison.Ordinal);
        Assert.DoesNotContain(JobId, brokeredAccessRef, StringComparison.Ordinal);
        Assert.DoesNotContain(ArtifactRefId, brokeredAccessRef, StringComparison.Ordinal);
        Assert.DoesNotContain(StorageRef, brokeredAccessRef, StringComparison.Ordinal);
        Assert.DoesNotContain(ValidSha256, brokeredAccessRef, StringComparison.Ordinal);
        Assert.DoesNotContain("/", brokeredAccessRef, StringComparison.Ordinal);
        Assert.DoesNotContain("\\", brokeredAccessRef, StringComparison.Ordinal);
    }
}
