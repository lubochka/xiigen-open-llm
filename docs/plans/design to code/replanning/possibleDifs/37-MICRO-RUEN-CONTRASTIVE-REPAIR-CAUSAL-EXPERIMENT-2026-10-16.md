# N37-MICRO: проверка потери контрастных RU→EN уроков за один короткий цикл

**2026-10-16 · WORK ORDER / NATIVE_NOT_RUN / NO_STUDENT_QUALITY_CLAIM.** Кодовый источник core codex/dpo-implementation@793d7b755b23aac90445da3ac6661b1ff7e6672. Не менять core docs/plans/replanning-possible-diffs. Живую диагностику делать в отдельном worktree/ветке исполнителя, не менять действующие RP01 Main500/B0.

## Что отличаем

Гипотеза H37: отбор новых уроков по множеству слов с порогом 0.70 отбрасывает значимые contrastive source pairs (отрицание, число, порядок), затем реальный RU→EN Translator не может использовать нужное различие. Альтернатива: пары не нужны, источник не достигнут или отсутствует learned consumer effect.

Источник: [Build filter L201–255](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L201-L255) → [Batch029 L86–160](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationBatch029Runner.cs#L86-L160) → [TrainStructuredBatch L89–147](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationMachine.cs#L89-L147) → [Executor Train/Execute](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L456). Проверить фактический запуск native, не переносить механически результаты toy proxy.

## M0 — механический gate, без обучения (сначала)

Один и тот же history и кандидат на обеих ветках. Четыре контролируемых source family, в каждом зафиксировать пару независимых меток: law-value, expected action и instance identity.

1. RU_EN NEGATION: «Этот важный файл уже доступен» / «Этот важный файл уже не доступен». Jaccard=5/6; разные truth values.
2. RU_EN ORDER: «Сначала проверь доступ затем опубликуй событие» / «Сначала опубликуй событие затем проверь доступ». Jaccard=1; порядок действий отличается.
3. RU_EN QUANTITY: «Клиент получил три важных письма вчера вечером» / «... четыре ...». Jaccard=6/8; другое значение числа.
4. CONTROL: тот же текст с регистром/пунктуацией — семантический дубликат и должен быть удалён.
5. NEGATIVE CONTROL: фраза про другой предмет — не должна удаляться.
6. BOUNDARY CONTROL: пример с Jaccard ровно 0.70, где training guard (>=0.70) и heldout guard (>0.70) расходятся; брать вычисляемую заранее пару.

Создать узкую диагностическую сборку, которая **вызывает настоящую** TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness и Batch029 contamination guard. Для приватного guard использовать scoped test/API, не переписывать production ради доступа. Выдать pair id, history id, similarity, status, reason. Expected current for cases 1–4: DROP; 5: KEEP. В heldout отдельный current guard проверять без доступа к sealed cases. Если native не воспроизводит proxy — первичный finding опровергнут или версия изменилась.

## M1 — one-factor Unit-learning AB

Зафиксировать один corpus-history checkpoint/selection, те же валидные train-ready RU_EN examples и exact same heldout-family inventory. Не делать новые DPO, не менять LoRA и tokenizer. Сохранить hashes источника, training-state и consumer.

**Arm A (baseline):** текущий history filter без изменений.

**Arm B (contrast protected):** при текстовом Jaccard>=0.70 и независимо подтверждённом **изменении семантического закона** допускать контрастную новую пару, **не** допускать реальный дубликат. Это селектор только в исследовательском стенде, не правка production до M2. Пользоваться явной семантической разметкой независимого oracle (для RU_EN polarity/actor/quantity/order); не делать закон из regex на всех доменах.

**Arm C (ablation, dev only):** все новые пары без lexical-history dedup; измеряет цену полной отмены защиты и leakage, не предлагается как производственное решение.

Для каждого arm: count proposed/admitted/removed, reason, recipient UnitIds, number of unique learned source lessons, checkpoint versions, selected ordinary Executor adapterId. Один и тот же pretrain state; нет совместного использования обученного output между ветками. Дополнительный тест меняет **только** порядок batch vs supplemental при одинаковом normalized A-side и двух допустимых chosen; фиксировать last-writer result отдельно, не смешивать с AB admission.

## M2 — отдельный доменный семантический контроль реального ответа

Не использовать для научного вывода те же фразы, по которым выбирался порог. Заранее заморозить семейства RU_EN: 6–12 development cases на polarity/order/quantity и минимум 12 **непересекающихся по исходному смысловому семейству** blind questions (не равнять варианты одной фразы независимым). Все arms на одинаковой панели; временно выделенный независимый reviewer проверяет смысл перевода: not/no, agent/action sequence, number, preserved names, и корректный отказ. Допустимы несколько адекватных английских форм.

Отчёт на arm и каждую семью: law-correct / wrong-accepted / correct-abstain / false-abstain / coverage / exact-match (secondary) / final adapterId / source-evidence / timing. Абсолютные числители и знаменатели; покажите paired per-case deltas с контрольным C. Ответ по literal template или изменилась только таблица bucket counts = **NO_QUALITY_GAIN_OBSERVED**.

Основное условие положительного результата: B против A действительно допускает правильные уроки, они достигают обычного TranslationRoutingFlow и на заранее выбранных новых вопросах повышают **семантическую** точность без критической регрессии и без грязного split. Если first two tiers PASS, но конечный ответ не изменился, искать **consumer visibility**, а не заявлять обученность.

## Зависимости, роли, быстрая остановка

Кодовые компоненты в scope: Builder, Batch029Runner, TranslationMachine, ExecutorUnit, TranslationRoutingFlow. Другие learning routers и RP01 neural B0 явно OUT OF SCOPE. Если фактический продукт никогда не вызывает Batch029, verdict **ROUTE_NOT_REACHED**, а задача становится исследованием reference trainer.

Исполнитель соблюдает [experiment-design-SKILL](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md), [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/generated-code-review-SKILL.md), [plan-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md). Отдельная задача имеет reviewer, который не создавал правильные answers и не меняет threshold по blind gold. Новые настоящие DPO-пары, если потребуются ПОЗЖЕ, должны генерироваться разрешённым внешним open-weight китайским teacher (обе стороны); CODEX/Astra выбирает задачи и независимо оценивает пары/Student, не выдавать synthetic за его экспертизу.

**Минимальный output:** M0 JSON per case, M1 source→lesson→Unit checkpoint record, M2 paired blind outcomes, failure taxonomy, commit/SHA identity, rollback plan, clean/dirty split verdict. Отключать все Windows GitHub runs для такого локального controlled experiment не требуется, но целевой тест и независимая проверка собственно learning mandatory. Native status на момент написания: NOT_RUN.
