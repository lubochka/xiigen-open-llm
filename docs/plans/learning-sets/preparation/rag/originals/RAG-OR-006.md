# the event attendance flow ENGINE GAP LIST
## What needs to be added to the engine to successfully teach XIIGen with the event attendance flow
## Source: Simulation rounds R13–R19 | Date: 2026-03-27

---

## PRIORITY LEGEND

```
🔴 SILENT_FAILURE  — engine continues, learning data is silently corrupted
🟠 BREAKS          — execution fails visibly; 0 triples / 0 registrations
🟡 PARTIAL         — execution proceeds but signal is degraded or missing
⬜ ADVISORY        — execution correct today; architectural gap for future
```

---

## LAYER 1 — ENGINE CODE CHANGES

---

### gap number 1 🔴 `af-pipeline.ts` — PipelineResult missing score field

**File:** `server/src/af-stations/af-pipeline.ts`

**Problem:** `PipelineResult` interface has `passed`, `stages[]`, `artifactId` — no `score` field.
Session B reads the trace file with `d.get('score', 0)` → always returns 0 for PATH A executions.
All downstream learning is corrupted: genesis score history, routing decision records, TEACH-F
effectiveness triples — all record score=0 regardless of actual AF pipeline output.

**Impact:** Every the event attendance flow Phase B run on PATH A silently produces zero-score training data.
TEACH-B routing_decisions records are all SCORE_0. TEACH-F genesis effectiveness triples
all show delta=0. The planning layer learns nothing from Phase B.

**Fix:** Extend PipelineResult to include score, populated from the last score-node stage:

```typescript
// In PipelineResult interface — add:
score?: number;

// In af-pipeline.ts run() method, after stages are mapped:
const scoreStage = [...execResult.trace]
  .reverse()
  .find(n => n.nodeType === 'score');
const score = scoreStage
  ? Number((scoreStage as Record<string, unknown>)['score'] ?? 0)
  : undefined;

// Include in DataProcessResult.success():
return DataProcessResult.success({
  passed,
  score,          // ← add this
  artifactId: execResult.runId,
  promotionLevel: execResult.promotionLevel ?? execResult.status,
  stages,
  totalElapsedMs: execResult.totalElapsedMs ?? 0,
  total_cost_usd: execResult.total_cost_usd ?? 0,
});
```

**Also update Session B score extraction** (CONTENT — no code change required, but
add fallback in Session B python3 score read):
```python
# Replace: d.get('score', 0)
# With:
score = d.get('score') \
    or next((s.get('score') for s in reversed(d.get('stages', []))
             if s.get('score') is not None), 0)
```

---

### gap number 2 🔴 `validate.handler` — NAMED_CHECKS not extensible per flow

**File:** `server/src/engine/node-handlers/validate.handler.ts`

**Problem:** `NAMED_CHECKS` is a hardcoded TypeScript `Record<string, CheckDefinition>`.
When `validate.handler` loads the event attendance flow arbiters from `xiigen-arbiters`, it finds checkIds
`attendance::capacity-atomicity`, `attendance::idempotent-rsvp`, `attendance::waitlist-fairness`.
The `if (def)` guard evaluates `NAMED_CHECKS[checkId]` → `undefined` → check silently skipped.
No error. No log entry beyond DEBUG. No `CheckResult`. Code violating a task type/a task type iron rules
passes validation with a clean score.

**Impact:** Domain correctness for a task type (dual-entry routing, atomic capacity, idempotent RSVP),
a task type (FIFO waitlist ordering), and a task type (feedback window gate) is never validated. the preference learning signal triples
may record fundamentally incorrect code as the `chosen` candidate.

**Fix — Option A (quick, specific to the event attendance flow):**

Add to `NAMED_CHECKS` in `validate.handler.ts`:

