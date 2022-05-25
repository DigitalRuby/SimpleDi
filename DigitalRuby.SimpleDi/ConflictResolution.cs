namespace DigitalRuby.SimpleDi;

/// <summary>
/// What to do if there is a conflict when registering services
/// </summary>
public enum ConflictResolution
{
    /// <summary>
    /// Add. This will result in multiple services for an interface if more than one are added.
    /// </summary>
    Add = 0,

    /// <summary>
    /// Replace. This will make sure only one implementation exists for an interface.
    /// </summary>
    Replace = 1,

    /// <summary>
    /// Skip. Do not register this service if another implementation exits for the interface.
    /// </summary>
    Skip = 2,

    /// <summary>
    /// Throw an exception if another class is registered for the interface.
    /// </summary>
    Error = 3
}
