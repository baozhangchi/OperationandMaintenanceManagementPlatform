//using System.Collections;

namespace OMMP.WebClient.States;

public interface IMonitorState : IEnumerable<KeyValuePair<string, string>>
{
    string this[string ip] { get; set; }

    void Remove(string ip);
}