Create Table PackingCompanies
(
	PackingCompanyId		Int Identity Primary Key,
	PackingCompanyTitle		Nvarchar(100) Not Null Unique,
	PackingCompanyAddress	Nvarchar(250),
	PackingCompanyPhone		Nvarchar(25)
)
Go
Create Table PackingCompanyTanks
(
	PackingCompanyTankId		Int Identity Primary Key,
	PackingCompanyId			Int,
	PackingCompanyTankName		Nvarchar(50),
	PackingCompanyTankCapacity	Decimal(10,2) Default 0.00,
	Constraint FK_PackingCompanyTanks_PackingCompanyId
	Foreign Key(PackingCompanyId)
	References PackingCompanies(PackingCompanyId)
)
Go
Create Table HoneySellingCompanies
(
	HoneySellingCompanyId		Int Identity Primary Key,
	HoneySellingCompanyTitle	Nvarchar(100) Unique,
	HoneySellingCompanyAddress	Nvarchar(250),
	HoneySellingCompanyPhone	Nvarchar(25)
)
Go
Create Table RawHoneyLots
(
	RawHoneyLotId Int Identity Primary Key,
	BeekeeperId uniqueidentifier,
	ApiaryId Int,
    HoneySellingCompanyId Int,
	QuantityKg DECIMAL(10,2),
	CollectionDate DATE,
    Description NVARCHAR(20),
	Constraint FK_RawHoneyLots_BeekeeperId Foreign Key(BeekeeperId)
	References Beekeepers(BeekeeperId),
	Constraint FK_RawHoneyLots_ApiaryId Foreign Key(ApiaryId)
	References Apiaries(ApiaryId),
	Constraint FK_RawHoneyLots_HoneySellingCompanyId Foreign Key(HoneySellingCompanyId)
	References HoneySellingCompanies(HoneySellingCompanyId)
)
Go
Create Table FinalProductBatches (
    BatchId					Int Identity PRIMARY KEY,
	HoneySellingCompanyId	Int,
    PackingCompanyTankId	Int,
    ProductionDate			Date,
    TotalQuantityKg			Decimal(10,2),
	Constraint FK_FinalProductBatches_PackingCompanyTankId
	Foreign Key(PackingCompanyTankId)
	References PackingCompanyTanks(PackingCompanyTankId),
	Constraint FK_FinalProductBatches_HoneySellingCompanyId
	Foreign Key(HoneySellingCompanyId)
	References HoneySellingCompanies(HoneySellingCompanyId)
);
Go
CREATE TABLE BatchCompositions (
    BatchId			INT FOREIGN KEY REFERENCES FinalProductBatches(BatchId),
    RawHoneyLotId	INT FOREIGN KEY REFERENCES RawHoneyLots(RawHoneyLotId),
    QuantityUsedKg	DECIMAL(10,2),
    PRIMARY KEY (BatchId, RawHoneyLotId)
);
Go
CREATE TABLE HoneyProducts (
    HoneyProductId	Int Primary Key Identity,
	BatchQRCode		Uniqueidentifier,
    BatchId			INT FOREIGN KEY REFERENCES FinalProductBatches(BatchID),
    ProductName		NVARCHAR(150),
    PackagingType	NVARCHAR(50),
    UnitWeightKg	DECIMAL(10,2),
    UnitsProduced	INT
);