# Журнал обнаружения проверок (носитель признаков запроса, карточка 126)

Ожидалось назначением: ровно 8 проверок в классе KnnPredicateCarrierLearningTests.

## baseline-red
файл: baseline-red/p108-carrier-baseline.trx
     1	PcAbsent_FourLawfulWaysToSendNoPredicates_StayCompatible
     2	PcCarriers_ListAndReadOnlyCollection_AnswerExactlyLikeTheArray
     3	PcEarlyRefusal_CancelledAndMissingInput_KeepTheirOwnCodes
     4	PcEnumeration_OneShotSequenceIsReadOnce_AndThrowingSequenceRefuses
     5	PcLearned_AllEightLessonsAnswerFully_AndConstantLabelFails
     6	PcNullItem_IsRefusedByNameAndIndex_ForAllThreeCarriers
     7	PcOwnership_CallerMutatesItsListAfterTheRead_MemoryAndAnswerStayPut
     8	PcUnsupported_SingleStringAndIntArray_AreRefusedByName
обнаружено: 8
total="8" executed="8" passed="3" failed="5"

## candidate-green
файл: candidate-green/p108-carrier-candidate.trx
     1	PcAbsent_FourLawfulWaysToSendNoPredicates_StayCompatible
     2	PcCarriers_ListAndReadOnlyCollection_AnswerExactlyLikeTheArray
     3	PcEarlyRefusal_CancelledAndMissingInput_KeepTheirOwnCodes
     4	PcEnumeration_OneShotSequenceIsReadOnce_AndThrowingSequenceRefuses
     5	PcLearned_AllEightLessonsAnswerFully_AndConstantLabelFails
     6	PcNullItem_IsRefusedByNameAndIndex_ForAllThreeCarriers
     7	PcOwnership_CallerMutatesItsListAfterTheRead_MemoryAndAnswerStayPut
     8	PcUnsupported_SingleStringAndIntArray_AreRefusedByName
обнаружено: 8
total="8" executed="8" passed="8" failed="0"

## regression
файл: regression/p108-regression.trx
     1	DuplicateIdAfterTrim_IsRefusedBeforeTokenizer_AndEarlierMemoryKeepsAnswering
     2	IncompleteRow_IsRejectedBeforeTokenizerAndIndexPreparation
     3	KnnLogicalUnit_TrainsWithBpeAndHnswThenPredictsNearestOutput
     4	ReturnedAnswerNestedStepListMutation_DoesNotChangeMemory
     5	ReturnedAnswerTopLevelMutation_DoesNotChangeMemory
     6	SourceAnswerMutatedAfterTraining_DoesNotChangeMemory
     7	TrainedRowKeepsItsOwnMetadata_AfterIngressAndEgressMutation
     8	UniqueIds_AreAccepted_AndEachQueryGetsItsOwnAnswer
обнаружено: 8
total="8" executed="8" passed="8" failed="0"
