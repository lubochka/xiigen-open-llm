# MICRO-N26-VERIFIER-POLARITY-01 — дешёвый причинный опыт над реально обученным Unit

<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Актуализация доказательств 01.11.2026 — P55/P56/P64

Узкий can’t source-law transfer в EN→RU KNN уже принят; whole-token ready/already и source polarity — разные checks. Для будущего рецепта сохранять actual tokenizer signature, learned law и reached tier; target compatibility до завершения tier и all-candidates-rejected fallback проверяются явно. Component gains не закрывают ordinary facade, особенно когда exact memory маскирует вмешательство. Старый budget не увеличивать и новый Train для коррекции report не запускать.

Основание: [2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md), закреплённый core `790228c256168a336b01abae4387e28391048c0c`. Старый текст и его ограничения относятся к указанной там дате; новый блок уточняет текущий статус, не переисполняет исторический опыт и не меняет научную очередь.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**Версия:** 1, 2026-10-16. **Режим:** EXECUTABLE EXPERIMENT SPEC / not yet native-executed. **Привязка:** [новый code-audit N26](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md), [immutable diagnostic fixture](26-MICRO-VERIFIER-POLARITY-FIXTURE-2026-10-16.json). **Source SHA:** `llm_mvp_core@b0afc08dc0df110b10ed5ed38b518906665c16f8`. **Неприкосновенно:** `llm_mvp_core/docs/plans/replanning-possible-diffs/**`; RP01 frozen B0/C50/C100, datasets/results/expected answers; никаких CI Windows.

## Вопрос, который можно опровергнуть

Остался ли в _обученном_ `QualityVerifierUnit` только lexical `required word`, так что проверка допускает ответ, явно отрицающий известный источник, или другие learned rules в фактическом consumer уже блокируют такую ошибку? И если добавить полученный из **training-only** law `predicate + signed relation`, устраняется ли хотя бы часть неверно принятых ответов без потери правильных?

**Точная гипотеза H26-V:** при одинаковых выученных required-term правилах и одинаковых candidate texts, нынешний Verifier примет как правильные некоторые семантически противоположные ответы; shadow-verifier с проверкой полярности, созданной из train law, уменьшит `wrongAccepted` на заранее зафиксированных отклонённых candidate texts. **NULL:** нынешние правила уже различают их / shadow не улучшает `wrongAccepted` / прирост достигнут утечкой labels.

Это **не** тест «DPO/LoRA стали обучаться» и **не** подтверждение генерализации на новый source: тест держит source-условия прежними, а правильные/неправильные **тексты ответов** новыми. После механизма потребуется отдельный family-disjoint опыт через обычный Flow.

## Входные данные и методы

