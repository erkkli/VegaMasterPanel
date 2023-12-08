
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';


SELECT 

ISNULL(
(SELECT COUNT(*) FROM PrintJob WHERE 
PrintJob.JobType=2 AND
PrintJob.BillHeaderId=DeletedBill.HeaderId AND PrintJob.DateCreated <DeletedBill.DeletingTime)

,0)
 printcount,
 -- ISNULL(Sum(Quantity*OriginalPrice),0) AS ToplamIptalTutari,
*FROM DeletedBill
WHERE 
DeletingTime >= @Trh1 AND DeletingTime <= @Trh2 and ProductName Not Like '$%'
order by DeletingTime desc 
