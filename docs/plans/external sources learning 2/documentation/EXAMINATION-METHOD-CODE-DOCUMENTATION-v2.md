# Examination Method — Code Documentation DPO Domain
# Version: 2.0 — 2026-05-10
# Supersedes: EXAMINATION-METHOD-CODE-DOCUMENTATION-v1.md
# Domain: code-documentation
# Authority: CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md
#             PLAN-CODE-DOCUMENTATION-DPO-v2.md
# Companion to: EXAMINATION-METHOD-ORCHESTRATION-v15.md
#               DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v8.md
#               XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md
# Operational runbook:
#               TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md

---

## What Changed from v1

v1 defined the processing cycle and per-source extraction rules but treated
negative example construction as a uniform four-type taxonomy. That was
insufficient.

The key insight from the internal guidance calibration run: 75% of batches
had negative reward margin because the rejected side shared too many tokens
with the chosen side. The failure mode was not random — it was structural.
Different document types produce different chosen-side vocabulary, and the
rejected side must be constructed differently for each to produce genuine
distributional separation.

v2 adds per-document-structure rejected construction rules. Every document
type that appears in the approved sources is classified by its structural
characteristics, and each classification carries its own rejected construction
protocol, expected overlap range, and hard negative patterns. This is the
parallel of the "rejected construction rules by domain" section in
XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md, written for the code-documentation domain.

---

## 2026-05-11 Run-Experience Addendum

The first large generated code-documentation archive was structurally useful
but not train-ready. The audit result changes the examination method:

```
Audited archive:              documentation/learning sets.zip
Rows:                         50,000 CODE_TO_DOC rows
JSON validity:                pass
Unique pair IDs:              pass
chosen == rejected:           4,660 rows
mojibake in train fields:     4,912 rows
strict safe rows:             3,921 rows
```

Therefore, examination now has two layers:

1. Document-structure reasoning still decides how a rejected side should be
   constructed.
2. Training-field hygiene decides whether the row may enter DPO at all.

No row enters train-ready unless both layers pass.

---

## The Quality Standard — Active Throughout Every Examination

A chosen side is clever documentation when it answers all three:

```
INTENT    — what does this code accomplish at the purpose level?
WHY       — what design decision or trade-off is embedded here?
EDGE CASE — what non-obvious condition changes behavior?
```

Curriculum tier 1: all three covered.
Curriculum tier 2: intent and why.
Curriculum tier 3: intent only — minimum acceptable chosen side.
None covered: discard from chosen pool. May be used as Type 1 rejected material.

The standard does not change by document type. What changes by document type
is the structural shape of the explanation prose and therefore how the rejected
side must be constructed to produce genuine distributional separation.

---

## Reward Margin Mechanics — Why Construction Varies by Document Type

From calibration run 2026-05-06: 29 of 40 batches had negative reward margin.
The root cause was not pair selection — it was that rejected shared too many
tokens with chosen because both described the same technical content.

For CODE_TO_DOC pairs, this risk is higher than for ARCH and PLAN pairs because
code documentation pairs share a code snippet as context. Both chosen and rejected
are responding to the same prompt (the snippet). Without structural discipline,
a mechanical paraphrase and a clever explanation of the same snippet will share
30–50% of their tokens — borderline at best, negative at worst.

The only reliable fix is constructing the rejected side from a vocabulary that
is structurally different from the chosen side. That structure comes from the
document type — how it is written, at what level of abstraction, for what audience.

Token overlap test — applies to every pair before batch assembly:

```
shared = tokens in both chosen and rejected NOT in the prompt (snippet)
total  = tokens in chosen + tokens in rejected − tokens in prompt
overlap_ratio = shared / total

chosen.trim() == rejected.trim() -> hard reject even if overlap_ratio is low
mojibake in prompt/chosen/rejected -> repair before training
> 0.60 → discard. Do not submit.
0.50–0.60 → keep only if annotation_strong is true AND annotation
            introduces ≥ 5 consequence tokens not in rejected.
< 0.50 → proceed.
```

Expected overlap by rejected type:

| Rejected type | Expected overlap | Margin |
|---|---|---|
| Type 2 — name echo | 0.05–0.20 | Reliably positive |
| Type 3 — wrong level | 0.15–0.35 | Reliably positive |
| Type 1 — mechanical paraphrase | 0.30–0.45 | Positive with annotation |
| Type 4 — missing edge case | 0.40–0.55 | Positive only with strong annotation |
| Generic negation | 0.65–0.80 | Reliably negative — forbidden |

Identity and encoding gates run before the overlap gate. Overlap can miss a
broken pair when chosen and rejected are identical but short, and it cannot tell
whether tokens like `Ã¢ÂÂ¦` are accidental encoding damage. These are separate
hard gates.

---

## Document Structure Classification

Before constructing any rejected side, classify the source document by its
structural type. The classification determines which rejected construction
protocol applies.

Every source in the external registry belongs to one of these document
structures. The structure is determined by examining three files per source
in Phase 2a.

