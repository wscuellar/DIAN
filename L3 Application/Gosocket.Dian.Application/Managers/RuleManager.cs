using Gosocket.Dian.Application.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application.Managers
{
    public class RuleManager
    {
        private static readonly TableManager tableManager = new TableManager("GlobalDocValidatorRule");
        static List<GlobalDocValidatorRule> _rules = new List<GlobalDocValidatorRule>();

        private static StackExchange.Redis.IDatabase cache;

        private static RuleManager _instance = null;

        private readonly string _category;

        private static readonly object _lock = new object();

        public static bool LoadingRules { get; private set; }

        public RuleManager()
        {
            //if (cache == null)
            //{
            //    var globalRedis = ConfigurationManager.GetValue("GlobalRedis");
            //    if (!string.IsNullOrEmpty(globalRedis))
            //        cache = RedisConnectorManager.Connection.GetDatabase();
            //}
        }

        public RuleManager(bool gadget = false)
        {

        }

        public RuleManager(string category)
        {
            _category = category;
        }

        public static RuleManager Instance => _instance ?? (_instance = new RuleManager());

        public bool AddOrUpdate(GlobalDocValidatorRule rule)
        {
            return tableManager.InsertOrUpdate(rule);
        }

        public List<GlobalDocValidatorRule> GetAll()
        {
            TableContinuationToken token = null;
            var rules = new List<GlobalDocValidatorRule>();
            do
            {
                var data = tableManager.GetRangeRows<GlobalDocValidatorRule>(1000, token);
                token = data.Item2;
                rules.AddRange(data.Item1);
            }
            while (token != null);

            return rules;
        }

        public async Task<List<GlobalDocValidatorRule>> GetRulesAsync()
        {
            return await cache.GetOrSetAsync("ruleList", () => GetAll());
        }

        public List<GlobalDocValidatorRule> GetRules()
        {
            return cache.GetOrSet("ruleList", () => GetAll());
        }

        public void DeleteCache()
        {
            cache.KeyDelete("ruleList");
        }

        public GlobalDocValidatorRule Get(string partitionKey, string rowKey)
        {
            return tableManager.Find<GlobalDocValidatorRule>(partitionKey, rowKey);
        }

        public List<GlobalDocValidatorRule> List()
        {
            TableContinuationToken token = null;
            var rules = new List<GlobalDocValidatorRule>();
            do
            {
                var data = tableManager.GetRangeRows<GlobalDocValidatorRule>(_category, 1000, token);
                token = data.Item2;
                rules.AddRange(data.Item1);
            }
            while (token != null);
            return rules;
        }

        public List<GlobalDocValidatorRule> ListAll()
        {
            TableContinuationToken token = null;
            var rules = new List<GlobalDocValidatorRule>();
            do
            {
                var data = tableManager.GetRangeRows<GlobalDocValidatorRule>(1000, token);
                token = data.Item2;
                rules.AddRange(data.Item1);
            }
            while (token != null);
            return rules;
        }
    }
}
