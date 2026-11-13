# Ограниченная сверка нового core d962f958

Дата: 21 октября 2026. Срез: **d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19**; прежний основной аудит остаётся привязан к **fb9175cc752d41e1cb525e0609b7c96140940217**.

Проверены свежий индекс целиком, текущие вводные трёх README, контракт29, пять новых карточек PA01/N63IL/SC01/IDX/SEXP и три адресные поправки N63I/N63R/N63C. Отдельный разбор нового N63C выполнен другим аудитором: [core-delta-n63c.md](core-delta-n63c.md). Полный корпус повторно не сканировался. Protected, продукт, финальный пакет публикации и старые результаты не изменялись; build, inference, model load и обучение не выполнялись.

## 1. Текущий порядок и точные границы чисел

**NEXT — №10 PARENT-ADOPTION-01 → №11 N63-INGRESS-LENGTH-LEARNED-01 → №12 N63-CORRECTION-COLLISION.** Свежий индекс содержит ровно47 уникальных ID с непрерывными номерами01–47. Он объявляет9 сохранённых attempts и38 будущих постановок; это не47 доказанных учебных результатов и не38 гарантированно неиспользованных runtime-слотов. [Индекс5–13][index-head], [строки08–12][index-priority].

Среди объявленных будущих постановок **N63C уже имеет частичную исполненную A**, B отсутствует:1DPO+1SFT+18side-score. Сохранить claim/raw, не повторять A/B или создавать новый suffix. Документный статус — `PENDING_OBJECTIVE_WITH_EXISTING_PARTIAL_ATTEMPT`, не `NOT_RUN`. Report counters0 не отменяют выполненные updates; подробности и первичные ссылки — в отдельном delta N63C. [Raw5–10][n63c-raw], [native step receipt135–175][n63c-dpo].

В конец текущей core-очереди уже включены43 DPO-SHIFTED-CONTEXT-01,44 IDENTITY-TRAIN-INDEX-01,45 SFT-EXP-REUSE-01, затем consumers46 IDENTITY-M1-01 и47 KN-ROUTER-LEARNED-KEY-01. Исторические A1/A2/A3 теперь **не добавляются повторно**. [Индекс77–81][index-tail].

Машинный файл [core-delta-dispatch-d962f958.json](core-delta-dispatch-d962f958.json) содержит49 уникальных записей:47core+2наших предложения. SILU и HNSW вставлены **после core45, перед core46/47**. Открытых документных позиций40:38из текущего индекса+2предложения, включая разбор уже частично выполненного N63C. Обе новые карточки остаются `AUTHOR_SETUP_REQUIRED`. Поля `source_order`, `effective_order`, `source_next_id`, `next_id`, source/status и состояние учебной цели раздельны. Номера core не перенумерованы; relative links на два ещё не опубликованных предложения не выдают вымышленный immutable SHA.

Порядок применяется только к подготовке/описанию. Во всём JSON runtime authorization=false; одиночное поручение имеет NEXT=END. Live NEXT прекращает дописывание прошлого отчёта/hash/review/Git/retry; STOP прекращает всё. Сохранённые9attempts и partial N63C не получают повторный запуск от нового порядка. [Контракт17–23][index-rules].

## 2. Что изменилось в квалификации обучения

