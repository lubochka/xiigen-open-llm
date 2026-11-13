# TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md
# Version: 1.6 - 2026-05-29
# Purpose: How to author accepted/rejected DPO pairs for each trainable unit in TranslationMachine.
# Scope: 27.TranslationDomain, Direction 1.
# Based on: CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1, CODE-DOC-DPO-REGISTRY-GUIDE-v1,
#   CODE-DOC-DPO-REJECTED-PROMPTS-v1, CODE-DOC-50-PAIR-ABLATION-RUNBOOK-v1.
# Translation-specific sources: translation_guidance_fixed / TRANSLATION-SOURCE-REGISTRY-v3,
#   DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1, TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1,
#   TRANSLATION-TOKENIZER-GUIDE-v1, EXAMINATION-METHOD-TRANSLATION-v1.

---

## 1. Why This Guide Exists

TranslationMachine is a full `ILearningMachine<TranslationDomain>`.
It must not be trained as one flat string-to-string appendix.

Every participant in the translation flow receives its own DPO examples:

```text
TranslationMachine
  -> TranslationRoutingFlow
     -> semantic-tokenizer
     -> executor-ru_en
     -> quality-verifier
```

The same source sentence normally expands into three unit-level examples.
Each example has:

- exact unit input object;
- exact chosen unit output object;
- plausible rejected unit output object;
- one specific `[WRONG: ...]` defect;
- a shared semantic signature so the unit outputs can meet again at runtime.

The goal is not to produce pretty training files. The goal is that a held-out
runtime prompt stops falling back to `[RU_EN:BOOTSTRAP]` and produces a useful
translation.

---

## 2. Universal DPO Rules

These rules are inherited from the code documentation DPO work.

### Chosen Side

The chosen side must be the exact output that this unit should learn to emit.
It cannot contain explanations unless the unit output contract explicitly has an
explanation field.

For translation:

- chosen tokenizer output is a `TokenizerOutput`;
- chosen executor output is an `ExecutorOutput`;
- chosen verifier output is a `VerifierOutput`;
- chosen runtime text must be only the translation, not "Here is the translation".

### Rejected Side

The rejected side must be plausible and close. It should look like a mistake a
bootstrap translator, small model, or junior authoring process would actually
make.

Every rejected side must include:

```text
[WRONG: <specific weakness> - <what cannot be learned> - <functional consequence>.]
```

Bad rejected side:

```text
{"Text":"bad translation"}
```

Good rejected side:

```text
{"Text":"He moved through the hostile crowd, betraying not a trace of fear!",
 "Confidence":0.31,
 "wrong":"[WRONG: adverb_value_omitted - the rejected output keeps the event but drops the fearless manner carried by 'бесстрашно' - runtime would translate the action while losing the quality the batch is meant to teach.]"}
```

### One Defect Per Pair

A rejected side teaches one boundary. Do not mix defects.

Good:

- "drops the adverb value";
- "collapses `проверку null` into the wrong compound";
- "accepts a translation that lost a code identifier".

Bad:

- wrong adverb, wrong tense, missing entity, and wrong register in the same row.

### Overlap

The rejected output should overlap enough with chosen to be tempting.

Recommended overlap:

- `0.25-0.55` for ordinary executor/verifier pairs;
- higher is acceptable for tokenizer pairs because the object is small;
- never use chosen copied verbatim with one token removed unless the defect is
  explicitly "omission of required quality marker" and the runner diagnoses it.

Executable builder rule: `TranslationStructuredBatchBuilder` may synthesize a
one-term-removed rejected candidate only for omission-like defects (`omission`,
`missed`, `marker_lost`, etc.). Grammar-rule defects such as agreement, case,
tense, aspect, gender, and morphology require an explicit rejected candidate in
the source row or dedicated rejected-DPO metadata. Do not let the builder turn
`She went` into `went` when the intended lesson is `she/he` agreement.

Translation-specific gate from the fixed guidance: when the combined chosen +
rejected text has at least 12 whitespace tokens, run the overlap check before a
row becomes train-ready. If the overlap ratio is above `0.50` in the current
pair-preparation script, or above `0.60` in a BPE/token-level audit, send the row
to `repair_required`. It is usually too similar to teach a preference boundary.

### Held-Out Is Sacred

Rows marked `HeldOut = true` must never be passed to `TrainOnExample`.

A prepared pairset is not an accepted batch.
A smoke that reproduces training rows is not a promotion.
Promotion requires runtime held-out evidence.

Batch-029 proved why this matters:

```text
training-row smoke accuracy: 1.0
held-out unit accuracy:      0.666667
held-out runtime accuracy:   0.000000
decision:                   do not promote
```

### Translation-Specific Source And Prompt Rules

These rules come from `translation_guidance_fixed` and are mandatory for unit
examples derived from external translation rows:

- source authority is `TRANSLATION-SOURCE-REGISTRY-v3.md`;
- clean first-run chosen-side sources are Tatoeba, ParaNames, and UN Parallel
  Corpus only in their allowed roles;
- Knesset is support-only for Hebrew tokenizer/register/morphology evidence, not
  chosen-side translation authority;
- FLORES/Wikipedia/WikiMatrix are quarantine/eval-only unless a later registry
  explicitly promotes them;
- blocked or review-required sources never enter train-ready rows;
- `a_side` is raw source text; the system prompt is injected by the training
  harness, not stored inside the pair;
- set `compiled_prompt_in_training: false` on source-level translation rows;
- chosen and rejected must both be in the declared target language;
- rejected must not be a refusal template such as "cannot translate" or "N/A";
- any mojibake marker in source/chosen/rejected makes the row unusable.

### Translation Error Taxonomy

Use these error families to keep rejected rows diagnostic:

| Family | What it teaches | Typical rejected type |
|---|---|---|
| T01 sentence core | meaning accuracy | `mistranslation`, `literal_calque` |
| T02 register | formality and style | `register_shift` |
| T03 morphology | RU aspect/case, HE gender/binyan/final letters | `morphological` |
| T04 idiom | non-literal expression | `idiom_calque` |
| T05 false friend | word that looks familiar but means something else | `false_cognate` |
| T06 Hebrew script | RTL/script discipline | `rtl_or_script_error` |
| T07 back-translation | round-trip meaning preservation | `back_translation_divergence` |
| T08 completeness | no dropped/added clause | `omission`, `addition` |
| T09 named entity | names survive translation/transliteration | `named_entity` |

Do not invent a new rejected type when one of these covers the error. Stable
labels are what let the runner report accuracy by weakness instead of hiding
failures inside one average.

---

## 3. Flow-Level Routing Boundary Before Unit Rows

Current code warning: the richer routing decision object described in the
fixed translation guidance is not implemented yet. The current executable
contract is `TranslationRoutingRequest` ->
`TranslationRoutingResult(Direction, TranslatedText, Confidence, AdapterId)`.
Any row that uses `register`, `risk_class`, `translation_level`, or
`evidence_terms` is a future design row and must be stored as
`planned_not_train_ready` until a real routing trainer exists.

Before a translator unit batch can be promoted, the route that chooses the
adapter must also be trainable. This is a flow-level DPO row, not a static
`switch`.

```text
TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
Input:  source_text, language hints, target language, user register hint, metadata
Current output: TranslationRoutingResult(Direction, TranslatedText, Confidence, AdapterId)
Future output:  richer routing choice with topic_domain, register, risk_class,
                translation_level, adapter_id, confidence, evidence_terms
```

Future routing pair, not train-ready today:

```text
Pair[TR-ROUTE-R1-001]
A:
  source_text: "добавь проверку null в метод ValidateToken"
  target_language: "en"
B:
  chosen:
    direction: RU_EN
    topic_domain: software_ui
    register: technical_prompt
    risk_class: normal
    translation_level: 1
    adapter_id: adapter-ru-en-technical
    evidence_terms: ["cyrillic", "target_language=en", "ValidateToken", "null"]
  rejected:
    direction: RU_EN
    topic_domain: general
    register: neutral
    risk_class: normal
    translation_level: 1
    adapter_id: adapter-ru-en-general
    evidence_terms: ["cyrillic", "target_language=en"]
    decision_reason: "[WRONG: wrong_topic_domain - the route ignores code identifiers and sends a technical prompt to the general adapter, so the executor may translate or damage code terms.]"
```

When a real routing trainer is added, routing rows must cover:

- R1 direction detection;
- R2 topic domain detection;
- R3 register and risk detection;
- R4 final adapter selection.

