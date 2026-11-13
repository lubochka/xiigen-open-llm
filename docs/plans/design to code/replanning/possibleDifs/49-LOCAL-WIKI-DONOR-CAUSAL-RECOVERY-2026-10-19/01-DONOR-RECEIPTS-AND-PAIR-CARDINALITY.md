# 49/01. Проверенные донорские архивы и кардинальность обучающих данных

**19 октября 2026. Срез: local_wiki_planning на 219d7ba89dc3ef43e03e17f1461ad96154d3dd7a.** Цифры из первичных сохранённых файлов. Новая оценка модели, генерация, .NET-сборка, обучение: **NOT_RUN**.

## 1. Три архивных результата 24 мая — три разных критерия

| Donor run | Предметная primary accuracy | DPO heldout accuracy | Статус фактической публикации |
|---|---:|---:|---|
| [CodeTrace Builder 064][ct_run] | 1.00 | **0.92** | published |
| [Query Planning 134][qp_run] | 1.00 | **1.00** | published |
| [Request Router 092][rr_run] | 1.00 | **0.66** | **trained_not_published** |

Сохранённый RequestRouter предупреждает request_routing_dpo_accuracy_below_0_75_publication_skipped. Таким образом, primary=1.0 не означает успешное neural DPO и не означает serving adoption. Метрика DPO heldout — предпочтение выбранной стороне относительно отклонённой, а не правильность свободной текстовой генерации.

Эти архивы — разные учебные единицы, не временной ряд одной и той же модели. Нельзя вычислять деградацию RP01 как разность 1.00−0.00 без matched consumer/model/задач. Исторические флаги published указывают на наличие публикационного артефакта, но не доказывают, что продуктовый endpoint использовал neural weights.

## 2. Прочитаны целиком ровно два DPO-pairs.jsonl (не только отчёты)

[CodeTrace, весь 100-строчный JSONL][ct_pairs]. Разделение по префиксу id с heldout; подсчёт по parse(JSON) каждой записи:

| Свойство | TRAIN | Heldout |
|---|---:|---:|
| Число строк | 50 | 50 |
| Уникальных полных prompts | **50** | **50** |
| Уникальных (prompt,chosen,rejected) | **50** | **50** |

Промпт train не встречается дословно среди heldout (0 совпадений). Во всех 100 строках — **100 разных chosen, 100 разных rejected**. Есть настоящее содержательное различие текстов source/trace: это наиболее полезный донор для first-slice восстановления конкретной информации, особенно caller/callee/value-flow. Но число разных строк всё ещё не гарантирует, что все семейства исходного кода независимы или что scorer научился отличать правильный ответ, не подсматривая метки.

[Query Planning, весь 100-строчный JSONL][qp_pairs]. Тем же способом:

| Свойство | TRAIN | Heldout |
|---|---:|---:|
| Число строк | 50 | 50 |
| Уникальных полных prompts | **8** | **8** |
| Уникальных (prompt,chosen,rejected) | **8** | **8** |

Exact overlap prompts train↔heldout равен **0**: они действительно разные строковые вопросы. Но **объединённые 100 записей имеют ровно 1 уникальный chosen и 1 уникальный rejected**; ответы идентичны между TRAIN и heldout. Пример массового постоянного текста chosen — повторение ACCEPT_CLIENT_SERVER_TRACE, fixed full-cross-codebase steps; rejected — повторение REJECT_CLIENT_SERVER_TRACE и отсутствующих частей trace. Если score основан на этих почти постоянных маркерах, 1.00 может появиться **без вопросо-зависимого построения планов**.

**Это не обвинение в exact train/heldout leakage.** Никакой дословной prompt-утечки не найдено; именно collapse ответов при разных prompts требует counterfactual exam. Отличается от прежнего результата про 40/50 совпадений в WikiTrace (см. [Core76][core76]), который нельзя переносить на этот run.

## 3. Смысловой критерий для решения «работало хорошо»

CodeTrace DPO heldout .92 на разнообразных raw target strings — более интересный экспериментальный кандидат, чем Query Planning DPO heldout 1.00 при одном unique chosen/rejected. **Это порядок исследования, а не численная оценка превосходства двух систем.** Для CodeTrace требуется проверка качества фактических трасс, включая ложные выдуманные edges; для QueryPlan — проверка обусловленности порядка шагов входом.

Прямые прежние [Core28][core28] и [Core39][core39] аудиты разбирали ранние майские CodeTrace run (15 и 19 мая), показали важный факт о trainable output-head и ограничения сравнения checkpoints. Новое в этой главе: **выбраны поздние успешные 24-мая donor receipts и посчитаны полные source-cardinalities**, а не объявлен заново найденным старый класс проблем LoRA/head.

## 4. Что ещё проверить для идентичности донорского запуска

1. Сохранить точный Git commit, SHA256 исходных JSONL bytes и parent checkpoint. Наличие JSONL в branch не аттестует SHA исполнявшегося тогда бинарника.
2. Сверить train/heldout source family, stack/framework, actual semantic gold (не только автоматически произведённый generator expected).
3. Отличить опубликованный JSON report/policy.bin от реального neural policy_final.bin; проверить модель/optimizer/tokenizer reference, настоящий state и обычный reader.
4. Сначала воспроизвести heldout scorer с no-change donor baseline; затем исполнить эксперимент 49/03 с одной заменой projection.
5. В документации никогда не говорить «50 новых законов» по одним только 50 строкам или «нейронная точность 100%» по предметному deterministic gate.

## Источники

[ct_run]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/project-wiki-training-loop-result.json
[qp_run]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/project-wiki-training-loop-result.json
[rr_run]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-request-router-post-registry-selector-promotion-confirmation-092/project-wiki-training-loop-result.json
[ct_pairs]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/dpo-pairs.jsonl
[qp_pairs]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/dpo-pairs.jsonl
[core76]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md
[core28]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/28-code-trace-learning-donor.md
[core39]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/39-code-trace-remediation-lineage.md
