#nullable enable
using System;
using System.Security.Cryptography;
using System.Text;
using IdleMmo.Client.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace IdleMmo.Client.Network.Auth;

/// <summary>
/// PlayerPrefs-backed session store, AES-encrypted with a per-device key derived from
/// SystemInfo.deviceUniqueIdentifier + a baked-in pepper. Not bulletproof against a determined
/// attacker with device access, but raises the bar above plaintext.
/// </summary>
public sealed class PrefsTokenStore : ITokenStore
{
    private const string PrefsKey = "idlemmo.session.v1";
    private const string Pepper = "8a4e9c2f-idle-mmo-do-not-leak-please-rotate";

    private readonly ILog _log;

    public PrefsTokenStore(ILog log)
    {
        _log = log;
    }

    public StoredSession? Load()
    {
        if (!PlayerPrefs.HasKey(PrefsKey)) return null;
        try
        {
            string blob = PlayerPrefs.GetString(PrefsKey);
            string json = Decrypt(blob);
            return JsonConvert.DeserializeObject<StoredSession>(json);
        }
        catch (Exception ex)
        {
            _log.Warn($"Failed to load session from PlayerPrefs: {ex.Message}");
            Clear();
            return null;
        }
    }

    public void Save(StoredSession session)
    {
        try
        {
            string json = JsonConvert.SerializeObject(session);
            string blob = Encrypt(json);
            PlayerPrefs.SetString(PrefsKey, blob);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            _log.Error("Failed to persist session", ex);
        }
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
    }

    private static byte[] DeriveKey()
    {
        string seed = SystemInfo.deviceUniqueIdentifier + ":" + Pepper;
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
    }

    private static string Encrypt(string plaintext)
    {
        byte[] key = DeriveKey();
        byte[] iv = new byte[16];
        RandomNumberGenerator.Fill(iv);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var enc = aes.CreateEncryptor();
        byte[] data = Encoding.UTF8.GetBytes(plaintext);
        byte[] cipher = enc.TransformFinalBlock(data, 0, data.Length);
        // layout: base64( iv || cipher )
        byte[] combined = new byte[iv.Length + cipher.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(cipher, 0, combined, iv.Length, cipher.Length);
        return Convert.ToBase64String(combined);
    }

    private static string Decrypt(string blob)
    {
        byte[] combined = Convert.FromBase64String(blob);
        byte[] iv = new byte[16];
        Buffer.BlockCopy(combined, 0, iv, 0, 16);
        byte[] cipher = new byte[combined.Length - 16];
        Buffer.BlockCopy(combined, 16, cipher, 0, cipher.Length);
        using var aes = Aes.Create();
        aes.Key = DeriveKey();
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var dec = aes.CreateDecryptor();
        byte[] plain = dec.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }
}
