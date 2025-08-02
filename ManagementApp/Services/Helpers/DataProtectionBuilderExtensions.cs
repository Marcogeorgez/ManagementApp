using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;

public static class DataProtectionBuilderExtensions
{
    public static IDataProtectionBuilder PersistKeysToDbContext<TContext>(
        this IDataProtectionBuilder builder) where TContext : DbContext
    {
        builder.Services.AddSingleton<IXmlRepository, EntityFrameworkDataProtectionKeysRepository>();
        return builder;
    }
}