# NEURAL-FROZEN-TRANSPOSE-01 — стоимость повторного transpose замороженных матриц при SFT/LoRA

## Паспорт и исходное поручение

**Цель:** проверить одним маленьким опытом, снижает ли повторное использование транспонированной frozen base расходы настоящего обучения при сохранении численной траектории. Предметный профиль — знак целочисленного вычитания в C#. Приоритет P1 после блокировок обучения. Модель ученика: `XIIGEN-native-1block-SFT-LoRA-64`; это сам C# Transformer, не модель исполнителя и не внешний judge.

Правила пользователя: измерять изменения в процессе обучения; использовать изолированную копию того же кода до production-исправлений; не запускать полную цепочку или продуктовые тесты; результаты передать Любе, которая решает, что переносить. **Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Рядом показываются изменения всех остальных измеренных показателей, включая ухудшения.** Старые эксперименты/результаты сохраняются.

Исходный commit core: `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`. Статус при создании: **`SCAFFOLD_MATERIALIZED_NOT_COMPILED_NOT_RUN`**. `dotnet` в среде документного аудита отсутствует. Подготовка исходников выполнена; обучающих запусков не было. Источники и его byte hashes лежат в `scaffold/source-manifest.json` и подготовленном `source-copy-manifest.json`.

## Связь с фактическим обучением и один фактор