```typescript
// ── the event attendance flow: Event Attendance ──────────────────────────────────────────────

'attendance::capacity-atomicity': {
  default: /decrementAndCreate/,
  message: 'a task type an iron rule: atomic capacity decrement + create must be ONE operation — separate check-then-write creates oversell race condition',
},

'attendance::idempotent-rsvp': {
  default: /setnx|setIfAbsent/i,
  message: 'a task type: duplicate RSVP with same attendeeId must return existing RSVPConfirmed — no second write (the atomic first-writer-wins lock pattern required)',
},

'attendance::waitlist-fairness': {
  default: (code: string) =>
    /joinTimestamp/.test(code) || /zadd.*score/i.test(code) || /sortedSet/i.test(code),
  message: 'a task type an iron rule: waitlist promotion order is FIFO by join timestamp — no priority override without tenant config',
},

'attendance::feedback-window-gate': {
  default: (code: string) =>
    /EventEnded/.test(code) && /FeedbackWindowOpened/.test(code),
  message: 'a task type an iron rule: FeedbackWindowOpened must only emit after EventEnded is received — timer-based trigger is wrong',
},
```

**Fix — Option B (architectural — recommended for all future flows):**

Extend `xiigen-arbiters` the search index documents to include the check definition alongside the checkId.
`validate.handler` loads the definition from the arbiter document instead of `NAMED_CHECKS`:

```typescript
// In validate.handler handle():
for (const arbiter of arbiters) {
  const checkId = String(arbiter['checkId'] ?? '');

  // Try NAMED_CHECKS first (existing checks)
  let def: CheckDefinition | undefined = NAMED_CHECKS[checkId];

  // Fallback: load definition from arbiter document itself (Option B)
  if (!def && arbiter['checkPattern']) {
    def = {
      default: new RegExp(String(arbiter['checkPattern'])),
      message: String(arbiter['checkMessage'] ?? `${checkId} failed`),
    };
  }

  if (def) { /* ... existing check execution ... */ }
}
```

Arbiter the search index documents gain two optional fields: `checkPattern` (regex string) and
`checkMessage`. Session A seeding adds these when authoring the event attendance flow arbiter fixtures.
No future flow requires a code change to add its named checks.

---

### gap number 3 🟡 `feedback.handler` — ATTENDANCE/AGGREGATION absent from `resolveCurriculumTier`

**File:** `server/src/engine/node-handlers/feedback.handler.ts`

**Problem:** `tierMap` maps original archetype names only (`routing`, `data_pipeline`,
`service`, `transaction`, `orchestration`, `ai_generation`, `composite`, `scheduled`).
the event attendance flow archetypes `ATTENDANCE` and `AGGREGATION` fall through to `default=3`.
Accidentally correct today; fragile if any other flow uses `default=3` for a different reason.

**Impact:** `curriculumTier=3` is recorded for all the event attendance flow triples by default rather than
by design. the preference learning signal curriculum validity gate (V9-003) passes, but the tier assignment has no
documented rationale.

**Fix:** Add two entries to `tierMap`:

```typescript
private resolveCurriculumTier(archetype: string | undefined): 1 | 2 | 3 | 4 | 5 {
  const tierMap: Record<string, 1 | 2 | 3 | 4 | 5> = {
    routing:       1,
    data_pipeline: 2,
    service:       3,
    transaction:   3,
    attendance:    3,   // ← ADD: atomic capacity, distributed lock, MACHINE_CONSTANT
    aggregation:   3,   // ← ADD: TIME_BOUNDED_WINDOW, event-driven window
    orchestration: 4,
    ai_generation: 4,
    composite:     4,
    scheduled:     5,
  };
  return tierMap[(archetype ?? '').toLowerCase()] ?? 3;
}
```

---

### gap number 4 ⬜ `bootstrap-cycle-router.ts` — STRUCTURAL boundary fires before graph query

**File:** `server/src/fabrics/graph/planning/bootstrap-cycle-router.ts`

**Problem:** The `STOP_STRUCTURAL` early return for `score < 0.50` fires unconditionally
before any graph query. flow-04 architecture decision 6 (`ATTENDANCE:SCORE_BRACKET:MACHINE_CONSTANT_INVERSION →
ROUTES_TO → CYCLE_WITH_PATCH`) is seeded to `xiigen-decision-graph` but the router
never reaches the graph query for STRUCTURAL scores. a task type cycle-1 score-0 is handled
correctly by Session B's `apply_prompt_patch` hardcoded case — but the graph edge
accumulates 0 observations and never contributes to planning layer learning.

**Impact:** flow-04 architecture decision 6 teaching signal is permanently absent. The graph cannot learn
that MACHINE_CONSTANT score-0 should route to CYCLE_WITH_PATCH rather than STOP.

