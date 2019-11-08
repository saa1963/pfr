CREATE OR REPLACE PACKAGE BODY XXI.SAA AS
/******************************************************************************
   NAME:       SAA
   PURPOSE:

   REVISIONS:
   Ver        Date        Author           Description
   ---------  ----------  ---------------  ------------------------------------
   1.0        07/11/2019      soshin       1. Created this package body.
******************************************************************************/

  FUNCTION RegisterPfr ( ErrorMsg      OUT VARCHAR2,                           -- Сообщение об ошибке для возврата
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
                        cIDOpen       IN  VARCHAR2 DEFAULT NULL,                 -- Владелец документа
                        iCat          IN  NUMBER DEFAULT NULL,        -- Категории-группы документа
                        iNum          IN  NUMBER DEFAULT NULL)
        RETURN VARCHAR2
    IS
        cRetCode   VARCHAR2(2000) := 'REG_UNKNOWN_ERROR';
        cErrorMsg  VARCHAR2(2000);
        tabCatNum  TS.T_TabCatNum;
        recCatNum TS.T_CatNum;
        rDeptInfo TS.T_DeptInfo;
    BEGIN
        rDeptInfo.cNalType := '1';
        if iCat is not null then
          begin
            recCatNum.iCat := iCat;
            recCatNum.iNum := iNum;
            tabCatNum := TS.T_TabCatNum();
            tabCatNum.EXTEND;
            tabCatNum(1).iCat := iCat;
            tabCatNum(1).iNum := iNum;
          end;
          else
          begin
            tabCatNum := NULL;
          end;
        end if;
        cRetCode := IDOC_REG.Register (ErrorMsg => cErrorMsg,
                                 OpType => OpType,
                                 RegDate => RegDate,
                                 PayerAcc => PayerAcc,
                                 RecipientAcc => RecipientAcc,
                                 Summa => Summa,
                                 DocDate => DocDate,
                                 Purpose => Purpose,
                                 DocNum => DocNum,
                                 BatNum => BatNum,
                                 ValDate => ValDate,
                                 SubOpType => SubOpType,
                                 cDocCurrency => cDocCurrency,
                                 cVO => cVO,
                                 cIDOpen => cIDOpen,
                                 rDeptInfo =>  rDeptInfo,
                                 tabCatNum => tabCatNum);

        BEGIN
            ErrorMsg := cErrorMsg;
        EXCEPTION
            WHEN VALUE_ERROR THEN
                 ErrorMsg := SUBSTR (cErrorMsg, 1, 256);
        END;

        RETURN cRetCode;
    END RegisterPfr;

END SAA;
/
