# Translation Source Registry
# Superseded by: TRANSLATION-SOURCE-REGISTRY-v3.md

## Superseded Warning

Do not use this registry for current train-ready decisions. It is retained as
historical evidence only. Current source authority is
`TRANSLATION-SOURCE-REGISTRY-v3.md`, where OpenSubtitles is not train-ready.
# Version: 2.0 — 2026-05-11
# Supersedes: TRANSLATION-SOURCE-REGISTRY-v1.md
# Domain: Language Translation — Russian/Hebrew ↔ English

---

## What changed from v1

- HuggingFace dataset IDs added for every source.
- HeDC4, Knesset, ParaNames added (all commercially clean).
- CCAligned corrected to Apache 2.0 (ACCEPT with quality filter, not REVIEW REQUIRED).
- CCMatrix (CC-BY, Meta) added as a large parallel RU–EN source.
- opus-books added as literary-register RU–EN source (mixed PD/CC-BY — per-batch check).
- Europarl removed — RU is not an EU language; Europarl does not cover RU–EN.
- OpenSubtitles raw (studio copyright) separated from OPUS-processed version (CC-BY).
- TED Talks added as BLOCK (CC BY-NC 4.0).
- Bible corpus clarified — text may be PD but specific translations are often copyright.
- First-run source list updated to 4 clean ACCEPT sources only.
- Phase 6 sources specified (CCMatrix, opus-books).

---

## Licence Decision States

| State | Meaning |
|---|---|
| ACCEPT | May enter train-ready batches directly after quality gates pass. |
| QUARANTINE | Commercially permitted but ShareAlike or mixed conditions apply. Keep in isolated batch files; tag `batch_isolation: true` on every pair. |
| REVIEW REQUIRED | Licence unclear or varies by sub-document. Record per-document decision before ingesting. |
| METHOD-ONLY | Use extraction method or pair structure as inspiration. Do not ingest raw text. |
| BLOCK | Do not use in production training. May be cited only as excluded source. |

---

## HE↔EN Sources

### ACCEPT

#### opus-tatoeba (HE–EN)

| Field | Value |
|---|---|
| Licence | CC-BY 2.0 |
| Decision | ACCEPT |
| Volume | ~20 K sentence pairs |
| HuggingFace ID | `Helsinki-NLP/opus-tatoeba-he-en` |
| Source id | `opus-tatoeba-he-en` |
| Register | Everyday, short sentences |
| Quality | High — community-curated, clean |
| Caveats | Use the OPUS-filtered release, not the raw Tatoeba site download. Raw site allows contributors to apply non-standard licences to individual sentences. The OPUS release filters to CC-BY 2.0 only. |
| Pair families | T01, T02, T04, T05, T09 |

#### opus-opensubtitles-2018 (HE–EN)

| Field | Value |
|---|---|
| Licence | CC-BY 4.0 (OPUS-processed version) |
| Decision | ACCEPT with OpusFilter required |
| Volume | ~3.7 M sentence pairs |
| HuggingFace ID | `Helsinki-NLP/opus-100` (he-en subset) |
| Source id | `opus-opensubtitles-he-en` |
| Register | Colloquial, spoken |
| Quality | Noisy — must filter before pair construction |
| Caveats | The raw studio subtitles are copyright. Use ONLY the OPUS-processed version, which is released under CC-BY 4.0. Apply OpusFilter (bicleaner-ai score ≥ 0.5 recommended) before extracting pairs. Do not ingest unfiltered lines. |
| Pair families | T01, T02, T04 |

#### HeDC4 — Hebrew Deduplicated Common Crawl

| Field | Value |
|---|---|
| Licence | Apache 2.0 |
| Decision | ACCEPT |
| Volume | Monolingual HE (large) |
| HuggingFace ID | `NNLP-IL/HeDC4` |
| Source id | `heddc4` |
| Register | Web prose, mixed |
| Quality | Cleaned and deduplicated |
| Caveats | Monolingual only — not a parallel corpus. Use for: back-translation base (T07), rejected-side naturalness check, tokenizer training. Do not use as chosen-side authority for translation pairs. |
| Pair families | T07 (back-translation base) |

#### Knesset Corpus 2004–2005

| Field | Value |
|---|---|
| Licence | Public Domain |
| Decision | ACCEPT |
| Volume | Monolingual HE (transcripts of Israeli parliament sessions) |
| HuggingFace ID | `NNLP-IL/Hebrew-Resources` |
| Source id | `knesset-he` |
| Register | **Formal spoken** (parliamentary speech — not formal written prose) |
| Quality | Tokenised and morphologically annotated |
| Caveats | Monolingual Hebrew only. Use for: **formal spoken** register pairs (T02, labelled `formal spoken`), morphological pair construction (T03) where annotated forms are needed, tokenizer training. Transcripts are Israeli Hebrew — Modern Hebrew, not Biblical. Note: Knesset language is spoken formal Hebrew; it differs from written formal Hebrew (press releases, legal documents). Label pairs with `(register: formal spoken)` not `(register: formal)` to avoid mixing registers. Reserve plain `formal` for written-prose sources (Ben-Yehuda, legal texts). |
| Pair families | T02 (formal spoken register — see register label note), T03 (morphological annotated source) |

