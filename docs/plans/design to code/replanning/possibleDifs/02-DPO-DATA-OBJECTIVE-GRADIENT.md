# 02 — DPO: от причины ошибки до работающего градиента и правильного получателя

<!-- POSTQ011-2026-11-04:START -->
## POSTQ011 — Пара, objective, градиент и полный ответ после Q011

P03 B2/4→1/4 при улучшении T1 NLL; P04 A/B2/4→2/4 с пятью ухудшениями NLL. Разные rejected в A/B не задают одну accuracy. Training loss по 21 позициям до update, answer NLL после update и итоговый margin — разные показатели. P05 sum/mean уже используют response mask;400SFT квалификации 0/6 не доказывают усвоение.

Q011 шесть derivative comparisons проходят с maxabs0,000026932268 и запасом 3,713018. First/second moments одного update не независимы. Новый saved scope-probe: changedchosen logP при прежнем loss проходит узкий checker, но нарушает цепь objective; honest12/12 согласованы. Для full gate связать сохранённые logP/reference/beta→margin→loss/derivative; missing per-evaluation mask/state не изготовлять новым Forward/Train. Native acceptance остаётся отдельной. В DPO restore Q007 finite/nonnegative-v и trusted continuation completeness проверяются раздельно; lawful sparse/Step0 сохранить. Existing owners и Q04 pause не меняются.

Основание: [единый разбор](codex-2026-11-04-POSTQ011-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core158](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md) и [план 159](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-159-postq011-addressed-repair-plan-2026-11-04.md). В передаваемое поручение включить собственные положительный/отрицательный случаи, actual consumer и предел доказательства. Исходные результаты, данные, бюджет и очередь сохранены; текущая правка документная.
<!-- POSTQ011-2026-11-04:END -->


<!-- POSTQ003-2026-11-03:START -->
## Уточнение приёмки 3 ноября 2026 — DPO-EXACT-STATE-AND-METRIC

До переноса результата DPO связать actual policy/reference/tokenizer/owner с exact saved attempt и фактическим original payload. Правильный pointer к конфликтному тексту и чужой payload того же размера не проходят. Sum/mean/last-pre-update/final — разные величины; отсутствие gained rows при улучшении logits не становится ростом accuracy. Recovery P01 с8DPO уже записан, повтор не нужен. Existing R128-02/R128-03 и ordinary caller проверяются отдельно.

Основание и точные владельцы — [аудит после Q003](codex-2026-11-03-POSTQ003-ARTIFACT-AND-TRANSFER-AUDIT.md). В передаваемое поручение включить назначенные state/attempt/path/authority, положительный и отрицательный случаи и ожидаемую причину отказа. Исходный ниже паспорт сохраняется как история; уточнение не запускает новый опыт, не меняет gold/budget и не отменяет доказанные прежние результаты. Нативная реализация и приёмка требуют назначенного исполнения.
<!-- POSTQ003-2026-11-03:END -->


<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Целостность reference, качество ответа и вероятностный сдвиг проверяются отдельно.** Сохранённые fingerprints P105 согласованы: reference изменила 0/16 ключей, policy — 4/16, шаг 400 → 401; точность осталась 2/4. Это не доказательство приобретённого навыка, ускорения или ненулевого градиента данных при восстановленных moments и weight decay. Фактические бинарные веса за LFS pointers этим аудитом не прочитаны.

Для отрицательных эталонов T2/E2 отношение шансов gold/rejected улучшилось при уменьшении абсолютного logP(gold). Хранить обе величины и accuracy с их определениями. Shared-model контроль достиг защиты до мутации; попытка установить один adapter на две модели отказала раньше, при регистрации, и не доказывает следующий guard.

Конечная Wiki-метрика уже измеряется отдельно от среднего по траектории. При переносе кэша или читателя сохраняются происхождение панели и checkpoint; подмена final средним должна менять verdict публикации на различающем примере. Честные и испорченные ответы проходят один оценщик. Сначала пересчитываются сохранённые данные, без нового DPO update.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:START -->
## POST102 — проверяется смысл пары и ответа у того же получателя

