# LANGUAGE-RULE-SOURCE-REGISTRY-v1.md
# Version: 1.0 - 2026-05-29
# Purpose: Approved sources for language rule features used in KNN-matrix rule tags
#          and grammar-rule DPO pairs. Covers how to find sources for any language.
# Scope: TranslationDomain (27.TranslationDomain) and any future domain requiring
#        morphological or syntactic precision signals.
# Based on: NEW-DOMAIN-DEVELOPMENT-GUIDE-v1 §3K,
#           GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1,
#           TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1 §3B.

---

## 1. Tier Whitelist

| Tier | Source | Primary use | License note |
|---|---|---|---|
| 1 | Universal Dependencies (UD) treebanks | POS, lemma, morphology features, dependency syntax; CoNLL-U format | Per-treebank; check LICENSE in each UD repo. Most use CC variants. |
| 1 | UD Universal Features (universaldependencies.org/u/feat) | Canonical feature schema: Case, Gender, Number, Tense, Aspect, Mood, Person, Animacy, Degree, VerbForm, Voice | Free reference; map all other sources to UD names. |
| 1 | UniMorph (unimorph.github.io) | Lemma → word form → morphology bundle; paradigm tables; irregular inflections | Per-language repository; check LICENSE. |
| 1 | Unicode CLDR (unicode-org/cldr) | Plural rules, number categories, date/time locale, unit patterns | Unicode License; commercial use allowed with attribution. |
| 2 | LanguageTool (github.com/languagetool-org) | Real grammar-error rules with correct/incorrect examples → best rejected DPO source | Per-module license; LGPL for core; check grammar.xml module license before distribution. |
| 2 | OpenCorpora (opencorpora.org) | Russian morphological dictionary; grammemes; forms; paradigms | CC-BY-SA. Keep in separate shard; fulfill share-alike. |
| 2 | pymorphy2 / pymorphy3 | Russian morphology analyzer and generator (tool, not primary source) | MIT. Uses OpenCorpora and LanguageTool data. |
| 2 | Wiktionary dumps + Wiktextract | Inflection tables, irregular forms, translations, usage examples | CC-BY-SA / GFDL. Keep in separate shard. |
| 3 | Apertium (github.com/apertium) | Morphological dictionaries, bilingual dictionaries, transfer rules | GPL for language data. Use as reference/research; do not ship GPL data in closed-source product. |
| 3 | GF Resource Grammar Library | Formal morphology and syntax generation; paradigm generation | LGPL. Use for synthetic probe generation only after license review. |
| 4 | Blogs, tutorials, textbooks | Manual reference only | Not a training source; never cite as source_id. |

---

## 2. Universal Feature Schema (UD-Based)

All rule tags in KNN signatures and DPO metadata must use these canonical names:

```text
POS       : NOUN VERB ADJ ADV PRON DET ADP CONJ AUX PART PUNCT
Case      : Nom Gen Dat Acc Ins Loc Voc (UD names)
            Russian-specific: Abl (archaic), PartGen, LocPrp exist as extensions
Gender    : Masc Fem Neut Com
Number    : Sing Plur Dual Ptan (pluralia tantum) Coll
Tense     : Past Pres Fut
Aspect    : Imp Perf
VerbForm  : Fin Inf Part Ger Conv
Mood      : Ind Imp Sub Cnd
Person    : 1 2 3
Animacy   : Anim Inan
Degree    : Pos Cmp Sup
Voice     : Act Pass Mid
```

Example rule tag using this schema:
```text
[RULE: case=Acc]
[RULE: animacy=Anim]
[RULE: aspect=Perf]
[RULE: tense=Pres_Perf]   (compound name for English present perfect)
```

---

## 3. Per-Language Source Configurations

### Russian

