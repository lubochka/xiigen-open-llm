# XIIGen Internal Source Registry
# Version: 1.0 — 2026-05-08
# Supersedes: EXECUTOR-PROMPT-REGISTER-ALL-31-SUB-PHASES-v2.md (as a source registry)

---

## Purpose

This registry maps the internal XIIGen data sources — session history, conversation
export, correction events, and local session files — to training domains, expected
pair quality, and extraction rules.

This is the internal counterpart to EXTERNAL-SOURCE-REGISTRY-v8.md. External sources
provide authority evidence for B sides and natural hard negatives. Internal sources
provide the only authentic A-side language and the highest-quality correction-derived
pairs.

The calibration run 2026-05-06 (40 batches, 683 training rows from XIIGEN-SESSION-1000)
confirmed that internal XIIGen sessions are the primary signal source. The two best
PLAN batches (acc=0.618, 0.771) came from sessions with named artifacts and dependency
language. The worst batch (acc=0.306) came from meta-commentary. This registry encodes
that finding as extraction rules.

---

## Registry Rule

Every source entry states:

```
location on disk
what session rows it covers
which domains and sub-phases it contains
pair types extractable (A, B, C, correction)
expected overlap ratio range
extraction rules and quality gates
excluded uses
```

---

## DPO Quality Rules — apply to all internal sources

**Token overlap test before submitting any pair:**

```
overlap_ratio = shared_tokens(chosen, rejected, not_in_prompt) / total_tokens
overlap_ratio > 0.60 → do not submit
```

**Prompt eligibility for PLAN and ARCH** (at least one required):
- Named artifact: component name, phase number, flow ID, file name
- Dependency language: "before", "after", "depends on", "once X is complete"
- Technical specificity: predicate vocabulary, sub-phase reference, technology name

**Prompts that must not enter training regardless of source:**
```
"Option A is clearly the right path"
"this is grounded in existing documents"
"let's continue with the plan"
"Continue."  "Read this."  "Please load the session."
```

**TrainingText must use compiledText** from `SemanticTeachingPromptCompiler.Compile()`,
not the raw prompt string. This is a hard gate: raw prompt in TrainingText = batch
rejected regardless of accuracy.

---

## Source 1 — XIIGEN-SESSION-1000 (calibration run source)

**Location:**
```
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\
  xiigen-sessions\
```

**What it is:** The primary session export used in calibration run 2026-05-06.
683 training rows spanning XIIGEN-SESSION-1000 rows 11 through 1753. The run
produced 40 batches across BUG (30), PLAN (6), CODE_Q (4), ARCH (0) domains.

**What calibration run showed about this source:**

| Batch range | Session rows | Domain | Mean acc | Notes |
|---|---|---|---|---|
| 1–9 | 11–336 | BUG | 0.275–0.415 | Early sessions; mostly 3.x design work |
| 10–11 | 200–463 | Q/BUG | 0.306–0.430 | Mixed signal |
| 12, 16, 17 | 164–567 | PLAN | 0.449–0.771 | Best batches — named artifacts present |
| 30 | 1235–1419 | PLAN | 0.306 | Worst PLAN batch — meta-commentary |
| 37 | 1388–1753 | PLAN | 0.415 | Abstract planning language |

**Session rows with confirmed positive margin (batch 16, rows ~521–727):**
```
"continue from where you stopped — Plan complete. Now executing Phase 1 —
 Factory Interfaces and Engine Processing"
"Please generate flow 10 files that will include only these specific to
 flow 10 sections: P1Family 27 — Tena..."
"This is excellent feedback! I see now — this gap analysis is critiquing
 the reference document (FLOW03_ENGINE_EXTENSION_PLAN.md...)"
"MVP is implemented in FLOW33_FINAL_COMPLETE.zip. Now if I want to start
 testing and define the access keys..."
```
All contain named artifacts (Flow 10, Flow 33, FLOW03, Phase 1, Factory Interfaces)
or dependency language. Overlap ratio for pairs from these rows: 0.15–0.35.

**Session rows with confirmed negative margin (batch 30, rows ~1235–1419):**
```
"Based on the review and the response you've shared, Option A is clearly
 the right path — the research is too..."
"The plan is structured in five phases, each with a clear gate before
 the next begins. What changed from the..."
"This is the right plan. It's grounded in the docs that already exist,
 it doesn't invent anything new..."
"Now you should be thinking about these not as json - but as an input
 for rag initiation - you need to define..."
```
No named artifacts. No dependency language. Abstract correctness meta-commentary.
`CompiledPredicateIds` near-empty. TrainingText carries no routing signal.