Translator rows are not promotable when routing-flow gates fail. A good
translator behind the wrong adapter is still a bad product result. Current
`TranslationStructuredBatchBuilder` must not ingest the future routing rows
above; today it only builds unit rows for tokenizer, executor, and verifier.

---

## 3A. FrequencyQuoteMap Layer For KNN Generalization

The session archive idea from
`C:\Users\lubao\Documents\xiigen\claude-sessions\Uncategorized` is important:
high-frequency words and common quote/sentence starts cover a large share of
real language use. They also give the KNN store something stronger than exact
sentence memorization.

The concrete sessions reviewed for this method are:

```text
22.03.2026 13.14/.../Самые цитируемые испанские слова.md
22.03.2026 14.00/.../Самые цитируемые испанские слова.md
23.03.2026 06.42/.../Самые цитируемые французские слова.md
```

The Spanish sessions are useful because they combine:

- top-10 most frequent words;
- top-10 common/famous sentence examples per word;
- a repeatable five-column format: original sentence, pronunciation aid,
  literal translation, natural translation, source.

The French session is useful because it adds a stricter source lesson:
frequency lists must be cross-checked across multiple corpora and bad sources
must be rejected. It explicitly compared corpus-style sources and rejected a
machine-translated frequency list after finding vocabulary errors.

For the current RU_EN/HE_EN translator, these FR/ES sessions are `METHOD_ONLY`.
They do not make French or Spanish production directions ready. They define the
method we now apply to RU_EN: build frequency words, common sentence starts,
literal-vs-natural translation rows, and source quality checks before claiming
language learning.

This layer is not a flat word list. It is a structural map:

```text
frequent source word or phrase
  -> likely semantic role
  -> common source sentence prefix
  -> target-language continuation pattern
  -> accepted/rejected continuation pair
```

Why this matters:

- a small number of frequent words covers many ordinary sentences;
- common sentence starts strongly constrain sentence continuation, similar to
  how a code prefix constrains the next expression;
- KNN can retrieve by prefix + POS pattern + semantic role, not only by whole
  sentence identity;
- executor can compose a held-out sentence from learned continuations instead
  of falling back to `[RU_EN:BOOTSTRAP]`.

### FrequencyQuoteMap Artifact

For each direction and register, build an artifact like this:

```json
{
  "map_id": "ru-en-general-frequency-quotes-v1",
  "direction": "RU_EN",
  "register": "literary",
  "source_scope": "allowed registry sources or first-party private material",
  "top_source_words": [
    { "word": "он", "lemma": "он", "pos": "PRON", "role": "subject" },
    { "word": "она", "lemma": "она", "pos": "PRON", "role": "subject" },
    { "word": "нежно", "lemma": "нежно", "pos": "ADV", "role": "manner" }
  ],
  "top_sentence_starts": [
    {
      "source_prefix": "Она нежно",
      "pattern": "SVO",
      "semantic_signature_prefix": "SVO|SUBJECT_SHE|ADVERB_MANNER:tenderness",
      "target_prefix": "She tenderly",
      "continuation_template": "{verb_phrase} {object_phrase}"
    }
  ]
}
```

The artifact itself can be generated from public/allowed corpora or first-party
private materials. Raw private source text must not be committed; commit only
inventory, derived signatures, and runner reports.

### Code Contract Boundary For Semantic Labels

`SemanticSignature` is a string. It may carry rich labels such as
`ADVERB_MANNER:tenderness`, `SUBJECT_SHE`, or `HOSTILE_CROWD` because the KNN
store uses signatures as structural text.

`ExecutorWordMapping.Concept` is different: it is the `SemanticConcept` enum
from `27.TranslationDomain`. Train-ready `WordMappings` must use only current
enum values:

```text
UNKNOWN
ADD_OP, VALIDATE_CHECK, NULL_LITERAL, METHOD_REF, CODE_IDENTIFIER
LOG_OP, WRITE_OP
THREAD_REF, FLOW_REF, STREAM_REF
ERROR_EXCEPTION, PROCESSING_OF, DATA_ENTITY
LAUNCH_OP, RETURN_OP, HANDLE_OP
```

Until the enum is intentionally extended, literary or social concepts such as
`ADVERB_MANNER:tenderness`, `MOVE_THROUGH`, or `HOSTILE_CROWD` belong in
`SemanticSignature`, verifier required terms, and authoring notes. Their
`WordMappings` use `Concept:UNKNOWN` with a precise `SourceWord` and
`TargetWord`.

### Unit-Level DPO From Frequency/Quote Rows

One frequent sentence start should produce rows for the units:

```text
source prefix: "Она нежно"
expected continuation family: subject + manner + action

semantic-tokenizer row:
  chosen signature prefix:
    SVO|SUBJECT_SHE|ADVERB_MANNER:tenderness
  rejected signature prefix:
    SVO|SUBJECT_SHE
  [WRONG: frequent_adverb_marker_dropped - the prefix becomes too generic and KNN retrieves bland movement sentences instead of tenderness-preserving translations.]

executor-ru_en row:
  chosen:
    "She tenderly ..."
  rejected:
    "She ..."
  [WRONG: prefix_continuation_lost - the target prefix drops the learned manner continuation, so held-out sentences that start with the same structure lose their main quality.]

quality-verifier row:
  candidate:
    "She brushed the hair from his forehead as he slept."
  chosen verifier output:
    Pass: false
    Issues: ["expected 'tenderly' or equivalent tenderness marker for prefix 'Она нежно'"]
  rejected verifier output:
    Pass: true
  [WRONG: verifier_misses_prefix_marker - the verifier accepts a fluent continuation that drops the high-frequency prefix meaning.]
```

For future FR/ES expansion, the same row shape becomes:

```text
FR_EN prefix example:
  source prefix: "Je suis ..."
  target prefix: "I am ..."
  rejected: "I follow ..."
  [WRONG: false_friend_prefix - 'suis' in this prefix is être, not suivre; KNN would learn the wrong continuation family.]

ES_EN prefix example:
  source prefix: "Estoy buscando ..."
  target prefix: "I am looking for ..."
  rejected: "I am searching ..."
  [WRONG: register_and_continuation_shift - the rejected form is possible but less natural for ordinary chat/networking prompts and can pull the continuation toward a stiff register.]
```

Those examples are not production training rows yet. They are templates for
future `FR_EN`, `EN_FR`, `ES_EN`, and `EN_ES` adapters after source registry,
tokenizer support, and held-out sets exist.

### KNN Signature Rule

Do not store only exact full-sentence keys for this layer. Store at least three
levels:

```text
prefix signature:   SVO|SUBJECT_SHE|ADVERB_MANNER:tenderness
structure signature:SVO|SUBJECT|ADVERB_MANNER|ACTION|OBJECT
full signature:     SVO|SUBJECT_SHE|ADVERB_MANNER:tenderness|BRUSH|HAIR|FOREHEAD
```

At runtime, KNN should try:

1. full signature;
2. prefix signature;
3. structure signature;
4. only then Bootstrap fallback.

Current code status: `TranslationKnnStore.FindNearestCascade` implements this
full -> prefix -> exact-structure cascade. It intentionally does not accept a
loose component-overlap match inside the cascade, because a broad match on only
`SVO` can retrieve an unrelated sentence and produce a fluent but wrong
translation. Training indexes executor examples under the authored full
signature plus tokenizer-derived prefix and structure signatures, so this path
is now runtime evidence rather than only guidance.

Low-information structure signatures are not enough for runtime translation.
Rows whose tokenizer output is only `SVO`, `SVO|NOUN`, or another signature with
no semantic concept, no explicit prefix label, and no reusable mapping must not
be allowed to drive held-out KNN answers. Runtime may use an exact-source lookup
for rows that were actually trained, but unseen rows need one of:

- explicit `WordMappings`;
- explicit `[PHRASE: source => target]` phrase boundaries;
- a rich authored `SemanticSignature`/prefix label from the frequency/quote
  map layer;
- a real semantic concept emitted by the tokenizer.

Otherwise the executor must fall back instead of retrieving a fluent but
unrelated sentence from a generic structure bucket. This rule came from the
P0 100+100 probe where `SVO|NOUN` retrieved `I am twenty years old.` for
unrelated short held-out prompts.

This is the difference between memorizing 40 training rows and learning a
reusable language map.

### Accepted Sources For Frequency/Quote Maps

Use the same source rules as translation pairs:

- `ACCEPT` sources can create train-ready frequency/quote maps;
- `ACCEPT_SUPPORT_ONLY` sources can support tokenizer and frequency statistics,
  but not chosen translation authority;
- first-party private materials can create private maps but raw content stays
  outside git;
