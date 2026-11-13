# MICRO-26 — минимальная причинная проверка: помогает ли добавить Current в обучаемую память E30?

**Дата:** 16.10.2026. **Состояние:** конкретный work order готов; Python proxy выполнен; исходный native .NET A/B **не выполнен**, blind CODEX/Astra quality judge **не запускался**, продуктовый consumer не проверен. Это **не** командa немедленно исправлять RP01 Student и **не** 500-DPO run. Основание: [source-backed NEW-26](26-NEW-E30-PREDICATE-CACHE-STATE-ALIAS-AND-MISREAD-2026-10-16.md).

## Почему это важно

У представления одного C# файла есть несколько переходов между логическими предикатами. Вектор всего файла для них одинаков, правильный следующий предикат — необязательно. Если в KNN нельзя указать, из какого состояния предсказывается следующий шаг, дополнительное обучение почти не может устранить путаницу. Микроопыт проверяет именно это свойство, а не абстрактную «силу моделей».

**H1:** при том же наборе supervised пар и том же KNN ядре добавление Current к ключу запроса/записи повысит честную точность Next на новых исходных C# файлах без ухудшения остальных переходов.  
**H0:** Current не улучшает качество при одинаковом бюджете и одинаковом split; либо старый bigram fallback/кодовый path уже решает проблему.  
**Решающее наблюдение:** не поведение созданного вручную key-value mock, а качество **реального E30-кэша** на новых source families и изменение ответа от полученного *обучением* состояния.

## Имеющиеся исходные доказательства

