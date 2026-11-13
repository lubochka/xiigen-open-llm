# SiLU denominator reuse — сверка с уже опубликованным SILU-SIGMOID-REUSE-01

**REVIEW_ALIAS / NOT_A_NEW_EXPERIMENT_ID / NOT_RUN.** До последней сверки этот черновик назывался `SILU-DENOM-REUSE-01`. В planning `8346dfb697be43041a477886db26c508453fe5ce` другой агент уже опубликовал тот же фактор как **SILU-SIGMOID-REUSE-01**: сохраняется именно знаменатель, forward остаётся `x/d`, backward читает `1/d`. Разные названия не создают две гипотезы.

Единственный владелец будущего опыта — [каноническая опубликованная карточка](../../2026-10-20-learning-causal-audit-fb9175cc/cards/SILU-SIGMOID-REUSE-01.md). Этот файл сохраняет независимую проверку источника и снимает дубликат. **Не создавать SD01, отдельный host, новый бюджет или вторую попытку по этому alias.** У canonical сейчас AUTHOR_SETUP_REQUIRED, не готовый измеренный результат. Текущая фаза — документы, без реализации и запуска.

## Что подтверждено по коду

[Trainer:117–135][trainer] выполняет настоящий Forward→loss→BackwardLoss→AdamW. [SwiGLU:40–51][ffn] вызывает [TensorOps.Silu:1132–1162][silu]. Forward считает `x/(1+Exp(-x))`, backward повторяет `1/(1+Exp(-x))`. Один Exp на grad-элемент можно повторно использовать через сохранённый denominator. Это чтение кода, не измеренный выигрыш.

Один фактор canonical сохраняет исходные деления, производную, precision, cancellation checks и AccumulateGrad. Нельзя дополнительно заменить `x/d` на `x*(1/d)`, восстанавливать sigmoid через output/x, кешировать между графами, менять mask, optimizer или LoRA targets. Дополнительный float-массив имеет цену allocations, live payload и GC.

**Важная граница readout:** [LoraLinear:107–126][lora] остаётся differentiable и при `training:false`, поэтому один `RequiresGrad` не гарантирует «кэш только во время обучения». Canonical карточка уже решает это для экспериментального фактора: switch выключен при подготовке, A и readout; включён только на четырёх B updates. Она измеряет training-only policy. Из неё не следует нулевая дополнительная цена production inference при безусловном переносе кеша. Это дополнение к границе переноса, не ещё одна гипотеза.

## Уточнение прежнего вывода о before

H16 RESTORE-MINIMAL-SET остаётся частичным donor: 3/40→4/40 токенов при 0/2 полных ответов. Он не подходит как уже освоенные два полных ответа и не совместим по форме с H64/H512.

Но вывод «вообще нет известного рецепта ненулевого полного ответа» был бы слишком широким. Повторно прочитаны [LEARN-SANITY-PROBE source][sanity-source] и [raw][sanity-raw]: H64/V256/L1/qv-rank4 после 400 native SFT updates даёт TRAIN 0/2→1/2 и EXAM 0/2→1/2, NLL 7.960885→1.887945 и 8.015334→2.798860. На каждой панели модель выбирает постоянный знак; условное арифметическое правило не доказано. Полезный измеренный сигнал сохраняется. Source пишет JSON, но не сохраняет H64 weights/optimizer checkpoint.

Canonical карточка поэтому задаёт конечную подготовку нового S0 по этому рецепту, сохранение weights и optimizer400, затем A/B по четыре SFT updates. Подготовка включена в бюджет: 408 updates и 24 scoring forwards. Это спецификация автора, не сделанный нами запуск и не уже полученный reusable checkpoint. Точные модель, данные, 36 source-файлов, `SILU01`, ограничения и будущая команда принадлежат canonical карточке; не собирать альтернативу по этому review.

## Исполнитель, успех и завершение

Модель исполнителя и проверяющего — текущая сессия по умолчанию, без override. Здесь результат — русский Markdown review, не runtime report. Для будущего canonical опыта формат отчёта и точные пути берутся из его полной карточки; общий смысл — before/after, native workload, полезная ось, все ухудшения и пределы.

Skills из `C:/Projects/llm_mvp_core/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Читать применимые разделы, честно указывать scope. Старые model overrides и автоматическое расширение бюджета не применять.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Рядом показываются остальные изменения, включая ухудшения.** Арифметический подсчёт Exp не заменяет измерение native обучения. Данный alias не добавляет результат в required-changes и не разрешает перенос в production.

**Запрет продолжать прежнюю задачу:** при NEXT немедленно прекратить предыдущие report/hash/review/Git/build/retry и перейти к назначенному следующему ID в разрешённой фазе. При STOP/«закончить немедленно» прекратить работу. Для отдельного review NEXT=END. В canonical общей очереди SiLU→HNSW-QUERY-NORM-REUSE-01; POS не вставляется перед HNSW и прежними приоритетами.

При будущем разрешённом runtime: Сначала обнаружить реально доступные launch/wait/status и прочитать их схемы. Зафиксировать tool_wait_binding: provider, handle, ongoing/terminal markers, смысл yield/RPC timeout и первоначальный experiment/phase deadline. После выдачи команды ждать тот же handle настоящим bounded wait, не более 60 секунд на один вызов, сообщая прогресс не реже раза в минуту. Два быстрых неблокирующих snapshot без новой информации требуют перейти в настоящий wait. Нормальный silent/running wait продолжать до terminal state или исходного deadline; правила «только два ожидания» нет. Независимая диагностика назначенных файлов и своего PID/session нужна при неисправном wait или противоречии каналов; затем для живого процесса вернуться к wait. Отсутствие stdout не доказывает отказ. Ни диагностика, ни tool yield не продлевают бюджет и не разрешают второй launch. RPC/yield timeout не равен experiment/phase deadline. [Контракт31](https://github.com/lubochka/llm_mvp_core/blob/003083feab72157e54184bb66d0d940f48d836fd/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md). Старые experiments/claims/raw сохраняются. Этот review не меняет protected docs и код.

[Промпт проверки alias](../prompts/SILU-DENOM-REUSE-01.md). [Актуализация источников](../08-latest-reconciliation.md).

[trainer]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/04.TrainingLoop/Trainer.cs#L117-L135
[ffn]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/03.TransformerModel/Layers/SwiGLUFeedForward.cs#L40-L51
[silu]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/00.TensorCore/Core/TensorOps.cs#L1132-L1162
[lora]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/10.LoRA/LoraLinear.cs#L107-L126
[sanity-source]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs
[sanity-raw]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json
