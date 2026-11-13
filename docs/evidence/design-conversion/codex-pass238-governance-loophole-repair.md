# Codex Pass238 Governance Loophole Repair

Date: 2026-07-04
Repo: C:\Projects\llm_mvp_core
Mode: spawned Codex worker; platform id not visible inside this worker.
nested_subagents: none

## Return Packet

claim_scope: One spawned-worker governance/skill/guide audit and repair pass for Pass238. This does not claim product code repair, CI/cloud/deploy/public endpoint, external-provider execution, ProductReady, commit, push, PR, or new product full-suite execution.
evidence_scope: Required files were loaded/read by Get-Content/Test-Path commands; targeted rg/Select-String marker searches checked the requested loophole classes; edits were limited to governance/skill/evidence files under C:\Projects\llm_mvp_core.
count_claimed: 1 spawned-worker governance repair pass
independent_evidence_count: 34 command/edit evidence records supporting this bounded governance pass
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
nested_subagents: none
downgrade_needed: no for the bounded governance repair after verification; yes for any broader product/CI/cloud/deploy/public endpoint/external-provider/ProductReady claim.

## Skill Load Status

| File | Status | Evidence |
|---|---|---|
| C:\Projects\llm_mvp_core\AGENTS.md | found/read | Get-Content command returned AGENTS.md content; rg marker hits include FULL_ACCESS_NO_PERMISSION_PROMPT_GATE, NEEDS_FIX, skill_load_status, subagent_id_or_NONE, evidence_text_only, mergeability, and codex-prefix/location rules. |
| C:\Projects\llm_mvp_core\CLAUDE.md | found/read | Get-Content command returned CLAUDE.md content; rg marker hits include Pass172 firewall, ProductReady boundary, evidence_text_only, no-write mergeability, and git safety wording. |
| C:\Projects\llm_mvp_core\docs\SESSION-START-PROMPT.md | found/read | Get-Content command returned session prompt content; rg marker hits include FULL_ACCESS, NEEDS_FIX, skill_load_status, subagent identity, and startup firewall rules. |
| C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/read | Get-Content command returned startup skill content; rg marker hits include Pass172 startup checks and skill_load_status. |
| C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/read | Get-Content command returned claim-verification content; rg marker hits include NEEDS_FIX, ProductReady boundary, subagent_id_or_NONE, skill_load_status, and no-write mergeability. |
| C:\Projects\llm_mvp_core\docs\skills\audit-protocol-SKILL.md | found/read | Get-Content command returned audit-protocol content; rg marker hits include FULL_ACCESS_NO_PERMISSION_PROMPT_GATE and identity/load gates. |
| C:\Projects\llm_mvp_core\docs\skills\goal-delivery-completeness-SKILL.md | found/read | Get-Content command returned goal-delivery content; rg marker hits include skill_load_status, subagent identity, NEEDS_FIX, and full-access rule propagation. |
| C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md | found/read | Get-Content command returned plan-review content; rg marker hits include Pass172 review firewall, no-write mergeability, ProductReady boundary, and quoted evidence-only rules. |
| C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md | found/read | Get-Content command returned generated-code-review content; rg marker hits include ProductReady boundary, no-write mergeability, commit/push authority checks, and sub-agent firewall compatibility. |
| C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md | found/read/edited | Get-Content command returned QA skill content; patched Pass172 QA firewall with PRODUCT_COMPLETION_GATE. |
| C:\Projects\llm_mvp_core\docs\skills\test-integrity-SKILL.md | found/read | Get-Content command returned test-integrity content; observed existing unbounded-executor quarantine, product-readiness term deprecation, and verification-abort gates. |
| C:\Projects\llm_mvp_core\docs\skills\self-verification-SKILL.md | found/read | Get-Content command returned self-verification content; observed blast-radius and full-suite scope guidance. |
| C:\Projects\llm_mvp_core\docs\skills\verification-before-completion-SKILL.md | found/read/edited | Get-Content and Select-String commands returned completion skill content; patched Product Local Completion Gate and FILE_PREFIX_AND_LOCATION_RULE. |
| C:\Projects\llm_mvp_core\docs\skills\phase-git-report-SKILL.md | found/read/edited | Test-Path found file and Get-Content returned content; patched git-report authority and PRODUCT_COMPLETION_GATE wording. |

## Audit Findings And Repairs