- `QUARANTINE` and `REVIEW_REQUIRED` sources cannot silently enter clean maps.

Additional rule from the French session: at least two independent accepted or
support-only frequency sources must agree before a word/prefix can become a
clean frequency map row. If a source looks like a machine-translated list, it is
method evidence at most and cannot become chosen-side authority.

### Diagnostics For This Layer

Every frequency/quote batch report must include:

```text
top_word_coverage_percent
top_prefix_coverage_percent
prefix_knn_hit_rate
structure_knn_hit_rate
bootstrap_fallback_rate
continuation_product_accuracy
```

The immediate target for batch-029 repair is:

```text
bootstrap_fallback_rate: below 100% on held-out rows
prefix_knn_hit_rate:    above 0 on unseen adverb sentences
```

That is a small but honest first proof that the model learned structure beyond
exact sentence lookup.

---

## 3B. Language Rule Matrix For KNN And DPO

KNN must not search only by nearby words. It must also search by grammar
structure. For TranslationDomain, grammar rules are stored as rule tags in the
semantic signature and can be repeated explicitly in rejected-side metadata:

```text
[RULE: tense=past]
[RULE: case=dative]
[RULE: agreement=subject_fem_past_fem]
[RULE: exception=motion_verb_go]
```

These tags are not deterministic translation rules. They are structural
addresses. The executor may retrieve a trained example with the same tense,
case, agreement, or exception family, but the target words still come only from
accepted DPO rows.

### Russian rule families to track

| Rule family | Examples | Why it matters for RU->EN |
|---|---|---|
| Tense | past, present, future | `она пошла` and `она идет` cannot share the same continuation class. |
| Aspect | imperfective/perfective | `делал` vs `сделал`, `писал` vs `написал`. |
| Gender agreement | `она пошла`, `он пошел`, `кот забыл` | Subject gender/animacy must connect to the past-tense verb form. |
| Number/person | `я иду`, `они идут`, `мы пошли` | Verb/person patterns affect English pronoun and auxiliary choice. |
| Cases | nominative, genitive, dative, accusative, instrumental, prepositional | Case often determines English preposition or object role. |
| Preposition governance | `к` -> dative, `из/от` -> genitive, `с` -> instrumental/genitive, `в/на` -> accusative/prepositional | A preposition alone is not enough; KNN needs the governed case family. |
| Motion verbs | идти/ехать/нести and prefixed forms | Russian motion verbs are exception-heavy and must be tagged. |
| Reflexive verbs | `вернулся`, `улыбнулась`, `ошибся` | May map to English intransitive/reflexive/non-reflexive forms. |
| Negation | `не сделал`, `никогда не`, `ни одного` | A dropped negation is a critical semantic failure. |
| Exception words | irregular motion forms, suppletive forms, false friends | Must be explicit DPO families, not hidden code assumptions. |
| Pronoun/coreference | `кот - он - забыл`, `Луба - она - решила` | The machine must connect the pronoun back to the object/person it refers to. |
| Adjective/noun agreement | gender, number, case of modifiers | Needed for EN->RU generation and verifier checks. |
| Numerals | `1 год`, `2 года`, `5 лет` | Russian numeral case patterns are exception-like. |

### English rule families to track

| Rule family | Examples | Why it matters for EN<->RU |
|---|---|---|
| Tense/aspect | simple, progressive, perfect, perfect progressive | `I went`, `I was going`, `I have gone` are different Russian choices. |
| Irregular verbs | go/went/gone, write/wrote/written, be/was/were | Must be explicit exception families. |
| Modal verbs | can, must, should, would, might | Often changes Russian mood/softness. |
| Articles | a/the/zero | RU has no articles; EN output must learn them from context. |
| Countability | advice, information, money | Prevents wrong plural/article choices. |
| Pronoun case | I/me, he/him, she/her, they/them | Important for EN->RU and verifier. |
| Possessives | my, mine, his, her, their | Connects ownership to case/gender in RU. |
| Phrasal verbs | look up, give up, run into | Must not be translated word-by-word. |
| Prepositions | in/on/at/to/for/from/with | Often maps from Russian case and governed preposition. |
| Word order | question inversion, adjective-before-noun, adverb placement | KNN needs structure beyond word overlap. |
| Idioms/exceptions | break the ice, cold call, make sense | Rejected side should teach literal-calque failure. |
| False friends | actual, magazine, accurate, fabric | Must be separate rejected types. |

### How to author DPO rows with rule tags

For every grammar-sensitive source row, add rule tags and one close rejected
failure:

```text
A-side:
  "она пошла к морю"

Chosen executor output:
  Text: "She went to the sea"
  WordMappings:
    [AGREEMENT: она => She]
    [AGREEMENT: пошла => went]

Rejected:
  "He went to the sea"
  [WRONG: agreement_context_lost - the rejected output ignores feminine subject agreement - runtime cannot learn that 'она' controls the pronoun and past-tense verb family.]
  [RULE: tense=past]
  [RULE: case=dative]
  [RULE: agreement=subject_fem_past_fem]
  [AGREEMENT: она => She]
  [AGREEMENT: пошла => went]
```

The same row must produce three unit examples:

```text
semantic-tokenizer:
  chosen signature includes RU:TENSE:PAST, RU:CASE:DATIVE, RU:AGREEMENT:...
  rejected signature drops one of those rule tags.

executor-ru_en:
  chosen preserves the pronoun/verb mapping.
  rejected changes exactly one rule boundary: wrong pronoun, wrong tense, wrong case, or literal preposition.

quality-verifier:
  chosen fails a fluent candidate that violates the rule.
  rejected falsely accepts it.
```

### Rule-tag batch mix

For the next 100 training rows, include at least:

```text
20 tense/aspect rows
20 agreement/coreference rows
20 case/preposition rows
15 exception-word rows
10 article/countability rows
10 word-order rows
5 idiom/false-friend rows
```

Then test on the next 100 never-seen rows from the same rule families. Remove
any candidate row whose source text overlaps more than 70% with training
history before the run.

---

### 3B-Ext. Per-Rule DPO Pair Examples — Russian

Every example below must expand into three unit-level rows (semantic-tokenizer,
executor, quality-verifier) when used in a translation training batch.

#### Russian — Case: Accusative Animate (G01)

```json
{
  "rule_id": "RU_CASE_ACCUSATIVE_ANIMATE",
  "rule_family": "G01",
  "a_side": "Translate: I see my brother.",
  "chosen": "Я вижу брата.",
  "rejected": "Я вижу брат.\n[WRONG: morphology_error - animate masculine noun as direct object requires accusative 'брата'; the rejected row would teach the model the wrong object case.]",
  "rejected_type": "wrong_case",
  "feature_changed": "Case",
  "correct_feature_value": "Acc",
  "rejected_feature_value": "Nom",
  "features": {"upos": "NOUN", "Animacy": "Anim", "Gender": "Masc", "Number": "Sing", "Case": "Acc"}
}
```

#### Russian — Adjective-Noun Gender Agreement (G03)

```json
{
  "rule_id": "RU_ADJ_NOUN_GENDER_AGREEMENT",
  "rule_family": "G03",
  "a_side": "Use adjective 'красивый' with noun 'книга'.",
  "chosen": "красивая книга",
  "rejected": "красивый книга\n[WRONG: agreement_error - 'книга' is feminine; the adjective must agree in gender; rejected row would teach wrong gender agreement.]",
  "rejected_type": "gender_agreement_error",
  "feature_changed": "Gender",
  "correct_feature_value": "Fem",
  "rejected_feature_value": "Masc"
}
```

#### Russian — Adjective-Noun Neuter Agreement (G03)

```json
{
  "rule_id": "RU_ADJ_NOUN_GENDER_AGREEMENT_NEUT",
  "rule_family": "G03",
  "a_side": "Use adjective 'большой' with noun 'окно'.",
  "chosen": "большое окно",
  "rejected": "большая окно\n[WRONG: agreement_error - 'окно' is neuter; adjective must agree in neuter gender.]",
  "rejected_type": "gender_agreement_error"
}
```

#### Russian — Verb Aspect Perfective (G04)

```json
{
  "rule_id": "RU_ASPECT_COMPLETED_ACTION",
  "rule_family": "G04",
  "a_side": "Express: I finished reading the book (completed result).",
  "chosen": "Я прочитал книгу.",
  "rejected": "Я читал книгу.\n[WRONG: aspect_error - imperfective form describes ongoing process, not completed result; rejected row would lose the completion signal.]",
  "rejected_type": "wrong_aspect",
  "feature_changed": "Aspect",
  "correct_feature_value": "Perf",
  "rejected_feature_value": "Imp"
}
```

