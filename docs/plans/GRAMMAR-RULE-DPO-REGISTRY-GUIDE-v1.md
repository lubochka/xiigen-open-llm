# GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1.md
# Version: 1.0 - 2026-05-29
# Purpose: Registry specification and authoring rules for grammar-rule DPO pairs
#          used to teach language-level accuracy in TranslationDomain and any
#          future domain that requires morphological/syntactic precision.
# Scope: All language-pair directions; primary coverage RU and EN.
# Based on: TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1 §3B,
#           NEW-DOMAIN-DEVELOPMENT-GUIDE-v1 §3J,
#           Universal Dependencies, UniMorph, CLDR, LanguageTool, OpenCorpora.

---

## 1. Purpose

Grammar-rule DPO pairs are separate from ordinary sentence-translation pairs.
A grammar-rule row teaches the model to prefer one grammatically or
semantically correct form over one plausible wrong form that violates exactly
one named rule.

These rows feed the same three-unit pipeline:
- semantic-tokenizer: signature must include the RULE:* tag
- executor: WordMappings or phrase boundary must encode the correct form
- quality-verifier: must reject the defective form as a near-miss candidate

All rules are DPO signals. None may be hardcoded as `if/else` or
as a static dictionary in production code (anti-determinism rule).

---

## 2. G01–G10 Family Taxonomy

| Family | Scope | Typical rejected defect |
|---|---|---|
| G01 morphology | case, number, verb form, person | `Я вижу брат` instead of `Я вижу брата` |
| G02 syntax | word order, embedded questions, dependency | `I know where does she live` |
| G03 agreement | adj/noun, subject/verb, pronoun/antecedent | `красивый книга`, `He work` |
| G04 tense/aspect | tense choice, perfective/imperfective, perfect/progressive | `I visited London` where experience requires `have visited` |
| G05 articles/determiners | a/an/the/zero, demonstratives | `a hour`, `an advice` |
| G06 pluralization | CLDR numeral rules, irregular plural | `5 файла`, `childs` |
| G07 exceptions | irregular verbs, suppletive forms, indeclinables | `goed`, `кофея` |
| G08 lexical traps | false friends, homophones, confusables | `магазин -> magazine` |
| G09 register/style | formal/informal mismatch | stilted formal in chat context |
| G10 punctuation/script | RTL discipline, capitalization, direct speech | wrong quote order in Hebrew |

---

## 3. Required Row Shape

Every grammar-rule DPO pair must include the following fields.

```json
{
  "pair_id": "G01-RU-CASE-ACC-ANIM-000001",
  "domain": "grammar_rule_dpo",
  "language": "ru",
  "direction": null,
  "source_family": "G01",
  "rule_id": "RU_CASE_ACCUSATIVE_ANIMATE",
  "rule_description": "Animate masculine nouns as direct object require accusative form (matching genitive).",
  "mechanism": "morphological_accuracy",
  "source_id": "opencorpora",
  "source_license_state": "ACCEPT",
  "a_side": "Translate: I see my brother.",
  "chosen": "Я вижу брата.",
  "rejected": "Я вижу брат.\n[WRONG: morphology_error - animate masculine direct object requires accusative 'брата'; rejected teaches wrong object case.]",
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

Fields that may be omitted only when genuinely not applicable:
- `direction`: omit for monolingual grammar rows
- `feature_changed` / `correct_feature_value` / `rejected_feature_value`: required when rule is morphological (G01, G03, G04, G05, G06, G07); optional for G02, G08, G09, G10

---

## 4. Pair ID Naming Convention

```text
<family>-<language>-<rule_short>-<sequence>

G01-RU-CASE-ACC-ANIM-000001   — Russian, accusative animate, row 1
G03-RU-AGREE-GENDER-000001    — Russian, gender agreement, row 1
G04-EN-TENSE-PRES-PERF-000001 — English, present perfect, row 1
G05-EN-ARTICLE-AN-000001      — English, article a/an, row 1
G07-RU-IRREG-PLURAL-000001    — Russian, irregular plural, row 1
G08-RU-EN-FALSE-FRIEND-000001 — RU→EN false friend, row 1
```

---

## 5. Rule ID Naming Convention

```text
<LANG>_<FAMILY>_<DESCRIPTION>

