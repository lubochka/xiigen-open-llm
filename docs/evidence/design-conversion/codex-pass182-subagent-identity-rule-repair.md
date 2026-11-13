# Codex Pass182 - Spawned Worker Identity Rule Repair

claim_scope: Pass182 governance/instruction repair for repeated spawned-worker identity failures after Pass172. This pass patches only allowed governance/skill files and writes this evidence artifact. It does not edit product code, safe worktrees/copies, source repos, C:\Users\lubao\Documents\xiigen, git branches, commits, pushes, merges, PRs, deploys, cloud resources, public endpoints, or external providers.
evidence_scope: Fresh targeted reads of the requested AGENTS, skill/guide, Pass172, Pass177, Pass178, Pass180, and Pass181 artifacts; direct edits to identity-return and claim/output gates; post-edit readback, marker, artifact-count, and diff-check verification.
count_claimed: 1 Pass182 governance repair artifact. This is not a 50-pass completion claim, not a ProductReady claim, and not a 20-fix/repair completion claim.
independent_evidence_count: 5 verification groups: required file/readback discovery; before/after identity-gate readback; conflict/mojibake marker scan; artifact field-count check; git diff whitespace check.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: No downgrade for the narrow Pass182 governance repair if final verification stays clean. Downgrade remains mandatory for any claim that 50 passes are complete, 20 fix/repair passes are complete, ProductReady is true, production/cloud/deploy/public endpoint/provider readiness is proven, source publication/materialization is complete, CI is green, or commit/push/merge/PR occurred.

## skill_load_status

| Required file | Status in this pass | Applied boundary |
|---|---|---|
| C:\Projects\llm_mvp_core\AGENTS.md | found/targeted_read | Current identity rules, sub-agent work-order completeness, Pass172 firewall, and skill-load gates read. |
| C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/targeted_read | Startup identity gate and return-schema self-check read. |
| C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md | found/targeted_read | Skill-load gate, Pass172 review gate, identity hard-fail rules, and NEEDS_FIX repair queue read. |
| C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/targeted_read | Claim-boundary and spawned-worker identity rejection rules read. |
| C:\Projects\llm_mvp_core\docs\skills\audit-protocol-SKILL.md | found/targeted_read | Audit identity evidence gate read; no edit because it was not in the allowed-file list. |
| C:\Projects\llm_mvp_core\docs\skills\codex-planning-quality-GUIDE.md | found/targeted_read | Planning-quality no-overclaim and spawned-worker identity boundary read; no edit because it was not in the allowed-file list. |
| C:\Projects\llm_mvp_core\docs\skills\agent-output-format-SKILL.md | found/targeted_read | Output format gates read before adding the final-packet identity gate. |

## files_read

- C:\Projects\llm_mvp_core\AGENTS.md.
- C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md.
- C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md.
- C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md.
- C:\Projects\llm_mvp_core\docs\skills\audit-protocol-SKILL.md.
- C:\Projects\llm_mvp_core\docs\skills\codex-planning-quality-GUIDE.md.
- C:\Projects\llm_mvp_core\docs\skills\agent-output-format-SKILL.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass172-governance-skill-repair.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass177-figma-browser-download-gap.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass178-figma-visual-drift.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass180-evidence-count-ledger.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass181-count-propagation-repair.md.

## files_changed

- C:\Projects\llm_mvp_core\AGENTS.md.
- C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md.
- C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md.
- C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md.
- C:\Projects\llm_mvp_core\docs\skills\agent-output-format-SKILL.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass182-subagent-identity-rule-repair.md.

## loophole_repaired

Before: Pass172-era rules required a platform id or hidden-id sentinel, but spawned-worker prompts and output gates could still be followed loosely enough that a worker returned the forbidden placeholder first and needed recovery. The parent gate also needed a sharper acceptance rule tying missing, false, or unscanned identity to NEEDS_FIX recovery rather than pass acceptance.

