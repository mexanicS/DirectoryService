using CSharpFunctionalExtensions;

using SharedKernel;

namespace DirectoryService.Application.Database;

public interface ITransactionScope
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}