#### ParaNames — Multilingual Named Entity Pairs

| Field | Value |
|---|---|
| Licence | MIT |
| Decision | ACCEPT |
| Volume | 16.8 M named entity translations across 400+ languages (HE and EN included) |
| HuggingFace ID | Search HuggingFace for `ParaNames` |
| Source id | `paranames-he-en` |
| Register | Named entities only |
| Quality | High — purpose-built for entity transliteration |
| Caveats | Named entity pairs only. Do not use as sentence translation source. Filter for `language_code: he` and `language_code: en`. |
| Pair families | T09 (Named Entity Preservation) — primary source |

#### opus-gnome / opus-kde (HE–EN)

| Field | Value |
|---|---|
| Licence | GPL for GNOME code; LGPL for KDE; UI strings under MIT/CC where separately licensed |
| Decision | ACCEPT for UI string pairs; REVIEW REQUIRED for document strings |
| Volume | Thousands of pairs |
| Source id | `opus-gnome-he-en`, `opus-kde-he-en` |
| Register | Formal, technical, UI |
| Caveats | UI localisation strings are short and highly formal. Good for formal-register pairs and technical vocabulary. Strip GPL-licensed code comments before ingesting UI strings. |
| Pair families | T02 (formal register), T09 (software entity names) |

---

## RU↔EN Sources

### ACCEPT

#### opus-tatoeba (RU–EN)

| Field | Value |
|---|---|
| Licence | CC-BY 2.0 |
| Decision | ACCEPT |
| Volume | ~700 K sentence pairs |
| HuggingFace ID | `Helsinki-NLP/opus-tatoeba-ru-en` |
| Source id | `opus-tatoeba-ru-en` |
| Register | Mixed, everyday |
| Quality | High — community-curated |
| Caveats | Same OPUS-filtered release caveat as HE version. |
| Pair families | T01, T02, T03, T04, T05, T09 |

#### opus-opensubtitles-2018 (RU–EN)

| Field | Value |
|---|---|
| Licence | CC-BY 4.0 (OPUS-processed) |
| Decision | ACCEPT with OpusFilter required |
| Volume | ~15 M sentence pairs |
| HuggingFace ID | `Helsinki-NLP/opus-100` (ru-en subset) |
| Source id | `opus-opensubtitles-ru-en` |
| Register | Colloquial, spoken |
| Quality | Noisy — filter required |
| Caveats | Same raw-vs-OPUS caveat as HE version. Apply bicleaner-ai score ≥ 0.5. 15M pairs are far too many for DPO — sample at most 500K after filtering. |
| Pair families | T01, T02, T04 |

#### mc4-ru — Multilingual C4 Russian

| Field | Value |
|---|---|
| Licence | Apache 2.0 |
| Decision | ACCEPT |
| Volume | Monolingual RU (large) |
| HuggingFace ID | `allenai/c4` (ru config) |
| Source id | `mc4-ru` |
| Register | Web prose, mixed |
| Caveats | Monolingual only. Use for back-translation base, rejected-side naturalness check, tokenizer training. Not a parallel corpus. |
| Pair families | T07 (back-translation base) |

#### CCAligned (RU–EN)

| Field | Value |
|---|---|
| Licence | Apache 2.0 |
| Decision | ACCEPT with quality filter |
| Volume | Large — RU–EN subset available |
| Source id | `ccaligned-ru-en` |
| HuggingFace ID | Available via OPUS |
| Register | Web, mixed |
| Quality | Medium — automatically aligned; apply bicleaner-ai ≥ 0.6 (stricter than OpenSubtitles because MT-origin risk is higher) |
| Caveats | Automatically web-aligned pairs have a higher FM-TR-05 (MT-origin) risk than curated corpora. Verify that chosen sides are human-written, not MT post-edited, before marking train-ready. |
| Pair families | T01, T03 |

#### CCMatrix (RU–EN)

| Field | Value |
|---|---|
| Licence | CC-BY (Meta/OPUS release) |
| Decision | ACCEPT with quality filter |
| Volume | ~700 M pairs total; RU–EN subset is large |
| Source id | `ccmatrix-ru-en` |
| HuggingFace ID | Available via OPUS |
| Register | Web, mixed |
| Quality | Automatically aligned by LASER embeddings — apply bicleaner-ai ≥ 0.6 |
| Caveats | Automatic alignment only. High FM-TR-05 risk. Treat all CCMatrix pairs as `quality_status: review_required` until human spot-check confirms at least 90% accuracy on a 100-pair sample. Do not mark train-ready from CCMatrix without that spot-check. |
| Pair families | T01 (large-volume augmentation after Phase 5 only) |

#### opus-books (RU–EN)