```
STRUCT-A — Tutorial narrative
  Structure: prose explanation → code example → prose elaboration.
  Audience: learners encountering a concept for the first time.
  Chosen vocabulary: purpose verbs, conceptual framing, "this allows you to",
  "the benefit of", "notice that", "important".
  Natural tier: 1 and 2 — tutorials tend to explain intent and reason.
  Rejected risk: explanation is often so complete that mechanical paraphrase
  overlaps too much. Must use name echo or wrong level as primary rejected type.

STRUCT-B — API reference with examples
  Structure: method/function signature → parameter table → code example →
  brief description.
  Audience: developers who already know the concept and need the exact API.
  Chosen vocabulary: field names, return types, exception names, parameter
  constraints. Prose is brief and precise.
  Natural tier: 3 — API reference tends to cover only intent (what does this
  return?). Why and edge case are rare.
  Rejected risk: the brief prose is already close to a name echo. Must use
  wrong level as primary rejected type to achieve sufficient distributional
  separation.

STRUCT-C — Conceptual guide
  Structure: explanation of a concept → when to use it → multiple code
  examples → comparison with alternatives.
  Audience: developers choosing between approaches.
  Chosen vocabulary: comparison language, trade-off framing, "prefer when",
  "unlike", "this approach avoids", "the cost is".
  Natural tier: 1 — conceptual guides naturally cover intent, why, and often
  edge cases.
  Rejected risk: low — conceptual guide chosen sides have rich vocabulary
  that diverges strongly from mechanical description. Missing edge case is
  the primary pair type because guides often discuss the happy path in detail
  and mention edge cases only briefly.

STRUCT-D — Step-by-step implementation guide
  Structure: numbered steps → code snippet per step → brief explanation.
  Audience: developers following a specific implementation procedure.
  Chosen vocabulary: sequencing language, "this step", "after completing",
  "this enables the next step". Explanation is step-scoped.
  Natural tier: 2 — implementation guides explain intent and often say why
  the step is placed here, but edge cases across step boundaries are rare.
  Rejected risk: mechanical paraphrase overlaps heavily because the explanation
  already describes what the step does. Must use name echo or wrong level.

STRUCT-E — Troubleshooting or error reference
  Structure: error message or symptom → cause → fix.
  Audience: developers encountering a specific error.
  Chosen vocabulary: causal language, "happens when", "caused by", "to fix",
  "the underlying issue is", "prevents".
  Natural tier: 1 — troubleshooting docs naturally cover intent (what the fix
  does), why (what causes the error), and edge case (when the fix does not apply).
  Rejected risk: mechanical paraphrase of a fix description produces low overlap
  because the vocabulary of the cause and the vocabulary of the mechanical
  description are different. Missing edge case is high-value here — many
  troubleshooting docs describe the common case but not the variant conditions.

STRUCT-F — Code sample repository (samples, examples, demos)
  Structure: standalone code file with inline comments or README per sample.
  Audience: developers using samples as starting points.
  Chosen vocabulary: depends on inline comments — ranges from terse to verbose.
  Natural tier: variable — samples with rich comments may reach tier 1;
  samples with sparse comments reach only tier 3.
  Rejected risk: high — sparse comments produce tier 3 chosen sides that are
  already close to a name echo. Must be scored carefully; many samples will
  not produce acceptable chosen sides and should be discarded.
```

---

## Per-Document-Structure Rejected Construction Protocol

This is the core section. For each document structure, the rules below define
how to construct the rejected side, which type to use when, what overlap to
expect, and how to write annotations that produce positive margin.

---

### STRUCT-A — Tutorial Narrative

**Chosen side characteristics:**
The tutorial explanation is the richest chosen side in the corpus. Tutorials
are written to be understood, which means they naturally cover intent and
reason. The prose uses purpose verbs and often explains the design decision
behind the code example. Edge case coverage varies — some tutorials include
"be aware that..." or "note that..." paragraphs; others do not.

**Why standard rejected types risk failure here:**
Because tutorial prose is thorough, a Type 1 (mechanical paraphrase) rejected
side describing the same operations tends to overlap at 0.40–0.55 — borderline.
The tutorial's explanation already uses fairly plain language, and a mechanical
version of the same explanation shares most of that vocabulary.

**Primary rejected type for STRUCT-A: Type 2 (name echo)**

Name echo produces the lowest overlap ratio regardless of how thorough the
chosen side is, because the echo ignores all the chosen-side vocabulary and
reduces to a single sentence. For every tutorial pair, produce a Type 2 as
the primary rejected.

Construction rule:
```
Identify the primary subject of the code snippet: the method being called,
the class being instantiated, the query being executed.
Rephrase the identifier in a single sentence without adding any information.
The sentence must be grammatically complete and end with a period.
Nothing else.
```

Expected overlap for STRUCT-A Type 2: 0.05–0.15.

**Secondary rejected type for STRUCT-A: Type 4 (missing edge case)**

When the tutorial explanation covers an edge case (edge_case_covered: true),
produce a Type 4 as a secondary pair. Remove only the edge case sentences.
Do not alter anything else.

Construction rule:
```
Locate every sentence in the chosen side containing: unless, when empty,
if null, throws when, returns null if, note that, be aware, cancellation,
disposed, be careful, warning, however, except when, only if.
Remove those sentences completely.
Ensure the remaining text is grammatically complete (fix connective words
if needed — "first X. Then Y" should not become "first X. Y" with a
dangling "then").
Do not add any new content.
Compute overlap_ratio after removal.
If overlap_ratio > 0.55: the tutorial explanation is too edge-case-heavy
for Type 4 to work. Switch to Type 2 as primary and omit Type 4 for this pair.
```

Expected overlap for STRUCT-A Type 4: 0.40–0.52.

Identity gate for Type 4:

```
If chosen.trim() == rejected.trim(), the edge-case removal failed.
Do not submit. Rebuild as Type 2 or Type 3, or move to repair-needed with
reason: missing-edge-case-removal-failed.
```

The exact-identity check is mandatory for every Type 4 pair. The 2026-05-11
archive audit found 4,660 Type 4 rows where chosen and rejected were identical.
Those rows are worse than weak rows: they contain no DPO preference direction.

**Annotation for STRUCT-A pairs:**

Type 2 annotation template:
```
[WRONG: "name echo" restates the {class/method/concept name} as a sentence.
The reader learns nothing about {the purpose the code serves / the design
decision it embeds / the condition that changes its behavior}.
A developer reading only this could not explain to a colleague why this
code exists or when they should use it.]
```

Type 4 annotation template:
```
[WRONG: "missing edge case" correctly explains what {the method/query/component}
does and why this approach was chosen, but omits {the specific edge case}.
A caller following only this documentation would {consequence — e.g. not know
to check the return value for null / not cancel the operation on timeout /
not handle the empty collection case}.]
```

**Hard negative for STRUCT-A:**

Tutorial pairs produce a hard negative class unique to this structure: the
tutorial explanation covers only the happy path in great detail, making the
"missing edge case" rejected look almost identical to the chosen side.

Rule: when the tutorial explanation is very long (over 200 words) and covers
the happy path thoroughly, do not attempt Type 4. The overlap will exceed 0.55.
Use Type 2 (name echo, extremely low overlap) and add one separate Type 1
(mechanical paraphrase) pair that narrates the tutorial's steps more briefly
than the tutorial itself.

For Type 1 on long STRUCT-A explanations, the mechanical paraphrase must be
significantly shorter than the chosen side — 40% of the word count or less.
This forces the vocabulary to diverge (the shorter version drops the purpose
and reason language, keeping only the operations) while keeping overlap below 0.45.

---

### STRUCT-B — API Reference with Examples

