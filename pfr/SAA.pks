CREATE OR REPLACE PACKAGE XXI.SAA AS
/******************************************************************************
   NAME:       SAA
   PURPOSE:

   REVISIONS:
   Ver        Date        Author           Description
   ---------  ----------  ---------------  ------------------------------------
   1.0        07/11/2019      soshin       1. Created this package.
******************************************************************************/

  FUNCTION RegisterPfr ( ErrorMsg      OUT VARCHAR2,                           -- ��������� �� ������ ��� ��������
                        OpType        IN  TRN.iTRNtype%TYPE,                  -- ��� ��������
                        RegDate       IN  DATE,                               -- ���� �����������
                        PayerAcc      IN  TRN.cTrnAccD%TYPE,                           -- ���� ������
                        RecipientAcc  IN  TRN.cTrnAccD%TYPE,                           -- ���� ����������
                        Summa         IN  TRN.mTRNsum%TYPE,                   -- �����, ���� �������� ���.���. �� ��� ����� � ������
                        iTrnNum       IN  TRN.ITRNNUM%TYPE,                   -- id �������� � ���
                        DocDate       IN  DATE DEFAULT NULL,                  -- ���� ���������
                        Purpose       IN  TRN.cTRNpurp%TYPE DEFAULT NULL,     -- ���������� �������
                        DocNum        IN  TRN.iTRNdocnum%TYPE DEFAULT 0,      -- ����� ���������
                        BatNum        IN  TRN.iTRNbatnum%TYPE DEFAULT 0,      -- ����� �����
                        ValDate       IN  DATE DEFAULT NULL,                  -- ���� ������� (�������������)
                        SubOpType     IN  TRN.iTRNsop%TYPE DEFAULT NULL,      -- ��� �������� 2-�� �������
                        cDocCurrency  IN  TRN.cTrnCur%TYPE DEFAULT NULL,              -- ������ ��������� (�����)
                        cVO           IN  TRN.cTRNVO%TYPE DEFAULT NULL,       -- ��� ��������
                        cIDOpen       IN  VARCHAR2 DEFAULT NULL,                 -- �������� ���������
                        iCat          IN  NUMBER DEFAULT NULL,        -- ���������-������ ���������
                        iNum          IN  NUMBER DEFAULT NULL
                        )
        RETURN VARCHAR2;
END SAA;
/