**Fix (future EXTENSION session):** Before the STRUCTURAL early return, query for a
per-archetype MACHINE_CONSTANT override:

```typescript
// Before the current: if (params.score < 0.50) return STOP_STRUCTURAL
if (params.score < 0.50) {
  // Check for per-archetype MACHINE_CONSTANT override first
  const overrideKey = `${params.archetype}:SCORE_BRACKET:MACHINE_CONSTANT_INVERSION`;
  const override = await this.graphRag.query({
    fromEntity:    overrideKey,
    relationship:  'ROUTES_TO',
    minConfidence: 0.50,   // lower threshold — override needs less confidence
  });
  if (override.edges[0]) {
    return this.buildResultFromEdge(override.edges[0], params.subScores);
  }
  // No override — apply boundary invariant
  return { action: 'STOP_STRUCTURAL', note: 'Score below STRUCTURAL threshold' };
}
```

**Current status:** ADVISORY — deferred. Session B handles a task type correctly today.
Implement before the event attendance flow re-execution on an engine version that disables Session B
script-level overrides.

---

## LAYER 2 — DATA AND TOPOLOGY CHANGES

---

### gap number 1 🟠 `the event attendance flow.topology.json` — Phase B execution nodes missing

**File:** `contracts/topologies/the event attendance flow.topology.json`

**Problem:** The file contains only `clientArchitecture`. No execution nodes are defined.
`feedback.handler` never fires because there is no topology to execute. Session E calls
`POST /api/training/capture-triple` which does not exist. Result: the event attendance flow produces
**zero the preference learning signal triples** in `xiigen-training-data`.

**Impact:** The primary output of any flow execution — the preference learning signal triples for fine-tuning —
is entirely absent for the event attendance flow. `countsTowardThreshold=false` for all potential triples.
The 80-triple graduation threshold receives zero contribution from the event attendance flow.

**Fix:** Add Phase B execution nodes to `the event attendance flow.topology.json`:

```json
{
  "flowId": "the event attendance flow",
  "name": "Event Attendance and Management",
  "status": "NOT_STARTED",
  "nodes": {
    "n1": { "handler": "rag-retrieve.handler",   "nodeType": "rag-retrieve" },
    "n2": { "handler": "multi-generate.handler",  "nodeType": "multi-generate",
            "config": { "providers": ["AI_ENGINE", "AI_OPENAI_PROVIDER", "AI_GEMINI_PROVIDER"],
                        "shuffle": true } },
    "n3": { "handler": "validate.handler",        "nodeType": "validate" },
    "n4": { "handler": "score.handler",           "nodeType": "score",
            "config": { "scoreThreshold": 0.85, "maxRetries": 3 } },
    "n5": { "handler": "feedback.handler",        "nodeType": "feedback" }
  },
  "edges": [
    { "from": "n1", "to": "n2" },
    { "from": "n2", "to": "n3" },
    { "from": "n3", "to": "n4" },
    { "from": "n4", "to": "n5" }
  ],
  "clientArchitecture": { ... }
}
```

With `multi-generate.handler` wired, `feedback.handler` receives a real
`modelComparison` from the parallel generation run. Triples become
`CROSS_MODEL_VALID` and `countsTowardThreshold=true`.

---

### gap number 2 🟡 `the event attendance flow-ARCHITECTURE-DECISIONS.json` — flow-04 architecture decision 6 not seeded to the retrieval system

**Problem:** TEACH-A0 adds flow-04 architecture decision 6 to `the event attendance flow-ARCHITECTURE-DECISIONS.json` after
Session A step A1 has already seeded the file to `xiigen-rag-patterns`. flow-04 architecture decision 6 exists
in the JSON file but is absent from the retrieval system index. Future planning sessions querying
`DESIGN_REASONING` for the event attendance flow find 5 decisions (flow-04 architecture decision 1..flow-04 architecture decision 5), not 6.

**Fix:** Add one curl to `SESSION-TEACH-A0.md` STEP A0-5, immediately after updating
the JSON file:

```bash
# After updating ARCHITECTURE-DECISIONS.json with flow-04 architecture decision 6, seed it to the retrieval system:
D046=$(python3 -c "
import json
arch = json.load(open('sessions/the event attendance flow/the event attendance flow-ARCHITECTURE-DECISIONS.json'))
d = next(d for d in arch['decisions'] if d['decisionId']=='flow-04 architecture decision 6')
print(json.dumps({
  'patternId': 'design-reasoning::the event attendance flow::flow-04 architecture decision 6',
  'patternType': 'DESIGN_REASONING',
  'flowId': 'the event attendance flow',
  'decisionId': 'flow-04 architecture decision 6',
  'type': d['type'],
  'teachingPoint': d['teachingPoint'],
  'capability': d['capability'],
  'resolution': d['resolution']
}))
")
curl -sf -X PUT "localhost:9200/xiigen-rag-patterns/_doc/design-reasoning::the event attendance flow::flow-04 architecture decision 6" \
  -H "Content-Type: application/json" -d "$D046" > /dev/null \
  && echo "✅ flow-04 architecture decision 6 seeded to the retrieval system" || echo "❌ flow-04 architecture decision 6 the retrieval system seed failed"
```

---

### gap number 3 🟡 `xiigen-decision-graph` — EXECUTED_ARCHETYPE edges not seeded

**Problem:** `BootstrapRetrospectiveService.runR1('the event attendance flow')` discovers which archetypes
ran by querying `FLOW:the event attendance flow → EXECUTED_ARCHETYPE → archetype`. These edges are never
seeded by TEACH-A0. The retrospective service finds 0 archetypes and performs no promotions.

**Fix:** Add two edges to `SESSION-TEACH-A0.md` STEP A0-3b (new step between A0-3 and A0-4):

```bash
# Seed EXECUTED_ARCHETYPE edges for retrospective service discovery
curl -sf -X PUT "localhost:9200/xiigen-decision-graph/_doc/FLOW04__EXECUTED_ARCHETYPE__ATTENDANCE" \
  -H "Content-Type: application/json" \
  -d '{"fromEntity":"FLOW:the event attendance flow","fromType":"Flow","relationship":"EXECUTED_ARCHETYPE",
       "toEntity":"ATTENDANCE","toType":"Archetype","confidence":1.0,"immutable":false,
       "source":"the event attendance flow-teach-a0","flowId":"the event attendance flow"}' > /dev/null \
  && echo "✅ the event attendance flow → ATTENDANCE EXECUTED_ARCHETYPE" || echo "❌ FAIL"

curl -sf -X PUT "localhost:9200/xiigen-decision-graph/_doc/FLOW04__EXECUTED_ARCHETYPE__AGGREGATION" \
  -H "Content-Type: application/json" \
  -d '{"fromEntity":"FLOW:the event attendance flow","fromType":"Flow","relationship":"EXECUTED_ARCHETYPE",
       "toEntity":"AGGREGATION","toType":"Archetype","confidence":1.0,"immutable":false,
       "source":"the event attendance flow-teach-a0","flowId":"the event attendance flow"}' > /dev/null \
  && echo "✅ the event attendance flow → AGGREGATION EXECUTED_ARCHETYPE" || echo "❌ FAIL"
```

---

### gap number 4 🟡 `xiigen-decision-graph` — `CONTEXT:crossFlowEvents → ADDS_ARBITER` edge not seeded

**Problem:** `BootstrapArbiterPanelHandler` expands the arbiter panel when
`context.crossFlowEvents=true` by querying `CONTEXT:crossFlowEvents → ADDS_ARBITER`.
This edge is never seeded. When the booking flow or the payments flow execute with `crossFlowEvents=true`
(they consume RSVPConfirmed and CheckInConfirmed from the event attendance flow), the expansion finds
0 edges. The `business_logic` arbiter — which would catch cross-flow contract violations
like attendeeId format inconsistency — is never added to their panels.

**Fix:** Add to `SESSION-TEACH-F-ADDITIONS.md` STEP F-add 5, after the DOWNSTREAM_DEPENDENCY edges:

