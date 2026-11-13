# Промпт исполнителю: ATTN-TRANSPOSE-STRIDES-01


**Позиция 39; NEXT = `JSON-OPTIONS-REUSE-01` (позиция 40).** Если назначен только один ID, после него завершить работу. Если уже назначена программа или дан переход «следующий», следовать этому literal NEXT из [реестра](04-execution-registry.md), без поиска другого опыта.

Это **будущее исполнение одного назначенного опыта**. В текущем аудите — только документы: host имеет статус `TODO_IMPLEMENT_EXPERIMENT_HOST / NOT_RUN`, обучение, сборка и измерения ещё не сделаны. Получив отдельное назначение ID, выполни только контракт [полной карточки](ATTN-TRANSPOSE-STRIDES-01-CARD.md). Не выбирай следующую карточку сам и не повторяй законченные опыты; очередность задаёт актуальный реестр. Защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs` не изменять.

## Сразу зафиксируй правила поведения

Пользователь описала недопустимый инцидент: **«четыре раза вместо переходя к следущему эксперименту продолжаешь прошлую задачу!!!»**, затем **«Закончить немедленно!!!!!!»**. При новом прямом стоп немедленно прекрати текущие действия, включая tools, отчёт, проверки, hashes и git, и сообщи только уже известное состояние. При «следующий эксперимент» прекрати дописывать/улучшать/проверять прежний и переходи только к названному следующему. Нельзя продолжать прежний опыт ради аккуратного отчёта, review, cleanup или commit. Историческая цитата выше не является текущей командой остановить подготовку этой карточки.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Рядом показать остальные показатели и ухудшения.** Запиши отдельно `observed_metric_success` и `causal_attribution`: одиночная пара не устанавливает устойчивость wall/CPU/peak, но это не обнуляет положительную измеренную дельту. Успех — кандидат для решения Любы, без автоматического исправления production. Не выдавай статический расчёт за измерение.

Исполнитель/последовательный проверяющий — модель своей сессии по умолчанию, без override или поиска другой. Student — собственный native `XIIGEN-native-1block-SFT-LoRA-64`. Роли CODEX / Astra 6 ultracode для спецификации/выбора и проверки качества не заменяют default исполнителя и native ученика; внешних calls здесь0. Новые DPO-пары не генерируются, китайская open-модель здесь не вызывается, DPO steps0. Используй уже заданные `trainable-architecture-decision`, `intent-to-plan`, `plan-self-validation`, `generated-code-review`, `experiment-design`, `prompt-patch-authoring`, `human-readable-architecture-audit` skills и `human-communication-GLOSSARY` из `docs/skills`; полный список имён в карточке. Отчёт по-русски; другой формат/язык/модель/скилы не искать.

## Механизм и единственный фактор

Native `Trainer.TrainSupervisedNextTokenStep` вызывает Transformer forward; `CausalSelfAttention.Reshape4D/Reshape3D` вызывают rank4 `TensorOps.Transpose(...,1,2)`. Текущий generic path выделяет `int[4]` для каждого scalar через `FlatToIndices`, а connected backward вызывает transpose обратно. A — исходный mapping; B — арифметический offset из карточки **только для rank4 axes1↔2**. Другие формы/оси и существующие safeguards сохраняются.

**Не включать frozen-weight transpose cache.** `Linear.cs` — неизменённый core669. Не добавлять RMS indexing, RoPE cache, MatMul-locality, buffer pooling, SIMD, objective/mask/tokenizer/optimizer changes. Сохраняются buffers, float copy order, Tensor graph, accumulation, shape checks, cancellation/progress cadence. Experiment switch и одинаковые scalar counters — только в копии `TensorOps.cs`; это допустимое исключение опыта, не разрешение для prod/submission.

## Место, версия, подготовка и команды

Фиксированный attempt — **`C:/x/at01/a001`**. Если существует — `ATTEMPT_EXISTS`, сохранить и закончить, без удаления, продолжения, автопереименования или retry. Source core — `C:/Projects/llm_mvp_core`, commit **66947785d1318f7cbbc88118f3156cdc75001e16**. [Manifest](ATTN-TRANSPOSE-STRIDES-01-SOURCE-MANIFEST.json) содержит ровно36 source paths с blob SHA; экспортируй `git show <pin>:<path>` с сохранением stdout bytes в `src/<path>`, original checkout не менять. Поздняя reconciliation core827 не меняет этот source pin; шесть source поддеревьев сверены отдельно. Состав унаследован от существующего frozen-transpose host, не его patched source.

Шаблоны Program/Runner/run.py и все permalink заданы в карточке, planning pin53cbcabd3926cda714bab428d294eb666301e9e4. Скопируй их в плоский attempt, убери frozen cache обращения/readouts; ID=`ATTN-TRANSPOSE-STRIDES-01`, assembly такое же, LoRA stream=`attention-transpose-probe-v1`. Новый Program/runner **нужно реализовать по этому заданному diff**, не вызывать отсутствующий entry и не искать другой проект. `Runner.csproj`: net9, only36 copied sources+Program, no ProjectReference, no default Compile, parent Directory.Build imports выключены до SDK imports, bin/obj только внутри attempt.

Program: отдельные процессы A/B, один Student и optimizer в каждом. Для A0/B0 switch=false; непосредственно перед TRAIN switch=`arm=="B"`, далее сохраняется до A1/B1. Native model config/seed/updater/dataset копировать из полной карточки буквально. Зафиксируй manifest последним: hashes Program, project, run, compare, one-factor.diff и36 copied sources; `one-factor.diff` только переключатель/счётчики/ранг4 ветвь. На preparation не создавать results/bin/obj и не запускать build.

После готового preparation выполнить ровно:

```powershell
Set-Location 'C:/x/at01/a001'
python ./run.py
python ./compare.py
```

Новый no-argument run.py использует literal root/Runner/results paths и jobs `[('A','code-sign'),('B','code-sign')]`; checksums один раз до build, результат эксклюзивно. Один `dotnet build Runner.csproj -c Release --output bin` с отключёнными parent props/targets, timeout120s. Затем `dotnet bin/ATTN-TRANSPOSE-STRIDES-01.dll A code-sign results/A/code-sign.json` и B аналогично, каждый60s, общий native deadline120s. Конечный JSON открывается `FileMode.CreateNew` **до модели и обучения**. Все logs/status сохраняются по путям карточки. Не собирать solution/dispatcher, не запускать tests/CI. Ошибка/timeout заканчивает attempt, неизвестные counts — null; build failure означает, что обучение не началось. Не устанавливать toolchain и не выбирать fallback project.

## Бюджет, измерения и сравнение

C# integer-sign: TRAIN `C# result sign: 9-4 =`→`+`, `C# result sign: 4-9 =`→`-`; EXAM `C# result sign: 8-3 =`→`+`, `C# result sign: 3-8 =`→`-`. **4 native update/arm, T1,T2,T1,T2;8 total.** Full-sequence SFT, UTF8 byte IDs256, no EOS, no merges; frozen base seed62001, LoRA q/v rank4 alpha8 dropout0 seed62002; native AdamW .9/.95/1e−8/WD0; LR.001, warmup0, clip1. Не добавлять уроки/seed/warmup/retry/EXAM training. Docs и другие домены — `NOT_MEASURED`, расширять панель запрещено.

