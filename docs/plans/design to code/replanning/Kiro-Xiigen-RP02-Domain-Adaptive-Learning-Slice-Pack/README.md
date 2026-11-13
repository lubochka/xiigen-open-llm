# RP02 · Предметные представления и обучаемый adaptive kNN

**Редакция 7.3 · адресный ремонт 16 октября 2026 · AUTHOR-REVIEWED PLANNING DRAFT.** Существующий пакет сохраняет 18 основных карточек, три дочерних packet, единый вход и предметную методику. Это не новый цикл R01–R25 и не утверждение, что алгоритмы уже реализованы/обучены. Независимый Astra review не проводился.

## Единственный маршрут исполнителя

[Старт одного задания](RP02-V7-START-PROMPT.md) → [доска и зависимости](EXECUTION-BOARD.md) → **одна** карточка из [tasks-v7](tasks-v7/) + [её effective context](RP02-EFFECTIVE-CONTEXT.md). [Manifest](PACKAGE-MANIFEST.json) содержит точные IDs/пути; [совместимое второе имя manifest](RP02-V7-PACKAGE-MANIFEST.json) имеет тот же контракт. `tasks/`, V6A/V7/V7.1 архивы и RP02 Retrieval sibling не являются вторым dispatcher.

## Как быстрее дойти до настоящего урока

Параллельно готовятся 00A trace, **один** готовый паспорт C#/EN→RU/SQL, минимальные 01D law features, 02B1/S и 03A/S. 00C нужен только при подтверждённом missing ordinary edge. Затем переиспользовать scorer либо выполнить [02B1 CODE](tasks-v7/children/RP02.02B1-CODE.md); [02B1 UPDATE](tasks-v7/children/RP02.02B1-UPDATE.md) делает ровно один новый разрешённый θ step; сразу [03A/E](tasks-v7/RP02.03A.md) сравнивает C0/T на независимом доменном материале. Trial сохраняется и читается изолированным обычным code path, но не принимается автоматически.

Первый scorer уже должен использовать релевантные **предметные законы**, а не только близость соседей. Один готовый домен не означает отказа от domain-specific алгоритма. Первый урок не ждёт полного токенизатора, всех доменов, typed fusion, learned-k, LoRA, соседних слайсов или Windows. Эти функции **сохраняются у своих владельцев**; широкий domain/hybrid/PC claim требует их фактических результатов.

## CX10–CX12 · 16 октября: проверены 21 packet, адресно исправлены шесть

[Дополнение Atlas: находки, примеры, исправления и границы проверки](../RP01-RP05-REPAIR-ATLAS-2026-10-15/RP02-CX10-CX12-BOUNDED-SEARCH-AND-STREAM-EXAM-2026-10-16.md). Прочитаны все 18 основных карточек и три дочерних; новая конкретика внесена в **пять основных и одну дочернюю**, а не во все заново. Сохранены CX01–CX09 и основной маршрут; новое число задач, V8 и общий цикл проверок не создавались.

