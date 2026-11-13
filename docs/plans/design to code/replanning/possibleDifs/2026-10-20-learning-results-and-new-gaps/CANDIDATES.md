# Кандидаты в исправления по уже сохранённым полезным результатам

**Это документирование результатов, не внедрение.** Все три записи относятся к существующим runs, без повторного обучения в этом аудите. Любое полезное измеренное улучшение является успехом по метрике/домену; сопутствующая регрессия не скрывается. Решение о переносе — за Любой.

## C01 · BPE-NO-MATCH-COPY-01 · успешен по allocations

`01.Tokenizer/BpeTokenizer.cs`, `MergeChunk`: при отсутствии сливаемой пары возвращать неизменённый chunk без новой копии. В native BPE Train allocations снизились code-doc3,904,664→1,126,744bytes (−71.14%), en-ru2,892,824→893,656 (−69.11%). По 64merges, vocab256→320; outputs/tokenizer hashes совпали, roundtrip4/4. Сохранять ownership входного chunk, merge order/tie и сериализацию. Это выигрыш выделений, не доказанное снижение RSS/CPU или рост языковой точности. [Доказательства](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/report.md).

## C02 · NEURAL-FROZEN-TRANSPOSE-01 · успешен по allocations

`00.TensorCore/Layers/Linear.cs`, `Forward`: кэшировать transpose только frozen weight. Native SFT/LoRA4updates/arm:13,647,840→12,140,448allocated bytes (−11.04%), одинаковые веса, optimizer и logits; frozen state неизменен. Плата+249,856bytes retained cache. Exact0/2→0/2; NLL падает одинаково. Перед переносом потребуется определённый lifetime/invalidation при load, base mutation/unfreeze; эти сценарии run не проверял. Wall/CPU выросли как одиночные наблюдения, причинный speed effect не установлен. [Доказательства](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/NEURAL-FROZEN-TRANSPOSE-01/report.md).

## C03 · N-KU-01 · успешен на части метрик, с явной регрессией

`12.SemanticCompiler/Prediction/FileActionKnn.cs`, построение метки для do-loop: сохранённое исправление дало correct1/4→3/4 и false ADD_LOOP3→0 после native обучения. Однако required ADD_LOOP1/1→0/1. Принять как локальный кандидат улучшения label quality, не как универсальную победу. Полный перенос должен учитывать известную недостаточность DoStatement-feature отдельно; не объединять её с NG01 и не скрывать регрессию классом «общая точность выросла». Новые проверки и изменения продукта этим документом не назначены. [Доказательства](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/report.md).

## Не выдаются за измеренные кандидаты

NG01 — новая постановка, ещё без результата; P54 — исправление применимости и способа измерения; KH-COST — исправление названия learner. Исторические E22/E25/E43 — полезные свидетельства своих доменов, но не автоматическое основание переносить другую реализацию в текущий RP01. E26 BuildDefault требует training-facing writer перед новой learning-проверкой.