- [E30PredicateCache.cs:21–64, 91–104](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E30PredicateCache.cs#L21-L104): train/test files, пары (Vec, Current, Next), Add(Vec,Next), Lookup(Vec), fallback bigram(Current). Реальный split по file order 70/30, а не строго новым семействам.
- [PredicateCache.cs:15–43](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Infrastructure/PredicateCache.cs#L15-L43): first-on-cosine-tie, 22D full-file feature.
- [Оригинальный отрицательный tier2 receipt](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/results/E30-PREDICATE-CACHE/results/run-2026-05-01-001.json): 276 testing pairs, 128 cache hits, 2.3% correct среди hits, enhanced overall 16.67% против bigram 32.25%.
- [Executed synthetic proxy Python](26-MICRO-E30-CONTEXT-IDENTITY-PROXY.py) / [result JSON](26-MICRO-E30-CONTEXT-IDENTITY-PROXY-RESULT.json): модельное тождество; baseline 6/12 vs conditioned 12/12, corrupted-label 6/12. Это **не** .NET и **не** оценка новых семантических семейств.

## Исполняемый протокол: один фактор, не меняем полпроекта

**Минимальный target:** создать изолированный экспериментальный runner / test under existing ExperimentsV2 test harness. **Production files не менять**, не включать предлагаемую функцию по умолчанию. В теневом treatment допускается локальная копия интерфейса PredicateCache, использующая контекст Current; максимальная модельная сложность — тот же cache storage, тот же similarity и threshold, плюс one-hot equality filter по Current перед KNN. Изменяемая переменная — **единственно условная идентичность запроса**, без нового обучающего корпуса, tokenizer, weights, "голоса" oracle во время inference или изменения данных.

| Плечо | Train (одинаково) | Query / output |
| --- | --- | --- |
| A: исторический E30 | Labelled transition (fileVec, current, next) → Add(fileVec,next) | Lookup(fileVec) и *существующий* fallback bigram(current) |
| B: controlled | Те же pairs, порядок и budget → Add((fileVec,current),next) | Lookup((fileVec,current)); при miss тот же fallback; одинаковый score/threshold |
| C: negative order control | Та же модель A, но порядок двух противоположных labels перевёрнут | Если winner следует первому label при равных векторах — first-wins подтверждён, но это не измерение полезного качества |
| D: corruption guard | Те же входы B, метки для одного current только в TRAIN заменены на заведомо неправильные | Изменение B output доказывает reliance on learned labels, а не скрытый hard-coded If(Current)→Next |

**Набор данных:** 12–20 небольших законных C# файлов/семейств, утверждённых CODEX как human selector, с несколькими AST-предикатными переходами; 6–10 train families и 6–10 *других* blind families, минимум 24 оценочных перехода. Split группировать по исходному файлу, синтаксическому шаблону/репозиторию/семантическому семейству, чтобы дубликаты, rename-и и почти одинаковые templates не попали по разные стороны; закрепить SHA и seed. Нужны пары с одинаковыми 22D частотами и разными Next, также контрольные однозначные пары и редкие/неизвестные Current. Проверить отсутствие train/test near-duplicate не только путём FileId.

**Oracle:** correct next predicate определяется независимой AST-проверкой + human-role CODEX/Astra 6 UltraCode до раскрытия arms. Это supervised KNN-исследование, **DPO здесь не требуется**. Если всё-таки нужны DPO, оба chosen/rejected генерирует только разрешённая внешняя открытая китайская модель; независимую проверку качества каждой пары **выполняет CODEX/Astra 6 UltraCode**.

**Логи для каждой пары:** source hash, family, original predicates, transition index, (Current,Next), vector hash and shape, candidates before/after, selected neighbor and insertion position, cache hit vs fallback, expected Next, returned Next, lawful correctness, latency. Не использовать число ASSERT/PASS как научную метрику.

## Оценка качества и блокирующие условия

Вывести paired confusion по переходам, file-family macro-accuracy, correct Next@1, cache-hit precision, hit coverage, fallback accuracy, сколько ошибочных решений было принято при высоком cosine; отдельно время построения, поиска и память процесса. Регистрировать абсолютные значения и дельту **B−A**, диапазон неопределённости (с маленьким n не выдавать её за population guarantee), семьям с zero coverage ставить 0/NA без исключения из общей оценки. Слепой экзамен не должен выбирать alpha/threshold post hoc.

**Решения:**
1. Если одинаковый вектор создаёт противоречащие метки и native B улучшает correctness на новых семействах без critical regression, это поддерживает *E30 representation mismatch*; дальше можно переносить **не конкретный mock**, а source-conditioned KNN feature в подходящий RP02 consumer после trace.
2. Если результат только на synthetic proxy либо на тех же train templates — **HYPOTHESIS_OPEN**, не продвигать production и не объявлять success.
3. Если рост hit-rate сопровождается падением correctness/precision, считать ухудшением независимо от числа найденных neighbours.
4. Если в текущем продукте нет actual caller обученной памяти E30, честный итог — **HISTORICAL_FINDING_NOT_ACTIVE_RP01**, а не правка в случайной LoRA модели.
5. Если paired B−A ≤ 0 на новых семантических семействах, отвергнуть полезность Current-only изменения и испытать отдельно AST local scope/role/position, **не соединяя** эти изменения в одну причинную проверку.

## Контракт исполнителя и независимой проверки

**CURRENT TASK:** reproducible native .NET E30 KNN query-key ablation + trace actual consumer.  
**NOT THE TASK:** 500 DPO, Windows CI, widespread code refactor, изменение алгоритма студента RP01 или защищённого каталога core.  
**Allowed files:** новый экспериментальный runner/test в test/experiments области core — **только после отдельного разрешения на код**, и публикация result/write-up только в planning possibleDifs. Сейчас разрешён **документ**, код продукта здесь не изменялся.  
**Forbidden files now:** core/docs/plans/replanning-possible-diffs/* (не редактировать), frozen archival E30 receipts, RP01 C#500/ENRU500 datasets/checkpoints.  
**Skills:** architecture human-readable-architecture-audit-SKILL, phase-planning-SKILL, plan-review-SKILL, code-examination-SKILL, experiment-design-SKILL, claim-verification-SKILL, prompt-patch-authoring-SKILL, conversation-intent-and-self-reflection-SKILL under [docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills). Read each before execution; independent reviewer compares actual code + plans against these gates.

**Work order to CODEX / Astra 6 UltraCode:** прочитай полностью указанные три файла и реальных caller/consumer; до модификации сформулируй фальсифицируемое предположение; реализуй только экспериментальный shadow comparator; сфиксируй исходные SHA/files/seed, blinded labels и native logs; проведи A/B/C/D; опубликуй source+artifacts+модельную оценку; другой CODEX/Astra reviewer, не author пары/проверки, заново прочитает native evidence и вынесет PASS/FAIL/INCONCLUSIVE. Если code runner будет создан, приложи caller→train→state→query→output trace и code-review проверки; не называй документацию выполненным экспериментом.

**Expected artifacts:** fixture JSONL с SHA + family split и label provenance; CSV all transitions A/B/C/D; wall/memory; README результатов с native command, reference core SHA и RESULT_TYPE; отдельный CODEX/Astra reviewer verdict; короткая рекомендация RP02 integration только при positive + source-traced consumer.

## Как запустить только имеющийся дешёвый proxy

В каталоге possibleDifs выполнить **python 26-MICRO-E30-CONTEXT-IDENTITY-PROXY.py** (stdlib only). Он проверяет инвариант представления и печатает JSON в stdout. Фактический запуск 16 октября: 12 synthetic heldout, A=6, B=12, D=6. **Не переносить эти цифры в показатель качества обученной машины.**

**Контроль итогового утверждения:** даже native положительный E30 на новом наборе не докажет, что RP01 LoRA Student научился; для этого уже есть отдельные [REC02/REC03](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md). Правильный следующий шаг — восстановить и измерить каждый получатель learning signal отдельно.