- [Fixture N26](26-MICRO-VERIFIER-POLARITY-FIXTURE-2026-10-16.json): ровно 4 source-law `G01…G04`, 16 новых ответов: 8 lawful / 8 wrong. Пример: `Start the process`, learned requirement `процесс`, кандидаты `Процесс запущен` и `Процесс не запущен`. Источник значения law подписан до выполнения и не берётся из метки `lawful_by_source`.
- `Training`: 4 synthetic `TranslationLearningExample`, по одному на источник, только для `QualityVerifierUnit.TrainOnExample`, с `HeldOut=false` и required-term, извлечённым из заранее допущенного train law. Все 16 `candidate.answer` **запрещено читать для построения train rows**. Для построения ответной оценки нельзя использовать `lawful_by_source`.
- `A baseline`: реальная `QualityVerifierUnit.Verify(candidate, originalTokens)` с реальным `TranslationSemanticTokenizer` из [TranslationTrainedUnits.cs:1052–1092](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/27.TranslationDomain/TranslationTrainedUnits.cs#L1052-L1092).
- `B enhanced`: тот же learn-state и те же 16 answers, **без переобучения**; shadow post-filter проверяет соответствие **выученного из train `law_id` signed relation** (action+polarity) с ответом. Парсер грамматики версионирован; в нём **нет fixture id, labels, expected Russian string или per-case overrides**. Если нет обобщаемого признака, arm B объявить `INVALID_INTERVENTION`, а не подсмотреть правильную сторону.
- `C negative control`: не обучать required-term в Verifier; этот arm выявляет, не сравниваем ли просто существующее лексическое правило с новым полным предметным оракулом.
- `D sham control`: добавить тот же дополнительный парсер/обёртку без signed-law check, исключить эффект иной маршрутизации/времени.

**Управляемый фактор:** только representation/application learned semantic law при оценке candidate. Не менять токенизатор, набор соседей, chosen/rejected, LoRA, длину контекста или source constraints. Одна и та же frozen candidate list в A/B/C/D.

## Исполнение: один процесс, без CI

1. В отдельной scratch-directory (не в core docs) зафиксировать `git rev-parse HEAD`, SHA256 fixture, SHA source files `TranslationTrainedUnits.cs`, `TranslationSemanticTokenizer.cs`, `TranslationMachine.cs`; снять preflight record. Не объявлять run существующим из одного плана.
2. Небольшой `.NET 9` console harness с `ProjectReference` на `27.TranslationDomain.csproj`; один экземпляр Unit на arm. Загрузить **4** обучающих rule examples через настоящий public `TrainOnExample`. Проверить, что исходный Unit действительно знает каждое term: заведомо пустой кандидат обязан давать `Pass=false`. При невыполнении — `PRECONDITION_FAILED`, нельзя проводить сравнение качества. Если выбранный `TryParseRequiredTerm`/signature route не сохраняет термин, не редактировать fixture под labels; устранить только ошибку создания train-law в harness, либо прекратить.
3. Запечатать train-state/counters и только затем прочитать список из 16 candidate texts и семантическую разметку отдельно. Для каждого записать `source_signature, required_terms, law_id, candidate_hash, baseline_pass, enhanced_pass, issues, reason/evidence, elapsed_ms`. Разметку истины подключить **после** inference при scorer. В реальном отчёте перечислить источник независимого экспертного решения, а не выдавать написанные для диагностического fixture labels за решение CODEX/Astra.
4. Провести A/B/C/D с теми же 16 candidate texts. Выход: `case-events.jsonl`, `comparison.json`, `preflight.json`, `build-exit.json`, `source-sha.json`; результаты записать **в новую run папку**, не в сохранённые RP01 outcome файлы. Вся работа — не более 4 seed train rows и 64 оценок, без Student/Teacher/DPO.
5. Если изменение на уровне Unit обнаружено, **только как второй необязательный шаг** проследить один похожий case через `TrainedTranslationAdapter.TranslateAsync → QualityVerifierUnit.Verify → TranslateAsync result`: [TranslationDirectionAdapters.cs:117–139](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L139). Учесть ранний `Executor` exact/phrase/word-map и phrase-map verifier bypass. Без обычного consumer наблюдения вывод «RP01 улучшен» запрещён.

**Заранее фиксируем метрику:** `wrongAccepted` среди 8 отрицательных answers (меньше — лучше); второй обязательный показатель `correctAccepted` среди 8 положительных (не должен снизиться). `Abstention` и throughput обязательны как цена. Основное условие `PILOT_GAIN`: B уменьшает `wrongAccepted` минимум на 2 из 8 при `correctAccepted_B >= correctAccepted_A`, при отсутствии leaks и прохождении preflight. Если B всего лишь отказывает всем, это FAIL, а не gain. `PILOT_GAIN` — локальный условный сигнал; **не** утверждение о generalization или улучшении RP01 TextGenerator. `NO_GAIN`, `INVALID_PREMISE`, `INCONCLUSIVE` — допустимые конечные научные исходы, не повод подогнать assertions.

## Одна уже проверенная source-level контрольная арифметика — не модельный эксперимент

Сохранённая [per-case запись выполненного JS source-proxy](26-MICRO-VERIFIER-POLARITY-SOURCE-PROXY-RESULT-2026-10-16.json) содержит все 16 вычисленных решений, SHA входного fixture и явное предупреждение, что это не обученный .NET Unit. По строковым значениям immutable fixture на 16 candidates, примитив `Contains(required_term)` истинный **для всех 16**, включая **все 8 семантически неверных**. Это было вычислено отдельным JS shadow-predicate после сохранения fixture; само по себе не доказывает, что реальный Verifier научится required term, что остальные правила отсутствуют, или что его consumer предъявит именно эти варианты. Контрольные ожидаемые числа для прозрачности: `lexicalCorrectAccepted = 8/8`, `lexicalWrongAccepted = 8/8`. Никаких C# training/inference вызовов в этом исследовательском проходе не было.

## Executor prompt — один узкий пакет работы

**Для CODEX executor:** «Работай с `MICRO-N26-VERIFIER-POLARITY-01`. Сначала открой действующие skill/AGENTS по архитектору, experiment-design, code-examination, prompt, plan-review, human communication. Прочитай полностью `QualityVerifierUnit.TrainOnExample/Verify`, tokenization, `TranslationLearningExample`, `TrainedTranslationAdapter` и 26 fixture. Подтверди SHA. Реализуй только изолированный диагностический .NET console harness в scratch, использующий настоящий `QualityVerifierUnit`; не клонируй core law code. Freeze train/state/fixture до обращения к labels. Запусти A/B/C/D, запиши per-case results и negative controls, отдели string proxy от native result. Не изменяй никакие документы в `llm_mvp_core/docs/plans/replanning-possible-diffs`, frozen RP01 datasets/checkpoints, не запускай Windows CI и не расходуй 500 новых DPO. Дай отчёт на русском: `PRECONDITION / wrongAccepted / correctAccepted / abstention / evidence + SHA / статус гипотезы / что осталось без доказательства`. Если построить generic signed law из train-only данных нельзя — `INVALID_INTERVENTION`, не изобретай label-based regex». 

## Plan-review и право на вывод

`SK-ARCH`: comparison local Unit, product RP01 route not assumed. `SK-PLAN`: один изменяемый механизм и четыре контроля. `SK-CODE`: call chain read before runtime and no shadow implementation of original Verifier. `SK-PROMPT`: ни один будущий агент не вправе говорить «proved gain» из source proxy. `SK-HUMAN`: в отчёте указывать, _что изменилось в качестве ответа_, не число зелёных GitHub runs. Независимый review и настоящие native результаты **не получены**; статус **DRAFT / ready for bounded implementation, not APPROVED**.
