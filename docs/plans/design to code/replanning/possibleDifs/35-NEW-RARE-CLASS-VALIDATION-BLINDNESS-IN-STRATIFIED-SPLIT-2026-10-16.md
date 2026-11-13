# N35-B — Rare-class validation blindness: стратификация прячет единственные примеры домена

**Дата:** 16.10.2026 · SOURCE_CONFIRMED_SPLIT_PROPERTY / HISTORICAL_QUALITY_UNMEASURED / NATIVE_NOT_RUN · core snapshot 216a1f3d6af3f40ceef081dc168b07e4ce3934f9. Дополнение, **не повтор** [N33 E44/E45 pair-target confound](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md). Protected core/docs/plans/replanning-possible-diffs: READ ONLY.

## Главное отличие

В исследовательской библиотеке алгоритм с названием StratifiedSplit обещает сохранить редкие классы в обеих выборках. Но реализовано nTrain = max(1, floor(n * ratio)); при **ровно одном** примере некоторого класса этот пример целиком уходит в train и **ни одной** проверки этого класса в val не появляется. При двух примерах будет один train и один val; при одном — нулевой шанс измерить обобщение класса. Отчёт об общей accuracy может быть высоким, хотя обучение на редком, наиболее важном законе вообще не проверялось.

Это не утверждение, что алгоритм обязан делить один пример между train и test. **Правильное действие — пометить такой закон UNMEASURED и добыть независимый случай, а не включать тот же пример в обе выборки.**

## Источники — точные строки и соседние ветки

