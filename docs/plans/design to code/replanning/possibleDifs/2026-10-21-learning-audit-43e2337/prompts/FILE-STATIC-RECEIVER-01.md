# Промпт будущему исполнителю: FILE-STATIC-RECEIVER-01

**Повтор уже сохранённого результата запрещён.** Сначала прочитать назначенные receipts/outputs; пригодный результат использовать автоматически без вопроса «reuse или retrain». Отсутствие полного снимка не разрешает новый warm-up, перенос другого donor либо восстановление missing native moments/RNG по догадке. Consumed опыт не получает дополнительных model calls.

Этот промпт предназначен для отдельного поручения **реализовать и исполнить этот ID**. Текущий аудит документный; сам факт наличия промпта не разрешает запуск. Единственный authoritative recipe — [карточка FILE-STATIC-RECEIVER-01](../cards/FILE-STATIC-RECEIVER-01.md), [общий контракт](../06-execution-contract.md) и [реестр](../registry.json). Карточка имеет статус NOT_RUN; runner ещё не существует и не объявлен проверенным.

Используй модель текущей сессии по умолчанию. Язык общения и отчёта — русский; код/API/IDs не переводить. Никакого поиска другого исполнителя, внешнего judge/teacher или переключения модели. В `C:/Projects/llm_mvp_core/docs/skills/` прочитай назначенные файлы: `architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `learning-path-audit-SKILL.md`, `learning-signal-capture-SKILL.md`, `experiment-design-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`, `output-readability-gate-SKILL.md`. Точные новые пользовательские правила о маленьком опыте, полезной метрике и отсутствии prod-цепочки имеют приоритет над старыми thresholds/Tier escalation и модельными ограничениями skills.

## Задание и границы

Исключить только resolved static IFieldSymbol из hasMemberAccess при построении учебной метки. Native kNN k3/30SyntaxKind features/порядок/reader неизменны; это direct Train памяти, не LoRA proxy.

Core source pin `43e2337d68dad0e4ab8d7fbb0cea38406b592b61`. Реализовать только source-copy host **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FSR01** и назначенные экспериментальные outputs. Старые результаты, product source/tests, защищённая `docs/plans/replanning-possible-diffs` неизменны. Не чинить всю solution/CI перед доказательством гипотезы. Experimental source-copy разрешён; production integration не разрешена.

Точный вход `fixtures/file-static-receiver.json`, SHA256 bc4a70298103d95728c0aa69211cbf5d93ac0dcc3babd7d36b163c43a27d1c69. Common6 один раз: TrainingSize6, plain3/3 и instance3/3; сохранить ordered memory.json. Два независимых restore6; затем native Train(common6+static3). Collision Q-CS/Q-CI только читаются. Не считать переименования независимыми structural families. Gold ADD_METHOD — остаток taxonomy; S1..S3 обязаны bind-иться в static fields, иначе BLOCKED_SEMANTIC_BINDING.

Ровно5 Train calls (1common+2restore+2update),36 переданных пар памяти и55 Predict;0neural updates,0teacher. Common и pair costs отдельно. Отчёт `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FSR01/a001/results/report.md`, raw `result.json`, common receipt/state и A/B readouts. Предел build180s+runtime120s, общий360s.

## Единственный entry после подготовки

```powershell
C:/Python314/python.exe C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FSR01/run.py --attempt a001 --fixture C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/FSR01/fixtures/file-static-receiver.json
```

Реализовать именно wrapper/arguments/project OutputPath/guards из карточки. До build и любого log redirect проверять existing claim/results; create-new owner один раз. Занятый слот читать как issued, не очищать, не выбирать a002/results-NN. Неполный исход — сохранить actual/null counters и stop; не превращать непостроенный host в нулевой учебный результат.

## Ожидание и оценка

До запуска найди доступный execute и его native wait/resume по описанию. Запусти единственный entry, сохрани handle, дождись terminal/deadline **в текущем ходе работы**. Используй тот же native handle, блокирующие ожидания до60секунд; не заканчивай раунд словами «жду», не перезапускай, не заводи shell sleep/polling loop, watchdog/monitor или параллельный отчётный процесс. Отсутствие файла не означает падение. Если инструмента ожидания действительно нет, запиши точную ошибку BLOCKED_TOOL_CAPABILITY и handle; не придумывай лимит среды.

Измеряй настоящее изменение learned state и его чтение, а не schema/metadata-only. Покажи A0/A1/B0/B1, per-row gold/output, fixed class/negative controls, константный baseline, actual updates/readouts, подготовку отдельно. `0→0` правильных учебных результатов не спасается сдвигом margin. Любое измеренное улучшение полезной метрики считается успехом именно этой метрики/темы; роста общей accuracy не требуется. Все ухудшения рядом; один timing sample не доказывает универсального speedup. При полезном эффекте — документный кандидат с исходным code symbol/diff/evidence/ограничениями; перенос решает пользователь.

## Немедленный STOP/NEXT

По прямому STOP немедленно прекрати действия. По прямому NEXT прекрати предыдущую задачу и переходи по указанному стабильному ID; запрещено перед этим дописывать прошлый отчёт, чинить скрипт, делать commit, обновлять память, запускать проверку или повторять опыт. Сообщи только уже известный статус и handle без новых вызовов ради уточнения. Следующий ID программы: **RMS-MEAN-BACKWARD-01**. При поручении только этого опыта после его результата — **END**; dependency не разрешает самовольно запускать producer. Не продолжай старую задачу ради «завершения хвостов».
