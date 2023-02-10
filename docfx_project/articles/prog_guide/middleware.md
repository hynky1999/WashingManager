ContextHookMiddleware
----------------------
Because the admin can change the DB models, we needed a way to handle such changes. It can happen that admin add a Reservation throught the UI and then we need to inform ReservationManager about it. However since the Model menu the same generic CRUD methods for all models, we can't just call a method on ReservationManager from there. Of course we could check for model type but we wanted to have some sort of separation of concerns.

We thus created a simple middleware where a component can register a hook that will be called when some action is performed on a model. The action supported are the ones that Entity Framework Core supports -> EntityState.Added, EntityState.Modified, EntityState.Deleted etc...

When we modify the model we then call the OnSave method of the middleware with model and entity state. The middleware will then call all the hooks that are registered for that model and entity state.

The source is at <xref:App.Middlewares.IContextHookMiddleware>.