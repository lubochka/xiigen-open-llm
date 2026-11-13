---
title: "79. Наблюдаемые перехода и условные связи compatibility profile"
source_pdf_pages: [301, 302]
book_pages: [265, 266]
supplementary_source_pdf_pages: [295, 296]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 79. Наблюдаемые перехода и условные связи compatibility profile

[Индекс библиотеки](../README.md) · [PDF 301–302](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#pdf-0301) · [Предыдущая карточка](78-margin-entropy-proxy-and-compatibility.md) · [Следующая карточка](80-conditional-compatibility-and-eigenplane-cancellation.md)

## Что добавляется к одному расстоянию между слоями

Автор продолжает §8.10 тремя видами изменений: переносом barycentric mass, изменением residual и изменением разброса популяции сигнатур. Один большой $W_2$ не сообщает, какая именно координата изменилась. Поэтому Figure 8.11 показывает несколько наблюдаемых на одной оси глубины. Подпись прямо называет данные **синтетическими**. [§8.10.2 и Figure 8.11](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-10-2).

Следует сохранить различие обозначений, введённых ранее:

| Наблюдаемая | Смысл | Что теряется |
|---|---|---|
| $D_\ell^\Lambda=\int(\Lambda'-\Lambda)\,d\pi_\ell$ | Signed vector: какие координаты в среднем получили массу | Взаимно компенсирующие перемещения |
| $\Phi_\ell^\Lambda=\int\|\Lambda'-\Lambda\|_2\,d\pi_\ell$ | Средняя величина перемещения barycentric coordinates | Направление перемещения |
| $D_\ell^\rho=\int(\rho'-\rho)\,d\pi_\ell$ | Знак среднего изменения residual | Встречные изменения разных голов |
| $\Phi_\ell^\rho=\int|\rho'-\rho|\,d\pi_\ell$ | Средняя величина изменения residual | Знак изменения |
| $\Delta\operatorname{Spread}_\ell$ | Разность определённого в §8.6.3 разброса | Полная форма обоих распределений |

[Определения на PDF 295–296](../source/0293-0300-ch08-reextraction-cost-transport-and-entropy.md#section-8-6), [карточка 76](76-transport-diagnostics-and-metric-dependence.md).

**Редакторское уточнение:** положительное значение $\Phi_\ell^\rho$ на рисунке не означает рост residual. Оно совместимо и с уменьшением. Чтобы установить «canonicalising» направление, нужен $D_\ell^\rho=\bar\rho_{\ell+1}-\bar\rho_\ell$. Signed drifts равны разностям marginal means и не зависят от выбранного coupling; unsigned quantities могут зависеть от него. Три скалярных summary также не восстанавливают весь транспортный план.

## Меньший residual относится к фиксированному словарю

В §8.10.3 автор интерпретирует $\bar\rho_{\ell+1}<\bar\rho_\ell$ как более каноническую геометрию следующего слоя. Это содержательное утверждение при неизменных dictionary, projection rule, norm и mask convention. Если словарь расширился, расстояние до hull могло уменьшиться из-за самого расширения. Рост residual может отражать полезную структуру за пределами словаря; знак изменения не определяет task quality. [§8.10.3](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-10-3), [карточка 68](68-causal-pruning-distribution-shift-and-dictionary-growth.md).

## Три связи имеют разную математическую силу

Источник отдельно рассматривает cone→activation, activation→margin и margin→entropy. [§§8.11.1–8.11.3](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-11-1).

Для skew matrix $M_a$ наблюдаемая

$$
\Gamma_\ell(M_a)=\|X_\ell M_aX_\ell^\top\|_F
$$

зависит от формы на фактически посещаемом row span. Ненулевой skew rank чётен; ненулевая форма требует двух подходящих направлений. Если $X=au^\top$, то $XM_aX^\top=0$. Высокий cone index не исключает этот случай. Сам по себе ненулевой projection на одну eigenplane ещё не исключает сокращения вкладов других плоскостей; это разобрано в [карточке 80](80-conditional-compatibility-and-eigenplane-cancellation.md).

Следующая связь записана **схематически**:

$$
\bar\Delta_\ell\ge c_\ell\Gamma_\ell-b_\ell.
$$

Нужно операционально определить alignment coefficient $c_\ell$, opposing contribution $b_\ell$ и масштаб logits. Выбор коэффициента задним числом из самого неравенства не создаёт независимого прогноза. Кроме того, raw score geometry и внимание после softmax — разные уровни: $\Gamma$ нельзя восстановить только из attention entropy. [§8.11.2](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-11-2).

Последняя связь использует уже проверенную оценку $H(p)\le B_n(\Delta)$. Это строгий rowwise envelope при согласованной поддержке и фактических logits; она не утверждает, что меньший envelope гарантирует меньшую измеренную entropy. [§8.11.3](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-11-3), [карточка 78](78-margin-entropy-proxy-and-compatibility.md).

## Как использовать профиль в проекте

Практический отчёт должен показывать отдельно signed/unsigned drift, нормировки cone и activation, фактическую entropy и proxy, а также версию словаря. Figure 8.12 сама предупреждает, что стрелки означают диагностическую совместимость. Эти страницы дают язык измерений, но не показывают нового запуска Xiigen или причинного улучшения обучения. [Figure 8.12](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#figure-8-12).
