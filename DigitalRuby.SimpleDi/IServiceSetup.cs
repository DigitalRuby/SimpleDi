namespace DigitalRuby.SimpleDi;

/// <summary>
/// Make a class with a private constructor and inherit this interface to force it to create on startup.<br/>
/// Constructor should have two parameters: IServiceColletion and IConfiguration.<br/>
/// Put your setup logic in the constructor.<br/>
/// You can use this to compartmentalize your startup code instead of having one giant setup method.<br/>
/// This allows class library authors to put setup logic in the class library as well.<br/>
/// </summary>
public interface IServiceSetup
{
}
