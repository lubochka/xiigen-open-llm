# Codex Pass156 - DPO usefulness user/API surface

claim_scope: One bounded Pass156 T5/R10 proof in current llm_mvp_core: persisted local/loop-created DPO candidate -> JsonlDesignConversionLocalArtifactDb -> DesignConversionDpoUsefulnessApi/readback -> ModelServer local HTTP route payload -> user-visible usefulness metrics/statuses, with raw/private payloads excluded.
evidence_scope: Current code inspection, exact T5 repair-guide readback, focused test patch, and focused dotnet test runs only. This does not claim external platform UI deployment, retrain/upload readiness, production DB readiness, or full product readiness.
count_claimed: 1 bounded Pass156 product-slice evidence packet.
independent_evidence_count: 5 current inspectable records: required skill read/hash record; T5 repair-guide line-window readback; changed test code readback; focused loop-created route test run; focused DPO usefulness API/ModelServer regression test run.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
requested_model: gpt-5.5
requested_reasoning_effort: xhigh

skill_load_status:
- AGENTS.md: found/read; 6913 lines; sha256 prefix 1783F2465B18.
- docs/SESSION-START-PROMPT.md: found/read; 621 lines; sha256 prefix 450961E74C8E.
- docs/skills/planning-session-startup-SKILL.md: found/read; 2041 lines; sha256 prefix 9889656CCE52.
- docs/skills/architect-behavior-classifier-SKILL.md: found/read; 1469 lines; sha256 prefix 02FD255413A2.
- docs/skills/plan-review-SKILL.md: found/read; 3747 lines; sha256 prefix AF9764553AB1.
- docs/skills/claim-verification-SKILL.md: found/read; 766 lines; sha256 prefix B230CB92DFD3.
- docs/skills/trainable-architecture-decision-SKILL.md: found/read; 738 lines; sha256 prefix 3A751518E20A.
- docs/skills/no-product-decisions-SKILL.md: found/read; 159 lines; sha256 prefix 7009F0B51642.
- docs/skills/qa-session-type-SKILL.md: found/read; 231 lines; sha256 prefix 114DA97E4FC8.
- docs/skills/generated-code-review-SKILL.md: found/read; 643 lines; sha256 prefix 89A5587A76B1.
- docs/skills/test-integrity-SKILL.md: found/read; 776 lines; sha256 prefix FD545554BC74.
- docs/skills/implementation-integrity-SKILL.md: found/read; 794 lines; sha256 prefix 18C33DD8C4D7.
- docs/skills/verification-before-completion-SKILL.md: found/read; 696 lines; sha256 prefix BDEC446CFC62.
- docs/skills/self-verification-SKILL.md: found/read; 192 lines; sha256 prefix 1A14F71DD7D6.
- docs/skills/agent-output-format-SKILL.md: found/read; 498 lines; sha256 prefix 22D9E15316D7.

active_task_sentence_source: Luba's current unquoted message: "Pass156 product slice: close T5/R10 from the repair guide: user-facing DPO surface with usefulness metrics..."

pre_fix_diagnosis:
- External repair guide read-only source found at C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\claude-FIX-GUIDE-scaffold-to-implemented-v1.md.
- Requested T5/R10 window read: lines 2816-3383.
- T5 guide status at lines 2818-2860: the historical gap was that the core display/usefulness projection existed, but a user surface/endpoint/UI transport path was missing.
- T5 guide acceptance at lines 2887-2895, 2954-2964, and 3298-3336: the DTO/transport must expose flat usefulness/review/train-ready/quarantine/private-data status and must not emit internal vectors, full HTML/CSS candidates, or raw payloads.
- Current llm_mvp_core status before this patch: DesignConversionDpoUsefulnessApi and the ModelServer route already existed and delegated to IDesignConversionLocalArtifactDb, but existing route tests covered seeded/static rows. The missing focused proof was that an actual loop-created persisted local artifact DB row reached the ModelServer route before and after approval.

files_changed:
- Tests/DesignConversionImprovementLoopCoordinatorTests.cs: extended the existing loop-created local artifact DB test to invoke the ModelServer DPO usefulness route before and after approval and assert the same user-visible usefulness/status/raw-boundary contract. This file was already untracked in git status before this pass; the pass modified that untracked file.
- docs/evidence/design-conversion/codex-pass156-dpo-usefulness-user-surface.md: this evidence packet.
- No production source file was changed; the current route already accepted a DesignConversionDpoUsefulnessApi backed by the local artifact DB.

