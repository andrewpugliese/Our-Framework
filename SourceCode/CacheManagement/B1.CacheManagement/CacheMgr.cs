﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;

using B1.ILoggingManagement;

namespace B1.CacheManagement
{
    /// <summary>
    /// Default CacheMgr class of type Object which has a case-sensitive key
    /// </summary>
    public class CacheMgr : CacheMgr<object>
    {
    }

    /// <summary>
    /// Main class for managing the in-memory caching in a thread-safe manner. A refreshable cache can be added which
    /// automatically refreshes the cache value at the chosen interval. It supports multiple read and single write
    /// lock. This is a template class and any type of value can be stored using a string key.  
    /// </summary>
    public class CacheMgr<TValue>
    {
        Dictionary<string, TValue> _cache = null;
        
        /// <summary>
        /// Defaults to a case-sensitive key
        /// </summary>
        public CacheMgr()
        {
            _cache = new Dictionary<string, TValue>();
        }

        /// <summary>
        /// Allows caller to override stringcomparer for cache key
        /// For example a case-insenistive cache key:
        /// CacheMgr(StringComparer.CurrentCultureIgnoreCase)
        /// </summary>
        /// <param name="stringComparer">A string comparer object to allow for case-insensitve cache-key</param>
        public CacheMgr(StringComparer stringComparer)
        {
            _cache = new Dictionary<string, TValue>(stringComparer);
        }

