# Parent review P23 - SkillSelection runtime holdout ledger

## Вердикт

P23 принят как honest holdout-ledger gap evidence. Принятые runtime holdout rows не созданы, потому что локальные кандидаты не доказывают одновременно runtime readiness, source-family disjointness и pair-id disjointness против обоих compared checkpoints.

## Что проверено

1. Accepted ledger намеренно пустой: `acceptedHoldoutRowCount=0`.
2. Gap ledger содержит 36 rejected candidates с причинами.
3. Side-by-side comparison не разрешён и не запускался.
4. P16E/P16E2 остаются schema-only evidence; P21 имеет `sourceDisjointProven=false`; P22 имеет zero accepted rows.
5. P16B/P17C нельзя переименовать в holdout, потому что они train-side/adaptive/candidate evidence или не имеют accepted runtime-ready proof.
6. Common/candidate checkpoint files не изменялись.
7. Focused P23 test прошёл; JSON/JSONL parse, UTF-8 scan, no-overclaim scan и model diff прошли.

## Parent repair

Parent review исправил два качества P23 artifacts до acceptance:

- human-facing next repair text был частично английским; generator теперь пишет русскую строку в Markdown/evidence card;
- два analyzer warnings в новом P23 test заменены на `Assert.DoesNotContain`.

После этого focused P23 test был перезапущен и artifacts были заново сгенерированы.

## Не доказано

- semantic quality;
- SkillSelection readiness;
- full API readiness;
- runtime promotion;
- common model update;
- trained-model readiness.

## Следующий шаг

P24 должен создать новые redacted local SkillSelection runtime holdout rows из approved source families, которых нет ни в common checkpoint, ни в P17E candidate checkpoint. Каждая строка должна иметь pair-id/source-family exclusion proof и runtimeInvocationReady proof до любого side-by-side comparison.
