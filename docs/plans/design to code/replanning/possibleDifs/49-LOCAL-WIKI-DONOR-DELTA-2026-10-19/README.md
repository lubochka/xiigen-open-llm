# 49 · Новые расхождения с реально обучавшейся веткой local_wiki_planning

19 октября 2026. Состояние работы: **конкретные исходники и сохранённый исторический JSON прочитаны; аналитический projection посчитан; новые .NET training/inference НЕ запускались.**

## Смысл

Архивная `local_wiki_planning` — **важный самостоятельный донор** успешных *предметных* обучающих потоков. Не сводить её ни к `language_translate`, ни к `PrepareToGPL`, ни к RP01 q/v LoRA. Найдены **две source-confirmed регрессии представления DPO для разных получателей**: Wiki Request Router и C# CodeTraceBuilder. В current branch исходные `prompt/A/chosen/rejected` доступны в исходных парах, но модельно видимая тройка строится из `RejectedType`, стирая различия, требуемые в домене.

Также найден отдельный новый **publication/serving gate**: `semanticQualityClaimed=false` фиксировано в текущем wiki router runner, вследствие чего `PublishAllowed` блокирует любой созданный им snapshot. Этот safeguard **нельзя просто удалить или принудительно включить**; требуется измеримое независимое смысловое доказательство.

## Материалы

- [01 — N49-01 / N49-03: точная потеря входных DPO-признаков, доказательства и историческая проекция 50→4](01-DPO-PAIR-LOSS-AND-HISTORICAL-PROJECTION.md)
- [02 — N49-02: публикация, learned state и настоящий consumer](02-PUBLICATION-GATE-AND-LEARNING-RECIPIENT.md)
- [03 — короткие эксперименты с контрольными случаями и явными опровержениями](03-FAST-DOMAIN-EXPERIMENTS.md)
- [06 — НОВОЕ: исторический checkpoint continuation vs новый deterministic-fresh default в wiki DPO, отдельная проверка сброса памяти между batch](06-NEW-CONTINUATION-RESET-POLICY-REFERENCE.md)
- [05 — сводка всех 25 уже исследованных классов причин + наши дельты, применимость по доменам](05-ALL-MECHANISMS-CROSSWALK-DOMAIN-APPLICABILITY.md)
- [04 — отдельность от уже известных исследований, RAG, skill stack, границы свидетельств и executor work order](04-PROVENANCE-REVIEW-AND-EXECUTOR.md)
- [Глава 0: восстановление положительных исторических способностей стоит впереди новых опций LoRA](../00-LOCAL-WIKI-SUCCESS-RECOVERY-P0-2026-10-19.md)

## Быстрая таблица claims

| Claim | Status | Чего пока НЕТ |
|---|---|---|
| Wiki routing old сохранял prompt/chosen/rejected, current заменил на defect-only | **SOURCE_CONFIRMED** | Причинный before/after neural benchmark |
| Старые 50+50 prompt при current projection превращаются в 4 типа каждой выборки | **SOURCE-EQUIVALENT ANALYTIC_PROXY** | Native current BuildBatch и updated-weights evidence |
| `wrong_training_unit` склеивает 6 разных целевых маршрутов | **ARCHIVE_DATA + SOURCE_CONFIRMED projection** | Точность после изменения projection |
| CodeTrace old сохранял program+answer, current defect-only | **SOURCE_CONFIRMED** | Verified independent code-trace quality gain |
| Router gate `semanticQualityClaimed=false` блокирует publication | **SOURCE_CONFIRMED current runner code** | Реальный publish/restore после нового независимого gate |
| Донорский run043 structural 1.0 / DPO heldout .88 и publication null | **ARCHIVAL_REPORT**, не matched experiment | Runtime/binary attestation, independent semantic generalization |
| Все эти различия объясняют RP01 C#0/3 ENRU0/4 | **NOT_PROVEN / НЕ УТВЕРЖДАЕМ** | Один recipient и matched causal test |

## Срезы

- Донор: `llm_mvp_core@local_wiki_planning`, HEAD `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`.
- Текущий: `llm_mvp_core@codex/dpo-implementation`, подтверждённый исследуемый snapshot `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`.
- Документация: `xiigen-planning@llm_plan3`.
- **READ ONLY:** `llm_mvp_core/docs/plans/replanning-possible-diffs/**`.

Домен = предмет обучения (проектная wiki-маршрутизация, C# код/трассировка, перевод), **а не** название алгоритма. Каждый результат должен назвать исходные пары, обучаемое состояние, этап потребления, метрику и ограничения.
