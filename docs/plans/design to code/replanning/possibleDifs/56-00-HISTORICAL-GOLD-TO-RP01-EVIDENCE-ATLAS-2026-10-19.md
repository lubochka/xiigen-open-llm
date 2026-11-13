# N56 — Gold-first audit: от успешного обучения к RP01 и обычному потребителю

**Дата:** 2026-10-19. **Авторская область:** только \`xiigen-planning/llm_plan3/llm mvp/design to code session/replanning/possibleDifs/\`. **Статус:** новый независимый сравнительный SOURCE / ARTIFACT REVIEW; **никаких новых native training runs и тестов в данном проходе не запускалось**. **Запрещённая к изменению область:** \`llm_mvp_core/codex/dpo-implementation/docs/plans/replanning-possible-diffs/**\` — только чтение.

**Прочитанный срез:** planning \`llm_plan3@790bcf3e396411fb8d5a29504cdadc5dd7ef32fa\`, core \`codex/dpo-implementation@eb05219c27663835ce927c7bf97cf1274c07cddb\`. Если HEAD переместился, выводы source относятся к этим SHA; перед разработкой заново проверять caller и актуальный SHA. Доноры: \`PrepareToGPL@31346d8928be4469b01aaf313780e8f950ff91e2\`, \`DesignToCode-clean@e346b88a3cc1fcce48aefa626bfc7b00509eefd6\`, \`language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9\`, \`external_domains@3036a01acec938f0b83fa4ffbda26202cc35d639\`, \`orchestrator-extensions@b293045bd89b1eb96d3e32fedd341f1a4170adbc\`, \`local_wiki_planning@219d7ba89dc3ef43e03e17f1461ad96154d3dd7a\`. Наличие ветки — **не** доказательство положительной метрики.

## Принцип и статус доказательства

Цепочка для каждого положительного механизма: **original TRAIN + held-out + measured before/after → точно названный получатель (neural/memory/Unit/Flow) → сохранённое состояние → cold restore → фактический ordinary consumer → новые предметные задачи**. Метки: **M** — измеренный опубликованный исход; **C** — непосредственно прочитанный контракт кода; **H** — проверяемое предположение; **S** — историческое описание/артефакт без нового измерения. **M в одном потребителе не переносится автоматически в другой**. Тема «перевод» — домен обучения; EN→RU/RU→EN — отдельные профили, а kNN/LoRA — механизмы; см. \`docs/skills/human-communication-GLOSSARY.md\`.

## Исторические положительные механизмы и пределы

| Область/источник | Что реально установлено | Что не установлено / минимальный следующий вопрос |
|---|---|---|
| **M — Executor EN→RU, 18-Oct P125**, 8 структурированных уроков | 0/24→19/24 на ограниченной frozen панели; exact 8/8, near 3/3, композиция 8/8; обычный Flow. | Не доказаны новые независимые source-families, шесть направлений, LoRA-эффект; 2 явных смысловых ошибки плюс неоднозначный cc03; сравнить на source-disjoint блоках. |
| **M — доставка phrase mappings** | matched 11/24→19/24; 16 mappings записаны в память. | Не смешивать effect of added mapping со всем \`TrainStructuredBatchAsync\`; проверять, что верная phrase сохраняет отрицание, объект и направление. |
| **M — P25 cold restore** | Сохранены 24/24 output/route/confidence/verdict и 19/24 accuracy, новых уроков 0. | Успешно для этого EN→RU checkpoint, не доказано восстановление веток v1/v2 и других владельцев. |
| **M — neural embedding/bias/DPO, 18-Oct** | Native updates/производные улучшены, но полезных ответов 0/24→0/24 в каждом опубликованном опыте. | Не утверждать, что всё нейронное обучение невозможно; каждый опыт проверял свой recipient, некоторые только косвенно относятся к RP01. |
| **M — RP01 simulated C#500 / ENRU500** | 16 LoRA tensors изменились, base/reference frozen; свободная C# 0/3→0/3, ENRU 0/4→0/4 после 50 шагов/кампанию. | 500 строк ≠ 500 законов: C# 25 semantic families; random B0, C# BPE629, q/v rank8. Нужен warm-base vs DPO-only и обучение/генерация на одинаковых heldout, а не CI PASS. |
| **S+historical M — E09 C# AST kNN** | Отдельные архивные receipts \`0.2413→0.3507\` при k=5; ранее \`0.3368→0.3472\` не прошёл порог matrix. | Первое сравнение **с разными знаменателями validation**, поэтому не доказан причинный +45%. Перезапустить matched split по **файлам** и реальному AST consumer. |
| **S+historical M — E25/E26** | E25 POS class 0.3438→0.875 (synthetic, 32 validation); E26 contextual dictionary 0.4→1.0 (15 fixed cases). | Не доказан перенос в 2026 RP01 Student или живой переведённый ответ; восстановить learned POS/contextual signal в названный переводческий Unit/KNN и измерить порядок/полярность. |
| **S+historical M — E43 replay** | В историческом bigram memory retention 0.3226→0.3978 на old/new/replay материалах. | Это не нейронная LoRA и не все домены. Идентифицировать буфер, sampling, actual old/new ordinary consumer и изменённый checkpoint. |
| **C/S — orchestrator-extensions** | Старый manifest с 15 learned patterns использует schema v1, а нынешний loader требует v2+admission proof. Source показывает отказ/ordinary fallback. | Не принимать «старый паттерн отсутствует» за плохое обучение; нужен законный v1→v2 migration/trace/cold restore, не обход provenance. |
| **C/S — DesignToCode-clean** | Утрачена прежняя прямая передача visual score; но old/new local \`AddDelta\` не потребляет этот score для градиента, он влияет на reporting margin. | Простое возвращение поля не доказало бы quality gain; нужна независимая проверка **знака** feature/weight contribution и visual heldout. |

**Первичный результат настоящих запусков:** [core results / README](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/README.md). **Близкие ранее подготовленные crosswalk**, НЕ мои новые находки: [планирование N38](38-MASTER-PRIORITIES-POSITIVE-HISTORY-NEW-HYPOTHESES-AND-MICRO-DECISIONS-2026-10-16.md), [orchestration gold-first](00-ORCHESTRATOR-EXTENSIONS-GOLDEN-RESTORE-P0-2026-10-19.md), [PrepareToGPL](55-PREPARETOGPL-RESTORATION-PRIORITY-AND-DIFFERENCE-MATRIX-2026-10-19.md), [DesignToCode-clean](54-DESIGNTOCODE-CLEAN-FAST-DOMAIN-EXPERIMENTS-2026-10-19.md). Не присваивать эти открытия N56.

## Не потерять реальные слои обучения

1. **DPO material:** chosen/rejected должны отличаться смыслом, иметь правильное направление предпочтения, source provenance, сохранять обязательный хвост/отрицание через actual tokenizer/mask; compiler/source-law/expert gold независимы от teacher. Условия \`chosen.model != rejected.model\` и запрет train on raw rows — дополнительные правила источников XIIGEN.
2. **Vector tokenizer/BPE:** одинаковый VocabSize ≠ одинаковая таблица tokenId→bytes; проверять frozen encoder SHA, root-dimension identity, Unicode/operator handling, model-visible contrast, prompt/answer/EOS framing. Условная дыра контроля \`DpoExactPairMaterial\` / Student tokenizer уже обнаружена N39, **не новая находка**.
3. **Neural student/LoRA:** до DPO проверять способность B0 понимать prompt и ответ; finite-difference, trainable parameter map, optimizer moments, reference freeze, inference cache parity; изменение logits/margin ≠ улучшение свободного ответа.
4. **Pair→Unit/Flow/Graph:** кто принимает пару, какую UnitId/Level, какой learned state/admission receipt, кто выполняет \`ExecuteAsync\`, типизированные many-to-many outputs, learned Flow order и graph edges — раздельные контракты. Детерминированный routing/lookup не выдавать за обучение.
5. **kNN and selective semantics:** один и тот же метод нельзя внедрять везде. AST/ROSlyn/SQL → синтаксическое дерево и роли; перевод → порядок, отрицание, морфология, падеж, направление и topic; Wiki → timestamp/source authority; orchestration → причинная цепь/зависимости. Не применять majority vote по свободным переводам без заранее согласованного identity law.
6. **Checkpoint and serving:** корректный checkpoint file, optimizer state и validated manifest — необходимы, но сами по себе не подтверждают, что **ordinary consumer** подключён к нужным весам, памяти, schema и профилю после fresh-process restore.

## ВАЖНО — отрицательное подтверждение, чтобы не чинить несуществующий дефект

Прочитан [\`ExecutorUnit.Execute\`](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L364): локальный fallback = confidence 0.49. Но [\`TrainedTranslationAdapter.TranslateAsync\`](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L139) **не выпускает** 0.49 как trained success, а возвращает Bootstrap. [Flow](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationRoutingFlow.cs#L83-L126) отличает Bootstrap. Следовательно, утверждение «0.49 автоматически становится успешным переводом обычного Flow» **опровергнуто прямым caller trace**. Для конкретной другой ветви продукта возможен другой consumer — нужна отдельная трасса.

## Следующие источники и эксперименты

- [N56-01: дополнительный переводческий consumer/kNN cross-seam](56-01-TRANSLATION-KNN-CONSUMER-NEW-CAUSAL-SEAMS-2026-10-19.md).
- [N56-02: аудит покрытия/реального исполнения защищённых экспериментов](56-02-PROTECTED-EXPERIMENT-COVERAGE-AND-QUALITY-AUDIT-2026-10-19.md).
- [N56-03: короткие воспроизводимые доменные карточки для следующих агентов](56-03-FAST-DOMAIN-AB-EXPERIMENTS-AND-EXECUTOR-REVIEW-2026-10-19.md).

**Вердикт:** установлено существенное несовпадение между доказанным обучением памяти/исторических функций и RP01 neural preferred response. **Не доказано**, что именно одна «неправильная LoRA» или один «плохой kNN» является root cause; для этого требуется matched A/B с реальным получателем и consumer.
