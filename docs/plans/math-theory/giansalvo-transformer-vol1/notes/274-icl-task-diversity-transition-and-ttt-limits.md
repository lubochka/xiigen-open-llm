---
title: "274. Task diversity и TTT: размер словаря, полнота покрытия и предел повторных шагов"
source_pdf_pages: [671, 673]
book_pages: [635, 637]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 274. Task diversity и TTT: размер словаря, полнота покрытия и предел повторных шагов

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Доказывают ли число задач и число TTT-шагов универсальный переход к generalisation?

## Число задач не равно размерности их словаря

[Proposition 16.79](../source/0671-0674-ch16-in-context-sample-complexity-and-test-time-adaptation.md#proposition-16-79) связывает переход с $K_{\rm pre}^*=\Theta(\kappa_A^2\log d)$. В proof предполагается, что обучение на $K_{\rm pre}$ задачах создаёт $K_{\rm pre}$-мерный словарь, который выше порога покрывает spectral structure.

Но для представлений $v_1,\ldots,v_K$ точное ограничение — $\operatorname{rank}[v_1,\ldots,v_K]\le K$, а равенство требует независимости. Даже сколь угодно много **различных** векторов $v_j=c_j e_1$ оставляют rank равным единице. Для полноты покрытия нужны распределение задач, вероятность появления новых направлений, пространство представлений и качество их восстановления. Количество задач само по себе этого не обеспечивает.

Covering number также относится к определённым множеству, метрике и радиусу. В (16.122) ни радиус покрытия, ни точность generalisation не определены. Переход от неполного словаря к обязательному nearest-neighbour memorisation — дополнительное описание поведения обученного алгоритма, а не линейно-алгебраическое следствие недостаточного rank. Источник не задаёт условия, которые исключают иные inductive biases. [Proof и caveat](../source/0671-0674-ch16-in-context-sample-complexity-and-test-time-adaptation.md#pdf-0672).

## Какие статусы сохраняются

Proposition 16.79 обозначена автором как proposition с proof sketch, связанным с linear attention. Следующая [Conjecture 16.80](../source/0671-0674-ch16-in-context-sample-complexity-and-test-time-adaptation.md#conjecture-16-80) явно распространяет переход на произвольный достаточно ёмкий transformer и оставляет task-independent constant гипотезой. Упоминание экспериментов с незаполненной ссылкой не меняет этот статус. Из утверждений о возможности реализовать gradient descent не следует, что выбранное pretraining действительно найдёт нужные веса.

## TTT уменьшает вычислительную ошибку при своих условиях

[Corollary 16.81](../source/0671-0674-ch16-in-context-sample-complexity-and-test-time-adaptation.md#corollary-16-81) заменяет $T$ на $T+T_{\rm TTT}$ в знаменателе sample complexity. Это требует, чтобы explicit steps и implicit layers имели сопоставимые objective, preconditioning, шаги и contraction. Один forward layer и один gradient update общего transformer не являются автоматически одной операцией с одинаковой стоимостью или действием.

Дополнительные шаги на том же prompt не добавляют независимые наблюдения. Statistical floor из [273](273-icl-statistical-error-versus-depth-optimisation.md) остаётся даже при точной оптимизации эмпирической задачи. Поэтому нельзя обещать неограниченное сокращение контекста как $1/(T+T_{\rm TTT})$; нужны хотя бы отдельные statistical и optimisation terms, целочисленность $n$ и конкретный prior.

Для практического TTT следует различать время до query, обновляемые параметры, сохранение состояния между prompts и качество на новых queries. Наличие loss для демонстраций ещё не обосновывает качество test-time update на других входах.

## Архитектурный рост остаётся направлением разработки

На [PDF 673](../source/0671-0674-ch16-in-context-sample-complexity-and-test-time-adaptation.md#pdf-0673) prompt-driven incremental growth прямо назван *natural follow-up direction*. Это не опубликованный в данном фрагменте законченный алгоритм с доказанной termination или latency bound. Спектральное отличие prompt от обучения может предложить сигнал, но не задаёт допустимый бюджет роста, confidence margin и проверку сохранения функции.

Кроме того, spectral overlap измеряет угол операторов, а не разницу масштабов; при абсолютном stopping threshold одинаковый угол не определяет одинаковый head count. Ограничение уже разобрано в [238](238-spectral-overlap-scale-and-transfer-distance.md). Для реализации надо сформулировать устойчивый estimator prompt operator, сравнивать его в общих единицах и проверять полезность добавленной головы отдельно от новизны спектра.
