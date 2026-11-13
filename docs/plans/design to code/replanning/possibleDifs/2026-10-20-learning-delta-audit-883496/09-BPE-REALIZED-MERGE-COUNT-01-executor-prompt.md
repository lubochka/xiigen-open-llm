> **Согласование9d311:** S04 — единственный новый distinct factor этого пакета. Предшественник NG01, NEXT исходный16 IDENTITY-M1-01, затем исходный32 KN-ROUTER. SKNN — reference alias существующего AST, не ещё один запуск. Все данные/Train budgets/host/result-root этого BPE рецепта сохранены. Текущая фаза документная.

# Промпт исполнителю: BPE-REALIZED-MERGE-COUNT-01

**Текущая фаза — только документы; этот промпт не разрешает создавать runner, собирать проект или запускать обучение.** Будущее исполнение только после отдельного назначения BPE-REALIZED-MERGE-COUNT-01. Место S04 после NG01; NEXT исходный №16 IDENTITY-M1-01, затем исходный №32 KN-ROUTER. SKNN — reference alias существующего AST, не отдельный запуск. Ближайшим общим NEXT остаётся №05 N62-R2.

Твоя модель — **текущая модель сессии по умолчанию**, без поиска другой. Отчёт — **русский Markdown**. Прочти [BPE-REALIZED-MERGE-COUNT-01.md](08-BPE-REALIZED-MERGE-COUNT-01.md); все пути, fixture, A/B diff, бюджет и CLI уже выбраны. Не выбирай другое место, модель, предметную тему, vocabulary size или способ запуска.

Применяй точные навыки из `C:/Projects/llm_mvp_core/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Уже прочитанное неизменившееся не перечитывай формально. Полные продуктовые CI/promotion требования навыков не расширяют это поручение. Protected docs не редактировать. Старые опыты/результаты не удалять.

## Что проверяешь после назначения

Тема — язык программирования/документация, профили lambda и комментарии. Получатель обучения — native `Tokenizer.BpeTokenizer`. Source pin: `lubochka/llm_mvp_core@883496bca18113bf37e0afe683c75b05a0075048`. В `Train` CountPairs выбирает merge, `_merges/_vocab` меняются, Encode использует результат. Native KNN caller затем читает эти IDs при обучении своего индекса; полная KNN-цепочка сюда не входит.

**A** неизменённый tokenizer. **B** меняет только `CountPairs` на literal тело §4 карточки: для серии одинаковых IDs учитывает неперекрывающиеся пары, остальные частоты и порядок первых появлений сохраняет. Это новая проверка другой цели обучения внутри известного BPE-семейства. Не называй перекрывающийся счёт новым открытым bug; он уже описан в BPE-WEIGHTED-CHUNKS. Не примешивай weighted chunks/no-match-copy. Не исправляй продукт.

Fixed fixture §5: TRAIN `///////`, `x=>x`, `y=>y`, `z=>z`, `w=>w`, между строками LF, без последнего LF; vocab257. EXAM ровно `a=>a`, `b=>b`, `c=>c`, `/////////`, `q+r`. Не подбирать строки после readout. Расчёт из карточки предсказывает выигрыш lambda и проигрыш комментариев/общего EXAM; это **прогноз, не measured result**.

## Где и сколько

Read-only checkout: `C:/Projects/llm_mvp_core`. Единственный новый пакет: `C:/xg/br01`, проект `Runner.csproj`, assembly `BpeRealizedRunner`, fixed output/obj по §8 карточки. Source-copy только двух указанных файлов pinned commit, B diff только CountPairs. При отсутствующем commit/.NET9/checkout — `PREPARATION_FAILED`, без поиска иного проекта/ветки/модели. Существующий пакет допустим только как та же подготовленная копия с совпадающим manifest, иной пакет — `PREPARATION_CONFLICT`. Существующий `r/0001` всегда `ATTEMPT_EXISTS_NO_REPLAY`, даже при matching manifest; отказ до build и повторный guard в controller. Не удалять, не перезаписывать и не выбирать новый номер.

В локальной оболочке допустимы `throw`, `try/catch`, `void`, DTO и простой JSON-журнал; продуктовый запрет исключений и полный code-slice workflow сюда не переносятся. Ошибка записывается с этапом, partial state и ненулевым exit code; это не нулевая точность и не повод повторять опыт.

