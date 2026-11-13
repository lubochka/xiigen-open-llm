# Batch Round Summary - E53 Round D

Date: 2026-05-04

Round: 16

## Section 1 - What Changed

Node-00 was replaced with `Flow01Node00TrainedLogicalCompiler`, and `BuildCompiledLogicalStructure` now compiles the raw E53 scenario input before handing the result to the Round C trained node-01 splitter.

## Section 2 - Gate Results

Focused E53 tests:

| Test | Status |
|---|---|
| `E53_Run_WritesJsonMarkdownAndMermaidEvidence` | pass |
| `E53_EvidenceJson_Uses_RequiredMetricKeys` | pass |
| `E53_Run_PreservesAllHardEdges` | pass |
| `E53_Run_PreservesArrayFanOutAndFanInCounts` | pass |
| `E53_Run_EnforcesDependencyOrder` | pass |
| `E53_Run_AllowsOnlyValidParallelGroups` | pass |
| `E53_Run_RoutesBiometricPassportToKnowledgeEdge` | pass |
| `E53_Run_RejectsDuplicateAccountCreation` | pass |
| `E53_Run_KeepsFinalResultStackIndependent` | pass |
| `E53_Run_FinalHumanResultPassesChecklist` | pass |
| `E53_Run_ProducesLearningGapPacketsForMissingCoverage` | pass |
| `E53_Run_ModelsPolicyGapAsEarlyStartAndResolvedDependency` | pass |

Full suite:

`997/997` passed.

Evidence file:

`C:\Projects\llm_mvp_core\experiments-v2\E53-HIGH-LEVEL-ORCHESTRATION-PROOF\results\run-2026-05-04-144327.json`

## Section 3 - Signal Comparison

Prior round: Round C, `run-2026-05-04-133842.json`.

This round: Round D, `run-2026-05-04-144327.json`.

| Signal name | Prior round value | This round value | Changed |
|---|---|---|---|
| domain packet count | 11 | 11 | no |
| `d01_provider_identity_entry` present | yes | yes | no |
| `d02_email_registration_state` present | yes | yes | no |
| `d03_duplicate_prevention` present | yes | yes | no |
| `d04_token_session_safety` present | yes | yes | no |
| `d05_questionnaire_onboarding` present | yes | yes | no |
| `d06_resume_state_boundary` present | yes | yes | no |
| `d07_policy_acceptance_boundary` present | yes | yes | no |
| `d08_identity_proofing_knowledge_edge` present | yes | yes | no |
| `d09_audit_traceability` present | yes | yes | no |
| `d10_arbiter_review_required` present | yes | yes | no |
| `d11_human_readable_decision` present | yes | yes | no |
| `e2e_orchestration_success` | true | true | no |
| `lost_edge_count` | 0 | 0 | no |
| `dependency_violation_count` | 0 | 0 | no |
| `local_subject` value from node-00 | not emitted in Round C evidence | `complex_registration_and_onboarding_orchestration` | yes, evidence visibility added |

## Section 4 - Self-Assessment Against Artifact Examination Protocol

Structural: file count matches the manifest, and the latest evidence triplet is present with the expected JSON, markdown, and Mermaid files.

Contract: every E53 gate kept its required value, including 11 domain packets, zero lost edges, zero dependency violations, zero live external calls, and five of five human result sections.

Judgment: the only signal change is that node-00's `local_subject` is now visible in evidence; no orchestration behavior changed, and no trained-compiler signal collapse appeared.
