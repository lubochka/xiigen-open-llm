# N41-T3 — TopicDomain передаётся через Flow, но не участвует в памяти Executor

18.10.2026. SOURCE-CONFIRMED INTRA-UNIT INPUT ERASURE; quality impact CONDITIONAL; native test NOT_RUN. Это не P73/E31 про ProbeNN и не P117 про направление перевода: речь о фактической теме перевода внутри конкретного ExecutorUnit.

## Адресная цепь от request к learned state

1. [TranslationMachine.cs L829–845](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L829-L845) берёт metadata **topic_domain** для ordinary TranslationRoutingRequest.
2. [TranslationRoutingFlow.cs L63–79](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationRoutingFlow.cs#L63-L79) передаёт запрос адаптеру; [TranslationDirectionAdapters.cs L117–127](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L127) вызывает **_executor.Execute(tokens, request.TopicDomain)**.
3. [TranslationTrainedUnits.cs L263–325](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L325): параметр **string topicDomain** объявлен, но во всём теле Execute **не читается**. Выбор происходит по exact, phrase, word maps или KNN.
4. [TranslationStructuredBatchBuilder.cs L12–25](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L12-L25) TrainReadyPair не имеет TopicDomain; [L112–125](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L112-L125) передаёт **"general"** в ExecutorInput даже для тематических примеров.
5. [TranslationTrainedUnits.cs L403–421](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L421): ключ точной памяти — NormalizeMapKey(input.SourceText), без темы. Две законные пары с одинаковым source и разными контекстными chosen перезаписывают значение exact-map в зависимости от порядка уроков.

Факт: данный исходник Unit имеет идентичный blob SHA **1742abfc9fb617dfc66e4d618e0c169da01a6ccc** в PrepareToGPL и current. Это унаследованная граница, **не доказанная новая регрессия** и не прямая причина нейронных free 0/4.

## Быстрый проверочный опыт TOPIC-MEMORY-01

**Домен:** перевод; **профили:** EN→RU технический/юридический, потом RU→EN отдельно. **Получатель:** translation Executor trained memory; neural weights не трогать.

TRAIN: два честно аннотированных примера с **одним** английским source «charge»: юридический «обвинение» и электротехнический «заряд». Независимый gold/judge обязан подтвердить обе допустимые трактовки при указанных темах. Если настоящий TRAIN DTO не позволяет topicDomain, обозначить producer-contract gap; controlled component input вручную не выдавать за обычную продуктовую автоматизацию.

A: существующий Executor Execute, одинаковый корпус в двух разных порядках обучения (law→electronics; electronics→law). B: bounded prototype exact key как тройка **(direction, normalizedSource, topicDomain)**, не меняя word/phrase, KNN, verifier. При неизвестной теме B явно сообщает UNKNOWN/abstain; source и все ответы уроков те же.

TEST: одинаковый «charge» с metadata.topic_domain=law/electronics, «charge» без контекста, отдельные «electrical charge»/«criminal charge», контроль однозначного «cache». Собрать ordinary Flow output (через GetOrchestratorAsync().RunAsync), выбранный route, hash сохранённого learned state. TEST expected scorer получает **после** native output, не при поиске.

Primary: correct/all по каждой теме; wrong-accepted/all; coverage и correct/answered; зависимость от последовательности уроков; checkpoint reload, latency/RSS, количества записей. Neural total/trainable layers N/A; levels Unit/Flow, executed depth отдельные измерения.

Успех A/B доказывает лишь то, что существующая память не различает допущенный topic при одинаковом source, а модифицированный exact-key различает. **Не доказывает** automatic topic inference или пользу во всех переводах. Если настоящий consumer никогда не передаёт достоверную тему, эффект product NOT_APPLICABLE. При одинаковом lawful translation во всех темах conditioning не нужен.

## Не дублировать

[P73 E31 domain conditioning](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P73-domain-conditioning.md) — другой learner; [P117 direction memory](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P117-translation-direction-memory.md) — другой ключ. Данная гипотеза проверяет сквозной путь metadata→Flow→unused Execute argument→train "general"→unscoped exact-key. Защищённый каталог core research только прочитан.