Native путь: [`Trainer.TrainSupervisedNextTokenStep:62–150`](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/04.TrainingLoop/Trainer.cs#L62-L150) → `LlmModel.Forward(training:true)` → [`Linear.Forward:45–50`](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/00.TensorCore/Layers/Linear.cs#L45-L50) / [`LoraLinear.Forward:112–125`](https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/10.LoRA/LoraLinear.cs#L112-L125) → native CE/backward/clipping → native `AdamW.Step`. В A и B действительно меняются обучаемые параметры.

A каждый раз транспонирует Weight исходным кодом. B при `!Weight.Value.RequiresGrad` лениво создаёт transpose один раз на экземпляр Linear и использует его повторно. Меняется только экспериментальная копия `00.TensorCore/Layers/Linear.cs`; `one-factor.diff` сохранён рядом. Общая ветвь переключения и счётчики являются инструментированием host. Body A остаётся исходным. LoRA A/B обучаемые и продолжают пересчитываться каждый раз.

Это опыт **SFT/LoRA**. DPO steps = 0; ни exact-pair admission, ни DPO runner не обходятся. Применимость того же механизма к DPO следует из общего Linear, но эта карточка не измеряет DPO gain и не доказывает улучшение RP01 end-to-end.

## Модель, данные и состояние — выбирать ничего не нужно

| Поле | Фиксированное значение |
|---|---|
| Архитектура | Vocab256; Context64; Hidden64; 1 Transformer block; 4 Q heads; 2 KV heads; FFN176; RopeTheta10000; RmsNormEps1e-5 |
| Base initialization | `Initializers.UseDeterministicSeed(62001UL)` в каждом новом процессе |
| LoRA | `ApplySeeded`; rootSeed62002; stream `frozen-transpose-probe-v1`; q_proj/v_proj; rank4; alpha8; dropout0 |
| TRAIN | Native `TrainSupervisedNextTokenStep`, weight1, ровно 4 updates; порядок T1,T2,T1,T2 |
| Optimizer | Native AdamW beta1=.9, beta2=.95, eps=1e-8, weightDecay0 |
| Schedule | warmup0, totalSteps4, maxLr=minLr=.001, gradClipNorm1 |
| Токены | UTF-8 byte IDs 0..255; это представление BPE с нулём merges; EOS не добавляется |
| Формат SFT | Полная последовательность prompt+answer, обычный shifted next-token loss. Prompt masking не является вторым фактором |
| Исходное состояние | Независимая воспроизводимая инициализация A/B; hash frozen и trainable state до TRAIN |
| Глубина | 1 Transformer block, 2 LoRA adapter slots, 1 native learner; Flow отсутствует |

| Split | ID | Prompt — буквально | Answer — буквально |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Новые числа проверяют тот же маленький закон, но не доказывают широкую арифметическую генерализацию. Предсказанный token ID берётся из фактического argmax последнего логита, gold не подставляется вместо выхода. Ни C# compiler, ни ручное вычисление знака не выдают ответ ученика.

В A0/B0 выполняется ровно один readout каждого EXAM. Кэш тогда выключен **в обоих** плечах, поэтому B не получает бесплатное заполнение. Затем четыре TRAIN updates. В A1/B1 выполняется такой же EXAM. Получение hashes и readouts находится вне измеряемого участка TRAIN. В нём нет restore, создания новой модели между шагами, настройки по EXAM или изменения frozen base. Создание кэша оплачивается первым TRAIN шагом B.

## Что измеряем до/после

| Группа | Реальные данные |
|---|---|
| A0,A1,B0,B1 | Для каждого EXAM: все 256 последних logits, argmax token, exact, NLL правильного следующего байта |
| Фактическое обучение | 4 native loss значения; AdamW step=4; hash trainable before/after; hash optimizer moments after; признак actual parameter write |
| Основная стоимость TRAIN | `GC.GetAllocatedBytesForCurrentThread`, `Stopwatch`, process CPU с Refresh перед вторым чтением; GC0/1/2 |
| Цена памяти | Process peak working set до конца TRAIN отдельно от TRAIN allocations; удерживаемый float payload кэша отдельно от всего процесса |
| Механизм | Cache misses/hits за TRAIN; сохранение frozen state; идентичность конечных trainable/optimizer state и readouts A/B |

Сравнение качества: `ΔA=A1−A0`, `ΔB=B1−B0`, затем `ΔB−ΔA`; для NLL меньше лучше, для exact больше лучше. Стоимость относится к переходу A0→A1 и B0→B1, поэтому сравниваются **два реальных TRAIN участка**, не нулевая стоимость «до обучения».

Ожидается точное равенство native logits/весов/moments A и B при экономии allocations; это гипотеза, не заранее заполненный результат. Для заданной архитектуры исходники предполагают восемь frozen Linears и 32 их вызова за четыре шага; при такой фактической топологии B должен иметь 8 misses и 24 hits. Эта арифметика — ожидаемый механизм, а не измеренная метрика. Если расходится, сначала в отчёте указать actual counts; не добавлять повторный TRAIN.

`compare.py` признаёт причинное сравнение только при одинаковых начальных весах, TRAIN/EXAM/config и фактических четырёх parameter updates. Если этого нет — `INVALID_CAUSAL_COMPARISON_DIAGNOSTIC_ONLY`, без заявления об успехе. При валидных одинаковых исходных условиях любое полезное снижение стоимости или улучшение качества — локальный успех. Конечное численное/предметное расхождение показывается рядом и **не скрывает** измеренный ресурсный выигрыш.

## Назначенные каталоги и команды

Все пути ниже находятся **в planning-пакете**, core используется только как источник. Ни один файл в core не изменяется. Рабочая директория команд: `xiigen-planning/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit`.

Host: `experiments/linear-transpose/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/NEURAL-FROZEN-TRANSPOSE-01/Runner.csproj`.

Подготовленные копии: `experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/src/` — **36 точных canonical исходников**. Program находится рядом с src. Host компилирует только этот набор; нет `ProjectReference`, общего dispatcher или product test project. Инертный `Program.cs.template` в 35-папке не создаёт второй Main для глобального glob.

Исходники уже подготовлены. При будущем поручении на исполнение:

```bash
python3 experiments/linear-transpose/scaffold/run.py --results-root results/NEURAL-FROZEN-TRANSPOSE-01
python3 experiments/linear-transpose/scaffold/compare.py --results-root results/NEURAL-FROZEN-TRANSPOSE-01
```

`run.py` собирает ровно указанный host `dotnet build -c Release --output .../bin`, затем вызывает его DLL с буквальными `A code-sign` и `B code-sign`. Два отдельных процесса не смешивают state и peak memory. Результаты: `results/NEURAL-FROZEN-TRANSPOSE-01/A/code-sign.json`, `.../B/code-sign.json`, stderr/stdout, `execution.json`, `comparison.json` и человеческий `report.md`, который исполнитель добавляет после read-only сравнения.

`prepare.py` нужен только для повторного получения отсутствующего source payload из read-only core; он **не нужен для уже подготовленного пакета**:

```bash
python3 experiments/linear-transpose/scaffold/prepare.py --core-root C:/Projects/llm_mvp_core
```

На другой машине меняется лишь уже известный путь к core; место назначения назначено скриптом и не выбирается исполнителем. Каждый input проверяется по Git blob SHA. Изменение чужого Markdown HEAD не требует нового обхода, source mismatch требует назвать конкретный изменившийся файл. Скрипт не создаёт worktree, не пишет в core и не скачивает другую реализацию.

## Бюджет, конфликты и условие остановки

- Один build данного host: максимум 120 секунд; отсутствие SDK/ошибка сборки фиксируется, без установки случайного toolchain, решения solution-wide тестов или обхода большого проекта.
- Два процесса, каждый максимум 60 секунд, весь native run максимум 120 секунд; 8 TRAIN calls суммарно и 8 EXAM forwards суммарно. Никаких warmup-уроков, автоматического повторения seed или новых доменов.
- Если deadline исчерпан, сохранить partial outputs и указать `TIMEOUT_NO_RETRY`; отсутствие полного A/B не трактуется как эффект.
- Если `results/ID` уже существует, предыдущий результат не перезаписывается и автоматического нового запуска нет. Разрешённая операция — читать существующий результат и назвать конфликт пути.
- Если prepared source-copy manifest существует, использовать его; не вызывать prepare повторно и не удалять каталог. Если исходники копии изменены после подготовки, run останавливается с точным file mismatch.
- Указания старых карточек писать результаты в `core/docs/plans/replanning-possible-diffs` здесь неприменимы: эта папка защищена прямым заданием пользователя. Наш единственный result root приведён выше.
- `Linear.Forward` не virtual: запрещено заменять данную source-копию незадействованным subclass. Ни fallback «голая матрица вне TRAIN», ни pure serving run не является выполнением этой карточки.

## Что предложить после измерения

Человеческий отчёт показывает фактические A0/A1/B0/B1, расходы, learned state write, все выигрыши/потери, ограничение одного маленького профиля и ссылку на `one-factor.diff`. Кандидат будущего переноса — исходный `00.TensorCore/Layers/Linear.cs::Forward`, только для frozen weights с ясным lifetime/инвалидацией при restore или base mutation. Статический фактор проверяется без этого дополнительного production-проектирования; успех опыта не разрешает перенос, изменение tests, commit/push core или полную цепочку внедрения.

## Поздняя сверка соседних обновлений

Пакет учитывает [адресную проверку root, 06-CONCURRENT-UPDATES](../../06-CONCURRENT-UPDATES.md): core `62bcc8f → 7c5718` изменил защищённую документацию, а native source данного фактора сохранился. Новые OCT20-R1, OCT20-R2 и E-OPT-STEP относятся к другим операциям; здесь они не присваиваются и не смешиваются с единственным фактором.

Точный исполнительский промпт: [EXECUTOR-PROMPT.md](EXECUTOR-PROMPT.md).

`run.py` отключает parent `Directory.Build.props/targets` через MSBuild flags, отказывается собирать поверх существующих `bin`/`obj` и записывает отсутствующий SDK/таймаут сборки в `status.json`. Это изолированная сборка текущего host, без очистки прошлых artifacts.

Настройки `bin`, `obj` и NuGet project-extension path задаются в csproj **до** SDK imports; все они находятся в указанном artifact-каталоге, который проверяет preflight. Это уточнение оболочки сборки, не второго учебного фактора.
