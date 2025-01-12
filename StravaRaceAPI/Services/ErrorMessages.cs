namespace StravaRaceAPI.Services;

/// <summary>
///     Error message templates.
/// </summary>
public static class ErrorMessages
{
    private static readonly Func<string, ulong, string> NotFoundMessage =
        (item, userId) => $"{item} with id {userId} not found.";

    public static readonly Func<int, string> UserNotFoundMessage = userId => NotFoundMessage("User", (ulong)userId);

    public static readonly Func<ulong, string> EventNotFoundMessage = eventId => NotFoundMessage("Event", eventId);
}