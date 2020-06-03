CREATE OR REPLACE PACKAGE XXI.SAA AS
/******************************************************************************
   NAME:       SAA
   PURPOSE:

   REVISIONS:
   Ver        Date        Author           Description
   ---------  ----------  ---------------  ------------------------------------
   1.0        07/11/2019      soshin       1. Created this package.
******************************************************************************/

  FUNCTION RegisterPfr ( ErrorMsg      OUT VARCHAR2,                           -- Сообщение об ошибке для возврата
                        OpType        IN  TRN.iTRNtype%TYPE,                  -- Тип операции
                        RegDate       IN  DATE,                               -- Дата регистрации
                        PayerAcc      IN  TRN.cTrnAccD%TYPE,                           -- Счет дебета
                        RecipientAcc  IN  TRN.cTrnAccD%TYPE,                           -- Счет получателя
                        Summa         IN  TRN.mTRNsum%TYPE,                   -- Сумма, если документ нац.вал. то это сумма в рублях
                        iTrnNum       IN  TRN.ITRNNUM%TYPE,                   -- id платежки с ПФР
                        DocDate       IN  DATE DEFAULT NULL,                  -- Дата документа
                        Purpose       IN  TRN.cTRNpurp%TYPE DEFAULT NULL,     -- Назначение платежа
                        DocNum        IN  TRN.iTRNdocnum%TYPE DEFAULT 0,      -- Номер документа
                        BatNum        IN  TRN.iTRNbatnum%TYPE DEFAULT 0,      -- Номер пачки
                        ValDate       IN  DATE DEFAULT NULL,                  -- Дата платежа (валютирования)
                        SubOpType     IN  TRN.iTRNsop%TYPE DEFAULT NULL,      -- Тип операции 2-го порядка
                        cDocCurrency  IN  TRN.cTrnCur%TYPE DEFAULT NULL,              -- Валюта документа (суммы)
                        cVO           IN  TRN.cTRNVO%TYPE DEFAULT NULL,       -- Вид операции
                        cIDOpen       IN  VARCHAR2 DEFAULT NULL,                 -- Владелец документа
                        iCat          IN  NUMBER DEFAULT NULL,        -- Категории-группы документа
                        iNum          IN  NUMBER DEFAULT NULL
                        )
        RETURN VARCHAR2;
END SAA;
/
