DECLARE
-- Проверка зачислений на крты МИР


vAcc  varchar2(20);

Mess varchar2(1000);
Res number;
Function get_CardMir_Acc(acc in acc.caccacc%type, msgErr out varchar2) return number is
  res number;
  agrid number;
  con number;
  platsysid_MIR number;
  stat number;
BEGIN
  begin
    -- ИД платежной системы МИР
    select max(iplplatsysid) into platsysid_MIR from pl_platsys s
      where instr(s.cplplatsysname, 'МИР')>0 or instr(s.cplplatsysname, 'MIR')>0;
    pl_util.outDBMS('ИД платежной системы МИР='||platsysid_MIR);
  exception
    when others then
      pl_util.outDBMS('Не найдена платежная система МИР');
      msgErr:='Не найдена платежная система МИР';
     return -1; -- ошибка
  end;
  -- найдем ИД договора (он должен быть ОДИН)
  select max(IPLAAGRID) into agrid from PL_CA where CACCACC=acc and iplscatype in (-1,14,53);
  pl_util.outDBMS('Договор '||agrid);
  if agrid is null then
    pl_util.outDBMS('пустой ИД Договора');
--    msgErr:='Пустой ИД Договора';
--    return -1; -- ошибка
    return 4; -- нет НИКАКИХ действующих карт - для посторонних счетов (Ржевцев 05.03.2018)
  end if;
  res:=0; -- нет никаких действующих карт на договоре
  -- все карты
  for rec in (select IPLCID,a.DPLCEND,a.IPLPID, a.DPLCOPEN from plc_all a where a.IPLAAGRID=agrid)
  loop
    stat:=pl_card.get_CardStatusNum(rec.iplcid);
     pl_util.outDBMS('stat='||stat);
    if rec.dplcend<sysdate then
      continue; -- срок действия карты закончился
    elsif stat NOT in (2,29,30,31) then
      continue; -- статус картs - блокирована
    end if;
    IF Rec.Dplcopen < to_date('01.07.2017','dd.mm.yyyy') then
      res := 5;
    ELSE  
      res :=2; -- есть карты других платежных систем (не МИР)
    end if; 
    -- наличие на договоре карты с платежной системой мир
    select count(*) into con from pl_p,pl_ctype
      where pl_p.iplpid=rec.iplpid and pl_p.iplctypeid=pl_ctype.iplctypeid
        and pl_ctype.iplplatsysid=platsysid_MIR;
    if con>0 then
      res:=1;
      exit;
    end if;
  end loop;
  return res;
exception
   when others then
     pl_util.outDBMS('Ошибка. Счет '||acc);
     msgErr:='Ошибка. Счет '||acc;
    return -1; -- ошибка
end get_CardMir_Acc;

begin
       For Rec In (select dtrntran, mtrnrsum, ctrnaccc, ctrnowna from trn where dtrntran >= '01-jul-18' and ctrnaccd = '47422810700000000064')
         LOOP
           Res := bsv.get_CardMir_Acc(Rec.Ctrnaccc, Mess);
           IF Res = 2 Then
            dbms_output.put_line( ' !!!!! Зачислено не на карту МИР' || Mess || ' Res= ' || Res ||'  Acc = '|| Rec.Ctrnaccc || ' Дата ' || to_char(Rec.Dtrntran,'dd.mm.yyyy') || ' ' || Rec.Ctrnowna);
           ELSIF Res = 0 then
            dbms_output.put_line( ' !!ОК ! Зачислено на карту МИР' || Mess || ' Res= ' || Res ||'  Acc = '|| Rec.Ctrnaccc || ' Дата ' || to_char(Rec.Dtrntran,'dd.mm.yyyy')|| ' ' || Rec.Ctrnowna);        
           ELSIF Res = 5 then
            dbms_output.put_line( ' Зачислено не на карту МИР но дата открытия < 01.07.2018 '  || Mess || ' Res= ' || Res ||'  Acc = '|| Rec.Ctrnaccc || ' Дата ' || to_char(Rec.Dtrntran,'dd.mm.yyyy')|| ' ' || Rec.Ctrnowna);            
           END IF; 
        END LOOP;
        dbms_output.put_line( '=========================================================================================================');
        For Rec In (select dtrntran, mtrnrsum, ctrnaccc, ctrnowna from trn where dtrntran >= '01-jul-18' and 
                        exists (select 1 from trn_dept_info di where di.inum = itrnnum AND di.ianum = itrnanum and di.cnaltype in ('1','01' ))
                    )    
         LOOP
           Res := get_CardMir_Acc(Rec.Ctrnaccc, Mess);
           IF Res = 2 Then
            dbms_output.put_line( ' !!!!! Зачислено не на карту МИР' || Mess || ' Res= ' || Res ||'  Acc = '|| Rec.Ctrnaccc || ' Дата ' || to_char(Rec.Dtrntran,'dd.mm.yyyy') || ' ' || Rec.Ctrnowna);
           ELSIF Res = 0 then
            dbms_output.put_line( ' !!ОК ! Зачислено на карту МИР' || Mess || ' Res= ' || Res ||'  Acc = '|| Rec.Ctrnaccc || ' Дата ' || to_char(Rec.Dtrntran,'dd.mm.yyyy')|| ' ' || Rec.Ctrnowna);     
           ELSIF Res = 5 then
            dbms_output.put_line( ' Зачислено не на карту МИР но дата открытия < 01.07.2017 '  || Mess || ' Res= ' || Res ||'  Acc = '|| Rec.Ctrnaccc || ' Дата ' || to_char(Rec.Dtrntran,'dd.mm.yyyy')|| ' ' || Rec.Ctrnowna);            
           END IF; 
        END LOOP;       
       
  end ;     
