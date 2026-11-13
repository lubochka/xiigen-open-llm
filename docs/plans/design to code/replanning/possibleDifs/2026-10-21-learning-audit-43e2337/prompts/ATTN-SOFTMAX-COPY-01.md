# Промпт будущему исполнителю: ATTN-SOFTMAX-COPY-01

**Повтор уже сохранённого результата запрещён.** Сначала прочитать назначенные receipts/outputs; пригодный результат использовать автоматически без вопроса «reuse или retrain». Отсутствие полного снимка не разрешает новый warm-up, перенос другого donor либо восстановление missing native moments/RNG по догадке. Consumed опыт не получает дополнительных model calls.

Этот промпт предназначен для отдельного поручения **реализовать и исполнить этот ID**. Текущий аудит документный; сам факт наличия промпта не разрешает запуск. Единственный authoritative recipe — [карточка ATTN-SOFTMAX-COPY-01](../cards/ATTN-SOFTMAX-COPY-01.md), [общий контракт](../06-execution-contract.md) и [реестр](../registry.json). Карточка имеет статус NOT_RUN; runner ещё не существует и не объявлен проверенным.

Используй модель текущей сессии по умолчанию. Язык общения и отчёта — русский; код/API/IDs не переводить. Никакого поиска другого исполнителя, внешнего judge/teacher или переключения модели. В `C:/Projects/llm_mvp_core/docs/skills/` прочитай назначенные файлы: `architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `learning-path-audit-SKILL.md`, `learning-signal-capture-SKILL.md`, `experiment-design-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`, `output-readability-gate-SKILL.md`. Точные новые пользовательские правила о маленьком опыте, полезной метрике и отсутствии prod-цепочки имеют приоритет над старыми thresholds/Tier escalation и модельными ограничениями skills.

## Задание и границы

В attention TensorOps.Softmax только last-axis initial-copy skip; max/exp читают исходный x, порядок float и backward прежние. RMS factor выключен; массивы результата остаются. Cancellation equivalence вне scope фиксировать как ограничение.

Core source pin `43e2337d68dad0e4ab8d7fbb0cea38406b592b61`. Реализовать только source-copy host **C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AC01/a001** и назначенные экспериментальные outputs. Старые результаты, product source/tests, защищённая `docs/plans/replanning-possible-diffs` неизменны. Не чинить всю solution/CI перед доказательством гипотезы. Experimental source-copy разрешён; production integration не разрешена.

Единственный donor `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/`: полный11-file bundle по карточке, `bundle.json` COMPLETE с SHA/length остальных10. Проверить learning_qualified и acquired полный однотокенный ответ, config/36source blobs/state parity. Consumer не запускает Prepare/Export; это два дополнительных consumers только planning-пакета, не изменение protected списка. При отсутствии — BLOCKED_DONOR_NOT_READY, common training=0; не искать checkpoint, не выполнять400шагов заново. Student V256/H64/L1/Q4/KV2/FF176, q/vLoRA4/alpha8/dropout0, seeds62001/62002, stream rms-row-reduce-probe-v1. Private RNG identity берётся из exact RD01 literals карточки, не из имени MB01/AC01. Restore AdamW400/m/v/RNG,4updates/arm→404; новая пара только из исходногоS0.

Тема одна: C# знак разности. T1 `C# result sign: 9-4 =`→`+`, T2 `C# result sign: 4-9 =`→`-`; EXAM8-3/3-8 с теми же формами.4updates T1,T2,T1,T2 на arm,8updates/8backward/24forward на пару;0Generate/DPO/teacher/retry/common learning. Отчёт `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AC01/a001/results/report.md`, raw A/B code-sign.json, state/receipts и итог result.json. Общий deadline600s, build180s, arm120s каждый.

## Единственный entry после подготовки

```powershell
Set-Location 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AC01/a001'
C:/Python314/python.exe ./run.py
```

Реализовать именно wrapper/arguments/project OutputPath/guards из карточки. До build и любого log redirect проверять existing claim/results; create-new owner один раз. Занятый слот читать как issued, не очищать, не выбирать a002/results-NN. Неполный исход — сохранить actual/null counters и stop; не превращать непостроенный host в нулевой учебный результат.

## Ожидание и оценка

До запуска найди доступный execute и его native wait/resume по описанию. Запусти единственный entry, сохрани handle, дождись terminal/deadline **в текущем ходе работы**. Используй тот же native handle, блокирующие ожидания до60секунд; не заканчивай раунд словами «жду», не перезапускай, не заводи shell sleep/polling loop, watchdog/monitor или параллельный отчётный процесс. Отсутствие файла не означает падение. Если инструмента ожидания действительно нет, запиши точную ошибку BLOCKED_TOOL_CAPABILITY и handle; не придумывай лимит среды.

Измеряй настоящее изменение learned state и его чтение, а не schema/metadata-only. Покажи A0/A1/B0/B1, per-row gold/output, fixed class/negative controls, константный baseline, actual updates/readouts, подготовку отдельно. `0→0` правильных учебных результатов не спасается сдвигом margin. Любое измеренное улучшение полезной метрики считается успехом именно этой метрики/темы; роста общей accuracy не требуется. Все ухудшения рядом; один timing sample не доказывает универсального speedup. При полезном эффекте — документный кандидат с исходным code symbol/diff/evidence/ограничениями; перенос решает пользователь.

## Немедленный STOP/NEXT

По прямому STOP немедленно прекрати действия. По прямому NEXT прекрати предыдущую задачу и переходи по указанному стабильному ID; запрещено перед этим дописывать прошлый отчёт, чинить скрипт, делать commit, обновлять память, запускать проверку или повторять опыт. Сообщи только уже известный статус и handle без новых вызовов ради уточнения. Следующий ID программы: **END**. При поручении только этого опыта после его результата — **END**; dependency не разрешает самовольно запускать producer. Не продолжай старую задачу ради «завершения хвостов».
