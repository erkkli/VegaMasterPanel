							declare @Sube nvarchar(100) = '{SUBEADI}';
                            declare @Trh1 nvarchar(20) = '{TARIH1}';
                            declare @Trh2 nvarchar(20) = '{TARIH2}';
							
                            declare @Satis_Cash decimal(18,2);
                            declare @Satis_Credit decimal(18,2);
                            declare @Satis_Ticket decimal(18,2);
                            declare @Satis_Debit decimal(18,2);
                            declare @Satis_ikram decimal(18,2);

                            declare @DebitPayment_Cash decimal(18,2);
                            declare @DebitPayment_Credit decimal(18,2);
                            declare @DebitPayment_Ticket decimal(18,2);

                            declare @DirectTransaction_Total decimal(18,2);
	                        
                            WITH Toplamsatis as (

                            --SELECT ISNULL(Sum(CashPayment),0.0) as Cash, ISNULL(Sum(CreditPayment),0.0) as Credit, ISNULL(Sum(TicketPayment),0.0) as Ticket, 0.0 as Debit, 0.0 as ikram 
                            --FROM Collect
                            --WHERE Paymenttime >= @Trh1 AND Paymenttime <= @Trh2

                            --UNION ALL 
                            SELECT ISNULL(Sum(CashPayment),0.0) as Cash, ISNULL(Sum(CreditPayment),0.0) as Credit, ISNULL(Sum(TicketPayment),0.0) as Ticket, ISNULL(Sum(Debit),0.0) as Debit, 0.0 as ikram 
                            FROM Payment
                            WHERE Paymenttime >= @Trh1 AND Paymenttime <= @Trh2

                            --UNION ALL
                            --select 0.0 as Cash, 0.0 as Credit, 0.0 as Ticket, 0.0 as Debit, ISNULL(sum(OriginalPrice),0.0) as ikram 
                            --FROM PaidBill 
                            --where PaymentTime >= @Trh1 and PaymentTime <= @Trh2 and Price=0 and ProductName Not Like '$%'

                            ) 
                            select 
	                            @Satis_Cash = SUM(Cash), 
	                            @Satis_Credit= SUM(Credit), 
	                            @Satis_Ticket= Sum(Ticket), 
	                            @Satis_Debit= Sum(Debit), 
	                            @Satis_ikram= Sum(ikram) 
                            from toplamsatis  

                            SELECT  
	                            @DebitPayment_Cash = ISNULL(Sum(CashPayment),0.0), 
	                            @DebitPayment_Credit = ISNULL(Sum(CreditPayment),0.0),
	                            @DebitPayment_Ticket = ISNULL(Sum(TicketPayment),0.0)
                            FROM DebitPayment 
                            WHERE [Date]> = @Trh1 AND [Date] <= @Trh2

                            SELECT 
	                            @DirectTransaction_Total = ISNULL(Sum(Total),0.0)
                            FROM DirectTransaction 
                            WHERE [Date] >= @Trh1 AND [Date] <= @Trh2

                            select
                             @Sube as Sube,
                             @Satis_Cash + @Satis_Credit + @Satis_Ticket + @DebitPayment_Cash + @DebitPayment_Credit + @DebitPayment_Ticket + @DirectTransaction_Total  as ToplamCiro,
                             @Satis_Cash + @DebitPayment_Cash + @DirectTransaction_Total as Cash,
                             @Satis_Credit + @DebitPayment_Credit as Credit,
                             @Satis_Ticket + @DebitPayment_Ticket as Ticket
                     
                        
                            
                          
                     
                        