**Sub-phases covered in calibration run:**
1.3, 2.1, 2.4, 3.1, 3.2, 3.3, 3.4, 3.5, 4.1–4.5, 5.1–5.4, 6.1–6.4, 7.1–7.5

**Sub-phases with ZERO batches:**
ARCH domain: 3.1, 3.2, 3.5 appeared in training rows but with BUG/PLAN domain labels,
not as ARCH pairs. Zero dedicated ARCH batches were produced.

**Domains NOT extracted from this source (gaps):**
```
ARCH  — zero batches despite ARCHITECTURE predicate vocabulary being present
        in many rows. Rows exist; pairs were never constructed.
```

**Extraction rules for this source:**

From rows 521–727 (batch 16/17 range) — extract PLAN Type B pairs:
- Prompts containing "Phase X", "Flow N", "execute", "continue from" with technical context
- These are the proven positive-margin prompts. Prioritise.

From rows 1–520 (batches 1–15) — extract ARCH Type A pairs:
- Rows containing "architecture", "component", "boundary", "topology" in the session text
- The domain label in the run was BUG, but the content covers 3.1, 3.2 decisions
- Reclassify as ARCH and construct Type A rejected (implementation vs boundary decision)

From rows 1235–1753 — exclude as PLAN prompts; eligible only for correction events:
- Raw prompt text is ineligible (meta-commentary)
- Correction events in this range may still be usable — check H0/A0/H1/A1 chain

**Expected overlap ratio:** 0.15–0.35 for rows 521–727; 0.40–0.55 for rows 1235–1753
(correction events excluded from this estimate)

**DPO pair types:** A (ARCH rows 1–520), B (PLAN rows 521–727), correction events

**Excluded use:**
```
Do not use rows 1235–1753 as PLAN A sides.
Do not use meta-commentary as the prompt side of any pair.
Do not mix session row text into the compiledText field — the compiler
processes the raw prompt; the session metadata stays in TrainingText prefix only.
```

---

## Source 2 — Claude Export Batch-0000 (968 conversations)

**Location:**
```
C:\Users\lubao\Documents\xiigen\old\
  data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178009-8b897c61-batch-0000.zip

Index (already built):
C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\claude-export\
  batch-0000\index\conversation-index.jsonl
```

**What it is:** Full Claude conversation export. 968 total conversations; 462
assessed as XIIGen-relevant; 92 date buckets with development signal.

**Why this source is highest value for correction events:** The export contains
conversations where Claude produced wrong answers that Luba corrected. These are
pre-made DPO pairs — not constructed, not approximated. The wrong answer (A0) is
what a capable model actually produced; the correction (H1/A1) is ground truth.
Token distributions are naturally non-overlapping. Overlap ratio typically < 0.25.

**Development cycle bundles identified:**

| Bundle ID | Date window | Conversations | Valid A-sides | Corrections | Primary domain |
|---|---|---|---|---|---|
| legacy-docs-skills-from-old-code | 2026-03-10–11 | 2 | 74 | 35 | CODE_Q, ARCH, DOCUMENT |
| figma-to-code-skills-to-xiigen | 2026-03-18–19 | 2 | 163 | 69 | PLAN, ARCH, IMPLEMENT |
| self-learning-cycle-validation | 2026-03-19–20 | 3 | 131 | 35 | PLAN, QA |
| flow-01-station-testing | 2026-03-23–24 | 5 | 209 | 79 | BUG, IMPLEMENT, TEST |
| session-prompt-and-learning-protocol | 2026-03-25 | 4 | 145 | 71 | PLAN, CODE_Q |
| value-proposition-code-gaps | 2026-03-24–26 | 8 | 229 | 104 | ARCH, BUG, PLAN |
| April orchestrator-machine-cycle bundles | 2026-04-xx | — | — | — | ARCH, PLAN |
| May implementation/teaching bundles | 2026-05-xx | — | — | — | All |

**Total correction events across all bundles: ~393 high-signal events.**
These are the primary target for the first extraction pass.

**What each bundle covers by domain:**

*legacy-docs-skills-from-old-code (highest priority for ARCH/CODE_Q):*
```
Covers: architecture extraction from existing code, documentation generation,
        skill-library generation, code understanding
Sub-phases likely: 3.1 (architecture extraction), 2.1 (business capability),
                   6.1 (review), CODE_Q (how does X work in this codebase)
Correction event type: architecture wrong, documentation incomplete, skill wrong
Expected overlap for correction pairs: 0.10–0.20
```

