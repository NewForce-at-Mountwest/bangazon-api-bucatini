# Bangazon API!!!!

# URL's Supported


# -Customers
URL - https://localhost:5001/api/Customer  

Supported Verbs
`GET`
`POST`
`PUT`

If the query string parameter of `?_include=products` is entered, then any products that the customer is selling will be included in the response.

If the query string parameter of `?_include=payments` is provided, then any payment types that the customer has used to pay for an order will be included in the response.

If the query string parameter of `q` is provided when querying the list of customers, then any customer that has a property value that matches the pattern will be returned.

If `/customers?q=mic` is requested, then any customer whose first name is Michelle, or Michael, or Domicio will be returned. Any customer whose last name is Michaelangelo, or Omici, Dibromic will be returned.

# -Product
URL - https://localhost:5001/api/Product

Supported Verbs
`GET`
`POST`
`PUT`
`DELETE`

# -Payment Type
URL - https://localhost:5001/api/PaymentType

Supported Verbs
`GET`
`POST`
`PUT`
`DELETE`

# -Product Type
URL - https://localhost:5001/api/ProductType

Supported Verbs
`GET`
`POST`
`PUT`
`DELETE`

# - Order
URL - https://localhost:5001/api/Order

Supported Verbs
`GET`
`POST`
`PUT`
`DELETE`

Can filter out completed orders with the `?completed=false` query string parameter. If the parameter value is true, then only completed order will be returned.

If the query string parameter of `?_include=products` is in the URL, then the list of products in the order will be returned.

If the query string parameter of `?_include=customers` is in the URL, then the customer representation will be included in the response.

