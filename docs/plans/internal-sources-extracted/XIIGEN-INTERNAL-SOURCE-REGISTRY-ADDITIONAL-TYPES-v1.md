# XIIGen Internal Source Registry - Additional Artifact Types
# Version: 1.19 - 2026-05-08
# Companion to: XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md

---
## 2026-05-11 Internal Run-Experience Fix

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

This guide must be read with the current run evidence in:
`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\RUN-STATUS.json`

Current protected baseline: RUN-416 / V246, 672 rows, weighted accuracy 0.7068,
weighted reward margin 0.016892, domains orchestration 257 / plans 341 / skills 74.
Future additions must protect orchestration, plans, and skills first.

Hard fixes from the live run:

1. Do not use deterministic filename/path code to decide semantic authority.
   Scripts may inventory, validate, split, hash, and train. They may not decide
   that a user request, `basic_prompt.txt`, session-start prompt, genesis prompt,
   or flow initiation document is low-authority without model-led registry
   adjudication.
2. Root prompt and user-intent documents are potential prompt authority. They
   belong in model-led authority adjudication, not generic review-required, when
   the script cannot type them.
3. Train-ready pairs must satisfy: `chosen != rejected`, rejected contains a
   specific `[WRONG: ...]`, no mojibake in prompt/a_side/chosen/rejected, source
   span recorded, canonicality/version state explicit.
4. Ledger, status, audit, inventory, and progress rows are evidence surfaces.
   They may justify pairs but must not be treated as DPO rows themselves.
5. New or repaired sets must run as 16-64 row standalone ablations before combo
   promotion. Promote only if reward margin is positive and weighted accuracy
   does not damage the protected baseline.
6. External code-documentation rows must not be mixed directly into internal
   orchestration/plans/skills training; they require their own code-doc gates.
7. If a document is a new type vs a new version, decide by structure, consumer
   action, A-side question, and lifecycle/canonicality. Do not decide by name
   alone.

Script references updated for this rule:

- `prepare_internal_learning_sets.py` now writes `model-led-authority-candidates.jsonl`
  for root prompt/user-intent candidates and `training-pairs-repair-required.jsonl`
  for rows blocked by hard DPO gates.
- `augment_internal_learning_sets_tier3.py` now routes B162/B165 prompt-authority
  candidates to model-led authority adjudication instead of treating them as
  ordinary low-confidence review rows.

---

## Purpose

This document turns the uncovered addendum families `A01-A51`, planning/session
families `B13-B43`, Older directory historical families `C01-C12`, Older directory
archive-content families `Z41-Z56`, conversation-derived families `D01-D19`,
second-pass session-analysis families `E01-E16`, and session-content families
`F01-F17`, runtime/archive artifact families `G01-G13`, and named session-document
families `H01-H08`, and session-message output families `I01-I14` into
registry-style handling rules, plus active-project archive families `J01-J09`
and bash/view-path discovered families `K01-K13`, remaining Older directory
repository families `L01-L08`, live training-run / teaching-cycle families
`M01-M21`, final tool-result reference families `N01-N09`, live docs-folder
examination/UX families `O01-O19`, final session-statistics families `P01-P12`,
final tool-result H1/header families `Q01-Q05`, and docs.zip internal session
process families `R01-R13`, llm_mvp_core documentation families `S01-S14`,
final docs.zip live-governance/evidence families `T01-T10`, and detailed
llm_mvp_core skill/plan families `U01-U20`, plus canonical NestJS-era
`xiigen-COMPLETE.zip` families `V01-V08`, final llm_mvp_core experiment/data
contract refinements `W01-W16`, Genie skill library package families `X01-X16`,
module-architecture / translation pipeline families `Y01-Y10`, XIIGen V18
composition-layer families `AA01-AA17`, V18 canonical migration /
architect-skill families `AB01-AB07`, compliance / flow-extension /
plugin-skill-factory families `AC01-AC20`, web-session / flow-state /
canonical-consolidation families `AD01-AD08`, and canonical extension /
V37 planning families `AE01-AE10`, flow planning / examination / artifact
verdict families `AF01-AF13`, and flow input / renumbering / merge-completion
families `AG01-AG06`.

It does **not** replace `XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md`. It is a companion
document for the artifact/document families found during the full recursive scan of:

```text
C:\Users\lubao\Documents\xiigen
```

The goal is to decide, for every additional family:

- whether it is a full source type
- whether it is limited-authority evidence
- whether it is excluded from direct DPO use
- what pair construction rule applies
- what preprocessing gate is required

---

## Scan Evidence

Inventory files produced for this registry addendum:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-FULL-FILES-AND-ZIP-INVENTORY-2026-05-08.json
C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-NESTED-ZIP-ENTRIES-2026-05-08.json
C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-RECURSIVE-ZIP-INVENTORY-DEPTHFIRST-2026-05-08.json
C:\Users\lubao\Documents\xiigen\llm mvp\intelrnal learn\XIIGEN-REGISTRY-UNCOVERED-DOCUMENT-TYPES-ADDENDUM-2026-05-08.json
```

Scan result:

```text
filesystem files scanned:      345005
top-level ZIP files found:     2030
top-level ZIP files opened:    2030
top-level ZIP failures:        0
recursive ZIP containers open: 7105
max ZIP nesting depth:         3
ZIP failures:                  8
```

The 8 recursive ZIP failures are all macOS resource-fork pseudo ZIP files under
`__MACOSX/.../._*.zip`. These are not real ZIP containers; they do not block the
classification.

---

## Classification Rule

Each additional family receives one of five statuses:

```text
full_type
full_type_candidate
limited_authority
excluded_direct
preprocess_then_reclassify
```

Definitions:

```text
full_type
  The family has stable pair construction rules and can become a numbered registry type.

full_type_candidate
  The family has strong corpus evidence and likely deserves a numbered type, but it
  still needs at least one worked DPO pair with source provenance, overlap ratio, and
  positive validation result before promotion. Promotion criterion: one accepted
  pair file plus one held-out validation example that uses the same extraction rule.

limited_authority
  The family can support DPO pairs only in a narrow scope. It must not override
  stronger XIIGen authority such as Type 2, Type 5, Type 9, Type 26, or runtime code.

excluded_direct
  The family must not enter text DPO directly. It may provide provenance,
  measurements, or binary identity only.

preprocess_then_reclassify
  The family is a container, rendered artifact, spreadsheet, PDF, or sidecar.
  Extract first, dedupe, then classify the extracted content as another source type.
```

---

### Additional Type A01 - Configuration Schema Contracts

**Registry status:** `full_type`

**Already represented in v11 as:** Type 25

**What it is:** Machine-readable schema files that define allowed fields, field
types, enum values, min/max constraints, required properties, and runtime config
shape.

**Examples in the XIIGen corpus:**

```text
genie-dna/config-schemas/*.schema.json
availability_rules.schema.json
cart_rules.schema.json
```

**Authority boundary:** Schema files are authoritative only for field-level
validity. They do not explain why a product decision was made.

**A-side use:** No. Do not use schema text as a developer prompt.

**B-side chosen use:** Yes. Use schema-valid object shape, valid enum values, and
required-field lists as chosen content.

**Rejected source:** Schema-invalid object, missing required field, wrong enum value,
wrong field type, or invented field.

**Expected overlap ratio:** 0.10-0.30.

**Excluded use:**

```text
Do not derive architecture decisions from schema files.
Do not invent enum values.
Do not use generated example config as schema authority unless it validates.
```

---

### Additional Type A02 - Dependency And Build Manifests

**Registry status:** `limited_authority`

**What it is:** Files that define project references, build graph, package versions,
target frameworks, and dependency pins.

**Examples in the XIIGen corpus:**

```text
package.json
package-lock.json
pnpm-lock.yaml
*.csproj
*.sln
*.props
*.targets
```

**Authority boundary:** Build/dependency evidence only. Dependency presence is not
architecture truth.

**A-side use:** No.

**B-side chosen use:** Yes, but only for version-pinning, project-reference, target
framework, and build-order pairs.

**Rejected source:** Wrong dependency version, missing project reference, wrong
target framework, or build order that violates project references.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not infer sub-phase ownership from dependency presence.
Do not use "latest" as chosen when a lock/manifest pins a version.
Do not treat package names as proof of runtime behavior.
```

---

### Additional Type A03 - CI, Editor, And Repository Configs

**Registry status:** `limited_authority`

**What it is:** Repository automation, editor behavior, ignore/include rules, and
workflow execution policy.

**Examples in the XIIGen corpus:**

```text
.github/workflows/*.yml
.vscode/settings.json
.gitignore
.gitattributes
.cursorrules
.clinerules
.windsurfrules
```

**Authority boundary:** CI workflows can define execution order. Tool rules can
extend Type 19. Editor preferences and ignore files rarely produce DPO pairs.

**A-side use:** No.

**B-side chosen use:** Yes for CI `needs:` ordering and tool-compliance rules.

**Rejected source:** Skipped CI prerequisite, wrong CI parallelization, ignored tool
rule, or wrong repository operation.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use .gitignore as product behavior authority.
Do not use editor settings as architecture decisions.
Do not use local tool rules if AGENTS.md supersedes them.
```

---

### Additional Type A04 - Executable Utility Scripts

**Registry status:** `limited_authority`

**What it is:** Scripts that encode process order and file handling behavior through
actual executable code paths.

**Examples in the XIIGen corpus:**

```text
translate_all_docs.py
push_to_github.py
generate_phase_a_fixtures.py
build-*.ps1
run-tests.sh
```

**Authority boundary:** Code path order is evidence. Comments are not evidence.

**A-side use:** No.

**B-side chosen use:** Yes for process-order pairs and operational procedure pairs.

**Rejected source:** Wrong step order, skipped validation step, output written before
input validation, or ledger update omitted.

**Expected overlap ratio:** 0.20-0.40.

**Excluded use:**

```text
Do not use script comments as authority.
Do not use scripts to override explicit Type 9 decisions unless the script is the
implemented source of truth and the decision is stale.
```

---

### Additional Type A05 - Source Code Contract Files

**Registry status:** `full_type`

**Already represented in v11 as:** Type 26

**What it is:** Live interface, DTO, enum, and runtime contract source code.

**Examples in the XIIGen corpus:**

```text
context/interfaces.cs
AdaptiveLogicalOrchestrator.cs
SemanticCompiler*.cs
00.Core/*.cs
```

**Authority boundary:** Strongest authority for exact runtime fields and method
contracts. Stronger than prose when the question is exact shape.

**A-side use:** No.

**B-side chosen use:** Yes for exact DTO fields, interface signatures, enum values,
and runtime contract shapes.

**Rejected source:** Inferred field name, missing required field, wrong DTO shape,
or prose-derived shape that does not match code.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not derive product rationale from source code alone.
Do not use obsolete extracted source if a newer canonical repo exists.
Do not train on generated code as chosen unless it is the accepted corrected version.
```

---

### Additional Type A06 - Model Checkpoint And Training Artifacts

**Registry status:** `excluded_direct`

**What it is:** Generated model weights, tokenizer state, checkpoint lineage, and
training run artifacts.

**Examples in the XIIGen corpus:**

```text
policy.bin
checkpoint manifests
tokenizer vocab artifacts
run checkpoint directories
```

**Authority boundary:** Binary artifacts are model state, not source documents.
Manifests can prove lineage and metrics only.

**A-side use:** No.

**B-side chosen use:** Only manifest metadata may be used, and only as Type 4 or
Type 22 evidence.

**Rejected source:** Missing manifest field, wrong parent checkpoint, or incorrect
lineage claim.

**Expected overlap ratio:** Inherits Type 4 or Type 22.

**Excluded use:**

```text
Do not train on policy.bin or tokenizer binary content.
Do not use checkpoint existence as proof of quality.
Do not mix stale raw-prompt checkpoints into compiled-prompt training claims.
```

---

### Additional Type A07 - Training Dataset Row Files

**Registry status:** `excluded_direct`

**What it is:** Already-generated learning rows, coverage ledgers, and tabular
training/test datasets.

**Examples in the XIIGen corpus:**

```text
rows.jsonl
node-*-set.jsonl
COVERAGE-LEDGER.jsonl
*.csv
```

**Authority boundary:** Dataset rows are derived artifacts. They are not original
authority unless provenance points back to a valid source type.

**A-side use:** No. If a dataset row contains an original real developer prompt, the
prompt must be traced back to the original Type 1 source and used from that source,
not from the generated row file.

**B-side chosen use:** No direct B-side use. After provenance audit, reclassify the
row's original source and apply that source type's rules. Model-generated rows remain
excluded even if they look well-formed.

**Rejected source:** Wrong row label, missing coverage field, stale row generated
from invalid guidance.

**Expected overlap ratio:** Inherits from the original source type.

**Excluded use:**

```text
Do not self-train on generated rows without provenance.
Do not count generated rows as independent evidence.
Do not use rows from failed batches as chosen content.
Do not reuse model-generated rows as chosen content even after provenance audit.
```

---

### Additional Type A08 - Spreadsheets And Decision Matrices

**Registry status:** `preprocess_then_reclassify`

**What it is:** Structured planning, comparison, mapping, and decision data stored
in spreadsheet form.

**Examples in the XIIGen corpus:**

```text
Hebrew planning workbook.xlsx
Decision-Matrix-v8-ALL-SCENES.xlsx
Figma-to-CSS-Mapping-Matrix-v*.xlsx
GraphRAG_Alternatives_Comparison.xlsx
```

**Authority boundary:** Each sheet is a separate zone. The `.xlsx` file itself is
not the pair source until extracted.

**A-side use:** No.

**B-side chosen use:** Yes after sheet extraction and zone classification.

**Rejected source:** Lower-scored alternative, wrong mapping cell, wrong metric
claim, or wrong task order.

**Expected overlap ratio:** 0.10-0.45 depending on sheet zone.

**Excluded use:**

```text
Do not process binary .xlsx as raw text.
Do not treat formatting-only sheets as evidence.
Do not treat translations as independent truth without canonical metadata.
```

---

### Additional Type A09 - Rendered HTML And Browser Captures

**Registry status:** `preprocess_then_reclassify`

**What it is:** Rendered pages, saved browser sessions, UI outputs, static site pages,
and captured prompt pages.

**Examples in the XIIGen corpus:**

```text
Attached text prompt - Claude.html
generated UI html files
index.html
privacy.html
```

**Authority boundary:** HTML is a rendered/container format. Extract visible text
or render output first.

**A-side use:** Only if extracted content contains an original real user/developer
prompt with provenance.

**B-side chosen use:** Only after extracting and classifying content by type.

**Rejected source:** Wrong rendered output, missing section, malformed HTML, or
incorrect static page claim.

**Expected overlap ratio:** Inherits extracted content type.

**Excluded use:**

```text
Do not use raw HTML markup as chosen text.
Do not classify sidecar files independently from the parent capture.
```

---

### Additional Type A10 - Visual Binary Assets And Theme Packages

**Registry status:** `excluded_direct`

**What it is:** Images, icons, fonts, visual references, screenshots, and packaged
themes.

**Examples in the XIIGen corpus:**

```text
*.png
*.svg
*.gif
*.ttf
Themes/*.zip
```

**Authority boundary:** Visual assets are not text DPO source. They can support
visual QA and provenance.

**A-side use:** No, unless a visual prompt is separately transcribed with evidence.

**B-side chosen use:** No direct text B-side.

**Rejected source:** Visual mismatch pairs require a visual extraction pipeline, not
plain text.

**Expected overlap ratio:** Not applicable.

**Excluded use:**

```text
Do not tokenize image/font binaries as training text.
Do not infer product decisions from stock/theme assets.
```

---

### Additional Type A11 - Localization And Language Variants

**Registry status:** `limited_authority`

**What it is:** Non-English or translated variants of existing documents.

**Examples in the XIIGen corpus:**

```text
README-HE.md
Hebrew planning spreadsheets
translated copies of guide documents
```

**Authority boundary:** Translation is a mirror unless explicitly declared canonical.

**A-side use:** Yes for real prompts in the original user language.

**B-side chosen use:** No, unless the localized document is the canonical source.

**Rejected source:** Wrong translation, stale translated wording, or response in
wrong canonical language.

**Expected overlap ratio:** Inherits source type.

**Excluded use:**

```text
Do not count translation as independent truth.
Do not let localized mirror override canonical source.
```

---

### Additional Type A12 - Runtime Environment And Deployment Templates

**Registry status:** `limited_authority`

**What it is:** Runtime wiring, deploy topology, service composition, and installation
descriptors.

**Examples in the XIIGen corpus:**

```text
docker-compose.yml
deployment manifests
.env samples
skill.yaml
```

**Authority boundary:** Operational contract only. Secret screening is mandatory.

**A-side use:** No.

**B-side chosen use:** Yes for service dependency, required env var, port binding,
and deployment ordering pairs.

**Rejected source:** Missing required env var, wrong startup order, wrong service
dependency, or unsafe inline credential.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Do not train on secrets or real credentials.
Do not treat deployment wiring as product architecture rationale.
```

---

### Additional Type A13 - Diagram And Topology Source Files

**Registry status:** `full_type`

**Already represented in v11 as:** Type 27

**What it is:** Machine-readable diagrams, topology graphs, dependency graphs, and
visual architecture source files.

**Examples in the XIIGen corpus:**

```text
*.mmd
*.mermaid
*.drawio
*.puml
DEPENDENCY-GRAPH.mmd
```

**Authority boundary:** Graph nodes and edges are authority. Rendered prose summary
is not.

**A-side use:** No.

**B-side chosen use:** Yes for parsed node, edge, dependency direction, and graph
membership.

**Rejected source:** Reversed edge, missing node, extra dependency, or wrong parallel
vs sequential interpretation.

**Expected overlap ratio:** 0.10-0.20.

**Excluded use:**

```text
Do not flatten diagrams to prose before preserving nodes and edges.
Do not use a screenshot of a diagram when source graph exists.
```

---

### Additional Type A14 - Runtime Debug And Tool Logs

**Registry status:** `limited_authority`

**What it is:** Raw execution logs from tools, builds, local servers, package managers,
and debugging sessions.

**Examples in the XIIGen corpus:**

```text
debug.log
firebase-debug.log
build/test console logs
tool execution logs
```

**Authority boundary:** Logs are evidence of one run, not general truth.

**A-side use:** No.

**B-side chosen use:** Yes for normalized BUG evidence when the log contains a real
failure and command context.

**Rejected source:** Wrong root cause, ignored error line, wrong command context, or
fabricated success.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Do not use raw logs with absolute private paths as training text.
Do not use timestamps/process IDs as semantic tokens.
Normalize before pair construction.
```

---

### Additional Type A15 - Test Suite Source Files

**Registry status:** `full_type`

**Already represented in v11 as:** Type 28

**What it is:** Executable tests and mocks that encode expected behavior more
precisely than prose plans.

**Examples in the XIIGen corpus:**

```text
*.test.ts
*.spec.ts
*Tests.cs
mock-figma.ts
test-settings.ts
```

**Authority boundary:** Passing assertions are authority. Skipped, ignored, or
obsolete tests are not.

**A-side use:** No.

**B-side chosen use:** Yes for expected value, contract behavior, acceptance condition,
or correct fix implied by failing assertion.

**Rejected source:** Code change that contradicts assertion, wrong expected value,
or fix that only changes the test.

**Expected overlap ratio:** 0.10-0.30.

**Excluded use:**

```text
Do not modify assertions into chosen content.
Do not use helper mocks as product behavior unless the test names that behavior.
```

---

### Additional Type A16 - Plugin Extension Manifest Files

**Registry status:** `limited_authority`

**What it is:** Plugin runtime permissions, entry points, UI surfaces, tool names,
and extension packaging metadata.

**Examples in the XIIGen corpus:**

```text
manifest.json
Figma plugin manifest
browser plugin descriptors
extension manifests
```

**Authority boundary:** Manifest defines plugin capability boundary, not core XIIGen
architecture.

**A-side use:** No.

**B-side chosen use:** Yes for plugin permission, entry point, command name, UI page,
and package capability pairs.

**Rejected source:** Permission not declared, wrong entry point, tool listed as
available when missing, or command routed to wrong surface.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not generalize plugin permissions to platform-wide permissions.
Do not use manifest as source for UX copy unless it contains that copy.
```

---

### Additional Type A17 - Office, PDF, And Exported Document Files

**Registry status:** `preprocess_then_reclassify`

**What it is:** Binary office/PDF documents whose contents may duplicate or supersede
markdown sources.

**Examples in the XIIGen corpus:**

```text
*.docx
*.pdf
*.xlsx
exported planning documents
rendered specification packets
```

**Authority boundary:** File format is not the type. Extract content, dedupe, then
classify by content.

**A-side use:** No direct use before extraction.

**B-side chosen use:** Yes only after content extraction and canonicality check.

**Rejected source:** Superseded exported version, wrong extracted table, missing
page/section, or stale PDF copy.

**Expected overlap ratio:** Inherits extracted type.

**Excluded use:**

```text
Do not train on binary payload.
Do not count exported copy and source markdown as two authorities.
```

---

### Additional Type A18 - Vendored Third-Party Codebases

**Registry status:** `excluded_direct`

**What it is:** Extracted third-party repositories embedded as reference material,
skills, examples, or vendor packages.

**Examples in the XIIGen corpus:**

```text
claude-useful-skills-master
vibecoder-master
get-shit-done-main
uiux-lib/*
```

**Authority boundary:** External boundary required. Not XIIGen internal truth unless
adopted by a Type 9 decision.

**A-side use:** No.

**B-side chosen use:** Only as external CODE_Q reference or after explicit adoption.

**Rejected source:** Outdated third-party behavior, incompatible license claim, or
mistaken adoption as XIIGen rule.

**Expected overlap ratio:** Inherits external registry type.

**Excluded use:**

```text
Do not silently mix vendored external material into internal batches.
Do not use third-party README as XIIGen architecture authority.
```

---

### Additional Type A19 - Generated UI Code Outputs

**Registry status:** `limited_authority`

**What it is:** Generated UI/code artifacts produced by conversion experiments.

**Examples in the XIIGen corpus:**

```text
*_generated.html
generated React/TSX files
generated CSS
Figma-to-code outputs
```

**Authority boundary:** Generated output is artifact evidence, not source-of-truth
specification.

**A-side use:** No.

**B-side chosen use:** Only when the generated output is the corrected/accepted
version. Otherwise it is usually rejected evidence.

**Rejected source:** Pre-debug generated output, layout-breaking code, wrong z-index,
or output that violates design mapping.

**Expected overlap ratio:** 0.25-0.50.

**Excluded use:**

```text
Do not use unreviewed generated code as chosen.
Do not infer product requirements from generator output.
```

---

### Additional Type A20 - Before/After Visual Comparison Sets

**Registry status:** `limited_authority`

**What it is:** Paired visual examples showing bad vs improved UI, layout, generated
design, or styling output.

**Examples in the XIIGen corpus:**

```text
dashboard.before.webp
dashboard.after.webp
form.before.webp
form.after.webp
```

**Authority boundary:** Visual direction must be preserved. These are visual QA
evidence, not tokenized text source.

**A-side use:** No direct text A-side.

**B-side chosen use:** Only after visual QA extraction describes the verified delta.

**Rejected source:** Reversed before/after direction, missing visual defect, or
claiming improvement without visual evidence.

**Expected overlap ratio:** Not applicable until converted to textual findings.

**Excluded use:**

```text
Do not train on images as text.
Do not infer semantic requirements from screenshots without a visual examiner.
```

---

### Additional Type A21 - Positive/Negative Example Catalogs

**Registry status:** `limited_authority`

**What it is:** Curated example banks where chosen/rejected, pass/fail, or good/bad
examples are already authored.

**Examples in the XIIGen corpus:**

```text
POSITIVE-NEGATIVE-EXAMPLES.md
CORRECTED-DEFECT-CATALOG
example catalogs
failure/success lists
```

**Authority boundary:** High-value only if provenance and human authorship are
confirmed. Synthetic catalogs inherit A07 risk.

**A-side use:** Yes if the positive/negative example includes a real prompt.

**B-side chosen use:** Yes when labels are trustworthy and source is canonical.

**Rejected source:** Existing negative example, failed example, or pre-correction
example.

**Expected overlap ratio:** 0.10-0.35.

**Excluded use:**

```text
Do not trust labels without provenance.
Do not treat model-generated examples as independent authority.
```

---

### Additional Type A22 - Algorithm Reference And Math Notes

**Registry status:** `limited_authority`

**What it is:** Algorithmic, mathematical, simulation, or ML reference material.

**Examples in the XIIGen corpus:**

```text
ml_algorithms_implementations_en.md
ML_SIMULATIONS.md
algorithm notebooks
math reference notes
```

**Authority boundary:** CODE_Q/reference only unless XIIGen explicitly adopts the
algorithm by Type 9 decision.

**A-side use:** No, unless a real developer question is present.

**B-side chosen use:** Yes for CODE_Q explaining algorithm mechanics.

**Rejected source:** Simplified wrong formula, wrong algorithm step, outdated
complexity claim.

**Expected overlap ratio:** 0.20-0.40.

**Excluded use:**

```text
Do not use as ARCH or PLAN authority without adoption decision.
Do not override runtime code with generic algorithm notes.
```

---

### Additional Type A23 - Localized User-Facing Guides

**Registry status:** `preprocess_then_reclassify`

**What it is:** Human-facing translated onboarding docs, manuals, and localized
guides.

**Examples in the XIIGen corpus:**

```text
README-HE.md
Hebrew guides
translated onboarding docs
localized user manuals
```

**Authority boundary:** Covered by A11 language rules. This is only distinct when
the document is user-facing rather than internal planning.

**A-side use:** Real user prompts in any language are valid Type 1.

**B-side chosen use:** Only if the localized guide is canonical for that locale.

**Rejected source:** Wrong locale wording, stale translation, or using translated
mirror as independent source.

**Expected overlap ratio:** Inherits content type.

**Excluded use:**

```text
Do not duplicate A11 handling.
Do not count localized guide as new truth when it mirrors canonical source.
```

---

### Additional Type A24 - Template And Boilerplate Libraries

**Registry status:** `limited_authority`

**What it is:** Reusable skeletons and templates used to generate projects, skills,
flows, or UI components.

**Examples in the XIIGen corpus:**

```text
templates/infra/*
skill templates
component templates
boilerplate project folders
```

**Authority boundary:** Template defines required output shape, placeholders, and
generation contract. It does not prove business logic.

**A-side use:** No.

**B-side chosen use:** Yes for required file/field/placeholder output shape.

**Rejected source:** Missing required template field, placeholder not filled, wrong
generated file path, or boilerplate copied without adaptation.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not treat placeholder values as real values.
Do not use template skeleton as proof of implemented behavior.
```

---

### Additional Type A25 - Infrastructure As Code Files

**Registry status:** `full_type_candidate`

**What it is:** Terraform, HCL, and infrastructure topology code that declares cloud,
container, or runtime resources.

**Examples in the XIIGen corpus:**

```text
*.tf
*.hcl
terraform-main.tf
ManagedK8s_GKE_EKS_AKS_DO.tf
```

**Authority boundary:** IaC is authoritative for declared resources and dependencies,
not for product rationale.

**A-side use:** No.

**B-side chosen use:** Yes for resource graph, dependency order, required variables,
and unsafe default detection.

**Rejected source:** Missing dependency, wrong provider/resource, hardcoded unsafe
value, or deploy order that violates resource graph.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not expose provider credentials or state files.
Do not treat IaC alternatives as chosen unless a Type 9 decision selects them.
```

---

### Additional Type A26 - Database Schema And Migration Files

**Registry status:** `full_type_candidate`

**What it is:** Database schema, migration ordering, seed SQL, and persistence
contract changes.

**Examples in the XIIGen corpus:**

```text
*.sql
supabase/migrations/*.sql
initial_schema.sql
```

**Authority boundary:** Migration order and schema definitions are storage-contract
authority.

**A-side use:** No.

**B-side chosen use:** Yes for table/column constraints, migration sequence, index
requirements, and persistence rules.

**Rejected source:** Wrong column type, missing constraint, reversed migration order,
or query that ignores tenant key.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not mix schema facts with runtime state.
Do not train on real production data dumps.
Do not treat seed/demo rows as business truth.
```

---

### Additional Type A27 - Publish Profiles And Credential-Adjacent Files

**Registry status:** `excluded_direct`

**What it is:** Deployment publishing metadata that can contain endpoints, usernames,
tokens, or environment-specific settings.

**Examples in the XIIGen corpus:**

```text
*.PublishSettings
.env
.env.sample
publish profiles
```

**Authority boundary:** Security-sensitive operational artifact.

**A-side use:** No.

**B-side chosen use:** Only sanitized required-variable names from sample files.

**Rejected source:** Missing required placeholder field, unsafe inline secret, or
wrong deploy target after sanitization.

**Expected overlap ratio:** Inherits A12 when sanitized.

**Excluded use:**

```text
Do not train on actual secrets, endpoints, usernames, passwords, private keys,
tokens, or publish credentials.
```

---

### Additional Type A28 - Coverage Binary And Test Result Artifacts

**Registry status:** `preprocess_then_reclassify`

**What it is:** Machine-generated coverage and test result artifacts that need the
owning tool to interpret them.

**Examples in the XIIGen corpus:**

```text
.coverage
coverage cache files
binary test-result outputs
```

**Authority boundary:** Aggregate metrics only after conversion. Binary content is
not source text.

**A-side use:** No.

**B-side chosen use:** No direct B-side use. After conversion, reclassify as Type 4
(measurement evidence) or Type 32 (validation report) and apply the target type's
rules.

**Rejected source:** Comes from the target type after conversion: wrong coverage
percentage, missing failed test, false pass, or coverage claim without converted
report.

**Expected overlap ratio:** Inherits Type 4 or Type 32.

**Excluded use:**

```text
Do not train on coverage binary payload.
Do not cite coverage without tool/version and converted summary.
```

---

### Additional Type A29 - Native Runtime Binary Dependencies

**Registry status:** `excluded_direct`

**What it is:** Compiled libraries and executables bundled with legacy apps, tools,
or package dependencies.

**Examples in the XIIGen corpus:**

```text
*.dll
*.so
*.dylib
*.exe
*.a
```

**Authority boundary:** Binary identity/provenance only.

**A-side use:** No.

**B-side chosen use:** No direct DPO use.

**Rejected source:** Wrong dependency identity or version claim only if metadata is
available elsewhere.

**Expected overlap ratio:** Not applicable.

**Excluded use:**

```text
Do not tokenize binaries.
Do not infer behavior from binary presence alone.
```

---

### Additional Type A30 - Git Repository Internal Metadata

**Registry status:** `excluded_direct`

**What it is:** Internal Git storage, commit logs, branch refs, object packs, and
local repository metadata.

**Examples in the XIIGen corpus:**

```text
.git/config
.git/logs/*
.git/objects/*
.git/index
```

**Authority boundary:** Use Git commands against a canonical repository instead of
training on `.git` internals. Text files such as `.git/COMMIT_EDITMSG` or
`.git/MERGE_MSG` are not object-storage internals; classify them by content only
when they are intentionally preserved as source evidence. Normal transient copies
remain excluded.

**A-side use:** No.

**B-side chosen use:** No direct use.

**Rejected source:** Wrong history claim can be constructed only from normalized
`git log` or `git show`, not raw object files.

**Expected overlap ratio:** Inherits Type 12 or Type 16 if normalized.

**Excluded use:**

```text
Do not train on .git object storage.
Do not use local branch names as canonical project history unless verified.
```

---

### Additional Type A31 - Web Server And Hosting Config Files

**Registry status:** `limited_authority`

**What it is:** Host-level routing, rewrite, static file, and server runtime
configuration.

**Examples in the XIIGen corpus:**

```text
web.config
.htaccess
*.conf
IIS config
Apache/Nginx config
```

**Authority boundary:** Deployment/runtime routing only.

**A-side use:** No.

**B-side chosen use:** Yes for route rewrites, static hosting, server variables,
and hosting constraints.

**Rejected source:** Wrong rewrite order, missing static route, unsafe hosting
setting, or route claim not present in config.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not treat hosting config as application API contract.
Screen for environment-specific paths and secrets.
```

---

### Additional Type A32 - Lock, Cache, And Partial Download Artifacts

**Registry status:** `excluded_direct`

**What it is:** Generated cache state, partial download artifacts, and lock-like
files that do not define the canonical dependency graph.

**Examples in the XIIGen corpus:**

```text
*.lock
*.cache
*.download
eslint cache
```

**Authority boundary:** Dependency lock files that define the actual dependency graph
belong to A02, not A32. `package-lock.json`, `pnpm-lock.yaml`, and equivalent package
manager lockfiles are A02 build-manifest evidence. A32 is for generated caches,
partial downloads, and lock-like temporary files.

**A-side use:** No.

**B-side chosen use:** No direct A32 use. Reclassify real dependency lockfiles as
A02. Exclude generated caches and partial downloads.

**Rejected source:** Wrong dependency version only after reclassification to A02.

**Expected overlap ratio:** Inherits A02.

**Excluded use:**

```text
Do not use caches or partial downloads as source documents.
Do not use A02 dependency lockfiles to infer architecture decisions.
```

---

### Additional Type A33 - IDE Project Metadata Files

**Registry status:** `limited_authority`

**What it is:** IDE and project settings that control local tooling behavior.

**Examples in the XIIGen corpus:**

```text
.project
.buildpath
.prefs
.editorconfig
```

**Authority boundary:** Formatting/tooling behavior only, not product truth.

**A-side use:** No.

**B-side chosen use:** Yes for formatting, toolchain, and local project setup pairs.

**Rejected source:** Wrong formatting rule, missing build path, or setup instruction
contradicting project metadata.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use IDE metadata as source for runtime behavior.
Do not override AGENTS.md with local IDE preferences.
```

---

### Additional Type A34 - Browser Export Resource Sidecars

**Registry status:** `excluded_direct`

**What it is:** Sidecar resources saved with exported browser pages: hashed blobs,
images, scripts, styles, and cached resources.

**Examples in the XIIGen corpus:**

```text
Attached text prompt - Claude_files/*
hashed extensionless resource files
HTML export sidecar folders
```

**Authority boundary:** Sidecars belong to parent HTML capture.

**A-side use:** No.

**B-side chosen use:** No independent use.

**Rejected source:** Not applicable unless parent HTML is processed and sidecar is
needed as evidence.

**Expected overlap ratio:** Not applicable.

**Excluded use:**

```text
Do not classify sidecars as independent documents.
Do not train on hashed resource blobs.
```

---

### Additional Type A35 - Non-ZIP Compressed Archives

**Registry status:** `preprocess_then_reclassify`

**What it is:** Compressed archive containers that are not ZIP.

**Examples in the XIIGen corpus:**

```text
*.tar.gz
*.gz
benchmark_complete.tar.gz
```

**Authority boundary:** Archive is a container, not source content.

**A-side use:** No.

**B-side chosen use:** Only after extraction, dedupe, and content classification.

**Rejected source:** Same as extracted content type.

**Expected overlap ratio:** Inherits extracted type.

**Excluded use:**

```text
Do not use archive filename as source title for DPO pair.
Do not train on compressed payload bytes.
```

---

### Additional Type A36 - Code Owner And Repository Governance Files

**Registry status:** `limited_authority`

**What it is:** Repository ownership, packaging exclusion, local hooks, and governance
files.

**Examples in the XIIGen corpus:**

```text
CODEOWNERS
.npmignore
pre-commit hooks
repository ownership files
```

**Authority boundary:** Governance/process evidence only. `CODEOWNERS` can identify
review routing for paths, but it is not definitive architecture ownership. If it
conflicts with a Type 9 decision, Type 26 interface boundary, or Type 27 topology
graph, the stronger source wins.

**A-side use:** No.

**B-side chosen use:** Yes for ownership routing, packaging inclusion/exclusion, and
pre-commit process rules.

**Rejected source:** Wrong owner, ignored pre-commit gate, package includes excluded
file, or review routed to wrong team.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use CODEOWNERS as definitive architecture ownership.
Do not mix repository governance with user-facing behavioral rules.
```

---

### Additional Type A37 - Typography And Font Binary Assets

**Registry status:** `excluded_direct`

**What it is:** Font files and typography binaries used by UI, canvas, design, or
rendering workflows.

**Examples in the XIIGen corpus:**

```text
*.ttf
*.woff
*.woff2
*.eot
*.otf
canvas-fonts/*.ttf
src/assets/fonts/*.ttf
```

**Authority boundary:** Font binaries are visual/runtime assets, not text authority.
They can support a design-system claim only through a manifest, CSS rule, or verified
rendering output that references the font by name.

**A-side use:** No.

**B-side chosen use:** No direct text B-side. After extraction, reclassify the
font reference as A10 visual asset evidence or A19 generated UI output evidence.

**Rejected source:** Wrong font family, missing fallback, or typography claim not
supported by CSS/design token reference.

**Expected overlap ratio:** Inherits A10 or A19 after extraction.

**Excluded use:**

```text
Do not train on font binary payload.
Do not infer product typography from a font file unless a source references it.
```

---

### Additional Type A38 - Source Maps, Debug Symbols, And Incremental Build Metadata

**Registry status:** `excluded_direct`

**What it is:** Generated debug maps and compiler state that connect built output
back to source files.

**Examples in the XIIGen corpus:**

```text
*.map
*.pdb
*.tsbuildinfo
*.pyc
*.dtbcache
```

**Authority boundary:** These files are generated artifacts. They may support
traceability from a runtime failure to original source, but they are not design,
architecture, or behavior authority by themselves.

**A-side use:** No.

**B-side chosen use:** No direct use. If a source map resolves a stack trace to an
original file and line, reclassify the resolved evidence as A14 runtime debug/log
evidence or A05 source-code contract evidence.

**Rejected source:** Wrong source-file mapping, stale build cache, or claiming a
source line without matching generated map evidence.

**Expected overlap ratio:** Inherits A14 or A05 after resolution.

**Excluded use:**

```text
Do not train on minified-source-map payload as source truth.
Do not use incremental compiler caches as proof that code is current.
```

---

### Additional Type A39 - Translation Catalog Files

**Registry status:** `limited_authority`

**What it is:** Machine-readable localization catalogs and message extraction
files, distinct from prose localization documents.

**Examples in the XIIGen corpus:**

```text
*.po
*.pot
*.mo
*.xlf
*.xliff
```

**Authority boundary:** Catalogs are authority only for message keys, source text,
translated strings, pluralization keys, and locale coverage. They do not define
product behavior.

**A-side use:** No, unless a real user prompt is embedded as a source message and
provenance is preserved.

**B-side chosen use:** Yes for localization key existence, source-to-translation
mapping, pluralization coverage, and missing-locale detection.

**Rejected source:** Missing key, stale translation, locale mismatch, wrong plural
form, or translated string that changes the source intent.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Do not treat translated UI text as architecture or API authority.
If a catalog conflicts with canonical source-language documentation, source-language wins.
```

---

### Additional Type A40 - API Description And Interface Definition Files

**Registry status:** `full_type_candidate`

**What it is:** Machine-readable API contracts and interface definition languages
that define endpoints, messages, schemas, or service boundaries.

**Examples in the XIIGen corpus:**

```text
openapi*.json
swagger*.json
asyncapi*.yaml
*.graphql
*.proto
*.thrift
postman_collection.json
```

**Authority boundary:** These files are API contract authority when canonical and
not generated from stale code. They are stronger than prose for exact route,
method, field, and message-shape claims.

**A-side use:** No.

**B-side chosen use:** Yes for API_CONTRACT pairs: endpoint method/path, request
schema, response schema, event topic, message field, required field, and compatibility
constraint.

**Rejected source:** Wrong method, missing required field, stale schema version,
wrong event topic, or response shape inferred from prose instead of contract.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use generated API docs as canonical unless generation source is confirmed.
Do not infer business workflow from API shape alone.
```

---

### Additional Type A41 - Browser, Test, And Interaction Trace Artifacts

**Registry status:** `preprocess_then_reclassify`

**What it is:** Browser automation traces, screenshots, videos, and test-run
recordings that show how a UI or flow behaved during validation.

**Examples in the XIIGen corpus:**

```text
trace.zip
playwright-report/*
test-results/*
*.mp4
screenshot-*.png
junit*.xml
*.trx
*.sarif
```

**Authority boundary:** Raw traces are not direct DPO text. They become evidence
only after a trace reader, test report parser, or visual QA extraction produces a
structured finding.

**A-side use:** No.

**B-side chosen use:** After preprocessing, reclassify as Type 8, A15, A20, or A28
depending on whether the extracted fact is a review finding, test assertion, visual
delta, or metric.

**Rejected source:** Wrong clicked element, skipped assertion, false pass, reversed
visual before/after, or trace step not present in the artifact.

**Expected overlap ratio:** Inherits target type after extraction.

**Excluded use:**

```text
Do not train on raw trace/video payload.
Do not cite a screenshot as passing evidence without the expected assertion.
```

---

### Additional Type A42 - Certificates, Keys, And Security Material

**Registry status:** `excluded_direct`

**What it is:** Certificate requests, public certificates, private keys, signing
material, tokens, and secret-adjacent security files.

**Examples in the XIIGen corpus:**

```text
*.crt
*.csr
*.pem
*.key
*.pfx
*.cer
```

**Authority boundary:** Security material is never training text. Public certificate
metadata can be used only after safe extraction of subject, issuer, expiry, and
intended environment.

**A-side use:** No.

**B-side chosen use:** No direct use. Sanitized metadata may reclassify to A12
runtime environment evidence or Type 8 security review evidence.

**Rejected source:** Expired certificate claim, wrong environment, missing rotation
requirement, or treating a private key as shareable material.

**Expected overlap ratio:** Inherits A12 or Type 8 after safe metadata extraction.

**Excluded use:**

```text
Do not include private keys, tokens, passwords, or raw certificate bodies in DPO rows.
Do not train on secret-bearing files even if they appear in archived corpora.
```

---

### Additional Type A43 - Package Publication And Registry Metadata

**Registry status:** `limited_authority`

**What it is:** Files that define how a package is named, versioned, published, or
listed in a package registry or marketplace.

**Examples in the XIIGen corpus:**

```text
*.nuspec
.npmrc
package metadata
plugin marketplace metadata
publish profile metadata after sanitization
```

**Authority boundary:** Publication metadata is authority for package identity,
entry points, versioning, registry target, and included files. It is not authority
for runtime architecture unless the package manifest explicitly declares an entry
point or dependency.

**A-side use:** No.

**B-side chosen use:** Yes for package name, version, publish target, file inclusion,
declared entry point, and registry compatibility.

**Rejected source:** Wrong package id, missing file include, unsafe publish target,
version mismatch, or entry point not declared in the package metadata.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Screen for credentials before extraction.
If package metadata conflicts with source code contracts, source code wins for behavior.
```

---

### Additional Type A44 - Platform Entitlement And Application Manifest Files

**Registry status:** `limited_authority`

**What it is:** Platform-level application manifests, entitlement declarations,
permission files, and runtime capability declarations.

**Examples in the XIIGen corpus:**

```text
*.plist
*.entitlements
AndroidManifest.xml
app.manifest
application manifest XML
```

**Authority boundary:** These files are authority for platform permission,
capability, bundle identity, and runtime entitlement claims only.

**A-side use:** No.

**B-side chosen use:** Yes for permission/entitlement/capability presence,
platform bundle id, required runtime capability, and sandbox limitation.

**Rejected source:** Undeclared permission, missing entitlement, wrong bundle id,
or claiming a platform capability that is not declared.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not infer internal architecture from platform entitlement files.
Treat secrets or team identifiers as sensitive metadata.
```

---

### Additional Type A45 - View Template And Legacy Web Markup Files

**Registry status:** `limited_authority`

**What it is:** Runtime view templates, server-rendered page markup, and legacy
web UI files that bind data to rendered output.

**Examples in the XIIGen corpus:**

```text
*.hbs
*.jade
*.aspx
*.asp
*.asax
*.xaml
*.tag
*.tt
```

**Authority boundary:** These files are authority for rendered fields, bindings,
UI conditions, and template placeholders. They are not authority for business logic
unless the logic is explicitly encoded in the template.

**A-side use:** No, unless the file embeds a real user/developer prompt as example
content with provenance.

**B-side chosen use:** Yes for template field presence, binding name, conditional
rendering rule, placeholder requirement, and view-level validation message.

**Rejected source:** Missing binding, wrong placeholder, view claims not present
in template, or treating generated markup as source template.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Do not use view templates as API or database schema authority.
If a generated HTML export exists, the source template wins.
```

---

### Additional Type A46 - Bulk Event, Search, And Stream Export Files

**Registry status:** `preprocess_then_reclassify`

**What it is:** Newline-delimited JSON, bulk search exports, event streams, and
append-only machine records.

**Examples in the XIIGen corpus:**

```text
*.ndjson
*.jsonl
progress.jsonl
batch-summary.jsonl
elasticsearch bulk export
redis stream export
```

**Authority boundary:** Stream files are containers. Each record must be parsed,
provenance-checked, and reclassified as dataset row, log, metric, state evidence,
or source prompt.

**A-side use:** Only if a parsed record contains an original real prompt and the
record provenance points back to an allowed source.

**B-side chosen use:** No direct stream-level B-side. Parsed records reclassify to
A07, A14, Type 4, Type 11, Type 32, or another specific source type.

**Rejected source:** Missing stream field, wrong event order, stale batch summary,
or false metric after record-level extraction.

**Expected overlap ratio:** Inherits target type after record classification.

**Excluded use:**

```text
Do not treat a JSONL file as one prompt.
Do not mix records from different runs into one DPO pair without run provenance.
```

---

### Additional Type A47 - Static Build Output Bundles And Generated Asset Manifests

**Registry status:** `excluded_direct`

**What it is:** Compiled frontend bundles, generated static assets, minified files,
asset manifests, and distribution output directories.

**Examples in the XIIGen corpus:**

```text
dist/*
build/*
*.min.js
*.min.css
asset-manifest.json
vite/webpack output
```

**Authority boundary:** Generated bundles are delivery artifacts. They can support
runtime verification only when traced back to source maps, source files, or accepted
visual output.

**A-side use:** No.

**B-side chosen use:** No direct use. Reclassify source-mapped claims through A38
or visual claims through A19/A20.

**Rejected source:** Wrong bundle inclusion, stale generated asset, missing output
file, or treating minified output as source behavior.

**Expected overlap ratio:** Inherits A19, A20, or A38 after trace-back.

**Excluded use:**

```text
Do not train on bundled/minified files as source truth.
Do not infer source-level architecture from generated asset names.
```

---

### Additional Type A48 - License, Notice, And Provenance Files

**Registry status:** `limited_authority`

**What it is:** License texts, notice files, attribution files, copying files, and
third-party provenance manifests.

**Examples in the XIIGen corpus:**

```text
LICENSE
NOTICE
COPYING
THIRD_PARTY_NOTICES
licenses/*
attribution files
```

**Authority boundary:** These files are authority for allowed use, required notice,
attribution, and provenance. They are not functional implementation authority.

**A-side use:** No.

**B-side chosen use:** Yes for license requirement, attribution requirement,
redistribution limitation, and whether a source can be used for training or external
sets.

**Rejected source:** Missing attribution, wrong license family, treating a restricted
source as reusable, or omitting required notice.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not turn license prose into architecture or CODE_Q truth.
If license status is ambiguous, exclude the source until reviewed.
```

---

### Additional Type A49 - Per-Flow Codebase State Review Documents

**Registry status:** `limited_authority`

**What it is:** Documents that capture the implementation state of a specific flow
or codebase slice at a point in time: implemented files, missing files, partial
contracts, and observed gaps.

**Examples in the XIIGen corpus:**

```text
FLOW-16-CODEBASE-STATE-REVIEW.md
FLOW-19-CODEBASE-STATE-REVIEW.md
FLOW-20-CODEBASE-STATE-REVIEW.md
FLOW-21-CODEBASE-STATE-REVIEW.md
FLOW-23-CODEBASE-STATE-REVIEW.md
CODEBASE-STATE-REVIEW-2026-04-13.md
```

**Authority boundary:** These are snapshot evidence, not timeless architecture
authority. They can support Type 7/Type 23 style pairs when the review cites concrete
files or missing code, but live source code wins for exact current implementation.

**A-side use:** No.

**B-side chosen use:** Yes for per-flow implemented/missing status, gap detection,
and remediation-priority pairs when the snapshot date and cited file evidence are
recorded.

**Rejected source:** Claiming a missing component is implemented, claiming an
implemented file is absent, using stale review state as current truth, or omitting
the date/scope of the review.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Do not use old state reviews as current code truth without live verification.
Do not merge multiple flow reviews into one pair without per-flow scope.
```

---

### Additional Type A50 - Architect Design Review And Code Review Verdict Documents

**Registry status:** `preprocess_then_reclassify`

**What it is:** Review-round documents that evaluate existing design/code, record
findings, and produce verdicts across R0/R1/architect review passes.

**Examples in the XIIGen corpus:**

```text
FLOW-46-DESIGN-AND-CODE-REVIEW-R1.md
FLOW-47-DESIGN-REVIEW-AND-CODE-REVIEW-R1.md
FLOW-47-DESIGN-REVIEW-R0.md
FLOW-47-v1.3-ARCHITECT-VERDICT.md
FLOW-47-DESIGN-REVIEW-VERDICT.html
```

**Authority boundary:** The document is a container with review zones. Verdict
findings reclassify to Type 32 validation reports or Type 8 artifact examination.
Design gap lists reclassify to Type 23 remediation. Architecture statements must
not override Type 9 decisions or live Type 26 contracts.

**A-side use:** No.

**B-side chosen use:** After zone extraction, yes for review finding, validation
verdict, design gap, and required remediation pair construction.

**Rejected source:** False pass, ignored blocking finding, wrong review round,
collapsed R0/R1 distinction, or treating a design gap list as an implementation
plan without remediation sequencing.

**Expected overlap ratio:** Inherits Type 8, Type 23, or Type 32 after zone mapping.

**Excluded use:**

```text
Do not train on the whole review document as one row.
Extract verdict, finding, evidence, and remediation zones separately.
```

---

### Additional Type A51 - Raw Claude Code Execution Session Captures

**Registry status:** `preprocess_then_reclassify`

**What it is:** Unsegmented plain-text captures of Claude Code or planning sessions
that mix human prompts, assistant responses, tool calls, command output, build logs,
and proposed plans.

**Examples in the XIIGen corpus:**

```text
planning session.txt
claude code execution session.txt
architecture gap/claude code session.txt
architecture gap/claude code session 2.txt
```

**Authority boundary:** Raw session captures are containers, not source rows. They
must be segmented by turn boundary, tool-call boundary, command-output boundary, or
timestamp marker before classification.

**A-side use:** Only human/developer turns with preserved boundary markers may
become Type 1 prompts.

**B-side chosen use:** Assistant text is not automatically chosen. It may become
Type 2, Type 3, Type 8, Type 23, Type 29, or Type 32 only after the segment is
validated against its artifact, code, or accepted session outcome.

**Rejected source:** Mixed-turn contamination, using command output as assistant
claim, missing tool boundary, stale proposal accepted as if executed, or treating
the whole transcript as one prompt.

**Expected overlap ratio:** Inherits target type after segmentation.

**Excluded use:**

```text
Do not use raw session text directly as a training row.
Do not combine Human, Assistant, and tool output into the same A-side.
```

---

## Planning ZIP Document Families

The `B13-B43` families below come from the expanded planning ZIP and session
package classification passes. These are document-family rules, not raw extension rules.
When a B-family conflicts with a broad A-family rule, the more specific B-family
rule wins for the named planning documents.

---

### Planning Type B13 - Codebase State Review Documents

**Registry status:** `full_type`

**What it is:** Per-flow codebase audit documents with status matrices, severity
ratings, concrete file findings, and fix prescriptions.

**Examples in the XIIGen corpus:**

```text
FLOW-16-CODEBASE-STATE-REVIEW.md
FLOW-19-CODEBASE-STATE-REVIEW.md
FLOW-20-CODEBASE-STATE-REVIEW.md
FLOW-21-CODEBASE-STATE-REVIEW.md
FLOW-23-CODEBASE-STATE-REVIEW.md
CODEBASE-STATE-REVIEW-2026-04-13.md
```

**Authority boundary:** This is stronger than generic A49 when the document has
severity ratings and file-level evidence. It is still snapshot authority: live code
wins for current exact implementation.

**A-side use:** No.

**B-side chosen use:** Yes for audit finding, severity, per-flow implemented/missing
status, concrete remediation prescription, and file-level evidence pairs.

**Rejected source:** Wrong severity, claiming an absent file exists, claiming a
fixed issue remains open, or using stale snapshot state without date/scope.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not merge findings from multiple flows into one row.
Do not use old state as current truth without live verification.
```

---

### Planning Type B14 - Architect And Design Review Verdict Documents

**Registry status:** `full_type`

**What it is:** Formal architect/design review verdicts with finding tables,
evidence verification, binary readiness answers, and reasoning chains.

**Examples in the XIIGen corpus:**

```text
FLOW-47-v1.3-ARCHITECT-VERDICT.md
PLAN-REVIEW-VERDICT-v1.md
FLOW-47-DESIGN-REVIEW-R0.md
FLOW-46-DESIGN-AND-CODE-REVIEW-R1.md
FLOW-47-DESIGN-REVIEW-AND-CODE-REVIEW-R1.md
```

**Authority boundary:** More specific than generic A50. Use as architect judgment
authority when the verdict cites code, file, or plan evidence. It does not override
canonical Type 9 architecture decisions unless the verdict explicitly supersedes
them.

**A-side use:** No.

**B-side chosen use:** Yes for review verdict, blocker/non-blocker classification,
evidence-backed finding, readiness answer, and reasoning-chain pairs.

**Rejected source:** False pass, ignored blocker, wrong review round, missing
evidence citation, or verdict claim without the reasoning chain.

**Expected overlap ratio:** 0.15-0.25.

**Excluded use:**

```text
Do not flatten R0/R1/final verdict into one source state.
Do not treat architect judgment as executed implementation.
```

---

### Planning Type B15 - Episode Map And Sub-Episode Narratives

**Registry status:** `full_type`

**What it is:** Narrative maps of working episodes: user goal, decomposition,
correction loop, artifacts created, and why each step happened.

**Examples in the XIIGen corpus:**

```text
EPISODE-MAP-*.md
SUB-EPISODE-MAP.md
SUB-EPISODE-EXPANSION-BATCH*.md
EPISODE-EXAMINATION-GUIDANCE.md
DEPTH-CALIBRATION-GUIDANCE.md
```

**Authority boundary:** This is the narrative-level orchestration source for
"how this kind of work was solved before." Guidance files define writing rules;
episode maps define the actual B-side examples.

**A-side use:** Real developer questions that match the episode intent may be used
as Type 1 prompts.

**B-side chosen use:** Yes for stage decomposition, problem-solving arc, correction
loop, artifact purpose, and orchestration-advisor RAG pairs.

**Rejected source:** Feature catalog without problem-solving arc, wrong episode
mapping, missing correction loop, or shallow summary that omits why the step exists.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Do not use episode guidance as if it were an episode result.
Do not collapse multiple episodes into one DPO row.
```

---

### Planning Type B16 - Flow Hypothesis Matrix Documents

**Registry status:** `full_type`

**What it is:** Fleet-level matrices that compare flows against hypotheses with
machine-readable verdict codes and readiness/coupling evidence.

**Examples in the XIIGen corpus:**

```text
FLOW-HYPOTHESIS-MATRIX*.md
FLOW-ROLE-ANALYSIS-*.md
```

**Authority boundary:** This is fleet-level comparative ARCH evidence. It is not
per-flow implementation truth unless the cell cites implementation evidence.

**A-side use:** No.

**B-side chosen use:** Yes for marketplace readiness, coupling defect, portability,
cross-flow dependency, fleet comparison, and hypothesis-verdict pairs.

**Rejected source:** Fabricated readiness, missing coupling analysis, wrong verdict
code, or treating a role-analysis draft as final matrix truth.

**Expected overlap ratio:** 0.10-0.20.

**Excluded use:**

```text
Do not infer individual file implementation from matrix code alone.
Record matrix version and hypothesis id for every pair.
```

---

### Planning Type B17 - Batch Reconnaissance Reports

**Registry status:** `limited_authority`

**What it is:** Pre-execution reconnaissance reports with fleet headline numbers,
per-flow cell counts, priority breakdowns, and proposed batch plan.

**Examples in the XIIGen corpus:**

```text
BATCH-5-RECON.md
BATCH-*-RECON.md
```

**Authority boundary:** Zone-mapped only: fleet metrics go to Type 4, per-flow gaps
to Type 23, and batch execution sequence to Type 29.

**A-side use:** No.

**B-side chosen use:** Yes after zone split for metric, gap, or execution-plan rows.

**Rejected source:** Wrong completion percentage, wrong priority count, missing
per-flow gap, or turning recon metrics into final validation.

**Expected overlap ratio:** Inherits Type 4, Type 23, or Type 29.

**Excluded use:**

```text
Do not treat recon as post-execution evidence.
Do not use a whole recon document as one row.
```

---

### Planning Type B18 - Carry-Forward Issue Documents

**Registry status:** `full_type`

**What it is:** Forward-looking issue trackers for known problems that must be
fixed in later sessions before packaging or release.

**Examples in the XIIGen corpus:**

```text
CARRY-FORWARD-ISSUES-v1.0.md
CARRY-FORWARD-ISSUES*.md
```

**Authority boundary:** Strong source for correction pairs because it records
priority, affected file/lines, fix description, status, and impact if not fixed.

**A-side use:** Real developer question about the same behavioral bug may be used
as Type 1.

**B-side chosen use:** Yes for BUG correction, impact annotation, priority, fix
target, and downstream failure mode.

**Rejected source:** Old broken behavior, ignored impact, wrong priority, or fix
that does not address the recorded downstream failure.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not mark an issue fixed without the cited fix evidence.
Do not drop the impact-if-not-fixed annotation.
```

---

### Planning Type B19 - Corrected Defect Catalogs And Complete Issue Registries

**Registry status:** `full_type`

**What it is:** Root-cause defect catalogs and issue registries with corrected
defect classes, evidence, regression rules, and "why it was not fixed" notes.

**Examples in the XIIGen corpus:**

```text
CORRECTED-DEFECT-CATALOG.md
COMPLETE-ISSUE-REGISTRY.md
COMPLETE-ISSUE-REGISTRY-v3.md
```

**Authority boundary:** Extends A21 example catalogs with root-cause analysis. Use
for defect-class and reasoning-error pairs, not general architecture.

**A-side use:** Real defect prompt may be used as Type 1 when present.

**B-side chosen use:** Yes for root cause, corrected defect class, regression rule,
visibility in prior session, and remediation annotation.

**Rejected source:** Wrong root cause, missing "why not fixed" reason, treating a
symptom as root cause, or losing the regression rule.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not convert root-cause catalogs into generic positive/negative examples.
Keep defect_class and reasoning_error metadata.
```

---

### Planning Type B20 - Fix Regression Reports

**Registry status:** `full_type`

**What it is:** Formal regression analyses that compare baseline behavior with
post-fix behavior and show unintended breakage caused by a fix.

**Examples in the XIIGen corpus:**

```text
FIX-27-REGRESSION-REPORT.md
FIX-*-REGRESSION-REPORT.md
```

**Authority boundary:** Type 23 extension with three states: baseline, wrong fix,
and regression evidence.

**A-side use:** No, unless the report includes the original developer correction
request as a distinct prompt.

**B-side chosen use:** Yes for regression-aware fix pairs where chosen preserves
baseline behavior and rejected reproduces the bad fix.

**Rejected source:** Wrong fix that breaks other nodes, missing baseline comparison,
or declaring regression absent without before/after evidence.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not train only before/after text without naming the regression mechanism.
Do not treat the bad fix as chosen.
```

---

### Planning Type B21 - A-To-B Source Map Documents

**Registry status:** `limited_authority`

**What it is:** Two-level mapping tables from List A source archives/documents to
List B canonical guide types, with roles such as PRIMARY, REFERENCE, STRUCTURE,
FIXTURE, and EVIDENCE.

**Examples in the XIIGen corpus:**

```text
A-TO-B-SOURCE-MAP.md
A-TO-B-SOURCE-MAP-v2.md
A-TO-B-AMENDMENT-MAP-ZIP17.md
```

**Authority boundary:** Type 31 extension for source attribution. It answers "which
source supports which guide type", not the domain question itself.

**A-side use:** No.

**B-side chosen use:** Yes for source attribution, primary/reference distinction,
and guide-source routing pairs.

**Rejected source:** Wrong source attribution, reversed A/B relation, treating a
REFERENCE as PRIMARY, or omitting amendment scope.

**Expected overlap ratio:** 0.05-0.15.

**Excluded use:**

```text
Do not use source maps as content authority for the mapped topic.
Use them to locate authority, then classify the located source.
```

---

### Planning Type B22 - Merge Conflict Resolution Maps

**Registry status:** `full_type`

**What it is:** Documents that resolve contradictory facts between versions before
merge, with authoritative value, overridden value, and source citation.

**Examples in the XIIGen corpus:**

```text
CONFLICT-MAP.md
CONFLICT-MAP-*.md
```

**Authority boundary:** Type 33 extension for conflict resolution, not general
comparison. The chosen value is the authoritative resolution; the rejected value is
the overridden conflicting claim.

**A-side use:** No.

**B-side chosen use:** Yes for contradiction resolution, authoritative value, source
precedence rule, and merge correction pairs.

**Rejected source:** Overridden number, wrong winning source, missing citation, or
retaining both conflicting values as if both are true.

**Expected overlap ratio:** 0.05-0.15.

**Excluded use:**

```text
Do not use unresolved conflict maps as final authority.
Record winning source and overridden source in metadata.
```

---

### Planning Type B23 - V17 Skill Alternative Implementations

**Registry status:** `limited_authority`

**What it is:** Multi-language alternative implementations for the same XIIGen
skill, usually under `alternatives/` folders.

**Examples in the XIIGen corpus:**

```text
xiigen_v18_complete/V17-skills/*/Implementation/*
xiigen_v18_complete/V17-skills/*/alternatives/java/*
xiigen_v18_complete/V17-skills/*/alternatives/php/*
xiigen_v18_complete/V17-skills/*/alternatives/rust/*
xiigen_v18_complete/V17-skills/*/alternatives/nodejs/*
```

**Authority boundary:** Canonical implementation is the primary implementation path
unless a Type 9 adoption decision promotes an alternative. Alternatives are useful
for IMPLEMENT portability pairs only.

**A-side use:** Real developer porting prompts may be used as Type 1.

**B-side chosen use:** Yes for implementation-porting pairs where chosen preserves
the canonical behavior in the requested language.

**Rejected source:** Platform-specific alternative that loses canonical behavior,
wrong language contract, or treating an alternative as architecture authority.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use alternatives as ARCH authority.
Do not choose non-canonical implementation unless adoption is explicit.
```

---

### Planning Type B24 - Episode Query Mapping Documents

**Registry status:** `limited_authority`

**What it is:** Mapping from canonical episode descriptions to alternative natural
language phrasings that developers might actually ask.

**Examples in the XIIGen corpus:**

```text
EPISODE-QUERY-MAPPING.md
EPISODE-QUERY-MAPPING-*.md
```

**Authority boundary:** A-side vocabulary expansion only. The B-side answer must
come from the matching B15 episode map, not from the query map itself.

**A-side use:** Yes. Alternative phrasings are valid prompt variants when mapped to
an episode id.

**B-side chosen use:** No direct B-side use. Use B15 episode map as chosen.

**Rejected source:** Wrong episode mapping, query phrase mapped to wrong episode,
or treating a query phrase as an answer.

**Expected overlap ratio:** Not applicable for B-side.

**Excluded use:**

```text
Do not use query maps as content authority.
Always record the target episode id.
```

---

### Planning Type B25 - Delivery Summary And Spec Location Maps

**Registry status:** `full_type`

**What it is:** Post-delivery summaries and maps that say where each design question
or specification answer lives.

**Examples in the XIIGen corpus:**

```text
DELIVERY_SUMMARY.md
SPEC-LOCATION-MAP.md
```

**Authority boundary:** DELIVERY_SUMMARY is Type 32-style post-delivery assessment.
SPEC-LOCATION-MAP is Type 13-style document-state routing for design/spec answers.

**A-side use:** No.

**B-side chosen use:** Yes for strongest approach selection, weakness comparison,
artifact completeness, and source-location lookup pairs.

**Rejected source:** Wrong source file for a design question, weaker approach chosen
as stronger, missing weakness note, or delivery summary used as implementation proof.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use delivery summary as source code truth.
For spec answers, follow the location map to the target document before extracting content.
```

---

### Planning Type B26 - Skill Reference Subdocuments

**Registry status:** `full_type`

**What it is:** Reference sections and rule appendices split out from skills into
separate documents: failure catalogs, detection commands, activation rules,
engine contracts, fabric interfaces, flow registry references, and governance rules.

**Examples in the XIIGen corpus:**

```text
plan-review-skill--references-failure-class-catalog.md
plan-review-skill--references-detection-commands.md
skill-advisor-skill--references-activation-rules.md
xiigen-engine--references-engine-contracts.md
xiigen-engine--references-fabric-interfaces.md
xiigen-flow-builder--references-flow-registry.md
no-product-decisions--rules-*.md
agent-constitution--rules-*.md
dev-safety--rules-*.md
```

**Authority boundary:** A skill subdocument inherits the parent skill's authority
scope. Governance rule appendices behave like Type 14/Type 19. Engine and fabric
references may support Type 2/Type 26 architecture-contract pairs when they cite
stable contracts.

**A-side use:** No, unless a real user prompt appears as a clearly marked example.

**B-side chosen use:** Yes for governance rule, failure class, detection command,
engine contract, fabric interface, and skill-boundary pairs.

**Rejected source:** Wrong failure class, skipped detection command, rule applied
outside its skill boundary, or treating a subdoc as global authority when it belongs
to one parent skill.

**Expected overlap ratio:** 0.10-0.30.

**Excluded use:**

```text
Record parent skill name for every row.
Do not use a subdocument without its parent skill context.
```

---

### Planning Type B27 - Session Load Plans And Start Configuration Guides

**Registry status:** `full_type`

**What it is:** Versioned session gate/load documents that define which skills,
state files, invariants, and prompts must be loaded at session start and in what
order.

**Examples in the XIIGen corpus:**

```text
HOW-TO-USE-SKILLS-v5.3.md
XIIGEN-SESSION-LOAD-PLAN-v16.md
XIIGEN-SESSION-LOAD-PLAN-v23.md
XIIGEN-SESSION-LOAD-PLAN-v32.md
XIIGEN-SESSION-START-PROMPT-v2.md
XIIGEN-SESSION-START-PROMPT-v5.0.md
XIIGEN-SESSION-SETUP-LIBRARY.md
```

**Authority boundary:** Extends Type 15. These documents are authority for load
sequence, required preconditions, session invariants, and startup checks; they are
not authority for implementation behavior after the session begins.

**A-side use:** No.

**B-side chosen use:** Yes for PLAN Type B pairs about session loading order,
required context, invariant checks, and what must be read before execution.

**Rejected source:** Wrong load order, missing invariant, skipped required skill,
or using a stale session-start version as canonical.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Do not mix session load plan with session state.
Record version and canonicality for every row.
```

---

### Planning Type B28 - Operational Procedure And Governance Protocol Documents

**Registry status:** `full_type`

**What it is:** Procedure documents that tell the agent how to conduct architect
sessions, code reviews, response construction, golden-rule enforcement, and other
operational gates.

**Examples in the XIIGen corpus:**

```text
XIIGEN-DESIGN-ARCHITECT-SESSION-GUIDE-v1.9.md
XIIGEN-CODE-REVIEW-PROTOCOL-v2.1.md
XIIGEN-GOLDEN-RULE.md
XIIGEN-RESPONSE-CONSTRUCTION-PROTOCOL-v1.md
XIIGEN-V2-MASTER-PLAN.md
```

**Authority boundary:** Extends Type 19. These are machine-facing procedures, not
user-facing behavior rules. Use for required steps, gate sequences, and fail
conditions.

**A-side use:** No.

**B-side chosen use:** Yes for operational ordering, mandatory gate, failure
condition, review protocol, and response-construction rule pairs.

**Rejected source:** Skipped required step, wrong review posture, using a golden
rule as optional, or confusing user-facing behavior with agent procedure.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use operational protocol as product architecture authority.
Do not convert a procedure document into a session-state document.
```

---

### Planning Type B29 - Behavioral Correction Registries

**Registry status:** `full_type`

**What it is:** Formatted registries of real behavioral corrections: wrong agent
behavior, concrete example, corrected rule, and when it must be loaded.

**Examples in the XIIGen corpus:**

```text
BEHAVIORAL-CORRECTIONS-REGISTRY.md
CARRY-FORWARD-ISSUES.md
```

**Authority boundary:** Strong Type 3 extension. Unlike raw correction events, this
is a curated registry with pre-made rejected and chosen behavior.

**A-side use:** No, unless the original user correction is quoted as a separate
source prompt.

**B-side chosen use:** Yes for behavioral_correction pairs: corrected behavior,
wrong prior behavior, trigger condition, and must-load rule.

**Rejected source:** The recorded wrong behavior, repeated systematic error, or
answer that ignores the correction registry.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not drop the concrete wrong-behavior example.
Do not treat a correction registry as optional guidance.
```

---

### Planning Type B30 - Plan Review Templates

**Registry status:** `limited_authority`

**What it is:** Reusable plan-review templates and gate checklists with placeholders,
failure classes, and acceptance criteria.

**Examples in the XIIGen corpus:**

```text
SESSION-0-PLAN-REVIEW-TEMPLATE.md
plan-review-skill--SESSION-0-PLAN-REVIEW-TEMPLATE.md
```

**Authority boundary:** Type 34 extension. Template defines review completeness,
not a concrete project finding until filled.

**A-side use:** No.

**B-side chosen use:** Yes for review_template pairs: required checklist field,
failure-class check, acceptance criterion, and missing gate detection.

**Rejected source:** Missing required placeholder, skipped failure class, template
used as if filled for a project, or false complete review.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not treat template placeholders as project facts.
Pair metadata must say template, not concrete review.
```

---

### Planning Type B31 - Examination Guidance Documents

**Registry status:** `preprocess_then_reclassify`

**What it is:** Large procedural guidance for examining flows, hypotheses,
episodes, and sub-episodes, often with examples and criteria.

**Examples in the XIIGen corpus:**

```text
Flow_examination_plan_enriched.md
SUB-EPISODE-EXAMINATION-GUIDANCE.md
```

**Authority boundary:** Multi-zone procedure document. Hypothesis sections become
ARCH pairs, examination steps become PLAN Type B pairs, and finding criteria become
Type 8 arbiter pairs.

**A-side use:** No, except clearly marked real developer examples.

**B-side chosen use:** Only after zone mapping to Type 2, Type 8, Type 23, or PLAN
Type B.

**Rejected source:** Wrong hypothesis tracking, skipped examination step, shallow
sub-episode decomposition, or using examples as canonical findings.

**Expected overlap ratio:** Inherits target zone type.

**Excluded use:**

```text
Do not train the whole guidance document as one row.
Zone id and target type are mandatory.
```

---

### Planning Type B32 - Document Authority And Provenance Maps

**Registry status:** `full_type`

**What it is:** Maps that define canonical document homes and which document is
authoritative for a given governance, protocol, or design concept.

**Examples in the XIIGen corpus:**

```text
DOCUMENT-AUTHORITY-MAP.md
DOCUMENT-AUTHORITY-MAP-v2.0.md
```

**Authority boundary:** Strong Type 31 extension for concept-level authority. It
answers "which document is canonical for this concept." It still locates authority;
the target document remains the content source for the concept itself.

**A-side use:** No.

**B-side chosen use:** Yes for document_provenance pairs: canonical home, authority
source, cross-reference versus canonical-source distinction, and stale-source
rejection.

**Rejected source:** Wrong canonical home, using a mirror as primary, treating a
cross-reference as canonical authority, or treating the map as content authority.

**Expected overlap ratio:** 0.05-0.15.

**Excluded use:**

```text
After routing, extract content from the target document.
Do not answer domain questions from the map alone.
```

---

### Planning Type B33 - Architect Session Infrastructure Fix Plans

**Registry status:** `limited_authority`

**What it is:** Phase-by-phase remediation plans for session infrastructure with
targets, results, dependencies, and versioned fixes.

**Examples in the XIIGen corpus:**

```text
ARCHITECT-SESSION-INFRASTRUCTURE-FIX-PLAN.md
ARCHITECT-SESSION-INFRASTRUCTURE-FIX-PLAN-v9.1.md
ARCHITECT-SESSION-INFRASTRUCTURE-FIX-PLAN-v10.md
```

**Authority boundary:** Type 29 extension. Latest canonical version wins; older
versions are stale unless used as evolution evidence.

**A-side use:** No.

**B-side chosen use:** Yes for phase ordering, dependency, target/result, and
versioned remediation-plan pairs.

**Rejected source:** Stale version used as canonical, wrong dependency order,
omitted target/result, or phase executed before prerequisite.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Record canonical version before pair construction.
Do not mix stale and current phase lists.
```

---

### Planning Type B34 - Business Flow Specification Documents

**Registry status:** `limited_authority`

**What it is:** Full business-flow specifications that combine domain, wave,
prerequisites, task types, engine contracts, and execution notes.

**Examples in the XIIGen corpus:**

```text
FLOW-40-CLIENT-PUSH-INFRASTRUCTURE.md
FLOW-*-CLIENT-*-INFRASTRUCTURE.md
business flow spec documents
```

**Authority boundary:** U01/Type 2 zone mapping. Business domain and task catalog
sections support Type 2/Type 5. Acceptance criteria support Type 8. Engine contract
sections support Type 26. Keep zones separate.

**A-side use:** No, unless a real user/business prompt is explicitly present.

**B-side chosen use:** Yes after zone mapping for business-flow architecture,
task-type routing, acceptance criteria, and engine-contract pairs.

**Rejected source:** Cross-zone misuse, wrong task type, missing prerequisite,
acceptance criterion used as architecture, or inferred engine contract not stated.

**Expected overlap ratio:** Inherits target zone type.

**Excluded use:**

```text
Do not use a full business-flow spec as one row.
Zone mapping is mandatory before DPO construction.
```

---

### Planning Type B35 - History RAG Fixture Pipeline Documents

**Registry status:** `full_type`

**What it is:** The History RAG pipeline family: fixture batch reports, RAG index
documents, and integration-design specs that wire the fixture index into governance
documents.

**Examples in the XIIGen corpus:**

```text
HIST-*-RAG-BATCH*.md
HISTORY-RAG-INDEX*.md
HISTORY-RAG-INTEGRATION-DESIGN-I1.md
HISTORY-RAG-INTEGRATION-DESIGN-I2.md
HISTORY-RAG-INTEGRATION-DESIGN-I3.md
HISTORY-RAG-INTEGRATION-DESIGN-I4.md
```

**Authority boundary:** Full source for RAG fixture correctness. Batch reports
provide Type 4-like metrics, index files provide fixture attribution, and integration
designs provide exact insertion-point and line-level wiring evidence.

**A-side use:** No, unless a real user query appears as an explicit fixture query.

**B-side chosen use:** Yes for fixture path, theme cluster, quality score, locked
decision marker, connectionType, batch field mapping, spot-check result, and exact
integration insertion point.

**Rejected source:** Wrong fixture attribution, fabricated quality score, wrong
connectionType, missing locked decision marker, or integration block inserted at
the wrong location.

**Expected overlap ratio:** 0.10-0.20.

**Excluded use:**

```text
Do not answer architectural questions from the RAG index alone; follow the fixture.
Do not mix batch metric rows with fixture attribution rows.
```

---

### Planning Type B36 - Gap-To-Guidance Mapping Documents

**Registry status:** `full_type`

**What it is:** Maps from a known codebase/process gap to the skill, guidance
document, missing coverage, and recommended addition that should close the gap.

**Examples in the XIIGen corpus:**

```text
GAP-TO-GUIDANCE-MAPPING-R3.md
GAP-TO-GUIDANCE-MAPPING*.md
```

**Authority boundary:** Strong source for gap-to-skill routing. It differs from
Type 23 because the central claim is "which guidance should cover this gap", not
the actual code fix.

**A-side use:** No.

**B-side chosen use:** Yes for gap id, target skill/guidance, missing-coverage
description, and recommended addition.

**Rejected source:** Wrong skill attribution, claiming existing guidance covers a
gap that it does not cover, or mapping a gap to a document that cannot enforce it.

**Expected overlap ratio:** 0.10-0.20.

**Excluded use:**

```text
Do not treat gap-to-guidance mapping as proof the code gap is fixed.
Record both gap id and target guidance id.
```

---

### Planning Type B37 - Skill Integration Instruction Documents

**Registry status:** `full_type`

**What it is:** One-time deployment procedures for applying skill-library updates
to a codebase: copy commands, patch order, verification loops, and expected output.

**Examples in the XIIGen corpus:**

```text
INTEGRATION-INSTRUCTIONS-v3.0.0.md
INTEGRATION-INSTRUCTIONS*.md
```

**Authority boundary:** Migration-plan extension for skill deployment. It is a
deployment procedure, not a runtime behavior contract.

**A-side use:** Real developer prompts such as "apply the Phase B skill update" may
be used as Type 1.

**B-side chosen use:** Yes for copy sequence, patch order, registration order,
verification command, and expected output.

**Rejected source:** Wrong order, missing verification, registering HOW-TO-USE before
the skill files it references, or treating a one-time deployment instruction as an
ongoing runtime rule.

**Expected overlap ratio:** 0.15-0.25.

**Excluded use:**

```text
Do not use integration instructions as canonical skill content.
Verify latest version before using as B-side.
```

---

### Planning Type B38 - Flow Planning Library Procedure Documents

**Registry status:** `full_type`

**What it is:** `LIBRARY-N` procedure files that describe how to produce a specific
flow-planning document type with variables, commands, expected outputs, and quality
gates.

**Examples in the XIIGen corpus:**

```text
LIBRARY-1-DESIGN-SIMULATION.md
LIBRARY-2-IMPLEMENTATION-PLAN.md
LIBRARY-3-TEACH-QA.md
LIBRARY-7-VISUALIZATION.md
```

**Authority boundary:** Process authority for producing planning documents. It is
not the same as GUIDE-B content authority; GUIDE-B says what belongs in the output,
while LIBRARY-N says how to create it.

**A-side use:** Real prompts asking how to produce a planning artifact may be used
as Type 1.

**B-side chosen use:** Yes for document-production step order, required variables,
quality gate, expected output file, and prerequisite check.

**Rejected source:** Wrong step order, missing prerequisite, creating TEACH-QA before
DESIGN-SIMULATION, or confusing output content rules with production procedure.

**Expected overlap ratio:** 0.20-0.35.

**Excluded use:**

```text
Do not use LIBRARY-N as the finished flow document.
Record target document type and library id.
```

---

### Planning Type B39 - Run Plan And Final Execution Reports

**Registry status:** `full_type`

**What it is:** Per-run UI execution plans and post-run final reports for concrete
pages, flows, or implementation runs.

**Examples in the XIIGen corpus:**

```text
RUN-03-PLAN-CheckoutPage.md
RUN-03-FINAL-CheckoutPage.md
RUN-XX-PLAN-*.md
RUN-XX-FINAL-*.md
```

**Authority boundary:** RUN-PLAN is a per-run execution contract. RUN-FINAL is a
post-execution validation report. Keep plan and final zones separate.

**A-side use:** Developer intent embedded in a RUN-PLAN may become Type 1 only when
clearly separated from instructions.

**B-side chosen use:** Yes for implementation scaffold, files-to-read, state that
must not be changed, expected output, PASS/FAIL result, and remaining-work note.

**Rejected source:** Wrong implementation pattern, ignoring do-not-touch state,
claiming final pass when final report records fail, or treating RUN-PLAN as RUN-FINAL.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not combine plan instructions and final validation into one row.
Record run id, page/flow id, and document phase.
```

---

### Planning Type B40 - UI Fleet Coverage And Complete Run Plans

**Registry status:** `full_type`

**What it is:** Fleet-level UI coverage plans and complete run plans that assign
architectural clusters, patterns, do-not-redo lists, and implementation scope across
many runs.

**Examples in the XIIGen corpus:**

```text
COMPLETE-PLAN-ALL-RUNS.md
FLOW-UI-COVERAGE-PLAN-v2.md
FLOW-UI-COVERAGE-PLAN-v3.md
FLOW-UI-COVERAGE-PLAN-v4.md
FLOW-UI-COVERAGE-PLAN-v5.md
```

**Authority boundary:** Fleet UI coverage authority. It differs from generic Type
29 because it contains UI cluster assignment and do-not-redo constraints.

**A-side use:** No.

**B-side chosen use:** Yes for cluster assignment, implementation pattern, page/run
scope, already-done exclusion, and fleet coverage gate.

**Rejected source:** Wrong cluster, redoing committed work, assigning a single-sided
pattern to a marketplace flow, or ignoring fleet coverage gates.

**Expected overlap ratio:** 0.15-0.25.

**Excluded use:**

```text
Do not use fleet coverage plan as code truth.
Keep cluster assignment separate from page-level implementation instructions.
```

---

### Planning Type B41 - Autonomous Session Operation Specifications

**Registry status:** `limited_authority`

**What it is:** Detailed session-operation specs with output contracts, prerequisite
commands, session loop rules, loaded skills, and done conditions.

**Examples in the XIIGen corpus:**

```text
FLOW-S4-CALIBRATION-SESSION.md
AUTONOMOUS-OPERATION-MASTER-PLAN.md
```

**Authority boundary:** Type 19 extension with zone mapping. Output contract maps
to Type 34, step sequence maps to Type 19, and prerequisite commands map to A03.

**A-side use:** No.

**B-side chosen use:** Yes after zone split for output contract, session rule,
prerequisite verification, and autonomous loop behavior.

**Rejected source:** Missing done condition, skipped prerequisite command, wrong
session loop, or using an operation plan as a completed session state.

**Expected overlap ratio:** Inherits Type 19, Type 34, or A03.

**Excluded use:**

```text
Do not train the entire session spec as one row.
Zone id is mandatory.
```

---

### Planning Type B42 - Flow JSON Definition Files

**Registry status:** `limited_authority`

**What it is:** Machine-readable flow contract definitions with flowId, version,
published/consumed events, skills used, lifelines, messages, and complexity metadata.

**Examples in the XIIGen corpus:**

```text
01-registration-flow.json
02-onboarding-flow.json
flow definition JSON files
```

**Authority boundary:** Type 25 extension for flow-definition contracts. Use for
schema-valid flow fields and topology/event claims, not prose architecture.

**A-side use:** No.

**B-side chosen use:** Yes for required field, event name, flow id, version, skills
used, lifeline/message metadata, and schema-valid flow-definition pairs.

**Rejected source:** Missing required event list, wrong event naming, topology
mismatch, or flow contract that contradicts the canonical topology.

**Expected overlap ratio:** 0.10-0.20.

**Excluded use:**

```text
Do not use JSON flow definition as narrative design rationale.
Validate against the flow schema before extraction.
```

---

### Planning Type B43 - XIIGen Gap Review, Skill Gap Analysis, And Design Vision

**Registry status:** `limited_authority`

**What it is:** Meta-analysis documents that compare expected versus actual system
behavior, analyze skill coverage gaps, or explain the intended system vision in
plain language.

**Examples in the XIIGen corpus:**

```text
XIIGEN-GAP-REVIEW*.md
XIIGEN-SKILL-GAP-ANALYSIS*.md
XIIGEN-DESIGN-VISION*.md
```

**Authority boundary:** Zone-mapped. Expected behavior can support ARCH/CODE_Q.
Actual behavior can support rejected or Type 33 comparison rows. Skill coverage
maps to B36. Design vision maps to Type 2.

**A-side use:** Real user/business prompts inside design-vision docs may become
Type 1 only when clearly separated.

**B-side chosen use:** Yes after zone mapping for expected-vs-actual contrast,
skill coverage gap, design intent, and simplified system explanation.

**Rejected source:** Treating actual broken behavior as desired behavior, wrong
skill coverage claim, or using simplified vision as exact implementation contract.

**Expected overlap ratio:** Inherits target zone type.

**Excluded use:**

```text
Do not use meta-analysis as direct code truth.
Separate expected behavior, actual behavior, skill gap, and design-vision zones.
```

---

## Older Directory Historical Families

The `C01-C12` families below come from the `Older` directory classification pass. These
documents are mostly pre-current architecture material. Their default canonicality
is `stale` unless the rule below explicitly says otherwise.

---

### Older Type C01 - Early Architecture Plan Documents

**Registry status:** `limited_authority`

**What it is:** Pre-current XIIGen architecture plans from the early .NET/C# era,
before later migration and flow-first architecture decisions.

**Examples in the XIIGen corpus:**

```text
architecture-plan v1.md
architecture-plan-v2.md
ARCHITECTURE_v4_USER_ALIGNED.md
XIIGEN_COMPLETE_ARCHITECTURE_PLAN.md
XIIGEN_MODERN_ARCHITECTURE.md
XIIGEN_ENHANCED_ARCHITECTURE_V2.md
XIIGEN_ENHANCED_ARCHITECTURE_V3.md
```

**Authority boundary:** Default canonicality is `stale`. Use primarily as Type 16
versioned-evolution evidence: old names, superseded interfaces, and rejected
vocabulary compared against current architecture.

**A-side use:** No.

**B-side chosen use:** Only when the row explicitly asks about historical evolution.
Current architecture claims must come from current Type 2/Type 9/Type 26 sources.

**Rejected source:** Superseded interface name, old stack assumption, stale system
boundary, or pre-current architecture used as current truth.

**Expected overlap ratio:** 0.05-0.15.

**Excluded use:**

```text
Do not use early architecture plans as current architecture authority.
Record stale canonicality and version/date for every row.
```

---

### Older Type C02 - Skills Inventory And Decision Guide Documents

**Registry status:** `limited_authority`

**What it is:** Early skills inventories, decision guides, quick-start guides, and
complete-system guides that describe previous skill-library scope and decision paths.

**Examples in the XIIGen corpus:**

```text
XIIGEN_SKILLS_INVENTORY.md
XIIGEN_SKILLS_INVENTORY_V2.md
XIIGen_Skills_Inventory_v8.md
XIIGEN_DECISION_GUIDE.md
XIIGEN_QUICK_START_GUIDE.md
XIIGEN_COMPLETE_SYSTEM_GUIDE.md
```

**Authority boundary:** Skills inventory files are stale Type 20/Type 30 evidence.
Decision guides are Type 33 comparison evidence. Use for versioned evolution and
superseded path comparison, not current skill registry truth.

**A-side use:** No.

**B-side chosen use:** Yes for historical skill count, old skill naming, old path
comparison, and why a path was superseded.

**Rejected source:** Old skill count used as current, superseded layer structure,
or unchosen decision path presented as current guidance.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use early inventory as current skills factory registry.
When teaching current behavior, old names belong on rejected side.
```

---

### Older Type C03 - Consolidation Session Prompt Packages

**Registry status:** `full_type`

**What it is:** One-shot multi-session kickoff and handoff prompt packages for
consolidation work, including what to attach, read order, continuation phrase, and
state-preservation protocol.

**Examples in the XIIGen corpus:**

```text
CONSOLIDATION_PROMPT.md
HOW_TO_USE_CONSOLIDATION_PROMPT.md
V18_CONSOLIDATION_PLAN.md
```

**Authority boundary:** Distinct from Type 15 general session start prompts and
Type 19 runtime behavior contracts. C03 is a task-specific handoff package.

**A-side use:** No.

**B-side chosen use:** Yes for handoff sequence, required attachments, continuation
phrase, consolidation phase, and state-preservation rules.

**Rejected source:** Missing attachment, wrong read order, lost continuation phrase,
or treating a one-shot handoff prompt as a permanent agent rule.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use consolidation prompt content as general architecture truth.
Record task/session scope and phase.
```

---

### Older Type C04 - Pattern Adoption And Migration Mapping Documents

**Registry status:** `full_type`

**What it is:** Direct mapping documents from original user code or prior patterns
to XIIGen abstractions, plus comparison/migration guides between architecture
approaches.

**Examples in the XIIGen corpus:**

```text
PATTERN_MAPPING.md
COMPARISON_AND_MIGRATION_GUIDE.md
ARCHITECTURE_v4_USER_ALIGNED.md
DatabaseService.cs
ObjectProcess.cs
```

**Authority boundary:** Source code files are A05/Type 26 authority for the original
pattern. Mapping documents are authority for "your pattern -> XIIGen abstraction"
adoption pairs, not for current runtime implementation unless verified in current
code.

**A-side use:** Real developer question about adapting their original pattern may
be used as Type 1.

**B-side chosen use:** Yes for pattern adoption, side-by-side mapping, migration
path, and original-code-to-XIIGen abstraction pairs.

**Rejected source:** Presenting adapted XIIGen abstraction as the original user
pattern, wrong migration direction, or choosing the superseded approach.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not erase original-code provenance.
Keep original pattern and adopted abstraction as separate fields.
```

---

### Older Type C05 - Flow Compliance Framework Family

**Registry status:** `full_type`

**What it is:** Fleet-level flow compliance framework, gap matrix, and remediation
plan defining production-readiness requirements across flows.

**Examples in the XIIGen corpus:**

```text
FLOW_COMPLIANCE_FRAMEWORK.md
FLOW_COMPLIANCE_GAP_MATRIX.md
FLOW_COMPLIANCE_PLAN.md
```

**Authority boundary:** Framework defines fleet-level DoD requirements. Gap matrix
is Type 32/B16-style compliance audit. Compliance plan is Type 23 remediation.
Keep zones separate.

**A-side use:** No.

**B-side chosen use:** Yes for requirement id, pass/fail criterion, per-flow
compliance status, enforcement mechanism, and remediation plan.

**Rejected source:** Wrong requirement result, missing enforcement, false compliant
status, or remediation that does not map to the failing requirement.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use compliance matrix as implementation proof.
Record requirement id R1-R8 and flow id.
```

---

### Older Type C06 - Requirement Fulfillment Specification Documents

**Registry status:** `full_type`

**What it is:** Requirement-specific compliance specs that define schema, file
patterns, test templates, probes, mock contracts, and enforcement rules for one
R1-R8 compliance area.

**Examples in the XIIGen corpus:**

```text
SECURITY_ISOLATION_MATRIX.md
OBSERVABILITY_CONTRACT.md
PROMPT_MANAGEMENT_CATALOG.md
THIRD_PARTY_MOCK_CATALOG.md
BUSINESS_CYCLE_TEST_MATRIX.md
MT_FEATURE_FLAG_SPEC.md
LIBRARY_BOUNDARY_SPEC.md
```

**Authority boundary:** Stronger than generic Type 18 portability protocols for
these R-compliance requirements because it includes concrete test contracts and
file templates.

**A-side use:** No.

**B-side chosen use:** Yes for requirement fulfillment, test template, probe type,
mock contract, feature flag rule, and enforcement file pattern.

**Rejected source:** Missing probe, wrong mock boundary, untestable compliance claim,
or requirement satisfied without the required test/template.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not generalize one R requirement to all compliance areas.
Record requirement id and enforcement artifact.
```

---

### Older Type C07 - Project Genealogy And Evolution Documents

**Registry status:** `full_type`

**What it is:** Architectural lineage documents that explain how XIIGen patterns
evolved from earlier systems, generations, or source projects.

**Examples in the XIIGen corpus:**

```text
EVOLUTION_MAP.md
ENGINE_ARCHITECTURE_MERGED.md
V17_PLAN_PACKAGE.zip/EVOLUTION_MAP.md
```

**Authority boundary:** Type 12 extension for genealogy. It explains why a pattern
exists and where it came from; current exact behavior still comes from current code
or architecture docs.

**A-side use:** Real "why does XIIGen use X" questions may be used as Type 1.

**B-side chosen use:** Yes for lineage explanation, origin pattern, generation
transition, and historical reason for current abstraction.

**Rejected source:** Invented origin story, missing generation step, wrong source
project, or genealogy used as current implementation proof.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use genealogy as current API contract.
Record origin, generation, and current target abstraction.
```

---

### Older Type C08 - Documentation Standard And Compliance Validation Documents

**Registry status:** `limited_authority`

**What it is:** Documentation compliance standards, validation-runner specs, and
audit reports for documentation completeness.

**Examples in the XIIGen corpus:**

```text
CODE_DOCUMENTATION_STANDARD.md
COMPLIANCE_VALIDATION_RUNNER.md
DOCUMENT_AUDIT_REPORT.md
```

**Authority boundary:** CODE_DOCUMENTATION_STANDARD is Type 34 DoD for documentation.
COMPLIANCE_VALIDATION_RUNNER is A04 executable-check evidence. DOCUMENT_AUDIT_REPORT
is Type 32 validation evidence.

**A-side use:** No.

**B-side chosen use:** Yes after zone mapping for required documentation files,
TSDoc coverage threshold, validation command, and audit result.

**Rejected source:** Missing required doc file, false documentation coverage,
validation command omitted, or audit result treated as remediation plan.

**Expected overlap ratio:** Inherits Type 34, A04, or Type 32.

**Excluded use:**

```text
Do not treat documentation audit as code behavior.
Zone mapping is mandatory.
```

---

### Older Type C09 - Versioned Skill Library Snapshot Archives

**Registry status:** `preprocess_then_reclassify`

**What it is:** Historical ZIP snapshots of V16/V17/V18 skill libraries and Genie
DNA composition builds.

**Examples in the XIIGen corpus:**

```text
xiigen_v16_v17_D6.zip
xiigen_v16_v17_D13_full.zip
xiigen_v18_E2_complete.zip
xiigen_v18_E5_complete.zip
xiigen-skills-v4-complete.zip
xiigen-skills-v9-complete.zip
```

**Authority boundary:** Archive container first. After extraction, most content is
stale Type 20 skill-library material. `EVOLUTION_MAP.md` and `STATE.md` inside are
priority Type 12/Type 16 extraction targets.

**A-side use:** No.

**B-side chosen use:** Only for historical evolution rows. For current behavior,
historical snapshots usually provide rejected vocabulary or superseded structure.

**Rejected source:** Old skill name, superseded interface, stale layer structure,
or treating D/E-series snapshot as current canonical source.

**Expected overlap ratio:** 0.05-0.20.

**Excluded use:**

```text
Do not train from snapshot archive filenames.
Extract and classify contents; default canonicality is stale.
```

---

### Older Type C10 - Writing Style Skill Library And Content Style Guides

**Registry status:** `limited_authority`

**What it is:** A content-generation skill library for writing in a specific author
voice, with templates, style examples, and rhetorical pattern rules.

**Examples in the XIIGen corpus:**

```text
How to/SKILL.md
section-templates.md
style-examples.md
skill-library-workbook.html
skill-library-workbook.pdf
```

**Authority boundary:** Type 20 extension for content/writing domain, not technical
orchestration. Rendered workbook files are A09/A17 and must be preprocessed before
use.

**A-side use:** Real content-generation prompt may be used as Type 1.

**B-side chosen use:** Yes for voice pattern, section template, rhetorical structure,
and style-preserving generation pairs.

**Rejected source:** Generic AI text, wrong voice pattern, missing required section
template, or using writing-style rules as technical architecture rules.

**Expected overlap ratio:** 0.20-0.40.

**Excluded use:**

```text
Do not mix writing-style skill rows into technical orchestration rows.
Record domain=content/writing.
```

---

### Older Type C11 - Versioned Office Architecture Exports

**Registry status:** `preprocess_then_reclassify`

**What it is:** DOCX/PDF architecture exports and workbook renderings from the
Older directory, usually versioned copies of architecture or technical guidance.

**Examples in the XIIGen corpus:**

```text
XIIGen_Architecture_Plan_v8.docx
XIIGen_Architecture_Plan_v12.docx
Architecture plan v3.docx
Architecture plan v4.docx
orchestrator-abstraction-guide-v2.docx
skill-library-workbook.pdf
```

**Authority boundary:** A17 first. Extract text, dedupe against MD sources, then
reclassify as Type 2, Type 16, Type 3, Type 20, or another target type. Default
canonicality is `stale` unless the extracted export is the only canonical copy.

**A-side use:** No direct use before extraction.

**B-side chosen use:** Only after extraction and target-type classification.

**Rejected source:** Stale exported architecture, old wording used as current,
or DOCX/PDF content used without extraction/dedupe.

**Expected overlap ratio:** Inherits target type after extraction.

**Excluded use:**

```text
Do not train on Office/PDF binary payload.
Do not prefer export text over canonical markdown when both exist.
```

---

### Older Type C12 - Original User Source Seed Files

**Registry status:** `limited_authority`

**What it is:** Original user code files that seeded early XIIGen pattern mapping,
before the adapted XIIGen abstractions existed.

**Examples in the XIIGen corpus:**

```text
DatabaseService.cs
ObjectProcess.cs
```

**Authority boundary:** A05/Type 26 authority for the original user pattern only.
They are not current XIIGen architecture authority. Use together with C04 pattern
adoption documents when teaching "original pattern -> XIIGen abstraction".

**A-side use:** No, unless paired with the original developer request that introduced
the code.

**B-side chosen use:** Yes for exact original interface/class shape and original
code contract claims.

**Rejected source:** Presenting XIIGen's adapted abstraction as the original user
code, or presenting original user code as current XIIGen runtime contract.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Keep original-source and adapted-XIIGen fields separate.
Do not erase provenance from user code.
```

---

## Older Directory ZIP-Content Families

The `Z41-Z49` families below come from the full recursive scan of ZIP files inside
the `Older` directory. They preserve the report's original IDs as aliases
(`reported_id: B41-B49`) while using `Z` registry IDs here to avoid colliding with
existing B-series planning/session families.

---

### Older Directory Archive Type Z41 - Delivery Phase Checkpoint Documents

**Reported id:** `B41`

**Registry status:** `full_type`

**What it is:** Phase completion and checkpoint documents inside D-series and
E-series ZIP snapshots. They record phase status, file-level progress, resume keys,
and next-phase instructions.

**Examples in the XIIGen corpus:**

```text
PHASE_Dxx_COMPLETE.md
PHASE_Dxx_CHECKPOINT_A.md
PHASE_Exx_COMPLETE.md
V17-states/PHASE_D*.md
V17-states/PHASE_E*.md
```

**Authority boundary:** Type 32 extension for delivery-phase validation, not
flow-level validation. A checkpoint is also a continuation contract when it includes
explicit recovery or next-phase instructions.

**A-side use:** No.

**B-side chosen use:** Yes for delivery phase status, file-status table, resume key,
checkpoint recovery instruction, next-phase instruction, and per-file completion
state.

**Rejected source:** Wrong phase status, skipped resume key, claiming complete while
file table shows pending, or resuming from a non-checkpoint file.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not treat checkpoint as current implementation truth without archive/version metadata.
Record ZIP series, phase id, checkpoint id, and canonicality.
```

---

### Older Directory Archive Type Z42 - Session DOC RAG Index Files

**Reported id:** `B42`

**Registry status:** `full_type`

**What it is:** Session-level RAG file indexes that map each uploaded/extracted file
to its contents and intended use case for an AI agent.

**Examples in the XIIGen corpus:**

```text
DOC_RAG_INDEX.md
D-series ZIP/DOC_RAG_INDEX.md
E-series ZIP/DOC_RAG_INDEX.md
```

**Authority boundary:** Distinct from B35 History RAG fixture pipeline. Z42 is a
session context map: "use this file when building/answering X." It locates source
files and use cases; it does not replace the target file content.

**A-side use:** No.

**B-side chosen use:** Yes for file-to-use-case mapping, which file contains which
implementation/design evidence, and session-context retrieval pairs.

**Rejected source:** Wrong file reference, fabricated file in index, using a file
for a use case not listed, or answering from the index without reading the target.

**Expected overlap ratio:** 0.05-0.15.

**Excluded use:**

```text
Follow the DOC_RAG_INDEX entry to the target file before extracting content.
Record session/archive version and target path.
```

---

### Older Directory Archive Type Z43 - DNA Compliance Audit Reports

**Reported id:** `B43`

**Registry status:** `full_type`

**What it is:** Cross-reference audit reports comparing Genie/DNA/V6 pattern
expectations against V17 skill-library implementation coverage.

**Examples in the XIIGen corpus:**

```text
DNA_AUDIT_REPORT.md
V17 skill library DNA audit reports
```

**Authority boundary:** Pattern compliance audit across the skill library. It is
not a codebase state review (B13) and not a generic validation certificate; it
checks architectural pattern adherence by layer/pattern.

**A-side use:** No.

**B-side chosen use:** Yes for DNA pattern coverage, STRONG/INCONSISTENT/MISSING
rating, layer scorecard, per-pattern finding, and evidence-backed compliance claim.

**Rejected source:** PASS claim without pattern evidence, typed-interface claim that
violates schema-free pattern, wrong layer rating, or missing/ignored audit evidence.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not treat DNA audit as general implementation status.
Record layer id, pattern id, rating, and evidence line.
```

---

### Older Directory Archive Type Z44 - Session Continuation Guides

**Reported id:** `B44`

**Registry status:** `full_type`

**What it is:** Explicit continuation guides for interrupted multi-session work:
resume prompt, sprint order, current state, and definition of done for the next
agent/session.

**Examples in the XIIGen corpus:**

```text
HOW_TO_CONTINUE.md
phase_c/HOW_TO_CONTINUE.md
```

**Authority boundary:** Distinct from Type 15 session start prompts and C03
consolidation prompt packages. Z44 says exactly what to do next in an interrupted
work stream.

**A-side use:** No.

**B-side chosen use:** Yes for resume prompt, sprint order, next task, per-skill
standard, definition of done, and continuation key.

**Rejected source:** Wrong resume phrase, wrong sprint order, continuing from stale
phase, or losing the per-skill done standard.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use continuation guide as current architecture authority.
Record interrupted work stream and resume key.
```

---

### Older Directory Archive Type Z45 - Stack Integration Recipe Documents

**Reported id:** `B45`

**Registry status:** `full_type`

**What it is:** Stack-combination recipes that describe when to use a stack pair,
project structure, key patterns, and team/profile fit.

**Examples in the XIIGen corpus:**

```text
skills/recipes/dotnet-react.md
skills/recipes/nodejs-react.md
skills/recipes/rust-angular.md
skills/recipes/*.md
```

**Authority boundary:** Distinct from Type 20 SKILL.md and Type 2 architecture docs.
Recipes are stack-selection and project-structure authority.

**A-side use:** Real developer prompts asking which stack to use may be used as
Type 1.

**B-side chosen use:** Yes for stack recommendation, project tree, when-to-use
condition, team profile, and pattern explanation.

**Rejected source:** Wrong stack for team/use case, recommendation without project
structure, or using recipe as implementation proof.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not treat stack recipe as app-specific architecture.
Record recipe id and stack pair.
```

---

### Older Directory Archive Type Z46 - Use Case Implementation Guides

**Reported id:** `B46`

**Registry status:** `full_type`

**What it is:** Use-case level guides describing when to build a type of system,
typical entities, recommended stacks, and key design patterns.

**Examples in the XIIGen corpus:**

```text
templates/use-cases/dynamic-crm.md
templates/use-cases/gamification.md
templates/use-cases/headless-api.md
templates/use-cases/task-management.md
```

**Authority boundary:** Use-case guidance, not stack recipe. Z46 answers "what does
this use case need"; Z45 answers "which stack recipe fits."

**A-side use:** Real user/business prompts matching the use case may be used as
Type 1.

**B-side chosen use:** Yes for build-this-when condition, typical entities,
recommended stack pairing, and use-case design pattern.

**Rejected source:** Wrong use-case classification, missing typical entity, wrong
stack pairing, or generic implementation that ignores the use-case guide.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use use-case guide as exact product requirements.
Record use-case id and recommended stack if used.
```

---

### Older Directory Archive Type Z47 - Weekly Delivery Implementation Guides

**Reported id:** `B47`

**Registry status:** `full_type`

**What it is:** Week/day-based delivery guides for skill-library work, with daily
milestones, skill status, and deliverables.

**Examples in the XIIGen corpus:**

```text
WEEK_1_IMPLEMENTATION_GUIDE.md
week1-path-a/WEEK_1_IMPLEMENTATION_GUIDE.md
week2/*
week3/*
```

**Authority boundary:** Distinct from Type 29 phase implementation plans. Z47 is a
timeline delivery guide for skill-library packages.

**A-side use:** No.

**B-side chosen use:** Yes for day-by-day plan, skill status, deliverable per day,
remaining work, and week-level execution order.

**Rejected source:** Wrong day order, status mismatch, omitted deliverable, or
treating weekly guide as current skill implementation proof.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use weekly guide as canonical skill content.
Record week/day and skill id.
```

---

### Older Directory Archive Type Z48 - Skill Manifest Files

**Reported id:** `B48`

**Registry status:** `limited_authority`

**What it is:** Machine-readable skill metadata files adjacent to SKILL.md, usually
describing name, description, allowed tools, and argument hints.

**Examples in the XIIGen corpus:**

```text
manifest.json
project-skills/*/manifest.json
ai-feedback-loop/manifest.json
dotnet-k8s-orchestrator/manifest.json
elasticsearch-ml-store/manifest.json
figma-api-integration/manifest.json
react-native-flow-ui/manifest.json
```

**Authority boundary:** Manifest is registry metadata; SKILL.md remains instruction
authority. Use manifest for discovery/routing/tool metadata, not detailed behavior.

**A-side use:** No direct prompt use. Manifest keywords may support prompt routing
or A-side vocabulary calibration only.

**B-side chosen use:** Yes for skill name, description, allowed tool, argument hint,
and registry metadata checks.

**Rejected source:** Wrong allowed tool, fabricated skill name, manifest metadata
used as full skill instructions, or missing SKILL.md cross-check.

**Expected overlap ratio:** 0.10-0.20.

**Excluded use:**

```text
Do not use manifest.json as the full skill behavior contract.
Cross-check SKILL.md for behavioral instructions.
```

---

### Older Directory Archive Type Z49 - Analysis Draft And Complete Pairs

**Reported id:** `B49`

**Registry status:** `full_type`

**What it is:** Explicit draft/final analysis pairs where an early analysis document
and a complete version both exist and the final document supersedes the draft.

**Examples in the XIIGen corpus:**

```text
GENERIC_MODULE_ANALYSIS.md
GENERIC_MODULE_ANALYSIS_COMPLETE.md
module-architecture ZIP analysis pairs
```

**Authority boundary:** Type 16 versioned-evolution pair with unusually clean
draft-to-final relation. The complete document is chosen; the draft is rejected or
stale depending on pair purpose.

**A-side use:** No.

**B-side chosen use:** Yes for depth improvement, completed capability matrix,
missing analysis filled in, and draft/final comparison.

**Rejected source:** Draft analysis used as final, incomplete matrix, missing
sections retained, or final answer that regresses to the generic draft.

**Expected overlap ratio:** 0.30-0.50.

**Excluded use:**

```text
Do not mix draft and complete claims without version fields.
Record draft path, complete path, and supersession relation.
```

---

### Nested Archive External Type Z50 - Lesson Transcript Documents

**Reported id:** `B50`

**Registry status:** `limited_authority`

**What it is:** Spoken-word or narrated tutorial transcripts embedded in external
skill repositories.

**Examples in the XIIGen corpus:**

```text
claude-useful-skills-master/skills/agent-architect/LESSON.MD
```

**Authority boundary:** External tutorial language is not XIIGen authority. It may
contribute A-side vocabulary for "how do I learn/build X" prompts, and structural
comparison examples for lesson-style teaching, but not XIIGen architecture claims.

**A-side use:** Yes, vocabulary only, when the prompt is about learning or asking
for a tutorial-style explanation.

**B-side chosen use:** No for XIIGen-specific pairs. Limited structural use only if
the row is explicitly about lesson transcript structure.

**Rejected source:** Treating a transcript demonstration as a locked XIIGen design
decision.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `source_canonicality=external`,
`external_source_family=third_party_skill_repo`, `structural_pattern_only=true`,
`archive_member_path`, and `lesson_topic`.

---

### Nested Archive External Type Z51 - Skill Mastery Guides

**Reported id:** `B51`

**Registry status:** `limited_authority`

**What it is:** External "maximum value" guides for skills, usually structured as
when to use, what to skip, iron rule, phase deep dive, and pro tips.

**Examples in the XIIGen corpus:**

```text
docs/MAX-VALUE-GUIDES/agent-architect-mastery.md
docs/MAX-VALUE-GUIDES/gsd-mastery.md
docs/MAX-VALUE-GUIDES/prompt-master-mastery.md
```

**Authority boundary:** These guides can teach the shape of skill usage advice. They
cannot select XIIGen skills, resolve XIIGen routing, or override Type 20 XIIGen skill
definitions.

**A-side use:** Yes for generic skill-usage prompts only.

**B-side chosen use:** No for XIIGen-specific decisions. Limited structural use for
skill-advice format pairs.

**Rejected source:** Applying an external "skip/use" rule to an internal XIIGen skill
without a Type 9 adoption decision.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `source_canonicality=external`, `skill_name`,
`guide_section`, `structural_pattern_only=true`, and `archive_member_path`.

---

### Nested Archive External Type Z52 - GSD Project Runtime State

**Reported id:** `B52`

**Registry status:** `limited_authority`

**What it is:** External `.gsd/` project runtime folders containing project vision,
requirements, roadmap, state, per-task XML-tagged plans, and phase summaries.

**Examples in the XIIGen corpus:**

```text
examples/gsd-todo-api/.gsd/PROJECT.md
examples/gsd-todo-api/.gsd/REQUIREMENTS.md
examples/gsd-todo-api/.gsd/ROADMAP.md
examples/gsd-todo-api/.gsd/STATE.md
examples/gsd-todo-api/.gsd/phases/phase-1/PLAN-1.md
examples/gsd-todo-api/.gsd/phases/phase-1/SUMMARY.md
```

**Authority boundary:** High-value structural pattern source. Content is external
and must not be used as XIIGen B-side authority. The useful part is the artifact
shape: project decision context, XML-tagged execution plan, verification block,
expected output, summary file table, and state tracking.

**A-side use:** Yes for generic project orchestration prompts. For XIIGen rows, use
only if a real XIIGen developer prompt asks for the same structure.

**B-side chosen use:** Structural pattern only. `PLAN-N.md` can teach execution-plan
DTO completeness: task, files, action, verify, expected.

**Rejected source:** Plan without verification block, plan without expected output,
summary without file table, or state file used as if it were a validation report.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `source_canonicality=external`, `gsd_artifact_kind`
(`PROJECT`, `REQUIREMENTS`, `ROADMAP`, `STATE`, `PLAN`, `SUMMARY`),
`phase_id`, `plan_id` when present, `structural_pattern_only=true`, and
`archive_member_path`.

---

### Nested Archive External Type Z53 - Agent System Prompt Files

**Reported id:** `B53`

**Registry status:** `limited_authority`

**What it is:** External per-role system prompts for a multi-agent pipeline, usually
role definition, available tools, process guidelines, and output format.

**Examples in the XIIGen corpus:**

```text
examples/research-agent-system/prompts/orchestrator_system.md
examples/research-agent-system/prompts/researcher_system.md
examples/research-agent-system/prompts/writer_system.md
```

**Authority boundary:** Structural prompt format source only. It cannot define
XIIGen agent behavior unless adopted by a Type 9 decision.

**A-side use:** Yes for generic prompt-authoring requests.

**B-side chosen use:** Structural pattern only: role separation, tool boundary, and
output contract. Not XIIGen runtime authority.

**Rejected source:** Monolithic prompt that collapses role/tool/output sections, or
using an external role prompt as an internal XIIGen agent contract.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `source_canonicality=external`, `agent_role`,
`prompt_sections_present`, `structural_pattern_only=true`, and `archive_member_path`.

---

### Nested Archive External Type Z54 - Skill Quality Gate Matrices

**Reported id:** `B54`

**Registry status:** `limited_authority`

**What it is:** External matrices defining skill quality gates, clusters, validation
rules, and pass criteria.

**Examples in the XIIGen corpus:**

```text
skillbuilder-main/references/70-gates.md
```

**Authority boundary:** This is a validation standard, not validation result. It is
closest to Type 34 but external and skill-specific.

**A-side use:** Yes for generic "what makes a skill production-ready" prompts.

**B-side chosen use:** Structural gate-matrix pairs only. Do not use to validate
XIIGen skill quality unless an internal Type 9/Type 34 document adopts the matrix.

**Rejected source:** Claiming a skill passed quality gates without a validation
result, or using the gate definition as a certificate.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `source_canonicality=external`, `gate_cluster_id`,
`gate_id`, `pass_criteria`, `structural_pattern_only=true`, and
`archive_member_path`.

---

### Nested Archive External Type Z55 - Skill Authoring Templates

**Reported id:** `B55`

**Registry status:** `limited_authority`

**What it is:** External fill-in templates for authoring SKILL.md and command files
at different complexity levels.

**Examples in the XIIGen corpus:**

```text
skillbuilder-main/assets/templates/skill-simple.template.md
skillbuilder-main/assets/templates/skill-standard.template.md
skillbuilder-main/assets/templates/skill-complex.template.md
skillbuilder-main/assets/templates/command.template.md
```

**Authority boundary:** Template structure source only. It does not define XIIGen
skill content, trigger rules, or allowed tools.

**A-side use:** Yes for generic skill-authoring prompts.

**B-side chosen use:** Structural pattern only after placeholders are resolved. Same
placeholder gate as A24 applies.

**Rejected source:** Unfilled placeholder, wrong template complexity, missing
required section, or using a template as if it were a finished skill.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `source_canonicality=external`, `template_kind`,
`placeholders_resolved`, `required_sections_present`,
`structural_pattern_only=true`, and `archive_member_path`.

---

### Nested Archive External Type Z56 - AI Context Documents

**Reported id:** `B56`

**Registry status:** `limited_authority`

**What it is:** Rich `.claude/AI_CONTEXT.md` style documents describing an external
project for AI assistants: architecture tree, tech stack, component purposes, state
management, and working conventions.

**Examples in the XIIGen corpus:**

```text
vibecoder-master/.claude/AI_CONTEXT.md
```

**Authority boundary:** Distinct from `CLAUDE.md` Type 19. AI_CONTEXT is richer
project context, not a tool execution contract. External content must not become
XIIGen B-side authority.

**A-side use:** Yes for generic "prepare AI project context" prompts.

**B-side chosen use:** Structural pattern only: project overview, stack table,
architecture tree, component responsibility list.

**Rejected source:** Treating AI_CONTEXT as CLAUDE.md tool instruction, or importing
external architecture facts into XIIGen pairs.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `source_canonicality=external`, `context_sections_present`,
`project_name`, `structural_pattern_only=true`, and `archive_member_path`.

---

## Conversation-Derived Session Families

The `D01-D19` families below come from analysis of `conversations.json`. They are
active session-produced artifacts, not static filesystem extension categories.

---

### Conversation Type D01 - Per-Flow Teaching Session Files

**Registry status:** `full_type`

**What it is:** Executable teaching-round files for one concrete flow/phase/session
round. They package context, required action, and named checks for that round.

**Examples in the XIIGen corpus:**

```text
SESSION-TEACH-A0.md
SESSION-TEACH-B-ADDITIONS.md
SESSION-TEACH-F-ADDITIONS.md
```

**Authority boundary:** Not Type 10. Type 10 is a teaching plan; D01 is an
executable per-round session artifact.

**A-side use:** No, unless the file contains a separate real user/developer prompt.

**B-side chosen use:** Yes for round dependency, action package, named check, and
per-flow teaching context.

**Rejected source:** Wrong round dependency, skipped named check, reused context
from another flow, or treating a teaching-round file as a generic plan.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Record flow id, phase, and round id.
Do not merge multiple teaching rounds into one row.
```

---

### Conversation Type D02 - Per-Flow Simulation Round Files

**Registry status:** `full_type`

**What it is:** Simulation-round contracts for one flow, including expected outputs,
acceptance criteria, and simulation gate conditions.

**Examples in the XIIGen corpus:**

```text
SESSION-SIM-R0.md
SESSION-SIM-R1.md
SESSION-SIM-R*.md
```

**Authority boundary:** Simulation verification artifact, not teaching instructions.
Pairs should focus on how the round is validated.

**A-side use:** No.

**B-side chosen use:** Yes for simulation expected output, acceptance criterion,
round contract, and pass/fail gate.

**Rejected source:** Wrong simulation round, missing acceptance criterion, false
expected output, or using a teach-round file as simulation authority.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not collapse all simulation rounds into one row.
Round id and expected output are mandatory.
```

---

### Conversation Type D03 - Round Precheck And Round Plan Documents

**Registry status:** `limited_authority`

**What it is:** Round-level precheck and plan documents that prepare a simulation or
implementation round before execution.

**Examples in the XIIGen corpus:**

```text
SESSION-R0-PRECHECK.md
R1-ROUND-PLAN.md
R*-ROUND-PLAN.md
```

**Authority boundary:** Type 34 for precheck gates, Type 29 for round plan sequence.
Do not treat precheck as final validation.

**A-side use:** No.

**B-side chosen use:** Yes after zone mapping for precheck condition, prerequisite,
round sequence, and expected handoff.

**Rejected source:** Skipped precheck, wrong round plan order, executing before
preconditions, or marking precheck as final pass.

**Expected overlap ratio:** Inherits Type 34 or Type 29.

**Excluded use:**

```text
Zone must be precheck or round-plan.
Do not use precheck as proof of implementation.
```

---

### Conversation Type D04 - Round Document Map Files

**Registry status:** `limited_authority`

**What it is:** Round-specific document maps that say which artifacts are read,
produced, or verified during a simulation/implementation round.

**Examples in the XIIGen corpus:**

```text
R1-DOCUMENT-MAP.md
R*-DOCUMENT-MAP.md
```

**Authority boundary:** Type 31 extension. It locates documents for a round; it does
not replace the target document's content.

**A-side use:** No.

**B-side chosen use:** Yes for round artifact routing, required input/output
document, and missing-document detection.

**Rejected source:** Wrong document assigned to round, missing required artifact,
or using a map as content authority.

**Expected overlap ratio:** 0.05-0.15.

**Excluded use:**

```text
Follow the map to the target document before extracting content.
Record round id.
```

---

### Conversation Type D05 - Flow Engine Gap List Documents

**Registry status:** `full_type`

**What it is:** Pre-fix inventory of all known engine gaps for one flow, often with
severity ordering and multi-layer gap categories.

**Examples in the XIIGen corpus:**

```text
FLOW-11-ENGINE-GAP-LIST.md
FLOW-XX-ENGINE-GAP-LIST.md
```

**Authority boundary:** Distinct from Type 23. Type 23 is remediation/fix arc; D05
is gap inventory before remediation starts.

**A-side use:** No.

**B-side chosen use:** Yes for gap inventory, severity, layer, priority order, and
which gaps must be remediated before execution.

**Rejected source:** Missing known gap, wrong severity, gap marked fixed before fix,
or remediation answer that ignores inventory ordering.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not treat a gap list as a fix.
Record flow id and gap id.
```

---

### Conversation Type D06 - Patch Addendum Documents

**Registry status:** `limited_authority`

**What it is:** Incremental addendum files that insert or modify a specific section
of an existing document.

**Examples in the XIIGen corpus:**

```text
PATCH--human-override-protocol-FC32.md
PATCH--xiigen-core-principles-P14-P15-P16.md
PATCH--contract-template-arbiters-field.md
PATCH--xiigen-core-principles-P17.md
```

**Authority boundary:** Type 16 extension with `pair_kind: patch_addendum`. A patch
is a delta, not a full replacement document.

**A-side use:** No.

**B-side chosen use:** Yes for exact inserted rule, target document, insertion
location, and before/after document behavior.

**Rejected source:** Old text before patch, applying patch to wrong target, missing
inserted rule, or treating patch as standalone source.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Target document id is mandatory.
Do not use a patch without knowing the pre-patch base.
```

---

### Conversation Type D07 - Flow Document Authoring Guides

**Registry status:** `full_type`

**What it is:** Procedural standards for writing flow specification documents:
mandatory sections, structure, formatting, and completeness gates.

**Examples in the XIIGen corpus:**

```text
XIIGEN-FLOW-DOCUMENT-AUTHORING-GUIDE-v1.md
XIIGEN-FLOW-DOCUMENT-AUTHORING-GUIDE*.md
```

**Authority boundary:** Authoring standard, not the authored flow spec itself.
Useful for structural completeness and format-gate pairs.

**A-side use:** No.

**B-side chosen use:** Yes for required section, required field, authoring order,
and flow-doc completeness checks.

**Rejected source:** Missing required section, wrong authoring structure, confusing
authoring guide with flow design content.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not answer flow architecture questions from the authoring guide.
Use it to judge document shape, not flow content.
```

---

### Conversation Type D08 - Simulation Methodology Documents

**Registry status:** `full_type`

**What it is:** Meta-process documents that define how to conduct simulation,
architectural rethinking, and gap simulation.

**Examples in the XIIGen corpus:**

```text
XIIGEN-SIMULATION-METHODOLOGY.md
XIIGEN-GAP-SIMULATION-PROTOCOL.md
XIIGEN-ARCHITECTURAL-RETHINKING.md
```

**Authority boundary:** Simulation-process authority. It is not the simulated result
and not Type 10 teaching plan.

**A-side use:** No.

**B-side chosen use:** Yes for simulation round count, success definition, gap
simulation procedure, and rethinking process.

**Rejected source:** Wrong simulation algorithm, skipped round, false success
definition, or treating methodology as completed evidence.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use methodology as a flow-specific result.
Record simulation protocol version.
```

---

### Conversation Type D09 - Per-Flow State JSON Artifacts

**Registry status:** `limited_authority`

**What it is:** Flow-specific state JSON files with task cycle budgets, artifact
numbers, expected scores, and per-phase completion fields.

**Examples in the XIIGen corpus:**

```text
FLOW-03-STATE.json
FLOW-XX-STATE.json
```

**Authority boundary:** Type 22 subtype for per-flow execution state. It is not
general workflow state and not a DPO source row by itself.

**A-side use:** No.

**B-side chosen use:** Yes for state field correctness, phase completion, budget,
artifact number, and expected score evidence.

**Rejected source:** Missing flow-specific state field, wrong phase completion,
wrong artifact number, or mixing state from another flow.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use state JSON as prose plan.
Record flow id and state schema version.
```

---

### Conversation Type D10 - Infrastructure Flows State Artifacts

**Registry status:** `limited_authority`

**What it is:** Aggregated state across infrastructure/precondition flows, used to
track readiness of prerequisite flows before main flow execution.

**Examples in the XIIGen corpus:**

```text
INFRASTRUCTURE-FLOWS-STATE.json
INFRASTRUCTURE-FLOWS-STATE.md
FLOW-PREREQ-01
FLOW-PREREQ-02
FLOW-PREREQ-03
```

**Authority boundary:** Type 22 extension for infrastructure readiness. It supports
precondition status, not business-flow behavior.

**A-side use:** No.

**B-side chosen use:** Yes for infrastructure flow readiness, prerequisite status,
and blocker-to-main-flow mapping.

**Rejected source:** Main flow run started while prerequisite is blocked, wrong
infrastructure flow state, or treating infrastructure state as user-facing behavior.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not collapse infrastructure state into business-flow state.
Record prerequisite flow id.
```

---

### Conversation Type D11 - Flow Prerequisite Documents

**Registry status:** `limited_authority`

**What it is:** Documents that define prerequisite flows, preconditions, and
readiness gates for later flow execution.

**Examples in the XIIGen corpus:**

```text
FLOW-PREREQ-*.md
FLOW-PREREQ-01.md
FLOW-PREREQ-02.md
FLOW-PREREQ-03.md
```

**Authority boundary:** Type 34 extension. These are precondition/DoD gates, not
implementation plans.

**A-side use:** No.

**B-side chosen use:** Yes for prerequisite gate, required prior flow, readiness
criterion, and blocked/unblocked state.

**Rejected source:** Missing prerequisite, wrong readiness gate, or executing a
dependent flow before prerequisite completion.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not treat prerequisite docs as implementation proof.
Record dependent flow id.
```

---

### Conversation Type D12 - Per-Flow Step Invariant Documents

**Registry status:** `full_type`

**What it is:** Flow-specific invariant documents defining constraints that a flow
must never violate during planning or execution.

**Examples in the XIIGen corpus:**

```text
FLOW-08-STEP-1-INVARIANTS.md
FLOW-NN-STEP-1-INVARIANTS.md
```

**Authority boundary:** Distinct from Type 18 portability protocols. Type 18 is
cross-flow; D12 is per-flow.

**A-side use:** No.

**B-side chosen use:** Yes for invariant, forbidden behavior, per-flow constraint,
and violation detection.

**Rejected source:** Violating invariant, applying invariant to wrong flow, or
treating invariant as completion criterion rather than behavioral constraint.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not generalize one flow's invariant to all flows.
Record flow id and invariant id.
```

---

### Conversation Type D13 - Live Engine Run Protocols

**Registry status:** `full_type`

**What it is:** Operational runbooks for live engine runs: endpoints, commands,
expected responses, and execution steps.

**Examples in the XIIGen corpus:**

```text
LIVE-ENGINE-RUN-PROTOCOL.md
```

**Authority boundary:** Execution runbook. It is not a deployment template (A12)
and not implementation code.

**A-side use:** No.

**B-side chosen use:** Yes for command order, endpoint URL, expected response,
live-run gate, and failure handling.

**Rejected source:** Wrong command, missing expected response, running against wrong
endpoint, or using run protocol as topology authority.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not train secrets or environment-specific credentials.
Record environment and protocol version.
```

---

### Conversation Type D14 - Flow Testing And OSS Teaching Protocols

**Registry status:** `full_type`

**What it is:** Combined testing and OSS teaching protocol documents used in
testing/QA sessions.

**Examples in the XIIGen corpus:**

```text
XIIGEN-FLOW-TESTING-AND-OSS-TEACHING-PROTOCOL.md
XIIGEN-FLOW-TESTING-AND-OSS-TEACHING-PROTOCOL-v*.md
```

**Authority boundary:** Multi-zone protocol: testing gate sections map to Type 8 or
Type 34; teaching sections map to Type 10/D01; OSS delivery sections map to Type 32.

**A-side use:** No.

**B-side chosen use:** Yes after zone mapping for test protocol, teaching protocol,
OSS delivery gate, and expected QA output.

**Rejected source:** Mixing testing and teaching zones, skipped OSS gate, or false
QA pass.

**Expected overlap ratio:** Inherits target zone type.

**Excluded use:**

```text
Do not use whole protocol as one row.
Zone id is mandatory.
```

---

### Conversation Type D15 - Feature Registry Documents

**Registry status:** `limited_authority`

**What it is:** Feature registry documents that define which features are planned,
active, or assigned to a session/scope.

**Examples in the XIIGen corpus:**

```text
FEATURE-REGISTRY-S1.md
FEATURE-REGISTRY-S1-PLAN-v2.md
```

**Authority boundary:** Type 30 extension for feature scope, not skill behavior.

**A-side use:** No.

**B-side chosen use:** Yes for feature id, planned session, active/inactive state,
and feature-to-scope mapping.

**Rejected source:** Feature assigned to wrong session, inactive feature treated as
active, or feature registry used as implementation evidence.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not infer implementation from feature registry alone.
Record session id.
```

---

### Conversation Type D16 - Feature Registry Plan Documents

**Registry status:** `limited_authority`

**What it is:** Plan variants attached to feature registries, describing how feature
registration should be expanded, verified, or sequenced.

**Examples in the XIIGen corpus:**

```text
FEATURE-REGISTRY-S1-PLAN-v2.md
FEATURE-REGISTRY-*-PLAN*.md
```

**Authority boundary:** Type 29/Type 30 bridge. Use for feature-registry evolution,
not direct feature implementation.

**A-side use:** No.

**B-side chosen use:** Yes for feature-registration sequence, verification step,
and scope evolution.

**Rejected source:** Wrong registry update order, missing verification, or treating
plan variant as current registry state.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use plan version as current registry without execution evidence.
Record plan version.
```

---

### Conversation Type D17 - Skill Graph Documents

**Registry status:** `full_type`

**What it is:** Graph documents describing dependencies, activation paths, or
relationships between skills.

**Examples in the XIIGen corpus:**

```text
SKILL-GRAPH-S1.md
SKILL-GRAPH-*.md
```

**Authority boundary:** Type 30 extension with graph semantics. Stronger than a flat
skills registry for dependency/activation questions.

**A-side use:** No.

**B-side chosen use:** Yes for skill dependency edge, activation path, prerequisite
skill, and graph traversal pairs.

**Rejected source:** Reversed edge, missing prerequisite, flat registry answer for
a graph question, or fabricated dependency.

**Expected overlap ratio:** 0.10-0.20.

**Excluded use:**

```text
Do not infer skill implementation from graph alone.
Record edge source and graph version.
```

---

### Conversation Type D18 - Flow Lifecycle Registry Documents

**Registry status:** `limited_authority`

**What it is:** Documents tracking where each flow is in its lifecycle: planned,
ready, in simulation, implemented, tested, blocked, or delivered.

**Examples in the XIIGen corpus:**

```text
FLOW-LIFECYCLE-S1.md
FLOW-LIFECYCLE-*.md
```

**Authority boundary:** Type 30/Type 22 bridge for lifecycle state. It is not
implementation proof.

**A-side use:** No.

**B-side chosen use:** Yes for lifecycle state, allowed next transition, blocked
reason, and session planning scope.

**Rejected source:** Wrong lifecycle state, illegal transition, omitted blocker, or
treating lifecycle state as final validation.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use lifecycle state as source code truth.
Record flow id and lifecycle version.
```

---

### Conversation Type D19 - Architectural Decision Addendum Documents

**Registry status:** `full_type`

**What it is:** Addendum documents that extend canonical architecture decision docs
with specific layered rules or convergence-handler constraints.

**Examples in the XIIGen corpus:**

```text
XIIGEN-ARCHITECTURAL-DECISION-ADDENDUM.md
```

**Authority boundary:** Type 9 extension. The addendum is authoritative only for
the specific rules it adds and only when it references the target decision scope.

**A-side use:** No.

**B-side chosen use:** Yes for added architecture rule, target decision extension,
two-layer rule, and convergence-handler constraint.

**Rejected source:** Ignoring addendum rule, applying rule outside target scope, or
treating addendum as a full replacement for the architecture decision document.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Record target decision document and addendum scope.
Do not use addendum without the base decision context.
```

---

## Second-Pass Session-Analysis Families

The `E01-E16` families below come from the second full pass over `conversations.json`.
They capture document patterns that were produced and consumed inside long sessions
but were not explicit in v11 or the earlier A/B/C/D/Z additions.
They are part of the session-history taxonomy; do not treat them as ordinary
filesystem families or deduplicate them into D01-D19 by filename alone.

---

### Conversation Type E01 - Flow Design Simulation Round Outputs

**Registry status:** `full_type`

**What it is:** Output documents for one design-simulation round of a concrete flow,
usually containing task type contracts, topology nodes, arbiter definitions, and
subflow boundaries.

**Examples in the XIIGen corpus:**

```text
FLOW-01-DESIGN-SIMULATION-R1.md
FLOW-01-DESIGN-SIMULATION-R4.md
FLOW-06-DESIGN-SIMULATION-R1.md
FLOW-07-DESIGN-SIMULATION-R1.md
FLOW-DESIGN-SIM-Rx.md
```

**Authority boundary:** Design-phase artifact. It is not FLOW-TEACH-QA and not
execution state. Use as B-side authority for ARCH and routing pairs when the round
output is accepted/canonical for that flow.

**A-side use:** No.

**B-side chosen use:** Yes for topology node, task type contract, arbiter definition,
subflow boundary, and design-round output.

**Rejected source:** Wrong round, stale design simulation used as current, topology
node invented outside the document, or teaching-QA content used as design authority.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Record flow id, design simulation round, and canonicality.
Do not collapse multiple design rounds into one row.
```

---

### Conversation Type E02 - Flow Deep Research Triple Documents

**Registry status:** `full_type`

**What it is:** XIIGen-specific formatted DR triples for a single flow: triple id,
teaching point, named checks, and arbiter criteria.

**Examples in the XIIGen corpus:**

```text
FLOW-02-DR-02-D.md
FLOW-08-DR-01.md
FLOW-19-DR-01.md
FLOW-DR-xx.md
```

**Authority boundary:** Internal DR triple source, not external deep research.
Feeds TEACH-QA and arbiter criteria for a specific flow.

**A-side use:** No.

**B-side chosen use:** Yes for teachingPoint, named check, triple id, and arbiter
criterion derived from the triple.

**Rejected source:** External research claim used as if it were XIIGen DR triple,
wrong triple id, missing named check, or teaching point assigned to wrong flow.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not mix external research reports with internal DR triples.
Record flow id and triple id.
```

---

### Conversation Type E03 - Session Brief Documents

**Registry status:** `limited_authority`

**What it is:** Short end-of-session summaries stating what changed, how to verify,
and which pre-flight checklist should be run.

**Examples in the XIIGen corpus:**

```text
SESSION-BRIEF.md
SESSION-BRIEF-*.md
```

**Authority boundary:** Distinct from full WEB-SESSION-HANDOFF. E03 is a compact
verification brief, not a full continuation package.

**A-side use:** No.

**B-side chosen use:** Yes for changed artifact summary, verification step,
pre-flight checklist, and session completion summary.

**Rejected source:** Missing verification step, wrong changed artifact, treating a
brief as full handoff context, or claiming completion without pre-flight checklist.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use session brief as source of detailed implementation truth.
Follow referenced artifacts for exact content.
```

---

### Conversation Type E04 - Pipeline Phase Master Plans

**Registry status:** `full_type`

**What it is:** Pipeline-level master plans for simulation, gap preparation, and
gap translation phases.

**Examples in the XIIGen corpus:**

```text
SIMULATION-MASTER-PLAN.md
GAP-PREP-MASTER-PLAN.md
GAP-TRANSLATE-MASTER-PLAN.md
```

**Authority boundary:** Pipeline-phase authority, not per-flow Type 29 plan. It
defines what a whole pipeline phase does and what completion criteria apply.

**A-side use:** No.

**B-side chosen use:** Yes for phase ordering, phase responsibility, completion
criteria, and pipeline-level dependency.

**Rejected source:** Wrong phase order, per-flow plan used as pipeline authority,
missing completion criteria, or applying simulation rules to gap-translate phase.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use pipeline master plan as a per-flow implementation plan.
Record pipeline phase name.
```

---

### Conversation Type E05 - Subflow Specification Documents

**Registry status:** `full_type`

**What it is:** Complete specifications of one subflow path, including nodes,
arbiters, MACHINE/FREEDOM classification, bypass conditions, and branch boundaries.

**Examples in the XIIGen corpus:**

```text
SUBFLOW-01A-SSO.md
SUBFLOW-01C.md
SUBFLOW-02B.md
SUBFLOW-03A-RATE.md
SUBFLOW-06P.md
```

**Authority boundary:** Flow-specific subflow architecture. More granular than Type
2 formal architecture and must remain tied to its parent flow.

**A-side use:** No.

**B-side chosen use:** Yes for subflow boundary, node sequence, arbiter, bypass
condition, and MACHINE/FREEDOM classification.

**Rejected source:** Wrong subflow path, branch boundary collapsed into parent flow,
wrong MACHINE/FREEDOM classification, or bypass condition omitted.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not generalize one subflow's rules to all flows.
Record parent flow id and subflow id.
```

---

### Conversation Type E06 - Simulation Gap Catalogs

**Registry status:** `full_type`

**What it is:** Round-attributed catalogs of gaps discovered during a simulation
round, usually structured by severity, root cause, and required fix.

**Examples in the XIIGen corpus:**

```text
R19-SIM-GAP-CATALOG.md
SIM-GAP-CATALOG-Rx.md
```

**Authority boundary:** Distinct from D05 pre-fix engine gap list. E06 records gaps
found during simulation and carries round attribution.

**A-side use:** No.

**B-side chosen use:** Yes for simulation-discovered gap, severity, root cause,
required fix, and simulation-round attribution.

**Rejected source:** Gap assigned to wrong round, pre-existing inventory used as
simulation finding, missing root cause, or required fix omitted.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Record simulation round and gap id.
Do not treat catalog entry as completed remediation.
```

---

### Conversation Type E07 - UI Reflection State Documents

**Registry status:** `limited_authority`

**What it is:** Per-flow or aggregate state describing whether a flow has UI,
coverage status, and which pages/screens are covered.

**Examples in the XIIGen corpus:**

```text
UI-REFLECTION-STATE.json
UI-REFLECTION-MASTER.json
```

**Authority boundary:** Type 22 extension for UI coverage metadata. It is not visual
evidence and not before/after UI quality evidence.

**A-side use:** No.

**B-side chosen use:** Yes for UI status, page coverage, flow UI scope, and UI work
planning state.

**Rejected source:** Flow marked FULL_UI when state says PARTIAL_UI, missing page
coverage, or treating UI state as visual screenshot evidence.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use UI reflection state as visual proof.
Record flow id and UI status enum.
```

---

### Conversation Type E08 - Versioned Gaps-Corrections Registers

**Registry status:** `full_type`

**What it is:** Versioned registers mapping gaps to corrections for a flow or
session across rounds.

**Examples in the XIIGen corpus:**

```text
GAPS-CORRECTIONS-v1_0.md
FLOW-07-08-GAPS-CORRECTIONS-v1_0.md
```

**Authority boundary:** Type 23 extension, but register-style. It tracks gap to
correction items across versions; it is not one concrete fix artifact by itself.

**A-side use:** No.

**B-side chosen use:** Yes for gap id, correction id, versioned correction state,
and tracking across rounds.

**Rejected source:** Wrong correction for gap, stale correction version, missing
tracking state, or treating register entry as executed fix.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Record register version, flow/session id, gap id, and correction id.
Do not use register as proof the correction was implemented.
```

---

### Conversation Type E09 - Client And Platform Architecture Specs

**Registry status:** `full_type`

**What it is:** Tier-3 design-level specs for client integration, platform
engineering, and hooks/contracts such as `useFlow`.

**Examples in the XIIGen corpus:**

```text
CLIENT-ARCHITECTURE-SPEC.md
CLIENT-SIDE-ARCHITECTURE-SPEC.md
PLATFORM-ENGINEERING-SPEC.md
```

**Authority boundary:** Type 2 architecture extension. These are design specs, not
runtime source code contracts.

**A-side use:** No.

**B-side chosen use:** Yes for client integration contract, hook contract,
platform-engineering rule, and Tier-3 architecture boundary.

**Rejected source:** Runtime code inferred from design spec, wrong hook contract,
client/server boundary confusion, or platform rule omitted.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not treat client architecture spec as source code.
Use Type 26 for exact runtime API shape.
```

---

### Conversation Type E10 - Design Review Protocol Documents

**Registry status:** `full_type`

**What it is:** Protocol for reviewing design artifacts as standalone artifacts,
including fleet-wide design review patterns.

**Examples in the XIIGen corpus:**

```text
XIIGEN-DESIGN-REVIEW-PROTOCOL-v1.md
XIIGEN-DESIGN-REVIEW-PROTOCOL*.md
```

**Authority boundary:** Type 19 extension. Distinct from code review protocol; this
reviews design artifacts and fleet design patterns.

**A-side use:** No.

**B-side chosen use:** Yes for design review step, artifact review gate, fleet-wide
review criterion, and design finding classification.

**Rejected source:** Applying code-review rule to design artifact, missing fleet
pattern check, or treating protocol as actual verdict.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not use design review protocol as design verdict.
Record protocol version.
```

---

### Conversation Type E11 - Gap Simulation Protocol Documents

**Registry status:** `full_type`

**What it is:** Specific protocol for conducting gap simulation, distinct from
general simulation methodology.

**Examples in the XIIGen corpus:**

```text
XIIGEN-GAP-SIMULATION-PROTOCOL.md
```

**Authority boundary:** Type 29/process extension. E11 is the concrete gap-simulation
protocol; D08 covers broader methodology.

**A-side use:** No.

**B-side chosen use:** Yes for gap simulation procedure, required inputs, output
format, and gap simulation success condition.

**Rejected source:** Generic simulation method used instead of gap protocol, missing
gap-specific output, or wrong phase application.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not merge gap protocol with general simulation methodology.
Record protocol version.
```

---

### Conversation Type E12 - Phase Three Preparation Guides

**Registry status:** `full_type`

**What it is:** Large procedural guides for Phase 3 preparation: required session
files, format, readiness, and preparation sequence.

**Examples in the XIIGen corpus:**

```text
PHASE-THREE-PREPARATION-GUIDE-v5.md
PHASE-THREE-PREPARATION-GUIDE*.md
```

**Authority boundary:** Type 29 extension for phase preparation, not implementation
execution and not session state.

**A-side use:** No.

**B-side chosen use:** Yes for required Phase 3 input, session file format,
preparation sequence, and readiness gate.

**Rejected source:** Missing required session file, wrong preparation order, stale
guide version, or using preparation guide as post-execution evidence.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Record guide version and target phase.
Do not use Phase 3 guide for other phases unless explicitly mapped.
```

---

### Conversation Type E13 - Topology Coverage State Documents

**Registry status:** `limited_authority`

**What it is:** Machine-readable or markdown state showing topology coverage for
flows: whether topology exists and whether it came from design-sim or step files.

**Examples in the XIIGen corpus:**

```text
TOPOLOGY-COVERAGE-STATE.json
TOPOLOGY-COVERAGE-STATE.md
```

**Authority boundary:** Type 22 extension for architecture coverage metadata.
Separate from UI reflection state.

**A-side use:** No.

**B-side chosen use:** Yes for topology file presence, coverage status, source type
of topology, and architecture scope gap analysis.

**Rejected source:** Flow marked covered without topology file, UI coverage confused
with topology coverage, or wrong topology source type.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Do not use topology coverage state as topology content.
Follow the recorded topology file for architecture details.
```

---

### Conversation Type E14 - Episode Examination Map Documents

**Registry status:** `full_type`

**What it is:** Episode and sub-episode maps used specifically for examination and
DPO pair extraction: which episode maps to which correction/task type.

**Examples in the XIIGen corpus:**

```text
EPISODE-MAP-EXTENDED.md
SUB-EPISODE-MAP.md
```

**Authority boundary:** Extends B15. B15 is narrative episode mapping; E14 is
examination/extraction mapping from episode to pair/correction type.

**A-side use:** Real prompt variants may be used only when clearly mapped to an
episode.

**B-side chosen use:** Yes for episode id, sub-episode id, extraction target,
correction type, and examination mapping.

**Rejected source:** Wrong episode-to-correction mapping, shallow episode extraction,
or using narrative summary without examination mapping.

**Expected overlap ratio:** 0.15-0.35.

**Excluded use:**

```text
Do not use episode map as final answer content.
Record episode id and extraction target.
```

---

### Conversation Type E15 - Knowledge Digest Documents

**Registry status:** `limited_authority`

**What it is:** Compact internal reference documents built from source material for
context compression and RAG-style recall inside large sessions.

**Examples in the XIIGen corpus:**

```text
KNOWLEDGE-DIGEST.md
KNOWLEDGE-DIGEST-*.md
```

**Authority boundary:** Internal digest, not original source. It can route the agent
to critical information but must not override source material.

**A-side use:** No.

**B-side chosen use:** Only as summary/routing evidence when source material is
preserved and cited.

**Rejected source:** Digest hallucination, missing source citation, using digest as
canonical instead of source, or compressing contradictory facts into one claim.

**Expected overlap ratio:** 0.15-0.30.

**Excluded use:**

```text
Do not train from digest without source provenance.
Record digest source set.
```

---

### Conversation Type E16 - Implementation Protocol And Prep Change Delta Documents

**Registry status:** `full_type`

**What it is:** Execution protocols and change-delta documents loaded with session
startup files to define how phases are executed and what changed since the previous
protocol version.

**Examples in the XIIGen corpus:**

```text
XIIGEN-IMPLEMENTATION-PROTOCOL-v3.md
FLOW-PREP-CHANGES-REQUIRED.md
```

**Authority boundary:** Implementation protocol maps to Type 15/Type 19 execution
rules. Prep changes required maps to Type 17 delta/migration logic.

**A-side use:** No.

**B-side chosen use:** Yes for execution protocol, phase execution rule, required
prep delta, changed requirement, and version-to-version change.

**Rejected source:** Old protocol step used as current, ignored prep delta, wrong
phase execution rule, or change document used without base protocol.

**Expected overlap ratio:** 0.10-0.25.

**Excluded use:**

```text
Record base protocol version and change-delta document.
Do not use delta without the protocol it modifies.
```

---

## Session-Content Derived Families

The `F01-F17` families below come from inspecting how sessions actually produce,
load, and verify artifacts. These are not just file-name categories. They are
session-output DTO shapes that must be preserved when creating training rows.

---

### Session Content Type F01 - Missing Project-Knowledge Skill Files

**Registry status:** `full_type`

**What it is:** Skill files that sessions actively load or cite but that are absent
from the project-knowledge skill corpus.

**Examples in the XIIGen corpus:**

```text
code-execution--learning-signal-capture
code-execution--topology-structure
planning--session-file-authoring
planning--root-cause-ladder
code-execution--node-convergence
planning--solution-scope-gate
planning--architect-behavior-classifier
planning--architecture-decision-capture
planning--convergence-round-design
planning--flow-retrospective
```

**Authority boundary:** These are Type 20 skill-library authorities after the actual
SKILL.md file is recovered. A session mention alone is not enough to author B-side
content; it only proves the skill is missing from the registry.

**A-side use:** No, unless the session contains a separate real developer prompt
asking for that skill behavior.

**B-side chosen use:** Yes only after the skill file content is present and versioned.
The skill boundary can then create Type C wrong-skill pairs and activation-order
pairs.

**Rejected source:** Skill activation guessed from the name, skill behavior inferred
from one session mention, or wrong-skill rejected without the recovered skill boundary.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `skill_id`, `skill_source_path`, `skill_loaded_in_session`,
`missing_from_project_knowledge=true`, `recovered_skill_version`,
`activation_rule_id`, and `skill_boundary_quote`.

---

### Session Content Type F02 - Per-Flow Architecture Decisions JSON

**Registry status:** `full_type`

**What it is:** Machine-readable architecture decisions for one flow.

**Examples in the XIIGen corpus:**

```text
FLOW-03-ARCHITECTURE-DECISIONS.json
FLOW-01-ARCHITECTURE-DECISIONS.json
```

**Authority boundary:** Type 9 extension. Unlike global `DECISIONS-LOCKED.md`, this
file is per-flow and structured. It is authority only for the named flow/version.

**A-side use:** No.

**B-side chosen use:** Yes for flow-specific architecture decisions, selected
alternative, rationale, and decision status.

**Rejected source:** Alternative rejected by the JSON rationale, stale per-flow
decision, or decision applied to the wrong flow.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `decision_id`, `decision_status`,
`decision_version`, `chosen_option`, `rejected_option`, `rationale_field`, and
`source_json_path`.

---

### Session Content Type F03 - Session Group Delivery Bundles

**Registry status:** `full_type`

**What it is:** Grouped delivery bundles that package several fixes or session files
under one theme.

**Examples in the XIIGen corpus:**

```text
SESSION-GROUP-A
SESSION-GROUP-B
SESSION-GROUP-C
SESSION-GROUP-D
SESSION-GROUP-E
```

**Authority boundary:** Type 34 and Type 32 extension. The bundle defines group
completion criteria and verifies a themed set of fixes; individual fixes still need
their own source family when authored as rows.

**A-side use:** No.

**B-side chosen use:** Yes for group scope, fix membership, completion criteria, and
bundle verification.

**Rejected source:** Treating the bundle as a single fix, dropping one fix from the
group while claiming group complete, or using group order as per-file dependency
without evidence.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `session_group_id`, `group_theme`, `fix_ids`,
`completion_criteria`, `verification_status`, and `member_artifact_paths`.

---

### Session Content Type F04 - Per-Phase Execution State JSON

**Registry status:** `full_type`

**What it is:** Per-phase gate state files recording phase-specific test counts,
created artifacts, gate outcomes, and approval checkpoints.

**Examples in the XIIGen corpus:**

```text
STATE-P1.json
STATE-P2.json
STATE-Px.json
```

**Authority boundary:** Type 22 subtype. It is not general workflow state and not
per-flow state; scope is one execution phase.

**A-side use:** No.

**B-side chosen use:** Yes for per-phase gate state, phase completion, test count,
created artifact list, and checkpoint resumption.

**Rejected source:** Phase state used as flow state, in-progress state used as
completed, or missing gate result fields.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `phase_id`, `phase_state_version`, `gate_results`,
`test_count`, `created_artifacts`, `state_status`, and `checkpoint_id`.

---

### Session Content Type F05 - Replan Summary Documents

**Registry status:** `full_type`

**What it is:** Summaries produced after a plan changes, recording what was removed,
what was added, and why.

**Examples in the XIIGen corpus:**

```text
FLOW-01-REPLAN-SUMMARY.md
REPLAN-SUMMARY.md
```

**Authority boundary:** Type 16 extension. It documents a completed plan revision,
not a carry-forward issue list.

**A-side use:** No.

**B-side chosen use:** Yes for old-plan to new-plan evolution, removed scope,
added scope, rationale, and supersession.

**Rejected source:** Old plan presented as current, replan delta used without base
plan, or "why" omitted from the chosen.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `old_plan_id`, `new_plan_id`, `removed_items`,
`added_items`, `replan_reason`, and `supersedes`.

---

### Session Content Type F06 - Community Flow Map And Gap Register Documents

**Registry status:** `limited_authority`

**What it is:** Community/OSS delivery flow maps and gap registers separate from the
internal XIIGen flow registry.

**Examples in the XIIGen corpus:**

```text
XIIGEN-COMMUNITY-FLOW-MAP.md
XIIGEN-COMMUNITY-GAP-REGISTER.md
```

**Authority boundary:** Limited community-delivery authority only. It must not be
mixed with internal flow registry rows unless the row explicitly asks about community
delivery.

**A-side use:** Yes for community-delivery prompts.

**B-side chosen use:** Yes for community flow map and community gap register only.

**Rejected source:** Treating community gaps as internal platform gaps, or using
internal flow state to answer community scope.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `delivery_scope=community`, `community_flow_id`,
`community_gap_id`, `internal_mapping_allowed=false` unless cited, and
`source_scope`.

---

### Session Content Type F07 - Snapshot State Bundles

**Registry status:** `preprocess_then_reclassify`

**What it is:** Snapshot bundles created before major restructuring: a state file,
reference plan, and learning backup strategy.

**Examples in the XIIGen corpus:**

```text
FLOW-0-SNAPSHOT-STATE.json
FLOW-0-SNAPSHOT-REFERENCE-PLAN.md
FLOW-0-LEARNING-BACKUP-STRATEGY.md
```

**Authority boundary:** Bundle is a container. Its parts reclassify separately:
state to Type 22, reference plan to Type 29/Type 16, backup strategy to Type 34 or
Type 19 depending on content.

**A-side use:** No.

**B-side chosen use:** Only after part-level classification.

**Rejected source:** Snapshot state used as current state, backup strategy used as a
validation result, or snapshot bundle treated as one document type.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `snapshot_id`, `snapshot_created_before`, `bundle_parts`,
`part_family`, `snapshot_canonicality=extracted_copy`, and `reclassify_target_type`.

---

### Session Content Type F08 - Reconciliation State Documents

**Registry status:** `full_type`

**What it is:** Documents that correct a discrepancy between an implementation state
file and actual flow reality.

**Examples in the XIIGen corpus:**

```text
FLOW-XX-RECONCILIATION-STATE.md
```

**Authority boundary:** Type 22 and Type 23 bridge. It is a correction document
about state discrepancy, not the state file itself.

**A-side use:** No.

**B-side chosen use:** Yes for reconciled reality, demonstrated wrong state, and
correction reason.

**Rejected source:** IMPL-STATE claim that the reconciliation document marks as
demonstrably wrong.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `conflicting_state_path`, `reconciled_state`,
`discrepancy_evidence`, `correction_reason`, and `superseded_state_fields`.

---

### Session Content Type F09 - Flow Graph Extension Documents

**Registry status:** `full_type`

**What it is:** Documents extending an existing flow graph with new task types,
nodes, or edges.

**Examples in the XIIGen corpus:**

```text
FLOW-GRAPH-EXTENSION.md
GRAPH-EXTENSION.md
T587+ task type extensions
```

**Authority boundary:** Type 2/Type 27 extension. It adds to an existing graph; it
does not define a new flow unless the document says so.

**A-side use:** No.

**B-side chosen use:** Yes for added node, added edge, task type extension, and
dependency relation.

**Rejected source:** New graph treated as standalone flow, extension applied to the
wrong base graph, or edge direction reversed.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `base_graph_id`, `extension_id`, `added_node_ids`,
`added_edge_ids`, `task_type_ids`, and `base_graph_version`.

---

### Session Content Type F10 - Planning Requirements Directory Artifacts

**Registry status:** `preprocess_then_reclassify`

**What it is:** Persistent planning requirements directories containing multiple
requirements artifacts across sessions.

**Examples in the XIIGen corpus:**

```text
PLANNING-REQUIREMENTS/
```

**Authority boundary:** Directory is a container. Each file inside must be classified
as requirement, plan, state, checklist, or decision.

**A-side use:** No.

**B-side chosen use:** Only after file-level classification.

**Rejected source:** Directory name used as proof of a requirement, or multiple
requirements files collapsed without preserving requirement IDs.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `requirements_directory_path`, `requirement_file_path`,
`requirement_id`, `reclassify_target_type`, and `directory_snapshot_id`.

---

### Session Content Type F11 - Phase Delivery Summary Structures

**Registry status:** `full_type`

**What it is:** Standard session-produced delivery summaries with sections such as
what was built, files created, test breakdown, and delivery result.

**Examples in the XIIGen corpus:**

```text
What was built
Files Created
Test Breakdown
```

**Authority boundary:** Type 32 extension for phase delivery. It reports what was
delivered and verified in that phase.

**A-side use:** No.

**B-side chosen use:** Yes for delivery summary, files-created table, test breakdown,
and phase verification result.

**Rejected source:** Claimed delivery without file table, test count mismatch, or
summary used as a plan before delivery happened.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `phase_id`, `built_items`, `created_files`,
`test_breakdown`, `verification_result`, and `delivery_timestamp`.

---

### Session Content Type F12 - Requirements Traceability Reports

**Registry status:** `full_type`

**What it is:** Session-produced "Validation Against Requirements" reports mapping
delivered artifacts back to requirements.

**Examples in the XIIGen corpus:**

```text
Validation Against Requirements
requirements traceability per phase
```

**Authority boundary:** Type 32/Type 34 extension. It verifies requirements coverage;
it does not define the requirements themselves.

**A-side use:** No.

**B-side chosen use:** Yes for requirement-to-artifact coverage, missing coverage,
and traceability verdict.

**Rejected source:** PASS when requirement lacks artifact evidence, or requirement
invented from delivery prose.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `requirement_id`, `artifact_path`, `coverage_status`,
`evidence_locator`, and `traceability_verdict`.

---

### Session Content Type F13 - Flow Status Matrix Documents

**Registry status:** `full_type`

**What it is:** Multi-flow status matrices produced in architect sessions.

**Examples in the XIIGen corpus:**

```text
FLOW STATUS MATRIX
```

**Authority boundary:** Type 7/Type 32 extension. It is a matrix of flow statuses,
not a replacement for detailed state files.

**A-side use:** No.

**B-side chosen use:** Yes for matrix status, per-flow coverage, and status comparison.

**Rejected source:** Matrix status applied to a different flow, stale status used as
current, or matrix status treated as implementation evidence without file proof.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `matrix_id`, `flow_id`, `status_cell`, `status_timestamp`,
`evidence_path`, and `matrix_version`.

---

### Session Content Type F14 - Teach-QA Review Outputs

**Registry status:** `full_type`

**What it is:** Review output documents for TEACH-QA rounds.

**Examples in the XIIGen corpus:**

```text
TEACH-QA REVIEW
```

**Authority boundary:** Type 8/Type 32 extension. It reviews a teach-QA artifact; it
is not the teach-QA source itself.

**A-side use:** No.

**B-side chosen use:** Yes for review findings, pass/concern/reprocess verdicts,
and named QA defects.

**Rejected source:** Review finding used as the original QA requirement, or PASS
verdict despite unresolved reprocess finding.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `teach_qa_artifact_id`, `review_round`, `finding_id`,
`verdict`, `severity`, and `review_evidence`.

---

### Session Content Type F15 - Delivery Verification Reports From Uploaded Package

**Registry status:** `full_type`

**What it is:** Session-produced reports that verify what was actually delivered in
an uploaded package.

**Examples in the XIIGen corpus:**

```text
WHAT WAS DELIVERED (verified in this zip)
```

**Authority boundary:** Type 32 extension. The uploaded package must be inspected;
claims are invalid without file-level verification.

**A-side use:** No.

**B-side chosen use:** Yes for verified delivered files, missing files, and artifact
presence/absence.

**Rejected source:** Delivery claim not found in package, package listing used
without extraction, or filename assumed from documentation.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `package_path`, `extraction_path`, `verified_file_paths`,
`missing_file_paths`, `verification_method`, and `package_hash`.

---

### Session Content Type F16 - Phased Session Loading Plans

**Registry status:** `full_type`

**What it is:** Session-gate documents that define phase groups to load before work
begins.

**Examples in the XIIGen corpus:**

```text
Phase Group 1 - RAG Preparation
Phase Group 2 - Integration Design
```

**Authority boundary:** Type 15 extension. It is a loading plan, not an execution
result.

**A-side use:** No.

**B-side chosen use:** Yes for phase-group load order, required documents, and gate
readiness.

**Rejected source:** Loading groups reordered without dependency evidence, phase
group used as completed work, or omitted required document.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `load_plan_id`, `phase_group_id`, `document_ids`,
`load_order`, `readiness_gate`, and `base_session_context`.

---

### Session Content Type F17 - Architecture Session Output Verification Blocks

**Registry status:** `full_type`

**What it is:** Architect-session verification blocks that summarize what was
delivered, what was verified, and what remains open after a design/review session.

**Examples in the XIIGen corpus:**

```text
WHAT WAS DELIVERED
verified in uploaded package
open issues after architect session
```

**Authority boundary:** Type 32 extension. It is a review/session output report, not
the original design document.

**A-side use:** No.

**B-side chosen use:** Yes for verified delivery, open issue list, and architect
session closeout verdict.

**Rejected source:** Report used as source of original requirement, unverified claim
marked delivered, or open issue omitted from completion verdict.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `session_id`, `verified_artifacts`, `open_issue_ids`,
`closeout_verdict`, `verification_evidence`, and `source_package_path` when used.

---

## Named Session Document Families

The `H01-H08` families were identified by extracting named artifacts from
`additional file types sessions.md`. They are deliberately explicit even when they
map to existing numbered types, because these names appear repeatedly in session
handoffs and can otherwise be swallowed by broad labels.

---

### Named Document Type H01 - GraphRAG Planning And Attribution Documents

**Registry status:** `full_type`

**What it is:** Documents that plan, attribute, or extend GraphRAG/RAG benchmark
coverage for XIIGen flows.

**Examples in the XIIGen corpus:**

```text
RAG-BENCHMARK-TO-XIIGEN-FLOWS-PLANNING-SKILL.md
GRAPHRAG_NODES_ATTRIBUTION.md
FEEDBACK_GRAPHRAG_EXTENSION.md
```

**Authority boundary:** Type 21 internal RAG and Type 31 mapping extension. These
documents can map RAG nodes to flows and explain attribution, but must not become
general architecture authority without a Type 9 decision.

**A-side use:** No.

**B-side chosen use:** Yes for RAG-to-flow attribution, benchmark planning, RAG node
ownership, and extension mapping.

**Rejected source:** RAG node attributed to the wrong flow, benchmark plan used as
completed benchmark result, or extension map used without source node id.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `rag_node_id`, `flow_id`, `benchmark_id`, `attribution_source`,
`connection_type`, `extension_id`, and `mapping_version`.

---

### Named Document Type H02 - Marketplace Research State And Store Listing Documents

**Registry status:** `limited_authority`

**What it is:** Marketplace research state and store listing documents.

**Examples in the XIIGen corpus:**

```text
STATE-marketplace-research.json
STORE_LISTING.md
```

**Authority boundary:** U24 marketplace/legal exception. Store listing text is
marketplace material, not architecture authority. Marketplace research state is
Type 22 state for that research workflow only.

**A-side use:** Yes only for marketplace/document-writing prompts.

**B-side chosen use:** Yes for store listing structure or marketplace research state,
not for product architecture.

**Rejected source:** Store copy used as technical source, marketplace state used as
implementation state, or legal/market claim used without source.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `marketplace_artifact_kind`, `listing_section`,
`research_state_status`, `source_scope=marketplace`, and `technical_authority=false`.

---

### Named Document Type H03 - Validation And Migration Certificate Documents

**Registry status:** `full_type`

**What it is:** Formal certificates and validation reports produced after a phase,
migration, or architecture review.

**Examples in the XIIGen corpus:**

```text
PHASE_19_CERTIFICATE.md
MIGRATION_CERTIFICATE.md
ENGINE_ARCHITECTURE_DR101_DR266_CERT.md
VALIDATION_REPORT.md
```

**Authority boundary:** Type 32 extension. A certificate records a verdict for a
specific scope and version; it does not define the plan or requirements.

**A-side use:** No.

**B-side chosen use:** Yes for certificate verdict, validated scope, failed checks,
and migration/phase completion evidence.

**Rejected source:** Certificate reused outside its scope, PASS claimed for a failed
check, or validation report used as original requirement.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `certificate_id`, `validated_scope`, `verdict`,
`check_ids`, `evidence_paths`, `validated_version`, and `issued_at`.

---

### Named Document Type H04 - Plan Comparison And Drift Analysis Documents

**Registry status:** `full_type`

**What it is:** Documents comparing alternative plans or detecting drift between
planned and actual architecture/execution.

**Examples in the XIIGen corpus:**

```text
PLAN_COMPARISON.md
DRIFT_ANALYSIS.md
APPROACH_COMPARISON_IMPROVED.md
CONFLICT_ANALYSIS.md
```

**Authority boundary:** Type 33 extension. Unchosen alternatives and drift findings
can become rejected content when explicitly present in the document.

**A-side use:** No.

**B-side chosen use:** Yes for chosen approach, rejected approach, drift finding,
and resolution rationale.

**Rejected source:** Rejected alternative invented by the author, drift claim without
baseline/current pair, or comparison used without version ids.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `comparison_id`, `baseline_version`, `current_version`,
`chosen_option`, `rejected_option`, `drift_id`, and `resolution_rationale`.

---

### Named Document Type H05 - Definition Of Done And Tier Prerequisite Documents

**Registry status:** `full_type`

**What it is:** Documents defining completion criteria or prerequisites for tiers,
phases, or deliverables.

**Examples in the XIIGen corpus:**

```text
XIIGEN_DEFINITION_OF_DONE.md
TIER1-PREREQUISITES-PLAN.md
```

**Authority boundary:** Type 34 extension. These documents define gates, not proof
that the gates passed.

**A-side use:** No.

**B-side chosen use:** Yes for prerequisite checklist, DoD criteria, and reprocess
verdict when any required item is missing.

**Rejected source:** DoD document used as validation result, missing criterion
ignored, or prerequisite order guessed without explicit chain.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `dod_id`, `criterion_id`, `prerequisite_id`, `gate_order`,
`required_evidence`, and `all_criteria_checked`.

---

### Named Document Type H06 - Task Type Catalog Documents

**Registry status:** `full_type`

**What it is:** Catalogs of task types, task IDs, and their lifecycle or phase
coverage.

**Examples in the XIIGen corpus:**

```text
TASK_TYPES_CATALOG_VXX.md
TASK_TYPES_T1_T100_P11.md
```

**Authority boundary:** Type 5 and Type 30 extension. These are routing and task
catalog authorities, not implementation proof.

**A-side use:** No.

**B-side chosen use:** Yes for task type ID, task type name, sub-phase mapping, and
task catalog coverage.

**Rejected source:** Wrong task type ID, stale catalog version, task type used as
proof of implementation, or task phase inferred from name only.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `task_type_id`, `task_type_name`, `catalog_version`,
`sub_phase_id`, `lifecycle_stage`, and `catalog_source_path`.

---

### Named Document Type H07 - Generic Phase Plan And Phase State Templates

**Registry status:** `limited_authority`

**What it is:** Generic `PHASE_N_PLAN` and `PHASE_N_STATE` templates used as
scaffolds for concrete phase artifacts.

**Examples in the XIIGen corpus:**

```text
PHASE_N_PLAN.md
PHASE_N_STATE.json
```

**Authority boundary:** Template/scaffold only. Concrete phase files inherit Type 29
or Type 22; generic N templates are not executable evidence.

**A-side use:** No.

**B-side chosen use:** Structural template only after placeholders are bound to a
concrete phase.

**Rejected source:** Placeholder `N` left unresolved, generic template used as
current phase state, or phase status invented from template.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `template_id`, `bound_phase_id`, `placeholders_resolved`,
`target_type`, and `template_source_path`.

---

### Named Document Type H08 - Session Gap And Session Plan Artifacts

**Registry status:** `full_type`

**What it is:** Named session-level gap and plan artifacts that are not necessarily
flow-specific.

**Examples in the XIIGen corpus:**

```text
SESSION-GAP-R
SESSION-PLAN-
SESSION-HANDOFF
```

**Authority boundary:** Session-gap files map to Type 23 gap inventory/remediation
logic. Session-plan files map to Type 29. Session-handoff maps to Type 15/19
depending on whether it is context loading or execution guidance.

**A-side use:** No.

**B-side chosen use:** Yes after subtype identification: gap, plan, or handoff.

**Rejected source:** Session gap used as completed fix, session plan used as state,
or handoff text used as developer prompt.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `session_artifact_kind`, `session_id`, `round_id`,
`target_type`, `handoff_target_session`, and `gap_or_plan_id`.

---

## Runtime And Archive Artifact Families

The `G01-G13` families come from a fourth pass over session history focused on
artifacts passed between sessions, runtime outputs, ZIP bundles, and short sessions.
They are not ordinary planning docs. Most G-family rows require either runtime
locator fields, archive manifests, or per-flow contract fields.

---

### Runtime Artifact Type G01 - Execution Log JSON Files

**Registry status:** `full_type`

**What it is:** Per-phase runtime execution logs produced by live product-flow runs.

**Examples in the XIIGen corpus:**

```text
EXECUTION-LOG-A.json
EXECUTION-LOG-X.json
```

**Authority boundary:** Type 4 extension for runtime measurement, not DPO training
metrics. ArbiterUnit pairs may use command pass/fail evidence from the log.

**A-side use:** No.

**B-side chosen use:** Yes for command output, test counts, build result, error
output, and runtime pass/fail evidence.

**Rejected source:** Calibration metrics substituted for product runtime logs, log
used without command id, or raw noisy output used without normalization.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `phase_id`, `execution_log_id`, `command_id`, `exit_code`,
`test_counts`, `build_result`, `error_summary`, and `log_path`.

---

### Runtime Artifact Type G02 - Learning Handoff JSON Files

**Registry status:** `full_type`

**What it is:** Inter-phase data transfer artifact produced after flow execution and
passed into later training sessions.

**Examples in the XIIGen corpus:**

```text
LEARNING-HANDOFF.json
```

**Authority boundary:** Type 22 subtype with learning-transfer semantics. It is not
raw training rows and not a generated dataset.

**A-side use:** No.

**B-side chosen use:** Yes for handoff fields, live-run calibration data, pattern IDs
activated, reward signals, and next-session input state.

**Rejected source:** Treating handoff as training dataset rows, missing pattern ID
verification, or using stale handoff as current state.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `handoff_id`, `source_phase_id`, `target_session_id`,
`pattern_ids`, `reward_signal`, `calibration_source`, `state_status`, and
`handoff_path`.

---

### Runtime Artifact Type G03 - Per-Flow Design Corpus JSON Files

**Registry status:** `full_type`

**What it is:** Curated per-flow design corpus seed files containing pattern IDs,
training triples, and design reasoning.

**Examples in the XIIGen corpus:**

```text
DESIGN-CORPUS
flow-01-design-corpus.json
flow-02-design-corpus.json
```

**Authority boundary:** Distinct from A07 dataset rows. G03 is curated design seed
data and requires a three-check rule for current pattern IDs.

**A-side use:** No.

**B-side chosen use:** Yes for pattern design, design-reasoning triples, and
flow-specific seed corpus content after pattern validation.

**Rejected source:** Generated row treated as curated corpus, stale pattern ID,
corpus used before the three-check rule, or corpus applied to wrong flow.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `corpus_id`, `pattern_ids`, `triple_ids`,
`design_reasoning_id`, `pattern_ids_confirmed_via`, and `corpus_path`.

---

### Runtime Artifact Type G04 - Per-Flow RAG Specification Documents

**Registry status:** `full_type`

**What it is:** Per-flow RAG documents listing RAG patterns, triples, and activation
conditions for one flow.

**Examples in the XIIGen corpus:**

```text
FLOW-01-RAG.md
FLOW-02-RAG.md
```

**Authority boundary:** Internal Type 21 subtype, but per-flow. Distinct from global
history RAG indexes.

**A-side use:** No.

**B-side chosen use:** Yes for flow-specific RAG pattern activation, triple mapping,
and routing evidence.

**Rejected source:** Global RAG fixture substituted for per-flow RAG, wrong flow
activation condition, or triple applied outside its flow.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `rag_doc_id`, `pattern_id`, `triple_id`,
`activation_condition`, and `rag_source_path`.

---

### Runtime Artifact Type G05 - Validation Matrix Sections

**Registry status:** `full_type`

**What it is:** Requirements traceability sections inside plans, usually a table like
`Requirement | Covered By`.

**Examples in the XIIGen corpus:**

```text
VALIDATION-MATRIX
| Requirement | Covered By |
```

**Authority boundary:** Type 29 Zone D. This is a section/zone, not necessarily a
standalone file.

**A-side use:** No.

**B-side chosen use:** Yes for requirement coverage and ArbiterUnit pass/fail pairs.

**Rejected source:** PASS when `Covered By` is empty, requirement invented from plan
title, or matrix row detached from the plan/version that owns it.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `source_family=G05`, `zone_id=Type29.ZoneD`,
`requirement_id`, `covered_by_artifact`, `coverage_status`, `matrix_row_id`, and
`owning_plan_id`.

---

### Archive Artifact Type G06 - Flow Prep Round Bundles

**Registry status:** `preprocess_then_reclassify`

**What it is:** Archive deliverable for one FLOW-PREP round containing session files
for one flow.

**Examples in the XIIGen corpus:**

```text
FLOW-PREP-R0.zip
FLOW-PREP-Rx.zip
```

**Authority boundary:** Archive container. Extract, require manifest, then classify
each contained session file.

**A-side use:** No.

**B-side chosen use:** Only after extraction and file-level classification.

**Rejected source:** ZIP filename used as proof of completed prep, missing manifest,
or contained files merged into one row.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `archive_path`, `archive_kind=flow_prep_round`,
`flow_id`, `round_id`, `content_manifest_path`, `extraction_path`, and
`archive_member_path`.

---

### Archive Artifact Type G07 - Gap Prep And Gap Translate Process Bundles

**Registry status:** `preprocess_then_reclassify`

**What it is:** Archive deliverables for GAP-PREP and GAP-TRANSLATE pipeline phases.

**Examples in the XIIGen corpus:**

```text
GAP-PREP-PROCESS-Rx.zip
GAP-TRANSLATE-PROCESS-Rx.zip
```

**Authority boundary:** Archive container. Extract and classify round session files
by content.

**A-side use:** No.

**B-side chosen use:** Only after extraction.

**Rejected source:** GAP-PREP bundle used as GAP-TRANSLATE evidence, or process
bundle treated as completed fix without contained verification.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `archive_path`, `archive_kind`, `pipeline_phase`,
`round_id`, `content_manifest_path`, `extraction_path`, and `archive_member_path`.

---

### Archive Artifact Type G08 - Final Flow Delivery Bundles

**Registry status:** `preprocess_then_reclassify`

**What it is:** Final flow delivery archives containing all session files ready for
execution.

**Examples in the XIIGen corpus:**

```text
flow-01-final-v4.zip
flow-NN-final-vX.zip
FLOW-03-COMPLETE-R1-R17.zip
```

**Authority boundary:** Canonical flow delivery archive only if manifest confirms
complete contents. The archive itself is not a DPO row source.

**A-side use:** No.

**B-side chosen use:** Only after extraction and manifest verification.

**Rejected source:** Final archive assumed complete by filename, missing content
manifest, or older final version used as current.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `archive_path`, `archive_kind=final_flow_delivery`,
`flow_id`, `delivery_version`, `content_manifest_path`, `manifest_complete`,
`extraction_path`, and `archive_member_path`.

---

### Archive Artifact Type G09 - Session Startup Document Bundles

**Registry status:** `preprocess_then_reclassify`

**What it is:** Curated document packages required to start architect or planning
sessions.

**Examples in the XIIGen corpus:**

```text
session_setup_main_docs.zip
xiigen_start_session_files.zip
```

**Authority boundary:** Archive container for session-start docs. Canonical only if
the session startup package version is current.

**A-side use:** No.

**B-side chosen use:** Only after extraction and per-document classification.

**Rejected source:** Treating startup bundle as repo snapshot, using old startup
bundle as current, or skipping manifest check.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `archive_path`, `archive_kind=session_startup_docs`,
`startup_package_version`, `current_package_confirmed_via`, `content_manifest_path`,
and `archive_member_path`.

---

### Archive Artifact Type G10 - Pool Session Bundles

**Registry status:** `preprocess_then_reclassify`

**What it is:** Archive bundles of pool session files for specific task types.

**Examples in the XIIGen corpus:**

```text
POOL-SESSIONS-T581-T582-T583.zip
```

**Authority boundary:** Archive container. Each pool session file must be classified
by task type and session role after extraction.

**A-side use:** No.

**B-side chosen use:** Only after extraction.

**Rejected source:** Task-type pool archive used without member-level task type, or
wrong task type inferred from archive filename alone.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `archive_path`, `archive_kind=pool_sessions`,
`task_type_ids`, `pool_id`, `content_manifest_path`, and `archive_member_path`.

---

### Archive Artifact Type G11 - Reference Documentation Bundles

**Registry status:** `preprocess_then_reclassify`

**What it is:** Read-only architecture/history documentation bundles used as input
to architect sessions.

**Examples in the XIIGen corpus:**

```text
module-architecture.zip
history.zip
```

**Authority boundary:** Archive container. Extract and classify each member as
architecture, history, decision, state, or validation document.

**A-side use:** No.

**B-side chosen use:** Only after member-level classification.

**Rejected source:** Bundle title used as source authority, or history bundle used
as current architecture without version check.

**Expected overlap ratio:** inherited from target type.

**DTO requirements:** `archive_path`, `archive_kind=reference_docs`,
`reference_scope`, `content_manifest_path`, `extraction_path`, and
`archive_member_path`.

---

### Source Contract Type G12 - Per-Flow TypeScript Contract And Seed Prompt Files

**Registry status:** `full_type`

**What it is:** Per-flow TypeScript runtime contract and seed-prompt files.

**Examples in the XIIGen corpus:**

```text
flow0-contracts.ts
flow25-contracts.ts
flow0-seed-prompts.ts
flow-XX-contracts.ts
flow-XX-seed-prompts.ts
```

**Authority boundary:** Type 26 subtype for TypeScript per-flow contracts. Seed
prompts are source contracts for prompt fields, not A-side developer prompts by
default.

**A-side use:** No, unless the file stores a real developer prompt with provenance.

**B-side chosen use:** Yes for DTO field names, contract ownership, seed prompt
schema, and flow-specific runtime contract.

**Rejected source:** Field invented from prose, TS contract applied to another flow,
or seed prompt text used as real developer input.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `contract_file_path`, `export_name`,
`field_names`, `seed_prompt_ids`, `contract_version`, and `language=typescript`.

---

### Source Contract Type G13 - Bootstrap Seeder Source Files

**Registry status:** `limited_authority`

**What it is:** Bootstrap-specific source files that seed initial flow/topology data.

**Examples in the XIIGen corpus:**

```text
bootstrap-seeder.service.ts
BootstrapSeeder
```

**Authority boundary:** Type 5 subtype 5b. It can define bootstrap topology/seed
behavior, but not general topology or all seeder behavior.

**A-side use:** No.

**B-side chosen use:** Yes for bootstrap seeding order, bootstrap-only topology
records, and initial seed contract.

**Rejected source:** Bootstrap seeder generalized to all seeders, seeding order
guessed from filenames, or runtime contract invented without source.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `source_family=G13`, `subtype=Type5b.bootstrap_seeder`,
`seeder_file_path`, `seed_record_ids`, `bootstrap_phase_id`, and
`topology_source_ids`.

---

## Session-Message Output Families

The `I01-I14` families come from reading assistant thinking blocks, tool calls, and
message content structures by session type. These artifacts were missed by filename
and archive scans because they often appear inside bash commands, thinking blocks,
or generated structured sections rather than as uploaded file names.

---

### Session Message Type I01 - Versioned Task Types Catalogs

**Registry status:** `full_type`

**What it is:** Versioned central catalogs of task types and T-number mappings.

**Examples in the XIIGen corpus:**

```text
TASK_TYPES_CATALOG_V8.md
TASK_TYPES_CATALOG_V13.md
TASK_TYPES_CATALOG_MERGED.md
TASK_TYPES_T1_T100_P11.md
```

**Authority boundary:** Distinct from Type 9 locked decisions. This is a living data
catalog. It supports Type 16 versioned evolution, task-type existence checks, and
routing pairs.

**A-side use:** No.

**B-side chosen use:** Yes for task type id, task type name, flow family, sub-phase
mapping, and catalog continuity.

**Rejected source:** Stale T-number, non-existent task type, wrong flow family, or
catalog version used without version field.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `catalog_id`, `catalog_version`, `task_type_id`,
`task_type_name`, `flow_family`, `sub_phase_id`, and `merged_from_versions`.

---

### Session Message Type I02 - BFA Rules Documents

**Registry status:** `full_type`

**What it is:** Business Flow Arbitration numbered rule documents and embedded BFA
rule sections.

**Examples in the XIIGen corpus:**

```text
V62_BFA_DESIGN_UPDATE.md
V62_BFA.md
CF-1 through CF-191+ rules
```

**Authority boundary:** Type 12 extension with explicit versioning. Standalone BFA
documents and embedded BFA sections must preserve source version and rule number.

**A-side use:** No.

**B-side chosen use:** Yes for CF rule application, flow compliance, and arbitration
criteria.

**Rejected source:** CF rule from stale version, embedded rule extracted without
parent document version, or stress-test ST rule treated as CF rule.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `bfa_version`, `rule_family=CF`, `rule_id`,
`parent_document_id`, `embedded_or_standalone`, and `rule_text_locator`.

---

### Session Message Type I03 - Stress Test Rule Documents

**Registry status:** `full_type`

**What it is:** Stress-test scenario documents and ST-numbered rules used to test
BFA compliance.

**Examples in the XIIGen corpus:**

```text
V62_BFA_STRESS_TEST_MERGED.md
ST-1 through ST-91+ rules
```

**Authority boundary:** Type 12 subtype. ST rules are scenarios/checks, not CF
arbitration rules.

**A-side use:** No.

**B-side chosen use:** Yes for stress-test scenario, expected failure mode, and
stress coverage.

**Rejected source:** ST rule treated as CF rule, stress scenario used as completed
validation, or missing linked CF rule.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `stress_test_version`, `rule_family=ST`, `st_rule_id`,
`linked_cf_rule_id`, `scenario_scope`, and `expected_stress_outcome`.

---

### Session Message Type I04 - Merged Canonical Document Family

**Registry status:** `full_type`

**What it is:** Canonical merged output documents produced from multiple accumulated
versioned source documents.

**Examples in the XIIGen corpus:**

```text
TASK_TYPES_CATALOG_MERGED.md
ENGINE_ARCHITECTURE_MERGED.md
SKILLS_FACTORY_RAG_MERGED.md
UNIFIED_SOURCE_INDEX_MERGED.md
```

**Authority boundary:** Type 16 merge-output subtype. A merged document can become
canonical only when merge source versions and continuity checks are recorded.

**A-side use:** No.

**B-side chosen use:** Yes for canonical merged content, continuity checks, and
supersession of part documents.

**Rejected source:** Old part used as current after merged exists, merge continuity
break (for example F197 after F196 missing), or merged file used without source map.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `merged_document_id`, `merged_document_kind`,
`source_versions`, `merge_phase_id`, `continuity_check_passed`,
`supersedes_documents`, and `canonical_confirmed_via`.

---

### Session Message Type I05 - Session Type Taxonomy

**Registry status:** `full_type`

**What it is:** Internal taxonomy used in thinking/session analysis to classify
sessions by what they produce.

**Examples in the XIIGen corpus:**

```text
PLANNING
MAINTENANCE
DESIGN
INVESTIGATION
DATA_PIPELINE
ORCHESTRATION
GENERATION
```

**Authority boundary:** Classification schema. It does not by itself create a DPO
pair, but it decides which artifact family and DTO gates apply.

**A-side use:** No.

**B-side chosen use:** Yes for session classification, expected artifact families,
and routing of extracted artifacts.

**Rejected source:** DATA_PIPELINE session classified as PLANNING, ORCHESTRATION
parallel plan treated as FLOW_IMPL, or session type inferred from one word only.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `session_id`, `session_type`, `classification_evidence`,
`expected_artifact_families`, and `message_range`.

---

### Session Message Type I06 - New Task Planning Prompt Documents

**Registry status:** `full_type`

**What it is:** Planning prompt templates loaded with session-gate startup docs for
new tasks.

**Examples in the XIIGen corpus:**

```text
NEW-TASK-PLANNING-PROMPT.md
```

**Authority boundary:** Type 15 subtype. Unlike HOW-TO-USE, this is the actual
planning prompt content, not just skill loading order.

**A-side use:** No. Prompt templates are not real developer prompts.

**B-side chosen use:** Yes for planning prompt structure and required fields.

**Rejected source:** Planning prompt template used as A-side, missing required
planning fields, or startup loading order confused with prompt content.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `planning_prompt_id`, `template_version`, `required_fields`,
`startup_bundle_id`, and `template_not_a_side=true`.

---

### Session Message Type I07 - Fleet Flow State Registry JSON

**Registry status:** `full_type`

**What it is:** Aggregate JSON registry of all flow completion statuses and FLOW-PREP
file counts.

**Examples in the XIIGen corpus:**

```text
FLOW-STATE-REGISTRY.json
```

**Authority boundary:** Type 22 subtype 22e. Distinct from infrastructure flow state
and per-flow state.

**A-side use:** No.

**B-side chosen use:** Yes for fleet-level flow status, completion state, and file
count checks.

**Rejected source:** Per-flow state used as fleet registry, stale completion status,
or file count claimed without registry row.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `registry_version`, `flow_id`, `flow_status`,
`flow_prep_file_count`, `status_updated_at`, and `registry_path`.

---

### Session Message Type I08 - Architect Behavior Execution Plans

**Registry status:** `full_type`

**What it is:** Versioned execution plans for architect behavior in specific review
or planning scenarios.

**Examples in the XIIGen corpus:**

```text
ARCHITECT-BEHAVIOR-EXECUTION-PLAN-R1.md
ARCHITECT-BEHAVIOR-EXECUTION-PLAN-R2.md
```

**Authority boundary:** Type 29 role-specific extension. It is an execution plan for
the architect role, not a general Type 19 tool instruction.

**A-side use:** No.

**B-side chosen use:** Yes for role behavior sequence, review scenario handling, and
round-versioned architect procedure.

**Rejected source:** R1 behavior used after R2 supersedes it, role behavior inferred
from generic code-review protocol, or step sequence omitted.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `role=architect`, `plan_round`, `scenario_id`,
`behavior_step_id`, `supersedes_round`, and `loaded_with_protocols`.

---

### Session Message Type I09 - Per-Flow Fix Maps

**Registry status:** `full_type`

**What it is:** Fleet-level maps from each gap to a concrete flow and fix action.

**Examples in the XIIGen corpus:**

```text
PER-FLOW-FIX-MAP-v2_8.md
```

**Authority boundary:** Type 23 extension. Distinct from GAP-TO-GUIDANCE-MAP: this
maps to concrete flow fix actions, not abstract guidance.

**A-side use:** No.

**B-side chosen use:** Yes for flow-specific fix action, gap-to-flow mapping, and
fix ownership.

**Rejected source:** Gap mapped to wrong flow, abstract guidance used as concrete
fix, or fix action missing flow id.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `fix_map_version`, `gap_id`, `flow_id`, `fix_action_id`,
`owner_artifact`, and `fix_status`.

---

### Session Message Type I10 - Visual Proof Plans

**Registry status:** `full_type`

**What it is:** Plans for visual verification coverage by flow/session document.

**Examples in the XIIGen corpus:**

```text
VISUAL-PROOF-PLAN-v1.0.md
```

**Authority boundary:** Type 18/Type 34 extension. It defines visual proof work; it
is not the screenshot or proof result.

**A-side use:** No.

**B-side chosen use:** Yes for visual verification target, required proof, and flow
visual coverage plan.

**Rejected source:** Visual proof plan used as completed visual evidence, missing
flow visual target, or screenshot requirement omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `visual_plan_version`, `flow_id`, `visual_target_id`,
`proof_method`, `required_artifact`, and `proof_status`.

---

### Session Message Type I11 - Track Topology Plans

**Registry status:** `full_type`

**What it is:** Track-specific topology coverage repair plans.

**Examples in the XIIGen corpus:**

```text
TRACK-A-TOPOLOGY-PLAN.md
TRACK-B-TOPOLOGY-PLAN.md
```

**Authority boundary:** Type 23 topology-domain extension. It is inventory plus fix
plan for topology coverage, not the topology file itself.

**A-side use:** No.

**B-side chosen use:** Yes for missing topology inventory, assigned track, and
topology repair action.

**Rejected source:** Topology plan used as completed topology file, wrong track, or
source document missing.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `track_id`, `flow_ids`, `missing_topology_ids`,
`source_documents`, `repair_action_ids`, and `track_plan_version`.

---

### Session Message Type I12 - Delivery Summary Documents

**Registry status:** `full_type`

**What it is:** Final session/package delivery summaries intended as the "start
here" confirmation for a delivery package.

**Examples in the XIIGen corpus:**

```text
DELIVERY_SUMMARY.md
DELIVERY_PACKAGE_SUMMARY.md
```

**Authority boundary:** Type 34/Type 32 extension. Distinct from SESSION-BRIEF:
this is end-of-session or delivery-package confirmation for a human reader.

**A-side use:** No.

**B-side chosen use:** Yes for delivered artifact list, package summary, completion
status, and remaining issues.

**Rejected source:** Delivery summary used as implementation plan, delivered status
claimed without artifact list, or SESSION-BRIEF substituted for package summary.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `delivery_summary_id`, `package_id`, `delivered_artifacts`,
`completion_status`, `remaining_issues`, and `start_here=true`.

---

### Session Message Type I13 - Data Pipeline Session Outputs

**Registry status:** `limited_authority`

**What it is:** Session-type output family for DATA_PIPELINE sessions producing
corpus files, training datasets, RAG indexes, and handoff files.

**Examples in the XIIGen corpus:**

```text
DATA_PIPELINE session type
corpus files
training datasets
RAG indexes
```

**Authority boundary:** Classification standard. Each produced artifact must still
map to G02/G03/G04/H01/A07 or another concrete family.

**A-side use:** No.

**B-side chosen use:** Only for session classification and expected artifact routing.

**Rejected source:** DATA_PIPELINE label used as source family instead of classifying
the produced artifact.

**Expected overlap ratio:** inherited from concrete target type.

**DTO requirements:** `session_type=DATA_PIPELINE`, `produced_artifact_paths`,
`artifact_target_families`, and `pipeline_stage`.

---

### Session Message Type I14 - Orchestration Session Outputs

**Registry status:** `limited_authority`

**What it is:** Session-type output family for ORCHESTRATION sessions producing
session groups, parallel execution plans, and cross-session coordination artifacts.

**Examples in the XIIGen corpus:**

```text
ORCHESTRATION session type
session groups
parallel execution plans
```

**Authority boundary:** Classification standard. Actual artifacts map to F03,
F16, H08, or Type 29 depending on content.

**A-side use:** No.

**B-side chosen use:** Only for routing artifacts to correct family and detecting
parallel execution plan structure.

**Rejected source:** ORCHESTRATION label used as final source type, or parallel plan
used without dependency/join fields.

**Expected overlap ratio:** inherited from concrete target type.

**DTO requirements:** `session_type=ORCHESTRATION`, `parallel_group_ids`,
`join_condition`, `produced_artifact_paths`, and `artifact_target_families`.

---

## Active Project And Training Package Families

The `J01-J09` families come from scanning all ZIP locations beyond Older directory:
active `llm_mvp_core`, `xiigen_hist2`, and `xiigen_sess` packages. These are not
stale archival documents by default. Some are current experiment/training artifacts
and need stronger DTO gates than normal plans.

---

### Active Project Type J01 - Subproject Reference Maps

**Reported id:** `B76`

**Registry status:** `full_type`

**What it is:** Component inventory maps for active multi-project codebases.

**Examples in the XIIGen corpus:**

```text
llm_mvp_core/docs/SUBPROJECT-MAP.md
```

**Authority boundary:** Component inventory and status authority for the named
project. Distinct from document authority maps: this maps subprojects, not concepts.

**A-side use:** No.

**B-side chosen use:** Yes for subproject number, directory, purpose, status, and
key files.

**Rejected source:** Wrong subproject number, stale status, or concept authority map
used as component inventory.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `project_id`, `subproject_number`, `subproject_directory`,
`subproject_name`, `status`, `key_files`, and `map_version`.

---

### Active Project Type J02 - Experiment Results Maps

**Reported id:** `B77`

**Registry status:** `full_type`

**What it is:** Navigation maps summarizing many experiments, validation status,
delta metrics, scale-up status, and target component use.

**Examples in the XIIGen corpus:**

```text
llm_mvp_core/docs/EXPERIMENT-RESULTS-MAP.md
```

**Authority boundary:** Type 4 navigation/summary extension. It is not raw run
measurement; use J05 for individual run JSON.

**A-side use:** No.

**B-side chosen use:** Yes for experiment-to-component routing, validation summary,
delta interpretation, and use-in mapping.

**Rejected source:** Treating summary as raw measurement, wrong component routing,
or validated status contradicted by run result.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `experiment_id`, `map_version`, `validation_status`,
`delta_metric`, `use_in_component`, `source_result_path`, and `map_path`.

---

### Active Project Type J03 - LLM Subproject Session Plans

**Reported id:** `B78`

**Registry status:** `full_type`

**What it is:** Implementation/session plans for ML subprojects with round-trip
steps, experiment references, preconditions, and inter-subproject dependencies.

**Examples in the XIIGen corpus:**

```text
llm_mvp_core/docs/plans/*.md
```

**Authority boundary:** Type 29 extension for ML subproject implementation. Distinct
from flow implementation plans.

**A-side use:** No.

**B-side chosen use:** Yes for phase order, round-trip step, experiment dependency,
and subproject dependency.

**Rejected source:** Flow plan used as subproject plan, missing experiment reference,
or dependency order reversed.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `subproject_id`, `plan_id`, `round_trip_step_id`,
`experiment_refs`, `preconditions`, `dependency_subprojects`, and `plan_language`.

---

### Active Project Type J04 - ML Research Hypothesis Plans

**Reported id:** `B79`

**Registry status:** `full_type`

**What it is:** Research hypothesis plans with project map, tiered datasets,
threshold rules, expected results, and micro-experiment framework.

**Examples in the XIIGen corpus:**

```text
llm_mvp_core/docs/plans/test hypothesys/*.md
MICRO-EXPERIMENT-FRAMEWORK
```

**Authority boundary:** Research-plan source. It creates hypothesis/tier progression
pairs, not implementation-status pairs.

**A-side use:** Yes only for ML researcher-style prompts.

**B-side chosen use:** Yes for hypothesis framing, baseline/enhanced comparison,
tier-scaling rule, and expected-result contract.

**Rejected source:** Hypothesis without baseline, tier scale-up without threshold,
or implementation plan confused with research hypothesis.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `hypothesis_id`, `tier_id`, `scale_up_threshold`,
`baseline_metric`, `enhanced_metric`, `expected_result`, and `subproject_map`.

---

### Active Project Type J05 - Experiment Run Result Files

**Reported id:** `B80`

**Registry status:** `full_type`

**What it is:** Machine-readable experiment run results with baseline/enhanced
values, improvement percent, pass flag, next action, scaling signal, and notes.

**Examples in the XIIGen corpus:**

```text
llm_mvp_core/experiments-v2/E01.../results/run-YYYY-MM-DD-NNN.json
```

**Authority boundary:** Type 4 measurement evidence extension with automated tier
progression semantics.

**A-side use:** No.

**B-side chosen use:** Yes for experiment result DTO, pass/fail, next action, and
scale-up decision.

**Rejected source:** PASS when improvement is below threshold, wrong next_action,
missing baseline/enhanced value, or human summary used instead of JSON.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `experiment_id`, `tier`, `run_at`, `primary_metric`,
`baseline_value`, `enhanced_value`, `improvement_pct`, `passed`, `next_action`,
`scaling_signal`, and `result_path`.

---

### Active Project Type J06 - Incremental Flow Build Snapshots

**Reported id:** `B81`

**Registry status:** `full_type`

**What it is:** Versioned codebase snapshot chain where each archive captures state
after a phase.

**Examples in the XIIGen corpus:**

```text
FLOW33_P1.zip
FLOW33_WITH_P0_P1_P2_P3_P4.zip
FLOW33_FINAL_COMPLETE.zip
```

**Authority boundary:** Type 16 archive chain. Final complete snapshot is canonical;
intermediate snapshots are stale but useful as rejected/evolution evidence.

**A-side use:** No.

**B-side chosen use:** Yes after extraction for version-evolution pairs and phase
delta evidence.

**Rejected source:** Intermediate snapshot claimed current, phase delta inferred
without comparing members, or final snapshot assumed complete without manifest.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `snapshot_chain_id`, `phase_id`, `archive_path`,
`snapshot_canonicality`, `previous_snapshot`, `next_snapshot`, `content_manifest_path`,
and `delta_summary`.

---

### Active Project Type J07 - Flow Session Input/Output File Pair Archives

**Reported id:** `B82`

**Registry status:** `full_type`

**What it is:** Paired archives for a flow session: input files loaded at start and
output files produced by the session.

**Examples in the XIIGen corpus:**

```text
N-input files.zip
N-files.zip
15-input files.zip
15-files.zip
```

**Authority boundary:** High-value orchestration source. Input archive informs prompt
construction and context; output archive provides expected produced artifacts.

**A-side use:** Only after extracting real developer/session prompt from input.

**B-side chosen use:** Yes after extraction for produced artifact set, expected output
shape, and input-to-output mapping.

**Rejected source:** Output produced without matching input archive, input file used
as output, or pair constructed without flow/session id.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `flow_id`, `session_id`, `input_archive_path`,
`output_archive_path`, `input_manifest`, `output_manifest`, `artifact_mapping`, and
`archive_member_path`.

---

### Active Project Type J08 - Conversation Export Batch Archives

**Reported id:** `B83`

**Registry status:** `preprocess_then_reclassify`

**What it is:** Large Claude API conversation export archives containing users,
projects, memories, and conversations JSON.

**Examples in the XIIGen corpus:**

```text
data-6308de18-...-batch-0000.zip
users.json
projects.json
memories.json
conversations.json
```

**Authority boundary:** Archive container. Must be split into individual conversations
and then classified as prompts, corrections, gap remediation, session artifacts, etc.

**A-side use:** Only after real user prompt extraction and provenance retention.

**B-side chosen use:** Only after per-conversation classification.

**Rejected source:** Raw conversation dump used as one source, assistant output used
as user prompt, or memories/project metadata mixed into training rows.

**Expected overlap ratio:** inherited from extracted target type.

**DTO requirements:** `export_batch_id`, `archive_path`, `conversation_id`,
`message_id`, `message_role`, `content_block_type`, `extraction_path`, and
`target_family`.

---

### Active Project Type J09 - Teaching Cycle Packages

**Reported id:** `B84`

**Registry status:** `full_type`

**What it is:** Complete DPO teaching cycle package with manifest, session map,
layer-specific YAML examples, held-out corruption/rephrase sets, cycle summary,
teaching ledger, and DPO checkpoints.

**Examples in the XIIGen corpus:**

```text
xiigen-cycle-001-last-product-session-internal-first.zip
manifest.yaml
session-map.yaml
layer-0-a/XIIGEN-LPS-001-compiler.yaml
layer-0-b/XIIGEN-LPS-001-router.yaml
held-out/XIIGEN-LPS-001-corruption.yaml
cycle-summary.json
teaching-ledger.jsonl
checkpoints/xiigen-cycle-001/3-3/dpo_step_000001.json
```

**Authority boundary:** Highest-value teaching-structure source. It is the actual
training artifact package, not guidance about training. Use as B-side authority for
correct teaching package DTO shapes and measurement rows.

**A-side use:** No.

**B-side chosen use:** Yes for teaching cycle manifest, layer file shape, held-out
set structure, ledger record, cycle summary, and checkpoint metadata.

**Rejected source:** Missing layer file, corrupted held-out partition, ledger row
without checkpoint link, or treating checkpoint as source text.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `cycle_id`, `manifest_path`, `session_map_path`,
`layer_id`, `training_file_path`, `held_out_kind`, `ledger_row_id`,
`checkpoint_path`, `dpo_step_id`, `cycle_summary_path`, and `package_archive_path`.

---

## Bash And View Path Discovered Families

The `K01-K13` families come from systematic scanning of bash command bodies and
view tool paths. These paths reveal what files the project actually loads and
opens during sessions. They must be classified by runtime use, not only by filename.

---

### Bash/View Type K01 - Master Run Instruction Inputs

**Registry status:** `full_type`

**What it is:** Human-provided master instruction files for a build or consolidation
cycle.

**Examples in the XIIGen corpus:**

```text
basic_prompt.txt
basic prompt.txt
CONSOLIDATION_PROMPT.md
```

**Authority boundary:** Input instruction document. Distinct from HOW-TO-USE-SKILLS
because it tells the agent what to build, not which skills to load.

**A-side use:** Yes when it is the human input instruction for the run.

**B-side chosen use:** Yes for required deliverables, structural completeness, and
build-cycle instructions.

**Rejected source:** Treating basic_prompt as session output, ignoring required
deliverables, or using stale basic prompt for a newer build cycle.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `run_instruction_id`, `build_cycle_id`, `instruction_version`,
`required_deliverables`, `loaded_by_user=true`, `source_prompt_role=input_instruction`.

---

### Bash/View Type K02 - Numbered Flow Session Documents

**Registry status:** `full_type`

**What it is:** Number-prefixed per-flow session context documents and matching deep
research extensions.

**Examples in the XIIGen corpus:**

```text
33-system_initiation.md
02-business-onboarding.md
04-post-publishing.md
05-lesson-completion-gamification.md
05-lesson-completion-gamification_deep_research.md
12_-_ERP_systems_deep_search_engine_multi_tenant.md
```

**Authority boundary:** Type 35 subtype by NN-prefix convention. The `_deep_research`
variant is an extension for the same numbered flow topic.

**A-side use:** No unless a separate developer prompt is embedded and marked.

**B-side chosen use:** Yes for per-flow context, design/research facts, and numbered
flow session scope.

**Rejected source:** Deep research extension used without base session doc, wrong
NN-prefix flow, or session doc used as completed implementation.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_number`, `flow_slug`, `session_doc_kind`,
`base_session_doc`, `deep_research_doc`, and `nn_prefix`.

---

### Bash/View Type K03 - Versioned Combined Engine Plans

**Registry status:** `full_type`

**What it is:** Versioned engine-level master plans that integrate previous plan
versions plus new additions.

**Examples in the XIIGen corpus:**

```text
V42_COMBINED_PLAN.md
V45_COMBINED_PLAN.md
V52_COMBINED_PLAN.md
V57_COMBINED_PLAN.md
V61_MASTER_PLAN_v2.md
V71_PLAN_v3.md
```

**Authority boundary:** Type 29 engine-version extension and Type 16 evolution
source. It is not a per-phase plan.

**A-side use:** No.

**B-side chosen use:** Yes for engine version plan, dependency order, versioned
additions, and supersession.

**Rejected source:** Wrong V-number dependency, old combined plan used as current,
or phase-level plan substituted for engine-level plan.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `engine_version`, `combined_plan_id`, `supersedes_versions`,
`added_sections`, `dependency_chain`, and `plan_scope=engine_version`.

---

### Bash/View Type K04 - Unified Plan Synthesis Documents

**Registry status:** `full_type`

**What it is:** Cohesive unified architecture/planning synthesis documents that act
as "start here" source for a delivery package.

**Examples in the XIIGen corpus:**

```text
XIIGEN_UNIFIED_PLAN.md
UNIFIED_PLAN.md
UNIFIED-PLAN-FINAL.md
UNIFIED-PLAN-v4_5.md
```

**Authority boundary:** Type 2 synthesis extension. Distinct from combined plans:
unified plan synthesizes the current whole system view.

**A-side use:** No.

**B-side chosen use:** Yes for system-level architecture synthesis, current cohesive
view, and cross-version reconciliation.

**Rejected source:** Unified plan treated as phase plan, stale unified plan used as
current, or partial plan substituted for synthesis.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `unified_plan_id`, `plan_version`, `synthesizes_sources`,
`current_view_confirmed_via`, and `system_scope`.

---

### Bash/View Type K05 - Per-Flow Master Execution Plans

**Registry status:** `full_type`

**What it is:** Per-flow execution plans with no-code explanation, validation matrix,
phases, RAG patterns, and state management.

**Examples in the XIIGen corpus:**

```text
FLOW07_MASTER_EXECUTION_PLAN.md
FLOW05_ENGINE_EXTENSION_UNIFIED.md
FLOW08_UNIFIED_EXECUTION_PLAN.md
```

**Authority boundary:** Type 29 per-flow execution subtype. Distinct from reference
plans, which describe what the flow does.

**A-side use:** No.

**B-side chosen use:** Yes for execution sequence, per-flow phase order, validation
matrix, RAG pattern use, and state management.

**Rejected source:** Reference plan used as execution plan, validation matrix omitted,
or wrong flow execution order.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `flow_id`, `execution_plan_id`, `phase_ids`,
`validation_matrix_id`, `rag_pattern_ids`, and `state_management_fields`.

---

### Bash/View Type K06 - Master Execution Plan Merged Documents

**Registry status:** `full_type`

**What it is:** Merged canonical master execution plan documents.

**Examples in the XIIGen corpus:**

```text
MASTER_EXECUTION_PLAN_MERGED.md
```

**Authority boundary:** I04 merged canonical document family, specific to master
execution plans. It completes the merged engine-state family.

**A-side use:** No.

**B-side chosen use:** Yes for canonical merged master execution plan content and
supersession of partial master plans.

**Rejected source:** Partial master execution plan used after merged canonical exists,
or merged file used without source versions.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `merged_document_id`, `merged_document_kind=master_execution_plan`,
`source_versions`, `continuity_check_passed`, and `supersedes_documents`.

---

### Bash/View Type K07 - DrawIO XML Diagram Files

**Registry status:** `full_type`

**What it is:** DrawIO XML diagrams with machine-readable graph structure.

**Examples in the XIIGen corpus:**

```text
building_a_code_generating_process_drawio.xml
flow_propagation_drawio.xml
Process_of_data.drawio.xml
umls.drawio.xml
```

**Authority boundary:** A13/Type 27 diagram subtype. The XML graph must be parsed;
do not flatten to a screenshot or prose summary first.

**A-side use:** No.

**B-side chosen use:** Yes for node/edge topology, graph direction, and dependency
relationships.

**Rejected source:** DrawIO used as visual-only asset, edge direction reversed, or
raw XML copied into A-side.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `diagram_format=drawio_xml`, `diagram_path`, `node_ids`,
`edge_ids`, `edge_direction`, and `parsed_graph_hash`.

---

### Bash/View Type K08 - Component-Scoped CSS Debug Histories

**Registry status:** `full_type`

**What it is:** Component-prefixed CSS debug history JSON files.

**Examples in the XIIGen corpus:**

```text
export_part_of_scene_css-debug-history.json
cart_updated_css-debug-history.json
login-1_css-debug-history.json
```

**Authority boundary:** Type 24/U23 subtype. It is generated-code debug evidence
scoped to one UI component.

**A-side use:** No. A-side is the original UI bug/task prompt if present.

**B-side chosen use:** Yes for component defect, wrong generated CSS, corrected CSS,
and verification evidence.

**Rejected source:** Debug history used without component id, uncorrected generated
CSS used as chosen, or component bug generalized to whole app.

**Expected overlap ratio:** 0.20-0.45.

**DTO requirements:** `component_id`, `debug_history_path`, `wrong_css_revision`,
`corrected_css_revision`, `verification_signal`, and `defect_class`.

---

### Bash/View Type K09 - Definitive Module Architecture Documents

**Registry status:** `full_type`

**What it is:** Definitive module architecture documents used as architecture
authority for a module.

**Examples in the XIIGen corpus:**

```text
DEFINITIVE-MODULE-ARCHITECTURE.md
```

**Authority boundary:** Type 2 extension. It is module-specific architecture
authority when confirmed current.

**A-side use:** No.

**B-side chosen use:** Yes for module boundaries, component responsibility, and
module-level architecture decisions.

**Rejected source:** Non-definitive draft used as current, module boundary applied
to another module, or architecture inferred from implementation files.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `module_id`, `architecture_doc_id`, `definitive=true`,
`current_version_confirmed_via`, and `module_boundary_ids`.

---

### Bash/View Type K10 - Engine Readiness And Domain Fabric Plans

**Registry status:** `full_type`

**What it is:** Engine/domain-specific readiness and fabric implementation plans.

**Examples in the XIIGen corpus:**

```text
XIIGEN-ENGINE-READINESS-PLAN.md
FLOW-SECRETS-FABRIC-PLAN.md
```

**Authority boundary:** Type 29 domain-plan extension. These are domain readiness
plans, not validation reports.

**A-side use:** No.

**B-side chosen use:** Yes for readiness gates, domain fabric steps, and dependency
ordering.

**Rejected source:** Readiness plan treated as passed readiness, secrets fabric step
omitted, or wrong domain plan applied.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `domain_plan_id`, `domain`, `readiness_gate_ids`,
`fabric_step_ids`, `prerequisite_chain`, and `plan_status`.

---

### Bash/View Type K11 - Self-Sufficiency Design Documents

**Registry status:** `full_type`

**What it is:** Design documents describing self-sufficiency principles or
independent-operation architecture.

**Examples in the XIIGen corpus:**

```text
SELF-SUFFICIENCY design docs
```

**Authority boundary:** Type 2 extension. Only current self-sufficiency docs can be
B-side authority.

**A-side use:** No.

**B-side chosen use:** Yes for independence constraints, no-external-dependency
design, and self-sufficiency architecture.

**Rejected source:** Self-sufficiency principle applied without current doc, or
dependency added that violates the design rule.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `self_sufficiency_doc_id`, `principle_id`,
`dependency_constraint`, `allowed_external_dependency`, and `violation_mode`.

---

### Bash/View Type K12 - Tool Maturity Lifecycle Documents

**Registry status:** `full_type`

**What it is:** Versioned lifecycle documents describing tool maturity stages.

**Examples in the XIIGen corpus:**

```text
Vxx-TOOL-MATURITY-LIFECYCLE.md
```

**Authority boundary:** Type 16 extension. It is lifecycle evolution evidence, not
implementation proof.

**A-side use:** No.

**B-side chosen use:** Yes for maturity stage, lifecycle transition, and superseded
tool behavior.

**Rejected source:** Tool maturity stage from stale lifecycle version, lifecycle
state used as implementation status, or illegal transition.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `tool_id`, `lifecycle_version`, `maturity_stage`,
`transition_id`, `previous_stage`, and `next_stage`.

---

### Bash/View Type K13 - Skills-To-Engine Dynamic Mapping Documents

**Registry status:** `full_type`

**What it is:** Documents mapping skill behavior into dynamic engine behavior.

**Examples in the XIIGen corpus:**

```text
XIIGEN-SKILLS-TO-ENGINE-DYNAMIC.md
```

**Authority boundary:** Type 2/Type 30 bridge. It maps skill registry knowledge into
engine architecture, but does not replace either source.

**A-side use:** No.

**B-side chosen use:** Yes for skill-to-engine mapping, dynamic behavior relation,
and engine integration boundary.

**Rejected source:** Skill treated as engine component directly, engine behavior
invented from skill name, or mapping used without both skill and engine ids.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `skill_id`, `engine_component_id`, `mapping_rule_id`,
`dynamic_behavior_id`, `source_skill_doc`, and `source_engine_doc`.

---

## Remaining Older Directory Repository Families

The `L01-L08` families come from the final deep read of the remaining Older
directory repository ZIPs. They are source-level documents inside historical
repositories, not generic archive labels.

---

### Older Repository Type L01 - Skill Routing Guides

**Registry status:** `full_type`

**What it is:** Scenario-to-skill-chain routing documents.

**Examples in the XIIGen corpus:**

```text
ROUTING_GUIDE.md
```

**Authority boundary:** Type 20/30 bridge. It answers which skills to invoke, and
in what order, for a scenario. It is not a RAG file-to-use-case index.

**A-side use:** Scenario prompt only when explicitly present.

**B-side chosen use:** Yes for skill chain order, required intermediate skills, and
scenario routing.

**Rejected source:** Missing a required skill, reversing chain order, or substituting
a file reference map for routing logic.

**Expected overlap ratio:** 0.10-0.20.

**DTO requirements:** `scenario_id`, `skill_chain`, `chain_order`, `required_skills`,
`routing_source_path`, and `canonicality`.

---

### Older Repository Type L02 - DNA Pattern Application Guides

**Registry status:** `full_type`

**What it is:** Practical guide for applying DNA patterns across skills.

**Examples in the XIIGen corpus:**

```text
GENIE_DNA_GUIDE.md
```

**Authority boundary:** Type 2/20 bridge. It is practical application guidance, not
only genealogy or philosophy.

**A-side use:** Only direct questions about applying a DNA pattern.

**B-side chosen use:** Yes for pattern-to-skill application, anti-pattern/correct
pattern examples, and per-skill DNA coverage.

**Rejected source:** Generic DNA philosophy used where an application rule is
required, typed-model implementation where dynamic-document DNA is required, or
wrong pattern version.

**Expected overlap ratio:** 0.10-0.20.

**DTO requirements:** `dna_pattern_id`, `skill_id`, `application_rule_id`,
`anti_pattern_id`, `correct_pattern_id`, and `dna_guide_version`.

---

### Older Repository Type L03 - Scope Isolation Guides

**Registry status:** `full_type`

**What it is:** Multi-tenancy scope isolation implementation specifications.

**Examples in the XIIGen corpus:**

```text
SCOPE_ISOLATION_GUIDE.md
```

**Authority boundary:** Type 25/security-pattern extension. It defines query scope,
ownership, visibility levels, and AI-agent scoping rules.

**A-side use:** Security/scope question only when explicit.

**B-side chosen use:** Yes for scope levels, query injection rules, ownership
patterns, and violation modes.

**Rejected source:** Public/team/private scope mixed, query without scope injection,
or AI agent allowed cross-scope access without rule support.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `scope_level`, `ownership_rule_id`, `query_scope_rule`,
`allowed_visibility`, `agent_scope_rule`, and `violation_mode`.

---

### Older Repository Type L04 - Phased Implementation Prompt Libraries

**Registry status:** `full_type`

**What it is:** Ordered prompt libraries for implementation phases.

**Examples in the XIIGen corpus:**

```text
PROMPTS.md
```

**Authority boundary:** Input prompt library. Distinct from `basic_prompt.txt`:
L04 contains many phase-specific prompts with exact files, packages, classes, and
technology constraints.

**A-side use:** Yes only for the phase prompt itself when it is the human input.

**B-side chosen use:** Yes for prompt phase order, exact required files/packages,
and scaffold requirements.

**Rejected source:** Wrong phase prompt, missing project references, wrong package
or technology version, or using prompt library as completed output.

**Expected overlap ratio:** 0.05-0.15.

**DTO requirements:** `prompt_library_id`, `phase_id`, `prompt_text_locator`,
`required_files`, `required_packages`, `required_classes`, and `technology_version`.

---

### Older Repository Type L05 - AI Tool Instruction Files Per Tool

**Registry status:** `full_type`

**What it is:** Tool-specific AI instruction files.

**Examples in the XIIGen corpus:**

```text
copilot-instructions.md
.cursorrules
.clinerules
CLAUDE.md
```

**Authority boundary:** Type 19 extension. Each file is canonical only for its target
tool; do not merge Copilot/Cursor/Cline/Claude rules into one generic instruction.

**A-side use:** No.

**B-side chosen use:** Yes for tool-specific runtime behavior, cross-references,
and mandatory rule order.

**Rejected source:** Applying Copilot rules to Cline, ignoring tool-specific syntax,
or using stale tool instructions.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `tool_name`, `instruction_file`, `tool_specific_rules`,
`cross_references`, `rule_ids`, and `canonical_for_tool=true`.

---

### Older Repository Type L06 - Skill-To-Reference RAG Indexes

**Registry status:** `full_type`

**What it is:** Permanent library indexes mapping each skill to must-read and
should-read reference documents.

**Examples in the XIIGen corpus:**

```text
V17_RAG_INDEX.md
```

**Authority boundary:** Skill library reference index. Distinct from session-level
DOC_RAG_INDEX, which maps uploaded files to use cases.

**A-side use:** No.

**B-side chosen use:** Yes for skill-to-reference mapping, must-read/should-read
classification, and reference prerequisites.

**Rejected source:** Session RAG index substituted for permanent skill index, missing
critical reference, or wrong skill reference set.

**Expected overlap ratio:** 0.05-0.10.

**DTO requirements:** `skill_id`, `must_read_refs`, `should_read_refs`,
`reference_index_version`, `reference_path`, and `mapping_kind`.

---

### Older Repository Type L07 - Secrets-Aware Configuration Templates

**Registry status:** `full_type`

**What it is:** Runtime config templates with secret and environment substitution
syntax.

**Examples in the XIIGen corpus:**

```text
config/config_template.json
```

**Authority boundary:** A12/Type 25 configuration-template extension. It defines
template structure, not live secret values.

**A-side use:** No.

**B-side chosen use:** Yes for config sections, `$secret:` references, `$env:`
references, fallback providers, and required runtime defaults.

**Rejected source:** Hardcoded credentials, missing fallback provider, wrong secret
path, or schema-only document used as concrete config template.

**Expected overlap ratio:** 0.05-0.10.

**DTO requirements:** `template_id`, `config_section`, `secret_refs`, `env_refs`,
`fallback_provider`, `sanitized=true`, and `no_plaintext_secret=true`.

---

### Older Repository Type L08 - Stack Translation Plans

**Registry status:** `full_type`

**What it is:** Versioned cross-stack translation plans and corrections.

**Examples in the XIIGen corpus:**

```text
XIIGEN_NESTJS_REACT_TRANSLATION_PLAN_v1.md
XIIGEN_NESTJS_REACT_TRANSLATION_PLAN_v2.md
XIIGEN_NESTJS_REACT_TRANSLATION_PLAN_v3.md
```

**Authority boundary:** Type 17/23 bridge. v1 is the initial translation plan; later
versions may be corrections for missing layers or skipped flow requirements.

**A-side use:** No unless the source contains an explicit translation task prompt.

**B-side chosen use:** Yes for stack mapping, missing-layer correction, translation
dependency order, and stale-stack rejection.

**Rejected source:** Older stack plan used after correction, skipped missing layers,
or direct file copy without pattern translation.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `translation_plan_version`, `source_stack`, `target_stack`,
`supersedes_plan`, `missing_layer_ids`, `translation_dependency_order`, and
`correction_reason`.

---

## Training Run And Teaching Cycle Families

The `M01-M12` families come from llm training run ZIPs, `xiigen_cycle_001`, and
the remaining `xiigen_hist2` packages. They are live training telemetry or
machine-readable teaching pipeline artifacts, not ordinary documentation.

---

### Training Type M01 - Training Run Batch-Summary Logs

**Registry status:** `full_type`

**What it is:** JSONL batch-level telemetry from internal training runs.

**Examples in the XIIGen corpus:**

```text
batch-summary.jsonl
```

**Authority boundary:** Type 4 measurement evidence extension for batch-level
training telemetry.

**A-side use:** No.

**B-side chosen use:** Yes for batch index, domain, row/branch counts, verdict,
elapsed time, verification status, and failure codes.

**Rejected source:** Treating RESOURCE_REVIEW as failure, claiming verification pass
when `VerificationPassed=false`, or using summary lines as DPO pairs.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `run_id`, `batch_index`, `domain`, `row_count`, `branch_count`,
`verdict`, `verification_passed`, `verification_failures`, and `elapsed_ms`.

---

### Training Type M02 - DPO Training Step Checkpoints

**Registry status:** `full_type`

**What it is:** Per-step DPO checkpoint JSON with reward and loss metrics.

**Examples in the XIIGen corpus:**

```text
checkpoints/batch-NNN/domain/sub-phase/dpo_step_000001.json
```

**Authority boundary:** Type 4 low-level measurement evidence. It is never a text
pair source.

**A-side use:** No.

**B-side chosen use:** Yes only for measurement questions about step, loss,
chosen/rejected reward, reward margin, and accuracy.

**Rejected source:** Progress claim without checkpoint evidence, wrong margin sign,
or checkpoint used as an authored DPO pair.

**Expected overlap ratio:** 0.05-0.15.

**DTO requirements:** `run_id`, `batch_id`, `domain`, `sub_phase_id`, `step`,
`loss`, `chosen_reward`, `rejected_reward`, `reward_margin`, and `accuracy`.

---

### Training Type M03 - Experiment Result JSON Schemas

**Registry status:** `full_type`

**What it is:** Formal JSON Schema definitions for experiment result files.

**Examples in the XIIGen corpus:**

```text
docs/experiments/schema/experiment-result.schema.json
```

**Authority boundary:** Type 25 schema contract for M/J experiment-result records.

**A-side use:** No.

**B-side chosen use:** Yes for required fields, enum values, numeric thresholds, and
schema-valid result shape.

**Rejected source:** Experiment result missing required fields, invalid `next_action`,
or schema confused with a run result.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `schema_id`, `schema_version`, `required_fields`,
`enum_fields`, `numeric_threshold_fields`, and `applies_to_family=M03/J05`.

---

### Training Type M04 - Session-To-Training Mapping Documents

**Registry status:** `full_type`

**What it is:** YAML semantic map from raw session/source material into training
prompts, dependencies, decisions, and arbiters.

**Examples in the XIIGen corpus:**

```text
session-map.yaml
```

**Authority boundary:** Teaching pipeline source map. It sits before pair YAML
generation and must preserve source fact, prompt, dependency, decision, and arbiter
provenance.

**A-side use:** Prompts inside the map may become A-side only when `prompt_id` and
source provenance are recorded.

**B-side chosen use:** Yes for session-to-training mapping, extracted facts,
dependency reasons, decision mapping, and arbiter correction fields.

**Rejected source:** Prompt extracted without source fact, dependency edge without
reason, or arbiter row without wrong/correct behavior.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `source_id`, `source_title`, `last_paragraph_fact`,
`prompt_id`, `sub_phases_mentioned`, `dependency_edges`, `decision_ids`,
`arbiter_ids`, and `source_locator`.

---

### Training Type M05 - Training Pair YAML Files

**Registry status:** `full_type`

**What it is:** Machine-readable DPO training pair YAML files with A and B sections.

**Examples in the XIIGen corpus:**

```text
layer-0-a/XIIGEN-LPS-001-compiler.yaml
layer-0-b/XIIGEN-LPS-001-router.yaml
layer-0-c/XIIGEN-LPS-001-knowledge-edge-and-capability.yaml
layer-1/XIIGEN-LPS-001-transition.yaml
```

**Authority boundary:** Concrete training pair DTO. This is the strictest DTO source
in the registry companion.

**A-side use:** Yes from the `A` block only.

**B-side chosen use:** Yes from the `B` block only.

**Rejected source:** A/B blocks swapped, missing `contract_signature`, unsupported
field names, or YAML row flattened into prose.

**Expected overlap ratio:** 0.10-0.25, depending on pair kind.

**DTO requirements:** `source_id`, `contract_signature`, `transition_type`, `phase`,
`sub_phase`, `task_type`, `node_role`, `coverage_status`, `pair_kind`, `A.run_id`,
`A.scenario_id`, `A.user_request`, `B.DocumentId`, `B.SourcePath`,
`B.CompiledText`, `B.CompiledPredicateIds`, and `B.Confidence`.

---

### Training Type M06 - Held-Out Corruption Test Cases

**Registry status:** `full_type`

**What it is:** Held-out eval YAML cases with explicit corrupted response, wrong B,
and correct B.

**Examples in the XIIGen corpus:**

```text
held-out/XIIGEN-LPS-001-corruption.yaml
```

**Authority boundary:** Type 8/evaluation holdout extension. It is not training data.

**A-side use:** Yes only for the held-out prompt if separated from evaluation answer.

**B-side chosen use:** Yes for correct B and corruption diagnosis.

**Rejected source:** Wrong B used as chosen, holdout leaked into training set, or
corruption case mixed with normal DPO pair.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `holdout_id`, `pair_kind=corruption_case`, `corruption`,
`wrong_B`, `correct_B`, `eval_only=true`, and `leakage_guard`.

---

### Training Type M07 - Held-Out Rephrase Test Cases

**Registry status:** `full_type`

**What it is:** Held-out rephrase robustness cases paired with a canonical prompt.

**Examples in the XIIGen corpus:**

```text
held-out/XIIGEN-LPS-001-rephrase.yaml
```

**Authority boundary:** Evaluation robustness artifact. It validates prompt
invariance and must not be mixed into training examples.

**A-side use:** Yes only as eval prompt.

**B-side chosen use:** Yes for expected equivalent answer or invariant verdict.

**Rejected source:** Rephrase treated as a new semantic task, or eval rephrase used
as training augmentation without holdout flag.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `holdout_id`, `canonical_prompt_id`, `rephrase_prompt`,
`expected_invariant`, `correct_B`, `eval_only=true`, and `leakage_guard`.

---

### Training Type M08 - Per-Unit Teaching Ledgers

**Registry status:** `full_type`

**What it is:** JSONL audit trail for each taught unit, with training metrics,
verification, baseline verdict, trust state, and outcome.

**Examples in the XIIGen corpus:**

```text
teaching-ledger.jsonl
```

**Authority boundary:** Type 4/22 bridge. It records teaching and verification
events, not authored source content.

**A-side use:** No.

**B-side chosen use:** Yes for teaching audit questions, verification correctness,
baseline/final verdict comparison, trust state, and CPU/checkpoint bytes.

**Rejected source:** Ledger used as source prompt, baseline verdict treated as final,
or training metrics omitted from row evidence.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `pair_id`, `component_role`, `sub_phase_id`,
`transition_type`, `row_index`, `training.elapsed_ms`, `training.cpu_percent`,
`training.checkpoint_bytes`, `verification.expected_verdict`,
`verification.actual_verdict`, `verification.verdict_correct`,
`verification.baseline_verdict`, `decision.trust_state`, and `decision.outcome`.

---

### Training Type M09 - Teaching Cycle Summary JSON

**Registry status:** `full_type`

**What it is:** Full summary JSON for one teaching cycle, from raw source/prompt to
compiler/router/baseline/final verdict.

**Examples in the XIIGen corpus:**

```text
cycle-summary.json
```

**Authority boundary:** Cycle-level Type 4/22/32 bridge. It is the authoritative
summary of one teaching cycle result.

**A-side use:** No.

**B-side chosen use:** Yes for cycle-level verdicts, prompt scope, topology/subphase
counts, compiler output, router output, baseline/final comparison, and promotion.

**Rejected source:** Cycle promoted claim without `promoted=true`, baseline verdict
used as final, or summary flattened without scope fields.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `cycle_id`, `source_document`, `source_excerpt_line`, `prompt`,
`full_scope.topology_phase_count`, `full_scope.sub_phase_count`, `compiler_output`,
`router_output`, `baseline.verdict`, `final.verdict`, and `promoted`.

---

### Training Type M10 - Flow Package Manifests

**Registry status:** `full_type`

**What it is:** Package-level manifest describing what a flow package includes,
excludes, and how Claude/Codex should use it.

**Examples in the XIIGen corpus:**

```text
MANIFEST.md
FLOW-0-COMPLETE-PACKAGE.zip/MANIFEST.md
```

**Authority boundary:** Archive/package manifest. It authorizes member-level use but
does not replace member classification.

**A-side use:** No.

**B-side chosen use:** Yes for package inventory, excluded/stale member warnings,
usage instructions, and package purpose.

**Rejected source:** Stale member used despite manifest warning, package title used
without manifest, or manifest treated as the content of all members.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `package_id`, `manifest_path`, `included_members`,
`excluded_members`, `do_not_use_members`, `usage_instructions`, and
`member_classification_required=true`.

---

### Training Type M11 - Flow Deep Research Documents

**Registry status:** `full_type`

**What it is:** Per-flow deep research input documents used before design simulation.

**Examples in the XIIGen corpus:**

```text
15 - build mvps deep research engine.md
```

**Authority boundary:** Type 2 research-framed architecture extension. It is internal
flow research input, not external-source research authority by itself.

**A-side use:** No unless a direct research question is present.

**B-side chosen use:** Yes for executive recommendations, requirement interpretation,
API contracts, and technology comparisons feeding design simulation.

**Rejected source:** External market fact used without source, deep research used as
implementation proof, or recommendation detached from flow requirements.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `research_doc_id`, `recommendation_ids`,
`requirement_ids`, `api_contract_ids`, `technology_comparison_ids`, and
`feeds_design_simulation=true`.

---

### Training Type M12 - Canonical Merged Reference Documents

**Registry status:** `full_type`

**What it is:** Canonical merged reference packages containing large merged engine,
task catalog, execution, and source-index documents.

**Examples in the XIIGen corpus:**

```text
FLOW35_P5_CANONICAL_DOCS.zip
ENGINE_ARCHITECTURE_MERGED.md
TASK_TYPES_CATALOG_MERGED.md
```

**Authority boundary:** I04/K06 merged canonical family extension. The merged file is
canonical at its creation time and stale when superseded by a newer merged package.

**A-side use:** No.

**B-side chosen use:** Yes for canonical merged reference content, source continuity,
and supersession.

**Rejected source:** Partial source used after canonical merged package, stale merged
package used as current, or merged package used without member-level file identity.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `canonical_package_id`, `merged_member_path`,
`merged_document_kind`, `source_versions`, `created_at_or_version`,
`superseded_by`, and `continuity_check_passed`.

---

### Training Type M13 - Flow Examination Result Pairs

**Registry status:** `full_type`

**What it is:** Per-flow examination result documents with paired human-readable MD
and machine-readable JSON representations.

**Examples in the XIIGen corpus:**

```text
FLOW-07-EXAMINATION-RESULT.md
FLOW-07-EXAMINATION-RESULT.json
```

**Authority boundary:** Type 8 artifact examination extension. It evaluates
hypotheses against a specific flow and records B-type coverage, historic firsts,
service categories, and line evidence.

**A-side use:** Direct examination query only.

**B-side chosen use:** Yes for hypothesis verdicts, B-type coverage maps, running
tally cells, Category A/C findings, and code line evidence.

**Rejected source:** Verdict without evidence, stale MD/JSON mismatch, or hypothesis
answer copied from another flow.

**Expected overlap ratio:** 0.05-0.15.

**DTO requirements:** `flow_id`, `examination_result_id`, `md_path`, `json_path`,
`hypothesis_id`, `verdict`, `b_type_coverage`, `running_tally_ref`,
`service_category_evidence`, and `line_evidence`.

---

### Training Type M14 - Flow Examination Progress State

**Registry status:** `full_type`

**What it is:** Fleet-level JSON state tracking which flows have been examined and
the running hypothesis tallies.

**Examples in the XIIGen corpus:**

```text
FLOW-EXAMINATION-PROGRESS.json
```

**Authority boundary:** Type 22 execution-state extension for examination progress.

**A-side use:** No.

**B-side chosen use:** Yes for completed flow list, next flow, blocked flows,
pre-examination skill load status, and RUNNING_TALLY counts.

**Rejected source:** Earlier progress file used after later examination, manual count
used instead of RUNNING_TALLY, or progress state treated as verdict evidence.

**Expected overlap ratio:** 0.05-0.15.

**DTO requirements:** `progress_file_path`, `completed_flows`, `next_to_examine`,
`current_position`, `blocked_flows`, `skill_load_status`, `running_tally`, and
`state_timestamp_or_sequence`.

---

### Training Type M15 - Multi-Pass Flow Arbiter Analysis

**Registry status:** `full_type`

**What it is:** Multi-flow, multi-pass arbiter analysis documents with MD and JSON
forms.

**Examples in the XIIGen corpus:**

```text
FLOW-01-13-ARBITER-ANALYSIS.md
FLOW-01-13-ARBITER-ANALYSIS.json
FLOW-01-13-SECOND-PASS-ARBITERS.zip
```

**Authority boundary:** Type 8 + Type 23 compound. The same arbiter set may be
re-run in second/fourth/fifth/sixth pass variants after plan changes.

**A-side use:** Direct arbiter question only.

**B-side chosen use:** Yes for A1-A4 findings, era boundary, priority, fix type,
flow range, pass number, and concrete dependency defects.

**Rejected source:** Earlier pass used as current, finding without pass id, priority
changed without evidence, or multi-flow analysis collapsed into one flow.

**Expected overlap ratio:** 0.10-0.20.

**DTO requirements:** `flow_range`, `pass_number`, `arbiter_id`, `finding_id`,
`priority`, `fix_type`, `era_boundary`, `affected_flow_ids`, `md_path`, `json_path`,
and `supersedes_pass`.

---

### Training Type M16 - Retroactive Hypothesis Analysis Packages

**Registry status:** `full_type`

**What it is:** Hypothesis-specific retroactive analysis packages applied to flows
that were examined before the hypothesis existed.

**Examples in the XIIGen corpus:**

```text
H10-CLIENT-SERVER-ANALYSIS.zip
H11-H12-H13-FLOW-01-06.zip
H21-NDI-STAR-H22-*.zip
```

**Authority boundary:** Type 33 comparison/conflict analysis extension for
retroactive hypothesis application.

**A-side use:** Direct hypothesis question only.

**B-side chosen use:** Yes for H-range, flow range, retroactive verdict, structural
evidence, and root cause.

**Rejected source:** New hypothesis omitted for older flows, verdict copied from
non-retroactive pass, or missing root cause.

**Expected overlap ratio:** 0.10-0.20.

**DTO requirements:** `hypothesis_range`, `flow_range`, `retroactive=true`,
`analysis_package`, `verdict_by_flow`, `root_cause`, and `evidence_locator`.

---

### Training Type M17 - Flow Live Run Evidence Pairs

**Registry status:** `full_type`

**What it is:** Companion live-run JSON artifacts for a flow: run results and
execution log.

**Examples in the XIIGen corpus:**

```text
FLOW-47-LIVE-RUN-RESULTS.json
EXECUTION-LOG.json
```

**Authority boundary:** Type 4/22/32 bridge. It is plan-vs-actual live-run evidence,
not ML experiment telemetry.

**A-side use:** No.

**B-side chosen use:** Yes for overall verdict, plan-vs-actual counts, source plans,
phase status, turns, gate result, and caveat.

**Rejected source:** GOAL_REACHED claimed when result says partial, execution log
used without run result, or index counts ignored.

**Expected overlap ratio:** 0.05-0.10.

**DTO requirements:** `flow_id`, `live_run_results_path`, `execution_log_path`,
`overall_verdict`, `index_counts`, `source_plans`, `captured_from`,
`phase_statuses`, `gate_result`, and `caveat`.

---

### Training Type M18 - Flow Goal Gap Reports

**Registry status:** `full_type`

**What it is:** Goal-element verdict reports comparing a flow's master-plan goal to
actual run evidence.

**Examples in the XIIGen corpus:**

```text
FLOW-47-GOAL-GAP-REPORT.md
```

**Authority boundary:** Type 32 validation report extension centered on goal
elements rather than generic gates.

**A-side use:** Direct goal-achievement query only.

**B-side chosen use:** Yes for master goal statement, GE table, goal-not-reached
items, plan-vs-actual comparisons, and partial findings.

**Rejected source:** Overall success claim while GE rows are partial, weakened
assertion treated as evidence, or goal report used without live-run companion.

**Expected overlap ratio:** 0.10-0.20.

**DTO requirements:** `flow_id`, `goal_gap_report_path`, `master_goal_statement`,
`goal_element_id`, `goal_element_verdict`, `evidence_locator`,
`plan_vs_actual_table`, and `live_run_companion`.

---

### Training Type M19 - GUIDE-B07 Through GUIDE-B12 Document Production Guides

**Registry status:** `full_type`

**What it is:** GUIDE-B documents for producing UI/RAG/gallery/reflection/automation
and design-simulation artifacts.

**Examples in the XIIGen corpus:**

```text
GUIDE-B07-RAG-MD.md
GUIDE-B08-GALLERY-HTML.md
GUIDE-B09-UI-REFLECTION-STATE-JSON.md
GUIDE-B10-UI-REFLECTION-STATE-MD.md
GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md
GUIDE-B12-DESIGN-SIMULATION-R1-MD.md
```

**Authority boundary:** Type 29 document-production guide extension. Each guide is
canonical only for its target B document type.

**A-side use:** No.

**B-side chosen use:** Yes for required output structure, required fields, production
sequence, and validation checks for the target document.

**Rejected source:** Wrong GUIDE-B used for a target document, missing required
section, or UI/RAG/gallery guide generalized to another artifact.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `guide_id`, `target_document_type`, `required_sections`,
`required_fields`, `production_steps`, `validation_checks`, and `canonical_for`.

---

### Training Type M20 - Versioned Combined Package Documents

**Registry status:** `full_type`

**What it is:** Session-version package outputs that combine plan, state, and mini
RAG documents into a merged session package.

**Examples in the XIIGen corpus:**

```text
V55_COMBINED_PACKAGE.zip
V55_COMBINED_PLAN.md
V55_COMBINED_STATE.json
V55_MINI_RAG.md
V56_3D_GRAPH_PACKAGE.zip
```

**Authority boundary:** Session-scoped merge output. Distinct from permanent
canonical merged references.

**A-side use:** No.

**B-side chosen use:** Yes for session-version plan/state/RAG synthesis, checksum or
summary header, and package-level completeness.

**Rejected source:** Partial source file presented as combined package, combined
state used without plan, or old session-version package used as current.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `session_version`, `combined_package_id`,
`combined_plan_path`, `combined_state_path`, `mini_rag_path`, `source_parts`,
`checksum_or_summary_header`, and `supersedes_package`.

---

### Training Type M21 - Translated Plain-English Session Packages

**Registry status:** `limited_authority`

**What it is:** Plain-language translated versions of technical session/skill files
for orchestration-advisor training.

**Examples in the XIIGen corpus:**

```text
ext_skills_flat_translated.zip
translated orchestrator learning sets.zip
plain-english-session-files-v2.0.zip
session_files_translated.zip
```

**Authority boundary:** Type 20/Type 15 vocabulary transform. It is an extracted copy
of original technical content in a plain-language register, not new authority.

**A-side use:** Yes for plain-language query vocabulary when provenance is retained.

**B-side chosen use:** Limited to content-domain orchestration advice when linked to
the original technical file.

**Rejected source:** Translation treated as canonical over original, provenance lost,
or plain-English version used for implementation specifics.

**Expected overlap ratio:** 0.30-0.60 versus original technical source.

**DTO requirements:** `translated_package_id`, `original_source_path`,
`translated_path`, `translation_register=plain_english`, `extracted_copy=true`,
`provenance_link`, and `allowed_use=vocabulary_or_advice_only`.

---

## Final Tool-Result Reference Families

The `N01-N09` families come from full inspection of tool_result content and
create_file/view paths. These are high-frequency reference/protocol documents that
were not visible from message text alone.

---

### Tool-Result Type N01 - Flow Registry Reference Documents

**Registry status:** `full_type`

**What it is:** Reference documents listing existing flows, flow numbers, families,
dependencies, and current availability.

**Examples in the XIIGen corpus:**

```text
FLOW-REGISTRY.md
Flow Registry - Existing Flows Reference
```

**Authority boundary:** Flow-level registry. Distinct from skill registries and
decision logs.

**A-side use:** No.

**B-side chosen use:** Yes for existing-flow lookup, flow-number identity, flow
family, dependency graph, and knowledge-edge decisions.

**Rejected source:** Claiming a flow exists without registry entry, wrong flow number,
or routing to obsolete flow.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `flow_registry_path`, `flow_id`, `flow_number`, `flow_family`,
`dependency_edges`, `registry_version`, and `exists_in_registry`.

---

### Tool-Result Type N02 - Engine Contract Templates

**Registry status:** `full_type`

**What it is:** Mandatory task-type engine contract templates.

**Examples in the XIIGen corpus:**

```text
ENGINE-CONTRACT-TEMPLATE.md
```

**Authority boundary:** Type 25/template contract for task types. It defines the
fields every task type contract must include.

**A-side use:** No.

**B-side chosen use:** Yes for FACTORY DEPENDENCIES, FREEDOM/MACHINE classification,
ARBITERS, STATE WRITES, and required task-type contract sections.

**Rejected source:** Task type contract missing required sections, runtime code
treated as the template, or FREEDOM/MACHINE omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `template_id`, `task_type_id`, `factory_dependencies`,
`freedom_machine_classification`, `arbiter_ids`, `state_writes`, and
`required_sections_present`.

---

### Tool-Result Type N03 - Canonical Merge Protocols

**Registry status:** `full_type`

**What it is:** Procedural protocols for adding flows and updates into canonical
documents.

**Examples in the XIIGen corpus:**

```text
MERGE-PROTOCOL.md
Merge Protocol - Adding Flows to Canonical Documents
```

**Authority boundary:** Merge procedure. It governs how merged documents are created;
it is not the merged document itself.

**A-side use:** No.

**B-side chosen use:** Yes for merge ordering, continuous numbering, required fields,
canonical target documents, and conflict handling.

**Rejected source:** Merged output produced without protocol, numbering gap, wrong
canonical target, or treating merge protocol as source content.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `merge_protocol_version`, `canonical_target_doc`,
`numbering_rule`, `required_merge_fields`, `source_insert_order`, and
`conflict_resolution_rule`.

---

### Tool-Result Type N04 - Wave Assignment Documents

**Registry status:** `full_type`

**What it is:** Documents assigning flows/tasks to deployment or planning waves.

**Examples in the XIIGen corpus:**

```text
WAVE-ASSIGNMENT.md
```

**Authority boundary:** Assignment authority, not a phase implementation plan.

**A-side use:** No.

**B-side chosen use:** Yes for wave id, flow membership, ordering constraints, and
deployment grouping.

**Rejected source:** Wrong wave for a flow, phase plan used as wave assignment, or
dependency ordering ignored.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `wave_assignment_path`, `wave_id`, `assigned_flow_ids`,
`ordering_constraints`, `assignment_reason`, and `supersedes_assignment`.

---

### Tool-Result Type N05 - Fabric Interfaces Reference Family

**Registry status:** `full_type`

**What it is:** Reference documents defining fabric-level interfaces and additions.

**Examples in the XIIGen corpus:**

```text
FABRIC-INTERFACES.md
IFabricResolver
IDatabaseFabric
IScheduler
ICodeRepo
```

**Authority boundary:** Type 26/Type 2 bridge. These are interface-contract authority
documents, not generic engine prose.

**A-side use:** No.

**B-side chosen use:** Yes for interface names, method boundaries, fabric
responsibility, and dependency rules.

**Rejected source:** Missing interface addition, interface invented from prose, or
using old fabric interface after additions.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `fabric_reference_path`, `interface_name`, `method_or_contract`,
`responsibility_boundary`, `dependency_rule`, and `version_or_addition_id`.

---

### Tool-Result Type N06 - Skill Overhaul Session Documents

**Registry status:** `full_type`

**What it is:** Session documents for bulk skill remediation/overhaul work.

**Examples in the XIIGen corpus:**

```text
SKILL-OVERHAUL-SESSION
SKILLS-MASTER-REMEDIATION
CARRY-FORWARD-ISSUES.md
```

**Authority boundary:** Session type + remediation bundle. Distinct from individual
skill files and generic planning sessions.

**A-side use:** Only the actual remediation request if preserved.

**B-side chosen use:** Yes for skill batch scope, remediation plan, carry-forward
issues, and before/after skill boundaries.

**Rejected source:** Bulk overhaul treated as one skill change, carry-forward issue
omitted, or stale remediation package used as current.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `overhaul_session_id`, `affected_skill_ids`,
`remediation_plan_path`, `carry_forward_issues_path`, `batch_scope`, and
`post_overhaul_status`.

---

### Tool-Result Type N07 - Known Violations Registries

**Registry status:** `full_type`

**What it is:** Registries of known codebase/architecture violations and whether
they can be auto-healed or require manual correction.

**Examples in the XIIGen corpus:**

```text
KNOWN-VIOLATIONS-REGISTRY.md
```

**Authority boundary:** Codebase violation registry. Distinct from behavioral
corrections registry, which tracks agent behavior.

**A-side use:** No.

**B-side chosen use:** Yes for violation id, class, auto-heal/manual-only status,
affected scope, and correction rule.

**Rejected source:** Manual-only violation auto-healed, known violation ignored, or
behavioral correction used as codebase violation.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `violation_id`, `violation_class`, `auto_heal_allowed`,
`manual_only`, `affected_scope`, `correction_rule`, and `registry_version`.

---

### Tool-Result Type N08 - Foundational Principles Gate References

**Registry status:** `full_type`

**What it is:** Plan gate reference documents distinguishing foundational principles
from conventions.

**Examples in the XIIGen corpus:**

```text
FOUNDATIONAL-PRINCIPLES.md
Foundational Principles - Plan Gate Reference
```

**Authority boundary:** Type 34 gate reference. It is a gate standard, not a review
verdict.

**A-side use:** No.

**B-side chosen use:** Yes for foundational principle id, convention boundary,
required plan gate, and review failure condition.

**Rejected source:** Convention treated as foundational principle, gate skipped, or
principle applied without locator.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `principle_id`, `principle_text`, `convention_boundary`,
`gate_id`, `failure_condition`, and `plan_review_locator`.

---

### Tool-Result Type N09 - Client Push Infrastructure And Module Map References

**Registry status:** `full_type`

**What it is:** Per-flow infrastructure reference docs and prose module dependency
maps.

**Examples in the XIIGen corpus:**

```text
FLOW-40-CLIENT-PUSH-INFRASTRUCTURE.md
CLIENT-PUSH-INFRA
MODULE-MAP-REFERENCE.md
```

**Authority boundary:** Type 2/Type 27 bridge. FLOW-40 is an infrastructure flow
spec for real-time client push patterns; module maps are dependency graph documents
in prose/table form.

**A-side use:** No.

**B-side chosen use:** Yes for real-time push architecture, SignalR/client-push
constraints, module dependency edges, and shared infrastructure relationships.

**Rejected source:** General flow spec used for client-push infrastructure, prose
module map ignored as graph evidence, or dependency edge fabricated.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `reference_doc_id`, `flow_id_or_module_id`,
`infrastructure_pattern`, `module_dependency_edges`, `shared_infrastructure_ids`,
`graph_extracted=true`, and `edge_locator`.

---

## Live Docs-Folder Examination And UX Families

The `O01-O19` families come from `C:\Projects\xiigen mvp\docs` examination. This
folder is live working documentation, not an old archive. Many artifacts are paired
machine/human evidence chains.

---

### Docs Type O01 - Fleet Raw Input JSONL

**Registry status:** `full_type`

**What it is:** Fleet-level JSONL inventory, one row per flow, combining session
files, service/test files, QA coverage, topology, automation, and Elasticsearch
evidence.

**Examples in the XIIGen corpus:**

```text
47-FLOW-RAW-INPUTS.jsonl
```

**Authority boundary:** Raw state input for mapping and examination. It is not a
summary and not a plan.

**A-side use:** No.

**B-side chosen use:** Yes for exact per-flow inventory fields such as topology
node/edge counts, package counts, missing CFs, and arbiter config ids.

**Rejected source:** Wrong JSONL row, fabricated number, or using rollup summary
instead of raw row.

**Expected overlap ratio:** 0.05-0.15.

**DTO requirements:** `jsonl_path`, `flow_id`, `flow_slug`, `d1_files`, `d2_services`,
`d2_tests`, `d3_qa_state`, `track_a_topology`, `track_b_e2e`, `track_c_elasticsearch`,
and `jsonl_line_number`.

---

### Docs Type O02 - Code-Derived Business Process Master State

**Registry status:** `full_type`

**What it is:** Code-derived fleet state for business processes, endpoints, events,
indices, and orphan services.

**Examples in the XIIGen corpus:**

```text
BUSINESS-PROCESS-MASTER-STATE.json
```

**Authority boundary:** Type 22 execution-state extension with `derivation=code-only`.
It should not be mixed with session-declared state.

**A-side use:** No.

**B-side chosen use:** Yes for total process counts, code-backed process counts,
endpoint/event/index counts, orphan services, and skepticism notes.

**Rejected source:** Session-declared count used as code-derived state, methodology
omitted, or orphan service ignored.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `state_path`, `derivation=code-only`, `total_processes`,
`processes_with_code`, `endpoint_counts`, `event_counts`, `index_counts`,
`orphan_services`, and `methodology_notes`.

---

### Docs Type O03 - Flow Gaps Master Plans

**Registry status:** `full_type`

**What it is:** Flow-scoped gap-to-fix master plans with pipeline status, root causes,
fix types, and task-type inventory.

**Examples in the XIIGen corpus:**

```text
FLOW-07-GAPS-MASTER-PLAN.md
```

**Authority boundary:** Type 23 gap remediation extension. It audits one flow's full
gap pipeline and fix state.

**A-side use:** No unless the source contains a real remediation request.

**B-side chosen use:** Yes for root cause ids, gap ids, fix type, status, fixed-in
file reference, task-type inventory, and pipeline status.

**Rejected source:** Issue tracker used as complete gap plan, fixed status claimed
without file reference, or root cause assigned to wrong gap.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `gaps_master_plan_path`, `pipeline_status`,
`root_cause_id`, `gap_id`, `fix_type`, `fix_status`, `fixed_in_file`, and
`task_type_inventory`.

---

### Docs Type O04 - Flow Step Teaching Series

**Registry status:** `full_type`

**What it is:** Per-flow STEP-1 through STEP-10 teaching/session documents for the
ten-step flow preparation pipeline.

**Examples in the XIIGen corpus:**

```text
FLOW-07-STEP-1-INVARIANTS.md
FLOW-07-STEP-2-CYCLE1-CONTEXT.md
FLOW-07-STEP-10-CHAIN-REVIEW.md
```

**Authority boundary:** Type 10 teaching-plan extension at step granularity.

**A-side use:** Embedded task prompt only when explicitly marked.

**B-side chosen use:** Yes for step purpose, cycle context, test gates, templates,
handoff conditions, visibility, and chain review.

**Rejected source:** Step order skipped, wrong cycle context, or STEP document used
as execution result.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `flow_id`, `step_number`, `step_name`, `cycle_id`,
`teaching_phase`, `required_inputs`, `expected_outputs`, `gate_ids`, and
`next_step_dependency`.

---

### Docs Type O05 - Flow Reconciliation State

**Registry status:** `full_type`

**What it is:** Plan-vs-code reconciliation state documents in MD and JSON form.

**Examples in the XIIGen corpus:**

```text
FLOW-07-RECONCILIATION-STATE.md
FLOW-07-RECONCILIATION-STATE.json
```

**Authority boundary:** Type 7/22 bridge. It reconciles planned state with what
exists in the codebase.

**A-side use:** No.

**B-side chosen use:** Yes for planned-vs-actual state, reconciled reality, missing
files, and correction status.

**Rejected source:** Old plan used as reality, code state ignored, or MD/JSON pair
mismatch not recorded.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `md_path`, `json_path`, `planned_state`,
`codebase_actual_state`, `reconciled_state`, `missing_artifacts`, and
`reconciliation_timestamp_or_sequence`.

---

### Docs Type O06 - Flow Mobility Auth State

**Registry status:** `full_type`

**What it is:** Per-flow auth, portability, and mobility state JSON.

**Examples in the XIIGen corpus:**

```text
FLOW-07-MOBILITY-AUTH-STATE.json
```

**Authority boundary:** Type 22 state extension for auth/portability readiness.

**A-side use:** No.

**B-side chosen use:** Yes for portability status, auth constraints, tenant
certification tier, and mobility blockers.

**Rejected source:** Portability claimed without auth state, tenant tier fabricated,
or mobility state used as UX evidence.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `flow_id`, `mobility_auth_state_path`, `portability_status`,
`auth_constraints`, `tenant_certification_tier`, `mobility_blockers`, and
`state_version`.

---

### Docs Type O07 - Adaptation Surface Specifications

**Registry status:** `full_type`

**What it is:** Per-flow FREEDOM key adaptation surface JSON documents.

**Examples in the XIIGen corpus:**

```text
adaptation-surface-event-attendance.json
```

**Authority boundary:** FREEDOM/MACHINE adaptation surface authority. It defines
which keys a tenant may adapt and how.

**A-side use:** Direct adaptation question only.

**B-side chosen use:** Yes for freedom keys, current/adapted values, semantics, IR
citations, profile/runtime readers, and observability.

**Rejected source:** MACHINE key presented as adaptable, missing IR citation, or
tenant profile value used as surface definition.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `flow_slug`, `adapter_tenant`, `adaptation_type`,
`freedom_key`, `current_value`, `adapted_value`, `semantics`, `ir_citation`,
`profile_reader`, `runtime_reader`, and `observable_in`.

---

### Docs Type O08 - Tenant Adaptation Profiles

**Registry status:** `full_type`

**What it is:** Per-tenant override profiles for FREEDOM key values.

**Examples in the XIIGen corpus:**

```text
tenant-profile-tenant-a.json
tenant-profile-tenant-b.json
```

**Authority boundary:** Runtime adaptation instance. It instantiates O07; it does not
define the adaptation surface.

**A-side use:** No.

**B-side chosen use:** Yes for tenant-specific adapted values and profile overrides
when linked to the surface spec.

**Rejected source:** Tenant override used as allowed surface, tenant values applied
to wrong tenant, or profile used without O07.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `tenant_id`, `profile_path`, `flow_slug`, `adapted_values`,
`freedom_key`, `surface_spec_path`, and `profile_version`.

---

### Docs Type O09 - FC-18 Audit Trails

**Registry status:** `full_type`

**What it is:** Portability check audit trail documents for a specific FC compliance
case and tenant variants.

**Examples in the XIIGen corpus:**

```text
FC-18-AUDIT-TRAIL.md
FC-18-AUDIT-TRAIL-tenant-a.md
FC-18-AUDIT-TRAIL-tenant-b.md
```

**Authority boundary:** Type 32 portability validation extension.

**A-side use:** No.

**B-side chosen use:** Yes for FC compliance evidence, tenant-specific proof,
audit trail entries, and portability verdict.

**Rejected source:** Compliance claimed without audit trail, wrong tenant audit used,
or FC-18 result generalized to another FC.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `fc_id`, `audit_trail_path`, `tenant_id`, `evidence_entries`,
`portability_verdict`, `checked_at_or_sequence`, and `scope_limit`.

---

### Docs Type O10 - Step-8 Handoff Contracts

**Registry status:** `full_type`

**What it is:** Structured planning-to-code-generation handoff contracts with iron
rules and I/O contracts.

**Examples in the XIIGen corpus:**

```text
FLOW-07-STEP-8-HANDOFF-CONTRACT.md
```

**Authority boundary:** Type 10/25 bridge and a high-priority B-side source for code
generation tasks.

**A-side use:** Only if the file contains an explicit user-facing task prompt.

**B-side chosen use:** Yes for iron rules, input contract required fields, output
contract structure, and handoff constraints.

**Rejected source:** Code generation task missing required handoff fields, iron rule
omitted, or STEP-8 applied to wrong flow.

**Expected overlap ratio:** 0.05-0.10.

**DTO requirements:** `flow_id`, `handoff_contract_path`, `iron_rule_ids`,
`input_contract_required_fields`, `output_contract_fields`, `handoff_boundary`, and
`code_generation_target`.

---

### Docs Type O11 - Flow Inventory Tables

**Registry status:** `full_type`

**What it is:** Fleet-level flow inventory with classification, topology, UI state,
and session stage.

**Examples in the XIIGen corpus:**

```text
flow-coverage/FLOW-INVENTORY.md
```

**Authority boundary:** Flow-level inventory, not implementation proof.

**A-side use:** No.

**B-side chosen use:** Yes for flow classification, topology status, UI existence
state, and session stage.

**Rejected source:** Inventory row used as code evidence, wrong classification, or
UI state inferred from screenshots instead of inventory when inventory is requested.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `inventory_path`, `flow_id`, `flow_classification`,
`topology_status`, `ui_exists_state`, `session_stage`, and `row_locator`.

---

### Docs Type O12 - Fleet Coverage Phase Analyses

**Registry status:** `full_type`

**What it is:** Per-phase fleet coverage analysis documents and rollups.

**Examples in the XIIGen corpus:**

```text
flow-coverage/P1-business-logic-inventory.md
flow-coverage/P13-qa-run.md
flow-coverage/*ROLLUP*.md
```

**Authority boundary:** Measurement/coverage analysis extension. Each phase has a
specific coverage question and should not be generalized.

**A-side use:** No.

**B-side chosen use:** Yes for phase-specific coverage findings, rollup counts, gap
status, and readiness findings.

**Rejected source:** P1 finding used as P13 QA result, rollup count without phase,
or coverage analysis treated as implementation completion.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `coverage_phase_id`, `coverage_doc_path`, `rollup_path`,
`coverage_axis`, `flow_id`, `finding_id`, `count_or_status`, and `evidence_locator`.

---

### Docs Type O13 - Multi-Tenant UI Screenshot Evidence

**Registry status:** `full_type`

**What it is:** Versioned PNG screenshots organized by flow, tenant variant, and
role.

**Examples in the XIIGen corpus:**

```text
e2e-snapshots/{slug}/{tenant-variant}/flow-NN-{slug}-{role}.png
```

**Authority boundary:** A10/A20 visual evidence subtype. PNG alone is visual evidence;
text DPO pairs require linked UX review or visual QA finding.

**A-side use:** No.

**B-side chosen use:** Yes as visual evidence when referenced by UX review,
before/after visual QA, or role/tenant screenshot task.

**Rejected source:** Screenshot filename used without image/UX finding, wrong tenant
variant, wrong role, or screenshot used as code proof.

**Expected overlap ratio:** n/a for image; paired text usually 0.10-0.30.

**DTO requirements:** `screenshot_path`, `flow_id`, `flow_slug`, `tenant_variant`,
`role`, `ui_version`, `linked_ux_review`, and `visual_evidence_id`.

---

### Docs Type O14 - Per-Flow UX Review Documents

**Registry status:** `full_type`

**What it is:** Per-flow UX reviews with per-PNG severity findings and suggested
fixes.

**Examples in the XIIGen corpus:**

```text
ux-review/FLOW-07/UX-REVIEW.md
```

**Authority boundary:** Type 32 visual/UX validation extension. It turns screenshots
into text findings.

**A-side use:** No.

**B-side chosen use:** Yes for PNG count, blocker/high/medium/low counts, overall
verdict, per-PNG finding, axis, severity, and suggested fix.

**Rejected source:** Suggested fix separated from finding, severity changed without
evidence, or UX review used without referenced screenshot.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `ux_review_path`, `png_file`, `severity`, `axis`,
`finding`, `suggested_fix`, `overall_verdict`, and `linked_screenshot_path`.

---

### Docs Type O15 - UX Review Rollups

**Registry status:** `full_type`

**What it is:** Fleet-level aggregation of UX review findings.

**Examples in the XIIGen corpus:**

```text
UX-REVIEW-ROLLUP.md
```

**Authority boundary:** Rollup/aggregation document. It summarizes O14; it does not
replace per-PNG evidence.

**A-side use:** No.

**B-side chosen use:** Yes for fleet-level counts, recurring patterns, and global UX
priority.

**Rejected source:** Rollup used for per-PNG detail, count without source reviews, or
old rollup used after newer review batch.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `rollup_path`, `included_flow_ids`, `aggregate_counts`,
`pattern_ids`, `priority_order`, `source_review_paths`, and `rollup_version`.

---

### Docs Type O16 - Fleet Validation Documents

**Registry status:** `full_type`

**What it is:** Multi-version fleet validation documents across UI, accessibility,
role coverage, and design review status.

**Examples in the XIIGen corpus:**

```text
FLEET-VALIDATION-v1.md
FLEET-VALIDATION-v5.md
```

**Authority boundary:** Type 32/B99-style multi-pass validation for visual/UI domain.

**A-side use:** No.

**B-side chosen use:** Yes for versioned fleet validation status, per-flow validation
cells, accessibility/role coverage, and pass/fail changes across versions.

**Rejected source:** v1 verdict used after v5, UI validation used as code validation,
or version omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `fleet_validation_version`, `validation_path`, `flow_id`,
`ui_status`, `accessibility_status`, `role_coverage_status`, `verdict`, and
`supersedes_version`.

---

### Docs Type O17 - Session Round ZIP Snapshots

**Registry status:** `preprocess_then_reclassify`

**What it is:** Incremental session round ZIPs, often placeholders, sometimes
containing actual session round artifacts.

**Examples in the XIIGen corpus:**

```text
sessions/FLOW-03-R1.zip
sessions/FLOW-03-R19.zip
```

**Authority boundary:** Archive container. Empty placeholder ZIPs are excluded; real
members must be extracted and classified.

**A-side use:** No.

**B-side chosen use:** Only after member-level extraction and classification.

**Rejected source:** ZIP name used as evidence, empty placeholder treated as artifact,
or round inferred without manifest/member.

**Expected overlap ratio:** inherited from extracted member.

**DTO requirements:** `session_zip_path`, `flow_id`, `round_id`, `is_empty`,
`member_count`, `manifest_path`, `archive_member_path`, and `target_member_family`.

---

### Docs Type O18 - Business Flow Product Specs And Deep Research

**Registry status:** `full_type`

**What it is:** Original product specification documents per flow and their deep
research expansions.

**Examples in the XIIGen corpus:**

```text
business-flows/01-user-registration.md
business-flows/01-user-registration deep search.md
business-flows/06-marketplace-publishing deep research engine.md
```

**Authority boundary:** Product-spec/design input. Deep research variant expands the
flow context but should not override canonical product spec without trace.

**A-side use:** Yes for original product request when authored by the user or source
prompt is preserved.

**B-side chosen use:** Yes for business requirements, domain vocabulary, product
constraints, and design-simulation inputs.

**Rejected source:** Deep research used as implementation proof, product spec for
wrong flow, or external market claim used without citation.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `flow_id`, `business_flow_path`, `deep_research_path`,
`source_role`, `product_requirement_ids`, `domain_terms`, `design_input=true`, and
`canonicality`.

---

### Docs Type O19 - RAG Benchmark Session Sequence

**Registry status:** `full_type`

**What it is:** Ordered RAG benchmark sessions for infrastructure setup, seeding,
baseline benchmark, and ML optimization.

**Examples in the XIIGen corpus:**

```text
rag-benchmark/SESSION-1.md
rag-benchmark/SESSION-2.md
rag-benchmark/SESSION-3.md
rag-benchmark/SESSION-4.md
```

**Authority boundary:** B78-style session plan extension for RAG quality measurement.

**A-side use:** No unless explicit benchmark task prompt appears.

**B-side chosen use:** Yes for benchmark session order, setup/seeding/baseline/ML
optimization phases, expected metrics, and readiness gates.

**Rejected source:** Session order changed, benchmark baseline skipped, or ML
optimization used without seeded baseline.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `benchmark_session_id`, `session_number`, `benchmark_phase`,
`prerequisites`, `expected_metrics`, `readiness_gate`, and `depends_on_session`.

---

## Final Session-Statistics Families

The `P01-P12` families come from the final statistical cross-matrix pass over
session type by document family, including rare sessions and tool_result content.

---

### Session Statistics Type P01 - TypeScript Service Interfaces

**Registry status:** `full_type`

**What it is:** NestJS/TypeScript runtime service interfaces that define fabric,
provider, scheduler, lifecycle, and GraphRAG contracts.

**Examples in the XIIGen corpus:**

```text
IDatabaseService
IQueueService
IExternalServiceFactory
IOrchestratorService
ISchedulerService
IDatabaseProvider
IAiProvider
IFlowLifecycleService
IGraphRagService
```

**Authority boundary:** Type 26 subtype for TypeScript service interfaces. It is not
the same as C# interface authority.

**A-side use:** No.

**B-side chosen use:** Yes for method signatures, service boundary, provider/fabric
selection, and interface-to-implementation correctness.

**Rejected source:** C# interface shape applied to TypeScript runtime, wrong service
interface for context, missing method field, or implementation violating signature.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `language=typescript`, `interface_name`, `source_file_path`,
`method_signature`, `service_boundary`, `provider_or_fabric_kind`, and
`implementation_contract`.

---

### Session Statistics Type P02 - Machine Constant Decision Records

**Registry status:** `full_type`

**What it is:** Named reusable machine/runtime constants with `DESCRIPTIVE-NAME-NNN`
ids.

**Examples in the XIIGen corpus:**

```text
TIMEZONE-FROM-PROFILE-001
SAFETY-GATE-TOKEN-PROTOCOL-001
PG-FIRST-ES-SECOND-001
DESIGN-TOKEN-DEFERRAL-001
SAGA-DURABILITY-FIRST-001
DUAL-ENTRY-COMPENSATION-001
DURABLE-TIMER-IDEMPOTENCY-001
BUILD-TIME-NOT-REQUEST-TIME-001
CONSENT-FAIL-CLOSED-001
```

**Authority boundary:** Named runtime rule constants. They are reusable behavior
rules, not full architecture decision documents.

**A-side use:** No.

**B-side chosen use:** Yes for applying named constants, citing runtime rule ids, and
arbiter checks over constant compliance.

**Rejected source:** Constant applied to wrong domain, id cited without rule text, or
design decision substituted for machine constant.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `constant_id`, `constant_family`, `rule_text`,
`applies_to_domain`, `source_locator`, `runtime_behavior`, and `violation_mode`.

---

### Session Statistics Type P03 - Round Gap Matrices

**Registry status:** `full_type`

**What it is:** Per-round simulation/audit output matrices that check additions
against master plan coverage.

**Examples in the XIIGen corpus:**

```text
R2-GAP-MATRIX.md
R3-GAP-MATRIX.md
```

**Authority boundary:** Round output artifact. Distinct from session simulation input
files.

**A-side use:** No.

**B-side chosen use:** Yes for addition coverage, missing coverage cells, round id,
and gap matrix verdicts.

**Rejected source:** Round input plan used as output matrix, wrong round matrix, or
addition marked covered without matrix row.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `round_id`, `gap_matrix_path`, `addition_id`, `coverage_status`,
`master_plan_reference`, `gap_id`, and `matrix_row_locator`.

---

### Session Statistics Type P04 - Round Root Cause Maps

**Registry status:** `full_type`

**What it is:** Per-round root cause maps that collapse many gaps into root causes
and record dependency ordering/scope classification.

**Examples in the XIIGen corpus:**

```text
R3-ROOT-CAUSE-MAP.md
```

**Authority boundary:** Round output artifact and Type 23 root-cause source.

**A-side use:** No.

**B-side chosen use:** Yes for N-to-K root cause collapse, dependency order, scope
classification, and correction target.

**Rejected source:** Individual gap treated as root cause, wrong dependency order, or
root cause map used for another round.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `round_id`, `root_cause_map_path`, `gap_ids`, `root_cause_id`,
`collapse_rule`, `dependency_order`, `scope_classification`, and `fix_target`.

---

### Session Statistics Type P05 - Historical RAG Batch Conversion Documents

**Registry status:** `full_type`

**What it is:** Batch conversion documents that turn historical session decisions
into RAG fixtures.

**Examples in the XIIGen corpus:**

```text
HIST-ARCH-RAG-BATCH.md
HIST-DR-RAG-BATCH.md
HIST-PERFLOW-RAG-BATCH.md
```

**Authority boundary:** Type 21 internal RAG conversion extension. It bridges raw
history and production RAG patterns.

**A-side use:** No.

**B-side chosen use:** Yes for conversion source, record count, fixture type,
decision family, and output RAG target.

**Rejected source:** Raw history used without conversion, wrong fixture family, or
record count/family fabricated.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `hist_batch_id`, `source_history_kind`, `decision_family`,
`record_count`, `rag_fixture_target`, `conversion_rule`, and `output_path`.

---

### Session Statistics Type P06 - Claude Code Start Prompt Documents

**Registry status:** `full_type`

**What it is:** Copy-paste prompt documents for starting Claude Code execution
sessions.

**Examples in the XIIGen corpus:**

```text
CLAUDE-CODE-QUICK-PROMPT.md
CLAUDE-CODE-START-PROMPT.md
```

**Authority boundary:** K01/L04 subtype for Claude Code execution context.

**A-side use:** Yes when the prompt is the actual input instruction.

**B-side chosen use:** Yes for expected start context, required files, execution
constraints, and start-here package behavior.

**Rejected source:** Claude Code prompt used for non-Claude tool, prompt treated as
completed output, or required context omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `prompt_doc_id`, `target_tool=claude_code`, `prompt_role`,
`required_context_files`, `execution_constraints`, and `start_here=true`.

---

### Session Statistics Type P07 - Gap Closure Plans

**Registry status:** `full_type`

**What it is:** Concrete actionable plans for closing a specific gap cluster.

**Examples in the XIIGen corpus:**

```text
GAP-CLOSURE-PLAN-v1.md
```

**Authority boundary:** Type 23 extension. It is concrete closure planning, not
abstract gap-to-guidance mapping.

**A-side use:** No unless the source includes a real gap-fix request.

**B-side chosen use:** Yes for GAP ids, closure steps, session references, owners,
and completion criteria.

**Rejected source:** Abstract guidance map used as closure plan, closure step without
gap id, or gap closed without evidence.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `gap_closure_plan_path`, `gap_ids`, `closure_steps`,
`session_refs`, `owner_or_role`, `completion_criteria`, and `evidence_required`.

---

### Session Statistics Type P08 - Skill Registration Manifests

**Registry status:** `full_type`

**What it is:** Manifests of currently deployed/registered skill files, versions,
and usage statistics.

**Examples in the XIIGen corpus:**

```text
SKILL-REGISTRATION-MANIFEST.md
SKILL-REGISTRATION-MANIFEST.json
```

**Authority boundary:** Type 30 subtype for active skill registration. Distinct from
factory catalog of all possible skills.

**A-side use:** No.

**B-side chosen use:** Yes for active skill status, registered version, deployment
state, and usage stats.

**Rejected source:** Skill listed in factory catalog treated as active, stale
registration manifest used, or usage stats fabricated.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `manifest_path`, `skill_id`, `registered_version`,
`active_status`, `deployment_scope`, `usage_stats`, and `manifest_version`.

---

### Session Statistics Type P09 - Flow Execution Visibility Plans

**Registry status:** `full_type`

**What it is:** Plans defining how flow execution progress and state should be made
visible during execution.

**Examples in the XIIGen corpus:**

```text
FLOW-EXECUTION-VISIBILITY-PLAN.md
```

**Authority boundary:** Type 29 extension for observability/visibility planning.

**A-side use:** No.

**B-side chosen use:** Yes for visibility events, state exposure, UI/logging hooks,
and execution progress checkpoints.

**Rejected source:** Visibility plan treated as implementation proof, missing progress
checkpoint, or wrong event exposed.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `visibility_plan_path`, `flow_id`, `visible_state_fields`,
`progress_events`, `logging_or_ui_hooks`, `checkpoint_ids`, and `implementation_target`.

---

### Session Statistics Type P10 - Fork Flow Engine Plans

**Registry status:** `full_type`

**What it is:** Plans for fork/join or branch execution behavior in the flow engine.

**Examples in the XIIGen corpus:**

```text
FORK-FLOW-ENGINE-PLAN.md
```

**Authority boundary:** Type 29/18 bridge for fork semantics and portability-style
invariants.

**A-side use:** No.

**B-side chosen use:** Yes for fork semantics, join conditions, branch state, and
dependency order.

**Rejected source:** Linear flow plan used for fork flow, branch state omitted, or
join condition fabricated.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `fork_plan_path`, `fork_node_id`, `branch_ids`,
`join_condition`, `branch_state_fields`, `dependency_order`, and `invariant_ids`.

---

### Session Statistics Type P11 - Infrastructure Audit Documents

**Registry status:** `full_type`

**What it is:** Numbered infrastructure audit documents.

**Examples in the XIIGen corpus:**

```text
INFRA-AUDIT-1.md
INFRA-AUDIT-2.md
INFRA-AUDIT-3.md
```

**Authority boundary:** Type 32 validation/audit extension.

**A-side use:** No.

**B-side chosen use:** Yes for infrastructure finding, severity, affected component,
evidence, and remediation recommendation.

**Rejected source:** Audit finding treated as fixed, wrong audit number, or severity
changed without evidence.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `infra_audit_id`, `audit_path`, `finding_id`, `severity`,
`affected_component`, `evidence_locator`, `recommendation`, and `audit_sequence`.

---

### Session Statistics Type P12 - Dated XIIGen Gap Review Documents

**Registry status:** `full_type`

**What it is:** Date-stamped XIIGen gap review documents.

**Examples in the XIIGen corpus:**

```text
XIIGEN-GAP-REVIEW-2026-04-17.md
```

**Authority boundary:** Type 32/33 gap review subtype. Date is part of canonicality.

**A-side use:** No.

**B-side chosen use:** Yes for date-scoped gap findings, expected-vs-actual review,
and review status at that date.

**Rejected source:** Old dated review used as current, date omitted, or gap finding
used outside reviewed scope.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `review_date`, `gap_review_path`, `review_scope`,
`expected_state`, `actual_state`, `gap_findings`, and `superseded_by`.

---

## Final Tool-Result Header Families

The `Q01-Q05` families come from full H1/header analysis of `tool_result` content.
They are source structures that were only visible after reading returned file text,
not just paths or filenames.

---

### Tool-Result Header Type Q01 - Architectural Gap To Skill Remediation Documents

**Registry status:** `full_type`

**What it is:** Documents mapping architectural gaps to skill edits, new skills, and
testing timeframes.

**Examples in the XIIGen corpus:**

```text
XIIGEN SKILLS GAP DOCUMENT
Missing Skills, Skills to Edit, and How to Design, Review and Test
```

**Authority boundary:** Architectural gap remediation source. Distinct from
individual SKILL.md files and skills factory registries.

**A-side use:** No unless the document preserves the original gap-remediation prompt.

**B-side chosen use:** Yes for gap id, affected skills, skills to edit, new skills,
testing timeframe, and verification script.

**Rejected source:** Treating a missing skill name as sufficient behavior authority,
wrong skill edit for a gap, or omitting verification script/timeframe.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `gap_document_path`, `gap_id`, `architectural_gap`,
`skills_to_edit`, `new_skill_ids`, `testing_timeframe`, `verification_script_locator`,
and `follows_after_document`.

---

### Tool-Result Header Type Q02 - Planning Session Startup Skill

**Registry status:** `full_type`

**What it is:** Missing SKILL.md file for planning-session startup orientation and Q0
protocol.

**Examples in the XIIGen corpus:**

```text
planning-session-startup-SKILL.md
name: planning-session-startup
version: 1.2.0
```

**Authority boundary:** Type 20 skill library source. It must be recovered as a full
skill before it is used as B-side authority.

**A-side use:** No.

**B-side chosen use:** Yes for planning-session startup risks, Q0 extraction
protocol, implementationMode, stackTargets, and complementary skill boundaries.

**Rejected source:** Startup skill inferred from name only, Q0 skipped, or stale
baseline review before implementationMode/stackTargets extraction.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `skill_id=planning-session-startup`, `skill_version`,
`skill_source_path`, `frontmatter_fields`, `q0_protocol_fields`, `risk_ids`,
`complements`, and `activation_boundary`.

---

### Tool-Result Header Type Q03 - Architectural Decision Testing Skill

**Registry status:** `full_type`

**What it is:** Missing SKILL.md file for testing architecture decisions over
immediate, first-flow, and three-flow timeframes.

**Examples in the XIIGen corpus:**

```text
planning--architectural-decision-testing-SKILL.md
```

**Authority boundary:** Type 20 skill source with embedded Type 28 verification query
zones.

**A-side use:** No.

**B-side chosen use:** Yes for testing timeframe, decision reopening protocol,
DECISION_REOPENING block, and verification query expectations.

**Rejected source:** Decision accepted without timeframe testing, reopening protocol
omitted, or ES query section ignored.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `skill_id=planning--architectural-decision-testing`,
`skill_source_path`, `testing_timeframe`, `decision_reopening_block`,
`verification_query_ids`, `expected_query_result`, and `skill_version`.

---

### Tool-Result Header Type Q04 - Claude Code Integration Instructions

**Registry status:** `full_type`

**What it is:** Claude Code specific integration instruction documents.

**Examples in the XIIGen corpus:**

```text
INTEGRATION INSTRUCTIONS - Claude Code
```

**Authority boundary:** Type 15/integration-instruction subtype specific to Claude
Code. It is not interchangeable with Claude.ai integration instructions.

**A-side use:** No unless the integration instruction is the copied execution prompt.

**B-side chosen use:** Yes for Claude Code setup, file placement, verification, and
tool-specific execution steps.

**Rejected source:** Claude.ai integration used for Claude Code, tool-specific step
omitted, or integration instruction treated as generic session load plan.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `integration_doc_id`, `target_tool=claude_code`,
`instruction_version`, `file_placement_steps`, `verification_steps`,
`tool_specific_constraints`, and `source_locator`.

---

### Tool-Result Header Type Q05 - Executable Verification Query Sections

**Registry status:** `full_type`

**What it is:** Executable verification sections embedded inside skill or remediation
documents, often using Elasticsearch queries.

**Examples in the XIIGen corpus:**

```text
Were there generation failures a better NODE would have prevented?
Compare GAP_SIGNAL count: flows with NODEs vs flows before NODEs
Expected: lower count for NODE-enabled flows
Did the stack profiles hold?
Did AF-4 retrieve ARCHITECTURE_DECISION patterns during 2nd and 3rd flows?
```

**Authority boundary:** Type 28 test assertion material embedded as zones inside
Type 20/Q01/Q03 documents.

**A-side use:** No.

**B-side chosen use:** Yes for verification query, expected result, index/query
target, and pass/fail assertion.

**Rejected source:** Skill/remediation claim used without running or citing
verification query, expected result omitted, or query header treated as prose only.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `parent_document_id`, `verification_query_id`, `query_header`,
`query_body_locator`, `target_index_or_source`, `expected_result`, `assertion_kind`,
and `target_registry_type=Type 28`.

---

## Docs ZIP Internal Session Process Families

The `R01-R13` families come from opening all non-empty ZIPs inside the live docs
folder. They distinguish reusable process templates, instantiated session rounds,
simulation outputs, and per-task teaching documents.

---

### Docs ZIP Type R01 - Planning Complete Gate Documents

**Registry status:** `full_type`

**What it is:** Pre-simulation approval gate documents listing deliverables produced
and requiring explicit approval before simulation starts.

**Examples in the XIIGen corpus:**

```text
PLANNING-COMPLETE.md
```

**Authority boundary:** Type 22/session gate checkpoint. It is a pre-simulation gate,
not a post-phase completion record.

**A-side use:** No.

**B-side chosen use:** Yes for deliverables audit, session-teach inventory, DPO triple
counts, RAG seed counts, and explicit approval requirement.

**Rejected source:** Simulation starts without gate approval, deliverables claimed
without inventory row, or checkpoint confused with final completion.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `planning_complete_path`, `flow_id`, `deliverables_by_round`,
`session_teach_inventory`, `dpo_triple_counts`, `rag_seed_counts`,
`approval_required=true`, and `approval_status`.

---

### Docs ZIP Type R02 - Per-Round Simulation Documents

**Registry status:** `full_type`

**What it is:** Individual simulation-round documents inside SIMULATION-FINAL
packages.

**Examples in the XIIGen corpus:**

```text
R12-FC-VALIDATION.md
R13-CODEBASE-BASELINE.md
R14-SIMULATION-TRACE.md
R18-*.md
```

**Authority boundary:** Simulation output artifact. It records what a simulation
round found; it is not the session instruction file.

**A-side use:** No.

**B-side chosen use:** Yes for per-round trace, component inventory, phase simulation
findings, and validation status.

**Rejected source:** Session prompt used as simulation result, wrong round trace,
or simulation output applied to another flow.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `simulation_round_id`, `round_doc_path`,
`round_purpose`, `trace_entries`, `validation_status`, `component_inventory`, and
`finding_ids`.

---

### Docs ZIP Type R03 - Simulation Gap Catalogs

**Registry status:** `full_type`

**What it is:** Final simulation gap catalogs with explicit defect classes such as
SILENT_FAILURE, BREAKS, WRONG, and PARTIAL.

**Examples in the XIIGen corpus:**

```text
R19-SIM-GAP-CATALOG.md
```

**Authority boundary:** Type 23/B97-style gap catalog for simulation rounds.

**A-side use:** No unless paired with a real fix request.

**B-side chosen use:** Yes for defect class, root cause, fix class, severity, and
simulation evidence.

**Rejected source:** Gap catalog treated as implemented fix, defect class omitted,
or wrong fix class chosen.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `flow_id`, `sim_gap_catalog_path`, `gap_id`, `defect_class`,
`root_cause`, `fix_class`, `severity`, and `simulation_evidence`.

---

### Docs ZIP Type R04 - Parameterized Gap Process Template Libraries

**Registry status:** `full_type`

**What it is:** Canonical template libraries for GAP-PREP and GAP-TRANSLATE process
sessions using `FLOW-XX` placeholders.

**Examples in the XIIGen corpus:**

```text
GAP-PREP-MASTER-PLAN.md
SESSION-GAPPREP-R0.md
GAP-TRANSLATE-MASTER-PLAN.md
SESSION-TRANSLATE-T0.md
```

**Authority boundary:** Process template authority. Concrete flow session files are
instances of these templates.

**A-side use:** No.

**B-side chosen use:** Yes for placeholder variables, process order, required
sessions, and correct instantiation rules.

**Rejected source:** Instance treated as canonical template, placeholder substitution
wrong, or process stage skipped.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `process_template_id`, `process_kind`, `template_path`,
`placeholder_variables`, `session_sequence`, `instantiation_rule`, and
`canonical_template=true`.

---

### Docs ZIP Type R05 - Gap Prep Intake And Investigation Sessions

**Registry status:** `full_type`

**What it is:** Instantiated SESSION-GAPPREP documents with fixed prompt reminders,
inline context, known facts, and bash verification commands.

**Examples in the XIIGen corpus:**

```text
SESSION-GAPPREP-R0-FLOW09.md
SESSION-GAPPREP-R8-FLOW09.md
```

**Authority boundary:** Instantiated investigation session. It must cite the template
when derived from R04.

**A-side use:** Yes only when the fixed prompt reminder is the actual task prompt.

**B-side chosen use:** Yes for inline variables, known facts, bash command sequence,
and expected outputs.

**Rejected source:** External variables required when context should be inline,
wrong FLOW_ID substitution, or missing verification command.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `flow_id`, `session_id`, `template_id`, `fixed_prompt_params`,
`inline_context`, `known_facts`, `bash_commands`, `expected_outputs`, and
`instantiated_from_template`.

---

### Docs ZIP Type R06 - Blocking Gate Simulation Sessions

**Registry status:** `full_type`

**What it is:** SESSION-SIM documents that act as blocking gates before simulation
continues.

**Examples in the XIIGen corpus:**

```text
SESSION-SIM-R0-FLOW09.md
```

**Authority boundary:** Type 34 gate-session subtype. It records why the gate must
run first and what happens if it is skipped.

**A-side use:** Gate prompt only when preserved.

**B-side chosen use:** Yes for blocking gate label, why-first chain, GREEN/RED gate
status, and verification steps.

**Rejected source:** Proceeding past gate without GREEN status, missing consequence
chain, or non-gate session used as gate.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `session_sim_id`, `gate_id`, `blocking_gate=true`,
`why_first_chain`, `required_status`, `verification_steps`, and `gate_result`.

---

### Docs ZIP Type R07 - Named Architectural Round Sessions

**Registry status:** `full_type`

**What it is:** Named round session documents where the round label encodes an
atomic architectural action.

**Examples in the XIIGen corpus:**

```text
R2-GRAPH-SEED
R8-ROUTING-DECISION
R13-ARBITER-PROMOTE
R17-HANDOFF
R19-WAVE2-GATE
```

**Authority boundary:** Atomic architectural action session. The action label is part
of the DTO and cannot be collapsed into a numeric round only.

**A-side use:** Yes when the session instruction is the actual task prompt.

**B-side chosen use:** Yes for action label, why-first explanation, state updates,
and expected artifact mutations.

**Rejected source:** Round number used without action label, action applied in wrong
order, or state mutation omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `flow_id`, `round_id`, `round_action_label`, `why_first`,
`expected_state_mutation`, `artifact_targets`, `depends_on_round`, and
`completion_gate`.

---

### Docs ZIP Type R08 - Precheck Results JSON

**Registry status:** `full_type`

**What it is:** Structured prerequisite verification JSON before a session begins.

**Examples in the XIIGen corpus:**

```text
FLOW-11-R0-PRECHECK-RESULTS.json
```

**Authority boundary:** Prerequisite verification state. It is not a teaching cycle
summary.

**A-side use:** No.

**B-side chosen use:** Yes for PC ids, RED/AMBER/GREEN status, evidence, prior gap
references, and required action commands.

**Rejected source:** Session begins despite RED precheck, precheck status omitted, or
action command ignored.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `flow_id`, `precheck_results_path`, `precheck_id`,
`precheck_status`, `evidence`, `prior_gap_refs`, `action_commands`, and
`blocks_session_start`.

---

### Docs ZIP Type R09 - Per-Task WHY Teaching Documents

**Registry status:** `full_type`

**What it is:** SESSION-TEACH-TNN documents that teach WHY reasoning for a specific
task type, explicitly not code execution.

**Examples in the XIIGen corpus:**

```text
SESSION-TEACH-T63-T64.md
SESSION-TEACH-T65.md
SESSION-TEACH-T66.md
```

**Authority boundary:** Reasoning DPO teaching source. It is a direct source for WHY
pairs, DPO triples, and RAG seeds.

**A-side use:** Yes if the teaching prompt is preserved.

**B-side chosen use:** Yes for task type reasoning, constraints, DPO triple count,
RAG seed list, and no-code-execution boundary.

**Rejected source:** Execution script substituted for WHY teaching, task type
constraint omitted, or RAG seeds ignored.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `session_teach_id`, `task_type_ids`, `session_type=TEACHING`,
`reasoning_scope`, `dpo_triple_count`, `rag_seed_ids`, `no_code_execution=true`,
and `why_constraints`.

---

### Docs ZIP Type R10 - Cross-Flow Edge Specification Tests

**Registry status:** `full_type`

**What it is:** TypeScript cross-flow dependency tests verifying emitted and consumed
events between flows.

**Examples in the XIIGen corpus:**

```text
FLOW-04_to_FLOW-09.edge.spec.ts
FLOW-04_to_FLOW-10.edge.spec.ts
```

**Authority boundary:** Type 26/A15 bridge. These are contract tests between flows,
not ordinary unit tests.

**A-side use:** No.

**B-side chosen use:** Yes for source flow, target flow, emitted event, consumed
event, dependency assertion, and test verdict.

**Rejected source:** Flow dependency declared without edge spec, wrong consumed event,
or unit test treated as cross-flow contract.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `edge_spec_path`, `source_flow_id`, `target_flow_id`,
`emitted_event`, `consumed_event`, `dependency_assertion`, `test_name`, and
`test_status`.

---

### Docs ZIP Type R11 - Flow Session Input Bundles

**Registry status:** `full_type`

**What it is:** Per-flow input ZIPs bundling product specs, deep research, and
pre-merged session context.

**Examples in the XIIGen corpus:**

```text
19-input files.zip
30-prompt_improvements.zip
NN-{flow-name}_ENGINE_ARCHITECTURE.md
```

**Authority boundary:** Session input package. Member-level classification remains
required, but the bundle shape itself is meaningful.

**A-side use:** Product spec member may be A-side when it preserves original product
request.

**B-side chosen use:** Yes for package composition, product spec/deep research pairing,
and merged context availability.

**Rejected source:** ZIP title as authority, missing product/deep research pairing,
or merged context used for wrong flow.

**Expected overlap ratio:** inherited from member; package metadata 0.10-0.25.

**DTO requirements:** `input_bundle_path`, `flow_id`, `product_spec_member`,
`deep_research_member`, `merged_context_members`, `member_manifest`, and
`member_classification_complete`.

---

### Docs ZIP Type R12 - Gap Matrix Audit Documents

**Registry status:** `full_type`

**What it is:** Audit matrices comparing planned additions against actual session
files with evidence citations.

**Examples in the XIIGen corpus:**

```text
R2-GAP-MATRIX.md
```

**Authority boundary:** Additions-vs-sessions audit. Similar to P03 but specialized
for planning additions and source-of-truth master plans.

**A-side use:** No.

**B-side chosen use:** Yes for source-of-truth plan, addition id, PRESENT/ABSENT
status, and evidence citation.

**Rejected source:** PRESENT claim without evidence, wrong master plan version, or
addition checked against wrong flow/session set.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `gap_matrix_audit_path`, `source_of_truth_plan`,
`source_plan_version`, `addition_id`, `audit_status`, `evidence_citation`,
`flow_id`, and `session_file_ref`.

---

### Docs ZIP Type R13 - Self-Contained Precheck Session Documents

**Registry status:** `full_type`

**What it is:** R0 precheck session documents with full inline context and bash
verification commands for infrastructure gate verification.

**Examples in the XIIGen corpus:**

```text
R0-PRECHECK.md
SESSION-FLOW-11-R0-PRECHECK.md
```

**Authority boundary:** Type 34/A04 gate-verification session. It is a runnable
precheck, not a generic plan.

**A-side use:** Yes when the precheck instruction is the task prompt.

**B-side chosen use:** Yes for inline context, verification command order, expected
output, and block/unblock decision.

**Rejected source:** Precheck references undefined external context, command order
changed, expected output omitted, or gate bypassed.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `precheck_session_path`, `flow_id`, `inline_context`,
`verification_commands`, `expected_outputs`, `gate_decision`, `blocking_conditions`,
and `self_contained=true`.

---

## llm_mvp_core Documentation Families

The `S01-S14` families come from `C:\Projects\llm_mvp_core\docs`. This is a related
C#/.NET LLM-from-scratch project with its own experiment, subproject, and capability
planning taxonomy. It is not an external vendor source.

---

### llm_mvp_core Type S01 - Subproject Maps

**Registry status:** `full_type`

**What it is:** Master registry of llm_mvp_core subprojects with number, directory,
name, purpose, status, and key files.

**Examples in the llm_mvp_core corpus:**

```text
SUBPROJECT-MAP.md
```

**Authority boundary:** Subproject registry. It is not a skill registry and not a
generic architecture diagram.

**A-side use:** No.

**B-side chosen use:** Yes for routing tasks to subprojects, subproject existence,
directory identity, status, and key-file lookup.

**Rejected source:** Task routed to nonexistent subproject, stale status, or directory
guessed from name.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `project_id=llm_mvp_core`, `subproject_number`,
`subproject_directory`, `subproject_name`, `purpose`, `status`, `key_files`, and
`map_version_or_date`.

---

### llm_mvp_core Type S02 - Experiment Results Maps

**Registry status:** `full_type`

**What it is:** Living registry of E01-E50 experiment results, metric deltas,
component routing, and result category.

**Examples in the llm_mvp_core corpus:**

```text
EXPERIMENT-RESULTS-MAP.md
```

**Authority boundary:** Experiment result registry. Distinct from individual run JSON
and experiment plans.

**A-side use:** No.

**B-side chosen use:** Yes for experiment status, metric delta, target component,
PASSED/FAILED/SCALE_UP/ARCH/DATA/TOOL result type, and next routing.

**Rejected source:** Passed claim when map says failed, scale-up rule ignored, or
metric delta fabricated.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `experiment_id`, `component_id`, `metric_delta`,
`result_type`, `use_in_component`, `next_action`, `map_path`, and `row_locator`.

---

### llm_mvp_core Type S03 - Cross-Project Interface References

**Registry status:** `full_type`

**What it is:** Cross-project C# interface reference listing public interfaces,
signatures, implementing classes, and consumers.

**Examples in the llm_mvp_core corpus:**

```text
COMPONENT-INTERFACES.md
ISessionStorage
ICodeReader
ICodeWriter
```

**Authority boundary:** Type 26 subtype for cross-project C# interface references.

**A-side use:** No.

**B-side chosen use:** Yes for interface signature, implementation class, consumer
list, and dependency boundary.

**Rejected source:** Interface inferred from implementation, wrong consumer list, or
TypeScript interface DTO reused for C# reference.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `language=csharp`, `interface_name`, `signature`,
`implementing_class`, `consumers`, `component_id`, and `source_locator`.

---

### llm_mvp_core Type S04 - Experiment Plans

**Registry status:** `full_type`

**What it is:** Strict experiment specification plans with ExperimentId, Baseline,
Enhanced, Metric, and Tier fields.

**Examples in the llm_mvp_core corpus:**

```text
EXPERIMENT-E01-AST-TOKENIZER-PLAN-v1.md
E15-E16-E17-SEMANTIC-COMPILER-PLANS-v1.0.md
```

**Authority boundary:** Experiment spec. It is not a result; result comes from S02 or
M/J experiment result artifacts.

**A-side use:** No unless an explicit experiment-design prompt is included.

**B-side chosen use:** Yes for baseline/enhanced design, metric, tier, experiment
scope, and escalation ordering.

**Rejected source:** Result stated in plan, missing mandatory field, wrong tier
escalation, or grouped experiment plan flattened into one experiment id.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `experiment_id`, `baseline`, `enhanced`, `metric`, `tier`,
`plan_version`, `grouped_experiment_ids`, and `expected_result_kind`.

---

### llm_mvp_core Type S05 - Subproject Implementation Plans

**Registry status:** `full_type`

**What it is:** Per-subproject implementation plans with dependency maps, phases, and
C# component mapping.

**Examples in the llm_mvp_core corpus:**

```text
SUBPROJECT-21-AGENT-INFRASTRUCTURE-PLAN-v1.md
```

**Authority boundary:** Type 29 subtype for C# subproject plans.

**A-side use:** No unless the plan preserves the implementation request.

**B-side chosen use:** Yes for dependency ordering, phase structure, component mapping,
and readiness gates.

**Rejected source:** Flow plan used for subproject, dependency order reversed, or C#
component omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `subproject_number`, `subproject_plan_path`, `dependency_map`,
`phase_ids`, `component_mapping`, `readiness_gates`, and `plan_version`.

---

### llm_mvp_core Type S06 - Capability Session Plans

**Registry status:** `full_type`

**What it is:** Claude Code execution plans for implementing one capability.

**Examples in the llm_mvp_core corpus:**

```text
CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md
ORCHESTRATOR-MODEL-SESSION-PLAN-v1.0.md
```

**Authority boundary:** Capability execution plan. It includes goal verbatim,
before/after state, round-trip steps, and Gate B checks.

**A-side use:** Goal statement only when it is the original user request.

**B-side chosen use:** Yes for capability scope, round-trip sequence, before/after
state, Gate B completeness, and implementation checkpoints.

**Rejected source:** Missing Gate B, before/after omitted, or capability routed to
wrong subproject.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `capability_id`, `session_plan_path`, `goal_verbatim`,
`before_state`, `after_state`, `round_trip_steps`, `gate_b_checks`, and
`target_subproject`.

---

### llm_mvp_core Type S07 - Dataset Collection Plans

**Registry status:** `full_type`

**What it is:** Data sourcing, evaluation, filtering, and packaging plans for
training examples across domains.

**Examples in the llm_mvp_core corpus:**

```text
DATASET-COLLECTION-PLAN.md
```

**Authority boundary:** Data sourcing spec. It is not an experiment result or training
row file.

**A-side use:** No unless the plan preserves the data-collection task prompt.

**B-side chosen use:** Yes for source domains, trustworthiness scoring, filtering
rules, packaging format, and evaluation gates.

**Rejected source:** Generated rows used without source scoring, domain omitted, or
collection plan treated as dataset itself.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `dataset_plan_path`, `domain_ids`, `source_types`,
`trust_score_rule`, `filtering_rules`, `packaging_format`, and `evaluation_gate`.

---

### llm_mvp_core Type S08 - Experiment Fix Plans

**Registry status:** `full_type`

**What it is:** Versioned fix plans for failed or scale-zero experiments.

**Examples in the llm_mvp_core corpus:**

```text
34-EXPERIMENTS-SCALE0-FIXES-PLAN-v2.md
34-EXPERIMENTS-SCALE0-FIXES-PLAN-v6.md
```

**Authority boundary:** Type 16/23 bridge. It tracks plan evolution for failed
experiments.

**A-side use:** No unless a fix request is present.

**B-side chosen use:** Yes for failed experiment id, fix version, before/after fix
strategy, and supersession.

**Rejected source:** Old fix plan used after newer version, fix applied to wrong
experiment, or scale-zero cause ignored.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `fix_plan_version`, `failed_experiment_ids`,
`scale_zero_reason`, `fix_steps`, `supersedes_version`, and `expected_validation`.

---

### llm_mvp_core Type S09 - Research Hypothesis Plans

**Registry status:** `full_type`

**What it is:** Hypothesis-first research plans with methodology, expected outcomes,
and failure modes.

**Examples in the llm_mvp_core corpus:**

```text
RESEARCH-01-SEMANTIC-TRAINING-PLAN-v1.md
```

**Authority boundary:** Research plan. It is not implementation plan and not result.

**A-side use:** Research question only if preserved.

**B-side chosen use:** Yes for hypothesis, methodology, expected outcome, failure
mode, and confirmation/refutation route.

**Rejected source:** Hypothesis treated as confirmed result, missing baseline,
methodology omitted, or failure mode ignored.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `research_plan_id`, `hypothesis`, `methodology`,
`expected_outcome`, `failure_modes`, `baseline`, and `confirmation_metric`.

---

### llm_mvp_core Type S10 - LLM Master Implementation Plans

**Registry status:** `full_type`

**What it is:** Versioned master implementation plans for the whole llm_mvp_core
project.

**Examples in the llm_mvp_core corpus:**

```text
LLM-MVP-CORE-SUBPROJECTS-IMPLEMENTATION-PLAN-v1.md
LLM-MVP-CORE-SUBPROJECTS-IMPLEMENTATION-PLAN-v2.md
```

**Authority boundary:** Type 16 master-plan evolution source for llm_mvp_core.

**A-side use:** No.

**B-side chosen use:** Yes for project-wide subproject ordering, supersession,
master plan version, and implementation phases.

**Rejected source:** Old master plan used as current, subproject dependency omitted,
or XIIGen Vxx plan substituted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `master_plan_version`, `project_id=llm_mvp_core`,
`subproject_order`, `implementation_phases`, `supersedes_version`, and
`current_confirmed_via`.

---

### llm_mvp_core Type S11 - Experiment Harness Documents

**Registry status:** `full_type`

**What it is:** Infrastructure documents for running experiments and harness proxies.

**Examples in the llm_mvp_core corpus:**

```text
EXPERIMENT-HARNESS-v2-MARKOV-PROXY.md
```

**Authority boundary:** Type 19/25 extension for experiment execution infrastructure.

**A-side use:** No.

**B-side chosen use:** Yes for harness setup, proxy configuration, run procedure, and
expected harness output.

**Rejected source:** Harness spec treated as experiment result, wrong proxy version,
or run procedure omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `harness_doc_path`, `harness_version`, `proxy_kind`,
`configuration_fields`, `run_procedure`, `expected_output`, and `applies_to_experiments`.

---

### llm_mvp_core Type S12 - Project-Specific LLM Skills

**Registry status:** `full_type`

**What it is:** Skills that exist specifically for the C#/.NET LLM-from-scratch
project.

**Examples in the llm_mvp_core corpus:**

```text
experiment-design
subproject-examination
capability-simulation
training-data-gap-audit
subproject-completeness-checker
engine-independence-tracking
quality-improvement-cycle
```

**Authority boundary:** Type 20 skill library source for llm_mvp_core only.

**A-side use:** No.

**B-side chosen use:** Yes for skill activation boundary, project-specific skill
behavior, and capability routing within llm_mvp_core.

**Rejected source:** Skill applied to XIIGen without adoption, wrong capability skill,
or skill behavior inferred from name.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `skill_id`, `project_id=llm_mvp_core`, `skill_source_path`,
`activation_rule`, `skill_boundary`, `allowed_context`, and `not_xiigen_authority=true`.

---

### llm_mvp_core Type S13 - Additional XIIGen-Missing Skills Found In llm_mvp_core

**Registry status:** `full_type`

**What it is:** Skills present in llm_mvp_core docs that also close gaps in XIIGen's
missing-skill list.

**Examples in the llm_mvp_core corpus:**

```text
dna-compliance-guard
freedom-machine
arbiter-panel-design
plan-review
reconnaissance-gate
session-mode-declaration
scope-isolation-arbiter
```

**Authority boundary:** Type 20 recovery source. Must be checked for whether it is
project-specific, shared, or adopted into XIIGen.

**A-side use:** No.

**B-side chosen use:** Yes only after adoption/canonicality is recorded.

**Rejected source:** llm_mvp_core skill treated as XIIGen canonical without adoption,
duplicate skill version conflict ignored, or missing-skill recovery inferred from
filename.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `skill_id`, `skill_source_path`, `found_in_project`,
`adoption_status`, `xiigen_canonicality`, `activation_rule`, and `skill_boundary`.

---

### llm_mvp_core Type S14 - Shared General-Purpose Skills

**Registry status:** `limited_authority`

**What it is:** General-purpose skills usable across both XIIGen and llm_mvp_core.

**Examples in the llm_mvp_core corpus:**

```text
blast-radius
change-propagation
test-integrity
self-verification
three-level-verification
work-scope-inventory
```

**Authority boundary:** Type 20 shared skill source. Authority is structural unless a
specific project adopts the skill.

**A-side use:** No.

**B-side chosen use:** Limited to shared skill behavior or adopted project context.

**Rejected source:** Shared skill applied as project-specific authority without
adoption, or activation boundary ignored.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `skill_id`, `skill_source_path`, `shared_skill=true`,
`applicable_projects`, `adoption_status_by_project`, `activation_rule`, and
`skill_boundary`.

---

## Final docs.zip Live Governance And Evidence Families

The `T01-T10` families come from the final full read of remaining live docs files.
They include compact handoff briefs, machine-readable learning handoffs, versioned
session governance, execution block plans, portability evidence, and planning gap
reports.

---

### Live Docs Type T01 - Session Brief Handoff Documents

**Registry status:** `full_type`

**What it is:** Compact human-readable session handoff briefs with current state,
concrete defects, thresholds, produced artifacts, and next-session guidance.

**Examples in the XIIGen corpus:**

```text
SESSION-BRIEF.md
SESSION-BRIEF-A.md
```

**Authority boundary:** Human-readable post-session handoff. It is not a continuation
instruction guide and not a cycle metrics file.

**A-side use:** Direct next-session question only.

**B-side chosen use:** Yes for defect list, thresholds, file paths, artifact list,
skill references, and next-session work.

**Rejected source:** Vague "needs improvement" without numbers, threshold omitted,
or brief treated as completed fix.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `session_brief_path`, `flow_id`, `branch_or_context`,
`state_verdict`, `defect_items`, `threshold_values`, `file_paths`, `artifact_ids`,
and `next_session_actions`.

---

### Live Docs Type T02 - Learning Handoff JSON

**Registry status:** `full_type`

**What it is:** Machine-readable flow learning-state transfer with per-task routing
decisions, scores, arbiter status, and notes.

**Examples in the XIIGen corpus:**

```text
FLOW-42-LEARNING-HANDOFF.json
```

**Authority boundary:** Type 22 learning-state extension. It is not raw training rows.

**A-side use:** No.

**B-side chosen use:** Yes for task type routing decision, routing/genesis/patched
scores, arbiter status, and learning notes.

**Rejected source:** Routing decision without score evidence, CYCLE_ONLY used when
handoff shows patching, or task type row copied from another flow.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `learning_handoff_path`, `flow_id`, `task_type_id`,
`routing_decision`, `routing_score`, `genesis_score`, `patched_score`,
`arbiter_status`, and `notes_locator`.

---

### Live Docs Type T03 - Versioned Session Load Plans

**Registry status:** `full_type`

**What it is:** Versioned session load governance documents defining pipeline data
contracts, phase status, session completion status, T-number registry, and hidden
dependency warnings.

**Examples in the XIIGen corpus:**

```text
XIIGEN-SESSION-LOAD-PLAN-v5.md
XIIGEN-SESSION-LOAD-PLAN-v29.md
```

**Authority boundary:** Session setup authority. Latest version is canonical; earlier
versions are Type 16 evolution evidence.

**A-side use:** No.

**B-side chosen use:** Yes for data contract rows, phase/session status, hidden
dependencies, T-number registry, and load requirements.

**Rejected source:** Session starts without required contracts, stale version used as
current, or hidden dependency warning ignored.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `session_load_plan_version`, `canonical_version`,
`pipeline_data_contracts`, `phase_status_table`, `session_completion_status`,
`t_number_registry`, `hidden_dependency_warnings`, and `supersedes_version`.

---

### Live Docs Type T04 - Block Plan JSON

**Registry status:** `full_type`

**What it is:** Machine-readable ordered gap execution blocks with round/session
mapping, severity, actions, files changed, scope, systemic flag, and dependencies.

**Examples in the XIIGen corpus:**

```text
FLOW-43-BLOCK-PLAN.json
```

**Authority boundary:** Type 23 ordered execution plan. Distinct from human-readable
gaps master plans.

**A-side use:** No.

**B-side chosen use:** Yes for block order, severity, gap id, round id, session file,
files changed, scope, systemic flag, and dependency constraints.

**Rejected source:** Phase execution starts before blocker block, dependency skipped,
or human-readable plan used instead of block order.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `block_plan_path`, `flow_id`, `block_label`, `round_id`,
`session_file`, `gap_id`, `severity`, `action`, `files_changed`, `scope`,
`systemic`, and `depends_on`.

---

### Live Docs Type T05 - Extraction Registries

**Registry status:** `full_type`

**What it is:** Document extraction audit registry tracking processed/skipped files,
file type counts, document type counts, per-file extraction status, and extracted
records.

**Examples in the XIIGen corpus:**

```text
extraction-registry.json
```

**Authority boundary:** Document-processing audit registry. It is not the extracted
content itself.

**A-side use:** No.

**B-side chosen use:** Yes for extraction counts, status counts, document type counts,
per-file status, and extracted record totals.

**Rejected source:** Count fabricated, TYPE_1..TYPE_5 mapping ignored, or skipped
file treated as processed.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `extraction_registry_path`, `total_files`, `status_counts`,
`file_type_counts`, `document_type_counts`, `file_record_locator`,
`extracted_records`, and `notes`.

---

### Live Docs Type T06 - Maintenance Migration And Seeding Scripts

**Registry status:** `full_type`

**What it is:** Single-purpose maintenance scripts for contract migration, RAG
seeding, contract verification, and spec parking.

**Examples in the XIIGen corpus:**

```text
add-stack-coupling.js
seed-rag.py
verify-contracts.js
park-specs.py
```

**Authority boundary:** A04/Type 26 bridge for maintenance scripts, not production
runtime code.

**A-side use:** Real script-writing prompt only.

**B-side chosen use:** Yes for script behavior, idempotency, markers, regex/file
selection, and verification command.

**Rejected source:** Non-idempotent migration, marker ignored, production code
treated as maintenance script, or verification omitted.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `script_path`, `script_language`, `maintenance_purpose`,
`target_files`, `idempotency_guard`, `markers_respected`, `verification_command`,
and `canonical_for_migration`.

---

### Live Docs Type T07 - Portability Phase Evidence JSON

**Registry status:** `full_type`

**What it is:** Machine-generated per-phase portability test evidence JSONs.

**Examples in the XIIGen corpus:**

```text
phase-1-unit-integration-evidence.json
phase-2-playwright-png-evidence.json
phase-3-role-matrix-report.json
phase-H-auth-decoration-evidence.json
phase-I-certification-declaration-evidence.json
```

**Authority boundary:** Type 32/4 portability evidence. It is structured proof for a
specific phase of the portability protocol.

**A-side use:** No.

**B-side chosen use:** Yes for phase status, verification command/result, exit code,
phaseCloseScan, portability phase, and pass/fail evidence.

**Rejected source:** Passed claim with non-zero close-scan count, wrong phase evidence,
or audit trail substituted for machine evidence.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `phase_evidence_path`, `flow_id`, `flow_slug`, `phase`,
`status`, `verification_commands`, `exit_codes`, `phase_close_scan`,
`tenant_variant`, and `certification_status`.

---

### Live Docs Type T08 - Versioned Skill Registration Manifests

**Registry status:** `full_type`

**What it is:** Versioned skill registration manifests with skill id, name, layer,
path, requirements, complements, weights, trigger keywords, namespace warnings, and
flow registration location.

**Examples in the XIIGen corpus:**

```text
SKILL-REGISTRATION-MANIFEST-v2.md
SKILL-REGISTRATION-MANIFEST-v5.md
```

**Authority boundary:** Type 30 active registration extension. Distinct from skill
reference indexes.

**A-side use:** No.

**B-side chosen use:** Yes for skill registration location, version, dependencies,
trigger keywords, and namespace warnings.

**Rejected source:** Skill factory catalog used as active registration, wrong
registration flow/phase, or stale manifest version.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `manifest_version`, `skill_id`, `skill_name`, `layer`, `path`,
`requires`, `complements`, `weight`, `trigger_keywords`, `namespace_warnings`,
`registration_location`, and `supersedes_version`.

---

### Live Docs Type T09 - Flow Execution Protocol And Preparation Guides

**Registry status:** `full_type`

**What it is:** Master procedure guides for flow preparation, execution protocol,
testing, OSS teaching, and missing-flow preparation.

**Examples in the XIIGen corpus:**

```text
XIIGEN-EXECUTION-PROTOCOL-v1.md
XIIGEN-FLOW-PLAN-PREPARATION-GUIDE-v2.md
XIIGEN-FLOW-TESTING-AND-OSS-TEACHING-PROTOCOL-v3.md
```

**Authority boundary:** Flow-preparation process authority. It governs process choice
by flow status; it is not a coding tool instruction.

**A-side use:** No.

**B-side chosen use:** Yes for flow inventory status, universal improvement loop,
calibration/OSS curriculum records, depth field semantics, and complete/different
format/missing flow procedure.

**Rejected source:** COMPLETE-flow protocol applied to MISSING-flow, stale protocol
version, or depth semantics omitted.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `protocol_doc_path`, `protocol_version`, `flow_status_class`,
`procedure_steps`, `depth_field_semantics`, `curriculum_record_structure`,
`improvement_loop`, and `canonical_version`.

---

### Live Docs Type T10 - Planning Requirements Gap Reports

**Registry status:** `full_type`

**What it is:** Fleet-level testing, E2E coverage, and observability gap reports.

**Examples in the XIIGen corpus:**

```text
FLOW-01-24-TESTING-GAP-REPORT-v4.md
E2E-COVERAGE-GAP.md
REPORT-OBSERVABILITY-GAP.md
```

**Authority boundary:** Flow-coverage/testing gap analysis extension. Latest version
wins when versions exist.

**A-side use:** Direct gap-status question only.

**B-side chosen use:** Yes for per-flow testing status, E2E gaps, observability gaps,
gap count, priority, and remediation progress.

**Rejected source:** Stale v3 data used after v4, coverage claim without gap report
row, or observability gap treated as E2E gap.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `gap_report_path`, `report_version`, `flow_range`,
`flow_id`, `test_status`, `gap_count`, `gap_priority`, `gap_kind`, and
`supersedes_report`.

---

## Detailed llm_mvp_core Skill And Plan Families

The `U01-U20` families refine S12-S14 where a broad "skill file" classification is
not enough. Each family has distinct DTO fields that change rejected construction.

---

### llm_mvp_core Detail Type U01 - Archetype Failure Map Skill

**Registry status:** `full_type`

**What it is:** Skill defining score brackets and required actions per capability
class.

**Examples in the llm_mvp_core corpus:**

```text
archetype-failure-map
```

**Authority boundary:** Type 20 skill. It is action policy for capability score
brackets, not a measurement result.

**A-side use:** No.

**B-side chosen use:** Yes for score bracket, action, capability class, and stop/patch
decision.

**Rejected source:** Proceeding after structural failure, treating near-pass as pass,
or missing iron-rule violation search.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `skill_id`, `capability_class`, `score_bracket`,
`action_required`, `threshold_min`, `threshold_max`, and `loaded_after_skill`.

---

### llm_mvp_core Detail Type U02 - Blast Radius And Tagger Skills

**Registry status:** `full_type`

**What it is:** Paired skills for change blast-radius classification and deliverable
file tagging.

**Examples in the llm_mvp_core corpus:**

```text
blast-radius
blast-radius-tagger
```

**Authority boundary:** Type 20 skill pair. One classifies change severity; the other
tags affected deliverables.

**A-side use:** No.

**B-side chosen use:** Yes for CRITICAL/HIGH/MEDIUM/LOW triggers, deliverable tags,
and tag-specific gates.

**Rejected source:** Interface change tagged LOW, production deliverable tagged
TEST-ONLY, or gate omitted for tag.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `change_id`, `blast_radius_level`, `csharp_trigger`,
`deliverable_file`, `deliverable_tag`, `tag_gate_requirements`, and `skill_pair_used`.

---

### llm_mvp_core Detail Type U03 - Challenger Round Design Skill

**Registry status:** `full_type`

**What it is:** Skill defining minimum challenger roles and context isolation by
capability archetype.

**Examples in the llm_mvp_core corpus:**

```text
challenger-round-design
```

**Authority boundary:** Type 20 skill for review/challenge orchestration.

**A-side use:** No.

**B-side chosen use:** Yes for capability archetype, required challenger roles, and
context isolation.

**Rejected source:** Missing required challenger role, challenger sees prohibited
context, or generic review used instead of archetype-specific challengers.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `capability_archetype`, `required_challenger_roles`,
`context_isolation_rule`, `minimum_role_count`, and `review_sequence`.

---

### llm_mvp_core Detail Type U04 - Defense In Depth Skill

**Registry status:** `full_type`

**What it is:** Skill defining four validation layers: Entry Point, Format
Normalization, Domain Invariant, Output Contract.

**Examples in the llm_mvp_core corpus:**

```text
defense-in-depth
```

**Authority boundary:** Type 20/Type 28 skill. It maps validation layers to C# checks.

**A-side use:** No.

**B-side chosen use:** Yes for validation layer choice, C# example, and layer order.

**Rejected source:** Domain invariant checked before normalization, missing output
contract, or single-layer validation presented as defense-in-depth.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `validation_layer`, `layer_order`, `csharp_example_locator`,
`input_condition`, `expected_failure_mode`, and `loaded_after_skill`.

---

### llm_mvp_core Detail Type U05 - E2E Test Matrix Skill

**Registry status:** `full_type`

**What it is:** Skill requiring eight scenario categories for each subproject and
xUnit test templates.

**Examples in the llm_mvp_core corpus:**

```text
e2e-test-matrix
```

**Authority boundary:** Type 20/A15 bridge.

**A-side use:** No.

**B-side chosen use:** Yes for required scenario categories, test coverage matrix,
and xUnit template structure.

**Rejected source:** Missing Recovery/Error/Concurrent category, test without category,
or unit test substituted for E2E matrix.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `subproject_id`, `scenario_category`, `required_categories`,
`xunit_template_locator`, `coverage_status`, and `missing_category_ids`.

---

### llm_mvp_core Detail Type U06 - Escalation Orchestrator Skill

**Registry status:** `full_type`

**What it is:** Skill aggregating panel verdicts with six priority-ordered decision
rules.

**Examples in the llm_mvp_core corpus:**

```text
escalation-orchestrator
```

**Authority boundary:** Type 20 arbiter aggregation skill.

**A-side use:** No.

**B-side chosen use:** Yes for decision rule ordering, ANY BLOCK/ALL PASS semantics,
and escalation outcome.

**Rejected source:** Lower-priority accept rule applied before block rule, panel
verdict ignored, or rule order omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `panel_verdicts`, `decision_rule_id`, `rule_priority_order`,
`aggregate_outcome`, `blocking_verdicts`, and `applied_rule`.

---

### llm_mvp_core Detail Type U07 - Freedom Machine Skill

**Registry status:** `full_type`

**What it is:** Skill classifying values as MACHINE or FREEDOM using behavioral
guarantee breakage.

**Examples in the llm_mvp_core corpus:**

```text
freedom-machine
```

**Authority boundary:** Type 20/architecture classifier. Distinct from informal
FREEDOM/MACHINE prose.

**A-side use:** Direct classification question only.

**B-side chosen use:** Yes for classifier question, MACHINE/FREEDOM verdict, canonical
MACHINE values, and rationale.

**Rejected source:** Configurable value classified MACHINE without guarantee break,
or MACHINE value exposed as FREEDOM.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `value_name`, `behavioral_guarantee_breaks`, `classification`,
`canonical_machine_value`, `rationale`, and `skill_version`.

---

### llm_mvp_core Detail Type U08 - Investigation Handoff Skill And Documents

**Registry status:** `full_type`

**What it is:** Skill and recurring document pattern for investigation handoffs.

**Examples in the llm_mvp_core corpus:**

```text
investigation-handoff
INVESTIGATION-HANDOFF-[topic]-[YYYY-MM-DD].md
```

**Authority boundary:** Type 20 skill plus document-template source.

**A-side use:** No unless investigation task prompt is preserved.

**B-side chosen use:** Yes for CONTEXT/DISCOVERIES/OPEN QUESTIONS/RECOMMENDED NEXT
sections and STATE discoveries[] update.

**Rejected source:** Handoff without open questions, missing STATE update, or
investigation result used as implementation proof.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `handoff_topic`, `handoff_date`, `context_section`,
`discoveries`, `open_questions`, `recommended_next`, and `state_discoveries_update`.

---

### llm_mvp_core Detail Type U09 - Materialization Session Type Skill

**Registry status:** `full_type`

**What it is:** Skill defining MATERIALIZATION as a session type for existing design
plus existing code plus wiring gap to user-visible surface.

**Examples in the llm_mvp_core corpus:**

```text
materialization-session-type
```

**Authority boundary:** Session type classifier. Distinct from PLANNING, GENERATION,
and EXECUTOR.

**A-side use:** Direct session-classification question only.

**B-side chosen use:** Yes for materialization trigger, comparison table, and default
rule.

**Rejected source:** UI wiring task classified as PLANNING, or MATERIALIZATION used
when design/code do not exist.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `session_candidate`, `existing_design`, `existing_code`,
`wiring_gap`, `user_visible_surface`, `session_type_verdict`, and `default_rule`.

---

### llm_mvp_core Detail Type U10 - Mental Debug C# Translation Skill

**Registry status:** `full_type`

**What it is:** C#/.NET v3 translation of mental-debug skill with diagnostic routing.

**Examples in the llm_mvp_core corpus:**

```text
mental-debug v3.0
```

**Authority boundary:** Type 20 versioned skill translation. It is not identical to
the NestJS/TypeScript XIIGen version.

**A-side use:** No.

**B-side chosen use:** Yes for symptom-to-diagnostic routing in C# context.

**Rejected source:** TypeScript diagnostic route applied to C# symptom, or v3.0
translation ignored.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `skill_version=3.0`, `source_stack`, `target_stack=csharp_dotnet`,
`symptom`, `apply_first`, `apply_then`, and `translation_note`.

---

### llm_mvp_core Detail Type U11 - Principles Arbiter Skill

**Registry status:** `full_type`

**What it is:** Skill defining arbiter isolation rules and verdict format.

**Examples in the llm_mvp_core corpus:**

```text
principles-arbiter
```

**Authority boundary:** Type 20/Type 8 arbiter skill. It limits what context the
arbiter may see.

**A-side use:** No.

**B-side chosen use:** Yes for allowed context, prohibited context, verdict fields,
and isolation rule.

**Rejected source:** Arbiter receives iron rules when prohibited, verdict format
wrong, or business logic domain mixed into principles arbiter.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `arbiter_id`, `allowed_context`, `prohibited_context`,
`verdict_format`, `isolation_rule`, and `input_code_locator`.

---

### llm_mvp_core Detail Type U12 - Retroactive Development C# Skill

**Registry status:** `full_type`

**What it is:** C#-specific retroactive development skill: fix the engine, not the
output.

**Examples in the llm_mvp_core corpus:**

```text
retroactive-development
```

**Authority boundary:** Type 20/23 skill. Distinct from XIIGen version when stack or
propagation rules differ.

**A-side use:** No.

**B-side chosen use:** Yes for source fix, downstream propagation, test rerun, and
regression verification.

**Rejected source:** Output patched directly, downstream not re-run, or C# engine
source not fixed.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `failure_source`, `engine_fix_target`, `downstream_targets`,
`rerun_tests`, `regression_checks`, and `stack=csharp_dotnet`.

---

### llm_mvp_core Detail Type U13 - Solution Scope Gate Skill

**Registry status:** `full_type`

**What it is:** Skill defining a five-level scope ladder from convention to new
interface.

**Examples in the llm_mvp_core corpus:**

```text
solution-scope-gate
```

**Authority boundary:** Type 20 planning gate.

**A-side use:** Direct scope question only.

**B-side chosen use:** Yes for ladder level, lower-level insufficiency evidence, and
anti-overengineering decision.

**Rejected source:** New interface chosen before convention/adaptation considered,
or scope jumps without insufficiency evidence.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `scope_ladder_level`, `candidate_solution`, `lower_level_checked`,
`insufficiency_evidence`, `chosen_level`, and `overengineering_guard`.

---

### llm_mvp_core Detail Type U14 - Subproject Completeness Checker Skill

**Registry status:** `full_type`

**What it is:** Skill with two pre-gates that block all other checks: goal-to-phase
mapping and design artifact population.

**Examples in the llm_mvp_core corpus:**

```text
subproject-completeness-checker
```

**Authority boundary:** Type 20/34 gate skill.

**A-side use:** No.

**B-side chosen use:** Yes for pre-gate status, blocking rule, and completeness
decision.

**Rejected source:** Downstream checks run before pre-gates pass, missing design
artifact treated as acceptable, or incomplete goal-phase map.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `subproject_id`, `goal_to_phase_map_status`,
`design_artifact_population_status`, `blocks_other_checks`, `missing_artifacts`,
and `gate_verdict`.

---

### llm_mvp_core Detail Type U15 - Subproject Examination Skill

**Registry status:** `full_type`

**What it is:** Skill requiring exactly three documents before any `.cs` file is
created: master session, interface gap analysis, dependency list.

**Examples in the llm_mvp_core corpus:**

```text
subproject-examination
```

**Authority boundary:** Type 20/29 gate skill for C# subproject execution.

**A-side use:** No.

**B-side chosen use:** Yes for three-document gate, approval requirement, and no-code
boundary.

**Rejected source:** C# file created before three docs approved, one of three docs
missing, or examination collapsed into implementation.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `subproject_id`, `required_doc_master_session`,
`required_doc_interface_gap_analysis`, `required_doc_dependency_list`,
`approval_status`, and `no_cs_before_approval=true`.

---

### llm_mvp_core Detail Type U16 - Temporal Behavior Skill

**Registry status:** `full_type`

**What it is:** Skill modeling time-dependent service behavior through anchor event,
phases, weights, and formula chain.

**Examples in the llm_mvp_core corpus:**

```text
temporal-behavior
```

**Authority boundary:** Type 20/CODE_Q skill. Distinct from scheduled archetype.

**A-side use:** Direct temporal behavior question only.

**B-side chosen use:** Yes for anchor event, phase sequence, weights, formula chain,
and non-scheduled distinction.

**Rejected source:** Scheduled archetype substituted, anchor event missing, or formula
chain omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `service_id`, `anchor_event`, `phases`, `weights`,
`formula_chain`, `temporal_vs_scheduled_verdict`, and `expected_behavior_change`.

---

### llm_mvp_core Detail Type U17 - Wave Assignment Skill

**Registry status:** `full_type`

**What it is:** Skill defining FOUNDATION / WAVE 0 / WAVE 1 / WAVE 2+ and absolute
vs delta gate models.

**Examples in the llm_mvp_core corpus:**

```text
wave-assignment
```

**Authority boundary:** Type 20 planning/routing skill. It complements N04 but is a
skill, not only a wave document.

**A-side use:** Direct wave assignment question only.

**B-side chosen use:** Yes for wave structure, infrastructure test rule, gate model,
and dependency threshold.

**Rejected source:** Capability with N>2 dependents assigned outside FOUNDATION, or
absolute/delta gate model swapped.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `capability_id`, `dependent_subproject_count`,
`assigned_wave`, `gate_model`, `infrastructure_test_rule`, and `assignment_reason`.

---

### llm_mvp_core Detail Type U18 - Goal Context And Delivery Completeness Skills

**Registry status:** `full_type`

**What it is:** Enhanced goal-context and goal-delivery completeness skills, including
the rule that goal-delivery-completeness runs first.

**Examples in the llm_mvp_core corpus:**

```text
goal-context-persistence
goal-delivery-completeness
```

**Authority boundary:** Type 20 skill enhancement. Treat as distinct when triggers or
execution order differ from older XIIGen entries.

**A-side use:** No.

**B-side chosen use:** Yes for FIRST-check rule, goal persistence, completeness
triggers, and order before other checks.

**Rejected source:** Another check runs before goal-delivery-completeness, or goal
context lost across session.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `skill_id`, `runs_first=true`, `goal_context_fields`,
`completeness_triggers`, `ordering_rule`, and `version_or_project_variant`.

---

### llm_mvp_core Detail Type U19 - Session Prompt Skills Adaptation Plans

**Registry status:** `full_type`

**What it is:** Meta-plan for adapting XIIGen session prompt/governance skills to a
different project.

**Examples in the llm_mvp_core corpus:**

```text
SESSION-PROMPT-SKILLS-ADAPTATION-PLAN-v1.0.md
```

**Authority boundary:** Governance migration plan. It maps what to remove, replace,
and translate.

**A-side use:** No unless the adaptation task prompt is preserved.

**B-side chosen use:** Yes for concept mapping, removed sections, replacements, and
local equivalents.

**Rejected source:** XIIGen concept copied without local equivalent, remove/replace
step skipped, or adaptation treated as direct import.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `adaptation_plan_path`, `source_project`, `target_project`,
`remove_items`, `replace_items`, `concept_mapping`, `local_equivalents`, and
`adaptation_version`.

---

### llm_mvp_core Detail Type U20 - Investigation Handoff Document Directory Pattern

**Registry status:** `full_type`

**What it is:** Recurring `docs/investigations/` handoff document pattern produced by
investigation-handoff skill.

**Examples in the llm_mvp_core corpus:**

```text
docs/investigations/INVESTIGATION-HANDOFF-*.md
```

**Authority boundary:** Investigation artifact. Distinct from the skill definition
that produces it.

**A-side use:** No unless original investigation prompt is preserved.

**B-side chosen use:** Yes for discoveries, open questions, recommended next action,
and state linkage.

**Rejected source:** Handoff used as final implementation answer, open questions
omitted, or STATE discoveries linkage missing.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `investigation_handoff_path`, `topic`, `date`,
`context_summary`, `discoveries`, `open_questions`, `recommended_next`,
`state_linkage`, and `producer_skill_id=investigation-handoff`.

---

## Canonical NestJS-Era xiigen-COMPLETE.zip Families

The `V01-V08` families come from `xiigen-COMPLETE.zip`, the canonical NestJS-era
engine archive. When this archive conflicts with older FLOW-33/Python snapshots, the
NestJS archive is the chosen-side authority and older snapshots are stale evolution
sources.

---

### NestJS Canonical Type V01 - Project-Level Completion State

**Registry status:** `full_type`

**What it is:** Project-level completion state for the canonical NestJS engine.

**Examples in the XIIGen corpus:**

```text
STATE.json
```

**Authority boundary:** Type 22 project-level state. Distinct from per-flow or
session STATE.json.

**A-side use:** No.

**B-side chosen use:** Yes for version, completed phases, cumulative test results,
server modules, client pages, and recovery instructions.

**Rejected source:** Flow STATE.json used for project completion, Python snapshot
state used as current, or test totals fabricated.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `archive_id=xiigen-COMPLETE.zip`, `project_version`,
`completed_phases`, `cumulative_test_results`, `server_modules`, `client_pages`,
`recovery_instructions`, and `canonicality_anchor=true`.

---

### NestJS Canonical Type V02 - DevOps Validation Module

**Registry status:** `full_type`

**What it is:** NestJS production DevOps validation module for CI, compose,
dockerfile, lint, request logging, and structured logging.

**Examples in the XIIGen corpus:**

```text
server/src/devops/
```

**Authority boundary:** Type 26 production module source. Distinct from standalone
maintenance scripts.

**A-side use:** No unless source contains implementation task prompt.

**B-side chosen use:** Yes for validator services, module boundaries, structured
logging, and production DevOps behavior.

**Rejected source:** Standalone script substituted for NestJS service, Console logging,
or Python-era DevOps pattern used as current.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `module_path`, `service_name`, `validator_kind`,
`nestjs_provider`, `logging_pattern`, `input_contract`, and `canonical_nestjs=true`.

---

### NestJS Canonical Type V03 - Self-Documentation Module

**Registry status:** `full_type`

**What it is:** Engine module that generates documentation from registries and source
metadata.

**Examples in the XIIGen corpus:**

```text
server/src/doc-gen/
ServiceCatalogGenerator
OpenApiGenerator
ModuleReadmeGenerator
```

**Authority boundary:** Type 26/source-code contract and self-documenting-system
source. It shows how the engine documents itself.

**A-side use:** Implementation prompt only if preserved.

**B-side chosen use:** Yes for service catalog generation, registry reading, DNA
checklist output, OpenAPI generation, and module README generation.

**Rejected source:** Static hand-written catalog used where generator required, DNA
checklist omitted, or generator not connected to registries.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `doc_gen_module_path`, `generator_class`, `registry_inputs`,
`generated_artifact_kind`, `dna_checklist_fields`, and `output_contract`.

---

### NestJS Canonical Type V04 - Continuous Learning Module

**Registry status:** `full_type`

**What it is:** Production NestJS learning pipeline for feedback storage, scoring,
model selection, prompt A/B testing, prompt evolution, RAG quality, and weight
integration.

**Examples in the XIIGen corpus:**

```text
server/src/learning/
FeedbackStore
QualityScorer
ModelPreference
ModelSelection
PromptAbTester
PromptEvolver
RagQualityTracker
RagWeightIntegrator
```

**Authority boundary:** Type 26 production learning module. It is the implemented
target of several ML/research hypotheses.

**A-side use:** No unless source implementation prompt is preserved.

**B-side chosen use:** Yes for feedback-to-quality-to-model/prompt update flow,
service boundaries, and learning loop contracts.

**Rejected source:** Research hypothesis treated as implementation, Python-era
learning pattern used as current, or prompt evolution step omitted.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `learning_module_path`, `learning_component`, `input_signal`,
`score_or_preference_output`, `model_selection_rule`, `prompt_evolution_rule`,
`rag_quality_fields`, and `canonical_nestjs=true`.

---

### NestJS Canonical Type V05 - CI/CD Pipeline Definition

**Registry status:** `full_type`

**What it is:** GitHub Actions CI/CD pipeline for server, client, and Docker build.

**Examples in the XIIGen corpus:**

```text
.github/workflows/ci.yml
```

**Authority boundary:** A03/CI pipeline full type for canonical NestJS archive.

**A-side use:** CI-writing prompt only if preserved.

**B-side chosen use:** Yes for jobs, caching, lint/format/test/build order, health
checks, and conditional Docker build.

**Rejected source:** Docker build before tests, missing cache, wrong client/server
job separation, or CI inferred from README.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `workflow_path`, `job_id`, `job_order`, `cache_keys`,
`test_commands`, `build_commands`, `docker_build_condition`, and `health_checks`.

---

### NestJS Canonical Type V06 - Production Docker Compose Definition

**Registry status:** `full_type`

**What it is:** Production Docker Compose definition with infra profile, health
checks, env_file, and named networks.

**Examples in the XIIGen corpus:**

```text
docker-compose.yml
```

**Authority boundary:** A12/deployment template extension. This is production compose,
not dev-only compose.

**A-side use:** Deployment prompt only if preserved.

**B-side chosen use:** Yes for services, profiles, health checks, env files, named
networks, and dependency startup.

**Rejected source:** Dev compose used as production, missing health check, secrets
hardcoded, or infra profile omitted.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `compose_path`, `service_name`, `profile`, `healthcheck`,
`env_file`, `networks`, `depends_on`, and `production_compose=true`.

---

### NestJS Canonical Type V07 - Tenant Isolation E2E Specification

**Registry status:** `full_type`

**What it is:** Canonical DNA-5 tenant isolation E2E test across tenants and fabrics.

**Examples in the XIIGen corpus:**

```text
server/test/fabrics/tenant-isolation-e2e.spec.ts
```

**Authority boundary:** A15/Type 26 bridge. It is the canonical cross-fabric tenant
isolation test.

**A-side use:** Test-writing prompt only if preserved.

**B-side chosen use:** Yes for two-tenant setup, six-fabric coverage, mock CLS helper,
and cross-invisibility assertions.

**Rejected source:** Per-fabric tests without cross-tenant assertions, no CLS helper,
or tenant isolation inferred from service code only.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `test_path`, `tenant_ids`, `fabric_ids`, `cls_helper_pattern`,
`cross_invisibility_assertions`, `test_framework`, and `dna_pattern=DNA-5`.

---

### NestJS Canonical Type V08 - Engine Introspection UI Pages

**Registry status:** `full_type`

**What it is:** React pages for inspecting and steering the engine itself.

**Examples in the XIIGen corpus:**

```text
client/src/pages/GenerationLabPage.tsx
client/src/pages/QualityDashboardPage.tsx
```

**Authority boundary:** Type 26 UI source plus visual/UX evidence target. These pages
are engine introspection surfaces, not ordinary product pages.

**A-side use:** UI implementation prompt only if preserved.

**B-side chosen use:** Yes for AF pipeline run UI, generated code inspection, scoring,
feedback, pass-rate chart, DNA compliance grid, and failure clusters.

**Rejected source:** Product page pattern used for engine introspection, feedback loop
omitted, or dashboard without quality metrics.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `page_path`, `page_kind`, `engine_action`, `visible_metrics`,
`feedback_controls`, `quality_grid_fields`, `failure_cluster_fields`, and
`canonical_nestjs=true`.

---

## Final llm_mvp_core Experiment And Data Contract Refinements

The `W01-W16` families refine S01-S11 after full-text reading of all plans and data
contracts in `C:\Projects\llm_mvp_core\docs`. They prevent broad "experiment plan"
or "plan file" labels from hiding mandatory DTO fields.

---

### llm_mvp_core Refinement Type W01 - Single Experiment Plans

**Registry status:** `full_type`

**What it is:** One-experiment-per-file plans with mandatory experiment fields.

**Examples in the llm_mvp_core corpus:**

```text
EXPERIMENT-E01-AST-TOKENIZER-PLAN-v1.md
```

**Authority boundary:** S04 subtype. It defines an experiment, not a result.

**A-side use:** Experiment-design prompt only if preserved.

**B-side chosen use:** Yes for ExperimentId, hypothesis, baseline/enhanced change,
metric, tier, scale-up conditions, and failure classification rules.

**Rejected source:** Result verdict authored into plan, missing one mandatory field,
or Tier 2 failure treated as hypothesis refutation.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `experiment_id`, `hypothesis`, `baseline`, `enhanced`,
`metric`, `tier`, `scale_up_conditions`, `failure_classification_rule`, and
`plan_version`.

---

### llm_mvp_core Refinement Type W02 - Batch Experiment Plans

**Registry status:** `full_type`

**What it is:** Multi-experiment plan files grouping several experiments by theme,
wave, or dependency.

**Examples in the llm_mvp_core corpus:**

```text
E15-E16-E17-SEMANTIC-COMPILER-PLANS-v1.0.md
E28V2-E31V2-E44V2-E45V2-PROBE-NEURAL-NETWORK-PLANS.md
```

**Authority boundary:** S04 subtype with per-experiment inner records. Do not flatten
the batch into one experiment.

**A-side use:** No unless grouped experiment prompt is preserved.

**B-side chosen use:** Yes for per-experiment mandatory fields plus batch ordering
and grouping reason.

**Rejected source:** One experiment's metric applied to another, batch order ignored,
or grouped plan treated as one experiment id.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `batch_plan_path`, `experiment_ids`, `grouping_reason`,
`experiment_records`, `batch_order`, `dependency_edges`, and `plan_version`.

---

### llm_mvp_core Refinement Type W03 - Experiment Indexes

**Registry status:** `full_type`

**What it is:** Navigation index for all experiments, with method, key metric, plan
file, and recommended run order.

**Examples in the llm_mvp_core corpus:**

```text
EXPERIMENT-INDEX.md
```

**Authority boundary:** Navigation document. It routes to experiment plans; it does
not replace them.

**A-side use:** No.

**B-side chosen use:** Yes for plan lookup, recommended run order, experiment method,
and key metric lookup.

**Rejected source:** Index row used as full experiment spec, wrong plan file, or run
order fabricated.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `experiment_index_path`, `experiment_id`, `experiment_title`,
`method`, `key_metric`, `plan_file`, and `recommended_run_order`.

---

### llm_mvp_core Refinement Type W04 - Experiment Infrastructure Plans

**Registry status:** `full_type`

**What it is:** Plans for test runner and experiment infrastructure, not individual
experiments.

**Examples in the llm_mvp_core corpus:**

```text
34-EXPERIMENTS-INFRASTRUCTURE-PLAN-v1.md
35-EXPERIMENTS-V2-INFRASTRUCTURE-PLAN-v1.md
```

**Authority boundary:** S11/Type 29 infrastructure planning extension.

**A-side use:** No unless infrastructure task prompt is preserved.

**B-side chosen use:** Yes for experiment runner architecture, storage layout,
execution flow, and infrastructure dependencies.

**Rejected source:** Infrastructure plan treated as experiment result, runner omitted,
or individual experiment plan substituted for infrastructure plan.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `infrastructure_plan_path`, `infrastructure_version`,
`runner_components`, `storage_layout`, `execution_flow`, `dependency_map`, and
`applies_to_experiment_range`.

---

### llm_mvp_core Refinement Type W05 - Experiment Strategy Plans

**Registry status:** `full_type`

**What it is:** High-level strategy documents for experiment sequencing and grouping.

**Examples in the llm_mvp_core corpus:**

```text
E51-DUAL-TRACK-EXPERIMENT-STRATEGY-v1.md
E51A-TIER2-COVERAGE-FIRST-PLAN-v1.md
```

**Authority boundary:** Experiment strategy source. It is not a single experiment plan.

**A-side use:** Strategy question only if preserved.

**B-side chosen use:** Yes for track design, priority order, strategic rationale, and
comparison table.

**Rejected source:** Strategy treated as experiment result, one-track plan used when
dual-track required, or priority reversed.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `strategy_plan_path`, `strategy_id`, `track_ids`,
`priority_order`, `rationale`, `comparison_table`, and `affected_experiments`.

---

### llm_mvp_core Refinement Type W06 - Experiment Promotion Plans

**Registry status:** `full_type`

**What it is:** Plans for promoting successful experiments into next runs or
production components.

**Examples in the llm_mvp_core corpus:**

```text
PROMOTION-PLAN-E01-E02-E03-E05-v1.0.md
```

**Authority boundary:** S02/S04 bridge. It uses real run numbers to decide what to
run next.

**A-side use:** No unless promotion task prompt is preserved.

**B-side chosen use:** Yes for known numbers, next run, expected change, pass
criterion, and failure fallback.

**Rejected source:** Promotion without actual run numbers, wrong next experiment, or
no fallback for failed promotion.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `promotion_plan_path`, `source_experiment_ids`,
`current_measured_values`, `next_run_ids`, `expected_change`, `pass_criterion`,
and `if_not_passed_action`.

---

### llm_mvp_core Refinement Type W07 - Session State JSON

**Registry status:** `full_type`

**What it is:** llm_mvp_core session state JSON with sessionId, sessionType, goal,
steps, genieRules, fabricFirst, documentationRule, testRule, and solutionContext.

**Examples in the llm_mvp_core corpus:**

```text
SESSION-STATE-MATRIX-ALGORITHMS-2026-04-30.json
```

**Authority boundary:** Type 22 subtype for llm_mvp_core sessions.

**A-side use:** No.

**B-side chosen use:** Yes for session goal, step state, rules, fabric-first flag,
documentation/test rules, and solution context.

**Rejected source:** XIIGen IMPL-STATE schema applied, missing genieRules/fabricFirst,
or in-progress state used as completion.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `session_state_path`, `session_id`, `session_type`, `goal`,
`steps`, `genie_rules`, `fabric_first`, `documentation_rule`, `test_rule`, and
`solution_context`.

---

### llm_mvp_core Refinement Type W08 - Experiment Result Schemas

**Registry status:** `full_type`

**What it is:** JSON Schema definition for experiment result records.

**Examples in the llm_mvp_core corpus:**

```text
experiment-result-schema-v1.json
```

**Authority boundary:** Type 25 schema contract for experiment run results.

**A-side use:** No.

**B-side chosen use:** Yes for required fields, `$defs`, meta/dataset/baseline/improved
/delta/verdict sections, and schema validity.

**Rejected source:** Result JSON missing required schema section, schema confused with
sample run, or invalid verdict value.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `schema_path`, `schema_id`, `schema_version`,
`required_fields`, `defs`, `result_sections`, `valid_verdict_values`, and
`applies_to=W09`.

---

### llm_mvp_core Refinement Type W09 - Experiment Runtime State Files

**Registry status:** `full_type`

**What it is:** Sample experiment run result and aggregated tier-progress state.

**Examples in the llm_mvp_core corpus:**

```text
E01-sample-run-tier1.json
progress-sample.json
```

**Authority boundary:** Runtime state/result family. It is actual run/progress data,
not plan or schema.

**A-side use:** No.

**B-side chosen use:** Yes for per-run metrics, tier progression, aggregate progress,
and current experiment state.

**Rejected source:** Schema used as result, result claimed without metrics, or
progress aggregated from stale sample.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `runtime_state_path`, `experiment_id`, `tier`, `metrics`,
`verdict`, `progress_state`, `aggregate_counts`, and `schema_ref`.

---

### llm_mvp_core Refinement Type W10 - Solution E2E Parity HTML

**Registry status:** `preprocess_then_reclassify`

**What it is:** HTML solution parity comparison artifact.

**Examples in the llm_mvp_core corpus:**

```text
llm_solution_e2e_parity.html
```

**Authority boundary:** Rendered comparison artifact. Extract/strip HTML before use
and classify rows as comparison/validation evidence.

**A-side use:** No.

**B-side chosen use:** Only after extraction for parity comparison findings.

**Rejected source:** Raw HTML used as text pair, visual comparison not extracted, or
parity row used without source locator.

**Expected overlap ratio:** inherited after extraction.

**DTO requirements:** `html_path`, `extraction_method`, `comparison_rows`,
`parity_axis`, `source_locator`, and `target_registry_type`.

---

### llm_mvp_core Refinement Type W11 - Domain Subprojects Plans

**Registry status:** `full_type`

**What it is:** Plans grouping multiple subprojects within a domain and defining
shared fabric interfaces.

**Examples in the llm_mvp_core corpus:**

```text
COMPRESSION-ALGORITHMS-SUBPROJECTS-PLAN-v1.md
```

**Authority boundary:** Type 29 grouping extension.

**A-side use:** No unless domain-planning prompt is preserved.

**B-side chosen use:** Yes for domain grouping, shared fabric interfaces, subproject
dependency order, and domain-level implementation sequence.

**Rejected source:** Single-subproject plan used for domain group, shared fabric
omitted, or wrong domain grouping.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `domain_plan_path`, `domain_id`, `subproject_ids`,
`shared_fabric_interfaces`, `dependency_order`, `implementation_sequence`, and
`plan_version`.

---

### llm_mvp_core Refinement Type W12 - Capability Enhancement Plans

**Registry status:** `full_type`

**What it is:** Plans for enhancing existing capabilities/subprojects rather than
creating a new capability from scratch.

**Examples in the llm_mvp_core corpus:**

```text
CAPABILITY-ENHANCEMENT-PLAN-*.md
```

**Authority boundary:** S06 extension for enhancement mode.

**A-side use:** Enhancement prompt only if preserved.

**B-side chosen use:** Yes for existing capability baseline, enhancement target,
compatibility constraints, and regression gates.

**Rejected source:** Enhancement treated as new capability, baseline omitted, or
regression gate missing.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `enhancement_plan_path`, `capability_id`, `existing_baseline`,
`enhancement_target`, `compatibility_constraints`, `regression_gates`, and
`plan_version`.

---

### llm_mvp_core Refinement Type W13 - Multi-Track Strategy Plans

**Registry status:** `full_type`

**What it is:** Research/experiment plans comparing multiple tracks instead of one
shared model.

**Examples in the llm_mvp_core corpus:**

```text
RESEARCH-02-THREE-TRACKS-PLAN-v1.md
```

**Authority boundary:** S09/W05 bridge. It is scientific track selection plus
engineering strategy.

**A-side use:** Research strategy question only if preserved.

**B-side chosen use:** Yes for hypothesis, track design, rationale for separation,
and comparison table.

**Rejected source:** One shared model used when tracks are required, track rationale
omitted, or comparison table ignored.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `multi_track_plan_path`, `hypothesis`, `track_ids`,
`track_rationale`, `comparison_table`, `shared_model_rejected_reason`, and
`expected_outcome`.

---

### llm_mvp_core Refinement Type W14 - LLM Master Plan Version Delta

**Registry status:** `full_type`

**What it is:** Explicit evolution from v1 to v2 master implementation plans, where
v2 adds coding rules and architecture standards.

**Examples in the llm_mvp_core corpus:**

```text
LLM-MVP-CORE-SUBPROJECTS-IMPLEMENTATION-PLAN-v1.md
LLM-MVP-CORE-SUBPROJECTS-IMPLEMENTATION-PLAN-v2.md
```

**Authority boundary:** S10 Type 16 delta subtype.

**A-side use:** No.

**B-side chosen use:** Yes for changed rules, added standards, and supersession.

**Rejected source:** v1 coding rules treated as current, v2 additions omitted, or
version delta not recorded.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `from_version`, `to_version`, `added_coding_rules`,
`added_architecture_standards`, `removed_or_changed_sections`, and
`supersession_reason`.

---

### llm_mvp_core Refinement Type W15 - Experiment Failure Classification Rules

**Registry status:** `full_type`

**What it is:** The classification semantics PASSED / FAILED / SCALE_UP / ARCH / DATA
/ TOOL used across experiment plans and results.

**Examples in the llm_mvp_core corpus:**

```text
PASSED
FAILED
SCALE_UP
ARCH
DATA
TOOL
```

**Authority boundary:** Cross-cutting experiment classification rule. It should be
cited in experiment verdict pairs.

**A-side use:** No.

**B-side chosen use:** Yes for verdict classification, failure diagnosis, and correct
next action.

**Rejected source:** Tier failure misclassified as hypothesis failure, DATA issue
treated as ARCH issue, or SCALE_UP omitted.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `experiment_id`, `classification`, `classification_reason`,
`tier`, `metric_result`, `next_action`, and `rule_source_locator`.

---

### llm_mvp_core Refinement Type W16 - Gate D Capability Plan Format

**Registry status:** `full_type`

**What it is:** Mandatory Gate D structure inside capability session plans.

**Examples in the llm_mvp_core corpus:**

```text
Goal statement verbatim
Before state
After state
Hard gates
Pre-condition check
Loading sequence
Round Contract
Gate B
```

**Authority boundary:** S06 structural completeness subtype.

**A-side use:** No.

**B-side chosen use:** Yes for mandatory sections, self-contained plan requirement,
and per-phase Gate B.

**Rejected source:** Capability session plan missing any mandatory section, external
cross-reference required, or Gate B omitted.

**Expected overlap ratio:** 0.15-0.35.

**DTO requirements:** `capability_plan_path`, `goal_statement_verbatim`,
`before_state`, `after_state`, `hard_gates`, `precondition_check`,
`loading_sequence`, `round_contract`, `gate_b_checks`, and `self_contained=true`.

---

## Genie Skill Library v6 Package Families

The `X01-X16` families come from `genie-skill-library-v6-FINAL.zip`, a third
project line distinct from both XIIGen and llm_mvp_core. It documents the Genie
Platform skill library across PHP WordPress, .NET, Elasticsearch, React,
Angular, React Native, Node.js, Python, Rust, and related stacks. These sources
are valuable for cross-stack pattern and template pairs, but they must record
`project_id=genie_skill_library` and must not be treated as XIIGen-specific
authority unless an explicit adoption or translation document says so.

---

### Genie Skill Library Type X01 - Technology-Specific Alternative Implementations

**Registry status:** `full_type`

**What it is:** `alternatives/[stack].md` files under each cross-cutting skill.
Each file expresses the same skill principle in one concrete technology stack.

**Examples in the Genie corpus:**

```text
skills/cross-cutting/*/alternatives/dotnet-modern.md
skills/cross-cutting/*/alternatives/nodejs.md
skills/cross-cutting/*/alternatives/python.md
skills/cross-cutting/*/alternatives/rust.md
skills/cross-cutting/*/alternatives/php.md
skills/cross-cutting/*/alternatives/angular.md
skills/cross-cutting/*/alternatives/react-modern.md
skills/cross-cutting/*/alternatives/react-native.md
```

**Authority boundary:** B-side authority for technology-specific implementation
of a Genie pattern. It is not general authority for every project using that
technology.

**A-side use:** Only when the user explicitly asks for a stack-specific port or
implementation.

**B-side chosen use:** Correct stack-specific code and explanation for the named
skill pattern.

**Rejected source:** Wrong stack, generic implementation that ignores the skill
principle, or technology code that contradicts the stack file.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `project_id=genie_skill_library`, `skill_id`, `pattern_id`,
`stack_id`, `alternative_path`, `core_principle`, `code_language`, and
`supported_runtime`.

---

### Genie Skill Library Type X02 - Full Code Reference Extracts

**Registry status:** `full_type`

**What it is:** `references/*-full.md` files containing full extracted
production code from original projects, not simplified examples.

**Examples in the Genie corpus:**

```text
auth-jwt/references/login-controller-full.md
auth-jwt/references/startup-jwt-config.md
dynamic-documents/references/dynamic-controller-full.md
dynamic-documents/references/object-process-full.md
```

**Authority boundary:** Type 26 subtype. These extracts are exact code
references for API, wiring, and method-shape pairs.

**A-side use:** No.

**B-side chosen use:** Exact API surface, controller shape, startup wiring, or
dynamic document behavior from the extract.

**Rejected source:** Simplified example presented as full production extract, or
missing method/route/signature compared with the extract.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `reference_path`, `component_name`, `source_project`,
`language`, `exported_classes_or_functions`, `route_or_method_ids`, and
`is_full_extract=true`.

---

### Genie Skill Library Type X03 - Stack Integration Recipes

**Registry status:** `full_type`

**What it is:** `[server]+[client].md` recipe files that wire several Genie
patterns together for one server/client stack combination.

**Examples in the Genie corpus:**

```text
dotnet-react.md
nodejs-angular.md
php-react.md
python-react.md
rust-angular.md
```

**Authority boundary:** Stack-selection and integration recipe authority for the
named server/client combination.

**A-side use:** Stack-selection prompts and "which recipe should I use" prompts.

**B-side chosen use:** Correct when-to-use rationale, project structure, server
wiring, client wiring, docker-compose expectations, and shared interface rules.

**Rejected source:** Wrong stack combo, missing shared interfaces, or using an
alternative implementation file as if it were a full integration recipe.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `recipe_path`, `server_stack`, `client_stack`,
`when_to_use`, `project_structure_sections`, `server_wiring_sections`,
`client_wiring_sections`, and `included_patterns`.

---

### Genie Skill Library Type X04 - AI Tool Project Configuration Files

**Registry status:** `full_type`

**What it is:** Persistent project rules for specific AI tools.

**Examples in the Genie corpus:**

```text
.cursorrules
.clinerules
CLAUDE.md
copilot-instructions.md
```

**Authority boundary:** Type 19 extension. Each file is canonical only for its
own tool and project.

**A-side use:** No.

**B-side chosen use:** Correct tool-specific project identity, navigation rules,
coding constraints, and pattern enforcement rules.

**Rejected source:** Rules from one AI tool applied to another without format
translation, or one-session prompt rules used as persistent project config.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `tool_name`, `config_path`, `project_id`,
`identity_section_present`, `navigation_rules`, `coding_constraints`, and
`pattern_enforcement_rules`.

---

### Genie Skill Library Type X05 - Decision Philosophy Guides

**Registry status:** `full_type`

**What it is:** `DECISION_GUIDE.md`, a philosophy and abstraction-ladder guide
for Genie dynamic design decisions.

**Authority boundary:** Type 9 extension for decision philosophy, not a locked
decision register.

**A-side use:** Architectural decision prompts about dynamic vs hardcoded
behavior.

**B-side chosen use:** Correct application of the one rule, abstraction ladder,
and dynamic pattern decision criteria.

**Rejected source:** Hardcoded choice where the ladder requires dynamic behavior,
or a dynamic abstraction when the guide says the value is fixed infrastructure.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `guide_path`, `one_rule`, `ladder_level`,
`decision_criterion`, `pattern_id`, and `dynamic_or_hardcoded_verdict`.

---

### Genie Skill Library Type X06 - Stack Migration Methodology Guides

**Registry status:** `full_type`

**What it is:** `MIGRATION_PATHS.md`, a reusable method for migrating Genie
patterns between stacks.

**Authority boundary:** Migration methodology, not a one-off migration plan.

**A-side use:** Prompts that ask how to migrate a Genie implementation from one
stack to another.

**B-side chosen use:** Correct audit-target-recipe-port sequence and correct
classification of what stays stable versus what changes.

**Rejected source:** Direct code translation without audit, wrong target recipe,
or changing invariant concepts such as API routes or JWT claims when the guide
says they stay stable.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `source_stack`, `target_stack`, `migration_step`,
`stable_concepts`, `changed_concepts`, `worked_example_id`, and
`recipe_reference`.

---

### Genie Skill Library Type X07 - Design Pattern Catalogs Mapped To Code

**Registry status:** `full_type`

**What it is:** `PATTERNS_IN_GENIE_DNA.md`, mapping GoF design patterns to
actual Genie code and simulation blueprints.

**Authority boundary:** Architectural pattern-application authority for Genie
DNA examples.

**A-side use:** Prompts asking which design pattern applies to a Genie behavior.

**B-side chosen use:** Correct pattern name, Genie use, concrete file reference,
and simulation blueprint.

**Rejected source:** Pattern name without code evidence, wrong GoF pattern, or
pattern used in the wrong Genie context.

**Expected overlap ratio:** 0.15-0.35.

**DTO requirements:** `pattern_name`, `genie_use`, `code_file_reference`,
`simulation_blueprint_id`, and `evidence_locator`.

---

### Genie Skill Library Type X08 - ML Integration Simulation Blueprints

**Registry status:** `full_type`

**What it is:** `ML_SIMULATIONS.md`, containing ML simulation blueprints such as
Naive Bayes, Linear Regression, Q-Learning, K-Means, and neural prediction.

**Authority boundary:** Blueprint authority for adding ML behavior to Genie
patterns. It is not experiment-result evidence.

**A-side use:** ML integration design prompts.

**B-side chosen use:** Correct algorithm, what-it-does statement, Genie DNA
connection, and implementation blueprint.

**Rejected source:** Treating a blueprint as measured performance evidence, or
using the wrong algorithm for the described Genie behavior.

**Expected overlap ratio:** 0.15-0.35.

**DTO requirements:** `blueprint_id`, `algorithm_name`, `what_it_does`,
`genie_dna_connection`, `implementation_steps`, and `measurement_available=false`.

---

### Genie Skill Library Type X09 - Platform Scale Evolution Maps

**Registry status:** `full_type`

**What it is:** `PLATFORM_SCALE.md`, mapping Genie patterns across scale levels
from Gen1 to XIIGen.

**Authority boundary:** Evolution map authority. It explains what stays the same,
what changes, and what new abstraction appears at each level.

**A-side use:** Scale/evolution prompts.

**B-side chosen use:** Correct scale level, stable pattern, changed element, and
new abstraction.

**Rejected source:** Claiming the same implementation works unchanged at a higher
scale level when the map introduces a new abstraction.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `scale_level`, `pattern_id`, `stays_same`,
`changes`, `new_abstraction`, and `next_level`.

---

### Genie Skill Library Type X10 - RAG Reference Indexes

**Registry status:** `full_type`

**What it is:** `RAG_INDEX.md`, a quick lookup from pattern and project context
to files.

**Authority boundary:** Navigation authority only. It points to sources; it does
not replace the referenced sources.

**A-side use:** Lookup prompts asking where a Genie pattern is documented.

**B-side chosen use:** Correct pattern-to-file or project-to-file lookup.

**Rejected source:** Using the index text as the pattern source, wrong target
file, or fabricated file path.

**Expected overlap ratio:** 0.05-0.15.

**DTO requirements:** `index_path`, `lookup_key`, `lookup_mode`,
`target_file_path`, `target_section`, and `index_only=true`.

---

### Genie Skill Library Type X11 - Use-Case Template Selection Guides

**Registry status:** `full_type`

**What it is:** `templates/use-cases/*.md` files for selecting a starting point
for a scenario.

**Examples in the Genie corpus:**

```text
templates/use-cases/dynamic-crm.md
templates/use-cases/gamification.md
templates/use-cases/headless-api.md
templates/use-cases/task-management.md
```

**Authority boundary:** Use-case selection and customization-point authority, not
full implementation authority.

**A-side use:** User asks which template/recipe/skills to start from for a use
case.

**B-side chosen use:** Correct recommended recipe, skill list, and customization
points.

**Rejected source:** Wrong scenario, wrong recipe, or presenting the guide as a
complete implementation plan.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `use_case_id`, `scenario_path`, `recommended_recipe`,
`skills_to_load`, `customization_points`, and `non_goals`.

---

### Genie Skill Library Type X12 - Boilerplate Starter Templates

**Registry status:** `full_type`

**What it is:** Production-ready starter code under `templates/servers` and
`templates/clients`.

**Authority boundary:** Scaffold authority for the named stack and template kind.
It is source code template evidence, not prose guidance.

**A-side use:** Scaffold-generation prompts.

**B-side chosen use:** Correct starter file shape, language idioms, framework
wiring, and required placeholders.

**Rejected source:** Explanation-only alternative used as scaffold, missing
required placeholders, or wrong client/server template kind.

**Expected overlap ratio:** 0.15-0.35.

**DTO requirements:** `template_path`, `template_kind`, `stack_id`, `language`,
`framework`, `entry_points`, `required_placeholders`, and `is_boilerplate=true`.

---

### Genie Skill Library Type X13 - Infrastructure Template Files

**Registry status:** `limited_authority`

**What it is:** Infrastructure templates such as docker-compose and env example
files under `templates/infra`.

**Authority boundary:** Template authority for local/dev scaffold wiring only
unless promoted by a deployment document. Secrets must remain placeholders.

**A-side use:** No, except explicit scaffold prompts.

**B-side chosen use:** Correct service names, environment placeholders, networks,
ports, and dependency ordering for the template.

**Rejected source:** Actual secrets, production claim without deployment proof, or
omitting required placeholder variables.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `template_path`, `template_kind`, `services`,
`env_placeholders`, `networks`, `volumes`, `secret_free=true`, and
`deployment_scope`.

---

### Genie Skill Library Type X14 - Unified Library Entry Points

**Registry status:** `full_type`

**What it is:** `README-UNIFIED.md`, a full entry point with orientation, core
philosophy, skill summaries, and persona-based reading order.

**Authority boundary:** Orientation and reading-order authority. It is not the
canonical source for detailed skill behavior when a skill-specific file exists.

**A-side use:** Navigation prompts and "where should I start" prompts.

**B-side chosen use:** Correct persona reading order, high-level library map, and
skill summary.

**Rejected source:** Using the README summary instead of the detailed source for
specific implementation behavior.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `entrypoint_path`, `persona`, `reading_order`,
`skill_summary_ids`, `orientation_section`, and `detailed_source_required_when`.

---

### Genie Skill Library Type X15 - Skill Library Master Plans

**Registry status:** `full_type`

**What it is:** `MASTER_PLAN.md`, describing the overall Genie skill library
purpose, project set, phase breakdown, and validation checklist.

**Authority boundary:** Skill-library production plan, not runtime architecture
and not individual skill authority.

**A-side use:** Prompts asking what the skill library is meant to contain or how
its build phases are organized.

**B-side chosen use:** Correct project summary, phase breakdown, and validation
checklist.

**Rejected source:** Treating the master plan as a complete implementation guide
or ignoring its validation checklist.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `master_plan_path`, `project_count`, `project_summary`,
`phase_ids`, `validation_checklist`, and `positive_examples`.

---

### Genie Skill Library Type X16 - Genie Skill Library Package Boundary

**Registry status:** `archive_container`

**What it is:** The `genie-skill-library-v6-FINAL.zip` package as a project
boundary and canonical package anchor.

**Authority boundary:** Archive-level provenance only. Pair authority comes from
the extracted member family X01-X15 or existing numbered source types.

**A-side use:** No.

**B-side chosen use:** Archive/member provenance, project boundary, and
canonicality anchor for Genie package sources.

**Rejected source:** Using the ZIP filename as source authority, or treating Genie
content as XIIGen authority without explicit adoption.

**Expected overlap ratio:** inherited from extracted member type.

**DTO requirements:** `archive_path`, `archive_id=genie-skill-library-v6-FINAL.zip`,
`project_id=genie_skill_library`, `member_path`, `member_family`, `extraction_path`,
and `source_project_boundary_confirmed=true`.

---

## Module Architecture And Translation Pipeline Families

The `Y01-Y10` families come from two early V17/V18-era archives:
`module-architecture.zip` and `phases_done.zip`. `Y01-Y05` define how V17 skills
compose into generic modules and how those modules compose into site types.
`Y06-Y10` define the C#/.NET to Python/React Native translation pipeline,
including translated content, part-level audit state, master state, and final
numbering authority. They are not ordinary skill files: they are composition and
stack-translation authorities.

---

### Module Architecture Type Y01 - Capability Matrix Analysis Documents

**Registry status:** `full_type`

**What it is:** `GENERIC_MODULE_ANALYSIS.md` and
`GENERIC_MODULE_ANALYSIS_COMPLETE.md`, the Phase 1A draft and Phase 1B complete
20-module by 15-site-type capability matrix.

**Authority boundary:** Product-level module decomposition authority. Phase 1B is
canonical; Phase 1A is stale draft/version evidence.

**A-side use:** Site/module selection prompts.

**B-side chosen use:** Correct site type to module requirement mapping and config
knob expectations.

**Rejected source:** Site-specific controller approach, wrong module list for a
site type, or stale Phase 1A matrix used as current.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `archive_id=module-architecture.zip`, `phase_id`,
`matrix_version`, `site_type`, `module_id`, `module_required`, `config_knobs`,
and `canonical_phase=1B`.

---

### Module Architecture Type Y02 - Module Architecture Reference Documents

**Registry status:** `full_type`

**What it is:** `DEFINITIVE_MODULE_ARCHITECTURE.md`, the Phase 1C reference for
generic module implementation.

**Authority boundary:** Architecture reference for module implementation
patterns, Elasticsearch indices, cross-module dependencies, shared registries,
style maps, and dynamic APIs.

**A-side use:** Architecture prompts about how a generic module works.

**B-side chosen use:** Correct module pattern, index name, code example, and
integration dependency.

**Rejected source:** Typed site-specific controller, missing control registry, or
wrong ES index for the module.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `module_id`, `index_name`, `design_pattern`,
`code_example_language`, `integration_dependencies`, `shared_registry_ids`, and
`dynamic_api_ids`.

---

### Module Architecture Type Y03 - Complete AI Agent Implementation Guides

**Registry status:** `full_type`

**What it is:** `COMPLETE_AI_AGENT_GUIDE.md` plus stale draft
`AI_AGENT_GUIDE_SKILL_MAPPING.md`, describing how agents use V17 skills to build
generic modules and compose them into site types.

**Authority boundary:** End-to-end AI agent composition authority for V17 skills
to modules to site-type deployment manifests.

**A-side use:** Agent implementation prompts for building a module or site type.

**B-side chosen use:** Correct one rule, four laws, skill evolution mapping,
module-to-skill mapping, per-module instructions, deployment manifest, and
anti-pattern.

**Rejected source:** Site-specific controller, omitted config index, wrong skill
sequence, or stale draft used as final guide.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `guide_version`, `module_id`, `site_type`,
`primary_skill_ids`, `support_skill_ids`, `config_index`, `deployment_manifest_id`,
`anti_pattern_id`, and `canonical_file=COMPLETE_AI_AGENT_GUIDE.md`.

---

### Module Architecture Type Y04 - V18 Consolidation Session Prompts

**Registry status:** `full_type`

**What it is:** `CONSOLIDATION_PROMPT.md`, a prompt for merging V17 skills and
module architecture artifacts into a V18 composition package.

**Authority boundary:** Consolidation procedure authority, not implementation
authority for modules themselves.

**A-side use:** Consolidation run prompts.

**B-side chosen use:** Correct Phase E1-E5 sequence, produced file structure,
deduplication rule, and save point.

**Rejected source:** Modifying the 62 V17 skills when the prompt says to preserve
them, or failing to archive superseded draft files.

**Expected overlap ratio:** 0.15-0.30.

**DTO requirements:** `consolidation_phase`, `input_archives`, `output_tree`,
`deduplication_rules`, `save_point`, and `preserve_v17_skills=true`.

---

### Module Architecture Type Y05 - Prompt Usage Guides

**Registry status:** `full_type`

**What it is:** `HOW_TO_USE_CONSOLIDATION_PROMPT.md`, a human-operator guide for
running the consolidation prompt.

**Authority boundary:** Operator execution guide. It tells what to attach, what
to paste, and what output to expect.

**A-side use:** Human workflow prompts about starting a consolidation session.

**B-side chosen use:** Correct attachment set, step order, output tree, and
changed-vs-unchanged table.

**Rejected source:** Session continuation guide used as prompt usage guide, wrong
ZIP attachment set, or missing expected output tree.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `usage_guide_path`, `required_attachments`, `execution_steps`,
`expected_output_tree`, `changes_table`, and `operator_context`.

---

### Module Architecture Type Y06 - Stack Translation Execution Phase Documents

**Registry status:** `full_type`

**What it is:** Phase execution documents from `phases_done.zip`, such as
`PHASE1_TECH_STACK_MAPPING.md`, `PHASE4A` through `PHASE8E`, and related phase
outputs that contain the actual translated Python/React Native patterns.

**Authority boundary:** Produced translation content authority. This is stronger
than a translation plan because it contains the translated code, tests, and
pattern-specific output.

**A-side use:** Stack translation prompts asking how a C#/.NET pattern should
look in Python/React Native.

**B-side chosen use:** Correct Python/React Native code, test expectations,
stack mapping, and DNA/skill/factory translation behavior.

**Rejected source:** C# original used as Python canonical, typed Pydantic model
where dynamic dict behavior is required, or translation plan used as produced
translated content.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `archive_id=phases_done.zip`, `phase_id`, `phase_name`,
`stack_target`, `depends_on`, `translated_artifact_ids`, `test_count`,
`code_language`, and `source_pattern_id`.

---

### Module Architecture Type Y07 - Translated Merged Document Parts

**Registry status:** `full_type`

**What it is:** `SS/MEP/USI/BFA/SKF/TTC/ENG *_PY_PARTn.md` part files,
translated from seven source merged documents into Python-stack language.

**Authority boundary:** Canonical Python-stack source-of-truth parts for the
specific line ranges they cover. The original C# merged documents are stale for
Python-stack pairs.

**A-side use:** Prompts asking for a Python-stack version of a specific merged
document pattern.

**B-side chosen use:** Correct translated text/code from the matching part and
line range, with verification command satisfied.

**Rejected source:** Wrong part, stale C# source used as chosen, or unverified
part with remaining C# syntax hits.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `source_merged_doc`, `part_id`, `line_start`, `line_end`,
`output_part_path`, `csharp_hits_after`, `verification_command`,
`expected_verification_count=0`, and `python_canonical=true`.

---

### Module Architecture Type Y08 - Per-Sub-Phase Translation State Files

**Registry status:** `full_type`

**What it is:** `STATE_P*.json` files that audit one translation sub-phase at a
time.

**Authority boundary:** Machine-readable audit trail for translation chunks:
source range, output file, substitutions, manual edits, before/after C# hit
counts, verification, next phase, and recovery instructions.

**A-side use:** No.

**B-side chosen use:** Correct status, source/output mapping, verification count,
and recovery path for a specific sub-phase.

**Rejected source:** Completion claim without state evidence, wrong line range,
or stale state used after a later master/final state exists.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `state_file`, `phase_id`, `parent_doc`, `input_source_file`,
`line_start`, `line_end`, `output_file`, `lines_produced`,
`csharp_hits_before`, `csharp_hits_after`, `substitutions_made`,
`verification_command`, `next_phase`, and `recovery_instructions`.

---

### Module Architecture Type Y09 - Translation Pipeline Master State

**Registry status:** `full_type`

**What it is:** `MASTER_STATE.json`, the pipeline-level state for the translation
run.

**Authority boundary:** Authoritative lifecycle state for which sub-phases are
complete and what remains next.

**A-side use:** No.

**B-side chosen use:** Correct pipeline completion count, phase status, next
sub-phase, and recovery instruction.

**Rejected source:** Per-sub-phase state used as fleet pipeline state, wrong
completion count, or old master state after final declaration.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `master_state_path`, `completed_subphase_count`,
`total_subphase_count`, `phases`, `next_to_execute`, `blocked_items`, and
`recovery_instructions`.

---

### Module Architecture Type Y10 - Translation Completion Declarations

**Registry status:** `full_type`

**What it is:** `PHASE_STATE_FINAL.json`, the final declaration for the
translation pipeline, including stack choices, artifact ranges, numbering
correction, next available IDs, loading order, and recovery instructions.

**Authority boundary:** Canonical final numbering and artifact baseline for the
Python/React Native translation state.

**A-side use:** Numbering and baseline prompts about the translated corpus.

**B-side chosen use:** Correct stack tuple, artifact ranges, numbering
correction, next available factory/task/skill IDs, and session loading order.

**Rejected source:** SK-330-SK-352 used as final Python library numbers when the
final declaration corrects them to SK-379-SK-401, or next IDs reported without
the correction context.

**Expected overlap ratio:** 0.05-0.15.

**DTO requirements:** `final_state_path`, `stack_choices`, `artifact_ranges`,
`numbering_correction`, `next_available_ids`, `session_loading_order`,
`recovery_instructions`, and `canonical_translation_final=true`.

---

## XIIGen V18 Composition Layer Families

The `AA01-AA17` families come from `xiigen_v18_complete.zip`: V17's 63-skill
library plus the V18 `genie-dna/` composition layer. Some member files confirm
families already defined elsewhere (`DOC_RAG_INDEX.md` -> X10, AI tool configs ->
X04, broad alternatives -> X01/X12/X13). The `AA` families capture the V18-only
composition-layer and per-skill production artifacts that need their own DTO
fields.

---

### XIIGen V18 Type AA01 - Genie DNA Architectural Law Documents

**Registry status:** `full_type`

**What it is:** `GENIE_DNA_RULES.md`, the V18 non-negotiable laws document.

**Authority boundary:** Architectural law authority: one rule, four-law
checklist, core DNA patterns, three-layer architecture, and MACHINE/FREEDOM
classification.

**A-side use:** Compliance and architectural-rule prompts.

**B-side chosen use:** Correct law/pattern classification and violation verdict.

**Rejected source:** Code that violates a law, site-specific implementation where
generic composition is required, or treating a locked decision register as the
law document.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `archive_id=xiigen_v18_complete.zip`, `law_id`,
`law_text`, `dna_pattern_id`, `layer_id`, `machine_freedom_verdict`, and
`violation_example`.

---

### XIIGen V18 Type AA02 - V18 Module Architecture References

**Registry status:** `full_type`

**What it is:** V18 `MODULE_ARCHITECTURE.md`, a 20-module reference with
Elasticsearch indices, primary/support skills, and integration map.

**Authority boundary:** V18 composition-layer module reference. It supersedes
earlier draft/module-architecture variants for V18 package questions.

**A-side use:** Module architecture prompts.

**B-side chosen use:** Correct module, ES index, primary/support skill, and
cross-module integration edge.

**Rejected source:** Wrong index, missing support skills, or older module doc used
as V18 current.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `module_id`, `module_name`, `es_index`, `primary_skill_ids`,
`support_skill_ids`, `integration_edges`, and `v18_current=true`.

---

### XIIGen V18 Type AA03 - V18 Capability Matrices

**Registry status:** `full_type`

**What it is:** V18 `CAPABILITY_MATRIX.md`, mapping modules to site types and
enabled counts.

**Authority boundary:** Current V18 site-type/module selection authority.

**A-side use:** Site-type capability prompts.

**B-side chosen use:** Correct enabled module list and zero-code extension rule.

**Rejected source:** New site type implemented with custom code instead of config
documents, or wrong module enabled state.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `site_type`, `module_id`, `enabled`, `enabled_count`,
`config_required`, and `zero_code_extension_rule=true`.

---

### XIIGen V18 Type AA04 - Skill Composition Guides

**Registry status:** `full_type`

**What it is:** `SKILL_COMPOSITION_GUIDE.md`, mapping modules to primary skills,
support skills, config indices, agent instructions, anti-patterns, and decision
tree.

**Authority boundary:** AI-agent skill composition authority for V18 modules.

**A-side use:** Agent implementation prompts.

**B-side chosen use:** Correct module-to-skill sequence and anti-pattern
avoidance.

**Rejected source:** Skill index used as composition guide, missing config index,
or anti-pattern accepted as implementation.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `module_id`, `primary_skill_ids`, `support_skill_ids`,
`config_index`, `agent_instruction_id`, `anti_pattern_id`, and `decision_tree_path`.

---

### XIIGen V18 Type AA05 - Site-Type Deployment Manifest Documents

**Registry status:** `full_type`

**What it is:** `DEPLOYMENT_MANIFESTS.md`, with YAML-style manifests for 15 site
types plus execution checklist and extension packs.

**Authority boundary:** Site-type deployment recipe authority.

**A-side use:** Deployment recipe prompts.

**B-side chosen use:** Correct skills to deploy, config indices to create,
site_settings example, checklist step, and extension pack.

**Rejected source:** Capability matrix used as deployment recipe, missing config
index creation, or wrong site-type manifest.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `site_type`, `manifest_id`, `skills_to_deploy`,
`config_indices`, `site_settings_fields`, `checklist_steps`, and `extension_packs`.

---

### XIIGen V18 Type AA06 - AI Agent Quick Start Cheat Sheets

**Registry status:** `full_type`

**What it is:** `AGENT_QUICKSTART.md`, a compact reading/checklist document for
AI coding assistants.

**Authority boundary:** Compact assistant onboarding authority. It is not the
full source for detailed module behavior.

**A-side use:** Short assistant-start prompts.

**B-side chosen use:** Correct pre-code checks, law table, service lookup, and
decision tree.

**Rejected source:** Quickstart summary used as detailed implementation authority,
or skipping pre-code law checks.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `quickstart_path`, `pre_code_checks`, `law_table_ids`,
`service_lookup_key`, `decision_tree_path`, and `detailed_source_required_when`.

---

### XIIGen V18 Type AA07 - Per-Site-Type Configuration Examples

**Registry status:** `full_type`

**What it is:** Populated JSON examples under `genie-dna/site-configs/[site-type]`.

**Authority boundary:** Example config authority for a site type. These are not
JSON Schema definitions.

**A-side use:** Config-generation prompts for a named site type.

**B-side chosen use:** Correct populated JSON fields such as `settingsId`,
`scopeId`, `siteType`, `modules_enabled`, and theme.

**Rejected source:** Schema used as populated config, wrong site type, or MACHINE
keys presented as tenant FREEDOM settings.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `site_type`, `config_path`, `settings_id`, `scope_id`,
`modules_enabled`, `theme_fields`, and `is_populated_example=true`.

---

### XIIGen V18 Type AA08 - Per-Skill Language Alternative Implementations

**Registry status:** `full_type`

**What it is:** Alternatives under each skill for Python, Node.js, Java, PHP,
Rust, frontend frameworks, deployment platforms, and RAG providers.

**Authority boundary:** V18 per-skill alternative authority for the named skill,
language/framework/platform/provider.

**A-side use:** Stack/language port prompts for a concrete skill.

**B-side chosen use:** Correct idiomatic implementation for the skill and target
language/framework/provider.

**Rejected source:** Generic X01 pattern alternative used as a per-skill
implementation, wrong language, or wrong provider.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `skill_id`, `alternative_kind`, `target_stack`,
`provider_id`, `file_path`, `language_or_framework`, and `canonical_for_skill=true`.

---

### XIIGen V18 Type AA09 - Skill Implementation Prompt Files

**Registry status:** `full_type`

**What it is:** `prompts/implement.md` files, one per skill, with prerequisites,
ordered implementation steps, and testing instructions.

**Authority boundary:** Reproduction/implementation procedure authority for the
skill. It is not the skill definition itself.

**A-side use:** Prompts asking how to implement or reproduce a skill.

**B-side chosen use:** Correct prerequisite, step order, and testing procedure.

**Rejected source:** SKILL.md description used as implementation order, skipped
prerequisite, or tests omitted.

**Expected overlap ratio:** 0.15-0.35.

**DTO requirements:** `skill_id`, `prompt_path`, `prerequisites`, `ordered_steps`,
`testing_steps`, and `implementation_order_locked=true`.

---

### XIIGen V18 Type AA10 - Flow Definition JSON Files

**Registry status:** `full_type`

**What it is:** `flow-definitions/[NN]-[name]-flow.json` files describing
machine-readable DAG definitions.

**Authority boundary:** Concrete flow instance definition authority. It is not
the flow registry, task type catalog, or prose plan.

**A-side use:** Flow-definition construction prompts.

**B-side chosen use:** Correct `flowId`, version, metadata, events published and
consumed, skills used, nodes, and edges.

**Rejected source:** Flow registry entry used as full DAG, missing events, or
edge/node mismatch.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `flow_id`, `flow_version`, `uml_source`, `lifelines`,
`messages`, `events_published`, `events_consumed`, `skills_used`, `nodes`, and
`edges`.

---

### XIIGen V18 Type AA11 - Versioned Skill Indexes With Module Mapping

**Registry status:** `full_type`

**What it is:** `INDEX.md` and `INDEX_V18.md`, versioned skill indexes where V18
adds module mapping.

**Authority boundary:** Skill inventory and module mapping authority, not skill
activation trigger authority.

**A-side use:** Skill lookup prompts.

**B-side chosen use:** Correct skill purpose, key files, dependencies,
alternatives, and V18 module mapping.

**Rejected source:** HOW-TO-USE skill trigger table used as index, or V17 index
used when module mapping is required.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `index_version`, `skill_id`, `purpose`, `key_files`,
`dependencies`, `alternatives`, `module_ids`, and `is_v18_module_mapped`.

---

### XIIGen V18 Type AA12 - Alternatives Catalog Documents

**Registry status:** `full_type`

**What it is:** `ALTERNATIVES.md`, a catalog and selection guide across
alternative categories.

**Authority boundary:** Alternative-selection authority. Individual alternative
files remain implementation authority.

**A-side use:** Prompts asking which alternative/provider/platform to choose.

**B-side chosen use:** Correct option selection by context and category.

**Rejected source:** Catalog used as implementation source, wrong category, or
prototype option selected for production contrary to the guide.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `catalog_version`, `category`, `option_id`,
`selection_context`, `recommended_option`, and `implementation_source_path`.

---

### XIIGen V18 Type AA13 - Skill Authoring Templates

**Registry status:** `full_type`

**What it is:** `SKILL_FACTORY.md`, a placeholder-based template for authoring
new skills, DI registration, and composition layer entries.

**Authority boundary:** Skill authoring/scaffolding authority.

**A-side use:** Skill creation prompts.

**B-side chosen use:** Correctly filled placeholders, required SKILL.md sections,
DI registration, and composition entry.

**Rejected source:** Missing placeholder, wrong DI shape, or skill created without
composition layer section.

**Expected overlap ratio:** 0.15-0.35.

**DTO requirements:** `template_path`, `placeholder_ids`, `required_sections`,
`di_registration_template`, `composition_section_template`, and `filled_correctly`.

---

### XIIGen V18 Type AA14 - Version Consolidation State Documents

**Registry status:** `full_type`

**What it is:** `CONSOLIDATION_STATE.md`, the V17-to-V18 delta state document.

**Authority boundary:** Package-level version delta authority, not individual
document evolution.

**A-side use:** Version-delta prompts.

**B-side chosen use:** Correct added/not-changed/archived file status and purpose.

**Rejected source:** Treating a changed package file as unchanged, or missing
archive status for superseded files.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `from_version`, `to_version`, `added_files`,
`not_changed_files`, `archived_files`, `line_counts`, and `purpose_by_file`.

---

### XIIGen V18 Type AA15 - Mid-Phase Checkpoints And Quality Variants

**Registry status:** `full_type`

**What it is:** `PHASE_Dx_CHECKPOINT.md` and `PHASE_Xx_REFRESHED/ENHANCED/VALIDATED`
quality annotation variants.

**Authority boundary:** Type 22 subtype for interrupted or quality-annotated
phase state.

**A-side use:** No.

**B-side chosen use:** Correct mid-phase status, pending/completed file table, and
quality annotation state.

**Rejected source:** Mid-phase checkpoint used as final complete state, or quality
variant treated as raw implementation source.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `phase_id`, `checkpoint_kind`, `completed_files`,
`pending_files`, `to_complete`, `quality_variant`, and `is_final=false`.

---

### XIIGen V18 Type AA16 - RAG Provider Alternative Skill Files

**Registry status:** `limited_authority`

**What it is:** Embedded `SKILL.md` files under RAG provider alternatives such as
Neo4j, Pinecone, Cosmos DB, Azure AI Search, and Elasticsearch KNN.

**Authority boundary:** Type 20 subtype for provider-specific RAG alternative
behavior. It does not replace the core RAG skill.

**A-side use:** Provider-selection or provider-implementation prompts.

**B-side chosen use:** Correct provider-specific capability, wiring, and
constraints.

**Rejected source:** Provider-specific SKILL.md used as generic RAG authority, or
wrong provider selected for required capability.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `core_skill_id`, `provider_id`, `provider_skill_path`,
`provider_capabilities`, `provider_constraints`, and `core_skill_reference`.

---

### XIIGen V18 Type AA17 - Confirmed V18 Alias Families

**Registry status:** `limited_authority`

**What it is:** V18 members that confirm already-defined families:
`DOC_RAG_INDEX.md` confirms X10, AI tool configs confirm X04, broad
alternatives confirm X01/X12/X13, and config schema READMEs reclassify to B11 or
Type 25 as appropriate.

**Authority boundary:** Provenance and alias mapping only.

**A-side use:** No.

**B-side chosen use:** Correct alias target and duplicate-family reconciliation.

**Rejected source:** Creating a new family when an existing X/Y/Type target
already covers the artifact.

**Expected overlap ratio:** inherited from target family.

**DTO requirements:** `member_path`, `alias_target_family`, `reason_for_alias`,
`duplicate_of`, and `do_not_create_new_family=true`.

---

## V18 Canonical Migration And Architect Skill Families

The `AB01-AB07` families come from `XIIGEN_NESTJS_REACT_TRANSLATION_PLAN.zip`,
`xiigen-architect-skill.zip`, and the canonical `xiigen_v18_complete.zip`
package. They establish canonical anchors: the v4 FINAL NestJS migration plan,
the V18 architect meta-skill package, V18 module config JSON Schemas, V18 site
configuration instances, and V18 deployment manifests.

---

### V18 Canonical Type AB01 - Versioned Stack Migration Plans With Corrections

**Registry status:** `full_type`

**What it is:** `XIIGEN_NESTJS_REACT_TRANSLATION_PLAN_v1..v4_FINAL`, a same-day
versioned plan chain for Python to NestJS/React migration.

**Authority boundary:** Migration plan evolution authority. v4 FINAL is
canonical; v1-v3 are stale but useful for versioned correction pairs.

**A-side use:** Migration planning prompts.

**B-side chosen use:** Correct v4 engine-first plan, 13-phase structure, kernel
multi-tenancy scope, and directory tree.

**Rejected source:** v3 FLOW-21-specific scope used as current, omitted missing
layers, or migration plan without correction rationale.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `plan_version`, `is_final`, `correction_ids`,
`removed_scope`, `added_layers`, `phase_count`, `directory_tree_present`, and
`canonical_version=v4_FINAL`.

---

### V18 Canonical Type AB02 - Architect Skill Packages

**Registry status:** `full_type`

**What it is:** `xiigen-architect-skill.zip`, consisting of `SKILL.md` plus eight
reference files.

**Authority boundary:** Canonical V18 architect meta-skill package. It governs how
to design skills and save/restore architect sessions.

**A-side use:** Architect workflow prompts and meta-skill prompts.

**B-side chosen use:** Correct YAML frontmatter, iron rules, command table,
phase outputs, and reference file activation.

**Rejected source:** Ad-hoc architect behavior without package references, or
ordinary code-generation skill treated as architect meta-skill.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `skill_package_archive`, `skill_name`,
`skill_version`, `allowed_tools`, `command_ids`, `iron_rule_ids`,
`phase_ids`, and `reference_file_ids`.

---

### V18 Canonical Type AB03 - Architect Skill Reference Protocol Library

**Registry status:** `full_type`

**What it is:** The eight `references/*.md` files consumed by the architect
SKILL.md: session protocol, abstraction extraction, regeneration prompts, build
standards, freedom machine, previous-code protocol, skill-library map, and related
protocol references.

**Authority boundary:** Standalone protocol authority only when activated through
the architect skill package.

**A-side use:** Prompts about architect session state, abstraction extraction,
skill regeneration, standards, or previous-code classification.

**B-side chosen use:** Correct protocol structure, required fields, trigger table,
or template placeholders from the named reference file.

**Rejected source:** SKILL.md summary used instead of the reference protocol, or
wrong reference file for the question.

**Expected overlap ratio:** 0.10-0.30.

**DTO requirements:** `reference_file`, `protocol_name`, `required_fields`,
`trigger_table_ids`, `template_ids`, `activated_by_skill=xiigen-architect`, and
`reference_role`.

---

### V18 Canonical Type AB04 - V18 Version Consolidation State Documents

**Registry status:** `full_type`

**What it is:** V18 `CONSOLIDATION_STATE.md`, the line-counted V17-to-V18 package
delta produced by the consolidation prompt.

**Authority boundary:** Canonical V18 package-level changelog and line-count
state. This is a stricter subtype of AA14.

**A-side use:** Version delta prompts.

**B-side chosen use:** Correct added groups, line counts, not-changed items,
archived items, and AI tool config table.

**Rejected source:** Session checkpoint used as version consolidation state, or
wrong line count/category.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `archive_id=xiigen_v18_complete.zip`,
`from_version=V17`, `to_version=V18`, `added_groups`, `line_counts`,
`not_changed`, `archived`, and `ai_tool_config_files`.

---

### V18 Canonical Type AB05 - Module Config JSON Schema Files

**Registry status:** `full_type`

**What it is:** `genie-dna/config-schemas/*.schema.json`, formal JSON Schema
draft-07 files for module config indices.

**Authority boundary:** Type 25 schema authority for V18 module configuration
validation.

**A-side use:** Config authoring prompts.

**B-side chosen use:** Schema-valid config fields, required keys, enums, and
cross-module references.

**Rejected source:** Site config instance used as schema, missing required field,
wrong enum value, or ignoring pattern-linked descriptions.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `schema_path`, `$id`, `module_number`, `config_index`,
`required_fields`, `properties`, `enums`, `referenced_skills`, and
`json_schema_draft`.

---

### V18 Canonical Type AB06 - Site Type Configuration Instance Files

**Registry status:** `full_type`

**What it is:** `genie-dna/site-configs/*.json`, concrete V18 site-type
configuration instances.

**Authority boundary:** Canonical populated config examples for V18 site types,
stronger than generic examples in AA07 when from the canonical V18 package.

**A-side use:** Site config generation prompts.

**B-side chosen use:** Correct `siteType`, `scopeId`, `modules_enabled`, theme,
features, SEO, and per-module parameters.

**Rejected source:** Schema as instance, wrong module list for site type, or
tenant profile used as full site config.

**Expected overlap ratio:** 0.05-0.20.

**DTO requirements:** `site_config_path`, `site_type`, `scope_id`,
`modules_enabled`, `theme`, `features`, `seo`, `module_parameters`, and
`canonical_v18_config=true`.

---

### V18 Canonical Type AB07 - Site Type Deployment Manifest Collections

**Registry status:** `full_type`

**What it is:** V18 `genie-dna/DEPLOYMENT_MANIFESTS.md`, the canonical 15
site-type deployment manifest collection with extension packs and execution
checklist.

**Authority boundary:** Canonical V18 deployment activation authority, stronger
than generic manifest guidance in AA05.

**A-side use:** Site deployment prompts.

**B-side chosen use:** Correct YAML manifest, skills to activate, config indices,
extension packs, and five-step checklist.

**Rejected source:** Site config instance used as deployment manifest, wrong
extension pack, or missing skill activation list.

**Expected overlap ratio:** 0.10-0.25.

**DTO requirements:** `manifest_path`, `site_type`, `skills_to_activate`,
`config_indices_to_create`, `extension_packs`, `execution_checklist`, and
`canonical_v18_manifest=true`.

---

## Compliance, Flow Extension, And Plugin Skill Factory Families

The `AC01-AC20` families come from `xiigen-nestjs-P7.3.zip`, `documents.zip`,
`flows_extensions.zip`, `plugin-skill-factory.zip`, and
`genie-plugin-skill-factory.zip`. They cover production TypeScript deliverables,
the Flow Compliance Initiative, per-flow canonical extension packages with
renumbering, and WordPress/plugin skill-factory documentation.

| Family | Registry status | What it is | Required DTO fields | Reject when |
|---|---|---|---|---|
| AC01 TypeScript NestJS source deliverables | full_type / Type 26 | P7.3 `.ts` production deliverables implementing XIIGen FREEDOM-layer patterns | `archive_id`, `file_path`, `export_names`, `pattern_id`, `data_process_result_used`, `dynamic_document_shape` | typed fixed business model or exception-throwing service used as chosen |
| AC02 TypeScript Jest DNA specs | full_type / Type 28 | `.spec.ts` files with DNA compliance tests | `spec_path`, `describe_block`, `it_block`, `dna_rule_id`, `expected_assertion`, `test_framework=jest` | test prose used without assertion or wrong DNA rule |
| AC03 Flow compliance framework | full_type | `FLOW_COMPLIANCE_FRAMEWORK.md`, R1-R8 requirement definitions | `requirement_id`, `requirement_name`, `formal_definition`, `pass_criteria`, `fail_criteria`, `enforcement_contract` | requirement cited without pass/fail contract |
| AC04 Compliance gap matrix | full_type | `FLOW_COMPLIANCE_GAP_MATRIX.md`, 35 flows x 8 requirement audit | `flow_id`, `requirement_id`, `cell_status`, `evidence`, `matrix_version` | row/column swapped or stale cell used |
| AC05 Flow compliance remediation plan | full_type | `FLOW_COMPLIANCE_PLAN.md`, five-phase remediation plan | `phase_id`, `requirement_ids`, `deliverables`, `dependency_order`, `automation_gate` | compliance spec used as remediation plan |
| AC06 Requirement-specific compliance specs | full_type | MT feature flags, security isolation, library boundary, docs, business cycles, prompt/visual/mock catalogs | `requirement_id`, `spec_file`, `new_artifacts`, `new_bfa_rules`, `file_patterns`, `test_templates` | requirement-specific artifact attributed to wrong R-number |
| AC07 Compliance validation runner specs | full_type | `COMPLIANCE_VALIDATION_RUNNER.md` script/gate specification | `runner_command`, `requirement_ids`, `flow_scope`, `expected_outputs`, `gate_rules` | runtime pass/fail claimed without runner spec or command |
| AC08 Compliance integration master guides | full_type | `INTEGRATION_MASTER_GUIDE.md`, routing compliance docs into canonical files | `compliance_doc`, `canonical_file`, `extension_point`, `routing_reason`, `append_order` | compliance doc merged into wrong canonical file |
| AC09 Compliance appendices | full_type / patch subtype | append-to canonical file appendices | `append_to`, `target_canonical_file`, `insertion_point`, `save_point`, `content_block_id` | appendix treated as standalone canonical file |
| AC10 Multi-tenant architecture research analyses | full_type | `multi-tenant-support.md`, file-by-file MT impact analysis | `research_file`, `affected_file`, `impact_type`, `adaptation_plan`, `placeholder_count` | research treated as implementation evidence |
| AC11 Per-flow engine extension docs | full_type | `FLOWxx_ENGINE_EXTENSION.md`, per-flow authoritative standalone extension | `flow_id`, `domain`, `fabric_architecture`, `mini_rag_index`, `phase_ids`, `resume_key`, `checksum` | aggregate master plan used as per-flow authority |
| AC12 Per-flow merge master plans | full_type | `FLOWxx_MERGE_MASTER_PLAN.md`, merge plan into canonical files | `flow_id`, `phase_id`, `target_canonical_file`, `merge_step`, `dependency_order` | merge completion report used as plan |
| AC13 Per-flow state JSON with renumbering | full_type | `FLOWxx_STATE.json`, phases and next offsets | `flow_id`, `status`, `phases_completed`, `renumbering`, `artifacts_added`, `next_flow_starts_at`, `files_produced` | offsets inferred from prose |
| AC14 Flow artifact renumber maps | full_type | `FLOWxx_RENUMBER_MAP.json`, explicit old/new artifact maps | `flow_id`, `actual_prior_state`, `factory_map`, `task_type_map`, `family_map`, `bfa_rule_map`, `skill_map` | any generated ID pair lacks map evidence |
| AC15 Per-flow validation reports | full_type | `FLOWxx_VALIDATION_REPORT.md`, 70-point post-merge protocol | `flow_id`, `check_id`, `check_status`, `evidence`, `failure_reason`, `protocol_version` | PASS claimed without check row |
| AC16 Flow merge completion reports | full_type | `FLOWxx_MERGE_COMPLETION_REPORT.md`, post-merge next IDs | `flow_id`, `merged_files`, `completion_status`, `next_available_ids`, `validation_summary` | completion report used before validation |
| AC17 Per-flow local RAG indexes | full_type | `FLOWxx_RAG_INDEX.md`, compact local lookup/renumbering index | `flow_id`, `lookup_key`, `artifact_table`, `renumbering_ref`, `target_file` | local index used as global RAG authority |
| AC18 Post-flow state and consolidation reports | full_type | `SESSION_STATE_FLOWxx_FINAL.md`, `FLOW_CONSOLIDATION_REPORT.md`, `FLOWxx_P0_PLAN.md`, mini session state | `flow_id`, `state_kind`, `what_changed`, `ready_for_next_phase`, `consolidated_files`, `p0_domain_scope` | planning, final state, and consolidation report swapped |
| AC19 Plugin skill factory examples and indexes | full_type | `EXAMPLES.md`, `SKILLS_INDEX.md`, per-skill `references/*.md` | `skill_id`, `example_polarity`, `dependency_edges`, `machine_freedom_class`, `copy_adapt_rewrite`, `reference_note_path` | positive/negative example polarity reversed |
| AC20 Plugin factory tool config and source-to-skill RAG index variants | full_type | JSON `.cursorrules` and `DOC_RAG_INDEX.md` source-code-to-skill mapping | `tool_name`, `json_rule_name`, `instructions`, `patterns`, `source_file`, `line_range`, `classification`, `phase` | markdown config assumed or file-to-use-case index used instead of source-code mapping |

**Authority boundary:** AC families are mostly B-side authority for compliance,
per-flow merge/renumbering, and plugin-skill-factory examples. AC09 is patch-like
and must name its append target. AC13 and AC14 are the only authorities for
renumbering offsets and artifact ID maps.

**A-side use:** Allowed for AC11/AC12/AC18/AC19 when the user prompt is a real
flow-extension or plugin-factory task. AC03-AC10 and AC13-AC17 are normally
B-side or measurement/provenance only.

---

## Web-Session, Flow State, And Canonical Consolidation Families

The `AD01-AD08` families come from `agent-output-format-skill_for_websessions.zip`,
`xiigen_with_flow21.zip`, `flow_35_done_so_far.zip`,
`FLOW35_P5_CANONICAL_DOCS.zip`, and the FLOW-30 result packages. They are high
priority because they prevent web-to-agent handoff drift and provide the exact
state/renumbering/validation evidence for FLOW-21, FLOW-30, and FLOW-35.

| Family | Registry status | What it is | Required DTO fields | Reject when |
|---|---|---|---|---|
| AD01 Web-session governance skills | full_type | agent-output-format SKILL.md + AGENTS.md + skill.yaml, loaded before all other skills | `skill_name`, `position`, `consumer_type`, `deliverable_kind`, `recheck_trigger`, `stop_between_sessions`, `execution_doc_required` | web session emits a large analysis doc as if Claude Code could execute it |
| AD02 Per-flow implementation completion state | full_type | `FLOW21_STATE.json`, final per-flow implementation metrics and enforced artifacts | `flow_id`, `families_completed`, `factory_range`, `test_count`, `per_phase_tests`, `bfa_rules`, `design_records`, `skills_applied`, `dna_patterns`, `backward_compatible` | fabricated metrics or project-level state used as per-flow state |
| AD03 SDK audit and provider plans | full_type | `FLOW35_SECRETS_FABRIC_PLAN.md`, SDK audit plus new provider plan | `flow_id`, `provider_id`, `current_code_status`, `sdk_status`, `action`, `deprecated_package`, `replacement_package`, `before_after_code` | provider status claimed without audit row |
| AD04 Post-flow artifact RAG indexes | full_type | `FLOW35_RAG_INDEX.md`, artifact counts/ranges/next available plus file map | `flow_id`, `artifact_type`, `count`, `range`, `next_available`, `new_artifact_id`, `modified_or_created_file` | post-flow artifact IDs inferred without index |
| AD05 Per-phase flow completion states | full_type | `FLOW35_STATE_P0..P5.json`, cumulative per-phase state and document updates | `flow_id`, `phase_id`, `phase_name`, `documents_updated`, `phases_completed`, `cumulative_test_count`, `post_flow_totals`, `next_available` | final state used for phase-specific question or phase state used as final completion |
| AD06 Flow merge master plans with renumbering map | full_type | `FLOW30_MERGE_MASTER_PLAN.md`, no-code explanation plus renumbering against actual prior state | `flow_id`, `canonical_files_to_update`, `draft_artifact_range`, `renumbered_artifact_range`, `prior_state_source`, `safety_rails` | draft numbers used after renumbering |
| AD07 Canonical document validation reports | full_type | `VALIDATION_REPORT.md` and `FLOW30_VALIDATION_MASTER_PLAN.md`, seven-file integrity validation | `flow_id`, `canonical_file`, `line_count`, `flow_present`, `header_updated`, `artifact_type`, `expected_count`, `max_found`, `terminal_id`, `status` | all-pass claim without validation matrix row |
| AD08 Document consolidation execution record pairs | full_type | `EXECUTION_PLAN.md` + `CONSOLIDATION_REPORT.md`, before/after consolidation evidence | `operation_id`, `execution_plan_path`, `consolidation_report_path`, `what_was_wrong`, `what_was_done`, `file_size_before_after`, `content_verification` | execution plan or report used alone when before/after pair is required |

**Authority boundary:** AD01 is a meta-governance skill for web-session output
format. AD02/AD05 are state authorities. AD06 is renumbering plan authority.
AD07 is validation authority. AD08 is before/after consolidation evidence.

---

## Canonical Extension, Research, And V37 Planning Families

The `AE01-AE10` families come from `16 - files.zip`, `16-input files.zip`,
`FLOW09_ALL_MERGED_FILES.zip`, and `XIIGEN_V37_PLAN_PACKAGE.zip`. They cover the
standard flow-boundary snapshot bundle, per-canonical-file append documents,
deep research with web/file citations, compact early session state, and V37
adaptive planning/RAG indexes.

| Family | Registry status | What it is | Required DTO fields | Reject when |
|---|---|---|---|---|
| AE01 Flow-versioned canonical snapshot bundles | archive_container | checkpoint package of 6-7 canonical MERGED files plus session state and per-flow docs | `archive_path`, `flow_boundary`, `canonical_files_included`, `session_state_path`, `snapshot_version`, `member_paths` | ZIP filename used as source authority instead of extracted member |
| AE02 Per-canonical-file flow extension documents | full_type | `FLOW-NN_*_ENGINE_ARCHITECTURE/TASK_TYPES/BFA/SKILLS/INDEX.md` append content for one canonical file | `flow_id`, `target_canonical_file`, `append_after_anchor`, `pre_flow_totals`, `new_artifacts`, `content_block`, `save_point_checksum` | merge master plan used as append content or wrong canonical target |
| AE03 Flow final session state documents | full_type | `FLOW-NN SESSION_STATE_FINAL.md`, post-flow system totals and final save point | `flow_id`, `save_point`, `system_totals`, `factories_range`, `task_types_range`, `families_range`, `skills_range`, `rules_range` | global merge session state used for per-flow final state |
| AE04 AI-generated domain research with web/file citations | full_type | deep research docs with `citeturn` and `filecite` references | `research_topic`, `domain`, `web_citations`, `file_citations`, `baseline_mapping`, `architecture_imperatives`, `recommendations` | uncited chat answer treated as cited deep research |
| AE05 Engine-specific deep research analyses | full_type | Genie/XIIGen-specific deep research with implementation guidance | `research_topic`, `engine_focus`, `implementation_patterns`, `saga_or_outbox_rules`, `idempotency_rules`, `citations` | generic research used as engine implementation guidance |
| AE06 Q&A chat research logs | full_type | saved question-answer domain exploration without formal citations | `question`, `answer_sections`, `domain_modules`, `citation_present=false`, `source_session` | Q&A log treated as cited research analysis |
| AE07 Versioned adaptive plans with sub-phase checkpoints | full_type / Type 16 extension | `XIIGEN_V37_MASTER_PLAN.md`, conflict-resolved adaptive plan with sub-phases | `plan_version`, `source_versions`, `conflict_resolution_table`, `approach_libraries`, `subphase_ids`, `checksum`, `resume_key` | prior V35/V36 plan used as V37 current |
| AE08 Sub-phase granularity session states | full_type / Type 22 | `V37_STATE.md`, 19-subphase planning state | `plan_version`, `subphase_id`, `status`, `save_key`, `estimated_time`, `unblocks`, `rule_status`, `gate_status` | completion checkpoint used as pre-execution sub-phase state |
| AE09 Approach library RAG indexes | full_type | `MINI_RAG_INDEX_V37.md`, approach library file to phase/skill lookup | `plan_version`, `library_file`, `key_pattern`, `use_in_phase`, `use_in_skill`, `verified_against_zip=true` | project RAG index used as approach-library index |
| AE10 Session decision log RAG indexes | full_type | `SESSION_RAG_INDEX_V37.md`, decision log + inventory + ancestry + recovery | `plan_version`, `decision_id`, `problem`, `decision`, `rationale`, `source_library`, `version_ancestry`, `recovery_key` | decision log used as implementation source without referenced file |

**Authority boundary:** AE02 is the append-content authority for canonical files.
AE07/AE08 are V37 planning and state authorities. AE09/AE10 are navigation and
decision-log authorities; they point to source libraries and decisions but do not
replace the underlying implementation sources.

---

## Flow Planning, Examination, And Artifact Verdict Families

The `AF01-AF13` families come from `plan files.zip`,
`XIIGEN_MODULE_ARCHITECTURE_P19.zip`, `architecture gap.zip`,
`FLOW-01-02 plan files.zip`, `FLOW-01-02 plan corrections v2.zip`,
`flow propagation.drawio.xml`, and `flow_47_design_review_verdict.html`. Some
members confirm earlier families (`B16`, `K07`, `M13-M16`, `Y01-Y05`); the AF
rows below capture the additional DTO shapes that were not explicit in v1.18.

| Family | Registry status | What it is | Required DTO fields | Reject when |
|---|---|---|---|---|
| AF01 Supreme-priority flow planning skills | full_type / Type 20 extension | `data-connection-classification`, `flow-implementation-guide`, and `flow-prerequisites` skills with `priority: SUPREME` and must-load-before planning rule | `skill_name`, `priority`, `load_before`, `trigger_ids`, `connection_type_or_prereq_fields`, `yaml_frontmatter_path` | skill treated as ordinary Type 20 without priority/load-order DTO |
| AF02 Versioned flow examination governance plans | full_type | `Flow_examination_plan_v3..v11.md`, protocol for examiner behavior and verdict recording | `plan_version`, `hypothesis_ids`, `positive_criteria`, `negative_criteria`, `verdict_legend`, `recording_format`, `supersedes` | per-flow result used as governance protocol or stale version used as current |
| AF03 Skeleton module-separation fix plans | full_type | `XIIGEN-MODULE-SEPARATION-FIX-PLAN-vN.md`, incomplete fix plan explicitly marked skeleton while examination continues | `plan_version`, `examination_count`, `skeleton_state=true`, `acceptance_test`, `phase_ids`, `bash_commands`, `placeholder_sections` | skeleton plan treated as final remediation plan |
| AF04 Claude session transcript text files | preprocess_then_reclassify | `module separation N.txt`, raw web/Claude session transcript with tool-call summaries and state reports | `transcript_path`, `session_number`, `turn_locator`, `tool_call_count`, `command_summary`, `state_report_locator`, `raw_transcript=true` | whole transcript used as one B-side without turn/zone extraction |
| AF05 Goal anchoring documents | full_type | `THE GOAL - always make analyzation in goal perspective.md`, verbatim goal plus architecture framing | `goal_doc_path`, `verbatim_goal`, `framing_principles`, `downstream_scope`, `applies_to_family`, `not_decision=true` | treated as locked decision or generic prompt instead of goal anchor |
| AF06 Planning simulation documents | full_type | `FLOW-NN-PLANNING-SIMULATION-R0.md`, simulation-mode planning output producing DPO triples from live run data | `flow_id`, `round_id`, `receives`, `produces`, `consumer`, `consumer_requirements`, `live_run_data`, `dpo_triples`, `graph_edges`, `rag_seeds` | design simulation or live execution report used as planning-simulation source |
| AF07 Deep design/execution simulation documents | full_type | `FLOW-NN-DESIGN-SIMULATION-Rx.md`, depth-trace simulation from user request to training corpus and tests | `flow_id`, `round_id`, `depth_levels`, `node_ids`, `cross_flow_edges`, `subflows`, `training_corpus_outputs`, `correction_source_round` | shallow flow plan used as deep simulation evidence |
| AF08 TEACH-QA-R0 bootstrap documents | full_type | `FLOW-NN-TEACH-QA-R0.md`, teaching QA session for `BootstrapFromDocumentsService` | `flow_id`, `round_id`, `consumer`, `phases`, `artifact_outputs`, `rag_seeds`, `teaching_goal` | design review QA or ordinary session brief used as TEACH-QA source |
| AF09 TEACH-QA-R1 correction documents | full_type / patch | `FLOW-NN-TEACH-QA-R1.md`, correction pass appending gap table to TEACH-QA-R0 | `flow_id`, `correction_round`, `appends_to`, `gap_table`, `severity`, `root_cause`, `phase_id` | R1 correction used without base R0 or appends-to target |
| AF10 Autonomous Claude Code execution plans | full_type | `FLOW-01-02-1DAY-IMPLEMENTATION-PLAN.md`, execution plan for Claude Code with branch, gates, and snapshots | `plan_path`, `executor`, `branch`, `schedule`, `automated_gate_protocol`, `playwright_snapshots`, `absolute_rules` | human planning doc treated as autonomous executor instructions |
| AF11 Simulation patch documents | full_type / patch | `FLOW-NN-THREE-FIXES.md`, `B2-SCOPE-PATCH.md`, patch/addendum docs for design simulations | `patch_path`, `appends_to`, `fix_ids`, `scope`, `target_design_simulation`, `gap_reason` | patch used without target simulation or base round |
| AF12 Design review verdict HTML artifacts | preprocess_then_reclassify | `flow_47_design_review_verdict.html`, Claude artifact HTML with verdict banner, claims, findings, and goal elements | `html_path`, `artifact_type`, `verdict`, `edit_count`, `claims_verified`, `findings`, `goal_elements`, `extraction_method` | raw HTML or rendered style used instead of extracted review zones |
| AF13 Existing-family confirmations | limited_authority | second-source confirmations for `B16`, `K07`, `M13-M16`, `Y01-Y05` | `source_path`, `confirmed_family`, `confirmed_field`, `reason`, `do_not_duplicate=true` | already-covered family duplicated as a new type without new DTO fields |

**Authority boundary:** AF06-AF09 are flow-learning/planning artifacts and may
produce training pairs directly. AF04 and AF12 must be extracted into zones
before DPO use. AF13 is a confirmation row only; it updates DTO rules but does
not create a new source family.

---

## Flow Input, Renumbering, And Merge-Completion Families

The `AG01-AG06` families come from `flows_extensions.zip`, `30-files.zip`,
`34-input files.zip`, `30-input files.zip`, `29-files.zip`, `33-input files.zip`,
`28-files.zip`, and `27-input files.zip`. They extend the AD/AE merge-plan
families by exposing raw flow genesis documents, standalone renumbering maps,
and applied merge-completion evidence across FLOW-26 through FLOW-34.

| Family | Registry status | What it is | Required DTO fields | Reject when |
|---|---|---|---|---|
| AG01 Standalone renumber map JSON | full_type | `FLOW33_RENUMBER_MAP.json`, machine-readable actual prior state, new artifacts, and ID maps | `flow_id`, `actual_prior_state`, `new_artifacts`, `next_available`, `factory_map`, `task_type_map`, `family_map`, `source_numbers`, `renumbered_numbers` | draft/pre-renumber IDs used when JSON map exists |
| AG02 Per-flow merge completion reports | full_type | `FLOW26/27_MERGE_COMPLETION_REPORT.md`, delivered files, applied offsets, next IDs, cross-validation | `flow_id`, `files_delivered`, `post_flow_next_available`, `renumbering_applied`, `offsets`, `cross_validation_counts`, `status` | validation report or merge plan used as applied completion evidence |
| AG03 Flow initiation description documents | full_type | `NN-flowname.md`, raw user intent plus immediate AI design response in one document | `flow_id`, `flow_name`, `verbatim_user_intent`, `design_response_sections`, `raw_intent_locator`, `response_locator`, `source_role` | AI response used without preserving user intent or intent used without response boundary |
| AG04 Flow deep research with web citations | full_type / limited_authority | `NN-flowname deep-research-report.md`, web-search-enhanced flow research with `citeturn` references | `flow_id`, `research_topic`, `web_citations`, `standards_or_versions`, `recommendations`, `limited_authority_reason` | citation-bearing research treated as implementation source without validation |
| AG05 Prompt improvement session documents | full_type | `30-prompt improvements.md`, FLOW-30 genesis prompt and PromptOps design response | `flow_id`, `prompt_ops_goal`, `schemas`, `runtime_loop`, `judge_learn_loop`, `canary_rules`, `implementation_link` | static-prompt plan used instead of PromptOps versioning design |
| AG06 Early-era engine extension documents | full_type | `FLOW01_ENGINE_EXTENSION.md`, business story, text DAG, Mini-RAG, and phase execution in one early document | `flow_id`, `business_story`, `text_dag_paths`, `mini_rag_index`, `resume_anchor`, `phase_ids`, `engine_translation` | later merge master plan used as early engine-extension authority |

**Authority boundary:** AG01 and AG02 are ID/renumbering authorities. AG03 is the
best raw-intent to design-response source and should preserve both sides. AG04
is research support, not implementation authority, unless corroborated by the
resulting flow documents.

---

## Quick Reference

| Family | Registry status | A-side | B-side chosen | Rejected source | Overlap |
|---|---|---|---|---|---|
| A01 Configuration schemas | full_type / Type 25 | no | schema-valid fields | schema violation | 0.10-0.30 |
| A02 Build manifests | limited | no | pinned dependency/build graph | wrong version/order | 0.15-0.30 |
| A03 CI/editor/repo configs | limited | no | CI/tool rule | skipped dependency | 0.15-0.30 |
| A04 Utility scripts | limited | no | code-path order | wrong process order | 0.20-0.40 |
| A05 Source code contracts | full_type / Type 26 | no | exact interface/DTO | inferred wrong shape | 0.10-0.25 |
| A06 Checkpoints | excluded | no | manifest only after Type 4/22 reclass | wrong lineage | inherited from Type 4/22 |
| A07 Dataset rows | excluded | no direct use | original source after audit | wrong label | inherited from original source |
| A08 Spreadsheets | preprocess | no | extracted sheet zone | wrong metric/mapping/order | typically 0.10-0.45 by sheet zone |
| A09 HTML captures | preprocess | original prompt only | extracted content | malformed/missing content | inherited from extracted content |
| A10 Visual assets | excluded | no | no text DPO | visual mismatch only | n/a |
| A11 Localization variants | limited | real prompt only | canonical only | stale translation | inherited from canonical source |
| A12 Deployment templates | limited | no | runtime wiring/env vars | missing var/order | 0.15-0.35 |
| A13 Diagrams | full_type / Type 27 | no | parsed graph edge | reversed/missing edge | 0.10-0.20 |
| A14 Logs | limited | no | normalized failure evidence | wrong root cause | 0.15-0.35 |
| A15 Test files | full_type / Type 28 | no | passing assertion | contradiction | 0.10-0.30 |
| A16 Plugin manifests | limited | no | permission/entry point | undeclared capability | 0.10-0.25 |
| A17 Office/PDF exports | preprocess | no | extracted canonical content | stale export | inherited from extracted content |
| A18 Vendored repos | excluded | no | external/adopted only | wrong adoption | inherited from external/adopted source |
| A19 Generated UI outputs | limited | no | accepted corrected output | pre-debug output | 0.25-0.50 |
| A20 Before/after visuals | limited | no | visual QA finding | reversed direction | n/a |
| A21 Example catalogs | limited | if real prompt | labeled positive | labeled negative | 0.10-0.35 |
| A22 Algorithm notes | limited | no | CODE_Q mechanism | wrong algorithm | 0.20-0.40 |
| A23 Localized guides | preprocess | real prompt only | canonical locale only | stale mirror | inherited from canonical source |
| A24 Templates | limited | no | required template shape | missing placeholder | 0.15-0.30 |
| A25 IaC files | full_type_candidate | no | resource graph | unsafe/wrong resource | 0.15-0.30 |
| A26 DB migrations | full_type_candidate | no | schema/migration order | wrong constraint/order | 0.10-0.25 |
| A27 Publish profiles | excluded | no | sanitized sample only | secret/missing placeholder | inherited from A12 if sanitized |
| A28 Coverage binaries | preprocess | no | Type 4/32 after conversion | false metric | inherited from Type 4/32 |
| A29 Native binaries | excluded | no | no text DPO | wrong metadata only | n/a |
| A30 Git internals | excluded | no | normalized git command only | wrong history claim | inherited from Type 12/16 if normalized |
| A31 Web server configs | limited | no | hosting/rewrite rule | wrong route/order | 0.15-0.30 |
| A32 Locks/caches | excluded | no | reclassify lockfiles to A02 | wrong version | inherited from A02 only |
| A33 IDE metadata | limited | no | tooling setup | wrong setting | 0.10-0.25 |
| A34 Browser sidecars | excluded | no | no independent use | n/a | n/a |
| A35 Non-ZIP archives | preprocess | no | extracted content | archive extraction mismatch | inherited from extracted content |
| A36 Repo governance | limited | no | owner/hook/package rule | wrong owner/gate | 0.10-0.25 |
| A37 Font assets | excluded | no | A10/A19 after extraction | wrong font/reference | inherited from A10/A19 |
| A38 Debug maps/build metadata | excluded | no | A14/A05 after resolution | wrong source mapping | inherited from A14/A05 |
| A39 Translation catalogs | limited | real prompt only | key/translation coverage | stale/missing locale key | 0.15-0.35 |
| A40 API/IDL contracts | full_type_candidate | no | endpoint/schema/message | wrong API contract | 0.10-0.25 |
| A41 Browser/test traces | preprocess | no | Type 8/A15/A20/A28 after extraction | false trace/test claim | inherited from target type |
| A42 Certificates/keys | excluded | no | sanitized metadata only | secret/expiry/env mismatch | inherited from A12/Type 8 |
| A43 Package publication metadata | limited | no | package/version/include rule | wrong package/publish claim | 0.15-0.30 |
| A44 Platform entitlements | limited | no | permission/capability rule | undeclared capability | 0.10-0.25 |
| A45 View templates | limited | real embedded prompt only | binding/placeholder rule | missing binding | 0.15-0.35 |
| A46 Bulk stream exports | preprocess | record prompt only | parsed record after reclass | wrong event/metric | inherited from target type |
| A47 Static build bundles | excluded | no | source-mapped/visual only | stale generated output | inherited from A19/A20/A38 |
| A48 License/provenance files | limited | no | license/notice rule | wrong reuse/attribution | 0.10-0.25 |
| A49 Codebase state reviews | limited | no | per-flow state/gap | stale or wrong state claim | 0.15-0.35 |
| A50 Design/code review verdicts | preprocess | no | Type 8/23/32 after zone mapping | false pass/wrong review round | inherited from Type 8/23/32 |
| A51 Raw Claude Code sessions | preprocess | human turn only | validated segment after reclass | mixed-turn contamination | inherited from target type |
| B13 Codebase state reviews | full_type | no | audit finding/state/gap | wrong severity/state | 0.10-0.25 |
| B14 Architect/design verdicts | full_type | no | evidence-backed verdict | false pass/wrong round | 0.15-0.25 |
| B15 Episode maps | full_type | real matched prompt | episode decomposition | shallow/wrong episode arc | 0.15-0.35 |
| B16 Flow hypothesis matrix | full_type | no | fleet verdict/coupling | fabricated readiness | 0.10-0.20 |
| B17 Batch recon | limited | no | Type 4/23/29 after zone split | wrong recon metric/gap | inherited from target type |
| B18 Carry-forward issues | full_type | related real bug prompt | correction plus impact | old broken behavior | 0.10-0.25 |
| B19 Corrected defect catalogs | full_type | real defect prompt only | root cause/defect class | wrong root cause | 0.15-0.30 |
| B20 Fix regression reports | full_type | original correction only | regression-aware fix | bad fix/regression denied | 0.15-0.30 |
| B21 A-to-B source maps | limited | no | source attribution | wrong primary/reference | 0.05-0.15 |
| B22 Conflict maps | full_type | no | authoritative resolved value | overridden value | 0.05-0.15 |
| B23 V17 skill alternatives | limited | porting prompt only | canonical behavior port | non-canonical alternative | 0.15-0.30 |
| B24 Episode query maps | limited | query phrase | B15 episode map only | wrong episode mapping | n/a |
| B25 Delivery/spec location maps | full_type | no | delivery/source-location | wrong source/weak approach | 0.10-0.25 |
| B26 Skill reference subdocs | full_type | marked example only | skill rule/reference | wrong rule or boundary | 0.10-0.30 |
| B27 Session load plans | full_type | no | session load order | skipped invariant/skill | 0.15-0.35 |
| B28 Operational protocols | full_type | no | required procedure/gate | skipped required step | 0.15-0.30 |
| B29 Behavioral correction registries | full_type | quoted correction only | corrected behavior | repeated wrong behavior | 0.10-0.25 |
| B30 Plan review templates | limited | no | required checklist field | missing template gate | 0.15-0.30 |
| B31 Examination guidance | preprocess | marked example only | zone-mapped guidance | wrong hypothesis/step | inherited from target type |
| B32 Document authority maps | full_type | no | canonical authority source | cross-ref as canonical | 0.05-0.15 |
| B33 Architect infra fix plans | limited | no | phase/dependency/version | stale version/order | 0.15-0.35 |
| B34 Business flow specs | limited | real business prompt only | zone-mapped flow spec | cross-zone misuse | inherited from target type |
| B35 History RAG fixture pipeline | full_type | explicit fixture query only | fixture/index/integration | wrong fixture/qs/type | 0.10-0.20 |
| B36 Gap-to-guidance maps | full_type | no | gap to skill/guidance | wrong skill coverage | 0.10-0.20 |
| B37 Skill integration instructions | full_type | deployment prompt only | deployment sequence | wrong integration order | 0.15-0.25 |
| B38 Flow planning libraries | full_type | production-procedure prompt | library procedure | wrong prereq/order | 0.20-0.35 |
| B39 Run plans/finals | full_type | separated intent only | run plan/final result | plan/final confusion | 0.15-0.30 |
| B40 UI fleet coverage plans | full_type | no | cluster/pattern/scope | wrong cluster/redo | 0.15-0.25 |
| B41 Autonomous session specs | limited | no | Type 19/34/A03 zones | wrong done/prereq | inherited from target type |
| B42 Flow JSON definitions | limited | no | flow schema contract | missing event/field | 0.10-0.20 |
| B43 Gap/skill/design meta-analysis | limited | separated prompt only | zone-mapped meta-analysis | actual-as-desired | inherited from target type |
| C01 Early architecture plans | limited | no | historical evolution only | stale current-architecture claim | 0.05-0.15 |
| C02 Skills inventories/guides | limited | no | historical skill/path comparison | stale skill registry claim | 0.10-0.25 |
| C03 Consolidation prompts | full_type | no | handoff/load package | wrong attachment/order | 0.15-0.30 |
| C04 Pattern adoption maps | full_type | adaptation prompt only | original-to-XIIGen mapping | wrong migration direction | 0.10-0.25 |
| C05 Flow compliance framework | full_type | no | R1-R8 compliance/gap | false compliant status | 0.10-0.25 |
| C06 Requirement fulfillment specs | full_type | no | test/probe/mock contract | untestable compliance | 0.10-0.25 |
| C07 Project genealogy | full_type | why-question only | lineage explanation | invented origin | 0.10-0.25 |
| C08 Documentation compliance | limited | no | doc standard/runner/audit | false doc coverage | inherited from target type |
| C09 Skill snapshot archives | preprocess | no | Type 12/16/20 after extraction | stale snapshot as current | 0.05-0.20 |
| C10 Writing style skill | limited | content prompt only | voice/template pattern | generic AI text | 0.20-0.40 |
| C11 Office architecture exports | preprocess | no | extracted target type | stale export as current | inherited from target type |
| C12 Original user source seeds | limited | original request only | original source contract | adapted abstraction as original | 0.10-0.25 |
| Z41 Delivery checkpoints | full_type | no | phase/file status/resume key | wrong phase/status | 0.10-0.25 |
| Z42 DOC RAG indexes | full_type | no | file-to-use-case map | wrong file/reference | 0.05-0.15 |
| Z43 DNA audits | full_type | no | pattern compliance rating | false pass/no evidence | 0.10-0.25 |
| Z44 Continuation guides | full_type | no | resume key/sprint order | wrong resume/order | 0.15-0.30 |
| Z45 Stack recipes | full_type | stack-choice prompt only | stack/project structure | wrong stack | 0.10-0.25 |
| Z46 Use case guides | full_type | matching business prompt | use-case entities/stack | wrong use case | 0.10-0.25 |
| Z47 Weekly delivery guides | full_type | no | day/skill deliverable | wrong day/status | 0.15-0.30 |
| Z48 Skill manifests | limited | no direct prompt | skill metadata/tools | manifest as full skill | 0.10-0.20 |
| Z49 Draft/complete pairs | full_type | no | complete version | draft as final | 0.30-0.50 |
| Z50 Lesson transcripts | limited external | vocabulary only | structural lesson pattern only | transcript as XIIGen authority | 0.15-0.30 |
| Z51 Skill mastery guides | limited external | generic skill prompt only | structural advice format only | external use rule as internal rule | 0.15-0.30 |
| Z52 GSD runtime state | limited external | generic orchestration prompt | structural DTO pattern only | external content as XIIGen state | 0.15-0.30 |
| Z53 Agent system prompts | limited external | generic prompt-authoring | role/tool/output structure | external prompt as internal contract | 0.10-0.25 |
| Z54 Skill gate matrices | limited external | generic quality prompt | gate definition structure | definition used as result | 0.15-0.30 |
| Z55 Skill templates | limited external | generic skill-authoring | resolved template structure | placeholder as finished skill | 0.15-0.30 |
| Z56 AI context docs | limited external | generic context prompt | project context structure | external architecture as internal fact | 0.15-0.30 |
| D01 Session teach files | full_type | embedded prompt only | teaching round context | wrong round/check | 0.15-0.30 |
| D02 Session simulation files | full_type | no | simulation round contract | wrong expected output | 0.15-0.30 |
| D03 Round precheck/plans | limited | no | Type 34/29 zones | skipped precheck/order | inherited from target type |
| D04 Round document maps | limited | no | artifact routing | wrong document/round | 0.05-0.15 |
| D05 Flow engine gap lists | full_type | no | gap inventory/severity | missing/wrong gap | 0.10-0.25 |
| D06 Patch addenda | limited | no | patch delta | wrong target/base | 0.10-0.25 |
| D07 Flow authoring guides | full_type | no | doc structure standard | missing required section | 0.15-0.30 |
| D08 Simulation methodology | full_type | no | simulation process | wrong simulation algorithm | 0.15-0.30 |
| D09 Per-flow state JSON | limited | no | flow state fields | wrong flow state | 0.10-0.25 |
| D10 Infrastructure flow state | limited | no | prerequisite readiness | wrong infra blocker | 0.10-0.25 |
| D11 Flow prerequisite docs | limited | no | prerequisite gate | missing prerequisite | 0.10-0.25 |
| D12 Step invariants | full_type | no | per-flow invariant | invariant violation | 0.10-0.25 |
| D13 Live engine run protocols | full_type | no | runbook command/response | wrong endpoint/command | 0.15-0.30 |
| D14 Testing/OSS teaching protocols | full_type | no | zone-mapped QA/teaching | mixed zones/false pass | inherited from target type |
| D15 Feature registries | limited | no | feature scope/state | wrong feature session | 0.10-0.25 |
| D16 Feature registry plans | limited | no | registry update plan | plan used as current state | 0.15-0.30 |
| D17 Skill graphs | full_type | no | skill dependency edge | reversed/fabricated edge | 0.10-0.20 |
| D18 Flow lifecycle registries | limited | no | lifecycle state/transition | illegal transition | 0.10-0.25 |
| D19 Architecture decision addenda | full_type | no | added architecture rule | addendum without base | 0.10-0.25 |
| E01 Design simulation rounds | full_type | no | design-round topology/arbiter | wrong design round | 0.10-0.25 |
| E02 Flow DR triples | full_type | no | teachingPoint/named check | wrong triple/flow | 0.10-0.25 |
| E03 Session briefs | limited | no | verification brief | missing pre-flight check | 0.15-0.30 |
| E04 Pipeline master plans | full_type | no | pipeline phase plan | wrong phase/order | 0.15-0.30 |
| E05 Subflow specs | full_type | no | subflow boundary/arbiter | wrong subflow path | 0.10-0.25 |
| E06 Sim gap catalogs | full_type | no | round-attributed gap | wrong round/root cause | 0.10-0.25 |
| E07 UI reflection state | limited | no | UI coverage state | state used as screenshot | 0.10-0.25 |
| E08 Gaps-corrections registers | full_type | no | gap-to-correction register | register used as executed fix | 0.10-0.25 |
| E09 Client/platform specs | full_type | no | design-level client contract | design spec as runtime code | 0.10-0.25 |
| E10 Design review protocols | full_type | no | design review procedure | protocol as verdict | 0.15-0.30 |
| E11 Gap simulation protocols | full_type | no | gap simulation procedure | generic simulation substitute | 0.15-0.30 |
| E12 Phase three prep guides | full_type | no | prep sequence/readiness | stale/wrong phase guide | 0.15-0.30 |
| E13 Topology coverage state | limited | no | topology coverage metadata | UI/topology confusion | 0.10-0.25 |
| E14 Episode examination maps | full_type | mapped prompt only | episode extraction map | wrong correction mapping | 0.15-0.35 |
| E15 Knowledge digests | limited | no | source-backed digest | digest as canonical | 0.15-0.30 |
| E16 Implementation protocol deltas | full_type | no | protocol/change delta | delta without base protocol | 0.10-0.25 |
| F01 Missing skill files | full_type after recovery | no | recovered skill boundary | inferred skill behavior | 0.10-0.30 |
| F02 Architecture decisions JSON | full_type | no | per-flow decision | wrong flow/stale decision | 0.10-0.25 |
| F03 Session group bundles | full_type | no | group scope/criteria | bundle as single fix | 0.15-0.30 |
| F04 Per-phase state JSON | full_type | no | phase gate state | phase state as flow state | 0.10-0.25 |
| F05 Replan summaries | full_type | no | plan revision delta | old plan as current | 0.10-0.25 |
| F06 Community flow/gap docs | limited | community prompts only | community scope | community as internal | 0.10-0.25 |
| F07 Snapshot bundles | preprocess | no | part-level target type | snapshot as current state | inherited from target type |
| F08 Reconciliation state | full_type | no | reconciled reality | demonstrably wrong state | 0.10-0.25 |
| F09 Flow graph extensions | full_type | no | graph extension edge/node | extension as standalone flow | 0.10-0.25 |
| F10 Planning requirements dirs | preprocess | no | file-level requirement | directory name as evidence | inherited from target type |
| F11 Phase delivery summaries | full_type | no | delivery summary/result | summary used as plan | 0.15-0.30 |
| F12 Traceability reports | full_type | no | requirement coverage | PASS without artifact | 0.10-0.25 |
| F13 Flow status matrices | full_type | no | matrix status cell | status without evidence | 0.10-0.25 |
| F14 Teach-QA reviews | full_type | no | review finding/verdict | review as original req | 0.10-0.25 |
| F15 Package verification | full_type | no | verified package files | unextracted package claim | 0.10-0.25 |
| F16 Session loading plans | full_type | no | phase group load order | load group as completed work | 0.15-0.30 |
| F17 Architect verification blocks | full_type | no | closeout/open issues | open issue omitted | 0.10-0.25 |
| G01 Execution logs | full_type | no | runtime command evidence | calibration metrics as runtime | 0.10-0.25 |
| G02 Learning handoff JSON | full_type | no | inter-phase learning handoff | handoff as training rows | 0.10-0.25 |
| G03 Design corpus JSON | full_type | no | curated pattern triples | stale/generated corpus row | 0.10-0.25 |
| G04 Per-flow RAG specs | full_type | no | flow RAG activation | global RAG as per-flow | 0.10-0.25 |
| G05 Validation matrix zones | full_type | no | requirement coverage row | PASS without covered-by | 0.10-0.25 |
| G06 Flow prep bundles | preprocess | no | member target type | ZIP title as evidence | inherited from target type |
| G07 Gap process bundles | preprocess | no | member target type | wrong gap phase bundle | inherited from target type |
| G08 Final flow bundles | preprocess | no | manifest-verified members | final filename as complete | inherited from target type |
| G09 Startup doc bundles | preprocess | no | startup docs by member | old startup package current | inherited from target type |
| G10 Pool session bundles | preprocess | no | task-type members | task type from ZIP name only | inherited from target type |
| G11 Reference doc bundles | preprocess | no | architecture/history members | bundle title as authority | inherited from target type |
| G12 TS flow contracts | full_type | provenance only | DTO fields/contract | field invented/wrong flow | 0.10-0.25 |
| G13 Bootstrap seeders | limited | no | bootstrap seed contract | generalized seeder behavior | 0.10-0.25 |
| H01 GraphRAG planning/attribution | full_type | no | RAG-flow attribution | wrong node/flow map | 0.10-0.25 |
| H02 Marketplace docs/state | limited | marketplace prompt only | listing/research state | marketplace as architecture | 0.15-0.30 |
| H03 Certificates/reports | full_type | no | validation verdict | certificate outside scope | 0.10-0.25 |
| H04 Comparison/drift docs | full_type | no | chosen/drift rationale | invented alternative | 0.05-0.20 |
| H05 DoD/prerequisites | full_type | no | criteria/gate order | DoD as validation result | 0.10-0.25 |
| H06 Task type catalogs | full_type | no | task/sub-phase mapping | task catalog as implementation | 0.10-0.25 |
| H07 Phase templates | limited | no | resolved template structure | placeholder as concrete state | inherited from target type |
| H08 Session gap/plan/handoff | full_type | no | subtype-specific artifact | handoff as prompt | 0.10-0.30 |
| I01 Task types catalogs | full_type | no | versioned T-number mapping | stale/nonexistent task type | 0.05-0.20 |
| I02 BFA rules | full_type | no | CF rule compliance | stale/wrong CF rule | 0.10-0.25 |
| I03 Stress-test rules | full_type | no | ST scenario/check | ST rule as CF rule | 0.10-0.25 |
| I04 Merged canonical docs | full_type | no | merged canonical content | part used after merged | 0.05-0.20 |
| I05 Session type taxonomy | full_type | no | session classification | wrong session type | 0.10-0.25 |
| I06 New task prompts | full_type | no | planning template structure | template as A-side | 0.15-0.30 |
| I07 Fleet flow state registry | full_type | no | fleet flow status | per-flow state as fleet | 0.10-0.25 |
| I08 Architect behavior plans | full_type | no | role execution sequence | superseded role plan | 0.15-0.30 |
| I09 Per-flow fix maps | full_type | no | gap-to-flow fix | abstract guidance as fix | 0.10-0.25 |
| I10 Visual proof plans | full_type | no | visual proof target | plan as proof result | 0.10-0.25 |
| I11 Track topology plans | full_type | no | topology repair plan | plan as topology file | 0.10-0.25 |
| I12 Delivery summaries | full_type | no | delivered package summary | summary as plan | 0.15-0.30 |
| I13 DATA_PIPELINE outputs | limited | no | artifact routing only | label as source family | inherited from target type |
| I14 ORCHESTRATION outputs | limited | no | parallel artifact routing | label as source family | inherited from target type |
| K01 Master run instructions | full_type | input instruction | deliverable requirements | prompt as output | 0.15-0.30 |
| K02 Numbered flow session docs | full_type | embedded prompt only | numbered flow context | wrong NN-prefix/deep research | 0.10-0.25 |
| K03 Vxx combined plans | full_type | no | engine version plan | old V-plan current | 0.10-0.25 |
| K04 Unified plans | full_type | no | synthesized system view | partial plan as synthesis | 0.10-0.25 |
| K05 Per-flow execution plans | full_type | no | flow execution sequence | reference plan as execution | 0.15-0.30 |
| K06 Merged master execution plans | full_type | no | canonical merged execution | partial plan after merged | 0.05-0.20 |
| K07 DrawIO XML diagrams | full_type | no | parsed graph nodes/edges | visual-only diagram use | 0.10-0.25 |
| K08 Component CSS debug histories | full_type | original prompt only | component debug correction | component id missing | 0.20-0.45 |
| K09 Definitive module architecture | full_type | no | module boundary | draft as definitive | 0.10-0.25 |
| K10 Engine/domain readiness plans | full_type | no | readiness/fabric sequence | plan as passed result | 0.15-0.30 |
| K11 Self-sufficiency docs | full_type | no | dependency constraint | violation as allowed | 0.10-0.25 |
| K12 Tool maturity lifecycle | full_type | no | maturity transition | stale lifecycle stage | 0.10-0.25 |
| K13 Skills-to-engine dynamic maps | full_type | no | skill-engine mapping | skill name as engine behavior | 0.10-0.25 |
| L01 Skill routing guides | full_type | scenario prompt only | skill chain order | missing/wrong skill chain | 0.10-0.20 |
| L02 DNA application guides | full_type | direct DNA question only | pattern-to-skill application | philosophy used as application rule | 0.10-0.20 |
| L03 Scope isolation guides | full_type | security/scope question only | scope/ownership/query rules | cross-scope access allowed | 0.10-0.25 |
| L04 Phased prompt libraries | full_type | phase prompt only | required prompt files/packages | wrong phase/package | 0.05-0.15 |
| L05 Tool instruction files | full_type | no | tool-specific rule | wrong tool instruction | 0.10-0.25 |
| L06 Skill RAG indexes | full_type | no | skill-to-reference map | session index as permanent map | 0.05-0.10 |
| L07 Secret config templates | full_type | no | sanitized secret/env template | hardcoded credentials | 0.05-0.10 |
| L08 Stack translation plans | full_type | explicit translation prompt only | stack translation/correction | old stack plan current | 0.10-0.25 |
| M01 Batch-summary logs | full_type | no | batch telemetry | RESOURCE_REVIEW as failure | 0.05-0.20 |
| M02 DPO checkpoints | full_type | no | loss/reward/accuracy metrics | progress without checkpoint | 0.05-0.15 |
| M03 Experiment result schemas | full_type | no | schema-valid result fields | missing required field | 0.10-0.25 |
| M04 Session-to-training maps | full_type | prompt with provenance only | prompt/dependency/arbiter map | edge without reason | 0.10-0.25 |
| M05 Training pair YAML | full_type | A block only | B block only | A/B swapped or flattened | 0.10-0.25 |
| M06 Corruption holdouts | full_type | eval prompt only | correct_B/corruption | holdout leaked to training | 0.05-0.20 |
| M07 Rephrase holdouts | full_type | eval prompt only | invariant answer | rephrase as new task | 0.10-0.30 |
| M08 Teaching ledgers | full_type | no | teaching/verification audit | baseline as final | 0.05-0.20 |
| M09 Cycle summaries | full_type | no | cycle verdict/promotion | baseline as final | 0.05-0.20 |
| M10 Flow package manifests | full_type | no | package inventory/warnings | package title as evidence | 0.10-0.25 |
| M11 Flow deep research docs | full_type | direct research question only | research recommendations/contracts | research as implementation proof | 0.10-0.25 |
| M12 Canonical merged refs | full_type | no | canonical merged member | stale/partial merged source | 0.05-0.20 |
| M13 Flow examination results | full_type | direct examination query only | hypothesis verdict/evidence | verdict without evidence | 0.05-0.15 |
| M14 Examination progress state | full_type | no | running tally/progress | stale count | 0.05-0.15 |
| M15 Arbiter analysis passes | full_type | direct arbiter question only | pass-specific finding | earlier pass as current | 0.10-0.20 |
| M16 Retro hypothesis analyses | full_type | direct hypothesis question only | retroactive H verdict | old flow missing new H | 0.10-0.20 |
| M17 Live run evidence | full_type | no | run verdict + execution log | goal reached despite partial | 0.05-0.10 |
| M18 Goal gap reports | full_type | goal query only | GE verdict/evidence | partial GE as success | 0.10-0.20 |
| M19 GUIDE-B07-B12 | full_type | no | document-production guide | wrong GUIDE-B target | 0.15-0.30 |
| M20 Vxx combined packages | full_type | no | session combined plan/state/RAG | partial as combined | 0.10-0.25 |
| M21 Plain-English packages | limited | vocabulary only | linked advice only | translation as canonical | 0.30-0.60 |
| N01 Flow registries | full_type | no | flow existence/dependencies | nonexistent/wrong flow | 0.05-0.20 |
| N02 Engine contract templates | full_type | no | required task contract fields | missing template section | 0.10-0.25 |
| N03 Merge protocols | full_type | no | canonical merge procedure | merged without protocol | 0.10-0.25 |
| N04 Wave assignments | full_type | no | wave membership/order | wrong wave | 0.10-0.25 |
| N05 Fabric interfaces | full_type | no | interface contract/boundary | invented/missing interface | 0.10-0.25 |
| N06 Skill overhaul sessions | full_type | remediation prompt only | batch skill remediation | one skill as whole overhaul | 0.10-0.30 |
| N07 Known violations registries | full_type | no | violation/auto-heal rule | manual-only auto-healed | 0.10-0.25 |
| N08 Foundational principles gates | full_type | no | principle gate rule | convention as principle | 0.10-0.25 |
| N09 Client push/module maps | full_type | no | infrastructure/module graph | fabricated dependency edge | 0.10-0.25 |
| O01 Fleet raw JSONL | full_type | no | per-flow raw inventory row | fabricated/wrong row number | 0.05-0.15 |
| O02 Code-derived process state | full_type | no | code-derived fleet counts | session-declared as code-derived | 0.05-0.20 |
| O03 Flow gaps master plans | full_type | remediation prompt only | gap/root-cause/fix audit | issue tracker as full plan | 0.10-0.25 |
| O04 Step teaching series | full_type | embedded prompt only | step/cycle teaching contract | wrong step/order | 0.10-0.30 |
| O05 Reconciliation state | full_type | no | plan-vs-code reality | old plan as reality | 0.10-0.25 |
| O06 Mobility auth state | full_type | no | auth/portability state | mobility as UX evidence | 0.05-0.20 |
| O07 Adaptation surfaces | full_type | adaptation question only | FREEDOM key surface | MACHINE key as adaptable | 0.05-0.20 |
| O08 Tenant profiles | full_type | no | tenant override instance | profile as surface definition | 0.05-0.20 |
| O09 FC-18 audit trails | full_type | no | portability audit evidence | wrong tenant/FC | 0.10-0.25 |
| O10 Step-8 handoff contracts | full_type | explicit task only | iron rules + I/O contract | missing handoff field | 0.05-0.10 |
| O11 Flow inventory | full_type | no | classification/topology/UI row | inventory as code evidence | 0.05-0.20 |
| O12 Coverage phase analyses | full_type | no | phase coverage finding | wrong coverage phase | 0.10-0.25 |
| O13 UI screenshots | full_type visual | no | linked visual evidence | screenshot name only | n/a |
| O14 Per-flow UX reviews | full_type | no | per-PNG finding/fix | finding without screenshot | 0.10-0.25 |
| O15 UX rollups | full_type | no | aggregate UX counts | rollup as per-PNG detail | 0.10-0.25 |
| O16 Fleet validations | full_type | no | versioned UI validation | old validation current | 0.10-0.25 |
| O17 Session round ZIPs | preprocess | no | extracted member only | empty ZIP as artifact | inherited |
| O18 Business flow specs | full_type | product prompt if preserved | product/deep research input | wrong flow/deep research as proof | 0.10-0.30 |
| O19 RAG benchmark sessions | full_type | benchmark prompt only | benchmark sequence/metrics | ML optimization without baseline | 0.15-0.30 |
| P01 TS service interfaces | full_type | no | TS interface contract | C# shape applied to TS | 0.05-0.20 |
| P02 Machine constants | full_type | no | named runtime rule | constant without rule text | 0.05-0.20 |
| P03 Round gap matrices | full_type | no | round gap coverage row | input plan as output matrix | 0.10-0.25 |
| P04 Round root cause maps | full_type | no | gap-to-root-cause collapse | gap as root cause | 0.10-0.25 |
| P05 HIST RAG batches | full_type | no | conversion record/fixture | raw history without conversion | 0.10-0.25 |
| P06 Claude Code prompts | full_type | actual prompt only | start prompt constraints | wrong tool context | 0.10-0.25 |
| P07 Gap closure plans | full_type | gap-fix request only | concrete closure steps | abstract guidance as closure | 0.10-0.25 |
| P08 Skill registration manifests | full_type | no | active skill/version/status | factory catalog as active | 0.05-0.20 |
| P09 Execution visibility plans | full_type | no | visible state/progress events | plan as implementation proof | 0.10-0.25 |
| P10 Fork flow engine plans | full_type | no | branch/join semantics | linear flow as fork plan | 0.10-0.25 |
| P11 Infra audit docs | full_type | no | infrastructure finding | audit finding as fixed | 0.10-0.25 |
| P12 Dated gap reviews | full_type | no | date-scoped gap review | old review current | 0.10-0.25 |
| Q01 Skills gap remediation docs | full_type | gap prompt only | gap-to-skill remediation | skill name as behavior authority | 0.10-0.25 |
| Q02 Planning startup skill | full_type | no | Q0/startup protocol | startup inferred from name | 0.10-0.30 |
| Q03 Arch decision testing skill | full_type | no | timeframe/testing protocol | decision accepted without testing | 0.10-0.30 |
| Q04 Claude Code integration instructions | full_type | copied prompt only | Claude Code setup/verify steps | Claude.ai instructions substituted | 0.15-0.30 |
| Q05 Verification query sections | full_type | no | executable query assertion | query header as prose only | 0.10-0.25 |
| R01 Planning complete gates | full_type | no | deliverables/approval gate | simulation before approval | 0.10-0.25 |
| R02 Per-round simulation docs | full_type | no | simulation trace/finding | prompt as result | 0.10-0.25 |
| R03 Simulation gap catalogs | full_type | fix prompt only | defect/root-cause/fix class | catalog as implemented fix | 0.05-0.20 |
| R04 Gap process templates | full_type | no | parameterized process template | instance as canonical template | 0.10-0.30 |
| R05 Gap prep sessions | full_type | fixed prompt only | inline context/bash verification | undefined external variables | 0.10-0.30 |
| R06 Blocking gate sessions | full_type | gate prompt only | why-first/gate status | bypassed gate | 0.10-0.25 |
| R07 Named round sessions | full_type | session instruction only | atomic action/state mutation | numeric round without action | 0.10-0.25 |
| R08 Precheck results JSON | full_type | no | PC status/evidence/action | RED ignored | 0.05-0.20 |
| R09 Per-task WHY teaching | full_type | teaching prompt only | WHY reasoning/DPO seeds | execution script as WHY | 0.05-0.20 |
| R10 Cross-flow edge tests | full_type | no | emitted/consumed dependency | dependency without edge spec | 0.05-0.20 |
| R11 Flow input bundles | full_type | product member if preserved | product+research+context package | ZIP title as authority | inherited |
| R12 Gap matrix audits | full_type | no | addition status/evidence | PRESENT without citation | 0.05-0.20 |
| R13 Precheck session docs | full_type | precheck instruction only | runnable gate commands | non-self-contained precheck | 0.10-0.25 |
| S01 Subproject maps | full_type | no | subproject routing/status | nonexistent/wrong subproject | 0.05-0.20 |
| S02 Experiment results maps | full_type | no | experiment status/delta/use-in | fabricated metric/result | 0.05-0.20 |
| S03 Cross-project interfaces | full_type | no | C# interface reference | implementation-inferred interface | 0.05-0.20 |
| S04 Experiment plans | full_type | experiment prompt only | baseline/enhanced/metric/tier | result stated in plan | 0.10-0.25 |
| S05 Subproject plans | full_type | implementation prompt only | subproject dependency plan | XIIGen flow plan substituted | 0.10-0.25 |
| S06 Capability session plans | full_type | goal if preserved | capability execution structure | missing Gate B | 0.10-0.30 |
| S07 Dataset collection plans | full_type | data task if preserved | data sourcing/filter/package | generated rows without scoring | 0.10-0.25 |
| S08 Experiment fix plans | full_type | fix prompt only | failed experiment fix version | old fix plan current | 0.10-0.30 |
| S09 Research hypothesis plans | full_type | research question only | hypothesis/method/failure mode | hypothesis as confirmed result | 0.10-0.25 |
| S10 LLM master plans | full_type | no | project-wide implementation order | old master plan current | 0.10-0.25 |
| S11 Experiment harness docs | full_type | no | harness setup/run procedure | harness spec as result | 0.10-0.25 |
| S12 llm-specific skills | full_type | no | llm_mvp_core skill boundary | skill applied to XIIGen directly | 0.10-0.30 |
| S13 XIIGen-missing skills | full_type after adoption check | no | recovered/adopted skill | llm skill as XIIGen canonical | 0.10-0.30 |
| S14 Shared general skills | limited | no | shared/adopted skill behavior | no adoption boundary | 0.10-0.30 |
| T01 Session briefs | full_type | next-session question only | concrete defects/thresholds | vague improvement note | 0.10-0.25 |
| T02 Learning handoff JSON | full_type | no | routing scores/status | scoreless decision | 0.05-0.20 |
| T03 Session load plans | full_type | no | data contracts/load governance | stale plan/current session | 0.10-0.30 |
| T04 Block plan JSON | full_type | no | ordered gap execution blocks | blocker skipped | 0.05-0.20 |
| T05 Extraction registries | full_type | no | processing counts/status | skipped file as processed | 0.05-0.20 |
| T06 Maintenance scripts | full_type | script prompt only | idempotent migration/seeding | marker ignored | 0.10-0.30 |
| T07 Portability evidence JSON | full_type | no | phase evidence/pass-fail | passed with nonzero scan | 0.05-0.20 |
| T08 Skill registration manifests | full_type | no | skill registration metadata | factory catalog as active | 0.05-0.20 |
| T09 Execution/prep protocols | full_type | no | flow-status procedure | wrong protocol for status | 0.10-0.30 |
| T10 Planning gap reports | full_type | gap-status question only | testing/E2E/observability gap | stale report data | 0.10-0.25 |
| U01 Archetype failure map | full_type | no | score bracket action | near-pass as pass | 0.10-0.25 |
| U02 Blast radius/tagger | full_type | no | severity + file tag gates | interface change tagged LOW | 0.10-0.25 |
| U03 Challenger round design | full_type | no | required challenger roles | missing challenger role | 0.10-0.25 |
| U04 Defense in depth | full_type | no | four-layer validation | single-layer validation | 0.10-0.25 |
| U05 E2E test matrix | full_type | no | eight scenario categories | missing category | 0.10-0.25 |
| U06 Escalation orchestrator | full_type | no | panel aggregation rule | lower rule before block | 0.10-0.25 |
| U07 Freedom machine skill | full_type | classification question only | MACHINE/FREEDOM verdict | guarantee-breaking value as FREEDOM | 0.05-0.20 |
| U08 Investigation handoff skill | full_type | investigation prompt only | handoff sections/state update | missing open questions | 0.10-0.25 |
| U09 Materialization session type | full_type | classification question only | materialization verdict | UI wiring as planning | 0.05-0.20 |
| U10 Mental-debug C# v3 | full_type | no | C# diagnostic route | TS route applied to C# | 0.10-0.25 |
| U11 Principles arbiter | full_type | no | isolated arbiter verdict | prohibited context included | 0.10-0.25 |
| U12 Retroactive development C# | full_type | no | engine fix propagation | output patched directly | 0.10-0.25 |
| U13 Solution scope gate | full_type | scope question only | 5-level scope choice | scope jump without evidence | 0.10-0.25 |
| U14 Subproject completeness checker | full_type | no | two pre-gates | downstream before pre-gates | 0.10-0.25 |
| U15 Subproject examination | full_type | no | three-doc no-code gate | .cs before approval | 0.05-0.20 |
| U16 Temporal behavior | full_type | temporal question only | anchor-phase-formula chain | scheduled archetype substituted | 0.10-0.25 |
| U17 Wave assignment skill | full_type | wave question only | wave/gate model | wrong foundation threshold | 0.10-0.25 |
| U18 Goal completeness skills | full_type | no | FIRST check/goal persistence | other check first | 0.10-0.25 |
| U19 Session prompt adaptation | full_type | adaptation prompt only | governance migration map | direct import without mapping | 0.10-0.25 |
| U20 Investigation handoff docs | full_type | investigation prompt only | discoveries/open questions | handoff as implementation | 0.10-0.25 |
| V01 Project completion state | full_type | no | version/phases/test totals | flow state as project state | 0.05-0.20 |
| V02 DevOps module | full_type | no | NestJS validator services | script as NestJS service | 0.05-0.20 |
| V03 Doc-gen module | full_type | implementation prompt only | generated catalog/readme | static catalog as generator | 0.05-0.20 |
| V04 Learning module | full_type | no | feedback-prompt-model loop | hypothesis as implementation | 0.05-0.20 |
| V05 CI/CD pipeline | full_type | CI prompt only | jobs/cache/test/build order | docker before tests | 0.10-0.25 |
| V06 Production compose | full_type | deploy prompt only | profile/health/env/networks | dev compose as prod | 0.10-0.25 |
| V07 Tenant isolation E2E | full_type | test prompt only | cross-tenant fabric assertions | per-fabric only | 0.05-0.20 |
| V08 Engine introspection pages | full_type | UI prompt only | generation/quality UI | product page as introspection | 0.10-0.25 |
| W01 Single experiment plans | full_type | experiment prompt only | hypothesis/baseline/enhanced/metric/tier | result inside plan | 0.10-0.25 |
| W02 Batch experiment plans | full_type | grouped prompt only | per-experiment records/order | flattened batch | 0.10-0.30 |
| W03 Experiment indexes | full_type | no | plan lookup/run order | index as full spec | 0.05-0.20 |
| W04 Experiment infra plans | full_type | infra prompt only | runner/storage/execution flow | infra as result | 0.10-0.25 |
| W05 Experiment strategy plans | full_type | strategy question only | tracks/priority/rationale | one experiment as strategy | 0.10-0.25 |
| W06 Experiment promotion plans | full_type | promotion prompt only | measured values/next run/pass criterion | promotion without numbers | 0.10-0.25 |
| W07 Session state JSON | full_type | no | llm session state/rules/context | XIIGen state schema applied | 0.05-0.20 |
| W08 Experiment result schemas | full_type | no | required result schema fields | sample run as schema | 0.10-0.25 |
| W09 Experiment runtime state | full_type | no | run metrics/progress | schema as result | 0.05-0.20 |
| W10 Solution parity HTML | preprocess | no | extracted parity rows | raw HTML as pair | inherited |
| W11 Domain subprojects plans | full_type | domain prompt only | grouped subproject/fabric plan | single plan as domain group | 0.10-0.25 |
| W12 Capability enhancement plans | full_type | enhancement prompt only | baseline/enhancement/regression gates | enhancement as new capability | 0.10-0.25 |
| W13 Multi-track strategy plans | full_type | strategy question only | track comparison/rationale | one shared model | 0.10-0.25 |
| W14 LLM master plan deltas | full_type | no | v1->v2 rule delta | v1 as current | 0.10-0.25 |
| W15 Experiment classification rules | full_type | no | verdict class/next action | DATA as ARCH | 0.05-0.20 |
| W16 Gate D capability format | full_type | no | mandatory plan sections | missing Gate B | 0.15-0.35 |
| X01 Tech-specific alternatives | full_type | stack port prompt | stack implementation | wrong stack | 0.10-0.30 |
| X02 Full code extracts | full_type / Type 26 | no | exact API/code surface | simplified example as full | 0.05-0.20 |
| X03 Stack integration recipes | full_type | stack selection prompt | recipe wiring | alternative file as recipe | 0.10-0.25 |
| X04 AI tool configs | full_type / Type 19 | no | tool-specific rules | wrong tool format | 0.10-0.25 |
| X05 Decision philosophy guides | full_type / Type 9 | architecture prompt | abstraction verdict | wrong ladder level | 0.10-0.25 |
| X06 Stack migration methodology | full_type | migration prompt | audit-target-recipe-port | direct translation | 0.15-0.30 |
| X07 Pattern catalogs | full_type | pattern prompt | pattern + code evidence | pattern without evidence | 0.15-0.35 |
| X08 ML simulation blueprints | full_type | ML design prompt | blueprint | metric claim from blueprint | 0.15-0.35 |
| X09 Scale evolution maps | full_type | scale prompt | stays/changes/new abstraction | unchanged higher scale | 0.10-0.25 |
| X10 RAG reference indexes | full_type | lookup prompt | target file lookup | index as source | 0.05-0.15 |
| X11 Use-case template guides | full_type | use-case prompt | recipe/skills/customization | complete plan claim | 0.10-0.25 |
| X12 Boilerplate templates | full_type | scaffold prompt | starter template | explanation as scaffold | 0.15-0.35 |
| X13 Infra templates | limited | scaffold prompt only | placeholder-safe infra | secret/prod claim | 0.15-0.30 |
| X14 Unified entry points | full_type | navigation prompt | persona reading order | summary as detailed source | 0.10-0.25 |
| X15 Skill library master plans | full_type | library purpose prompt | phase/checklist | plan as runtime architecture | 0.15-0.30 |
| X16 Genie package boundary | archive_container | no | member provenance only | ZIP filename as authority | inherited |
| Y01 Capability matrix analysis | full_type | site/module prompt | 20x15 matrix row | stale draft matrix | 0.10-0.25 |
| Y02 Module architecture reference | full_type | module architecture prompt | pattern/index/dependency | site-specific controller | 0.10-0.25 |
| Y03 AI agent implementation guide | full_type | agent implementation prompt | module-skill-manifest sequence | stale draft/site-specific code | 0.15-0.30 |
| Y04 V18 consolidation prompt | full_type | consolidation run prompt | E1-E5 merge sequence | modified preserved skills | 0.15-0.30 |
| Y05 Prompt usage guide | full_type | operator workflow prompt | attach/paste/output order | wrong attachment set | 0.10-0.25 |
| Y06 Translation execution phases | full_type | stack translation prompt | translated Python/RN content | plan or C# source as output | 0.05-0.20 |
| Y07 Translated merged parts | full_type | Python merged-doc prompt | correct part/range | stale C# source | 0.05-0.20 |
| Y08 Sub-phase translation state | full_type | no | chunk audit state | completion without state | 0.10-0.25 |
| Y09 Translation master state | full_type | no | pipeline lifecycle state | sub-phase as master | 0.10-0.25 |
| Y10 Translation final declaration | full_type | numbering prompt | final artifact baseline | wrong SK numbering | 0.05-0.15 |
| AA01 V18 DNA laws | full_type | compliance prompt | law/pattern verdict | law violation | 0.05-0.20 |
| AA02 V18 module architecture | full_type | module prompt | module/index/skills | older module doc | 0.10-0.25 |
| AA03 V18 capability matrix | full_type | site prompt | enabled module list | custom code extension | 0.05-0.20 |
| AA04 Skill composition guide | full_type | agent prompt | module skill sequence | index as guide | 0.10-0.25 |
| AA05 Deployment manifests | full_type | deployment prompt | site recipe | matrix as manifest | 0.10-0.25 |
| AA06 Agent quickstart | full_type | assistant start prompt | pre-code checks | summary as detailed source | 0.10-0.25 |
| AA07 Site config examples | full_type | config prompt | populated JSON | schema as config | 0.05-0.20 |
| AA08 Per-skill alternatives | full_type | stack port prompt | skill-specific alternative | generic alternative | 0.05-0.20 |
| AA09 Skill implementation prompts | full_type | implementation prompt | ordered steps/tests | SKILL.md as order | 0.15-0.35 |
| AA10 Flow definition JSON | full_type | flow JSON prompt | nodes/events/skills | registry as DAG | 0.05-0.20 |
| AA11 Versioned skill indexes | full_type | lookup prompt | V18 module mapping | V17 as V18 | 0.10-0.25 |
| AA12 Alternatives catalog | full_type | selection prompt | option selection | catalog as implementation | 0.10-0.25 |
| AA13 Skill authoring template | full_type | skill creation prompt | filled template | missing placeholders | 0.15-0.35 |
| AA14 Consolidation state | full_type | version delta prompt | added/archived status | wrong delta | 0.10-0.25 |
| AA15 Mid-phase checkpoints | full_type | no | checkpoint status | checkpoint as final | 0.10-0.25 |
| AA16 RAG provider skills | limited | provider prompt | provider constraints | generic RAG replacement | 0.10-0.25 |
| AA17 V18 alias families | limited | no | alias target | duplicate family | inherited |
| AB01 Versioned stack migration plans | full_type | migration prompt | v4 final plan | v3 FLOW-21 scope | 0.10-0.25 |
| AB02 Architect skill packages | full_type | architect prompt | package rules/references | ordinary skill as meta-skill | 0.10-0.25 |
| AB03 Architect reference protocols | full_type | protocol prompt | named reference protocol | SKILL summary as protocol | 0.10-0.30 |
| AB04 V18 consolidation state | full_type | version delta prompt | line-counted package delta | session checkpoint as delta | 0.10-0.25 |
| AB05 Module config schemas | full_type / Type 25 | config prompt | schema-valid fields | instance as schema | 0.05-0.20 |
| AB06 Site config instances | full_type | config prompt | populated site config | schema/tenant profile as config | 0.05-0.20 |
| AB07 Deployment manifest collections | full_type | deployment prompt | YAML manifest/checklist | config as manifest | 0.10-0.25 |
| AC01 TS source deliverables | full_type / Type 26 | no | exported TS contract | typed/throwing service | 0.05-0.20 |
| AC02 TS Jest DNA specs | full_type / Type 28 | no | assertion/DNA rule | prose as test | 0.10-0.25 |
| AC03 Compliance framework | full_type | no | R1-R8 definitions | requirement without criteria | 0.10-0.25 |
| AC04 Compliance gap matrix | full_type | no | flow x requirement cell | stale/wrong cell | 0.05-0.20 |
| AC05 Compliance plan | full_type | plan prompt | remediation phases | spec as plan | 0.10-0.25 |
| AC06 Requirement specs | full_type | no | requirement-specific artifact/test | wrong R-number | 0.10-0.25 |
| AC07 Validation runner | full_type | no | command/gate spec | pass without command | 0.10-0.25 |
| AC08 Integration master guide | full_type | no | canonical routing | wrong target file | 0.05-0.20 |
| AC09 Compliance appendices | full_type / patch | no | append-to block | standalone canonical | 0.10-0.25 |
| AC10 MT research analysis | full_type | no | impact/adaptation plan | research as implementation | 0.15-0.35 |
| AC11 Flow engine extension | full_type | flow extension prompt | authoritative per-flow doc | aggregate as per-flow | 0.10-0.25 |
| AC12 Flow merge master plan | full_type | merge prompt | merge phase/order | completion as plan | 0.10-0.25 |
| AC13 Flow state JSON | full_type | no | renumbering state | inferred offsets | 0.05-0.20 |
| AC14 Renumber map | full_type | no | explicit ID map | generated IDs without map | 0.05-0.15 |
| AC15 Flow validation report | full_type | no | 70-point check row | false pass | 0.10-0.25 |
| AC16 Merge completion report | full_type | no | merged files/next IDs | pre-validation completion | 0.10-0.25 |
| AC17 Per-flow RAG index | full_type | lookup prompt | local lookup | global authority claim | 0.05-0.15 |
| AC18 Post-flow state/consolidation | full_type | flow state prompt | final/consolidation state | planning/final swap | 0.10-0.25 |
| AC19 Plugin examples/index | full_type | plugin prompt | positive/negative/index edge | polarity reversed | 0.10-0.30 |
| AC20 Plugin config/RAG variants | full_type | plugin prompt | JSON config/source map | markdown/index variant swap | 0.10-0.25 |
| AD01 Web-session governance skill | full_type | handoff prompt | executable output split | analysis doc as execution | 0.05-0.20 |
| AD02 Per-flow completion state | full_type | no | FLOW21 metrics/artifacts | project state as flow state | 0.05-0.20 |
| AD03 SDK audit/provider plan | full_type | SDK prompt | provider action | status without audit | 0.10-0.25 |
| AD04 Post-flow RAG index | full_type | lookup prompt | artifact counts/ranges | inferred IDs | 0.05-0.15 |
| AD05 Per-phase flow state | full_type | phase prompt | phase document/test state | final/phase swap | 0.05-0.20 |
| AD06 Merge plan with renumber map | full_type | merge prompt | renumbered ranges | draft numbers | 0.05-0.15 |
| AD07 Canonical validation report | full_type | no | validation matrix row | false all-pass | 0.10-0.25 |
| AD08 Consolidation record pair | full_type | consolidation prompt | before/after evidence | plan/report alone | 0.10-0.25 |
| AE01 Flow-versioned snapshots | archive_container | no | member provenance | ZIP as source | inherited |
| AE02 Canonical flow extensions | full_type | no | append content block | merge plan as content | 0.05-0.20 |
| AE03 Flow final state | full_type | no | post-flow totals | global state as flow final | 0.05-0.20 |
| AE04 Cited domain research | full_type | research prompt | cited analysis | uncited chat as research | 0.15-0.35 |
| AE05 Engine-specific research | full_type | research prompt | implementation guidance | generic research as engine guide | 0.15-0.35 |
| AE06 Q&A research logs | full_type | domain question | module breakdown | cited analysis claim | 0.20-0.40 |
| AE07 V37 adaptive plan | full_type / Type 16 | plan prompt | sub-phase/checkpoint plan | stale V35/V36 | 0.10-0.25 |
| AE08 V37 sub-phase state | full_type / Type 22 | no | subphase status | completion checkpoint | 0.10-0.25 |
| AE09 Approach RAG index | full_type | lookup prompt | library-to-phase lookup | project RAG index | 0.05-0.15 |
| AE10 Session decision RAG index | full_type | decision lookup | decision/rationale/ancestry | implementation source claim | 0.10-0.25 |
| AF01 Supreme planning skills | full_type / Type 20 | no | priority/load-order rule | ordinary skill label | 0.05-0.20 |
| AF02 Examination governance plans | full_type | no | protocol/verdict schema | per-flow result as protocol | 0.10-0.25 |
| AF03 Skeleton fix plans | full_type | fix prompt | skeleton acceptance/phase plan | final plan claim | 0.10-0.25 |
| AF04 Session transcripts | preprocess | human turn only | extracted zone | whole transcript B-side | inherited |
| AF05 Goal anchors | full_type | verbatim goal | scoped goal framing | locked decision claim | 0.05-0.15 |
| AF06 Planning simulations | full_type | planning prompt | DPO triples/edges/seeds | design sim swap | 0.05-0.20 |
| AF07 Deep design simulations | full_type | design prompt | depth trace/corpus output | shallow plan | 0.10-0.25 |
| AF08 TEACH-QA-R0 | full_type | teaching prompt | bootstrap phases/artifacts | review QA swap | 0.05-0.20 |
| AF09 TEACH-QA-R1 corrections | full_type / patch | correction prompt | gap table/appends-to | base R0 missing | 0.05-0.20 |
| AF10 Autonomous execution plans | full_type | execution prompt | gate/schedule/branch | human plan as executor doc | 0.10-0.25 |
| AF11 Simulation patches | full_type / patch | patch prompt | target sim fix | patch without target | 0.05-0.20 |
| AF12 HTML verdict artifacts | preprocess | no | extracted verdict zones | raw HTML as review | inherited |
| AF13 Confirmations | limited_authority | no | existing-family DTO update | duplicate family | inherited |
| AG01 Renumber map JSON | full_type | no | explicit ID maps | draft ID range | 0.05-0.15 |
| AG02 Merge completion reports | full_type | no | applied offset/next IDs | plan as completion | 0.05-0.20 |
| AG03 Flow initiation docs | full_type | raw user intent | bounded design response | response without intent | 0.10-0.30 |
| AG04 Cited flow research | full_type / limited | research prompt | cited recommendations | research as implementation | 0.15-0.35 |
| AG05 Prompt improvement docs | full_type | PromptOps request | schemas/runtime loop | static prompt design | 0.10-0.25 |
| AG06 Early engine extensions | full_type | business story | text DAG/Mini-RAG/phase spec | later merge plan as early source | 0.10-0.25 |

---

## Pre-Submission Checklist For Additional Types

Before any pair from A01-A51, B13-B43, C01-C12, Z41-Z56, D01-D19, E01-E16, F01-F17, G01-G13, H01-H08, I01-I14, J01-J09, K01-K13, L01-L08, M01-M21, N01-N09, O01-O19, P01-P12, Q01-Q05, R01-R13, S01-S14, T01-T10, U01-U20, V01-V08, W01-W16, X01-X16, Y01-Y10, AA01-AA17, AB01-AB07, AC01-AC20, AD01-AD08, AE01-AE10, AF01-AF13, or AG01-AG06 enters a batch, run the full v11 pre-submission
checklist first. This checklist **extends v11; it does not replace it.**

```text
[ ] Family identified: A01-A51, B13-B43, C01-C12, Z41-Z56, D01-D19, E01-E16, F01-F17, G01-G13, H01-H08, I01-I14, J01-J09, K01-K13, L01-L08, M01-M21, N01-N09, O01-O19, P01-P12, Q01-Q05, R01-R13, S01-S14, T01-T10, U01-U20, V01-V08, W01-W16, X01-X16, Y01-Y10, AA01-AA17, AB01-AB07, AC01-AC20, AD01-AD08, AE01-AE10, AF01-AF13, or AG01-AG06
[ ] Registry status recorded: full_type / full_type_candidate / limited_authority / excluded_direct / preprocess_then_reclassify
[ ] If already represented in v11 as Type 25-28, use the numbered type in pair metadata
[ ] Canonicality confirmed: canonical or extracted_copy only
[ ] canonical_confirmed_via recorded
[ ] pair_kind recorded
[ ] rejected_type recorded when a text DPO pair is created
[ ] Archive/container files extracted before classification
[ ] Binary files excluded unless converted by the owning tool
[ ] Secret-bearing files excluded or sanitized before any text extraction
[ ] External/vendored sources routed through external-source registry unless explicitly adopted
[ ] For Z50-Z56 external structural families: source_canonicality=external, structural_pattern_only=true, and no XIIGen-specific B-side authority
[ ] For F01 missing skills: recovered SKILL.md content present before any Type 20 B-side pair is authored
[ ] For G06-G11 archive bundles: content manifest present and member-level classification completed before pair construction
[ ] For G01-G04 runtime artifacts: runtime source id and flow/phase id recorded; not confused with DPO training metrics
[ ] For G12-G13 source contracts: field names and seed records extracted from source code, not prose
[ ] For H01-H08 named session documents: subtype-specific DTO fields present; no broad Type 29/32 label without named artifact kind
[ ] For I01-I14 session-message outputs: thinking/tool-call origin recorded when applicable; generated output shape is not treated as uploaded source without locator
[ ] For I02/I03: CF and ST rule families are not mixed; rule_id and version recorded
[ ] For I04: merged document source versions and continuity check recorded before canonical use
[ ] For I13/I14: session type label routes artifacts only; produced artifacts still classified to concrete families
[ ] For J01-J09 active-project artifacts: project id, archive/member path, and experiment/training DTO fields recorded when applicable
[ ] For K01-K13 bash/view path families: file was observed through command/view path or loaded input; runtime use and exact path recorded
[ ] For K07: DrawIO XML parsed into graph nodes/edges before topology pair construction
[ ] For K08: component_id and corrected/wrong CSS revisions recorded before Type 24 pair construction
[ ] For L01-L08 repository reference families: repository-era canonicality and target tool/stack/pattern identity recorded
[ ] For L07: no plaintext secret value enters pair text; only sanitized `$secret:` / `$env:` references allowed
[ ] For M01-M21 training/examination/live-run artifacts: machine-readable DTO fields parsed from JSON/YAML/JSONL, not paraphrased from filenames
[ ] For M05-M09: training/eval leakage guard recorded; held-out rows are not promoted to training rows
[ ] For M13-M18: MD/JSON companion identity, flow id, hypothesis/goal/verdict id, and evidence locator recorded
[ ] For M19: target GUIDE-B document type recorded; guide not generalized to another artifact
[ ] For M20-M21: package/member provenance recorded; translated package linked to original technical source
[ ] For N01-N09 tool-result reference families: tool_result/view/create_file locator recorded and reference family not inferred from plain filename
[ ] For N02/N03: template/protocol used as procedure authority only; output documents still classified separately
[ ] For N05/N09: interface or module-map graph edges extracted explicitly before source-code/topology pair construction
[ ] For O01-O19 live docs-folder families: flow id, phase/step/tenant/role/screenshot locators recorded as applicable; live docs are not treated as old archive material
[ ] For O07/O08: adaptation surface and tenant profile are not swapped; FREEDOM key provenance is recorded
[ ] For O10: iron rules and input/output contract fields recorded before code-generation pair construction
[ ] For O13/O14: screenshot evidence is linked to UX finding row; PNG filename alone is not accepted
[ ] For O17: empty session ZIP placeholders excluded; non-empty ZIPs extracted to member-level families
[ ] For P01-P12 session-statistics families: session type/matrix/tool_result origin recorded; no type added from count alone
[ ] For P01: language=typescript and interface source path recorded; do not reuse C# Type 26 DTO shape
[ ] For P02: constant_id and rule text recorded; named constant citation alone is insufficient
[ ] For P03/P04: round id and output artifact kind recorded; do not confuse round plan with round output
[ ] For P08: active registration status is not inferred from skill factory catalog
[ ] For Q01-Q05 tool-result header families: parent document and H1/header locator recorded; H1 text alone is not accepted as the full source
[ ] For Q01: gap-to-skill remediation fields include skills_to_edit, new_skill_ids, testing_timeframe, and verification script locator
[ ] For Q02/Q03: full SKILL.md content and version recovered before Type 20 B-side use
[ ] For Q05: executable query body, target index/source, and expected result recorded before Type 28 pair construction
[ ] For R01-R13 docs.zip internal session process families: ZIP member path, flow id, round/session id, and template-vs-instance role recorded
[ ] For R04/R05: parameterized template and instantiated session are not swapped; placeholder substitutions recorded
[ ] For R09: no-code teaching boundary, task type ids, DPO triple count, and RAG seed ids recorded
[ ] For R10: emitted/consumed event contract and both flow ids recorded before cross-flow dependency use
[ ] For S01-S14 llm_mvp_core families: project_id=llm_mvp_core recorded; do not treat llm_mvp_core-specific skills as XIIGen authority without adoption
[ ] For S04: ExperimentId, Baseline, Enhanced, Metric, and Tier all present before experiment-plan pair construction
[ ] For S12-S14: skill adoption status and project applicability recorded before Type 20 pair construction
[ ] For T01-T10 live-governance/evidence families: version, flow id, and machine-readable evidence fields parsed where applicable; latest version wins for versioned governance docs
[ ] For T03/T08/T09/T10: stale governance/report versions used only as Type 16 evolution rejected-side evidence
[ ] For T07: phaseCloseScan and exit codes recorded before portability pass/fail pair construction
[ ] For U01-U20 detailed llm_mvp_core skills: exact skill id and skill-specific DTO fields recorded; broad "skill file" classification is insufficient
[ ] For U15: three required pre-code documents and approval status recorded before any C# implementation pair
[ ] For V01-V08 xiigen-COMPLETE families: archive_id=xiigen-COMPLETE.zip and canonical_nestjs=true recorded; older Python/FLOW33 snapshots are stale when conflicting
[ ] For V07: cross-tenant assertions and all fabric ids recorded before DNA-5 pair construction
[ ] For W01-W16 llm_mvp_core experiment/data contract refinements: experiment id, plan subtype, schema/result/progress role, and mandatory fields recorded; broad "experiment plan" label is insufficient
[ ] For W01/W02: hypothesis, baseline, enhanced, metric, tier, and scale-up/failure rules present before experiment-plan pair construction
[ ] For W07-W09: session state, result schema, and runtime result/progress are not swapped
[ ] For W16: Gate D mandatory sections and Gate B checks recorded before capability-session PLAN pair construction
[ ] For X01-X16 Genie skill library families: project_id=genie_skill_library and archive/member provenance recorded; do not use Genie content as XIIGen-specific authority without explicit adoption
[ ] For X01/X03/X12/X13: stack_id, recipe_combo or template_kind, and supported runtime/placeholders recorded before stack/template pairs
[ ] For X04: tool_name recorded; Cursor/Cline/Copilot/Claude config formats are not swapped
[ ] For X05/X06/X09: ladder/migration/scale level recorded; philosophy or evolution maps are not used as measured runtime evidence
[ ] For X10/X14: index/entrypoint records point to target sources; summary text is not used as detailed implementation authority
[ ] For Y01-Y10 module-architecture and translation families: archive_id=module-architecture.zip or phases_done.zip and composition/translation role recorded; do not collapse matrix, module architecture, agent guide, consolidation prompt, usage guide, translated parts, state, and final declaration into one "architecture doc"
[ ] For Y01/Y03: canonical draft/final status recorded; Phase 1A and AI_AGENT_GUIDE_SKILL_MAPPING are stale when COMPLETE versions exist
[ ] For Y02/Y03: module_id, site_type, config_index, skill ids, and deployment manifest ids recorded before composition pairs
[ ] For Y06/Y07: stack_target, source document/line range, output part, and verification command recorded; C# originals are stale for Python-stack pairs
[ ] For Y08-Y10: sub-phase state, master state, and final declaration are not swapped; PHASE_STATE_FINAL numbering correction is authoritative
[ ] For AA01-AA17 xiigen_v18_complete families: archive_id=xiigen_v18_complete.zip recorded; V18 composition-layer sources are not collapsed into generic Type 20 skill files
[ ] For AA01-AA05: law/module/matrix/composition/manifest ids recorded before architecture or deployment pairs
[ ] For AA07/AA10: populated site config JSON and flow definition JSON are parsed; schema/index/registry prose is not used as the DTO
[ ] For AA08/AA09/AA16: skill_id plus target stack/provider and implementation prompt/test fields recorded before per-skill alternative pairs
[ ] For AA17: alias target recorded; do not create duplicate families for DOC_RAG_INDEX, AI tool configs, or broad alternatives already covered by X/Y
[ ] For AB01-AB07 canonical V18 migration/architect families: canonical anchor recorded (v4 FINAL, xiigen-architect-skill, or xiigen_v18_complete) before pair construction
[ ] For AB01: correction_ids and removed/added scope recorded; v1-v3 plans are stale when v4 FINAL conflicts
[ ] For AB02/AB03: SKILL.md package and named reference protocol file are not swapped
[ ] For AB05-AB07: schema, config instance, and deployment manifest roles are distinct and parsed into their own DTO fields
[ ] For AC01-AC20 compliance/flow-extension/plugin-factory families: family-specific DTO fields from the AC table recorded; broad labels like "flow doc", "state", "test", or "skill index" are insufficient
[ ] For AC03-AC09: requirement_id, target canonical file or append_to field, and pass/fail or validation command fields recorded
[ ] For AC13-AC14: renumbering offsets and explicit map entries recorded before artifact-number pairs
[ ] For AC19-AC20: positive/negative polarity, dependency edges, JSON rule shape, and source-code-to-skill mapping variant recorded
[ ] For AD01-AD08 web-session/flow-state/consolidation families: handoff consumer, per-flow metrics, SDK audit row, artifact ranges, validation matrix, or before/after consolidation fields recorded as applicable
[ ] For AD06-AD07: renumbered artifact ranges and validation matrix evidence are required before any "canonical docs are complete" pair
[ ] For AE01-AE10 canonical extension/V37 planning families: flow boundary, append target, citation type, V37 subphase state, or RAG decision/index DTO recorded as applicable
[ ] For AE02: append_after_anchor, target_canonical_file, save_point_checksum, and content block recorded before canonical-file extension pairs
[ ] For AE04-AE06: cited deep research, engine-specific research, and Q&A chat research logs are not swapped
[ ] For AE09-AE10: RAG index records point to source libraries/decisions and are not used as implementation authority
[ ] For AF01-AF13 flow planning/examination/artifact verdict families: priority/load-order, governance version, simulation round, TEACH-QA round, HTML extraction, or confirmation target DTO recorded as applicable
[ ] For AF04/AF12: raw transcript or HTML artifact is zone-extracted before DPO use; whole-file text is not accepted as a single pair source
[ ] For AF06-AF09: planning simulation, design simulation, TEACH-QA-R0, and TEACH-QA-R1 correction roles are not swapped
[ ] For AG01-AG06 flow input/renumbering/merge families: flow id, raw intent boundary, citation type, standalone renumber map, applied offset, or early extension DTO recorded as applicable
[ ] For AG01/AG02: draft artifact IDs are rejected when explicit renumber maps or completion offsets exist
[ ] For AG03: verbatim user intent and AI design response locators are both preserved before instruction-following pair construction
[ ] Source role recorded: A-side, B-side chosen, rejected source, measurement evidence, provenance only, excluded
[ ] Pair includes source_family, source_canonicality, source_path, and preprocessing_method
[ ] Target numbered type recorded when family reclassifies to Type 2, 4, 8, 13, 19, 22, 23, 25-29, 31, 32, or 34
[ ] DPO/training result fields included when the row is a teaching or measurement row
[ ] Token overlap test recorded when text pair is created
[ ] [WRONG: ...] annotation names the exact violated source rule
```

---

## Last Updated

2026-05-08

