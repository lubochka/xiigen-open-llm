# XIIGen Registry Uncovered Document Families - Assessment

Date: 2026-05-08

Input:

`XIIGEN-REGISTRY-UNCOVERED-DOCUMENT-TYPES-2026-05-08.json`

Registry compared:

`XIIGEN-INTERNAL-SOURCE-REGISTRY-v5.md`

## Core Finding

The registry should not treat "document type" as the only classification axis.

Many XIIGen files are bundles. One file can contain architecture authority, plan ordering, task contracts, arbiter criteria, source indexes, and session state. Classifying the whole file as one type loses the DPO pair construction rule.

The next registry version should separate:

- `source_family`: what kind of file or bundle this is
- `zone`: the section pattern inside the file
- `extraction_role`: A-side, B-side chosen, rejected source, measurement evidence, vocabulary-only, or excluded
- `canonicality`: canonical source, mirror, extracted copy, stale version, external source, archive container

## High Priority New Explicit Types

These deserve explicit registry types or explicit top-level sections.

| Family | Recommendation | Why |
|---|---|---|
| U03 translation_migration_plans | New type | Best PLAN dependency-order source; rejected is wrong migration order |
| U07 flow_portability_protocols | New type | Only clear invariant-based source; rejected can be a portability violation |
| U11 agent_tooling_instructions | New type | Machine-facing executable contract, not the same as user behavioral rules |
| U05 skill_library_docs | New type | Skill boundary pairs; rejected should be wrong-skill / wrong-domain behavior |
| U14 gap_remediation_fix_docs | New type or strong Type 7/8 extension | Contains real issue -> fix -> evidence structure; rejected is wrong fix / false pass |
| U09 machine_state_json | New type or Type 4 subtype | Workflow execution state is not DPO metric evidence |
| U23 generated_code_debug_histories | New type | Natural correction source for BUG/REFACTOR/UI repair pairs |

## Zone Mapping, Not New Type

These should be explicitly split inside the document or bundle.

| Family | Zone Mapping |
|---|---|
| U01 product_business_flow_specs | ENGINE_ARCHITECTURE -> ARCH; MASTER_EXECUTION_PLAN -> PLAN; TASK_TYPES_CATALOG -> topology/routing; BFA/VALIDATION/STRESS -> arbiter/measurement; SKILLS_FACTORY_RAG -> vocabulary/index |
| U06 delivery_test_packages | GATE findings -> Type 8 arbiter; E2E plans -> TEST/PLAN; STATE.json -> execution state |
| U12 capability_catalogs_and_matrices | capability matrix -> Type 7 / knowledge-edge; source index -> provenance only; RAG index -> vocabulary calibration |
| U15 visual_ui_quality_evidence | visual findings -> Type 8; role/screen maps -> UI/ARCH B-side; UML coverage -> architecture/coverage evidence |
| U16 flow_generator_guidance_library | GUIDE-B files -> procedure/PLAN B-side; coverage reports -> Type 8/13; source maps -> provenance |
| U20 benchmark_stress_dry_run_logs | actual metrics -> Type 4; expected traces -> plan/test authority; dry-run reports -> evidence with status flag |
| U21 prompt_handoff_and_session_briefs | session start gates -> Type 15; prompt templates -> prompt-template authority; handoff contracts -> PLAN/agent behavior |

## External Or Limited Authority

These can be useful, but must not become internal XIIGen authority without source-boundary metadata.

| Family | Rule |
|---|---|
| U02 deep_research_reports | CODE_Q only unless explicitly adopted by XIIGen decision; require source/date authority |
| U04 plugin_extension_marketplace_docs | A-side prompts are useful; B-side only for platform-specific integration, not core XIIGen architecture |
| U08 design_conversion_docs | A-side UI prompts and 4.2/1.1 B-side only; not general orchestration authority |
| U22 external_training_sets_and_dpo_pairs | Keep in external registry; never mix into internal learning silently |
| U24 marketplace_legal_store_assets | Usually compliance/release metadata; not core training unless the task is marketplace/legal/package release |

## Exclude Or Preprocess First

These are not direct DPO sources.

| Family | Rule |
|---|---|
| U10 source_archives_and_snapshots | Archive is a container. Unpack, dedupe, classify contents. Do not train on zip as document. |
| U13 raw_merged_text_dumps | Segment first; then classify each segment. Never train on the merged dump directly. |
| U17 worktree_mirrors_and_duplicate_copies | Canonicalization rule, not a training source. Exclude mirrors unless canonical source is missing. |
| U18 certificates_and_environment_material | Exclude or sanitize. Treat as secure operational artifact. |
| U19 code_repository_snapshots_with_docs | Snapshot is container plus code. Unpack, dedupe, classify docs/code separately. |

## Disagreement With The 13-Family Draft

The 13-family draft is mostly right, but I would adjust three points.

1. U12 should not mostly go to Type 6. Capability matrices and task catalogs are useful as coverage-edge and routing evidence. Only source indexes and RAG indexes are vocabulary/provenance-only.

2. U08 is not high-value for orchestration, but it should still be a named source family because design conversion has geometry/layout-specific mistakes that normal Type 2 does not model.

3. U14, U15, U20, and U23 were not in the first 13-family draft and are important. They cover real repair evidence, visual QA evidence, actual/expected run evidence, and generated-code correction histories.

## Proposed Registry v6 Shape

Add a section before the type taxonomy:

```json
{
  "source_family": "business_flow_spec",
  "canonicality": "canonical | mirror | extracted_copy | stale | external | archive_container",
  "zones": [
    {
      "name": "ENGINE_ARCHITECTURE",
      "extraction_role": "B_SIDE_CHOSEN",
      "target_pair_kind": "ARCH"
    }
  ]
}
```

Then update the taxonomy with:

- new type: migration / translation plan
- new type: portability / invariant protocol
- new type: machine-facing agent instruction contract
- new type: skill behavior contract
- new type: execution state evidence
- new type: gap remediation / fix evidence
- new type: generated-code debug correction history

And add global exclusion rules:

- archive containers are never direct training rows
- worktree mirrors are excluded unless selected as canonical
- raw merged dumps must be segmented first
- external sources require external registry and cannot be silently mixed with internal teaching
- sensitive env/cert material is excluded or sanitized