Note: the rejected side here is grammatically correct Russian — it is semantically
wrong for the intended meaning. This is a semantic DPO signal, not a grammar error.

#### Russian — Past Tense Gender Agreement (G03)

```json
{
  "rule_id": "RU_PAST_TENSE_GENDER_FEM",
  "rule_family": "G03",
  "a_side": "Translate: She went to the sea.",
  "chosen": "Она пошла к морю.\n[AGREEMENT: Она => She]\n[AGREEMENT: пошла => went]\n[RULE: tense=past]\n[RULE: case=dative]\n[RULE: agreement=subject_fem_past_fem]",
  "rejected": "Он пошёл к морю.\n[WRONG: agreement_context_lost - feminine subject 'она' requires feminine past verb form 'пошла'; rejected output uses masculine subject and verb, losing gender agreement.]",
  "rejected_type": "gender_agreement_error",
  "feature_changed": "Gender",
  "correct_feature_value": "Fem",
  "rejected_feature_value": "Masc"
}
```

Alternate form storage: executor must store both forms:
`[AGREEMENT: пошла => went]` + `[AGREEMENT: пошёл => went]`
so unseen feminine triggers match from masculine training and vice versa.

#### Russian — Dative after Preposition К (G01)

```json
{
  "rule_id": "RU_CASE_DATIVE_PREPOSITION_K",
  "rule_family": "G01",
  "a_side": "Translate: She went to the sea.",
  "chosen": "Она пошла к морю.",
  "rejected": "Она пошла к море.\n[WRONG: morphology_error - preposition 'к' governs dative; 'море' is prepositional form; rejected row would teach the wrong case for 'к'.]",
  "rejected_type": "wrong_case",
  "feature_changed": "Case",
  "correct_feature_value": "Dat",
  "rejected_feature_value": "Prep"
}
```

#### Russian — Prepositional after О (G01)

```json
{
  "rule_id": "RU_CASE_PREPOSITIONAL_AFTER_O",
  "rule_family": "G01",
  "a_side": "We talked about the house.",
  "chosen": "Мы говорили о доме.",
  "rejected": "Мы говорили о дом.\n[WRONG: morphology_error - preposition 'о' requires prepositional case; rejected uses nominative 'дом'.]",
  "rejected_type": "wrong_case",
  "feature_changed": "Case",
  "correct_feature_value": "Prep",
  "rejected_feature_value": "Nom"
}
```

#### Russian — Animacy in Accusative Plural (G01)

```json
{
  "rule_id": "RU_CASE_ACCUSATIVE_ANIMATE_PLURAL",
  "rule_family": "G01",
  "a_side": "I see the students.",
  "chosen": "Я вижу студентов.",
  "rejected": "Я вижу студенты.\n[WRONG: morphology_error - animate plural direct object uses genitive-like accusative form 'студентов'; rejected uses nominative plural 'студенты'.]",
  "rejected_type": "wrong_case"
}
```

#### Russian — Numeral Plural Rules via CLDR (G06)

```json
{
  "rule_id": "RU_PLURAL_NUMERAL_5_GENITIVE_PLURAL",
  "rule_family": "G06",
  "a_side": "5 files in Russian.",
  "chosen": "5 файлов",
  "rejected": "5 файла\n[WRONG: plural_rule_error - numerals 5-20 require genitive plural; rejected uses genitive singular (2-4 pattern).]",
  "rejected_type": "wrong_plural_form",
  "features": {"Number": "Plur", "Case": "Gen", "Numeral": "5"}
}
```

```json
{
  "rule_id": "RU_PLURAL_NUMERAL_21",
  "rule_family": "G06",
  "a_side": "21 files in Russian.",
  "chosen": "21 файл",
  "rejected": "21 файлов\n[WRONG: plural_rule_error - compound numeral ending in 1 uses nominative singular; rejected uses genitive plural.]",
  "rejected_type": "wrong_plural_form"
}
```

#### Russian — Pronoun-Noun Coreference (G03)

```json
{
  "rule_id": "RU_PRONOUN_NOUN_COREFERENCE_KOT",
  "rule_family": "G03",
  "a_side": "The cat forgot. Use correct pronoun.",
  "chosen": "Кот забыл. Он потерял.\n[AGREEMENT: кот => he]\n[AGREEMENT: забыл => forgot]",
  "rejected": "Кот забыл. Она потеряла.\n[WRONG: coreference_error - 'кот' (cat, masculine) binds to pronoun 'он/he', not 'она/she'; rejected assigns feminine agreement to masculine antecedent.]",
  "rejected_type": "coreference_error"
}
```

Standard coreference bindings to store:
```text
кот        → он / he
кошка      → она / she
дерево     → оно / it
ребёнок    → он / he (default, or she depending on context)
персонаж с именем → контекстно-зависимо
```

#### Russian — Irregular Plural (G07)

```json
{
  "rule_id": "RU_IRREGULAR_PLURAL_HUMAN",
  "rule_family": "G07",
  "a_side": "Many people came.",
  "chosen": "Пришло много людей.",
  "rejected": "Пришло много человеков.\n[WRONG: irregular_form_error - plural of 'человек' is suppletive 'люди/людей'; regular form 'человеков' does not exist.]",
  "rejected_type": "irregular_form_error"
}
```

#### Russian — False Friend (G08)

```json
{
  "rule_id": "RU_EN_FALSE_FRIEND_MAGAZIN",
  "rule_family": "G08",
  "a_side": "Я зашёл в магазин.",
  "chosen": "I went into a store.",
  "rejected": "I went into a magazine.\n[WRONG: false_cognate - Russian 'магазин' means store/shop, not magazine.]",
  "rejected_type": "false_cognate"
}
```

---

### 3B-Ext. Per-Rule DPO Pair Examples — English

#### English — Irregular Verb Past Tense (G07)

```json
{
  "rule_id": "EN_IRREGULAR_VERB_GO_PAST",
  "rule_family": "G07",
  "a_side": "Use 'go' in past tense: She __ home.",
  "chosen": "She went home.",
  "rejected": "She goed home.\n[WRONG: morphology_error - 'go' has irregular past 'went'; rejected regularizes an irregular verb.]",
  "rejected_type": "irregular_form_error",
  "feature_changed": "Tense",
  "correct_feature_value": "Past_went",
  "rejected_feature_value": "Past_regularized"
}
```

Required irregular verb list (minimum for RU→EN direction):

| Base | Past | Participle |
|---|---|---|
| go | went | gone |
| be | was / were | been |
| have | had | had |
| do | did | done |
| see | saw | seen |
| write | wrote | written |
| bring | brought | brought |
| think | thought | thought |
| take | took | taken |
| give | gave | given |
| come | came | come |
| say | said | said |

#### English — Present Perfect vs Past Simple (G04)

```json
{
  "rule_id": "EN_PRESENT_PERFECT_EXPERIENCE",
  "rule_family": "G04",
  "a_side": "Express: life experience, no finished time marker.",
  "chosen": "I have visited London.",
  "rejected": "I visited London.\n[WRONG: tense_error - past simple presents event as finished; present perfect signals experience relevant to the present; rejected row would lose tense-aspect distinction.]",
  "rejected_type": "wrong_tense_aspect",
  "feature_changed": "Tense",
  "correct_feature_value": "Pres_Perf",
  "rejected_feature_value": "Past_Simple"
}
```

#### English — Article `an` Before Vowel Sound (G05)

```json
{
  "rule_id": "EN_ARTICLE_AN_BEFORE_VOWEL_SOUND",
  "rule_family": "G05",
  "a_side": "Use indefinite article before 'hour'.",
  "chosen": "an hour",
  "rejected": "a hour\n[WRONG: article_error - 'hour' begins with a vowel sound /aʊ/; article choice follows pronunciation, not spelling; rejected would teach spelling-based selection.]",
  "rejected_type": "wrong_article"
}
```

#### English — Uncountable Noun (G05)

```json
{
  "rule_id": "EN_UNCOUNTABLE_NOUN_ADVICE",
  "rule_family": "G05",
  "a_side": "Give me some advice (uncountable).",
  "chosen": "some advice",
  "rejected": "an advice\n[WRONG: article_error - 'advice' is uncountable and takes no indefinite article; rejected applies countable article pattern.]",
  "rejected_type": "wrong_article"
}
```

#### English — Third-Person Singular Agreement (G03)

```json
{
  "rule_id": "EN_SUBJECT_VERB_AGREEMENT_3SG",
  "rule_family": "G03",
  "a_side": "He work(s) every day.",
  "chosen": "He works every day.",
  "rejected": "He work every day.\n[WRONG: agreement_error - third-person singular present requires -s suffix; rejected drops the suffix.]",
  "rejected_type": "agreement_error"
}
```

#### English — Embedded Question Word Order (G02)

```json
{
  "rule_id": "EN_EMBEDDED_QUESTION_WORD_ORDER",
  "rule_family": "G02",
  "a_side": "Make an indirect statement: I know + where she lives.",
  "chosen": "I know where she lives.",
  "rejected": "I know where does she live.\n[WRONG: word_order_error - embedded questions use statement word order (subject before verb); rejected applies direct-question inversion inside a subordinate clause.]",
  "rejected_type": "wrong_word_order"
}
```

#### English — Modal No -s (G03)

```json
{
  "rule_id": "EN_MODAL_BASE_VERB_NO_S",
  "rule_family": "G03",
  "a_side": "He can speak(s) English.",
  "chosen": "He can speak English.",
  "rejected": "He can speaks English.\n[WRONG: agreement_error - modal verbs are always followed by bare infinitive; -s suffix is not used after modals.]",
  "rejected_type": "agreement_error"
}
```

#### English — Irregular Plural (G07)

```json
{
  "rule_id": "EN_IRREGULAR_PLURAL_CHILD",
  "rule_family": "G07",
  "a_side": "Many children came.",
  "chosen": "Many children came.",
  "rejected": "Many childs came.\n[WRONG: irregular_form_error - plural of 'child' is suppletive 'children'; rejected regularizes an irregular noun.]",
  "rejected_type": "irregular_form_error"
}
```

#### English — Preposition Collocations (G01 / G08)

```json
{
  "rule_id": "EN_PREPOSITION_INTERESTED_IN",
  "rule_family": "G01",
  "a_side": "She is interested in music.",
  "chosen": "She is interested in music.",
  "rejected": "She is interested about music.\n[WRONG: wrong_preposition - the fixed collocation 'interested in' is required; 'about' is a common learner error from Russian prepositional reasoning.]",
  "rejected_type": "wrong_preposition"
}
```

Common EN preposition traps for Russian speakers:

| Correct English | Typical Russian-influenced error |
|---|---|
| interested in | interested about |
| depend on | depend from |
| listen to music | listen music |
| wait for someone | wait someone |
| responsible for | responsible of |
| good at something | good in something |

---

### Language Rule Source Protocol

Language rules must come from source-governed datasets, not from agent memory.
For every language, create or update a rule-source inventory before generating
grammar-rule DPO rows.

Required source families:

| Tier | Source family | What it contributes | Required handling |
|---|---|---|---|
| 1 | Universal Dependencies treebanks | POS, lemma, morphology features, dependency syntax | Use as canonical feature schema; check each treebank LICENSE. |
| 1 | UD Universal Features | Standard names: Case, Gender, Number, Tense, Aspect, Mood, Person, Animacy, Degree, VerbForm, Voice | Map all other sources into this schema. |
| 1 | UniMorph | lemma -> word form -> morphology bundle; paradigms and irregular inflections | Use for form/paradigm rows; check language repository license. |
| 1 | Unicode CLDR | plural rules, number/date/unit locale behavior | Use for plural and numeral rows; keep CLDR attribution. |
| 2 | LanguageTool | real grammar-error rules with incorrect/correct examples | Best source for rejected DPO; check module license before distribution. |
| 2 | Wiktionary via Wiktextract | inflection tables, exceptions, translations, usage examples | Keep as separate CC-BY-SA/GFDL shard. |
| 2 | OpenCorpora / pymorphy for Russian | Russian forms, grammemes, analyzer/generator path | Source is OpenCorpora; pymorphy is a tool. |
| 3 | Apertium | morphology dictionaries, bilingual dictionaries, transfer rules | Treat GPL data as isolated/reference unless product license is approved. |
| 3 | GF Resource Grammar Library | formal morphology and syntax generation | Use for synthetic grammar probes only after license review. |

Search pattern for any new language `<LANG>`:

```text
Universal Dependencies <LANG>
UD <LANG> treebank GitHub
UniMorph <LANG>
CLDR plural rules <LANG>
Wiktionary <LANG> inflection tables
Wiktextract <LANG>
LanguageTool <LANG> grammar.xml
Apertium <LANG>
Apertium <LANG1>-<LANG2>
GF Resource Grammar Library <LANG>
<LANG> morphological dictionary open source
<LANG> dependency treebank license
```

Russian source priority:

```text
UD Russian SynTagRus / GSD
UniMorph Russian
OpenCorpora Russian morphology dictionary
pymorphy2 or pymorphy3 as generator tool
CLDR Russian plural rules
LanguageTool Russian grammar.xml
Wiktionary Russian declension / conjugation through Wiktextract
Apertium Russian and GF RGL Russian as advanced/reference layers
```

English source priority:

```text
UD English EWT / GUM
UniMorph English
CLDR English plural rules
LanguageTool English grammar.xml
Wiktionary English irregular forms through Wiktextract
WordNet for semantic class only, not tense/article grammar
Apertium English and GF RGL English as advanced/reference layers
```

Every rule-source record must include:

```json
{
  "source_id": "ud_russian_syntagrus",
  "language": "ru",
  "source_family": "treebank",
  "features": ["pos", "case", "gender", "number", "animacy", "syntax"],
  "format": "conllu",
  "license": "CHECK_REPO_LICENSE",
  "commercial_allowed": "depends_on_license",
  "attribution_required": true,
  "share_alike": "depends_on_license",
  "allowed_for_training": "after_license_review"
}
```

### Grammar-Rule Rejected DPO Store

Language rules need their own rejected DPO, separate from ordinary sentence
translation rows. A grammar-rule row teaches the model to prefer one valid
grammar form over one plausible wrong form.

Required row shape:

```json
{
  "pair_id": "G01-RU-CASE-ACC-ANIM-000001",
  "domain": "grammar_rule_dpo",
  "language": "ru",
  "rule_family": "case",
  "rule_id": "RU_CASE_ACCUSATIVE_ANIMATE",
  "mechanism": "morphological_accuracy",
  "source_id": "opencorpora_or_ud_ru",
  "source_license_state": "ACCEPT",
  "a_side": "Translate or evaluate: I see my brother.",
  "chosen": "Я вижу брата.",
  "rejected": "Я вижу брат. [WRONG: morphology_error - animate masculine direct object requires accusative/genitive-like form 'брата'; the rejected row would teach the model the wrong object case.]",
  "rejected_type": "wrong_case",
  "feature_changed": "Case",
  "correct_feature_value": "Acc",
  "rejected_feature_value": "Nom",
  "evidence_terms": ["брат", "брата", "Animacy=Anim", "Case=Acc"],
  "quality_status": "train_ready",
  "compiled_prompt_in_training": false,
  "eval_only": false
}
```

Allowed grammar-rule DPO families:

| Family | Scope | Example rejected defect |
|---|---|---|
| G01 morphology | case, gender, number, person, verb form | `Я вижу брат` instead of `Я вижу брата`. |
| G02 syntax | word order, embedded questions, dependency role | `I know where does she live`. |
| G03 agreement | adjective/noun, subject/verb, pronoun/antecedent | `красивый книга`, `He work`. |
| G04 tense/aspect | tense choice, perfective/imperfective, perfect/progressive | `I visited London` where experience requires `have visited`. |
| G05 articles/determiners | a/an/the/zero, demonstratives | `a hour`, `an advice`. |
| G06 pluralization | CLDR numeral forms, irregular plural | `5 файла`, `childs`. |
| G07 exceptions | irregular verbs, suppletive forms, indeclinables | `goed`, `кофея`. |
| G08 lexical traps | false friends, homophones, confusables | `магазин -> magazine`. |
| G09 register/style | formal/informal mismatch | stiff formal chat translation. |
| G10 punctuation/script | commas, RTL, capitalization, direct speech | Hebrew visual-order or quote punctuation errors. |

Rejected generation strategies:

| Rule family | Negative strategy |
|---|---|
| case | Replace the correct case with the nearest plausible wrong case. |
| gender agreement | Keep the noun, change adjective/pronoun/verb gender only. |
| number agreement | Change number on one dependent word only. |
| tense | Replace tense with a close but semantically wrong tense. |
| aspect | Swap perfective and imperfective while keeping the sentence fluent. |
| article | Swap `a/an/the/zero` according to the targeted article rule. |
| plural | Regularize an irregular plural or choose the wrong numeral form. |
| irregular verb | Regularize an irregular verb: `goed`, `writed`. |
| word order | Apply direct-question inversion where embedded order is required. |
| preposition | Use a common learner error: `interested about`, `depend from`. |
| false friend | Choose the tempting cognate: `магазин -> magazine`. |
| idiom | Use literal calque instead of idiomatic translation. |
| register | Preserve meaning but shift tone to the wrong product register. |