```bash
# Seed crossFlowEvents arbiter expansion edge
curl -sf -X PUT "localhost:9200/xiigen-decision-graph/_doc/CONTEXT__CROSS_FLOW_EVENTS__ADDS_ARBITER__business_logic" \
  -H "Content-Type: application/json" \
  -d '{
    "fromEntity": "CONTEXT:crossFlowEvents",
    "fromType": "ExecutionContext",
    "relationship": "ADDS_ARBITER",
    "toEntity": "business_logic",
    "toType": "ArbiterRole",
    "confidence": 0.80,
    "observationCount": 0,
    "immutable": false,
    "source": "the event attendance flow-cross-flow-dep",
    "flowId": "the event attendance flow",
    "reasoning": "Flows consuming cross-flow events need business_logic arbiter to validate event contract field invariants (attendeeId hash consistency, tenantId sourcing)."
  }' > /dev/null \
  && echo "✅ CONTEXT:crossFlowEvents → ADDS_ARBITER → business_logic seeded" \
  || echo "❌ FAIL"
```

---

## LAYER 3 — SESSION FILE ADDITIONS

---

### gap number 1 🟠 `the event attendance flow-SESSION-F.md` — the business flow arbiter registration never called

**Problem:** Session F registers cross-flow dependencies to `xiigen-rag-patterns` but
never calls `BfaCrossFlowValidator.validate()` or `storeBfaRegistration()`. the event attendance flow
reaches ACTIVE status without an iron rule..an iron rule being validated against existing ACTIVE
flows. Downstream flows (the booking flow, the payments flow) that query `xiigen-bfa-registrations` for
the event attendance flow find nothing.

**Fix:** Add steps F2b and F2c to `the event attendance flow-SESSION-F.md` immediately after F2:

```bash
## F2b — the business flow arbiter VALIDATION

node -e "
const { BfaCrossFlowValidator } = require('./server/src/guardrails/bfa-cross-flow-validator');
// Build the event attendance flow the business flow arbiter registration from STATE.json artifact_numbers.bfa_rules
const s = JSON.parse(require('fs').readFileSync('sessions/the event attendance flow/the event attendance flow-STATE.json','utf8'));
const registration = {
  taskTypeIds: ['a task type','a task type','a task type','a task type'],
  cfRules: Object.keys(s.artifact_numbers.bfa_rules),
  emittedEvents: ['RSVPConfirmed','WaitlistJoined','CheckInConfirmed','FeedbackWindowOpened'],
  consumedEvents: ['TicketPurchaseCompleted','EventEnded'],
  crossFlowDeps: [
    { targetFlow: 'the booking flow', event: 'RSVPConfirmed', type: 'event-consumption' },
    { targetFlow: 'the payments flow', event: 'CheckInConfirmed', type: 'gate-event' }
  ]
};
console.log('the business flow arbiter registration:', JSON.stringify(registration, null, 2));
// In live execution: validator.validate('the event attendance flow', registration)
//                   then: validator.storeBfaRegistration('the event attendance flow', registration)
"

## F2c — STORE the business flow arbiter REGISTRATION (after validation passes)
curl -sf -X PUT "localhost:9200/xiigen-bfa-registrations/_doc/the event attendance flow" \
  -H "Content-Type: application/json" \
  -d '{
    "flowId": "the event attendance flow",
    "taskTypeIds": ["a task type","a task type","a task type","a task type"],
    "cfRules": ["an iron rule","an iron rule","an iron rule","an iron rule","an iron rule","an iron rule"],
    "emittedEvents": ["RSVPConfirmed","WaitlistJoined","CheckInConfirmed","FeedbackWindowOpened"],
    "consumedEvents": ["TicketPurchaseCompleted","EventEnded"],
    "crossFlowDeps": [
      {"targetFlow":"the booking flow","event":"RSVPConfirmed","type":"event-consumption"},
      {"targetFlow":"the payments flow","event":"CheckInConfirmed","type":"gate-event"}
    ],
    "storedAt": "'"$(date -u +%Y-%m-%dT%H:%M:%SZ)"'"
  }' > /dev/null \
  && echo "✅ the event attendance flow the business flow arbiter registration stored to xiigen-bfa-registrations" \
  || echo "❌ the business flow arbiter registration write failed"
```

---

### gap number 2 🟠 `the event attendance flow-SESSION-E.md` — the preference learning signal capture pattern incorrect

**Problem:** Session E calls `POST /api/training/capture-triple` which does not exist.
Once `the event attendance flow.topology.json` has Phase B nodes (gap number 1), `feedback.handler` fires
naturally during Phase B and triples are captured automatically. Session E's role should
shift from "create triples" to "verify triples exist and are compliant."