```json
{
  "language": "ru",
  "canonical_feature_schema": "Universal Dependencies Features",
  "sources": [
    {
      "source_id": "ud_russian_syntagrus",
      "type": "treebank",
      "provider": "Universal Dependencies",
      "github": "UniversalDependencies/UD_Russian-SynTagRus",
      "features": ["pos", "case", "gender", "number", "animacy", "aspect", "tense", "person", "syntax"],
      "format": "conllu",
      "license": "CHECK_REPO_LICENSE",
      "commercial_allowed": "depends_on_license",
      "attribution_required": true,
      "share_alike": "depends_on_license",
      "allowed_for_training": "after_license_review",
      "tier": 1
    },
    {
      "source_id": "ud_russian_gsd",
      "type": "treebank",
      "provider": "Universal Dependencies",
      "github": "UniversalDependencies/UD_Russian-GSD",
      "features": ["pos", "lemma", "morphology", "syntax"],
      "format": "conllu",
      "license": "CC-BY-SA-4.0",
      "commercial_allowed": true,
      "attribution_required": true,
      "share_alike": true,
      "allowed_for_training": "separate_shard",
      "tier": 1
    },
    {
      "source_id": "unimorph_ru",
      "type": "morphological_paradigms",
      "provider": "UniMorph",
      "github": "unimorph/rus",
      "features": ["lemma", "wordform", "aspect", "tense", "person", "number", "gender", "case", "animacy", "irregular_forms"],
      "format": "tsv",
      "license": "CHECK_REPO_LICENSE",
      "commercial_allowed": "depends_on_license",
      "allowed_for_training": "after_license_review",
      "tier": 1
    },
    {
      "source_id": "cldr_ru",
      "type": "locale_rules",
      "provider": "Unicode CLDR",
      "github": "unicode-org/cldr",
      "features": ["plural_rules", "number_categories", "date_time", "units"],
      "format": "xml_json",
      "license": "Unicode_License",
      "commercial_allowed": true,
      "attribution_required": true,
      "share_alike": false,
      "allowed_for_training": true,
      "tier": 1
    },
    {
      "source_id": "opencorpora",
      "type": "morphological_dictionary",
      "provider": "OpenCorpora",
      "url": "opencorpora.org",
      "features": ["lemma", "forms", "grammemes", "paradigms", "animacy", "gender", "case", "aspect", "tense", "person"],
      "format": "xml",
      "license": "CC-BY-SA",
      "commercial_allowed": true,
      "attribution_required": true,
      "share_alike": true,
      "allowed_for_training": "separate_cc_by_sa_shard",
      "tier": 2,
      "notes": "pymorphy2/pymorphy3 is the recommended tool for using OpenCorpora data programmatically."
    },
    {
      "source_id": "languagetool_ru",
      "type": "grammar_error_rules",
      "provider": "LanguageTool",
      "github": "languagetool-org/languagetool",
      "path": "languagetool-language-modules/ru/src/main/resources/org/languagetool/rules/ru/grammar.xml",
      "features": ["agreement_errors", "case_errors", "aspect_errors", "spelling_rules"],
      "format": "xml",
      "license": "CHECK_MODULE_LICENSE",
      "commercial_allowed": "depends_on_license",
      "allowed_for_training": "after_license_review",
      "tier": 2,
      "notes": "Ideal source for negative DPO examples. Each rule has incorrect/correct examples."
    },
    {
      "source_id": "wiktextract_ru",
      "type": "dictionary_with_inflections",
      "provider": "Wiktionary via Wiktextract",
      "github": "tatuylonen/wiktextract",
      "features": ["lemma", "inflection_tables", "irregular_forms", "irregular_plural", "declension_tables", "conjugation_tables", "translations"],
      "format": "jsonl",
      "license": "CC-BY-SA-4.0_GFDL",
      "commercial_allowed": true,
      "attribution_required": true,
      "share_alike": true,
      "allowed_for_training": "separate_cc_by_sa_shard",
      "tier": 2
    }
  ]
}
```

#### Russian grammeme → UD feature mapping

```text
OpenCorpora grammeme → UD feature name
---
род (gender):  мр → Masc, жр → Fem, ср → Neut
число:         ед → Sing, мн → Plur
падеж:         именит → Nom, родит → Gen, дат → Dat,
               винит → Acc, твор → Ins, предл → Loc
одуш:          одуш → Anim, неод → Inan
вид:           сов → Perf, несов → Imp
время:         прош → Past, наст → Pres, буд → Fut
лицо:          1лицо → 1, 2лицо → 2, 3лицо → 3
форма:         инфин → Inf, прич → Part, деепр → Conv, кратк → Short
```

---

### English