В A0/A1/B0/B1 по2 TRAIN и2 EXAM forward, **16 диагностических forward total**, без backward. Из этих же результатов вычисли per-row target NLL/rank/argmax exact и сохрани256 last logits; для проверки exact optimization — shape/full float32 bits logits, параметров и moments. Не делай дополнительных model calls ради метрик. TRAIN cost — actual bytes, wall, CPU, GC вокруг четырёх updates, snapshots/diagnostics/serialization снаружи. Readout allocation bytes по четырём состояниям — отдельно от TRAIN. Process peak до after-readout пометить как scope всего процесса. Запиши actual optimizer steps4, real parameter write, unchanged frozen,4 online pre-step losses, eligible transpose calls/elements в TRAIN.

`compare.py` только читает raw и создаёт comparison.json: A0=B0; A1=B1 full float bits/model states/moments;4 loss bits; ΔA,ΔB,ΔB−ΔA по TRAIN/EXAM отдельно. Bit mismatch показать как неподтверждённую exact optimization; сохраняй наблюдаемые улучшения/ухудшения, не скрывай изменение поведения и не начинай новый ремонт/опыт. Отсутствующие значения «не измерено». Native update и реальная запись параметров обязательны для вывода об обучении.

## Завершение

`results/report.md`: русский Markdown, **до350слов плюс таблица**, разделы и относительные ссылки уже заданы карточкой. Отдельные таблицы качества A0/A1/B0/B1 и стоимости TRAIN A/B; все ухудшения рядом. После любого измеренного полезного улучшения — `results/fix-candidate.md`, до120слов: method/diff, полезная ось и домен, дельта/ухудшения, attribution, `[report](report.md)`, статус кандидата для Любы. Если успех не измерен, не создавай успешную рекомендацию. Не менять protected/prod, не удалять прежние опыты.

Один раз прочитай собственный report; исправляй только фактическую ошибку/ссылку/кодировку и закончи. **Команда пользователя стоп/следующий отменяет даже это завершающее чтение**. Не делать повторных hash-review, новых тестов или rerun для улучшения отчёта.