**Chosen side characteristics:**
API reference explanations are brief and precise. They describe what a method
returns, what parameters it accepts, and what exceptions it raises. They do not
explain why the API is designed this way or compare it to alternatives.
Natural tier is 3 — intent coverage only. Why and edge case are rare.

**Why standard rejected types risk failure here:**
A brief explanation of "Returns the first matching element or null if not found"
is already close to a name echo. A Type 2 rejected for a method named
`FindFirstOrDefault` would say "Finds the first item or the default." The overlap
between "Returns the first matching element or null if not found" and "Finds the
first item or the default" is high — both describe the same concept with similar
vocabulary.

**Primary rejected type for STRUCT-B: Type 3 (wrong level)**

API reference explanations describe the contract (what goes in, what comes out).
A wrong-level rejected describes the implementation (how the result is found,
what data structure is traversed, what internal cache is consulted). This produces
near-zero overlap because contract vocabulary and implementation vocabulary are
structurally different domains.

Construction rule:
```
Look at the code snippet itself — not the API reference explanation.
Identify the internal mechanics visible in the code: the loop, the dictionary
lookup, the temp variable, the internal helper call, the intermediate result.
Write documentation that describes these internal mechanics in detail.
Do not mention: what the caller passes in, what the method returns, what
exceptions the caller must handle.
```

Example — for an API reference on `Dictionary<TKey, TValue>.TryGetValue`:

```
Chosen (API reference — tier 3, intent only):
Returns true and writes the value to the out parameter when the key is
present. Returns false and sets the out parameter to the default value
for TValue when the key is absent. Does not throw when the key is missing —
callers should use this instead of the indexer when key presence is uncertain.

Rejected (Type 3 — wrong level):
TryGetValue computes the hash code of the key using the dictionary's
comparer. It uses the hash to locate the bucket, then walks the bucket
chain comparing keys. When the chain contains a matching entry, it copies
the value field to the out parameter. When no entry matches, it writes the
type default.

[WRONG: "wrong level" describes the internal hash computation and bucket
chain walk that callers never interact with. The caller needs to know what
to check in the return value and what happens when the key is absent —
not how the hash table is traversed internally.]
```

Expected overlap for STRUCT-B Type 3: 0.10–0.25. Hash/bucket/chain vocabulary
vs return-value/caller/exception vocabulary are structurally disjoint.

**Secondary rejected type for STRUCT-B: Type 2 (name echo)**

When the method name is highly specific (e.g. `GetOrCreateAsync`, `TryRemoveFirst`,
`ComputeIfAbsent`), produce a Type 2 as a secondary pair. The name echo is
structurally simpler than the wrong-level rejected and produces the lowest possible
overlap.

**Annotation for STRUCT-B pairs:**

Type 3 annotation template:
```
[WRONG: "wrong level" describes the internal {hash computation / dictionary
traversal / cache lookup / iterator protocol} that a caller of this API
never interacts with. The caller needs to know {what the return value means /
what parameter to check / what exception to catch} — internal mechanics answer
none of these questions. Documentation at the implementation level is only
useful to someone modifying the method, not to someone calling it.]
```

**Hard negative for STRUCT-B:**

The hard negative for API reference pairs is the description that sounds like
a contract but actually describes only a subset of the contract.

Example: for an API that returns a nullable value:

```
Chosen:
Returns the cached user for the given ID, or null when the cache does not
contain the ID. The cache does not fetch from the database — callers must
handle null explicitly and decide whether to fall back to the database or
return a not-found response.

Hard negative (structurally correct but incomplete contract):
Returns the cached user for the given ID. Returns null when the user is
not in the cache.

[WRONG: "incomplete contract" correctly states the return values but omits
the critical caller responsibility: the cache does not fetch from the database.
A caller following only this documentation might assume a null result means
the user does not exist, when it means only that the cache does not hold it.
The distinction matters — null from cache versus null from database are
different caller obligations.]
```

Expected overlap for STRUCT-B hard negative: 0.45–0.55. Annotation must name
the specific omitted obligation, not just "information is missing."

---

### STRUCT-C — Conceptual Guide

**Chosen side characteristics:**
Conceptual guides are the highest-quality natural chosen sides in the corpus.
They explain why a concept exists, compare it to alternatives, and often
mention edge cases explicitly. Natural tier 1 is common. The vocabulary is
rich in comparison and trade-off language.

**Why standard rejected types work reliably here:**
Conceptual guides produce chosen sides with strong comparison vocabulary.
A mechanical paraphrase (Type 1) of a conceptual guide drops all the comparison
language and describes only the operations — producing low to moderate overlap.
Missing edge case (Type 4) also works well when the guide has an explicit edge
case section or paragraph.

**Primary rejected type for STRUCT-C: Type 4 (missing edge case)**

Because conceptual guides naturally cover edge cases, Type 4 is the highest-value
rejected type for this structure. The chosen side has the edge case covered;
the rejected removes it.

Construction rule:
```
Same as STRUCT-A Type 4. Remove edge-case sentences.
For STRUCT-C, also check for edge case information embedded in comparison
paragraphs: "Unlike X, this approach also handles Y when Z" — the "when Z"
clause may be edge case content embedded in a comparison sentence.
If removing the edge case clause from a comparison sentence leaves a grammatically
awkward sentence, rewrite the sentence to remove the comparison direction that
depended on the edge case.
```

Expected overlap for STRUCT-C Type 4: 0.35–0.50.

**Secondary rejected type for STRUCT-C: Type 1 (mechanical paraphrase)**

Conceptual guides often begin with a brief explanation of the basic mechanism
before moving to design rationale. The mechanical paraphrase captures only the
basic mechanism — the first paragraph of the guide, essentially — while ignoring
the comparison and trade-off content.

Construction rule:
```
Read the code snippet only — ignore the conceptual guide explanation.
Write one to three sentences describing what the code does mechanically:
what it calls, what it produces, what it stores.
Do not use any vocabulary from the guide's explanation.
The rejected must read as if the author understood the code but had not
read the guide.
```

Expected overlap for STRUCT-C Type 1: 0.25–0.40. Guide vocabulary (prefer when,
unlike, the trade-off, avoids) is largely absent from a pure mechanical
description written independently from the guide.

**Annotation for STRUCT-C pairs:**

Type 4 annotation template:
```
[WRONG: "missing edge case" correctly explains what {concept} does and
why it was designed this way, but omits {the specific condition}.
A developer using this as their only reference would not know that
{consequence of the missing edge case — e.g. the operator does not
complete until all source observables complete / the signal does not
recompute when the dependency has the same value as before / the cache
is not invalidated across requests}.]
```

