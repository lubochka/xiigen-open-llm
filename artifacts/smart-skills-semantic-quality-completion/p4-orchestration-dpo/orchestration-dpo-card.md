# P4 OrchestrationReasoning DPO card

Статус: draft pending parent review. Этот P4 artifact не является training-ready, не является DPO-ready и не заявляет semantic quality.

## Источник

Использован только accepted Orchestration seed `P2-REPAIRED-SEED-002` / `PATCH-phase-transition-protocol-SKILL.md`.

`P2-REPAIRED-SEED-001` / `basic prompt.txt` исключен как PromptToTask source и не используется как accepted OrchestrationReasoning source.

Source ref и path hash используются только как provenance. Raw private content не копировался.

## Счетчики

| Метрика | Значение |
|---|---:|
| Draft rows | 2 |
| Review question rows | 2 |
| Training-ready rows | 0 |
| DPO-ready rows | 0 |
| Same-source variants | 2 |
| Independent source-family evidence | 0 |

## Универсальная boundary

Phase-boundary orchestration: review/fix/commit/next-phase нельзя схлопывать; следующая фаза стартует только после принятого evidence и явной границы.

## Chosen examples

1. Сначала собрать evidence текущей фазы, затем parent review, и только после accepted evidence отделять commit или next phase boundary.
2. Механически валидный draft остается draft: review и source-disjoint gates идут до DPO-ready/promotion.

## Rejected near misses

1. Перейти к следующей фазе или commit до parent review, потому что draft существует или source title выглядит как phase protocol.
2. Считать JSON parse и наличие phase/review keywords достаточными для DPO-ready и независимого обучения.

## Что исключено

Исключены WordPress path, plugin folder mechanics, PowerShell/host details, exact patch commands, filename/title/static keyword shortcuts и raw private protocol text. В draft остается только stack-free phase-boundary reasoning.

## Почему не training-ready

Parent review еще не выполнен для P4 draft rows. Source-disjoint eval и ablation gates еще не выполнены. Обе строки происходят из одного seed/source family, поэтому они не считаются двумя независимыми обучающими источниками.

Promotion guard: только один same-source variant может быть повышен, если source-disjoint eval и parent review не докажут независимую обучающую ценность.

## Required ablations

- filename/path lookup
- WordPress/plugin title shortcut
- static phase keyword shortcut
- exact protocol text lookup

## No-claim boundary

No code, tests, build, training, checkpoint/model, commit, push, branch switch, or semantic quality claim happened in P4 authoring.
