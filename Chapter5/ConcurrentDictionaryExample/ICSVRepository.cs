using System;
using System.Collections.Generic;

namespace ConcurrentDictionaryExample
{
    public interface ICSVRepository
    {
        IEnumerable<string> Files { get; }
        IEnumerable<T> Map<T>(string dataFile, Func<string[], T> map);
        bool VerifyEachFileOnlyLoadedOnce();
    }
}