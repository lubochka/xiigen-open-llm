# RP03 · 16 октября 2026 · уточнения первого урока и проверка findings

Статус первой публикации: **SOURCE-BOUND AUTHOR AUDIT / REPAIR IN PROGRESS**. Это расширение существующего атласа, не новый RP/V8, не повтор R01–R25 и не исполнение продукта. Идентификаторы `RP03-O16-*` локальны этому уточнению: они не переименовывают N22–N31 и не представляют уже известные ограничения как новые открытия.

Проверяемый пакет: [RP03 V7](../Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/README.md). Planning HEAD перед первой записью: `f4b911fff5237958df1bfb17512298b77c7e0ebf`. Core source snapshot: `58d12ecfe7af05e3221f984bf6c4c67d82fe3b96`. Для прочитанных до pin документов ниже приведены конкретные blob SHA; это не утверждение об отсутствии параллельных изменений всей ветки.

## Что подтвердилось, а не найдено заново

[N26–N31](11-RP03-N26-N31-SOURCE-REPAIR-2026-10-15.md) уже правильно различают экспериментального Candidate и принятый serving, latest exact-ID shadowing, CandidateId и typed payload, версию представления, владельца обучения и семь доменных направлений. В свежем Core `RegistryDynamicGraphEdgeAuthorityResolver.Resolve` действительно требует `IsLearned` и минимум `Probationary`, а latest exact-id разрешается до этой проверки. Поэтому первый урок нельзя разблокировать выставлением доверительных флагов. Это повторное подтверждение N26/N27, **не новый defect**.

Источник: [resolver, весь файл](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/21.AgentInfrastructure/Logical/RegistryDynamicGraphEdgeAuthorityResolver.cs), blob `fef6479a71dc0bb042bcdfb4132e14a1a8d5e077`.

## RP03-O16-01 — новый остаточный конфликт: cold trial требует преждевременный serving

**Класс:** VERIFIED_PROMPT_CONFLICT; конкретизация N26/N27, не новая неисправность runtime. **Приоритет:** первый урок / 02B→02D. **Владельцы:** 02A, 02B, 02D, 02C, dispatcher/board.

В 02B одновременно сказано использовать experimental Candidate вне served exact-ID и показать ordinary inference reader. В 02D step3 требует новый PID и ordinary registered authority route; последующие разделы требуют accepted serving W1. Но N26 запрещает неаттестованному Candidate такой доступ, а независимый экзамен стоит дальше. План тем самым оставляет исполнителю ложный выбор: задержать cold proof до adoption или подделать допуск.

Проверенные исходные blobs: `tasks/v7/RP03.02B.md`=`c21d4feafc8bf9aefb9f1a9ec67dc4df6c1a7822`; `RP03.02D.md`=`c51b973f49b7374edbaadda29dbb6233d2004b72`; dispatcher=`eff4f97c8fd526ab3ee9e3a08a83bd79fa187c16`.

**Исправление:** один и тот же production-compatible scorer/serializer/schema использовать сначала в явно изолированном, разрешённом экспериментальном runner. 02B доказывает численное обновление и чтение trial W; 02D — новый PID, восстановление trial W, replay и rollback trial. Это `TRIAL_COLD_CONSUMED`, не `USER_ADOPTED`. Обычный Host продолжает читать ранее принятый tuple. После необходимой scoped acceptance 02C выполняет promotion/adoption; технический atomic publish/rollback использует механизм 02D. Независимое обучение не ждёт полной typed-ветки, всех семи доменов или ordinary Windows Host. Никакой ослабленный resolver не попадает в production DI.

**GOOD:** trial checkpoint загружен PID2, same scorer применяет W1; live accepted tuple не изменён. **BAD:** `IsLearned=true`/Trust присвоены до легитимного admission, либо isolated runner назван ordinary Host. **EDGE:** нет экспериментального caller → 02A отмечает конкретный missing adapter, 02B реализует только его; native block → exact BLOCKED_COLD, не симуляция. **Опровержение:** найден существующий lawful experimental caller, уже однозначно описанный этими step3/step4; тогда достаточно сослаться на него, не строить дубль. **Минимальная проверка:** затронутая граница trial/live и cold replay; никаких новых coverage-целей.

## RP03-O16-02 — новый ложный критерий: один update обязан перевернуть ranking

**Класс:** VERIFIED_ACCEPTANCE_OVERCONSTRAINT + арифметический контрпример; не измерение Student/Flow. **Приоритет:** первый урок. **Владельцы:** 02B, 02D, 02C, scientific contract.

Текущие 02B/02D требуют changed lawful rank/choice как свидетельство одного W0→W1. Корректный небольшой шаг может улучшить margin/loss и не пересечь границу argmax. Нельзя подбирать eta или пример до обязательного flip и называть это доказательством обобщения.

**Контрпример, illustration only:** phi(A)=(1,0), phi(B)=(0,1), наблюдение B>A, W0=(1,0), eta=.2, lambda=0, bounds не активны. g=(-1,1), p=sigmoid(-1)=0.2689414214. W1=(0.8537882843,0.1462117157). Margin B−A вырос с -1 до -0.7075765685; pairwise loss снизился с 1.3132616875 до 1.1082549776. A всё ещё первый. Это успешное применение данного шага, а не failure и не доказанный domain gain. Координаты здесь — иллюстрация арифметики, **не candidate-ID one-hot признаки**.