Два child-процесса A→B; каждый один раз инициализирует byte vocabulary через `Train("",256)`, создаёт no-update C строго `tokenizer.Save(stream); stream.Position = 0; var control = BpeTokenizer.Load(stream);` на новом MemoryStream. Rewind обязателен: Save оставляет конец потока, Load не перематывает его сам. Далее одинаковый EXAM до, один `Train(corpus,257)`, EXAM после у обученного состояния и у C. У C нет Train. TRAIN Encode по одному до/после на каждое плечо. Controller только сравнивает JSON.

Предел: **2 содержательных Train +2 initialization =4 native Train API calls; максимум2 learned merges; 40 EXAM Encode/Decode +4 TRAIN Encode; 0 DPO/SFT/KNN/LLM calls**. Child30 секунд, controller90 секунд, build60 секунд каждый. Никаких warmup, retry, дополнительных корпусов, vocabulary sweeps или полного solution/test chain. После сбоя сохранить partial artifacts/UNKNOWN; не возобновлять автоматически.

Точные команды предназначены для будущего host, который сначала материализуется по карточке. Сейчас host не существует и команды не исполняются.

```powershell
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath C:/xg/br01/r/0001) { throw 'ATTEMPT_EXISTS_NO_REPLAY: no build; no training' }
dotnet build C:/xg/br01/Runner.csproj -c Release -p:Variant=A -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build A; training not started; no retry' }
dotnet build C:/xg/br01/Runner.csproj -c Release -p:Variant=B -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build B; training not started; no retry' }
dotnet C:/xg/br01/bin/A/Release/net9.0/BpeRealizedRunner.dll run-pair --peer C:/xg/br01/bin/B/Release/net9.0/BpeRealizedRunner.dll --fixture C:/xg/br01/in/fixture.json --attempt 0001
if ($LASTEXITCODE -ne 0) { throw 'ATTEMPT_FAILED: preserve partial artifacts; no retry' }
```

## Измерение и единственный отчёт

Подтверди A0=B0, fixed source/corpus/fixture hashes и неизменность C. Измеряй native Train, не отдельно переписанную формулу. EXAM readout одинаковый до/после и у C. Запиши actual merges/vocab/hash, IDs/Decode/token counts каждого EXAM, TRAIN token count, TRAIN allocation/CPU/wall time/GC0/GC1/GC2. Encode/Save/report находятся вне TRAIN timer.

Покажи ΔA, ΔB и ΔB−ΔA для выходов; A/B/B−A для стоимости полного Train. A1/B1 различаются намеренно. Одинаковые финальные merges дают `FACTOR_NOT_DISTINGUISHED`: другой выбор merge не подтверждён; отдельно измеренную полезную ресурсную ось и регрессии всё равно сохранить. Изменившийся no-update или разные входы — `INVALID_COMPARISON`: raw сохранить, причинный эффект из несопоставимых значений не выводить. Прогноз не заменяет raw. Один A→B timed result не доказывает устойчивый speedup.

**Любое измеренное улучшение полезного показателя — успех по этому показателю и профилю/домену; рост точности не обязателен. Все остальные изменения, включая ухудшения comment и общей EXAM-компрессии, показать рядом.** Не требуй общего улучшения всех осей, не скрывай regressions, не называй token_count accuracy программы или перевода.

Файлы `C:/xg/br01/r/0001/report.md`, raw JSON и при полезном выигрыше `candidate.md`. Report на русском, до350 слов пояснений плюс полная таблица: «Что меняли», «На чём проверяли», «Что получилось», «Выигрыши и ухудшения», «Граница вывода и кандидат исправления». Candidate до120 слов: измеренная ось/профиль, регрессии, original `BpeTokenizer.CountPairs`, конкретный фактор, ограничение одного merge/vocab257, решение о переносе оставлено Любе. При отсутствии измеренного выигрыша `NO_MEASURED_GAIN`, без новых запусков. Один readback отчёта для фактических ошибок, затем завершить назначенную карточку. Никакого CI, product promotion, автоматического commit/push.

## STOP/NEXT: обязательный запрет

Если Люба говорит «стоп/закончить немедленно», сразу прекрати действия. Нельзя делать новый tool call, запись памяти, checksum, review, отчёт или commit; допустимо только остановить собственный уже работающий процесс, если иначе он продолжит обучение. Сообщи кратко известное состояние. Если Люба говорит «следующий эксперимент», немедленно прекращай прежний и переходи к назначенному следующему ID/единому реестру. **Запрещено возвращаться к прежнему отчёту/candidate/hashes/CI, доделывать его или повторять запуск.** Если нового назначения нет — сообщи известный статус, не возобновляй старое. Уже предоставленные GitHub permissions повторно не запрашивай.
