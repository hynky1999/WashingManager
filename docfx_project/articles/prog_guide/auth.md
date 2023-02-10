Authentication
--------------
Authentication is done using ASP.NET Core Identity.
Since we already use an EF Core we decided to use for Identity as well.
This however is problematic as there is no native support for Blazor to use Identity as it depends on request-response model.

We thus use Razor Pages to handle authentication and then use Blazor Server to handle the rest of the app.
We used scaffolding to create the pages and then modified them to our needs.
The source codes can be found at App.Areas.

Authorization
-------------
Authorization is done using ASP.NET Core Authorization.
In Blazor Server the authorization of pages is done using the Authorize attribute. We use policies to define who can access what
Authorization of specific parts of page is done using AuthenticationStateProvider. Such a provider will be injected to the component and the provides a ClaimsPrincipal that can be used to check if the user has the required claims.


In Razor Pages the authorization is also done using the Folder policy access. In page authorization is done using IAuthorizationService.

We wanted to use just claims but unfortunately Blazor Server doesn't support claims in the Authorize attribute. Thus we had to use policies.

Overall there are 3 different policies:
- UserManagement -> Can create new users
- ModelManagement -> Can create new models (Direct DB access through UI)
- BorrowManagement -> Can create new borrows and end borrows

Each policy requires a user to have certain claim. The claims are:
- ManageUsers -> UserManagement
- ManageModels -> ModelManagement
- ManageBorrows -> BorrowManagement
- UserID -> ID of the user in DB where we store other data

Authorization related things can be found at <xref:PrackyASusarny.Auth.Utils>.
