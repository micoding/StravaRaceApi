namespace StravaRaceAPI.Services;

/// <summary>
///     Error message templates.
/// </summary>
public static class ErrorMessages
{
    private static readonly Func<string, int, string> NotFoundMessage =
        (item, userId) => $"{item} with id {userId} not found.";

    public static readonly Func<int, string> UserNotFoundMessage = userId => NotFoundMessage("User", userId);

    public static readonly Func<int, string> EventNotFoundMessage = eventId => NotFoundMessage("Event", eventId);
}