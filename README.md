# QueryConverter
This is built for a student project. The purpose of it is to make it easier to convert query parameters into relevant linq statements. 

## Note
Please note that this method only works for properties that are directly part of the class in question. For example Product.Price would work. Product.PriceDetails.Price won't. 

## Change 1.0.3
- It is no longer necessary to provide the property to search for. Instead this needs to be supplied in the queryparameter, see search below. 
- Multiple sorting statements can be provided. They will be evaluated in order. First is OrderBy, the rest will be ThenBy. 

## Usage
The package provides a generic extension for IQueryable called Build that returns a class called QueryOutcome that has a property called Query that holds the IQueryable that the query parameters have been applied to. You can then use .ToListAsync(), SingleOrDefaultAsync() etc on it. 
Example usage with Entity Framework
```C#
var query = _context.Products.AsNoTracking().AsQueryable():
var queryOutcome = query.Build<ProductEntity>(queryParams);
if (queryOutcome.Success) 
{
  var result = await queryOutcome.Query.ToListAsync();
  return result;
}
```

The build extension method takes in an IQueryCollection of queryParams. If using Azure Functions, for example, they can be accessed with HttpRequest.Query. 

## Supported parameters

Currently it supports the following query parameters (case-insensitive)

Search[property]=string

Limit=int

Offset=int

Sort=property to sort after, asc default

Sort[asc]=property to sort after ascending

Sort[desc]=property to sort after descending

Property= equal to value

Property[gt]= greater than value

Property[lt] = less than value

Property[gte] = greater than or equal to value

Property[lte] = less than or equal to value

Example query: products?search[title]=h&price[gt]=2500&price[lt]=5000&category=pants&sort[desc]=price

Would result in all products with a title that has an h in it, with a price greater than 2500 but less than 5000, that has the category "Pants" and sorted by price descending. 
