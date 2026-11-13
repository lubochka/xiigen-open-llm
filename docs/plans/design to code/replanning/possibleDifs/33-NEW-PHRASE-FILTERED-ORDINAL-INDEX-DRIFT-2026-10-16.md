# N33 — новое адресное расхождение: после фильтрации source-токенов word-map читает исходный токен по чужому индексу

**16.10.2026 · P1 для обучаемого Translation ExecutorUnit; SOURCE-CONFIRMED MECHANISM; QUALITY IMPACT = HYPOTHESIS; NATIVE .NET = NOT_RUN.** Только `xiigen-planning@llm_plan3/possibleDifs`; protected `llm_mvp_core/docs/plans/replanning-possible-diffs` READ ONLY.

## 1. Что обнаружено и почему это отдельное расхождение

В обучаемом `ExecutorUnit` фразовый сборщик строит укороченный массив `sourceTokens`: каждый исходный токен сначала нормализуется, после чего токены, состоявшие только из пунктуации/символов, удаляются. Цикл перебирает **позицию в укороченном массиве** `i`, но при попытке дополнить перевод словарём использует `tokens.Tokens[Math.Min(i, tokens.Tokens.Count - 1)]` — **позицию исходного, нефильтрованного массива**. Если отброшенный символ стоял до слова, значение `i` больше не указывает на это слово. Вместо нужного лексического соответствия система может вставить перевод **другого** слова или отказаться от корректно покрываемого предложения.

Это **не** уже описанные N21 «неизвестные слова выпадают», N30-03 «удалённый `->` создаёт ложное соседство для phrase matching», N30-01 «жадная сегментация не ищет альтернативы» и N29 «первое слово переводится в первое слово». Здесь даже при **правильном phrase match** и двух точных **выученных word mappings** выбирается запись не того оригинального токена.

## 2. Прямая трассировка исходников

