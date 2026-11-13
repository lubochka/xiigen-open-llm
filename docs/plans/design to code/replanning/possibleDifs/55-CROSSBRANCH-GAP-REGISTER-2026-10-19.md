# N55 — реестр возможных потерь обучения, их источников и области применимости

**Evidence boundary 2026-10-19:** PrepareToGPL@31346d8928be4469b01aaf313780e8f950ff91e2 и codex/dpo-implementation@6d36492714ebc1b9ae5f31a5a1da00673f524d83. Плюс prior RAG из xiigen-planning/.../replanning и защищённого (только чтение) llm_mvp_core/docs/plans/replanning-possible-diffs. Это **дополнение** к сотням прежних записей, а не заявление, что остальные не существуют.

## 1. Прямой разрез branch ↔ recipient ↔ learning evidence

| Компонент | В PrepareToGPL | В текущем core | Что действительно следует |
|---|---|---|---|
| Runtime перевода | [4 языка-направления](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L555-L620) | [6 направлений](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L714-L790) | Действительный branch diff; legacy restore requires versioned migration |
| Unit алгоритм | [TranslationTrainedUnits](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationTrainedUnits.cs) | [current](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationTrainedUnits.cs) | **Идентичный Git blob** 1742abfc9fb617dfc66e4d618e0c169da01a6ccc. Регрессия прежних функций должна искать другие inputs/consumers/checkpoints |
| Semantic tokenizer | [gold](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationSemanticTokenizer.cs) | [current](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationSemanticTokenizer.cs) | Идентичный blob ee6d81c7933fa33a9c57a2111990019ec55e5dab; новых функций за счёт одного нового runtime не появилось |
| Translation Eval helper | [hardcoded RU_EN](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L527-L545) | [то же](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L686-L705) | **Не diff**, но новый контекст 6 directions даёт проверяемую проблему N55-01 |
| LoRA mechanism | [external adapter list](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/10.LoRA/LoraApplicator.cs#L10-L62) | [in-model typed slot](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/10.LoRA/LoraApplicator.cs#L432-L493) | Настоящая разница, но исторический переводческий Unit не требовал neural-gradient LoRA |
| DPO step | [simple trainer](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/11.DPO/DpoTrainer.cs#L38-L80) | [admitted trainer](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/11.DPO/DpoTrainer.cs#L650-L703) | Без matched gradient + output не доказан numerical regression |
| Synthetic RP01 | не равен исторической Unit-памяти | [frozen C#500/ENRU500 evidence](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | 0/3 и 0/4 unchanged free answer; «обновились веса» не значит «освоены смыслы» |

## 2. Таксономия возможных различий — каждое требует отдельного доказательства

| Область | Проверяемый риск | Домен/задача, где уместно | Новизна/статус |
|---|---|---|---|
| DPO data | Chosen/rejected совпали после byte/BPE tokenize или потеряли смысл отрицания; тестовый law не соответствует паре | C# семантика, ENRU, DesignConversion | Известно ранее, надо native material trace |
| Preference objective | Разные mask/length response log-prob, FP32 cancellation или некорректный relative reference margin | Только neural DPO | Earlier records; source audit ≠ численное доказательство |
| LoRA target | Q/V-only недостаточно при random untrained base; rank/alpha/dropout, attached adapter изменён | Neural Student | Existing known, causality OPEN |
| Pretrain/SFT | B0 untrained, DPO не формирует словарь/грамматику | Neural C# + ENRU | Сильная существующая гипотеза, не new |
| Vector tokenizer | Изменились vocabulary identity, pad/EOS/mask, pre-vs-post normalization, positional information | Neural и индекс отдельно | Earlier docs, проверять exact consumer |
| DPO→sub-Unit | Direction, law, source-id, роли и multi-step graph split пропали между pair и Unit | Translation/Design/Graph | Source/contract mapping needed |
| Unit implementation | Реальная запись выбранного правила в exact/phrase/learned maps, отсутствие silent skip | Translation, graph, DTC | Mixed; inherited code same |
| KNN selection | top1 instead of neighbor vote, signature aliases, post-filter misses, blocked correct candidates, stale HNSW | Любой домен с реальным KNN | Already covered by prior N11–N54, domain native proof OPEN |
| AST/order | Struct features устарели или учтены в matrix, но не в query-dependent neighbor comparison | C#/TypeScript AST | Только structural coding/domain; non-universal |
| POS/negation | Порядок участников, отрицание и морфология проигнорированы bag-of-words | ENRU/RUEN, domain language laws | Domain-specific. UNKNOWN не hard-veto |
| SQL | NULL vs COUNT(), WHERE vs HAVING, outer join semantics не распознаются | SQL only if real SQL consumer exists | Пилот условен, нельзя требовать Roslyn AST |
| Old matrix FAIL→KNN PASS | Потеряли функцию, ошибочно отвергнутую в другой carrier | E25/E26/E09 donor paths | Положительный факт переноса ещё нуждается в точной идентичности функции/commits |
| Checkpoint | Старая обученная память не импортируется 4→6, corrupted/stale index, training provenance mismatch | Translation, later graph | Реальная 4→6 diff уже установлена коллегами |
| Replay | Refit забывает прежний batch, partial state/failed HNSW insertion и priority bias | Continual memory | Ранее подробно описано, state tests not domain gain |
| Deterministic bypass | Fallback/verifier/exact lookup выдаёт хороший тест, но user learned Unit never invoked | Every hybrid flow | Нужно trace каждого ответа |
| Graph | Split DPO → wrong ILearningUnit/Flow, выброшены causal edges/typed outputs | Orchestration, code trace | Разные owners/results; не сводить к generic accuracy |
| Ordinary consumer | Learner state не загружен по registry; prompt отличается от trained task; fallback hides learned decision | Project Wiki/Translation/Design | Chain-dependent; cannot infer from test PASS |
| Multidirection Eval | Все EvalItem переводятся под RU→EN и проверяются единым exact expected | Mixed six-direction translation | **N55-01 узкое source observation; inherited** |
| Accumulated metric | N55-01 score перезаписывает AccuracyBefore следующего batch на same instance | Continual translation | **N55-02 source-derived interaction**, influence unproven |
| Metric validity | AccuracyAfter означает coverage, macro Unit loss или real end-to-end? source leakage, seed coupled, confounded sizes | All | Пункты coverage/macro ранее доказаны core №58; не new |
| Generation | EOS/early stop/decoder output vs normal served output, next-token semantics | Neural generation | Earlier documented; need matched semantic output |

## 3. Не терять доказанный позитив из «неудачных» экспериментов

**Рекомендация:** отдельный ledger «что обучалось → где измеряли → что именно переносится». E09 structural kNN имеет несовпадавшие baseline/eval population; не объявлять чистый +10.94 pp эффект без same-input replication. E22 имел поздний positive macro-F1 0.4709→0.5646, но поменялись labels/corpus; [protected record](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md). Phrase mapping 11/24→19/24 после 8 lessons — положительная уже сохранённая память, **не нейронный LoRA результат** [gold-first dossier](00-PREPARETOGPL-RECOVERY-FIRST-2026-10-18.md).

## 4. Что считается доказательством

- CODE_CONFIRMED: точная branch SHA, path/line, вызывающий код и формат параметров.
- HISTORY_REPORTED: raw frozen experiment + dataset/target/holdout + ограничения.
- PROXY_ONLY: source-equivalent formula или игрушечный oracle; отдельно от actual C#.
- CAUSAL_NATIVE: same task, material, seed, same legitimate recipient & score, A/B с одним изменённым фактором и свежими raw native traces, до/после cold restore.
- NOT_PROVEN: причину падения нельзя вывести из самого наличия багоподобной ветки кода.

**Навыки и граница:** применены human-readable-architecture-audit, phase-planning, plan-review, code-examination, prompt-patch-authoring, learning-path-audit, human-communication glossary из [skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills). Защищённые документы core/docs/plans/replanning-possible-diffs использованы только как reference и НЕ редактировались. В архиве проверены именованные пересечения; глобальная уникальность N55 по всем конкурентным агентам не гарантируется.