RU_CASE_ACCUSATIVE_ANIMATE
RU_CASE_DATIVE_PREPOSITION_K
RU_CASE_PREPOSITIONAL_AFTER_O
RU_ADJ_NOUN_GENDER_AGREEMENT
RU_ASPECT_COMPLETED_ACTION
RU_PAST_TENSE_GENDER_FEM
RU_PLURAL_NUMERAL_5_GENITIVE_PLURAL
RU_PLURAL_NUMERAL_21_NOMINATIVE_SING
RU_IRREGULAR_PLURAL_HUMAN
RU_PRONOUN_NOUN_COREFERENCE_KOT
EN_IRREGULAR_VERB_GO_PAST
EN_PRESENT_PERFECT_EXPERIENCE
EN_ARTICLE_AN_BEFORE_VOWEL_SOUND
EN_UNCOUNTABLE_NOUN_ADVICE
EN_SUBJECT_VERB_AGREEMENT_3SG
EN_EMBEDDED_QUESTION_WORD_ORDER
EN_MODAL_BASE_VERB_NO_S
EN_IRREGULAR_PLURAL_CHILD
EN_PREPOSITION_INTERESTED_IN
RU_EN_FALSE_FRIEND_MAGAZIN
```

---

## 6. Negative Strategies

For each rule family, the rejected row must be constructed by applying exactly
one of these strategies. All other sentence elements remain correct.

| Rule family | Negative strategy |
|---|---|
| case (G01) | Replace the correct case with the nearest plausible wrong case |
| gender agreement (G03) | Keep noun; change adjective/pronoun/verb gender only |
| number agreement (G03) | Change number on one dependent word only |
| subject-verb agreement (G03) | Remove -s or add -s where opposite is required |
| tense (G04) | Replace tense with a close but semantically wrong tense |
| aspect (G04) | Swap perfective ↔ imperfective; keep sentence otherwise fluent |
| article (G05) | Swap a/an/the/zero according to the targeted article rule |
| plural (G06) | Regularize an irregular plural or pick wrong CLDR numeral form |
| irregular verb (G07) | Regularize: `goed`, `writed`, `человеков` |
| word order (G02) | Apply direct-question inversion where embedded order is required |
| preposition (G01/G08) | Use a common learner error: `interested about`, `depend from` |
| false friend (G08) | Choose the tempting cognate: `магазин -> magazine` |
| idiom (G08) | Use literal calque instead of idiomatic translation |
| register (G09) | Preserve meaning but shift tone to wrong product register |

---

## 7. Rejected Row Construction Rules

```text
1. One defect only — rejected must violate exactly one rule_id.
2. Plausible defect — the wrong form must be the kind of mistake a learner makes,
   not an obvious nonsense substitution.
3. Fluent except for defect — rejected must be otherwise grammatically sound.
4. [WRONG: ...] annotation is mandatory — format:
     [WRONG: rejected_type - weakness - gap - consequence]
     rejected_type must match the rule family.
5. Evidence terms — list tokens and UD features that identify the defect.
```

Prohibited rejected patterns:

```text
FORBIDDEN: "Я видеть брат."   (two errors at once: verb form + case)
FORBIDDEN: "He can speaks."   (acceptable if teaching ONLY the -s error)
FORBIDDEN: [WRONG: bad grammar]  (too generic; must name the rule)
```

---

## 8. Generating Rejected Rows from Sources

### From Universal Dependencies treebanks

```text
CoNLL-U token line → extract form, lemma, upos, feats, deprel
chosen: correct form from corpus
rejected: same sentence with one feature changed
  Case=Acc → Case=Nom
  Number=Plur → Number=Sing
  Tense=Past → Tense=Pres
