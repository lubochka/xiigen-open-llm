# Multilingual Tokenizer Guide — Translation Domain
# Version: 1.0 — 2026-05-11
# Domain: Russian/Hebrew ↔ English Translation

---

## Purpose

The translation domain requires a BPE (Byte Pair Encoding) tokenizer that covers
three scripts without conflating characters from different scripts. This guide
specifies the requirements, coverage targets, and extension procedure for adapting
the existing `01.Tokenizer/BpeTokenizer.cs` to handle Russian, Hebrew, and English.

---

## Script Coverage Requirements

### Script Ranges

| Script | Unicode range(s) | Notes |
|---|---|---|
| Latin (English) | U+0020–U+007F (Basic Latin), U+00C0–U+024F (Latin Extended) | Include accented characters for proper names |
| Cyrillic (Russian) | U+0400–U+04FF | 256 code points; Russian uses approximately 66 of them |
| Hebrew | U+0590–U+05FF (Hebrew block), U+FB1D–U+FB4F (Alphabetic Presentation Forms) | Include both blocks; FB block contains legacy compatibility characters |

### Coverage Targets

Before any translation training begins (Phase 0 pre-condition):

| Language | Minimum token coverage | Test corpus |
|---|---|---|
| English | ≥ 97% | First 10,000 sentences from FLORES-200 EN |
| Russian | ≥ 95% | First 10,000 sentences from FLORES-200 RU |
| Hebrew | ≥ 90% | First 10,000 sentences from FLORES-200 HE |

Coverage = fraction of tokens that are NOT split into individual byte-fallback tokens.
If a token is represented as a sequence of `<byte_XX>` tokens, it is not covered.

---

## Tokenizer Extension Procedure

If the current BPE vocabulary does not meet coverage targets:

### Step 1: Collect multilingual training text

Assemble a tokenizer training corpus (NOT pair training — tokenizer training only):

```
mc4-ru:    500,000 sentences
mc4-en:    500,000 sentences
oscar-he:  200,000 sentences   (Hebrew is smaller; oversample relative to corpus size)
```

Total: ~1.2M sentences for tokenizer training.

### Step 2: Train or extend the BPE vocabulary

Two options:

**Option A — Retrain from scratch (preferred if orchestration tokenizer is Latin-only):**
Train a new BPE tokenizer on the multilingual corpus. Vocabulary size: 64,000–100,000.
Preserve the existing English subword merges by weighting the English component 2×.

**Option B — Extend existing vocabulary:**
Add the top-N Cyrillic and Hebrew tokens (by frequency in the respective monolingual
corpora) to the existing vocabulary. Set N so coverage targets are met.
Risk: merge table ordering may not be optimal. Use only if vocabulary extension is
strictly simpler than retraining.

### Step 3: Verify coverage

```csharp
// In BpeTokenizerTests.cs — add to existing test suite
[Theory]
[InlineData("ru", 0.95f)]
[InlineData("he", 0.90f)]
[InlineData("en", 0.97f)]
public void Tokenizer_MeetsScriptCoverage(string langCode, float minCoverage)
{
    // Load first 1000 sentences of flores200 for langCode
    // Tokenize each sentence
    // Compute fraction of non-byte-fallback tokens
    // Assert >= minCoverage
}
```

Run this test before starting Phase 1 training.

---

## Hebrew-Specific Tokenizer Notes

### Nikud (Vowel Points)

Hebrew vowel diacritics (U+05B0–U+05BD, U+05C1–U+05C2, U+05C7) combine with
base characters. The tokenizer must treat base + nikud as a single combining
character sequence, not split them.

Recommendation: normalise Hebrew text to NFD (Unicode Canonical Decomposition)
before tokenisation. This ensures base letters and combining marks are consistently
separated at the unicode level, then the BPE merge rules can recombine them.

### Final Letter Forms

