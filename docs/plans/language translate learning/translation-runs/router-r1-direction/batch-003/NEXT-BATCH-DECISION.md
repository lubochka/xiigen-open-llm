# Next Batch Decision
# Stream: router-r1-direction
# After: batch-003
# Date: 2026-05-11

## Decision

stream_status: R1_direction_stream_complete — advance to R2 topic domain

next_stream: router-r2-topic/batch-001

## Reason

Batch-003 confirmed the R1 direction gate across the hardest edge cases at the
R1 difficulty boundary: code-switched Cyrillic+Latin sentences where Cyrillic
barely wins, exact-tie resolution (9 Cyrillic vs 9 Latin, Cyrillic wins by
>= rule), Cyrillic-lookalike glyphs (Ок = Cyrillic О+к not Latin O+k),
brand names and tech abbreviations (McDonalds, Wi-Fi, Skype, Windows, Android)
inside Cyrillic/Hebrew sentences, and cultural loanwords / transliterations
in Latin-source sentences (blini, vodka, troika, hummus, kibbutz, challah,
Kaddish, Knesset).

All 50 held-out rows passed (50/50 = 1.00). Cumulative: 150 train pairs across
3 batches, 150 held-out rows, all passing.

## R2 Topic Domain Stream Shape

- New stream: router-r2-topic
- Router level: R2
- Input: source text after R1 direction is known
- Goal: detect topic_domain (general, medical, legal, technical, personal, cultural)
- DPO pair structure: chosen has correct topic_domain; rejected has plausible
  but wrong topic_domain (e.g., general instead of medical)
- All 4 directions covered in each batch

## Do Not Proceed To

- Translator adapter training until R1 + R2 gates both pass.
- EN_HE or HE_EN translator batches until tokenizer Hebrew coverage test passes.
- Neural DPO training on router pairs until R2 is also gated.