Type 1 annotation template:
```
[WRONG: "mechanical paraphrase" describes what the code does — which calls
it makes and what it produces — without explaining {why this approach was
designed this way / what problem it solves that simpler alternatives do not /
what condition makes this the right choice over {alternative}}.
A developer reading only this cannot decide when to use this approach versus
{alternative} because the distinction is not explained.]
```

**Hard negative for STRUCT-C:**

The hard negative for conceptual guides is the explanation that uses comparison
language but compares the wrong things — describing how this concept differs
from a similar one in a different language or framework rather than explaining
the design decision within the current technology.

Example — Angular signals guide:

```
Chosen:
Signals propagate changes lazily — a computed signal does not recalculate
until it is read, not when its dependencies change. This avoids unnecessary
computation when a derived value is never accessed in a given render cycle.
Unlike traditional RxJS observables, signals do not require explicit subscription
management — the framework tracks consumers automatically.

Hard negative (wrong comparison — compares to wrong ecosystem):
Signals in Angular work similarly to Stores in Svelte and Atoms in Jotai.
They hold reactive state that components subscribe to automatically.
When the signal value changes, components that read it re-render.
This is different from React useState, where re-renders are triggered by
the setter function call.

[WRONG: comparing Angular signals to Svelte stores and Jotai atoms and React
useState addresses a cross-framework architecture question, not the design
decision within Angular. A developer choosing between signals and RxJS observables
in an Angular project learns nothing from a Svelte/Jotai/React comparison.
The relevant explanation is what signals add over the observable approach already
present in Angular — lazy propagation and automatic consumer tracking.]
```

Expected overlap for STRUCT-C hard negative: 0.20–0.35. The comparison vocabulary
differs but the technical noun (signal, component, re-render) may overlap.
Annotation must name the specific comparison gap, not just "wrong framework."

---

### STRUCT-D — Step-by-Step Implementation Guide

**Chosen side characteristics:**
Implementation guide explanations are step-scoped. Each explanation covers
what one step does and why it is at this position in the sequence. The vocabulary
includes sequencing language ("this step", "after completing", "this enables").
Natural tier 2 — intent and why are present (the step is here because of its
dependencies); edge case is rare within a single step.

