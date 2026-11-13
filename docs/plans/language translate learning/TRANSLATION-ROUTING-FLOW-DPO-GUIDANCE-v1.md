# Translation Routing Flow DPO Guidance
# Version: 1.0 - 2026-05-11
# Domain: Russian/Hebrew <-> English Translation Routing
# Depends on: TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md
# Runtime contract: TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>

---

## Purpose

This guide defines DPO pairs for the translation routing layer.

The translation router is a learnable orchestration structure, not a static
classifier. The public training target is:

```csharp
TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
```

The assistant-level coordinator may be:

```csharp
TranslationRoutingOrchestrator : ILogicalOrchestrator
```

Internal nodes may implement `ILogicalUnit`, but pair authors must not reduce the
training surface to isolated labels. The chosen side should teach a complete
routing decision: direction, topic domain, register, risk class, adapter id,
confidence, evidence, and rejected alternative.

---

## Pair Shape

Every routing-flow pair uses this shape:

```json
{
  "id": "TR-ROUTE-R1-0001",
  "domain": "TRANSLATION_ROUTING",
  "contract_signature": "TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>",
  "router_level": "R1",
  "a_side": {
    "source_text": "...",
    "source_language_hint": null,
    "target_language": "en",
    "user_domain_hint": null,
    "user_register_hint": null,
    "metadata": {}
  },
  "chosen": {
    "direction": "RU_EN",
    "topic_domain": "general",
    "register": "neutral",
    "risk_class": "normal",
    "translation_level": 1,
    "adapter_id": "adapter-ru-en-general",
    "confidence": 0.94,
    "evidence_terms": ["cyrillic", "target_language=en", "short_general_sentence"],
    "rejected_adapters": ["adapter-en-ru-general"],
    "decision_reason": "Russian source with English target should route to RU_EN general."
  },
  "rejected": {
    "direction": "EN_RU",
    "topic_domain": "general",
    "register": "neutral",
    "risk_class": "normal",
    "translation_level": 1,
    "adapter_id": "adapter-en-ru-general",
    "confidence": 0.88,
    "evidence_terms": ["target_language=ru"],
    "decision_reason": "Routes to the reverse direction.\n[WRONG: wrong_direction - the adapter expects English input and Russian output, so a Russian source sentence will be handled by the wrong decoder and produce an invalid translation path.]"
  }
}
```

Rules:

1. `chosen` must contain the full routing decision, not only `adapter_id`.
2. `rejected` must be a plausible adjacent wrong route, not random noise.
3. `rejected.decision_reason` must include `[WRONG: ...]` with the functional
   consequence.
4. The pair must identify the runtime contract as `ILogicalFlow`.
5. If the pair targets an internal node, set `node_role`, but still state the
   flow-level consequence.

---

## Router Levels

### R1 - Direction Detection

Teaches:

- `RU_EN`
- `EN_RU`
- `HE_EN`
- `EN_HE`

Good rejected choices:

- reverse direction
- wrong source script direction
- source-English direction when the source is Russian or Hebrew

Bad rejected choices:

- an impossible adapter id unrelated to the prompt
- a generic "bad translation" without routing consequence

### R2 - Topic Domain Detection

Teaches:

- `general`
- `named_entity`
- `legal_diplomatic`
- `software_ui` only after source promotion
- `literary` only after source promotion
- `high_stakes_blocked`

Good rejected choices:

- named entity routed as ordinary sentence
- UN formal text routed as `general`
- high-stakes medical/finance text routed as normal trainable translation

### R3 - Register and Risk Detection

Teaches:

- `neutral`
- `colloquial`
- `formal_spoken`
- `formal_written`
- `legal_diplomatic`
- `ui_short`
- `named_entity`

Good rejected choices:

- formal-written routed as colloquial
- named-entity-sensitive routed as normal
- high-risk text allowed through instead of blocked or review-gated

### R4 - Adapter Selection

Teaches final adapter selection after direction, topic, register, and risk are
known.

Good rejected choices:

- adjacent direction adapter
- adjacent topic adapter
- general adapter for formal legal-diplomatic text
- named-entity adapter for full sentence translation

---

## Flow-Level vs Unit-Level Pairs

Use flow-level pairs when the output is a complete route:

```text
TranslationRoutingFlow -> TranslationRoutingDecision
```

Use unit-level pairs only when teaching one internal responsibility:

```text
TranslationDirectionRouterUnit -> direction
TranslationTopicRouterUnit -> topic_domain
TranslationRegisterRiskRouterUnit -> register/risk
TranslationAdapterSelectorUnit -> adapter_id
```

Even unit-level pairs must include a `flow_consequence` field explaining how the
wrong unit result would damage the final `TranslationRoutingFlow` decision.

---

## Promotion Gates

Router-flow batches are promoted only when:

- direction accuracy >= 99%
- topic-domain accuracy >= 95%
- adapter-id accuracy >= 95%
- false low-risk rate = 0 on formal/high-risk probes
- DPO reward margin > 0
- no `chosen == rejected`
- every rejected side has a specific `[WRONG: ...]`
- no mojibake in a_side, chosen, or rejected

If a translator batch improves while the routing-flow gate fails, reject the
translator promotion. The translator cannot become production-candidate when
adapter selection is unreliable.

---

## What Not To Do

- Do not implement translation routing as a hardcoded `switch`.
- Do not train adapter selection only from filenames or source corpus ids.
- Do not train the router and translator from the same 50 rows in the same run.
- Do not let `general` become the fallback for formal/high-risk text.
- Do not create a universal mixed adapter before routing-flow gates pass.
- Do not treat `ILogicalUnit` labels as sufficient replacement for
  `ILogicalFlow` behavior.
