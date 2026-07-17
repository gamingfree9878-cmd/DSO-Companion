using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;
using DSOCompanion.Models;

namespace DSOCompanion.Services;

public sealed class LicenseService
{
    private const string PublicKeyPem = """
-----BEGIN PUBLIC KEY-----
MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAw+nSZrF0jnpT7RzIEo6R
YnyJUpAVyrEUOAdZZ/28CMeEESoCykrc7mCSPoqujyACO0sycfLdg7FIT9jRxqEB
kOqICd8kP8JEe3TLpjs7yiIiI5kxirpEjvxv09InqlONblwYp4QQK4pl2pHt0kYi
3vy+rL37bIZiLHQM5rDsq0Ms7B0+Du2Grdoykkd15ak5UZ2B7FUbo2wk2rBtmMe3
I70qnozYYFAWQ8WSGJmesC/xwMgbmNuFtudg1fzApKq9pZNQh4eS0S+nvZtF5OxN
smVDqfKSx1ZToQ4RndbZQ+ujzbppjcsm2wweVMfRVvGGarFqlQaxY2NmquLn1L0q
2VtwQBSRjEMz7iwtrkSY/CVqIzPMrRf1F4QQw3PVlkuA5Ttr8rJS4DBaWFtoDeJu
xKOBXeFLZdIDC/MVjFSICYvfx4gJ0SA/SEH35WFZHKr9kbTJDb1vjgnOkrVQnrrc
1H4uMvjg0phR8iOWW1Ez9pkh9kmFDbf9c6eyvfViq7pdAgMBAAE=
-----END PUBLIC KEY-----
""";

    private static readonly string LicenseFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DSOCompanion");

    private static readonly string LicenseFile =
        Path.Combine(LicenseFolder, "license.dat");

    public string HardwareId { get; } = CreateHardwareId();

    public LicensePayload? CurrentLicense { get; private set; }

    public string LastError { get; private set; } = "";

    public bool HasValidLicense()
    {
        LastError = "";

        try
        {
            if (!File.Exists(LicenseFile))
            {
                LastError = "Noch keine Lizenz aktiviert.";
                return false;
            }

            string token = DecryptStoredToken(File.ReadAllBytes(LicenseFile));
            return ValidateToken(token, saveAfterValidation: false);
        }
        catch (Exception ex)
        {
            LastError = "Die gespeicherte Lizenz konnte nicht gelesen werden: " + ex.Message;
            CurrentLicense = null;
            return false;
        }
    }

    public bool Activate(string activationCode)
    {
        LastError = "";
        return ValidateToken(activationCode.Trim(), saveAfterValidation: true);
    }

    public void RemoveLicense()
    {
        CurrentLicense = null;

        if (File.Exists(LicenseFile))
            File.Delete(LicenseFile);
    }

    public string MaskedLicenseId =>
        CurrentLicense is null
            ? "Nicht aktiviert"
            : CurrentLicense.LicenseId;

    public string ExpiryText
    {
        get
        {
            if (CurrentLicense is null)
                return "Keine gültige Lizenz";

            if (CurrentLicense.ExpiresUtc is null)
                return "Lifetime";

            return CurrentLicense.ExpiresUtc.Value.ToLocalTime()
                .ToString("dd.MM.yyyy");
        }
    }

    private bool ValidateToken(string token, bool saveAfterValidation)
    {
        try
        {
            string[] parts = token.Split('.');

            if (parts.Length != 2)
            {
                LastError = "Der Aktivierungscode hat ein ungültiges Format.";
                return false;
            }

            byte[] payloadBytes = FromBase64Url(parts[0]);
            byte[] signatureBytes = FromBase64Url(parts[1]);

            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(PublicKeyPem);

            bool signatureValid = rsa.VerifyData(
                payloadBytes,
                signatureBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pss);

            if (!signatureValid)
            {
                LastError = "Die Signatur der Lizenz ist ungültig.";
                return false;
            }

            LicensePayload? payload =
                JsonSerializer.Deserialize<LicensePayload>(payloadBytes);

            if (payload is null)
            {
                LastError = "Die Lizenzdaten sind leer oder beschädigt.";
                return false;
            }

            if (!string.Equals(
                    payload.HardwareId,
                    HardwareId,
                    StringComparison.OrdinalIgnoreCase))
            {
                LastError =
                    "Diese Lizenz gehört zu einem anderen PC.\n\n" +
                    "HWID dieses PCs:\n" + HardwareId;
                return false;
            }

            if (payload.ExpiresUtc is not null &&
                payload.ExpiresUtc.Value <= DateTime.UtcNow)
            {
                LastError = "Diese Lizenz ist am " +
                    payload.ExpiresUtc.Value.ToLocalTime().ToString("dd.MM.yyyy") +
                    " abgelaufen.";
                return false;
            }

            CurrentLicense = payload;

            if (saveAfterValidation)
            {
                Directory.CreateDirectory(LicenseFolder);
                File.WriteAllBytes(
                    LicenseFile,
                    EncryptTokenForThisComputer(token));
            }

            return true;
        }
        catch (Exception ex)
        {
            LastError = "Die Lizenz konnte nicht geprüft werden: " + ex.Message;
            CurrentLicense = null;
            return false;
        }
    }

    private byte[] EncryptTokenForThisComputer(string token)
    {
        byte[] key = SHA256.HashData(
            Encoding.UTF8.GetBytes(
                HardwareId + "|DSO-Companion-License-v1"));

        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] plain = Encoding.UTF8.GetBytes(token);
        byte[] cipher = new byte[plain.Length];
        byte[] tag = new byte[16];

        using AesGcm aes = new(key, 16);
        aes.Encrypt(nonce, plain, cipher, tag);

        byte[] result = new byte[1 + nonce.Length + tag.Length + cipher.Length];
        result[0] = 1;
        Buffer.BlockCopy(nonce, 0, result, 1, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, 1 + nonce.Length, tag.Length);
        Buffer.BlockCopy(
            cipher,
            0,
            result,
            1 + nonce.Length + tag.Length,
            cipher.Length);

        return result;
    }

    private string DecryptStoredToken(byte[] data)
    {
        if (data.Length < 30 || data[0] != 1)
            throw new InvalidDataException("Unbekanntes Lizenzdateiformat.");

        byte[] key = SHA256.HashData(
            Encoding.UTF8.GetBytes(
                HardwareId + "|DSO-Companion-License-v1"));

        byte[] nonce = data[1..13];
        byte[] tag = data[13..29];
        byte[] cipher = data[29..];
        byte[] plain = new byte[cipher.Length];

        using AesGcm aes = new(key, 16);
        aes.Decrypt(nonce, cipher, tag, plain);

        return Encoding.UTF8.GetString(plain);
    }

    private static string CreateHardwareId()
    {
        string machineGuid = "";

        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Cryptography");

            machineGuid =
                key?.GetValue("MachineGuid")?.ToString() ?? "";
        }
        catch
        {
        }

        string source =
            machineGuid + "|" +
            Environment.MachineName + "|" +
            Environment.OSVersion.Platform;

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        string hex = Convert.ToHexString(hash);

        return string.Join(
            "-",
            Enumerable.Range(0, 5)
                .Select(i => hex.Substring(i * 8, 8)));
    }

    private static byte[] FromBase64Url(string value)
    {
        string base64 = value.Replace('-', '+').Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }
}