```json
{
  "language": "en",
  "canonical_feature_schema": "Universal Dependencies Features",
  "sources": [
    {
      "source_id": "ud_english_ewt",
      "type": "treebank",
      "provider": "Universal Dependencies",
      "github": "UniversalDependencies/UD_English-EWT",
      "features": ["pos", "lemma", "tense", "verbform", "number", "degree", "prontype", "syntax"],
      "format": "conllu",
      "license": "CC-BY-SA-4.0",
      "commercial_allowed": true,
      "attribution_required": true,
      "share_alike": true,
      "allowed_for_training": "separate_cc_by_sa_shard",
      "tier": 1
    },
    {
      "source_id": "ud_english_gum",
      "type": "treebank",
      "provider": "Universal Dependencies",
      "github": "UniversalDependencies/UD_English-GUM",
      "features": ["pos", "lemma", "morphology", "syntax"],
      "format": "conllu",
      "license": "CHECK_REPO_LICENSE",
      "commercial_allowed": "depends_on_license",
      "tier": 1
    },
    {
      "source_id": "unimorph_en",
      "type": "morphological_paradigms",
      "provider": "UniMorph",
      "github": "unimorph/eng",
      "features": ["irregular_verbs", "verb_paradigms", "irregular_plurals", "comparative_superlative"],
      "format": "tsv",
      "license": "CHECK_REPO_LICENSE",
      "commercial_allowed": "depends_on_license",
      "allowed_for_training": "after_license_review",
      "tier": 1
    },
    {
      "source_id": "cldr_en",
      "type": "locale_rules",
      "provider": "Unicode CLDR",
      "github": "unicode-org/cldr",
      "features": ["plural_rules", "number_categories", "ordinal_rules"],
      "format": "xml_json",
      "license": "Unicode_License",
      "commercial_allowed": true,
      "attribution_required": true,
      "allowed_for_training": true,
      "tier": 1
    },
    {
      "source_id": "languagetool_en",
      "type": "grammar_error_rules",
      "provider": "LanguageTool",
      "github": "languagetool-org/languagetool",
      "path": "languagetool-language-modules/en/src/main/resources/org/languagetool/rules/en/grammar.xml",
      "features": ["article_errors", "agreement_errors", "tense_errors", "confusables", "irregular_verbs", "preposition_errors"],
      "format": "xml",
      "license": "CHECK_MODULE_LICENSE",
      "commercial_allowed": "depends_on_license",
      "allowed_for_training": "after_license_review",
      "tier": 2,
      "notes": "Direct source for G05, G07 rejected rows: rule has incorrect + correction."
    },
    {
      "source_id": "wiktextract_en",
      "type": "dictionary_with_inflections",
      "provider": "Wiktionary via Wiktextract",
      "github": "tatuylonen/wiktextract",
      "features": ["irregular_verbs", "irregular_plurals", "translations", "usage_examples", "false_friends"],
      "format": "jsonl",
      "license": "CC-BY-SA-4.0_GFDL",
      "commercial_allowed": true,
      "attribution_required": true,
      "share_alike": true,
      "allowed_for_training": "separate_cc_by_sa_shard",
      "tier": 2
    }
  ]
}
```

---

## 4. Search Algorithm — How to Find Sources for Any Language

For any new language `<LANG>`, follow these steps in order.

**Step 1: Universal Dependencies treebank**
```text
Search:
  site:github.com/UniversalDependencies UD_<Language>
  "Universal Dependencies" "<language name>"
  universaldependencies.org/treebanks/<lang_code>/index.html
Collect:
  - treebank name(s) and repo URLs
  - LICENSE file content
  - README for annotation notes
  - CoNLL-U files for feature extraction
```

**Step 2: UniMorph language data**
```text
Search:
  site:github.com/unimorph <lang_iso_code>
  unimorph.github.io <language>
  "UniMorph" "<language>"
Collect:
  - TSV files with lemma/form/feature bundles
  - README for feature notation
  - LICENSE
```

**Step 3: Unicode CLDR plural rules**
```text
Search:
  unicode-org/cldr plural rules <language>
  CLDR plural rules <lang_bcp47_code>
  unicode.org/cldr/charts/latest/supplemental/language_plural_rules.html
Collect:
  - plural categories (one/two/few/many/other)
  - ordinal rules
  - minimum pairs for each category
```

**Step 4: Wiktionary inflection tables**
```text
Search:
  Wiktionary "<language>" inflection tables
  Wiktextract "<language>"
  site:github.com/tatuylonen/wiktextract
Collect:
  - inflection paradigm tables
  - irregular forms
  - translations
Note: CC-BY-SA/GFDL shard — keep separate.
```

**Step 5: LanguageTool grammar rules**
```text
Search:
  site:github.com/languagetool-org/languagetool <language_code>/grammar.xml
  "LanguageTool" "<language>" "grammar.xml"
Collect:
  - grammar.xml rule files
  - example correction pairs (chosen from <example correction="...">)
  - category/rule_id labels
Note: Check per-module license before using in distribution.
```