Quality gates for grammar-rule rejected DPO:

```text
1. chosen != rejected
2. rejected contains [WRONG: ...]
3. rejected violates exactly one named rule_id
4. rejected remains plausible except for that one target defect
5. source_id and source_license_state are present
6. rejected_type, feature_changed, correct_feature_value, rejected_feature_value are present when morphology is in scope
7. chosen/rejected overlap is useful but not identical
8. chosen and rejected do not differ only by punctuation
9. the rule row expands into semantic-tokenizer, executor, and quality-verifier examples when used for translation training
10. eval-only sources never become train-ready rows
```

Important: a bad rejected row like `Я видеть брат` is invalid for case training
because it violates verb form and case at the same time. The valid rejected row
is `Я вижу брат`: same sentence, one defect.

Runtime use:

```text
rule-source registry
  -> grammar-rule DPO rows
  -> unit-level tokenizer/executor/verifier examples
  -> KNN signatures with RULE:* tags
  -> next 100 training rows
  -> next 100 unseen rows tested for >= 95% target after the user upgraded the goal
```

## 4. Universal TranslationLearningExample Shape

Every unit-level row uses the same container:

```text
TranslationLearningExample
  ExampleId
  UnitId
  FlowId = "translation-routing-flow"
  InputJson
  ChosenOutputJson
  RejectedOutputJson
  RejectedType
  SemanticSignature
  SourceId
  HeldOut
```

Only the JSON objects differ by unit.

Do not put training data only into `Prompt`, `ChosenResponse`, or
`RejectedResponse` from the legacy flat `LearningTrainingItem` path.
That path is a compatibility scaffold and does not teach translation units.

---

## 5. Unit 1 - SemanticTokenizerUnit

### Contract

```text
UnitId: semantic-tokenizer
Input:  TokenizerInput(RawText, SourceLang)
Output: TokenizerOutput(Pattern, Signature)
```

### What This Unit Learns

It learns how to turn a source sentence into structure:

- sentence pattern: `SVO`, `IMPERATIVE_V`, `VSO`, etc.;
- semantic signature used by KNN and executor;
- concept boundaries: adverb, action, object, code id, emoji, register marker.

If this unit is wrong, the executor will look in the wrong bucket.

### Good Pair - Technical Prompt

```text
Pair[RU-EN-TOK-001]
A:
  TokenizerInput {
    RawText: "добавь проверку null в метод ValidateToken",
    SourceLang: "ru"
  }
B:
  chosen:
    TokenizerOutput {
      Pattern: IMPERATIVE_V,
      Signature: "IMPERATIVE_V|ADD_OP|VALIDATE_CHECK|NULL_LITERAL|METHOD_REF|CODE_IDENTIFIER"
    }
  rejected:
    TokenizerOutput {
      Pattern: SVO,
      Signature: "SVO|ADD_OP|COMPOUND_UNKNOWN|METHOD_REF"
    }
    [WRONG: wrong_pattern_and_compound - this collapses 'проверку null' into an unknown compound and invents an explicit subject pattern - executor will miss the learned 'null check' structural bucket.]
Reason:
  why_chosen_is_correct: Russian imperative has no explicit subject; null is a modifier for check; ValidateToken is a code identifier.
  why_rejected_is_wrong: It loses the object/modifier boundary and sends the row to the wrong sentence pattern.
Coverage:
  source_id: ru-en-technical-prompt-001
  unit_id: semantic-tokenizer
  rejected_type: wrong_pattern_and_compound
  can_train: true
```

### Good Pair - Literary Adverb

```text
Pair[RU-EN-TOK-002]
A:
  TokenizerInput {
    RawText: "Он бесстрашно двинулся через враждебную толпу, не выдав ни малейшего страха.",
    SourceLang: "ru"
  }
B:
  chosen:
    TokenizerOutput {
      Pattern: SVO,
      Signature: "SVO|SUBJECT_HE|ADVERB_MANNER:fearless_absence|MOVE_THROUGH|HOSTILE_CROWD|NO_FEAR_TRACE"
    }
  rejected:
    TokenizerOutput {
      Pattern: SVO,
      Signature: "SVO|SUBJECT_HE|MOVE_THROUGH|HOSTILE_CROWD"
    }
    [WRONG: adverb_semantic_marker_dropped - this keeps the event structure but drops the semantic value carried by 'бесстрашно' - executor cannot learn to prefer 'intrepidly' over a bland movement translation.]
Reason:
  why_chosen_is_correct: The batch is about manner quality; the adverb must be a first-class token, not decoration.
  why_rejected_is_wrong: It makes the sentence look structurally complete while hiding the very learning target.
Coverage:
  source_id: ru-en-general-l1-b029-001
  unit_id: semantic-tokenizer
  rejected_type: adverb_semantic_marker_dropped
```

### Rejected Types For Tokenizer

| Rejected type | One defect | Functional consequence |
|---|---|---|
| `wrong_sentence_pattern` | SVO vs imperative/VSO/SOV wrong | executor applies wrong word order |
| `compound_collapse` | two meaningful tokens merged | KNN cannot find correct analogues |
| `code_identifier_lost` | code/entity token treated as ordinary word | verifier cannot protect it |
| `adverb_semantic_marker_dropped` | manner/value token omitted | executor cannot learn quality |
| `register_marker_dropped` | chat/business/formal marker omitted | output gets wrong tone |

### Tokenizer Hard Gates

- chosen signature must include the one concept that the pair is meant to teach;
- rejected signature must be plausible, not random;
- `RejectedType` must name the actual structural defect;
- held-out tokenizer rows must use unseen sentences, not the same source text
  with a different id.
- script coverage must be acceptable before training: English >= 97%, Russian >=
  95%, Hebrew >= 90% non-byte-fallback coverage on the configured evaluation
  corpus;
- reject tokenizer rows where more than 10% of tokens fall back to byte tokens;
  flag rows with 1%-10% byte fallback for review;
- Hebrew input must be logical-order UTF-8, not visual-order text;
- Russian `Е`/`Ё`, hard sign, and soft sign must not be collapsed when the row
  is teaching morphology or named entities.

---

## 6. Unit 2 - ExecutorUnit<RU_EN>

### Contract

```text
UnitId: executor-ru_en
Input:  ExecutorInput(SourceText, SourceLang, TopicDomain)
Output: ExecutorOutput(Text, Confidence, WordMappings?)
```

### What This Unit Learns

It assembles the target English translation.

For RU_EN it must learn:

- source word/value -> target phrase;
- structural template;
- word-order correction;
- article/compound choices;
- code/entity preservation;
- register and tone.

The executor is where the held-out failure currently lives. Batch-029 train-row
smoke can memorize rows, but unseen rows fall back to Bootstrap. That means new
executor pairs must teach reusable value mappings and templates, not only exact
sentence lookup.

Current code status: `TranslationStructuredBatchBuilder` now emits initial
`WordMappings` for known technical concepts, code identifiers, and simple
adverb markers when it can infer them. Human-authored train-ready executor rows
should still include explicit `WordMappings` for important meaning carriers;
otherwise the builder fallback may mark them as `UNKNOWN` and the row will teach
less than intended.

### Good Pair - Adverb Value Mapping

```text
Pair[RU-EN-EXEC-001]
A:
  ExecutorInput {
    SourceText: "Он бесстрашно двинулся через враждебную толпу, не выдав ни малейшего страха.",
    SourceLang: "ru",
    TopicDomain: "general"
  }
  SemanticSignature: "SVO|SUBJECT_HE|ADVERB_MANNER:fearless_absence|MOVE_THROUGH|HOSTILE_CROWD|NO_FEAR_TRACE"
B:
  chosen:
    ExecutorOutput {
      Text: "He intrepidly advanced through the hostile crowd, betraying not a trace of fear!",
      Confidence: 0.93,
      WordMappings: [
        { SourceWord:"бесстрашно", Concept:UNKNOWN, TargetWord:"intrepidly" },
        { SourceWord:"двинулся", Concept:UNKNOWN, TargetWord:"advanced" },
        { SourceWord:"толпу", Concept:UNKNOWN, TargetWord:"crowd" }
      ]
    }
  rejected:
    ExecutorOutput {
      Text: "He advanced through the hostile crowd, betraying not a trace of fear!",
      Confidence: 0.31,
      WordMappings: [
        { SourceWord:"двинулся", Concept:UNKNOWN, TargetWord:"advanced" }
      ]
    }
    [WRONG: adverb_value_omitted - the rejected translation keeps the event but drops the fearless manner carried by 'бесстрашно' - held-out rows with new adverbs will keep falling back to bland movement translations.]
Reason:
  why_chosen_is_correct: It preserves the event and the manner value.
  why_rejected_is_wrong: It is grammatical and plausible, but loses the batch target.
Coverage:
  source_id: ru-en-general-l1-b029-001
  unit_id: executor-ru_en
  rejected_type: adverb_value_omitted
  stream: RU_EN x general x adverb-quality
```

