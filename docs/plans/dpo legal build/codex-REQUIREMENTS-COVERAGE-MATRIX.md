# Codex Requirements Coverage Matrix - ILearningDynamicGraph

Artifact: `codex-REQUIREMENTS-COVERAGE-MATRIX.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: after cycle-42 integration, a bulk status sync damaged localized prose in this file. This recovered matrix restates the active canon in ASCII and does not claim byte-for-byte preservation of the prior Russian prose.

## Authority Anchor

```text
CURRENT TASK:
Compare the Codex ILearningDynamicGraph plan with Claude benchmark evidence, improve Codex-owned planning artifacts, and complete 50 real GPT-5.5/xhigh review cycles.

NOT THE TASK:
Do not edit Claude-authored files. Do not count ledger rows or duplicate notification wrappers as review cycles. Do not implement product code, run tests/builds/training, use git, or call external models.

EVIDENCE-ONLY:
Claude attachment, claude-* files, old W2c overlays, the Claude-authored codex supplement, old packets, and foreign/parallel artifacts.
```

## Current Honest State

```text
verified_subagent_packets_completed_current_run = 50/50
cycles_01_50_complete_current_run = true
old_50_cycle_ledgers = evidence_only_for_current_run
requirements_total = 21
covered = 15
partial = 6
unmapped = 0
covered_set = R2, R3, R4, R5, R6, R7, R8, R10, R11, R12, R13, R18, R19, R20, R21
partial_set = R1, R9, R14, R15, R16, R17
baseline_phases = 12
baseline_subphases = 26
baseline_substeps = 59
reserve_rows = F10.3.1..F10.3.6 outside baseline-59 until Q41 Variant B is accepted
implementation_authorized = false
```

## Requirement Matrix

| ID | Requirement | Coverage / Gate | Status |
|---|---|---|---|
| R1 | ILearningDynamicGraph builds DPO for different ILearningUnit levels inside the machine. | Builder levels `unit`, `edge`, `graph`; unit train step F4.3.1; edge train step F4.3.2; measurement F5.2.1; loop F7. Still partial until implementation-level evidence proves unit/edge training behavior, not only plan structure. | partial |
| R2 | Legal open-weight teacher allow-list and closed proprietary deny-list. | F0.1.3, F2.1.1, F2.1.2; allowed Qwen3, DeepSeek, Kimi, GLM, Mistral; denied Claude/OpenAI/Gemini as teachers. | covered |
| R3 | ILearningDynamicGraph is L5 `ILearningMachine<DynamicGraphDomain>`, not a sixth level. | Master Part A and plan-04/12 preserve L5 boundary. | covered |
| R4 | Dynamic topology is built by a learned graph, not a hard-coded adjacency matrix. | V1 rejects adjacency matrix as primary representation; uses node/edge contracts plus `BuildDynamicTopology(DynamicGraphBuildSignal signal, TopologyBuildBudget budget)`. | covered |
| R5 | Edge final authority is trainable and not hidden deterministic routing. | `IDynamicGraphEdge.AuthorityUnitId`, `AuthorityUnitLevel=edge`, `DeterministicFinalAuthority=false`, baseline only as eval/ablation. | covered |
| R6 | Review-pass claims must distinguish real returned packets from row counts. | Ledger rows are not cycles; duplicate notifications do not count. Current comparison run has 50/50 returned packet records in the controlling current-run ledger; old ledgers remain evidence-only unless reconciled. | covered |
| R7 | Auto-DPO loop has no-signal stop path and no accidental promotion. | `NO_AUTO_DPO_SIGNAL=true`, accepted pair count after all gates is 0, `ShouldStopResult.stop_reason=no_signal`, no training/promotion, previous checkpoint retained. | covered |
| R8 | GraphDpoPair carries train-ready triad and per-side authority/origin. | `chosen`, `rejected`, authority refs, origin refs, legal verdict, source category, rejected origin, and `dpo_train_ready` gate. | covered |
| R9 | Per-document authoring and review traceability must not be inflated. | Some documents are recovered/integrated by parent; no false per-document sub-agent authorship claim. | partial |
| R10 | Async/fan-out/fan-in/many-to-many combinations have explicit gates. | Plan-06 plus board gates cover async branches, merge semantics, and no hidden deterministic winner. | covered |
| R11 | Phase numbering is stable and sequential. | Baseline board uses 12 phases / 26 subphases / 59 substeps; F4.3.1 unit, F4.3.2 edge, F4.3.3 split. | covered |
| R12 | Codex-owned files use codex prefix; Claude-authored codex-looking files are evidence-only exceptions. | Master F.6 and BDR list exact Claude-authored evidence files and the new Codex-owned skill-ideas file separately. | covered |
| R13 | All source plan blocks P-00..P-14 are represented. | Master index and board source map include all plan blocks; recovered support docs restate their canon. | covered |
| R14 | Legal provenance parity must propagate across train-ready projections. | `TeacherProvenance` has 18 fields; plan-08 owns legal gate; plan-06/07/09/10 projections are aligned in plan text, but runtime/data-manifest parity remains implementation evidence. | partial |
| R15 | Additional reference domains must be explicit. | Six reserve domains: small-model, language-translate, local-wiki, extensions, documentation-learning, session-start. Rows F10.3.1..F10.3.6 remain outside baseline until Q41 Variant B. | partial |
| R16 | Common vs personal/adaptive host mapping needs Luba decision. | `LegScope` and source mapping exist; final host/default policy remains in final questions. | partial |
| R17 | FLOW-38 better-output legality needs Luba decision for chosen/rejected origin. | FLOW-38 bridge exists; `decision_ref=Q37/Q38` blocks final legal verdict. | partial |
| R18 | Codex-prefix ownership rule is documented with exceptions. | Exact evidence-only exceptions plus Codex-owned auxiliary artifact are listed. | covered |
| R19 | Questions stay in final sections only. | Open Q37/Q38/Q41/R16 questions are referenced by decision ids in body; actual questions stay in final sections. | covered |
| R20 | Run 50 real review/fix cycles before claiming full review depth. | Current comparison run: `codex-GPT55-50-CYCLE-CLAUDE-COMPARISON-LEDGER-20260624.md` has 50/50 returned packet records after C49-C50 integration. Older 50/50 ledgers remain evidence-only for this run unless exact packet evidence is reconciled. | covered |
| R21 | Before implementation, each participating repository uses its most recently updated branch as source authority; the future working branch name is `DPOLegalLearning`. | Pre-flight Gate B requires a repo-by-repo authority table with repo path/url, selected latest-updated branch, timestamp/ref evidence, divergence note, and proposed new branch `DPOLegalLearning`; candidate participating repo identifiers are listed in Gate Map and must be verified/updated before implementation. This is plan-only and does not create/switch branches now. | covered |

## Gate Map

| Area | Required evidence |
|---|---|
| Repository branch authority | For every participating repo: repo path/url, inspected branch list source, selected latest-updated branch, timestamp/ref evidence, divergence note, and future working branch name `DPOLegalLearning`. Candidate repo identifiers to verify/update: `llm_mvp_core`, `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular`, `FigmaToCode-design-gen`, `CanvaToCode`. |
| Split proof | `train_source_roots`, `eval_source_roots`, `source_hashes`, `cross_split_near_duplicate_count`, `simhash_or_minhash`, `near_duplicate_threshold`. |
| Lifecycle proof | `process_id`, `restart_id`, `entrypoint_trace_id`, `no_replay_cache_proof`, loaded checkpoint/manifest hashes, `manifest_schema_version`, `source_cutoff`, `path_id_strategy`, `rollback_ref`. |
| Edge authority | `AuthorityUnitId`, `AuthorityUnitLevel=edge`, `ParallelEligible`, `DeterministicFinalAuthority=false`, `BaselineUnitId`, `BaselineRole`, leakage/source/cutover/ablation proofs. |
| Legal provenance | 18-field `TeacherProvenance`; `SourceCategory` values: local, human, open_weight, hosted, deterministic_baseline, self_judge, client_unverified, third_party. |
| Review honesty | Count only returned packets with agent id, target-library evidence, findings or no-fix rationale, and parent integration/defer decision. |

## Numbering Crosswalk

This crosswalk resolves the two active numbering systems: master plan
`R-01..R-20` and Luba requirement ids `R1..R21`.

| master_id | master_meaning | luba_id | luba_meaning | canonical_owner | collision_note |
|---|---|---|---|---|---|
| R-01 | DynamicGraph remains L5, not a sixth level | R3 | L5 boundary | master + plan-04 | none |
| R-02 | Dynamic topology is learned, not hard-coded adjacency | R4 | learned graph helps create DPO | master + plan-04 | none |
| R-03 | legal open-weight teacher allow-list / closed teacher deny-list | R2, R14 | legal teacher and provenance | plan-08 | none |
| R-04 | closed judge may evaluate but is not trained target | R2, R14 | legal teacher and provenance | plan-08 | none |
| R-05 | GraphDpoPair provenance/source train-ready fields | R8, R14, R17 | DPO pair triad, legal provenance, better-output origin | plan-07 / plan-09 / plan-08 | field vocabulary must stay mapped |
| R-06 | auto-DPO no-signal stop/no accidental promotion | R7 | auto-DPO no-signal stop path | plan-09 | none |
| R-07 | no hidden deterministic final authority | R5, R13 | trainable edge authority and no hidden subtask | plan-12 | none |
| R-08 | trainability lifecycle and checkpoint/export/import | R1, R14 | DPO levels and legal provenance parity | plan-10 | none |
| R-09 | measurement speed/accuracy/quality | R1, R5, R14 | DPO proof and trained edge acceptance | plan-11 | none |
| R-10 | CommonGlobal vs PersonalAdaptive split | R16 | host/common-vs-personal decision | plan-10 + final questions | none |
| R-11 | thin agent, core logic in llm_mvp_core | R16, R17 | API/client host mapping and better-output flow | plan-14 + board F10 | none |
| R-12 | `codex-*` prefix for Codex-owned artifacts | R12, R18 | prefix and ownership exception rule | master index + BDR | none |
| R-13 | plan produced/reviewed through sub-agent packets | R6, R9, R20 | review honesty, authoring traceability, 50-cycle depth | current-run ledger | old ledgers are evidence-only |
| R-14 | questions stay in the final section | R19 | questions stay final | master/matrix final sections | Luba R19 is not master R-19 |
| R-15 | Part A completeness / source blocks represented | R13 | P-00..P-14 represented | master + plan-14 | none |
| R-16 | test strategy over nine layers | R13 | no hidden source block or test gap | plan-13 | none |
| R-17 | recon contamination finding | R14 | legal provenance parity and dataset rebuild | plan-08 | master recon id, not Luba R17 |
| R-18 | recon train-ready pointer mismatch | R18 | Codex-prefix ownership exception documentation | master §F / BDR | numbering collision: meanings differ |
| R-19 | recon hardcoded RewardMargin finding | R19 | questions stay in final sections | plan-10 / plan-11 vs final questions | numbering collision: meanings differ |
| R-20 | current-turn branch authority | R21 | latest-updated branch authority and `DPOLegalLearning` | board §0.3.1 + matrix R21 | master R-20 maps to Luba R21 |
| board-only | strict sequence / execution visibility | R11 | stable sequential phase numbering | board §0.3/§B | not in master §B.1 |
| board/ledger-only | 50 review/fix cycles | R20 | review depth before completion claim | current-run ledger + board §D | not in master §B.1 |

## Final Questions Only

1. Should Q41 Variant B activate all six R15 domains now, or should the six reserve rows remain blocked outside baseline until implementation scope is approved?
2. Should generated HTML/CSS and FLOW-38 feedback default to `PersonalAdaptive` unless legal common-training proof is present?
3. Where does the personal/adaptive legal manifest live, and does it mirror `legal-provenance-manifest.json` with `LegScope=PersonalAdaptive`?
4. Which host is canonical for R16: new endpoint in `xiigen-platform`, or FLOW-31 in `xiigen-flows` with `xiigen-platform` as marketplace/client host?
5. Which exact open-weight hosted providers are allowed for teacher output used for training, and which are eval/judge-only?
