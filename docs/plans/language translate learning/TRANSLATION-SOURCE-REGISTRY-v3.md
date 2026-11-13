# Translation Source Registry
# Version: 3.0 - 2026-05-11
# Domain: Parallel Translation Models - Russian/Hebrew <-> English
# Supersedes: TRANSLATION-SOURCE-REGISTRY-v2.md
# Parallel format: EXTERNAL-SOURCE-REGISTRY-v7.md

---

## Purpose

This registry decides which external translation sources may enter train-ready
learning sets for four direction-specific translator models:

- `RU_EN`
- `EN_RU`
- `HE_EN`
- `EN_HE`

The rule is deliberately conservative. A corpus is not train-ready just because
it is downloadable or commonly used in MT research. It must have a verified
commercially usable license, usable attribution metadata, and a clear source
role.

---

## Decision States

| State | Meaning |
|---|---|
| ACCEPT | May enter clean train-ready sets after quality gates pass. |
| ACCEPT_SUPPORT_ONLY | May support tokenizer, language-id, morphology, or entity checks, but is not chosen-side translation authority. |
| QUARANTINE | May be evaluated in isolated batches only. Do not merge into clean production training. |
| REVIEW_REQUIRED | Do not train until per-item or per-corpus license evidence is recorded. |
| METHOD_ONLY | May inspire extraction or evaluation methods. Do not ingest source text. |
| BLOCK | Do not use for training or eval batches. Record only as excluded source. |

---

## Clean First-Run Sources

### TR-S01 - Tatoeba Parallel Sentences

| Field | Value |
|---|---|
| Decision | ACCEPT |
| License | CC-BY 2.0 FR, with attribution required |
| Official source | `https://tatoeba.org/en/downloads` |
| Useful language pairs | RU-EN, HE-EN |
| Source role | Human/community parallel sentence authority |
| Use in training | T01 sentence core, T02 register, T03 morphology, T04 idioms when sentence is suitable |
| Required metadata | sentence ids, contributor/source attribution when available, language pair, direction, extraction date |
| Caveat | Do not strip attribution. Do not use rows with license uncertainty from non-standard mirrors. |

### TR-S02 - ParaNames

| Field | Value |
|---|---|
| Decision | ACCEPT |
| License | Data: CC-BY 4.0 according to the ParaNames publication; repository code/license file: MIT |
| Official source | `https://github.com/bltlab/paranames` |
| Useful language pairs | RU-EN, HE-EN, plus reverse directions |
| Source role | Named entity translation/transliteration authority |
| Use in training | T09 named entity preservation and transliteration |
| Required metadata | entity id/name, source language, target language, source path or release id |
| Caveat | Named entities only. Do not use as sentence translation authority. Keep data attribution separate from code license attribution. |

### TR-S03 - Knesset Hebrew Corpus

| Field | Value |
|---|---|
| Decision | ACCEPT_SUPPORT_ONLY |
| License | Public-sector/open Hebrew resource; verify exact release metadata before redistribution |
| Source role | Monolingual Hebrew support source |
| Use in training | Tokenizer coverage, Hebrew language-id, formal-spoken register examples, morphology analysis |
| Do not use for | Chosen-side translation pairs, because it is monolingual |
| Register label | `formal_spoken`, not plain `formal` |

### TR-S04 - United Nations Parallel Corpus

| Field | Value |
|---|---|
| Decision | ACCEPT for RU-EN / EN-RU formal-domain training |
| License/status | United Nations official records and parliamentary documents are described by UN DGACM as public domain; cite the UN Parallel Corpus paper/reference |
| Official source | `https://www.un.org/dgacm/en/content/uncorpus` |
| Useful language pairs | RU-EN and EN-RU |
| Source role | Formal diplomatic / legal-administrative parallel translation authority |
| Use in training | Domain router `legal_diplomatic`, Level 2 completeness, Level 3 formal register, Level 4 long formal sentence robustness |
| Required metadata | document id, language pair, UN corpus citation, sentence alignment id |
| Caveat | No Hebrew. Do not use for HE_EN or EN_HE. Keep domain label `legal_diplomatic`, not `general`. |

