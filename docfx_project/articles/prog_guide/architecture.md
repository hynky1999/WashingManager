Web server Framework
--------------------
The project is based upon .NET 6.0 and C# 10.0.

The project uses all 3 Frameworks currently supported by ASP.NET Core.

- Blazor Server
- MVC
- Razor Pages

At the start the plan was to only use Blazor Server, however due to it's architecture the MVC/Razor Pages had to be added.

To see the reasons refer to: 
- [Razor Pages](auth.md)
- [MVC](loc.md)


Blazor Server
-------------
Blazor Server is SPA-like framework and unlike MPA (MVC/Razor Pages) it doesn't exchange any HTTP requests after initial page load.
However it is also not fully fledged SPA. The state of the app is stored on the server and the updates are communicated to the client via SignalR.
This possesses a problem when we have high IO operations app as there needs to be a lot of communication between the server and the client.
Since the state is stored on the server, it is also way more resource intensive than Blazor WebAssembly counterpart.

However unlike the WebAssembly conterpart it allows to use direct DB communication without making api endpoints for DB.


Frontend
--------

For UI we used predefined components from [AntDesign](https://antblazor.com/en-US/).
We used this as it is a very good looking UI and it is also very easy to use.

It also provides support for charting and tables which we used for displaying data.

All the pages and components can be found at App.Features.
The pages and divided into their respective features:

- <xref:App.Features.Charts.Pages> -> All pages which shows how many borrows were made etc...
- <xref:App.Features.AdminPanel.Pages> -> Serves for managing DB models. We have created a UI which is able to handle models without creating separate pages for each model.
- <xref:App.Features.BEManagment.Pages> -> Serves for managing borrows and end borrows and showing availability of Borrowable Entities.
- <xref:App.Features.Reservations.Pages> -> Serves for managing reservations.
- <xref:App.Features.UserPages.Pages> -> Serves for showing user data (Currently only cash).
- <xref:App.Features.Layout> -> Contains the layout of Blazor side of app.

Every single feature has components and pages folder which contains the components and pages respectively.

Other components that are used in multiple places are stored in App.Shared.

For non Blazor components we use bootstrap 5.3.