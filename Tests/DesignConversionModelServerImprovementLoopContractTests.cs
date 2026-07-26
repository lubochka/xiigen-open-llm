// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerImprovementLoopContractTests
{
    [Fact]
    public void DescribeImprovementLoopRoute_DocumentsLocalOnlyRouteAndRequiredFields()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeImprovementLoopRoute();
        var expectedRequiredRequestFields = RequiredRequestFieldNames(descriptor.RequestFields);

        Assert.Equal(DesignConversionModelServerCallerContract.ImprovementLoopRouteSchemaVersion, descriptor.SchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.ImprovementLoopRouteBoundary, descriptor.Boundary);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointMethod, descriptor.EndpointMethod);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, descriptor.EndpointPath);
        Assert.Equal("application/json", descriptor.RequestContentType);
        Assert.Equal("application/json; charset=utf-8", descriptor.ResponseContentType);
        Assert.Equal(
            expectedRequiredRequestFields,
            DesignConversionModelServerCallerContract.ImprovementLoopRequiredRequestFieldNames());
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "loopId", Required: true, MayCarryPrivateInput: false, ResponseMayEchoRawValue: true });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "initialPackage", Required: true, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "maxAttempts", Required: true, MayCarryPrivateInput: false, ResponseMayEchoRawValue: true });
        Assert.Contains(descriptor.RequestFields, field =>
            field is
            {
                JsonName: "externalVisualProviderConfig",
                DotNetType: nameof(DesignConversionExternalVisualProviderConfigInput),
                Required: false,
                MayCarryPrivateInput: true,
                ResponseMayEchoRawValue: false
            } &&
            field.Requirement.Contains("private endpoint", StringComparison.Ordinal) &&
            field.Requirement.Contains("credential", StringComparison.Ordinal) &&
            field.Requirement.Contains("artifact-root", StringComparison.Ordinal) &&
            field.Requirement.Contains("route never echoes raw config values or secrets", StringComparison.Ordinal));
        Assert.Contains(descriptor.RequestFields, field =>
            field is { JsonName: "metadata", Required: false, MayCarryPrivateInput: true, ResponseMayEchoRawValue: false });
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["improvement_loop_route_discoverable_by_contract"]);
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["improvement_loop_route_local_only"]);
    }

    [Fact]
    public void DescribeImprovementLoopRoute_DocumentsResponseAndEnvelopeWithoutRawPrivatePayloadPolicy()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeImprovementLoopRoute();

        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "contractStatus", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "stopReason", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "attemptCount", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "attempts", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.refs_or_hashes_only", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.raw_html_included", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.raw_css_included", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.raw_design_tree_included", Required: true });
        Assert.Contains(descriptor.ResponseFields, field => field is { JsonName: "boundaryMetadata.response_contains_raw_private_data", Required: true });
        Assert.Contains(descriptor.LocalCallerEnvelopeFields, field =>
            field is { JsonName: "rawPrivateResponseLeakageDetected", Required: true });
        Assert.Contains(descriptor.LocalCallerEnvelopeFields, field =>
            field is { JsonName: "externalCallerRepoTouched", Required: true });
        Assert.Equal(bool.TrueString, descriptor.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["raw_user_private_data_included"]);
        Assert.Equal(bool.FalseString, descriptor.BoundaryMetadata["response_contains_raw_private_data"]);
    }

    [Fact]
    public void DescribeImprovementLoopRoute_DocumentsStableValidationFailures()
    {
        var descriptor = DesignConversionModelServerCallerContract.DescribeImprovementLoopRoute();

        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: DesignConversionModelServerImprovementLoopHandler.CoordinatorNotConfiguredErrorCode, StatusCode: 503 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderCoordinatorResolverNotConfiguredErrorCode, StatusCode: 503 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderConfigInvalidErrorCode, StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: DesignConversionModelServerImprovementLoopHandler.ExternalVisualProviderCoordinatorResolveFailedErrorCode, StatusCode: 503 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "REQUEST_BODY_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "REQUEST_JSON_INVALID", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "IMPROVEMENT_LOOP_MAX_ATTEMPTS_REQUIRED", StatusCode: 400 });
        var maxAttemptsExceeded = Assert.Single(
            descriptor.ValidationFailures,
            failure => failure.ErrorCode == DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode);
        Assert.Equal(400, maxAttemptsExceeded.StatusCode);
        Assert.Contains(DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(), maxAttemptsExceeded.Trigger);
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "IMPROVEMENT_LOOP_INITIAL_PACKAGE_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_HTML_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_CSS_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_SCREENSHOT_OR_RENDER_REQUIRED", StatusCode: 400 });
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_VIEWPORT_REQUIRED", StatusCode: 400 });
        var improvementCandidateBudgetRequired = Assert.Single(
            descriptor.ValidationFailures,
            failure => failure.ErrorCode == DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        Assert.Equal(400, improvementCandidateBudgetRequired.StatusCode);
        Assert.Contains("maxImprovementCandidates", improvementCandidateBudgetRequired.Trigger);
        Assert.Contains(descriptor.ValidationFailures, failure =>
            failure is { ErrorCode: "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", StatusCode: 400 });
    }

    [Fact]
    public void DescribeImprovementLoopRoute_KeepsEvaluateDescriptorStableAndSeparate()
    {
        var evaluateDescriptor = DesignConversionModelServerCallerContract.Describe();
        var loopDescriptor = DesignConversionModelServerCallerContract.DescribeImprovementLoopRoute();

        Assert.Equal(DesignConversionModelServerCallerContract.SchemaVersion, evaluateDescriptor.SchemaVersion);
        Assert.Equal(DesignConversionModelServerHttpHandler.EndpointPath, evaluateDescriptor.EndpointPath);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, loopDescriptor.EndpointPath);
        Assert.NotEqual(evaluateDescriptor.SchemaVersion, loopDescriptor.SchemaVersion);
        Assert.NotEqual(evaluateDescriptor.Boundary, loopDescriptor.Boundary);
        Assert.DoesNotContain(evaluateDescriptor.RequestFields, field => field.JsonName == "loopId");
        Assert.DoesNotContain(evaluateDescriptor.ResponseFields, field => field.JsonName == "stopReason");
        Assert.Contains(evaluateDescriptor.RequestFields, field => field.JsonName == "html");
        Assert.Equal(
            RequiredRequestFieldNames(evaluateDescriptor.RequestFields),
            DesignConversionModelServerCallerContract.RequiredRequestFieldNames());
        Assert.Equal(bool.FalseString, loopDescriptor.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, loopDescriptor.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, loopDescriptor.BoundaryMetadata["socket_opened"]);
        Assert.Equal(bool.FalseString, loopDescriptor.BoundaryMetadata["deployment_verified"]);
        Assert.Equal(bool.FalseString, loopDescriptor.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, loopDescriptor.BoundaryMetadata["external_provider_called"]);
    }

    private static IReadOnlyList<string> RequiredRequestFieldNames(
        IReadOnlyList<DesignConversionModelServerCallerContractField> requestFields)
        => requestFields
            .Where(field => field.Required)
            .Select(field => field.JsonName)
            .ToArray();
}
