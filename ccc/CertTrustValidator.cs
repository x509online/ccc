using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ccc
{
    internal class CertTrustValidator
    {
        static internal (bool, IList<string>) IsTrusted(X509Certificate2 cert)
        {
            bool trusted = true;
            IList<string> reasons = new List<string>();
            X509Chain chain = new X509Chain(true);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            //chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            if (!chain.Build(cert))
            {
                trusted = false;
                foreach (var st in chain.ChainStatus)
                {
                    reasons.Add(st.StatusInformation);
                    reasons.Add(st.Status.ToString());
                    //if (st.Status==X509ChainStatusFlags.UntrustedRoot)
                    //{
                    //    trusted = true;
                    //}
                }
            }

            return (trusted, reasons);
        }

        static internal void AddCertToStoreIfNotExist(X509Certificate2 cert)
        {
            X509Store store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var foundCerts = store.Certificates.Find(X509FindType.FindBySerialNumber, cert.SerialNumber, false);
            if (foundCerts.Count > 0) // should not happen
            {
                store.Close();
                Console.WriteLine("Certificate already found. SN: " + cert.SerialNumber);
            }
            else
            {
                store.Close();
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
                Console.WriteLine("CERT Added successfully.");
            }
        }
    }
}
