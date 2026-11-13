# Translation Source Registry
# Superseded by: TRANSLATION-SOURCE-REGISTRY-v3.md

## Superseded Warning

Do not use this registry for current train-ready decisions. It is retained as
historical evidence only. Current source authority is
`TRANSLATION-SOURCE-REGISTRY-v3.md`, where OpenSubtitles is not train-ready.
# Version: 1.0 — 2026-05-11
# Domain: Language Translation — Russian/Hebrew ↔ English
# Authority: TRANSLATION-DPO-REGISTRY-GUIDE-v1.md

---

## Purpose

This registry records every dataset source considered for the translation
training domain and assigns a licence decision, a use role, and a pair
family code. It is the single source of truth for what may enter a
train-ready batch.

A source that is not listed here is BLOCK by default. Add it to this
registry first; write pairs second.

---

## Licence Decision States

| State | Meaning |
|---|---|
| ACCEPT | May enter train-ready batches directly after quality gates pass. |
| QUARANTINE | Licence allows use under specific conditions (e.g. ShareAlike). Do not mix with ACCEPT rows without explicit ShareAlike handling. |
| REVIEW REQUIRED | Licence is unclear or varies by language subset. Record exact release card and per-subset decision before ingesting. |
| METHOD-ONLY | Use the dataset's extraction method or pair structure as inspiration. Do not ingest raw text. |
| BLOCK | Do not use in production training. May be cited only as excluded source. |

---

## Primary Parallel Corpora

### OPUS Collections

OPUS is an umbrella. Each sub-corpus has its own licence.
Decision is per sub-corpus, not per the OPUS portal.

| Sub-corpus | Languages relevant | Licence | Decision | Role |
|---|---|---|---|---|
| OpenSubtitles 2018 | RU–EN, HE–EN | CC-BY 4.0 | ACCEPT | Spoken/colloquial register pairs |
| Tatoeba | RU–EN, HE–EN | CC-BY 2.0 | ACCEPT | Short sentence pairs; good for morphology coverage |
| WikiMatrix | RU–EN, HE–EN | CC-BY-SA 4.0 | QUARANTINE | Encyclopaedic register; ShareAlike — keep in separate batches |
| CCAligned | RU–EN | Licence not stated / CC0 claimed on some releases | REVIEW REQUIRED | Do not ingest until exact release card is confirmed. |
| FLORES-200 | RU, HE, EN | CC-BY-SA 4.0 | QUARANTINE | High-quality evaluation set; prefer as held-out, not train |
| ParaCrawl v9 | RU–EN | CC0 | ACCEPT | Web-crawled; filter for quality; noisy but large |
| Bible Corpus (Christos Christodoulopoulos) | HE–EN, RU–EN | CC-BY-SA 4.0 | QUARANTINE | Classical register; ShareAlike |
| JW300 | HE–EN, RU–EN | CC-BY-SA 4.0 | QUARANTINE | Religious text; ShareAlike |
| QED | RU–EN | CC-BY-SA 4.0 | QUARANTINE | Educational subtitle pairs; ShareAlike |
| GNOME / KDE / Ubuntu | RU–EN | LGPL / GPL / CC-BY | REVIEW REQUIRED | Per-file licence varies; strip GPL before ingestion |

### Accepted OPUS batches for first run

Use ONLY these for the first standalone ablation:

- OpenSubtitles 2018 (CC-BY 4.0) — source id: `opus-opensubtitles-2018`
- Tatoeba (CC-BY 2.0) — source id: `opus-tatoeba`
- ParaCrawl v9 (CC0) — source id: `opus-paracrawl-v9`

---

## Monolingual Corpora (for back-translation and language modelling)

### mC4 — Multilingual C4

| Field | Value |
|---|---|
| Languages | 101 (RU and EN confirmed; HE present) |
| Licence | Apache-2.0 |
| Decision | ACCEPT |
| Role | Monolingual fluency reference; back-translation base; rejected-side naturalness check |
| Source id | `mc4-ru`, `mc4-he`, `mc4-en` |
| Caveats | Filter for quality using perplexity or C4 cleaning pipeline before use. |

### CC-100

