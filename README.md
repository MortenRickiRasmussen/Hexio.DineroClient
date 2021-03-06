## Dinero Client

A simple and small package for interacting with the Dinero Public Api found at [Dinero Public api](https://api.dinero.dk)

**This package is not complete, it is ready for use by some endpoints have still not been implemented**

### How to Install

Find the package on [Nuget.org](https://www.nuget.org/packages/Hexio.DineroClient/)

OR

`dotnet add package Hexio.DineroClient`

And you are good to go :tada:

### How To Use

This package uses autofac for easy integration into your dependency injection chain. 

In you Startup.cs in method depending on if your are going to use the client with only 1 or multiple organizations
``` cs
public void ConfigureContainer(ContainerBuilder builder)
{
    // To use with one organization
    var settings = _configuration.GetSection("DineroApiSettings").Get<SingleDineroAccountApiSettings>();

    // To use with multiple organizations
    var settings = _configuration.GetSection("DineroApiSettings").Get<DineroApiSettings>();
    
    var module = new DineroModule(settings);
    
    builder.RegisterModule(module);
}
```

And then you need to setup your environment e.g. in your appsettings file
```
 {
   "DineroApiSettings": {
     "AuthUrl": "https://authz.dinero.dk",
     "ApiUrl": "https://api.dinero.dk",
     "ClientId": "SOMEClIENTID",
     "ClientSecret": "SOMECLIENTSECRET",
     "ApiKey": "SOMEAPIKEY", //ONLY NEEDED IF USING SingleDineroAccountApiSettings
     "OrganizationId": 123456 //ONLY NEEDED IF USING SingleDineroAccountApiSettings
   }
 }
```

### Authentication

To start using the Client you have to call the method `SetAuthorizationHeader(IDineroAuthClient authClient, string apiKey, int organizationId)`
This makes a call to the Token service at Dinero which then issues a JWT which is then set on the client to use for all further requests.

The AuthClient can be supplied by Dependency Injection
``` cs
await client.SetAuthorizationHeader(authClient, "SOMEAPIKEY", 123456");
```

### Filtering on list endpoints
When making calls to list endpoints in Dinero, they often supply the possibility for filtering and selecting which fields should be returned

To interact with these endpoints you can use the QueryCreator class

``` cs
public async Task GetMyProduct(IDineroClient client, IDineroAuthClient authClient, SingleDineroAccountApiSettings settings)
{
    await client.SetAuthorizationHeader(authClient, settings.ApiKey, settings.OrganizationId);

    var query = new QueryCreator<ProductReadModel>()
        .Where(x => x.Name, QueryOperator.Eq, "MyProduct")
        .Include(x => x.TotalAmount);

    var products = await client.GetProducts(query);
}
```
