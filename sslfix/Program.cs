using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace sslfix
{
    internal class Program
    {
        /// <summary>
        /// Downloads the certificate from the given URL.
        /// </summary>
        /// <param name="url">URL of the certificate.</param>
        /// <returns>Byte array of the downloaded certificate.</returns>
        private static async Task<byte[]> DownloadCertificateAsync(string url)
        {
            // Ensure that the URL is from a trusted source (Let's Encrypt in this case).
            if (!url.StartsWith("https://letsencrypt.org/certs/"))
            {
                throw new InvalidOperationException("Attempted to download a certificate from an unauthorized source.");
            }

            using (var client = new HttpClient())
            {
                return await client.GetByteArrayAsync(url);
            }
        }

        /// <summary>
        /// Installs the certificate to the system's Root store.
        /// This requires the program to be run with administrator privileges.
        /// </summary>
        /// <param name="certificateBytes">Byte array of the certificate to be installed.</param>
        private static void InstallCertificate(byte[] certificateBytes)
        {
            using (var cert = new X509Certificate2(certificateBytes))
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                }
                finally
                {
                    store.Close();
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// Downloads and installs specified certificates from Let's Encrypt.
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // List of Let's Encrypt certificates to be downloaded and installed.
                string[] urls =
                {
                    "https://letsencrypt.org/certs/isrgrootx1.der",
                    "https://letsencrypt.org/certs/isrg-root-x2.der"
                };

                foreach (var url in urls)
                {
                    Console.WriteLine($"Downloading -> {url}");
                    var certificateBytes = await DownloadCertificateAsync(url);
                    InstallCertificate(certificateBytes);
                    Console.WriteLine($"Installed successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Wait for a key press to exit.
            Console.ReadKey();
        }
    }
}
