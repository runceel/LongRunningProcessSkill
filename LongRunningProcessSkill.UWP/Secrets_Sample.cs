using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningProcessSkill.UWP
{
    // 正しい値を入れて、Secretsにリネームしてください。
    public static class Secrets_Sample
    {
        public static string StorageConnectionString { get; } = "Function App の Azure Storage への接続文字列";
        public static string SystemKey { get; } = "Function App の _master ホストキー";
        public static string AzureFunctionsUrl { get; } = "Function App の URL (例: https://xxx.azurewebsites.net)";
    }
}
