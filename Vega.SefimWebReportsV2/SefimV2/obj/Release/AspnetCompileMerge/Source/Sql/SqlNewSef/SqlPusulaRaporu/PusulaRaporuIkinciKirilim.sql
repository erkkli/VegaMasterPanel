
Declare @par1 nvarchar(20) = '{ProductId}';


select    p.CashPayment,p.CreditPayment,p.TicketPayment, (bwh.Date) as SiparisZamani, * from BillWithHeader bwh
LEFT join Payment p on bwh.HeaderId= p.HeaderId

where bwh. ProductName!='[R]Rezervasyon' and bwh.HeaderId=@par1 