**Step 6: Apertium language data**
```text
Search:
  site:github.com/apertium apertium-<lang_code>
  "Apertium" "<language>" morphology
Collect:
  - monolingual morphological dictionary
  - transfer rules if bilingual pair exists
Note: GPL — use as reference/research only unless product license allows.
```

**Step 7: GF Resource Grammar Library**
```text
Search:
  "Grammatical Framework" "<language>" RGL
  GF Resource Grammar Library <language>
  site:github.com/GrammaticalFramework GF
Collect:
  - language module for morphology/syntax generation
Note: LGPL — review before shipping generated data commercially.
```

---

## 5. License Decision Table

Before assigning `allowed_for_training` or `allowed_for_distribution`, apply:

| License | Shard | Train | Distribute derived data |
|---|---|---|---|
| CC0 / Public Domain | main | yes | yes |
| CC-BY | main or attributed | yes | yes with attribution |
| CC-BY-SA | separate shard | yes | only under same license |
| Unicode License | main | yes | yes with attribution |
| MIT / Apache-2.0 | main | yes | yes |
| LGPL | separate shard | after review | after review |
| GPL | isolated reference | research only | no |
| GFDL | separate shard | after review | only under GFDL |
| Unknown / CHECK | blocked | after review | after review |

Rule: if `license = CHECK_REPO_LICENSE` or `depends_on_license`,
set `allowed_for_training: "after_license_review"` and do not use in production
batches until the review produces a confirmed license state.

---

## 6. Source Record Required Fields

Every entry in the language rule source registry must contain:

```json
{
  "source_id":              "<unique identifier>",
  "language":               "<ISO 639-1 code>",
  "type":                   "<treebank|morphological_paradigms|locale_rules|grammar_error_rules|dictionary_with_inflections|morphological_dictionary>",
  "provider":               "<organization or project name>",
  "github":                 "<owner/repo or null>",
  "features":               ["<list of features provided>"],
  "format":                 "<conllu|tsv|xml|xml_json|jsonl|csv>",
  "license":                "<SPDX id or CHECK_REPO_LICENSE>",
  "commercial_allowed":     "<true|false|depends_on_license>",
  "attribution_required":   "<true|false>",
  "share_alike":            "<true|false|depends_on_license>",
  "allowed_for_training":   "<true|false|after_license_review|separate_shard>",
  "allowed_for_distribution":"<true|false|after_license_review|separate_shard>",
  "tier":                   "<1|2|3|4>"
}
```

---

## 7. Source Priority for RU and EN (Quick Reference)

### Russian sources — ordered by priority

```text
1. UD Russian SynTagRus  — grammar features + syntax (check license)
2. UD Russian GSD        — grammar features + syntax (CC-BY-SA)
3. UniMorph Russian      — morphology paradigms (check license)
4. Unicode CLDR          — plural rules (Unicode License, allowed)
5. OpenCorpora           — Russian morphology dictionary (CC-BY-SA shard)
6. pymorphy2/pymorphy3   — generator tool using OpenCorpora (MIT)
7. LanguageTool Russian  — error rules for rejected DPO (check license)
8. Wiktextract Russian   — exceptions, inflections (CC-BY-SA shard)
9. Apertium Russian      — reference only (GPL isolated)
10. GF RGL Russian       — generation probes only (LGPL review)
```

### English sources — ordered by priority

```text
1. UD English EWT         — grammar features + syntax (CC-BY-SA)
2. UD English GUM         — grammar features (check license)
3. UniMorph English       — irregular verbs, paradigms (check license)
4. Unicode CLDR           — plural rules (Unicode License, allowed)
5. LanguageTool English   — error rules for rejected DPO (check license)
6. Wiktextract English    — irregular forms, false friends (CC-BY-SA shard)
7. WordNet                — semantic class features only; not tense/article source
8. Apertium English       — reference only (GPL isolated)
9. GF RGL English         — generation probes only (LGPL review)
```

---

## 8. Related Documents

| Document | Content |
|---|---|
| `docs/plans/GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1.md` | G01–G10 taxonomy, row schema, quality gates |
| `docs/plans/TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md §3B` | Rule families, per-rule examples, source protocol |
| `docs/plans/NEW-DOMAIN-DEVELOPMENT-GUIDE-v1.md §3J §3K` | Linguistic rules as DPO protocol; source search algorithm |
| `docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` | General translation corpus whitelist |
