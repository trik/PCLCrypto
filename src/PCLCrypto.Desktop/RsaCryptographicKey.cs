﻿//-----------------------------------------------------------------------
// <copyright file="RsaCryptographicKey.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace PCLCrypto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Validation;

    /// <summary>
    /// The .NET Framework implementation of the <see cref="ICryptographicKey"/> interface
    /// for RSA keys.
    /// </summary>
    internal class RsaCryptographicKey : ICryptographicKey
    {
        /// <summary>
        /// The platform crypto key.
        /// </summary>
        private readonly RSACryptoServiceProvider key;

        /// <summary>
        /// The algorithm to use when performing cryptography.
        /// </summary>
        private readonly AsymmetricAlgorithm algorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaCryptographicKey" /> class.
        /// </summary>
        /// <param name="key">The RSA crypto service provider.</param>
        /// <param name="algorithm">The algorithm.</param>
        internal RsaCryptographicKey(RSACryptoServiceProvider key, AsymmetricAlgorithm algorithm)
        {
            Requires.NotNull(key, "key");

            this.key = key;
            this.algorithm = algorithm;
        }

        /// <inheritdoc />
        public int KeySize
        {
            get { return this.key.KeySize; }
        }

        /// <summary>
        /// Gets the RSA crypto service provider that contains this key.
        /// </summary>
        internal RSACryptoServiceProvider Rsa
        {
            get { return this.key; }
        }

        /// <summary>
        /// Gets the algorithm to use with this key.
        /// </summary>
        internal AsymmetricAlgorithm Algorithm
        {
            get { return this.algorithm; }
        }

        /// <inheritdoc />
        public byte[] Export(CryptographicPrivateKeyBlobType blobType)
        {
            switch (blobType)
            {
                case CryptographicPrivateKeyBlobType.Pkcs8RawPrivateKeyInfo:
                    return this.key.ExportCspBlob(includePrivateParameters: true);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public byte[] ExportPublicKey(CryptographicPublicKeyBlobType blobType)
        {
            switch (blobType)
            {
                case CryptographicPublicKeyBlobType.X509SubjectPublicKeyInfo:
                    return this.key.ExportCspBlob(includePrivateParameters: false);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