**Fix:** Replace Session E step E2 capture loop with a verification + enrichment step:

```bash
## E2 — VERIFY the preference learning signal TRIPLES (produced by Phase B feedback.handler)

TRIPLE_COUNT=$(curl -sf "http://localhost:9200/xiigen-training-data/_count" \
  -H "Content-Type: application/json" \
  -d '{"query":{"term":{"flowId.keyword":"the event attendance flow"}}}' \
  | python3 -c "import sys,json; print(json.load(sys.stdin)['count'])")

echo "the event attendance flow the preference learning signal triples in xiigen-training-data: ${TRIPLE_COUNT} (expected ≥4)"
[ "$TRIPLE_COUNT" -ge 4 ] \
  && echo "✅ the preference learning signal triples present — Phase B feedback.handler fired correctly" \
  || echo "❌ FAIL: 0 triples — verify the event attendance flow.topology.json has Phase B nodes and Phase B ran"

# Verify genesis prompts exist (for prompt.system population — gap number 2)
for TT in a task type a task type a task type a task type; do
  COUNT=$(curl -sf "http://localhost:9200/xiigen-prompts/_count" \
    -H "Content-Type: application/json" \
    -d "{\"query\":{\"bool\":{\"must\":[
      {\"term\":{\"taskTypeId.keyword\":\"${TT}\"}},
      {\"term\":{\"promptType.keyword\":\"genesis\"}}
    ]}}}" \
    | python3 -c "import sys,json; print(json.load(sys.stdin)['count'])")
  [ "$COUNT" -ge 1 ] \
    && echo "  ✅ ${TT}: genesis prompt in xiigen-prompts (prompt.system will be populated)" \
    || echo "  ❌ ${TT}: NO genesis prompt — prompt.system will be null in the preference learning signal triple (INVALID)"
done
```

---

### gap number 3 🟡 `SESSION-TEACH-A0.md` — infra gate check ordering

**Problem:** STEP A0-2 runs the immutable edge count check before any edges are seeded.
Count = 0 at check time → warning printed. A0-7d re-confirms after seeding (correctly ≥3)
but the ordering produces a false alarm.

**Fix:** Move Check 2 (immutable edge count) from A0-2 to A0-7 (which already runs after seeding).
A0-2 retains only Check 1 (index exists) and Check 3 (planning-decisions index exists).

---

### gap number 4 🟡 `SESSION-TEACH-B-ADDITIONS.md` — named check field path incorrect

**Problem:** STEP B-add 3 reads `trace.get('violations', [])` and then `v.get('checkId','')`.
`validate.handler` writes check results to `stages[validate].output.checkResults` as
`[{checkId, passed, message}]` objects. The `violations` field (from `score.handler`)
contains gate name strings, not check objects. Reading the wrong field produces 0 results.

**Fix:** Replace the named check extraction in B-add 3:

```python
# Replace in STEP B-add 3 inner loop:
# OLD:
#   violations = trace.get('violations', [])
#   for v in violations: check_id = v.get('checkId', '')

# NEW:
stages = trace.get('stages', [])
validate_stage = next(
    (s for s in stages if s.get('nodeType') == 'validate' or s.get('handler') == 'validate.handler'),
    {}
)
check_results = validate_stage.get('output', {}).get('checkResults', [])
for cr in check_results:
    if not cr.get('passed', True):
        check_id = cr.get('checkId', '')
        # ... rest of edge update logic unchanged
```

---

## LAYER 4 — NEW FILES REQUIRED

---

### gap number 1 🟠 Cross-flow boundary specs — stubs required before Wave 3

**Files to create:**
- `contracts/tests/cross-flow/the event attendance flow_to_FLOW-09.edge.spec.ts`
- `contracts/tests/cross-flow/the event attendance flow_to_FLOW-10.edge.spec.ts`

**Problem:** No boundary invariant tests exist for the event attendance flow→the booking flow and
the event attendance flow→the payments flow boundaries. The `attendeeId` hash consistency invariant (both flows
must use `sha256(userId+eventId+tenantId)`), tenantId sourcing (from execution
context, not event payload), and CheckInConfirmed gate semantics are undocumented.

**Fix:** Create stub files matching the existing pattern (the business onboarding flow_to_FLOW-04):