### Good Pair - Technical Compound

```text
Pair[RU-EN-EXEC-002]
A:
  ExecutorInput {
    SourceText: "добавь проверку null в метод ValidateToken",
    SourceLang: "ru",
    TopicDomain: "technical"
  }
  SemanticSignature: "IMPERATIVE_V|ADD_OP|VALIDATE_CHECK|NULL_LITERAL|METHOD_REF|CODE_IDENTIFIER"
B:
  chosen:
    ExecutorOutput {
      Text: "Add a null check to the ValidateToken method",
      Confidence: 0.92,
      WordMappings: [
        { SourceWord:"добавь", Concept:ADD_OP, TargetWord:"Add" },
        { SourceWord:"проверку", Concept:VALIDATE_CHECK, TargetWord:"check" },
        { SourceWord:"null", Concept:NULL_LITERAL, TargetWord:"null" },
        { SourceWord:"ValidateToken", Concept:CODE_IDENTIFIER, TargetWord:"ValidateToken" }
      ]
    }
  rejected:
    ExecutorOutput {
      Text: "Add check null to method ValidateToken",
      Confidence: 0.35,
      WordMappings: [
        { SourceWord:"проверку", Concept:VALIDATE_CHECK, TargetWord:"check" },
        { SourceWord:"null", Concept:NULL_LITERAL, TargetWord:"null" }
      ]
    }
    [WRONG: literal_compound_order - this keeps the right content words but preserves Russian order instead of English 'null check' compound order - runtime output remains understandable but visibly machine-translated.]
Coverage:
  rejected_type: literal_compound_order
```

### Good Pair - Chat Register

```text
Pair[RU-EN-EXEC-003]
A:
  ExecutorInput {
    SourceText: "Брат, я уже третий день мучаюсь с этим роутером 😅",
    SourceLang: "ru",
    TopicDomain: "chat"
  }
  SemanticSignature: "CHAT|FRIENDLY_ADDRESS|STRUGGLE|ROUTER|EMOJI"
B:
  chosen:
    ExecutorOutput {
      Text: "Bro, I've been stuck with this router for three days now 😅",
      Confidence: 0.94,
      WordMappings: []
    }
  rejected:
    ExecutorOutput {
      Text: "Brother, I am suffering with this router for the third day.",
      Confidence: 0.37,
      WordMappings: []
    }
    [WRONG: chat_register_and_emoji_loss - this is semantically close but too literal, too stiff, and drops the emoji - chat translation would feel unnatural to users.]
Coverage:
  rejected_type: chat_register_and_emoji_loss
```

### Rejected Types For Executor

| Rejected type | One defect | Functional consequence |
|---|---|---|
| `adverb_value_omitted` | event translated, manner value lost | batch-029 cannot improve |
| `wrong_adverb_value` | wrong English adverb chosen | tone/meaning shifts |
| `literal_compound_order` | source order copied | machine-translation artifact |
| `entity_loss` | code/person/place/currency altered | user-visible factual error |
| `register_shift` | formal vs chat/business wrong | product tone fails |
| `emoji_loss` | emoji removed | chat/post fidelity fails |
| `omission` | clause dropped | meaning incomplete |

### Executor Batch Mix

For one 50-row RU_EN executor batch:

```text
15 adverb/value rows
10 structure/template rows
8 entity-preservation rows
7 register/tone rows
5 omission rows
5 compound/article rows
```

If the batch follows the older T01-T09 stream instead of the adverb repair
stream, keep the fixed-guidance proportions visible in the report:

```text
>= 40% semantic errors
20%-30% morphology where relevant
15%-25% register/style
10%-15% structural/completeness
```

Do not train only one rejected type unless the previous mixed diagnostic showed
that exact type as weak. Focused repetition alone did not solve code-doc
negative DPO; the fix was richer evidence shape and a mixed confirmation batch.

---

## 7. Unit 3 - QualityVerifierUnit

### Contract

```text
UnitId: quality-verifier
Input:  VerifierInput(SourceText, CandidateText)
Output: VerifierOutput(Score, Issues, Pass, CorrectionHint)
```

### What This Unit Learns

It decides whether a candidate translation should be accepted or rejected.

The verifier must learn:

- required meaning markers;
- required entities;
- register preservation;
- emoji preservation;
- code identifier preservation;
- false acceptance vs false rejection.

This unit must not be a rubber stamp. If the executor emits a fluent but
meaning-losing candidate, verifier must fail it and provide a correction hint.

### Good Pair - False Acceptance Of Missing Adverb

```text
Pair[RU-EN-VER-001]
A:
  VerifierInput {
    SourceText: "Она горестно прощалась с домом, в котором прожила тридцать лет.",
    CandidateText: "She bid farewell to the house she had lived in for thirty years!"
  }
  SemanticSignature: "SVO|SUBJECT_SHE|ADVERB_MANNER:grief|FAREWELL|HOUSE|THIRTY_YEARS"
B:
  chosen:
    VerifierOutput {
      Score: 0.42,
      Issues: ["expected 'dolefully' or equivalent grief marker for 'горестно'"],
      Pass: false,
      CorrectionHint: "She dolefully bid farewell to the house she had lived in for thirty years!"
    }
  rejected:
    VerifierOutput {
      Score: 0.91,
      Issues: [],
      Pass: true,
      CorrectionHint: null
    }
    [WRONG: verifier_false_acceptance - the candidate is fluent but drops the grief manner marker - promotion would accept translations that erase the value target of the batch.]
Coverage:
  rejected_type: verifier_false_acceptance
```

### Good Pair - False Rejection

```text
Pair[RU-EN-VER-002]
A:
  VerifierInput {
    SourceText: "Спасибо за коннект! Хочу обсудить возможное сотрудничество 🤝",
    CandidateText: "Thanks for connecting! I'd love to discuss potential collaboration 🤝"
  }
B:
  chosen:
    VerifierOutput {
      Score: 0.96,
      Issues: [],
      Pass: true,
      CorrectionHint: null
    }
  rejected:
    VerifierOutput {
      Score: 0.44,
      Issues: ["too informal"],
      Pass: false,
      CorrectionHint: "Thank you for establishing a connection. I would like to discuss possible cooperation."
    }
    [WRONG: verifier_false_rejection - this rejects a natural networking-chat translation and pushes the system toward stiff formal English - chat users would receive unnatural messages.]
Coverage:
  rejected_type: verifier_false_rejection
```

### Rejected Types For Verifier

| Rejected type | One defect | Functional consequence |
|---|---|---|
| `verifier_false_acceptance` | bad candidate passes | low-quality translation promoted |
| `verifier_false_rejection` | good candidate rejected | model falls back or over-formalizes |
| `required_marker_missing` | adverb/value marker ignored | batch target disappears |
| `entity_loss_missed` | entity/code/currency loss ignored | factual corruption |
| `register_mismatch_missed` | tone mismatch ignored | chat/post UX fails |
| `emoji_loss_missed` | emoji loss ignored | social meaning lost |

### Verifier Hard Gates

- Every fail example must name the missing marker or entity.
- Every pass example must be genuinely acceptable, not just similar.
- Verifier batches must include false acceptance and false rejection rows.
- Diagnostics must report accuracy by rejected type, not only average accuracy.

---

## 8. One Source Row Expanded Into Three Unit Rows

For a source pair:

```text
source:  "Он бесстрашно двинулся через враждебную толпу..."
chosen:  "He intrepidly advanced through the hostile crowd..."
rejected:"[WRONG: fearless_quality_missed ...]"
```

Create:

```text
1. semantic-tokenizer example
   Teaches: 'бесстрашно' must exist as ADVERB_MANNER:fearless_absence.

2. executor-ru_en example
   Teaches: source structure + adverb marker maps to the chosen English sentence.

3. quality-verifier example
   Teaches: a candidate missing 'intrepidly' or equivalent must fail.
```

