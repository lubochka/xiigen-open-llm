---
title: "321. Offline–runtime equivalence: предпосылки Proposition 18.3 и смысл head-count buffer"
source_pdf_pages: [745, 750]
book_pages: [709, 714]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 321. Offline–runtime equivalence: предпосылки Proposition 18.3 и смысл head-count buffer

## Вопрос

Опровергает ли улучшение runtime-контроллера Proposition 18.3 и что именно означает заявленная equivalence?

**Proposition 18.3 условна по convexity, общему оператору и matched thresholds, а её явное заключение ограничивает только разницу конечных head counts.** Эксперимент вне этих предпосылок и различие RMSE при одинаковом $K$ не являются прямым опровержением такого заключения. [Proposition 18.3](../source/0743-0745-ch18-euler-lagrange-control-and-deployment-equivalence.md#proposition-18-3).

## Что сформулировано и чего здесь нет

Автор предполагает locally convex downstream objective без spurious attractors в initialisation basin, один residual operator и одинаковые $(\gamma_{\mathrm{add}},\gamma_{\mathrm{prune}})$. Затем утверждает

$$
|K_{\mathrm{offline}}^\star-K_{\mathrm{runtime}}^\star|
\le\left\lceil\log_2(1/\varepsilon_{\mathrm{prune}})\right\rceil.
$$

Это редакторская запись словесной границы proposition. Proof sketch и derivation buffer вынесены во внешнюю работу; ссылка на Chapter 17 содержит незаполненный номер. В доступном фрагменте нет recurrence для $K$, coupling двух траекторий, условий на estimator noise или доказательства существования их пределов.

Convexity downstream loss сама по себе не задаёт contraction spectral residual и правило выбора integer architecture. Например, family convex objectives может иметь одинаковый минимум при разных количествах избыточных параметров; хороший loss landscape не выбирает единственный rank. Для INCRT нужен дополнительный invariant, связывающий fixed residual, sequence of events и stop criterion. Это объясняет недостающий переход, а не подменяет неизвестный внешний алгоритм контрпримером.

При $\varepsilon_{\mathrm{prune}}=10^{-3}$ буфер равен $\lceil\log_2(1000)\rceil=10$. Если обе сравниваемые архитектуры ограничены диапазоном $1,\ldots,8$, такая граница автоматически выполнена и практически ничего не различает. При других caps её информативность надо проверять отдельно.

## Эксперимент меняет больше, чем время вызова

Static mode использует temporal gradient operator, оценённый до RL, затем выбирает retained $K$ по Phase-2 search. Runtime наблюдает effective rank недавних **on-policy encoder tokens**, начинает с одной активной головы, имеет cap 8 и другую диагностическую формулировку grow/prune. Эти различия могут быть полезной адаптацией, но одинаковость operator и threshold semantics не проверена одной фразой “differ only in when”. [Static setup](../source/0746-0748-ch18-static-control-failure-and-runtime-growth.md#section-18-3-2), [runtime prescription](../source/0746-0748-ch18-static-control-failure-and-runtime-growth.md#prescription-0748-runtime-grow-prune-monitor).

Кроме того, SAC выбран именно как non-convex downstream learner. Из импликации «если convexity, то equivalence» не следует equivalence вне convexity; её нарушение там совместимо с proposition. Нельзя одновременно ссылаться на отсутствие предпосылки как объяснение результата и считать этот же результат логическим опровержением условной теоремы.

## Count equivalence не равна performance equivalence

На длинном горизонте static retained $K=8$, и runtime также достигает $K_{\max}=8$. Head-count difference здесь нулевая, хотя reported RMSE различается. Это поддерживает исследование **траектории обучения при одинаковой конечной ширине**, но не опровергает count bound.

Равное число heads не означает равные weights, function, effective rank, optimiser state или test accuracy. Prediction P5 добавляет требование statistical indistinguishability downstream accuracies; такое требование не следует из одного count conclusion без дополнительного результата. [Runtime interpretation](../source/0749-0750-ch18-runtime-control-results-curriculum-and-prediction-p5.md#pdf-0749), [P5](../source/0749-0750-ch18-runtime-control-results-curriculum-and-prediction-p5.md#prediction-p5).

Применение proposition к protein MLM и trainable classification encoder также требует собственной convexity-проверки; loss-name argument недостаточен, как показано в [319](319-plasticity-gaps-and-mlm-convexity-domain.md). Для проекта proposition сохраняется как **условное авторское утверждение с внешним proof reference**, а наблюдаемая runtime performance — как отдельный empirical result.