*figma-to-code-skills-to-xiigen (primary PLAN/ARCH source):*
```
Covers: skill adaptation, platform mapping, per-logical-unit testing
Sub-phases likely: 3.1, 3.3, 5.1, PLAN dependency ordering
Correction event type: wrong stack assumption, wrong platform mapping,
                       wrong dependency order — pure Type B material
Expected overlap for correction pairs: 0.15–0.25
```

*flow-01-station-testing (primary BUG/IMPLEMENT source):*
```
Covers: flow execution, station testing, generated vs manual code distinction
Sub-phases likely: 4.1, 5.1, 5.2, 7.1, BUG domain
Correction event type: wrong station output, wrong code generated,
                       false assumption about state
Expected overlap for correction pairs: 0.15–0.30
```

*value-proposition-code-gaps (primary ARCH source):*
```
Covers: product value → code gap analysis → roadmap decisions
Sub-phases likely: 3.1, 3.2, 2.1, ARCH domain
Correction event type: wrong architecture boundary, wrong gap classification
Expected overlap for correction pairs: 0.10–0.20
```

**Streaming extraction requirement (mandatory):**

conversations.json is ~1.1 GB uncompressed. Must be streamed.

```
Allowed:  streaming JSON reader, one conversation at a time
Forbidden: json.loads(z.read("conversations.json"))
           File.ReadAllText("conversations.json")
           loading all messages into memory
```

**Extraction sequence:**
```
Pass 1 — Build conversation-index.jsonl (metadata only, all 968 conversations)
Pass 2 — Group into bundles using date window + delivery evidence
Pass 3 — Extract pair rows from selected bundle only
Pass 4 — Teach one row at a time, measure, write ledger
```

**Correction event extraction rule:**
```
H0: original human prompt  → A side
A0: Claude's wrong answer  → rejected (add [WRONG: {H1 correction text}])
H1: Luba's correction      → annotation source
A1: corrected answer       → chosen

Correction signal phrases: "no", "wrong", "incorrect", "not right",
"you missed", "fix this", "that is not what I asked", "why did you"
```

**A-side eligibility (same as Source 1):**
```
Accept: real development intent, natural wording, typos OK
Reject: "Continue.", "Please load the session.", assistant summaries,
        architecture document sentences rewritten as prompts
```

**Output location:**
```
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\
  claude-export\batch-0000\{conversation-slug}\
    layer-0-a\  layer-0-b\  layer-0-c\  layer-1\  layer-2\  layer-3\
    correction-pairs\  held-out\
```

**Conversation manifest required per processed conversation:**
```yaml
source: "claude-export"
source_batch: "batch-0000"
conversation_uuid: "{uuid}"
valid_a_sides: 0
correction_events: 0
rows_produced: { compiler, domain_classifier, domain_splitter, node_pairs,
                 correction_pairs, held_out }
teaching_status: { taught_rows, promoted_units, rejected_units }
```

**DPO pair types:** A, B, C, correction events
**Expected overlap ratio:** 0.10–0.25 for correction events; 0.15–0.40 for constructed pairs

**Excluded use:**
```
Do not copy account emails or user identity fields into pair rows.
Do not use assistant message text as A sides.
Do not use memory.json content as pair material — vocabulary calibration only.
```

---

## Source 3 — Local XIIGen Session Files (current product sessions)

**Location:**
```
C:\Users\lubao\Documents\xiigen\llm mvp\last product session.txt
  (and any subsequent session files in same directory)
```

**What it is:** The most recent active product session. Contains current
architecture decisions, active development work, and the freshest correction events.
This is the source of the one completed teaching cycle (XIIGEN-SESSION-1000 row 402,
cycle-001, sub-phase 3.3, promoted to Probationary).

**Coverage:**
- Sub-phase 3.3 API Contract Definition: one completed cycle (acc confirmed, PASS result)
- Prompts spanning projects 21–23: internal LLM integration, idle learning, approval flow
- Multi-phase prompts: IMPLEMENT + SECURITY + API_CONTRACT predicates simultaneously

**Why this source is valuable for multi-predicate pairs:**

The prompt that produced the first successful cycle:
```
"So I would like to make currently 21-23 to work with the internal llms as much
as possible, and request at the first phase for an approval to call an external
api for AI assistance - with proper prompt and expected results criterias..."
```
`CompiledPredicateIds: ["API_CONTRACT"]` — single predicate, routed to 3.3.
This is a real developer prompt with technical specificity. Overlap ratio for
pairs derived from this prompt: verified positive margin (PASS result, promoted).

