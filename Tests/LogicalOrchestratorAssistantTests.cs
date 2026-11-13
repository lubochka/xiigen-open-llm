// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using OrchestratorModel.Logical;

namespace LlmFromScratch.Tests;

public sealed class LogicalOrchestratorAssistantTests
{
    [Fact]
    public void LogicalOrchestratorAssistant_CreateDefault_ReturnsAssistant()
    {
        // Build the default assistant from the project-15 runtime wrapper.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage());

        // Default construction must succeed through the project result envelope.
        Assert.True(assistant.IsSuccess, assistant.ErrorMessage);

        // The default scenario must stay bound to the proven T34 transition flow.
        Assert.Equal(T34LogicalFlowAdapter.ScenarioId, assistant.Value!.DefaultScenarioId);
    }

    [Fact]
    public void LogicalOrchestratorAssistant_CreateDefaultWithAuthorityComposesExactTrainerAuthority()
    {
        var authority = new RecordingLogicalCapabilityTrainingAdmissionAuthority();

        var assistant = LogicalOrchestratorAssistant
            .CreateDefaultWithCapabilityTrainingAuthority(
            new AssistantNullSessionStorage(),
            authority);

        Assert.True(assistant.IsSuccess, assistant.ErrorMessage);
        var configured = assistant.Value!.ReadConfiguredOrchestrator();
        Assert.True(configured.IsSuccess, configured.ErrorMessage);
        var adaptive = Assert.IsType<AdaptiveLogicalOrchestrator>(configured.Value);
        var trainerField = typeof(AdaptiveLogicalOrchestrator).GetField(
            "_capabilityTrainer",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        Assert.NotNull(trainerField);
        var trainer = Assert.IsType<DefaultLogicalCapabilityTrainer>(
            trainerField.GetValue(adaptive));
        var authorityField = typeof(DefaultLogicalCapabilityTrainer).GetField(
            "_admissionAuthority",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        Assert.NotNull(authorityField);
        Assert.Same(authority, authorityField.GetValue(trainer));
        Assert.Equal(0, authority.LeaseCount);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunT34Request_ReturnsPassWithGaps()
    {
        // Build the default assistant with the T34 flow registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Run an explicit T34 request through the assistant surface.
        var result = await assistant.RunAsync("Check the technical-design to implementation transition.", T34LogicalFlowAdapter.ScenarioId);

        // The assistant run must succeed through the project result envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the T34 logical flow.
        Assert.Equal(T34LogicalFlowAdapter.FlowId, result.Value!.SelectedFlowId);

        // The verdict must match the accepted T34 proof.
        Assert.Equal("PASS_WITH_GAPS", result.Value.OverallVerdict);

        // The metrics must expose the T34 transition success signal.
        Assert.Equal("True", result.Value.Metrics["transition_success"]);

        // The final result must include the human-readable transition verdict.
        Assert.Contains("PASS WITH GAPS", result.Value.FinalHumanResult);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunSecurityVerification34_PreservesPassReprocessConcern()
    {
        // Build the default assistant with both the T34 and CB4 logical flows registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Run the S1 verified-control oracle scenario through the assistant route.
        var s1 = await assistant.RunAsync(BuildSecurityScenarioS1(), SecurityVerification34FlowAdapter.ScenarioId);

        // S1 must succeed through the assistant envelope.
        Assert.True(s1.IsSuccess, s1.ErrorMessage);

        // S1 must expose the stage marker followed by one exact shared CB4 source identity.
        Assert.Equal(
            ["CB4-STAGE-5-S1", "CB4-3.4-001"],
            s1.Value!.EvidenceIds);
        Assert.NotEmpty(s1.Value.EvidenceIds);
        Assert.Equal(
            s1.Value.EvidenceIds.Length,
            new HashSet<string>(s1.Value.EvidenceIds, StringComparer.Ordinal).Count);

        // S1 must select the registered security-verification flow.
        Assert.Equal(SecurityVerification34FlowAdapter.FlowId, s1.Value.SelectedFlowId);

        // S1 must return the pass verdict required by the OWASP oracle.
        Assert.Equal("PASS", s1.Value.OverallVerdict);

        // S1 must expose the internal-first no-external-call metric.
        Assert.Equal("0", s1.Value.Metrics["external_ai_call_count"]);

        // S1 must prove node-01 is now executed through a logical unit.
        Assert.Equal(SecuritySplitterUnit.Id, s1.Value.Metrics["node_01_unit_id"]);

        // S1 must preserve the CB4 splitter pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_01_source_pair_id"]);

        // S1 must keep the split in the security-and-authorization domain.
        Assert.Equal("security_and_authorization_model", s1.Value.Metrics["node_01_domain_label"]);

        // S1 must prove node-07 is now executed through a logical unit.
        Assert.Equal(SecurityArbiterSelectorUnit.Id, s1.Value.Metrics["node_07_unit_id"]);

        // S1 must route complete ASVS evidence to all four clean ASVS arbiters.
        Assert.Equal("ASVS-V6-Auth-Arbiter;ASVS-V7-Session-Arbiter;ASVS-V8-AuthZ-Arbiter;ASVS-V9-Token-Arbiter", s1.Value.Metrics["node_07_selected_arbiters"]);

        // S1 must preserve the CB4 selector pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_07_source_pair_id"]);

        // S1 must prove node-02 is now executed through a logical unit.
        Assert.Equal(SecurityTranslatorUnit.Id, s1.Value.Metrics["node_02_unit_id"]);

        // S1 must preserve the CB4 translator pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_02_source_pair_id"]);

        // S1 must prove node-03 is now executed through a logical unit.
        Assert.Equal(SecurityPlannerUnit.Id, s1.Value.Metrics["node_03_unit_id"]);

        // S1 must preserve the CB4 planner pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_03_source_pair_id"]);

        // S1 must prove node-04 is now executed through a logical unit.
        Assert.Equal(SecurityProcessorUnit.Id, s1.Value.Metrics["node_04_unit_id"]);

        // S1 must preserve the CB4 processor pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_04_source_pair_id"]);

        // S1 must prove node-08 is now executed through a logical unit.
        Assert.Equal(SecurityArbiterUnit.Id, s1.Value.Metrics["node_08_unit_id"]);

        // S1 must preserve the CB4 PASS pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_08_source_pair_id"]);

        // S1 must keep the node-08 confidence above the replacement floor.
        Assert.True(double.Parse(s1.Value.Metrics["node_08_unit_confidence"], System.Globalization.CultureInfo.InvariantCulture) >= 0.70);

        // S1 must prove node-05 is now executed through a logical unit.
        Assert.Equal(SecurityApplierUnit.Id, s1.Value.Metrics["node_05_unit_id"]);

        // S1 must preserve the CB4 applier pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_05_source_pair_id"]);

        // S1 must prove node-06 is now executed through a logical unit.
        Assert.Equal(SecurityResultTranslatorUnit.Id, s1.Value.Metrics["node_06_unit_id"]);

        // S1 must preserve the CB4 result-translator pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_06_source_pair_id"]);

        // S1 must prove node-09 is now executed through a logical unit.
        Assert.Equal(SecurityFormatterUnit.Id, s1.Value.Metrics["node_09_unit_id"]);

        // S1 must preserve the CB4 formatter pair anchor.
        Assert.Equal("CB4-3.4-001", s1.Value.Metrics["node_09_source_pair_id"]);

        // S1 must show the OWASP source attribution in the human result.
        Assert.Contains("OWASP ASVS 5.0.0", s1.Value.FinalHumanResult);

        // Run the S2 PKCE downgrade oracle scenario through the assistant route.
        var s2 = await assistant.RunAsync(BuildSecurityScenarioS2(), SecurityVerification34FlowAdapter.ScenarioId);

        // S2 must succeed through the assistant envelope.
        Assert.True(s2.IsSuccess, s2.ErrorMessage);

        // S2 must return the reprocess verdict required by the OWASP oracle.
        Assert.Equal("REPROCESS", s2.Value!.OverallVerdict);

        // S2 must name the ASVS PKCE requirement.
        Assert.Contains("ASVS 10.4.6", s2.Value.FinalHumanResult);

        // S2 must name S256 because plain PKCE is the unsafe path.
        Assert.Contains("S256", s2.Value.FinalHumanResult);

        // S2 must not repeat the submitted approval word in its final answer.
        Assert.DoesNotContain("compliant", s2.Value.FinalHumanResult, StringComparison.OrdinalIgnoreCase);

        // S2 must not repeat the submitted satisfaction word in its final answer.
        Assert.DoesNotContain("satisfies", s2.Value.FinalHumanResult, StringComparison.OrdinalIgnoreCase);

        // S2 must select the OAuth arbiter before node-08 judges the PKCE evidence.
        Assert.Equal("ASVS-V10-OAuth-Arbiter", s2.Value.Metrics["node_07_selected_arbiters"]);

        // S2 must preserve the CB4 splitter pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_01_source_pair_id"]);

        // S2 must preserve the CB4 selector pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_07_source_pair_id"]);

        // S2 must preserve the CB4 translator pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_02_source_pair_id"]);

        // S2 must preserve the CB4 planner pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_03_source_pair_id"]);

        // S2 must preserve the CB4 processor pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_04_source_pair_id"]);

        // S2 must preserve the CB4 REPROCESS pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_08_source_pair_id"]);

        // S2 must preserve the CB4 applier pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_05_source_pair_id"]);

        // S2 must preserve the CB4 result-translator pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_06_source_pair_id"]);

        // S2 must preserve the CB4 formatter pair anchor.
        Assert.Equal("CB4-3.4-005", s2.Value.Metrics["node_09_source_pair_id"]);

        // Run the S3 ASVS-versus-internal-standard oracle scenario through the assistant route.
        var s3 = await assistant.RunAsync(BuildSecurityScenarioS3(), SecurityVerification34FlowAdapter.ScenarioId);

        // S3 must succeed through the assistant envelope.
        Assert.True(s3.IsSuccess, s3.ErrorMessage);

        // S3 must return the concern verdict required by the OWASP oracle.
        Assert.Equal("CONCERN", s3.Value!.OverallVerdict);

        // S3 must name the ASVS session requirement.
        Assert.Contains("ASVS 7.3.1", s3.Value.FinalHumanResult);

        // S3 must keep the internal enterprise standard separate from ASVS.
        Assert.Contains("Internal standard ESS-SESSION-001", s3.Value.FinalHumanResult);

        // S3 must select the session arbiter before node-08 judges the policy distinction.
        Assert.Equal("ASVS-V7-Session-Arbiter", s3.Value.Metrics["node_07_selected_arbiters"]);

        // S3 must preserve the CB4 splitter pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_01_source_pair_id"]);

        // S3 must preserve the CB4 selector pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_07_source_pair_id"]);

        // S3 must preserve the CB4 translator pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_02_source_pair_id"]);

        // S3 must preserve the CB4 planner pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_03_source_pair_id"]);

        // S3 must preserve the CB4 processor pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_04_source_pair_id"]);

        // S3 must preserve the CB4 CONCERN pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_08_source_pair_id"]);

        // S3 must preserve the CB4 applier pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_05_source_pair_id"]);

        // S3 must preserve the CB4 result-translator pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_06_source_pair_id"]);

        // S3 must preserve the CB4 formatter pair anchor.
        Assert.Equal("CB4-3.4-009", s3.Value.Metrics["node_09_source_pair_id"]);

        // All three scenarios must report source attribution as visible.
        Assert.All(new[] { s1.Value, s2.Value, s3.Value }, result => Assert.Equal("True", result.Metrics["attribution_visible"]));
    }

    [Fact]
    public void SecurityVerification34FlowAdapter_EvidenceDeduplicationUsesOrdinalFirstOccurrence()
    {
        var method = Assert.IsAssignableFrom<System.Reflection.MethodInfo>(
            typeof(SecurityVerification34FlowAdapter).GetMethod(
                "BuildOrderedDistinctEvidenceIds",
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.DeclaredOnly));
        Assert.True(method.IsPrivate);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(string[]), method.ReturnType);
        var parameter = Assert.Single(method.GetParameters());
        Assert.Equal(typeof(string[]), parameter.ParameterType);
        Assert.True(parameter.IsDefined(typeof(ParamArrayAttribute), inherit: false));
        var source = new[] { "stage-marker", "Pair", "pair", "Pair", "source" };

        var result = Assert.IsType<string[]>(method.Invoke(null, [source]));

        Assert.Equal(["stage-marker", "Pair", "pair", "source"], result);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_AdaptiveSecurityPrompt_Selects34FlowWithoutScenarioId()
    {
        // Build the default assistant with adaptive routing enabled.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Run a security prompt without providing a scenario id.
        var result = await assistant.RunAsync(BuildSecurityScenarioS1());

        // The adaptive route must succeed through the assistant envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the sub-phase 3.4 security flow.
        Assert.Equal(SecurityVerification34FlowAdapter.FlowId, result.Value!.SelectedFlowId);

        // The security flow must still execute node-01 through its logical unit.
        Assert.Equal(SecuritySplitterUnit.Id, result.Value.Metrics["node_01_unit_id"]);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunSecurityVerification34_Ext31Scenarios_PreservesNewCoverage()
    {
        // Build the default assistant with the CB4 logical flow registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Run the S4 RFC 9700 public-client PKCE enforcement violation.
        var s4 = await assistant.RunAsync(BuildSecurityScenarioS4(), SecurityVerification34FlowAdapter.ScenarioId);

        // S4 must succeed through the assistant envelope.
        Assert.True(s4.IsSuccess, s4.ErrorMessage);

        // S4 must return the REPROCESS verdict from CB4-3.4-803.
        Assert.Equal("REPROCESS", s4.Value!.OverallVerdict);

        // S4 must preserve the splitter pair anchor from Action 3.
        Assert.Equal("CB4-3.4-803", s4.Value.Metrics["node_01_source_pair_id"]);

        // S4 must preserve the arbiter pair anchor from Action 3.
        Assert.Equal("CB4-3.4-803", s4.Value.Metrics["node_08_source_pair_id"]);

        // S4 must name the current PKCE enforcement requirement in the human result.
        Assert.Contains("RFC 9700 2.1.1", s4.Value.FinalHumanResult);

        // Run the S5 IP-only throttling defense-in-depth concern.
        var s5 = await assistant.RunAsync(BuildSecurityScenarioS5(), SecurityVerification34FlowAdapter.ScenarioId);

        // S5 must succeed through the assistant envelope.
        Assert.True(s5.IsSuccess, s5.ErrorMessage);

        // S5 must return CONCERN rather than escalating a SHOULD-level gap to REPROCESS.
        Assert.Equal("CONCERN", s5.Value!.OverallVerdict);

        // S5 must preserve the splitter pair anchor from Action 3.
        Assert.Equal("CB4-3.4-807", s5.Value.Metrics["node_01_source_pair_id"]);

        // S5 must preserve the arbiter pair anchor from Action 3.
        Assert.Equal("CB4-3.4-807", s5.Value.Metrics["node_08_source_pair_id"]);

        // S5 must name the per-client-id throttling gap in the human result.
        Assert.Contains("per-client-id throttling", s5.Value.FinalHumanResult, StringComparison.OrdinalIgnoreCase);

        // Run the S6 per-client-id throttling pass case.
        var s6 = await assistant.RunAsync(BuildSecurityScenarioS6(), SecurityVerification34FlowAdapter.ScenarioId);

        // S6 must succeed through the assistant envelope.
        Assert.True(s6.IsSuccess, s6.ErrorMessage);

        // S6 must return PASS for the correctly implemented graduated throttling controls.
        Assert.Equal("PASS", s6.Value!.OverallVerdict);

        // S6 must preserve the splitter pair anchor from Action 3.
        Assert.Equal("CB4-3.4-808", s6.Value.Metrics["node_01_source_pair_id"]);

        // S6 must preserve the arbiter pair anchor from Action 3.
        Assert.Equal("CB4-3.4-808", s6.Value.Metrics["node_08_source_pair_id"]);

        // S6 must expose the graduated response signal in the human result.
        Assert.Contains("Retry-After", s6.Value.FinalHumanResult);

        // All extended scenarios must stay internal-first.
        Assert.All(new[] { s4.Value, s5.Value, s6.Value }, result => Assert.Equal("0", result.Metrics["external_ai_call_count"]));
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunSecurityVerification34_Ext31CrossSourceScenarios_PreservesCombinedRules()
    {
        // Build the default assistant with the CB4 logical flow registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Run the S7 PKCE plus HS256 multi-party cross-source violation.
        var s7 = await assistant.RunAsync(BuildSecurityScenarioS7(), SecurityVerification34FlowAdapter.ScenarioId);

        // S7 must succeed through the assistant envelope.
        Assert.True(s7.IsSuccess, s7.ErrorMessage);

        // S7 must return REPROCESS because either MUST-level violation is enough to block advancement.
        Assert.Equal("REPROCESS", s7.Value!.OverallVerdict);

        // S7 must preserve the cross-source splitter pair anchor.
        Assert.Equal("CB4-3.4-819", s7.Value.Metrics["node_01_source_pair_id"]);

        // S7 must preserve the cross-source arbiter pair anchor.
        Assert.Equal("CB4-3.4-819", s7.Value.Metrics["node_08_source_pair_id"]);

        // S7 must keep both standards visible in the human result.
        Assert.Contains("RFC 8725 3.6", s7.Value.FinalHumanResult);

        // Run the S8 PKCE plus RS256 cross-source pass case.
        var s8 = await assistant.RunAsync(BuildSecurityScenarioS8(), SecurityVerification34FlowAdapter.ScenarioId);

        // S8 must succeed through the assistant envelope.
        Assert.True(s8.IsSuccess, s8.ErrorMessage);

        // S8 must return PASS because both cross-source controls are present.
        Assert.Equal("PASS", s8.Value!.OverallVerdict);

        // S8 must preserve the cross-source splitter pair anchor.
        Assert.Equal("CB4-3.4-820", s8.Value.Metrics["node_01_source_pair_id"]);

        // S8 must preserve the cross-source arbiter pair anchor.
        Assert.Equal("CB4-3.4-820", s8.Value.Metrics["node_08_source_pair_id"]);

        // S8 must name the asymmetric signing signal in the human result.
        Assert.Contains("RS256 asymmetric signing", s8.Value.FinalHumanResult);

        // Run the S9 IP-only throttling plus missing logout hint concern case.
        var s9 = await assistant.RunAsync(BuildSecurityScenarioS9(), SecurityVerification34FlowAdapter.ScenarioId);

        // S9 must succeed through the assistant envelope.
        Assert.True(s9.IsSuccess, s9.ErrorMessage);

        // S9 must return CONCERN instead of escalating two SHOULD-level gaps to REPROCESS.
        Assert.Equal("CONCERN", s9.Value!.OverallVerdict);

        // S9 must preserve the cross-source splitter pair anchor.
        Assert.Equal("CB4-3.4-821", s9.Value.Metrics["node_01_source_pair_id"]);

        // S9 must preserve the cross-source arbiter pair anchor.
        Assert.Equal("CB4-3.4-821", s9.Value.Metrics["node_08_source_pair_id"]);

        // S9 must name the concern graduation rule in the human result.
        Assert.Contains("N CONCERNs do not escalate to REPROCESS", s9.Value.FinalHumanResult);

        // All cross-source scenarios must stay internal-first.
        Assert.All(new[] { s7.Value, s8.Value, s9.Value }, result => Assert.Equal("0", result.Metrics["external_ai_call_count"]));
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunSecurityVerification34_Ext24RemainingScenarios_ClosesCrossSourceFile()
    {
        // Build the default assistant with the CB4 logical flow registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Run the S10 per-client throttling plus logout pass case.
        var s10 = await assistant.RunAsync(BuildSecurityScenarioS10(), SecurityVerification34FlowAdapter.ScenarioId);

        // S10 must succeed through the assistant envelope.
        Assert.True(s10.IsSuccess, s10.ErrorMessage);

        // S10 must return PASS for both accepted controls.
        Assert.Equal("PASS", s10.Value!.OverallVerdict);

        // S10 must preserve the CB4-3.4-822 source pair through splitter and arbiter.
        Assert.Equal("CB4-3.4-822", s10.Value.Metrics["node_01_source_pair_id"]);
        Assert.Equal("CB4-3.4-822", s10.Value.Metrics["node_08_source_pair_id"]);

        // S10 must keep the logout validation signal visible.
        Assert.Contains("id_token_hint required", s10.Value.FinalHumanResult);

        // Run the S11 HS256 plus ROPC double failure.
        var s11 = await assistant.RunAsync(BuildSecurityScenarioS11(), SecurityVerification34FlowAdapter.ScenarioId);

        // S11 must succeed through the assistant envelope.
        Assert.True(s11.IsSuccess, s11.ErrorMessage);

        // S11 must return REPROCESS for two independent MUST-level failures.
        Assert.Equal("REPROCESS", s11.Value!.OverallVerdict);

        // S11 must preserve the CB4-3.4-823 source pair through splitter and arbiter.
        Assert.Equal("CB4-3.4-823", s11.Value.Metrics["node_01_source_pair_id"]);
        Assert.Equal("CB4-3.4-823", s11.Value.Metrics["node_08_source_pair_id"]);

        // S11 must keep the ROPC violation visible.
        Assert.Contains("ROPC enabled", s11.Value.FinalHumanResult);

        // Run the S12 RS256 plus ROPC-disabled pass case.
        var s12 = await assistant.RunAsync(BuildSecurityScenarioS12(), SecurityVerification34FlowAdapter.ScenarioId);

        // S12 must succeed through the assistant envelope.
        Assert.True(s12.IsSuccess, s12.ErrorMessage);

        // S12 must return PASS because both replacement controls are present.
        Assert.Equal("PASS", s12.Value!.OverallVerdict);

        // S12 must preserve the CB4-3.4-824 source pair through splitter and arbiter.
        Assert.Equal("CB4-3.4-824", s12.Value.Metrics["node_01_source_pair_id"]);
        Assert.Equal("CB4-3.4-824", s12.Value.Metrics["node_08_source_pair_id"]);

        // S12 must keep the ROPC-disabled signal visible.
        Assert.Contains("ROPC disabled", s12.Value.FinalHumanResult);

        // Run the S13 PKCE failure caused by a SSDF PO.1 training gap.
        var s13 = await assistant.RunAsync(BuildSecurityScenarioS13(), SecurityVerification34FlowAdapter.ScenarioId);

        // S13 must succeed through the assistant envelope.
        Assert.True(s13.IsSuccess, s13.ErrorMessage);

        // S13 must return REPROCESS because the production MUST violation is present.
        Assert.Equal("REPROCESS", s13.Value!.OverallVerdict);

        // S13 must preserve the CB4-3.4-825 source pair through splitter and arbiter.
        Assert.Equal("CB4-3.4-825", s13.Value.Metrics["node_01_source_pair_id"]);
        Assert.Equal("CB4-3.4-825", s13.Value.Metrics["node_08_source_pair_id"]);

        // S13 must keep the training-gap cause visible.
        Assert.Contains("SSDF PO.1", s13.Value.FinalHumanResult);

        // Run the S14 HS256 plus no-exp compound token failure.
        var s14 = await assistant.RunAsync(BuildSecurityScenarioS14(), SecurityVerification34FlowAdapter.ScenarioId);

        // S14 must succeed through the assistant envelope.
        Assert.True(s14.IsSuccess, s14.ErrorMessage);

        // S14 must return REPROCESS for forgeable and non-expiring tokens.
        Assert.Equal("REPROCESS", s14.Value!.OverallVerdict);

        // S14 must preserve the CB4-3.4-826 source pair through splitter and arbiter.
        Assert.Equal("CB4-3.4-826", s14.Value.Metrics["node_01_source_pair_id"]);
        Assert.Equal("CB4-3.4-826", s14.Value.Metrics["node_08_source_pair_id"]);

        // S14 must keep the no-exp signal visible.
        Assert.Contains("no exp claim", s14.Value.FinalHumanResult);

        // Run the S15 IP-only concern absorbed by a no-PKCE REPROCESS finding.
        var s15 = await assistant.RunAsync(BuildSecurityScenarioS15(), SecurityVerification34FlowAdapter.ScenarioId);

        // S15 must succeed through the assistant envelope.
        Assert.True(s15.IsSuccess, s15.ErrorMessage);

        // S15 must return REPROCESS because the current PKCE MUST violation overrides the concern.
        Assert.Equal("REPROCESS", s15.Value!.OverallVerdict);

        // S15 must preserve the CB4-3.4-827 source pair through splitter and arbiter.
        Assert.Equal("CB4-3.4-827", s15.Value.Metrics["node_01_source_pair_id"]);
        Assert.Equal("CB4-3.4-827", s15.Value.Metrics["node_08_source_pair_id"]);

        // S15 must keep the graduation rule visible.
        Assert.Contains("REPROCESS overrides CONCERN", s15.Value.FinalHumanResult);

        // Run the S16 comprehensive ext31 pass case.
        var s16 = await assistant.RunAsync(BuildSecurityScenarioS16(), SecurityVerification34FlowAdapter.ScenarioId);

        // S16 must succeed through the assistant envelope.
        Assert.True(s16.IsSuccess, s16.ErrorMessage);

        // S16 must return PASS for all four ext31 domains.
        Assert.Equal("PASS", s16.Value!.OverallVerdict);

        // S16 must preserve the CB4-3.4-828 source pair through splitter and arbiter.
        Assert.Equal("CB4-3.4-828", s16.Value.Metrics["node_01_source_pair_id"]);
        Assert.Equal("CB4-3.4-828", s16.Value.Metrics["node_08_source_pair_id"]);

        // S16 must keep the four-domain coverage signal visible.
        Assert.Contains("all four ext31 domains", s16.Value.FinalHumanResult);

        // All remaining ext24 scenarios must stay internal-first.
        Assert.All(new[] { s10.Value, s11.Value, s12.Value, s13.Value, s14.Value, s15.Value, s16.Value }, result => Assert.Equal("0", result.Metrics["external_ai_call_count"]));
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunSecurityVerification34_Ext25PipelineChainScenarios_PreserveNineNodeTrace()
    {
        // Build the default assistant with the CB4 logical flow registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Run the eight ext25 full-pipeline traces through the executable logical flow.
        var results = new[]
        {
            // S17 is the ext25 AS-side PKCE REPROCESS trace.
            (Input: BuildSecurityScenarioS17(), PairId: "CB4-3.4-811", Verdict: "REPROCESS", Signal: "AS-side PKCE enforcement"),

            // S18 is the ext25 AS-side PKCE PASS trace.
            (Input: BuildSecurityScenarioS18(), PairId: "CB4-3.4-812", Verdict: "PASS", Signal: "RFC 7636 4.2"),

            // S19 is the ext25 HS256 multi-party REPROCESS trace.
            (Input: BuildSecurityScenarioS19(), PairId: "CB4-3.4-813", Verdict: "REPROCESS", Signal: "token forgery"),

            // S20 is the ext25 RS256 multi-party PASS trace.
            (Input: BuildSecurityScenarioS20(), PairId: "CB4-3.4-814", Verdict: "PASS", Signal: "private key exclusive"),

            // S21 is the ext25 IP-only throttling CONCERN trace.
            (Input: BuildSecurityScenarioS21(), PairId: "CB4-3.4-815", Verdict: "CONCERN", Signal: "high-volume argument addressable"),

            // S22 is the ext25 per-client throttling PASS trace.
            (Input: BuildSecurityScenarioS22(), PairId: "CB4-3.4-816", Verdict: "PASS", Signal: "per-client-id counter"),

            // S23 is the ext25 OIDC logout CONCERN trace.
            (Input: BuildSecurityScenarioS23(), PairId: "CB4-3.4-817", Verdict: "CONCERN", Signal: "harmless logout rejected"),

            // S24 is the ext25 OIDC logout PASS trace.
            (Input: BuildSecurityScenarioS24(), PairId: "CB4-3.4-818", Verdict: "PASS", Signal: "session forcing blocked")
        };

        // Verify every ext25 trace against the same per-node contract.
        foreach (var expected in results)
        {
            // Run the specific pipeline-chain scenario.
            var result = await assistant.RunAsync(expected.Input, SecurityVerification34FlowAdapter.ScenarioId);

            // Each pipeline-chain run must succeed through the assistant envelope.
            Assert.True(result.IsSuccess, result.ErrorMessage);

            // The final verdict must match the accepted ext25 trace.
            Assert.Equal(expected.Verdict, result.Value!.OverallVerdict);

            // All nine station source anchors must preserve the ext25 pair id.
            AssertSecurityNodeAnchors(result.Value, expected.PairId);

            // The human result must keep the trace-specific signal visible.
            Assert.Contains(expected.Signal, result.Value.FinalHumanResult);

            // The pipeline-chain checks must stay internal-first.
            Assert.Equal("0", result.Value.Metrics["external_ai_call_count"]);
        }
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunTopologyCommand_AddsDomain()
    {
        // Build the default assistant with the topology mutation flow configured.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Build a typed command instead of relying on natural-language intent routing.
        var command = BuildAddDomainCommand();

        // Run the typed topology command through the assistant.
        var result = await assistant.RunTopologyCommandAsync(command);

        // The topology command must succeed through the assistant envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the topology mutation flow.
        Assert.Equal(TopologyMutationFlow.FlowId, result.Value!.SelectedFlowId);

        // The verdict must name the applied topology mutation.
        Assert.Equal("DOMAIN_MUTATION_APPLIED", result.Value.OverallVerdict);

        // The metrics must expose the stored domain id.
        Assert.Equal("ui_ux_definition", result.Value.Metrics["stored_domain_id"]);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunTopologyCommand_AddsPhase()
    {
        // Build the default assistant with the topology mutation flow configured.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Build a typed phase command instead of relying on natural-language intent routing.
        var command = BuildAddPhaseCommand();

        // Run the typed topology command through the assistant.
        var result = await assistant.RunTopologyCommandAsync(command);

        // The topology command must succeed through the assistant envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the topology mutation flow.
        Assert.Equal(TopologyMutationFlow.FlowId, result.Value!.SelectedFlowId);

        // The verdict must name the applied phase mutation.
        Assert.Equal("PHASE_MUTATION_APPLIED", result.Value.OverallVerdict);

        // The metrics must expose the stored phase id.
        Assert.Equal("implementation", result.Value.Metrics["stored_phase_id"]);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunTopologyCommand_AddsSubPhase()
    {
        // Build the default assistant with the topology mutation flow configured.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Build a typed sub-phase command instead of relying on natural-language intent routing.
        var command = BuildAddSubPhaseCommand();

        // Run the typed topology command through the assistant.
        var result = await assistant.RunTopologyCommandAsync(command);

        // The topology command must succeed through the assistant envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the topology mutation flow.
        Assert.Equal(TopologyMutationFlow.FlowId, result.Value!.SelectedFlowId);

        // The verdict must name the applied sub-phase mutation.
        Assert.Equal("SUB_PHASE_MUTATION_APPLIED", result.Value.OverallVerdict);

        // The metrics must expose the stored sub-phase id.
        Assert.Equal("implementation_planning", result.Value.Metrics["stored_sub_phase_id"]);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunTopologyCommand_AddsTransition()
    {
        // Build the default assistant with the topology mutation flow configured.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Build a typed transition command instead of relying on natural-language intent routing.
        var command = BuildAddTransitionCommand();

        // Run the typed topology command through the assistant.
        var result = await assistant.RunTopologyCommandAsync(command);

        // The topology command must succeed through the assistant envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the topology mutation flow.
        Assert.Equal(TopologyMutationFlow.FlowId, result.Value!.SelectedFlowId);

        // The verdict must name the applied transition mutation.
        Assert.Equal("TRANSITION_MUTATION_APPLIED", result.Value.OverallVerdict);

        // The metrics must expose the stored transition id.
        Assert.Equal("technical-to-implementation", result.Value.Metrics["stored_transition_id"]);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunTopologyCommand_PreservesFlowFailureCode()
    {
        // Build the default assistant with the topology mutation flow configured.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Build a command without an audit reason.
        var command = new AddDomainCommand(BuildDomain(), " ");

        // Run the invalid command through the assistant.
        var result = await assistant.RunTopologyCommandAsync(command);

        // Invalid topology commands must fail through the assistant envelope.
        Assert.False(result.IsSuccess);

        // The unit-level error code must survive the assistant boundary.
        Assert.Equal("EMPTY_REASON", result.ErrorCode);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunTopologyCommand_MissingFlowFailsCleanly()
    {
        // Build an assistant without the topology mutation flow.
        var assistant = new LogicalOrchestratorAssistant(new AssistantNullSessionStorage(), null);

        // Run a valid topology command against the incomplete assistant.
        var result = await assistant.RunTopologyCommandAsync(BuildAddDomainCommand());

        // Missing topology flow must fail through the assistant envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing topology flow.
        Assert.Equal("TOPOLOGY_FLOW_NOT_CONFIGURED", result.ErrorCode);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_RunTopologyCommand_CancelledRequestFailsCleanly()
    {
        // Build the default assistant with the topology mutation flow configured.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Create a token that is already cancelled.
        using var source = new CancellationTokenSource();

        // Cancel before calling the assistant.
        source.Cancel();

        // Run a cancelled topology command through the assistant.
        var result = await assistant.RunTopologyCommandAsync(BuildAddDomainCommand(), cancellationToken: source.Token);

        // Cancellation must fail through the assistant envelope.
        Assert.False(result.IsSuccess);

        // The failure code must name cancellation explicitly.
        Assert.Equal("CANCELLED", result.ErrorCode);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_EmptyRequest_FailsCleanly()
    {
        // Build the default assistant with the T34 flow registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Run an empty request through the assistant surface.
        var result = await assistant.RunAsync(" ");

        // Empty requests must fail through the project result envelope.
        Assert.False(result.IsSuccess);

        // The failure code must identify input validation rather than orchestration failure.
        Assert.Equal("EMPTY_REQUEST", result.ErrorCode);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_UnknownScenario_FailsCleanly()
    {
        // Build the default assistant without registering the unknown scenario.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Request a scenario that the flow registry did not bind.
        var result = await assistant.RunAsync("Run a missing scenario.", "unknown-scenario");

        // Unknown scenarios must fail through the project result envelope.
        Assert.False(result.IsSuccess);

        // The assistant should classify this as an orchestrator selection failure.
        Assert.Equal("ORCHESTRATOR_FAILED", result.ErrorCode);

        // The failure message should name the missing scenario.
        Assert.Contains("unknown-scenario", result.ErrorMessage);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_CustomFlags_AreApplied()
    {
        // Build the default assistant with the T34 flow registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Disable one experiment-control flag for this caller-level inspection.
        var flags = new LogicalExecutionFlags { UseDeterministicFixtureBaseline = true };

        // Run the assistant with custom flags through an explicit scenario.
        var result = await assistant.RunAsync("Run with explicit flags.", T34LogicalFlowAdapter.ScenarioId, flags);

        // The run must still succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The applied flags must preserve the caller's experiment-control value.
        Assert.True(result.Value!.AppliedFlags.UseDeterministicFixtureBaseline);

        // Safety flags must remain enabled in the custom flag set.
        Assert.True(result.Value.AppliedFlags.UseArbiterAuthority);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_WDA00TopologyBootstrap_RegistersWholeDevelopmentLifecycle()
    {
        // Create a registry that the test can inspect after assistant-routed mutations.
        var topologyRegistry = new InMemoryLogicalTopologyRegistry();

        // Create the domain mutation unit so the flow is complete even though WDA-00 does not add domains.
        var addDomainUnit = new AddDomainTopologyMutationUnit(topologyRegistry);

        // Create the phase mutation unit that WDA-00 uses for seven phase commands.
        var addPhaseUnit = new AddPhaseTopologyMutationUnit(topologyRegistry);

        // Create the sub-phase mutation unit that WDA-00 uses for thirty-one sub-phase commands.
        var addSubPhaseUnit = new AddSubPhaseTopologyMutationUnit(topologyRegistry);

        // Create the transition mutation unit that WDA-00 uses for six transition commands.
        var addTransitionUnit = new AddTransitionTopologyMutationUnit(topologyRegistry);

        // Build the topology mutation flow from the same unit surface used by production-adjacent assistant routing.
        var topologyMutationFlow = new TopologyMutationFlow(addDomainUnit, addPhaseUnit, addSubPhaseUnit, addTransitionUnit);

        // Build an assistant that routes typed topology commands through the topology mutation flow.
        var assistant = new LogicalOrchestratorAssistant(new AssistantNullSessionStorage(), null, topologyMutationFlow: topologyMutationFlow);

        // Create the canonical WDA-00 command list with seven phases, thirty-one sub-phases, and six transitions.
        var commands = new List<LogicalTopologyCommand>();

        // Add the seven lifecycle phase commands to the command list.
        commands.AddRange(BuildWda00PhaseCommands());

        // Add the thirty-one lifecycle sub-phase commands to the command list.
        commands.AddRange(BuildWda00SubPhaseCommands());

        // Add the six adjacent phase transition commands to the command list.
        commands.AddRange(BuildWda00TransitionCommands());

        // Track the number of successful typed commands executed through the assistant route.
        var successfulCommandCount = 0;

        // Execute every topology bootstrap command through the assistant.
        foreach (var command in commands)
        {
            // Run this typed topology command without invoking natural-language intent classification.
            var result = await assistant.RunTopologyCommandAsync(command);

            // Every bootstrap command must succeed through the shared operation envelope.
            Assert.True(result.IsSuccess, result.ErrorMessage);

            // Every bootstrap command must select the topology mutation flow.
            Assert.Equal(TopologyMutationFlow.FlowId, result.Value!.SelectedFlowId);

            // Count the successful typed command for the WDA-00 command metric.
            successfulCommandCount++;
        }

        // Capture the sub-phase count before the edit probe runs.
        var subPhasesBeforeEdit = topologyRegistry.ListSubPhases();

        // The pre-edit sub-phase list must be available through the registry envelope.
        Assert.True(subPhasesBeforeEdit.IsSuccess, subPhasesBeforeEdit.ErrorMessage);

        // The pre-edit topology must contain the canonical thirty-one sub-phases.
        Assert.Equal(31, subPhasesBeforeEdit.Value!.Count);

        // Build the edit probe that updates documentation without changing its identifier.
        var editCommand = BuildWda00DocumentationEditCommand();

        // Run the edit probe through the assistant route.
        var editResult = await assistant.RunTopologyCommandAsync(editCommand);

        // The edit probe must succeed through the shared operation envelope.
        Assert.True(editResult.IsSuccess, editResult.ErrorMessage);

        // The edit probe must still select the topology mutation flow.
        Assert.Equal(TopologyMutationFlow.FlowId, editResult.Value!.SelectedFlowId);

        // Count the successful edit probe as the forty-fifth topology command.
        successfulCommandCount++;

        // Resolve the stored phase entries after all mutations.
        var phases = topologyRegistry.ListPhases();

        // The phase list must be available through the registry envelope.
        Assert.True(phases.IsSuccess, phases.ErrorMessage);

        // Resolve the stored sub-phase entries after all mutations.
        var subPhases = topologyRegistry.ListSubPhases();

        // The sub-phase list must be available through the registry envelope.
        Assert.True(subPhases.IsSuccess, subPhases.ErrorMessage);

        // Resolve the stored transition entries after all mutations.
        var transitions = topologyRegistry.ListTransitions();

        // The transition list must be available through the registry envelope.
        Assert.True(transitions.IsSuccess, transitions.ErrorMessage);

        // Materialize the phase list after the success assertion so nullable flow analysis knows it is present.
        var phaseEntries = phases.Value!;

        // Materialize the sub-phase list after the success assertion so nullable flow analysis knows it is present.
        var subPhaseEntries = subPhases.Value!;

        // Materialize the transition list after the success assertion so nullable flow analysis knows it is present.
        var transitionEntries = transitions.Value!;

        // Materialize phase ids for parent and transition endpoint validation.
        var phaseIds = phaseEntries.Select(entry => entry.Phase.PhaseId).ToHashSet(StringComparer.Ordinal);

        // Count sub-phases whose parent phase is missing from the topology.
        var subPhasesWithoutParent = subPhaseEntries.Count(entry => !phaseIds.Contains(entry.SubPhase.PhaseId));

        // Count transitions whose source phase is missing from the topology.
        var transitionsWithMissingSource = transitionEntries.Count(entry => !phaseIds.Contains(entry.Transition.FromTopologyId));

        // Count transitions whose target phase is missing from the topology.
        var transitionsWithMissingTarget = transitionEntries.Count(entry => !phaseIds.Contains(entry.Transition.ToTopologyId));

        // Count duplicate phase identifiers in the stored topology.
        var duplicatePhaseIdentifiers = CountDuplicates(phaseEntries.Select(entry => entry.Phase.PhaseId));

        // Count duplicate sub-phase identifiers in the stored topology.
        var duplicateSubPhaseIdentifiers = CountDuplicates(subPhaseEntries.Select(entry => entry.SubPhase.SubPhaseId));

        // Count duplicate transition identifiers in the stored topology.
        var duplicateTransitionIdentifiers = CountDuplicates(transitionEntries.Select(entry => entry.Transition.TransitionId));

        // Resolve the edited documentation sub-phase by its stable identifier.
        var editedDocumentation = topologyRegistry.ResolveSubPhase("4.5-documentation");

        // The edited documentation sub-phase must still exist under the same identifier.
        Assert.True(editedDocumentation.IsSuccess, editedDocumentation.ErrorMessage);

        // Build a human-readable topology result from the stored registry state.
        var humanResult = BuildWda00HumanResult(phaseEntries, subPhaseEntries, transitionEntries);

        // Assert the WDA-00 command count metric.
        Assert.Equal(45, successfulCommandCount);

        // Assert the WDA-00 phase count metric.
        Assert.Equal(7, phaseEntries.Count);

        // Assert the WDA-00 sub-phase count metric.
        Assert.Equal(31, subPhaseEntries.Count);

        // Assert the WDA-00 transition count metric.
        Assert.Equal(6, transitionEntries.Count);

        // Assert that no phase id was duplicated.
        Assert.Equal(0, duplicatePhaseIdentifiers);

        // Assert that no sub-phase id was duplicated.
        Assert.Equal(0, duplicateSubPhaseIdentifiers);

        // Assert that no transition id was duplicated.
        Assert.Equal(0, duplicateTransitionIdentifiers);

        // Assert that every sub-phase references an existing phase.
        Assert.Equal(0, subPhasesWithoutParent);

        // Assert that every transition source references an existing phase.
        Assert.Equal(0, transitionsWithMissingSource);

        // Assert that every transition target references an existing phase.
        Assert.Equal(0, transitionsWithMissingTarget);

        // Assert that the edit operation preserved the sub-phase count.
        Assert.Equal(subPhasesBeforeEdit.Value.Count, subPhaseEntries.Count);

        // Assert that the edit operation updated the existing documentation sub-phase content.
        Assert.Contains("decision trace", editedDocumentation.Value!.SubPhase.Purpose, StringComparison.OrdinalIgnoreCase);

        // Assert that the human result includes all seven phase sections.
        Assert.Equal(7, CountHumanPhaseSections(humanResult));

        // Assert that the human result names the canonical thirty-one-sub-phase correction.
        Assert.Contains("31 sub-phases", humanResult);

        // Assert that the human result names WDA-01 as the next experiment.
        Assert.Contains("WDA-01", humanResult);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_CancelledRequest_FailsCleanly()
    {
        // Build the default assistant with the T34 flow registered.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new AssistantNullSessionStorage()).Value!;

        // Create a token that is already cancelled.
        using var source = new CancellationTokenSource();

        // Cancel before calling the assistant.
        source.Cancel();

        // Run a cancelled request through the assistant surface.
        var result = await assistant.RunAsync("Run T34.", cancellationToken: source.Token);

        // Cancellation must fail through the project result envelope.
        Assert.False(result.IsSuccess);

        // The failure code must name cancellation explicitly.
        Assert.Equal("CANCELLED", result.ErrorCode);
    }

    private static AddDomainCommand BuildAddDomainCommand()
    {
        // Build a valid add-domain command for assistant topology tests.
        return new AddDomainCommand(BuildDomain(), "Register UI and UX definition as a logical topology domain.");
    }

    private static LogicalDomain BuildDomain()
    {
        // Build a valid UI/UX definition domain.
        return new LogicalDomain(
            "ui_ux_definition",
            "UI UX Definition",
            "Owns user experience definition and design orchestration.",
            ["DECLARE", "VALIDATE_OP"],
            ["development-phases-guide"]);
    }

    private static AddPhaseCommand BuildAddPhaseCommand()
    {
        // Build a valid add-phase command for assistant topology tests.
        return new AddPhaseCommand(BuildPhase(), "Register implementation as a logical topology phase.");
    }

    private static LogicalPhase BuildPhase()
    {
        // Build a valid implementation phase.
        return new LogicalPhase(
            "implementation",
            "Implementation",
            "Owns implementation orchestration.",
            ["ui_ux_definition", "implementation"],
            ["CREATE", "ASSIGN"],
            ["development-phases-guide"]);
    }

    private static AddSubPhaseCommand BuildAddSubPhaseCommand()
    {
        // Build a valid add-sub-phase command for assistant topology tests.
        return new AddSubPhaseCommand(BuildSubPhase(), "Register implementation planning as a logical topology sub-phase.");
    }

    private static LogicalSubPhase BuildSubPhase()
    {
        // Build a valid implementation planning sub-phase.
        return new LogicalSubPhase(
            "implementation_planning",
            "implementation",
            "Implementation Planning",
            "Owns implementation planning orchestration.",
            ["implementation"],
            ["CREATE", "ITERATE"],
            ["development-phases-guide"]);
    }

    private static AddTransitionCommand BuildAddTransitionCommand()
    {
        // Build a valid add-transition command for assistant topology tests.
        return new AddTransitionCommand(BuildTransition(), "Register technical design to implementation as a logical topology transition.");
    }

    private static LogicalTransition BuildTransition()
    {
        // Build a valid technical design to implementation transition.
        return new LogicalTransition(
            "technical-to-implementation",
            "phase",
            "technical_design",
            "phase",
            "implementation",
            "Carries technical design decisions into implementation.",
            ["DECLARE", "RETURN"],
            ["PRESERVE_CONSTRAINTS"],
            ["development-phases-guide"]);
    }

    private static AddPhaseCommand[] BuildWda00PhaseCommands()
    {
        // Return the seven canonical development lifecycle phase commands.
        return
        [
            // Phase 1 owns UI and UX definition work.
            BuildWda00PhaseCommand("phase-1-ui-ux-definition", "UI/UX Definition", "Owns experience definition, design rules, and interaction boundaries.", ["ui_ux"], ["DECLARE", "VALIDATE_OP"]),

            // Phase 2 owns functional specification work.
            BuildWda00PhaseCommand("phase-2-functional-specification", "Functional Specification", "Owns user stories, acceptance criteria, business rules, and data boundaries.", ["functional_spec"], ["DECLARE", "RETURN"]),

            // Phase 3 owns technical design work.
            BuildWda00PhaseCommand("phase-3-technical-design", "Technical Design", "Owns architecture, schemas, APIs, security, and infrastructure design.", ["technical_design"], ["DECLARE", "CREATE"]),

            // Phase 4 owns implementation work.
            BuildWda00PhaseCommand("phase-4-implementation", "Implementation", "Owns backend, frontend, connector, migration, and documentation implementation.", ["implementation"], ["CREATE_OP", "HANDLE_ERROR"]),

            // Phase 5 owns testing work.
            BuildWda00PhaseCommand("phase-5-testing", "Testing", "Owns unit, contract, integration, end-to-end, and performance testing.", ["testing"], ["VALIDATE_OP", "FAIL"]),

            // Phase 6 owns QA review work.
            BuildWda00PhaseCommand("phase-6-qa", "QA", "Owns functional review, regression verification, security review, and accessibility audit.", ["qa"], ["VALIDATE_OP", "RETURN"]),

            // Phase 7 owns QA automation work.
            BuildWda00PhaseCommand("phase-7-qa-automation", "QA Automation", "Owns automated scenarios, regression maintenance, CI gates, and coverage reporting.", ["qa_automation"], ["CREATE", "ITERATE"])
        ];
    }

    private static AddPhaseCommand BuildWda00PhaseCommand(string phaseId, string name, string purpose, string[] domainIds, string[] predicateIds)
    {
        // Build one phase record from the canonical lifecycle source packet.
        var phase = new LogicalPhase(phaseId, name, purpose, domainIds, predicateIds, ["whole-development-area-experiment-set"]);

        // Wrap the phase in a typed topology command with an audit reason.
        return new AddPhaseCommand(phase, $"Register {name} as a whole-development lifecycle phase.");
    }

    private static AddSubPhaseCommand[] BuildWda00SubPhaseCommands()
    {
        // Return the thirty-one canonical sub-phase commands grouped by parent phase.
        return
        [
            // Phase 1 sub-phases cover UI and UX definition.
            BuildWda00SubPhaseCommand("1.1-design-system-and-tokens", "phase-1-ui-ux-definition", "Design system and tokens", "Owns token, typography, color, spacing, and component rules.", ["ui_ux"], ["VALIDATE_OP", "GET_OP", "RETURN"]),
            BuildWda00SubPhaseCommand("1.2-screen-and-layout-structure", "phase-1-ui-ux-definition", "Screen and layout structure", "Owns screen inventory, layout hierarchy, and responsive structure.", ["ui_ux"], ["CREATE", "ITERATE", "RETURN"]),
            BuildWda00SubPhaseCommand("1.3-interaction-model-and-state-transitions", "phase-1-ui-ux-definition", "Interaction model and state transitions", "Owns interaction states, gestures, loading states, and error states.", ["ui_ux"], ["BRANCH", "UPDATE_OP", "RETURN"]),
            BuildWda00SubPhaseCommand("1.4-accessibility-and-compliance", "phase-1-ui-ux-definition", "Accessibility and compliance", "Owns WCAG, keyboard, ARIA, contrast, and compliance requirements.", ["ui_ux"], ["VALIDATE_OP", "FAIL", "RETURN"]),
            BuildWda00SubPhaseCommand("1.5-motion-and-animation", "phase-1-ui-ux-definition", "Motion and animation", "Owns transition timing, animation boundaries, and reduced-motion rules.", ["ui_ux"], ["PROCESS_OP", "BRANCH", "RETURN"]),

            // Phase 2 sub-phases cover functional specification.
            BuildWda00SubPhaseCommand("2.1-user-stories-and-acceptance-criteria", "phase-2-functional-specification", "User stories and acceptance criteria", "Owns user stories, actor goals, and acceptance criteria.", ["functional_spec"], ["DECLARE", "VALIDATE_OP", "RETURN"]),
            BuildWda00SubPhaseCommand("2.2-business-rules", "phase-2-functional-specification", "Business rules", "Owns allowed outcomes, rejected outcomes, and business constraints.", ["functional_spec"], ["BRANCH", "FAIL", "VALIDATE_OP"]),
            BuildWda00SubPhaseCommand("2.3-integration-point-specification", "phase-2-functional-specification", "Integration point specification", "Owns integration handoffs, provider boundaries, and external dependencies.", ["functional_spec"], ["SEND_OP", "AUTH_OP", "HANDLE_ERROR"]),
            BuildWda00SubPhaseCommand("2.4-data-model-boundaries", "phase-2-functional-specification", "Data model boundaries", "Owns ownership, lifecycle, privacy, and data boundary requirements.", ["functional_spec"], ["CREATE", "GET_OP", "VALIDATE_OP"]),

            // Phase 3 sub-phases cover technical design.
            BuildWda00SubPhaseCommand("3.1-architecture-and-component-topology", "phase-3-technical-design", "Architecture and component topology", "Owns component topology, service boundaries, and runtime flow decisions.", ["technical_design"], ["DECLARE", "CREATE", "RETURN"]),
            BuildWda00SubPhaseCommand("3.2-data-schema-and-migration-design", "phase-3-technical-design", "Data schema and migration design", "Owns schema, migration, backfill, rollback, and compatibility design.", ["technical_design"], ["CREATE_OP", "UPDATE_OP", "VALIDATE_OP"]),
            BuildWda00SubPhaseCommand("3.3-api-contract-definition", "phase-3-technical-design", "API contract definition", "Owns request, response, auth, versioning, and error contracts.", ["technical_design"], ["DECLARE", "SEND_OP", "AUTH_OP"]),
            BuildWda00SubPhaseCommand("3.4-security-and-authorization-model", "phase-3-technical-design", "Security and authorization model", "Owns permissions, identity, token handling, and rejection rules.", ["technical_design"], ["AUTH_OP", "VALIDATE_OP", "FAIL"]),
            BuildWda00SubPhaseCommand("3.5-infrastructure-and-deployment-topology", "phase-3-technical-design", "Infrastructure and deployment topology", "Owns environments, deployment, observability, rollback, and scaling design.", ["technical_design"], ["CREATE", "PROCESS_OP", "HANDLE_ERROR"]),

            // Phase 4 sub-phases cover implementation.
            BuildWda00SubPhaseCommand("4.1-backend-services", "phase-4-implementation", "Backend services", "Owns backend service implementation, interfaces, and error handling.", ["implementation"], ["CALL", "CREATE_OP", "HANDLE_ERROR"]),
            BuildWda00SubPhaseCommand("4.2-frontend-and-client-rendering", "phase-4-implementation", "Frontend and client rendering", "Owns UI implementation, rendering states, and client data binding.", ["implementation"], ["GET_OP", "UPDATE_OP", "RETURN"]),
            BuildWda00SubPhaseCommand("4.3-integration-and-provider-connectors", "phase-4-implementation", "Integration and provider connectors", "Owns connector implementation, provider authentication, and failure paths.", ["implementation"], ["SEND_OP", "AUTH_OP", "HANDLE_ERROR"]),
            BuildWda00SubPhaseCommand("4.4-data-migration-execution", "phase-4-implementation", "Data migration execution", "Owns migration execution, batching, rollback, and logging.", ["implementation"], ["ITERATE", "UPDATE_OP", "LOG_OP"]),
            BuildWda00SubPhaseCommand("4.5-documentation", "phase-4-implementation", "Documentation", "Owns implementation documentation updates and audience mapping.", ["implementation"], ["CONVERT_OP", "RETURN", "VALIDATE_OP"]),

            // Phase 5 sub-phases cover testing.
            BuildWda00SubPhaseCommand("5.1-unit-and-contract-tests", "phase-5-testing", "Unit and contract tests", "Owns unit tests, contract tests, and expected assertions.", ["testing"], ["VALIDATE_OP", "FAIL", "RETURN"]),
            BuildWda00SubPhaseCommand("5.2-integration-tests", "phase-5-testing", "Integration tests", "Owns integration boundaries and service failure simulation.", ["testing"], ["CALL", "SEND_OP", "HANDLE_ERROR"]),
            BuildWda00SubPhaseCommand("5.3-end-to-end-scenario-tests", "phase-5-testing", "End-to-end scenario tests", "Owns user journeys, state verification, and full scenario traces.", ["testing"], ["ITERATE", "VALIDATE_OP", "RETURN"]),
            BuildWda00SubPhaseCommand("5.4-performance-and-load-tests", "phase-5-testing", "Performance and load tests", "Owns load, latency, capacity, budget, and degradation testing.", ["testing"], ["PROCESS_OP", "LOG_OP", "FAIL"]),

            // Phase 6 sub-phases cover QA.
            BuildWda00SubPhaseCommand("6.1-functional-review", "phase-6-qa", "Functional review", "Owns QA review against acceptance criteria.", ["qa"], ["VALIDATE_OP", "RETURN", "FAIL"]),
            BuildWda00SubPhaseCommand("6.2-regression-coverage-verification", "phase-6-qa", "Regression coverage verification", "Owns regression review for changed areas and preserved behaviors.", ["qa"], ["ITERATE", "VALIDATE_OP", "LOG_OP"]),
            BuildWda00SubPhaseCommand("6.3-security-and-compliance-review", "phase-6-qa", "Security and compliance review", "Owns compliance review, security review, and audit obligations.", ["qa"], ["AUTH_OP", "FAIL", "VALIDATE_OP"]),
            BuildWda00SubPhaseCommand("6.4-accessibility-audit", "phase-6-qa", "Accessibility audit", "Owns keyboard, screen reader, and contrast audit checks.", ["qa"], ["VALIDATE_OP", "FAIL", "RETURN"]),

            // Phase 7 sub-phases cover QA automation.
            BuildWda00SubPhaseCommand("7.1-test-scenario-automation", "phase-7-qa-automation", "Test scenario automation", "Owns automation scenario plans, steps, and fixtures.", ["qa_automation"], ["CREATE", "ITERATE", "CALL"]),
            BuildWda00SubPhaseCommand("7.2-regression-suite-maintenance", "phase-7-qa-automation", "Regression suite maintenance", "Owns regression suite updates, grouping, deletion, and ownership.", ["qa_automation"], ["UPDATE_OP", "ITERATE", "LOG_OP"]),
            BuildWda00SubPhaseCommand("7.3-ci-cd-gate-configuration", "phase-7-qa-automation", "CI/CD gate configuration", "Owns pipeline gates, failure thresholds, and deployment blocking rules.", ["qa_automation"], ["BRANCH", "FAIL", "SEND_OP"]),
            BuildWda00SubPhaseCommand("7.4-coverage-and-reporting", "phase-7-qa-automation", "Coverage and reporting", "Owns coverage reports, dashboards, metrics, and evidence publishing.", ["qa_automation"], ["GET_OP", "CONVERT_OP", "RETURN"])
        ];
    }

    private static AddSubPhaseCommand BuildWda00SubPhaseCommand(string subPhaseId, string phaseId, string name, string purpose, string[] domainIds, string[] predicateIds)
    {
        // Build one sub-phase record from the canonical lifecycle source packet.
        var subPhase = new LogicalSubPhase(subPhaseId, phaseId, name, purpose, domainIds, predicateIds, ["whole-development-area-experiment-set"]);

        // Wrap the sub-phase in a typed topology command with an audit reason.
        return new AddSubPhaseCommand(subPhase, $"Register {name} as a whole-development lifecycle sub-phase.");
    }

    private static AddTransitionCommand[] BuildWda00TransitionCommands()
    {
        // Return the six adjacent phase transition commands.
        return
        [
            // The first transition carries UI and UX decisions into functional specification.
            BuildWda00TransitionCommand("transition-1-to-2-ui-ux-to-functional-specification", "phase-1-ui-ux-definition", "phase-2-functional-specification", "Carries design decisions into functional specification."),

            // The second transition carries requirements into technical design.
            BuildWda00TransitionCommand("transition-2-to-3-functional-specification-to-technical-design", "phase-2-functional-specification", "phase-3-technical-design", "Carries functional requirements into technical design."),

            // The third transition carries technical decisions into implementation.
            BuildWda00TransitionCommand("transition-3-to-4-technical-design-to-implementation", "phase-3-technical-design", "phase-4-implementation", "Carries technical design decisions into implementation."),

            // The fourth transition carries implementation evidence into testing.
            BuildWda00TransitionCommand("transition-4-to-5-implementation-to-testing", "phase-4-implementation", "phase-5-testing", "Carries implementation evidence into testing."),

            // The fifth transition carries test evidence into QA.
            BuildWda00TransitionCommand("transition-5-to-6-testing-to-qa", "phase-5-testing", "phase-6-qa", "Carries test evidence into QA review."),

            // The sixth transition carries QA verdicts into QA automation.
            BuildWda00TransitionCommand("transition-6-to-7-qa-to-qa-automation", "phase-6-qa", "phase-7-qa-automation", "Carries QA verdicts into automation gates.")
        ];
    }

    private static AddTransitionCommand BuildWda00TransitionCommand(string transitionId, string fromPhaseId, string toPhaseId, string purpose)
    {
        // Build one transition record from the canonical lifecycle source packet.
        var transition = new LogicalTransition(transitionId, "phase", fromPhaseId, "phase", toPhaseId, purpose, ["DECLARE", "RETURN"], ["PRESERVE_CONSTRAINTS"], ["whole-development-area-experiment-set"]);

        // Wrap the transition in a typed topology command with an audit reason.
        return new AddTransitionCommand(transition, $"Register transition from {fromPhaseId} to {toPhaseId}.");
    }

    private static AddSubPhaseCommand BuildWda00DocumentationEditCommand()
    {
        // Build an edit probe that keeps the documentation sub-phase id stable.
        return BuildWda00SubPhaseCommand("4.5-documentation", "phase-4-implementation", "Documentation", "Owns implementation documentation updates, audience mapping, and decision trace publication.", ["implementation"], ["CONVERT_OP", "RETURN", "VALIDATE_OP"]);
    }

    private static int CountDuplicates(IEnumerable<string> identifiers)
    {
        // Count stable identifiers whose occurrence count is greater than one.
        return identifiers.GroupBy(identifier => identifier, StringComparer.Ordinal).Count(group => group.Count() > 1);
    }

    private static string BuildSecurityScenarioS1()
    {
        // Return the OWASP S1 verified-control oracle input.
        return """
            Our backend authentication service has verified controls in production: password minimum length 15 characters, OTP expiry 10 minutes, TOTP window 30 seconds, session idle timeout 20 minutes with risk analysis RA-AUTH-003, re-authentication before username email password or MFA factor changes, JWT validation with RS256 only, none algorithm rejected, exp nbf and aud validated, and RBAC plus per-resource ownership checks on all API endpoints. Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4?
            """;
    }

    private static string BuildSecurityScenarioS2()
    {
        // Return the OWASP S2 PKCE downgrade oracle input.
        return """
            Our single-page application uses OAuth 2.0 authorization code flow with PKCE. Due to authorization server compatibility, we are using code_challenge_method=plain. The authorization server accepts both S256 and plain. Our security review team has marked this implementation as satisfying ASVS 10.4.6 compliance. Please confirm whether this assessment is correct.
            """;
    }

    private static string BuildSecurityScenarioS3()
    {
        // Return the OWASP S3 internal-standard distinction oracle input.
        return """
            Our session management implementation uses a 20-minute idle timeout with risk analysis RA-AUTH-003. However, enterprise security standard ESS-SESSION-001 requires exactly 30 minutes to align with enterprise SSO token lifetime. ASVS 7.3.1 says to enforce re-authentication per documented risk analysis. Is there a compliance issue with OWASP ASVS 5.0.0?
            """;
    }

    private static string BuildSecurityScenarioS4()
    {
        // Return the CB4 Action 3 S4 RFC 9700 PKCE enforcement violation input.
        return """
            Authorization server does not require PKCE for public clients. It processes authorization code requests from SPAs and native mobile apps without code_challenge. PKCE is optional for older client versions, and the team says backward compatibility with older client versions is the reason.
            """;
    }

    private static string BuildSecurityScenarioS5()
    {
        // Return the CB4 Action 3 S5 IP-only throttling concern input.
        return """
            The authorization server token endpoint uses IP-based rate limiting only: each source IP gets 100 requests per minute. There is no per-client-id throttling, so a botnet can attempt many client_secret guesses against the same client_id while staying under each IP limit.
            """;
    }

    private static string BuildSecurityScenarioS6()
    {
        // Return the CB4 Action 3 S6 per-client-id throttling pass input.
        return """
            The authorization server applies per-client-id rate limiting to token endpoint requests: each client_id is throttled to 30 failed authentication attempts per 15-minute window in Redis. Breaches return 429 with Retry-After, and repeated windows trigger a secret rotation flag plus client owner notification.
            """;
    }

    private static string BuildSecurityScenarioS7()
    {
        // Return the CB4 Action 3 S7 cross-source PKCE plus HS256 violation input.
        return """
            An authorization system review finds that the AS accepts authorization code requests from public clients such as SPAs and native apps without code_challenge, so PKCE is optional. JWTs are signed with HS256, and the same HMAC secret is distributed to three resource server teams. The team says PKCE adoption is gradual and HS256 is operationally simpler.
            """;
    }

    private static string BuildSecurityScenarioS8()
    {
        // Return the CB4 Action 3 S8 cross-source PKCE plus RS256 pass input.
        return """
            The authorization system requires code_challenge_method=S256 for all public client authorization code requests and returns 400 when the challenge is missing. Tokens are signed with RS256, the authorization server keeps the private key in a secrets manager, and all resource servers verify through HTTPS JWKS public keys.
            """;
    }

    private static string BuildSecurityScenarioS9()
    {
        // Return the CB4 Action 3 S9 cross-source two-concern input.
        return """
            Two SHOULD-level gaps are present in an OIDC system: token endpoint rate limiting is IP-based only with no per-client-id throttling, and OIDC End Session accepts logout requests without id_token_hint. The team says both are acceptable gaps for this deployment.
            """;
    }

    private static string BuildSecurityScenarioS10()
    {
        // Return the CB4 Action 3 S10 throttling plus logout pass input.
        return """
            Two controls are confirmed correct: token endpoint per-client-id rate limiting allows 30 failed attempts per 15-minute window per client_id, returns 429 with Retry-After on breach, and blocks distributed attacks. OIDC End Session requires id_token_hint with signature, subject, audience, expiry, and session-only validation, so session forcing is blocked.
            """;
    }

    private static string BuildSecurityScenarioS11()
    {
        // Return the CB4 Action 3 S11 HS256 plus ROPC double failure input.
        return """
            An authorization system review finds JWTs signed with HS256, with the same HMAC secret distributed to all 4 resource server teams. ROPC grant_type=password is enabled, so users post credentials directly to the token endpoint and the application collects username and password before sending them to the authorization server. The team says both are legacy choices that work reliably.
            """;
    }

    private static string BuildSecurityScenarioS12()
    {
        // Return the CB4 Action 3 S12 RS256 plus ROPC-disabled pass input.
        return """
            Tokens are signed with RS256, the authorization server holds the private key exclusively, and resource servers use HTTPS JWKS public keys. ROPC disabled across the system; all user authentication now uses authorization code plus PKCE S256, so the application never collects user credentials and MFA remains enforceable.
            """;
    }

    private static string BuildSecurityScenarioS13()
    {
        // Return the CB4 Action 3 S13 PKCE plus SSDF PO.1 training-gap input.
        return """
            A security audit finds the authorization server does not require PKCE for public clients: a React SPA and a native mobile app both operate without code_challenge. SSDF PO.1 review shows the engineer who configured the authorization server was trained on RFC 6749 but not RFC 9700 and was not aware RFC 9700 changed PKCE from optional to required for public clients.
            """;
    }

    private static string BuildSecurityScenarioS14()
    {
        // Return the CB4 Action 3 S14 HS256 plus no-exp compound token failure input.
        return """
            JWT access tokens are signed with HS256, and the same HMAC secret is distributed to 3 resource server teams. The access tokens have no exp claim and are non-expiring at the resource server. The team says HS256 is simple and revocation is handled by a backend API.
            """;
    }

    private static string BuildSecurityScenarioS15()
    {
        // Return the CB4 Action 3 S15 no-PKCE plus IP-only concern input.
        return """
            Two security gaps are identified: token endpoint rate limiting is IP-based only with per-client-id throttling absent, and the authorization server does not enforce PKCE for public clients because a React SPA operates without PKCE. The team says rate limiting covers most attacks and PKCE will be required in a future release.
            """;
    }

    private static string BuildSecurityScenarioS16()
    {
        // Return the CB4 Action 3 S16 comprehensive ext31 pass input.
        return """
            A complete authorization system review covers all ext31 domains: the authorization server enforces PKCE S256 for all public client authorization code requests and rejects non-PKCE requests with 400; JWT access tokens are signed with RS256 and resource servers use HTTPS JWKS public keys; token endpoint per-client-id rate limiting uses 30 attempts per 15-minute client_id window with 429 Retry-After and rotation flagging; OIDC End Session requires id_token_hint with full validation so session forcing is blocked.
            """;
    }

    private static string BuildSecurityScenarioS17()
    {
        // Return the CB4 Action 3 S17 ext25 AS-side PKCE failure input.
        return """
            AS accepts authorization code requests from public clients without code_challenge. PKCE optional. Team says backward compatibility with older client versions.
            """;
    }

    private static string BuildSecurityScenarioS18()
    {
        // Return the CB4 Action 3 S18 ext25 AS-side PKCE pass input.
        return """
            AS rejects public client authorization code requests without code_challenge with 400 invalid_request. All registered public clients send code_challenge_method=S256.
            """;
    }

    private static string BuildSecurityScenarioS19()
    {
        // Return the CB4 Action 3 S19 ext25 HS256 multi-party failure input.
        return """
            AS signs tokens with HS256. Same HMAC secret distributed to 4 resource server teams. HS256 is simpler to configure.
            """;
    }

    private static string BuildSecurityScenarioS20()
    {
        // Return the CB4 Action 3 S20 ext25 RS256 multi-party pass input.
        return """
            AS signs tokens with RSA private key using RS256. 4 resource servers fetch the public key from HTTPS JWKS. The private key is stored in the AS secrets manager only, and no resource server holds it.
            """;
    }

    private static string BuildSecurityScenarioS21()
    {
        // Return the CB4 Action 3 S21 ext25 IP-only throttling concern input.
        return """
            AS limits token endpoint to 100 req/min per source IP. No per-client-id counter. A botnet with 100 IPs could attempt 10000 client_secret guesses per minute per target client_id. High-volume legitimate services cannot be per-client throttled.
            """;
    }

    private static string BuildSecurityScenarioS22()
    {
        // Return the CB4 Action 3 S22 ext25 per-client throttling pass input.
        return """
            30 failed auth attempts per 15-min window per client_id in Redis. Breaches return 429 plus Retry-After. Consecutive-window breaches trigger secret rotation flag and owner notification. IP rate limiting is also present.
            """;
    }

    private static string BuildSecurityScenarioS23()
    {
        // Return the CB4 Action 3 S23 ext25 OIDC logout concern input.
        return """
            End session endpoint accepts post_logout_redirect_uri only. No id_token_hint is required. Any caller can force logout, and the team says logout is not security-sensitive.
            """;
    }

    private static string BuildSecurityScenarioS24()
    {
        // Return the CB4 Action 3 S24 ext25 OIDC logout pass input.
        return """
            End session requires id_token_hint. AS validates signature, sub against the active session, aud against the requesting client, exp is not expired, and only the matching session is terminated.
            """;
    }

    private static void AssertSecurityNodeAnchors(LogicalAssistantRunResult result, string expectedPairId)
    {
        // Node 01 must preserve the expected pair id from the splitter.
        Assert.Equal(expectedPairId, result.Metrics["node_01_source_pair_id"]);

        // Node 02 must preserve the expected pair id from the domain translator.
        Assert.Equal(expectedPairId, result.Metrics["node_02_source_pair_id"]);

        // Node 03 must preserve the expected pair id from the planner.
        Assert.Equal(expectedPairId, result.Metrics["node_03_source_pair_id"]);

        // Node 04 must preserve the expected pair id from the processor.
        Assert.Equal(expectedPairId, result.Metrics["node_04_source_pair_id"]);

        // Node 05 must preserve the expected pair id from the applier.
        Assert.Equal(expectedPairId, result.Metrics["node_05_source_pair_id"]);

        // Node 06 must preserve the expected pair id from the result translator.
        Assert.Equal(expectedPairId, result.Metrics["node_06_source_pair_id"]);

        // Node 07 must preserve the expected pair id from the arbiter selector.
        Assert.Equal(expectedPairId, result.Metrics["node_07_source_pair_id"]);

        // Node 08 must preserve the expected pair id from the arbiter verdict.
        Assert.Equal(expectedPairId, result.Metrics["node_08_source_pair_id"]);

        // Node 09 must preserve the expected pair id from the formatter.
        Assert.Equal(expectedPairId, result.Metrics["node_09_source_pair_id"]);
    }

    private static int CountHumanPhaseSections(string humanResult)
    {
        // Count the seven required phase headings in the human-readable topology result.
        return new[]
        {
            // Phase 1 heading expected by the WDA-00 human checklist.
            "Phase 1 - UI/UX Definition",

            // Phase 2 heading expected by the WDA-00 human checklist.
            "Phase 2 - Functional Specification",

            // Phase 3 heading expected by the WDA-00 human checklist.
            "Phase 3 - Technical Design",

            // Phase 4 heading expected by the WDA-00 human checklist.
            "Phase 4 - Implementation",

            // Phase 5 heading expected by the WDA-00 human checklist.
            "Phase 5 - Testing",

            // Phase 6 heading expected by the WDA-00 human checklist.
            "Phase 6 - QA",

            // Phase 7 heading expected by the WDA-00 human checklist.
            "Phase 7 - QA Automation"
        }.Count(humanResult.Contains);
    }

    private static string BuildWda00HumanResult(
        IReadOnlyList<LogicalPhaseEntry> phases,
        IReadOnlyList<LogicalSubPhaseEntry> subPhases,
        IReadOnlyList<LogicalTransitionEntry> transitions)
    {
        // Start the human-readable result with the required topology summary.
        var lines = new List<string> { "Topology bootstrapped with 7 phases, 31 sub-phases, and 6 transitions." };

        // Add each phase and its sub-phases in lifecycle order.
        foreach (var phase in phases.OrderBy(entry => entry.Phase.PhaseId, StringComparer.Ordinal))
        {
            // Add the phase heading expected by the human result checklist.
            lines.Add(phase.Phase.Name switch
            {
                // Render the canonical Phase 1 heading.
                "UI/UX Definition" => "Phase 1 - UI/UX Definition",

                // Render the canonical Phase 2 heading.
                "Functional Specification" => "Phase 2 - Functional Specification",

                // Render the canonical Phase 3 heading.
                "Technical Design" => "Phase 3 - Technical Design",

                // Render the canonical Phase 4 heading.
                "Implementation" => "Phase 4 - Implementation",

                // Render the canonical Phase 5 heading.
                "Testing" => "Phase 5 - Testing",

                // Render the canonical Phase 6 heading.
                "QA" => "Phase 6 - QA",

                // Render the canonical Phase 7 heading.
                "QA Automation" => "Phase 7 - QA Automation",

                // Fall back to the stored phase name for unexpected headings.
                _ => phase.Phase.Name
            });

            // Add each sub-phase owned by this phase under the phase heading.
            foreach (var subPhase in subPhases.Where(entry => entry.SubPhase.PhaseId == phase.Phase.PhaseId).OrderBy(entry => entry.SubPhase.SubPhaseId, StringComparer.Ordinal))
            {
                // Add the readable sub-phase line.
                lines.Add($"- {subPhase.SubPhase.SubPhaseId}: {subPhase.SubPhase.Name}");
            }
        }

        // Add the transition map heading.
        lines.Add("Transitions");

        // Add every transition edge to the human-readable result.
        foreach (var transition in transitions.OrderBy(entry => entry.Transition.TransitionId, StringComparer.Ordinal))
        {
            // Add the readable transition line.
            lines.Add($"- {transition.Transition.FromTopologyId} -> {transition.Transition.ToTopologyId}");
        }

        // Add the structural correction required by the WDA-00 checklist.
        lines.Add("The canonical count is 31 sub-phases. The earlier 27-sub-phase number is not used by this experiment.");

        // Add the out-of-scope note required by the WDA-00 checklist.
        lines.Add("Out of scope: natural-language topology mutation, document classification, transition carry-forward, learned unit replacement, and full lifecycle execution.");

        // Add the next action required by the WDA-00 checklist.
        lines.Add("Next action: run WDA-01 document classification.");

        // Return the human-readable result.
        return string.Join(Environment.NewLine, lines);
    }
}

file sealed class AssistantNullSessionStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
    {
        // The assistant tests do not persist sessions.
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
    {
        // The assistant tests never load sessions from storage.
        return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));
    }

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
    {
        // The assistant tests expose an empty storage surface.
        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok((IReadOnlyList<string>)Array.Empty<string>()));
    }
}
