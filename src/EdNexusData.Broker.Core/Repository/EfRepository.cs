// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

namespace EdNexusData.Broker.Core;  
  
// We are using the EfRepository from Ardalis.Specification
// https://github.com/ardalis/Specification/blob/v5/ArdalisSpecificationEF/src/Ardalis.Specification.EF/RepositoryBaseOfT.cs
public class EfRepository<T> : RepositoryBase<T>, IRepository<T> where T : BaseEntity, IAggregateRoot
{
    private readonly DbContext dbContext;
    private readonly ICurrentUser _currentUser;
    
    public EfRepository(DbContext dbContext, ICurrentUser currentUser) : base(dbContext)
    {
        this.dbContext = dbContext;
        _currentUser = currentUser;
    }

    // public override async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    // {
    //     dbContext.Database.ExecuteSqlRaw("SET collation = 'en_US.utf8'");
    //     return await base.FirstOrDefaultAsync(specification, cancellationToken);
    // }

    public override async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.CreatedBy = _currentUser.AuthenticatedUserId();
        entity.UpdatedAt = null;
        entity.UpdatedBy = null;
        
        return await base.AddAsync(entity, cancellationToken);
    }

    public override async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = _currentUser.AuthenticatedUserId();
        
        await base.UpdateAsync(entity, cancellationToken);
    }
}