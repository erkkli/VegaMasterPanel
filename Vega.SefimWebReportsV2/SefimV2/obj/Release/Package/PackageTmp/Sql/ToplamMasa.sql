declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';
declare @tablo table (Id bigint, BillState varchar(1),BillType varchar(1),Tarih datetime)

insert into @tablo ( Id,BillState,BillType,Tarih)(
Select 
BillHeader.Id,
BillHeader.BillState,
BillHeader.BillType
,(select top 1 Date from Bill where bill.HeaderId=BillHeader.Id)
From BillHeader
)
select count(*) as 'toplam' from @tablo where Tarih>@Trh1 and Tarih<@Trh2