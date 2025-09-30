namespace Metrics.Application.Results;

public enum ErrorType
{
    None = 0, //
    NotFound, // ----------resource not found,    
    ConcurrencyConflict, //
    InvalidArgument, // ---invalid (empty) input, argument 
    ValidationError, // ---invalid format, 
    DatabaseError, // -----connection, deadlock, timout, ... (GENERIC)
    DuplicateKey, // ------constraint violation, 
    UnexpectedError //
}