```

### From UniMorph

```text
lemma + feature_bundle → correct_form
lemma + adjacent_wrong_bundle → wrong_form (rejected)
Example:
  писать + V;IND;PRS;1;SG → пишу    (chosen)
  писать + V;IND;PRS;3;SG → пишет   (wrong for 1st person)
```

### From Unicode CLDR plural rules

```text
chosen: correct CLDR plural category for numeral
rejected: adjacent CLDR category
Example RU:
  one (1, 21, 31...) → nominative singular: файл
  few (2-4, 22-24)   → genitive singular: файла
  many (5+, 11-19)   → genitive plural: файлов
rejected for 5: "5 файла" (few category applied to many context)
```

### From LanguageTool grammar rules

```text
LanguageTool rule XML:
  <example correction="an apple">a apple</example>
Maps to:
  chosen = correction
  rejected = incorrect example + [WRONG: article_error - ...]
This is the most direct source for G05 and G07 rows.
```

---

## 9. Quality Gates

Every grammar-rule DPO pair must pass all applicable gates before
`quality_status: "train_ready"`:

```text
1.  chosen != rejected
2.  rejected contains [WRONG: ...] with named rejected_type
3.  rejected violates exactly one rule_id
4.  rejected remains plausible except for the target defect
5.  source_id is present and source_license_state is ACCEPT or ACCEPT_SUPPORT_ONLY
6.  For G01/G03/G04/G05/G06/G07: feature_changed, correct_feature_value,
    rejected_feature_value are present
7.  evidence_terms lists the key tokens and UD feature strings
8.  chosen and rejected do not differ only by punctuation or whitespace
9.  a_side describes what the model should do, not the answer itself
10. eval_only sources never become train_ready rows
```

---

## 10. Unit Expansion

Each grammar-rule DPO pair must expand into three unit-level training examples:

```text
semantic-tokenizer example:
  chosen signature includes [RULE: case=accusative_animate] or equivalent
  rejected signature drops or misnames that rule tag

executor-ru_en example:
  chosen WordMappings include [AGREEMENT:] and [PHRASE:] for the corrected form
  rejected changes exactly one rule boundary

quality-verifier example:
  chosen: fail the defective candidate
  rejected: falsely accept the defective candidate
```

---

## 11. Minimum Grammar-Rule DPO Coverage per Language Pair

Before a translation unit batch is promoted to the next level, the grammar-rule
DPO store must include at least:

```text
RU:
  6 case rows (one per case, covering Acc animate as priority)
  4 gender agreement rows (M/F/N + past tense)
  3 aspect rows (perfective result, imperfective process, aspectual ambiguity)
  3 numeral/plural rows (CLDR one/few/many)
  2 irregular plural rows (люди, дети)
  2 coreference rows (кот→он, кошка→она)
  2 verb government rows (гордиться + Instr, нуждаться + в + Prep)
  2 irregular verb rows (идти/пойти, дать)

EN:
  3 tense rows (Past Simple vs Present Perfect, Past Simple vs Past Continuous)
  4 irregular verb rows (go/went, write/wrote, bring/brought, take/took)
  3 article rows (a/an rule, the vs zero, uncountable)
  2 subject-verb agreement rows (3sg -s, there is/are)
  2 word order rows (embedded question, question inversion)
  2 preposition collocation rows (interested in, depend on)
  2 modal verb rows (no -s, no to after modal)

RU→EN cross:
  3 false friend rows (магазин, аккуратный, симпатичный)
  2 aspect-tense mapping rows (perfective → Past Simple; imperfective → Past Continuous)
```

---

## 12. Related Documents

| Document | Content |
|---|---|
| `docs/plans/TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md §3B` | Rule families, per-rule DPO examples, quality gates |
| `docs/plans/LANGUAGE-RULE-SOURCE-REGISTRY-v1.md` | Approved sources, Tier 1–4, search algorithm |
| `docs/plans/NEW-DOMAIN-DEVELOPMENT-GUIDE-v1.md §3J` | Linguistic rules as mandatory DPO protocol |
| `docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` | General corpus sourcing whitelist |