commands_run:
- Required skill-load read/hash command over AGENTS.md, docs/SESSION-START-PROMPT.md, and the required docs/skills files.
- git status --short -- Tests\DesignConversionImprovementLoopCoordinatorTests.cs docs\evidence\design-conversion\codex-pass156-dpo-usefulness-user-surface.md
- rg --files -g "*FIX*GUIDE*.md" -g "*repair*guide*.md" -g "*REPAIR*GUIDE*.md" "C:\Users\lubao\Documents\xiigen"
- rg -n "T5|R10|usefulness|DPO" "C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\claude-FIX-GUIDE-scaffold-to-implemented-v1.md"
- Get-Content readback for lines 2816-3383 of the T5/R10 guide section.
- rg/readback of changed test anchors: routeBeforeApproval, routeAfterApproval, serializedRouteBeforeApproval, serializedRouteAfterApproval, InvokeDpoUsefulnessRouteAsync, AssertLoopCreatedDpoUsefulnessRoute.
- dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionImprovementLoopCoordinatorTests.RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidateForDpoSourceWithoutRawPrivatePayload"
- dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerDpoUsefulnessTests|FullyQualifiedName~DesignConversionDpoUsefulnessApiTests"
- Identity-field readback over docs\evidence\design-conversion\codex-pass156-dpo-usefulness-user-surface.md
- Line-anchored conflict-marker scan over both changed files.
- Configured mojibake marker scan over both changed files.
- rg/readback of evidence schema anchors.
- git status --short -- Tests\DesignConversionImprovementLoopCoordinatorTests.cs docs\evidence\design-conversion\codex-pass156-dpo-usefulness-user-surface.md

tests_run:
- PASS: focused loop-created persisted local artifact DB -> DPO usefulness API -> ModelServer route test. Result: Failed 0, Passed 1, Skipped 0, Total 1, Duration 1 s.
- PASS: focused DPO usefulness API and ModelServer route regression set. Result: Failed 0, Passed 6, Skipped 0, Total 6, Duration 1 s.
- Earlier assertion-shaping attempts failed while tightening evidence-ref expectations; the final two focused test runs above are the accepted evidence.

user_surface_usefulness_proof:
- The existing loop-created candidate test now calls InvokeDpoUsefulnessRouteAsync(reloaded), which registers DesignConversionModelServerDpoUsefulnessHandler with a DesignConversionDpoUsefulnessApi backed by the reloaded JsonlDesignConversionLocalArtifactDb.
- Before approval, the route is invoked at /api/design-conversion/dpo-usefulness with includeBlocked=true&limit=10 and must return one row with usefulnessScore > 0, usefulnessStatus present, reviewStatus runtime_candidate_review_pending, approvalStatus approval-pending, trainReady false, trainReadyCount 0, blockedCount 1, reviewRequired true, trainingCandidateOnly true, productNotReady true, productionDpo false, modelReadinessClaimed false, heldOutNotProven true, and sourceDisjointNotProven true.
- After approval, the same persisted DB and route must return one row with reviewStatus train-ready, approvalStatus approved, trainReady true, trainReadyCount 1, blockedCount 0, while still marking productNotReady true and not claiming model readiness.
- The route payload must expose endpoint/api shape, summary counts, warnings including modelserver_route_delegates_to_domain_dpo_usefulness_api, evidenceRefs, usefulnessFactors including the approval status, and boundaryMetadata naming DesignConversionDpoUsefulnessApi plus IDesignConversionLocalArtifactDb.

raw_private_boundary_proof:
- The test seeds distinct raw markers for source HTML, CSS, design tree, improved HTML, and improved CSS, then asserts those markers are absent from persisted JSONL, domain usefulness API JSON, local readback API JSON, ModelServer route JSON before approval, ModelServer route JSON after approval, approval result JSON, handoff JSON, intake JSON, and run JSON.
- The route helper also asserts the response JSON excludes <main, C:/Users/lubao, data:image, and raw_provider_payload.
- Route row metadata must report raw_html_included=false, raw_css_included=false, raw_design_tree_included=false, raw_user_private_data_included=false, and provider_payload_included=false.
- Route root metadata must report dpo_usefulness_response_refs_only=true, response_contains_raw_private_data=false, product_ready=false, and the same raw/private false flags.
- Evidence refs are asserted non-empty, contain a design-conversion-dpo-display prefix, and are individually checked not to contain raw/private markers or local private paths.

verification_results:
- Focused tests passed as listed above.
- Full suite was not run.
- No external AI/provider API was called.
- No git branch, commit, push, PR, reset, clean, or checkout was run.
- Post-write identity readback passed: exactly one line contains the required identity field, line 7, with UNKNOWN_IN_SUBAGENT.
- Post-write line-anchored conflict marker scan over the changed test and evidence file returned no matches.
- Post-write mojibake marker scan over the changed test and evidence file returned no matches.
- Post-write evidence schema readback returned the required claim/evidence/count/user-surface/raw-boundary/verification/downgrade anchors.
- Post-write git status showed both touched files as untracked: Tests\DesignConversionImprovementLoopCoordinatorTests.cs and docs\evidence\design-conversion\codex-pass156-dpo-usefulness-user-surface.md.

mergeability_status_or_not_checked: Full mergeability was not checked. Focused test builds succeeded. git status showed a dirty workspace and this test file was already untracked before this pass.

blockers: None for this bounded Pass156 local user/API surface proof. Remaining outside scope: external platform/UI deployment, retrain/upload, production DB, and full product readiness.

downgrade_needed: No downgrade for the bounded T5/R10 local llm_mvp_core user/API surface proof. Downgrades retained: no full product readiness, no model upload readiness, no retrain readiness, no external platform UI claim, and no production DB claim.

next_recommended_pass: If Luba wants the next slice, verify or wire the external platform/UI surface against this local route contract without moving usefulness scoring, filtering, or raw payload handling out of the core/API boundary.