| Field | Value |
|---|---|
| Languages | 100 (RU, HE, EN present) |
| Licence | Mixed — follows CC licence of Common Crawl; CC0 for crawl metadata |
| Decision | REVIEW REQUIRED |
| Role | Monolingual fallback for low-resource Hebrew |
| Source id | `cc100-ru`, `cc100-he`, `cc100-en` |
| Caveats | Confirm per-language release terms on the CC-100 dataset card before ingesting HE subset. RU subset is large and relatively clean. |

### CulturaX

| Field | Value |
|---|---|
| Languages | 167 (RU, HE, EN present) |
| Licence | CC-BY-NC-4.0 |
| Decision | BLOCK for production; METHOD-ONLY |
| Role | Use cleaning methodology only; do not ingest rows into production batches |
| Source id | — |
| Caveats | NC licence prohibits commercial production use. If research-only deployment is explicitly accepted, move to QUARANTINE and keep in separate batches. |

---

## Hebrew-Specific Resources

### Oscar (Hebrew subset)

| Field | Value |
|---|---|
| Licence | Apache-2.0 |
| Decision | ACCEPT |
| Role | Monolingual Hebrew base for back-translation and language model reference |
| Source id | `oscar-he` |
| Caveats | Filter for length and perplexity. Hebrew web text contains mixed-script tokens; strip non-Hebrew tokens before pair construction. |

### HeWiki — Hebrew Wikipedia

| Field | Value |
|---|---|
| Licence | CC-BY-SA 4.0 |
| Decision | QUARANTINE |
| Role | Encyclopaedic Hebrew monolingual reference |
| Source id | `hewiki` |
| Caveats | ShareAlike. Keep in dedicated batches. Do not mix with Apache/CC-BY rows. |

### MNLP Hebrew NLP Resources

| Field | Value |
|---|---|
| Licence | Varies by sub-dataset; most are Apache-2.0 or MIT |
| Decision | REVIEW REQUIRED per sub-dataset |
| Role | Morphological analysis data; POS (Part-Of-Speech) tagging; NER (Named Entity Recognition) for Hebrew |
| Source id | `mnlp-he-*` |
| Caveats | Confirm each sub-dataset licence individually. Do not batch-ingest. |

### Ben-Yehuda Project (Hebrew classical texts)

| Field | Value |
|---|---|
| Licence | Public domain (pre-1925 works) |
| Decision | ACCEPT for classical register pairs only |
| Role | Classical Hebrew and Modern Hebrew examples; useful for register contrast pairs |
| Source id | `benyehuda` |
| Caveats | Classical register only. Use as rejected-register contrast in formal/informal pairs, not as primary translation target. |

---

## Russian-Specific Resources

### OPUS Russian–English (Tatoeba, OpenSubtitles, ParaCrawl)

Already listed above under OPUS. Use those source ids.

### Russian National Corpus (RNC)

| Field | Value |
|---|---|
| Licence | Research use only; no commercial redistribution |
| Decision | BLOCK for production |
| Role | Can be used as register/style reference only; do not ingest rows |
| Source id | — |

---

## Evaluation / Held-Out Sets (do not train on these)

| Dataset | Languages | Decision | Role |
|---|---|---|---|
| FLORES-200 | RU, HE, EN | QUARANTINE — held-out only | Evaluation; measure BLEU/CHRF before and after teaching |
| WMT test sets (2020–2024, RU–EN) | RU–EN | ACCEPT for eval | Annual WMT (Workshop on Machine Translation) reference |
| NTREX-128 | RU, HE, EN | CC-BY-SA 4.0 | QUARANTINE — held-out only |

---

## First-Run Source List

For the initial 64-pair standalone ablation, use only:

```text
opus-opensubtitles-2018   (CC-BY 4.0)
opus-tatoeba              (CC-BY 2.0)
oscar-he                  (Apache-2.0)
mc4-ru                    (Apache-2.0)
```

All other sources are deferred until standalone ablation passes.

---

## Registry Maintenance Rules

1. Every new source must be added here with a decision before any pair references it.
2. `source_license_state` field in every pair must match the decision in this registry.
3. QUARANTINE sources must be tagged `batch_isolation: true` in every pair row.
4. REVIEW REQUIRED sources must not appear in `train_ready` rows.
5. After each new source is added, update the `source_registry_version` field in all
   affected pair rows.
