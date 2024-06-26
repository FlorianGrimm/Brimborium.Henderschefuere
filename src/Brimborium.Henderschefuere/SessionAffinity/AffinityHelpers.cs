// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.SessionAffinity;

internal static class AffinityHelpers {
    internal static CookieOptions CreateCookieOptions(SessionAffinityCookieConfig? config, bool isHttps, TimeProvider timeProvider) {
        return new CookieOptions {
            Path = config?.Path ?? "/",
            SameSite = config?.SameSite ?? Microsoft.AspNetCore.Http.SameSiteMode.Unspecified,
            HttpOnly = config?.HttpOnly ?? true,
            MaxAge = config?.MaxAge,
            Domain = config?.Domain,
            IsEssential = config?.IsEssential ?? false,
            Secure = config?.SecurePolicy == CookieSecurePolicy.Always || (config?.SecurePolicy == CookieSecurePolicy.SameAsRequest && isHttps),
            Expires = config?.Expiration is not null ? timeProvider.GetUtcNow().Add(config.Expiration.Value) : default(DateTimeOffset?),
        };
    }
}
