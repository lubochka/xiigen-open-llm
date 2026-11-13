# Промпт исполнителю SC01

Этот промпт применяется **только после явного назначения одной фазы** из REGISTRY общего пакета: строка A1 после исходных №09–40, перед read-only №41–42. В текущем запросе выполняется документирование; запуск обучения, создание runner и правка production не назначены. Документ не превращает отсутствующий host в исполняемую команду.

```text
CURRENT TASK
Выполни только назначенную фазу DPO-SHIFTED-CONTEXT-01 (SC01): Prepare,
Calibrate или Compare. Если фаза не назначена, работай только с документами
и верни текущий PREPARATION_REQUIRED. Не спрашивай повторно GitHub permissions.

EXECUTOR / LANGUAGE / SKILLS
Executor — модель по умолчанию текущей сессии, без model override.
Отчёт на русском. CODEX Astra 6 ultracode выполняет selection и DPO quality
evaluation; внешний открытый китайский model, если отдельно нужен новый
generated corpus, только генерирует. Для нашего фиксированного mechanical
simulation fixture внешняя модель не вызывается, чужое authorship не выдумывается.
Прочитай docs/skills/generated-code-review-SKILL.md,
docs/skills/training-data-gap-audit-SKILL.md,
docs/skills/prompt-patch-authoring-SKILL.md,
docs/skills/trainable-architecture-decision-SKILL.md,
docs/skills/intent-to-plan-SKILL.md,
docs/skills/plan-review-SKILL.md,
docs/skills/human-readable-architecture-audit-SKILL.md.
Текущие требования пользователя к изолированному эксперименту и STOP/NEXT
приоритетнее старых production-only workflow правил. Не запускай полный
pipeline, UI certification, CI или repair tests ради этого маленького опыта.

READ EXACTLY
01-DPO-SHIFTED-CONTEXT-01.md
02-DPO-SHIFTED-CONTEXT-01-CARD.md
04-SC01-FIXTURE.md
Оригинал core: dd150aa5216e42b8a8bba14955337398ec5f8397.
Protected docs/plans/replanning-possible-diffs — READ ONLY.
Не переписывай старые карточки и не удаляй старые эксперименты.

FIXED LOCATION
Host C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01
Local source copies C:/Rp01/SC01-src
Result C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/results/attempt-0001
Никакого поиска «куда поместить», случайного temp path или смены каталога.
Prepare единожды создаёт attempt-0001/manifest.json и subdir prepare.
Root уже существует на Prepare — BLOCKED_OUTPUT_EXISTS. Calibrate/Compare
допускают existing root только с точным manifest ID/SHA/fixture/donor и
создают новую subdir calibrate/compare. Уже занятая phase — BLOCKED_PHASE_EXISTS.
Не стирай lock/status/build.stdout, не переписывай previous phase records
и не создавай attempt-0002 сам. Calibrate/readiness.json выдаёт READY_COMPARE
только со ссылкой на успешный неизменный prepare/readiness.json.

BEFORE ANY TRAINING
Сейчас статус PREPARATION_REQUIRED. В этой audit-сессии получены только LFS
pointers donor weights/optimizer, не payload. Проверь exact LFS hash/size
из карточки. Не выдавай pointer bytes за модель.
Донор RESTORE-MINIMAL-SET snapshot1000: tiny C# byte-level model,
weights SHA ba8ec8c20250bc431da4bf9993ff22c508422cd7224b5c57cec33ce7b98c6e0e,
43500 bytes. Exact config/LoRA/BPE — из карточки, не RP01 large B0.
Нет payload — зафиксируй DONOR_PAYLOAD_UNAVAILABLE, без random fallback,
неявного warm-up или повторного 1000-step обучения.

SFT optimizer1000 не выдавай за DPO continuation: оба плеча получают warm
weights1000, frozen warm reference, явно новый DPO optimizer0 с одинаковыми
native defaults. Новый SFT→DPO переход общий, не фактор A/B.
Native restore и anchors РОВНО3/40 нужно реально подтвердить (integer exact);
state hash и заранее фиксированные NLL/gradient tolerances из карточки
не ослаблять после результата. Positive anchor не заменяет semantic terminal
correctness SC01. Terminal0→0 — NEEDS_CALIBRATION/NO_TASK_PROOF, даже при
NLLgain; полезная NLLдельта остаётся успехом этой оси. Terminal0→1 допустим.
Terminal1→0 показать регрессией, не сохранением навыка.

Старый raw report — evidence чужого завершённого запуска, не новый запуск.

EXACT FACTOR
A: original full carrier T and context validation T<=64.
B: полный immutable carrier IDs/masks/admission T сохраняется;
   forward input prefix T-1<=64; scorer/backward используют ВСЕ T-1 logits
   с targets ids[1..T-1]. Последний target сохраняется.
Четыре forward policy/reference используют одинаковый extent.
Не добавляй PAD per-side crop, response-only LM head, LR sweep, optimizer
memory transfer, иной tokenizer, новый sampler или иной recipient.
Все edits только в новой experiment copy; product original не менять.

FIXTURE AND VALIDITY
Используй ровно два JSONL набора с SHA из 04-SC01-FIXTURE.md.
Boundary pairs65 отличаются только target64, последней смысловой цифрой.
Control pairs64 отличаются только target63.
При ошибочном потерянном target весь contrast пропадает — это FAIL.
Проверь native BPE256/no merges/roundtrip/actual masks, gradient последней row.
Anchor first20 response tokens One/Two показывает ненулевую донорскую память;
не называй это fullResponseExact. EXAM и CONTROL не обучаются.

PHASE CONTRACT
Prepare: materialize назначенный узкий host из точных donor layouts,
restore/check/readout; НИКАКОГО скрытого обучения.
Calibrate: только variant A (original full-input), ровно200 DPO updates
суммарно на control64, batch1, alternating One/Two (100 каждая); B не обучать.
Before/after report обязателен. Старый SFT budget200
не доказывает DPO sufficiency. Нет learnable signal — PREPARATION_REQUIRED,
не увеличивать budget/LR автоматически.
Compare: только после READY_COMPARE; A0 expected capacity refusal
с точным trace (bridge validation может отказать ещё до StepVector),
B ровно200 updates на boundary65; start weights1000 заново и одинаково.
Не используйте calibration state как скрытый более обученный B0.
Заверши один phase и вернись в реестр; не запускай все phases автоматически.

MEASURE
Только показатели, зависящие от actual training, его загрузки или результата.
Общий diagnostic scorer одинаков для всех before/after и не меняет веса.
Online DpoMetrics из step НЕ after-update; пересчитай на обновлённой policy.
Запиши terminal probability/NLL/rank, two-candidate preference и ties,
all-vocab tail top1, anchor /40, teacher-forced response exact /2, EXAM/CONTROL,
free-generation exact=N/A (декодирование здесь не измеряется),
actual optimizer updates, LoRA delta, frozen hashes, elapsed/CPU/alloc/GC.
A0/B200 — unblock, не speedup двух одинаковых обучений; A per-update=N/A.
Любое измеренное улучшение полезного показателя считается успехом именно
этого показателя и домена; рост accuracy НЕ обязателен. Рядом все регрессии.
0/0 semantic terminal correctness не обходить положительным anchor:
это не доказательство решённого подответа. Для полного SC01 learning proof
требуется positive terminal after и positive learned anchor before,
рост accuracy не обязателен. Старые resource-success этим не стирать.

REPORT / CANDIDATE
Все выходы только в новой subdir назначенной фазы внутри папки попытки:
status.json, source-manifest.json,
fixture-manifest.json, before.json, after.json, per-token.jsonl,
update-trace.jsonl, report.ru.md, candidate-fix.md.
Отдели source-confirmed, imported historical measurements и measured-now.
При полезном эффекте оформи candidate с exact original files/functions
DpoExactPairMaterial.Validate, DpoTrainer forward/scorer/backward, profile
execution semantics. В оригинал ничего не переносить: решение пользователя.

STOP / NEXT — БЕЗ ИСКЛЮЧЕНИЙ
На «стоп» немедленно прекратить, без нового tool call/commit/cleanup.
На «следующий эксперимент» не продолжать прежний ради отчёта, проверки,
перезапуска, сохранения lesson или улучшения упаковки. Выдать короткий
точный статус и перейти только к следующему назначенному ID реестра.
Не повторять завершённый №08 из-за устаревшего index; актуальный выбор
берётся из reconciliation текущего пакета, не из старой NEXT08 надписи.
Нельзя молча повторить попытку, поменять model/budget/domain или удалить
старый experiment. Один результат — один отчёт — остановка выбранной фазы.
```

## Проверка промпта при документировании

У промпта задано место, модель исполнителя, языки, данные, source SHA, donor identity, конечный budget, оригинальный код возможного исправления, separation SFT/DPO optimizer, алгоритм before/after, правила успеха/регрессий и STOP/NEXT. Он не утверждает, что неизвестные native payload или host уже готовы. Продолжение исследования после аудита остаётся явно назначаемой фазой, а не скрытой частью документирования.

