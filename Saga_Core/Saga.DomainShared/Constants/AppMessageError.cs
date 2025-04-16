namespace Saga.DomainShared.Constants;

public static class AppMessageError
{
    public static string SessionExpired => "Session's Expired";
    public static string UserNotFound => "User's Not Found";
    public static string RequireEmpty => "Required's Empty";

    //// access
    public static string DontHaveReadAccess => "You don't have access to this page.";
    public static string DontHaveWriteAccess => "You don't have access to create data.";
    public static string DontHaveUpdateAccess => "You don't have access to update data.";
    public static string DontHaveDeleteAccess => "You don't have access to delete data.";

    public static string TokenIsRequired => "Application Token's Required";
    public static string TokenNotValid => "Application Token's Not Valid";
    public static string TokenNotRecognized => "Application Token not Recognized";
    public static string ValidationError => "One or more validation failures have occurred.";

    public static string ForbiddenReadAccess => "You dont have access to this action";
    public static string ForbiddenWriteAccess => "You dont have access to Write data";
    public static string ForbiddenUpdateAccess => "You dont have access to Update data";
    public static string ForbiddenDeleteAccess => "You dont have access to Delete data";

    public static string CodeAlreadyExists => "Code Already Exists";
    public static string CodeIsRequired => "Code's Required";
    public static string InvalidRequest => "Invalid Request";
    public static string NotConnectToServer => "Can't connect to server";
}

