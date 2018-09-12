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

  FUNCTION Register ( ErrorMsg      OUT VARCHAR2,                           -- ��������� �� ������ ��� ��������
                        OpType        IN  TRN.iTRNtype%TYPE,                  -- ��� ��������
                        RegDate       IN  DATE,                               -- ���� �����������
                        PayerAcc      IN  TRN.cTrnAccD%TYPE,                           -- ���� ������
                        RecipientAcc  IN  TRN.cTrnAccD%TYPE,                           -- ���� ����������
                        Summa         IN  TRN.mTRNsum%TYPE,                   -- �����, ���� �������� ���.���. �� ��� ����� � ������
                        DocDate       IN  DATE DEFAULT NULL,                  -- ���� ���������
                        Purpose       IN  TRN.cTRNpurp%TYPE DEFAULT NULL,     -- ���������� �������
                        DocNum        IN  TRN.iTRNdocnum%TYPE DEFAULT 0,      -- ����� ���������
                        BatNum        IN  TRN.iTRNbatnum%TYPE DEFAULT 0,      -- ����� �����
                        ValDate       IN  DATE DEFAULT NULL,                  -- ���� ������� (�������������)
                        SubOpType     IN  TRN.iTRNsop%TYPE DEFAULT NULL,      -- ��� �������� 2-�� �������
                        cDocCurrency  IN  TRN.cTrnCur%TYPE DEFAULT NULL,              -- ������ ��������� (�����)
                        cVO           IN  TRN.cTRNVO%TYPE DEFAULT NULL,       -- ��� ��������
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