| Field | Value |
|---|---|
| Licence | Mixed — Public Domain (pre-1925) and CC-BY; some titles CC-BY-SA |
| Decision | ACCEPT for PD and CC-BY titles; QUARANTINE for CC-BY-SA titles |
| Volume | ~400 K pairs |
| Source id | `opus-books-ru-en` |
| HuggingFace ID | Via OPUS |
| Register | Literary — formal prose, narrative |
| Quality | High quality alignment |
| Caveats | Check per-title licence. PD and CC-BY titles → ACCEPT batch. CC-BY-SA titles → QUARANTINE batch. Do not mix. Literary register is important for T03 (Russian morphological variety is highest in literary prose). |
| Pair families | T01, T02 (formal literary register), T03 (morphological complexity) |

#### ParaNames (RU–EN)

| Field | Value |
|---|---|
| Licence | MIT |
| Decision | ACCEPT |
| Volume | 16.8 M entities; RU and EN included |
| Source id | `paranames-ru-en` |
| Caveats | Same as HE–EN entry. Named entities only. Filter for `ru` and `en`. |
| Pair families | T09 |

---

## Sources Removed or Corrected from v1

### Europarl — REMOVED

Europarl covers EU official languages. Russian is not an EU language. Europarl
does not have a RU–EN component. v1 listed it incorrectly.

### CulturaX — BLOCK (unchanged from v1)

CC-BY-NC-4.0 prohibits commercial production use.

### CC-100 — REVIEW REQUIRED (unchanged)

Per-language release terms vary. Confirm each language subset's licence card
before ingesting.

---

## Quarantine Sources

| Source | Issue | Action |
|---|---|---|
| FLORES-200 / FLORES+ | CC-BY-SA 4.0 — ShareAlike; commercially permitted but derivatives must be same licence | Evaluation held-out only. Do not write DPO pairs from FLORES sentences. Tag `eval_only: true`. |
| WikiMatrix | CC-BY-SA 3.0 (Wikipedia) | Isolated batches, `batch_isolation: true`. |
| opus-books (CC-BY-SA titles) | ShareAlike condition on specific titles | Per-title isolation. Check OPUS book metadata for licence tag. |
| NTREX-128 | CC-BY-SA 4.0 | Evaluation only. |
| Tatoeba (raw site, not OPUS) | Individual contributors may apply non-standard licences | Use ONLY the OPUS-filtered release. Never download directly from tatoeba.org for training. |

---

## Block Sources

| Source | Reason |
|---|---|
| OpenSubtitles (raw download) | Subtitles are copyright of film studios. The raw files are not freely licensed. Use ONLY the OPUS-processed version. |
| TED Talks | CC BY-NC 4.0 — non-commercial restriction. BLOCK for any production training. |
| Bible translations (specific translations) | The biblical text itself may be public domain (ancient), but specific modern translations (NIV, ESV, NLT; modern Hebrew translations) are copyright. Do not use any named translation without confirming the specific version's licence. |
| CulturaX | CC-BY-NC-4.0 — non-commercial restriction. |
| Russian National Corpus (RNC) | Research-only; no redistribution. |
| OpenAssistant (OASST1/OASST2) | Apache-2.0 but these are instruction-following corpora, not parallel translation. Keep in orchestration domain only (IT-ORCH family). Do not cross-use in translation training. |

---

## First-Run Source List (Phase 1–2)

Use ONLY these four sources for the initial 64-pair standalone ablation:

```
RU–EN:
  opus-tatoeba-ru-en        CC-BY 2.0    ~700K pairs   (clean, curated)
  opus-opensubtitles-ru-en  CC-BY 4.0    filtered only (OpusFilter bicleaner-ai ≥ 0.5)

HE–EN:
  opus-tatoeba-he-en        CC-BY 2.0    ~20K pairs    (clean, curated)
  opus-opensubtitles-he-en  CC-BY 4.0    filtered only (OpusFilter bicleaner-ai ≥ 0.5)
```

All other sources deferred until Phase 1–2 Gate B passes.

For T09 (Named Entity) pairs in Phase 3, add:
```
  paranames-ru-en           MIT          named entities only
  paranames-he-en           MIT          named entities only
```

---

## Phase 6 (QUARANTINE Integration) Sources

After Phase 5 combo passes, introduce:

```
  ccmatrix-ru-en            CC-BY        large volume; spot-check required
  opus-books-ru-en (PD/CC-BY titles only)  literary register
```

Both must run in separate batch files with `batch_isolation: true`.
Spot-check 100 pairs per source before marking any pairs train-ready.

---

## Registry Maintenance Rules

1. Every new source must be added here with a decision before any pair references it.
2. `source_license_state` in every pair must match the decision in this registry.
3. `source_registry_version` in every pair must reference `v2` after this file is active.
4. QUARANTINE sources must have `batch_isolation: true` on every pair row.
5. REVIEW REQUIRED sources must not appear in `train_ready` rows.
6. When the OPUS-filtered version of a source is used, set `source_id` to the OPUS
   id (e.g. `opus-opensubtitles-ru-en`), not the raw upstream id.
7. OpusFilter score must be recorded in pair metadata for any OpenSubtitles or
   CCAligned/CCMatrix pair: `"filter_score": 0.73`.
