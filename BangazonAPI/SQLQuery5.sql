

						--SELECT c.Id AS 'Customer Id', 
      --                  c.FirstName AS 'Customer First Name', 
      --                  c.LastName AS 'Customer Last Name',
      --                  c.AccountCreated AS 'Date Joined', 
      --                  c.LastActive AS 'Last Active',
      --                  c.Archived,
						--m.AcctNumber AS 'Account Number',
      --                  m.[Name] As 'Account Name',
      --                  m.Archived
						--FROM Customer c
						--JOIN PaymentType m ON m.CustomerId = c.Id

						SELECT c.Id AS 'CustomerId', 
                        c.FirstName AS 'CustomerFirstName', 
                        c.LastName AS 'CustomerLastName',
                        c.AccountCreated AS 'DateJoined', 
                        c.LastActive AS 'LastActive',
                        c.Archived AS 'CustomerArchived',
						pt.Name AS 'ProductType',
                        p.Id AS 'ProductId',
                        p.Title AS 'ProductName', 
                        p.Description AS 'ProductDescription', 
                        p.Price AS 'ProductPrice',
                        p.Quantity AS 'QuantityAvailable',
                        p.Archived AS 'ProductArchived'
						FROM Customer c
						JOIN Product p ON p.CustomerId = c.Id 
                        JOIN ProductType pt ON pt.Id = p.ProductTypeId;