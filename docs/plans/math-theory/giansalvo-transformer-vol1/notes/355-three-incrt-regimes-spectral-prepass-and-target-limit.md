---
title: "355. Три режима INCRT: growth, stabilisation и spectral pre-pass без гарантии общего attractor"
source_pdf_pages: [789, 791]
book_pages: [753, 755]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 355. Три режима INCRT: growth, stabilisation и spectral pre-pass без гарантии общего attractor

## Вопрос

Как один controller используется во время обучения и до него, и гарантируется ли одинаковый итоговый размер?

**§19.11 объединяет три режима по общим diagnostics и event templates, но прямо отказывается считать $K^\star$ гарантированным attractor из любых начальных условий.** [Три режима](../source/0788-0791-ch19-residual-questions-spectral-design-and-conclusion.md#section-19-11), [ограничение fixed point](../source/0788-0791-ch19-residual-questions-spectral-design-and-conclusion.md#pdf-0790).

| Режим источника | Исходное состояние и время | Предполагаемое действие |
|---|---|---|
| Expansion | Random backbone, во время training | Добавление anchors и билинейных heads при residual triggers |
| Stabilisation | Warm-start backbone с немногими dominant signed modes | Protection/pruning; отсутствие growth описывает именно этот режим |
| Architecture-initialisation | Candidate backbone, до training | Spectral pre-pass даёт seeds и head counts; затем architecture фиксируется |

“Warm-start” сам по себе не доказывает, что growth никогда не понадобится. В тексте дополнительно задано состояние с малым числом dominant modes. Аналогично termination в expansion ссылается на четыре core hypotheses и thresholds: это условное утверждение, а не результат одной только структуры A-MHB-G.

## Что значит deterministic pre-pass

При фиксированных данных, кандидатных projections, правилах построения $A_h^{\mathrm{sig}}$ и residual budget вычисленная spectral information определена. Но из спектра ещё нужно получить однозначные anchors. Eigenvectors имеют неопределённый знак; при кратных eigenvalues можно вращать basis внутри eigenspace. Для bitwise/repeatable seeds нужны conventions выбора ориентации и basis, tie-breaking, preprocessing и численные tolerances.

Источник говорит об input covariance и query–key projections. Для конкретного A-MHB-G требуется явно связать его routing matrix и bilinear projections с тем signed operator, который берётся в pre-pass. Эта страница не выписывает executable mapping. Условный compressed-sensing bound также не становится автоматически точным оптимумом архитектуры.

## Сильная оговорка, которую нельзя потерять

$K^\star$ назван diagnostic reference; controller не обязан достигать его автономно из произвольного initial state. Значит, три варианта нельзя объединять обещанием “все неизбежно приходят к одному числу heads”. И детерминированность выбора при фиксированном входе не означает оптимальность по test quality или стоимости.

Заключение главы ещё раз называет эмпирическую часть **proof of concept**, а не окончательной валидацией. [Последняя страница главы 19](../source/0788-0791-ch19-residual-questions-spectral-design-and-conclusion.md#pdf-0791). В этом проходе ни controller, ни spectral NAS не реализованы и не запускались.