All three examples share the same semantic signature family.
If the tokenizer and executor use unrelated signatures, training will look good
internally and fail at runtime.

---

## 9. Batch Construction Rules

### 50-Pair Stream

One accepted batch is:

```text
50 train source rows
50 held-out source rows
150 train unit examples
150 held-out unit examples
one runner report
one failed-row ledger
one next-batch decision
```

For a small local proof, `40 train / 10 held-out` is allowed only as a diagnostic
runner. It is not promotion evidence.

### Stream Identity

Do not mix these in one batch:

- RU_EN and HE_EN;
- chat and literary;
- technical prompt and social post;
- adverb-quality and idiom/finance/medical/legal.

Use stream identity:

```text
direction x register x topic_domain x pair_family x unit_id
```

For the full translation curriculum, the stream identity is:

```text
direction x topic_domain x translation_level x register x family x unit_id
```

The first production-like run should stay narrow, for example:

```text
RU_EN x general x Level 1 x literary x T01/T08 x executor-ru_en
```

Do not mix `ACCEPT`, `QUARANTINE`, and `REVIEW_REQUIRED` source rows in one
training file. Quarantine sources require isolated evaluation reports and never
become clean train-ready rows by accident.

Examples:

```text
RU_EN x literary x general x adverb-quality x executor-ru_en
RU_EN x informal x chat x chat-message x quality-verifier
HE_EN x business x networking x post-text x semantic-tokenizer
```

### Minimum Per-Unit Coverage

Every batch must contain all three unit ids:

```text
semantic-tokenizer
executor-ru_en
quality-verifier
```

If only executor rows exist, the flow can memorize chosen translations but
cannot learn stable routing through tokenizer/verifier.

---

## 10. Diagnostics Required Before Promotion

The runner report must include:

```text
batch_id
direction
register
topic_domain
source_rows
train_rows
heldout_rows
unit_examples_trained
unit_examples_heldout
accuracy_by_unit
accuracy_by_rejected_type
runtime_exact_accuracy
runtime_product_accuracy
reward_margin
loss
bleu
chrf
target_script_accuracy
copied_source_rate
wrong_direction_rate
named_entity_preservation
completeness_score
cpu_ms
peak_ram_mb
elapsed_ms
checkpoint_ref
manifest_ref
failed_row_ledger_path
passes_gate
```

The human-readable run report must also explain:

- what improved;
- what got worse;
- most common error category;
- whether the failure is pair quality, tokenizer, routing, or model-capacity;
- whether to continue the stream, repair the same batch, or stop the stream.

Promotion is blocked when:

- runtime exact/product accuracy is `0.0`;
- any required unit id is missing;
- a rejected type is below threshold even if average accuracy passes;
- reward margin is projected or missing;
- prompt tokens are trained as answer tokens;
- held-out rows were included in training;
- output is only `[RU_EN:BOOTSTRAP] ...`.

---

## 11. How To Repair Batch-029

Batch-029 currently teaches RU_EN literary adverb quality.
The failure is clear:

```text
unit-level held-out accuracy: 0.666667
runtime exact accuracy:      0.000000
actual output:               [RU_EN:BOOTSTRAP] unseen Russian sentence
```

The next pair-authoring repair is not "add more of the same exact rows".

Required repair:

1. tokenizer pairs must mark each Russian adverb as a reusable semantic value:
   `ADVERB_MANNER:grief`, `ADVERB_MANNER:tenderness`,
   `ADVERB_MANNER:lingering_duration`, etc.;
   these are `SemanticSignature` labels, not `SemanticConcept` enum values;
2. executor pairs must include reusable `WordMappings` for the adverb value:
   `горестно -> dolefully`, `нежно -> tenderly`, `долго -> lingeringly`;
3. executor rejected sides must be concrete near-miss translations, not only a
   prose explanation;
4. verifier pairs must fail fluent candidates that omit the required adverb
   value;
5. held-out rows must be unseen adverbs or unseen sentence templates from the
   same stream.

The executable `TranslationBatch029Runner` now also accepts explicit
supplemental train-ready rows for this repair path. These rows must not copy the
held-out source sentence. They should use a different source sentence that
teaches the same reusable marker or phrase boundary. Example shape:

```text
supplemental source: "Инвестор дальновидно оценил риск до подписания договора."
chosen:              "The investor astutely assessed the risk before signing the contract!"
held-out target:     "Инвестор дальновидно вложил капитал до начала подъёма рынка."
lesson:              learn the marker дальновидно -> astutely without memorizing the held-out sentence.
```

This is still not full sentence translation. The first repair target is product
accuracy: no Bootstrap fallback and the learned meaning marker is preserved.
Full English sentence exactness requires a later sentence-template/composition
training slice.

That later slice now has an executable row format. Put reusable phrase
boundaries in the rejected-side metadata as data, not as code:

```text
[PHRASE: Она => She]
[PHRASE: горестно => dolefully]
[PHRASE: прощалась => bid farewell]
[PHRASE: с домом => to the house]
[PHRASE: в котором прожила тридцать лет => she had lived in for thirty years!]
[WRONG: phrase_boundary_lost - missing reusable source-to-target phrase boundaries keeps exact held-out sentence composition at zero.]
```

`TranslationStructuredBatchBuilder` turns those annotations into
`ExecutorPhraseMapping` rows. `ExecutorUnit` then composes a runtime sentence
from learned phrase mappings only when phrase coverage is high enough. The code
does not decide that `прощалась` means `bid farewell`; that boundary must come
from the DPO row.

The source-governed batch-029 phrase repair artifact is:

```text
27.TranslationDomain/TrainingData/ru-en-general-l1-b029-phrase-repair-train-ready.jsonl
```

That artifact is the preferred way to supply phrase-boundary rows to
`TranslationBatch029Runner`. Tests and live probes should pass it through
`supplementalTrainingPath`, not through hidden in-memory helper rows.

Expected improvement for this narrow repair stream:

```text
runtime_exact_accuracy: target >= 0.95 on batch-029 rows 041-050
runtime_product_accuracy: target >= 0.95 on batch-029 rows 041-050
runtime_bootstrap_rate:  target <= 0.15
```

Exact-match may stay harsh because natural translation has many valid phrasings.
Therefore every report must include both exact match and product-level review.

---

## 12. Bad Pair Patterns To Reject

Reject a pair before training if:

- chosen and rejected differ only by label text;
- rejected has no `[WRONG: ...]`;
- rejected is nonsense rather than a plausible wrong translation;
- rejected is a refusal or placeholder ("cannot translate", "N/A");
- tokenizer rejected drops every concept instead of one teaching boundary;
- executor rejected changes several things at once;
- verifier rejected always says `Pass: true`;
- source row lacks license/source id;
- source row is not allowed by `TRANSLATION-SOURCE-REGISTRY-v3`;
- chosen came from raw machine translation without human/corpus authority;
- chosen/rejected are written in different target languages;
- Hebrew has visual-order/bidi corruption or mojibake;
- row comes from blocked or repair-required file;
- row is training and held-out at the same time;
- pair teaches an exact id signature only when the goal is generalization.

Exact-id signatures are allowed for smoke tests.
Structural signatures are required for held-out generalization tests.

---

## 13. Authoring Checklist

Before adding a pair:

```text
[ ] I know which unit this pair trains.
[ ] I know whether this is flow-level routing or unit-level translation.
[ ] A-side is the exact input object for that unit.
[ ] Chosen is the exact output object for that unit.
[ ] Rejected is plausible and has one primary defect.
[ ] Rejected contains [WRONG: weakness - cannot learn - consequence].
[ ] SemanticSignature is shared with the matching unit examples.
[ ] SourceId is traceable.
[ ] Source is allowed by TRANSLATION-SOURCE-REGISTRY-v3 for this role.
[ ] compiled_prompt_in_training is false on source-level translation rows.
[ ] Chosen and rejected are both in the declared target language.
[ ] Token overlap gate passed or the row is short enough to skip it.
[ ] HeldOut is correct.
[ ] RejectedType is one of the tracked diagnostics.
[ ] This row belongs to one stream only.
```

Before accepting a batch:

```text
[ ] all three unit ids are represented;
[ ] 50 train source rows and 50 held-out source rows exist for promotion;
[ ] runner emitted metrics, not projected metrics;
[ ] per-unit and per-rejected-type diagnostics pass;
[ ] runtime held-out output is not Bootstrap;
[ ] failed rows are persisted and reviewed;
[ ] checkpoint and manifest refs are non-empty;
[ ] no raw/private source material is committed.
```
