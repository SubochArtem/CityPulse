namespace Polls.Domain.Common;

public static class CommonErrors
{
    public static Error DatabaseError => 
        Error.Failure("An error occurred while saving changes to the database");
}
