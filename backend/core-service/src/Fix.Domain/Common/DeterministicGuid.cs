using System.Security.Cryptography;
using System.Text;

namespace Fix.Domain.Common;

/// <summary>Gera sempre o mesmo Guid para a mesma chave (usado nos registros de seed).</summary>
public static class DeterministicGuid
{
    public static Guid From(string key) => new(MD5.HashData(Encoding.UTF8.GetBytes(key)));
}