**What to extract from this source:**

Multi-phase prompts (the same prompt covering multiple sub-phases):
```
IMPLEMENT (21-23 internal LLM work) + API_CONTRACT (external API approval) +
SECURITY (approval with criteria) + PLAN (schedule idle learning)
```
Each predicate combination produces a separate Layer 0-B router pair showing
different routing decisions from the same compiled input.

PLAN Type B pairs from the architectural work described in the session:
- "request at the first phase for an approval" → dependency order pair
  (3.4 Security approval before 4.1 implementation)
- "use the internal llm to examine the result, reuse it for learning" →
  dependency order pair (examine before reuse, reuse before schedule)

**Expected overlap ratio:** 0.15–0.35 (technical vocabulary, named components)

**DPO pair types:** A (multi-predicate router pairs), B (dependency ordering), C (domain boundary)

**Excluded use:**
```
Do not use session loading commands as A sides.
Session file may contain personal context — apply same redaction rule as export.
```

---

## Source 4 — Orchestrator Teaching Plan Documents

**Location:**
```
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator teaching plan\
  ORCHESTRATOR-TEACHING-PLAN-v3.md
  TEACHING-MEASUREMENT-GUIDANCE-v4.md
  XIIGEN-DRIVEN-SELF-TEACHING-GUIDANCE-v2.md
  XIIGEN-EXPORT-TEACHING-ADDENDUM-v3.md
  CLAUDE-EXPORT-DEVELOPMENT-CYCLE-BUNDLE-MAP-v1.md
```

**What it is:** Architecture and orchestration planning documents produced during
the XIIGen project. These are NOT A-side sources. They are authority evidence for
B sides and for ArbiterUnit, SchemePlannerUnit, and DomainTranslatorUnit pairs.

**Why this source must not be used as A side:** These documents use formal
architecture vocabulary — precise, structured, document-register language. Real
developer prompts use informal, abbreviated, action-oriented language. Mixing
document text into A sides produces register mismatch that the model cannot
generalise from.

**What to extract:**

For SchemePlannerUnit B sides (dependency ordering authority):
- ORCHESTRATOR-TEACHING-PLAN-v2/v3 contains explicit sub-cycle steps with ordering
  constraints. These are authoritative Type B chosen content.

For ArbiterUnit B sides (correctness criteria):
- Teaching plan stopping rules state MUST conditions: "LoRA adapters must be ≤ 2MB",
  "five consecutive promoted decisions before Trusted state". These become arbiter
  pass/fail criteria for teaching-related arbiter pairs.

For DomainTranslatorUnit B sides (formal vocabulary):
- Sub-phase Purpose fields from DevelopmentLifecycleTopologySeeder.cs (31 sub-phases)
  provide the formal domain vocabulary for translation pairs.

**Expected overlap ratio for pairs using this source:** 0.10–0.20 (distinct vocabularies)

**DPO pair types:** B (SchemePlannerUnit), arbiter pairs
**Excluded use:** Never use as A side. Document text is authority evidence only.

---

## Source 5 — Session Teaching Ledger (cycle-001 result)

**Location:**
```
C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
  xiigen-sessions\2026-05-05-last-product-session-internal-first\
    teaching-ledger.jsonl
    cycle-summary.json
    checkpoints\xiigen-cycle-001\3-3\dpo_step_000001.json
```

**What it is:** The one completed teaching cycle result. Sub-phase 3.3, promoted
to Probationary, DPO step recorded. Contains baseline and post-teaching measurements.

**Teaching result:**
```
Baseline:      KNOWLEDGE_EDGE, elapsed=75.87ms, cpu=12.87%, working_set_delta=1.53MB
Post-teaching: PASS, elapsed=3783.59ms, cpu=11.05%, working_set_delta=56.81MB
Checkpoint:    124 bytes (tiny model, 69,920 params)
Decision:      promoted_to_probationary
```

**What to extract:**

This is not a pair source — it is measurement evidence. Use it as:

For ModelTrainingUnit B sides:
- The `teaching_and_retry` block provides real resource numbers: elapsed, CPU,
  working_set_delta, checkpoint_bytes. These populate the B side for training
  measurement pairs.

For knowledge-edge detection comparison pairs:
- The baseline result (KNOWLEDGE_EDGE for 3.3 before teaching) is the gap branch.
- The post-teaching result (PASS for 3.3) is the registered-flow comparison branch.
- Together they form one complete knowledge-edge detection pair — exactly the
  structure required by Layer 0-C pairs.