Hebrew has five final letters (ף ך ם ן ץ) that replace their regular forms
at word end. These are separate Unicode code points (not combining characters).
The tokenizer must treat them as distinct from their non-final counterparts:
- פ (pe) ≠ ף (final pe)
- כ (kaf) ≠ ך (final kaf)
- מ (mem) ≠ ם (final mem)
- נ (nun) ≠ ן (final nun)
- צ (tsadi) ≠ ץ (final tsadi)

Verify that the vocabulary contains both forms for each pair.

### Gershayim and Geresh

- Geresh (׳ U+05F3): marks abbreviations and foreign-origin letters
- Gershayim (״ U+05F4): marks acronyms and compound abbreviations

These must be in the vocabulary as single tokens, not split as punctuation.

---

## Russian-Specific Tokenizer Notes

### Ё (Yo)

"Ё" (U+0401) and "Е" (U+0415) are different letters with different pronunciations.
Russian text often substitutes "Е" for "Ё" informally. Both forms must be in the
vocabulary. Do not merge them into a single token.

Test: "ёлка" and "елка" must tokenise to different token sequences.

### Hard and Soft Signs

"Ъ" (hard sign, U+042A) and "Ь" (soft sign, U+042C) appear in the middle and
end of words and change pronunciation. They must not be split off from their host
word-stem as punctuation.

### BPE Merge Priorities for Russian

Russian morphology produces many suffixes on a shared root. BPE naturally captures
common roots + suffix patterns. For best coverage of case endings, train the BPE
on the FULL word forms including inflections, not just lemmatized text.

---

## Byte Fallback Handling

The existing BPE tokenizer should have a byte fallback for out-of-vocabulary
characters (representing unknown code points as `<byte_XX>` tokens).

For translation training:
- A sentence where > 10% of tokens are byte-fallback tokens is not usable.
  Add it to `discard` during pair construction.
- A sentence where 1%–10% are byte-fallback tokens: flag for review.
- A sentence where < 1% are byte-fallback: acceptable.

Hebrew text from the web often contains:
- Arabic Unicode characters mixed in (U+0600–U+06FF) — strip before use
- Latin numerals inside Hebrew text — these should tokenise as Latin, not byte-fallback
- Emoji — strip before tokenisation

---

## Mixed-Script Handling

Both Russian and Hebrew text can contain English words (brand names, technical
terms, proper names). These are expected and must tokenise correctly.

Example:
- Russian: "iPhone оказался дорогим" — "iPhone" must tokenise as Latin tokens
- Hebrew: "הפגישה ב-Zoom" — "Zoom" must tokenise as Latin tokens

The tokenizer must NOT attempt to force all tokens in a Russian sentence through
the Cyrillic merge table, or all tokens in a Hebrew sentence through the Hebrew
merge table. Script detection must be per-token, not per-sentence.

---

## Integration with 01.Tokenizer

The existing `BpeTokenizer.cs` is an English-first tokenizer. To extend it:

1. Add the three script ranges to the regex pre-tokenizer pattern. The pattern
   currently splits on whitespace and punctuation for English. Extend it to also
   split on Hebrew and Cyrillic word boundaries.

2. Extend the merge table to include Cyrillic and Hebrew merges learned from the
   multilingual training corpus.

3. Add a `ScriptDetect(string token)` helper that returns `LATIN`, `CYRILLIC`,
   `HEBREW`, or `OTHER`. Use this in the coverage test.

4. All new code must follow Rule 1 (OperationResult<T>) and Rule 3 (every line
   has a comment) from CLAUDE.md.

Example test addition:
```csharp
[Fact]
public void Tokenizer_EncodesHebrewWithoutByteFallback()
{
    // Arrange — Hebrew sentence from oscar-he
    var sentence = "הספרייה הלאומית של ישראל";
    // Act
    var tokens = _tokenizer.Encode(sentence);
    // Assert — no byte-fallback tokens (those start with <byte_)
    var byteFallbackCount = tokens.Count(t => t.StartsWith("<byte_"));
    Assert.True(byteFallbackCount == 0,
        $"Expected no byte-fallback tokens, got {byteFallbackCount}");
}
```
