# LC-REF-CACHE — промпт будущему исполнителю

## Задача и текущая граница

Проверь один фактор: экономит ли bounded cache двух log-probability чисел неизменной reference время/CPU **настоящих** повторных DPO-обновлений в `DpoTrainer.StepVector`. Основной документ — [01-reference-score-reuse.md](01-reference-score-reuse.md). Источники и точные входные строки — [02-source-selection.json](02-source-selection.json) и две fixture JSONL рядом.

Этот текст подготовлен в рамках аудита. Он не утверждает, что core уже можно менять или запускать. Сейчас меняется только документация planning/possibleDifs; защищённая `llm_mvp_core/docs/plans/replanning-possible-diffs` не меняется. При отдельном поручении на реализацию используй его фактический scope и уже имеющуюся авторизацию; не придумывай новый обязательный approval процесс из исторической карточки.

## Обязательное правило успеха и участия

Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. В отчёте рядом покажи изменения всех остальных измеренных показателей, включая ухудшения.

Опыт действителен только при реальном участии фактора в обучении: exact material → штатный reference score → DPO objective → policy backward → AdamW → сохранённое изменённое LoRA состояние. Cache-only benchmark, serving-only вызов, mock optimizer, Python-замена DPO, lookup вместо Student или zero-update контроль не являются этим опытом. Не трать runtime на неучебную подмену.

## Что уже определено

Core snapshot: `bf9b543e149ef197da85a767821d98c9b67916d2`. Оригинальный owner исправления: `11.DPO/DpoTrainer.cs:878–883`; реальный повторяющий вызов: `18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1347–1378`. В source no-cache путь вычисляет обе reference стороны заново; `BatchLogProbs:1007–1012` использует `Forward(training:false)`.

Основная тема — документация C#. Используй fixture из **существующих** source строк1/2: postfix increment и zero-based index. EN→RU строки5/22 — отдельная необязательная панель с собственным baseline. Нельзя незаметно перенести на неё C# tokenizer/B0 или изменить нынешний ENRU C50→100 admission.

A обучается текущим кодом. B обучается тем же кодом с единственным кэшем reference. Два точных batch entries на каждом из двух шагов, один trainer/reference/optimizer на плечо, cap2 **пары**, `MaxStepCount=2`, `ConvergenceWindowSteps=3`. Оба плеча начинают с одинаковых B0/tokenizer/optimizer/RNG bytes. В B кэш до шага1 пуст. Порядок пар и штатные settings одинаковы; LoRA dropout0 в основной C# конфигурации. Helper делает один lookup пары и возвращает два scores. Счётчики `pair_cache_hits`/`pair_cache_misses` измеряются lookup пары, `reused_reference_scores` — scores отдельных сторон, `actual_reference_forward_calls` — настоящими Forward одной стороны. Предсказание B:2 pair misses+2 pair hits,4 reused scores,4 actual reference forwards. A:8 actual reference forwards и0 cache hits/misses/reused scores, поскольку кэш отключён. Все числа здесь ожидаемые, не измеренные.

## Что реализовать после предоставления runtime-scope

1. Сначала закрыть существующий native entry, а не переписывать обучающий алгоритм. Для малого host использовать штатные saved-root loaders, governance/typed authors, `LogicalDpoPreparedBatchMaterializer.Prepare`, exact material validation, scoped server-issued mutation capability и `CreateForProjectLora`. Эти элементы доступны в `CodeDocumentationSimulationDpoExperiment.cs:106–167`. Новый helper не выдаёт себе разрешение. Фактор подключения двух пар одинаков в A/B.
2. Назвать и реально открыть B0 manifest/weights/tokenizer и исходные optimizer/RNG. Если их нет, вернуть конкретный `BLOCKED_NATIVE_INPUT` с недостающими путями/identity. Не заменять B0 random моделью, не подставлять source pins вместо проверки файла и не сочинять CLI-флаги. Фактическую команду дать только после появления host.
3. Добавить per-trainer кэш только для проверенных project exact pairs. Key учитывает immutable reference/config, exact tokens+mask/material digest, tokenizer и numeric mode. Сохранить все admission/frozen-reference/rollback/publication guards; legacy без кэша. Cache miss вызывает прежнюю формулу. Policy Forward/backward/update не кэшировать и не пропускать. Проверка frozen reference остаётся; новый trainer — новый кэш.
4. Сборку/startup отделить от измеряемого обучения одинаково в A/B. В этом первом опыте дополнительных Train/StepVector для warmup нет; не прогретый JIT/backward первого шага остаётся в измерении, это cold micro-run. B-кэш перед шагом1 пуст. Записать actual время **двух целых StepVector**, CPU difference того же окна, method/peak RAM, bytes кэша и настоящие counts. Не переименовывать phase observations в actual forward count.
5. Снять B0 и конечные LoRA/base/reference state и optimizer m/v/step. Для loss/margin вызвать настоящий `EvaluateMany` (DpoTrainer:767–809) до шага1 и после шага2 каждого плеча, каждый раз на тех же2 парах: всего4 calls, отдельно от training timer, без cache helper. Это16 reference и16 policy evaluation forwards A+B сверх тренировочных; учитывать отдельно. Его average loss не содержит teaching-vector scaling. StepVector metrics до ApplyOptimizer не выдавать за post-update. Доказать реальные два updates каждого плеча, зафиксировать несовпадение A/B, если появится. Предсказание8→4 training reference forwards не является измерением и не доказывает speedup.
6. Отчёт сохранить в разрешённом месте planning/possibleDifs с source commit, patch, actual command/runtime, inputs identity, raw observations и человекочитаемым выводом. Для generation accuracy писать `NOT_MEASURED`, если её не измеряли. При resource success не требовать роста accuracy; все известные ухудшения указать.

## Проверка результата перед передачей

При подготовке host учти `CodeDocumentationSimulationDpoExperiment.cs:85–87,173`: domain/mode/TRAIN-hash once guard не разделяет два плеча новым RunId. Нужен заранее объявленный study с отдельными arm-journals, теми же исходными identities и штатными live capabilities. Не удаляй issued markers, не меняй TRAIN для обхода и не повторяй arm с неизвестным эффектом. Общая схема paired owner из параллельного пакета61 пригодна как ограничение исполнения; PAD-трансформацию и его batch1 one-pass сюда не переносить. Ровно две пары × два повторных updates на плечо остаются выбранным расписанием.

Прочитай текущие skills по code examination, learning path, improvement measurement, prompt patch и человеческой коммуникации в `docs/skills`. Не объявляй весь skill загруженным, если прочитал только фрагмент. Приоритет — пользовательская граница и реальный путь обучения.

Не добавляй универсальных AST/порядковых правил: они не являются выбранным фактором. Не применяй reuse к unique one-epoch main500 без анализа hit rate. Не объявляй новый опыт исполнением D04 с его прежним расписанием двух разных одиночных уроков.

Итог в понятном виде: какой именно native механизм учился; что изменено в B; какие пары и сколько обновлений выполнено; сколько заняло A/B; на каком полезном показателе есть измеренный успех; что стало хуже или не измерялось; где лежат доказательства. Если native run не состоялся, написать это явно.
