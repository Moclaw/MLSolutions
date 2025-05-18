namespace Core.Constants
{
    public static class AutofacConstants
    {
        public static class ServiceConventions
        {
            /// <summary>
            /// Returns the default suffixes for service types
            /// </summary>
            public static string[] GetDefaultSuffixes()
            {
                return ["Service", "Repository", "Manager"];
            }
        }
    }
}

