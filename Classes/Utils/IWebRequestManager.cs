namespace Koinonia
{
    public interface IWebRequestManager
    {

        byte[] GetBytes(string url);
        string GetText(string url);

    }
}