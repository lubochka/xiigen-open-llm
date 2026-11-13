# P24 evidence card

P24 создал redacted SkillSelection runtime holdout rows до любого comparison или promotion.

Принято строк: 4.

Отклонено кандидатов: 3.

Почему строки можно использовать как holdout:
- sourceFamily отсутствует в обоих сравниваемых checkpoints;
- pairId отсутствует в обоих checkpoints;
- строки не использовались для training и common training для них запрещен;
- prompts представлены только как redacted summaries и hashes;
- leakage controls блокируют exact prompt, source id, path, filename, candidate order и pair id.

No-overclaim statement:
Это только eval/holdout rows ledger. Он не доказывает semantic quality, SkillSelection readiness, full API readiness, runtime promotion или common model replacement.

Next:
P25 должен запустить side-by-side comparison только на этих accepted holdout rows и сохранить promotion blocked до отдельного проходящего runtime gate.