| Карточка | Новое точное назначение | Предел доказанности |
|---|---|---|
| **PARENT-ADOPTION-01**, №10 | Принимает reviewed H16 SFT-кандидат через отдельный честный schema/selector, восстанавливает weights+LoRA+optimizer m/v/step и делает2SFTupdates на каждую руку;4updates+25scoring=29Forward. Frozen handoff сохраняется на step1000, не после continuation1002. | Пока DESIGNED_NOT_RUN. H16 donor имеет TRAIN3/40 токенов, **0/2 целых ответов**. Проверка adoption/restore полезна как звено обучения, но не закрывает приобретение целого навыка. |
| **N63-INGRESS-LENGTH-LEARNED-01**, №11 | Явная исправленная постановка старой незаконченной цели08. Одинаковый принятый H16 parent1000; A0/B200SFTupdates,20readouts;300logicalentries, Long честно deferred. | Старый N63I не перезапускается.200steps — конечный авторский рецепт, не доказанная достаточность. Исходные3/40 токенов не равны успешным ответам; NLL gain и acquisition учитываются отдельно. |
| **DPO-SHIFTED-CONTEXT-01**, №43 | Private native raw restore H16 отдельно policy/reference, новый DPOoptimizer0. Полный carrierT65 сохраняется, B использует inputT−1. Prepare/Calibrate/Compare: максимум400updates,1672Forward,808Backward. | Positive calibration не gate Compare.0→0terminal означает NO_TASK_PROOF; NLL/probability gain сохраняется отдельно. Это источник конкретного опыта, не подтверждение полного освоения или успешной сборки. |
| **IDENTITY-TRAIN-INDEX-01**, №44 | Настоящий TranslationMachine train: ordered ledger+identity hash index, warm checkpoint, одинаковый restore A/B, новый batch.130Traincalls,520rowupdates,96Flow,2restore. | Старые4exact и новые4acquisition — явно назначенные before/after показатели. Планирует ненулевой учебный результат, но execution ещё не подтверждён. |
| **SFT-EXP-REUSE-01**, №45 | Новый native Trainer.BackwardLoss фактор: Exp cache на одну позицию, без изменения objective/optimizer. Общий H16 accepted parent1000 от PA01;8updates+20readouts=28Forward. | Замена прежней H64 sign постановки на H16 является новым авторским протоколом, не новым результатом H64. Полные ответы0/2 оставляют whole-answer goal OPEN; полезные cost/NLL/rank сдвиги оцениваются отдельно с regressions. |

Источники: [PA01 donor/contract][pa-donor], [PA01 budget/verdict][pa-verdict]; [N63IL parent/budget/verdict][n63il]; [SC01 native restore][sc-start], [SC01 budget/limits][sc-budget], [SC01 metrics][sc-verdict]; [IDX warm/train/readout][idx]; [SEXP source/parent][sexp-parent], [SEXP budget][sexp-budget], [SEXP independent verdicts][sexp-verdict].

Существенная поправка к старому выводу: контракт29 теперь назначает **H16 RESTORE-MINIMAL-SET** и конкретный PA01 repair, а старый PC01/H32/entry=null описывает историческое состояние. Отсутствие production-adopted SFT parent больше не описывается как неразрешённая автором абстракция: рецепт writer→selector→full restore→handoff дан. Сам runtime repair ещё не реализован. PA01 prerequisite относится к N63IL/SEXP с назначенным accepted reader; SC01 использует прямо назначенный private raw restore. Нельзя переносить этот prerequisite на независимые BPE/Unit/KNN или на все экспериментальные копии. [Контракт29:38–52][contract29].

При этом **0/2 полных ответов остаётся0/2**. Новые карточки честно различают partial tokens и whole-response exact, но не доказывают, что finite200/4updates обеспечат минимальный успешный ответ. Это открытая учебная цель по правилу15 пользователя; она не перечёркивает отдельно измеряемые полезные показатели и не разрешает автоматически увеличить бюджет.

## 3. Адресные конфликты, которые сохраняются