For held-out evaluation:
- The DPO step file `dpo_step_000001.json` contains loss and accuracy for step 1.
  Use as anchor evidence for the HeldOutEvaluationUnit held-out row.

**DPO pair types:** ModelTrainingUnit B evidence, knowledge-edge detection pair
**Excluded use:** Do not treat teaching ledger rows as training prompts.

---

## Gap Map by Domain

### ARCH — zero training signal

Zero ARCH batches in calibration run. ARCH vocabulary (architecture, component,
topology, boundary, dependency) appeared in session rows 1–520 but those rows
were labelled BUG/PLAN, not ARCH.

**Primary extraction target:** Source 2 (export), bundles:
- `legacy-docs-skills-from-old-code` — architecture extraction corrections (0.10–0.20)
- `value-proposition-code-gaps` — architecture boundary decisions (0.10–0.20)

**Secondary target:** Source 1, rows 1–520 — reclassify ARCHITECTURE-predicate
rows as ARCH domain and construct Type A rejected (4.1 implementation vs 3.1 boundary).

**Minimum before ARCH gap is closed:** 20 pairs across 3.1, 3.2, 3.5, 6.1,
with reward_margin_mean > 0 confirmed.

### PLAN — 4 of 6 batches negative margin

Root cause: batches 30, 37, 26, 12 used meta-commentary prompts from rows
1235–1753. Batches 16, 17 (rows 521–727) produced positive margin.

**Primary extraction target:** Source 1, rows 521–727 — extend positive-margin
prompt range with additional Type B (dependency ordering) pairs.

**Secondary target:** Source 2 (export), bundle `figma-to-code-skills-to-xiigen`
— skill adaptation prompts with explicit dependency language.

**Minimum before PLAN gap is reduced:** 15 Type B pairs from eligible prompts
(named artifact or dependency language), reward_margin_mean > 0 confirmed.

### CODE_Q — 0 of 4 batches positive margin

Zero positive margin. All CODE_Q pairs had overlap_ratio ~0.70 (negation template).

**Primary extraction target:** Source 2 (export), bundles with "how does X work"
intent verb: `legacy-docs-skills-from-old-code`, `session-prompt-and-learning-protocol`.

**Pair construction:** For every CODE_Q A side, rejected must be an action response
(BUG fix or PLAN step), not another explanation. Overlap ratio target: 0.15–0.30.

### Correction events — not yet extracted

~393 correction events identified in Source 2. None have been extracted.
These are the highest-quality pairs available — pre-made, naturally non-overlapping,
ground truth from real corrections. Expected overlap ratio: < 0.25.

**Immediate action:** Extract correction events from `legacy-docs-skills-from-old-code`
bundle first (35 corrections). Rate each High/Medium/Low before including.

---

## Processing Priority Order

```
Priority 1 — Correction events from Source 2 (all bundles)
  Expected overlap ratio: 0.10–0.25
  Expected margin direction: reliably positive
  Why first: no construction needed, ground truth, strongest signal

Priority 2 — ARCH pairs from Source 1 rows 1–520 (reclassified)
  Expected overlap ratio: 0.10–0.20
  Why second: zero ARCH signal; these rows contain the vocabulary

Priority 3 — PLAN Type B from Source 1 rows 521–727 (extend)
  Expected overlap ratio: 0.15–0.35
  Why third: proven positive margin range; extend it

Priority 4 — CODE_Q pairs from Source 2 export conversations
  Expected overlap ratio: 0.15–0.30
  Why fourth: zero positive margin; needs different rejected type

Priority 5 — Layer 0 adaptive routing pairs (compiler + router)
  Sources 1 and 3
  Why fifth: required before adaptive orchestrator can train
```

---

## Acceptance Gate Per Source

Before pair rows from any source enter a batch:

```
□ Source location verified on disk
□ Prompt eligibility verified (named artifact OR dependency language OR technical specificity)
□ No meta-commentary prompts included
□ Token overlap test applied: overlap_ratio < 0.60
□ [WRONG: ...] annotation specific — names the confusion, not generic
□ For correction events: quality rated High or Medium (not Low/phrasing-only)
□ TrainingText uses compiledText — not raw prompt string
□ conversation_uuid recorded for Source 2 pairs
□ Manifest written for Source 2 bundle before next bundle begins
□ reward_margin_mean >= 0 verified before batch committed
```

---

## Last Updated

2026-05-08.
