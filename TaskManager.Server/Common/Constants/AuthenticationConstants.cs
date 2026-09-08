namespace TaskManager.Server.Common.Constants;

public static class AuthenticationConstants
{
    public const string ApplicationCookieScheme = "TaskManagerCookie";

    public const string WindowsScheme = "Windows";

    public const string ApplicationCookieName = "TaskManager.Auth";

    public const string UserIdClaim = "taskmanager:user-id";

    public const string DisplayNameClaim = "taskmanager:display-name";
}
