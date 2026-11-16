public record ResourceNotification<T>(
    T id,
    string resource,
    string method, 
    string status,
    string message
);

