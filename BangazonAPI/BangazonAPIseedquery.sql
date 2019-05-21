DELETE FROM OrderProduct;
DELETE FROM ComputerEmployee;
DELETE FROM EmployeeTraining;
DELETE FROM Employee;
DELETE FROM TrainingProgram;
DELETE FROM Computer;
DELETE FROM Department;
DELETE FROM [Order];
DELETE FROM PaymentType;
DELETE FROM Product;
DELETE FROM ProductType;
DELETE FROM Customer;


ALTER TABLE Employee DROP CONSTRAINT [FK_EmployeeDepartment];
ALTER TABLE ComputerEmployee DROP CONSTRAINT [FK_ComputerEmployee_Employee];
ALTER TABLE ComputerEmployee DROP CONSTRAINT [FK_ComputerEmployee_Computer];
ALTER TABLE EmployeeTraining DROP CONSTRAINT [FK_EmployeeTraining_Employee];
ALTER TABLE EmployeeTraining DROP CONSTRAINT [FK_EmployeeTraining_Training];
ALTER TABLE Product DROP CONSTRAINT [FK_Product_ProductType];
ALTER TABLE Product DROP CONSTRAINT [FK_Product_Customer];
ALTER TABLE PaymentType DROP CONSTRAINT [FK_PaymentType_Customer];
ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Customer];
ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Payment];
ALTER TABLE OrderProduct DROP CONSTRAINT [FK_OrderProduct_Product];
ALTER TABLE OrderProduct DROP CONSTRAINT [FK_OrderProduct_Order];


DROP TABLE IF EXISTS OrderProduct;
DROP TABLE IF EXISTS ComputerEmployee;
DROP TABLE IF EXISTS EmployeeTraining;
DROP TABLE IF EXISTS Employee;
DROP TABLE IF EXISTS TrainingProgram;
DROP TABLE IF EXISTS Computer;
DROP TABLE IF EXISTS Department;
DROP TABLE IF EXISTS [Order];
DROP TABLE IF EXISTS PaymentType;
DROP TABLE IF EXISTS Product;
DROP TABLE IF EXISTS ProductType;
DROP TABLE IF EXISTS Customer;


CREATE TABLE Department (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL,
	Budget 	INTEGER NOT NULL
);

CREATE TABLE Employee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL,
	DepartmentId INTEGER NOT NULL,
	IsSuperVisor BIT NOT NULL DEFAULT(0),
	Archived BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_EmployeeDepartment FOREIGN KEY(DepartmentId) REFERENCES Department(Id)
);

CREATE TABLE Computer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	PurchaseDate DATETIME NOT NULL,
	DecomissionDate DATETIME,
	Make VARCHAR(55) NOT NULL,
	Manufacturer VARCHAR(55) NOT NULL,
	Archived BIT NOT NULL DEFAULT(0)
);

CREATE TABLE ComputerEmployee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	ComputerId INTEGER NOT NULL,
	AssignDate DATETIME NOT NULL,
	UnassignDate DATETIME,
    CONSTRAINT FK_ComputerEmployee_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_ComputerEmployee_Computer FOREIGN KEY(ComputerId) REFERENCES Computer(Id)
);


CREATE TABLE TrainingProgram (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(255) NOT NULL,
	StartDate DATETIME NOT NULL,
	EndDate DATETIME NOT NULL,
	MaxAttendees INTEGER NOT NULL
);

CREATE TABLE EmployeeTraining (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	TrainingProgramId INTEGER NOT NULL,
    CONSTRAINT FK_EmployeeTraining_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_EmployeeTraining_Training FOREIGN KEY(TrainingProgramId) REFERENCES TrainingProgram(Id)
);

CREATE TABLE ProductType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL
);

CREATE TABLE Customer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL,
 AccountCreated DATE NOT NULL,
 LastActive Date NOT NULL,
	Archived BIT NOT NULL DEFAULT(0)
);

CREATE TABLE Product (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	ProductTypeId INTEGER NOT NULL,
	CustomerId INTEGER NOT NULL,
	Price INTEGER NOT NULL,
	Title VARCHAR(255) NOT NULL,
	[Description] VARCHAR(255) NOT NULL,
	Quantity INTEGER NOT NULL,
	Archived BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_Product_ProductType FOREIGN KEY(ProductTypeId) REFERENCES ProductType(Id),
    CONSTRAINT FK_Product_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id)
);


CREATE TABLE PaymentType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	AcctNumber INTEGER NOT NULL,
	[Name] VARCHAR(55) NOT NULL,
	CustomerId INTEGER NOT NULL,
	Archived BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_PaymentType_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id)
);