---

## Quarantine Sources

### TR-Q01 - FLORES-200

| Field | Value |
|---|---|
| Decision | QUARANTINE_EVAL_ONLY |
| License | CC-BY-SA 4.0 |
| Source role | Held-out evaluation benchmark only |
| Use | BLEU/chrF evaluation in isolated reports |
| Do not use for | Clean production training rows |

### TR-Q02 - Wikipedia / Wiktionary / WikiMatrix

| Field | Value |
|---|---|
| Decision | QUARANTINE |
| License | CC-BY-SA family |
| Source role | Optional isolated evaluation or lexical stress testing |
| Do not use for | Clean train-ready production batches |

---

## Review-Required Sources

### TR-R01 - OPUS OpenSubtitles

| Field | Value |
|---|---|
| Decision | REVIEW_REQUIRED |
| Why not ACCEPT | Raw subtitles are studio/copyright text; OPUS availability is not enough for clean model-training use without exact license evidence. |
| Possible role | Noisy colloquial augmentation after legal review and bicleaner filtering |
| Required before use | Written source license evidence, OPUS release metadata, attribution plan, bicleaner score threshold |

### TR-R02 - CCMatrix / CCAligned / ParaCrawl

| Field | Value |
|---|---|
| Decision | REVIEW_REQUIRED |
| Why not ACCEPT | Web-mined parallel data can contain mixed source rights and machine-origin artifacts. |
| Possible role | Large-volume augmentation only after clean license evidence and human spot-check |
| Required before use | Per-release license evidence, bicleaner score file, 100-row human spot-check with >= 90% alignment quality |

### TR-R03 - OPUS Books

| Field | Value |
|---|---|
| Decision | REVIEW_REQUIRED_PER_TITLE |
| Why not ACCEPT | Mixed public-domain, CC-BY, and share-alike/copyright title status. |
| Possible role | Literary-register RU-EN after per-title filtering |
| Required before use | Title-level license ledger. PD/CC-BY titles may enter ACCEPT; CC-BY-SA titles stay QUARANTINE. |

### TR-R04 - KDE / GNOME / Ubuntu UI Localization Strings

| Field | Value |
|---|---|
| Decision | REVIEW_REQUIRED |
| Why not ACCEPT | Project and string licenses vary by component. UI strings may be clean in some components but cannot be assumed globally. |
| Possible role | `software_ui` domain router training and short UI translation pairs |
| Required before use | Component-level license evidence, source repository URL, translation file path, language pair, attribution plan |

### TR-R05 - Medical / Legal / Finance Domain Corpora

| Field | Value |
|---|---|
| Decision | REVIEW_REQUIRED |
| Why not ACCEPT | High-stakes and domain-specific corpora often have mixed or unclear rights. |
| Possible role | Domain router stress tests only after a clean source registry entry is added |
| Required before use | Exact corpus license, source provenance, attribution, and manual quality audit |

---

## Blocked Sources

| Source | Decision | Reason |
|---|---|---|
| MIT OpenCourseWare language materials | BLOCK | CC BY-NC-SA; no commercial model-training use. |
| Stack Exchange / Stack Overflow dump | BLOCK | Download terms create practical LLM-training risk. |
| TED / TED Talks translation data | BLOCK | Non-commercial license variants. |
| MUSE dictionaries if using the CC-BY-NC release | BLOCK | Non-commercial restriction. |
| Bible corpora without translation-level license proof | BLOCK | Many modern translations are copyrighted even when underlying text is public domain. |

---

## First-Run Source Policy

The first translation DPO run may use only:

1. `TR-S01` Tatoeba for sentence translation.
2. `TR-S02` ParaNames for named entity preservation.
3. `TR-S04` UN Parallel Corpus for RU_EN / EN_RU `legal_diplomatic` domain only.

`TR-S03` Knesset may be used only for tokenizer and Hebrew support checks, not
as chosen-side translation authority.

Everything else must remain out of train-ready until this registry is updated
with exact license evidence.
