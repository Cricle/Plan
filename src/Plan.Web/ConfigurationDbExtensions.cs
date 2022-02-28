namespace Plan.Web
{
    internal static class ConfigurationDbExtensions
    {
        public static string GetDbConnect(this IConfiguration cfg)
        {
#if DBSELECT_MSSQL
            return cfg["PlanSqlService"];
#elif DBSELECT_SQLITE
            return "Data Source=plan.db";
#endif
        }
        public static string GetRedisConnect(this IConfiguration cfg)
        {
#if REDIS_LOCAL
            return "127.0.0.1:6379";
#elif REDIS_REMOTE
            return cfg["CacheConnection"];
#endif
        }
        public static string GetMongoConnect(this IConfiguration cfg)
        {
#if REDIS_LOCAL
            return "mongodb://127.0.0.1:27017";
#elif REDIS_REMOTE
            return cfg["MongoConnection"];
#endif
        }
    }
}