1. [StratifiedSplit.cs L12–36](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/Infrastructure/StratifiedSplit.cs#L12-L36): GroupBy(label), внутренний shuffle, nTrain=Math.Max(1,(int)(items.Count*trainRatio)), train.Take(nTrain), val.Skip(nTrain). Предварительного minimum-per-class и проверки per-class val coverage нет.
2. [E40TrustScorerV2.cs L8–11, L31–40](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs#L8-L40): комментарий обещает, что LOW/MEDIUM/HIGH есть в обоих splits, но допускается общий val.Count>=3, без обязательной проверки LOW count.
3. [E40TrustScorerV2.cs L69–100](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs#L69-L100): отсутствующий val-класс представлен как LOW=N/A, но overall experiment возвращает обычные accuracy, scaling и ShouldScaleUp на базе **общего** объёма val, а не per-class availability. Это не доказательство, что конкретный архивный E40 run действительно лишился LOW; требуется распечатать классовые counts.
4. [E44DatasetWeighterV2.cs L35–54, L70–90](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs#L35-L90): Split применяется отдельно к генерируемым general/security predicate IDs; если secVal.Count<=1, функция подставляет baseGen вместо baseSec и enhGen вместо enhSec. Это отдельный **metric-substitution**: при отсутствующей security-val метрика security не просто UNKNOWN, она берётся из другого домена. Порог secPreds.Count>=2 не исключает два уникальных singleton predicate-classes.
5. Сверены [language_translate](https://github.com/lubochka/llm_mvp_core/blob/language_translate/35.ExperimentsV2/Infrastructure/StratifiedSplit.cs) и [DesignToCode](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/35.ExperimentsV2/Infrastructure/StratifiedSplit.cs), тот же max(1,floor(...)) (исторический blob 259663c8c9575aa604a4b51e281e6e96ac73c8f8). **Риск унаследован**, не установлен как новая регрессия RP01.
6. [Существующий разбор E44/E45](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md) уже показывает другое расхождение: перемешивание отдельных предикатов разрушает source→target transitions. Здесь новая ось — **невидимость rare-class validation и domain substitution**, а не обучение разным последовательностям.

## Source-level вычисление на минимальных данных (не C# run)

Для классов RARE=1, SMALL=2, MID=3, COMMON=5 и ratio 0.8:
- RARE: train=1, val=0;
- SMALL: train=1, val=1;
- MID: train=2, val=1;
- COMMON: train=4, val=1.

Итого train=8, val=3, **3 из 4 классов проверяются**, один самый редкий класс вообще не измерен. Это прямое следствие floor/max, независимо от seed/shuffle. Общая val.Count=3 проходит ограничение E40, и отчёт может выглядеть успешным без проверки RARE.

## Быстрый эксперимент SPLIT35 (differential measurement, domain-aware)

**M0: реальный unit-test механики, без модели.** Задать 11 объектов четырёх классов с приведёнными счётчиками, вызвать StratifiedSplit.Split(..., ratio:0.8, seed:42), вывести per-class train/val counts и exact IDs. Ожидается 1/0, 1/1, 2/1, 4/1. Если хотя бы один singleton оказался в val при исходном коде, исправить описание механизма. Этот тест измеряет только split.

**M1: измерить скрытую ошибку на честной панели, не смешивая data и method.** CODEX с Astra 6 UltraCode задаёт 3 доменные панели: (a) C# security null/authorization rules, (b) EN→RU negation/coreference, (c) SQL NULL/three-valued semantics. В каждой иметь один искусственно редкий law (1 historical example), но приготовить **2 дополнительных source-family-disjoint независимых** heldout prompts для этого закона. Gold и статус ambiguity устанавливаются до обучения. Все 3 extra примера **не** участвуют в обучении или выборе гиперпараметров.
- **A / привычная отчётность:** та же frozen trained model оценивается на original stratified validation. Замерить overall accuracy, per-law accuracy, NOT_MEASURED и вычисленный валидатором метрик hard failure.
- **B / проверка слепой зоны:** **без изменения модели/обучения** запустить тот же consumer на дополнительно подготовленной sealed rare-law panel. Доля ошибок на B доказывает проблему измерения, **не** причинное улучшение обучения и не A/B модели на одной выборке. Не сравнивать A и B общим score как равноправные алгоритмы: это два разных evaluation sets.
- **C / при доказанном вреде:** получить второй *только train* самостоятельный пример редкого закона от внешней открытой китайской модели (chosen/rejected) после выбора CODEX/Astra и независимого CODEX/Astra контроля; заморозить ранее созданную sealed rare-law evaluation, сравнить original student A0 vs A0+one new lesson при одинаковом шаге/optimizer/seed. Если качество не меняется или ухудшается — не масштабировать.
- **E44-specific M2:** воспроизвести случай secPreds=[two different IDs], genPreds>=4 и убедиться, что secVal.Count=0; проверить baseSec==baseGen, enhSec==enhGen. Затем **не менять scorer**, а заменить скрытую подстановку в тестовом diagnostic path явным UNMEASURED. Проверить, изменился ли допустимый вывод об успехе E44. Это исправление отчёта, а не нейронная победа.

**Протокол метрик:** per-domain law coverage, per-class train/val/external IDs, number of unmeasured laws, critical-wrong acceptance, correct/partial/abstain, paired ordinary outputs before/after, latency, memory, independent judge result. Если известен всего один закон без правомерно независимого heldout, статус UNVERIFIED, не 100%-accuracy. Регрессии на общих законах — обязательный отрицательный контроль.

## Применимость и запреты на экстраполяцию

- **Релевантно** к малым исследовательским корпусам, rarer security failure modes, DSL/SQL edge laws, языковым negation, E40 trust levels и E44 security. Для C# AST inference проверять отдельно family-disjointness: один Method/file может порождать сотни correlated predicate tokens.
- **Не напрямую** к RP01 500 exact DPO LoRA: тот pipeline не вызывает экспериментальный StratifiedSplit как обязательный Student обучающий узел; нет оснований объяснять RP01 0/3/0/4 этой ошибкой.
- **Не переносить один глобальный sampling policy на все домены**: редкую ошибку безопасности важнее явно проверять; в обычных языковых преобразованиях может требоваться representative morphology; для SQL dialect различия существенны.

## Роль человека и автора DPO

**CODEX / Astra 6 UltraCode выполняет человеческий выбор** независимого закона и **отдельно оценивает** качество обеих сторон DPO и blind итоговых ответов. **Внешняя разрешённая открытая китайская модель производит обе стороны chosen/rejected.** Нельзя объявлять их уже исполненными в этом source-only документе. Исходные frozen run receipts и protected core планы не изменять.

**Priority:** P1 для доверия результатам E40/E44 и оценки rare-domain learning; P2 условно для будущих active domain learners; для RP01 Student — DIAGNOSTIC_NOT_ACTIVE без отдельной трассы.


## Поправка численности панели и защита отсутствующего измерения — 12 ноября 2026

**Устранена неоднозначность плана M1:** выше указаны три доменные панели и по два дополнительных независимых heldout prompts в каждой, но следующая фраза говорит «все 3 extra примера». При сохранении назначенной структуры правильно **3 × 2 = 6 heldout prompts**, с шестью уникальными RowId и принадлежностью домену. Старый текст оставлен для истории; этот блок имеет приоритет для численности M1. Это исправление карточки, не результат выполненного опыта.

Панель A и дополнительная панель B оценивают одно и то же frozen trained state. За их разницу нельзя выдать эффект обучения: меняется измерительный набор. Условный C имеет отдельный training budget и запускается только как отдельно назначенная интервенция; его примеры не переходят в sealed B. Разрешение такого запуска данным дополнением не создаётся.

| Состояние метрики | Обязательное представление |
|---|---|
| В классе нет validation rows | support=0, accuracy отсутствует, статус UNMEASURED |
| Rows есть, верных нет | support>0, correct=0, accuracy=0 — это измеренный провал |
| Не выполнен consumer или отсутствует результат | NOT_RUN / INCOMPLETE, не нулевая accuracy |
| Для другого домена есть хороший score | Сохранить только под тем доменом; подстановка вместо отсутствующего security score недопустима |

Для каждого домена/закона вычислять собственные support, correct, false accept/reject и constant-answer ceiling по его gold. Чужой холодный score не является потолком; общий успех не закрывает отсутствующую rare-law проверку. Пример из [свежего P49](https://github.com/lubochka/llm_mvp_core/blob/5afb9d265fb62db8c9d4ea00f61e5f2ed9e9fc3a/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/codex-experiment-review.md): общий gain соседствует с 3/4 против 3/4 на NEW_ONLY, где ceiling тоже 3/4.

До обучения проверить exact IDs и source-family disjointness, а также отношение фактических input/target к TRAIN. Новый RowId, файл или target не делает знакомый input новым. Если единственный train case просто копируется в heldout под новым ID, gate обязан отказать.

Проверка writer/decision без новой модели: удалить все rare-law outputs при неизменном общем числе за счёт дублей common; подставить general score вместо missing security; заменить null на 0. Валидатор обязан выявить каждую подмену, а решение о пригодности опыта — использовать этот отказ. Один лишь список предупреждений рядом с прежним ShouldScaleUp=true не завершает ремонт.
