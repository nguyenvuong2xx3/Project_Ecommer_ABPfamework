namespace Acme.SimpleTaskApp.Authentication.External.External.Facebook;

public class FacebookExternalLoginInfoProvider : IExternalLoginInfoProvider
{
    public string Name { get; } = "Facebook";


    protected string AppId { get; set; }

    protected string AppSecret { get; set; }

    protected ExternalLoginProviderInfo ExternalLoginProviderInfo { get; set; }

    public FacebookExternalLoginInfoProvider(string appId, string appSecret)
    {
        AppId = appId;
        AppSecret = appSecret;
        CreateExternalLoginInfo();
    }

    private void CreateExternalLoginInfo()
    {
        ExternalLoginProviderInfo = new ExternalLoginProviderInfo("Facebook", AppId, AppSecret, typeof(FacebookAuthProviderApi));
    }

    public virtual ExternalLoginProviderInfo GetExternalLoginInfo()
    {
        return ExternalLoginProviderInfo;
    }
}
