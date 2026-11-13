# Источники, целостность и проверка добавочного пакета

> **Окончательный срез аудита:** native code совпадает с прочитанным `62bcc8f`; адресная документная сверка доведена до core `ce890a48efda7f00d0ee040f2383cc2d06428ea6` и planning `3b625a83ee878906d42544516741849a16c04689`. В protected **291 Markdown / 128 файлов карточек**. N61 уже покрывают Mode1 и reference reuse; N-KU имеет опубликованную узкую таблицу ответов; N60-E2 конкретизировал writer/state/reader, но host не завершён. Числа 286/126/577 ниже — исходный воспроизводимый срез. [Поздние изменения и приоритет версий](06-CONCURRENT-UPDATES.md), [последняя дельта core](coverage/06-FINAL-CORE-DELTA-ce890a.md).

## Версии и границы чтения

| Источник | Закреплённая версия | Что проверено |
|---|---|---|
| `llm_mvp_core`, `codex/dpo-implementation` | `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39` | Текущий selected native code, 286 Markdown protected, 126 cards/470 P-subcases |
| protected subtree | `b0dd91ced4ef3b2d40026bce829d0a11e5b4577e` | Все 286 Markdown локально равны Git blob; папка не редактируется |
| `xiigen-planning`, `llm_plan3` | `fb9aadf69e0669d36b820d61f76fe3ecc8697a7d` | 577 прежних Markdown possibleDifs, RAG index и выбранные первичные родительские документы |
| `language_translate` | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Первичные E25/E26/E43 и предметный смысл механизма |
| `orchestrator-extensions` | `b293045bd89b1eb96d3e32fedd341f1a4170adbc` | Первичный E09 и несовпадение размеров validation в исходном коде |
| `local_wiki_planning` | `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` | Предшествующая схема labels/enum/reader ProjectWiki |

Другие соседние branches перечислены в `evidence/source-ledger.json` как inventory, а не как полностью прочитанные реализации. Отсутствие механизма в 577+286 MD означает результат scoped novelty scan; это не полный поиск всей истории GitHub.

## Как проверялось покрытие

`coverage/catalog.json` сохраняет card path, blob SHA, число подслучаев, учебный механизм и статус семантической проверки. `cards.tsv`, `subcases.tsv` и `prompts.tsv` позволяют перейти от вывода к конкретной записи. `source-link-census.tsv` ищет точный путь исходного документа; буквальное упоминание не приравнивается к корректной экспериментальной постановке. `protected-integrity.json` доказывает равенство прочитанных Markdown закреплённому source tree.

Адресная проверка кода проведена для обучения/reader ProjectWiki, текущего neural StepVector/LoRA/Linear, tokenizer MergeChunk/Train, Translation loader/builder/TrainOnExample/Verifier/restore и отдельных Design Conversion state readers. У других семейств есть карта вопросов и предыдущая evidence lineage; здесь не объявляются выполненными 470 новых code reviews или экспериментов.

## Ключевые первичные подтверждения

- [DPO actual update][dpo], [LoRA base/adapter forward][lora], [повторный Linear transpose][linear], [BPE trainer][bpe].
- [ProjectWiki current learner][wiki-model], [enum новых маршрутов][wiki-enum], [native evaluation runner][wiki-runner], [предшествующий learner][wiki-donor].
- [Translation structured builder][translation-builder], [machine training][translation-machine], [trained units][translation-units], [принятый ingress update][required].
- [P125 learning][memory-result], [P25 cold restore][restore-result], [ingress A/B][ingress-result].
- [E25][e25], [E26][e26], [E43][e43], [E09][e09] и [код E09][e09-code].

Предыдущие четыре точных вмешательства сохраняют авторство (три из них уже отражены новой N61 на позднем head): [Mode1 retention / identity][prior-61], [frozen-reference reuse][prior-cache], [learned keyword canonicalization][prior-keyword]. Сходное имя family-карточки не делает их автоматически полными готовыми опытами protected.

## Независимая проверка и фактическое выполнение

Подготовка разделена между независимыми направлениями: native neural/tokenizer, neighboring branches/Unit routing, каталог карточек/правила. Итоговый пакет проходит перекрёстное чтение источников, факторов, метрик и точных команд. Конкретные замечания и исправления фиксируются в `evidence/review-log.md`; машинная проверка файлов/ссылок и разрешённых путей — в `evidence/validation.json`.

В этой среде отсутствует .NET SDK. Поэтому нельзя заявить compiler PASS для новых C# hosts. Нет новых training runs, generated answers, искусственно подставленных measured metrics или product test результатов. Python preparation/shape/source-diff/link проверки, если выполнены, названы именно так. Проверка одних исходных bytes не доказывает runtime-equivalence кандидата; это предмет предусмотренного малого опыта.

Перед публикацией проверяется актуальный planning head; добавляется только новый package и новый navigation file. Если head изменился, публикация строится поверх свежего head с проверкой отсутствия коллизий, без удаления и force. Кодовые findings остаются привязанными к прочитанному core SHA; обновление чужой документации не ретроспективно меняет фактический source review.

[bpe]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/01.Tokenizer/BpeTokenizer.cs
[dpo]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/11.DPO/DpoTrainer.cs
[e09]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json
[e09-code]: https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/Experiments/E09StructuralKnn.cs
[e25]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[e26]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-001.json
[e43]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json
[ingress-result]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[linear]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/00.TensorCore/Layers/Linear.cs#L44-L50
[lora]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/10.LoRA/LoraLinear.cs
[memory-result]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[prior-61]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-FIRST-AUDIT-2026-10-20/02-HISTORY-AND-KNN-HYPOTHESES.md
[prior-cache]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/dpo/01-reference-score-reuse.md
[prior-keyword]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/routing/KN-ROUTER-SMALL-LEARNING-EXPERIMENT-2026-10-20.md
[required]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/required-changes.md
[restore-result]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[translation-builder]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationStructuredBatchBuilder.cs
[translation-machine]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationMachine.cs
[translation-units]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain/TranslationTrainedUnits.cs
[wiki-donor]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs
[wiki-enum]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiDomainModels.cs#L473-L492
[wiki-model]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs
[wiki-runner]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs
