# 80. План исправлений после KNN repairs и P03–P05

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN/METRIC:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно. Различать шкалу, знаменатель, trajectory mean, last-pre-update и final checkpoint; unknown не0. Отрицательный свидетель — одинаковый итог при gained/lost либо подмена final средним; положительный — те же raw дают правильный построчный вывод. Пересчёт сохранённых raw — отдельная проверка измерителя сTrain0; он НЕ заменяет обязательные updates/учебный цикл данной карточки. Новое неназначенное вспомогательное обучение не добавлять; source proof, научный запуск и formatter-проверка имеют разные статусы.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


**Текущее дополнение, 28.10.2026:** [87 — аудит P05/P10–P12](87-gradient-warmup-transfer-audit-2026-10-28.md), [88 — точные исправления](88-gradient-warmup-transfer-repair-plan-2026-10-28.md), [89 — применимость ко всем 62 ID /56 владельцам](89-gradient-warmup-card-coverage-2026-10-28.md). Основной writer P05 исправлен cb3c9dc, его учебная цель остаётся OPEN; новые остаточные дефекты относятся к целостности панели и terminal gate. P10/P11 подтверждают существующие native механизмы, не новый общий алгоритм. P12 — кандидат узкого ремонта первого SFT шага с импортированными AdamW400/scheduler0, не глобального scheduler shift. Перенос P12 в native ещё не выполнен. Требования текущего дополнения имеют приоритет над историческими статусами ниже; очередь выполненных опытов не переоткрывать. Новые тесты/обучение этой документной работой не запускались.


Основание — [79](79-followup-transfers-and-learning-audit-2026-10-27.md). Это план, не запуск и не реализация product. Не повторять оплаченные updates ради исправления отчёта. Новых дублирующих FIX-ID нет; владельцы KDP/KDR —74, измерители —77, защита рецептов —81.

## Порядок

1. Исправить квалификацию и reader P05; сохранить raw и честно разделить execution/training/learning.
2. Продолжить полный restore и quality oracle KDP по74/77; реализованные K1 и fail-closed capture не создавать повторно.
3. Довести producer P02: staged helper не отменяет remaining SftWallMs=0d.
4. Применять исправленные DPO настройки, контракты reader и актуальную навигацию во всех связанных постановках.

## R28-P05-A. Исправить отчёт по уже выполненному обучению

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** METRIC по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Будущие файлы изменения: `experiments-v2/RP01-MICRO/P0501/Program.cs::Readout/validity/DecideHypothesis`, отдельный postprocess рядом с host и новый output `results/attempt-0001/audit-correction-79/`. Старые before/arms/after/metrics/status/report не перезаписывать. Текущий план не создаёт postprocess и не выполняет его.

В receipt раздельно хранить real_training_performed=true, completed_updates=8, квалификацию каждого consumer/группы и валидность сравнения. Delta/hash/optimizer counter — доказательства изменения состояния, а не достаточное условие learned validity. Старую полную панель0/8 сохранить. Отдельно прочитать сохранённые первые символы donor R-T1/R-T2 для S0/A4/B4 как восстановление **старого однознакового reader**; это не новая оценка восьмизнакового ответа и не доказательство освоения слов positive/negative.

Бюджет исправления текущего отчёта:0 model calls,0 updates. Если raw не позволяет восстановить токен однозначно (например знак замены UTF-8), записать UNKNOWN, не угадывать byte ID. Уже записанный ASCII '+' достаточен для указанного узкого факта. При необходимости отдельного явно порученного recovery — только S0 и два сохранённых after-update04 checkpoint, donor2 строки×3 состояния×1 generation token=6 Forward,0 optimizer updates. Это чтение ранее выполненного учебного участка, не новый безучебный эксперимент.

**Тесты будущего writer:** взять сохранённые записи P05 и вызвать действующий builder: (1) updates>0, hashes различны, но все ответы0 — не VALID; (2) donor '+' с raw '+x…' — ошибка полного8-token reader, но отдельный прежний1-token reader видит '+'; (3) Unicode replacement не раскрывает исходный token ID; (4) terminal receipt не закрывает learning goal. Не тестировать вручную написанный expected JSON вместо builder.

WRONG: переименовать0→0 в успех из-за сдвига весов или оценивать новый ответ до длины gold. CORRECT: фиксированный контракт потребителя до измерения, квалификация по группам, отдельные признаки реального обучения и его полезного результата.

## R28-P05-B. Полный повтор — отдельная квалифицированная постановка

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN/METRIC по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Простой повтор старого entry запрещён: он воспроизведёт ошибочный consumer и тот же нулевой старт. Ниже зафиксирован ограниченный дизайн для будущего прямого поручения; в текущую автоматическую Q-очередь он не включён, код не существует и готовность запуска не заявляется. Автор регистрирует этот ремонт как отдельный ID до передачи исполнителю; исполнитель не выбирает иной набор/модель/seed самостоятельно.

Назначенный host: `experiments-v2/RP01-MICRO/P05Q01/`; entry после реализации — `pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P05Q01/Run-Experiment.ps1`; output `results/attempt-0001/`. Сохранить byte-identity256 и H64/qv rank4 alpha8 dropout0 из P05, тот же donor S0 и его weights/RNG identities. Этот дизайн явно изменяет precondition и termination; его нельзя выдать за повтор старого результата1:1.

**TRAIN:** четыре прежних P05 пары с теми же prompts/сторонами плюс завершающий LF в каждой стороне: '+\n'/'negative\n', 'negative\n'/'+\n', 'positive\n'/'-\n', '-\n'/'positive\n'. LF — byte10, объявленный EOS только этого рецепта, включён в response mask; длины2 и9. EXAM — прежние два запроса7−3/3−7 с допустимыми знаком или словом, каждый с LF. Donor retention — две исходные строки со знаком и LF. Gold не передаётся генератору, EOS одинаков для всех строк, maxNewTokens9; потребитель требует реальный LF, снимает ровно его, не обрезает по длине gold.

**Общий учебный S1:** восстановить S0 weights/RNG. Отдельный SFT epoch с fresh moments/step0, LR.001, decay0, clip1; native SFT по round-robin шести строкам (4 выбранных ответа TRAIN +2 donor) с LF. Максимум400 updates, квалификация каждые20; полный greedy reader должен выдать все6 правильных завершённых ответов. Первая подходящая точка становится S1; если лимит исчерпан, terminal unqualified и A/B не начинается. Нельзя искать другой checkpoint или увеличивать steps. Сохранить S1 weights, typed optimizer, RNG, tokenizer/EOS declaration, данные, reader и квалификацию; один раз подготовленное S1 далее переиспользовать по этим identities.

**A/B:** две копии S1, общий immutable reference S1. Новая DPO optimizer epoch с одинаковыми fresh moments/step0 (это назначенное отличие от старого P05, не скрытая потеря состояния). Четыре updates каждой руки на исходных4 парах; A sum, B mean, один denominator в objective/derivative; beta.1/LR1e-5/native decay.01/no clipping. DPO masks включают LF. Exact experimental objective version отделён от product default. Контроль формулы использует уже сохранённые logits/tensors, без новых model calls/updates. Он не доказывает parity полного native шага: эта граница остаётся NOT_MEASURED и закрывается отдельным назначением перед product переносом. Дополнительный native backward/update для parity этим бюджетом не разрешён.

Назначены ровно4 факторные панели: before A,after A,before B,after B; каждая содержит4 TRAIN+2 EXAM+2 donor строки. В каждой строке оцениваются ровно4 кандидата: `+\n`, `-\n`, `positive\n`, `negative\n`; обе свёртки берутся из одного Forward каждого кандидата. Получаются32 generation до9 токенов и128 candidate-scoring Forward. Эталон правильного ответа не управляет набором кандидатов или остановкой.

**Предел всей программы:** ≤408 updates (400 SFT+8 DPO), ≤464 Backward, ≤2400 model Forward, включая qualification/generation/scoring/reference и проверку сохранённого S1. Это верхние пределы, не заявленные фактические counts; trainer/consumer receipts считают реальные вызовы. Warm qualification: максимум20 панелей×6×9 Forward плюс initial/restored панели; факторная часть сохраняет8 DPO updates/32 пары/≤128 training Forward/64 Backward. Build≤600s, runtime≤1800s; достигнутый лимит не расширяется. Контроли на сохранённых logits используют0 model calls.

Проверяемая верхняя сумма заданных Forward:400 warm +1080 qualification +108 initial/restored шестистрочных панелей +128 DPO +288 factor generation +128 candidate scoring =2132, ниже2400. Оставшиеся268 — запас потолка, не разрешение на новые шаги или дополнительные панели; любой незаданный model call — расхождение рецепта. Все464 Backward уже распределены, скрытого резерва для full-step parity нет.

Сравнение: точные TRAIN/EXAM/retention ответы и завершение, обе sum/mean оценки на одних кандидатах, per-row margin и затраты; этапные S0→S1 и S1→A/B раздельно. Любой измеренный полезный сдвиг сохраняется с регрессиями; малый процент не объявляется нулём. Устойчивость и численная погрешность — отдельные свойства, не придуманная граница после результата. Без успешной квалификации S1 новый опыт не имеет права называться завершённым учебным сравнением.

## R28-OPT. Паспорт фактического оптимизатора

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN/METRIC по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Источники: `11.DPO/DpoTrainer.cs` DefaultWeightDecay381, CreateDefault428/505, ApplyOptimizer1299; SFT — отдельный `04.TrainingLoop/Trainer.cs`. Исправлены21 общих ложных DPO defaults и ещё одна форма в P122 (22 карточки); в P01 дополнительно разделены continuation state и effective configuration. При будущем назначении фиксировать effective values после construction/restore и до первого update: LR/beta/epsilon/decay/clipping/moments/step/allowed keys. Если реально полученная конфигурация не соответствует назначенной, не продолжать обучение молча. Существующий результат с одинаковым отклонением обоих плеч сохраняется с точной границей, а не объявляется идеально выполненным паспортом.

Контроль для тестового builder/config receipt — DPO .01/no clipping и SFT0/clip1 должны остаться разными. Пустое поле clipping означает unknown, а явное 'not applied' — проверенный путь. Нельзя менять native defaults в продукте ради устранения документной опечатки.

## R28-KDP. Остаток после исправлений

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN/METRIC по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Прочитанное исправление K1 сохраняет допущенный descriptor и копирует параметры; прежние обходы из76 не надо чинить повторно. Отказ Capture configured catalogue закрывает молчаливую потерю режима, но **не** задачу полноценного save/load. Продолжать77/R27-K2: versioned serialized config, digest/registry resolution, unknown version refusal, actual cold restore и consumer. Продолжать77/R27-K3: точный oracle правильного RU/OR ответа после native Train; Contains(memory) и неизменность после атаки этого не доказывают. Сохранить исходный лимит74 (13 native Train/105 предъявлений/≤960 чтений), не создавать Train для каждого assertion. Подробная сверка закрытий и логов — независимый review79.

## R28-P02/NAV. Не закрывать producer индексом и не возвращать очередь назад

**Census selector/receipt:** точное имя в docs/TESTING.md исправлено на StaysAtSeven. Следующий прямо порученный test run должен доказать discovery и ненулевое число выбранных ожидаемых тестов; успешный exit при0 тестов не приёмка. Для обещания полного census изменить Tests/LlmIngredientLogicalUnitTests.cs на проверку constructor semantics и фактических Compile items либо честно ограничить гарантию canonical literal формой. Контроли: target-typed new, alias, перенос строки, новый compiled file под results, изменение author/materializer после неизменной constructor-строки. Эти статические/unit проверки не являются новыми учебными опытами. Каждый настоящий run/mutation получает отдельный create-new log/TRX/manifest с SHA/filter/UTC/exit и per-test pass/fail/skip; арифметика totals обязана сходиться. Не повторять обучение для восстановления потерянного лога.

103c8c50ce1 — только поправка индекса. Параллельный staged PreferenceSeparationReport проверять по будущему завершённому изменению; он не выдаёт историю SFT wall time. `Program.cs::RunArm` всё ещё возвращал0d при чтении этого аудита. Продолжить77/W1: nullable/NOT_MEASURED, отдельные timing boundaries, чистые тесты builder; существующие12 updates не повторять.

При записи результата обновлять в одной документной операции строку Q,00-README, вход00-execution-index и промпт P-SERIES-CONTINUE. Сейчас первый неисполненный PRIMARY Q008/P07; Q006/P05 имеет отдельный ремонтный остаток, который не разрешает повторять attempt-0001. Приоритет прямого ID/STOP/NEXT Любы сохраняется. Terminal execution, learning qualified, hypothesis outcome и eligibility for product — четыре разных состояния.

## Контракт будущего исполнителя

Прочитать owner,03/69,79/80, project skills architecture/planning/review/readability; отчёт по-русски. Модель операционного исполнителя — его текущая разрешённая модель по умолчанию; исследователь Astra не запускается как runtime executor. Ждать исходный process handle до terminal/deadline, не дублировать запуск и не заменять ожидание финальным ответом. Старые результаты не удалять. Реализация и запуск выполняются только по отдельному прямому поручению; этот аудит создаёт документы.
