namespace StravaRaceAPI.Services;

public static class ErrorMessages
{
    private static readonly Func<string, int, string> NotFoundMessage =
        (item, userId) => $"{item} with id {userId} not found.";

    public static Func<int, string> UserNotFoundMessage = userId => NotFoundMessage("User", userId);

    public static Func<int, string> EventNotFoundMessage = eventId => NotFoundMessage("Event", eventId);
}