**Действующее уточнение 02.11.2026; NAMED_CHECK: POST102-DPO-EVIDENCE-CHAIN.** Нижнее тело сохраняет исторические выводы; новые обучения и бюджеты этим блоком не назначаются. Основание и продолжение: [адресный аудит POST102](2026-11-02-POST102-TRANSFER-AND-EXECUTOR-AUDIT.md), core `15745b357e055055ee9bc58b52cc374d90580961`.

**WRONG:** «admission принят, веса изменились, девять строк калибровки прошли — смысл выучен». В P102 калибровка использует `actual.Trim()==gold`, а итоговый `SemanticAnswerCorrect` — `StartsWith(gold)`: это разные проверяющие ([bridge:381–409](https://github.com/lubochka/llm_mvp_core/blob/15745b357e055055ee9bc58b52cc374d90580961/experiments-v2/RP01-MICRO/P10201/src/P102TrainingStepBridgeCopy.cs#L381-L409), [bridge:675–718](https://github.com/lubochka/llm_mvp_core/blob/15745b357e055055ee9bc58b52cc374d90580961/experiments-v2/RP01-MICRO/P10201/src/P102TrainingStepBridgeCopy.cs#L675-L718)). Правильное начало с противоречащим хвостом может пройти, а законная перефразировка — не пройти.

**CORRECT / проверка DIF-203 и DIF-205:** назвать один callable оценщика и его версию; калибровка и оценка сохранённых ответов вызывают его с одинаковой схемой аргументов. Отдельно проверять число результата, закон операции, противоречие и полноту, а exact text хранить отдельной мерой. Для gold 20: «выводит 120» и «postfix возвращает новое значение» должны провалиться; верная перефразировка должна пройти смысловую рубрику; gold с ложным хвостом должен провалиться. Порча текста проверяется повторным вызовом оценщика, не заменой готового `true` на `false`. Для пары сохранять независимый verdict VALID/TIE/BOTH_BAD/UNJUDGEABLE по точным сторонам; прохождение admission не заменяет этот verdict.

**Предел вывода и адресное продолжение DIF-208:** записать `pair → admitted material → изменённое состояние → фактически вызванный consumer → оценённый ответ`. Штатный DPO-примитив внутри экспериментального copied bridge подтверждает этот примитив, но не весь обычный серверный маршрут. Владелец оценки сначала пересматривает четыре готовых текста P102 без Train; владелец переноса закрывает конкретного читателя состояния отдельно. Равенство одному memorized-answer baseline на одном EXAM не означает нулевую способность модели вообще.
<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:END -->

**Статус:** CODE-Traced for named methods; exact RP01 native numbers sourced from existing audit, no new DPO execution.

## Цепочка и границы
Исправляемое действие: **real observed weak skill → конкретный domain/task/law → два проверенных ответа на одном prompt/context → допущенная точная пара → response-only tensor material → frozen reference / trainable policy → DPO optimizer → candidate checkpoint → independent heldout → ordinary consumer**.
Слой legal/provenance отличается от semantic pair judge; право использовать материал не означает предпочтительность или полезное обучение. Source of truth: [WorkLeft DPO conformance](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md#L11-L45) и [design contract](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/RECOVERED-DESIGN-CONTRACT.md#L76-L96).

## DIF-201 (P0) — Данные simulation не равны проверенным внешним teaching pairs
**Доказательство:** [CodeDocumentationSimulationDpoExperiment.Run](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L99-L125) публикует provenance="simulation", sourcePolicy="existing DeterministicGenerator only"; chosen и rejected имеют DpoPairSideAuthorKind.DeterministicGenerator. Он проводит admission, canonical masks и реальное обучение — не пустышка. Но [первичное требование](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/AGENTS.md#L27-L31) и [WorkLeft](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md#L11-L24) разделяют human CODEX/Astra lesson selection и exact-pair judge, а **внешняя разрешённая Chinese/open model должна стать автором обеих final sides** в этом целевом режиме.
**Расхождение:** research simulation честно размечена и позволяет измерить механические свойства, но **не является выполнением намеченного external-teacher first-lesson режима**; не утверждать отсутствие внешних моделей во всех других маршрутах.
**Различающий опыт:** сопоставить 1 admitted direct pair, прошедшую отдельное semantic review с важным defect reason, с программной simulation pair по одинаковому Student/B0, tokenizer, размеру prompt и бюджету; изменять только pair content/author process в рамках прав. Verify pair judge and provider trace; не выдавать 0 вызовов за китайский teacher.

## DIF-202 (P0) — 500 строк ≠ 500 семантически различных уроков
[RP01 runtime manifest/source](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md#L20-L30) описывает 25 шаблонных C# families×20 строк, exact byte diversity, но не semantic independence. Paired preference на известном шаблоне может улучшиться без новых правил. Dataset expansion не лечит однотипность.
**Фальсификатор:** heldout-by-source-family с удалением всех paraphrases/templates из train, учёт counterexamples C#/SQL/EN→RU отдельно, forced ties/BOTH_BAD при отсутствии действительной ошибки. Не менять закрытую оценочную панель, не добавлять её в train.

## DIF-203 (P0) — Не установлен достаточный причинный контраст chosen vs rejected
Семантический вопрос: выбранная сторона должна отличаться **существенной устранимой ошибкой** при прочих равных. Валидный DPO JSONL может содержать stylistic-only preference, одинаковую правильность, одинаковый шаблон или ошибочный chosen.
**Что уже проверено:** [prepare/admission](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L113-L125) проверяет governance, lossless length, masks, content authority. **Чего это НЕ устанавливает:** независимой полезности всех 500 смысловых противопоставлений.
**Проверка:** human-role judge рассматривает exact chosen/rejected bytes и task law, присваивает VALID/TIE/BOTH_BAD/UNJUDGEABLE, defect class, anticipated capability recipient; report per-family, не positive/negative string count.

## DIF-204 (P0) — TeachingVector не переносит смысл DPO в координаты LoRA
[StepVectorCore](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L861-L904): scaledWeight = normalizedPairWeight × VectorSignal(TeachingVector). [VectorSignal](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1159-L1166) считает только **число ненулевых координат**: clamp(1+activeSlots/60,1,2). Их значение, знак, индекс, связи, пространство и lawVersion этим scalar не используются; Student градиент остаётся от next-token logprob objective.
**Доказанный факт:** этот конкретный API использует «вектор» лишь для веса пары, а не для обучения output semantic vectors.
**Проверка:** два TeachingVector одинаковой sparsity, но разной семантики/координат при тех же pairs дают ту же числовую scalar weight; отдельно проверить, что expected recipient вообще должен зависеть от vector coordinates. Нельзя требовать semantic meaning в StepVector, если контракт сознательно говорит pair weighting: тогда **missing transposition/recipient** находится выше, у consumer.

## DIF-205 (P1) — Preference metric не равна свободной генерации
[Step](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L650-L699), [StepMany](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L701-L750), [StepVector](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L891-L940) возвращают loss/reward margin/Accuracy на train pair. Ни одно не оценивает compositional law correctness, quality of C# documentation, BLEU/chrF или fluent Russian output. [Отчёт RP01](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/RP01-LEARNING-MECHANISM-REVIEW-2026-10-16.md#L17-L29) фиксирует изменённые margins без semantic gains.
**Проверка:** unchanged C0→candidate blind free generation при одинаковом decoder, independent rubric, domain-law failures, source-family split и confidence interval; не делать promotion по chosen preference.

## DIF-206 (P1) — Objective/mask/prefix: проверить, но НЕ объявлять дефектом без репродукции
Реальный код [TargetMask](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1002-L1016) использует ChosenResponseMask/RejectedResponseMask для exact material; [ComputeTargetLogProbs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1036-L1069) суммирует masked target log probabilities (exact path), [backward](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1071-L1119) переносит d(log-softmax) с правильной сигнатурой. [Обучающая композиция](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L116-L125) вызывает CaptureAndValidate. Это **позитивные существующие safeguards**.
**Оставшиеся условные риски:** (a) не совпадает joint prefix BPE vs separate encoding, (b) EOS/padding включён в response targets, (c) mask fractional >0 при backward, который counts >0, (d) слишком длинный prompt делает response unlearnable, (e) wrong completion start offset, (f) truncated free target despite correct tensor material.
**Минимальная проверка без большой CI:** на **новой разрешённой мини-паре** (не старой already applied) вывести только hashes/length/mask sums и ручной tiny forward + finite difference для одного LoRA weight при замороженном B0. Если no violation, закрыть гипотезу, а не править код под тест.

## DIF-207 (P1) — Batch size / beta / learning dynamics
[StepMany](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L721-L748) усредняет градиенты по N и делает 1 optimizer update за группу; [simulation](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L99-L111) использует learning rate=1e−5, beta=.1, 1 epoch, 50 updates при 500 pairs. **500 предъявленных пар не эквивалентны 500 optimizer steps.**
Гипотезы: малый LR/одна эпоха/мало полезных семейств/короткий context/случайная B0 дали слишком слабый capability gradient. Обратная опасность — увеличить LR/эпохи ради train margin и получить забывание. Не рекомендовать hyperparameter sweep до baseline & data controls.
**Проверка:** gradient norm per allowed tensor, relative Δweight, per-pair scalar, response target tokens, best-of-fixed few legal settings на **отдельных новых** допущенных lessons, одна изменяемая переменная. Старые one-pair/no-repeat не переигрывать.

## DIF-208 (P1) — Не тот optimizer recipient (route/Unit/Flow/Graph)
Логическое требование [восстановленного контракта](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/RECOVERED-DESIGN-CONTRACT.md#L29-L44): weak outcome → конкретный owner → modified state → actual downstream consumer. DpoTrainer работает с разрешёнными LoRA-параметрами Student; это может исправлять text logits, но не relation policy Unit/Flow, KNN index или Graph selection. Good pair на «какой unit вызывать» не обязан обучить сам router при генеративном recipient.
**Проверка:** report pair/task/defect → learning recipient ID/parameter keys → optimizer delta → cold consumer ID. Если отсутствует доступный consumer, verdict MISSING_EFFECTIVE_RECIPIENT, а не DPO_BROKEN.

## DIF-209 (P1) — Ошибки тракта gradient/optimizer остаются возможными, но не подтверждены
Каноническая цель стандартного DPO: margin β × [(log policy chosen−log reference chosen) − (log policy rejected−log reference rejected)]; L=−log sigmoid(margin). При policy==reference margin=0 и derivative=−0.5 **не означает нулевой градиент автоматически**.
[Реальный код](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L668-L681), [sign/backward](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1109-L1119) и [apply recipient](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1218-L1225) выглядят согласованно по знакам; **проверять цепное правило, frozen base и численную точность TensorCore**, не писать «broken» по факту нулевого quality-gain.
**Признаки настоящего дефекта:** bad finite difference; optimizer updates forbidden tensor; reference changes; gradient zero unexpectedly on discriminating pair; response-only mask all zero; restored optimizer moments/state mismatch; changed layer not used by Generate.

## Принципы оценки и остановки
Паспорт результата: pair ID and bytes digest; domain/source family; chosen/rejected semantic reasons; tokenizer+prompt/response target lengths; requested/actual recipient; updated tensor map; reference unchanged; C0/T response diff; independent semantic grade; forgetting; complete cost. **Один реальный полезный урок важнее 500 однотипных rows.**
