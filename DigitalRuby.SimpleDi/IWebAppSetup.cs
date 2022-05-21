namespace DigitalRuby.SimpleDi;

/// <summary>
/// Make a class with a private constructor and inherit this interface to force it to create on web app building.<br/>
/// Constructor should have two parameters: IApplicationBuilder and IConfiguration.<br/>
/// Put your setup logic in the constructor.<br/>
/// You can use this to compartmentalize your web app builder code instead of having one giant app setup method.<br/>
/// This allows class library authors to put web app setup logic in the class library as well.<br/>
/// </summary>
public interface IWebAppSetup
{
}
