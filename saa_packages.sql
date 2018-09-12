DROP PACKAGE XXI.SAA_PACKAGE;

CREATE OR REPLACE PACKAGE XXI.SAA_PACKAGE AS
/******************************************************************************
   NAME:       SAA_PACKAGE
   PURPOSE:

   REVISIONS:
   Ver        Date        Author           Description
   ---------  ----------  ---------------  ------------------------------------
   1.0        11/09/2018      soshin       1. Created this package.
******************************************************************************/

  FUNCTION Register ( ErrorMsg      OUT VARCHAR2,                           -- Сообщение об ошибке для возврата
                        OpType        IN  TRN.iTRNtype%TYPE,                  -- Тип операции
                        RegDate       IN  DATE,                               -- Дата регистрации
                        PayerAcc      IN  TRN.cTrnAccD%TYPE,                           -- Счет дебета
                        RecipientAcc  IN  TRN.cTrnAccD%TYPE,                           -- Счет получателя
                        Summa         IN  TRN.mTRNsum%TYPE,                   -- Сумма, если документ нац.вал. то это сумма в рублях
                        DocDate       IN  DATE DEFAULT NULL,                  -- Дата документа
                        Purpose       IN  TRN.cTRNpurp%TYPE DEFAULT NULL,     -- Назначение платежа
                        DocNum        IN  TRN.iTRNdocnum%TYPE DEFAULT 0,      -- Номер документа
                        BatNum        IN  TRN.iTRNbatnum%TYPE DEFAULT 0,      -- Номер пачки
                        ValDate       IN  DATE DEFAULT NULL,                  -- Дата платежа (валютирования)
                        SubOpType     IN  TRN.iTRNsop%TYPE DEFAULT NULL,      -- Тип операции 2-го порядка
                        cDocCurrency  IN  TRN.cTrnCur%TYPE DEFAULT NULL,              -- Валюта документа (суммы)
                        cVO           IN  TRN.cTRNVO%TYPE DEFAULT NULL,       -- Вид операции
                        cCreatStatus  IN  VARCHAR2 DEFAULT NULL,
                        cBudCode      IN  VARCHAR2 DEFAULT NULL,
                        cOKATOCode    IN  VARCHAR2 DEFAULT NULL,
                        cNalPurp      IN  VARCHAR2 DEFAULT NULL,
                        cNalPeriod    IN  VARCHAR2 DEFAULT NULL,
                        cNalDocNum    IN  VARCHAR2 DEFAULT NULL,
                        cNalDocDate   IN  VARCHAR2 DEFAULT NULL,
                        cNalType      IN  VARCHAR2 DEFAULT NULL,
                        cNalFlag      IN  VARCHAR2 DEFAULT NULL,
                        cDocIndex     IN  VARCHAR2 DEFAULT NULL)
        RETURN VARCHAR2;

END SAA_PACKAGE;
/