**Исправление:** различать PARAMETER_APPLIED, SCORE_EFFECT, RANK_CHANGED, TRIAL_COLD_CONSUMED, DOMAIN_GAIN. Обязательны корректная формула/finite state, фактическое применение один раз и чтение той же версии. Rank change фиксировать как наблюдение, не заставлять. Для cold сравнивать состояние и score contract; при stochastic decoding отдельная distribution evaluation, не требование одинаковой строки. Regularization/clipping могут изменить знак частного margin-effect: записать objective и границы, не обещать монотонность каждой метрики. g=0, TIE и недействительный сигнал остаются NO_UPDATE; clip-saturated нулевой delta — честный NO_EFFECT, не invented W1.

**GOOD:** численный update и изменившийся score при прежнем argmax; независимый экзамен ещё NOT_MEASURED. **BAD:** увеличивать шаг до смены ответа, сообщить trained по флагу либо reject корректный шаг только из-за прежнего top1. **EDGE:** preferred option уже первая; tied scores; bounds saturation; регуляризация. **Опровержение:** return-критерии уже допускают корректный шаг без rank flip — тогда исправить только противоречащие фразы. **Проверка:** один воспроизводимый численный пример затронутой формулы плюс реальный reader; не новая матрица unit tests.

## RP03-O16-03 — уточнение N29 по реальному caller: Metadata создаётся пустым

**Класс:** CURRENT_SOURCE_VERIFIED в конкретном методе; отсутствие глобальной версии/feature provider пока НЕ доказано. **Приоритет:** корректный первый feature reader, не общий рефакторинг. **Владельцы:** 02A/02B; интеграция RP02.

В `DynamicGraphEdgeAuthorityExecutionPipeline.ExecuteAsync` при создании `LogicalUnitRequest<EdgeAuthorityInput>` передаётся новый пустой `ReadOnlyDictionary<string,object>`; такие же пустые metadata создаются для guard requests. Источник: [pipeline](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityExecutionPipeline.cs), просмотренный участок ExecuteAsync от request construction до guard dispatch. Это сильнее предположения «нужный version passport, наверное, лежит в request.Metadata», но не доказывает, что scorer не имеет immutable state/config/canonical input с нужной версией.

**Исправление:** 02A обязан показать существующий источник domain/law/feature-coordinate/index/checkpoint версии и actual read. Выбранный минимальный контракт — immutable decision snapshot в scorer/experiment context с явной identity и валидируемым reader; если существующий publisher уже обеспечивает это, переиспользовать его. Не получать pre-outcome phi из candidate ID, order, feedback label или outcome; не считать одинаковую размерность совместимостью. Один snapshot для main+order+identity probes; no mutable ambient global. Сначала trace, затем только missing seam. Product ABI не менять без доказанной необходимости.

**GOOD:** `feature_snapshot_id` ведёт к версиям и реальному producer, все диагностические вызовы читают один W/snapshot. **BAD:** читать несуществующий Metadata["law_version"], молча брать latest index или шифровать ответ в CandidateId. **EDGE:** mixed domain, неизвестная версия, concurrent update, renaming diagnostic → явный refusal/unknown без мутации. **Опровержение:** exact existing out-of-band immutable reader удовлетворяет контракту; тогда код не меняется. **Проверка:** narrow reader/version/immutability risk, не повтор всех guards.

## RP03-O16-04 — подтверждённая навигационная мелочь, не алгоритмическое открытие

README blob `6656e87a6359e75d97f55ee4cb3879d24762ec0d` одновременно сообщает 5 и 9 children; manifest blob `567837b226e7444a0de928f532251caffdf06249` содержит 9. Исправить устаревшее 5 на 9 и сохранить 15 parents/9 children/один V7 route. Это не новый этап и не причина задерживать обучение.

## Границы и способ ремонта

Ремонтировать непосредственно затронутые active task cards, dispatcher, board и scientific contract, а не наращивать противоречащие banners. Сохранить RAG→primary источники, многие выходы/typed provenance, семидоменный horizon, F0/F1 границу, law-conditioned adaptive kNN и отдельный RP18 semantic projection. Варианты H/ablation не превращать в обязательную очередь до первого урока.

Роли DPO для RP03 handoff: **CODEX / Astra 6 UltraCode** выполняет роль человека при выборе; **внешняя открытая китайская модель** создаёт обе стороны; **отдельная CODEX / Astra 6 UltraCode** оценивает их качество. Flow pairwise preference update — не Student DPO. RP01 development exception не переносится сюда автоматически.

Навыки, действительно прочитанные scoped sections: architect-behavior-classifier (1–100), phase-planning (1–120), plan-review (1–110), generated-code-review (1–100), prompt-patch-authoring (1–130), conversation-intent-and-self-reflection (1–110), trainable-architecture-decision (1–140). Применены narrow live scope, explicit work order, source-first claim, WRONG/CORRECT и candidate-bound readback. Исторические требования чужих 30/50-pass кампаний не становятся новым поручением этого RP03 ремонта.

**Не выполнялось:** продуктовый code patch, сбор реальных пар, Flow/Student обучение, external Chinese/CODEX calls, эксперименты на PC, независимая Astra экспертиза. Исторические claims не считаются свежими результатами. Первичная публикация этого файла сама по себе ещё не означает, что противоречащие active cards исправлены; exact publication/readback receipt добавляется после их изменения.