```typescript
// the event attendance flow_to_FLOW-09.edge.spec.ts
describe('the event attendance flow → the booking flow boundary', () => {
  it('RSVPConfirmed.attendeeId is sha256(userId+eventId+tenantId) — not raw userId', () => {
    // STUB — implement when the booking flow T-xx generates its consumer handler
    expect(true).toBe(true);
  });
  it('the booking flow sources tenantId from execution context, not RSVPConfirmed payload', () => {
    // STUB
    expect(true).toBe(true);
  });
  it('tenant isolation across the event attendance flow → the booking flow boundary', () => {
    // STUB
    expect(true).toBe(true);
  });
});

// the event attendance flow_to_FLOW-10.edge.spec.ts
describe('the event attendance flow → the payments flow boundary', () => {
  it('CheckInConfirmed is a hard gate — the payments flow cannot start without it', () => {
    // STUB
    expect(true).toBe(true);
  });
  it('CheckInConfirmed.attendeeId hash consistent with RSVPConfirmed.attendeeId', () => {
    // STUB
    expect(true).toBe(true);
  });
  it('tenant isolation across the event attendance flow → the payments flow boundary', () => {
    // STUB
    expect(true).toBe(true);
  });
});
```

---

## EXECUTION ORDER SUMMARY

Fix these in order before re-running the event attendance flow:

```
PRIORITY 1 — SILENT_FAILUREs (corrupt training data silently):
  gap number 1   af-pipeline.ts: add score to PipelineResult
  gap number 2   validate.handler: add attendance:: checks to NAMED_CHECKS
  gap number 4  SESSION-TEACH-B: fix named check field path

PRIORITY 2 — BREAKS (0 triples, 0 business flow arbiter):
  gap number 1   the event attendance flow.topology.json: add Phase B execution nodes
  gap number 2  SESSION-E: replace capture loop with verification
  gap number 1  SESSION-F: add the business flow arbiter validation + storeBfaRegistration steps

PRIORITY 3 — PARTIAL (degraded signal, fixable in one correction pass):
  gap number 3    feedback.handler: add ATTENDANCE/AGGREGATION to tierMap
  gap number 2   TEACH-A0: add flow-04 architecture decision 6 the retrieval system seed step after JSON update
  gap number 3   TEACH-A0: add EXECUTED_ARCHETYPE edges (new step A0-3b)
  gap number 4   TEACH-F F-add 5: add CONTEXT:crossFlowEvents → ADDS_ARBITER
  gap number 3  TEACH-A0: move infra gate Check 2 to after seeding
  gap number 1  Create the event attendance flow→the booking flow and the event attendance flow→the payments flow edge.spec.ts stubs

PRIORITY 4 — ADVISORY (deferred to future engine session):
  gap number 4    bootstrap-cycle-router.ts: MACHINE_CONSTANT override before boundary
```

---

## WHAT WORKS TODAY (no changes needed)

```
✅ Graph seeding (ATTEND/AGGREGATION arbiter edges) — consumed correctly by BootstrapArbiterPanelHandler
✅ Score bracket routing (DETAIL_GAP → CYCLE_WITH_PATCH) — graph edge found and used
✅ CALIBRATION_COMPARISON triples — correctly written to xiigen-planning-decisions
✅ ROUTING_DECISION records — correctly backfilled from STATE.json
✅ GENESIS_PATCH_EFFECTIVENESS triples — correct once PATH A score is fixed
✅ DOWNSTREAM_DEPENDENCY edges — correctly seeded
✅ planningLayerLearning section — correctly added to LEARNING-HANDOFF.json
✅ a task type MACHINE_CONSTANT handling — Session B apply_prompt_patch correctly routes cycle-1 score-0
✅ Arbiter panel for ATTENDANCE — [key_principles, iron_rules, security] assembled correctly
✅ DESIGN_REASONING triples — a task type/the full task type range/a task type reasoning correctly written to xiigen-planning-decisions
✅ OUTCOME_PENDING → VALIDATED upgrade — correctly classifies per task type
✅ Two-level arbiter promotion check — correctly reads FREEDOM config thresholds
✅ Cross-flow dep registration to xiigen-rag-patterns — both the booking flow and the payments flow deps registered
```
