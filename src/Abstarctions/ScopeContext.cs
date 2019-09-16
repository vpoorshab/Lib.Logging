using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Lib.Logging.Abstractions
{
    public class ScopeContext
    {
        public ScopeContext()
        {
            _items = new Dictionary<string, object>();
        }

        private readonly Dictionary<string,object> _items;
        
        public ScopeContext WithName(string name)
        {
            Push("ScopeName", name);
            return this;
        }

        public ScopeContext WithRequestId(string requestId)
        {
            Push("RequestId", requestId);
            return this;
        }

        public ScopeContext WithUserName(string userName)
        {
            Push("UserName", userName);
            return this;
        }

             

        public ScopeContext WithUserId(object userId)
        {
            Push("UserId", userId);
            return this;
        }


        public ScopeContext Push(string name, object value)
        {

            if (_items.ContainsKey(name))
                _items[name] = value;
            else
                _items.Add(name, value);
            return this;
        }


        public ScopeContext Push(object state)
        {

            if (state is IDictionary<string, object> objects)
            {
                foreach (var keyValuePair in objects)
                {
                    Push(keyValuePair.Key, keyValuePair.Value);
                }
            }
            else
            if (state is KeyValuePair<string, object> keyValuePair)
            {
                Push(keyValuePair.Key, keyValuePair.Value);
            }
            else
            {
                Push(state.ToString(), state);
            }

            return this;

        }

        public ScopeContext Push(ScopeContext context)
        {

            Push(context._items);
            return this;

        }

        public IDisposable BeginScope(ILogger logger)
        {
            return logger.BeginScope(_items);
        }

        public Dictionary<string, object> ToDictionary()
        {
            return _items;
        }

        public ScopeContext Clone()
        {
            return new ScopeContext().Push(this);
        }

        public static ScopeContext Instance => new ScopeContext();
    }
}
