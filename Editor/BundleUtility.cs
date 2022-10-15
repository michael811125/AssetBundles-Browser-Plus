using System;
using System.Text;

namespace AssetBundleBrowser.Utilities
{
    public static class BundleUtility
    {
        /// <summary>
        /// Make MD5 for string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MakeMd5ForString(string str, bool uppercase = false)
        {
            using (var createMd5 = System.Security.Cryptography.MD5.Create())
            {
                // get string bytes
                byte[] bytes = Encoding.UTF8.GetBytes(str);

                // get hash from bytes
                byte[] hashBytes = createMd5.ComputeHash(bytes);

                // make a MD5
                string md5 = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                if (uppercase) md5 = md5.ToUpper();
                else md5 = md5.ToLower();

                return md5;
            }
        }
    }
}
