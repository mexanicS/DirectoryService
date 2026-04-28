using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Application.DataBase;

public interface ITransactionScope
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}