---
title: "299. Structural units: head count, rank basis и kernel directions считаются отдельно"
source_pdf_pages: [703, 713]
book_pages: [667, 677]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 299. Structural units: head count, rank basis и kernel directions считаются отдельно

## Полезное различие Definition 17.13

Structural unit — реально выделенная head с ненулевым motor и участием в attention. Kernel direction — столбец, добавляемый в deflation basis без новой head. Это полезное различие архитектурной стоимости и численной работы: zero-energy direction не должна автоматически порождать обучаемый блок. [Definition 17.13](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#definition-17-13).

Однако Proposition 17.14 дополнительно отождествляет одну structural unit с одной dimension basis. Именно этот шаг не согласован с Algorithm 7 и собственным Listing 17.1.

## Bidirectional head использует две eigen-directions

В Step 5 один motor записан в виде

$$
M=\alpha\bigl(u^+(u^-)^\top-u^-(u^+)^\top\bigr).
$$

При ортонормированных $u^+,u^-$ и $\alpha\ne0$ его rank равен двум: он ненулевым вращательным блоком действует на $\operatorname{span}\{u^+,u^-\}$. Step 5 выделяет $m$ heads, а Step 6 добавляет в $Q$ обе группы направлений и возвращает $K+2m$. [Algorithm 7](../source/0703-0705-ch17-unified-gate-algorithm-and-regularised-block-flow.md#algorithm-7).

Если все новые directions независимы и ранее не включались в basis, для этого конкретного варианта корректный dimension bookkeeping имеет вид

$$
r_Q=2N_{\mathrm{struct}}+K_C,
$$

где $r_Q=\operatorname{rank}Q$, а $K_C$ — число **новых независимых** kernel directions. Для variable-rank heads надо суммировать соответствующие ranks; при перекрытии пространств считать rank их объединения. Простая сумма counts тогда уже не достаточна.

Переименование $N_{\mathrm{defl}}:=N_{\mathrm{struct}}+K_C$ делает (17.46) тождеством по определению, но перестаёт означать «total number of deflated directions», на чём основано доказательство. [Proposition 17.14 и (17.46)](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#proposition-17-14).

## Собственный пример показывает несогласованность единиц

В dimension-four listing одна head использует $v_1,v_4$, а kernel direction равна $e_4$. В результате $Q\in\mathbb R^{4\times3}$ и $\operatorname{rank}Q=3$. Но (17.48) суммирует $N_{\mathrm{struct}}^*+K_C^*=1+1=2$. Число 2 корректно считает head плюс kernel item; оно не равно dimension deflated subspace. [Listing 17.1](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#listing-17-1), [(17.48)](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#equation-17-48), [локальный пересчёт](300-dimension-four-listing-recalculation-and-residual.md).

Кроме того, уже включённые в $Q$ directions лежат в kernel нового $P_Q^\perp AP_Q^\perp$. Повторно обнаружить их как zero eigenvalues легко; повторно засчитать как новые независимые directions нельзя. Нужен поиск на действующем complement и rank check при добавлении.

## Что нельзя получить простым вычитанием

Exact kernel direction не содержит энергии исходного оператора. Её явное обнаружение улучшает guard и accounting, но не означает, что оптимальному алгоритму без этой записи обязательно понадобилась бы лишняя head. Чтобы утверждать экономию «одна kernel direction — одна head», нужна отдельная модель ошибочных allocations и их исправления.

Также здесь $\kappa_A$ определена как $\Gamma_0/\theta_w$, тогда как прежняя directional complexity использовала другую шкалу направленного захвата. Замена denominator меняет содержание оценки, а не только обозначение. Формула $\Theta(\cdot)$ содержит неизвестные constants и сама по себе не задаёт конкретного численного upper bound $\approx4$. [Определение при (17.45)](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#equation-17-45), [сравнение на PDF 713](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#pdf-0713).

Для проекта стоит хранить четыре разные величины: количество heads, суммарный motor rank, rank накопленного $Q$ и число новых kernel directions. Слово «zero-cost» здесь означает отсутствие trainable head; хранение basis и его orthogonalisation всё равно требуют памяти и вычислений. Отделённые counters позволяют оценивать заявленную sparsity без смешения архитектурного бюджета и spectral dimension.
