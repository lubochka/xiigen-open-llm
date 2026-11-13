---
title: "162. Эквивалентности голов: пороговая близость и обращённые импликации hierarchy"
source_pdf_pages: [483, 484]
book_pages: [447, 448]
batch: "010"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 162. Эквивалентности голов: пороговая близость и обращённые импликации hierarchy

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Что определяет source

Definition 13.22 называет три отношения эквивалентностями: параметрические tuples в одном fibre union of kernels; равенство activation operators почти всюду по облакам; orbit distance не больше $\delta$. Definition 13.23 вводит NO-1 и NO-2, после чего Theorem 13.24 заявляет $\mathcal R_{\mathrm{dyn}}\subseteq\mathcal R_{\mathrm{fn}}\subseteq\mathcal R_{\mathrm{par}}$. [Definitions 13.22–13.23](../source/0479-0484-ch13-trajectory-values-and-redundancy-hierarchy.md#definition-13-22), [Theorem 13.24](../source/0479-0484-ch13-trajectory-values-and-redundancy-hierarchy.md#theorem-13-24).

## Направление логики

Буквальные предпосылки книги:

$$
\text{NO-1: }P\Rightarrow F,\qquad
\text{NO-2: }F\Rightarrow D.
$$

Здесь $P,F,D$ означают соответствующие отношения совпадения для пары голов. Из них следует

$$
\mathcal R_{\mathrm{par}}\subseteq\mathcal R_{\mathrm{fn}}\subseteq\mathcal R_{\mathrm{dyn}},
$$

то есть **обратное** напечатанному (13.55) направление. Контрапозиция $P\Rightarrow F$ — это $\neg F\Rightarrow\neg P$, а не $F\Rightarrow P$. Доказательство именно подменяет импликацию её обращением. Кроме того, пример «различные параметры, но одинаковая функция» не опровергает NO-1: он опровергал бы её обратное утверждение.

## Почему близость при положительном пороге не эквивалентность

Для любой обычной метрики отношение $d(x,y)\le\delta$ рефлексивно и симметрично, но обычно нетранзитивно. На прямой точки $0,0.75\delta,1.5\delta$ дают две соседние близкие пары и одну далёкую. Triangle inequality даёт только $d(x,z)\le2\delta$. Поэтому $\mathcal R_{\mathrm{dyn}}(\delta)$ можно рассматривать как множество пар или граф близости, но не как готовое разбиение на equivalence classes. При $\delta=0$ нужны свойства конкретной метрики/псевдометрики и идентификация её нулевых классов.

## Что теряется в orbit measure и activation operator

Равенство эмпирических occupation measures траекторий не восстанавливает временной порядок или endpoint. Последовательности $(a,b)$ и $(b,a)$ имеют одну и ту же эмпирическую меру, но разные конечные точки. Совпадение signature coordinates также не является доказанной инъективной кодировкой $\mathcal A$: координаты могут терять масштаб, направление, симметричный сектор и values. Поэтому первая часть proof не восстанавливает отсутствующую импликацию $D\Rightarrow F$.

Равенство $\mathcal A_h=P_UM_{a,h}P_U$ означает совпадение только сжатого skew-оператора. Две головы с $M_a=0$ имеют одинаковую нулевую $\mathcal A$, но могут иметь разные симметричные scores и разные $W_VW_O$, то есть разные input–output maps. Union of kernels обычно не является линейным подпространством и без отображения/группового действия не определяет fibre. Для полноценной параметрической эквивалентности нужно задать именно такое отображение или gauge action.

## Граница чтения и практический вывод

Страница 484 заканчивается внутри обсуждения strict inclusions, а не в конце доказательства. Продолжение находится на [PDF 485](../source/0485-0490-ch13-dictionary-thresholds-and-rank-contraction.md#pdf-0485), дальнейший разбор — [163](163-channel-symmetry-and-orbit-separation.md). Для использования достаточно уже установленных дефектов: сохранённые предпосылки не доказывают заявленную иерархию, а threshold relation не даёт classes.

Храните разные отношения раздельно: parameter-gauge coincidence, совпадение выбранного observable, близость occupation measures, pointwise совпадение траекторий и task-level ablation effect. Переход между ними требует явной теоремы с корректным направлением, а не совпадения слова «redundancy».