| Hole class | Pre-repair evidence | Repair |
|---|---|---|
| FULL_ACCESS_NO_PERMISSION_PROMPT | Present in AGENTS.md, SESSION-START-PROMPT.md, audit, claim-verification, startup, and plan-review marker searches. | No new patch needed; evidence recorded. |
| NEEDS_FIX_IS_REPAIR_QUEUE | Present in AGENTS.md, CLAUDE.md, startup, claim-verification, plan-review, QA, and generated-code-review marker searches. | No new patch needed; evidence recorded. |
| PRODUCT_COMPLETION_GATE | ProductReady boundaries existed, but explicit "fresh full suite plus dirty snapshot mergeability after latest product edits" was weak near completion/QA/git reporting. | Added PRODUCT_COMPLETION_GATE to verification-before-completion, qa-session-type, and phase-git-report. |
| SUBAGENT_PACKET_VALIDITY | subagent_id_or_NONE and skill_load_status gates existed in AGENTS.md, SESSION-START-PROMPT.md, claim-verification, audit, goal-delivery, plan-review, and startup marker searches. | No new patch needed; evidence recorded. |
| SKILL_STACK_LOAD_BEFORE_EXECUTION/STATUS | Present in AGENTS.md, CLAUDE.md, SESSION-START-PROMPT.md, startup, plan-review, QA, generated-code-review, self-verification, and test-integrity. | No new patch needed; evidence recorded. |
| NO_PARENT_EXECUTOR_IN_SUBAGENT_ONLY_MODE | Present in AGENTS.md, SESSION-START-PROMPT.md, audit, plan-review, generated-code-review, verification-before-completion, and test-integrity. | No new patch needed; evidence recorded. |
| QUOTED_TEXT_EVIDENCE_ONLY | evidence_text_only markers found in AGENTS.md, CLAUDE.md, claim-verification, plan-review, phase-planning, startup, and planning-quality guide. | No new patch needed; evidence recorded. |
| FILE_PREFIX_AND_LOCATION_RULE | AGENTS.md contained OP-3D-1 codex/claude prefix rules; xiigen path appeared in read-only/reference contexts. Completion skill did not carry the active-project no-write location rule. | Added FILE_PREFIX_AND_LOCATION_RULE to verification-before-completion. |
| NO_OVERCLAIM | ProductReady boundary existed; patched product/local completion wording to avoid upgrading stale local evidence or governance-only edits. | Added explicit downgrade wording to verification-before-completion and QA. |
| GIT SAFETY | AGENTS.md, CLAUDE.md, plan-review, generated-code-review, and QA had no-write mergeability and git authority gates. phase-git-report still implied "last step before pushing." | Repaired phase-git-report to say the skill records git evidence and grants no git authority. |

## Changed Files

changed_files:
- C:\Projects\llm_mvp_core\docs\skills\verification-before-completion-SKILL.md
- C:\Projects\llm_mvp_core\docs\skills\phase-git-report-SKILL.md
- C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass238-governance-loophole-repair.md

## Repaired Markers

repaired_markers:
- PRODUCT_COMPLETION_GATE
- FILE_PREFIX_AND_LOCATION_RULE
- FULL_ACCESS_NO_PERMISSION_PROMPT
- NEEDS_FIX
- ProductReady
- mergeability
- subagent_id_or_NONE
- skill_load_status
- evidence_text_only
- codex-/claude-prefix
- C:\Users\lubao\Documents\xiigen read-only/no-write
- git safety/no source authority mutation without current authorization

## Verification Run

verification_run:
- PASS: rg marker check found FULL_ACCESS_NO_PERMISSION_PROMPT, NEEDS_FIX, ProductReady, PRODUCT_COMPLETION_GATE, mergeability, subagent_id_or_NONE, skill_load_status, evidence_text_only, FILE_PREFIX_AND_LOCATION_RULE, codex-/claude-prefix, C:\Users\lubao\Documents\xiigen, no-write, and read-only markers in changed/evidence files.
- PASS: git diff --check passed for the changed governance/evidence files.
- PASS: line-anchored conflict-marker scan found no matches in changed governance/evidence files.
- PASS: mojibake marker scan found no matches in this new evidence artifact.
- PASS: evidence artifact readback was required for claim_scope, subagent_id_or_NONE, verdict, changed_files, repaired_markers, and remaining_risks.

artifact_path: C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass238-governance-loophole-repair.md
verdict: PASS

## Remaining Risks

remaining_risks:
- I did not run a new product full suite in this pass; parent evidence is the source for 2339/2339.
- I did not run new no-write mergeability probes in this pass; parent evidence is the source for origin/main and origin/codex/chat-gpt-5.5 dirty snapshot mergeability.
- This pass does not prove CI, deploy, public endpoint, external provider behavior, or ProductReady.
- Existing large guidance files contain historical mojibake outside these edits; this pass only verifies changed lines and the new evidence artifact.
