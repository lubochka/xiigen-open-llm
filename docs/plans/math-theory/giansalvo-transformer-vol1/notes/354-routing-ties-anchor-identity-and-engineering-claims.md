---
title: "354. Routing stability и anchor identity: что означает архитектурное закрытие вопроса"
source_pdf_pages: [787, 789]
book_pages: [751, 753]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 354. Routing stability и anchor identity: что означает архитектурное закрытие вопроса

## Вопрос

Почему measure-zero score ties и именованные anchors ещё не гарантируют стабильное обучение?

**Источник предлагает конкретные объекты для реализации, но заявленное “resolved” не заменяет условий устойчивости.** Полезно отделять определённость операции, её гладкость внутри режима и контролируемость переходов между режимами. [Routing stability](../source/0785-0787-ch19-empirical-illustration-and-rank-results.md#section-19-10), [обзор остальных вопросов](../source/0788-0791-ch19-residual-questions-spectral-design-and-conclusion.md#pdf-0788).

## Score ties и небольшой запас — разные случаи

Для hard top-$k$ выбор локально постоянен, если есть положительный score gap между последним выбранным и первым невыбранным кандидатом. Если этот gap равен $\gamma>0$, perturbation каждого score меньше $\gamma/2$ сохраняет membership. Это элементарное достаточное условие; источник в этом разделе не сообщает измеренные $\gamma$ по training trajectory.

Равенство scores может иметь measure zero при generic weights, но окрестности почти равных scores имеют ненулевой объём. Optimisation может проходить через них многократно. Поэтому measure-zero statement не задаёт частоту переключений, размер jump или допустимый learning rate.

Предлагаемые dense routing, enlarged-$k$ и soft top-$k$ с annealing — разумные варианты протокола. Для воспроизводимости нужны конкретные temperature schedule, tie-breaking, gradient rule и сравнение train/inference operators.

## Что дают anchors

Идентификатор dictionary atom позволяет прикрепить к head историю load, entropy, warm-up, protection и pruning. При перестановке heads вместе с anchors сам словарь просто переименовывается, что источник признаёт. Поэтому “head identity” в controller надо закреплять стабильными identifiers, а не позицией в массиве после merge/split.

Совпадающие anchors, одинаковые head projections и softmax saturation могут ослаблять различимость. Claim о generic symmetry breaking нужно читать в рамках исходного аудита, а не как гарантию единственного параметрического описания.

## Что переносится в проект

Переносимый результат здесь — схема измеримых объектов: selected partners, edge features, anchor gates, load, effective rank и validation sensitivity. Эти объекты позволяют формулировать маленькие проверяемые изменения. Утверждения о сохранении качества после growth/pruning, эффективности long-context search и превосходстве над альтернативами требуют своих условий и измерений. Авторская риторика о “remaining empirical scaling” не добавляет недостающей реализации.

