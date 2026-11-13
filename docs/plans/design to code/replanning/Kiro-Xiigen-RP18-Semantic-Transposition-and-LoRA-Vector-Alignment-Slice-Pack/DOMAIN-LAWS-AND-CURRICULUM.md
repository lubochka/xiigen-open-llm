# RP18 — domain-law passports, alignment contrasts and curriculum

Use **real laws** and independently authored source-family tasks, not unit-test strings as the only quality definition. Record each passport: domain, intended user task, scope and version, primary authority, protected invariants, forbidden transformations, positive candidate, hard negative, warning near-miss, ABSTAIN trigger, typical vector features and actual downstream consumer.

## First two independent fast lanes

**C# API documentation (C# law and project policy are different):** 
- GOOD: nullable property with optional docs stays optional; no invented `required`; explicit project-version rule may make a *different* property required under v2.
- BAD: embed by word co-occurrence and merge `string? Name` with `required non-null Name`; LoRA generator rewrites requiredness without project authority.
- EDGE: unknown project rule version → law-scoped abstain or deterministic unchanged fallback, not index nearest v2 extrapolation.
- Feature preservation: nullable marker, type, declaration, version, source span; check output class/contract and user-visible documentation.

**EN→RU negation and exception:**
- GOOD: “must not delete except when owner approved” preserves prohibition **and** exception; candidate nearest positive `must delete` is excluded/warning.
- BAD: high cosine changes `not` to affirmative; quantizer maps `unless` to same class as `always`; single-neighbor accepted by similarity alone.
- EDGE: mixed-language legal clause or source ambiguity → UNKNOWN/MIXED and faithful ask/abstain, not confident cross-domain memory.
- Feature preservation: polarity, scope, actor, temporal condition, exception, deontic strength and morphology.

**Training/OP_DEV/CERT** split by **source family**, document origin and law version. Freeze CERT before choosing tokens, weights, projection dimensions or prompt examples. Code review may use OP_DEV; never query sealed CERT to patch vector schema or pick DPO pairs.

## Five additional core subjects and extras (not prerequisite for first proof)

- SQL schema/query: `NULL` vs non-NULL, transaction scope, parameterization, dialect/version.
- Angular/TypeScript: nullable forms, observable lifecycles, DOM/data binding, versioned framework behavior.
- Figma/HTML-CSS design conversion: responsive constraints, element order, accessibility, never invent pixels as law.
- Async orchestration: cancellation, dependencies, branch safety, bounded latency, producer→consumer ordering.
- Builder/Graph patterns: higher-order ordered associations, authorized project context, no shortcut replacing real typed relationships.
- DTC/IDE/native extras: path/version/host/security/binding authority, held by RP06/S08–S10.

For each non-pilot subject make a **passport and activation trigger**, not seven mandatory implementation gates. Prioritize two law-distinguishing tasks that let fixed lexicon vs adapted projection make opposite predictions; if not distinguishable, label EXPERIMENT_INADEQUATE. Label positive/negative/warning separately. Final arbiter can reject even a nearest correct-looking memory.

## DPO if independently admitted

One new frozen x and permitted context; CODEX GPT-6 Astra UltraCode performs human **case selection**. An actually approved **external open Chinese model authors BOTH final chosen and final rejected** under that x/context. Separate CODEX Astra invocation assesses exact immutable pair before optimizer. TIE/BOTH_BAD/UNJUDGEABLE are **not** forced positives. Student no-repeat one-pair update and response-only objective belong to RP01/R18; trained embedding/projection targets are NOT automatically the same Student update. RP09 evaluates on independent families. A deterministic valid answer may be positive even if not neural. Data/provider/licensing/tenant grants are independently required.

## Curriculum proof progression

`B0` frozen correctly lawful baseline → `B1` fixed space-compatible projection and ordinary use → `B2` learned projector ONLY on proved weakness → `B3` optional Student LoRA separate treatment → `B4` cross-domain transfer/retention on independent laws. Each stage permits `REUSE_NO_CHANGE`, `INCONCLUSIVE` or `REGRESS`; do not demand B4 to measure B1.
