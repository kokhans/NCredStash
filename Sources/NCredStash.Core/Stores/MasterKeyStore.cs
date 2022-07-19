﻿// MIT License
//
// Copyright (c) 2022 Serhii Kokhan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Carcass.Core;
using NCredStash.Core.Stores.Abstracts;

namespace NCredStash.Core.Stores;

public sealed class MasterKeyStore : IMasterKeyStore
{
    private readonly IAmazonKeyManagementService _amazonKeyManagementService;

    public MasterKeyStore(IAmazonKeyManagementService amazonKeyManagementService)
    {
        ArgumentVerifier.NotNull(amazonKeyManagementService, nameof(amazonKeyManagementService));

        _amazonKeyManagementService = amazonKeyManagementService;
    }

    public async Task<byte[]> DecryptAsync(
        byte[] keyBytes,
        Dictionary<string, string> encryptionContext,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentVerifier.NotNull(keyBytes, nameof(keyBytes));
        ArgumentVerifier.NotNull(encryptionContext, nameof(encryptionContext));

        DecryptResponse decryptResponse = await _amazonKeyManagementService.DecryptAsync(
            new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(keyBytes),
                EncryptionContext = encryptionContext
            },
            cancellationToken);

        byte[] buffer = new byte[decryptResponse.Plaintext.Length];

        _ = await decryptResponse.Plaintext.ReadAsync(
            buffer.AsMemory(0, (int) decryptResponse.Plaintext.Length),
            cancellationToken
        );

        return buffer;
    }
}