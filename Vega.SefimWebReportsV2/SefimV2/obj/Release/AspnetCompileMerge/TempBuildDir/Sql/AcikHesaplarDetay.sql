exec subebazli '##SubeAcikHesaplar', '(SELECT CustomerName, Sum(Debit) as Debit FROM Payment WHERE 
Paymenttime>=''{TARIH1}'' AND Paymenttime<=''{TARIH2}'' AND Debit<>0 group by CustomerName) ','{SUBE}'; 
select Toplamsatis.ServerName as Sube, Toplamsatis.Debit, Toplamsatis.CustomerName from ##SubeAcikHesaplar
as Toplamsatis where Toplamsatis.ServerName not in ('local') 