        /// <summary>
        /// This object allows multiple thread for reading while grants exclusive access for writing. When a thread is in
        /// write mode, no other thread can enter the lock in any mode. When a thread is in upgradable mode (only one thread
        /// allowed to be in this mode at any time), other threads can be in read mode. Upgradeable mode is intended for cases
        /// where a thread usually reads from the protected resource, but might need to write to it if some condition is met.
        /// </summary>
        static ReaderWriterLockSlim _cacheReadWriteLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Lookup a cache value given a cacheKey.
        /// </summary>
        /// <param name="cacheKey">Cache key to lookup</param>
        /// <returns>Value for the given cache key</returns>
        public TValue Get(string cacheKey)
        {
            // Multiple threads can enter the read mode at the same time.
            // If one or more threads are waiting to enter write mode than this function calls blocks till those threads are done.
            _cacheReadWriteLock.EnterReadLock();
            try
            {
                if (_cache.ContainsKey(cacheKey))
                    return _cache[cacheKey];
                else throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                        , string.Format("Cachekey: {0} not found in cache", cacheKey));
            }
            finally
            {
                _cacheReadWriteLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Lookup the cache value whose key passes the predicate test.
        /// </summary>
        /// <param name="predicate">Predicate function which tests the key</param>
        /// <param name="defaultValue">Default value if no key satisfies the predicate.</param>
        /// <returns></returns>
        public TValue FirstOrDefault(Func<TValue, bool> predicate, TValue defaultValue)
        {
            _cacheReadWriteLock.EnterReadLock();
            try
            {
                var entryFound = _cache.FirstOrDefault(kv => predicate(kv.Value));
                return entryFound.Key != null ? entryFound.Value : defaultValue;
            }
            finally
            {
                _cacheReadWriteLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Lookup a cache value given a cacheKey or return the default.
        /// </summary>
        /// <param name="cacheKey">Cache key to lookup</param>
        /// <param name="defaultValue">Default value which is returned if key NOT found in the cache</param>
        /// <returns>Value for the given cache key or default if NOT found</returns>
        public TValue GetOrDefault(string cacheKey, TValue defaultValue)
        {
            // Multiple threads can enter the read mode at the same time.
            // If one or more threads are waiting to enter write mode than this function calls blocks till those threads are done.
            _cacheReadWriteLock.EnterReadLock();
            try
            {
                return _cache.ContainsKey(cacheKey) ? _cache[cacheKey] : defaultValue;
            }
            finally
            {
                _cacheReadWriteLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Lookup a cached value for the given cacheKey. If NO value is found then evaluate the passed function to get
        /// the value, store it in the cache and return the value.
        /// </summary>
        /// <param name="cacheKey">Cache key to lookup</param>
        /// <param name="getValueFunc">A delegate function which returns the value if NO cached value is found</param>
        /// <returns>Value for the given cache key</returns>
        public TValue GetOrAdd(string cacheKey, Func<TValue> getValueFunc)
        {
            // Only one thread can enter upgradable mode or write mode at any given time.
            // Any number of threads can enter read mode if there are no threads waiting to enter write mode.
            _cacheReadWriteLock.EnterUpgradeableReadLock();

            try
            {
                if (_cache.ContainsKey(cacheKey))
                    return _cache[cacheKey];

                TValue value = getValueFunc();
                _cacheReadWriteLock.EnterWriteLock();

                try
                {
                    _cache.Add(cacheKey, value);
                }
                finally
                {
                    _cacheReadWriteLock.ExitWriteLock();
                }
               
                return _cache[cacheKey];
            }
            finally
            {
                _cacheReadWriteLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Get the list of keys stored in the cache. 
        /// </summary>
        public List<string> Keys
        {
            get
            {
                _cacheReadWriteLock.EnterReadLock();
                try
                {
                    return _cache.Keys.ToList();
                }
                finally
                {
                    _cacheReadWriteLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Adds the specified key and value to the cache.
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="value">Cache value</param>
        public void Add(string cacheKey, TValue value)
        {
            _cacheReadWriteLock.EnterWriteLock();
            try
            {
                _cache.Add(cacheKey, value);
            }
            finally
            {
                _cacheReadWriteLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds the specified key and value to the cache. Also refreshes the cache value at a given interval using the
        /// given function.
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="getValueFunc">A delegate function which returns the value</param>
        /// <param name="refreshValueSeconds">Interval at which the cache value will be auto updated.</param>
        public void Add(string cacheKey, Func<string, TValue> getValueFunc, int refreshValueSeconds)
        {
            Add(cacheKey, getValueFunc(cacheKey));
            RecurringCallbackMgr.Default.Add(cacheKey, key => Set(key, getValueFunc(key)), refreshValueSeconds);
        }

        /// <summary>
        /// Sets a new value to the cache or add the key and value to the value if key NOT found in the cache.
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="value">Cache value</param>
        public void Set(string cacheKey, TValue value)
        {
            _cacheReadWriteLock.EnterUpgradeableReadLock();
            try
            {
                if (_cache.ContainsKey(cacheKey))
                {
                    _cacheReadWriteLock.EnterWriteLock();
                    try
                    {
                        _cache[cacheKey] = value;
                    }
                    finally
                    {
                        _cacheReadWriteLock.ExitWriteLock();
                    }
                }
                else
                {
                    Add(cacheKey, value);
                }
            }
            finally
            {
                _cacheReadWriteLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Check if a key exists in the cache.
        /// </summary>
        /// <param name="cacheKey">Cache key to lookup</param>
        /// <returns>True if the key exists in the cache else False</returns>
        public bool Exists(string cacheKey)
        {
            _cacheReadWriteLock.EnterReadLock();
            try
            {
                return _cache.ContainsKey(cacheKey);
            }
            finally
            {
                _cacheReadWriteLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes the value from the specified key in the cache.
        /// It also removes the recurring callback method (if any) that was associated with this key.
        /// </summary>
        /// <param name="cacheKey">Cache key to lookup</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns
        /// false if key is not found in the cache</returns>
        public bool Remove(string cacheKey)
        {
            _cacheReadWriteLock.EnterWriteLock();
            try
            {
                if (RecurringCallbackMgr.Default.ContainsKey(cacheKey))
                    RecurringCallbackMgr.Default.Remove(cacheKey);
                return _cache.Remove(cacheKey);
            }
            finally
            {
                _cacheReadWriteLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes all the keys and values from the cache.
        /// </summary>
        public void Clear()
        {
            _cacheReadWriteLock.EnterWriteLock();
            try
            {
                _cache.Clear();
            }
            finally
            {
                _cacheReadWriteLock.ExitWriteLock();
            }
        }
    }
}
