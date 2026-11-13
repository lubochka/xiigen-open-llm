# В НАЧАЛЕ СПИСКА: восстановление успешного language_translate

**19.10.2026 · RESEARCH / PRIORITY INDEX · native .NET reproduction NOT_RUN.** Цель — сначала вернуть реально полезные способности из ветки [language_translate](https://github.com/lubochka/llm_mvp_core/tree/language_translate), затем переносить их в RP01 и только после этого увеличивать объём DPO/LoRA обучения.

## P0 — три исторических capability, проверенных по настоящим архивным receipts

1. **E09 / C# structural KNN:** исторически top1 0.2413 → 0.3507 при k5, 4492 train pairs. Это положительный *архивный receipt*, но baseline и kNN оценены на разных validation denominators; old per-file ordinal SyntaxKind не эквивалентен текущему target codec. Действие: matched file-level holdout, один label codec, correct downstream C# consumer. [Evidence, sources and limitations](44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md).
2. **E26 / контекстное значение RU→EN:** 0.40 → 1.00 на только 15 fixed cases. Исторический E26 использовал BuildDefault/POS-context dictionary и не доказывал обучение нового параметра. Действие: восстановить правильное значение в разных контекстах и отдельно подтвердить saved typed memory на unseen переформулировках. [Evidence](44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md).
3. **E43 / replay и сохранение старого:** old retention 0.3226 → 0.3978 при old465/new414/replay177. Это bigram/replay result, не LoRA-continuation proof. Действие: matched old/new source families, одинаковые обновления, measure old retention and new acquisition. [Evidence](44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md).

## P1 — восстанавливать только условно

- **E22 / multi-dataset router:** ранний FAIL .4892→.4944; поздний на другом корпусе PASS .4709→.5646. У позднего TypeScript и Tests всё ещё F1=0. Действие: пройти малые классы и показать, какой dataset реально получил Flow.
- **E51A / code-context trace:** 27 T / 13 V, predicate overlap .2051→1.0 в узком корпусе. Действие: сохранить predicate→neighbor→consumer trace, а не выдавать это за free code generation.
- **Сначала вернуть recipient:** исторический KNN/memory и RP01 q/v LoRA с random B0 — разные state owners; изменённые LoRA веса при C# free 0/3→0/3 и ENRU free0/4→0/4 ещё не подтверждают полезное обучение.

## Два дополнительных кандидата, не объявленные root-cause RP01

- **N45-A:** ProductHeldOutRunner ProductMatch не использует ExpectedTranslation/ExactMatch/ChrF/BLEU при решении; source-only counterexample «Do not delete the file» → «Удаляй файл» при маркере «файл» даёт положительный ProductMatch, несмотря на инверсию закона.
- **N45-B:** агрегатор EmojiPreservationAccuracy использует foreach(char)+UnicodeCategory.OtherSymbol, не распознавая surrogate pairs для ряда emoji, включая 👋, 😀; пустые optional emoji-markers создают ещё один vacuous-pass путь.

**Источник, точные линии, отрицательные контроли:** [N45 source audit](45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md). Реальный продуктовый эффект и улучшение после исправления НЕ измерены.

## Короткие A/B вместо большого рана

Полный набор с входами, владельцем состояния, A/B, критерием успеха/опровержения и предметными guardrails: [N46 — микроэксперименты](46-MICRO-LANGUAGE-TRANSLATE-RESTORATION-AND-EVAL-2026-10-19.md). Начать с E09/E26/E43 на соответствующих доменах и реально потребляющем Flow; N45 проверить на уже полученных frozen ответах. Никаких переименований замороженных старых PASS/FAIL, никаких фиктивных новых train runs или подгонки тестов.

**Граница безопасности:** изменялась только planning/possibleDifs; core/docs/plans/replanning-possible-diffs строго READ ONLY.

## Дедупликация

Уже найдены прежними агентами: E09 per-file labels/split, E26 historical static dictionary, ошибочный Verifier concept/context parser по порядку enum, DPO prompt truncation/hash collisions, RuleTags/language-key drift, rejected phrase annotations, legacy 4→6 checkpoint. Эта глава ссылается на них; **не повторно считает их новыми**. Новые source-level кандидаты в этом пакете относятся к конкретному ProductHeldOutRunner gate/emoji aggregation, а не к общим заявлениям «не работает LoRA».