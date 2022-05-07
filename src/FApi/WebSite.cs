using System.Net;
using System.Text;
using hw.DebugFormatter;

namespace FactorioApi;

sealed class WebSite : DumpableObject
{
    readonly string Name;

    public WebSite(string name) => Name = name;

    public string String => GetString();

    string GetString() => Encoding.ASCII.GetString(new WebClient().DownloadData(Name));
}