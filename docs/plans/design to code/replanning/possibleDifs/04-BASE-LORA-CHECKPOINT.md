# Base, LoRA, tokenizer, сохранение и генерация

<!-- POSTQ003-2026-11-03:START -->
## Уточнение приёмки 3 ноября 2026 — PUBLISHED-BYTES-AND-COLD-LOAD

При выборе reusable checkpoint различать Git pointer, фактический payload, полноту typed state и Load новым обычным потребителем. Девятнадцать путей адресуют конкретный289-байтовый конфликтный текст; P03 after.json с hashes/counts не является полным final state. Положительный контроль: подлинный полный bundle с сохранением ответов. Отрицательные: правильный pointer к конфликту или receipt вместо обязательного компонента. Отсутствующий original не заменять Train; исторические положительные C50 и другие приёмки сохранить в их установленной области.

Основание и точные владельцы — [аудит после Q003](codex-2026-11-03-POSTQ003-ARTIFACT-AND-TRANSFER-AUDIT.md). В передаваемое поручение включить назначенные state/attempt/path/authority, положительный и отрицательный случаи и ожидаемую причину отказа. Исходный ниже паспорт сохраняется как история; уточнение не запускает новый опыт, не меняет gold/budget и не отменяет доказанные прежние результаты. Нативная реализация и приёмка требуют назначенного исполнения.
<!-- POSTQ003-2026-11-03:END -->

## Почему технически работающая LoRA может не давать смысла
Текущий baseline задаёт 629 BPE tokens, hidden=512, 4 Transformer слоя, contextLen=2048, 8 heads, 2 kv heads; `baseInitializationSeed` фиксирован, исходный B0 случайно инициализирован и не был pretrained. LoRA: rank8, alpha16, dropout0, `q_proj,v_proj` у всех 4 слоёв. [generation-v1.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/config/code-documentation/generation-v1.json#L7-L49); [RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md#L21-L35). Это **не** доказанная программная ошибка, но экспериментально обучать семантическому C# и ENRU с DPO-only на таком B0 особенно трудно; DPO предпочтений не является автоматически полноценным language pretraining.

`LoraLinear.Forward`: `base(x)+(alpha/r)*((dropout(x) Aᵀ) Bᵀ)`, seed-A Kaiming, B zero. Логика математически соответствует LoRA. [LoraLinear.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/10.LoRA/LoraLinear.cs#L44-L59); [LoraLinear.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/10.LoRA/LoraLinear.cs#L115-L125). Измеренный checkpoint: **16/16 allowed LoRA tensors изменились; 0/39 base changed**, 53,248 adapter elements и 11,920,896 frozen base elements в ENRU. Значит нет доказательства «адаптер не присоединён» или «AdamW вообще ничего не меняет»: [scientific-result.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L11-L21).

Наряду с этим предсказание почти неподвижно: сравнение первых решений для одного prompt показало `max|Δlogit|=0.0106492`, `max|Δlog-softmax|=0.0108445`, argmax 388→388; 2 prefill / 0 decode. Это доказательство изменения распределения, **не** новой компетенции: [README.md](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/tools/Rp01SimulationDpo/README.md#L1-L19).

## Возможные причины, которые ещё нельзя путать
| Механизм | Что уже доказано | Чего не хватает |
| --- | --- | --- |
| B0 initial competence | random/non-pretrained; free 0/3 и 0/4 | same-budget supervised CE warmup vs DPO-only, ideally licensed pretrained checkpoint |
| LoRA attachment | tensors changed, Base frozen | rank/target capacity, ∥ΔA∥/∥ΔB∥, gradient test, effective rank, per-layer response |
| Learning rate / beta | 1e-5 / 0.1, 50 steps per 500 | update sensitivity curve, train-overfit and induced logits vs decoder |
| Tokenizer | UTF-8 BPE is byte lossless on admitted inputs | fragmentation ENRU, tokens/word, source/target entropy, effective context, EOS/truncation |
| Training/inference parity | saved full checkpoint and shared TextGenerator | exact prompt framing and first-token prefix, decoding stop/invalid UTF-8, natural output |
| Multi-domain transfer | C50 loaded into ENRU C100 | replay, skill before transfer, domain-specific adapters and catastrophic forgetting |

## Checkpoint: **подтверждённая положительная часть**
`SimulationEnRuContinuation.ImportSource` загружает сохранённый full checkpoint C50 **в policy и reference по отдельности** после SHA verification; `ImportFullCheckpoint` вызывает `ValidateFullCheckpointArtifact` и `model.LoadCheckpoint(stream)`; `OpenStudent` проверяет identity/manifest/step. [SimulationEnRuContinuation.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/SimulationEnRuContinuation.cs#L192-L247). Научный ENRU итог сообщает `fullStateReopenedAndEqualLive=true` и `separateFrozenC50ReferenceUnchanged=true`; оригинальный B0/C50 не следует объявлять испорченным. [scientific-result.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L10-L29).

**Отдельный реально открытый seam:** private RESEARCH_ONLY checkpoint **не был** promoted/adopted; один успешный offline load не доказывает, что обычный пользовательский CodeDocumentation/translation запрос читает именно эту версию. Проверить `requested checkpoint sha → validated manifest → adapter topology → ordinary TextGenerator → output receipt`. Для ENRU reference должен быть замороженным **C50**, а не B0 (так реализовано); сравнение DPO control arms должно сохранять одинакового соответствующего reference. Не называть reuse C50 доказательством переноса полезных C# знаний, если C# free =0.

## Эксперимент с минимальным временем
A = current B0+DPO-only; B = same B0+короткая законная SFT/CE на chosen next-token, затем DPO; C = доступная уже обученная совместимая base (только если это legal/resource-feasible). Одинаковые token/prompt/heldout settings; измерить NLL, intended language validity, exact law success, compute and old-domain retention. Не переписывать frozen C#500/ENRU500; использовать новую малую семью. Early stop, если entropy/first-token остаётся случайным или tiny train overfit провален.