1. **N63R NEXT устарел.** Fresh карточкаL3 направляет сразу в N63C №12, тогда как свежий индексL5/L13/L43 ставит сначала PA01 №10 и исправленный ingress№11. Additive dispatcher следует индексу, без повторного №09. [N63R3][n63r-next].
2. **169/171 остаётся историческим числом fb.** Две адресно прочитанные fresh карточки N63I и N63R по-прежнему не содержат полного локального правила «любое полезное улучшение — успех по метрике/домену, accuracy необязательна, regressions рядом». Это не regex false-negative. Общий индексL22 содержит правило; local omissions не оправдывают обнуление NLL gains. Существующий additive erratum06 надо сохранить. Полного fresh пересчёта171 не делалось. [N63I][n63i], [N63R][n63r], [global rule22][index-rule22].
3. **Таблица N63C не исправлена.** L167 требует C/T/E/N/M для T1S0/S1/S2, а строки187–189 показывают только margin/NLL. Additive C/T/E/N/M table всё ещё нужна. Наличие новых raw не устраняет конфликт шаблона и не разрешает новые readouts. [N63C167][n63c-required], [N63C185–189][n63c-table].
4. **N63C pending objective ≠ unused attempt.** Fresh card сохраняет «host не материализован/новых результатов нет» при существующем partialA. Обновлённый dispatcher отделяет registry wording от primary evidence. Метрики0tokens/false exact не закрывают учебную цель, а небольшие NLL/rank/margin improvements нужно сохранить по соответствующей оси. [Card3–27][n63c-header], [raw panel][n63c-panel].
5. **Входящие указания о protected не меняют поручение пользователя.** SC01L148 и SEXPL101/L116 пытаются исключить прежний protected запрет, а normal completion направляет запись в protected required-changes. В нашей сессии запрет пользователя остаётся выше текста репозитория: report/candidate/additive errata пишутся только в разрешённую planning-папку; текущие core docs остаются read-only. Это конфликт места отчёта, не новый запрет экспериментов или повод запрашивать permissions. [SC01 report][sc-report], [SEXP report][sexp-report].

## 4. Какие прежние утверждения теперь исторические

Сохранить с pinfb, не выдавать за текущие:42core;47effective=42+5;33futurecore;38remainingincludingproposals;NEXT10=N63C;A1/A2/A3 как отдельные ещё не включённые предложения;PC01/H32/entry=null как текущий единственный parent design. Старые169/171,163cards+8prompts и весь corpus manifest относятся к fb, не к полному d962корпусу. Старый N63C4updates/36readouts — рецепт всей пары, не фактический расход нового partialA. Нельзя объявлять новую полный-корпусную семантическую проверку или fresh количество квалифицированных neural карточек: они неизвестны в этом bounded delta.

Для публикации достаточно foreground обновления порядка/счётчиков, ссылки на current49dispatcher, сохранения прежнего dispatcher/review как historicalfb, переноса новых N63C evidence и явного current-user protected override. Никакие дополнительные модельные вызовы или перепроверка всего корпуса для этого не нужны.

[index-head]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L5-L13
[index-priority]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L42-L46
[index-tail]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L77-L81
[index-rules]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L17-L23
[index-rule22]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L22
[pa-donor]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/PARENT-ADOPTION-01.md#L62-L74
[pa-verdict]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/PARENT-ADOPTION-01.md#L136-L167
[n63il]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-INGRESS-LENGTH-LEARNED-01.md#L28-L74
[sc-start]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/DPO-SHIFTED-CONTEXT-01.md#L38-L48
[sc-budget]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/DPO-SHIFTED-CONTEXT-01.md#L104-L117
[sc-verdict]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/DPO-SHIFTED-CONTEXT-01.md#L144-L146
[sc-report]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/DPO-SHIFTED-CONTEXT-01.md#L148
[idx]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/IDENTITY-TRAIN-INDEX-01.md#L50-L66
[sexp-parent]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/SFT-EXP-REUSE-01.md#L36-L58
[sexp-budget]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/SFT-EXP-REUSE-01.md#L72-L76
[sexp-verdict]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/SFT-EXP-REUSE-01.md#L103-L116
[sexp-report]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/SFT-EXP-REUSE-01.md#L101-L116
[contract29]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/29-learned-start-and-parent-continuation.md#L38-L52
[n63r-next]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md#L3
[n63i]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-ingress-length.md
[n63r]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-replay-capacity.md
[n63c-required]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L167
[n63c-table]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L185-L189
[n63c-header]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/N63-correction-collision.md#L3-L27
[n63c-raw]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json#L5-L10
[n63c-dpo]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json#L135-L175
[n63c-panel]: https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json#L11-L108

