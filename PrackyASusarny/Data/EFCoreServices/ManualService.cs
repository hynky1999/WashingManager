using Microsoft.EntityFrameworkCore;
using PrackyASusarny.Data.Models;

namespace PrackyASusarny.Data.EFCoreServices;

public class ManualService : CrudService<Manual>
{
    public ManualService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<ManualService> logger) : base(
        dbFactory, logger, context => context.Manuals, manual => manual.ManualID)
    {
    }
}