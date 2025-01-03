// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Ardalis.Specification.EntityFrameworkCore;

namespace EdNexusData.Broker.Core;  
  
// We are using the EfRepository from Ardalis.Specification
// https://github.com/ardalis/Specification/blob/v5/ArdalisSpecificationEF/src/Ardalis.Specification.EF/RepositoryBaseOfT.cs
public class EfRepository<T> : RepositoryBase<T>, IRepository<T> where T : BaseEntity, IAggregateRoot
{
    private readonly ICurrentUser _currentUser;
    
    public EfRepository(DbContext dbContext, ICurrentUser currentUser) : base(dbContext)
    {
        _currentUser = currentUser;
    }

    public override Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        entity.CreatedBy = _currentUser.AuthenticatedUserId();
        entity.UpdatedAt = null;
        entity.UpdatedBy = null;
        
        return base.AddAsync(entity, cancellationToken);
    }

    public override Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = _currentUser.AuthenticatedUserId();
        
        return base.UpdateAsync(entity, cancellationToken);
    }
}