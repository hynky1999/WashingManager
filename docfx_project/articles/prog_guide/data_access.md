Data Access
-----------

For Data Access we use Entity Framework Core connected to PostgreSQL with [NPsql](https://www.npgsql.org/index.html).
We wanted to create monolitic app as the app itself is not really complex and creating API Server for DB access needs more work.
Thus EF Core was chosen since it allows relatively safe access to resources and supports LINQ queries.
This also forced us to use Blazor Server as Blazor WebAssembly can't use EF Core as it is client-side only.

However there is one big problem with EF Core. It's has no support for concurrency. This is no problem for MPA where all dependency injection are scoped to the request. Thus each request we get a new instance of the DBContext and thus no concurrency issues.

However Blazor Server has 3 different scopes:
- Singleton
- Scoped
- Transient

Singleton is created once throught the whole app lifetime and the same instance is injected to all the components.
Scoped is created once per SignalR circuit and the same instance is injected to all the components.
Transient creates a new instance of the object every time it is injected.

While the first two are obvious no go for concurrency, the last one might seem plausible. However it is important that making sure that there will be no concurrency issues is the responsibility of the developer of component and is relatively easy to mess up. Such an example can be found by having two buttons that both access DBcontext in one component. If both buttons are clicked at the same time, the Dbcontext will be accessed by two different threads and thus concurrency issues might occur.

We thus decided to go with different approach. For any data access we use a service that has a Factory method that creates a new instance of the DBContext.
Then whenever we need to access a data we create a context in that method, get data and then dispose the context. Such a pattern resembles the Repository pattern.

Such a services can also have a method that takes a DBContext as a parameter. This was needed because we wanted to do a separation of concerns while we also needed to handle multiple updates of db in one transaction. Thus rule of thumb: 
- If data access method takes a DBContext it will just update the DbContext and will not save it or dispose it.
- If it doesn't take a DBContext it will create a new one, update it and then save it and dispose it.

Since all our data access services implement certain interface, it should be easy to switch to API access in future.

The interfaces can be found at <xref:App.Data.ServiceInterfaces>.

The EF Core implementation for these services can be found at <xref:App.Data.EFCoreServices>.


Models
------
The models are stored in App/Data/Models.
All models implement IDBModel interface which is used for the description of model instance.

The DB architecture is as follows:
![DB Architecture](/images/db.png)



Errors
------
All Data Access method should throw these exceptions:
- DBException or its descendants -> General DB Exception
- ArgumentException -> Invalid arguments

We catch any erros that are EF Core related and rethrow them as particular DBException. This is to allow for possible change of Data Access method.
