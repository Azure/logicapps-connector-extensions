using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Microsoft.WindowsAzure.ResourceStack.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.FTP.Providers
{
    public class FluentClientCacheKey
    {

        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseSSL { get; set; }
        public bool ActiveMode { get; set; }
        public bool ImplicitMode { get; set; }
        public bool UseSelfSignedCert { get; set; }
        public bool UseBinaryMode { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as FluentClientCacheKey);
        }

        /// <summary>
        /// Indicates if the two message sender cache keys are equal.
        /// </summary>
        /// <param name="other">The object to be compared against.</param>
        public bool Equals(FluentClientCacheKey other)
        {
            return other != null &&
                this.Host.EqualsOrdinally(other.Host) &&
                this.UserName.EqualsOrdinally(other.Host) &&
                this.Password.EqualsOrdinally(other.Password) &&
                this.UseSSL.Equals(other.UseSSL) &&
                this.ActiveMode.Equals(other.ActiveMode) &&
                this.ImplicitMode.Equals(other.ImplicitMode) &&
                this.UseBinaryMode.Equals(other.ImplicitMode) &&
                this.UseSelfSignedCert.Equals(other.UseSelfSignedCert);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCodeUtility.CombineHashCodes(
                StringComparer.Ordinal.GetHashCode(this.Host),
                StringComparer.Ordinal.GetHashCode(this.UserName),
                StringComparer.Ordinal.GetHashCode(this.Password),
                Convert.ToInt32(this.UseSSL),
                Convert.ToInt32(this.UseBinaryMode),
                Convert.ToInt32(this.ActiveMode),
                Convert.ToInt32(this.ImplicitMode),
                Convert.ToInt32(this.UseSelfSignedCert));
        }
    }
}