1. [SemanticTokenizer.Tokenize, L98–114](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationSemanticTokenizer.cs#L98-L114): вход режется по ASCII пробелам, самостоятельные `—` и `->` остаются элементами `tokens.Tokens` и не выбрасываются в этом методе.
2. [ExecutorUnit.TryComposeFromPhraseMappings, L798–859](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L859): `sourceTokens = ... Select(NormalizeMapKey).Where(nonempty).ToArray()` (L804–808); `i` индексирует отфильтрованный массив (L824–845), **однако** `var token = tokens.Tokens[Math.Min(i, tokens.Tokens.Count - 1)]` (L837). Исходный `originalIndex` не сохраняется.
3. [NormalizeMapKey, L1008–1023](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1008-L1023): ветка обычной строки оставляет Unicode letters/digits, `_`, `$`, whitespace; standalone `—` превращается в пустую строку.
4. [ExecutorUnit.Execute, L264–277](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L277): после exact-source path фразовый сборщик имеет **приоритет** перед полным word-map и KNN. Поэтому уверенный, но неверный phrase candidate может скрыть корректный поздний маршрут.
5. [ExecutorUnit.TrainOnExample, L404–466](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L466): DPO-derived chosen заполняет словарь и phrase mappings; [TranslationMachine.TrainBatchAsync, L51–119](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L51-L119) вызывает train на получателе.
6. [TrainedTranslationAdapter.TranslateAsync, L117–140](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L140): ordinary consumer вызывает `_semanticTokenizer.Tokenize` → `_executor.Execute` → `_verifier.Verify`; это **не** Student `TextGenerator` в RP01. Существенно: [L127–139](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L127-L139) разрешает кандидату `-phrase-map` с `Confidence >= 0.85` пройти **даже если `verification.Pass=false`**. Phrase-map в [ExecutorUnit L270–271](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L270-L271) получает `0.86`. Значит, поздний Verifier обычно **не отсекает** эту категорию ошибок; возможны дальнейшие ограничения в Product/Flow, их надо отдельно трассировать. Сам bypass — уже существующая находка N16-06, не новизна N33.

**Доказано по коду:** существует несоответствие двух индексаций после фильтрации. **Не доказано до native A/B:** что нужный fixture обучен/допущен, какой путь реально побеждает в TranslationMachine, насколько часто это ухудшает перевод, и тем более что этот баг объясняет C#500/ENRU500 Student LoRA `0/3,0/4`.

## 3. Минимальный контрпример — источниковый proxy, не .NET результат

Замороженная законная память RU→EN: `PhraseMap[«проверь»] = «check»`; `WordMap[«проверь»] = «check»`; `WordMap[«файл»] = «file»`. TRAIN-семейства отличаются от входного предложения, exact-source полного запроса в памяти нет. Gold для предложений, где `—` служит **только разделителем**, — `check file`.

| Вход (исходные токены) | Укороченные токены | Текущий выбор word-map по `tokens.Tokens[i]` | Требуемый выбор по `originalIndex` |
|---|---|---|---|
| `проверь файл` | `проверь, файл` | `check file` | `check file` |
| `— проверь файл` | `проверь, файл` | `check check`, coverage 100%, приём фразового результата | `check file`, coverage 100% |
| `проверь — файл` | `проверь, файл` | `check`, coverage 50%, фразовый результат отвергнут | `check file`, coverage 100% |
| `проверь файл —` | `проверь, файл` | `check file` | `check file` |

Этот контрпример вычислен **source-logic proxy**, в котором воспроизведены нормализация, выбор phrase или word-map и порог acceptance. [Код proxy](33-MICRO-PHRASE-INDEX-SOURCE-PROXY.mjs) и [receipt](33-MICRO-PHRASE-INDEX-SOURCE-PROXY-RESULT.json). Проверка на реальных `TranslationLearningExample`, tokenizer, validator и cold restore ещё **NOT_RUN**; proxy не симулирует полноту нативной системы.

## 4. Доменная и причинная граница

- **RU→EN/EN→RU и другие языки:** имеет значение, если punctuation является отдельным whitespace-delimited токеном, phrase-map покрывает часть source, а остаток покрывает word-map. Где точное совпадение/полная phrase покрывает всё, неверный `tokens.Tokens[i]` может не использоваться. Для полноценной языковой грамматики разрешать/игнорировать пунктуацию следует по доменному закону.
- **DSL/Graph/code documentation:** `->`, `=>`, `!`, скобки часто семантически значимы. Сохранение `originalIndex` устраняет лишь **неверный lookup**, но **не устраняет** отдельную N30-03 ошибку ложного phrase match через удалённый оператор. Требуется *ещё и* operator-preserving domain policy.
- **C#/SQL AST:** структурный parser и role/operator features проверять отдельно. Нельзя переносить универсальную punctuation stripping policy из переводов.
- **RP01 neural DPO/LoRA:** эта неисправность не входит в непосредственную трассу `tools/Rp01SimulationDpo → DpoTrainer → TextGenerator`; это **условный дефект другого выученного потребителя**, важный для сравнения старых успешных гибридных экспериментов.

## 5. Альтернативные объяснения и фальсификаторы

Приоритет exact-source перекрывает дефект на train-строке; Verifier может зафиксировать issues, но текущий trained adapter **не обязан блокировать** accepted phrase-map с confidence 0.86 (N16-06); дальнейший product flow может выбрать другой исход. Lookup может не иметь валидной словарной записи; при отсутствии отдельного standalone punctuation token индексы не расходятся. Любая из этих ситуаций должна быть показана в трассе как **NOT_TRIGGERED**, а не как доказательство отсутствия дефекта. Фальсификатор качества: реальный source-family-disjoint native A/B не даёт меньше wrong-accepted/больше законных переводов при том же trained state и том же verifier.

## 6. Минимальная корректировка, только если A/B подтвердит качество

Перед фильтрацией хранить пары `(NormalizedText, OriginalTokenIndex)`; phrase matching по `NormalizedText`; word fallback по `tokens.Tokens[OriginalTokenIndex]`. Preserve span boundaries и не расширять scope до «всегда стирать операторы». Независимо доказать равенство cold restored run и отсутствие регрессий на контрольных примерах. См. [N33 native causal protocol](33-MICRO-PAIRED-PUNCTUATION-INDEX-CAUSAL-EXPERIMENT-2026-10-16.md).

## 7. Новизна / дедупликация

Сверены `possibleDifs` README, мастер N30, N31/N32 и N21/N29/N30 соседние seam-исследования. Они уже знают о `.Where(nonempty)` и false adjacency, но **не выделяют самостоятельную ошибку обращения к `tokens.Tokens[i]` после compaction как проверяемую причину другого output**. Новизна ограничена этой точной ошибкой индекса и её контрастом; глобальная заявка «ни один агент никогда не находил» НЕ делается.