**Why standard rejected types risk failure here:**
A step explanation like "Register the authentication middleware before the
authorization middleware so authentication runs first" already describes the
what and the why. A mechanical paraphrase ("Registers the authentication
middleware, then registers the authorization middleware") overlaps at 0.45–0.55
because the key vocabulary (middleware, authentication, authorization, register)
appears in both.

**Primary rejected type for STRUCT-D: Type 2 (name echo)**

Step explanations tend to reference the method or class being configured.
A name echo of the primary method produces very low overlap.

Construction rule:
```
Identify the primary method call or class instantiation in the code snippet.
Rephrase it as a single sentence.
Do not include any sequencing language, dependency language, or reason.
```

Expected overlap for STRUCT-D Type 2: 0.05–0.20.

**Secondary rejected type for STRUCT-D: Type 3 (wrong level)**

Implementation guides explain what the configuration or registration does
at the framework level — what the framework now knows, what behavior it enables.
A wrong-level rejected describes the internal mechanics of how the framework
processes the registration.

Construction rule:
```
For a registration or configuration step: describe what happens internally
when the registration is processed — how the framework stores it, what
dependency graph entry is created, how the factory function is recorded.
Do not describe: what behavior is now enabled, what the caller can now do,
what order guarantees now exist.
```

Example — ASP.NET Core middleware registration:

```
Chosen (implementation guide — tier 2):
Register the authentication middleware before authorization. Authentication
runs first and establishes the ClaimsPrincipal on the request context.
Authorization then reads that principal to evaluate policy requirements.
Registering them in reverse order means authorization runs against an
unauthenticated request and will reject it regardless of credentials.

Rejected (Type 3 — wrong level):
AddAuthentication stores the scheme handler factory in the service collection
keyed by scheme name. The middleware pipeline builder reads this collection
during Build() and inserts the UseAuthentication() lambda into the pipeline
at the registered position. Each request invocation calls the lambda which
retrieves the handler from the DI container using the scheme key.

[WRONG: "wrong level" describes the internal service collection storage and
factory retrieval mechanism that an ASP.NET Core developer configuring
middleware never interacts with. The developer needs to know what order the
middleware runs in, what each middleware does to the request context, and
what breaks when the order is wrong — not how the pipeline builder processes
the service collection at build time.]
```

Expected overlap for STRUCT-D Type 3: 0.15–0.30.

**Annotation for STRUCT-D pairs:**

Sequencing annotation (when the step explanation covers dependency order):
```
[WRONG: "{rejected type}" fails to explain why this step is at this position
in the sequence. The dependency is: {what must be true before this step can
be meaningful}. A developer following only this documentation might move the
step or skip it, without understanding that {consequence of wrong order or
omission}.]
```

---

### STRUCT-E — Troubleshooting or Error Reference

**Chosen side characteristics:**
Troubleshooting documentation naturally covers all three quality dimensions.
Intent: what the fix does. Why: what causes the error. Edge case: when the
standard fix does not apply (because the error can have multiple causes).
Natural tier 1 is common.

**Why this is the richest source of tier 1 chosen sides:**
Troubleshooting docs are written by authors who understand the underlying
cause, not just the symptom. They explain why the error occurs (which maps to
why coverage) and often note that the fix applies only under specific conditions
(which maps to edge case coverage). The prose is concise because it is targeted.

**Primary rejected type for STRUCT-E: Type 4 (missing edge case)**

Troubleshooting docs frequently describe the common cause and fix, then mention
in a secondary paragraph that the same error can also occur under different
conditions with a different fix. Removing the secondary paragraph produces a
clean Type 4 pair.

Construction rule:
```
Identify the primary cause-and-fix block.
Identify any secondary or alternative cause blocks: "This error can also occur
when...", "If the above fix does not resolve the issue...", "In some cases..."
Remove the secondary blocks.
Ensure the primary block is grammatically complete.
```

Example — T-SQL deadlock error:

```
Chosen (tier 1 — all three flags):
A deadlock occurs when two transactions each hold a lock the other needs and
neither can proceed. SQL Server detects this automatically and terminates the
transaction with the lower deadlock priority, returning error 1205.
Fix: add a TRY/CATCH block and retry the terminated transaction — most
deadlocks are transient and the retry succeeds. If deadlocks persist, review
the lock acquisition order: transactions that access tables in the same order
rarely deadlock each other.

Rejected (Type 4 — missing edge case):
A deadlock occurs when two transactions each hold a lock the other needs and
neither can proceed. SQL Server terminates the transaction with the lower
deadlock priority, returning error 1205. Add a TRY/CATCH block and retry
the terminated transaction.

[WRONG: "missing edge case" correctly identifies what a deadlock is and how
to handle the immediate error, but omits the condition where the basic retry
fix is insufficient: persistent deadlocks require reviewing lock acquisition
order. A developer who follows only this documentation will implement the
retry correctly but will be unprepared when the same deadlock recurs on
every transaction pair because the root cause is lock ordering, not
transient contention.]
```

Expected overlap for STRUCT-E Type 4: 0.50–0.58. The primary block appears
in both. Annotation must name the specific condition where the primary fix
fails — not just "there is more to know."

**Secondary rejected type for STRUCT-E: Type 3 (wrong level)**

For troubleshooting docs that explain an error at the framework or runtime
level, a wrong-level rejected can describe the error at the user-visible
symptom level without explaining the underlying cause.

Construction rule:
```
Write documentation that describes what the developer sees (the error message,
the stack trace pattern, the failing test) without explaining why it happens
or what the underlying mechanism is.
This inverts the troubleshooting doc structure: the doc starts with cause and
moves to fix; the rejected starts with symptom and stays at symptom.
```

Expected overlap for STRUCT-E Type 3 (inverted): 0.20–0.35.

**Hard negative for STRUCT-E — correct fix for wrong cause:**

This is the most valuable hard negative class for troubleshooting documentation.
The rejected names a real fix that would solve a different instance of the
same error message, but not the specific instance the prompt describes.

Example — Angular ExpressionChangedAfterItHasBeenCheckedError:

```
Chosen:
This error fires when a component changes a value that was already read
during the current change detection cycle. It almost always means the
component is modifying parent state from within its own change detection
(via an @Output emission that triggers parent logic, which modifies the
child's @Input). Fix: move the state change outside the synchronous
change detection cycle using setTimeout(0) or schedule it with async
to defer to the next tick.

Hard negative (correct fix, wrong cause):
This error fires when a component updates a value in ngAfterViewInit that
was already checked. Fix: wrap the change in a setTimeout(0) to defer
it outside the current change detection cycle.

[WRONG: ngAfterViewInit is one specific cause of this error. The fix
(setTimeout) is correct for that cause, but applying it blindly to any
occurrence of this error produces code that defers changes unnecessarily
when the real cause is @Output-driven parent state mutation. A developer
following only this documentation will apply setTimeout to every instance
without diagnosing which cause applies — producing deferred changes where
immediate architectural fixes are needed.]
```

Expected overlap for STRUCT-E hard negative: 0.40–0.55. Both describe the same
error and the same fix. The distinction is cause precision. Annotation must name
the specific cause that the rejected misses and the consequence of applying the
right fix to the wrong cause.

---

### STRUCT-F — Code Sample Repository

**Chosen side characteristics:**
Samples vary widely in explanation quality. Some have rich inline comments
that reach tier 1; most have sparse comments that reach only tier 3. Samples
with no prose explanation at all must be discarded entirely — there is no
chosen side to extract.

**Scoring approach for STRUCT-F:**

Before attempting pair construction, score the inline comments for each sample
using the three-flag test. Only samples with at least `intent_covered: true`
produce any pair. Samples where the only comments are labels ("// setup",
"// call the API", "// output") score false on all three flags and are discarded.

**Primary rejected type for STRUCT-F: Type 2 (name echo)**

For sparse-comment samples (tier 3 only), the only reliable rejected type is
name echo. Any more complex rejected type will overlap too much with the
already-sparse explanation.

**When to use STRUCT-F samples:**

Use STRUCT-F samples only as a supplementary source when Tier 1 EXT-1 sources
have been exhausted for a specific sub-domain. The examination overhead
(scoring every sample individually) is high relative to the pair yield for
most sample repositories.

---

## Per-Source Examination Protocol

For each source in the external registry, Phase 2a examination classifies the
source by document structure and records the findings. This must be completed
before the harvester is configured for that source.

### dotnet-docs-mit examination

**Expected structure mix by path:**

| Path | Expected structure | Notes |
|---|---|---|
| docs/csharp/asynchronous-programming/ | STRUCT-C (conceptual guide) | Why coverage is high — async design rationale is explained |
| docs/csharp/linq/ | STRUCT-C with some STRUCT-B | LINQ overview pages are conceptual; specific operator pages are API reference |
| docs/csharp/fundamentals/exceptions/ | STRUCT-E (troubleshooting) | Exception handling pages explain cause and fix patterns |
| docs/csharp/language-reference/ | STRUCT-B (API reference) | Language reference is precision documentation — brief, contract-focused |
| docs/standard/collections/ | STRUCT-C | Collections documentation explains design decisions |
| docs/csharp/programming-guide/ | STRUCT-A (tutorial) and STRUCT-C (conceptual) | Mixed — examine three files to determine |

**Rejected type priority for dotnet-docs-mit by structure:**

```
STRUCT-C pages: Type 4 primary, Type 1 secondary
STRUCT-B pages: Type 3 primary, Type 2 secondary
STRUCT-A pages: Type 2 primary, Type 4 secondary (when edge_case_covered: true)
STRUCT-E pages: Type 4 primary, hard negative secondary
```

**Three files to examine in Phase 2a:**

```
File 1: docs/csharp/asynchronous-programming/async-in-depth.md
  Record: structure type, explanation position, typical explanation length,
  presence of edge case content, presence of comparison language.
  Expected: STRUCT-C — async docs explain the await mechanism conceptually,
  compare it to synchronous code, and note cancellation edge cases.

File 2: docs/csharp/language-reference/operators/member-access-operators.md
  Record: same fields.
  Expected: STRUCT-B — language reference is short, precise, API-focused.
  If explanations are under 60 words consistently, confirm STRUCT-B and
  apply Type 3 as primary rejected for all language-reference pages.

File 3: docs/csharp/fundamentals/exceptions/exception-handling.md
  Record: same fields.
  Expected: STRUCT-E — exception handling docs explain what causes exceptions
  and note that different catch block configurations handle different scenarios.
```

**Sub-domain to document structure mapping (to confirm in examination):**

| Sub-domain | Expected structure | Primary rejected type |
|---|---|---|
| async-await | STRUCT-C | Type 4 |
| linq | STRUCT-C + STRUCT-B | Type 4 for guide pages, Type 3 for reference pages |
| dependency-injection | STRUCT-D | Type 2 |
| generics | STRUCT-C | Type 4 |
| extension-methods | STRUCT-C | Type 1 |
| attributes-reflection | STRUCT-B | Type 3 |
| exception-handling | STRUCT-E | Type 4 + hard negative |
| collections | STRUCT-C | Type 4 |
| records-structs | STRUCT-C | Type 4 |
| pattern-matching | STRUCT-C | Type 4 |

---

### sql-docs-mit examination

**Expected structure mix by path:**

| Path | Expected structure | Notes |
|---|---|---|
| docs/t-sql/queries/ | STRUCT-B (API reference) | T-SQL query syntax reference — precise and brief |
| docs/t-sql/functions/ranking-functions-* | STRUCT-C (conceptual) | Window functions have design rationale — when to use RANK vs DENSE_RANK |
| docs/t-sql/language-elements/try-catch-* | STRUCT-E (troubleshooting) | Error handling docs explain cause-and-fix |
| docs/relational-databases/indexes/ | STRUCT-C (conceptual) | Index design is explicitly about trade-offs |
| docs/t-sql/statements/create-procedure-* | STRUCT-D (implementation guide) | Procedure creation is step-by-step |

**Critical note for sql-docs-mit structure classification:**

SQL documentation often presents multiple code blocks before a single explanation.
Examine whether the explanation covers all blocks or only the final one. This
determines whether to split the blocks (STRUCT-D) or treat them as one unit.

A block sequence like:
```
-- Create the table
CREATE TABLE Orders (...)

-- Query the table
SELECT * FROM Orders WHERE ...
```
Followed by an explanation of only the SELECT query: split. Pair the SELECT
block with the explanation, discard the CREATE TABLE block from the pair.

A block sequence where the explanation refers to both blocks: do not split.
Use the full sequence as the prompt. This produces longer prompts with higher
information density — acceptable.

**Three files to examine in Phase 2a:**

```
File 1: docs/t-sql/functions/ranking-functions-transact-sql.md
  Record: structure type, whether comparison between ROW_NUMBER/RANK/DENSE_RANK
  is present (signals STRUCT-C), typical explanation length.
  Expected: STRUCT-C — window function docs compare the functions and explain
  when each is appropriate.

File 2: docs/relational-databases/indexes/clustered-and-nonclustered-indexes-described.md
  Record: same fields.
  Expected: STRUCT-C — index docs explain the trade-off (clustered gives row
  access directly; non-clustered requires a key lookup; covering index eliminates
  the lookup).

File 3: docs/t-sql/language-elements/try-catch-transact-sql.md
  Record: structure type, presence of multi-cause patterns ("this can also fail
  when..."), typical explanation length.
  Expected: STRUCT-E — TRY/CATCH docs explain what error conditions are caught
  and note the conditions where XACT_ABORT changes behavior.
```

**Sub-domain to document structure mapping:**

| Sub-domain | Expected structure | Primary rejected type |
|---|---|---|
| select-queries | STRUCT-B | Type 3 |
| window-functions | STRUCT-C | Type 4 |
| ctes | STRUCT-C | Type 4 |
| stored-procedures | STRUCT-D | Type 2, Type 3 |
| indexes | STRUCT-C | Type 4 + hard negative |
| transactions | STRUCT-E | Type 4 + hard negative |
| dynamic-sql | STRUCT-D | Type 3 |
| error-handling | STRUCT-E | Type 4 |

**Hard negatives for sql-docs-mit:**

Transactions and indexes produce the most valuable hard negatives in the SQL
corpus because the same symptom can have multiple causes with different fixes.

Transaction hard negative template:
```
A deadlock / timeout / dirty read symptom has more than one root cause.
The chosen side explains the most common cause and fix.
The hard negative explains an alternative cause and a partially correct fix
that works for the alternative cause but not for the cause in the chosen side.
Annotation must name: the specific cause the hard negative targets, why
applying the hard negative's fix to the chosen side's cause fails, what
would happen if a developer applied the wrong fix.
```

Index hard negative template:
```
An index is added "to improve query performance" — the chosen explains
what kind of index and why (covering index because the query projects
specific columns; clustered index because range scans dominate).
The hard negative recommends a different index type that would also improve
performance but less optimally or under narrower conditions.
Annotation must name: what workload the hard negative's index is optimised
for, why it underperforms for the query in the chosen side's scenario.
```

---

### angular-adev-mit examination

**Expected structure mix by path:**

| Path | Expected structure | Notes |
|---|---|---|
| adev/src/content/guide/signals.md | STRUCT-C (conceptual) | Signals docs compare to zone-based change detection |
| adev/src/content/guide/di/ | STRUCT-C + STRUCT-D | DI overview is conceptual; specific registration patterns are step-by-step |
| adev/src/content/guide/forms/ | STRUCT-D (implementation) | Forms are step-by-step — create FormGroup, add controls, subscribe to changes |
| adev/src/content/guide/routing/ | STRUCT-D + STRUCT-E | Routing setup is step-by-step; routing errors (CanActivate failures) are troubleshooting |
| adev/src/content/guide/http/ | STRUCT-C + STRUCT-E | HTTP guide explains patterns (STRUCT-C); error handling pages are STRUCT-E |
| adev/src/content/tutorials/ | STRUCT-A (tutorial) | Tutorials walk through features step by step with full narrative |

**Three files to examine in Phase 2a:**

```
File 1: adev/src/content/guide/signals.md
  Record: structure type, comparison language (signals vs zone-based change
  detection, computed vs derived async), edge case content (signal computation
  not triggering when value unchanged), explanation length per code block.
  Expected: STRUCT-C — Angular signals documentation explicitly compares to
  the previous zone.js approach and notes the lazy computation behavior.

File 2: adev/src/content/guide/di/dependency-injection.md
  Record: structure type, whether individual registration patterns are
  explained as separate steps or as conceptual alternatives.
  Expected: mixed STRUCT-C (why DI exists, what it solves) and STRUCT-D
  (how to register a provider with each registration type). Classify by page
  section — conceptual sections use STRUCT-C rules, registration sections
  use STRUCT-D rules.

File 3: adev/src/content/guide/http/http-error-handling.md (or similar)
  Record: structure type, presence of multi-cause error handling patterns,
  RxJS operator examples.
  Expected: STRUCT-E — HTTP error handling explains what causes specific errors
  (network errors vs HTTP status errors) and how to handle each differently.
```

**Sub-domain to document structure mapping:**

| Sub-domain | Expected structure | Primary rejected type |
|---|---|---|
| components | STRUCT-A (tutorials) + STRUCT-D | Type 2 for tutorial pages, Type 3 for guide pages |
| services-di | STRUCT-C + STRUCT-D | Type 4 for conceptual sections, Type 2 for registration steps |
| rxjs-observables | STRUCT-C | Type 4 |
| signals | STRUCT-C | Type 4 + hard negative |
| forms | STRUCT-D | Type 2, Type 3 |
| routing | STRUCT-D + STRUCT-E | Type 2 for setup steps, Type 4 for error pages |
| change-detection | STRUCT-C | Type 4 + hard negative |
| http-client | STRUCT-C + STRUCT-E | Type 4 for pattern pages, Type 4 hard negative for error pages |

**Hard negatives for angular-adev-mit:**

Signals and change detection produce the most valuable hard negatives because
they have common misconceptions documented in the Angular guides themselves.

Signal hard negative template:
```
The chosen explains a computed signal that re-evaluates lazily.
The hard negative explains it as if it re-evaluates eagerly (like zone.js
change detection), which is wrong.
Annotation must name: the specific condition that demonstrates the difference
(computed is not re-evaluated when no consumer reads it), and what
misbehavior the developer would observe when building on the wrong mental model
(unnecessary computation they expect to occur but do not see, or computation
they do not expect that fails to occur).
```

OnPush change detection hard negative template:
```
The chosen explains that OnPush components only update when their @Input
references change or when markForCheck() is called explicitly.
The hard negative explains that they update whenever the parent updates —
the "default" behavior — which is wrong.
Annotation must name: what the developer would observe (component not updating
when they expect it to) and what the correct trigger is (reference change,
not value change).
```

---

## Tier 2 Source Examination Notes

Tier 2 sources (automapper, mediatr, fluentvalidation, dapper, ngrx, angular-material)
are examined after Tier 1 is complete. The examination protocol is the same but with
lower expected volume. Structure classification rules by source:

| Registry ID | Expected structure | Key characteristic |
|---|---|---|
| automapper-mit | STRUCT-D (configuration steps) + STRUCT-C (profile design concepts) | AutoMapper docs explain both how to configure and why profiles are structured this way |
| mediatr-mit | STRUCT-C (handler design) + STRUCT-A (getting started tutorial) | MediatR README is tutorial-first; design sections are conceptual |
| fluentvalidation-apache | STRUCT-D (rule configuration) + STRUCT-C (rule composition concepts) | FluentValidation docs explain both steps and design rationale for rule chaining |
| dapper-apache | STRUCT-D (query execution steps) + STRUCT-B (API reference for method signatures) | Dapper README mixes tutorial and API reference |
| ngrx-mit | STRUCT-C (state management concepts) | NgRx docs are heavily conceptual — why actions, reducers, selectors are separate |
| angular-material-mit | STRUCT-B (component API) + STRUCT-D (integration steps) | Material docs are API-focused with integration examples |

---

## Batch Assembly — Combining Structures

When assembling a batch from multiple sources and structures, apply these
additional rules.

**Structure balance within domain:**

A batch that draws entirely from one document structure will have systematic
vocabulary patterns in the chosen sides. The rejected sides constructed for
that structure will also be systematically similar. This reduces the diversity
of training signal.

Target: no more than 60% of any batch from one document structure type.

**Tier 1 and Tier 2 may mix within a batch** as long as:
- All source_boundary fields are "external"
- All registry_ids are recorded correctly
- The batch does not mix external pairs with internal XIIGen pairs

**Structure-specific composition minimums (applied within the type quotas
from the batch assembly section of the plan):**

| Structure | Minimum contribution per batch | Notes |
|---|---|---|
| STRUCT-C (conceptual) | 30% of batch | Richest chosen sides; Type 4 primary |
| STRUCT-E (troubleshooting) | 15% of batch | Most valuable hard negatives |
| STRUCT-B (API reference) | 20% of batch | Type 3 primary; lowest chosen-side quality |
| STRUCT-A (tutorial) | 20% of batch | Type 2 primary; Type 4 when edge_case_covered |
| STRUCT-D (implementation) | 15% of batch | Type 2 primary |

---

## Negative Example Reference Table

Complete reference for all rejected construction by document structure and sub-domain.

| Domain | Sub-domain | Document structure | Primary rejected | Secondary rejected | Hard negative applies? |
|---|---|---|---|---|---|
| dotnet-csharp | async-await | STRUCT-C | Type 4 (remove cancellation clause) | Type 1 (mechanical, short) | Yes — wrong cancellation scope |
| dotnet-csharp | linq | STRUCT-C | Type 4 (remove empty-collection clause) | Type 2 | Yes — lazy vs eager execution confusion |
| dotnet-csharp | dependency-injection | STRUCT-D | Type 2 | Type 3 | No |
| dotnet-csharp | generics | STRUCT-C | Type 4 (remove constraint edge case) | Type 1 | Yes — covariance direction confusion |
| dotnet-csharp | extension-methods | STRUCT-C | Type 1 | Type 2 | No |
| dotnet-csharp | attributes-reflection | STRUCT-B | Type 3 | Type 2 | No |
| dotnet-csharp | exception-handling | STRUCT-E | Type 4 | Hard negative (wrong catch scope) | Yes |
| dotnet-csharp | collections | STRUCT-C | Type 4 | Type 1 | Yes — thread safety confusion |
| dotnet-csharp | records-structs | STRUCT-C | Type 4 (remove value-semantics edge case) | Type 1 | Yes — equality semantics confusion |
| dotnet-csharp | pattern-matching | STRUCT-C | Type 4 | Type 2 | No |
| sql-tsql | select-queries | STRUCT-B | Type 3 | Type 2 | No |
| sql-tsql | window-functions | STRUCT-C | Type 4 (remove NULL ordering edge case) | Type 1 | Yes — RANK vs DENSE_RANK confusion |
| sql-tsql | ctes | STRUCT-C | Type 4 (remove recursive termination clause) | Type 1 | Yes — recursive vs non-recursive CTE confusion |
| sql-tsql | stored-procedures | STRUCT-D | Type 2 | Type 3 | No |
| sql-tsql | indexes | STRUCT-C | Type 4 (remove lookup cost clause) | Hard negative (wrong index type) | Yes |
| sql-tsql | transactions | STRUCT-E | Type 4 (remove isolation level note) | Hard negative (wrong isolation level) | Yes |
| sql-tsql | dynamic-sql | STRUCT-D | Type 3 | Type 2 | No |
| sql-tsql | error-handling | STRUCT-E | Type 4 (remove XACT_ABORT clause) | Hard negative (CATCH scope confusion) | Yes |
| angular-typescript | components | STRUCT-A+D | Type 2 (A pages), Type 3 (D pages) | Type 4 when edge case present | No |
| angular-typescript | services-di | STRUCT-C+D | Type 4 (C sections), Type 2 (D sections) | Type 1 | No |
| angular-typescript | rxjs-observables | STRUCT-C | Type 4 (remove unsubscribe clause) | Type 1 | Yes — hot vs cold observable confusion |
| angular-typescript | signals | STRUCT-C | Type 4 (remove lazy evaluation note) | Hard negative (eager evaluation model) | Yes |
| angular-typescript | forms | STRUCT-D | Type 2 | Type 3 | No |
| angular-typescript | routing | STRUCT-D+E | Type 2 (D sections), Type 4 (E sections) | Hard negative (guard vs resolver confusion) | Yes |
| angular-typescript | change-detection | STRUCT-C | Type 4 (remove markForCheck clause) | Hard negative (OnPush trigger confusion) | Yes |
| angular-typescript | http-client | STRUCT-C+E | Type 4 (C sections), Type 4 hard negative (E sections) | Type 1 | Yes |

---

## Failure Modes

```
CD-01: Structural prose used as explanation
  Detection: explanation begins with "The following example shows..."
  Fix: extend extraction window past the structural sentence.

CD-02: Chosen side scores false on all three quality flags
  Detection: automated scorer returns intent_covered: false.
  Fix: discard from chosen pool. Mark as candidate-for-rejected-type-1.
  Do not rewrite — the explanation came from the source as-is.

CD-03: Type 4 rejected exceeds overlap threshold
  Detection: overlap_ratio > 0.55 for a Type 4 pair.
  Fix: see per-structure rules above. For STRUCT-A: switch to Type 2.
  For STRUCT-C: extend the chosen side with more edge case detail.
  For STRUCT-E: strengthen annotation.

CD-04: Wrong document structure classification
  Detection: overlap ratios consistently outside expected range for the
  classified structure (e.g. expected 0.10–0.20 for Type 2 but getting 0.35).
  Fix: re-examine the source file. The structure classification may be wrong.
  Re-run Phase 2a examination protocol for that source.

CD-05: Hard negative annotation names the wrong distinction
  Detection: the chosen side and the rejected hard negative are both correct
  for different conditions, but the annotation describes a condition that
  does not appear in either.
  Fix: re-read both the chosen and rejected. Write the annotation specifically
  about the condition in the snippet — not a generic version of the distinction.

CD-06: Single document structure dominates a batch
  Detection: more than 60% of batch pairs share the same document structure.
  Fix: draw additional pairs from a different sub-domain or source file with
  a different structure. Balance before committing.

CD-07: STRUCT-F sample with no prose explanation treated as valid chosen
  Detection: snippet_raw contains inline comments only — no separate prose
  paragraph was extracted. Explanation_raw is populated with comment text.
  Fix: score the comments with the three-flag test. If intent_covered is false,
  discard entirely. Comment text does not become a prose explanation.

CD-08: External pair in internal batch
  Detection: source_boundary field missing or set to "internal".
  Fix: separate into external-only batch file before committing.
```

---

## Acceptance Checklist

```
□ CANONICALITY GATE — stop if failed.
  source_canonicality: extracted_copy
  source_boundary: external
  canonical_confirmed_via: records registry entry + verification date

□ Document structure classified (STRUCT-A through STRUCT-F)
  Classification recorded in source row as structure_type field.
  Rejected type chosen according to per-structure rules above.

□ Rejected type appropriate for classified structure
  See Negative Example Reference Table above.

□ Language tag matches domain:
  dotnet-csharp: [csharp, cs, c#]
  sql-tsql: [sql, tsql, sql-server, t-sql]
  angular-typescript: [typescript, ts]

□ snippet_lines 5–80 inclusive

□ explanation_words 40–600 inclusive

□ intent_covered: true

□ Rejected is a complete, coherent response — not a negation phrase

□ [WRONG: ...] annotation is present, specific, and names:
  — the rejected type label
  — what the caller cannot learn from the rejected side
  — the consequence of relying only on the rejected side

□ For hard negatives: annotation names the specific condition where
  the hard negative's approach fails (not just "this is incomplete")

□ overlap_ratio <= 0.60

□ For overlap_ratio > 0.45: annotation_strong is true AND ≥ 5 consequence
  tokens in annotation not present in rejected

□ Structure composition: no more than 60% of batch from one STRUCT type

□ compiled_prompt_in_training: false

□ source_boundary: external on every row in the batch

□ chosen.trim() != rejected.trim()

□ prompt/chosen/rejected contain no mojibake markers: Ã, Â, â€, Ã¢

□ domain is registered in CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md

□ compiled_prompt_in_training: false AND prompt_mode: raw_snippet
```

---

## Output Split Required After Examination

Do not produce one monolithic train-ready JSONL from a generated archive.
Produce three files:

```
code-doc-clean-trainable.jsonl
  Only rows passing every hard gate in this checklist.

code-doc-repair-needed.jsonl
  Rows with identity failure, mojibake, weak annotation, unregistered domain,
  or missing provenance.

code-doc-audit-ledger.jsonl
  Ledger and run metadata. Not a DPO training input.
```

Train `code-doc-clean-trainable.jsonl` in 16-64 row standalone ablations first.
Only after a positive standalone margin should a chunk be tested in a combo run
with the current baseline.

---

## Last Updated

2026-05-11. Version 2.0 plus run-experience addendum from V244-V247 and
`learning sets.zip` audit.
