using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace BeatScrobbler.Utils;

[UsedImplicitly]
public class SignatureUtils
{
    public static string SignedParams(Dictionary<string, string> parameters, string secret)
    {
        StringBuilder builder = new();

        foreach ((string? key, string? value) in parameters)
            builder.Append(key).Append('=').Append(WebUtility.UrlEncode(value)).Append('&');

        string signature = Sign(parameters, secret);

        builder.Append("api_sig=").Append(signature).Append("&format=json");

        return builder.ToString();
    }

    private static string Sign(Dictionary<string, string> parameters, string secret)
    {
        MD5 md5 = MD5.Create();

        StringBuilder builder = new();

        foreach ((string? key, string? value) in parameters.OrderBy(p => p.Key)) builder.Append(key).Append(value);

        builder.Append(secret);

        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));

        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}