[PR №16](https://github.com/lubochka/xiigen-planning/pull/16) объединён в `llm_plan3`, merge **`0150cef44594920216265a109802f448eac582fb`**. Отдельная правка 01C опубликована ранее в **`2f884cf17f85c767b8e7e18b0485a051ac298ffe`**. Первое промежуточное дополнение Atlas — **`0cb0467b7f09d0bcbb9e40c5d8a270c4b1d0d116`**. Это применённые изменения документов, не исправленный/обученный core.

| Физическая карточка | Предметная конкретика текущего ремонта |
|---|---|
| [01C: LoRA → semantic vector](tasks-v7/RP02.01C.md) | CX11: реальные space/version/width/finite/zero admission; короткий вектор не сравнивается с длинным через игнорирование хвоста. Существующий защищённый fixed-space путь не получает лишнюю миграцию. |
| [02B2: adaptive k и поиск](tasks-v7/RP02.02B2.md) | CX10: ef/topK не равны числу посещений; одинаковые scores, реальная cooperative cancellation, общий бюджет probe/expansion и наблюдаемое окончание работы. |
| [02B2/LEARN: обучаемая политика](tasks-v7/children/RP02.02B2-LEARN.md) | Завершённый action, bounded stop, timeout с продолжающимся CPU и ненаблюдавшийся action дают разные evidence. Стоимость и negative labels не выдумываются. |
| [02C: exact/ANN](tasks-v7/RP02.02C.md) | CX10/11: одинаковые допустимые координаты; прерванный brute-force scan не становится exact full-pool oracle. Подмножество и неизвестный reference обозначаются явно. |
| [03B: feedback и bulk](tasks-v7/RP02.03B.md) | CX12: prediction до correction, диагностика уже выученного случая и независимый экзамен разделены; unique lessons/steps/families и все отсеянные/неудачные материалы учитываются. |
| [03A: независимый экзамен](tasks-v7/RP02.03A.md) | Применяет эти различия только к выбранному опыту; считает полную стоимость и сохраняет missing/invalid evidence, не выполняя update и не требуя новую telemetry-платформу до первого урока. |

У каждой правки есть actual source anchors, фазы/права, GOOD/BAD/EDGE и причины, опровергающий пример, восстановление и конкретный результат. Новые подробности находятся **в самих карточках**; читать весь Atlas перед заданием не требуется. Риск shared SetEfSearch уже был учтён и не объявлен новой находкой. CX11 — конкретизация существующей совместимости представлений; CX12 — уточнение метода, не найденная production-авария. Все числовые/языковые примеры — PUBLIC объяснения, не реально созданные Chinese DPO и не SEALED задания.

Проверка публикации: candidate `dd927c3aeb9ec6702e1221f5fcc13431bce1a24f` → merge не меняет RP02, сохраняя параллельную работу RP04. В merge повторно прочитаны предметные участки 01C/02B2/03A и оба manifests; manifests имеют один blob `863797be89d62900709b8c6f7e6110fe593eb59d`. Полный повтор всех RAG/WorkLeft/R01–R25 не заявляется. Первый урок остаётся **ready domain → law features → reuse/CODE → один UPDATE → 03A/E**.

## CX07–CX09: дополнительная проверка и пять исправленных карточек

[Новые source/design findings](../RP01-RP05-REPAIR-ATLAS-2026-10-15/RP02-CX07-CX09-STATE-MAPPING-AND-RETRIEVAL.md) опубликованы промежуточно в `430728ac5f15fa1b88c56385a7637404130c9182`. Затем инструкции внесены **в пять существующих физических карточек: четыре основные и одну дочернюю**, без нового dispatcher, task ID или эксперимента. Промежуточный статус `IN_PLACE_INTEGRATION_PENDING` в исходном checkpoint описывает момент его публикации, не текущий статус этих пяти правок. Это авторский ремонт плановых заданий, не исправление core или независимая приёмка.

| Карточка | Новая конкретика и граница применения |
|---|---|
| [00A: действительный путь](tasks-v7/RP02.00A.md) | CX07: кто владеет изменяемыми payload/arrays; `with` не deep-copy. Отдельно учтены новый EvidenceIds и свежий приватный Vectorize, чтобы не приписывать им ложную внешнюю мутацию. |
| [02B1/CODE: минимальный scorer](tasks-v7/children/RP02.02B1-CODE.md) | CX07: только доказанно открытая typed admission/egress граница; reuse immutable/exclusive ownership, без generic deep-cloner и копии всего индекса на каждый урок. |
| [01A: domain mapping](tasks-v7/RP02.01A.md) | CX08: UsedKnn confidence floor 0.90 не означает 90% точности; собственный предыдущий confidence не новое независимое evidence. C01 не становится prerequisite первого выбранного домена. |
| [02C: exact/ANN](tasks-v7/RP02.02C.md) | CX09: exact cosine reference и domain-law scorer reference различаются; два recall, first-lost-stage diagnosis, ties/empty/support/cost. Только существующий C05. |
| [03A: независимый экзамен](tasks-v7/RP02.03A.md) | Применяет ownership к выбранным arms, CX08 только к C01, CX09 только к C05. Не запускает все проверки перед C03 и не повторяет applied lesson при испорченном измерении. |

CX07/CX08 опираются на конкретный исходный код, но обычная runtime-мутация, ошибка потребителя confidence и прирост качества не наблюдались. CX09 уточняет уже существовавшее требование rare-law recall; это не третья обнаруженная production-авария. Каждая изменённая карточка содержит исходники, шаги, GOOD/BAD/EDGE, причину, опровержение, восстановление и результат. Весь Atlas читать перед её исполнением не требуется.

**Ближайшее действие не изменилось:** один ready domain → информативные law features → reuse/CODE → один разрешённый UPDATE → 03A/E. Calibrated mapping, ANN reference и остальные домены расширяют соответствующие claims позже, а не задерживают этот урок. Проверяется только реальный риск неизменности данных на уже выбранном пути.

## Адресный follow-up CX01–CX06: исправлены сами задания

[Новые source/design findings в Atlas](../RP01-RP05-REPAIR-ATLAS-2026-10-15/RP02-CX01-CX06-TARGETED-REPAIR.md) были опубликованы промежуточно в `bfa515b4750fdfbaa161f3cb696e7fec5e7c2fe2`, после чего уточнения внесены в **шесть существующих карточек, включая две дочерние**, и effective context. Это не шесть новых последовательных запусков и не шесть доказанных production incidents.

| Карточка | Что уточнено |
|---|---|
| [01B: представление](tasks-v7/RP02.01B.md) | Реальный overload, общий смысл координат и отдельный контрпример потери before/after; минимальная миграция, UNKNOWN, источник и caller. |
| [01D: предметные признаки](tasks-v7/RP02.01D.md) | Признак запроса, кандидата или их отношения; информативность после hard filtering; отсутствие скрытого oracle в runtime features. |
| [02B1/UPDATE: первый урок](tasks-v7/children/RP02.02B1-UPDATE.md) | Допущенный контраст действительно доходит до data-gradient; once/rollback/cold и отдельная оценка сохранены. |
| [02B2/LEARN: dynamic k](tasks-v7/children/RP02.02B2-LEARN.md) | Явные взаимодействия контекст × действие вместо сокращающегося общего контекста; реальные outcomes и доступность признаков. |
| [02D: следующий шаг](tasks-v7/RP02.02D.md) | Byte/BPE/слово/AST — разные единицы; допустимый незавершённый префикс не равен неверному конечному результату. |
| [03A: доменный экзамен](tasks-v7/RP02.03A.md) | Сопоставимые cold/warm/cache/order/deadline; таймауты, отказы и ошибки среды не исчезают из учёта. |

Для каждой адресной правки есть конкретные GOOD/BAD/EDGE, причины, ограничения эффекта, источник, опровержение и восстановление. Не нужно читать весь Atlas перед исполнением: нужная конкретика уже находится внутри назначенной карточки. Остальные карточки не объявляются заново переписанными или полностью проверенными этим историческим follow-up.

## Сохраняемая архитектура

Domain/intent/direction/version → предметные token/vector/AST/POS/referent признаки → разрешённый source pool → domain-law scorer θ → контекстный search effort/k → typed consumer → final arbiter → новый разрешённый учебный сигнал. Права/типы и доказанные законы не торгуются за similarity; неоднозначная грамматика остаётся гипотезой. Negative memory учит, чего избегать, но не становится accepted positive output.

RP01 — Student response-DPO/LoRA; RP02 — retrieval/ranking/next-step; RP03 — Unit many→many и Flow corrections; RP04 — async Orchestrator/higher-order Graph; RP05 — continual learning. RP18 сохраняет ownership проекции; LoRA не переименовывается в tokenizer. DTC/IDE и другие предметные требования не закрываются тремя RP02 экзаменами.

## Проверять способности, а не процент покрытия

[Алгоритм и минимальные проверки](RP02-V7.2-LEARN-FIRST-RUNBOOK.md) · [C01–C08, три домена и bulk-to-bulk методика](RP02-V7.2-FUNCTIONAL-EXAMS-AND-TRACEABILITY.md).

Source/plan/review/no-change: **0 новых product tests**. Для изменённого hard invariant — ближайшая существующая проверка; новая только под непокрытый изменённый риск. Рост точности, перенос, retention и экономичность устанавливаются независимым экспериментом, не Windows/coverage. C0/T различаются объявленным обучаемым состоянием; P — необязательный исторический comparator. Судья получает opaque labels. Update, consumption, output change, quality, cost и promotion — отдельные результаты.

**DPO роли:** CODEX с requested model **astra 6 ultracode** выполняет делегированный человеческий выбор. Допущенная внешняя открытая китайская модель пишет **обе** final chosen/rejected стороны. Отдельный CODEX с **astra 6 ultracode** оценивает exact unchanged pair. Фактические IDs/receipts не выдумываются. Independent code/science reviewer — ещё один отдельный реальный вызов.

## История и границы

[Первый промежуточный аудит](../RP01-RP05-REPAIR-ATLAS-2026-10-15/05-RP02-REVIEW-IN-PROGRESS.md) и [применённый ремонт PR14 / F17–F32](../RP01-RP05-REPAIR-ATLAS-2026-10-15/08-RP02-APPLIED-REPAIR-AND-ADDITIONAL-FINDINGS.md) остаются своим историческим evidence, не новым выполнением. Прежние тексты сохранены Git на P `78ccad8763adc9fd6bf6499f27260c2e9c4c8bab`. Пользовательские результаты соседних слайсов не переписываются.

`CORE_UNCHANGED_BY_THIS_REPAIR; PRODUCT_TESTS_ADDED=0; DATA_WEIGHTS_UNCHANGED; TRAINING_NOT_RUN; DOMAIN_GAIN_NOT_MEASURED; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED`. Исполнимый prompt и его публикация не равны исполнению алгоритма или измеренному улучшению.
