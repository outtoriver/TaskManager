namespace TaskManager.Server.Common.Constants;

public static class PermissionCodes
{
    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersEdit = "Users.Edit";
    public const string UsersDelete = "Users.Delete";

    public const string DepartmentsView = "Departments.View";
    public const string DepartmentsManage = "Departments.Manage";

    public const string PositionsView = "Positions.View";
    public const string PositionsManage = "Positions.Manage";

    public const string RolesView = "Roles.View";
    public const string RolesManage = "Roles.Manage";
    public const string PermissionsManage = "Permissions.Manage";

    public const string TasksViewOwn = "Tasks.ViewOwn";
    public const string TasksViewDepartment = "Tasks.ViewDepartment";
    public const string TasksViewAll = "Tasks.ViewAll";
    public const string TasksCreate = "Tasks.Create";
    public const string TasksAssign = "Tasks.Assign";
    public const string TasksEdit = "Tasks.Edit";
    public const string TasksDelete = "Tasks.Delete";
    public const string TasksApprove = "Tasks.Approve";

    public const string CalendarView = "Calendar.View";
    public const string CalendarManage = "Calendar.Manage";

    public const string MeetingsView = "Meetings.View";
    public const string MeetingsCreate = "Meetings.Create";
    public const string MeetingsEdit = "Meetings.Edit";
    public const string MeetingsDelete = "Meetings.Delete";

    public const string SettingsManage = "Settings.Manage";
}