After: AGENTS, startup, plan-review, claim-verification, and output-format rules now require a visible `Do not use NONE` instruction beside spawned-worker return schemas; require a final packet self-scan before return; require hidden platform ids to use exactly `UNKNOWN_IN_SUBAGENT`; and require parent acceptance to reject missing, false, placeholder, or unscanned identity as NEEDS_FIX/recovery with 0 accepted count.

## rules_added_or_strengthened

- AGENTS.md: added `pass182_spawned_worker_identity_final_scan_gate` near existing identity gates and added `identity_instruction: Do not use NONE` to the return-schema block.
- planning-session-startup-SKILL.md: added the same startup hardening so new sessions/work orders carry the instruction before any status or handoff.
- plan-review-SKILL.md: strengthened hard-fail review bullets so parent review rejects a spawned packet without clean self-scan and valid identity.
- claim-verification-SKILL.md: added a final-packet fail gate so claim verification fails before return/acceptance for spawned packets with forbidden or unscanned identity.
- agent-output-format-SKILL.md: added a final packet identity gate with the required fields and parent recovery treatment.

## commands_run

| Command group | Result | Evidence boundary |
|---|---|---|
| File discovery and line counts for required AGENTS, skills/guides, and Pass172/177/178/180/181 artifacts | Required files were found in C:\Projects\llm_mvp_core; line counts were read. | Discovery/readback only. |
| Targeted readbacks for existing identity, skill-load, Pass172, and Pass180/181 count gates | Existing loophole surface was read before patching. | Supports this governance repair only. |
| Post-edit `rg -n -C 4` readback over Pass182 markers and identity instruction markers | Changed sections in the five instruction files and this artifact read back with the new self-scan and parent acceptance language. | Readback of changed sections only. |
| Artifact field-count check | Identity field count was 1 and Pass182 verdict field count was 1. | Artifact schema check only. |
| Line-anchored conflict-marker scan over changed files | Conflict-marker count was 0 for every changed file. | Conflict check only. |
| Changed-section configured mojibake scan | Changed-section mojibake-marker count was 0 for every changed file. | Edited-section encoding check only; existing unrelated historical text elsewhere was not claimed clean. |
| `git diff --check` over the changed files | Exit code 0 with no output. | Whitespace check only. |

## readback_verification

- AGENTS.md readback showed `identity_instruction: Do not use NONE` in the spawned-worker return schema and the new `pass182_spawned_worker_identity_final_scan_gate`.
- planning-session-startup-SKILL.md readback showed the same return-schema instruction and startup parent acceptance gate.
- plan-review-SKILL.md readback showed the strengthened hard-fail bullet requiring `Do not use NONE`, final self-scan, and NEEDS_FIX/recovery for missing, false, placeholder, or unscanned identity.
- claim-verification-SKILL.md readback showed the new final-packet fail gate before return/acceptance.
- agent-output-format-SKILL.md readback showed the new `Spawned Worker Final Packet Identity Gate`.
- This artifact readback showed the before/after loophole, exact files changed, verification summary, count boundary, remaining count status, and Pass182 verdict.
- Conflict markers were 0 in all changed files.
- Changed-section mojibake markers were 0 in all changed files.
- Artifact identity field count was 1 and artifact verdict field count was 1 before this verification text update; final parent/current rerun must recheck the final bytes.
- `git diff --check` over the changed files exited 0 with no output before this verification text update; final parent/current rerun must recheck the final bytes.

## remaining_count_status

- If Pass182 is accepted as this bounded governance repair, current accepted main count becomes 11/50, with 39 remaining before any 50-done claim.
- If Pass182 is accepted as this bounded governance repair, current strict fix/repair count becomes 7/20, with 13 additional fix/repair passes needed before any 20-fix/repair claim.
- ProductReady, production/cloud/deploy/public endpoint/provider readiness, source publication/materialization, CI, commit, push, merge, and PR remain unproven and unclaimed.

pass182_verdict: PASS_SUBAGENT_IDENTITY_RULE_REPAIR