CREATE TABLE [Order] (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	CustomerId INTEGER NOT NULL,
	PaymentTypeId INTEGER,
	Archived BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_Order_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
    CONSTRAINT FK_Order_Payment FOREIGN KEY(PaymentTypeId) REFERENCES PaymentType(Id)
);

CREATE TABLE OrderProduct (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	OrderId INTEGER NOT NULL,
	ProductId INTEGER NOT NULL,
    CONSTRAINT FK_OrderProduct_Product FOREIGN KEY(ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_OrderProduct_Order FOREIGN KEY(OrderId) REFERENCES [Order](Id)
);

INSERT INTO Customer (FirstName, LastName, AccountCreated, LastActive, Archived) VALUES ('Bobby', 'Fitzpatrick', '2019-01-14', '2019-05-17', 0);
INSERT INTO Customer (FirstName, LastName, AccountCreated, LastActive, Archived) VALUES ('Russ', 'Miller', '2019-01-21', '2019-01-21', 0), ('Charles', 'Belcher', '2018-10-31', '2018-10-31', 1);

ALTER TABLE Computer ALTER COLUMN PurchaseDate date;
ALTER TABLE Computer ALTER COLUMN DecomissionDate date;

INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Manufacturer, Archived) VALUES ('2018-12-31', NULL, 'Latitude 5590', 'Dell', 0), ('2018-12-31', NULL, 'MacBook', 'Apple', 0), ('2018-12-31', NULL, 'K53E', 'ASUS', 0);

ALTER TABLE ComputerEmployee ALTER COLUMN AssignDate Date;
ALTER TABLE ComputerEmployee ALTER COLUMN UnassignDate Date;

INSERT INTO Department ([Name], Budget) VALUES ('Accounting', 400000), ('Production', 1500000), ('Maintenance', 250000), ('IT', 1500);

INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor, Archived) VALUES ('Jordan', 'Castelloe', 1, 1, 0), ('Kim', 'Preece', 2, 1, 0), ('Josh', 'Joseph', 3, 1, 0), ('Steve', 'Brownlee', 1, 0, 1), ('Natalie', 'Roper', 4, 0, 0);

INSERT INTO ComputerEmployee (ComputerId, EmployeeId, AssignDate, UnassignDate) VALUES (2, 2, '2019-01-01', NULL), (3, 3, '2019-01-01', NULL), (1, 4 , '2019-01-01', '2019-03-01');

ALTER TABLE TrainingProgram ALTER COLUMN StartDate Date;
ALTER TABLE TrainingProgram ALTER COLUMN EndDate Date;
INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees) VALUES ('Sensitivity Training', '2019-04-01', '2019-04-08', 20), ('Bangazon Corp. Culture', '2019-01-14', '2019-01-21', 20), ('Taco Building', '2019-04-14', '2019-04-15', 25);

INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId) VALUES (2, 3), (2, 2), (3, 1), (4, 2), (5, 3);

ALTER TABLE PaymentType ALTER COLUMN AcctNumber bigint;

INSERT INTO PaymentType ([Name], AcctNumber, CustomerId, Archived) VALUES ('Mastercard',  12345567891234567, 2, 0), ('J.P. Morgan Chase Bank', 555999444789, 3, 0), ('VISA', 09876543210123456, 1, 1), ('Discover', 135790864213579, 2, 0);

INSERT INTO ProductType ([Name]) VALUES ('Coffee Mugs'), ('Electronics'), ('Home Decor'), ('WMDs'), ('Bitcoin & Accessories');

ALTER TABLE Product ALTER COLUMN Price decimal(9,2);

INSERT INTO Product (CustomerId, ProductTypeId, Title, [Description], Price, Quantity, Archived) VALUES (2, 4, 'ICBM', 'Multiple Warhead Re-entry Vehicle', 999999.99, 1, 0), (1, 1, 'Hoagie coffee mug', 'Hand-thrown and fired coffee mug with photo of Hoagie encased in clear glaze', 15.99, 3, 0), (3, 5, 'Bitcoin Miner', 'Lightly, gently used', 350.00, 0, 1), (3, 3, 'Entry Mat', 'Artisan hand made using recycled garbage bags', 150.00, 5, 0);

INSERT INTO [Order] (CustomerId, PaymentTypeId, Archived) VALUES (1, 4, 0), (2, 2, 1), (3, 3, 0);

INSERT INTO OrderProduct (OrderId, ProductId) VALUES (1, 2), (2, 1), (3, 3);

Select * From OrderProduct;
Select * From Customer;
Select * From PaymentType;
Select * From [Order];
Select * From Product;
Select * From ProductType;
Select * FROM EmployeeTraining;
Select * From TrainingProgram;
Select * From ComputerEmployee;
Select * From Employee;
Select * From Department;
Select * From Computer;