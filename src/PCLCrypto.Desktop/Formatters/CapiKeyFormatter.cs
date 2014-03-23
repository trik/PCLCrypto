﻿//-----------------------------------------------------------------------
// <copyright file="CapiKeyFormatter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace PCLCrypto.Formatters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Formats keys in the CAPI file format.
    /// This is the format used by RSACryptoServiceProvider.ExportCspBlob
    /// </summary>
    internal class CapiKeyFormatter : KeyFormatter
    {
        /// <summary>
        /// Determines whether the specified RSA parameters
        /// can be represented in the CAPI format.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns><c>true</c> if CAPI is compatible with these parameters; <c>false</c> otherwise.</returns>
        internal static bool IsCapiCompatible(RSAParameters parameters)
        {
            // Only private keys have this restriction.
            if (!KeyFormatter.HasPrivateKey(parameters))
            {
                return true;
            }

            int halfModulusLength = (parameters.Modulus.Length + 1) / 2;

            // These are the same assertions that Windows crypto lib itself
            // follows when it returns 'bad data'.
            // CAPI's file format does not include lengths for parameters.
            // Instead it makes some assumptions about their relative lengths
            // which make it fundamentally incompatible with some private keys
            // generated by iOS.
            return
                halfModulusLength == parameters.P.Length &&
                halfModulusLength == parameters.Q.Length &&
                halfModulusLength == parameters.DP.Length &&
                halfModulusLength == parameters.DQ.Length &&
                halfModulusLength == parameters.InverseQ.Length &&
                parameters.Modulus.Length == parameters.D.Length;
        }

        /// <summary>
        /// Throws an exception if the specified RSAParameters cannot be
        /// serialized in the CAPI format.
        /// </summary>
        /// <param name="parameters">The RSA parameters.</param>
        internal static void VerifyCapiCompatibleParameters(RSAParameters parameters)
        {
            try
            {
                KeyFormatter.VerifyFormat(IsCapiCompatible(parameters), "Private key parameters have lengths that are not supported by CAPI.");
            }
            catch (FormatException ex)
            {
                throw new NotSupportedException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Reads a key from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// The RSA Parameters of the key.
        /// </returns>
        protected override RSAParameters ReadCore(Stream stream)
        {
            byte[] keyBlob = new byte[stream.Length];
            stream.Read(keyBlob, 0, keyBlob.Length);
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(keyBlob);
            return rsa.ExportParameters(!rsa.PublicOnly);
        }

        /// <summary>
        /// Writes a key to the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="parameters">The RSA parameters of the key.</param>
        protected override void WriteCore(Stream stream, RSAParameters parameters)
        {
            VerifyCapiCompatibleParameters(parameters);
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(parameters);
            byte[] keyBlob = rsa.ExportCspBlob(!rsa.PublicOnly);
            stream.Write(keyBlob, 0, keyBlob.Length);
        }
    }
}